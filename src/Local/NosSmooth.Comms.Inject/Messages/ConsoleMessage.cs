//
//  ConsoleMessage.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.Comms.Inject.Messages;

/// <summary>
/// A message used for opening or closing console.
/// </summary>
/// <param name="Open">Whether to open the console.</param>
public record ConsoleMessage(bool Open);