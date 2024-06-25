namespace DotBoil.MassTransit.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DomainEventAttribute : Attribute
    {
        public string QueueName { get; set; }
    }
}
