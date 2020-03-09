using System;
using NodaTime;

namespace Ranger.Common
{
    public class Schedule
    {
        public Schedule(
            DailySchedule sunday,
            DailySchedule monday,
            DailySchedule tuesday,
            DailySchedule wednesday,
            DailySchedule thursday,
            DailySchedule friday,
            DailySchedule saturday,
            string timeZoneId
            )
        {
            Sunday = sunday;
            Monday = monday;
            Tuesday = tuesday;
            Wednesday = wednesday;
            Thursday = thursday;
            Friday = friday;
            Saturday = saturday;
            TimeZoneId = timeZoneId;
        }

        public string TimeZoneId { get; set; }
        public DailySchedule Sunday { get; set; }
        public DailySchedule Monday { get; set; }
        public DailySchedule Tuesday { get; set; }
        public DailySchedule Wednesday { get; set; }
        public DailySchedule Thursday { get; set; }
        public DailySchedule Friday { get; set; }
        public DailySchedule Saturday { get; set; }

        public static DailySchedule FullDay => new DailySchedule(new LocalTime(0, 0, 0, 0), new LocalTime(23, 59, 59, 999));
        public static DailySchedule EmptyDay => new DailySchedule(new LocalTime(0, 0, 0, 0), new LocalTime(0, 0, 0, 0));

        public static Schedule FullSchedule(string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
            {
                throw new ArgumentException($"{nameof(timeZoneId)} was null or whitespace.");
            }
            return new Schedule(FullDay, FullDay, FullDay, FullDay, FullDay, FullDay, FullDay, timeZoneId);
        }

        public bool IsWithinSchedule(DateTime eventDateTime)
        {
            if (eventDateTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException($"{nameof(eventDateTime)} is not a UTC DateTime.");
            }
            var eventInstance = LocalDateTime.FromDateTime(eventDateTime).InUtc().ToInstant().InZone(DateTimeZoneProviders.Tzdb[this.TimeZoneId]).ToDateTimeUnspecified();

            var daySchedule = GetScheduleForEventDay(eventInstance.DayOfWeek);

            var offseLocalStartTime = LocalDateTime.FromDateTime(new DateTime(
                eventInstance.Year,
                eventInstance.Month,
                eventInstance.Day,
                daySchedule.StartTime.Hour,
                daySchedule.StartTime.Minute,
                daySchedule.StartTime.Second,
                DateTimeKind.Unspecified)).InUtc().ToDateTimeUnspecified();

            var offsetLocalEndTime = LocalDateTime.FromDateTime(new DateTime(
                eventInstance.Year,
                eventInstance.Month,
                eventInstance.Day,
                daySchedule.EndTime.Hour,
                daySchedule.EndTime.Minute,
                daySchedule.EndTime.Second,
                DateTimeKind.Unspecified)).InUtc().ToDateTimeUnspecified();

            return (offseLocalStartTime <= eventInstance && eventInstance <= offsetLocalEndTime) ? true : false;
        }

        private Offset OffsetFromUTCForEventDay(DateTime dateTime)
        {
            return DateTimeZoneProviders.Tzdb[this.TimeZoneId].GetUtcOffset(Instant.FromDateTimeUtc(dateTime));
        }

        private DailySchedule GetScheduleForEventDay(DayOfWeek dayOfWeek)
        {
            var day = Enum.GetName(typeof(DayOfWeek), dayOfWeek);
            var propertyInfo = this.GetType().GetProperty(day);
            var daySchedule = (DailySchedule)propertyInfo.GetValue(this);
            return daySchedule;
        }
    }
}