//
//  HandshakeResponse.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.Comms.Data.Messages;

/// <summary>
/// A response to <see cref="HandshakeRequest"/>.
/// </summary>
/// <param name="ClientRunning">Whether the client is currently running. Will be false in case of an error or if the client has not been started yet.</param>
/// <param name="InitializationResult">The result obtained from client initialization.</param>
/// <param name="CharacterId">The id of current character, if any.</param>
/// <param name="CharacterName">The name of current character, if any.</param>
public record HandshakeResponse
(
    bool ClientRunning,
    Result? InitializationResult,
    Result? BindingManagerResult
);