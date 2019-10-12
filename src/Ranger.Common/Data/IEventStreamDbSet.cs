using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ranger.Common
{
    public interface IEventStreamDbSet<TStreamData>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        int Id { get; set; }
        Guid StreamId { get; set; }
        [Required]
        [StringLength(28)]
        string Domain { get; set; }
        [Required]
        int Version { get; set; }
        [Required]
        [Column(TypeName = "jsonb")]
        string Data { get; set; }
        [Required]
        string Event { get; set; }
        [Required]
        DateTime InsertedAt { get; set; }
        [Required]
        string InsertedBy { get; set; }
    }
}