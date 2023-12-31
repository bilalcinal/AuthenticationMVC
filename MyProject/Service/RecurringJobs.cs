using Hangfire;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;

namespace MyProject.Service
{
    public class RecurringJobs
    {
        private readonly IServiceProvider _serviceProvider;

        public RecurringJobs(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SendReminderEmailsJob()
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                var emailList = await applicationDbContext.Accounts.Select(e => e.Email).ToListAsync();

                foreach (var email in emailList)
                {
                    var emailSendModel = new EmailModel
                    {
                        ToEmail = email,
                        Subject = "Reminding",
                        Body = "Hello, come back!"
                    };

                    await emailService.SendEmailAsync(emailSendModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void ConfigureRecurringJobs(IServiceProvider serviceProvider)
        {
            var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
            recurringJobManager.AddOrUpdate("HatırlatmaEpostalarınıGonder", () => new RecurringJobs(serviceProvider).SendReminderEmailsJob(), Cron.Daily);
        }
    }
}
