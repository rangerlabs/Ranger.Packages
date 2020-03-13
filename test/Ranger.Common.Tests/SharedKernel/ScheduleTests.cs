using System;
using NodaTime;
using Xunit;
using Shouldly;

namespace Ranger.Common.Tests
{

    public class ScheduleFixture
    {
        public Schedule FullSchedule { get; }
        public Schedule NoSunday { get; }
        public Schedule HalfSunday { get; }

        public ScheduleFixture()
        {
            this.FullSchedule = Schedule.FullSchedule("America/New_York");

            this.NoSunday = Schedule.FullSchedule("America/New_York");
            this.NoSunday.Sunday = Schedule.EmptyDay;

            this.HalfSunday = Schedule.FullSchedule("America/New_York");
            this.HalfSunday.Sunday = new DailySchedule(new LocalTime(0, 0, 0, 0), new LocalTime(12, 0, 0, 0));
        }
    }

    public class ScheduleTests : IClassFixture<ScheduleFixture>
    {
        ScheduleFixture fixture;
        public ScheduleTests(ScheduleFixture scheduleFixture)
        {
            fixture = scheduleFixture;
        }

        [Fact]
        public void IsWithinSchedule_Returns_True_When_Time_Is_In_FullSchedule()
        {
            var aUtcEventOnSundayAfternoon = new DateTime(2020, 3, 8, 12, 0, 0, DateTimeKind.Utc);
            var fullScheduleResult = fixture.FullSchedule.IsWithinSchedule(aUtcEventOnSundayAfternoon);
            fullScheduleResult.ShouldBeTrue();
        }

        // March 8, 2020 12:00pm UTC -> 7:00AM or 8:00AM America/New_York depending on Daylight Savings
        [Fact]
        public void IsWithinSchedule_Returns_True_When_Time_Is_In_HalfSchedule()
        {
            var aUtcEventOnSundayAfternoon = new DateTime(2020, 3, 8, 12, 0, 0, DateTimeKind.Utc);

            var halfScheduleResult = fixture.HalfSunday.IsWithinSchedule(aUtcEventOnSundayAfternoon);
            halfScheduleResult.ShouldBeTrue();
        }

        // March 8, 2020 10:00PM UTC -> 5:00PM or 6:00PM America/New_York depending on Daylight Savings
        [Fact]
        public void IsWithinSchedule_Returns_False_When_Time_Is_NOT_In_HalfSchedule()
        {
            var aUtcEventOnSundayAfternoonNearTheBoundry = new DateTime(2020, 3, 8, 20, 0, 0, DateTimeKind.Utc);

            var halfScheduleResult = fixture.HalfSunday.IsWithinSchedule(aUtcEventOnSundayAfternoonNearTheBoundry);
            halfScheduleResult.ShouldBeFalse();
        }

        [Fact]
        public void IsWithinSchedule_Returns_False_When_Time_Is_NOT_In_NoSundaySchedule()
        {
            // March 8, 2020 10:00PM UTC -> 5:00PM or 6:00PM America/New_York depending on Daylight Savings
            var aUtcEventOnSundayAfternoon = new DateTime(2020, 3, 8, 20, 0, 0, DateTimeKind.Utc);
            var noSundayResult1 = fixture.NoSunday.IsWithinSchedule(aUtcEventOnSundayAfternoon);
            noSundayResult1.ShouldBeFalse();

            // March 9, 2020 1:00AM UTC -> March 8, 2020 8:00PM or 9:00PM America/New_York depending on Daylight Savings
            var aUtcEventOnMondayMorningNearTheBoundry = new DateTime(2020, 3, 9, 1, 0, 0, DateTimeKind.Utc);
            var noSundayResult2 = fixture.NoSunday.IsWithinSchedule(aUtcEventOnMondayMorningNearTheBoundry);
            noSundayResult2.ShouldBeFalse();
        }

        [Fact]
        public void IsUtcFullSchedule_Returns_False_When_TimeZoneId_Is_NOT_UTC()
        {
            var schedule = Schedule.FullUtcSchedule;
            schedule.TimeZoneId = "America/New_York";
            var result = Schedule.IsUtcFullSchedule(schedule);
            result.ShouldBeFalse();
        }

        [Fact]
        public void IsUtcFullSchedule_Returns_True_When_FullUtcSchedule()
        {
            var schedule = Schedule.FullUtcSchedule;
            var result = Schedule.IsUtcFullSchedule(schedule);
            result.ShouldBeTrue();
        }

        [Fact]
        public void IsUtcFullSchedule_Returns_False_When_NotFull()
        {
            var schedule = Schedule.FullUtcSchedule;
            schedule.Monday = new DailySchedule(new LocalTime(0, 0, 0, 0), new LocalTime(12, 0, 0, 0));
            var result = Schedule.IsUtcFullSchedule(schedule);
            result.ShouldBeFalse();
        }

        [Fact]
        public void IsFullDailySchedule_Returns_False_When_NotFull()
        {
            var dailySchedule = new DailySchedule(new LocalTime(0, 0, 0, 0), new LocalTime(12, 0, 0, 0));
            var result = Schedule.IsFullDailySchedule(dailySchedule);
            result.ShouldBeFalse();
        }

        [Fact]
        public void IsFullDailySchedule_Returns_True_When_Full()
        {
            var dailySchedule = Schedule.FullDay;
            var result = Schedule.IsFullDailySchedule(dailySchedule);
            result.ShouldBeTrue();
        }
    }
}
