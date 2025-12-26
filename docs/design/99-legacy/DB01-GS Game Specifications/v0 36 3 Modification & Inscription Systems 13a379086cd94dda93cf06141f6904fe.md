# v0.36.3: Modification & Inscription Systems

Type: Technical
Description: ModificationService - equipment modifications, runic inscriptions, stat adjustments
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.36.1, v0.36.2
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.36: Advanced Crafting System (v0%2036%20Advanced%20Crafting%20System%20e00690b9cf4b48538f10810b7f477711.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.36.3-MODIFICATIONS

**Parent Specification:** [v0.36: Advanced Crafting System](v0%2036%20Advanced%20Crafting%20System%20e00690b9cf4b48538f10810b7f477711.md)

**Status:** Design Complete — Ready for Implementation

**Timeline:** 7-11 hours

**Prerequisites:** v0.36.1 (Database), v0.36.2 (CraftingService)

---

## I. Executive Summary

v0.36.3 implements **equipment modification and runic inscription systems**:

- **ModificationService** — Apply and manage equipment modifications
- **Runic inscription crafting** — Apply temporary and permanent runes
- **Stat adjustment mechanics** — Modify equipment stats dynamically
- **Special effect application** — Add elemental damage, resistances, status effects
- **Modification slot system** — Limit modifications per item (max 3)

This specification transforms equipment from static items into customizable gear that players can enhance.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.36.3)

- ModificationService implementation
- Equipment stat modification system
- Runic inscription application
- Temporary modification tracking (uses remaining)
- Permanent modification system
- Modification slot limits (max 3 per item)
- Modification removal mechanics
- Unit test suite (10+ tests, 85%+ coverage)
- Serilog structured logging

### ❌ Explicitly Out of Scope

- Recipe discovery (defer to v0.36.4)
- Modification UI/menu (defer to v0.36.4)
- Crafting menu integration (defer to v0.36.4)
- Component purchasing (defer to v0.36.4)
- Legendary modifications (defer to v0.37)
- Set bonuses (defer to v0.37)

---

## III. ModificationService Implementation

### Core Service

