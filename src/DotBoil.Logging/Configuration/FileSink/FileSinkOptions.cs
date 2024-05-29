using DotBoil.Configuration;
using Serilog;

namespace DotBoil.Logging.Configuration.FileSink
{
    internal class FileSinkOptions : IOptions
    {
        public string Key => "DotBoil:Logging:FileSink";

        public string LogFileName { get; set; }
        public RollingInterval RollingInterval { get; set; }
    }
}
