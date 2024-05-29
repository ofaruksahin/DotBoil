using Microsoft.AspNetCore.Builder;
using Serilog;

namespace DotBoil.Logging.Sink
{
    public interface ISink
    {
        Task UseSink(WebApplicationBuilder builder,LoggerConfiguration loggerConfiguration);
    }
}
