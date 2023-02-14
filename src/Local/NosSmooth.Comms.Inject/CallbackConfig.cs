//
//  CallbackConfig.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.Comms.Inject;

/// <summary>
/// A config of a client setting what messages to send.
/// </summary>
/// <param name="SendRawPackets">Whether to send raw serialized packets to client upon receive/send.</param>
/// <param name="SendDeserializedPackets">Whether to send deserialized packets to client upon receive/send.</param>
public record CallbackConfig(bool SendRawPackets, bool SendDeserializedPackets);