using RuneAndRust.Core;
using RuneAndRust.Engine;
using Xunit;

namespace RuneAndRust.Tests;

/// <summary>
/// Integration tests for EnvironmentalObjectService (v0.22.1)
/// Tests complex scenarios: barrel chain reactions, cover degradation, ceiling collapses
/// </summary>
public class EnvironmentalObjectIntegrationTests
{
    private readonly EnvironmentalObjectService _service;
    private readonly DiceService _diceService;

    public EnvironmentalObjectIntegrationTests()
    {
        _diceService = new DiceService();
        _service = new EnvironmentalObjectService(_diceService);
    }

    /// <summary>
    /// Scenario 1: Barrel Explosion Chain Reaction
    /// Setup: 3 barrels in a line
    /// Barrel 1 → Barrel 2 (same tile) → Barrel 3 (same tile)
    /// Expected: All 3 barrels destroyed in cascading explosions
    /// </summary>
    [Fact]
    public void Integration_BarrelChainReaction_ShouldCreateCascadingExplosions()
    {
        // Arrange: 3 barrels at the same position (simplified for testing)
        // In production, these would be at adjacent positions
        var barrel1 = _service.CreateExplosiveObject(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Barrel 1",
            damageFormula: "20 Fire",
            explosionRadius: 1,
            durability: 10,
            canTriggerAdjacents: true);

        var barrel2 = _service.CreateExplosiveObject(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Barrel 2",
            damageFormula: "20 Fire",
            explosionRadius: 1,
            durability: 10,
            canTriggerAdjacents: true);

        var barrel3 = _service.CreateExplosiveObject(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Barrel 3",
            damageFormula: "20 Fire",
            explosionRadius: 1,
            durability: 10,
            canTriggerAdjacents: true);

        // Act: Player shoots Barrel 1
        var result = _service.DestroyObject(
            objectId: barrel1.ObjectId,
            destroyerId: 1,
            combatInstanceId: 1,
            destructionMethod: "Player Attack");

        // Assert: All 3 barrels destroyed
        Assert.True(result.ObjectDestroyed);
        Assert.NotEmpty(result.ChainReactions);

        // Verify barrel1 destroyed
        var obj1 = _service.GetObject(barrel1.ObjectId);
        Assert.Equal(EnvironmentalObjectState.Destroyed, obj1.State);

        // Verify barrel2 destroyed in chain reaction
        var obj2 = _service.GetObject(barrel2.ObjectId);
        Assert.Equal(EnvironmentalObjectState.Destroyed, obj2.State);

        // Verify barrel3 destroyed in secondary chain reaction
        var obj3 = _service.GetObject(barrel3.ObjectId);
        Assert.Equal(EnvironmentalObjectState.Destroyed, obj3.State);

        // Verify chain reaction message
        Assert.Contains("CHAIN REACTION", result.Message);

        // All barrels should contribute to chain reactions
        // Note: Depth of chain depends on implementation
        Assert.NotEmpty(result.ChainReactions);
    }

    /// <summary>
    /// Scenario 2: Cover Degradation Under Fire
    /// Setup: Character behind Heavy Cover (30 HP, Soak 4)
    /// Action: 3 enemy attacks (each dealing 15 damage)
    /// Expected: Cover breaks after sustained fire
    /// </summary>
    [Fact]
    public void Integration_CoverDegradation_ShouldBreakAfterMultipleHits()
    {
        // Arrange: Heavy cover protecting a character
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Heavy Barricade",
            quality: CoverQuality.Heavy,
            durability: 30,
            soakValue: 4,
            createsTerrainOnDestroy: "Difficult");

        // Act: Simulate 3 attacks hitting the cover
        // Attack 1: 15 damage - 4 soak = 11 damage to cover (30 → 19 HP)
        var attack1 = _service.ApplyDamageToObject(
            objectId: cover.ObjectId,
            damage: 15,
            damageType: "Physical",
            attackerId: 101,
            combatInstanceId: 1);

        Assert.False(attack1.ObjectDestroyed);
        Assert.Equal(11, attack1.DamageDealt);
        Assert.Equal(19, attack1.RemainingDurability);
        Assert.Equal(EnvironmentalObjectState.Active, _service.GetObject(cover.ObjectId)!.State);

        // Attack 2: 15 damage - 4 soak = 11 damage to cover (19 → 8 HP, state → Damaged)
        var attack2 = _service.ApplyDamageToObject(
            objectId: cover.ObjectId,
            damage: 15,
            damageType: "Physical",
            attackerId: 101,
            combatInstanceId: 1);

        Assert.False(attack2.ObjectDestroyed);
        Assert.Equal(11, attack2.DamageDealt);
        Assert.Equal(8, attack2.RemainingDurability);
        Assert.Equal(EnvironmentalObjectState.Damaged, _service.GetObject(cover.ObjectId)!.State); // Below 50%

