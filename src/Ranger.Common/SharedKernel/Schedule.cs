using System;
using NodaTime;

namespace Ranger.Common
{
    public class Schedule
    {
        public Schedule(
            Tuple<LocalTime, LocalTime> sunday,
            Tuple<LocalTime, LocalTime> monday,
            Tuple<LocalTime, LocalTime> tuesday,
            Tuple<LocalTime, LocalTime> wednesday,
            Tuple<LocalTime, LocalTime> thursday,
            Tuple<LocalTime, LocalTime> friday,
            Tuple<LocalTime, LocalTime> saturday
            )
        {
            Sunday = sunday;
            Monday = monday;
            Tuesday = tuesday;
            Wednesday = wednesday;
            Thursday = thursday;
            Friday = friday;
            Saturday = saturday;
        }

        public Tuple<LocalTime, LocalTime> Sunday { get; set; }
        public Tuple<LocalTime, LocalTime> Monday { get; set; }
        public Tuple<LocalTime, LocalTime> Tuesday { get; set; }
        public Tuple<LocalTime, LocalTime> Wednesday { get; set; }
        public Tuple<LocalTime, LocalTime> Thursday { get; set; }
        public Tuple<LocalTime, LocalTime> Friday { get; set; }
        public Tuple<LocalTime, LocalTime> Saturday { get; set; }

        public static Tuple<LocalTime, LocalTime> FullDay => new Tuple<LocalTime, LocalTime>(new LocalTime(0, 0, 0, 0), new LocalTime(23, 59, 59, 999));

        public static Schedule FullSchedule => new Schedule(FullDay, FullDay, FullDay, FullDay, FullDay, FullDay, FullDay);

        public bool IsWithinSchedule(DateTime dateTime)
        {
            var dayOfWeek = Enum.GetName(typeof(DayOfWeek), dateTime.DayOfWeek);
            var propertyInfo = this.GetType().GetProperty(dayOfWeek);
            var daySchedule = (Tuple<LocalTime, LocalTime>)propertyInfo.GetValue(this);
            var localDateTime = LocalDateTime.FromDateTime(dateTime.ToUniversalTime());
            return (daySchedule.Item1 <= localDateTime.TimeOfDay && localDateTime.TimeOfDay >= daySchedule.Item2) ? true : false;
        }
    }
}