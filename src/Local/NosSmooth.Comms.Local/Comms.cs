//
//  Comms.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using NosSmooth.Comms.Core;
using NosSmooth.Core.Client;

namespace NosSmooth.Comms.Local;

/// <summary>
/// A group describing a connection client.
/// </summary>
/// <param name="NosTaleProcess">The process the connection is established with.</param>
/// <param name="Connection">The connection handler wrapping the client connection.</param>
/// <param name="Client">The nostale client for handling and sending packets, commands.</param>
public record Comms(Process NosTaleProcess, ConnectionHandler Connection, INostaleClient Client);