```csharp
public class ModificationService
{
    private readonly IDbConnection _db;
    private readonly ILogger<ModificationService> _logger;
    private readonly CraftingService _craftingService;
    
    private const int MAX_MODIFICATIONS_PER_ITEM = 3;
    
    public ModificationService(
        IDbConnection db,
        ILogger<ModificationService> logger,
        CraftingService craftingService)
    {
        _db = db;
        _logger = logger;
        _craftingService = craftingService;
    }
    
    /// <summary>
    /// Apply a modification to equipment.
    /// </summary>
    public async Task<ModificationResult> ApplyModification(
        int characterId,
        int equipmentInventoryId,
        int inscriptionId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Apply modification: character={CharacterId}, equipment={EquipmentId}, inscription={InscriptionId}",
            characterId, equipmentInventoryId, inscriptionId);
        
        try
        {
            // Get inscription details
            var inscription = await GetInscription(inscriptionId);
            if (inscription == null)
            {
                _logger.Warning("Inscription {InscriptionId} not found", inscriptionId);
                return ModificationResult.Failure("Inscription not found");
            }
            
            // Get equipment item
            var equipment = await GetEquipmentItem(characterId, equipmentInventoryId);
            if (equipment == null)
            {
                _logger.Warning("Equipment {EquipmentId} not found for character {CharacterId}",
                    equipmentInventoryId, characterId);
                return ModificationResult.Failure("Equipment not found");
            }
            
            // Validate equipment type matches inscription target
            if (!ValidateEquipmentType(equipment.ItemType, inscription.TargetEquipmentType))
            {
                _logger.Information(
                    "Equipment type mismatch: {EquipmentType} cannot use {TargetType} inscription",
                    equipment.ItemType, inscription.TargetEquipmentType);
                return ModificationResult.Failure(
                    $"This inscription can only be applied to {inscription.TargetEquipmentType}");
            }
            
            // Check modification slot limit
            var existingMods = await GetActiveModifications(equipmentInventoryId);
            if (existingMods.Count >= MAX_MODIFICATIONS_PER_ITEM)
            {
                _logger.Information(
                    "Equipment {EquipmentId} has {Count} modifications (max {Max})",
                    equipmentInventoryId, existingMods.Count, MAX_MODIFICATIONS_PER_ITEM);
                return ModificationResult.Failure(
                    $"Equipment already has maximum {MAX_MODIFICATIONS_PER_ITEM} modifications");
            }
            
            // Parse component requirements
            var components = ParseComponentRequirements(inscription.ComponentRequirements);
            
            // Validate player has components
            var validation = await _craftingService.ValidateComponents(characterId, components);
            if (!validation.Success)
            {
                _logger.Information(
                    "Insufficient components for inscription {InscriptionId}: {Reason}",
                    inscriptionId, validation.Message);
                return ModificationResult.Failure(validation.Message);
            }
            
            // Check credit cost
            if (inscription.CraftingCostCredits > 0)
            {
                var hasCredits = await HasSufficientCredits(characterId, inscription.CraftingCostCredits);
                if (!hasCredits)
                {
                    return ModificationResult.Failure(
                        $"Insufficient credits: need {inscription.CraftingCostCredits}");
                }
            }
            
            // Consume components
            await _craftingService.ConsumeComponents(characterId, validation.PlayerComponents);
            
            // Deduct credits
            if (inscription.CraftingCostCredits > 0)
            {
                await DeductCredits(characterId, inscription.CraftingCostCredits);
            }
            
            // Apply modification
            var modificationId = await CreateModification(
                equipmentInventoryId,
                inscription);
            
            _logger.Information(
                "Applied {InscriptionName} to equipment {EquipmentId} (mod {ModificationId})",
                inscription.InscriptionName, equipmentInventoryId, modificationId);
            
            return ModificationResult.Success(
                modificationId,
                inscription.InscriptionName,
                inscription.IsTemporary ? inscription.UsesIfTemporary : null);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Failed to apply modification: character={CharacterId}, equipment={EquipmentId}",
                characterId, equipmentInventoryId);
            throw;
        }
    }
    
    /// <summary>
    /// Remove a modification from equipment.
    /// </summary>
    public async Task<bool> RemoveModification(
        int characterId,
        int modificationId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Remove modification: character={CharacterId}, modification={ModificationId}",
            characterId, modificationId);
        
        try
        {
            // Verify modification belongs to character's equipment
            var isValid = await ValidateModificationOwnership(characterId, modificationId);
            if (!isValid)
            {
                _logger.Warning(
                    "Modification {ModificationId} does not belong to character {CharacterId}",
                    modificationId, characterId);
                return false;
            }
            
            // Delete modification
            var deleted = await _db.ExecuteAsync(@"
                DELETE FROM Equipment_Modifications
                WHERE modification_id = @ModificationId",
                new { ModificationId = modificationId });
            
            if (deleted > 0)
            {
                _logger.Information(
                    "Removed modification {ModificationId} for character {CharacterId}",
                    modificationId, characterId);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Failed to remove modification: character={CharacterId}, modification={ModificationId}",
                characterId, modificationId);
            throw;
        }
    }
    
    /// <summary>
    /// Decrement uses on temporary modifications.
    /// Called after combat or equipment use.
    /// </summary>
    public async Task DecrementTemporaryModificationUses(
        int equipmentInventoryId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Decrement modification uses: equipment={EquipmentId}",
            equipmentInventoryId);
        
        try
        {
            // Get temporary modifications
            var tempMods = await _db.QueryAsync<EquipmentModification>(@"
                SELECT *
                FROM Equipment_Modifications
                WHERE equipment_item_id = @EquipmentId
                AND is_permanent = 0
                AND remaining_uses > 0",
                new { EquipmentId = equipmentInventoryId });
            
            foreach (var mod in tempMods)
            {
                int newUses = mod.RemainingUses.Value - 1;
                
                if (newUses <= 0)
                {
                    // Remove expired modification
                    await _db.ExecuteAsync(@"
                        DELETE FROM Equipment_Modifications
                        WHERE modification_id = @ModificationId",
                        new { ModificationId = mod.ModificationId });
                    
                    _logger.Information(
                        "Temporary modification {ModificationId} expired on equipment {EquipmentId}",
                        mod.ModificationId, equipmentInventoryId);
                }
                else
                {
                    // Decrement uses
                    await _db.ExecuteAsync(@"
                        UPDATE Equipment_Modifications
                        SET remaining_uses = @NewUses
                        WHERE modification_id = @ModificationId",
                        new { NewUses = newUses, ModificationId = mod.ModificationId });
                    
                    _logger.Debug(
                        "Decremented modification {ModificationId} to {Uses} uses remaining",
                        mod.ModificationId, newUses);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Failed to decrement modification uses: equipment={EquipmentId}",
                equipmentInventoryId);
            throw;
        }
    }
    
    /// <summary>
    /// Get all active modifications on equipment.
    /// </summary>
    public async Task<List<EquipmentModification>> GetActiveModifications(
        int equipmentInventoryId)
    {
        return (await _db.QueryAsync<EquipmentModification>(@"
            SELECT *
            FROM Equipment_Modifications
            WHERE equipment_item_id = @EquipmentId
            AND (is_permanent = 1 OR remaining_uses > 0)
            ORDER BY applied_at ASC",
            new { EquipmentId = equipmentInventoryId })).ToList();
    }
    
    /// <summary>
    /// Calculate total stat bonuses from modifications.
    /// </summary>
    public async Task<ModificationStats> CalculateModificationStats(
        int equipmentInventoryId)
    {
        var modifications = await GetActiveModifications(equipmentInventoryId);
        var stats = new ModificationStats();
        
        foreach (var mod in modifications)
        {
            var effectData = JsonSerializer.Deserialize<Dictionary<string, object>>(
                mod.ModificationValue);
            
            switch (mod.ModificationType)
            {
                case "Stat_Boost":
                    ApplyStatBoost(stats, effectData);
                    break;
                    
                case "Resistance":
                    ApplyResistance(stats, effectData);
                    break;
                    
                case "Elemental":
                    ApplyElementalEffect(stats, effectData);
                    break;
                    
                case "Status":
                    ApplyStatusEffect(stats, effectData);
                    break;
                    
                case "Special":
                    ApplySpecialEffect(stats, effectData);
                    break;
            }
        }
        
        return stats;
    }
    
    // Helper methods
    
    private async Task<RunicInscription> GetInscription(int inscriptionId)
    {
        return await _db.QuerySingleOrDefaultAsync<RunicInscription>(@"
            SELECT * FROM Runic_Inscriptions WHERE inscription_id = @InscriptionId",
            new { InscriptionId = inscriptionId });
    }
    
    private async Task<EquipmentItem> GetEquipmentItem(
        int characterId,
        int inventoryId)
    {
        return await _db.QuerySingleOrDefaultAsync<EquipmentItem>(@"
            SELECT ci.inventory_id, ci.item_id, i.item_name, i.item_type, i.quality_tier
            FROM Character_Inventory ci
            INNER JOIN Items i ON ci.item_id = i.item_id
            WHERE ci.character_id = @CharacterId
            AND ci.inventory_id = @InventoryId",
            new { CharacterId = characterId, InventoryId = inventoryId });
    }
    
    private bool ValidateEquipmentType(string equipmentType, string targetType)
    {
        if (targetType == "Both")
            return true;
        
        return equipmentType == targetType;
    }
    
    private List<RecipeComponent> ParseComponentRequirements(string json)
    {
        var requirements = JsonSerializer.Deserialize<List<ComponentRequirement>>(json);
        return [requirements.Select](http://requirements.Select)(r => new RecipeComponent
        {
            ComponentItemId = r.ItemId,
            QuantityRequired = r.Quantity,
            MinimumQuality = r.MinQuality
        }).ToList();
    }
    
    private async Task<bool> HasSufficientCredits(int characterId, int cost)
    {
        var credits = await _db.QuerySingleAsync<int>(@"
            SELECT credits FROM Characters WHERE character_id = @CharacterId",
            new { CharacterId = characterId });
        
        return credits >= cost;
    }
    
    private async Task DeductCredits(int characterId, int cost)
    {
        await _db.ExecuteAsync(@"
            UPDATE Characters
            SET credits = credits - @Cost
            WHERE character_id = @CharacterId",
            new { CharacterId = characterId, Cost = cost });
    }
    
    private async Task<int> CreateModification(
        int equipmentInventoryId,
        RunicInscription inscription)
    {
        return await _db.QuerySingleAsync<int>(@"
            INSERT INTO Equipment_Modifications (
                equipment_item_id,
                modification_type,
                modification_name,
                modification_value,
                is_permanent,
                remaining_uses,
                applied_by_recipe_id
            )
            VALUES (
                @EquipmentId,
                @ModificationType,
                @ModificationName,
                @ModificationValue,
                @IsPermanent,
                @RemainingUses,
                NULL
            );
            SELECT last_insert_rowid();",
            new
            {
                EquipmentId = equipmentInventoryId,
                ModificationType = inscription.EffectType,
                ModificationName = inscription.InscriptionName,
                ModificationValue = inscription.EffectValue,
                IsPermanent = !inscription.IsTemporary,
                RemainingUses = inscription.IsTemporary ? inscription.UsesIfTemporary : (int?)null
            });
    }
    
    private async Task<bool> ValidateModificationOwnership(
        int characterId,
        int modificationId)
    {
        var count = await _db.QuerySingleAsync<int>(@"
            SELECT COUNT(*)
            FROM Equipment_Modifications em
            INNER JOIN Character_Inventory ci ON [em.equipment](http://em.equipment)_item_id = ci.inventory_id
            WHERE em.modification_id = @ModificationId
            AND ci.character_id = @CharacterId",
            new { ModificationId = modificationId, CharacterId = characterId });
        
        return count > 0;
    }
    
    private void ApplyStatBoost(ModificationStats stats, Dictionary<string, object> effectData)
    {
        string stat = effectData["stat"].ToString();
        int value = Convert.ToInt32(effectData["value"]);
        
        switch (stat.ToLower())
        {
            case "damage":
                stats.BonusDamage += value;
                break;
            case "mitigation":
                stats.BonusMitigation += value;
                break;
            case "accuracy":
                stats.BonusAccuracy += value;
                break;
            case "evasion":
                stats.BonusEvasion += value;
                break;
        }
    }
    
    private void ApplyResistance(ModificationStats stats, Dictionary<string, object> effectData)
    {
        string resistanceType = effectData["resistance_type"].ToString();
        int value = Convert.ToInt32(effectData["value"]);
        
        if (!stats.Resistances.ContainsKey(resistanceType))
            stats.Resistances[resistanceType] = 0;
        
        stats.Resistances[resistanceType] += value;
    }
    
    private void ApplyElementalEffect(ModificationStats stats, Dictionary<string, object> effectData)
    {
        string element = effectData["element"].ToString();
        int bonusDamage = Convert.ToInt32(effectData["bonus_damage"]);
        
        stats.ElementalDamage.Add(new ElementalEffect
        {
            Element = element,
            BonusDamage = bonusDamage,
            ApplicationChance = effectData.ContainsKey("burn_chance") 
                ? Convert.ToDouble(effectData["burn_chance"]) 
                : 0.0
        });
    }
    
    private void ApplyStatusEffect(ModificationStats stats, Dictionary<string, object> effectData)
    {
        stats.StatusEffects.Add(new StatusEffect
        {
            StatusName = effectData["status"].ToString(),
            ApplicationChance = Convert.ToDouble(effectData["application_chance"]),
            Duration = Convert.ToInt32(effectData["duration"])
        });
    }
    
    private void ApplySpecialEffect(ModificationStats stats, Dictionary<string, object> effectData)
    {
        string effectType = effectData["effect"].ToString();
        
        switch (effectType.ToLower())
        {
            case "regeneration":
                stats.RegenerationPerTurn = Convert.ToInt32(effectData["hp_per_turn"]);
                break;
            // Add more special effects as needed
        }
    }
}
```

