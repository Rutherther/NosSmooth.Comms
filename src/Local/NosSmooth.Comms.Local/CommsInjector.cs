//
//  CommsInjector.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NosSmooth.Comms.Core;
using NosSmooth.Comms.Data;
using NosSmooth.Comms.NamedPipes;
using NosSmooth.Injector;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Options;
using Remora.Results;

namespace NosSmooth.Comms.Local;

/// <summary>
/// Injects communication (tcp or named pipes) into a nostale process.
/// </summary>
public class CommsInjector
{
    private readonly IServiceProvider _serviceProvider;
    private readonly NosInjector _injector;
    private readonly NostaleClientResolver _resolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommsInjector"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="injector">The injector.</param>
    /// <param name="resolver">The nostale client resolver.</param>
    public CommsInjector(IServiceProvider serviceProvider, NosInjector injector, NostaleClientResolver resolver)
    {
        _serviceProvider = serviceProvider;
        _injector = injector;
        _resolver = resolver;
    }

    /// <summary>
    /// Find processes that are NosTale and create a <see cref="NosBrowserManager"/> from them.
    /// </summary>
    /// <param name="filterNames">The names to filter when searching the processes. In case the array is empty, look for all processes.</param>
    /// <returns>A list of the NosTale processes.</returns>
    public static IEnumerable<Result<NosBrowserManager>> CreateNostaleProcesssesBrowsers(params string[] filterNames)
    {
        return FindNosTaleProcesses(filterNames)
            .Select
            (
                x =>
                {
                    var manager = new NosBrowserManager
                    (
                        x,
                        new PlayerManagerOptions(),
                        new SceneManagerOptions(),
                        new PetManagerOptions(),
                        new NetworkManagerOptions(),
                        new UnitManagerOptions()
                    );

                    var initResult = manager.Initialize();
                    if (!initResult.IsSuccess)
                    {
                        return Result<NosBrowserManager>.FromError(initResult.Error);
                    }

                    return manager;
                }
            );
    }

    /// <summary>
    /// Find processes that are NosTale.
    /// </summary>
    /// <param name="filterNames">The names to filter when searching the processes. In case the array is empty, look for all processes.</param>
    /// <returns>A list of the NosTale processes.</returns>
    public static IEnumerable<Process> FindNosTaleProcesses(params string[] filterNames)
    {
        var processes = Process.GetProcesses().AsEnumerable();

        if (filterNames.Length > 0)
        {
            processes = processes.Where(x => filterNames.Contains(x.ProcessName));
        }

        return processes
            .Where
            (
                x =>
                {
                    try
                    {
                        return NosBrowserManager.IsProcessNostaleProcess(x);
                    }
                    catch
                    {
                        return false;
                    }
                }
            );
    }

    /// <summary>
    /// Inject NosSmooth.Comms.Inject.dll into the process,
    /// enable tcp server and establish a connection to the server.
    /// </summary>
    /// <returns>The result containing information about the established connection.</returns>
    public Task<Result<Comms>>
        EstablishTcpConnectionAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Inject NosSmooth.Comms.Inject.dll into the process,
    /// enable named pipes server and establish a connection to the server.
    /// </summary>
    /// <param name="process">The process to establish named pipes with.</param>
    /// <param name="stopToken">The token used for stopping the connection.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>The result containing information about the established connection.</returns>
    public async Task<Result<Comms>> EstablishNamedPipesConnectionAsync
        (Process process, CancellationToken stopToken, CancellationToken ct)
    {
        var injectResult = _injector.Inject
        (
            process,
            Path.GetFullPath("NosSmooth.Comms.Inject.dll"),
            "NosSmooth.Comms.Inject.DllMain, NosSmooth.Comms.Inject",
            "EnableNamedPipes"
        );
        if (!injectResult.IsSuccess)
        {
            return Result<Comms>.FromError(injectResult);
        }

        var namedPipeClient = new NamedPipeClient($"NosSmooth_{process.Id}");

        var connectionResult = await namedPipeClient.ConnectAsync(ct);
        if (!connectionResult.IsSuccess)
        {
            return Result<Comms>.FromError(connectionResult);
        }

        var handler = ActivatorUtilities.CreateInstance<ConnectionHandler>
            (_serviceProvider, (IConnection)namedPipeClient);
        handler.StartHandler(stopToken);

        var nostaleClient = _resolver.Resolve(handler);
        return new Comms(process, handler, nostaleClient);
    }
}