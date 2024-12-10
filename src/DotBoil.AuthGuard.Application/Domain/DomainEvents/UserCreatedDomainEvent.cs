using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.MassTransit.Attributes;
using DotBoil.MessageBroker;

namespace DotBoil.AuthGuard.Application.Domain.DomainEvents;

[Queue("user-created")]
public class UserCreatedDomainEvent : IEvent
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

    public User User { get; set; }
}