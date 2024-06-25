using DotBoil.Events;

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

        private List<IDomainEvent> _domainEvents;
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

        protected BaseEntity()
        {
            _domainEvents = new List<IDomainEvent>();
        }

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
    }
}
