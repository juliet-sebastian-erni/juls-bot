// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace EchoBot.Bots
{
    /// <summary>
    /// A Facebook message payload.
    /// </summary>
    public class FacebookMessage
    {
        /// <summary>
        /// Gets or sets the quick reply.
        /// </summary>
        [JsonProperty("quick_reply")]
        public FacebookQuickReply QuickReply { get; set; }

        /// <summary>
        /// Gets or sets the attachment.
        /// </summary>
        [JsonProperty("attachments")]
        public List<FacebookAttachment> Attachments { get; set; }

    }
}