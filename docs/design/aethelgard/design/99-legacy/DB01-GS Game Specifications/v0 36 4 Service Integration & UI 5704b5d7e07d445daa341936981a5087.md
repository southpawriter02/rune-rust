# v0.36.4: Service Integration & UI

Type: UI
Description: Complete crafting integration - UI, recipe discovery, merchant sales, Einbui field crafting
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.36.1, v0.36.2, v0.36.3
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.36: Advanced Crafting System (v0%2036%20Advanced%20Crafting%20System%20e00690b9cf4b48538f10810b7f477711.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.36.4-INTEGRATION-UI

**Parent Specification:** [v0.36: Advanced Crafting System](v0%2036%20Advanced%20Crafting%20System%20e00690b9cf4b48538f10810b7f477711.md)

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-10 hours

**Prerequisites:** v0.36.1 (Database), v0.36.2 (CraftingService), v0.36.3 (ModificationService)

---

## I. Executive Summary

v0.36.4 completes the **Advanced Crafting System** with final integrations and UI:

- **Complete integration** — Wire CraftingService and ModificationService into game systems
- **Crafting menu UI** — Player interface for crafting at stations
- **Recipe book interface** — View discovered recipes and requirements
- **Component management UI** — Track available components
- **Merchant recipe sales** — Purchase recipes from NPCs
- **Einbui field crafting** — Special case for field station
- **Final unit tests** — Complete test suite with 85%+ coverage

This specification delivers a fully functional crafting system ready for player use.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.36.4)

- Complete service integration (v0.3, v0.9, v0.27.2)
- Crafting menu UI implementation
- Recipe book interface
- Component inventory display
- Recipe discovery mechanics
- Merchant recipe purchasing
- Einbui field crafting special case
- RecipeService implementation
- Final unit test suite (15+ tests total)
- Performance optimization
- Serilog logging throughout

### ❌ Explicitly Out of Scope

- Visual crafting animations (separate polish phase)
- Crafting skill progression (using PP/Legend only)
- Real-time crafting minigames (defer to v2.0+)
- Advanced material transmutation (defer to v2.0+)
- Legendary crafting (defer to v0.37)
- Set item crafting (defer to v0.37)

---

## III. RecipeService Implementation

### Core Service

