//
//  MessageWrapper.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.Comms.Data.Messages;

public record MessageWrapper<TMessage>(long ProtocolVersion, long MessageId, TMessage Data);