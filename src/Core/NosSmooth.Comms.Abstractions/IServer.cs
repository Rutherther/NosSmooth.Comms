//
//  IServer.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Remora.Results;

namespace NosSmooth.Comms.Data;

/// <summary>
/// An abstraction for a server.
/// </summary>
public interface IServer
{
    /// <summary>
    /// Gets the clients connected to the server.
    /// </summary>
    public IReadOnlyList<IConnection> Clients { get; }

    /// <summary>
    /// Listen for a new connection and wait for it.
    /// </summary>
    /// <param name="ct">The cancellation token for cancelling the operation.</param>
    /// <returns>A client connection, returned after the client has connected.</returns>
    public Task<Result<IConnection>> WaitForConnectionAsync(CancellationToken ct = default);

    /// <summary>
    /// Start the server.
    /// </summary>
    /// <param name="stopToken">The token used for stopping the server. <see cref="CloseAsync"/> may also be used.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Task<Result> ListenAsync(CancellationToken stopToken = default);

    /// <summary>
    /// Close all connections, stop listening.
    /// </summary>
    public void Close();
}