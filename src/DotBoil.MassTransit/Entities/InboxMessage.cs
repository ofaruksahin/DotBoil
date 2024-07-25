using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotBoil.MassTransit.Entities
{
    [Table("Inbox")]
    internal class InboxMessage
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("MessageId")]
        public Guid MessageId { get; set; }

        [Column("ProcessedTime")]
        public DateTimeOffset ProcessedTime { get; set; }
    }
}
