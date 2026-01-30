// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeResourceBonusesTests.cs
// Unit tests for the ArchetypeResourceBonuses value object verifying static
// archetype bonus profiles, factory method validation, computed properties,
// display summaries, and edge cases.
// Version: 0.17.3b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ArchetypeResourceBonuses"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that ArchetypeResourceBonuses correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Provides correct static bonus profiles for all 4 archetypes</description></item>
///   <item><description>Computes Has* boolean properties from bonus values</description></item>
///   <item><description>Calculates total resource bonus (HP + Stamina + AP)</description></item>
///   <item><description>Validates non-negative values via the Create factory method</description></item>
///   <item><description>Formats display summaries for the character creation UI</description></item>
///   <item><description>Handles the None empty bonus set correctly</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ArchetypeResourceBonuses"/>
/// <seealso cref="ArchetypeSpecialBonus"/>
[TestFixture]
public class ArchetypeResourceBonusesTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS — Warrior
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.Warrior"/> has the
    /// correct bonus values: +49 HP, +5 Stamina, 0 AP, 0 Movement, no special.
    /// </summary>
    [Test]
    public void Warrior_HasCorrectBonuses()
    {
        // Arrange & Act
        var bonuses = ArchetypeResourceBonuses.Warrior;

        // Assert
        bonuses.MaxHpBonus.Should().Be(49);
        bonuses.MaxStaminaBonus.Should().Be(5);
        bonuses.MaxAetherPoolBonus.Should().Be(0);
        bonuses.MovementBonus.Should().Be(0);
        bonuses.HasSpecialBonus.Should().BeFalse();
        bonuses.HasHpBonus.Should().BeTrue();
        bonuses.HasStaminaBonus.Should().BeTrue();
        bonuses.HasAetherPoolBonus.Should().BeFalse();
        bonuses.HasMovementBonus.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS — Skirmisher
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.Skirmisher"/> has the
    /// correct bonus values, specifically the +1 Movement bonus that defines
    /// the Skirmisher's mobility advantage.
    /// </summary>
    [Test]
    public void Skirmisher_HasMovementBonus()
    {
        // Arrange & Act
        var bonuses = ArchetypeResourceBonuses.Skirmisher;

        // Assert
        bonuses.MaxHpBonus.Should().Be(30);
        bonuses.MaxStaminaBonus.Should().Be(5);
        bonuses.MaxAetherPoolBonus.Should().Be(0);
        bonuses.MovementBonus.Should().Be(1);
        bonuses.HasMovementBonus.Should().BeTrue();
        bonuses.HasSpecialBonus.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS — Mystic
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.Mystic"/> has the
    /// correct bonus values, specifically the +20 Aether Pool bonus that
    /// defines the Mystic's magical resource advantage.
    /// </summary>
    [Test]
    public void Mystic_HasAetherPoolBonus()
    {
        // Arrange & Act
        var bonuses = ArchetypeResourceBonuses.Mystic;

        // Assert
        bonuses.MaxHpBonus.Should().Be(20);
        bonuses.MaxAetherPoolBonus.Should().Be(20);
        bonuses.HasAetherPoolBonus.Should().BeTrue();
        bonuses.HasStaminaBonus.Should().BeFalse();
        bonuses.HasMovementBonus.Should().BeFalse();
        bonuses.HasSpecialBonus.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS — Adept
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.Adept"/> has the
    /// correct bonus values, specifically the +20% ConsumableEffectiveness
    /// special bonus that defines the Adept's support identity.
    /// </summary>
    [Test]
    public void Adept_HasSpecialBonus()
    {
        // Arrange & Act
        var bonuses = ArchetypeResourceBonuses.Adept;

        // Assert
        bonuses.MaxHpBonus.Should().Be(30);
        bonuses.MaxStaminaBonus.Should().Be(0);
        bonuses.MaxAetherPoolBonus.Should().Be(0);
        bonuses.MovementBonus.Should().Be(0);
        bonuses.HasSpecialBonus.Should().BeTrue();
        bonuses.SpecialBonus!.Value.BonusType.Should().Be("ConsumableEffectiveness");
        bonuses.SpecialBonus!.Value.BonusValue.Should().Be(0.20f);
        bonuses.SpecialBonus!.Value.PercentageValue.Should().Be(20);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS — None
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.None"/> has all bonuses
    /// at zero and no special bonus.
    /// </summary>
    [Test]
    public void None_HasNoActiveBonuses()
    {
        // Arrange & Act
        var bonuses = ArchetypeResourceBonuses.None;

        // Assert
        bonuses.MaxHpBonus.Should().Be(0);
        bonuses.MaxStaminaBonus.Should().Be(0);
        bonuses.MaxAetherPoolBonus.Should().Be(0);
        bonuses.MovementBonus.Should().Be(0);
        bonuses.HasHpBonus.Should().BeFalse();
        bonuses.HasStaminaBonus.Should().BeFalse();
        bonuses.HasAetherPoolBonus.Should().BeFalse();
        bonuses.HasMovementBonus.Should().BeFalse();
        bonuses.HasSpecialBonus.Should().BeFalse();
        bonuses.TotalResourceBonus.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOTAL RESOURCE BONUS TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.TotalResourceBonus"/>
    /// correctly sums HP + Stamina + AP for the Warrior archetype (49+5+0=54).
    /// </summary>
    [Test]
    public void TotalResourceBonus_Warrior_Returns54()
    {
        // Arrange & Act
        var bonuses = ArchetypeResourceBonuses.Warrior;

        // Assert
        bonuses.TotalResourceBonus.Should().Be(54);
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.TotalResourceBonus"/>
    /// correctly sums HP + Stamina + AP for the Skirmisher archetype (30+5+0=35).
    /// </summary>
    [Test]
    public void TotalResourceBonus_Skirmisher_Returns35()
    {
        // Arrange & Act
        var bonuses = ArchetypeResourceBonuses.Skirmisher;

        // Assert
        bonuses.TotalResourceBonus.Should().Be(35);
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.TotalResourceBonus"/>
    /// correctly sums HP + Stamina + AP for the Mystic archetype (20+0+20=40).
    /// </summary>
    [Test]
    public void TotalResourceBonus_Mystic_Returns40()
    {
        // Arrange & Act
        var bonuses = ArchetypeResourceBonuses.Mystic;

        // Assert
        bonuses.TotalResourceBonus.Should().Be(40);
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.TotalResourceBonus"/>
    /// correctly sums HP + Stamina + AP for the Adept archetype (30+0+0=30).
    /// </summary>
    [Test]
    public void TotalResourceBonus_Adept_Returns30()
    {
        // Arrange & Act
        var bonuses = ArchetypeResourceBonuses.Adept;

        // Assert
        bonuses.TotalResourceBonus.Should().Be(30);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — Create Validation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.Create"/> creates a
    /// valid bonus set with correct properties when given valid data.
    /// </summary>
    [Test]
    public void Create_WithValidData_CreatesBonuses()
    {
        // Arrange & Act
        var bonuses = ArchetypeResourceBonuses.Create(49, 5, 0, 0);

        // Assert
        bonuses.MaxHpBonus.Should().Be(49);
        bonuses.MaxStaminaBonus.Should().Be(5);
        bonuses.MaxAetherPoolBonus.Should().Be(0);
        bonuses.MovementBonus.Should().Be(0);
        bonuses.HasSpecialBonus.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.Create"/> correctly
    /// includes a special bonus when provided.
    /// </summary>
    [Test]
    public void Create_WithSpecialBonus_IncludesSpecialBonus()
    {
        // Arrange
        var specialBonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);

        // Act
        var bonuses = ArchetypeResourceBonuses.Create(30, 0, 0, 0, specialBonus);

        // Assert
        bonuses.HasSpecialBonus.Should().BeTrue();
        bonuses.SpecialBonus!.Value.BonusType.Should().Be("ConsumableEffectiveness");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when the HP bonus is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeHpBonus_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArchetypeResourceBonuses.Create(-10, 0, 0, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when the Stamina bonus is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeStaminaBonus_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArchetypeResourceBonuses.Create(0, -5, 0, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when the Aether Pool bonus is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeAetherPoolBonus_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArchetypeResourceBonuses.Create(0, 0, -20, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when the Movement bonus is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeMovementBonus_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArchetypeResourceBonuses.Create(0, 0, 0, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY SUMMARY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.GetDisplaySummary"/>
    /// returns a formatted string containing all active Warrior bonuses.
    /// </summary>
    [Test]
    public void GetDisplaySummary_Warrior_ReturnsFormattedString()
    {
        // Arrange
        var bonuses = ArchetypeResourceBonuses.Warrior;

        // Act
        var summary = bonuses.GetDisplaySummary();

        // Assert
        summary.Should().Contain("+49 HP");
        summary.Should().Contain("+5 Stamina");
        summary.Should().NotContain("Aether Pool");
        summary.Should().NotContain("Movement");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.GetDisplaySummary"/>
    /// includes the Skirmisher's movement bonus in the summary.
    /// </summary>
    [Test]
    public void GetDisplaySummary_Skirmisher_IncludesMovementBonus()
    {
        // Arrange
        var bonuses = ArchetypeResourceBonuses.Skirmisher;

        // Act
        var summary = bonuses.GetDisplaySummary();

        // Assert
        summary.Should().Contain("+1 Movement");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.GetDisplaySummary"/>
    /// includes the Mystic's Aether Pool bonus in the summary.
    /// </summary>
    [Test]
    public void GetDisplaySummary_Mystic_IncludesAetherPoolBonus()
    {
        // Arrange
        var bonuses = ArchetypeResourceBonuses.Mystic;

        // Act
        var summary = bonuses.GetDisplaySummary();

        // Assert
        summary.Should().Contain("+20 Aether Pool");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.GetDisplaySummary"/>
    /// includes the Adept's special bonus description in the summary.
    /// </summary>
    [Test]
    public void GetDisplaySummary_Adept_IncludesSpecialBonusDescription()
    {
        // Arrange
        var bonuses = ArchetypeResourceBonuses.Adept;

        // Act
        var summary = bonuses.GetDisplaySummary();

        // Assert
        summary.Should().Contain("+30 HP");
        summary.Should().Contain("+20% effectiveness from all consumable items");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.GetDisplaySummary"/>
    /// returns "No bonuses" for the <see cref="ArchetypeResourceBonuses.None"/> set.
    /// </summary>
    [Test]
    public void GetDisplaySummary_None_ReturnsNoBonuses()
    {
        // Arrange
        var bonuses = ArchetypeResourceBonuses.None;

        // Act
        var summary = bonuses.GetDisplaySummary();

        // Assert
        summary.Should().Be("No bonuses");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SPECIAL BONUS DESCRIPTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.GetSpecialBonusDescription"/>
    /// returns the description for the Adept's special bonus.
    /// </summary>
    [Test]
    public void GetSpecialBonusDescription_Adept_ReturnsDescription()
    {
        // Arrange
        var bonuses = ArchetypeResourceBonuses.Adept;

        // Act
        var description = bonuses.GetSpecialBonusDescription();

        // Assert
        description.Should().Be("+20% effectiveness from all consumable items");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.GetSpecialBonusDescription"/>
    /// returns null for archetypes without special bonuses.
    /// </summary>
    [Test]
    public void GetSpecialBonusDescription_Warrior_ReturnsNull()
    {
        // Arrange
        var bonuses = ArchetypeResourceBonuses.Warrior;

        // Act
        var description = bonuses.GetSpecialBonusDescription();

        // Assert
        description.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.ToString"/> returns the
    /// expected debug format for the Warrior archetype.
    /// </summary>
    [Test]
    public void ToString_Warrior_ReturnsFormattedString()
    {
        // Arrange
        var bonuses = ArchetypeResourceBonuses.Warrior;

        // Act
        var result = bonuses.ToString();

        // Assert
        result.Should().Be("HP+49, Stam+5, AP+0, Mov+0");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeResourceBonuses.ToString"/> includes the
    /// special bonus type suffix for the Adept archetype.
    /// </summary>
    [Test]
    public void ToString_Adept_IncludesSpecialBonusType()
    {
        // Arrange
        var bonuses = ArchetypeResourceBonuses.Adept;

        // Act
        var result = bonuses.ToString();

        // Assert
        result.Should().Be("HP+30, Stam+0, AP+0, Mov+0, Special: ConsumableEffectiveness");
    }
}
