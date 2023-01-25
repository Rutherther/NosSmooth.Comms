//
//  ConnectionInjector.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Data;

namespace NosSmooth.Comms.Core;

/// <summary>
/// Injects connection and connection handler into dependency injection.
/// </summary>
public class ConnectionInjector
{
    /// <summary>
    /// Gets or sets the connection.
    /// </summary>
    public IConnection? Connection { get; set; }

    /// <summary>
    /// Gets or sets the connection handler.
    /// </summary>
    public ConnectionHandler? ConnectionHandler { get; set; }
}