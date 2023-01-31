//
//  RawPacketResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using NosSmooth.Comms.Data.Messages;
using NosSmooth.Comms.Data.Responders;
using NosSmooth.Core.Client;
using NosSmooth.Core.Extensions;
using NosSmooth.Core.Packets;
using NosSmooth.Packets;
using NosSmooth.PacketSerializer;
using NosSmooth.PacketSerializer.Errors;
using Remora.Results;

namespace NosSmooth.Comms.Local.MessageResponders;

/// <summary>
/// Responds to raw packets.
/// </summary>
public class RawPacketResponder : IMessageResponder<RawPacketMessage>
{
    private readonly INostaleClient _client;
    private readonly PacketHandler _packetHandler;
    private readonly IPacketSerializer _serializer;
    private readonly ILogger<RawPacketResponder> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RawPacketResponder"/> class.
    /// </summary>
    /// <param name="client">The nostale client.</param>
    /// <param name="packetHandler">The packet handler.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="logger">The logger.</param>
    public RawPacketResponder(INostaleClient client, PacketHandler packetHandler, IPacketSerializer serializer, ILogger<RawPacketResponder> logger)
    {
        _client = client;
        _packetHandler = packetHandler;
        _serializer = serializer;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Result> Respond(RawPacketMessage message, CancellationToken ct = default)
    {
        var deserializedResult = _serializer.Deserialize(message.Packet, message.Source);
        IPacket packet;

        if (!deserializedResult.IsSuccess)
        {
            if (deserializedResult.Error is not PacketConverterNotFoundError)
            {
                _logger.LogWarning("Could not parse {Packet}. Reason:", message.Packet);
                _logger.LogResultError(deserializedResult);
                packet = new ParsingFailedPacket(deserializedResult, message.Packet);
            }
            else
            {
                packet = new UnresolvedPacket(message.Packet.Split(' ')[0], message.Packet);
            }
        }
        else
        {
            packet = deserializedResult.Entity;
        }

        return _packetHandler.HandlePacketAsync
        (
            _client,
            message.Source,
            packet,
            message.Packet,
            ct
        );
    }
}