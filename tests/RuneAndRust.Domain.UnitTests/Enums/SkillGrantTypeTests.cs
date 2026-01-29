using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="SkillGrantType"/> enum.
/// </summary>
/// <remarks>
/// Verifies that the SkillGrantType enum has the expected number of values
/// and that each value has the correct explicit integer assignment for
/// stable serialization and configuration mapping.
/// </remarks>
[TestFixture]
public class SkillGrantTypeTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ENUM VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that SkillGrantType has exactly 3 values.
    /// </summary>
    /// <remarks>
    /// The three grant types (Permanent, StartingBonus, Proficiency) cover all
    /// skill application modes needed by the background system.
    /// </remarks>
    [Test]
    public void SkillGrantType_HasThreeValues_CountEquals3()
    {
        // Arrange
        var values = Enum.GetValues<SkillGrantType>();

        // Assert
        values.Should().HaveCount(3,
            "SkillGrantType should define exactly 3 grant types: Permanent, StartingBonus, Proficiency");
    }

    /// <summary>
    /// Verifies that all SkillGrantType values have the expected explicit integer assignments.
    /// </summary>
    /// <remarks>
    /// Explicit integer values are critical for stable serialization and
    /// configuration file mapping. Permanent=0, StartingBonus=1, Proficiency=2.
    /// </remarks>
    [Test]
    public void SkillGrantType_ValuesHaveExpectedIntegers()
    {
        // Assert
        ((int)SkillGrantType.Permanent).Should().Be(0,
            "Permanent is the default grant type and should be 0");
        ((int)SkillGrantType.StartingBonus).Should().Be(1,
            "StartingBonus should be 1");
        ((int)SkillGrantType.Proficiency).Should().Be(2,
            "Proficiency should be 2");
    }
}
