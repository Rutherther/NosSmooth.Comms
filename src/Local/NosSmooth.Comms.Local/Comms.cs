//
//  Comms.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using NosSmooth.Comms.Core;
using NosSmooth.Core.Client;

namespace NosSmooth.Comms.Local;

public record Comms(Process NosTaleProcess, ConnectionHandler Connection, INostaleClient Client);