```csharp
public class RecipeService
{
    private readonly IDbConnection _db;
    private readonly ILogger<RecipeService> _logger;
    
    public RecipeService(
        IDbConnection db,
        ILogger<RecipeService> logger)
    {
        _db = db;
        _logger = logger;
    }
    
    /// <summary>
    /// Discover a recipe for a character.
    /// </summary>
    public async Task<bool> DiscoverRecipe(
        int characterId,
        int recipeId,
        string discoverySource)
    {
        using var operation = _logger.BeginTimedOperation(
            "Discover recipe: character={CharacterId}, recipe={RecipeId}, source={Source}",
            characterId, recipeId, discoverySource);
        
        try
        {
            // Check if already known
            var exists = await _db.QuerySingleAsync<int>(@"
                SELECT COUNT(*)
                FROM Character_Recipes
                WHERE character_id = @CharacterId AND recipe_id = @RecipeId",
                new { CharacterId = characterId, RecipeId = recipeId });
            
            if (exists > 0)
            {
                _logger.Information(
                    "Character {CharacterId} already knows recipe {RecipeId}",
                    characterId, recipeId);
                return false; // Already known
            }
            
            // Add recipe knowledge
            await _db.ExecuteAsync(@"
                INSERT INTO Character_Recipes (
                    character_id,
                    recipe_id,
                    discovered_at,
                    discovery_source
                )
                VALUES (@CharacterId, @RecipeId, CURRENT_TIMESTAMP, @Source)",
                new 
                { 
                    CharacterId = characterId, 
                    RecipeId = recipeId,
                    Source = discoverySource
                });
            
            _logger.Information(
                "Character {CharacterId} discovered recipe {RecipeId} via {Source}",
                characterId, recipeId, discoverySource);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Failed to discover recipe: character={CharacterId}, recipe={RecipeId}",
                characterId, recipeId);
            throw;
        }
    }
    
    /// <summary>
    /// Get all recipes known by character.
    /// </summary>
    public async Task<List<RecipeDetails>> GetKnownRecipes(int characterId)
    {
        return (await _db.QueryAsync<RecipeDetails>(@"
            SELECT 
                r.*,
                cr.times_crafted,
                cr.discovered_at,
                cr.discovery_source
            FROM Crafting_Recipes r
            INNER JOIN Character_Recipes cr ON r.recipe_id = cr.recipe_id
            WHERE cr.character_id = @CharacterId
            ORDER BY r.recipe_tier, r.recipe_name",
            new { CharacterId = characterId })).ToList();
    }
    
    /// <summary>
    /// Get craftable recipes (player has components).
    /// </summary>
    public async Task<List<CraftableRecipe>> GetCraftableRecipes(
        int characterId,
        int? stationId = null)
    {
        var knownRecipes = await GetKnownRecipes(characterId);
        var craftable = new List<CraftableRecipe>();
        
        foreach (var recipe in knownRecipes)
        {
            // Filter by station if provided
            if (stationId.HasValue)
            {
                var station = await GetStation(stationId.Value);
                if (station != null && recipe.RequiredStation != "Any" &&
                    recipe.RequiredStation != station.StationType)
                {
                    continue; // Wrong station type
                }
            }
            
            // Check if player has components
            var components = await GetRecipeComponents(recipe.RecipeId);
            var hasComponents = await CheckComponentAvailability(
                characterId, components);
            
            craftable.Add(new CraftableRecipe
            {
                Recipe = recipe,
                Components = components,
                CanCraft = hasComponents
            });
        }
        
        return craftable;
    }
    
    /// <summary>
    /// Purchase recipe from merchant.
    /// </summary>
    public async Task<PurchaseResult> PurchaseRecipe(
        int characterId,
        int recipeId,
        int merchantId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Purchase recipe: character={CharacterId}, recipe={RecipeId}, merchant={MerchantId}",
            characterId, recipeId, merchantId);
        
        try
        {
            // Get recipe details
            var recipe = await _db.QuerySingleOrDefaultAsync<Recipe>(@"
                SELECT * FROM Crafting_Recipes WHERE recipe_id = @RecipeId",
                new { RecipeId = recipeId });
            
            if (recipe == null)
            {
                return PurchaseResult.Failure("Recipe not found");
            }
            
            // Calculate cost based on tier
            int cost = CalculateRecipeCost(recipe.RecipeTier);
            
            // Check if player has credits
            var credits = await _db.QuerySingleAsync<int>(@"
                SELECT credits FROM Characters WHERE character_id = @CharacterId",
                new { CharacterId = characterId });
            
            if (credits < cost)
            {
                _logger.Information(
                    "Insufficient credits: need {Cost}, have {Credits}",
                    cost, credits);
                return PurchaseResult.Failure(
                    $"Insufficient credits: need {cost}, have {credits}");
            }
            
            // Deduct credits
            await _db.ExecuteAsync(@"
                UPDATE Characters
                SET credits = credits - @Cost
                WHERE character_id = @CharacterId",
                new { CharacterId = characterId, Cost = cost });
            
            // Discover recipe
            var discovered = await DiscoverRecipe(characterId, recipeId, "Merchant");
            
            if (!discovered)
            {
                // Refund if already known
                await _db.ExecuteAsync(@"
                    UPDATE Characters
                    SET credits = credits + @Cost
                    WHERE character_id = @CharacterId",
                    new { CharacterId = characterId, Cost = cost });
                
                return PurchaseResult.Failure("You already know this recipe");
            }
            
            _logger.Information(
                "Character {CharacterId} purchased recipe {RecipeName} for {Cost} credits",
                characterId, recipe.RecipeName, cost);
            
            return PurchaseResult.Success(recipe.RecipeName, cost);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Failed to purchase recipe: character={CharacterId}, recipe={RecipeId}",
                characterId, recipeId);
            throw;
        }
    }
    
    /// <summary>
    /// Get recipes available from merchant.
    /// </summary>
    public async Task<List<RecipeDetails>> GetMerchantRecipes(int merchantId)
    {
        return (await _db.QueryAsync<RecipeDetails>(@"
            SELECT *
            FROM Crafting_Recipes
            WHERE discovery_method = 'Merchant'
            OR discovery_method = 'Default'
            ORDER BY recipe_tier, recipe_name")).ToList();
    }
    
    // Helper methods
    
    private int CalculateRecipeCost(string tier)
    {
        return tier switch
        {
            "Basic" => 75,
            "Advanced" => 225,
            "Expert" => 525,
            "Master" => 1000,
            _ => 100
        };
    }
    
    private async Task<List<RecipeComponent>> GetRecipeComponents(int recipeId)
    {
        return (await _db.QueryAsync<RecipeComponent>(@"
            SELECT * FROM Recipe_Components WHERE recipe_id = @RecipeId",
            new { RecipeId = recipeId })).ToList();
    }
    
    private async Task<bool> CheckComponentAvailability(
        int characterId,
        List<RecipeComponent> components)
    {
        foreach (var component in components)
        {
            var available = await _db.QuerySingleAsync<int>(@"
                SELECT COALESCE(SUM(quantity), 0)
                FROM Character_Inventory
                WHERE character_id = @CharacterId
                AND item_id = @ItemId
                AND quality_tier >= @MinQuality",
                new
                {
                    CharacterId = characterId,
                    ItemId = component.ComponentItemId,
                    MinQuality = component.MinimumQuality
                });
            
            if (available < component.QuantityRequired)
                return false;
        }
        
        return true;
    }
    
    private async Task<CraftingStation> GetStation(int stationId)
    {
        return await _db.QuerySingleOrDefaultAsync<CraftingStation>(@"
            SELECT * FROM Crafting_Stations WHERE station_id = @StationId",
            new { StationId = stationId });
    }
}
```

