// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace EchoBot.Bots
{
    // Defines a state property used to track information about the user.
    public class UserProfile
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Number { get; set; }
        public string DateTimeOfAppointment { get; set; }
        public string Service { get; set; }
    }
}