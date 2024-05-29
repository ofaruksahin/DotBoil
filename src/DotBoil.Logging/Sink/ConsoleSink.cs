using Microsoft.AspNetCore.Builder;
using Serilog;

namespace DotBoil.Logging.Sink
{
    internal class ConsoleSink : ISink
    {
        public Task UseSink(WebApplicationBuilder builder, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration
                .WriteTo.Console();

            return Task.CompletedTask;
        }
    }
}
