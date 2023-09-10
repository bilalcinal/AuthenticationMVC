using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MyProject.Models;
using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;

namespace MyProject.Service
{
    public class EmailQueueConsumer : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly RabbitMqService _rabbitMqService;

        public EmailQueueConsumer(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }
        
        public EmailQueueConsumer()
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqps://wjagymjh:CXETsmwe1_FRjhexW3DnApC73t3sxI4B@toad.rmq.cloudamqp.com/wjagymjh")
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _queueName = "email_queue";

            _channel.QueueDeclare(queue: _queueName, durable: true, false, false, null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var emailMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<EmailModel>(message);

                ProcessEmail(emailMessage);

                _channel.BasicAck(e.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }
        private void ProcessEmail(EmailModel emailMessage)
        {
            try
            {
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("smtp.gmail.com", 587, false);
                    smtpClient.Authenticate("hbilalcinal@gmail.com", "tqktaustbvneybed");

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("BilalCinal", "hbilalcinal@gmail.com"));
                    message.To.Add(new MailboxAddress("", emailMessage.ToEmail));
                    message.Subject = emailMessage.Subject;

                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = emailMessage.Body;
                    message.Body = bodyBuilder.ToMessageBody();

                    smtpClient.Send(message);
                    smtpClient.Disconnect(true);
                }

                var confirmationMessage = new EmailModel
                {
                    ToEmail = emailMessage.ToEmail,
                };

                var confirmationMessageJson = JsonConvert.SerializeObject(confirmationMessage);

                using (var rabbitMqService = new RabbitMqService())
                {
                    rabbitMqService.PublishEmail(confirmationMessageJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

              public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