### Supporting Classes

```csharp
public class RunicInscription
{
    public int InscriptionId { get; set; }
    public string InscriptionName { get; set; }
    public int InscriptionTier { get; set; }
    public string TargetEquipmentType { get; set; }
    public string EffectType { get; set; }
    public string EffectValue { get; set; }
    public bool IsTemporary { get; set; }
    public int UsesIfTemporary { get; set; }
    public string ComponentRequirements { get; set; }
    public int CraftingCostCredits { get; set; }
    public string InscriptionDescription { get; set; }
}

public class EquipmentModification
{
    public int ModificationId { get; set; }
    public int EquipmentItemId { get; set; }
    public string ModificationType { get; set; }
    public string ModificationName { get; set; }
    public string ModificationValue { get; set; }
    public bool IsPermanent { get; set; }
    public int? RemainingUses { get; set; }
    public DateTime AppliedAt { get; set; }
    public int? AppliedByRecipeId { get; set; }
}

public class EquipmentItem
{
    public int InventoryId { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string ItemType { get; set; }
    public int QualityTier { get; set; }
}

public class ComponentRequirement
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public int MinQuality { get; set; }
}

public class ModificationStats
{
    public int BonusDamage { get; set; }
    public int BonusMitigation { get; set; }
    public int BonusAccuracy { get; set; }
    public int BonusEvasion { get; set; }
    public Dictionary<string, int> Resistances { get; set; } = new();
    public List<ElementalEffect> ElementalDamage { get; set; } = new();
    public List<StatusEffect> StatusEffects { get; set; } = new();
    public int RegenerationPerTurn { get; set; }
}

public class ElementalEffect
{
    public string Element { get; set; }
    public int BonusDamage { get; set; }
    public double ApplicationChance { get; set; }
}

public class StatusEffect
{
    public string StatusName { get; set; }
    public double ApplicationChance { get; set; }
    public int Duration { get; set; }
}

public class ModificationResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int? ModificationId { get; set; }
    public string ModificationName { get; set; }
    public int? RemainingUses { get; set; }
    
    public static ModificationResult Failure(string message) =>
        new ModificationResult { Success = false, Message = message };
    
    public static ModificationResult Success(
        int modificationId,
        string name,
        int? remainingUses) =>
        new ModificationResult
        {
            Success = true,
            ModificationId = modificationId,
            ModificationName = name,
            RemainingUses = remainingUses,
            Message = remainingUses.HasValue
                ? $"Applied {name} ({remainingUses} uses)"
                : $"Applied {name} (permanent)"
        };
}
```

