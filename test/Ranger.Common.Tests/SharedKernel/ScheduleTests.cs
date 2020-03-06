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
            this.NoSunday.SetSunday(Schedule.EmptyDay);

            this.HalfSunday = Schedule.FullSchedule("America/New_York");
            this.HalfSunday.SetSunday(new Tuple<LocalTime, LocalTime>(new LocalTime(0, 0, 0, 0), new LocalTime(12, 0, 0, 0)));
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
        public void Setting_A_Day_Throws_If_First_Time_Greater_Than_Second_Time()
        {
            Should.Throw<ArgumentException>(() =>
            {
                fixture.NoSunday.SetFriday(new Tuple<LocalTime, LocalTime>(new LocalTime(0, 0, 0, 1), new LocalTime(0, 0, 0, 0)));
            });
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
    }
}
