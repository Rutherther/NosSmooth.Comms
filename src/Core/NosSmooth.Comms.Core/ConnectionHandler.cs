//
//  ConnectionHandler.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data;
using MessagePack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NosSmooth.Comms.Data;
using NosSmooth.Comms.Data.Messages;
using NosSmooth.Core.Contracts;
using NosSmooth.Core.Extensions;
using Remora.Results;

namespace NosSmooth.Comms.Core;

/// <summary>
/// Manages a connection, calls message handler when message is received.
/// Serializes and deserializes the messages from the stream.
/// </summary>
public class ConnectionHandler
{
    private readonly Contractor? _contractor;
    private readonly IConnection _connection;
    private readonly MessageHandler _messageHandler;
    private readonly MessagePackSerializerOptions _options;
    private readonly ILogger<ConnectionHandler> _logger;
    private long _messageId = 1;
    private Task<Result>? _task;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionHandler"/> class.
    /// </summary>
    /// <param name="contractor">The contractor.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    public ConnectionHandler
    (
        Contractor? contractor,
        IConnection connection,
        MessageHandler messageHandler,
        IOptions<NosSmoothMessageSerializerOptions> options,
        ILogger<ConnectionHandler> logger
    )
    {
        _contractor = contractor;
        _connection = connection;
        _messageHandler = messageHandler;
        _options = options.Value;
        _logger = logger;
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Gets the id of the connection.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The connection has been closed.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Gets the connection.
    /// </summary>
    public IConnection Connection => _connection;

    /// <summary>
    /// Run the handler and await the task.
    /// </summary>
    /// <param name="stopToken">The token used for stopping the handler and disconnecting the connection.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Task<Result> RunHandlerAsync(CancellationToken stopToken)
    {
        StartHandler(stopToken);
        return _task!;
    }

    /// <summary>
    /// Start the connection handler task, do not wait for it.
    /// </summary>
    /// <param name="stopToken">The token used for stopping/disconnecting the connection and handling.</param>
    public void StartHandler(CancellationToken stopToken)
    {
        if (_task is not null)
        {
            return;
        }

        _task = HandlerTask(stopToken);
    }

    private async Task<Result> HandlerTask(CancellationToken ct)
    {
        using var reader = new MessagePackStreamReader(_connection.ReadStream, true);
        while (!ct.IsCancellationRequested && _connection.State == ConnectionState.Open)
        {
            try
            {
                var read = await reader.ReadAsync(ct);
                if (!read.HasValue)
                {
                    continue;
                }

                var message = MessagePackSerializer.Typeless.Deserialize
                    (read.Value, _options, ct);

                var result = await _messageHandler.HandleMessageAsync(this, message, ct);

                if (!result.IsSuccess)
                {
                    _logger.LogResultError(result);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception was thrown during deserialization of a message.");
            }
        }

        _connection.Disconnect();
        Closed?.Invoke(this, EventArgs.Empty);
        return Result.FromSuccess();
    }

    /// <summary>
    /// Create a contract for sending a message,
    /// <see cref="ResponseResult"/> will be returned back.
    /// </summary>
    /// <param name="handshake">The handshake request.</param>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <returns>A contract representing send message operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown in case contract is created on the server. Clients do not send responses.</exception>
    public IContract<HandshakeResponse, DefaultStates> ContractHanshake(HandshakeRequest handshake)
    {
        if (_contractor is null)
        {
            throw new InvalidOperationException
            (
                "Contracting is not supported, the other side does not send responses. Only server sends responses back."
            );
        }

        return new ContractBuilder<HandshakeResponse, DefaultStates, NoErrors>(_contractor, DefaultStates.None)
            .SetMoveAction
            (
                DefaultStates.None,
                async (a, ct) =>
                {
                    var result = await SendMessageAsync(handshake, ct);
                    if (!result.IsDefined(out _))
                    {
                        return Result<bool>.FromError(result);
                    }

                    return true;
                },
                DefaultStates.Requested
            )
            .SetMoveFilter<HandshakeResponse>
                (DefaultStates.Requested, DefaultStates.ResponseObtained)
            .SetFillData<HandshakeResponse>(DefaultStates.ResponseObtained, r => r)
            .Build();
    }

    /// <summary>
    /// Create a contract for sending a message,
    /// <see cref="ResponseResult"/> will be returned back.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <returns>A contract representing send message operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown in case contract is created on the server. Clients do not send responses.</exception>
    public IContract<Result, DefaultStates> ContractSendMessage<TMessage>(TMessage message)
    {
        if (_contractor is null)
        {
            throw new InvalidOperationException
            (
                "Contracting is not supported, the other side does not send responses. Only server sends responses back."
            );
        }

        long messageId = 0;
        return new ContractBuilder<Result, DefaultStates, NoErrors>(_contractor, DefaultStates.None)
            .SetMoveAction
            (
                DefaultStates.None,
                async (a, ct) =>
                {
                    var result = await SendMessageAsync(message, ct);
                    if (!result.IsDefined(out messageId))
                    {
                        return Result<bool>.FromError(result);
                    }

                    return true;
                },
                DefaultStates.Requested
            )
            .SetMoveFilter<ResponseResult>
                (DefaultStates.Requested, (r) => r.MessageId == messageId, DefaultStates.ResponseObtained)
            .SetFillData<ResponseResult>(DefaultStates.ResponseObtained, r => r.Result)
            .Build();
    }

    /// <summary>
    /// Send  message to the other end.
    /// </summary>
    /// <param name="message">The message to send. It will be wrapped before sending.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <typeparam name="TMessage">Type of the message to send.</typeparam>
    /// <returns>The id of the message sent or an error.</returns>
    public async Task<Result<long>> SendMessageAsync<TMessage>(TMessage message, CancellationToken ct = default)
    {
        var messageId = _messageId++;
        var messageWrapper = new MessageWrapper<TMessage>(1, messageId, message);

        try
        {
            await MessagePackSerializer.Typeless.SerializeAsync(_connection.WriteStream, messageWrapper, _options, ct);
            await _connection.WriteStream.FlushAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }

        return messageId;
    }
}