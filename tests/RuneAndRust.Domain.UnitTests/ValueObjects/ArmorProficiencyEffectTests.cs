using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ArmorProficiencyEffect"/> value object.
/// </summary>
[TestFixture]
public class ArmorProficiencyEffectTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests - Create
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create method produces a valid ArmorProficiencyEffect with valid parameters.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesArmorProficiencyEffect()
    {
        // Arrange & Act
        var effect = ArmorProficiencyEffect.Create(
            ArmorProficiencyLevel.Expert,
            penaltyMultiplier: 1.0m,
            attackModifier: 0,
            defenseModifier: 0,
            tierReduction: 1,
            canUseSpecialProperties: true,
            "Expert",
            "Armor treated as one tier lighter.");

        // Assert
        effect.Level.Should().Be(ArmorProficiencyLevel.Expert);
        effect.PenaltyMultiplier.Should().Be(1.0m);
        effect.AttackModifier.Should().Be(0);
        effect.DefenseModifier.Should().Be(0);
        effect.TierReduction.Should().Be(1);
        effect.CanUseSpecialProperties.Should().BeTrue();
        effect.DisplayName.Should().Be("Expert");
        effect.Description.Should().Be("Armor treated as one tier lighter.");
    }

    /// <summary>
    /// Verifies that Create throws when displayName is null.
    /// </summary>
    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentException()
    {
        // Arrange
        var act = () => ArmorProficiencyEffect.Create(
            ArmorProficiencyLevel.Proficient,
            1.0m, 0, 0, 0, true,
            null!,
            "Description");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws when description is empty.
    /// </summary>
    [Test]
    public void Create_WithEmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        var act = () => ArmorProficiencyEffect.Create(
            ArmorProficiencyLevel.Proficient,
            1.0m, 0, 0, 0, true,
            "Proficient",
            "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws when penaltyMultiplier is out of range.
    /// </summary>
    [Test]
    [TestCase(0.5)]
    [TestCase(3.5)]
    public void Create_WithOutOfRangePenaltyMultiplier_ThrowsArgumentOutOfRangeException(decimal penaltyMultiplier)
    {
        // Arrange
        var act = () => ArmorProficiencyEffect.Create(
            ArmorProficiencyLevel.Proficient,
            penaltyMultiplier,
            0, 0, 0, true,
            "Test",
            "Test description");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws when attackModifier is out of range.
    /// </summary>
    [Test]
    [TestCase(-6)]
    [TestCase(1)]
    public void Create_WithOutOfRangeAttackModifier_ThrowsArgumentOutOfRangeException(int attackModifier)
    {
        // Arrange
        var act = () => ArmorProficiencyEffect.Create(
            ArmorProficiencyLevel.Proficient,
            1.0m,
            attackModifier,
            0, 0, true,
            "Test",
            "Test description");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws when defenseModifier is out of range.
    /// </summary>
    [Test]
    [TestCase(-1)]
    [TestCase(4)]
    public void Create_WithOutOfRangeDefenseModifier_ThrowsArgumentOutOfRangeException(int defenseModifier)
    {
        // Arrange
        var act = () => ArmorProficiencyEffect.Create(
            ArmorProficiencyLevel.Proficient,
            1.0m,
            0,
            defenseModifier,
            0, true,
            "Test",
            "Test description");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateNonProficient produces correct default values.
    /// </summary>
    [Test]
    public void CreateNonProficient_ReturnsCorrectDefaults()
    {
        // Act
        var effect = ArmorProficiencyEffect.CreateNonProficient();

        // Assert
        effect.Level.Should().Be(ArmorProficiencyLevel.NonProficient);
        effect.PenaltyMultiplier.Should().Be(2.0m);
        effect.AttackModifier.Should().Be(-2);
        effect.DefenseModifier.Should().Be(0);
        effect.TierReduction.Should().Be(0);
        effect.CanUseSpecialProperties.Should().BeFalse();
        effect.DisplayName.Should().Be("Non-Proficient");
    }

    /// <summary>
    /// Verifies that CreateProficient produces correct default values.
    /// </summary>
    [Test]
    public void CreateProficient_ReturnsCorrectDefaults()
    {
        // Act
        var effect = ArmorProficiencyEffect.CreateProficient();

        // Assert
        effect.Level.Should().Be(ArmorProficiencyLevel.Proficient);
        effect.PenaltyMultiplier.Should().Be(1.0m);
        effect.AttackModifier.Should().Be(0);
        effect.DefenseModifier.Should().Be(0);
        effect.TierReduction.Should().Be(0);
        effect.CanUseSpecialProperties.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CreateExpert produces correct default values.
    /// </summary>
    [Test]
    public void CreateExpert_ReturnsCorrectDefaults()
    {
        // Act
        var effect = ArmorProficiencyEffect.CreateExpert();

        // Assert
        effect.Level.Should().Be(ArmorProficiencyLevel.Expert);
        effect.PenaltyMultiplier.Should().Be(1.0m);
        effect.AttackModifier.Should().Be(0);
        effect.DefenseModifier.Should().Be(0);
        effect.TierReduction.Should().Be(1);
        effect.CanUseSpecialProperties.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CreateMaster produces correct default values.
    /// </summary>
    [Test]
    public void CreateMaster_ReturnsCorrectDefaults()
    {
        // Act
        var effect = ArmorProficiencyEffect.CreateMaster();

        // Assert
        effect.Level.Should().Be(ArmorProficiencyLevel.Master);
        effect.PenaltyMultiplier.Should().Be(1.0m);
        effect.AttackModifier.Should().Be(0);
        effect.DefenseModifier.Should().Be(1);
        effect.TierReduction.Should().Be(1);
        effect.CanUseSpecialProperties.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies HasDoubledPenalties is true for NonProficient.
    /// </summary>
    [Test]
    public void HasDoubledPenalties_ForNonProficient_ReturnsTrue()
    {
        // Arrange
        var effect = ArmorProficiencyEffect.CreateNonProficient();

        // Assert
        effect.HasDoubledPenalties.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasDoubledPenalties is false for Proficient.
    /// </summary>
    [Test]
    public void HasDoubledPenalties_ForProficient_ReturnsFalse()
    {
        // Arrange
        var effect = ArmorProficiencyEffect.CreateProficient();

        // Assert
        effect.HasDoubledPenalties.Should().BeFalse();
    }

    /// <summary>
    /// Verifies HasAttackPenalty is true for NonProficient.
    /// </summary>
    [Test]
    public void HasAttackPenalty_ForNonProficient_ReturnsTrue()
    {
        // Arrange
        var effect = ArmorProficiencyEffect.CreateNonProficient();

        // Assert
        effect.HasAttackPenalty.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasAttackPenalty is false for Proficient.
    /// </summary>
    [Test]
    public void HasAttackPenalty_ForProficient_ReturnsFalse()
    {
        // Arrange
        var effect = ArmorProficiencyEffect.CreateProficient();

        // Assert
        effect.HasAttackPenalty.Should().BeFalse();
    }

    /// <summary>
    /// Verifies HasDefenseBonus is true for Master.
    /// </summary>
    [Test]
    public void HasDefenseBonus_ForMaster_ReturnsTrue()
    {
        // Arrange
        var effect = ArmorProficiencyEffect.CreateMaster();

        // Assert
        effect.HasDefenseBonus.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasDefenseBonus is false for Expert.
    /// </summary>
    [Test]
    public void HasDefenseBonus_ForExpert_ReturnsFalse()
    {
        // Arrange
        var effect = ArmorProficiencyEffect.CreateExpert();

        // Assert
        effect.HasDefenseBonus.Should().BeFalse();
    }

    /// <summary>
    /// Verifies HasTierReduction is true for Expert and Master.
    /// </summary>
    [Test]
    [TestCase(ArmorProficiencyLevel.NonProficient, false)]
    [TestCase(ArmorProficiencyLevel.Proficient, false)]
    [TestCase(ArmorProficiencyLevel.Expert, true)]
    [TestCase(ArmorProficiencyLevel.Master, true)]
    public void HasTierReduction_ReturnsExpectedValue(
        ArmorProficiencyLevel level, bool expected)
    {
        // Arrange
        var effect = level switch
        {
            ArmorProficiencyLevel.NonProficient => ArmorProficiencyEffect.CreateNonProficient(),
            ArmorProficiencyLevel.Proficient => ArmorProficiencyEffect.CreateProficient(),
            ArmorProficiencyLevel.Expert => ArmorProficiencyEffect.CreateExpert(),
            ArmorProficiencyLevel.Master => ArmorProficiencyEffect.CreateMaster(),
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };

        // Assert
        effect.HasTierReduction.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Format Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies FormatPenaltyMultiplier produces correct format.
    /// </summary>
    [Test]
    public void FormatPenaltyMultiplier_ReturnsCorrectFormat()
    {
        // Arrange
        var nonProficient = ArmorProficiencyEffect.CreateNonProficient();
        var proficient = ArmorProficiencyEffect.CreateProficient();

        // Assert
        nonProficient.FormatPenaltyMultiplier().Should().Be("2.0x");
        proficient.FormatPenaltyMultiplier().Should().Be("1.0x");
    }

    /// <summary>
    /// Verifies FormatAttackModifier produces correct format.
    /// </summary>
    [Test]
    public void FormatAttackModifier_ReturnsCorrectFormat()
    {
        // Arrange
        var nonProficient = ArmorProficiencyEffect.CreateNonProficient();
        var proficient = ArmorProficiencyEffect.CreateProficient();

        // Assert
        nonProficient.FormatAttackModifier().Should().Be("-2");
        proficient.FormatAttackModifier().Should().Be("+0");
    }

    /// <summary>
    /// Verifies FormatDefenseModifier produces correct format.
    /// </summary>
    [Test]
    public void FormatDefenseModifier_ReturnsCorrectFormat()
    {
        // Arrange
        var master = ArmorProficiencyEffect.CreateMaster();
        var expert = ArmorProficiencyEffect.CreateExpert();

        // Assert
        master.FormatDefenseModifier().Should().Be("+1");
        expert.FormatDefenseModifier().Should().Be("+0");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies ToString produces a readable summary.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedSummary()
    {
        // Arrange
        var effect = ArmorProficiencyEffect.CreateExpert();

        // Act
        var result = effect.ToString();

        // Assert
        result.Should().Contain("Expert");
        result.Should().Contain("Penalty 1.0x");
        result.Should().Contain("Atk +0");
        result.Should().Contain("TierReduce 1");
    }

    /// <summary>
    /// Verifies ToDebugString produces detailed output.
    /// </summary>
    [Test]
    public void ToDebugString_ReturnsDetailedOutput()
    {
        // Arrange
        var effect = ArmorProficiencyEffect.CreateMaster();

        // Act
        var result = effect.ToDebugString();

        // Assert
        result.Should().Contain("ArmorProficiencyEffect");
        result.Should().Contain("Level: Master");
        result.Should().Contain("(3)");
        result.Should().Contain("Def: +1");
        result.Should().Contain("TierReduce: 1");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LevelValue Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies LevelValue returns the integer representation of the level.
    /// </summary>
    [Test]
    [TestCase(ArmorProficiencyLevel.NonProficient, 0)]
    [TestCase(ArmorProficiencyLevel.Proficient, 1)]
    [TestCase(ArmorProficiencyLevel.Expert, 2)]
    [TestCase(ArmorProficiencyLevel.Master, 3)]
    public void LevelValue_ReturnsIntegerRepresentation(
        ArmorProficiencyLevel level, int expectedValue)
    {
        // Arrange
        var effect = level switch
        {
            ArmorProficiencyLevel.NonProficient => ArmorProficiencyEffect.CreateNonProficient(),
            ArmorProficiencyLevel.Proficient => ArmorProficiencyEffect.CreateProficient(),
            ArmorProficiencyLevel.Expert => ArmorProficiencyEffect.CreateExpert(),
            ArmorProficiencyLevel.Master => ArmorProficiencyEffect.CreateMaster(),
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };

        // Assert
        effect.LevelValue.Should().Be(expectedValue);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Identity Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies IsNonProficient property for NonProficient level.
    /// </summary>
    [Test]
    public void IsNonProficient_ForNonProficient_ReturnsTrue()
    {
        // Arrange
        var effect = ArmorProficiencyEffect.CreateNonProficient();

        // Assert
        effect.IsNonProficient.Should().BeTrue();
    }

    /// <summary>
    /// Verifies IsNonProficient property for other levels.
    /// </summary>
    [Test]
    public void IsNonProficient_ForOtherLevels_ReturnsFalse()
    {
        // Assert
        ArmorProficiencyEffect.CreateProficient().IsNonProficient.Should().BeFalse();
        ArmorProficiencyEffect.CreateExpert().IsNonProficient.Should().BeFalse();
        ArmorProficiencyEffect.CreateMaster().IsNonProficient.Should().BeFalse();
    }

    /// <summary>
    /// Verifies IsMaster property for Master level.
    /// </summary>
    [Test]
    public void IsMaster_ForMaster_ReturnsTrue()
    {
        // Arrange
        var effect = ArmorProficiencyEffect.CreateMaster();

        // Assert
        effect.IsMaster.Should().BeTrue();
    }

    /// <summary>
    /// Verifies IsMaster property for other levels.
    /// </summary>
    [Test]
    public void IsMaster_ForOtherLevels_ReturnsFalse()
    {
        // Assert
        ArmorProficiencyEffect.CreateNonProficient().IsMaster.Should().BeFalse();
        ArmorProficiencyEffect.CreateProficient().IsMaster.Should().BeFalse();
        ArmorProficiencyEffect.CreateExpert().IsMaster.Should().BeFalse();
    }
}
