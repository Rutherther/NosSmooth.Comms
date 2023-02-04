//
//  HandshakeResponse.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.Comms.Data.Messages;

public record HandshakeResponse
(
    bool ClientRunning,
    Result? InitializationErrorfulResult,
    long? CharacterId,
    string? CharacterName
);