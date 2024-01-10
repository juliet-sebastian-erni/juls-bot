// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v4.17.1

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;



namespace EchoBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        //Change into server domain URL
        private readonly string DOMAIN_NAME = "http://localhost:3978/";

        private BotState _conversationState;
        private BotState _userState;

        private BotService _botService;

         private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

         private  HttpClient _httpClient;

         private static Timer timer;

        public EchoBot(ConversationState conversationState, UserState userState, BotService botService, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _conversationState = conversationState;
            _userState = userState;
            _botService = botService;
            _conversationReferences = conversationReferences;
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected Task StartScheduledMessages(string recipientId, string dateTimeOfAppointment, string reminder)
        {
            DateTime appointmentDt = DateTime.Parse(dateTimeOfAppointment);

            timer = new Timer(async (_) =>
            {
                var currentUtcTime = DateTime.UtcNow;
                TimeZoneInfo phZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                DateTime convertedTime = TimeZoneInfo.ConvertTimeFromUtc(currentUtcTime, phZone);

                TimeSpan difference = appointmentDt.Subtract(convertedTime);
                var diffFormat = difference.ToString(@"hh\:mm");
                // ChatLog(recipientId, appointmentDt + "//" + convertedTime + "//" + diffFormat + "🥹");
                Console.WriteLine(diffFormat + " 🥹");

                if(diffFormat == reminder){
                     await SendMessage(recipientId);
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        protected async Task SendMessage(string recipientId)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(DOMAIN_NAME);

            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync("api/notify");
                Console.WriteLine("🥳Sending reminder for your appointment...");
                timer?.Change(Timeout.Infinite, Timeout.Infinite);
                Console.WriteLine("🥳Time stopped...");
                ChatLog(recipientId, "NUDGE SUCCESS");
            }
            catch(Exception e)
            {
                ChatLog(recipientId, e.ToString());
                Console.WriteLine(e.ToString());
            }
            
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            var conversationStateAccessors = _conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
            var flow = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationFlow(), cancellationToken);

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var profile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile(), cancellationToken);

            var templates = _botService.GetTemplates();
            var error  = templates.FirstOrDefault<Template>(w=> w.TemplateType == "error");
            var answer = error == null ? "Error getting templates" : error.BotMessage;

            var recipientId = turnContext.Activity.GetConversationReference().Conversation.Id;

            var reply = turnContext.Activity.GetChannelData<FacebookPayload>()?.Message?.QuickReply?.Payload ?? turnContext.Activity.Text;
            ChatLog(recipientId, reply);

            var isIntro = reply.ToLower().Contains("get started") || reply.ToLower().Contains("new customer") || reply.ToLower().Contains("end") || reply.ToLower().Contains("hi") || reply.ToLower().Contains("hello");

            if(isIntro){
                answer = "How may I help you today?";
            } else if(reply.ToLower().Contains("view") &&  (reply.ToLower().Contains("rates") || reply.ToLower().Contains("services"))){
                List<Services> services = _botService.GetServices();
                answer = string.Concat(services.Select(o=>o.ServiceType + " - " + o.Price + "PHP" + "\n\n"));
            }else if (reply.ToLower().Contains("view")  && reply.ToLower().Contains("dentists")){
                List<Dentist> dentists = _botService.GetDentists();
                answer = string.Concat(dentists.Select(o=>o.Name + "\n\n"));
            }else if(reply.ToLower().Contains("dentist")  && reply.ToLower().Contains("schedule")){
                List<Schedule> schedules = _botService.GetSchedules();
                answer = string.Concat(schedules.Select(o=>o.Dentist + " - " + o.DateTimeOfSchedule + "\n\n"));
            }else if((reply.ToLower().Contains("open") || reply.ToLower().Contains("office") ) && reply.ToLower().Contains("hours")){
                answer = "We are open Monday-Saturday from 9:00AM-5:00PM. See you there!";
            }else if(reply.ToLower().Contains("dentist")  || reply.ToLower().Contains("available")){
                answer = DentistSchedule(reply);
            }else if(reply.ToLower().Contains("book")  && reply.ToLower().Contains("appointment")){
                await BookAppointment(flow, profile, turnContext, cancellationToken);
                return;
            }else if(flow.LastQuestionAsked != ConversationFlow.Question.None){
                await BookAppointment(flow, profile, turnContext, cancellationToken);
                return;
            }else if(reply.ToLower().Contains("quick replies")){
                await DefaultQuickReplies(turnContext, cancellationToken);
                return;
            }else if(reply.ToLower().Contains("upload")){
                answer = "Please upload a picture";
            }else if(reply.ToLower().Contains("nudge")){
                answer = "NUDGE SUCCESS";
            }
          

            await turnContext.SendActivityAsync(MessageFactory.Text(answer, answer), cancellationToken);
            await DefaultQuickReplies(turnContext, cancellationToken);
        }
        

        private void ChatLog(string recipientId, string reply){
            var chatLogCount = _botService.GetChatLogs().Count();
            var chatLog = new ChatLog {
                Id = chatLogCount + 1,
                Username = recipientId,
                InputLog = reply,
            };
            _botService.AddChatLog(chatLog);
        }


        private static String DentistSchedule(String reply){
                var dateNow = DateTime.Now;

                if(reply.ToLower().Contains("today") || reply.ToLower().Contains("now")){
                    var dayOfWeek = dateNow.DayOfWeek;
                    if(dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Wednesday || dayOfWeek == DayOfWeek.Friday){
                        return "Dr. Viviana Joseph is happy to serve you today.";
                    }else if (dayOfWeek == DayOfWeek.Tuesday || dayOfWeek == DayOfWeek.Thursday){
                        return "Dr.Adelynn Navarro is happy to serve you today.";
                    }else if (dayOfWeek == DayOfWeek.Sunday){
                        return"Dr.Adelynn Walls is happy to serve you today.";
                    }else{
                        return "I'm very sorry but our dentists are out today. We are open Monday-Saturday from 9:00AM-5:00PM. See you there!";
                    }
                }else {
                    return"Here is our dentists schedule: \n\nDr. Viviana Joseph - M/W/F (9:00AM-5:00PM) \n\nDr. Adelynn Navarro - T/TH (9:00AM-5:00PM) \n\nDr.Adelynn Walls - S (9:00AM-5:00PM) \n\nWe are happy to serve you!";
                }
        }

         private async Task BookAppointment(ConversationFlow flow, UserProfile profile, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await FillOutUserProfileAsync(flow, profile, turnContext, cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);

            if(flow.LastQuestionAsked == ConversationFlow.Question.None){
                var recipientId = turnContext.Activity.GetConversationReference().Conversation.Id;
                
                var appointmentCount = _botService.GetAppointments().Count;
                var appointment = new Appointment()
                {
                    Id = appointmentCount + 1,
                    RecipientId = recipientId,
                    Patient = profile.Name,
                    Age = profile.Age,
                    Email = profile.Email,
                    Number = profile.Number,
                    DateTimeOfAppointment = profile.DateTimeOfAppointment,
                    ServiceType = profile.Service,
                };
                _botService.SendEmail(appointment);
                _botService.SendSms(appointment);
                _botService.AddAppointment(appointment);

                //add nudge
                string reminder = _botService.GetReminder();
                await StartScheduledMessages(recipientId, appointment.DateTimeOfAppointment, reminder);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // CreateServices();
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                    await DefaultQuickReplies(turnContext, cancellationToken);
                }
            }
        }

        private  async Task DefaultQuickReplies(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            Activity replyToConversation = turnContext.Activity.CreateReply();
            replyToConversation.Text = "Options:";
            dynamic quickReplies = new JObject();

            dynamic fbServices = new JObject();
            fbServices.content_type = "text";
            fbServices.title = "View Dental Services";
            fbServices.payload = "view dental services";

            dynamic fbOfficeHours = new JObject();
            fbOfficeHours.content_type = "text";
            fbOfficeHours.title = "Book an Appointment";
            fbOfficeHours.payload = "book appointment";

            dynamic fbAppointment = new JObject();
            fbAppointment.content_type = "text";
            fbAppointment.title = "Working Hours";
            fbAppointment.payload = "office hours";

            quickReplies.quick_replies = new JArray(fbServices, fbOfficeHours, fbAppointment);
            replyToConversation.ChannelData = quickReplies;
            await turnContext.SendActivityAsync(replyToConversation, cancellationToken);
        }

        private  async Task ServiceQuickReplies(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            Activity replyToConversation = turnContext.Activity.CreateReply();
            replyToConversation.Text = "Options:";
            dynamic quickReplies = new JObject();

            dynamic fbCheckup = new JObject();
            fbCheckup.content_type = "text";
            fbCheckup.title = "Dental Checkup";
            fbCheckup.payload = "dental checkup";

            dynamic fbBraces = new JObject();
            fbBraces.content_type = "text";
            fbBraces.title = "Metal Braces";
            fbBraces.payload = "metal braces";

            dynamic fbVeneers = new JObject();
            fbVeneers.content_type = "text";
            fbVeneers.title = "Veneers";
            fbVeneers.payload = "veneers";

            quickReplies.quick_replies = new JArray(fbCheckup, fbBraces, fbVeneers);
            replyToConversation.ChannelData = quickReplies;
            await turnContext.SendActivityAsync(replyToConversation, cancellationToken);
        }

        // private async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken, bool isNew)
        // {
        //     var templates = _botService.GetTemplates();
        //     var intro  = templates.FirstOrDefault<Template>(w=> w.TemplateType == "intro");
        //     var answer = intro == null ? "Error getting templates" : intro.BotMessage;
            
        //     var reply = MessageFactory.Text(isNew ? answer: "");

        //     reply.SuggestedActions = new SuggestedActions()
        //     {
        //         Actions = new List<CardAction>()
        //         {
        //             new CardAction() { Title = "View Dental Services", Type = ActionTypes.ImBack, Value = "view dental services"},
        //             new CardAction() { Title = "View our Dentists", Type = ActionTypes.ImBack, Value = "view dentists"},
        //             new CardAction() { Title = "Our Dentists Schedule", Type = ActionTypes.ImBack, Value = "dentist schedule"},
        //             new CardAction() { Title = "Working Hours", Type = ActionTypes.ImBack, Value = "office hours"},
        //             new CardAction() { Title = "Book an Appointment", Type = ActionTypes.ImBack, Value = "book appointment"},
        //         },
        //     };
        //     await turnContext.SendActivityAsync(reply, cancellationToken);
        // }

        private async Task DateQuickReplies(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            Activity replyToConversation = turnContext.Activity.CreateReply();
            replyToConversation.Text = "Options:";
            dynamic quickReplies = new JObject();

            var currentUtcTime = DateTime.UtcNow;
            TimeZoneInfo phZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            DateTime convertedTime = TimeZoneInfo.ConvertTimeFromUtc(currentUtcTime, phZone);

            // var dateNow = DateTime.Now;
            var tomorrow = convertedTime.AddMinutes(5);
            var tomFormat = tomorrow.ToString("MM/dd/yyyy hh:mm tt");
            dynamic fbDate = new JObject();
            fbDate.content_type = "text";
            fbDate.title = "5 minutes from now";
            fbDate.payload = tomFormat;

            quickReplies.quick_replies = new JArray(fbDate);
            replyToConversation.ChannelData = quickReplies;
            await turnContext.SendActivityAsync(replyToConversation, cancellationToken);
        }

         private static async Task SendSuggestedDateAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("");

            var dateNow = DateTime.Now;
            var tomorrow = dateNow.AddDays(1);


            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "tomorrow", Type = ActionTypes.ImBack, Value = tomorrow.ToString("MMMM dd, yyyy")},
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }


         private async Task FillOutUserProfileAsync(ConversationFlow flow, UserProfile profile, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var input = turnContext.Activity.GetChannelData<FacebookPayload>()?.Message?.QuickReply?.Payload?.Trim() ?? turnContext.Activity.Text?.Trim();
            string message;

            // else if(reply.ToLower().Contains("email")){
            //     _botService.SendEmail();
            //     answer = "Please check your confirmation email.";
            // }

            switch (flow.LastQuestionAsked)
            {
                case ConversationFlow.Question.None:
                    var text = "Let's get started. What's your name?";
                    await turnContext.SendActivityAsync(MessageFactory.Text(text, text),  cancellationToken);
                    flow.LastQuestionAsked = ConversationFlow.Question.Name;
                    break;
                case ConversationFlow.Question.Name:
                    if (ValidateName(input, out var name, out message))
                    {
                        profile.Name = name;
                        await turnContext.SendActivityAsync($"Hi {profile.Name}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("How old are you?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.Age;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }

                case ConversationFlow.Question.Age:
                    if (ValidateAge(input, out var age, out message))
                    {
                        profile.Age = age;
                        await turnContext.SendActivityAsync($"I have your age as {profile.Age}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("Please enter your email address.", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.Email;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.Email:
                    if (ValidateEmail(input, out var email, out message))
                    {
                        profile.Email = email;
                        await turnContext.SendActivityAsync($"I have your email as {profile.Email}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("Please enter your phone number in this format (+639000000000)", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.Number;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.Number:
                    if (ValidateNumber(input, out var number, out message))
                    {
                        profile.Number = number;
                        await turnContext.SendActivityAsync($"I have your phone number as {profile.Number}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("When would you like to book? Please enter in MM/dd/yyyy hh:mm tt format", null, null, cancellationToken);
                        await DateQuickReplies(turnContext, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.DateTimeOfAppointment;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                
                case ConversationFlow.Question.DateTimeOfAppointment:
                    if (ValidateDate(input, out var date, out message))
                    {
                        profile.DateTimeOfAppointment = date;
                        await turnContext.SendActivityAsync($"Your dental appointment is scheduled for {profile.DateTimeOfAppointment}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What service would you like to avail?", null, null, cancellationToken);
                        await ServiceQuickReplies(turnContext, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.Service;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }

                case ConversationFlow.Question.Service:
                    if (ValidateService(input, out var service, out message))
                    {
                        profile.Service = service;
                        await turnContext.SendActivityAsync($"You are going for a {profile.Service} procedure.");
                        await turnContext.SendActivityAsync($"We have sent you the confirmation email. Thanks for completing the booking {profile.Name}. See you at the clinic on {profile.DateTimeOfAppointment}!");
                        await turnContext.SendActivityAsync($"Type 'END' to run the bot again.");
                        flow.LastQuestionAsked = ConversationFlow.Question.None;
                        profile = new UserProfile();
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
            }

            return;
        }

        private static bool ValidateName(string input, out string name, out string message)
        {
            name = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a name that contains at least one character.";
            }
            else
            {
                name = input.Trim();
            }

            return message is null;
        }

        private static bool ValidateAge(string input, out int age, out string message)
        {
            age = 0;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter an age that contains at least one character.";
            }
            else
            {
                try
                {
                    age = Int32.Parse(input);
                    if(age < 8){
                        message = "Patient should be at least 8 years old and above.";
                    }
                }
                catch (FormatException)
                {
                    message = "Please enter a valid number.";
                }
            }

            return message is null;
        }

        private static bool ValidateEmail(string input, out string email, out string message)
        {
            email = null;
            message = null;


            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter an email that contains at least one character.";
            }

            email = input;
            return  message is null;
        }

        private static bool ValidateNumber(string input, out string number, out string message)
        {
            number = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a number that contains at least one character.";
            }

            number = input;
            return  message is null;

        }

        private static bool ValidateDate(string input, out string date, out string message)
        {
            date = null;
            message = null;


            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a date that contains at least one character.";
            }

            date = input;
            return  message is null;
        }


        private static bool ValidateService(string input, out string service, out string message)
        {
            service = null;
            message = null;


            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a service that contains at least one character.";
            }
            else 
            {
               bool isValid = input.ToLower().Contains("dental checkup") || input.ToLower().Contains("flouride treatment") || input.ToLower().Contains("tooth extraction") || input.ToLower().Contains("metal braces") || input.ToLower().Contains("veneers");
               service = input.ToString();
               return isValid;
            }

           service = input;
           return  message is null;
        }
    }
}
