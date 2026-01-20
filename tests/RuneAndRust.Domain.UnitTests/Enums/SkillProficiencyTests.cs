using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for SkillProficiency enum.
/// </summary>
[TestFixture]
public class SkillProficiencyTests
{
    [Test]
    public void SkillProficiency_HasSixLevels()
    {
        // Assert
        var values = Enum.GetValues<SkillProficiency>();
        values.Should().HaveCount(6);
    }

    [Test]
    public void SkillProficiency_UntrainedIsZero()
    {
        // Assert
        ((int)SkillProficiency.Untrained).Should().Be(0);
    }

    [Test]
    public void SkillProficiency_MasterIsFive()
    {
        // Assert
        ((int)SkillProficiency.Master).Should().Be(5);
    }
}
