using System;
using NodaTime;

namespace Ranger.Common
{
    public class DailySchedule
    {
        public DailySchedule(LocalTime startTime, LocalTime endTime)
        {
            if (startTime > endTime)
            {
                throw new ArgumentException($"{nameof(startTime)} must be before or equal to ${nameof(endTime)}.");
            }
            StartTime = startTime;
            EndTime = endTime;
        }

        public LocalTime StartTime { get; }
        public LocalTime EndTime { get; }
    }
}