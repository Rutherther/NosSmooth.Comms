//
//  CallbackConfig.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.Comms.Inject;

public record CallbackConfig(bool SendRawPackets, bool SendDeserializedPackets);