### Supporting Classes

```csharp
public class RecipeDetails : Recipe
{
    public int TimesCrafted { get; set; }
    public DateTime DiscoveredAt { get; set; }
    public string DiscoverySource { get; set; }
}

public class CraftableRecipe
{
    public RecipeDetails Recipe { get; set; }
    public List<RecipeComponent> Components { get; set; }
    public bool CanCraft { get; set; }
}

public class PurchaseResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string RecipeName { get; set; }
    public int Cost { get; set; }
    
    public static PurchaseResult Failure(string message) =>
        new PurchaseResult { Success = false, Message = message };
    
    public static PurchaseResult Success(string recipeName, int cost) =>
        new PurchaseResult
        {
            Success = true,
            RecipeName = recipeName,
            Cost = cost,
            Message = $"Learned recipe: {recipeName} ({cost} credits)"
        };
}
```

---

## IV. Crafting Menu UI

### Menu Structure

```
╔════════════════════════════════════════════════════════╗
║         CRAFTING STATION: Engineering Workshop         ║
╠════════════════════════════════════════════════════════╣
║ [Recipes]  [Components]  [Modifications]  [Exit]       ║
╠════════════════════════════════════════════════════════╣
║                                                         ║
║  Known Recipes (12/45)                                  ║
║  ────────────────────────────────────────              ║
║  ✓ Improvised Blade            (Basic)     [CRAFT]     ║
║  ✓ Plasma Rifle                (Advanced)  [CRAFT]     ║
║  ✗ Fusion Lance                (Expert)    [─────]     ║
║    └─ Missing: 2x Nano-Processor (Tier 3)              ║
║  ✓ Basic Stim-Pack             (Basic)     [CRAFT]     ║
║                                                         ║
║  Filter: [All] [Weapons] [Armor] [Consumables]         ║
║  Sort:   [Name] [Tier] [Craftable]                     ║
╠════════════════════════════════════════════════════════╣
║ Selected: Plasma Rifle (Advanced)                      ║
║ ───────────────────────────────────────                ║
║ Requirements:                                           ║
║  • 1x Reinforced Frame (Tier 3)       [✓ Available]    ║
║  • 1x Plasma Core (Tier 3)            [✓ Available]    ║
║  • 1x Synthetic Grip (Tier 2)         [✓ Available]    ║
║                                                         ║
║ Station: Workshop (Max Tier 4)                         ║
║ Output Quality: Tier 3 (with +1 bonus = Tier 4)        ║
║                                                         ║
║                              [CRAFT ITEM]  [CANCEL]    ║
╚════════════════════════════════════════════════════════╝
```

