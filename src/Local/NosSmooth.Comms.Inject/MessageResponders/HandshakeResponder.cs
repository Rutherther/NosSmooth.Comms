//
//  HandshakeResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using NosSmooth.Comms.Core;
using NosSmooth.Comms.Data.Messages;
using NosSmooth.Comms.Data.Responders;
using NosSmooth.LocalBinding;
using Remora.Results;

namespace NosSmooth.Comms.Inject.MessageResponders;

/// <summary>
/// A responder to <see cref="HandshakeRequest"/>.
/// </summary>
public class HandshakeResponder : IMessageResponder<HandshakeRequest>
{
    private readonly ClientState _state;
    private readonly NosBrowserManager _browserManager;
    private readonly ConnectionHandler _connectionHandler;
    private readonly CallbackConfigRepository _config;
    private readonly ILogger<HandshakeResponder> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HandshakeResponder"/> class.
    /// </summary>
    /// <param name="state">The state of the application.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="connectionHandler">The connection handler.</param>
    /// <param name="config">The config.</param>
    /// <param name="logger">The logger.</param>
    public HandshakeResponder
    (
        ClientState state,
        NosBrowserManager browserManager,
        ConnectionHandler connectionHandler,
        CallbackConfigRepository config,
        ILogger<HandshakeResponder> logger
    )
    {
        _state = state;
        _browserManager = browserManager;
        _connectionHandler = connectionHandler;
        _config = config;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> Respond(HandshakeRequest message, CancellationToken ct = default)
    {
        var config = new CallbackConfig(message.SendRawPackets, message.SendDeserializedPackets);
        _config.SetConfig(_connectionHandler, config);

        string? playerName = null;
        long? playerId = null;

        if (_browserManager.PlayerManager.TryGet(out var playerManager) &&
            _browserManager.IsInGame.TryGet(out var isInGame) &&
            isInGame)
        {
            playerName = playerManager.Player.Name;
            playerId = playerManager.PlayerId;
        }

        var result = await _connectionHandler.SendMessageAsync
        (
            new HandshakeResponse(_state.IsRunning, _state.InitResult, playerId, playerName),
            ct
        );

        _logger.LogInformation
        (
            "Handshaked with {Identification}! (connection {ConnectionID})",
            message.Identification,
            _connectionHandler.Id
        );

        return result.IsSuccess ? Result.FromSuccess() : Result.FromError(result);
    }
}