using DotBoil.Configuration;
using DotBoil.Email.Models;

namespace DotBoil.Email.Configuration
{
    public class EmailSettings : IOptions
    {
        public string Key => "DotBoil:Email";

        public IDictionary<string, ServerSettings> ServerSettings { get; set; }

        public EmailSettings()
        {
            ServerSettings = new Dictionary<string, ServerSettings>();
        }
    }
}