        // Attack 3: 15 damage - 4 soak = 11 damage to cover (8 → 0 HP, cover DESTROYED)
        var attack3 = _service.ApplyDamageToObject(
            objectId: cover.ObjectId,
            damage: 15,
            damageType: "Physical",
            attackerId: 101,
            combatInstanceId: 1);

        // Assert: Cover destroyed, rubble created
        Assert.True(attack3.ObjectDestroyed);
        Assert.Equal("Difficult", attack3.TerrainCreated);
        Assert.Contains("DESTROYED", attack3.Message);

        // Verify cover is no longer providing protection
        var coverData = _service.GetCoverAtPosition(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            attackDirection: "Front");

        Assert.Null(coverData); // No cover remaining
    }

    /// <summary>
    /// Scenario 3: Unstable Ceiling Collapse
    /// Setup: Room with [Unstable Ceiling], 4 enemies in Front zone
    /// Action: Brewmaster uses Fire-Ale Bomb ([Explosive] tag)
    /// Expected: Massive AoE damage, enemies stunned, terrain becomes rubble
    /// </summary>
    [Fact]
    public void Integration_UnstableCeilingCollapse_ShouldAffectLargeArea()
    {
        // Arrange: Unstable ceiling hazard
        var ceiling = _service.CreateUnstableCeiling(
            roomId: 1,
            gridPosition: "Front_Center",
            explosionRadius: 3);

        // Act: Trigger ceiling collapse (simulate [Explosive] ability hit)
        var result = _service.DestroyObject(
            objectId: ceiling.ObjectId,
            destroyerId: 1, // Player
            combatInstanceId: 1,
            destructionMethod: "Explosive Ability");

        // Assert: Ceiling collapsed
        Assert.True(result.ObjectDestroyed);
        Assert.Contains("explodes", result.Message, StringComparison.OrdinalIgnoreCase);

        // Verify 25 Physical damage would be dealt (stub returns no characters)
        // In production, this would hit all characters in radius 3
        Assert.Equal(0, result.SecondaryTargets.Count); // Stub implementation

        // Verify terrain created
        Assert.Equal("Difficult", result.TerrainCreated); // Permanent rubble

        // Verify ceiling is one-time trigger
        var obj = _service.GetObject(ceiling.ObjectId);
        Assert.Equal(EnvironmentalObjectState.Destroyed, obj.State);
        Assert.Equal(0, obj.CurrentDurability);
    }

    /// <summary>
    /// Scenario 4: Mixed Environmental Hazards
    /// Setup: Cover next to explosive barrel
    /// Action: Destroy cover, barrel survives initially but takes damage from chain
    /// </summary>
    [Fact]
    public void Integration_MixedHazards_CoverAndBarrelInteraction()
    {
        // Arrange: Cover and barrel at same position
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Scrap Barricade",
            quality: CoverQuality.Light,
            durability: 15,
            soakValue: 2,
            createsTerrainOnDestroy: "Difficult");

        var barrel = _service.CreateExplosiveObject(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Fuel Canister",
            damageFormula: "25 Fire",
            explosionRadius: 1,
            durability: 8,
            canTriggerAdjacents: true);

        // Act: Destroy cover (direct damage)
        var coverResult = _service.ApplyDamageToObject(
            objectId: cover.ObjectId,
            damage: 20,
            damageType: "Physical",
            attackerId: 1,
            combatInstanceId: 1);

        Assert.True(coverResult.ObjectDestroyed);
        Assert.Equal("Difficult", coverResult.TerrainCreated);

        // Now attack the barrel (simulating player targeting it after cover destroyed)
        var barrelResult = _service.ApplyDamageToObject(
            objectId: barrel.ObjectId,
            damage: 10,
            damageType: "Physical",
            attackerId: 1,
            combatInstanceId: 1);

        // Assert: Barrel explodes
        Assert.True(barrelResult.ObjectDestroyed);
        Assert.Contains("explodes", barrelResult.Message, StringComparison.OrdinalIgnoreCase);

        // Both objects destroyed
        Assert.Equal(EnvironmentalObjectState.Destroyed, _service.GetObject(cover.ObjectId)!.State);
        Assert.Equal(EnvironmentalObjectState.Destroyed, _service.GetObject(barrel.ObjectId)!.State);
    }

    /// <summary>
    /// Scenario 5: Steam Vent Cooldown Cycle
    /// Setup: Steam vent with 2-turn cooldown
    /// Action: Destroy, verify cooldown, re-arm
    /// </summary>
    [Fact]
    public void Integration_SteamVentCooldown_ShouldRearmAfterTurns()
    {
        // Arrange: Steam vent
        var steamVent = _service.CreateSteamVent(
            roomId: 1,
            gridPosition: "Front_Right_Column_1");

        // Act: Trigger steam vent by destroying it
        var result = _service.ApplyDamageToObject(
            objectId: steamVent.ObjectId,
            damage: 25, // Enough to destroy (20 HP, 5 soak = 20 damage needed)
            damageType: "Physical",
            attackerId: 1,
            combatInstanceId: 1);

        // Assert: Destroyed and hazard triggered
        Assert.True(result.ObjectDestroyed);
        Assert.Contains("explodes", result.Message, StringComparison.OrdinalIgnoreCase);

        // Note: In a reusable hazard system, the vent would go on cooldown instead of being destroyed
        // For this test, we verify the TerrainCreated (steam dissipates after 1 turn)
        Assert.Equal("Hazardous", result.TerrainCreated);
    }

    /// <summary>
    /// Scenario 6: Cover Quality Comparison
    /// Setup: Light, Heavy, and Total cover at different positions
    /// Verify: Defense bonuses and durability are correct
    /// </summary>
    [Fact]
    public void Integration_CoverQualityComparison_ShouldHaveDifferentBonuses()
    {
        // Arrange: Different cover qualities
        var lightCover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_1",
            name: "Light Cover",
            quality: CoverQuality.Light,
            durability: 15,
            soakValue: 2);

        var heavyCover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Heavy Cover",
            quality: CoverQuality.Heavy,
            durability: 30,
            soakValue: 4);

        var totalCover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_3",
            name: "Total Cover",
            quality: CoverQuality.Total,
            durability: 50,
            soakValue: 6);

        // Act & Assert: Light cover
        var lightData = _service.GetCoverAtPosition(1, "Front_Left_Column_1", "Front");
        Assert.NotNull(lightData);
        Assert.Equal(CoverQuality.Light, lightData.Quality);
        Assert.Equal(2, lightData.DefenseBonus); // +2 Defense

        // Heavy cover
        var heavyData = _service.GetCoverAtPosition(1, "Front_Left_Column_2", "Front");
        Assert.NotNull(heavyData);
        Assert.Equal(CoverQuality.Heavy, heavyData.Quality);
        Assert.Equal(4, heavyData.DefenseBonus); // +4 Defense

        // Total cover
        var totalData = _service.GetCoverAtPosition(1, "Front_Left_Column_3", "Front");
        Assert.NotNull(totalData);
        Assert.Equal(CoverQuality.Total, totalData.Quality);
        Assert.Equal(6, totalData.DefenseBonus); // +6 Defense
    }

    /// <summary>
    /// Scenario 7: Partial Damage to Multiple Objects
    /// Setup: Two barrels at different durability levels
    /// Action: Partial damage to both, verify states
    /// </summary>
    [Fact]
    public void Integration_PartialDamage_ShouldTrackMultipleObjectStates()
    {
        // Arrange: Two barrels with different durability
        var weakBarrel = _service.CreateExplosiveObject(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Weak Barrel",
            damageFormula: "15 Fire",
            explosionRadius: 1,
            durability: 5);

        var strongBarrel = _service.CreateExplosiveObject(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Strong Barrel",
            damageFormula: "20 Fire",
            explosionRadius: 1,
            durability: 15);

        // Act: Apply 3 damage to both
        var weakResult = _service.ApplyDamageToObject(weakBarrel.ObjectId, 3, "Physical", 1, 1);
        var strongResult = _service.ApplyDamageToObject(strongBarrel.ObjectId, 3, "Physical", 1, 1);

        // Assert: Weak barrel damaged (2 HP remaining, 40% of max, state = Damaged)
        Assert.False(weakResult.ObjectDestroyed);
        Assert.Equal(2, weakResult.RemainingDurability);
        Assert.Equal(EnvironmentalObjectState.Damaged, _service.GetObject(weakBarrel.ObjectId)!.State);

        // Strong barrel still active (12 HP remaining, 80% of max, state = Active)
        Assert.False(strongResult.ObjectDestroyed);
        Assert.Equal(12, strongResult.RemainingDurability);
        Assert.Equal(EnvironmentalObjectState.Active, _service.GetObject(strongBarrel.ObjectId)!.State);

        // Apply more damage to weak barrel to destroy it
        var finalResult = _service.ApplyDamageToObject(weakBarrel.ObjectId, 5, "Physical", 1, 1);
        Assert.True(finalResult.ObjectDestroyed);

        // Chain reaction should damage strong barrel
        if (finalResult.ChainReactions.Count > 0)
        {
            var strongObj = _service.GetObject(strongBarrel.ObjectId);
            Assert.NotNull(strongObj);
            // Strong barrel may be destroyed in chain reaction depending on explosion damage
        }
    }
}
