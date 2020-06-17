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
            if (StartTime > EndTime)
            {
                throw new ArgumentException($"{nameof(startTime)} must be before or equal to ${nameof(endTime)}");
            }
        }

        public LocalTime StartTime { get; }
        public LocalTime EndTime { get; }
    }
}