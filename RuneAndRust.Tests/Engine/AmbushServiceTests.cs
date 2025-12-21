using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the AmbushService class.
/// Validates ambush risk calculation, mitigation mechanics, and encounter generation.
/// </summary>
public class AmbushServiceTests
{
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<ILogger<AmbushService>> _mockLogger;
    private readonly AmbushService _sut;

    public AmbushServiceTests()
    {
        _mockDiceService = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<AmbushService>>();
        _sut = new AmbushService(_mockDiceService.Object, _mockLogger.Object);
    }

    private Character CreateTestCharacter(int wits = 5, int level = 1)
    {
        return new Character
        {
            Name = "Test Hero",
            Wits = wits,
            Level = level,
            CurrentHP = 100,
            MaxHP = 100
        };
    }

    private Room CreateTestRoom(DangerLevel dangerLevel = DangerLevel.Hostile, BiomeType biome = BiomeType.Ruin)
    {
        return new Room
        {
            Name = "Test Room",
            DangerLevel = dangerLevel,
            BiomeType = biome
        };
    }

    #region GetBaseRisk Tests

    [Fact]
    public void GetBaseRisk_SafeZone_ReturnsZero()
    {
        // Act
        var result = _sut.GetBaseRisk(DangerLevel.Safe);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetBaseRisk_UnstableZone_ReturnsFifteen()
    {
        // Act
        var result = _sut.GetBaseRisk(DangerLevel.Unstable);

        // Assert
        result.Should().Be(15);
    }

    [Fact]
    public void GetBaseRisk_HostileZone_ReturnsThirty()
    {
        // Act
        var result = _sut.GetBaseRisk(DangerLevel.Hostile);

        // Assert
        result.Should().Be(30);
    }

    [Fact]
    public void GetBaseRisk_LethalZone_ReturnsFifty()
    {
        // Act
        var result = _sut.GetBaseRisk(DangerLevel.Lethal);

        // Assert
        result.Should().Be(50);
    }

    #endregion

    #region Safe Zone Tests

    [Fact]
    public async Task CalculateAmbush_SafeZone_AlwaysReturnsSafe()
    {
        // Arrange
        var character = CreateTestCharacter();
        var room = CreateTestRoom(DangerLevel.Safe);

        // Act
        var result = await _sut.CalculateAmbushAsync(character, room);

        // Assert
        result.IsAmbush.Should().BeFalse();
        result.BaseRiskPercent.Should().Be(0);
        result.FinalRiskPercent.Should().Be(0);
        result.Message.Should().Contain("secure");
    }

    [Fact]
    public async Task CalculateAmbush_SafeZone_NoRollPerformed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var room = CreateTestRoom(DangerLevel.Safe);

        // Act
        await _sut.CalculateAmbushAsync(character, room);

        // Assert - No dice rolls should be made
        _mockDiceService.Verify(x => x.Roll(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _mockDiceService.Verify(x => x.RollSingle(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Mitigation Tests

    [Fact]
    public async Task CalculateAmbush_HighWits_ReducesRisk()
    {
        // Arrange - Wits 8 should roll 8 dice
        var character = CreateTestCharacter(wits: 8);
        var room = CreateTestRoom(DangerLevel.Hostile); // 30% base

        // Mock 4 successes = 20% mitigation
        _mockDiceService
            .Setup(x => x.Roll(8, It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 4, Botches: 0, Rolls: new List<int> { 8, 8, 8, 8, 3, 2, 1, 5 }));

        // Mock roll above the reduced risk
        _mockDiceService
            .Setup(x => x.RollSingle(100, It.IsAny<string>()))
            .Returns(15); // Above 10% (30-20)

        // Act
        var result = await _sut.CalculateAmbushAsync(character, room);

        // Assert
        result.MitigationPercent.Should().Be(20); // 4 successes * 5%
        result.FinalRiskPercent.Should().Be(10); // 30 - 20 = 10
        result.IsAmbush.Should().BeFalse();
    }

    [Fact]
    public async Task CalculateAmbush_SuccessfulMitigation_AppliesFivePercentPerSuccess()
    {
        // Arrange
        var character = CreateTestCharacter(wits: 5);
        var room = CreateTestRoom(DangerLevel.Hostile); // 30% base

        // Mock 3 successes = 15% mitigation
        _mockDiceService
            .Setup(x => x.Roll(5, It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 3, Botches: 0, Rolls: new List<int> { 9, 8, 10, 2, 4 }));

        _mockDiceService
            .Setup(x => x.RollSingle(100, It.IsAny<string>()))
            .Returns(20);

        // Act
        var result = await _sut.CalculateAmbushAsync(character, room);

        // Assert
        result.MitigationSuccesses.Should().Be(3);
        result.MitigationPercent.Should().Be(15); // 3 * 5 = 15
        result.FinalRiskPercent.Should().Be(15); // 30 - 15 = 15
    }

    #endregion

    #region Minimum Risk Floor Tests

    [Fact]
    public async Task CalculateAmbush_ExcessiveMitigation_ClampsToFivePercent()
    {
        // Arrange - Very high wits for many successes
        var character = CreateTestCharacter(wits: 10);
        var room = CreateTestRoom(DangerLevel.Unstable); // 15% base

        // Mock 8 successes = 40% mitigation (more than base risk)
        _mockDiceService
            .Setup(x => x.Roll(10, It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 8, Botches: 0, Rolls: new List<int> { 8, 8, 8, 8, 8, 8, 8, 8, 2, 3 }));

        _mockDiceService
            .Setup(x => x.RollSingle(100, It.IsAny<string>()))
            .Returns(6); // Above the 5% floor

        // Act
        var result = await _sut.CalculateAmbushAsync(character, room);

        // Assert
        result.FinalRiskPercent.Should().Be(5); // Clamped to minimum
        result.IsAmbush.Should().BeFalse();
    }

    [Fact]
    public async Task CalculateAmbush_DangerousZone_NeverBelowFivePercent()
    {
        // Arrange
        var character = CreateTestCharacter(wits: 10);
        var room = CreateTestRoom(DangerLevel.Lethal); // 50% base

        // Mock 10 successes = 50% mitigation
        _mockDiceService
            .Setup(x => x.Roll(10, It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 10, Botches: 0, Rolls: new List<int> { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 }));

        _mockDiceService
            .Setup(x => x.RollSingle(100, It.IsAny<string>()))
            .Returns(4); // Below 5% floor - should trigger

        // Act
        var result = await _sut.CalculateAmbushAsync(character, room);

        // Assert
        result.FinalRiskPercent.Should().Be(5); // Clamped to minimum 5%
        result.IsAmbush.Should().BeTrue(); // 4 <= 5
    }

    #endregion

    #region Ambush Trigger Tests

    [Fact]
    public async Task CalculateAmbush_RollBelowRisk_TriggersAmbush()
    {
        // Arrange
        var character = CreateTestCharacter(wits: 5);
        var room = CreateTestRoom(DangerLevel.Hostile);

        // No successes
        _mockDiceService
            .Setup(x => x.Roll(5, It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 0, Botches: 0, Rolls: new List<int> { 1, 2, 3, 4, 5 }));

        // Roll under risk
        _mockDiceService
            .Setup(x => x.RollSingle(100, It.IsAny<string>()))
            .Returns(15); // Under 30%

        // Act
        var result = await _sut.CalculateAmbushAsync(character, room);

        // Assert
        result.IsAmbush.Should().BeTrue();
        result.RollValue.Should().Be(15);
        result.Encounter.Should().NotBeNull();
        result.Message.Should().Contain("not alone");
    }

    [Fact]
    public async Task CalculateAmbush_RollAboveRisk_ReturnsSafe()
    {
        // Arrange
        var character = CreateTestCharacter(wits: 5);
        var room = CreateTestRoom(DangerLevel.Hostile);

        // No successes
        _mockDiceService
            .Setup(x => x.Roll(5, It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 0, Botches: 0, Rolls: new List<int> { 1, 2, 3, 4, 5 }));

        // Roll above risk
        _mockDiceService
            .Setup(x => x.RollSingle(100, It.IsAny<string>()))
            .Returns(50); // Above 30%

        // Act
        var result = await _sut.CalculateAmbushAsync(character, room);

        // Assert
        result.IsAmbush.Should().BeFalse();
        result.RollValue.Should().Be(50);
        result.Encounter.Should().BeNull();
        result.Message.Should().Contain("vigilance");
    }

    [Fact]
    public async Task CalculateAmbush_RollEqualsRisk_TriggersAmbush()
    {
        // Arrange
        var character = CreateTestCharacter(wits: 5);
        var room = CreateTestRoom(DangerLevel.Hostile);

        // No successes
        _mockDiceService
            .Setup(x => x.Roll(5, It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 0, Botches: 0, Rolls: new List<int> { 1, 2, 3, 4, 5 }));

        // Roll exactly equals risk
        _mockDiceService
            .Setup(x => x.RollSingle(100, It.IsAny<string>()))
            .Returns(30); // Exactly 30%

        // Act
        var result = await _sut.CalculateAmbushAsync(character, room);

        // Assert
        result.IsAmbush.Should().BeTrue(); // <= comparison
    }

    #endregion

    #region Encounter Generation Tests

    [Fact]
    public void GenerateAmbushEncounter_UsesBudgetMultiplier()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        var encounter = _sut.GenerateAmbushEncounter(room, partyLevel: 1);

        // Assert
        // Base budget 100 * 0.8 = 80
        encounter.Budget.Should().Be(80f);
        encounter.IsAmbush.Should().BeTrue();
        encounter.EncounterType.Should().Be("Ambush");
    }

    [Fact]
    public void GenerateAmbushEncounter_AlwaysReturnsAtLeastOneEnemy()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        var encounter = _sut.GenerateAmbushEncounter(room, partyLevel: 1);

        // Assert
        encounter.TemplateIds.Should().NotBeEmpty();
        encounter.TemplateIds.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void GenerateAmbushEncounter_HigherPartyLevel_ScalesBudget()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        var encounter = _sut.GenerateAmbushEncounter(room, partyLevel: 3);

        // Assert
        // Base budget 100 * (1 + 0.2) * 0.8 = 120 * 0.8 = 96
        encounter.Budget.Should().BeApproximately(96f, 0.01f);
    }

    [Fact]
    public void GenerateAmbushEncounter_PrioritizesVargr()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        var encounter = _sut.GenerateAmbushEncounter(room, partyLevel: 1);

        // Assert - Ash-Vargr should be prioritized for ambushes
        encounter.TemplateIds.Should().Contain("bst_vargr_01");
    }

    #endregion
}
