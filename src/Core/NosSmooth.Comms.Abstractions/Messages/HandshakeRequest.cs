//
//  HandshakeRequest.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.Comms.Data.Messages;

/// <summary>
/// A handshake, signaling use of MessagePack protocol and
/// registering what events to return.
/// </summary>
/// <param name="Identification">Identification of the client, does not have to be unique for clients.</param>
/// <param name="SendRawPackets">Whether to send raw serialized packets using <see cref="RawPacketMessage"/>.</param>
/// <param name="SendDeserializedPackets">Whether to send deserialized packets using <see cref="PacketMessage"/>.</param>
public record HandshakeRequest(string Identification, bool SendRawPackets, bool SendDeserializedPackets);