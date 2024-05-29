using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.Logging.Configuration;
using DotBoil.Logging.Sink;
using DotBoil.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DotBoil.Logging
{
    internal class LoggingModule : Module
    {
        public override async Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var loggingOptions = builder.Configuration.GetConfigurations<LoggingOptions>();

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information();

            foreach (var sinkTypeName in loggingOptions.Sinks)
            {
                var sinkType = AppDomain.CurrentDomain.FindType(sinkTypeName);

                if (sinkType is null) continue;

                var sinkInstance = (ISink)Activator.CreateInstance(sinkType);
                await sinkInstance.UseSink(builder, loggerConfiguration);
            }

            var logger = loggerConfiguration.CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            return builder;
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
