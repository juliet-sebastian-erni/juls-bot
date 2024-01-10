// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace EchoBot.Bots
{
    /// <summary>
    /// Simple version of the payload received from the Facebook channel.
    /// </summary>
    public class SmsMessage
    {
        [JsonProperty("destinations")]
        public List<Destination> Destinations { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Destination 
    {
        [JsonProperty("to")]
        public string To { get; set; }
    }
}