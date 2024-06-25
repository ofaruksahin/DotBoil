using System.ComponentModel.DataAnnotations.Schema;

namespace DotBoil.MassTransit.Entities
{
    [Table("Inbox", Schema = "MassTransit")]
    public class InboxMessage
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
