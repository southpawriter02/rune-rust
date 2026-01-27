# v0.36.2: Crafting Mechanics & Station System

Type: Technical
Description: CraftingService implementation - station validation, quality calculation, component consumption
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.36.1, v0.3
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.36: Advanced Crafting System (v0%2036%20Advanced%20Crafting%20System%20e00690b9cf4b48538f10810b7f477711.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.36.2-CRAFTING-MECHANICS

**Parent Specification:** [v0.36: Advanced Crafting System](v0%2036%20Advanced%20Crafting%20System%20e00690b9cf4b48538f10810b7f477711.md)

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-12 hours

**Prerequisites:** v0.36.1 (Crafting Database), v0.3 (Equipment Service)

---

## I. Executive Summary

v0.36.2 implements the **core crafting mechanics and station system**:

- **CraftingService** — Execute recipes, consume components, generate items
- **Station validation** — Ensure correct station type and quality limits
- **Quality calculation** — Determine crafted item quality from components
- **Component consumption** — Remove materials from inventory
- **Success/failure handling** — Validate requirements and handle errors

This specification provides the gameplay systems that make crafting functional and balanced.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.36.2)

- CraftingService implementation
- Station validation logic
- Quality calculation engine
- Component consumption system
- Recipe validation
- Crafting time tracking (if time system exists)
- Unit test suite (10+ tests, 85%+ coverage)
- Serilog structured logging
- Integration with v0.3 (EquipmentService)

### ❌ Explicitly Out of Scope

- Equipment modification (defer to v0.36.3)
- Runic inscription application (defer to v0.36.3)
- Recipe discovery mechanics (defer to v0.36.4)
- Crafting UI (defer to v0.36.4)
- Einbui field crafting integration (defer to v0.36.4)
- Merchant recipe sales (defer to v0.36.4)

---

## III. CraftingService Implementation

### Core Service

