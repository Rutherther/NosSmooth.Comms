//
//  ResponseResultResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Data.Messages;
using NosSmooth.Comms.Data.Responders;
using NosSmooth.Core.Contracts;
using Remora.Results;

namespace NosSmooth.Comms.Core.Responders;

/// <summary>
/// Responds to <see cref="ResponseResult"/> by updating contractor with the response.
/// </summary>
public class ResponseResultResponder : IMessageResponder<ResponseResult>
{
    private readonly Contractor _contractor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseResultResponder"/> class.
    /// </summary>
    /// <param name="contractor">The contractor.</param>
    public ResponseResultResponder(Contractor contractor)
    {
        _contractor = contractor;

    }

    /// <inheritdoc />
    public Task<Result> Respond(ResponseResult message, CancellationToken ct = default)
        => _contractor.Update(message, ct);
}