### Recipe Book Interface

```
╔════════════════════════════════════════════════════════╗
║                   RECIPE BOOK                          ║
╠════════════════════════════════════════════════════════╣
║                                                         ║
║  Total Recipes: 12 / 100                                ║
║                                                         ║
║  WEAPONS (4)                                            ║
║  ──────────                                             ║
║  • Improvised Blade       Crafted: 2 times              ║
║  • Salvaged Pistol        Crafted: 1 time               ║
║  • Plasma Rifle           Crafted: 0 times              ║
║  • Vibro-Blade            Crafted: 1 time               ║
║                                                         ║
║  ARMOR (3)                                              ║
║  ──────────                                             ║
║  • Salvaged Chest Plate   Crafted: 1 time               ║
║  • Scrap Helmet           Crafted: 1 time               ║
║  • Makeshift Gloves       Crafted: 0 times              ║
║                                                         ║
║  CONSUMABLES (5)                                        ║
║  ──────────                                             ║
║  • Basic Stim-Pack        Crafted: 12 times             ║
║  • Combat Stimulant       Crafted: 5 times              ║
║  • EMP Grenade            Crafted: 2 times              ║
║  • Smoke Charge           Crafted: 8 times              ║
║  • Signal Flare           Crafted: 3 times              ║
║                                                         ║
║                                      [CLOSE]            ║
╚════════════════════════════════════════════════════════╝
```

### Component Management UI

```
╔════════════════════════════════════════════════════════╗
║              CRAFTING COMPONENTS                       ║
╠════════════════════════════════════════════════════════╣
║                                                         ║
║  METAL INGOTS                                           ║
║  ──────────                                             ║
║  • Scrap Iron Ingot (T1)         x15                    ║
║  • Steel Ingot (T2)               x8                    ║
║  • Titanium Alloy (T3)            x3                    ║
║  • Star-Metal Ingot (T4)          x1                    ║
║                                                         ║
║  POWER CORES                                            ║
║  ──────────                                             ║
║  • Basic Energy Cell (T2)         x6                    ║
║  • Plasma Core (T3)               x2                    ║
║  • Fusion Cell (T4)               x0                    ║
║                                                         ║
║  AETHERIC COMPONENTS                                    ║
║  ──────────                                             ║
║  • Minor Aetheric Shard (T3)      x4                    ║
║  • Aetheric Shard (T4)            x1                    ║
║  • Stabilizing Compound (T2)      x10                   ║
║                                                         ║
║  [Sort: Type] [Filter: Tier]              [CLOSE]      ║
╚════════════════════════════════════════════════════════╝
```

---

## V. Einbui Field Crafting Integration

### Special Case Implementation

```csharp
public class EinbuiCraftingHandler
{
    private readonly CraftingService _craftingService;
    private readonly RecipeService _recipeService;
    private readonly ILogger<EinbuiCraftingHandler> _logger;
    
    private const int FIELD_STATION_ID = 100; // Virtual station ID
    
    public EinbuiCraftingHandler(
        CraftingService craftingService,
        RecipeService recipeService,
        ILogger<EinbuiCraftingHandler> logger)
    {
        _craftingService = craftingService;
        _recipeService = recipeService;
        _logger = logger;
    }
    
    /// <summary>
    /// Check if character can field craft (has Einbui spec).
    /// </summary>
    public async Task<bool> CanFieldCraft(int characterId)
    {
        var hasEinbui = await _db.QuerySingleAsync<bool>(@"
            SELECT COUNT(*) > 0
            FROM Character_Specializations
            WHERE character_id = @CharacterId
            AND specialization_id = 27002",
            new { CharacterId = characterId });
        
        return hasEinbui;
    }
    
    /// <summary>
    /// Get field-craftable recipes (consumables only, Basic tier).
    /// </summary>
    public async Task<List<CraftableRecipe>> GetFieldCraftableRecipes(
        int characterId)
    {
        var knownRecipes = await _recipeService.GetKnownRecipes(characterId);
        
        // Filter to field-craftable only
        var fieldRecipes = knownRecipes.Where(r =>
            r.CraftedItemType == "Consumable" &&
            (r.RequiredStation == "Field_Station" || r.RecipeTier == "Basic"))
            .ToList();
        
        // Check component availability
        var craftable = new List<CraftableRecipe>();
        foreach (var recipe in fieldRecipes)
        {
            var components = await _recipeService.GetRecipeComponents(recipe.RecipeId);
            var hasComponents = await _recipeService.CheckComponentAvailability(
                characterId, components);
            
            craftable.Add(new CraftableRecipe
            {
                Recipe = recipe,
                Components = components,
                CanCraft = hasComponents
            });
        }
        
        return craftable;
    }
    
    /// <summary>
    /// Craft item in field (Einbui ability).
    /// </summary>
    public async Task<CraftingResult> FieldCraft(
        int characterId,
        int recipeId)
    {
        // Validate Einbui specialization
        if (!await CanFieldCraft(characterId))
        {
            return CraftingResult.Failure(
                "Field crafting requires the Einbui specialization");
        }
        
        // Use virtual field station
        return await _craftingService.CraftItem(
            characterId,
            recipeId,
            FIELD_STATION_ID);
    }
}
```

