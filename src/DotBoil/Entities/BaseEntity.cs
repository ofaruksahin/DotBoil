using DotBoil.MessageBroker;

namespace DotBoil.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public string CreateUser { get; set; }
        public string ModifyUser { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public bool IsDeleted { get; set; }

        private List<IEvent> _events;
        public IReadOnlyList<IEvent> Events => _events;

        protected BaseEntity()
        {
            _events = new List<IEvent>();
        }

        public void AddEvent(IEvent domainEvent)
        {
            _events.Add(domainEvent);
        }
    }
}
