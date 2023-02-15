//
//  ConnectionHandlerExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Comms.Core;
using NosSmooth.Comms.Inject.Messages;
using NosSmooth.Core.Contracts;

namespace NosSmooth.Comms.Local.Extensions;

/// <summary>
/// Extension methods for <see cref="ConnectionHandler"/>.
/// </summary>
public static class ConnectionHandlerExtensions
{
    /// <summary>
    /// Contract <see cref="RunClientRequest"/>, <see cref="RunClientResponse"/>.
    /// </summary>
    /// <param name="connectionHandler">The connection handler.</param>
    /// <param name="message">The request.</param>
    /// <returns>The contract.</returns>
    public static IContract<RunClientResponse, DefaultStates> ContractRunClient
        (this ConnectionHandler connectionHandler, RunClientRequest message)
        => connectionHandler.ContractCustomResponse<RunClientRequest, RunClientResponse>(message, _ => true);
}