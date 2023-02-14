//
//  PacketMessage.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Packets;
using NosSmooth.PacketSerializer.Abstractions.Attributes;

namespace NosSmooth.Comms.Data.Messages;

/// <summary>
/// A message containing deserialized packet.
/// </summary>
/// <remarks>
/// May be used for sending or receiving a packet.
/// </remarks>
/// <param name="Source">The source the packet comes from.</param>
/// <param name="Packet">The deserialized packet.</param>
public record PacketMessage(PacketSource Source, IPacket Packet);