---

## VI. Merchant Integration

### Recipe Shop UI

```
╔════════════════════════════════════════════════════════╗
║          MERCHANT: Craftmaster Thorin                  ║
╠════════════════════════════════════════════════════════╣
║ "Looking to expand your repertoire? I have schematics  ║
║  from across Midgard."                                  ║
╠════════════════════════════════════════════════════════╣
║                                                         ║
║  AVAILABLE RECIPES                                      ║
║  ────────────────────                                  ║
║                                                         ║
║  [✓] Improvised Blade       75 credits    [KNOWN]      ║
║  [ ] Plasma Rifle          225 credits    [BUY]        ║
║  [ ] Vibro-Blade           225 credits    [BUY]        ║
║  [ ] Reinforced Chest      225 credits    [BUY]        ║
║  [ ] Combat Helmet         225 credits    [BUY]        ║
║  [ ] Advanced Stim-Pack    225 credits    [BUY]        ║
║  [ ] EMP Grenade           225 credits    [BUY]        ║
║                                                         ║
║  Your Credits: 450                                      ║
║                                                         ║
║                         [TRADE]  [LEAVE]                ║
╚════════════════════════════════════════════════════════╝
```

### Purchase Flow

```csharp
// When player clicks [BUY]
public async Task HandleRecipePurchase(
    int characterId,
    int recipeId,
    int merchantId)
{
    var result = await _recipeService.PurchaseRecipe(
        characterId,
        recipeId,
        merchantId);
    
    if (result.Success)
    {
        DisplayMessage($"Learned: {result.RecipeName}!");
        PlaySound("recipe_learned");
        RefreshRecipeList();
    }
    else
    {
        DisplayError(result.Message);
    }
}
```

---

## VII. Final Unit Tests

### RecipeService Tests

```csharp
[TestClass]
public class RecipeServiceTests
{
    [TestMethod]
    public async Task DiscoverRecipe_NewRecipe_Success()
    {
        // Arrange
        int characterId = 1;
        int recipeId = 1010;
        string source = "Quest";
        
        // Act
        var result = await _recipeService.DiscoverRecipe(
            characterId, recipeId, source);
        
        // Assert
        Assert.IsTrue(result);
    }
    
    [TestMethod]
    public async Task DiscoverRecipe_AlreadyKnown_ReturnsFalse()
    {
        // Arrange: Character already knows recipe
        int characterId = 1;
        int recipeId = 1001;
        
        // Act
        var result = await _recipeService.DiscoverRecipe(
            characterId, recipeId, "Duplicate");
        
        // Assert
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    public async Task PurchaseRecipe_SufficientCredits_Success()
    {
        // Arrange: Character has 300 credits, recipe costs 225
        int characterId = 1;
        int recipeId = 1010;
        int merchantId = 1;
        
        // Act
        var result = await _recipeService.PurchaseRecipe(
            characterId, recipeId, merchantId);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(225, result.Cost);
    }
    
    [TestMethod]
    public async Task PurchaseRecipe_InsufficientCredits_Fails()
    {
        // Arrange: Character has 100 credits, recipe costs 225
        int characterId = 1;
        int recipeId = 1010;
        
        // Act
        var result = await _recipeService.PurchaseRecipe(
            characterId, recipeId, 1);
        
        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Insufficient credits"));
    }
    
    [TestMethod]
    public async Task GetCraftableRecipes_ReturnsOnlyWithComponents()
    {
        // Arrange: Character knows 5 recipes, has components for 2
        int characterId = 1;
        
        // Act
        var craftable = await _recipeService.GetCraftableRecipes(characterId);
        var canCraft = craftable.Where(c => c.CanCraft).ToList();
        
        // Assert
        Assert.AreEqual(2, canCraft.Count);
    }
}
```

