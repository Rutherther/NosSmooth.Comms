//
//  ClientState.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.Comms.Inject;

/// <summary>
/// A state of the client.
/// </summary>
public class ClientState
{
    /// <summary>
    /// Gets or sets whether the client is running.
    /// </summary>
    public bool IsRunning { get; internal set; }

    /// <summary>
    /// Gets or sets the result that was obtained upon initialization of the client.
    /// </summary>
    public Result? InitResult { get; internal set; }
}