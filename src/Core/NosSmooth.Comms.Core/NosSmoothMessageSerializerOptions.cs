//
//  NosSmoothMessageSerializerOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MessagePack;

namespace NosSmooth.Comms.Core;

/// <summary>
/// Contains options for MessagePack.
/// </summary>
public class NosSmoothMessageSerializerOptions
{
    /// <summary>
    /// Gets or sets the message pack options.
    /// </summary>
    public MessagePackSerializerOptions Options { get; set; } = MessagePackSerializer.Typeless.DefaultOptions;

    /// <summary>
    /// Obtain the options.
    /// </summary>
    /// <param name="options">The options wrapper.</param>
    /// <returns>The options.</returns>
    public static implicit operator MessagePackSerializerOptions(NosSmoothMessageSerializerOptions options)
    {
        return options.Options;
    }
}