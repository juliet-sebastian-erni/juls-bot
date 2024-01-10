// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace EchoBot.Bots
{
    /// <summary>
    /// Simple version of the payload received from the Facebook channel.
    /// </summary>
    public class FacebookPayload
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [JsonProperty("message")]
        public FacebookMessage Message { get; set; }
    }
}