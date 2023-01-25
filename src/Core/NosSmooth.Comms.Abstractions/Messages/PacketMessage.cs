﻿//
//  PacketMessage.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Packets;
using NosSmooth.PacketSerializer.Abstractions.Attributes;

namespace NosSmooth.Comms.Data.Messages;

public record PacketMessage(PacketSource Source, IPacket Packet);