using RuneAndRust.Core;
using RuneAndRust.Engine;
using Xunit;

namespace RuneAndRust.Tests;

/// <summary>
/// Unit tests for EnvironmentalObjectService (v0.22.1)
/// Tests cover: damage, destruction, chain reactions, cover, and hazard triggers
/// </summary>
public class EnvironmentalObjectServiceTests
{
    private readonly EnvironmentalObjectService _service;
    private readonly DiceService _diceService;

    public EnvironmentalObjectServiceTests()
    {
        _diceService = new DiceService();
        _service = new EnvironmentalObjectService(_diceService);
    }

    #region Object Creation Tests

    [Fact]
    public void CreateCover_ShouldCreateDestructibleCoverWithSoak()
    {
        // Arrange & Act
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Light Barricade",
            quality: CoverQuality.Light,
            durability: 15,
            soakValue: 2);

        // Assert
        Assert.NotNull(cover);
        Assert.Equal("Light Barricade", cover.Name);
        Assert.Equal(15, cover.CurrentDurability);
        Assert.Equal(15, cover.MaxDurability);
        Assert.Equal(2, cover.SoakValue);
        Assert.True(cover.IsDestructible);
        Assert.True(cover.ProvidesCover);
        Assert.Equal(CoverQuality.Light, cover.CoverQuality);
        Assert.Equal("Difficult", cover.CreatesTerrainOnDestroy);
    }

    [Fact]
    public void CreateExplosiveObject_ShouldCreateExplosiveBarrel()
    {
        // Arrange & Act
        var barrel = _service.CreateExplosiveObject(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Explosive Barrel",
            damageFormula: "20 Fire",
            explosionRadius: 1,
            durability: 10,
            canTriggerAdjacents: true);

        // Assert
        Assert.NotNull(barrel);
        Assert.Equal("Explosive Barrel", barrel.Name);
        Assert.Equal(10, barrel.CurrentDurability);
        Assert.True(barrel.IsHazard);
        Assert.Equal(HazardTrigger.OnDestroy, barrel.HazardTrigger);
        Assert.Equal("20 Fire", barrel.DamageFormula);
        Assert.Equal("Fire", barrel.DamageType);
        Assert.Equal(1, barrel.ExplosionRadius);
        Assert.True(barrel.CanTriggerAdjacents);
    }

    [Fact]
    public void CreateUnstableCeiling_ShouldCreateCeilingHazard()
    {
        // Arrange & Act
        var ceiling = _service.CreateUnstableCeiling(
            roomId: 1,
            gridPosition: "Front_Center",
            explosionRadius: 3);

        // Assert
        Assert.NotNull(ceiling);
        Assert.Equal("Unstable Ceiling", ceiling.Name);
        Assert.False(ceiling.IsDestructible); // Triggered by abilities, not direct damage
        Assert.True(ceiling.IsHazard);
        Assert.Equal("25 Physical", ceiling.DamageFormula);
        Assert.Equal("[Stunned]", ceiling.StatusEffect);
        Assert.True(ceiling.IgnoresSoak);
        Assert.Equal(3, ceiling.ExplosionRadius);
        Assert.Equal(1, ceiling.TriggersRemaining);
    }

    [Fact]
    public void CreateSteamVent_ShouldCreateReusableHazard()
    {
        // Arrange & Act
        var steamVent = _service.CreateSteamVent(
            roomId: 1,
            gridPosition: "Front_Right_Column_1");

        // Assert
        Assert.NotNull(steamVent);
        Assert.Equal("High-Pressure Steam Vent", steamVent.Name);
        Assert.Equal(20, steamVent.CurrentDurability);
        Assert.Equal(5, steamVent.SoakValue);
        Assert.Equal("15 Fire", steamVent.DamageFormula);
        Assert.Equal("[Obscuring Terrain]", steamVent.StatusEffect);
        Assert.Equal(2, steamVent.CooldownDuration);
        Assert.Equal("Hazardous", steamVent.CreatesTerrainOnDestroy);
        Assert.Equal(1, steamVent.TerrainDuration);
    }

    #endregion

    #region Damage and Destruction Tests

    [Fact]
    public void ApplyDamageToObject_ShouldReduceDurabilityAfterSoak()
    {
        // Arrange: Light cover with 15 HP, Soak 2
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Light Barricade",
            quality: CoverQuality.Light,
            durability: 15,
            soakValue: 2);

        // Act: Apply 10 damage
        var result = _service.ApplyDamageToObject(
            objectId: cover.ObjectId,
            damage: 10,
            damageType: "Physical",
            attackerId: 1,
            combatInstanceId: 1);

        // Assert: Damage after soak = 8, durability = 7
        Assert.True(result.Success);
        Assert.False(result.ObjectDestroyed);
        Assert.Equal(8, result.DamageDealt);
        Assert.Equal(7, result.RemainingDurability);

        // Verify object state
        var obj = _service.GetObject(cover.ObjectId);
        Assert.NotNull(obj);
        Assert.Equal(7, obj.CurrentDurability);
        Assert.Equal(EnvironmentalObjectState.Active, obj.State); // Still above 50%
    }

    [Fact]
    public void ApplyDamageToObject_ShouldSetDamagedStateBelow50Percent()
    {
        // Arrange: Cover with 20 HP
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Heavy Barricade",
            quality: CoverQuality.Heavy,
            durability: 20,
            soakValue: 0);

        // Act: Apply 15 damage (leaves 5 HP = 25% of max)
        var result = _service.ApplyDamageToObject(
            objectId: cover.ObjectId,
            damage: 15,
            damageType: "Physical",
            attackerId: 1,
            combatInstanceId: 1);

        // Assert: State changed to Damaged
        Assert.True(result.Success);
        Assert.False(result.ObjectDestroyed);
        Assert.Equal(5, result.RemainingDurability);

        var obj = _service.GetObject(cover.ObjectId);
        Assert.NotNull(obj);
        Assert.Equal(EnvironmentalObjectState.Damaged, obj.State);
    }

    [Fact]
    public void ApplyDamageToObject_ShouldDestroyWhenDurabilityZero()
    {
        // Arrange: Weak cover with 5 HP
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Flimsy Boards",
            quality: CoverQuality.Light,
            durability: 5,
            soakValue: 0);

        // Act: Apply 10 damage (overkill)
        var result = _service.ApplyDamageToObject(
            objectId: cover.ObjectId,
            damage: 10,
            damageType: "Physical",
            attackerId: 1,
            combatInstanceId: 1);

        // Assert: Object destroyed
        Assert.True(result.Success);
        Assert.True(result.ObjectDestroyed);
        Assert.Equal("Flimsy Boards", result.ObjectName);
        Assert.NotEmpty(result.Message);

        // Verify object state
        var obj = _service.GetObject(cover.ObjectId);
        Assert.NotNull(obj);
        Assert.Equal(EnvironmentalObjectState.Destroyed, obj.State);
        Assert.Equal(0, obj.CurrentDurability);
    }

    [Fact]
    public void ApplyDamageToObject_SoakShouldPreventDamage()
    {
        // Arrange: Cover with high soak
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Reinforced Wall",
            quality: CoverQuality.Heavy,
            durability: 30,
            soakValue: 10);

        // Act: Apply 8 damage (less than soak)
        var result = _service.ApplyDamageToObject(
            objectId: cover.ObjectId,
            damage: 8,
            damageType: "Physical",
            attackerId: 1,
            combatInstanceId: 1);

        // Assert: No damage dealt
        Assert.True(result.Success);
        Assert.False(result.ObjectDestroyed);
        Assert.Equal(0, result.DamageDealt);
        Assert.Equal(30, result.RemainingDurability); // Unchanged

        var obj = _service.GetObject(cover.ObjectId);
        Assert.Equal(30, obj.CurrentDurability);
    }

    [Fact]
    public void DestroyObject_ShouldCreateTerrainAftermath()
    {
        // Arrange: Cover that creates rubble when destroyed
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Heavy Barricade",
            quality: CoverQuality.Heavy,
            durability: 30,
            soakValue: 0,
            createsTerrainOnDestroy: "Difficult");

        // Act: Destroy the object
        var result = _service.DestroyObject(
            objectId: cover.ObjectId,
            destroyerId: 1,
            combatInstanceId: 1,
            destructionMethod: "Direct Damage");

        // Assert: Difficult terrain created
        Assert.True(result.ObjectDestroyed);
        Assert.Equal("Difficult", result.TerrainCreated);
        Assert.Contains("DESTROYED", result.Message);
    }

    [Fact]
    public void DestroyObject_NonDestructibleObject_ShouldFail()
    {
        // Arrange: Non-destructible object
        var obj = _service.GetObject(999); // Non-existent object

        // Act
        var result = _service.DestroyObject(
            objectId: 999,
            destroyerId: 1,
            combatInstanceId: 1,
            destructionMethod: "Test");

        // Assert
        Assert.False(result.Success);
        Assert.False(result.ObjectDestroyed);
    }

    #endregion

    #region Hazard Trigger Tests

    [Fact]
    public void DestroyObject_ExplosiveBarrel_ShouldTriggerHazard()
    {
        // Arrange: Explosive barrel
        var barrel = _service.CreateExplosiveObject(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Explosive Barrel",
            damageFormula: "20 Fire",
            explosionRadius: 1,
            durability: 10);

        // Act: Destroy barrel
        var result = _service.DestroyObject(
            objectId: barrel.ObjectId,
            destroyerId: 1,
            combatInstanceId: 1,
            destructionMethod: "Attack");

        // Assert: Hazard triggered
        Assert.True(result.ObjectDestroyed);
        Assert.Contains("explodes", result.Message, StringComparison.OrdinalIgnoreCase);
        // Note: No characters in range, so no secondary targets
        Assert.Empty(result.SecondaryTargets);
    }

    [Fact]
    public void DestroyObject_SteamVent_ShouldTriggerHazardWithStatusEffect()
    {
        // Arrange: Steam vent
        var steamVent = _service.CreateSteamVent(
            roomId: 1,
            gridPosition: "Front_Right_Column_1");

        // Reduce durability to near destruction
        _service.ApplyDamageToObject(steamVent.ObjectId, 15, "Physical", 1, 1);

        // Act: Destroy steam vent
        var result = _service.ApplyDamageToObject(
            objectId: steamVent.ObjectId,
            damage: 10,
            damageType: "Physical",
            attackerId: 1,
            combatInstanceId: 1);

        // Assert: Hazard triggered
        Assert.True(result.ObjectDestroyed);
        Assert.Contains("explodes", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Hazardous", result.TerrainCreated);
    }

    #endregion

    #region Chain Reaction Tests

    [Fact]
    public void DestroyObject_BarrelNearOtherBarrels_ShouldNotChainWithoutAdjacents()
    {
        // Arrange: Two barrels at same position, but source doesn't trigger adjacents
        var barrel1 = _service.CreateExplosiveObject(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Barrel 1",
            damageFormula: "20 Fire",
            explosionRadius: 1,
            durability: 10,
            canTriggerAdjacents: false); // No chain reactions

        var barrel2 = _service.CreateExplosiveObject(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Barrel 2",
            damageFormula: "20 Fire",
            explosionRadius: 1,
            durability: 10);

        // Act: Destroy barrel 1
        var result = _service.DestroyObject(
            objectId: barrel1.ObjectId,
            destroyerId: 1,
            combatInstanceId: 1,
            destructionMethod: "Attack");

        // Assert: No chain reaction (barrel1.CanTriggerAdjacents = false)
        Assert.True(result.ObjectDestroyed);
        Assert.Empty(result.ChainReactions);
    }

    [Fact]
    public void DestroyObject_BarrelWithChainReaction_ShouldDestroyNearbyBarrels()
    {
        // Arrange: Two barrels at same position
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
            durability: 10);

        // Act: Destroy barrel 1
        var result = _service.DestroyObject(
            objectId: barrel1.ObjectId,
            destroyerId: 1,
            combatInstanceId: 1,
            destructionMethod: "Attack");

        // Assert: Chain reaction triggered
        Assert.True(result.ObjectDestroyed);
        Assert.NotEmpty(result.ChainReactions);
        Assert.Single(result.ChainReactions);
        Assert.True(result.ChainReactions[0].ObjectDestroyed);
        Assert.Contains("CHAIN REACTION", result.Message);

        // Verify barrel2 is destroyed
        var obj2 = _service.GetObject(barrel2.ObjectId);
        Assert.Equal(EnvironmentalObjectState.Destroyed, obj2.State);
    }

    #endregion

    #region Cover Management Tests

    [Fact]
    public void GetCoverAtPosition_ShouldReturnCoverData()
    {
        // Arrange: Heavy cover at position
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Heavy Scrap Wall",
            quality: CoverQuality.Heavy,
            durability: 30,
            soakValue: 4);

        // Act: Check for cover
        var coverData = _service.GetCoverAtPosition(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            attackDirection: "Front");

        // Assert: Cover found with correct properties
        Assert.NotNull(coverData);
        Assert.Equal(CoverQuality.Heavy, coverData.Quality);
        Assert.Equal(4, coverData.DefenseBonus); // Heavy = +4
        Assert.Equal(30, coverData.RemainingDurability);
    }

    [Fact]
    public void GetCoverAtPosition_DestroyedCover_ShouldReturnNull()
    {
        // Arrange: Cover that will be destroyed
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Destroyed Wall",
            quality: CoverQuality.Light,
            durability: 5,
            soakValue: 0);

        // Destroy it
        _service.DestroyObject(cover.ObjectId, "Previous Combat");

        // Act: Check for cover at that position
        var coverData = _service.GetCoverAtPosition(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            attackDirection: "Front");

        // Assert: No cover found
        Assert.Null(coverData);
    }

    [Fact]
    public void GetCoverAtPosition_MultipleCover_ShouldReturnBest()
    {
        // Arrange: Multiple cover objects at same position
        var lightCover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Light Cover",
            quality: CoverQuality.Light,
            durability: 10,
            soakValue: 1);

        var heavyCover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Heavy Cover",
            quality: CoverQuality.Heavy,
            durability: 30,
            soakValue: 4);

        // Act: Get cover at position
        var coverData = _service.GetCoverAtPosition(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            attackDirection: "Front");

        // Assert: Best cover (Heavy) returned
        Assert.NotNull(coverData);
        Assert.Equal(CoverQuality.Heavy, coverData.Quality);
        Assert.Equal(4, coverData.DefenseBonus);
    }

    #endregion

    #region Object Retrieval Tests

    [Fact]
    public void GetObject_ValidId_ShouldReturnObject()
    {
        // Arrange
        var cover = _service.CreateCover(
            roomId: 1,
            gridPosition: "Front_Left_Column_2",
            name: "Test Cover",
            quality: CoverQuality.Light,
            durability: 15,
            soakValue: 2);

        // Act
        var retrieved = _service.GetObject(cover.ObjectId);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(cover.ObjectId, retrieved.ObjectId);
        Assert.Equal("Test Cover", retrieved.Name);
    }

    [Fact]
    public void GetObject_InvalidId_ShouldReturnNull()
    {
        // Act
        var retrieved = _service.GetObject(999);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void GetObjectsInRoom_ShouldReturnAllActiveObjects()
    {
        // Arrange
        var cover1 = _service.CreateCover(1, "Pos1", "Cover1", CoverQuality.Light, 15);
        var cover2 = _service.CreateCover(1, "Pos2", "Cover2", CoverQuality.Heavy, 30);
        var cover3 = _service.CreateCover(2, "Pos3", "Cover3", CoverQuality.Light, 10); // Different room

        // Destroy one cover
        _service.DestroyObject(cover2.ObjectId, "Test");

        // Act
        var objects = _service.GetObjectsInRoom(1);

        // Assert: Only active objects in room 1
        Assert.Single(objects);
        Assert.Equal(cover1.ObjectId, objects[0].ObjectId);
    }

    [Fact]
    public void GetObjectsAtPosition_ShouldReturnObjectsAtSpecificPosition()
    {
        // Arrange
        var cover1 = _service.CreateCover(1, "Front_Left_Column_2", "Cover1", CoverQuality.Light, 15);
        var barrel = _service.CreateExplosiveObject(1, "Front_Left_Column_2", "Barrel", "20 Fire", 1);
        var cover2 = _service.CreateCover(1, "Front_Right_Column_1", "Cover2", CoverQuality.Heavy, 30);

        // Act
        var objects = _service.GetObjectsAtPosition(1, "Front_Left_Column_2");

        // Assert: Two objects at this position
        Assert.Equal(2, objects.Count);
        Assert.Contains(objects, o => o.Name == "Cover1");
        Assert.Contains(objects, o => o.Name == "Barrel");
    }

    [Fact]
    public void GetDestructibleObjectsInRoom_ShouldOnlyReturnDestructible()
    {
        // Arrange
        var destructibleCover = _service.CreateCover(1, "Pos1", "Cover", CoverQuality.Light, 15);
        var ceiling = _service.CreateUnstableCeiling(1, "Pos2"); // Not destructible by damage

        // Act
        var destructible = _service.GetDestructibleObjectsInRoom(1);

        // Assert: Only destructible cover returned
        Assert.Single(destructible);
        Assert.Equal(destructibleCover.ObjectId, destructible[0].ObjectId);
    }

    #endregion

    #region Cooldown Tests

    [Fact]
    public void UpdateCooldowns_ShouldDecrementAndRearm()
    {
        // Arrange: Steam vent with cooldown
        var steamVent = _service.CreateSteamVent(1, "Front_Right_Column_1");

        // Manually set cooldown (simulating after trigger)
        var obj = _service.GetObject(steamVent.ObjectId);
        Assert.NotNull(obj);
        obj.CooldownRemaining = 2;
        obj.TriggersRemaining = 0;
        obj.State = EnvironmentalObjectState.Triggered;

        // Act: Update cooldowns twice
        _service.UpdateCooldowns(1);
        Assert.Equal(1, obj.CooldownRemaining);

        _service.UpdateCooldowns(1);
        Assert.Equal(0, obj.CooldownRemaining);

        // Assert: Re-armed
        Assert.Equal(EnvironmentalObjectState.Active, obj.State);
        Assert.Equal(1, obj.TriggersRemaining);
    }

    #endregion

    #region Utility Tests

    [Fact]
    public void ClearRoom_ShouldRemoveAllObjectsInRoom()
    {
        // Arrange
        var cover1 = _service.CreateCover(1, "Pos1", "Cover1", CoverQuality.Light, 15);
        var cover2 = _service.CreateCover(1, "Pos2", "Cover2", CoverQuality.Heavy, 30);
        var cover3 = _service.CreateCover(2, "Pos3", "Cover3", CoverQuality.Light, 10); // Different room

        // Act
        _service.ClearRoom(1);

        // Assert: Room 1 cleared, room 2 intact
        var room1Objects = _service.GetObjectsInRoom(1);
        var room2Objects = _service.GetObjectsInRoom(2);

        Assert.Empty(room1Objects);
        Assert.Single(room2Objects);
        Assert.Equal(cover3.ObjectId, room2Objects[0].ObjectId);
    }

    #endregion
}
