//
//  NameStringFormatter.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using MessagePack;
using MessagePack.Formatters;
using NosSmooth.PacketSerializer.Abstractions.Common;

namespace NosSmooth.Comms.Core.Formatters;

/// <summary>
/// A formatter for <see cref="NameString"/>.
/// </summary>
public class NameStringFormatter : IMessagePackFormatter<NameString?>
{
    /// <inheritdoc />
    public void Serialize(ref MessagePackWriter writer, NameString? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(value.Name);
        writer.WriteString(bytes);
    }

    /// <inheritdoc />
    public NameString? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);
        var name = reader.ReadString();

        reader.Depth--;
        return NameString.FromString(name);
    }
}