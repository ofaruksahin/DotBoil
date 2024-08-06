using Serilog;

namespace DotBoil.Logging.Sink
{
    public interface ISink
    {
        Task UseSink(LoggerConfiguration loggerConfiguration);
    }
}
