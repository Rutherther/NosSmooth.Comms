//
//  CommandResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Data.Messages;
using NosSmooth.Comms.Data.Responders;
using NosSmooth.Core.Client;
using Remora.Results;

namespace NosSmooth.Comms.Inject.MessageResponders;

/// <summary>
/// A responder to <see cref="CommandMessage"/>.
/// </summary>
public class CommandResponder : IMessageResponder<CommandMessage>
{
    private readonly INostaleClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandResponder"/> class.
    /// </summary>
    /// <param name="client">The client.</param>
    public CommandResponder(INostaleClient client)
    {
        _client = client;

    }

    /// <inheritdoc />
    public Task<Result> Respond(CommandMessage message, CancellationToken ct = default)
        => _client.SendCommandAsync(message.Command, ct);
}