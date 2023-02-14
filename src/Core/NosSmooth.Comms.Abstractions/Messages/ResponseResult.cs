//
//  ResponseResult.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.Comms.Data.Messages;

/// <summary>
/// A response to a received message,
/// sent from server to client.
/// </summary>
/// <param name="MessageId">The id of the message this response is for.</param>
/// <param name="Result">The result.</param>
public record ResponseResult(long MessageId, Result Result);