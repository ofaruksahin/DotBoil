using DotBoil.Dependency;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Email
{
    internal class MailSenderModule : Module
    {
        public override Task AddModule()
        {
            DotBoilApp.Services.AddSingleton<IMailSender, SmtpSender>();

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
