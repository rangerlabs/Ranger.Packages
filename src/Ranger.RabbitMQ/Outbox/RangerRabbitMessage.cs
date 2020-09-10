using System.ComponentModel.DataAnnotations;

namespace Ranger.RabbitMQ
{
    public class RangerRabbitMessage
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public float MessageVersion { get; set; }
        [Required]
        public string Headers { get; set; }
        [Required]
        public string Body { get; set; }
    }
}