// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace EchoBot.Bots
{
    public class FacebookAttachmentResponse
    {
        [JsonProperty("messaging")]
        public List<Messaging> Messaging { get; set; }
    }

    public class Messaging 
    {
        [JsonProperty("message")]
        public FacebookMessage Message { get; set; }
    }
}