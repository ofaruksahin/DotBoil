using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotBoil.MassTransit.Entities
{
    [Table("RetryPolicyExceptions")]
    internal class RetryPolicyException
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("MessageId")]
        public Guid MessageId { get; set; }

        [Column("Exception")]
        public string Exception { get; set; }

        [Column("CreatedAt")]
        public DateTimeOffset CreateTime { get; set; }

        public RetryPolicyException()
        {
        }

        public RetryPolicyException(Guid messageId, string exception)
        {
            MessageId = messageId;
            Exception = exception;
            CreateTime = DateTimeOffset.Now;
        }
    }
}
