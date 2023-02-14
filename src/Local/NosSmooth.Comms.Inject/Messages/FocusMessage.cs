//
//  FocusMessage.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.Comms.Inject.Messages;

/// <summary>
/// Focus the given entity.
/// </summary>
/// <param name="EntityId">The id of the entity, unfocus if null.</param>
public record FocusMessage(long? EntityId);