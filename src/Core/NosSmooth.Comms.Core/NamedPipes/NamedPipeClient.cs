//
//  NamedPipeClient.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data;
using System.IO.Pipes;
using NosSmooth.Comms.Data;
using Remora.Results;

namespace NosSmooth.Comms.Core.NamedPipes;

/// <summary>
/// A client using named pipes.
/// </summary>
public class NamedPipeClient : IClient
{
    private readonly NamedPipeClientStream _stream;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedPipeClient"/> class.
    /// </summary>
    /// <param name="pipeName">The name of the pipe.</param>
    public NamedPipeClient(string pipeName)
    {
        _stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
    }

    /// <inheritdoc />
    public ConnectionState State => _stream.IsConnected ? ConnectionState.Open : ConnectionState.Closed;

    /// <inheritdoc />
    public Stream ReadStream => _stream;

    /// <inheritdoc />
    public Stream WriteStream => _stream;

    /// <inheritdoc />
    public void Disconnect()
    {
        _stream.Close();
        _stream.Dispose();
    }

    /// <inheritdoc />
    public async Task<Result> ConnectAsync(CancellationToken ct)
    {
        try
        {
            await _stream.ConnectAsync(ct);
            return Result.FromSuccess();
        }
        catch (OperationCanceledException)
        {
            // ignored
            return Result.FromSuccess();
        }
        catch (Exception e)
        {
            return e;
        }
    }
}