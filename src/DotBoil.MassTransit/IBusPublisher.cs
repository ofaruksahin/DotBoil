using DotBoil.MessageBroker;

namespace DotBoil.MassTransit
{
    public interface IBusPublisher
    {
        Task Publish<T>(T message) where T : IEvent;
    }
}
