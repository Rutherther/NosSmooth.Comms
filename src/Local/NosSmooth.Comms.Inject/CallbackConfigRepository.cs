//
//  CallbackConfigRepository.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using NosSmooth.Comms.Core;

namespace NosSmooth.Comms.Inject;

/// <summary>
/// A repository containing configurations for given connection handlers.
/// </summary>
public class CallbackConfigRepository
{
    private readonly ConcurrentDictionary<ConnectionHandler, CallbackConfig> _configs;

    /// <summary>
    /// Initializes a new instance of the <see cref="CallbackConfigRepository"/> class.
    /// </summary>
    public CallbackConfigRepository()
    {
        _configs = new ConcurrentDictionary<ConnectionHandler, CallbackConfig>();
    }

    /// <summary>
    /// Get config of the given connection, or default.
    /// </summary>
    /// <param name="connection">The connection to get config of.</param>
    /// <returns>A config for the connection.</returns>
    public CallbackConfig GetConfig(ConnectionHandler connection)
    {
        return _configs.GetValueOrDefault(connection, new CallbackConfig(false, false));
    }

    /// <summary>
    /// Set config of the given connection.
    /// </summary>
    /// <param name="connection">The connection to set config.</param>
    /// <param name="config">The config to set.</param>
    public void SetConfig(ConnectionHandler connection, CallbackConfig config)
    {
        _configs.AddOrUpdate(connection, _ => config, (a, b) => config);
    }
}