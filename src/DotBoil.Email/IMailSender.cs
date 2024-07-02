using DotBoil.Email.Models;

namespace DotBoil.Email
{
    public interface IMailSender
    {
        Task<bool> SendAsync(ServerSettings settings, Message message);
    }
}
