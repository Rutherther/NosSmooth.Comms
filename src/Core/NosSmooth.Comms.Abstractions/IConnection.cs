//
//  IConnection.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data;

namespace NosSmooth.Comms.Data;

/// <summary>
/// A connection, either a client or a server connection.
/// </summary>
public interface IConnection
{
    /// <summary>
    /// Gets the state of the connection.
    /// </summary>
    public ConnectionState State { get; }

    /// <summary>
    /// Gets the stream used for reading the data received.
    /// </summary>
    public Stream ReadStream { get; }

    /// <summary>
    /// Gets the stream used for writing data.
    /// </summary>
    public Stream WriteStream { get; }

    /// <summary>
    /// Disconnect, close the connection.
    /// </summary>
    public void Disconnect();
}