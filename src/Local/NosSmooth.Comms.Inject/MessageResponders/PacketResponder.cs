//
//  PacketResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Data.Messages;
using NosSmooth.Comms.Data.Responders;
using NosSmooth.Core.Client;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using Remora.Results;

namespace NosSmooth.Comms.Inject.MessageResponders;

/// <summary>
/// A responder to <see cref="RawPacketMessage"/> and <see cref="PacketMessage"/>.
/// </summary>
public class PacketResponder : IMessageResponder<RawPacketMessage>, IMessageResponder<PacketMessage>
{
    private readonly INostaleClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketResponder"/> class.
    /// </summary>
    /// <param name="client">The NosTale client.</param>
    public PacketResponder(INostaleClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public Task<Result> Respond(RawPacketMessage message, CancellationToken ct = default)
    {
        if (message.Source == PacketSource.Client)
        {
            return _client.SendPacketAsync(message.Packet, ct);
        }

        return _client.ReceivePacketAsync(message.Packet, ct);
    }

    /// <inheritdoc />
    public Task<Result> Respond(PacketMessage message, CancellationToken ct = default)
    {
        if (message.Source == PacketSource.Client)
        {
            return _client.SendPacketAsync(message.Packet, ct);
        }

        return _client.ReceivePacketAsync(message.Packet, ct);
    }
}