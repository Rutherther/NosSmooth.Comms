//
//  PacketResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Data.Messages;
using NosSmooth.Comms.Data.Responders;
using NosSmooth.Core.Client;
using NosSmooth.Core.Packets;
using NosSmooth.PacketSerializer;
using Remora.Results;

namespace NosSmooth.Comms.Local.MessageResponders;

/// <summary>
/// Responds to deserialized packets.
/// </summary>
public class PacketResponder : IMessageResponder<PacketMessage>
{
    private readonly INostaleClient _client;
    private readonly IPacketHandler _packetHandler;
    private readonly IPacketSerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketResponder"/> class.
    /// </summary>
    /// <param name="client">The nostale client.</param>
    /// <param name="packetHandler">The packet handler.</param>
    /// <param name="serializer">The serializer.</param>
    public PacketResponder(INostaleClient client, IPacketHandler packetHandler, IPacketSerializer serializer)
    {
        _client = client;
        _packetHandler = packetHandler;
        _serializer = serializer;
    }

    /// <inheritdoc />
    public Task<Result> Respond(PacketMessage message, CancellationToken ct = default)
    {
        var serializedResult = _serializer.Serialize(message.Packet);
        if (!serializedResult.IsDefined(out var serialized))
        {
            return Task.FromResult(Result.FromError(serializedResult));
        }

        return _packetHandler.HandlePacketAsync
        (
            _client,
            message.Source,
            serialized,
            ct
        );
    }
}