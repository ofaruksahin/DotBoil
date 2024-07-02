using DotBoil.Configuration;
using DotBoil.Email.Models;

namespace DotBoil.Email.Configuration
{
    public class EmailOptions : IOptions
    {
        public string Key => "DotBoil:Email";

        public IDictionary<string, ServerSettings> ServerSettings { get; set; }

        public EmailOptions()
        {
            ServerSettings = new Dictionary<string, ServerSettings>();
        }
    }
}
