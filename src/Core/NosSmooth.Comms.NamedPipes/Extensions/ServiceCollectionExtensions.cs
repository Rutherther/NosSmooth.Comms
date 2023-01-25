//
//  ServiceCollectionExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO.Pipes;
using Microsoft.Extensions.DependencyInjection;
using NosSmooth.Comms.Data;

namespace NosSmooth.Comms.NamedPipes.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a named pipe client as a <see cref="IClient"/>.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="pipeNameResolver">A function for resolving the name of the pipe using service provider.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddNamedPipeClient
        (this IServiceCollection serviceCollection, Func<IServiceProvider, string> pipeNameResolver)
        => serviceCollection
            .AddSingleton<NamedPipeClient>(p => new NamedPipeClient(pipeNameResolver(p)))
            .AddSingleton<IClient>(p => p.GetRequiredService<NamedPipeClient>());

    /// <summary>
    /// Adds a named pipe server as a <see cref="IServer"/>.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="pipeNameResolver">A function for resolving the name of the pipe using service provider.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddNamedPipeServer
        (this IServiceCollection serviceCollection, Func<IServiceProvider, string> pipeNameResolver)
        => serviceCollection
            .AddSingleton<NamedPipeServer>(p => new NamedPipeServer(pipeNameResolver(p)))
            .AddSingleton<IServer>(p => p.GetRequiredService<NamedPipeServer>());
}