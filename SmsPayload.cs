// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace EchoBot.Bots
{
    /// <summary>
    /// Simple version of the payload received from the Facebook channel.
    /// </summary>
    public class SmsPayload
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [JsonProperty("messages")]
        public List<SmsMessage> Messages { get; set; }
    }
}