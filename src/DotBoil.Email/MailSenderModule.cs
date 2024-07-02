using DotBoil.Dependency;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Email
{
    internal class MailSenderModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IMailSender, SmtpSender>();

            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
