//
//  NosSmoothResolver.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MessagePack;
using NosSmooth.Comms.Core.Formatters;

namespace NosSmooth.Comms.Core;

/// <summary>
/// A class for obtaining MessagePack formatter resolver.
/// </summary>
public class NosSmoothResolver
{
    /// <summary>
    /// Gets a formatter resolver for NosSmooth messages.
    /// </summary>
    public static IFormatterResolver Instance => MessagePack.Resolvers.CompositeResolver.Create
    (
        new[]
        {
            new NameStringFormatter()
        },
        new[]
        {
            MessagePackSerializer.Typeless.DefaultOptions.Resolver,
        }
    );
}