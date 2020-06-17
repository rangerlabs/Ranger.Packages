using System;
using NodaTime;
using Xunit;
using Shouldly;

namespace Ranger.Common.Tests
{



    public class DailyScheduleTests : IClassFixture<ScheduleFixture>
    {
        [Fact]
        public void Constructor_Throws_If_First_Time_Greater_Than_Second_Time()
        {
            Should.Throw<ArgumentException>(() =>
            {
                new DailySchedule(new LocalTime(0, 0, 1, 0), new LocalTime(0, 0, 0, 0));
            });
        }

        [Fact]
        public void Constructor_Trancates_Fractional_Seconds_Before_Comparison()
        {
            Should.NotThrow(() =>
            {
                new DailySchedule(new LocalTime(0, 0, 0, 1), new LocalTime(0, 0, 0, 0));
            });
        }

        [Fact]
        public void Consructor_Truncates_Fractional_Seconds()
        {

            var startTime = new LocalTime(0, 0, 0, 1);
            var endTime = new LocalTime(0, 0, 0, 0);
            var schedule = new DailySchedule(startTime, endTime);

            schedule.StartTime.Hour.ShouldBe(startTime.Hour);
            schedule.StartTime.Minute.ShouldBe(startTime.Minute);
            schedule.StartTime.Second.ShouldBe(startTime.Second);
            schedule.StartTime.Millisecond.ShouldBe(0);

            schedule.EndTime.Hour.ShouldBe(endTime.Hour);
            schedule.EndTime.Minute.ShouldBe(endTime.Minute);
            schedule.EndTime.Second.ShouldBe(endTime.Second);
            schedule.EndTime.Millisecond.ShouldBe(0);
        }
    }
}
