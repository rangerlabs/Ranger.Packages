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
                new DailySchedule(new LocalTime(0, 0, 0, 1), new LocalTime(0, 0, 0, 0));
            });
        }
    }
}
