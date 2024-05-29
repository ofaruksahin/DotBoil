using DotBoil.Configuration;
using DotBoil.Logging.Sink;
using FSink = DotBoil.Logging.Sink.FileSink;

namespace DotBoil.Logging.Configuration
{
    internal class LoggingOptions : IOptions
    {
        public string Key => "DotBoil:Logging";
        public List<string> Sinks { get; set; }

        public LoggingOptions()
        {
            Sinks = new List<string>()
            {
                typeof(ConsoleSink).FullName,
                typeof(FSink).FullName,
            };
        }
    }
}
