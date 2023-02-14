//
//  FollowMessage.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.Comms.Inject.Messages;

/// <summary>
/// Follow the given entity, unfollow if null.
/// </summary>
/// <param name="EntityId">The id of the entity, unfollow if null.</param>
public record FollowMessage(long? EntityId);