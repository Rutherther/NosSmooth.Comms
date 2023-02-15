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
using NosSmooth.Comms.Inject.Messages;
using NosSmooth.Comms.Local;
using NosSmooth.Comms.Local.Extensions;
using NosSmooth.Core.Contracts;
using NosSmooth.Core.Extensions;
using NosSmooth.LocalBinding.Options;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
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
        _injector.OpenConsole(process);
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

        if (!handshakeResponse.ClientRunning)
        {
            _logger.LogInformation("The client is not running, going to start it");
        }
        else
        {
            if (handshakeResponse.InitializationResult is not null
                && !handshakeResponse.InitializationResult.Value.IsSuccess)
            {
                _logger.LogError("Received an error from the Inject assembly that failed on initialization.");
                _logger.LogResultError(handshakeResponse.InitializationResult);
            }
        }

        // should be run only if !handshakeResponse.ClientRunning
        // but will work correctly even if client is already running.
        var clientRunResult = await connection.Connection.ContractRunClient(new RunClientRequest())
            .WaitForAsync(DefaultStates.ResponseObtained, ct: stoppingToken);

        if (!clientRunResult.IsDefined(out var clientRun))
        {
            _logger.LogResultError(clientRunResult);
            _lifetime.StopApplication();
            return;
        }

        if (clientRun.InitializationResult is null)
        {
            _logger.LogError("Huh, the client did not return a result?");
        }

        if (!(clientRun.BindingManagerResult?.IsSuccess ?? true))
        {
            _logger.LogError("Binding manager threw an error.");
            _logger.LogResultError(clientRun.BindingManagerResult);
        }

        if (!(clientRun.InitializationResult?.IsSuccess ?? true))
        {
            _logger.LogResultError(clientRun.InitializationResult);
        }

        _logger.LogInformation($"Connected to NosTale");

        var runResult = await connection.Connection.RunHandlerAsync(stoppingToken);
        if (!runResult.IsSuccess)
        {
            _logger.LogResultError(runResult);
        }

        _lifetime.StopApplication();
    }
}