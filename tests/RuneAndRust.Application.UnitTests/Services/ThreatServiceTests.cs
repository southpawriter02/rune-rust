using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Mocks;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="ThreatService"/> class.
/// </summary>
[TestFixture]
public class ThreatServiceTests
{
    private Mock<ICombatGridService> _gridServiceMock = null!;
    private MockConfigurationProvider _config = null!;
    private Mock<ILogger<ThreatService>> _loggerMock = null!;
    private CombatGrid _grid = null!;

    private static readonly Guid PlayerId = Guid.NewGuid();
    private static readonly Guid MonsterId = Guid.NewGuid();

    [SetUp]
    public void Setup()
    {
        _gridServiceMock = new Mock<ICombatGridService>();
        _config = new MockConfigurationProvider().WithDefaultOpportunityAttackConfiguration();
        _loggerMock = new Mock<ILogger<ThreatService>>();
        _grid = CombatGrid.Create(8, 8);

        _gridServiceMock.Setup(g => g.GetActiveGrid()).Returns(_grid);
    }

    // ===== GetThreatenedSquares Tests =====

    [Test]
    public void GetThreatenedSquares_EntityOnGrid_ReturnsAdjacentPositions()
    {
        // Arrange
        var position = new GridPosition(3, 3);
        _grid.PlaceEntity(PlayerId, position, isPlayer: true);

        var service = CreateService();

        // Act
        var threatened = service.GetThreatenedSquares(PlayerId).ToList();

        // Assert
        threatened.Should().HaveCount(8);
        threatened.Should().Contain(new GridPosition(2, 2)); // NW
        threatened.Should().Contain(new GridPosition(3, 2)); // N
        threatened.Should().Contain(new GridPosition(4, 2)); // NE
        threatened.Should().Contain(new GridPosition(2, 3)); // W
        threatened.Should().Contain(new GridPosition(4, 3)); // E
        threatened.Should().Contain(new GridPosition(2, 4)); // SW
        threatened.Should().Contain(new GridPosition(3, 4)); // S
        threatened.Should().Contain(new GridPosition(4, 4)); // SE
    }

    [Test]
    public void GetThreatenedSquares_EntityNotOnGrid_ReturnsEmpty()
    {
        // Arrange
        var service = CreateService();

        // Act
        var threatened = service.GetThreatenedSquares(PlayerId).ToList();

        // Assert
        threatened.Should().BeEmpty();
    }

    [Test]
    public void GetThreatenedSquares_DisabledConfig_ReturnsEmpty()
    {
        // Arrange
        _config.WithOpportunityAttackConfiguration(new OpportunityAttackConfiguration { Enabled = false });
        _grid.PlaceEntity(PlayerId, new GridPosition(3, 3), isPlayer: true);
        var service = CreateService();

        // Act
        var threatened = service.GetThreatenedSquares(PlayerId).ToList();

        // Assert
        threatened.Should().BeEmpty();
    }

    [Test]
    public void GetThreatenedSquares_NoActiveGrid_ReturnsEmpty()
    {
        // Arrange
        _gridServiceMock.Setup(g => g.GetActiveGrid()).Returns((CombatGrid?)null);
        var service = CreateService();

        // Act
        var threatened = service.GetThreatenedSquares(PlayerId).ToList();

        // Assert
        threatened.Should().BeEmpty();
    }

    // ===== GetThreateningEntities Tests =====

    [Test]
    public void GetThreateningEntities_MonsterAdjacent_ReturnsMonsterId()
    {
        // Arrange
        _grid.PlaceEntity(PlayerId, new GridPosition(3, 3), isPlayer: true);
        _grid.PlaceEntity(MonsterId, new GridPosition(4, 3), isPlayer: false); // Adjacent E
        var service = CreateService();

        // Act
        var threateners = service.GetThreateningEntities(new GridPosition(3, 3)).ToList();

        // Assert
        threateners.Should().Contain(MonsterId);
    }

