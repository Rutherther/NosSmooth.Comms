//
//  NamedPipeServer.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data;
using System.IO.Pipes;
using System.Xml;
using NosSmooth.Comms.Data;
using Remora.Results;

namespace NosSmooth.Comms.NamedPipes;

/// <summary>
/// A server using named pipes.
/// </summary>
public class NamedPipeServer : IServer
{
    private readonly List<IConnection> _connections;
    private readonly ReaderWriterLockSlim _readerWriterLock;
    private readonly string _pipeName;
    private bool _listening;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedPipeServer"/> class.
    /// </summary>
    /// <param name="pipeName">The name of the pipe.</param>
    public NamedPipeServer(string pipeName)
    {
        _readerWriterLock = new ReaderWriterLockSlim();
        _pipeName = pipeName;
        _connections = new List<IConnection>();
    }

    /// <inheritdoc />
    public IReadOnlyList<IConnection> Clients
    {
        get
        {
            _readerWriterLock.EnterReadLock();
            try
            {
                return _connections.ToArray();
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }
    }

    /// <inheritdoc />
    public async Task<Result<IConnection>> WaitForConnectionAsync(CancellationToken ct = default)
    {
        if (!_listening)
        {
            throw new InvalidOperationException("The server is not listening.");
        }

        var serverStream = new NamedPipeServerStream
        (
            _pipeName,
            PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous
        );

        await serverStream.WaitForConnectionAsync(ct);

        var connection = new NamedPipeConnection(this, serverStream);
        _readerWriterLock.EnterWriteLock();
        try
        {
            _connections.Add(connection);
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return connection;
    }

    /// <inheritdoc />
    public Task<Result> ListenAsync(CancellationToken stopToken = default)
    {
        _listening = true;
        stopToken.Register(Close);
        return Task.FromResult(Result.FromSuccess());
    }

    /// <inheritdoc />
    public void Close()
    {
        _readerWriterLock.EnterReadLock();
        IReadOnlyList<IConnection> connections;
        try
        {
            connections = new List<IConnection>(_connections);
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        foreach (var connection in connections)
        {
            connection.Disconnect();
        }

        _listening = false;
    }

    private class NamedPipeConnection : IConnection
    {
        private readonly NamedPipeServer _server;
        private readonly NamedPipeServerStream _serverStream;

        public NamedPipeConnection(NamedPipeServer server, NamedPipeServerStream serverStream)
        {
            _server = server;
            _serverStream = serverStream;
        }

        public ConnectionState State => _serverStream.IsConnected ? ConnectionState.Open : ConnectionState.Closed;

        public Stream ReadStream => _serverStream;

        public Stream WriteStream => _serverStream;

        public void Disconnect()
        {
            _serverStream.Disconnect();
            _serverStream.Close();

            _server._readerWriterLock.EnterWriteLock();
            try
            {
                _server._connections.Remove(this);
            }
            finally
            {
                _server._readerWriterLock.ExitWriteLock();
            }
        }
    }
}