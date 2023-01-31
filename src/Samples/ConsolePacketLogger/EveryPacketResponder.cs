//
//  EveryPacketResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Core.Packets;
using NosSmooth.Packets;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using Remora.Results;

namespace ConsolePacketLogger;

/// <inheritdoc />
public class EveryPacketResponder : IEveryPacketResponder
{
    /// <inheritdoc />
    public Task<Result> Respond<TPacket>(PacketEventArgs<TPacket> packetArgs, CancellationToken ct = default)
        where TPacket : IPacket
    {
        Console.WriteLine((packetArgs.Source == PacketSource.Server ? "[Recv]\t" : "[Sent]\t") + packetArgs.PacketString);
        return Task.FromResult(Result.FromSuccess());
    }
}