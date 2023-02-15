//
//  RunClientResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Core;
using NosSmooth.Comms.Data.Responders;
using NosSmooth.Comms.Inject.Messages;
using Remora.Results;

namespace NosSmooth.Comms.Inject.MessageResponders;

/// <summary>
/// A responder to <see cref="RunClientRequest"/>.
/// </summary>
public class RunClientResponder : IMessageResponder<RunClientRequest>
{
    private readonly ClientState _state;
    private readonly ConnectionHandler _connectionHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunClientResponder"/> class.
    /// </summary>
    /// <param name="state">The client state.</param>
    /// <param name="connectionHandler">The connection handler wrapper.</param>
    public RunClientResponder
    (
        ClientState state,
        ConnectionHandler connectionHandler
    )
    {
        _state = state;
        _connectionHandler = connectionHandler;
    }

    /// <inheritdoc />
    public async Task<Result> Respond(RunClientRequest request, CancellationToken ct = default)
    {
        _state.HookOptions = request.HookOptions;
        _state.NetworkManagerOptions = request.NetworkManagerOptions;
        _state.NtClientOptions = request.NtClientOptions;
        _state.PetManagerOptions = request.PetManagerOptions;
        _state.PlayerManagerOptions = request.PlayerManagerOptions;
        _state.SceneManagerOptions = request.SceneManagerOptions;
        _state.UnitManagerOptions = request.UnitManagerOptions;
        _state.UnitManagerOptions = request.UnitManagerOptions;

        if (!_state.Starting.IsCancellationRequested)
        { // start the client.
            _state.Starting.Cancel();
        }

        if (!_state.Started.IsCancellationRequested)
        { // wait until the client has started.
            try
            {
                await Task.Delay(-1, _state.Started.Token);
            }
            catch
            {
                // ignored
            }
        }

        var sendMessageResult = await _connectionHandler.SendMessageAsync
            (new RunClientResponse(_state.InitResult, _state.BindingResult), ct);

        return sendMessageResult.IsSuccess ? Result.FromSuccess() : Result.FromError(sendMessageResult);
    }
}