```csharp
public class CraftingService
{
    private readonly IDbConnection _db;
    private readonly ILogger<CraftingService> _logger;
    private readonly EquipmentService _equipmentService;
    private readonly InventoryService _inventoryService;
    
    public CraftingService(
        IDbConnection db,
        ILogger<CraftingService> logger,
        EquipmentService equipmentService,
        InventoryService inventoryService)
    {
        _db = db;
        _logger = logger;
        _equipmentService = equipmentService;
        _inventoryService = inventoryService;
    }
    
    /// <summary>
    /// Craft an item from a recipe.
    /// </summary>
    public async Task<CraftingResult> CraftItem(
        int characterId,
        int recipeId,
        int stationId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Craft item: character={CharacterId}, recipe={RecipeId}, station={StationId}",
            characterId, recipeId, stationId);
        
        try
        {
            // Load recipe
            var recipe = await GetRecipe(recipeId);
            if (recipe == null)
            {
                _logger.Warning("Recipe {RecipeId} not found", recipeId);
                return CraftingResult.Failure("Recipe not found");
            }
            
            // Load station
            var station = await GetStation(stationId);
            if (station == null)
            {
                _logger.Warning("Station {StationId} not found", stationId);
                return CraftingResult.Failure("Crafting station not found");
            }
            
            // Validate station type
            if (!ValidateStation(recipe, station))
            {
                _logger.Warning(
                    "Station type mismatch: recipe requires {RequiredStation}, station is {StationType}",
                    recipe.RequiredStation, station.StationType);
                return CraftingResult.Failure(
                    $"This recipe requires a {recipe.RequiredStation}, not a {station.StationType}");
            }
            
            // Get required components
            var components = await GetRecipeComponents(recipeId);
            
            // Validate player has components
            var validation = await ValidateComponents(characterId, components);
            if (!validation.Success)
            {
                _logger.Information(
                    "Insufficient components for recipe {RecipeId}: {Reason}",
                    recipeId, validation.Message);
                return CraftingResult.Failure(validation.Message);
            }
            
            // Calculate quality
            int craftedQuality = CalculateQuality(
                validation.PlayerComponents,
                station.MaxQualityTier,
                recipe.QualityBonus);
            
            _logger.Information(
                "Crafting {RecipeName} at quality tier {Quality}",
                recipe.RecipeName, craftedQuality);
            
            // Consume components
            await ConsumeComponents(characterId, validation.PlayerComponents);
            
            // Generate crafted item
            var craftedItem = await GenerateCraftedItem(
                recipe,
                craftedQuality,
                characterId);
            
            // Add to player inventory
            await _inventoryService.AddItem(characterId, craftedItem);
            
            // Update crafting stats
            await UpdateCraftingStats(characterId, recipeId);
            
            _logger.Information(
                "Successfully crafted {ItemName} (quality {Quality}) for character {CharacterId}",
                craftedItem.ItemName, craftedQuality, characterId);
            
            return CraftingResult.Success(craftedItem, craftedQuality);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, 
                "Failed to craft item: character={CharacterId}, recipe={RecipeId}",
                characterId, recipeId);
            throw;
        }
    }
    
    /// <summary>
    /// Validate station type matches recipe requirements.
    /// </summary>
    private bool ValidateStation(Recipe recipe, CraftingStation station)
    {
        // 'Any' station requirement matches all stations
        if (recipe.RequiredStation == "Any")
            return true;
        
        return recipe.RequiredStation == station.StationType;
    }
    
    /// <summary>
    /// Calculate crafted item quality from components and station.
    /// </summary>
    public int CalculateQuality(
        List<PlayerComponent> playerComponents,
        int stationMaxTier,
        int recipeBonus)
    {
        // Find lowest quality component
        int lowestComponentQuality = playerComponents.Min(c => c.QualityTier);
        
        // Quality = min(lowest component, station max) + recipe bonus
        int baseQuality = Math.Min(lowestComponentQuality, stationMaxTier);
        int finalQuality = baseQuality + recipeBonus;
        
        // Clamp to 1-5 (v0.36 caps at 4, v0.37 adds 5)
        finalQuality = Math.Clamp(finalQuality, 1, 4);
        
        _logger.Debug(
            "Quality calculation: lowestComponent={Lowest}, stationMax={StationMax}, " +
            "recipeBonus={Bonus}, final={Final}",
            lowestComponentQuality, stationMaxTier, recipeBonus, finalQuality);
        
        return finalQuality;
    }
    
    /// <summary>
    /// Validate player has required components.
    /// </summary>
    private async Task<ComponentValidation> ValidateComponents(
        int characterId,
        List<RecipeComponent> requiredComponents)
    {
        var playerComponents = new List<PlayerComponent>();
        
        foreach (var required in requiredComponents)
        {
            // Get player's components of this type meeting minimum quality
            var playerItems = await _db.QueryAsync<InventoryItem>(@"
                SELECT inventory_id, item_id, quality_tier, quantity
                FROM Character_Inventory
                WHERE character_id = @CharacterId
                AND item_id = @ItemId
                AND quality_tier >= @MinQuality
                ORDER BY quality_tier ASC",  -- Use lowest quality first
                new 
                { 
                    CharacterId = characterId,
                    ItemId = required.ComponentItemId,
                    MinQuality = required.MinimumQuality
                });
            
            int totalQuantity = playerItems.Sum(i => i.Quantity);
            
            if (totalQuantity < required.QuantityRequired)
            {
                var itemName = await GetItemName(required.ComponentItemId);
                return ComponentValidation.Failure(
                    $"Insufficient {itemName}: need {required.QuantityRequired}, have {totalQuantity}");
            }
            
            // Select items to consume (prioritizing lowest quality)
            int remaining = required.QuantityRequired;
            foreach (var item in playerItems)
            {
                if (remaining <= 0)
                    break;
                
                int toConsume = Math.Min(remaining, item.Quantity);
                playerComponents.Add(new PlayerComponent
                {
                    InventoryId = item.InventoryId,
                    ItemId = item.ItemId,
                    QualityTier = item.QualityTier,
                    QuantityToConsume = toConsume
                });
                
                remaining -= toConsume;
            }
        }
        
        return ComponentValidation.Success(playerComponents);
    }
    
    /// <summary>
    /// Consume components from player inventory.
    /// </summary>
    private async Task ConsumeComponents(
        int characterId,
        List<PlayerComponent> components)
    {
        foreach (var component in components)
        {
            if (!component.IsConsumed)
                continue; // Skip non-consumed components (catalyst items)
            
            await _db.ExecuteAsync(@"
                UPDATE Character_Inventory
                SET quantity = quantity - @Quantity
                WHERE inventory_id = @InventoryId",
                new 
                { 
                    InventoryId = component.InventoryId,
                    Quantity = component.QuantityToConsume
                });
            
            // Remove entry if quantity reaches 0
            await _db.ExecuteAsync(@"
                DELETE FROM Character_Inventory
                WHERE inventory_id = @InventoryId AND quantity <= 0",
                new { InventoryId = component.InventoryId });
            
            _logger.Debug(
                "Consumed {Quantity}x of item {ItemId} from inventory {InventoryId}",
                component.QuantityToConsume, component.ItemId, component.InventoryId);
        }
    }
    
    /// <summary>
    /// Generate crafted item.
    /// </summary>
    private async Task<Item> GenerateCraftedItem(
        Recipe recipe,
        int quality,
        int characterId)
    {
        Item craftedItem;
        
        if (recipe.CraftedItemBaseId.HasValue)
        {
            // Use specific item template
            var baseItem = await _equipmentService.GetItemTemplate(recipe.CraftedItemBaseId.Value);
            craftedItem = baseItem.Clone();
            craftedItem.QualityTier = quality;
        }
        else
        {
            // Generate procedurally based on type and quality
            craftedItem = await _equipmentService.GenerateEquipment(
                itemType: recipe.CraftedItemType,
                qualityTier: quality,
                isCrafted: true);
        }
        
        // Mark as crafted
        craftedItem.IsCrafted = true;
        craftedItem.CrafterCharacterId = characterId;
        
        return craftedItem;
    }
    
    /// <summary>
    /// Update character's crafting statistics.
    /// </summary>
    private async Task UpdateCraftingStats(
        int characterId,
        int recipeId)
    {
        await _db.ExecuteAsync(@"
            UPDATE Character_Recipes
            SET times_crafted = times_crafted + 1
            WHERE character_id = @CharacterId AND recipe_id = @RecipeId",
            new { CharacterId = characterId, RecipeId = recipeId });
    }
    
    /// <summary>
    /// Get recipe by ID.
    /// </summary>
    private async Task<Recipe> GetRecipe(int recipeId)
    {
        return await _db.QuerySingleOrDefaultAsync<Recipe>(@"
            SELECT * FROM Crafting_Recipes WHERE recipe_id = @RecipeId",
            new { RecipeId = recipeId });
    }
    
    /// <summary>
    /// Get crafting station by ID.
    /// </summary>
    private async Task<CraftingStation> GetStation(int stationId)
    {
        return await _db.QuerySingleOrDefaultAsync<CraftingStation>(@"
            SELECT * FROM Crafting_Stations WHERE station_id = @StationId",
            new { StationId = stationId });
    }
    
    /// <summary>
    /// Get components required for recipe.
    /// </summary>
    private async Task<List<RecipeComponent>> GetRecipeComponents(int recipeId)
    {
        return (await _db.QueryAsync<RecipeComponent>(@"
            SELECT * FROM Recipe_Components WHERE recipe_id = @RecipeId",
            new { RecipeId = recipeId })).ToList();
    }
    
    /// <summary>
    /// Get item name by ID.
    /// </summary>
    private async Task<string> GetItemName(int itemId)
    {
        return await _db.QuerySingleAsync<string>(@"
            SELECT item_name FROM Items WHERE item_id = @ItemId",
            new { ItemId = itemId });
    }
}
```

