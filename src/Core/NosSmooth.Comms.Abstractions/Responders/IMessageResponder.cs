//
//  IMessageResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.Comms.Data.Responders;

/// <summary>
/// A responder to a received message from client or server.
/// </summary>
/// <typeparam name="TMessage">The type of the message to respond to.</typeparam>
public interface IMessageResponder<TMessage>
{
    /// <summary>
    /// Respond to the given message.
    /// </summary>
    /// <param name="message">The message received.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Task<Result> Respond(TMessage message, CancellationToken ct = default);
}