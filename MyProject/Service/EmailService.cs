using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MyProject.Data;
using MyProject.Models;
using MyProject.Service;
using MyProject.Utilities.Token;
using Newtonsoft.Json;

namespace MyProject.Service
{
    public class EmailService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly TokenGenerator _tokenGenerator;
        private readonly IConfiguration _configuration;
        private readonly RabbitMqService _rabbitMqService;

        public EmailService(IConfiguration configuration, RabbitMqService rabbitMqService, ApplicationDbContext applicationDbContext, TokenGenerator tokenGenerator)
        {
            _configuration = configuration;
            _rabbitMqService = rabbitMqService;
            _applicationDbContext = applicationDbContext;
            _tokenGenerator = tokenGenerator;
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
        public async Task SendValidationEmailAsync(AccountModel accountModel)
        {
            var token = _tokenGenerator.GenerateToken();
            var registerTokenEntity = new RegisterToken
            {
                Email = accountModel.Email,
                Token = token,
                Expires = DateTime.UtcNow.AddHours(24)
            };

            await _applicationDbContext.RegisterTokens.AddAsync(registerTokenEntity);
            await _applicationDbContext.SaveChangesAsync();

            string validateTokenUrl = $"https://localhost:7179/Authentication/ValidateTokenCallBack?validationToken={token}";
            var emailModel = new EmailModel
            {
                ToEmail = accountModel.Email,
                Subject = "Hoş Geldiniz!",
                Body = $@"<!DOCTYPE html>
                            <html lang=""en"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Complete Registration</title>
                            </head>
                            <body>
                                <h1>Welcome to our platform!</h1>
                                <p>Thank you for registering. To complete your registration, please click the following link:</p>
                                <p><a href=""{validateTokenUrl}"">Complete Registration</a></p>
                                <p>If the link above doesn't work, you can copy and paste the following URL into your browser's address bar:</p>
                                <p>{validateTokenUrl}</p>
                                <p>We're excited to have you on board. If you have any questions, feel free to contact us.</p>
                                <p>Best regards,</p>
                                <p>Your Team</p>
                            </body>
                            </html>"
            };
            var emailModelJson = JsonConvert.SerializeObject(emailModel);

            _rabbitMqService.PublishEmail(emailModelJson);
        }
    }
}
