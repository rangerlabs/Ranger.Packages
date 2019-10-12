using System.Collections.Generic;

namespace Ranger.Common
{
    public class ApiErrorContent
    {
        public IList<string> Errors { get; set; } = new List<string>();
    }
}