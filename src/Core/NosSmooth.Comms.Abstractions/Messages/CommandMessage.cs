//
//  CommandMessage.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Core.Commands;

namespace NosSmooth.Comms.Data.Messages;

public record CommandMessage(ICommand Command);