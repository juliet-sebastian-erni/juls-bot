// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace EchoBot.Bots
{
    // Defines a state property used to track information about the user.
    public class Template
    {
        public int Id { get; set; }
        public string TemplateType { get; set; }
        public string BotMessage { get; set; }
    }
}