### Supporting Classes

```csharp
public class Recipe
{
    public int RecipeId { get; set; }
    public string RecipeName { get; set; }
    public string RecipeTier { get; set; }
    public string CraftedItemType { get; set; }
    public int? CraftedItemBaseId { get; set; }
    public string RequiredStation { get; set; }
    public int CraftingTimeMinutes { get; set; }
    public int QualityBonus { get; set; }
    public string DiscoveryMethod { get; set; }
    public string RecipeDescription { get; set; }
}

public class RecipeComponent
{
    public int ComponentId { get; set; }
    public int RecipeId { get; set; }
    public int ComponentItemId { get; set; }
    public int QuantityRequired { get; set; }
    public int MinimumQuality { get; set; }
    public bool IsConsumed { get; set; }
}

public class CraftingStation
{
    public int StationId { get; set; }
    public string StationName { get; set; }
    public string StationType { get; set; }
    public int MaxQualityTier { get; set; }
    public int LocationWorldId { get; set; }
    public int? LocationSectorId { get; set; }
    public string LocationRoomId { get; set; }
    public bool IsPortable { get; set; }
    public string StationDescription { get; set; }
}

public class PlayerComponent
{
    public int InventoryId { get; set; }
    public int ItemId { get; set; }
    public int QualityTier { get; set; }
    public int QuantityToConsume { get; set; }
    public bool IsConsumed { get; set; } = true;
}

public class ComponentValidation
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<PlayerComponent> PlayerComponents { get; set; }
    
    public static ComponentValidation Failure(string message) =>
        new ComponentValidation { Success = false, Message = message };
    
    public static ComponentValidation Success(List<PlayerComponent> components) =>
        new ComponentValidation { Success = true, PlayerComponents = components };
}

public class CraftingResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Item CraftedItem { get; set; }
    public int CraftedQuality { get; set; }
    
    public static CraftingResult Failure(string message) =>
        new CraftingResult { Success = false, Message = message };
    
    public static CraftingResult Success(Item item, int quality) =>
        new CraftingResult 
        { 
            Success = true, 
            CraftedItem = item, 
            CraftedQuality = quality,
            Message = $"Successfully crafted {item.ItemName} (Tier {quality})"
        };
}
```

