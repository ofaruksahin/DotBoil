namespace DotBoil.MassTransit.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConsumerAttribute : Attribute
    {
        public string QueueName { get; private set; }

        public ConsumerAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}
