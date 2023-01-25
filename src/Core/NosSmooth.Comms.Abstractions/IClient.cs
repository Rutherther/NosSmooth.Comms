//
//  IClient.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data;
using Remora.Results;

namespace NosSmooth.Comms.Data;

/// <summary>
/// An abstraction for a client connection.
/// </summary>
public interface IClient : IConnection
{
    /// <summary>
    /// Connect to the server.
    /// </summary>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Task<Result> ConnectAsync(CancellationToken ct = default);
}