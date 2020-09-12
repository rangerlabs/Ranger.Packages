using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ranger.RabbitMQ
{
    public class OutboxMessage
    {
        [Required]
        public int Id { get; set; }
        public int MessageId { get; set; }
        [Required]
        public RangerRabbitMessage Message { get; set; }
        [Required]
        public DateTime InsertedAt { get; set; }
        [Required]
        public bool Nacked { get; set; }
    }
}