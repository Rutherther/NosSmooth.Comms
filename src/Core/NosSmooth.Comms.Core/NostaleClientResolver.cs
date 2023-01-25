//
//  NostaleClientResolver.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using NosSmooth.Comms.Data;
using NosSmooth.Core.Client;

namespace NosSmooth.Comms.Core;

/// <summary>
/// Resolves <see cref="IConnection"/>s into <see cref="INostaleClient"/>s.
/// </summary>
/// <remarks>
/// Clients will be connected in case the client is not registered yet.
/// If you wish to register the client yourself, use <see cref="RegisterClient"/>.
/// </remarks>
public class NostaleClientResolver
{
    private readonly IServiceProvider _services;
    private readonly ConcurrentDictionary<IConnection, INostaleClient> _clients;

    /// <summary>
    /// Initializes a new instance of the <see cref="NostaleClientResolver"/> class.
    /// </summary>
    /// <param name="services">The service provider.</param>
    public NostaleClientResolver(IServiceProvider services)
    {
        _services = services;
        _clients = new ConcurrentDictionary<IConnection, INostaleClient>();
    }

    /// <summary>
    /// Resolve the connection handler into nostale client.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>The resolved client.</returns>
    public INostaleClient Resolve(ConnectionHandler connection)
    {
        if (!_clients.ContainsKey(connection.Connection))
        {
            RegisterClient(connection, ActivatorUtilities.CreateInstance<ClientNostaleClient>(_services, connection));
        }

        return _clients[connection.Connection];
    }

    /// <summary>
    /// Register the given client for the given connection.
    /// </summary>
    /// <param name="connection">The connection handler.</param>
    /// <param name="client">The client to register for the given handler.</param>
    public void RegisterClient(ConnectionHandler connection, INostaleClient client)
    {
        _clients[connection.Connection] = client;
    }
}