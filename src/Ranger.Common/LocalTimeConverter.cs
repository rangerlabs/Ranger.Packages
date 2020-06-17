using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using NodaTime.Text;

namespace Ranger.Common
{
    public class ScheduleDailyLocalTimeConverter : DelegatingConverterBase
    {
        private static readonly JsonConverter converter = new NodaPatternConverter<LocalTime>(LocalTimePattern.CreateWithInvariantCulture("HH:mm:ss"));


        public ScheduleDailyLocalTimeConverter() : base(converter)
        {
        }
    }
}