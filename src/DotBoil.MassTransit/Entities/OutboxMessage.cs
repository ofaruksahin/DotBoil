using System.ComponentModel.DataAnnotations.Schema;

namespace DotBoil.MassTransit.Entities
{
    [Table("Outbox", Schema = "MassTransit")]
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string MessageType { get; set; }
        public string QueueName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Processed { get; set; }
    }
}
