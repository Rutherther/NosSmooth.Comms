//
//  ServiceCollectionExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using NosSmooth.Comms.Core.Extensions;
using NosSmooth.Comms.Local.MessageResponders;
using NosSmooth.Injector;

namespace NosSmooth.Comms.Local.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add <see cref="CommsInjector"/>.
    /// </summary>
    /// <param name="serviceCollection">The service ocllection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddLocalComms(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddMultiClientHandling()
            .AddMessageResponder<PacketResponder>()
            .AddMessageResponder<RawPacketResponder>()
            .AddSingleton<NosInjector>()
            .AddSingleton<CommsInjector>();
    }
}