using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotBoil.MassTransit.Entities
{
    [Table("Outbox")]
    internal class OutboxMessage
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("MessageType")]
        public string MessageType { get; set; }

        [Column("Content")]
        public string Content { get; set; }

        [Column("CreatedAt")]
        public DateTimeOffset CreateTime { get; set; }

        [Column("Processed")]
        public bool Processed { get; set; }
    }
}
