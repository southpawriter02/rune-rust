// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeSpecialBonusTests.cs
// Unit tests for the ArchetypeSpecialBonus value object verifying factory
// methods, computed properties, application logic, validation, and formatting.
// Version: 0.17.3b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ArchetypeSpecialBonus"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that ArchetypeSpecialBonus correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates consumable effectiveness bonuses via the dedicated factory method</description></item>
///   <item><description>Creates validated bonuses via the general Create factory method</description></item>
///   <item><description>Rejects null or whitespace bonus types and descriptions</description></item>
///   <item><description>Computes multiplier, percentage, and type identification properties</description></item>
///   <item><description>Applies bonus multiplier correctly to int and float base values</description></item>
///   <item><description>Produces correctly formatted display and debug strings</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ArchetypeSpecialBonus"/>
/// <seealso cref="ArchetypeResourceBonuses"/>
[TestFixture]
public class ArchetypeSpecialBonusTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — CreateConsumableEffectiveness
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.CreateConsumableEffectiveness"/>
    /// creates a bonus with the correct type, value, percentage, and description
    /// for the Adept's +20% consumable effectiveness.
    /// </summary>
    [Test]
    public void CreateConsumableEffectiveness_CreatesCorrectBonus()
    {
        // Arrange & Act
        var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);

        // Assert
        bonus.BonusType.Should().Be("ConsumableEffectiveness");
        bonus.BonusValue.Should().Be(0.20f);
        bonus.PercentageValue.Should().Be(20);
        bonus.Description.Should().Be("+20% effectiveness from all consumable items");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.CreateConsumableEffectiveness"/>
    /// correctly identifies the bonus as a consumable effectiveness type.
    /// </summary>
    [Test]
    public void CreateConsumableEffectiveness_IsConsumableEffectiveness_ReturnsTrue()
    {
        // Arrange & Act
        var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);

        // Assert
        bonus.IsConsumableEffectiveness.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — Create
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.Create"/> creates a valid
    /// bonus with correct properties when given valid data.
    /// </summary>
    [Test]
    public void Create_WithValidData_CreatesBonus()
    {
        // Arrange & Act
        var bonus = ArchetypeSpecialBonus.Create(
            "ConsumableEffectiveness",
            0.20f,
            "+20% effectiveness from all consumable items");

        // Assert
        bonus.BonusType.Should().Be("ConsumableEffectiveness");
        bonus.BonusValue.Should().Be(0.20f);
        bonus.Description.Should().Be("+20% effectiveness from all consumable items");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.Create"/> throws
    /// <see cref="ArgumentException"/> when the bonus type is null.
    /// </summary>
    [Test]
    public void Create_WithNullBonusType_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeSpecialBonus.Create(null!, 0.20f, "Description");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.Create"/> throws
    /// <see cref="ArgumentException"/> when the bonus type is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceBonusType_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeSpecialBonus.Create("   ", 0.20f, "Description");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.Create"/> throws
    /// <see cref="ArgumentException"/> when the description is null.
    /// </summary>
    [Test]
    public void Create_WithNullDescription_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeSpecialBonus.Create("ConsumableEffectiveness", 0.20f, null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.Create"/> throws
    /// <see cref="ArgumentException"/> when the description is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceDescription_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeSpecialBonus.Create("ConsumableEffectiveness", 0.20f, "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.Multiplier"/> correctly
    /// returns 1.20 for a +20% bonus (1 + 0.20 = 1.20).
    /// </summary>
    [Test]
    public void Multiplier_Returns1Point2_ForTwentyPercentBonus()
    {
        // Arrange
        var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);

        // Act & Assert
        bonus.Multiplier.Should().Be(1.20f);
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.PercentageValue"/> correctly
    /// converts the float BonusValue to an integer percentage.
    /// </summary>
    [Test]
    public void PercentageValue_ReturnsIntegerPercentage()
    {
        // Arrange
        var bonus = new ArchetypeSpecialBonus("Test", 0.50f, "Test bonus");

        // Act & Assert
        bonus.PercentageValue.Should().Be(50);
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.IsConsumableEffectiveness"/>
    /// returns <c>false</c> for non-consumable bonus types.
    /// </summary>
    [Test]
    public void IsConsumableEffectiveness_ReturnsFalse_ForOtherBonusTypes()
    {
        // Arrange
        var bonus = new ArchetypeSpecialBonus("SomeOtherType", 0.10f, "Other bonus");

        // Act & Assert
        bonus.IsConsumableEffectiveness.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // APPLICATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.ApplyTo(int)"/> correctly
    /// multiplies an integer base value by the bonus multiplier.
    /// A +20% bonus applied to 100 should return 120.
    /// </summary>
    [Test]
    public void ApplyTo_Int_MultipliesCorrectly()
    {
        // Arrange
        var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);

        // Act
        var result = bonus.ApplyTo(100);

        // Assert
        result.Should().Be(120);
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.ApplyTo(float)"/> correctly
    /// multiplies a float base value by the bonus multiplier.
    /// A +20% bonus applied to 50.0 should return approximately 60.0.
    /// </summary>
    [Test]
    public void ApplyTo_Float_MultipliesCorrectly()
    {
        // Arrange
        var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);

        // Act
        var result = bonus.ApplyTo(50.0f);

        // Assert — use approximate comparison for floating-point arithmetic
        result.Should().BeApproximately(60.0f, 0.001f);
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.ApplyTo(int)"/> correctly
    /// truncates fractional results when applying to integers.
    /// A +20% bonus applied to 55 = 66.0 (55 × 1.20), truncated to 66.
    /// </summary>
    [Test]
    public void ApplyTo_Int_TruncatesFractionalResult()
    {
        // Arrange
        var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);

        // Act
        var result = bonus.ApplyTo(55);

        // Assert — 55 × 1.20 = 66.0, truncated to 66
        result.Should().Be(66);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY AND FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.GetShortDisplay"/> returns
    /// the expected compact format string.
    /// </summary>
    [Test]
    public void GetShortDisplay_ReturnsFormattedString()
    {
        // Arrange
        var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);

        // Act
        var result = bonus.GetShortDisplay();

        // Assert
        result.Should().Be("+20% ConsumableEffectiveness");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecialBonus.ToString"/> returns the
    /// expected debug format string.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);

        // Act
        var result = bonus.ToString();

        // Assert
        result.Should().Be("ConsumableEffectiveness: +20%");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the <see cref="ArchetypeSpecialBonus.ConsumableEffectivenessType"/>
    /// constant has the expected value.
    /// </summary>
    [Test]
    public void ConsumableEffectivenessType_HasExpectedValue()
    {
        // Assert
        ArchetypeSpecialBonus.ConsumableEffectivenessType.Should().Be("ConsumableEffectiveness");
    }
}
