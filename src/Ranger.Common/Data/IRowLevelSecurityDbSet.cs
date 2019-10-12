using System.ComponentModel.DataAnnotations;

namespace Ranger.Common
{
    public interface IRowLevelSecurityDbSet
    {
        [Required]
        string DatabaseUsername { get; set; }
    }
}