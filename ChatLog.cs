// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace EchoBot.Bots
{
    // Defines a state property used to track information about the user.
    public class ChatLog
    {
        public int Id { get; set; }
        public string Username{ get; set; }
        public string InputLog { get; set; }
    }
}