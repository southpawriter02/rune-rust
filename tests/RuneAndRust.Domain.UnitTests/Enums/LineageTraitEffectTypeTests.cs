using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="LineageTraitEffectType"/> enum.
/// </summary>
/// <remarks>
/// Verifies correct enum values and count for trait effect types.
/// </remarks>
[TestFixture]
public class LineageTraitEffectTypeTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ENUM VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the enum has exactly 4 values.
    /// </summary>
    [Test]
    public void LineageTraitEffectType_HasFourValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<LineageTraitEffectType>();

        // Assert
        values.Should().HaveCount(4);
    }

    /// <summary>
    /// Verifies that all enum values have the expected integer assignments.
    /// </summary>
    [Test]
    public void LineageTraitEffectType_AllValuesHaveExpectedIntegerValues()
    {
        // Assert
        ((int)LineageTraitEffectType.BonusDiceToSkill).Should().Be(0);
        ((int)LineageTraitEffectType.PercentageModifier).Should().Be(1);
        ((int)LineageTraitEffectType.BonusDiceToResolve).Should().Be(2);
        ((int)LineageTraitEffectType.PassiveAuraBonus).Should().Be(3);
    }
}
