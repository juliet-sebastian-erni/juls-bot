// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace EchoBot.Bots
{
    // Defines a state property used to track information about the user.
    public class Schedule
    {
        public int Id { get; set; }
        public string Dentist { get; set; }
        public string DateTimeOfSchedule { get; set; }
    }
}