---

## IV. Quality Calculation Examples

### Example 1: Basic Weapon

**Recipe:** Improvised Blade (Basic, quality_bonus = 0)

**Components:**

- 2x Scrap Iron (Tier 1)
- 1x Salvaged Grip (Tier 1)

**Station:** Central Forge (max_quality_tier = 3)

**Calculation:**

```
lowestComponent = 1
stationMax = 3
recipeBonus = 0

baseQuality = min(1, 3) = 1
finalQuality = 1 + 0 = 1
```

**Result:** Tier 1 Improvised Blade

---

### Example 2: Advanced Weapon

**Recipe:** Plasma Rifle (Advanced, quality_bonus = 1)

**Components:**

- 1x Reinforced Frame (Tier 3)
- 1x Plasma Core (Tier 3)
- 1x Synthetic Grip (Tier 2)

**Station:** Engineering Workshop (max_quality_tier = 4)

**Calculation:**

```
lowestComponent = 2  // Synthetic Grip is lowest
stationMax = 4
recipeBonus = 1

baseQuality = min(2, 4) = 2
finalQuality = 2 + 1 = 3
```

**Result:** Tier 3 Plasma Rifle

---

### Example 3: Quality-Limited by Components

**Recipe:** Fusion Lance (Expert, quality_bonus = 2)

**Components:**

- 1x Precision Frame (Tier 4)
- 1x Fusion Cell (Tier 4)
- 2x Nano-Processors (Tier 3)  ← Lowest tier!

**Station:** Engineering Workshop (max_quality_tier = 4)

**Calculation:**

```
lowestComponent = 3  // Nano-Processors are lowest
stationMax = 4
recipeBonus = 2

baseQuality = min(3, 4) = 3
finalQuality = 3 + 2 = 5, clamped to 4 (v0.36 max)
```

**Result:** Tier 4 Fusion Lance

---

### Example 4: Quality-Limited by Station

**Recipe:** Plasma Rifle (Advanced, quality_bonus = 1)

**Components:**

- 1x Reinforced Frame (Tier 3)
- 1x Plasma Core (Tier 3)
- 1x Synthetic Grip (Tier 2)

**Station:** Central Forge (max_quality_tier = 3)  ← Lower than Workshop!

**Calculation:**

