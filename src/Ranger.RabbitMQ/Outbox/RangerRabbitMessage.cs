using System.ComponentModel.DataAnnotations;

namespace Ranger.RabbitMQ
{
    public class RangerRabbitMessage
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Headers { get; set; }
        [Required]
        public string Body { get; set; }
    }
}