---

## IV. Modification Examples

### Example 1: Apply Temporary Flame Rune

**Inscription:** Rune of Flame (Tier 3, temporary, 10 uses)

**Equipment:** Plasma Rifle (Tier 3)

**Components Required:**

- 1x Minor Aetheric Shard (Tier 3)
- 2x Stabilizing Compound (Tier 2)

**Credit Cost:** 150 credits

**Effect:**

```json
{
  "element": "Fire",
  "bonus_damage": 5,
  "burn_chance": 0.15
}
```

**Result:**

- Plasma Rifle gains +5 fire damage
- 15% chance to apply Burn status
- Lasts for 10 uses (decrements after each combat)

---

### Example 2: Apply Permanent Sharpness Rune

**Inscription:** Rune of Sharpness (Tier 4, permanent)

**Equipment:** Monomolecular Sword (Tier 4)

**Components Required:**

- 1x Aetheric Shard (Tier 4)
- 2x Binding Catalyst (Tier 3)

**Credit Cost:** 500 credits

**Effect:**

```json
{
  "stat": "damage",
  "value": 10
}
```

**Result:**

- Monomolecular Sword gains +10 damage
- Effect is permanent (never expires)

---

### Example 3: Apply Fire Resistance Rune to Armor

**Inscription:** Rune of Fire Resistance (Tier 3, temporary, 10 uses)

