// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace EchoBot.Bots
{
    /// <summary>
    /// A Facebook quick reply.
    /// </summary>
    /// <remarks>See <see cref="https://developers.facebook.com/docs/messenger-platform/send-messages/quick-replies/"> Quick Replies Facebook Documentation</see> '
    /// for more information on quick replies.</remarks>
    public class FacebookAttachment
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("payload")]
        public AttachmentPayload Payload { get; set; }
    }

    public class AttachmentPayload
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}