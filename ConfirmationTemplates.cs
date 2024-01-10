// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace EchoBot.Bots
{
    /// <summary>
    /// Simple version of the payload received from the Facebook channel.
    /// </summary>
    public class ConfirmationTemplates
    {
        string htmlTemplate = @"
        <html lang='en'>

        <head>
            <meta charset='UTF-8'>
            <title>Appointment Confirmation</title>
            <style>
                /* CSS styles for email layout */
                body {
                    font-family: Arial, sans-serif;
                    line-height: 1.6;
                    margin: 0;
                    padding: 0;
                    background-color: #f4f4f4;
                }

                .container {
                    max-width: 600px;
                    margin: 20px auto;
                    padding: 20px;
                    background-color: #fff;
                    border-radius: 5px;
                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                }

                h1 {
                    color: #333;
                }

                p {
                    margin-bottom: 20px;
                }

                .footer {
                    margin-top: 20px;
                    font-size: 0.8em;
                    color: #888;
                }
            </style>
        </head>

        <body>
            <div class='container'>
                <h1>Appointment Confirmation: [Date] at Juls Dental Test Bot</h1>
                <p>Dear [Patient's Name],</p>

                <p>We are delighted to confirm your upcoming appointment at Juls Dental Test Bot. Your commitment to your dental health is commendable, and we are excited to assist you in achieving your oral wellness goals.</p>

                <h2>Appointment Details:</h2>
                <ul>
                    <li><strong>Date:</strong> [Date]</li>
                    <li><strong>Procedure Booked:</strong> [Procedure Name]</li>
                </ul>

                <p>Our team is dedicated to providing you with exceptional care and ensuring your experience at our clinic is comfortable and beneficial. We kindly ask you to arrive a few minutes before your scheduled time to complete any necessary paperwork and to ensure a prompt start to your appointment.</p>

                <p>If you have any questions or need to make changes to your appointment, please don't hesitate to contact us at <a href='tel:+639171157047'>+639171157047</a> or reply to this email.</p>

                <p>Thank you for choosing Juls Dental Test Bot for your dental care needs. We look forward to seeing you on [Date].</p>

                <p>Warm regards,</p>
                <p>Juliet Sebastian<br>Juls Dental Test Bot<br>
                    +639171157047<br>Marikina City</p>
            </div>

            <div class='footer'>
                <p>This is an automated message. Please do not reply to this email.</p>
            </div>
        </body>

        </html>
        "; 

        string smsTemplate = "Hi [Patient's Name], your appt. at Juls Dental Test Bot is confirmed on [Date] for [Procedure]. Please arrive a few mins early. For changes, call +639171157047. Thank you.";


        public string getEmailTemplate(string Date, string Procedure, string Patient)
        {
            var emailBody = htmlTemplate.Replace("[Date]", Date).Replace("[Procedure Name]", Procedure).Replace("[Patient's Name]", Patient);
            return emailBody;
        }

        public string getSmsTemplate(string Date, string Procedure, string Patient)
        {
            var smsBody = smsTemplate.Replace("[Date]", Date).Replace("[Procedure]", Procedure).Replace("[Patient's Name]", Patient);
            return smsBody;
        }
    }
}