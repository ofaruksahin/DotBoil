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

    public string Email { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Username { get; set; }

    public UserCreatedDomainEvent()
    {
    }

    public UserCreatedDomainEvent(
        string email,
        string name,
        string surname,
        string username)
    {
        Email = email;
        Name = name;
        Surname = surname;
        Username = username;
    }
}