using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Builders;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for StanceService.
/// </summary>
/// <remarks>
/// <para>Tests cover all stance operations:</para>
/// <list type="bullet">
///   <item><description>Stance query operations (GetCurrentStance, GetStanceDefinition)</description></item>
///   <item><description>Stance change operations with once-per-round limit</description></item>
///   <item><description>Stance modifier retrieval (attack, damage, defense, save bonuses)</description></item>
///   <item><description>Round reset operations</description></item>
///   <item><description>Stance initialization for new combatants</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class StanceServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<IStanceProvider> _mockStanceProvider = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<StanceService>> _mockLogger = null!;
    private StanceService _service = null!;

    // Test stance definitions
    private StanceDefinition _balancedStance = null!;
    private StanceDefinition _aggressiveStance = null!;
    private StanceDefinition _defensiveStance = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockStanceProvider = new Mock<IStanceProvider>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<StanceService>>();

        // Create test stance definitions
        _balancedStance = StanceDefinition.Create(
            "balanced",
            "Balanced Stance",
            "Default stance with no modifiers.",
            attackBonus: 0,
            damageBonus: null,
            defenseBonus: 0,
            saveBonus: 0,
            isDefault: true,
            iconPath: "icons/stances/balanced.png");

        _aggressiveStance = StanceDefinition.Create(
            "aggressive",
            "Aggressive Stance",
            "Offensive stance.",
            attackBonus: 2,
            damageBonus: "1d4",
            defenseBonus: -2,
            saveBonus: -1,
            isDefault: false,
            iconPath: "icons/stances/aggressive.png");

        _defensiveStance = StanceDefinition.Create(
            "defensive",
            "Defensive Stance",
            "Protective stance.",
            attackBonus: -2,
            damageBonus: "-1d4",
            defenseBonus: 2,
            saveBonus: 2,
            isDefault: false,
            iconPath: "icons/stances/defensive.png");

        // Setup default provider behavior
        SetupDefaultStanceProvider();

        _service = new StanceService(
            _mockStanceProvider.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // STANCE QUERY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetCurrentStance returns the combatant's current stance.
    /// </summary>
    [Test]
    public void GetCurrentStance_ReturnsCombatantStance()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();

        // Act
        var result = _service.GetCurrentStance(combatant);

        // Assert
        result.Should().Be(CombatStance.Balanced);
    }

    /// <summary>
    /// Verifies that GetStanceDefinition returns the definition for the combatant's current stance.
    /// </summary>
    [Test]
    public void GetStanceDefinition_ReturnsDefinitionForCurrentStance()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();

        // Act
        var result = _service.GetStanceDefinition(combatant);

        // Assert
        result.Should().NotBeNull();
        result!.StanceId.Should().Be("balanced");
        result.IsDefault.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // STANCE CHANGE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that SetStance successfully changes to a new stance.
    /// </summary>
    [Test]
    public void SetStance_ChangesStanceSuccessfully()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();

        // Act
        var result = _service.SetStance(combatant, CombatStance.Aggressive);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.OldStance.Should().Be(CombatStance.Balanced);
        result.NewStance.Should().Be(CombatStance.Aggressive);
        combatant.CurrentStance.Should().Be(CombatStance.Aggressive);
    }

    /// <summary>
    /// Verifies that SetStance returns AlreadyInStance when requesting the current stance.
    /// </summary>
    [Test]
    public void SetStance_ReturnsAlreadyInStance_WhenSameStance()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        // Combatant starts in Balanced stance

        // Act
        var result = _service.SetStance(combatant, CombatStance.Balanced);

        // Assert
        // AlreadyInStance returns IsSuccess=true but StanceChanged=false
        result.IsSuccess.Should().BeTrue();
        result.StanceChanged.Should().BeFalse();
        result.OldStance.Should().Be(result.NewStance);
    }

    /// <summary>
    /// Verifies that SetStance fails when the combatant has already changed stance this round.
    /// </summary>
    [Test]
    public void SetStance_Fails_WhenChangedThisRound()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();

        // First stance change - should succeed
        var firstResult = _service.SetStance(combatant, CombatStance.Aggressive);
        firstResult.IsSuccess.Should().BeTrue();

        // Act - Second stance change in same round
        var secondResult = _service.SetStance(combatant, CombatStance.Defensive);

        // Assert
        secondResult.IsSuccess.Should().BeFalse();
        secondResult.StanceChanged.Should().BeFalse();
        secondResult.FailureReason.Should().Contain("Already changed stance this round");
    }

    /// <summary>
    /// Verifies that SetStance fails when requesting an unknown stance.
    /// </summary>
    [Test]
    public void SetStance_Fails_WhenStanceNotConfigured()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();

        // Setup provider to return null for aggressive stance
        _mockStanceProvider
            .Setup(p => p.GetStance(CombatStance.Aggressive))
            .Returns((StanceDefinition?)null);

        // Act
        var result = _service.SetStance(combatant, CombatStance.Aggressive);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Contain("Unknown stance");
    }

    // ═══════════════════════════════════════════════════════════════
    // CAN CHANGE STANCE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CanChangeStance returns true initially.
    /// </summary>
    [Test]
    public void CanChangeStance_ReturnsTrue_Initially()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();

        // Act
        var result = _service.CanChangeStance(combatant);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CanChangeStance returns false after a stance change.
    /// </summary>
    [Test]
    public void CanChangeStance_ReturnsFalse_AfterChange()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        _service.SetStance(combatant, CombatStance.Aggressive);

        // Act
        var result = _service.CanChangeStance(combatant);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // STANCE MODIFIER TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAttackBonus returns the correct value for each stance.
    /// </summary>
    [Test]
    [TestCase(CombatStance.Balanced, 0)]
    [TestCase(CombatStance.Aggressive, 2)]
    [TestCase(CombatStance.Defensive, -2)]
    public void GetAttackBonus_ReturnsCorrectValue_ForEachStance(CombatStance stance, int expectedBonus)
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        if (stance != CombatStance.Balanced)
        {
            combatant.SetStance(stance);
        }

        // Act
        var result = _service.GetAttackBonus(combatant);

        // Assert
        result.Should().Be(expectedBonus);
    }

    /// <summary>
    /// Verifies that GetDamageBonus returns the correct dice notation for each stance.
    /// </summary>
    [Test]
    [TestCase(CombatStance.Balanced, null)]
    [TestCase(CombatStance.Aggressive, "1d4")]
    [TestCase(CombatStance.Defensive, "-1d4")]
    public void GetDamageBonus_ReturnsDiceNotation_ForEachStance(CombatStance stance, string? expectedDice)
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        if (stance != CombatStance.Balanced)
        {
            combatant.SetStance(stance);
        }

        // Act
        var result = _service.GetDamageBonus(combatant);

        // Assert
        result.Should().Be(expectedDice);
    }

    /// <summary>
    /// Verifies that GetDefenseBonus returns the correct value for each stance.
    /// </summary>
    [Test]
    [TestCase(CombatStance.Balanced, 0)]
    [TestCase(CombatStance.Aggressive, -2)]
    [TestCase(CombatStance.Defensive, 2)]
    public void GetDefenseBonus_ReturnsCorrectValue_ForEachStance(CombatStance stance, int expectedBonus)
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        if (stance != CombatStance.Balanced)
        {
            combatant.SetStance(stance);
        }

        // Act
        var result = _service.GetDefenseBonus(combatant);

        // Assert
        result.Should().Be(expectedBonus);
    }

    /// <summary>
    /// Verifies that GetSaveBonus returns the correct value for each stance.
    /// </summary>
    [Test]
    [TestCase(CombatStance.Balanced, 0)]
    [TestCase(CombatStance.Aggressive, -1)]
    [TestCase(CombatStance.Defensive, 2)]
    public void GetSaveBonus_ReturnsCorrectValue_ForEachStance(CombatStance stance, int expectedBonus)
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        if (stance != CombatStance.Balanced)
        {
            combatant.SetStance(stance);
        }

        // Act
        var result = _service.GetSaveBonus(combatant);

        // Assert
        result.Should().Be(expectedBonus);
    }

    // ═══════════════════════════════════════════════════════════════
    // RESET TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ResetStanceChange allows a new stance change.
    /// </summary>
    [Test]
    public void ResetStanceChange_AllowsNewChange()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        _service.SetStance(combatant, CombatStance.Aggressive);
        _service.CanChangeStance(combatant).Should().BeFalse();

        // Act
        _service.ResetStanceChange(combatant);

        // Assert
        _service.CanChangeStance(combatant).Should().BeTrue();

        // Should be able to change stance again
        var result = _service.SetStance(combatant, CombatStance.Defensive);
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ResetAllStanceChanges clears all tracking.
    /// </summary>
    [Test]
    public void ResetAllStanceChanges_ClearsAllTracking()
    {
        // Arrange
        var combatant1 = CreatePlayerCombatant("Player1");
        var combatant2 = CreateMonsterCombatant();

        _service.SetStance(combatant1, CombatStance.Aggressive);
        _service.SetStance(combatant2, CombatStance.Defensive);

        _service.CanChangeStance(combatant1).Should().BeFalse();
        _service.CanChangeStance(combatant2).Should().BeFalse();

        // Act
        _service.ResetAllStanceChanges();

        // Assert
        _service.CanChangeStance(combatant1).Should().BeTrue();
        _service.CanChangeStance(combatant2).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // INITIALIZATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that InitializeStance sets the default stance on a combatant.
    /// </summary>
    [Test]
    public void InitializeStance_SetsDefaultStance()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        // Manually set to aggressive to verify initialization resets it
        combatant.SetStance(CombatStance.Aggressive);

        // Act
        _service.InitializeStance(combatant);

        // Assert
        combatant.CurrentStance.Should().Be(CombatStance.Balanced);
    }

    // ═══════════════════════════════════════════════════════════════
    // EVENT LOGGING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that SetStance logs a combat event on successful change.
    /// </summary>
    [Test]
    public void SetStance_LogsCombatEvent_OnSuccessfulChange()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();

        // Act
        _service.SetStance(combatant, CombatStance.Aggressive);

        // Assert
        _mockEventLogger.Verify(
            e => e.LogCombat(
                "StanceChanged",
                It.Is<string>(s => s.Contains("Balanced") && s.Contains("Aggressive")),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that ResetStanceChange logs a combat event.
    /// </summary>
    [Test]
    public void ResetStanceChange_LogsCombatEvent()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        _service.SetStance(combatant, CombatStance.Aggressive);

        // Act
        _service.ResetStanceChange(combatant);

        // Assert
        _mockEventLogger.Verify(
            e => e.LogCombat(
                "StanceChangeReset",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAvailableStances returns all configured stances.
    /// </summary>
    [Test]
    public void GetAvailableStances_ReturnsAllStances()
    {
        // Arrange
        var allStances = new List<StanceDefinition> { _balancedStance, _aggressiveStance, _defensiveStance };
        _mockStanceProvider.Setup(p => p.GetAllStances()).Returns(allStances);

        // Act
        var result = _service.GetAvailableStances();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(s => s.StanceId == "balanced");
        result.Should().Contain(s => s.StanceId == "aggressive");
        result.Should().Contain(s => s.StanceId == "defensive");
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets up the default stance provider mock behavior.
    /// </summary>
    private void SetupDefaultStanceProvider()
    {
        _mockStanceProvider
            .Setup(p => p.GetStance(CombatStance.Balanced))
            .Returns(_balancedStance);

        _mockStanceProvider
            .Setup(p => p.GetStance(CombatStance.Aggressive))
            .Returns(_aggressiveStance);

        _mockStanceProvider
            .Setup(p => p.GetStance(CombatStance.Defensive))
            .Returns(_defensiveStance);

        _mockStanceProvider
            .Setup(p => p.GetDefaultStance())
            .Returns(_balancedStance);

        _mockStanceProvider
            .Setup(p => p.Count)
            .Returns(3);
    }

    /// <summary>
    /// Creates a player combatant for testing.
    /// </summary>
    /// <param name="name">Optional player name.</param>
    /// <returns>A Combatant wrapping a test player.</returns>
    private static Combatant CreatePlayerCombatant(string name = "TestPlayer")
    {
        var player = PlayerBuilder.Create()
            .WithName(name)
            .WithAttributes(10, 10, 10, 10, 10)
            .Build();
        return Combatant.ForPlayer(player, CreateInitiativeRoll());
    }

    /// <summary>
    /// Creates a monster combatant for testing.
    /// </summary>
    /// <returns>A Combatant wrapping a test monster.</returns>
    private static Combatant CreateMonsterCombatant()
    {
        var monster = MonsterBuilder.Goblin().Build();
        return Combatant.ForMonster(monster, CreateInitiativeRoll(), displayNumber: 0);
    }

    /// <summary>
    /// Creates a valid InitiativeRoll for testing.
    /// </summary>
    /// <param name="rollValue">The dice roll value.</param>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>An InitiativeRoll instance.</returns>
    private static InitiativeRoll CreateInitiativeRoll(int rollValue = 10, int modifier = 0)
    {
        var diceResult = new DiceRollResult
        {
            Pool = DicePool.Parse("1d10"),
            Rolls = new[] { rollValue },
            ExplosionRolls = Array.Empty<int>(),
            DiceTotal = rollValue,
            Total = rollValue
        };
        return new InitiativeRoll(diceResult, modifier);
    }
}
