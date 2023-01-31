//
//  ClientService.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NosSmooth.Comms.Core;
using NosSmooth.Comms.Data.Messages;
using NosSmooth.Comms.Local;
using NosSmooth.Core.Contracts;
using NosSmooth.Core.Extensions;
using NosSmooth.PacketSerializer.Extensions;
using NosSmooth.PacketSerializer.Packets;

namespace ConsolePacketLogger;

/// <inheritdoc />
public class ClientService : BackgroundService
{
    private readonly IPacketTypesRepository _packetTypesRepository;
    private readonly CommsInjector _injector;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ClientService> _logger;
    private readonly PacketLoggerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientService"/> class.
    /// </summary>
    /// <param name="packetTypesRepository">The packet types repository.</param>
    /// <param name="injector">The injector.</param>
    /// <param name="lifetime">The lifetime.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    public ClientService
    (
        IPacketTypesRepository packetTypesRepository,
        CommsInjector injector,
        IHostApplicationLifetime lifetime,
        ILogger<ClientService> logger,
        IOptions<PacketLoggerOptions> options
    )
    {
        _packetTypesRepository = packetTypesRepository;
        _injector = injector;
        _lifetime = lifetime;
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var packetsResult = _packetTypesRepository.AddDefaultPackets();
        if (!packetsResult.IsSuccess)
        {
            _logger.LogResultError(packetsResult);
        }

        var process = Process.GetProcessById(_options.ProcessId);
        var connectionResult = await _injector.EstablishNamedPipesConnectionAsync
            (process, stoppingToken, stoppingToken);
        if (!connectionResult.IsDefined(out var connection))
        {
            _logger.LogResultError(connectionResult);
            _lifetime.StopApplication();
            return;
        }

        var handshakeResponseResult = await connection.Connection
            .ContractHanshake(new HandshakeRequest("ConsolePacketLogger example", true, false))
            .WaitForAsync(DefaultStates.ResponseObtained, ct: stoppingToken);

        if (!handshakeResponseResult.IsDefined(out var handshakeResponse))
        {
            _logger.LogResultError(handshakeResponseResult);
            _lifetime.StopApplication();
            return;
        }

        _logger.LogInformation
        (
            $"Connected to {handshakeResponse.CharacterName ?? "Not in game"} ({handshakeResponse.CharacterId?.ToString() ?? "Not in game"})"
        );

        var runResult = await connection.Connection.RunHandlerAsync(stoppingToken);
        if (!runResult.IsSuccess)
        {
            _logger.LogResultError(runResult);
        }

        _lifetime.StopApplication();
    }
}