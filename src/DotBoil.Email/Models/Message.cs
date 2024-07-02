namespace DotBoil.Email.Models
{
    public class Message
    {
        public List<string> From { get; set; }
        public List<string> To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<Attachment> Attachments { get; set; }

        public Message()
        {
            Attachments = new List<Attachment>();
        }

        public Message(List<string> from, List<string> to, string subject, string body, List<Attachment> attachments)
        {
            From = from;
            To = to;
            Subject = subject;
            Body = body;
            Attachments = attachments;
        }
    }
}
