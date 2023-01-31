//
//  ServiceCollectionExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NosSmooth.Comms.Core.NamedPipes;
using NosSmooth.Comms.Core.Responders;
using NosSmooth.Comms.Core.Tcp;
using NosSmooth.Comms.Data;
using NosSmooth.Comms.Data.Responders;
using NosSmooth.Core.Client;

namespace NosSmooth.Comms.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds server handling (<see cref="MessageHandler"/> and <see cref="ServerManager"/>).
    /// </summary>
    /// <remarks>
    /// The specific server has to be added separately as <see cref="IServer"/>.
    /// </remarks>
    /// <param name="serviceCollection">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddServerHandling(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddNosSmoothResolverOptions()
            .AddSingleton<MessageHandler>(p => new MessageHandler(p, true))
            .AddSingleton<ServerManager>()
            .AddInjecting();

    /// <summary>
    /// Adds handling for a single client.
    /// </summary>
    /// <remarks>
    /// The specific client has to be added separately as <see cref="IConnection"/>.
    /// </remarks>
    /// <param name="serviceCollection">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddSingleClientHandling(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddInjecting()
            .AddNosSmoothResolverOptions()
            .AddMessageResponder<ResponseResultResponder>()
            .AddSingleton<NostaleClientResolver>()
            .AddSingleton<IConnection>(p => p.GetRequiredService<IClient>())
            .AddSingleton<MessageHandler>(p => new MessageHandler(p, false))
            .AddScoped<ConnectionHandler>()
            .AddScoped<INostaleClient, ClientNostaleClient>();

    /// <summary>
    /// Add handling for multiple clients.
    /// </summary>
    /// <remarks>
    /// The clients should not be inside of the provider.
    /// Initialize clients outside of the provider and use the
    /// provider for injecting connection handler and nostale client.
    /// Nostale client will be created automatically if connection is injected successfully.
    /// Connection will be injected when calling message handler with the specific connection.
    ///
    /// Connection may be injected by setting <see cref="ConnectionInjector"/> properties in a scope.
    /// </remarks>
    /// <param name="serviceCollection">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMultiClientHandling(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddNosSmoothResolverOptions()
            .AddSingleton<NostaleClientResolver>()
            .AddSingleton<MessageHandler>(p => new MessageHandler(p, false))
            .AddInjecting()
            .AddScoped<INostaleClient>
                (p => p.GetRequiredService<NostaleClientResolver>().Resolve(p.GetRequiredService<ConnectionHandler>()));

    /// <summary>
    /// Add <see cref="NosSmoothMessageSerializerOptions"/> with default NosSmooth options.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddNosSmoothResolverOptions(this IServiceCollection serviceCollection)
        => serviceCollection
            .Configure<NosSmoothMessageSerializerOptions>
                (o => o.Options = o.Options.WithResolver(NosSmoothResolver.Instance));

    /// <summary>
    /// Adds a message responder.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <typeparam name="TResponder">The type of the responder.</typeparam>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMessageResponder<TResponder>(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddMessageResponder(typeof(TResponder));
    }

    /// <summary>
    /// Adds a message responder.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="responderType">The type of the responder.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMessageResponder(this IServiceCollection serviceCollection, Type responderType)
    {
        if (serviceCollection.Any(x => x.ImplementationType == responderType))
        { // already added... assuming every packet responder was added even though that may not be the case.
            return serviceCollection;
        }

        if (!responderType.GetInterfaces().Any
            (
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageResponder<>)
            ))
        {
            throw new ArgumentException
            (
                $"{nameof(responderType)} should implement IMessageResponder.",
                nameof(responderType)
            );
        }

        var responderTypeInterfaces = responderType.GetInterfaces();
        var responderInterfaces = responderTypeInterfaces.Where
        (
            r => r.IsGenericType && r.GetGenericTypeDefinition() == typeof(IMessageResponder<>)
        );

        foreach (var responderInterface in responderInterfaces)
        {
            serviceCollection.AddScoped(responderInterface, responderType);
        }

        return serviceCollection;
    }

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

    private static IServiceCollection AddInjecting(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddScoped<ConnectionInjector>()
            .AddScoped<ConnectionHandler>
            (
                p =>
                {
                    var handler = p.GetRequiredService<ConnectionInjector>().ConnectionHandler;
                    if (handler is null)
                    {
                        throw new InvalidOperationException("Connection handler was requested, but is not injected.");
                    }

                    return handler;
                }
            )
            .AddScoped<IConnection>
            (
                p =>
                {
                    var connection = p.GetRequiredService<ConnectionInjector>().Connection;
                    if (connection is null)
                    {
                        throw new InvalidOperationException("Connection was requested, but is not injected.");
                    }

                    return connection;
                }
            );
}