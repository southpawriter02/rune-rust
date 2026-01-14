using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for TimeOfDay enum.
/// </summary>
[TestFixture]
public class TimeOfDayTests
{
    [Test]
    public void TimeOfDay_HasSevenValues()
    {
        // Assert
        var values = Enum.GetValues<TimeOfDay>();
        values.Should().HaveCount(7);
    }

    [Test]
    public void TimeOfDay_ContainsExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(TimeOfDay), TimeOfDay.Dawn).Should().BeTrue();
        Enum.IsDefined(typeof(TimeOfDay), TimeOfDay.Morning).Should().BeTrue();
        Enum.IsDefined(typeof(TimeOfDay), TimeOfDay.Noon).Should().BeTrue();
        Enum.IsDefined(typeof(TimeOfDay), TimeOfDay.Afternoon).Should().BeTrue();
        Enum.IsDefined(typeof(TimeOfDay), TimeOfDay.Dusk).Should().BeTrue();
        Enum.IsDefined(typeof(TimeOfDay), TimeOfDay.Evening).Should().BeTrue();
        Enum.IsDefined(typeof(TimeOfDay), TimeOfDay.Night).Should().BeTrue();
    }
}
