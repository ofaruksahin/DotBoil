namespace DotBoil.MassTransit.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class QueueAttribute : Attribute
    {
        public string Name { get; set; }

        public QueueAttribute(string name)
        {
            Name = name;
        }
    }
}
