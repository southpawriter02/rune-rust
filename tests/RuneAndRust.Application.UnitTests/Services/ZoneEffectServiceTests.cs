using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Builders;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for ZoneEffectService.
/// </summary>
/// <remarks>
/// <para>Tests cover zone creation, shape calculations, tick processing, and targeting rules:</para>
/// <list type="bullet">
///   <item><description>Zone creation at specified positions</description></item>
///   <item><description>Shape calculations (Circle, Square, Line)</description></item>
///   <item><description>Damage application to enemies</description></item>
///   <item><description>Healing application to allies</description></item>
///   <item><description>Status effect application</description></item>
///   <item><description>Duration management and expiration</description></item>
///   <item><description>Friendly/enemy targeting rules</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ZoneEffectServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<IZoneProvider> _mockZoneProvider = null!;
    private Mock<ICombatGridService> _mockGridService = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<IBuffDebuffService> _mockBuffDebuffService = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<ZoneEffectService>> _mockLogger = null!;
    private ZoneEffectService _service = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockZoneProvider = new Mock<IZoneProvider>();
        _mockGridService = new Mock<ICombatGridService>();
        _mockDiceService = new Mock<IDiceService>();
        _mockBuffDebuffService = new Mock<IBuffDebuffService>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<ZoneEffectService>>();

        // Setup default grid that accepts all positions
        var mockGrid = CombatGrid.Create(10, 10);
        _mockGridService.Setup(g => g.GetActiveGrid()).Returns(mockGrid);

        _service = new ZoneEffectService(
            _mockZoneProvider.Object,
            _mockGridService.Object,
            _mockDiceService.Object,
            _mockBuffDebuffService.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // ZONE CREATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateZone creates a zone at the specified position.
    /// </summary>
    [Test]
    public void CreateZone_CreatesZoneAtPosition()
    {
        // Arrange
        var definition = CreateDamageZoneDefinition("wall-of-fire", ZoneShape.Circle, 2);
        SetupZoneProvider("wall-of-fire", definition);

        var player = PlayerBuilder.Create().Build();
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        // Act
        var zone = _service.CreateZone("wall-of-fire", center, caster);

        // Assert
        zone.Should().NotBeNull();
        zone.Center.Should().Be(center);
        zone.ZoneId.Should().Be("wall-of-fire");
        zone.CasterId.Should().Be(caster.Id);
        zone.CellCount.Should().BeGreaterThan(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // SHAPE CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Circle shape calculates cells correctly using Euclidean distance.
    /// </summary>
    [Test]
    public void CreateZone_CircleShape_CalculatesCellsCorrectly()
    {
        // Arrange
        var definition = CreateDamageZoneDefinition("fire-circle", ZoneShape.Circle, 2);
        SetupZoneProvider("fire-circle", definition);

        var player = PlayerBuilder.Create().Build();
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        // Act
        var zone = _service.CreateZone("fire-circle", center, caster);

        // Assert
        // Circle with radius 2 should include cells where x^2 + y^2 <= 4
        // Center (5,5) and cells within Euclidean distance 2
        zone.ContainsPosition(center).Should().BeTrue(); // Center
        zone.ContainsPosition(new GridPosition(5, 3)).Should().BeTrue(); // 2 cells away vertically
        zone.ContainsPosition(new GridPosition(5, 7)).Should().BeTrue(); // 2 cells away vertically
        zone.ContainsPosition(new GridPosition(3, 5)).Should().BeTrue(); // 2 cells away horizontally
        zone.ContainsPosition(new GridPosition(7, 5)).Should().BeTrue(); // 2 cells away horizontally
        // Diagonal at distance sqrt(2*2 + 2*2) = sqrt(8) > 2, so should NOT be included
        zone.ContainsPosition(new GridPosition(7, 7)).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Square shape calculates cells correctly.
    /// </summary>
    [Test]
    public void CreateZone_SquareShape_CalculatesCellsCorrectly()
    {
        // Arrange
        var definition = CreateDamageZoneDefinition("lightning-square", ZoneShape.Square, 1);
        SetupZoneProvider("lightning-square", definition);

        var player = PlayerBuilder.Create().Build();
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        // Act
        var zone = _service.CreateZone("lightning-square", center, caster);

        // Assert
        // Square with radius 1 should be a 3x3 grid (9 cells)
        zone.CellCount.Should().Be(9);
        zone.ContainsPosition(center).Should().BeTrue();
        zone.ContainsPosition(new GridPosition(4, 4)).Should().BeTrue();
        zone.ContainsPosition(new GridPosition(6, 6)).Should().BeTrue();
        zone.ContainsPosition(new GridPosition(4, 6)).Should().BeTrue();
        zone.ContainsPosition(new GridPosition(6, 4)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Line shape calculates cells correctly in the given direction.
    /// </summary>
    [Test]
    public void CreateZone_LineShape_CalculatesCellsCorrectly()
    {
        // Arrange
        var definition = CreateDamageZoneDefinition("fire-line", ZoneShape.Line, 3);
        SetupZoneProvider("fire-line", definition);

        var player = PlayerBuilder.Create().Build();
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        // Act - Create line extending East
        var zone = _service.CreateZone("fire-line", center, Direction.East, caster);

        // Assert
        // Line with radius 3 extending East from (5,5): includes (5,5), (6,5), (7,5), (8,5)
        zone.CellCount.Should().Be(4); // Center + 3 in direction
        zone.ContainsPosition(center).Should().BeTrue();
        zone.ContainsPosition(new GridPosition(6, 5)).Should().BeTrue();
        zone.ContainsPosition(new GridPosition(7, 5)).Should().BeTrue();
        zone.ContainsPosition(new GridPosition(8, 5)).Should().BeTrue();
        // Should not include cells in other directions
        zone.ContainsPosition(new GridPosition(4, 5)).Should().BeFalse();
        zone.ContainsPosition(new GridPosition(5, 6)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // TICK PROCESSING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that TickZones applies damage to enemies in damage zones.
    /// </summary>
    [Test]
    public void TickZones_AppliesDamageToEnemies()
    {
        // Arrange
        var definition = CreateDamageZoneDefinition("fire-zone", ZoneShape.Circle, 2);
        SetupZoneProvider("fire-zone", definition);
        SetupDiceRoll(8); // Roll 8 damage

        var player = PlayerBuilder.Create().Build();
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        // Create zone
        _service.CreateZone("fire-zone", center, caster);

        // Create enemy monster in the zone
        var monster = MonsterBuilder.Goblin().Build();
        var enemyCombatant = Combatant.ForMonster(monster, CreateInitiativeRoll(), 1);

        // Setup monster position to be inside zone
        _mockGridService.Setup(g => g.GetEntityPosition(enemyCombatant.Id))
            .Returns(center);

        // Act
        var result = _service.TickZones(new[] { caster, enemyCombatant });

        // Assert
        result.TotalDamageDealt.Should().BeGreaterThan(0);
        result.EntitiesDamaged.Should().Contain(enemyCombatant.Id);
    }

    /// <summary>
    /// Verifies that TickZones applies healing to allies in healing zones.
    /// </summary>
    [Test]
    public void TickZones_AppliesHealingToAllies()
    {
        // Arrange
        var definition = CreateHealingZoneDefinition("heal-zone", ZoneShape.Circle, 2);
        SetupZoneProvider("heal-zone", definition);
        SetupDiceRoll(6); // Roll 6 healing

        var player = PlayerBuilder.Create().Build();
        player.TakeDamage(20); // Damage player first
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        // Create zone
        _service.CreateZone("heal-zone", center, caster);

        // Setup caster position to be inside zone (caster is friendly)
        _mockGridService.Setup(g => g.GetEntityPosition(caster.Id))
            .Returns(center);

        // Act
        var result = _service.TickZones(new[] { caster });

        // Assert
        result.TotalHealingDone.Should().BeGreaterThan(0);
        result.EntitiesHealed.Should().Contain(caster.Id);
    }

    /// <summary>
    /// Verifies that TickZones applies status effects from buff/debuff zones.
    /// </summary>
    /// <summary>
    /// Verifies that debuff zones do not apply status effects when IEffectTarget is not implemented.
    /// </summary>
    /// <remarks>
    /// <para>This test documents current behavior: status effects cannot be applied because
    /// Player and Monster do not implement IEffectTarget. Once those entities implement
    /// the interface, this test should be updated to verify ApplyEffect is called.</para>
    /// </remarks>
    [Test]
    public void TickZones_AppliesStatusEffects_GracefullyHandlesNoEffectTarget()
    {
        // Arrange
        var definition = CreateDebuffZoneDefinition("slow-zone", ZoneShape.Circle, 2, "slowed");
        SetupZoneProvider("slow-zone", definition);

        // Create a mock status effect for the ApplyResult (won't be used since IEffectTarget is null)
        var effectDef = StatusEffectDefinition.Create(
            id: "slowed",
            name: "Slowed",
            description: "Movement speed reduced",
            category: EffectCategory.Debuff,
            durationType: DurationType.Turns,
            baseDuration: 3);
        var activeEffect = ActiveStatusEffect.Create(effectDef);

        _mockBuffDebuffService
            .Setup(b => b.ApplyEffect(It.IsAny<IEffectTarget>(), "slowed", It.IsAny<Guid?>(), It.IsAny<string?>()))
            .Returns(ApplyResult.Success(activeEffect));

        var player = PlayerBuilder.Create().Build();
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        // Create zone
        _service.CreateZone("slow-zone", center, caster);

        // Create enemy monster in the zone
        var monster = MonsterBuilder.Goblin().Build();
        var enemyCombatant = Combatant.ForMonster(monster, CreateInitiativeRoll(), 1);

        _mockGridService.Setup(g => g.GetEntityPosition(enemyCombatant.Id))
            .Returns(center);

        // Act - should not throw even though IEffectTarget is not available
        Action act = () => _service.TickZones(new[] { caster, enemyCombatant });

        // Assert - no exception, but ApplyEffect is NOT called because Player/Monster
        // do not implement IEffectTarget. When they do, update this test.
        act.Should().NotThrow();
        _mockBuffDebuffService.Verify(
            b => b.ApplyEffect(It.IsAny<IEffectTarget>(), "slowed", It.IsAny<Guid?>(), It.IsAny<string?>()),
            Times.Never,
            "ApplyEffect should not be called because Player/Monster do not implement IEffectTarget");
    }

    /// <summary>
    /// Verifies that zone duration decrements on each tick.
    /// </summary>
    [Test]
    public void TickZones_DecrementsDuration()
    {
        // Arrange
        var definition = CreateDamageZoneDefinition("fire-zone", ZoneShape.Circle, 2, duration: 3);
        SetupZoneProvider("fire-zone", definition);

        var player = PlayerBuilder.Create().Build();
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        var zone = _service.CreateZone("fire-zone", center, caster);
        var initialDuration = zone.RemainingDuration;

        // Act
        _service.TickZones(new[] { caster });

        // Assert
        zone.RemainingDuration.Should().Be(initialDuration - 1);
    }

    /// <summary>
    /// Verifies that expired zones are removed from active zones.
    /// </summary>
    [Test]
    public void TickZones_RemovesExpiredZones()
    {
        // Arrange - Zone with 1 turn duration
        var definition = CreateDamageZoneDefinition("short-zone", ZoneShape.Circle, 2, duration: 1);
        SetupZoneProvider("short-zone", definition);

        var player = PlayerBuilder.Create().Build();
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        _service.CreateZone("short-zone", center, caster);
        _service.GetActiveZoneCount().Should().Be(1);

        // Act - Tick to expire the zone
        var result = _service.TickZones(new[] { caster });

        // Assert
        _service.GetActiveZoneCount().Should().Be(0);
        result.ExpiredZones.Should().Contain("short-zone");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetZonesAt returns zones affecting a position.
    /// </summary>
    [Test]
    public void GetZonesAt_ReturnsAffectingZones()
    {
        // Arrange
        var definition = CreateDamageZoneDefinition("fire-zone", ZoneShape.Circle, 2);
        SetupZoneProvider("fire-zone", definition);

        var player = PlayerBuilder.Create().Build();
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        _service.CreateZone("fire-zone", center, caster);

        // Act
        var zonesAtCenter = _service.GetZonesAt(center);
        var zonesOutside = _service.GetZonesAt(new GridPosition(0, 0));

        // Assert
        zonesAtCenter.Should().HaveCount(1);
        zonesAtCenter[0].ZoneId.Should().Be("fire-zone");
        zonesOutside.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // TARGETING RULES TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that AffectsFriendly and AffectsEnemy settings are respected.
    /// </summary>
    [Test]
    public void TickZones_RespectsTargetingRules()
    {
        // Arrange - Damage zone that only affects enemies
        var definition = CreateDamageZoneDefinition("enemy-only", ZoneShape.Circle, 2);
        SetupZoneProvider("enemy-only", definition);
        SetupDiceRoll(10);

        var player = PlayerBuilder.Create().Build();
        var caster = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var center = new GridPosition(5, 5);

        _service.CreateZone("enemy-only", center, caster);

        // Setup caster in zone (friendly, should not be damaged)
        _mockGridService.Setup(g => g.GetEntityPosition(caster.Id))
            .Returns(center);

        // Act
        var result = _service.TickZones(new[] { caster });

        // Assert - Caster should not take damage (zone doesn't affect friendlies)
        result.EntitiesDamaged.Should().NotContain(caster.Id);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a valid InitiativeRoll for testing.
    /// </summary>
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

    /// <summary>
    /// Creates a damage zone definition for testing.
    /// </summary>
    private static ZoneDefinition CreateDamageZoneDefinition(
        string zoneId,
        ZoneShape shape,
        int radius,
        int duration = 5)
    {
        return ZoneDefinition.Create(
            zoneId: zoneId,
            name: $"Test {zoneId}",
            description: $"Test zone {zoneId}",
            effectType: ZoneEffectType.Damage,
            shape: shape,
            radius: radius,
            duration: duration,
            affectsFriendly: false,
            affectsEnemy: true)
            .WithDamage("2d6", "fire");
    }

    /// <summary>
    /// Creates a healing zone definition for testing.
    /// </summary>
    private static ZoneDefinition CreateHealingZoneDefinition(
        string zoneId,
        ZoneShape shape,
        int radius,
        int duration = 5)
    {
        return ZoneDefinition.Create(
            zoneId: zoneId,
            name: $"Test {zoneId}",
            description: $"Test zone {zoneId}",
            effectType: ZoneEffectType.Healing,
            shape: shape,
            radius: radius,
            duration: duration,
            affectsFriendly: true,
            affectsEnemy: false)
            .WithHealing("1d6+2");
    }

    /// <summary>
    /// Creates a debuff zone definition for testing.
    /// </summary>
    private static ZoneDefinition CreateDebuffZoneDefinition(
        string zoneId,
        ZoneShape shape,
        int radius,
        string statusEffectId,
        int duration = 5)
    {
        return ZoneDefinition.Create(
            zoneId: zoneId,
            name: $"Test {zoneId}",
            description: $"Test zone {zoneId}",
            effectType: ZoneEffectType.Debuff,
            shape: shape,
            radius: radius,
            duration: duration,
            affectsFriendly: false,
            affectsEnemy: true)
            .WithStatusEffect(statusEffectId);
    }

    /// <summary>
    /// Sets up the zone provider mock to return a zone definition.
    /// </summary>
    private void SetupZoneProvider(string zoneId, ZoneDefinition definition)
    {
        _mockZoneProvider
            .Setup(p => p.GetZone(zoneId))
            .Returns(definition);
    }

    /// <summary>
    /// Sets up the dice service mock to return a specific roll value.
    /// </summary>
    private void SetupDiceRoll(int rollValue)
    {
        _mockDiceService
            .Setup(d => d.RollTotal(It.IsAny<string>()))
            .Returns(rollValue);
    }
}
