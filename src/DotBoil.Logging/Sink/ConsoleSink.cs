using Serilog;

namespace DotBoil.Logging.Sink
{
    internal class ConsoleSink : ISink
    {
        public Task UseSink(LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration
                .WriteTo.Console();

            return Task.CompletedTask;
        }
    }
}
