using DotBoil.Dependency;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotBoil.Email
{
    internal class MailSenderModule : Module
    {
        public override Task AddModule()
        {
            DotBoilApp.Services.TryAddSingleton<IMailSender, SmtpSender>();

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
