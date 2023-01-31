//
//  TcpServer.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data;
using System.Net;
using System.Net.Sockets;
using NosSmooth.Comms.Data;
using Remora.Results;

namespace NosSmooth.Comms.Core.Tcp;

/// <summary>
/// A server using tcp.
/// </summary>
public class TcpServer : IServer, IDisposable
{
    private readonly TcpListener _listener;
    private readonly List<IConnection> _connections;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpServer"/> class.
    /// </summary>
    /// <param name="ip">The ip to bind to.</param>
    /// <param name="port">The port to bind to.</param>
    public TcpServer(IPAddress ip, int port)
    {
        _listener = new TcpListener(ip, port);
        _connections = new List<IConnection>();
    }

    /// <inheritdoc />
    public IReadOnlyList<IConnection> Clients => _connections.AsReadOnly();

    /// <inheritdoc />
    public async Task<Result<IConnection>> WaitForConnectionAsync(CancellationToken ct = default)
    {
        var tcpClient = await _listener.AcceptTcpClientAsync(ct);
        var connection = new TcpConnection(tcpClient);

        _connections.Add(connection);
        return connection;
    }

    /// <inheritdoc />
    public Task<Result> ListenAsync(CancellationToken stopToken = default)
    {
        try
        {
            _listener.Start();
            return Task.FromResult(Result.FromSuccess());
        }
        catch (Exception e)
        {
            return Task.FromResult<Result>(e);
        }
    }

    /// <inheritdoc />
    public void Close()
    {
        _listener.Stop();
    }

    private class TcpConnection : IConnection, IDisposable
    {
        private readonly System.Net.Sockets.TcpClient _client;

        public TcpConnection(System.Net.Sockets.TcpClient client)
        {
            _client = client;
        }

        public ConnectionState State => _client.Connected ? ConnectionState.Open : ConnectionState.Closed;

        public Stream ReadStream => _client.GetStream();

        public Stream WriteStream => _client.GetStream();

        public void Disconnect()
        {
            _client.Close();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var connection in _connections.Cast<IDisposable>())
        {
            connection.Dispose();
        }
    }
}