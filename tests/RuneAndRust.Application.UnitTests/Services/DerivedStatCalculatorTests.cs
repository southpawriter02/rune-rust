// ═══════════════════════════════════════════════════════════════════════════════
// DerivedStatCalculatorTests.cs
// Unit tests for DerivedStatCalculator (v0.17.2g).
// Tests cover full stat calculation, single stat lookup, preview generation,
// lineage bonuses, lineage multipliers, and unknown stat handling.
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="DerivedStatCalculator"/> (v0.17.2g).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Warrior HP calculation (139 with standard build)</description></item>
///   <item><description>Warrior Stamina calculation (60 with standard build)</description></item>
///   <item><description>Mystic Aether Pool calculation (80 with standard build)</description></item>
///   <item><description>Clan-Born lineage HP bonus (+5, total 144)</description></item>
///   <item><description>Rune-Marked lineage Aether Pool multiplier (×1.10, total 93)</description></item>
///   <item><description>Iron-Blooded lineage Soak bonus (+2, total 4)</description></item>
///   <item><description>Vargr-Kin lineage Movement Speed bonus (+1, total 6)</description></item>
///   <item><description>Preview generation from AttributeAllocationState</description></item>
///   <item><description>Single stat calculation by name</description></item>
///   <item><description>Unknown stat name returns zero</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class DerivedStatCalculatorTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Mock for the logger dependency.</summary>
    private Mock<ILogger<DerivedStatCalculator>> _loggerMock = null!;

    /// <summary>The calculator under test, initialized with default formulas.</summary>
    private DerivedStatCalculator _calculator = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// Creates a calculator with default formulas using a mock logger.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<DerivedStatCalculator>>();
        _calculator = DerivedStatCalculator.CreateWithDefaultFormulas(_loggerMock.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CalculateDerivedStats TESTS - WARRIOR BUILD
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a Warrior build with standard attributes produces
    /// the correct Max HP: (4 × 10) + 50 + 49 = 139.
    /// </summary>
    [Test]
    public void CalculateDerivedStats_WarriorBuild_ReturnsCorrectHp()
    {
        // Arrange
        var attributes = CreateWarriorAttributes();

        // Act
        var stats = _calculator.CalculateDerivedStats(attributes, "warrior", null);

        // Assert — (STURDINESS 4 × 10) + 50 base + 49 warrior bonus = 139
        stats.MaxHp.Should().Be(139);
    }

    /// <summary>
    /// Verifies that a Warrior build with standard attributes produces
    /// the correct Max Stamina: (3 × 5) + (4 × 5) + 20 + 5 = 60.
    /// </summary>
    [Test]
    public void CalculateDerivedStats_WarriorBuild_ReturnsCorrectStamina()
    {
        // Arrange
        var attributes = CreateWarriorAttributes();

        // Act
        var stats = _calculator.CalculateDerivedStats(attributes, "warrior", null);

        // Assert — (FINESSE 3 × 5) + (MIGHT 4 × 5) + 20 base + 5 warrior bonus = 60
        stats.MaxStamina.Should().Be(60);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CalculateDerivedStats TESTS - MYSTIC BUILD
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a Mystic build with standard attributes produces
    /// the correct Max Aether Pool: (4 × 10) + (4 × 5) + 20 = 80.
    /// </summary>
    [Test]
    public void CalculateDerivedStats_MysticBuild_ReturnsCorrectAetherPool()
    {
        // Arrange
        var attributes = CreateMysticAttributes();

        // Act
        var stats = _calculator.CalculateDerivedStats(attributes, "mystic", null);

        // Assert — (WILL 4 × 10) + (WITS 4 × 5) + 20 mystic bonus = 80
        stats.MaxAetherPool.Should().Be(80);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CalculateDerivedStats TESTS - LINEAGE BONUSES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Clan-Born lineage adds +5 HP bonus to the Warrior's
    /// base calculation: 139 + 5 = 144.
    /// </summary>
    [Test]
    public void CalculateDerivedStats_WithClanBornLineage_AddsHpBonus()
    {
        // Arrange
        var attributes = CreateWarriorAttributes();

        // Act
        var stats = _calculator.CalculateDerivedStats(attributes, "warrior", "clan-born");

        // Assert — Warrior base HP 139 + Clan-Born lineage bonus 5 = 144
        stats.MaxHp.Should().Be(144);
    }

    /// <summary>
    /// Verifies that the Rune-Marked lineage applies both the +5 flat bonus
    /// and the ×1.10 multiplier to Max Aether Pool:
    /// ((4 × 10) + (4 × 5) + 20 + 5) × 1.10 = 85 × 1.10 = 93.5 → 93 (truncated).
    /// </summary>
    [Test]
    public void CalculateDerivedStats_RuneMarkedMystic_AppliesMultiplier()
    {
        // Arrange
        var attributes = CreateMysticAttributes();

        // Act
        var stats = _calculator.CalculateDerivedStats(attributes, "mystic", "rune-marked");

        // Assert — (WILL 4×10 + WITS 4×5 + Mystic bonus 20 + Rune-Marked bonus 5)
        //          = 85, then × 1.10 = 93.5, truncated to 93
        stats.MaxAetherPool.Should().Be(93);
    }

    /// <summary>
    /// Verifies that the Iron-Blooded lineage adds +2 Soak bonus:
    /// (STURDINESS 4 × 0.5) + 2 = 2 + 2 = 4.
    /// </summary>
    [Test]
    public void CalculateDerivedStats_IronBlooded_AddsSoakBonus()
    {
        // Arrange
        var attributes = CreateWarriorAttributes();

        // Act
        var stats = _calculator.CalculateDerivedStats(attributes, "warrior", "iron-blooded");

        // Assert — (STURDINESS 4 × 0.5) = 2 + Iron-Blooded bonus 2 = 4
        stats.Soak.Should().Be(4);
    }

    /// <summary>
    /// Verifies that the Vargr-Kin lineage adds +1 Movement Speed:
    /// base 5 + 1 = 6.
    /// </summary>
    [Test]
    public void CalculateDerivedStats_VargrKin_AddsMovementSpeed()
    {
        // Arrange
        var attributes = CreateWarriorAttributes();

        // Act
        var stats = _calculator.CalculateDerivedStats(attributes, "warrior", "vargr-kin");

        // Assert — Base movement speed 5 + Vargr-Kin bonus 1 = 6
        stats.MovementSpeed.Should().Be(6);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetPreview TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetPreview extracts attributes from an AllocationState
    /// and produces the same results as CalculateDerivedStats.
    /// Uses the Warrior recommended build (M4/F3/Wi2/Wl2/S4).
    /// </summary>
    [Test]
    public void GetPreview_FromAllocationState_CalculatesCorrectly()
    {
        // Arrange — Create a Warrior recommended build state
        var state = AttributeAllocationState.CreateFromRecommendedBuild(
            "warrior", 4, 3, 2, 2, 4, 15);

        // Act
        var preview = _calculator.GetPreview(state, "warrior", null);

        // Assert — Should match CalculateDerivedStats with Warrior attributes
        preview.MaxHp.Should().Be(139);
        preview.MaxStamina.Should().Be(60);
        preview.MaxAetherPool.Should().Be(30);
        preview.Initiative.Should().Be(4);
        preview.Soak.Should().Be(2);
        preview.MovementSpeed.Should().Be(5);
        preview.CarryingCapacity.Should().Be(40);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CalculateStat TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CalculateStat returns the correct value for a single
    /// stat lookup. Uses "MaxHp" (case-insensitive) with Warrior attributes.
    /// </summary>
    [Test]
    public void CalculateStat_SingleStat_ReturnsCorrectValue()
    {
        // Arrange
        var attributes = CreateWarriorAttributes();

        // Act — "MaxHp" should normalize to "maxhp" and match the formula key
        var hp = _calculator.CalculateStat("MaxHp", attributes, "warrior", null);

        // Assert — Same as full calculation: 139
        hp.Should().Be(139);
    }

    /// <summary>
    /// Verifies that CalculateStat returns 0 for an unknown stat name.
    /// </summary>
    [Test]
    public void CalculateStat_UnknownStat_ReturnsZero()
    {
        // Arrange
        var attributes = CreateWarriorAttributes();

        // Act
        var result = _calculator.CalculateStat("UnknownStat", attributes, "warrior", null);

        // Assert — Unknown stat names should return 0
        result.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates the standard Warrior attribute set: M4/F3/Wi2/Wl2/S4.
    /// Total point cost: 15 points (all standard tier).
    /// </summary>
    /// <returns>
    /// A dictionary mapping each <see cref="CoreAttribute"/> to the Warrior
    /// recommended build values.
    /// </returns>
    private static Dictionary<CoreAttribute, int> CreateWarriorAttributes() =>
        new()
        {
            { CoreAttribute.Might, 4 },
            { CoreAttribute.Finesse, 3 },
            { CoreAttribute.Wits, 2 },
            { CoreAttribute.Will, 2 },
            { CoreAttribute.Sturdiness, 4 }
        };

    /// <summary>
    /// Creates the standard Mystic attribute set: M2/F3/Wi4/Wl4/S2.
    /// Total point cost: 15 points (all standard tier).
    /// </summary>
    /// <returns>
    /// A dictionary mapping each <see cref="CoreAttribute"/> to the Mystic
    /// recommended build values.
    /// </returns>
    private static Dictionary<CoreAttribute, int> CreateMysticAttributes() =>
        new()
        {
            { CoreAttribute.Might, 2 },
            { CoreAttribute.Finesse, 3 },
            { CoreAttribute.Wits, 4 },
            { CoreAttribute.Will, 4 },
            { CoreAttribute.Sturdiness, 2 }
        };
}
