using System;
using System.Text.Json.Serialization;
using NodaTime;

namespace Ranger.Common
{
    public class DailySchedule
    {
        public DailySchedule(LocalTime startTime, LocalTime endTime)
        {
            StartTime = TimeAdjusters.TruncateToSecond(startTime);
            EndTime = TimeAdjusters.TruncateToSecond(endTime);
        }

        public LocalTime StartTime { get; }
        public LocalTime EndTime { get; }
    }
}