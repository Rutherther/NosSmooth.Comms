//
//  ClientNostaleClient.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Data;
using NosSmooth.Comms.Data.Messages;
using NosSmooth.Core.Client;
using NosSmooth.Core.Commands;
using NosSmooth.Packets;
using NosSmooth.PacketSerializer;
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
    public async Task<Result> SendPacketAsync(IPacket packet, CancellationToken ct = default)
    {
        var messageResponse = await _connection.SendMessageAsync
            (new PacketMessage(PacketSource.Client, packet), ct);
        return messageResponse.IsSuccess ? Result.FromSuccess() : Result.FromError(messageResponse);
    }

    /// <inheritdoc />
    public async Task<Result> SendPacketAsync(string packetString, CancellationToken ct = default)
    {
        var messageResponse = await _connection.SendMessageAsync
            (new RawPacketMessage(PacketSource.Client, packetString), ct);
        return messageResponse.IsSuccess ? Result.FromSuccess() : Result.FromError(messageResponse);
    }

    /// <inheritdoc />
    public async Task<Result> ReceivePacketAsync(string packetString, CancellationToken ct = default)
    {
        var messageResponse = await _connection.SendMessageAsync
            (new RawPacketMessage(PacketSource.Server, packetString), ct);
        return messageResponse.IsSuccess ? Result.FromSuccess() : Result.FromError(messageResponse);
    }

    /// <inheritdoc />
    public async Task<Result> ReceivePacketAsync(IPacket packet, CancellationToken ct = default)
    {
        var messageResponse = await _connection.SendMessageAsync
            (new PacketMessage(PacketSource.Server, packet), ct);
        return messageResponse.IsSuccess ? Result.FromSuccess() : Result.FromError(messageResponse);
    }

    /// <inheritdoc />
    public async Task<Result> SendCommandAsync(ICommand command, CancellationToken ct = default)
    {
        var messageResponse = await _connection.SendMessageAsync
            (new CommandMessage(command), ct);
        return messageResponse.IsSuccess ? Result.FromSuccess() : Result.FromError(messageResponse);
    }
}