using System.ComponentModel.DataAnnotations;

namespace Ranger.Common
{
    public class RowLevelSecurityDbSet
    {
        [Required]
        public string DatabaseUsername { get; set; }
    }
}