**Equipment:** Reinforced Chest Plate (Tier 3)

**Components Required:**

- 1x Minor Aetheric Shard (Tier 3)
- 2x Stabilizing Compound (Tier 2)

**Credit Cost:** 150 credits

**Effect:**

```json
{
  "resistance_type": "Fire",
  "value": 15
}
```

**Result:**

- Reinforced Chest Plate gains +15% fire resistance
- Lasts for 10 uses (decrements after each combat)

---

### Example 4: Modification Slot Limit

**Scenario:** Player tries to apply 4th modification

**Equipment:** Plasma Rifle with 3 existing modifications:

1. Rune of Flame (+5 fire damage)
2. Rune of Sharpness (+10 damage)
3. Rune of Bleeding (25% bleed chance)

**Attempted:** Rune of Frost

**Result:** ❌ Failure - "Equipment already has maximum 3 modifications"

**Solution:** Player must remove one modification before applying another

---

## V. Temporary Modification Lifecycle

### Use Tracking

```csharp
// After combat, decrement uses on all equipped items
public async Task PostCombatEquipmentMaintenance(int characterId)
{
    var equippedItems = await GetEquippedItems(characterId);
    
    foreach (var item in equippedItems)
    {
        await _modificationService.DecrementTemporaryModificationUses(
            item.InventoryId);
    }
}
```

