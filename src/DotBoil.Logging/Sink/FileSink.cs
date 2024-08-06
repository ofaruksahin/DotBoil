using DotBoil.Configuration;
using DotBoil.Logging.Configuration.FileSink;
using Serilog;

namespace DotBoil.Logging.Sink
{
    internal class FileSink : ISink
    {
        public Task UseSink(LoggerConfiguration loggerConfiguration)
        {
            var fileSinkOptions = DotBoilApp.Configuration.GetConfigurations<FileSinkOptions>();

            loggerConfiguration
                .WriteTo.File(fileSinkOptions.LogFileName, rollingInterval: fileSinkOptions.RollingInterval);

            return Task.CompletedTask;
        }
    }
}
