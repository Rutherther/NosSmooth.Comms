//
//  FocusResponder.cs
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
/// A resopnder to <see cref="FocusResponder"/>.
/// </summary>
public class FocusResponder : IMessageResponder<FocusMessage>
{
    private readonly NosBrowserManager _browserManager;
    private readonly NosThreadSynchronizer _synchronizer;
    private readonly Optional<IEntityFocusHook> _focusHook;

    /// <summary>
    /// Initializes a new instance of the <see cref="FocusResponder"/> class.
    /// </summary>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="synchronizer">The synchronizer.</param>
    /// <param name="focusHook">The focus hook.</param>
    public FocusResponder
        (NosBrowserManager browserManager, NosThreadSynchronizer synchronizer, Optional<IEntityFocusHook> focusHook)
    {
        _browserManager = browserManager;
        _synchronizer = synchronizer;
        _focusHook = focusHook;
    }

    /// <inheritdoc />
    public async Task<Result> Respond(FocusMessage message, CancellationToken ct = default)
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
            () => _focusHook.MapResult
            (
                hook => hook.WrapperFunction.MapResult
                (
                    wrapper =>
                    {
                        wrapper(entity);
                        return Result.FromSuccess();
                    }
                )
            ),
            ct
        );
    }
}