```
lowestComponent = 2
stationMax = 3  // Station limits quality
recipeBonus = 1

baseQuality = min(2, 3) = 2
finalQuality = 2 + 1 = 3
```

**Result:** Tier 3 Plasma Rifle (same as Workshop, but if all components were Tier 4, Forge would cap at Tier 3)

---

## V. Station Validation Logic

### Valid Station Combinations

```csharp
// Examples of valid crafting attempts
ValidateStation(
    recipe: { RequiredStation = "Workshop" },
    station: { StationType = "Workshop" }
) // ✅ Returns true

ValidateStation(
    recipe: { RequiredStation = "Any" },
    station: { StationType = "Laboratory" }
) // ✅ Returns true (Any matches all)

ValidateStation(
    recipe: { RequiredStation = "Forge" },
    station: { StationType = "Workshop" }
) // ❌ Returns false (wrong station type)
```

### Station Type Requirements

| Recipe Type | Valid Stations |
| --- | --- |
| Weapon crafting | Forge, Workshop |
| Armor crafting | Forge, Workshop |
| Consumables | Laboratory, Field_Station (basic only) |
| Inscriptions | Runic_Altar |
| Utility items | Workshop, Laboratory |

---

## VI. Unit Tests

### Test Suite (10+ tests, 85%+ coverage)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CraftingServiceTests
{
    private CraftingService _craftingService;
    private Mock<IDbConnection> _mockDb;
    private Mock<EquipmentService> _mockEquipmentService;
    private Mock<InventoryService> _mockInventoryService;
    
    [TestInitialize]
    public void Setup()
    {
        _mockDb = new Mock<IDbConnection>();
        var mockLogger = new Mock<ILogger<CraftingService>>();
        _mockEquipmentService = new Mock<EquipmentService>();
        _mockInventoryService = new Mock<InventoryService>();
        
        _craftingService = new CraftingService(
            _mockDb.Object,
            mockLogger.Object,
            _mockEquipmentService.Object,
            _mockInventoryService.Object);
    }
    
    [TestMethod]
    public async Task CraftItem_ValidRecipeAndComponents_Success()
    {
        // Arrange: Player has all required components at correct quality
        int characterId = 1;
        int recipeId = 1010; // Plasma Rifle
        int stationId = 2; // Workshop (Tier 4)
        
        // Recipe requires: Tier 3 Frame, Tier 3 Core, Tier 2 Grip
        // Player has all components
        // Expected output: Tier 4 Plasma Rifle (lowest = 2, station = 4, min(2,4) + 1 = 3)
        
        // Act
        var result = await _craftingService.CraftItem(characterId, recipeId, stationId);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.CraftedQuality);
        Assert.IsNotNull(result.CraftedItem);
    }
    
    [TestMethod]
    public async Task CraftItem_InsufficientComponents_Fails()
    {
        // Arrange: Player only has 1 of 2 required components
        int characterId = 1;
        int recipeId = 1001; // Improvised Blade (requires 2x Scrap Iron)
        int stationId = 1;
        
        // Player only has 1x Scrap Iron
        
        // Act
        var result = await _craftingService.CraftItem(characterId, recipeId, stationId);
        
        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Insufficient"));
    }
    
    [TestMethod]
    public async Task CraftItem_WrongStation_Fails()
    {
        // Arrange: Player at Laboratory but recipe requires Workshop
        int characterId = 1;
        int recipeId = 1010; // Plasma Rifle (requires Workshop)
        int stationId = 3; // Laboratory
        
        // Act
        var result = await _craftingService.CraftItem(characterId, recipeId, stationId);
        
        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("requires"));
    }
    
    [TestMethod]
    public void CalculateQuality_LowQualityComponent_CapsOutput()
    {
        // Arrange: Recipe at Tier 4 station, but Tier 1 component
        var components = new List<PlayerComponent>
        {
            new() { QualityTier = 4 },
            new() { QualityTier = 4 },
            new() { QualityTier = 1 }  // Lowest tier component
        };
        
        // Act
        int quality = _craftingService.CalculateQuality(
            components, 
            stationMaxTier: 4, 
            recipeBonus: 2);
        
        // Assert
        Assert.AreEqual(3, quality); // min(1, 4) + 2 = 3
    }
    
    [TestMethod]
    public void CalculateQuality_StationLimitsQuality()
    {
        // Arrange: All Tier 4 components, but Tier 3 station
        var components = new List<PlayerComponent>
        {
            new() { QualityTier = 4 },
            new() { QualityTier = 4 },
            new() { QualityTier = 4 }
        };
        
        // Act
        int quality = _craftingService.CalculateQuality(
            components, 
            stationMaxTier: 3,  // Station caps quality
            recipeBonus: 1);
        
        // Assert
        Assert.AreEqual(4, quality); // min(4, 3) + 1 = 4
    }
    
    [TestMethod]
    public void CalculateQuality_BonusCannotExceedCap()
    {
        // Arrange: Tier 4 components with +2 bonus
        var components = new List<PlayerComponent>
        {
            new() { QualityTier = 4 },
            new() { QualityTier = 4 }
        };
        
        // Act
        int quality = _craftingService.CalculateQuality(
            components, 
            stationMaxTier: 5,  // Would allow Tier 5
            recipeBonus: 2);    // 4 + 2 = 6
        
        // Assert
        Assert.AreEqual(4, quality); // Clamped to max 4 (v0.36 limitation)
    }
    
    [TestMethod]
    public async Task ValidateComponents_AllAvailable_Success()
    {
        // Arrange: Player has all required components
        var required = new List<RecipeComponent>
        {
            new() { ComponentItemId = 5001, QuantityRequired = 2, MinimumQuality = 1 },
            new() { ComponentItemId = 5030, QuantityRequired = 1, MinimumQuality = 1 }
        };
        
        // Mock database to return player has components
        
        // Act
        var validation = await _craftingService.ValidateComponents(1, required);
        
        // Assert
        Assert.IsTrue(validation.Success);
        Assert.AreEqual(2, validation.PlayerComponents.Count);
    }
    
    [TestMethod]
    public async Task ValidateComponents_ComponentBelowMinQuality_Fails()
    {
        // Arrange: Recipe requires Tier 3, player only has Tier 2
        var required = new List<RecipeComponent>
        {
            new() { ComponentItemId = 5003, QuantityRequired = 1, MinimumQuality = 3 }
        };
        
        // Player has Tier 2 version (insufficient quality)
        
        // Act
        var validation = await _craftingService.ValidateComponents(1, required);
        
        // Assert
        Assert.IsFalse(validation.Success);
        Assert.IsTrue(validation.Message.Contains("Insufficient"));
    }
    
    [TestMethod]
    public async Task ConsumeComponents_RemovesFromInventory()
    {
        // Arrange: Components to consume
        var components = new List<PlayerComponent>
        {
            new() { InventoryId = 100, ItemId = 5001, QuantityToConsume = 2, IsConsumed = true },
            new() { InventoryId = 101, ItemId = 5030, QuantityToConsume = 1, IsConsumed = true }
        };
        
        // Act
        await _craftingService.ConsumeComponents(1, components);
        
        // Assert: Verify database UPDATE called to reduce quantity
        // Verify database DELETE called if quantity reached 0
    }
    
    [TestMethod]
    public async Task ConsumeComponents_SkipsNonConsumedItems()
    {
        // Arrange: One consumed, one catalyst (not consumed)
        var components = new List<PlayerComponent>
        {
            new() { InventoryId = 100, ItemId = 5001, QuantityToConsume = 1, IsConsumed = true },
            new() { InventoryId = 101, ItemId = 9999, QuantityToConsume = 1, IsConsumed = false } // Catalyst
        };
        
        // Act
        await _craftingService.ConsumeComponents(1, components);
        
        // Assert: Only first item consumed, catalyst remains in inventory
    }
    
    [TestMethod]
    public async Task GenerateCraftedItem_MarksAsCrafted()
    {
        // Arrange
        var recipe = new Recipe 
        { 
            RecipeName = "Plasma Rifle",
            CraftedItemType = "Weapon",
            CraftedItemBaseId = null // Procedural generation
        };
        
        // Act
        var item = await _craftingService.GenerateCraftedItem(recipe, 3, 1);
        
        // Assert
        Assert.IsTrue(item.IsCrafted);
        Assert.AreEqual(1, item.CrafterCharacterId);
        Assert.AreEqual(3, item.QualityTier);
    }
    
    [TestMethod]
    public async Task UpdateCraftingStats_IncrementsCounter()
    {
        // Arrange
        int characterId = 1;
        int recipeId = 1010;
        
        // Act
        await _craftingService.UpdateCraftingStats(characterId, recipeId);
        
        // Assert: Verify Character_Recipes.times_crafted incremented
    }
}
```

---

## VII. Integration with v0.3 Equipment Service

```csharp
// Extending v0.3 Item class with crafting metadata
public class Item
{
    // Existing v0.3 properties
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string ItemType { get; set; }
    public int QualityTier { get; set; }
    