---

## VIII. Deployment Instructions

### Step 1: Add Services to DI Container

```csharp
// Startup.cs or Program.cs
services.AddScoped<CraftingService>();
services.AddScoped<ModificationService>();
services.AddScoped<RecipeService>();
services.AddScoped<EinbuiCraftingHandler>();
```

### Step 2: Run Full Test Suite

```bash
# Run all crafting tests
dotnet test --filter "FullyQualifiedName~Crafting"

# Expected results:
# - CraftingServiceTests: 10+ tests
# - ModificationServiceTests: 10+ tests
# - RecipeServiceTests: 5+ tests
# - Total: 25+ tests, 85%+ coverage
```

### Step 3: Integration Verification

```sql
-- Full crafting workflow test

-- 1. Character discovers recipe
INSERT INTO Character_Recipes (character_id, recipe_id, discovery_source)
VALUES (1, 1010, 'Merchant');

-- 2. Give components
INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier)
VALUES
(1, 5021, 1, 3), -- Reinforced Frame
(1, 5011, 1, 3), -- Plasma Core
(1, 5031, 1, 2); -- Synthetic Grip

-- 3. Craft item (via game code)
-- craftingService.CraftItem(1, 1010, 2);

-- 4. Apply modification (via game code)
-- modificationService.ApplyModification(1, equipmentId, 8001);

-- 5. Verify complete state
SELECT 
    i.item_name,
    ci.quality_tier,
    COUNT(em.modification_id) as mod_count
FROM Character_Inventory ci
INNER JOIN Items i ON ci.item_id = i.item_id
LEFT JOIN Equipment_Modifications em ON ci.inventory_id = [em.equipment](http://em.equipment)_item_id
WHERE ci.character_id = 1
GROUP BY i.item_name, ci.quality_tier;
```

---

## IX. Success Criteria

**Functional Requirements:**

- ✅ Players can access crafting menu at stations
- ✅ Recipe book displays discovered recipes
- ✅ Component inventory shows available materials
- ✅ Merchants sell recipes for credits
- ✅ Einbui can field craft basic consumables
- ✅ All services integrated with existing systems
- ✅ UI responsive and intuitive

**Quality Gates:**

- ✅ 25+ total unit tests across all services
- ✅ 85%+ code coverage
- ✅ Serilog structured logging throughout
- ✅ v5.0 compliance (Layer 2 voice)
- ✅ Performance: All operations <500ms

**Content:**

- ✅ 100+ recipes available
- ✅ 50+ component items
- ✅ 20+ runic inscriptions
- ✅ 10+ crafting stations placed
- ✅ Complete integration documentation

---

## X. v5.0 Compliance Notes

### Layer 2 Voice (Diagnostic/Clinical)

**✅ Correct UI Text:**

- "Crafting Station Access"
- "Component Synthesis Parameters"
- "Recipe Data Archive"
- "Runic Inscription Protocols"

**❌ Incorrect UI Text:**

- ~~"Magical Enchanting Table"~~
- ~~"Alchemical Transmutation Circle"~~
- ~~"Divine Forge of the Ancients"~~

---

**Status:** Complete Advanced Crafting System implementation-ready.

**v0.36 Total Timeline:** 30-45 hours across 4 specifications.

**Next System:** v0.37 (Legendary Items & Sets)