using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.Logging.Configuration;
using DotBoil.Logging.Sink;
using DotBoil.Reflection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DotBoil.Logging
{
    internal class LoggingModule : Module
    {
        public override async Task AddModule()
        {
            var loggingOptions = DotBoilApp.Configuration.GetConfigurations<LoggingOptions>();

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information();

            foreach (var sinkTypeName in loggingOptions.Sinks)
            {
                var sinkType = AppDomain.CurrentDomain.FindType(sinkTypeName);

                if (sinkType is null) continue;

                var sinkInstance = (ISink)Activator.CreateInstance(sinkType);
                await sinkInstance.UseSink(loggerConfiguration);
            }

            var logger = loggerConfiguration.CreateLogger();

            DotBoilApp.Logging.ClearProviders();
            DotBoilApp.Logging.AddSerilog(logger);
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
