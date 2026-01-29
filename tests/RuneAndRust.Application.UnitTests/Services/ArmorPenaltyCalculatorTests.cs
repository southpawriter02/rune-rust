// ═══════════════════════════════════════════════════════════════════════════════
// ArmorPenaltyCalculatorTests.cs
// Unit tests for the ArmorPenaltyCalculator service.
// Version: 0.16.2d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="ArmorPenaltyCalculator"/>.
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>NonProficient penalty doubling (2.0x multiplier, -2 attack)</description></item>
///   <item><description>Expert tier reduction (Heavy → Medium penalties)</description></item>
///   <item><description>Master tier reduction with defense bonus</description></item>
///   <item><description>Archetype-based proficiency lookup</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ArmorPenaltyCalculatorTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mock armor category provider for testing.
    /// </summary>
    private Mock<IArmorCategoryProvider> _mockCategoryProvider = null!;

    /// <summary>
    /// Mock proficiency effect provider for testing.
    /// </summary>
    private Mock<IArmorProficiencyEffectProvider> _mockEffectProvider = null!;

    /// <summary>
    /// Mock archetype armor proficiency provider for testing.
    /// </summary>
    private Mock<IArchetypeArmorProficiencyProvider> _mockArchetypeProvider = null!;

    /// <summary>
    /// Mock logger for the service.
    /// </summary>
    private Mock<ILogger<ArmorPenaltyCalculator>> _mockLogger = null!;

    /// <summary>
    /// The service under test.
    /// </summary>
    private ArmorPenaltyCalculator _calculator = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // Standard Penalty Values (from design spec)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Light armor penalties (none).
    /// </summary>
    private static readonly ArmorPenalties LightPenalties = ArmorPenalties.None;

    /// <summary>
    /// Medium armor penalties: -1d10 Agi, +2 Stam, -5ft Move, Stealth Disadvantage.
    /// </summary>
    private static readonly ArmorPenalties MediumPenalties =
        ArmorPenalties.Create(-1, 2, -5, true);

    /// <summary>
    /// Heavy armor penalties: -2d10 Agi, +5 Stam, -10ft Move, Stealth Disadvantage.
    /// </summary>
    private static readonly ArmorPenalties HeavyPenalties =
        ArmorPenalties.Create(-2, 5, -10, true);

    /// <summary>
    /// Shield penalties: 0 Agi, +1 Stam, 0 Move, No Stealth Disadvantage.
    /// </summary>
    private static readonly ArmorPenalties ShieldPenalties =
        ArmorPenalties.Create(0, 1, 0, false);

    // ═══════════════════════════════════════════════════════════════════════════
    // Setup
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets up the test fixtures before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Initialize mocks
        _mockCategoryProvider = new Mock<IArmorCategoryProvider>();
        _mockEffectProvider = new Mock<IArmorProficiencyEffectProvider>();
        _mockArchetypeProvider = new Mock<IArchetypeArmorProficiencyProvider>();
        _mockLogger = new Mock<ILogger<ArmorPenaltyCalculator>>();

        // Set up default provider behavior
        SetupDefaultCategoryProvider();
        SetupDefaultEffectProvider();

        // Create the service under test
        _calculator = new ArmorPenaltyCalculator(
            _mockCategoryProvider.Object,
            _mockEffectProvider.Object,
            _mockArchetypeProvider.Object,
            _mockLogger.Object);
    }

    /// <summary>
    /// Configures the mock category provider with default definitions.
    /// </summary>
    private void SetupDefaultCategoryProvider()
    {
        // Light armor definition (Tier 0)
        var lightDef = ArmorCategoryDefinition.Create(
            ArmorCategory.Light,
            "Light Armor",
            "Flexible protection allowing unrestricted movement.",
            new[] { "Leather Armor", "Padded Armor" },
            LightPenalties,
            weightTier: 0,
            requiresSpecialTraining: false);

        // Medium armor definition (Tier 1)
        var mediumDef = ArmorCategoryDefinition.Create(
            ArmorCategory.Medium,
            "Medium Armor",
            "Balanced protection and mobility.",
            new[] { "Chain Mail", "Scale Mail" },
            MediumPenalties,
            weightTier: 1,
            requiresSpecialTraining: false);

        // Heavy armor definition (Tier 2)
        var heavyDef = ArmorCategoryDefinition.Create(
            ArmorCategory.Heavy,
            "Heavy Armor",
            "Maximum protection at the cost of mobility.",
            new[] { "Half Plate", "Full Plate" },
            HeavyPenalties,
            weightTier: 2,
            requiresSpecialTraining: false);

        // Shields definition (Tier -1, not in tier system)
        var shieldsDef = ArmorCategoryDefinition.Create(
            ArmorCategory.Shields,
            "Shields",
            "Held defensive equipment.",
            new[] { "Buckler", "Tower Shield" },
            ShieldPenalties,
            weightTier: -1,
            requiresSpecialTraining: false);

        // Setup GetDefinition for each category
        _mockCategoryProvider.Setup(p => p.GetDefinition(ArmorCategory.Light)).Returns(lightDef);
        _mockCategoryProvider.Setup(p => p.GetDefinition(ArmorCategory.Medium)).Returns(mediumDef);
        _mockCategoryProvider.Setup(p => p.GetDefinition(ArmorCategory.Heavy)).Returns(heavyDef);
        _mockCategoryProvider.Setup(p => p.GetDefinition(ArmorCategory.Shields)).Returns(shieldsDef);

        // Setup GetPenalties for each category
        _mockCategoryProvider.Setup(p => p.GetPenalties(ArmorCategory.Light)).Returns(LightPenalties);
        _mockCategoryProvider.Setup(p => p.GetPenalties(ArmorCategory.Medium)).Returns(MediumPenalties);
        _mockCategoryProvider.Setup(p => p.GetPenalties(ArmorCategory.Heavy)).Returns(HeavyPenalties);
        _mockCategoryProvider.Setup(p => p.GetPenalties(ArmorCategory.Shields)).Returns(ShieldPenalties);
    }

    /// <summary>
    /// Configures the mock effect provider with default proficiency effects.
    /// </summary>
    private void SetupDefaultEffectProvider()
    {
        // NonProficient: 2.0x penalty, -2 attack, 0 defense, no tier reduction
        var nonProficientEffect = ArmorProficiencyEffect.CreateNonProficient();

        // Proficient: 1.0x penalty, 0 attack, 0 defense, no tier reduction
        var proficientEffect = ArmorProficiencyEffect.CreateProficient();

        // Expert: 1.0x penalty, 0 attack, 0 defense, 1 tier reduction
        var expertEffect = ArmorProficiencyEffect.CreateExpert();

        // Master: 1.0x penalty, 0 attack, +1 defense, 1 tier reduction
        var masterEffect = ArmorProficiencyEffect.CreateMaster();

        // Setup GetEffect for each level
        _mockEffectProvider
            .Setup(p => p.GetEffect(ArmorProficiencyLevel.NonProficient))
            .Returns(nonProficientEffect);
        _mockEffectProvider
            .Setup(p => p.GetEffect(ArmorProficiencyLevel.Proficient))
            .Returns(proficientEffect);
        _mockEffectProvider
            .Setup(p => p.GetEffect(ArmorProficiencyLevel.Expert))
            .Returns(expertEffect);
        _mockEffectProvider
            .Setup(p => p.GetEffect(ArmorProficiencyLevel.Master))
            .Returns(masterEffect);

        // Setup GetTierReduction for each level
        _mockEffectProvider
            .Setup(p => p.GetTierReduction(ArmorProficiencyLevel.NonProficient))
            .Returns(0);
        _mockEffectProvider
            .Setup(p => p.GetTierReduction(ArmorProficiencyLevel.Proficient))
            .Returns(0);
        _mockEffectProvider
            .Setup(p => p.GetTierReduction(ArmorProficiencyLevel.Expert))
            .Returns(1);
        _mockEffectProvider
            .Setup(p => p.GetTierReduction(ArmorProficiencyLevel.Master))
            .Returns(1);

        // Setup GetPenaltyMultiplier for each level
        _mockEffectProvider
            .Setup(p => p.GetPenaltyMultiplier(ArmorProficiencyLevel.NonProficient))
            .Returns(2.0m);
        _mockEffectProvider
            .Setup(p => p.GetPenaltyMultiplier(ArmorProficiencyLevel.Proficient))
            .Returns(1.0m);
        _mockEffectProvider
            .Setup(p => p.GetPenaltyMultiplier(ArmorProficiencyLevel.Expert))
            .Returns(1.0m);
        _mockEffectProvider
            .Setup(p => p.GetPenaltyMultiplier(ArmorProficiencyLevel.Master))
            .Returns(1.0m);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NonProficient Penalty Doubling Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that NonProficient with Heavy armor doubles all penalties.
    /// </summary>
    /// <remarks>
    /// Heavy penalties: -2d10 Agi, +5 Stam, -10ft Move
    /// Doubled: -4d10 Agi, +10 Stam, -20ft Move
    /// Also: Attack -2
    /// </remarks>
    [Test]
    public void CalculatePenalties_NonProficientInHeavy_DoublesPenalties()
    {
        // Arrange
        var category = ArmorCategory.Heavy;
        var level = ArmorProficiencyLevel.NonProficient;

        // Act
        var result = _calculator.CalculatePenalties(category, level);

        // Assert - Penalties are doubled
        result.EffectivePenalties.AgilityDicePenalty.Should().Be(-4);  // -2 * 2 = -4
        result.EffectivePenalties.StaminaCostModifier.Should().Be(10); // 5 * 2 = 10
        result.EffectivePenalties.MovementPenalty.Should().Be(-20);    // -10 * 2 = -20
        result.EffectivePenalties.HasStealthDisadvantage.Should().BeTrue();

        // Assert - Attack modifier applied
        result.AttackModifier.Should().Be(-2);

        // Assert - Metadata
        result.WasMultiplied.Should().BeTrue();
        result.WasTierReduced.Should().BeFalse();
        result.ProficiencyLevel.Should().Be(ArmorProficiencyLevel.NonProficient);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Expert Tier Reduction Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Expert proficiency with Heavy armor uses Medium penalties.
    /// </summary>
    /// <remarks>
    /// Heavy (Tier 2) → Reduced to Medium (Tier 1) penalties.
    /// Medium penalties: -1d10 Agi, +2 Stam, -5ft Move
    /// </remarks>
    [Test]
    public void CalculatePenalties_ExpertInHeavy_UsesMediumPenalties()
    {
        // Arrange
        var category = ArmorCategory.Heavy;
        var level = ArmorProficiencyLevel.Expert;

        // Act
        var result = _calculator.CalculatePenalties(category, level);

        // Assert - Uses Medium tier penalties (reduced from Heavy)
        result.EffectivePenalties.AgilityDicePenalty.Should().Be(-1);
        result.EffectivePenalties.StaminaCostModifier.Should().Be(2);
        result.EffectivePenalties.MovementPenalty.Should().Be(-5);

        // Assert - Tier reduction metadata
        result.OriginalTier.Should().Be(2);  // Heavy
        result.EffectiveTier.Should().Be(1); // Medium
        result.WasTierReduced.Should().BeTrue();
        result.WasMultiplied.Should().BeFalse();

        // Assert - No modifiers at Expert level
        result.AttackModifier.Should().Be(0);
        result.DefenseModifier.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Master Tier Reduction + Defense Bonus Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Master proficiency provides tier reduction and +1 Defense.
    /// </summary>
    /// <remarks>
    /// Master gets Expert benefits plus +1 Defense bonus.
    /// Heavy → Medium penalties, Defense +1.
    /// </remarks>
    [Test]
    public void CalculatePenalties_MasterInHeavy_ReducesTierAndGrantsDefense()
    {
        // Arrange
        var category = ArmorCategory.Heavy;
        var level = ArmorProficiencyLevel.Master;

        // Act
        var result = _calculator.CalculatePenalties(category, level);

        // Assert - Same tier reduction as Expert
        result.EffectivePenalties.AgilityDicePenalty.Should().Be(-1); // Medium tier
        result.WasTierReduced.Should().BeTrue();

        // Assert - Master-specific defense bonus
        result.DefenseModifier.Should().Be(1);
        result.HasDefenseBonus.Should().BeTrue();

        // Assert - No attack penalty
        result.AttackModifier.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Archetype-Based Calculation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CalculateForArchetype uses the archetype's proficiency level.
    /// </summary>
    /// <remarks>
    /// Mystic is NonProficient with Medium armor (per design spec).
    /// Should result in doubled Medium penalties.
    /// </remarks>
    [Test]
    public void CalculateForArchetype_MysticWithMedium_ReturnsNonProficientPenalties()
    {
        // Arrange
        string archetypeId = "mystic";
        var category = ArmorCategory.Medium;

        // Setup archetype provider to return NonProficient for Mystic + Medium
        _mockArchetypeProvider
            .Setup(p => p.GetStartingProficiency(archetypeId, category))
            .Returns(ArmorProficiencyLevel.NonProficient);

        // Act
        var result = _calculator.CalculateForArchetype(archetypeId, category);

        // Assert - NonProficient penalties for Medium armor (doubled)
        result.ProficiencyLevel.Should().Be(ArmorProficiencyLevel.NonProficient);
        result.WasMultiplied.Should().BeTrue();
        result.AttackModifier.Should().Be(-2);

        // Doubled Medium penalties: -1 → -2, +2 → +4, -5 → -10
        result.EffectivePenalties.AgilityDicePenalty.Should().Be(-2);
        result.EffectivePenalties.StaminaCostModifier.Should().Be(4);
        result.EffectivePenalties.MovementPenalty.Should().Be(-10);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that WouldDoublePenalties returns true only for NonProficient.
    /// </summary>
    [Test]
    [TestCase(ArmorProficiencyLevel.NonProficient, ExpectedResult = true)]
    [TestCase(ArmorProficiencyLevel.Proficient, ExpectedResult = false)]
    [TestCase(ArmorProficiencyLevel.Expert, ExpectedResult = false)]
    [TestCase(ArmorProficiencyLevel.Master, ExpectedResult = false)]
    public bool WouldDoublePenalties_ReturnsCorrectValue(ArmorProficiencyLevel level)
    {
        // Act
        return _calculator.WouldDoublePenalties(ArmorCategory.Heavy, level);
    }

    /// <summary>
    /// Verifies that WouldReduceTier returns true for Expert/Master with Heavy.
    /// </summary>
    [Test]
    [TestCase(ArmorCategory.Heavy, ArmorProficiencyLevel.Expert, ExpectedResult = true)]
    [TestCase(ArmorCategory.Heavy, ArmorProficiencyLevel.Master, ExpectedResult = true)]
    [TestCase(ArmorCategory.Heavy, ArmorProficiencyLevel.Proficient, ExpectedResult = false)]
    [TestCase(ArmorCategory.Light, ArmorProficiencyLevel.Expert, ExpectedResult = false)] // Already minimum tier
    public bool WouldReduceTier_ReturnsCorrectValue(ArmorCategory category, ArmorProficiencyLevel level)
    {
        // Act
        return _calculator.WouldReduceTier(category, level);
    }
}
