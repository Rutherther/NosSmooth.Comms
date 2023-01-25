//
//  MessageHandler.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NosSmooth.Comms.Data;
using NosSmooth.Comms.Data.Messages;
using NosSmooth.Comms.Data.Responders;
using Remora.Results;

namespace NosSmooth.Comms.Core;

/// <summary>
/// An executor of message responders.
/// </summary>
public class MessageHandler
{
    private readonly IServiceProvider _services;
    private readonly bool _respond;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="respond">Whether to respond to the messages.</param>
    public MessageHandler(IServiceProvider services, bool respond)
    {
        _services = services;
        _respond = respond;
    }

    /// <summary>
    /// Handle the given message, call responders.
    /// </summary>
    /// <param name="connection">The connection the message comes from.</param>
    /// <param name="wrappedMessage">The message to handle.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public async Task<Result> HandleMessageAsync
        (ConnectionHandler connection, object wrappedMessage, CancellationToken ct)
    {
        var wrappedType = wrappedMessage.GetType();
        if (!wrappedType.IsGenericType)
        {
            return new GenericError($"Message type is not MessageWrapper<>, but {wrappedType.FullName}");
        }

        if (wrappedType.GetGenericTypeDefinition() != typeof(MessageWrapper<>))
        {
            return new GenericError($"Message type is not MessageWrapper<>, but {wrappedType.FullName}");
        }

        var messageType = wrappedType.GetGenericArguments().First();

        var handleMessageMethod = GetType().GetMethod
            (nameof(GenericHandleMessageAsync), BindingFlags.NonPublic | BindingFlags.Instance)!.MakeGenericMethod
            (new[] { messageType });

        var task = (Task<Result>)handleMessageMethod.Invoke(this, new[] { connection, wrappedMessage, ct })!;
        return await task;
    }

    private async Task<Result> GenericHandleMessageAsync<TMessage>
        (ConnectionHandler connection, MessageWrapper<TMessage> wrappedMessage, CancellationToken ct)
    {
        var data = wrappedMessage.Data;

        await using var scope = _services.CreateAsyncScope();
        var injector = scope.ServiceProvider.GetRequiredService<ConnectionInjector>();
        injector.ConnectionHandler = connection;
        injector.Connection = connection.Connection;

        var responders = scope.ServiceProvider
            .GetServices<IMessageResponder<TMessage>>()
            .Select(x => x.Respond(data, ct));

        var results = (await Task.WhenAll(responders))
            .Where(x => !x.IsSuccess)
            .Cast<IResult>()
            .ToList();

        var result = results.Count switch
        {
            0 => Result.FromSuccess(),
            1 => (Result)results[0],
            _ => new AggregateError(results)
        };

        if (_respond && wrappedMessage.Data is not ResponseResult)
        {
            var response = new ResponseResult(wrappedMessage.MessageId, result);
            var sentMessageResult = await connection.SendMessageAsync(response, ct);
            if (!sentMessageResult.IsSuccess)
            {
                results.Add(sentMessageResult);
                result = results.Count switch
                {
                    0 => Result.FromSuccess(),
                    1 => (Result)results[0],
                    _ => new AggregateError(results)
                };
            }
        }

        return result;
    }
}