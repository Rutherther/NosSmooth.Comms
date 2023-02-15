//
//  RunClientRequest.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Data.Messages;
using NosSmooth.LocalBinding.Options;

namespace NosSmooth.Comms.Inject.Messages;

/// <summary>
/// Request to run NosTale client.
/// </summary>
/// <remarks>
/// Should be used if <see cref="HandshakeResponse"/>
/// contained not started .
/// </remarks>
/// <param name="HookOptions"></param>
/// <param name="UnitManagerOptions"></param>
/// <param name="SceneManagerOptions"></param>
/// <param name="NetworkManagerOptions"></param>
/// <param name="PlayerManagerOptions"></param>
/// <param name="PetManagerOptions"></param>
/// <param name="NtClientOptions"></param>
public record RunClientRequest
(
    HookManagerOptions? HookOptions = default,
    UnitManagerOptions? UnitManagerOptions = default,
    SceneManagerOptions? SceneManagerOptions = default,
    NetworkManagerOptions? NetworkManagerOptions = default,
    PlayerManagerOptions? PlayerManagerOptions = default,
    PetManagerOptions? PetManagerOptions = default,
    NtClientOptions? NtClientOptions = default
);