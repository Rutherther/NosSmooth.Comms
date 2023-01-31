//
//  MessageHandlerNotFoundError.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.Comms.Core.Errors;

/// <summary>
/// No message handler was found for the received message.
/// </summary>
public record MessageHandlerNotFoundError() : ResultError("Message handler for the given message was not found.");