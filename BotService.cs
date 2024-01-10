using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using EchoBot.Controllers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace EchoBot.Bots
{
    public class BotService : CloudAdapter
    {
        private readonly MyDbContext _dbContext;

        public BotService(MyDbContext dbContext )
        {
            _dbContext = dbContext;
        }

  
        public async void SendSms(Appointment appointment)
        {
            var number = appointment.Number.Replace("+","");

            
            var options = new RestClientOptions("https://5yp5mg.api.infobip.com")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/sms/2/text/advanced", Method.Post);
            request.AddHeader("Authorization", "App cffb2221a0269cb87b9532cc6720b5b7-8e61c19c-7b8c-4ddd-b57a-7448da2d8449");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");

            var smsPayload = new SmsPayload()
            {
                Messages = [
                    new SmsMessage()
                    {
                        Destinations = [
                            new Destination()
                            {
                                To = number,
                            }
                        ],
                        From = "Juls Dental Test Bot",
                        Text = new ConfirmationTemplates().getSmsTemplate(appointment.DateTimeOfAppointment, appointment.ServiceType, appointment.Patient)
                    }
                ],
            };
            var jsonBody = JsonConvert.SerializeObject(smsPayload);
            request.AddStringBody(jsonBody, DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
        }

        public void SendEmail(Appointment appointment)
        {
            string fromEmail = "juliet.sebastian.erni@gmail.com";
            string fromPassword = "rxqtpddotkkmnrea";

            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromEmail);
            message.Subject = "Juls Dental Test Bot - Booking Confirmation";
            message.To.Add(new MailAddress(appointment.Email));
            // message.Body = $"<html><body> You have booked to Juls Dental Test Bot on {appointment.DateTimeOfAppointment} with a procedure of {appointment.ServiceType}. See you there! </body></html>";
            message.IsBodyHtml = true;
            message.Body = new ConfirmationTemplates().getEmailTemplate(appointment.DateTimeOfAppointment, appointment.ServiceType, appointment.Patient);

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true,
            };

            smtpClient.Send(message);
        }

        public string GetReminder()
        {
            Reminder reminder = _dbContext.Reminder.ToList().First();
            return reminder.Minutes;
        }

        public List<Services> GetServices()
        {
            List<Services> services = _dbContext.Services.ToList();
            return services;
        }

        public List<Dentist> GetDentists()
        {
            List<Dentist> dentists = _dbContext.Dentists.ToList();
            return dentists;
        }

        public List<Schedule> GetSchedules()
        {
            List<Schedule> schedules = _dbContext.Schedules.ToList();
            return schedules;
        }

        public List<ChatLog> GetChatLogs()
        {
            List<ChatLog> chatLogs = _dbContext.ChatLogs.ToList();
            return chatLogs;
        }

        public List<Template> GetTemplates()
        {
            List<Template> templates = _dbContext.Templates.ToList();
            return templates;
        }

        public List<Appointment> GetAppointments()
        {
            List<Appointment> appointments = _dbContext.Appointments.ToList();
            return appointments;
        }

        public void AddChatLog(ChatLog log){
            _dbContext.Add(log);
            _dbContext.SaveChanges();
        }

        public void AddAppointment(Appointment appointment){
            _dbContext.Add(appointment);
            _dbContext.SaveChanges();
        }

        public async void FileUpload(String fileName){
            var connectionString  = "DefaultEndpointsProtocol=https;AccountName=dentalbotblob;AccountKey=iDmMLZhTLr8bBZbUnflY6Ky9pHSqtW/nLymdWzMKdVLdHltgpOevsG3typxHY6DMH9x5tj34AoGk+ASt4+p6tw==;EndpointSuffix=core.windows.net";
            var containerName = "fileupload";
            var container = new BlobContainerClient(connectionString, containerName);
            var blob = container.GetBlobClient("tooth_from_fb.png");

            var stream = File.OpenRead("tooth_from_fb.png");
            await blob.UploadAsync(stream);
        }
    }
}