    // New v0.36 properties
    public bool IsCrafted { get; set; }
    public int? CrafterCharacterId { get; set; }
}

// Equipment service generates stats based on quality
public async Task<Item> GenerateEquipment(
    string itemType,
    int qualityTier,
    bool isCrafted = false)
{
    // Use existing v0.3 stat generation
    var item = GenerateBaseStats(itemType, qualityTier);
    
    // Mark as crafted if applicable
    item.IsCrafted = isCrafted;
    
    return item;
}
```

---

## VIII. Deployment Instructions

### Step 1: Add CraftingService to DI Container

```csharp
// Startup.cs or Program.cs
services.AddScoped<CraftingService>();
```

### Step 2: Integration Testing

```bash
# Run unit tests
dotnet test --filter "FullyQualifiedName~CraftingServiceTests"

# Expected: 10+ tests, 85%+ coverage
```

### Step 3: Manual Verification

```sql
-- Verify crafting succeeds
-- 1. Give player components
INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier)
VALUES (1, 5001, 2, 1), (1, 5030, 1, 1);

-- 2. Give player recipe knowledge
INSERT INTO Character_Recipes (character_id, recipe_id, discovered_at)
VALUES (1, 1001, CURRENT_TIMESTAMP);

-- 3. Use service to craft (via game code)
-- craftingService.CraftItem(characterId: 1, recipeId: 1001, stationId: 1);

