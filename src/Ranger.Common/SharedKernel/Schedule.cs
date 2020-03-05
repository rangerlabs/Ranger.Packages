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
            var daySchedule = GetScheduleForEventDay(eventDateTime);

            Offset offset = OffsetFromUTCForEventDay(eventDateTime);
            var offsetEventTime = LocalDateTime.FromDateTime(eventDateTime).WithOffset(offset);
            var offsetStartTime = daySchedule.Item1.WithOffset(offset);
            var offsetEndTime = daySchedule.Item2.WithOffset(offset);

            var offsetEventDateTime = LocalDateTime.FromDateTime(new DateTime(
                eventDateTime.Year,
                eventDateTime.Month,
                eventDateTime.Day,
                offsetEventTime.Hour,
                offsetEventTime.Minute,
                offsetEventTime.Second,
                DateTimeKind.Unspecified
            ));
            var offseLocalStartTime = LocalDateTime.FromDateTime(new DateTime(
                eventDateTime.Year,
                eventDateTime.Month,
                eventDateTime.Day,
                offsetStartTime.Hour,
                offsetStartTime.Minute,
                offsetStartTime.Second,
                DateTimeKind.Unspecified));

            var offsetLocalEndTime = LocalDateTime.FromDateTime(new DateTime(
                eventDateTime.Year,
                eventDateTime.Month,
                eventDateTime.Day,
                offsetEndTime.Hour,
                offsetEndTime.Minute,
                offsetEndTime.Second,
                DateTimeKind.Unspecified));

            return (offseLocalStartTime <= offsetEventDateTime && offsetEventDateTime <= offsetLocalEndTime) ? true : false;
        }

        private Offset OffsetFromUTCForEventDay(DateTime dateTime)
        {
            return DateTimeZoneProviders.Tzdb[this.TimeZoneId].GetUtcOffset(Instant.FromDateTimeUtc(dateTime));
        }

        private Tuple<LocalTime, LocalTime> GetScheduleForEventDay(DateTime dateTime)
        {
            var dayOfWeek = Enum.GetName(typeof(DayOfWeek), dateTime.DayOfWeek);
            var propertyInfo = this.GetType().GetProperty(dayOfWeek);
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
            if (tuple.Item1 >= tuple.Item2)
            {
                throw new ArgumentException("Item1 of the tuple must a time which is before Item2.");
            }
        }
    }
}