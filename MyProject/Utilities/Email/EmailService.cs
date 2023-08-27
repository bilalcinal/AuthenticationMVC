using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MyProject.Models;

namespace MyProject.Utilities.Email
{   
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(EmailModel emailModel)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("BilalCinal", "hbilalcinal@gmail.com")); 
                message.To.Add(new MailboxAddress("", emailModel.ToEmail));
                message.Subject = emailModel.Subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = emailModel.Body;
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync("hbilalcinal@gmail.com", "tqktaustbvneybed");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
    }
}
