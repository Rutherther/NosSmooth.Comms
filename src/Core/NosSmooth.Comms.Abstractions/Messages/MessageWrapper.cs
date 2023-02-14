//
//  MessageWrapper.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.Comms.Data.Messages;

/// <summary>
/// A wrapper, each message is sent wrapped.
/// </summary>
/// <param name="ProtocolVersion">The version of protocol used.</param>
/// <param name="MessageId">The id of the message, used for connecting responses to messages.</param>
/// <param name="Data">The message itself.</param>
/// <typeparam name="TMessage">The type of the message.</typeparam>
public record MessageWrapper<TMessage>(long ProtocolVersion, long MessageId, TMessage Data);