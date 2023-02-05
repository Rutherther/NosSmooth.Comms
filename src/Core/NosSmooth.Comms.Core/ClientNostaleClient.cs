//
//  ClientNostaleClient.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Data;
using NosSmooth.Comms.Data.Messages;
using NosSmooth.Core.Client;
using NosSmooth.Core.Commands;
using NosSmooth.Core.Contracts;
using NosSmooth.Packets;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using Remora.Results;

namespace NosSmooth.Comms.Core;

/// <summary>
/// A NosTale client using <see cref="IClient"/>.
/// </summary>
public class ClientNostaleClient : INostaleClient
{
    private readonly ConnectionHandler _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientNostaleClient"/> class.
    /// </summary>
    /// <param name="connection">The connection handler.</param>
    public ClientNostaleClient
        (ConnectionHandler connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<Result> RunAsync(CancellationToken stopRequested = default)
    {
        _connection.StartHandler(stopRequested);
        return Task.FromResult(Result.FromSuccess());
    }

    /// <inheritdoc />
    public async Task<Result> SendPacketAsync(string packetString, CancellationToken ct = default)
    {
        var messageResponse = await _connection.ContractSendMessage
                (new RawPacketMessage(PacketSource.Client, packetString))
            .WaitForAsync(DefaultStates.ResponseObtained, ct: ct);
        return messageResponse.IsSuccess ? messageResponse.Entity : Result.FromError(messageResponse);
    }

    /// <inheritdoc />
    public async Task<Result> ReceivePacketAsync(string packetString, CancellationToken ct = default)
    {
        var messageResponse = await _connection.ContractSendMessage
                (new RawPacketMessage(PacketSource.Server, packetString))
            .WaitForAsync(DefaultStates.ResponseObtained, ct: ct);
        return messageResponse.IsSuccess ? messageResponse.Entity : Result.FromError(messageResponse);
    }

    /// <inheritdoc />
    public async Task<Result> SendCommandAsync(ICommand command, CancellationToken ct = default)
    {
        var messageResponse = await _connection.ContractSendMessage
                (new CommandMessage(command))
            .WaitForAsync(DefaultStates.ResponseObtained, ct: ct);
        return messageResponse.IsSuccess ? messageResponse.Entity : Result.FromError(messageResponse);
    }
}