// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace EchoBot.Bots
{
    // Defines a state property used to track information about the user.
    public class Appointment
    {
        public int Id { get; set; }
        public string RecipientId { get; set; }
        public string Patient { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Number { get; set; }
        public string DateTimeOfAppointment { get; set; }
        public string ServiceType { get; set; }
    }
}