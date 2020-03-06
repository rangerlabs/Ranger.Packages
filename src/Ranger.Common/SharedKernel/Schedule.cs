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
            Tuple<LocalTime, LocalTime> saturday,
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

        public string TimeZoneId { get; private set; }
        public Tuple<LocalTime, LocalTime> Sunday { get; private set; }
        public Tuple<LocalTime, LocalTime> Monday { get; private set; }
        public Tuple<LocalTime, LocalTime> Tuesday { get; private set; }
        public Tuple<LocalTime, LocalTime> Wednesday { get; private set; }
        public Tuple<LocalTime, LocalTime> Thursday { get; private set; }
        public Tuple<LocalTime, LocalTime> Friday { get; private set; }
        public Tuple<LocalTime, LocalTime> Saturday { get; private set; }

        public static Tuple<LocalTime, LocalTime> FullDay => new Tuple<LocalTime, LocalTime>(new LocalTime(0, 0, 0, 0), new LocalTime(23, 59, 59, 999));
        public static Tuple<LocalTime, LocalTime> EmptyDay => new Tuple<LocalTime, LocalTime>(new LocalTime(0, 0, 0, 0), new LocalTime(0, 0, 0, 0));

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
                daySchedule.Item1.Hour,
                daySchedule.Item1.Minute,
                daySchedule.Item1.Second,
                DateTimeKind.Unspecified)).InUtc().ToInstant().InZone(DateTimeZoneProviders.Tzdb[this.TimeZoneId]).ToDateTimeUnspecified();

            var offsetLocalEndTime = LocalDateTime.FromDateTime(new DateTime(
                eventInstance.Year,
                eventInstance.Month,
                eventInstance.Day,
                daySchedule.Item2.Hour,
                daySchedule.Item2.Minute,
                daySchedule.Item2.Second,
                DateTimeKind.Unspecified)).InUtc().ToInstant().InZone(DateTimeZoneProviders.Tzdb[this.TimeZoneId]).ToDateTimeUnspecified();

            return (offseLocalStartTime <= eventInstance && eventInstance <= offsetLocalEndTime) ? true : false;
        }

        private Offset OffsetFromUTCForEventDay(DateTime dateTime)
        {
            return DateTimeZoneProviders.Tzdb[this.TimeZoneId].GetUtcOffset(Instant.FromDateTimeUtc(dateTime));
        }

        private Tuple<LocalTime, LocalTime> GetScheduleForEventDay(DayOfWeek dayOfWeek)
        {
            var day = Enum.GetName(typeof(DayOfWeek), dayOfWeek);
            var propertyInfo = this.GetType().GetProperty(day);
            var daySchedule = (Tuple<LocalTime, LocalTime>)propertyInfo.GetValue(this);
            return daySchedule;
        }

        public void SetSunday(Tuple<LocalTime, LocalTime> scheduleTuple)
        {
            IsValidTuple(scheduleTuple);
            this.Sunday = scheduleTuple;
        }
        public void SetMonday(Tuple<LocalTime, LocalTime> scheduleTuple)
        {
            IsValidTuple(scheduleTuple);
            this.Monday = scheduleTuple;
        }
        public void SetTuesday(Tuple<LocalTime, LocalTime> scheduleTuple)
        {
            IsValidTuple(scheduleTuple);
            this.Sunday = scheduleTuple;
        }
        public void SetWednesday(Tuple<LocalTime, LocalTime> scheduleTuple)
        {
            IsValidTuple(scheduleTuple);
            this.Wednesday = scheduleTuple;
        }
        public void SetThursday(Tuple<LocalTime, LocalTime> scheduleTuple)
        {
            IsValidTuple(scheduleTuple);
            this.Thursday = scheduleTuple;
        }
        public void SetFriday(Tuple<LocalTime, LocalTime> scheduleTuple)
        {
            IsValidTuple(scheduleTuple);
            this.Friday = scheduleTuple;
        }
        public void SetSaturday(Tuple<LocalTime, LocalTime> scheduleTuple)
        {
            IsValidTuple(scheduleTuple);
            this.Saturday = scheduleTuple;
        }

        private void IsValidTuple(Tuple<LocalTime, LocalTime> tuple)
        {
            if (tuple is null)
            {
                throw new ArgumentException("Tuple was null.");
            }
            if (tuple.Item1 > tuple.Item2)
            {
                throw new ArgumentException("Item1 of the tuple must a time which is before Item2.");
            }
        }
    }
}