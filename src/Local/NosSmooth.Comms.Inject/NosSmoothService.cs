//
//  NosSmoothService.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NosSmooth.Core.Client;
using NosSmooth.Core.Extensions;
using NosSmooth.LocalBinding;
using NosSmooth.PacketSerializer.Extensions;
using NosSmooth.PacketSerializer.Packets;
using Remora.Results;

namespace NosSmooth.Comms.Inject;

/// <summary>
/// Nostale client runner.
/// </summary>
public class NosSmoothService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ClientState _state;
    private readonly IPacketTypesRepository _packetTypesRepository;
    private readonly ILogger<NosSmoothService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NosSmoothService"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="state">The state of the application.</param>
    /// <param name="packetTypesRepository">The packet types repository.</param>
    /// <param name="logger">The logger.</param>
    public NosSmoothService
    (
        IServiceProvider services,
        ClientState state,
        IPacketTypesRepository packetTypesRepository,
        ILogger<NosSmoothService> logger
    )
    {
        _services = services;
        _state = state;
        _packetTypesRepository = packetTypesRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ctSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _state.Starting.Token);
        try
        {
            await Task.Delay(-1, ctSource.Token);
        }
        catch (Exception e)
        {
        }

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        var packetResult = _packetTypesRepository.AddDefaultPackets();
        if (!packetResult.IsSuccess)
        {
            _state.InitResult = packetResult;
            _logger.LogError
                ("Could not initialize the packet types (will still work fine if only raw packets are used)");
            _logger.LogResultError(packetResult);
        }

        var bindingResult = _services.GetRequiredService<NosBindingManager>().Initialize();
        if (!bindingResult.IsSuccess)
        {
            _state.InitResult = Result.FromError(bindingResult.Error);
            _logger.LogError("Could not initialize the binding manager.");
            _logger.LogResultError(bindingResult);
            _state.BindingResult = Result.FromError
                (new GenericError("Binding manager not initialized correctly."), bindingResult);
        }
        else
        {
            _state.BindingResult = Result.FromSuccess();
        }

        var nostaleClient = _services.GetRequiredService<INostaleClient>();
        var runTask = nostaleClient.RunAsync(stoppingToken);
        await Task.Delay(10, stoppingToken);
        Result runResult;
        if (runTask.IsCompleted)
        {
            runResult = await runTask;
            _state.InitResult = runResult;
            _state.Started.Cancel();
        }
        else
        {
            _state.IsRunning = true;
            _state.InitResult = Result.FromSuccess();
            _state.Started.Cancel();

            runResult = await runTask;
        }

        if (!runResult.IsSuccess)
        {
            _state.InitResult = runResult;
            _logger.LogResultError(runResult);
        }

        await Task.Delay(-1, stoppingToken);
    }
}