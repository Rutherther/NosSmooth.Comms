//
//  RunClientResponse.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.Comms.Inject.Messages;

/// <summary>
/// A response to run client.
/// </summary>
/// <param name="InitializationResult">The run result. If error, that means the client is not running.</param>
/// <param name="BindingManagerResult">The result from binding manager initialization.</param>
public record RunClientResponse
(
    Result? InitializationResult,
    Result? BindingManagerResult
);