namespace DotBoil.MessageBroker
{
    public interface IEvent
    {
        public Guid Id { get; set; }
    }

    public abstract class Event : IEvent
    {
        public Guid Id { get; set; }
    }
}
