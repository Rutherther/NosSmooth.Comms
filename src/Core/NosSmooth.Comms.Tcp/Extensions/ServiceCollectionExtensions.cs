//
//  ServiceCollectionExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NosSmooth.Comms.Data;

namespace NosSmooth.Comms.Tcp.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds tcp server as <see cref="IServer"/>.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="bindResolver">A resovler used for resolving the ip and port to bind to.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddTcpServer
    (
        this IServiceCollection serviceCollection,
        Func<IServiceProvider, (IPAddress BindAddress, int Port)> bindResolver
    )
    {
        return serviceCollection
            .AddSingleton<IServer>(p => p.GetRequiredService<TcpServer>())
            .AddSingleton<TcpServer>
            (
                p =>
                {
                    var resolved = bindResolver(p);
                    return new TcpServer(resolved.BindAddress, resolved.Port);
                }
            );
    }

    /// <summary>
    /// Adds tcp client as <see cref="IClient"/>.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="connectionResolver">A resovler used for resolving the hostname and port to connect to.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddTcpClient
    (
        this IServiceCollection serviceCollection,
        Func<IServiceProvider, (string Hostname, int Port)> connectionResolver
    )
    {
        return serviceCollection
            .AddSingleton<IClient>(p => p.GetRequiredService<TcpClient>())
            .AddSingleton<TcpClient>
            (
                p =>
                {
                    var resolved = connectionResolver(p);
                    return new TcpClient(resolved.Hostname, resolved.Port);
                }
            );
    }
}