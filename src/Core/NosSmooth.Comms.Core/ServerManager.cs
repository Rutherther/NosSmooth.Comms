//
//  ServerManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NosSmooth.Comms.Data;
using NosSmooth.Core.Extensions;
using Remora.Results;

namespace NosSmooth.Comms.Core;

/// <summary>
/// Manages a server, awaits connections, handles messages.
/// </summary>
public class ServerManager
{
    private readonly IServer _server;
    private readonly MessageHandler _messageHandler;
    private readonly IOptions<NosSmoothMessageSerializerOptions> _options;
    private readonly ILogger<ServerManager> _logger;
    private readonly ILogger<ConnectionHandler> _handlerLogger;
    private readonly List<ConnectionHandler> _connectionHandlers;
    private readonly ReaderWriterLockSlim _readerWriterLock;
    private Task<Result>? _task;
    private CancellationTokenSource? _ctSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerManager"/> class.
    /// </summary>
    /// <param name="server">The server to manage.</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="handlerLogger">The logger for message handler.</param>
    public ServerManager
    (
        IServer server,
        MessageHandler messageHandler,
        IOptions<NosSmoothMessageSerializerOptions> options,
        ILogger<ServerManager> logger,
        ILogger<ConnectionHandler> handlerLogger
    )
    {
        _server = server;
        _connectionHandlers = new List<ConnectionHandler>();
        _readerWriterLock = new ReaderWriterLockSlim();
        _messageHandler = messageHandler;
        _options = options;
        _logger = logger;
        _handlerLogger = handlerLogger;
    }

    /// <summary>
    /// Gets connection handlers.
    /// </summary>
    public IReadOnlyList<ConnectionHandler> ConnectionHandlers
    {
        get
        {
            _readerWriterLock.EnterReadLock();
            try
            {
                return _connectionHandlers.ToArray();
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Run the manager and await the task.
    /// </summary>
    /// <param name="stopToken">The token used for stopping the handler and disconnecting the connection.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Task<Result> RunManagerAsync(CancellationToken stopToken)
    {
        StartManager(stopToken);
        return _task!;
    }

    /// <summary>
    /// Broadcast the given message to all clients.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <returns>A result that may or may not have succeeded.</returns>
    public async Task<Result> BroadcastAsync<TMessage>(TMessage message, CancellationToken ct = default)
    {
        var errors = new List<IResult>();
        foreach (var handler in _connectionHandlers)
        {
            var result = await handler.SendMessageAsync<TMessage>(message, ct);
            if (!result.IsSuccess)
            {
                errors.Add(Result.FromError(result));
            }
        }

        return errors.Count switch
        {
            0 => Result.FromSuccess(),
            1 => (Result)errors[0],
            _ => new AggregateError(errors)
        };
    }

    /// <summary>
    /// Run the handler without awaiting the task.
    /// </summary>
    /// <param name="stopToken">The token used for stopping the handler and disconnecting the connection.</param>
    public void StartManager(CancellationToken stopToken = default)
    {
        if (_task is not null)
        {
            return;
        }

        _ctSource = CancellationTokenSource.CreateLinkedTokenSource(stopToken);
        _task = ManagerTask();
    }

    /// <summary>
    /// Request stop the server.
    /// </summary>
    public void RequestStop()
    {
        _ctSource?.Cancel();
    }

    private async Task<Result> ManagerTask()
    {
        if (_ctSource is null)
        {
            throw new InvalidOperationException("The ct source is not initialized.");
        }

        await _server.ListenAsync(_ctSource!.Token);

        while (!_ctSource.IsCancellationRequested)
        {
            var connectionResult = await _server.WaitForConnectionAsync(_ctSource.Token);
            if (!connectionResult.IsDefined(out var connection))
            {
                _logger.LogResultError(connectionResult);
                continue;
            }

            var handler = new ConnectionHandler
            (
                null,
                connection,
                _messageHandler,
                _options,
                _handlerLogger
            );
            _readerWriterLock.EnterWriteLock();

            try
            {
                _connectionHandlers.Add(handler);
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }

            handler.Closed += (o, e) =>
            {
                _logger.LogInformation("A connection ({ConnectionId}) has been closed", handler.Id);
                _readerWriterLock.EnterWriteLock();
                try
                {
                    _connectionHandlers.Remove(handler);
                }
                finally
                {
                    _readerWriterLock.ExitWriteLock();
                }
            };

            _logger.LogInformation("A connection ({ConnectionId}) has been established", handler.Id);
            handler.StartHandler(_ctSource.Token);
        }

        List<IResult> errors = new List<IResult>();
        foreach (var handler in ConnectionHandlers)
        {
            var handlerResult = await handler.RunHandlerAsync(_ctSource.Token);

            if (!handlerResult.IsSuccess)
            {
                errors.Add(handlerResult);
            }
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            _connectionHandlers.Clear();
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        _server.Close();
        return errors.Count switch
        {
            0 => Result.FromSuccess(),
            1 => (Result)errors[0],
            _ => new AggregateError(errors)
        };
    }
}