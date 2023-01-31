//
//  PacketLoggerOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ConsolePacketLogger;

/// <summary>
/// Options telling the process to inject into.
/// </summary>
public class PacketLoggerOptions
{
    /// <summary>
    /// Gets or sets the id of the process to connect to.
    /// </summary>
    public int ProcessId { get; set; }
}