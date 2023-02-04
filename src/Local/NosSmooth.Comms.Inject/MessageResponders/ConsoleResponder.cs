//
//  ConsoleResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Data.Responders;
using NosSmooth.Comms.Inject.Messages;
using Remora.Results;

namespace NosSmooth.Comms.Inject.MessageResponders;

/// <inheritdoc />
public class ConsoleResponder : IMessageResponder<ConsoleMessage>
{
    /// <inheritdoc />
    public Task<Result> Respond(ConsoleMessage message, CancellationToken ct = default)
    {
        if (message.Open)
        {
            WinConsole.Initialize(false);
        }
        else
        {
            WinConsole.Close();
        }

        return Task.FromResult(Result.FromSuccess());
    }
}