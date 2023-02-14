//
//  CommandMessage.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Core.Commands;

namespace NosSmooth.Comms.Data.Messages;

/// <summary>
/// Send a command.
/// </summary>
/// <param name="Command">The command to send.</param>
public record CommandMessage(ICommand Command);