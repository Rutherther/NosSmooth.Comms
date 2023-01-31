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
using NosSmooth.Comms.Inject.MessageResponders;
using NosSmooth.Comms.Inject.PacketResponders;
using NosSmooth.Comms.NamedPipes;
using NosSmooth.Comms.NamedPipes.Extensions;
using NosSmooth.Core.Extensions;
using NosSmooth.Extensions.SharedBinding.Extensions;
using NosSmooth.LocalClient.Extensions;
using Remora.Results;

namespace NosSmooth.Comms.Inject;

/// <summary>
/// A main entrypoint to NosSmooth local communications.
/// </summary>
public class DllMain
{
    private static IHost? _host;

    /// <summary>
    /// Allocate console.
    /// </summary>
    /// <returns>Whether the operation was successful.</returns>
    [DllImport("kernel32")]
    public static extern bool AllocConsole();

    /// <summary>
    /// Enable named pipes server.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "EnableNamedPipes")]
    public static void EnableNamedPipes()
    {
        Main
        (
            host =>
            {
                var manager = host.Services.GetRequiredService<ServerManager>();
                return manager.RunManagerAsync(host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
            }
        );
    }

    private static void Main(Func<IHost, Task<Result>> host)
    {
        AllocConsole();
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
                        .AddSingleton<CallbackConfigRepository>()
                        .AddNostaleCore()
                        .AddLocalClient()
                        .ShareNosSmooth()
                        .AddNamedPipeServer(p => $"NosSmooth_{Process.GetCurrentProcess().Id}")
                        .AddPacketResponder<EveryPacketResponder>()
                        .AddServerHandling()
                        .AddMessageResponder<CommandResponder>()
                        .AddMessageResponder<FocusResponder>()
                        .AddMessageResponder<FollowResponder>()
                        .AddMessageResponder<HandshakeResponder>()
                        .AddMessageResponder<PacketResponder>();
                    s.AddHostedService<NosSmoothService>();
                }
            ).Build();

        await _host.StartAsync();
        var hostTask = _host.RunAsync();
        var serverTask = host(_host);

        await Task.WhenAll(hostTask, serverTask);
    }
}