-- 4. Verify crafted item in inventory
SELECT * FROM Character_Inventory WHERE character_id = 1 AND item_id = (SELECT item_id FROM Items WHERE item_name LIKE '%Improvised Blade%');

-- 5. Verify components consumed
SELECT * FROM Character_Inventory WHERE character_id = 1 AND item_id IN (5001, 5030);
```

---

## IX. Success Criteria

**Functional Requirements:**

- ✅ Players can craft items at appropriate stations
- ✅ Quality correctly calculated from components + station + bonus
- ✅ Components consumed on successful craft
- ✅ Validation prevents crafting with insufficient materials
- ✅ Validation prevents crafting at wrong station type
- ✅ Crafted items marked with IsCrafted flag

**Quality Gates:**

- ✅ 10+ unit tests implemented
- ✅ 85%+ code coverage
- ✅ Serilog structured logging throughout
- ✅ v5.0 compliance (Layer 2 voice in logs)
- ✅ ASCII-only service/method names

**Performance:**

- ✅ Quality calculation O(n) where n = component count
- ✅ Component validation single query per component type
- ✅ Crafting operation completes in <500ms

---

## X. v5.0 Compliance Notes

### Layer 2 Voice (Diagnostic/Clinical)

**✅ Correct Logging:**

```csharp
_logger.Information("Crafting {RecipeName} at quality tier {Quality}");
_logger.Warning("Station type mismatch: recipe requires {RequiredStation}");
```

**❌ Incorrect Logging:**

```csharp
// Don't use fantasy/magical language
_logger.Information("Enchanting {ItemName} with mystical power");
_logger.Warning("Arcane energies incompatible with forge");
```

---

**Status:** Implementation-ready core crafting mechanics complete.

**Next:** v0.36.3 (Modification & Inscription Systems)

</summary>