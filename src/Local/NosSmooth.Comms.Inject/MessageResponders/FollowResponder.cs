//
//  FollowResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Data.Responders;
using NosSmooth.Comms.Inject.Messages;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Structs;
using Remora.Results;

namespace NosSmooth.Comms.Inject.MessageResponders;

/// <summary>
/// A responder to <see cref="FollowMessage"/>.
/// </summary>
public class FollowResponder : IMessageResponder<FollowMessage>
{
    private readonly NosBrowserManager _browserManager;
    private readonly NosThreadSynchronizer _synchronizer;
    private readonly IHookManager _hookManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="FollowResponder"/> class.
    /// </summary>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="synchronizer">The synchronizer.</param>
    /// <param name="hookManager">The hook manager.</param>
    public FollowResponder
        (NosBrowserManager browserManager, NosThreadSynchronizer synchronizer, IHookManager hookManager)
    {
        _browserManager = browserManager;
        _synchronizer = synchronizer;
        _hookManager = hookManager;
    }

    /// <inheritdoc />
    public async Task<Result> Respond(FollowMessage message, CancellationToken ct = default)
    {
        MapBaseObj? entity = null;
        if (!_browserManager.SceneManager.TryGet(out var sceneManager))
        {
            return new OptionalNotPresentError(nameof(SceneManager));
        }

        if (message.EntityId is not null)
        {
            var entityResult = sceneManager.FindEntity(message.EntityId.Value);

            if (!entityResult.IsDefined(out entity))
            {
                return Result.FromError(new NotFoundError($"Entity with id {message.EntityId} not found."));
            }
        }

        return await _synchronizer.SynchronizeAsync
        (
            () =>
            {
                if (entity is null)
                {
                    return _hookManager.EntityUnfollow.MapResult
                    (
                        unfollow => unfollow.WrapperFunction.MapResult
                        (
                            wrapper =>
                            {
                                wrapper();
                                return Result.FromSuccess();
                            }
                        )
                    );
                }

                return _hookManager.EntityFollow.MapResult
                (
                    unfollow => unfollow.WrapperFunction.MapResult
                    (
                        wrapper =>
                        {
                            wrapper(entity);
                            return Result.FromSuccess();
                        }
                    )
                );
            },
            ct
        );
    }
}