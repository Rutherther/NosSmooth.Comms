//
//  ObjectExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;

namespace NosSmooth.Comms.Inject.Extensions;

/// <summary>
/// Extension methods for object.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Extension for 'Object' that copies the properties to a destination object.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <typeparam name="T">The type.</typeparam>
    public static void CopyProperties<T>(this T? source, T destination)
        where T : notnull
    {
        if (source is null)
        {
            return;
        }

        var properties = source.GetType().GetProperties();

        foreach (var p in properties.Where(prop => prop.CanRead && prop.CanWrite))
        {
            object? copyValue = p.GetValue(source);
            p.SetValue(destination, copyValue);
        }
    }
}