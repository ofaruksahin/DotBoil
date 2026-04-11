using DotBoil.MassTransit.Attributes;
using DotBoil.MessageBroker;

namespace DotBoil.AuthGuard.Application.Domain.DomainEvents;

[Queue("consent-accepted")]
public class ConsentAcceptedDomainEvent : IEvent
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

    public string UserId { get; set; }
    public string ConsentType { get; set; }
    public string Language { get; set; }
    public int Version { get; set; }

    public ConsentAcceptedDomainEvent()
    {
    }

    public ConsentAcceptedDomainEvent(string userId, string consentType, string language, int version)
    {
        UserId = userId;
        ConsentType = consentType;
        Language = language;
        Version = version;
    }
}