### Expiration Handling

**Scenario Timeline:**

**Initial State:**

- Plasma Rifle has Rune of Flame (10 uses remaining)

**After Combat 1:** 9 uses remaining

**After Combat 2:** 8 uses remaining

...

**After Combat 9:** 1 use remaining

**After Combat 10:** 0 uses → **Modification removed automatically**

**Player Notification:**

```
"Your Rune of Flame has expired and been removed from Plasma Rifle."
```

---

## VI. Integration with Combat System

### Stat Calculation Integration

```csharp
public async Task<CombatStats> CalculateFinalStats(
    int characterId,
    int weaponInventoryId)
{
    // Get base weapon stats
    var baseStats = await GetBaseWeaponStats(weaponInventoryId);
    
    // Get modification bonuses
    var modStats = await _modificationService.CalculateModificationStats(
        weaponInventoryId);
    
    // Combine stats
    var finalStats = new CombatStats
    {
        Damage = baseStats.Damage + modStats.BonusDamage,
        Accuracy = baseStats.Accuracy + modStats.BonusAccuracy,
        ElementalEffects = modStats.ElementalDamage,
        StatusEffects = modStats.StatusEffects
    };
    
    return finalStats;
}
```

### Elemental Damage Application

```csharp
public async Task<DamageResult> ApplyWeaponDamage(
    int attackerId,
    int targetId,
    int weaponInventoryId)
{
    var stats = await CalculateFinalStats(attackerId, weaponInventoryId);
    
    // Base damage
    int totalDamage = stats.Damage;
    
    // Apply elemental damage
    foreach (var elemental in stats.ElementalEffects)
    {
        totalDamage += elemental.BonusDamage;
        
        // Check for status application (burn, freeze, etc.)
        if (Random.NextDouble() < elemental.ApplicationChance)
        {
            await ApplyStatus(targetId, elemental.Element, 3); // 3 turn duration
        }
    }
    
    // Apply status effects
    foreach (var status in stats.StatusEffects)
    {
        if (Random.NextDouble() < status.ApplicationChance)
        {
            await ApplyStatus(targetId, status.StatusName, status.Duration);
        }
    }
    
    return new DamageResult { TotalDamage = totalDamage };
}
```

---

## VII. Unit Tests

### Test Suite (10+ tests, 85%+ coverage)

