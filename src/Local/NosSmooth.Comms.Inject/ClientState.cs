//
//  ClientState.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Inject.Messages;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Options;
using Remora.Results;

namespace NosSmooth.Comms.Inject;

/// <summary>
/// A state of the client.
/// </summary>
public class ClientState
{
    /// <summary>
    /// Gets the token cancelled upon starting the client (<see cref="RunClientRequest"/> received).
    /// </summary>
    public CancellationTokenSource Starting { get; } = new CancellationTokenSource();

    /// <summary>
    /// Gets the token cancelled when client has started (inside of <see cref="NosSmoothService"/>).
    /// </summary>
    public CancellationTokenSource Started { get; } = new CancellationTokenSource();

    /// <summary>
    /// Gets or sets whether the client is running.
    /// </summary>
    public bool IsRunning { get; internal set; }

    /// <summary>
    /// Gets or sets the result that was obtained upon initialization of the client.
    /// </summary>
    public Result? InitResult { get; internal set; }

    /// <summary>
    /// Gets or sets the result that was obtained from initialization of <see cref="NosBindingManager"/>.
    /// </summary>
    public Result? BindingResult { get; internal set; }

    /// <summary>
    /// Gets or sets hook manager options.
    /// </summary>
    public HookManagerOptions? HookOptions { get; internal set; }

    /// <summary>
    /// Gets or sets unit manager options.
    /// </summary>
    public UnitManagerOptions? UnitManagerOptions { get; internal set; }

    /// <summary>
    /// Gets or sets scene manager options.
    /// </summary>
    public SceneManagerOptions? SceneManagerOptions { get; internal set; }

    /// <summary>
    /// Gets or sets network manager options.
    /// </summary>
    public NetworkManagerOptions? NetworkManagerOptions { get; internal set; }

    /// <summary>
    /// Gets or sets player manager options.
    /// </summary>
    public PlayerManagerOptions? PlayerManagerOptions { get; internal set; }

    /// <summary>
    /// Gets or sets pet manager options.
    /// </summary>
    public PetManagerOptions? PetManagerOptions { get; internal set; }

    /// <summary>
    /// Gets or sets nt client options.
    /// </summary>
    public NtClientOptions? NtClientOptions { get; internal set; }
}