using DotBoil.Email.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace DotBoil.Email
{
    public class SmtpSender : IMailSender
    {
        public async Task<bool> SendAsync(ServerSettings settings, Message message)
        {
            var mimeMessage = new MimeMessage();
            var bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = message.Body;

            message.Attachments.ForEach(attachment => bodyBuilder.Attachments.Add(attachment.FileName, attachment.Stream, new ContentType(attachment.MediaType, attachment.SubMediaType)));

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            mimeMessage.Subject = message.Subject;
            message.From.ForEach(from => mimeMessage.From.Add(new MailboxAddress(from, from)));
            message.To.ForEach(to => mimeMessage.To.Add(new MailboxAddress(to, to)));

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, settings.EnableSsl);
                await client.AuthenticateAsync(settings.EmailAddress, settings.Password);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }

            return true;
        }
    }
}
