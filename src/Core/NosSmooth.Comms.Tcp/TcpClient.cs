//
//  TcpClient.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data;
using NosSmooth.Comms.Data;
using Remora.Results;

namespace NosSmooth.Comms.Tcp;

/// <summary>
/// A client using tcp.
/// </summary>
public class TcpClient : IClient
{
    private readonly string _hostname;
    private readonly int _port;
    private readonly System.Net.Sockets.TcpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpClient"/> class.
    /// </summary>
    /// <param name="hostname">The hostname to connet to.</param>
    /// <param name="port">Teh port to connect to.</param>
    public TcpClient(string hostname, int port)
    {
        _hostname = hostname;
        _port = port;
        _client = new System.Net.Sockets.TcpClient();
    }

    /// <inheritdoc />
    public ConnectionState State => _client.Connected ? ConnectionState.Open : ConnectionState.Closed;

    /// <inheritdoc />
    public Stream ReadStream => _client.GetStream();

    /// <inheritdoc />
    public Stream WriteStream => _client.GetStream();

    /// <inheritdoc />
    public void Disconnect()
    {
        _client.Close();
    }

    /// <inheritdoc />
    public async Task<Result> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            await _client.ConnectAsync(_hostname, _port, ct);
            return Result.FromSuccess();
        }
        catch (Exception e)
        {
            return e;
        }
    }
}