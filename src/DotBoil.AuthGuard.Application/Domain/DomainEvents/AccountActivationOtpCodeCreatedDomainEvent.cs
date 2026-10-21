using DotBoil.MassTransit.Attributes;
using DotBoil.MessageBroker;

namespace DotBoil.AuthGuard.Application.Domain.DomainEvents;

[Queue("account-activation")]
public class AccountActivationOtpCodeCreatedDomainEvent : IEvent
{
    private Guid _id = Guid.Empty;

    public Guid Id
    {
        get
        {
            if (_id == Guid.Empty)
                _id = Guid.NewGuid();

            return _id;
        }
        set
        {
            _id = value;
        }
    }

    public string Email { get; set; }
    public string OtpCode { get; set; }

    public AccountActivationOtpCodeCreatedDomainEvent()
    {
    }

    public AccountActivationOtpCodeCreatedDomainEvent(string email, string otpCode)
    {
        Email = email;
        OtpCode = otpCode;
    }
}
