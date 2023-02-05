//
//  Program.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ConsolePacketLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NosSmooth.Comms.Core.Extensions;
using NosSmooth.Comms.Local;
using NosSmooth.Comms.Local.Extensions;
using NosSmooth.Core.Extensions;
using NosSmooth.Core.Packets;
using NosSmooth.Data.Abstractions;
using NosSmooth.LocalBinding;
using Spectre.Console;

/// <summary>
/// A class with main.
/// </summary>
public static class Program
{
    /// <summary>
    /// A main.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task Main()
    {
        var processInitResults = CommsInjector.CreateNostaleProcesssesBrowsers().ToArray();

        if (processInitResults.Length == 0)
        {
            Console.WriteLine("Could not find any NosTale processes.");
            return;
        }

        foreach (var initResult in processInitResults.Where(x => !x.IsSuccess))
        {
            Console.WriteLine($"There was an error initializing browser manager for a process.");
            Console.WriteLine(initResult.ToFullString());
        }

        var nostaleProcesses = processInitResults.Where(x => x.IsSuccess).Select(x => x.Entity);

        var selectedProcess = AnsiConsole.Prompt
        (
            new SelectionPrompt<NosBrowserManager>()
                .Title("Choose NosTale process to log packets from.")
                .UseConverter
                (
                    x => x.IsInGame
                        ? $"{x.PlayerManager.Player.Name} ({x.Process.ProcessName} - {x.Process.Id})"
                        : $"Not in game ({x.Process.ProcessName} - {x.Process.Id})"
                )
                .AddChoices(nostaleProcesses)
        );

        var host = new HostBuilder()
            .ConfigureLogging(b => b.ClearProviders().AddConsole())
            .ConfigureServices
            (
                s => s
                    .Configure<PacketLoggerOptions>(o => o.ProcessId = selectedProcess.Process.Id)
                    .AddNostaleCore()
                    .AddSingleClientHandling()
                    .AddLocalComms()
                    .AddHostedService<ClientService>()
                    .AddScoped<IRawPacketResponder, EveryPacketResponder>()
            )
            .Build();

        await host.RunAsync();
    }
}