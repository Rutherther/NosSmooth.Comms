//
//  NosSmoothService.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NosSmooth.Core.Client;
using NosSmooth.Core.Extensions;
using NosSmooth.LocalBinding;
using NosSmooth.PacketSerializer.Extensions;
using NosSmooth.PacketSerializer.Packets;

namespace NosSmooth.Comms.Inject;

/// <summary>
/// Nostale client runner.
/// </summary>
public class NosSmoothService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IPacketTypesRepository _packetTypesRepository;
    private readonly NosBindingManager _bindingManager;
    private readonly IHostLifetime _lifetime;
    private readonly ILogger<NosSmoothService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NosSmoothService"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="packetTypesRepository">The packet types repository.</param>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="lifetime">The lifetime.</param>
    /// <param name="logger">The logger.</param>
    public NosSmoothService
    (
        IServiceProvider services,
        IPacketTypesRepository packetTypesRepository,
        NosBindingManager bindingManager,
        IHostLifetime lifetime,
        ILogger<NosSmoothService> logger
    )
    {
        _services = services;
        _packetTypesRepository = packetTypesRepository;
        _bindingManager = bindingManager;
        _lifetime = lifetime;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var packetResult = _packetTypesRepository.AddDefaultPackets();
        if (!packetResult.IsSuccess)
        {
            _logger.LogResultError(packetResult);
            return;
        }

        var bindingResult = _bindingManager.Initialize();
        if (!bindingResult.IsSuccess)
        {
            _logger.LogResultError(bindingResult);
            return;
        }

        var nostaleClient = _services.GetRequiredService<INostaleClient>();
        var runResult = await nostaleClient.RunAsync(stoppingToken);
        if (!runResult.IsSuccess)
        {
            _logger.LogResultError(runResult);
            await _lifetime.StopAsync(default);
        }
    }
}