    [Test]
    public void GetThreateningEntities_NoAdjacentEntities_ReturnsEmpty()
    {
        // Arrange
        _grid.PlaceEntity(MonsterId, new GridPosition(6, 6), isPlayer: false); // Far away
        var service = CreateService();

        // Act
        var threateners = service.GetThreateningEntities(new GridPosition(3, 3)).ToList();

        // Assert
        threateners.Should().BeEmpty();
    }

    [Test]
    public void GetThreateningEntities_MultipleAdjacent_ReturnsAll()
    {
        // Arrange
        var monster1 = Guid.NewGuid();
        var monster2 = Guid.NewGuid();
        _grid.PlaceEntity(monster1, new GridPosition(2, 3), isPlayer: false); // W
        _grid.PlaceEntity(monster2, new GridPosition(4, 3), isPlayer: false); // E
        var service = CreateService();

        // Act
        var threateners = service.GetThreateningEntities(new GridPosition(3, 3)).ToList();

        // Assert
        threateners.Should().HaveCount(2);
        threateners.Should().Contain(monster1);
        threateners.Should().Contain(monster2);
    }

    // ===== IsPositionThreatened Tests =====

    [Test]
    public void IsPositionThreatened_EnemyAdjacent_ReturnsTrue()
    {
        // Arrange
        _grid.PlaceEntity(MonsterId, new GridPosition(4, 3), isPlayer: false);
        var service = CreateService();
        service.RegisterPlayer(PlayerId);

        // Act
        var isThreatened = service.IsPositionThreatened(new GridPosition(3, 3), PlayerId);

        // Assert
        isThreatened.Should().BeTrue();
    }

    [Test]
    public void IsPositionThreatened_NoEnemyAdjacent_ReturnsFalse()
    {
        // Arrange
        _grid.PlaceEntity(MonsterId, new GridPosition(6, 6), isPlayer: false);
        var service = CreateService();
        service.RegisterPlayer(PlayerId);

        // Act
        var isThreatened = service.IsPositionThreatened(new GridPosition(3, 3), PlayerId);

        // Assert
        isThreatened.Should().BeFalse();
    }

    [Test]
    public void IsPositionThreatened_Disabled_ReturnsFalse()
    {
        // Arrange
        _config.WithOpportunityAttackConfiguration(new OpportunityAttackConfiguration { Enabled = false });
        _grid.PlaceEntity(MonsterId, new GridPosition(4, 3), isPlayer: false);
        var service = CreateService();

        // Act
        var isThreatened = service.IsPositionThreatened(new GridPosition(3, 3), PlayerId);

        // Assert
        isThreatened.Should().BeFalse();
    }

    // ===== CheckOpportunityAttacks Tests =====

