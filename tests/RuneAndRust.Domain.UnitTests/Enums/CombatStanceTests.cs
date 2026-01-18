using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the CombatStance enum.
/// </summary>
/// <remarks>
/// <para>Verifies that the CombatStance enum has the expected values:</para>
/// <list type="bullet">
///   <item><description>Balanced (0): Default stance with no modifiers</description></item>
///   <item><description>Aggressive (1): Offensive stance with +ATK/+DMG/-DEF/-Saves</description></item>
///   <item><description>Defensive (2): Protective stance with -ATK/-DMG/+DEF/+Saves</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class CombatStanceTests
{
    // ═══════════════════════════════════════════════════════════════
    // DEFAULT VALUE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the default value of CombatStance is Balanced.
    /// </summary>
    /// <remarks>
    /// The default stance (value 0) should be Balanced, as new combatants
    /// start in a neutral stance without any modifiers.
    /// </remarks>
    [Test]
    public void CombatStance_DefaultValue_ShouldBeBalanced()
    {
        // Arrange & Act
        var defaultStance = default(CombatStance);

        // Assert
        defaultStance.Should().Be(CombatStance.Balanced);
    }

    // ═══════════════════════════════════════════════════════════════
    // ENUM VALUE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the CombatStance enum contains all expected values.
    /// </summary>
    [Test]
    public void CombatStance_ShouldHaveAllExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<CombatStance>();

        // Assert
        values.Should().HaveCount(3);
        values.Should().Contain(CombatStance.Balanced);
        values.Should().Contain(CombatStance.Aggressive);
        values.Should().Contain(CombatStance.Defensive);
    }

    /// <summary>
    /// Verifies that each stance has the correct underlying integer value.
    /// </summary>
    /// <remarks>
    /// The explicit values ensure stable serialization and JSON mapping:
    /// Balanced = 0, Aggressive = 1, Defensive = 2
    /// </remarks>
    [Test]
    [TestCase(CombatStance.Balanced, 0)]
    [TestCase(CombatStance.Aggressive, 1)]
    [TestCase(CombatStance.Defensive, 2)]
    public void CombatStance_ShouldHaveCorrectIntegerValues(CombatStance stance, int expectedValue)
    {
        // Act
        var actualValue = (int)stance;

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    // ═══════════════════════════════════════════════════════════════
    // STRING CONVERSION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that each stance converts to the expected string representation.
    /// </summary>
    /// <remarks>
    /// String representation is used for logging and JSON configuration mapping.
    /// </remarks>
    [Test]
    [TestCase(CombatStance.Balanced, "Balanced")]
    [TestCase(CombatStance.Aggressive, "Aggressive")]
    [TestCase(CombatStance.Defensive, "Defensive")]
    public void CombatStance_ToString_ShouldReturnExpectedString(CombatStance stance, string expectedString)
    {
        // Act
        var actualString = stance.ToString();

        // Assert
        actualString.Should().Be(expectedString);
    }

    // ═══════════════════════════════════════════════════════════════
    // STANCE DISTINCTION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Aggressive and Defensive stances are distinct from Balanced.
    /// </summary>
    /// <remarks>
    /// This ensures that modifier-applying stances can be distinguished from the neutral stance.
    /// </remarks>
    [Test]
    [TestCase(CombatStance.Aggressive)]
    [TestCase(CombatStance.Defensive)]
    public void CombatStance_ModifierStances_ShouldBeDistinctFromBalanced(CombatStance modifierStance)
    {
        // Assert
        modifierStance.Should().NotBe(CombatStance.Balanced);
    }

    /// <summary>
    /// Verifies that Aggressive and Defensive stances are distinct from each other.
    /// </summary>
    /// <remarks>
    /// Aggressive and Defensive represent opposite combat philosophies
    /// and must be distinguishable.
    /// </remarks>
    [Test]
    public void CombatStance_AggressiveAndDefensive_ShouldBeDistinct()
    {
        // Assert
        CombatStance.Aggressive.Should().NotBe(CombatStance.Defensive);
    }

    // ═══════════════════════════════════════════════════════════════
    // PARSE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CombatStance can be parsed from string representations.
    /// </summary>
    [Test]
    [TestCase("Balanced", CombatStance.Balanced)]
    [TestCase("Aggressive", CombatStance.Aggressive)]
    [TestCase("Defensive", CombatStance.Defensive)]
    [TestCase("balanced", CombatStance.Balanced)]
    [TestCase("AGGRESSIVE", CombatStance.Aggressive)]
    public void CombatStance_Parse_ShouldHandleVariousCases(string input, CombatStance expected)
    {
        // Act
        var success = Enum.TryParse<CombatStance>(input, ignoreCase: true, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }
}