```csharp
[TestClass]
public class ModificationServiceTests
{
    private ModificationService _modificationService;
    private Mock<IDbConnection> _mockDb;
    private Mock<CraftingService> _mockCraftingService;
    
    [TestInitialize]
    public void Setup()
    {
        _mockDb = new Mock<IDbConnection>();
        var mockLogger = new Mock<ILogger<ModificationService>>();
        _mockCraftingService = new Mock<CraftingService>();
        
        _modificationService = new ModificationService(
            _mockDb.Object,
            mockLogger.Object,
            _mockCraftingService.Object);
    }
    
    [TestMethod]
    public async Task ApplyModification_ValidInscription_Success()
    {
        // Arrange: Player has components and credits
        int characterId = 1;
        int equipmentId = 100;
        int inscriptionId = 8001; // Rune of Flame
        
        // Act
        var result = await _modificationService.ApplyModification(
            characterId, equipmentId, inscriptionId);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.ModificationId);
        Assert.AreEqual(10, result.RemainingUses); // Temporary rune with 10 uses
    }
    
    [TestMethod]
    public async Task ApplyModification_MaxSlotsFull_Fails()
    {
        // Arrange: Equipment already has 3 modifications
        int characterId = 1;
        int equipmentId = 100;
        int inscriptionId = 8001;
        
        // Mock: GetActiveModifications returns 3 mods
        
        // Act
        var result = await _modificationService.ApplyModification(
            characterId, equipmentId, inscriptionId);
        
        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("maximum 3"));
    }
    
    [TestMethod]
    public async Task ApplyModification_WrongEquipmentType_Fails()
    {
        // Arrange: Weapon inscription on armor
        int characterId = 1;
        int equipmentId = 100; // Chest Plate (armor)
        int inscriptionId = 8001; // Rune of Flame (weapon only)
        
        // Act
        var result = await _modificationService.ApplyModification(
            characterId, equipmentId, inscriptionId);
        
        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("can only be applied to"));
    }
    
    [TestMethod]
    public async Task ApplyModification_InsufficientComponents_Fails()
    {
        // Arrange: Player missing required components
        int characterId = 1;
        int equipmentId = 100;
        int inscriptionId = 8001;
        
        // Mock: ValidateComponents returns failure
        
        // Act
        var result = await _modificationService.ApplyModification(
            characterId, equipmentId, inscriptionId);
        
        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Insufficient"));
    }
    
    [TestMethod]
    public async Task ApplyModification_InsufficientCredits_Fails()
    {
        // Arrange: Player has components but not enough credits
        int characterId = 1;
        int equipmentId = 100;
        int inscriptionId = 8001; // Costs 150 credits
        
        // Mock: Character has 100 credits
        
        // Act
        var result = await _modificationService.ApplyModification(
            characterId, equipmentId, inscriptionId);
        
        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Insufficient credits"));
    }
    
    [TestMethod]
    public async Task RemoveModification_ValidOwnership_Success()
    {
        // Arrange: Modification belongs to character
        int characterId = 1;
        int modificationId = 1000;
        
        // Act
        var result = await _modificationService.RemoveModification(
            characterId, modificationId);
        
        // Assert
        Assert.IsTrue(result);
    }
    
    [TestMethod]
    public async Task RemoveModification_NotOwned_Fails()
    {
        // Arrange: Modification belongs to different character
        int characterId = 1;
        int modificationId = 1000; // Owned by character 2
        
        // Act
        var result = await _modificationService.RemoveModification(
            characterId, modificationId);
        
        // Assert
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    public async Task DecrementUses_TemporaryMod_DecrementsCorrectly()
    {
        // Arrange: Equipment has temporary mod with 3 uses
        int equipmentId = 100;
        
        // Act
        await _modificationService.DecrementTemporaryModificationUses(equipmentId);
        
        // Assert: Remaining uses now 2
    }
    
    [TestMethod]
    public async Task DecrementUses_ReachesZero_RemovesMod()
    {
        // Arrange: Equipment has temporary mod with 1 use
        int equipmentId = 100;
        
        // Act
        await _modificationService.DecrementTemporaryModificationUses(equipmentId);
        
        // Assert: Modification deleted from database
    }
    
    [TestMethod]
    public async Task CalculateModificationStats_MultipleMods_CombinesCorrectly()
    {
        // Arrange: Equipment has multiple modifications
        int equipmentId = 100;
        // Mod 1: +10 damage
        // Mod 2: +5 fire damage with burn chance
        // Mod 3: +15% fire resistance
        
        // Act
        var stats = await _modificationService.CalculateModificationStats(equipmentId);
        
        // Assert
        Assert.AreEqual(10, stats.BonusDamage);
        Assert.AreEqual(1, stats.ElementalDamage.Count);
        Assert.AreEqual(5, stats.ElementalDamage[0].BonusDamage);
        Assert.AreEqual(15, stats.Resistances["Fire"]);
    }
    
    [TestMethod]
    public async Task GetActiveModifications_ExcludesExpired_Success()
    {
        // Arrange: Equipment has 2 mods: 1 permanent, 1 expired (0 uses)
        int equipmentId = 100;
        
        // Act
        var mods = await _modificationService.GetActiveModifications(equipmentId);
        
        // Assert
        Assert.AreEqual(1, mods.Count); // Only permanent mod returned
    }
}
```

