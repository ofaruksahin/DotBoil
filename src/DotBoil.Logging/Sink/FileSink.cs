using DotBoil.Configuration;
using DotBoil.Logging.Configuration.FileSink;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace DotBoil.Logging.Sink
{
    internal class FileSink : ISink
    {
        public Task UseSink(WebApplicationBuilder builder, LoggerConfiguration loggerConfiguration)
        {
            var fileSinkOptions = builder.Configuration.GetConfigurations<FileSinkOptions>();

            loggerConfiguration
                .WriteTo.File(fileSinkOptions.LogFileName, rollingInterval: fileSinkOptions.RollingInterval);

            return Task.CompletedTask;
        }
    }
}
