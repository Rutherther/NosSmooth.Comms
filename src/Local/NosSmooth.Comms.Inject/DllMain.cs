//
//  DllMain.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NosSmooth.Comms.Core;
using NosSmooth.Comms.Core.Extensions;
using NosSmooth.Comms.Data;
using NosSmooth.Comms.Inject.Extensions;
using NosSmooth.Comms.Inject.MessageResponders;
using NosSmooth.Comms.Inject.PacketResponders;
using NosSmooth.Core.Extensions;
using NosSmooth.Extensions.SharedBinding.Extensions;
using NosSmooth.LocalBinding.Options;
using NosSmooth.LocalClient.Extensions;
using Remora.Results;

namespace NosSmooth.Comms.Inject;

/// <summary>
/// A main entrypoint to NosSmooth local communications.
/// </summary>
public class DllMain
{
    private static bool _consoleAllocated;
    private static IHost? _host;

    /// <summary>
    /// Enable named pipes server.
    /// </summary>
    /// <param name="data">Should be zero, is not used.</param>
    /// <returns>The result, 0 success, 1 failure.</returns>
    [UnmanagedCallersOnly(EntryPoint = "EnableNamedPipes")]
    public static int EnableNamedPipes(nuint data)
        => Main
        (
            host =>
            {
                var manager = host.Services.GetRequiredService<ServerManager>();
                return manager.RunManagerAsync
                    (host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
            }
        );

    /// <summary>
    /// Open a console.
    /// </summary>
    /// <param name="data">Should be zero, is not used.</param>
    /// <returns>The result, 0 success, 1 failure.</returns>
    [UnmanagedCallersOnly(EntryPoint = "OpenConsole")]
    public static int OpenConsole(nuint data)
    {
        WinConsole.Initialize(false);
        return 0;
    }

    /// <summary>
    /// Close a console.
    /// </summary>
    /// <param name="data">Should be zero, is not used.</param>
    /// <returns>The result, 0 success, 1 failure.</returns>
    [UnmanagedCallersOnly(EntryPoint = "CloseConsole")]
    public static int CloseConsole(nuint data)
    {
        WinConsole.Close();
        return 0;
    }

    private static int Main(Func<IHost, Task<Result>> host)
    {
        try
        {
            new Thread
            (
                () =>
                {
                    try
                    {
                        MainEntry(host).GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            ).Start();

            return 0;
        }
        catch (Exception)
        {
            return 1;
        }
    }

    /// <summary>
    /// The entrypoint method.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private static async Task MainEntry(Func<IHost, Task<Result>> host)
    {
        if (_host is not null)
        {
            var result = await host(_host);
            if (!result.IsSuccess)
            {
                _host.Services.GetRequiredService<ILogger<DllMain>>().LogResultError(result);
            }
            return;
        }

        var clientState = new ClientState();
        _host = Host.CreateDefaultBuilder()
            .UseConsoleLifetime()
            .ConfigureLogging
            (
                b =>
                {
                    b
                        .ClearProviders()
                        .AddConsole();
                }
            )
            .ConfigureServices
            (
                s =>
                {
                    s
                        .AddSingleton<ClientState>(_ => clientState)
                        .AddSingleton<CallbackConfigRepository>()
                        .AddManagedNostaleCore()
                        .AddLocalClient()
                        .ShareNosSmooth()
                        .AddNamedPipeServer(p => $"NosSmooth_{Process.GetCurrentProcess().Id}")
                        .AddPacketResponder<EveryPacketResponder>()
                        .AddServerHandling()
                        .AddMessageResponder<CommandResponder>()
                        .AddMessageResponder<FocusResponder>()
                        .AddMessageResponder<FollowResponder>()
                        .AddMessageResponder<HandshakeResponder>()
                        .AddMessageResponder<ConsoleResponder>()
                        .AddMessageResponder<PacketResponder>()
                        .AddMessageResponder<RunClientResponder>()
                        .Configure<HookManagerOptions>(clientState.HookOptions.CopyProperties)
                        .Configure<PlayerManagerOptions>(clientState.PlayerManagerOptions.CopyProperties)
                        .Configure<NetworkManagerOptions>(opts => clientState.NetworkManagerOptions.CopyProperties(opts))
                        .Configure<SceneManagerOptions>(clientState.SceneManagerOptions.CopyProperties)
                        .Configure<UnitManagerOptions>(clientState.UnitManagerOptions.CopyProperties)
                        .Configure<NtClientOptions>(clientState.NtClientOptions.CopyProperties)
                        .Configure<PetManagerOptions>(clientState.PetManagerOptions.CopyProperties);
                    s.AddHostedService<NosSmoothService>();
                }
            ).Build();

        await _host.StartAsync();
        var hostTask = _host.WaitForShutdownAsync();
        var serverTask = host(_host);

        await Task.WhenAll(hostTask, serverTask);
    }
}