---

## VIII. Deployment Instructions

### Step 1: Add ModificationService to DI Container

```csharp
// Startup.cs or Program.cs
services.AddScoped<ModificationService>();
```

### Step 2: Integration Testing

```bash
# Run unit tests
dotnet test --filter "FullyQualifiedName~ModificationServiceTests"

# Expected: 10+ tests, 85%+ coverage
```

### Step 3: Manual Verification

```sql
-- 1. Give player equipment
INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier)
VALUES (1, 2001, 1, 3); -- Tier 3 weapon

-- 2. Give player inscription components
INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier)
VALUES (1, 9001, 1, 3), (1, 9020, 2, 2);

-- 3. Give player credits
UPDATE Characters SET credits = 500 WHERE character_id = 1;

-- 4. Apply modification (via game code)
-- modificationService.ApplyModification(characterId: 1, equipmentId: 100, inscriptionId: 8001);

-- 5. Verify modification applied
SELECT * FROM Equipment_Modifications WHERE equipment_item_id = 100;

-- 6. Verify components consumed
SELECT * FROM Character_Inventory WHERE character_id = 1 AND item_id IN (9001, 9020);

-- 7. Verify credits deducted
SELECT credits FROM Characters WHERE character_id = 1;
```

---

## IX. Success Criteria

**Functional Requirements:**

- ✅ Players can apply inscriptions to equipment
- ✅ Modifications respect slot limits (max 3 per item)
- ✅ Equipment type validation (weapon runes on weapons only)
- ✅ Temporary modifications track remaining uses
- ✅ Expired modifications auto-removed
- ✅ Permanent modifications persist forever
- ✅ Stat bonuses calculated correctly

**Quality Gates:**

- ✅ 10+ unit tests implemented
- ✅ 85%+ code coverage
- ✅ Serilog structured logging throughout
- ✅ v5.0 compliance (Layer 2 voice)
- ✅ ASCII-only service/method names

**Performance:**

- ✅ Modification application completes in <300ms
- ✅ Stat calculation O(n) where n = modification count
- ✅ Use decrement batch operation for multiple items

---

## X. v5.0 Compliance Notes

### Layer 2 Voice (Diagnostic/Clinical)

**✅ Correct Terminology:**

- "Runic inscriptions" (data-weaving instruction sets)
- "Equipment modification subroutines"
- "Stat adjustment protocols"
- "Aetheric data fragments"

**❌ Incorrect Terminology:**

- ~~"Magical enchantments"~~
- ~~"Divine blessings"~~
- ~~"Mystical augmentation"~~
- ~~"Arcane empowerment"~~

---

**Status:** Implementation-ready modification & inscription systems complete.

**Next:** v0.36.4 (Service Integration & UI)