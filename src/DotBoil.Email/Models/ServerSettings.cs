namespace DotBoil.Email.Models
{
    public class ServerSettings
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }

        public ServerSettings()
        {
        }

        public ServerSettings(string smtpHost, int smtpPort, string emailAddress, string password, bool enableSsl)
        {
            SmtpHost = smtpHost;
            SmtpPort = smtpPort;
            EmailAddress = emailAddress;
            Password = password;
            EnableSsl = enableSsl;
        }
    }
}
