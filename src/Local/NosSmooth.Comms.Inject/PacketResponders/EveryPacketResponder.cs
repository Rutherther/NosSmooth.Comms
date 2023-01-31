//
//  EveryPacketResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Core;
using NosSmooth.Comms.Data.Messages;
using NosSmooth.Core.Packets;
using NosSmooth.Packets;
using Remora.Results;

namespace NosSmooth.Comms.Inject.PacketResponders;

/// <inheritdoc />
public class EveryPacketResponder : IEveryPacketResponder
{
    private readonly CallbackConfigRepository _config;
    private readonly ServerManager _serverManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="EveryPacketResponder"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="serverManager">The server manager.</param>
    public EveryPacketResponder(CallbackConfigRepository config, ServerManager serverManager)
    {
        _config = config;
        _serverManager = serverManager;
    }

    /// <inheritdoc />
    public async Task<Result> Respond<TPacket>(PacketEventArgs<TPacket> packetArgs, CancellationToken ct = default)
        where TPacket : IPacket
    {
        var errors = new List<IResult>();

        foreach (var connectionHandler in _serverManager.ConnectionHandlers)
        {
            var config = _config.GetConfig(connectionHandler);
            if (config.SendRawPackets)
            {
                var result = await connectionHandler.SendMessageAsync
                    (new RawPacketMessage(packetArgs.Source, packetArgs.PacketString), ct);

                if (!result.IsSuccess)
                {
                    errors.Add(Result.FromError(result));
                }
            }

            if (config.SendDeserializedPackets)
            {
                var result = await connectionHandler.SendMessageAsync(new PacketMessage(packetArgs.Source, packetArgs.Packet), ct);

                if (!result.IsSuccess)
                {
                    errors.Add(Result.FromError(result));
                }
            }
        }

        return errors.Count switch
        {
            0 => Result.FromSuccess(),
            1 => (Result)errors[0],
            _ => new AggregateError(errors)
        };
    }
}