    [Test]
    public void CheckOpportunityAttacks_LeavingThreatRange_ReturnsTriggered()
    {
        // Arrange
        var playerPos = new GridPosition(3, 3);
        var monsterPos = new GridPosition(4, 3);
        var destination = new GridPosition(1, 3); // Moving away

        _grid.PlaceEntity(PlayerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(MonsterId, monsterPos, isPlayer: false);

        var service = CreateService();
        service.RegisterPlayer(PlayerId);

        // Act
        var result = service.CheckOpportunityAttacks(PlayerId, playerPos, destination);

        // Assert
        result.TriggersOpportunityAttacks.Should().BeTrue();
        result.AttackingEntities.Should().Contain(MonsterId);
    }

    [Test]
    public void CheckOpportunityAttacks_StayingAdjacent_NoTrigger()
    {
        // Arrange
        var playerPos = new GridPosition(3, 3);
        var monsterPos = new GridPosition(4, 3);
        var destination = new GridPosition(4, 4); // Still adjacent to monster

        _grid.PlaceEntity(PlayerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(MonsterId, monsterPos, isPlayer: false);

        var service = CreateService();
        service.RegisterPlayer(PlayerId);

        // Act
        var result = service.CheckOpportunityAttacks(PlayerId, playerPos, destination);

        // Assert
        result.TriggersOpportunityAttacks.Should().BeFalse();
    }

    [Test]
    public void CheckOpportunityAttacks_Disengaging_NoTrigger()
    {
        // Arrange
        var playerPos = new GridPosition(3, 3);
        var monsterPos = new GridPosition(4, 3);
        var destination = new GridPosition(1, 3);

        _grid.PlaceEntity(PlayerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(MonsterId, monsterPos, isPlayer: false);

        var service = CreateService();
        service.RegisterPlayer(PlayerId);
        service.SetDisengaging(PlayerId);

        // Act
        var result = service.CheckOpportunityAttacks(PlayerId, playerPos, destination);

        // Assert
        result.TriggersOpportunityAttacks.Should().BeFalse();
        result.IsDisengaging.Should().BeTrue();
    }

    [Test]
    public void CheckOpportunityAttacks_ReactionAlreadyUsed_NoTrigger()
    {
        // Arrange
        var playerPos = new GridPosition(3, 3);
        var monsterPos = new GridPosition(4, 3);
        var destination = new GridPosition(1, 3);

        _grid.PlaceEntity(PlayerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(MonsterId, monsterPos, isPlayer: false);

        var service = CreateService();
        service.RegisterPlayer(PlayerId);
        service.UseReaction(MonsterId); // Monster already used reaction

        // Act
        var result = service.CheckOpportunityAttacks(PlayerId, playerPos, destination);

        // Assert
        result.TriggersOpportunityAttacks.Should().BeFalse();
    }

    [Test]
    public void CheckOpportunityAttacks_Disabled_NoTrigger()
    {
        // Arrange
        _config.WithOpportunityAttackConfiguration(new OpportunityAttackConfiguration { Enabled = false });
        var service = CreateService();

        // Act
        var result = service.CheckOpportunityAttacks(PlayerId, new GridPosition(3, 3), new GridPosition(1, 3));

        // Assert
        result.TriggersOpportunityAttacks.Should().BeFalse();
        result.Message.Should().Contain("disabled");
    }

    [Test]
    public void CheckOpportunityAttacks_NoThreateners_NoTrigger()
    {
        // Arrange
        _grid.PlaceEntity(PlayerId, new GridPosition(3, 3), isPlayer: true);
        var service = CreateService();

        // Act
        var result = service.CheckOpportunityAttacks(PlayerId, new GridPosition(3, 3), new GridPosition(1, 3));

        // Assert
        result.TriggersOpportunityAttacks.Should().BeFalse();
    }

    // ===== ExecuteOpportunityAttacks Tests =====

    [Test]
    public void ExecuteOpportunityAttacks_Triggered_ReturnsResults()
    {
        // Arrange
        var playerPos = new GridPosition(3, 3);
        var monsterPos = new GridPosition(4, 3);
        var destination = new GridPosition(1, 3);

        _grid.PlaceEntity(PlayerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(MonsterId, monsterPos, isPlayer: false);

        var service = CreateService();
        service.RegisterPlayer(PlayerId);

        // Act
        var results = service.ExecuteOpportunityAttacks(PlayerId, playerPos, destination).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].AttackerId.Should().Be(MonsterId);
    }

    [Test]
    public void ExecuteOpportunityAttacks_UsesReaction()
    {
        // Arrange
        var playerPos = new GridPosition(3, 3);
        var monsterPos = new GridPosition(4, 3);
        var destination = new GridPosition(1, 3);

        _grid.PlaceEntity(PlayerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(MonsterId, monsterPos, isPlayer: false);

        var service = CreateService();
        service.RegisterPlayer(PlayerId);

        // Act
        _ = service.ExecuteOpportunityAttacks(PlayerId, playerPos, destination).ToList();

        // Assert
        service.HasUsedReaction(MonsterId).Should().BeTrue();
    }

    [Test]
    public void ExecuteOpportunityAttacks_NoTrigger_ReturnsEmpty()
    {
        // Arrange
        _grid.PlaceEntity(PlayerId, new GridPosition(3, 3), isPlayer: true);
        var service = CreateService();

        // Act
        var results = service.ExecuteOpportunityAttacks(PlayerId, new GridPosition(3, 3), new GridPosition(2, 3)).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void ExecuteOpportunityAttacks_MultipleEnemies_AllAttack()
    {
        // Arrange
        var monster1 = Guid.NewGuid();
        var monster2 = Guid.NewGuid();
        var playerPos = new GridPosition(3, 3);

        _grid.PlaceEntity(PlayerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(monster1, new GridPosition(2, 3), isPlayer: false); // W
        _grid.PlaceEntity(monster2, new GridPosition(4, 3), isPlayer: false); // E

        var service = CreateService();
        service.RegisterPlayer(PlayerId);

        // Act - Move north (leaving threat range of both)
        var results = service.ExecuteOpportunityAttacks(PlayerId, playerPos, new GridPosition(3, 1)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Select(r => r.AttackerId).Should().Contain(monster1);
        results.Select(r => r.AttackerId).Should().Contain(monster2);
    }

    // ===== Reaction System Tests =====

    [Test]
    public void HasUsedReaction_NotUsed_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        service.HasUsedReaction(MonsterId).Should().BeFalse();
    }

    [Test]
    public void UseReaction_MarksAsUsed()
    {
        // Arrange
        var service = CreateService();

        // Act
        service.UseReaction(MonsterId);

        // Assert
        service.HasUsedReaction(MonsterId).Should().BeTrue();
    }

    [Test]
    public void ResetReactions_ClearsAllReactions()
    {
        // Arrange
        var service = CreateService();
        service.UseReaction(MonsterId);
        service.UseReaction(PlayerId);

        // Act
        service.ResetReactions();

        // Assert
        service.HasUsedReaction(MonsterId).Should().BeFalse();
        service.HasUsedReaction(PlayerId).Should().BeFalse();
    }

    [Test]
    public void ReactionLimitDisabled_CanUseMultiple()
    {
        // Arrange
        _config.WithOpportunityAttackConfiguration(new OpportunityAttackConfiguration { OneReactionPerRound = false });
        var playerPos = new GridPosition(3, 3);
        var monsterPos = new GridPosition(4, 3);

        _grid.PlaceEntity(PlayerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(MonsterId, monsterPos, isPlayer: false);

        var service = CreateService();
        service.RegisterPlayer(PlayerId);
        service.UseReaction(MonsterId);

        // Act - Even with reaction used, should still trigger
        var result = service.CheckOpportunityAttacks(PlayerId, playerPos, new GridPosition(1, 3));

        // Assert
        result.TriggersOpportunityAttacks.Should().BeTrue();
    }

    // ===== Disengage System Tests =====

    [Test]
    public void IsDisengaging_NotSet_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        service.IsDisengaging(PlayerId).Should().BeFalse();
    }

    [Test]
    public void SetDisengaging_MarksAsDisengaging()
    {
        // Arrange
        var service = CreateService();

        // Act
        service.SetDisengaging(PlayerId);

        // Assert
        service.IsDisengaging(PlayerId).Should().BeTrue();
    }

    [Test]
    public void ClearDisengaging_RemovesStatus()
    {
        // Arrange
        var service = CreateService();
        service.SetDisengaging(PlayerId);

        // Act
        service.ClearDisengaging(PlayerId);

        // Assert
        service.IsDisengaging(PlayerId).Should().BeFalse();
    }

    [Test]
    public void ClearAllDisengaging_ClearsAll()
    {
        // Arrange
        var service = CreateService();
        service.SetDisengaging(PlayerId);
        service.SetDisengaging(MonsterId);

        // Act
        service.ClearAllDisengaging();

        // Assert
        service.IsDisengaging(PlayerId).Should().BeFalse();
        service.IsDisengaging(MonsterId).Should().BeFalse();
    }

    // ===== Helper Methods =====

    private ThreatService CreateService() =>
        new(_gridServiceMock.Object, _config, _loggerMock.Object);
}
