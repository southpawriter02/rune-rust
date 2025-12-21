using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Data;

/// <summary>
/// Static registry containing all available crafting recipes.
/// Provides lookup methods for recipe access by ID or trade.
/// </summary>
public static class RecipeRegistry
{
    private static readonly Dictionary<string, Recipe> _recipes = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a read-only view of all registered recipes.
    /// </summary>
    public static IReadOnlyDictionary<string, Recipe> Recipes => _recipes;

    /// <summary>
    /// Static constructor initializes all seed recipes.
    /// </summary>
    static RecipeRegistry() => InitializeRecipes();

    /// <summary>
    /// Gets a recipe by its unique ID.
    /// </summary>
    /// <param name="recipeId">The recipe identifier.</param>
    /// <returns>The recipe if found, null otherwise.</returns>
    public static Recipe? GetById(string recipeId)
    {
        return _recipes.TryGetValue(recipeId, out var recipe) ? recipe : null;
    }

    /// <summary>
    /// Gets all recipes belonging to a specific trade.
    /// </summary>
    /// <param name="trade">The crafting trade to filter by.</param>
    /// <returns>List of recipes for the specified trade.</returns>
    public static IReadOnlyList<Recipe> GetByTrade(CraftingTrade trade)
    {
        return _recipes.Values.Where(r => r.Trade == trade).ToList();
    }

    /// <summary>
    /// Gets all registered recipes.
    /// </summary>
    /// <returns>List of all recipes.</returns>
    public static IReadOnlyList<Recipe> GetAll()
    {
        return _recipes.Values.ToList();
    }

    /// <summary>
    /// Registers a recipe in the registry.
    /// </summary>
    private static void Register(Recipe recipe)
    {
        _recipes[recipe.RecipeId] = recipe;
    }

    /// <summary>
    /// Initializes all seed recipes (12 total, 3 per trade).
    /// </summary>
    private static void InitializeRecipes()
    {
        // ==================== BODGING (3 recipes) ====================
        Register(new Recipe
        {
            RecipeId = "RCP_BOD_TORCH",
            Name = "Improvised Torch",
            Description = "A crude but functional light source fashioned from salvaged wood and oil-soaked rags.",
            Trade = CraftingTrade.Bodging,
            BaseDc = 2,
            Ingredients = new Dictionary<string, int>
            {
                { "scrap_wood", 1 },
                { "oily_rag", 1 }
            },
            OutputItemId = "torch",
            OutputQuantity = 1
        });

        Register(new Recipe
        {
            RecipeId = "RCP_BOD_LOCKPICK",
            Name = "Bent Lockpick",
            Description = "Metal scraps bent and filed into a basic lockpick. Fragile but serviceable.",
            Trade = CraftingTrade.Bodging,
            BaseDc = 3,
            Ingredients = new Dictionary<string, int>
            {
                { "scrap_metal", 2 }
            },
            OutputItemId = "lockpick",
            OutputQuantity = 1
        });

        Register(new Recipe
        {
            RecipeId = "RCP_BOD_ROPE",
            Name = "Knotted Rope",
            Description = "Plant fibers woven and knotted into a rough but sturdy rope.",
            Trade = CraftingTrade.Bodging,
            BaseDc = 2,
            Ingredients = new Dictionary<string, int>
            {
                { "fiber_bundle", 3 }
            },
            OutputItemId = "rope",
            OutputQuantity = 1
        });

        // ==================== ALCHEMY (3 recipes) ====================
        // Alchemy recipes have Explosive catastrophe type (v0.3.1c)
        Register(new Recipe
        {
            RecipeId = "RCP_ALC_STIM",
            Name = "Basic Stimulant",
            Description = "A bitter tincture that sharpens the senses and quickens the pulse. The Blight-taint adds an unpleasant aftertaste.",
            Trade = CraftingTrade.Alchemy,
            BaseDc = 3,
            Ingredients = new Dictionary<string, int>
            {
                { "blight_moss", 1 },
                { "clean_water", 1 }
            },
            OutputItemId = "stimulant",
            OutputQuantity = 1,
            CatastropheType = CatastropheType.Explosive,
            CatastropheDamage = 5
        });

        Register(new Recipe
        {
            RecipeId = "RCP_ALC_ANTITOX",
            Name = "Crude Antitoxin",
            Description = "Charcoal-filtered extract that neutralizes common poisons. Works against Blight toxins with reduced efficacy.",
            Trade = CraftingTrade.Alchemy,
            BaseDc = 4,
            Ingredients = new Dictionary<string, int>
            {
                { "blight_moss", 2 },
                { "charcoal", 1 }
            },
            OutputItemId = "antitoxin",
            OutputQuantity = 1,
            CatastropheType = CatastropheType.Explosive,
            CatastropheDamage = 8
        });

        Register(new Recipe
        {
            RecipeId = "RCP_ALC_FIREBOMB",
            Name = "Alchemist Fire",
            Description = "Volatile compound sealed in glass. Ignites on impact, spreading adhesive flames.",
            Trade = CraftingTrade.Alchemy,
            BaseDc = 5,
            Ingredients = new Dictionary<string, int>
            {
                { "oily_rag", 2 },
                { "sulfur", 1 },
                { "glass_vial", 1 }
            },
            OutputItemId = "firebomb",
            OutputQuantity = 1,
            CatastropheType = CatastropheType.Explosive,
            CatastropheDamage = 15
        });

        // ==================== RUNEFORGING (3 recipes) ====================
        // Runeforging recipes have Corruption catastrophe type (v0.3.1c)
        Register(new Recipe
        {
            RecipeId = "RCP_RUN_GLOW",
            Name = "Glow Stone",
            Description = "Quartz infused with residual Aether. Emits a pale, steady light that never dims.",
            Trade = CraftingTrade.Runeforging,
            BaseDc = 2,
            Ingredients = new Dictionary<string, int>
            {
                { "quartz_shard", 1 },
                { "aether_dust", 1 }
            },
            OutputItemId = "glowstone",
            OutputQuantity = 1,
            CatastropheType = CatastropheType.Corruption,
            CatastropheCorruption = 3
        });

        Register(new Recipe
        {
            RecipeId = "RCP_RUN_WARD",
            Name = "Minor Ward Token",
            Description = "Iron disc inscribed with protective runes. Offers fleeting resistance against Blight corruption.",
            Trade = CraftingTrade.Runeforging,
            BaseDc = 4,
            Ingredients = new Dictionary<string, int>
            {
                { "iron_shard", 1 },
                { "aether_dust", 2 }
            },
            OutputItemId = "wardtoken",
            OutputQuantity = 1,
            CatastropheType = CatastropheType.Corruption,
            CatastropheCorruption = 5
        });

        Register(new Recipe
        {
            RecipeId = "RCP_RUN_CHARGE",
            Name = "Runic Battery",
            Description = "Copper coils wrapped around an Aether core. Stores and releases magical energy on demand.",
            Trade = CraftingTrade.Runeforging,
            BaseDc = 5,
            Ingredients = new Dictionary<string, int>
            {
                { "copper_wire", 2 },
                { "aether_dust", 3 }
            },
            OutputItemId = "battery",
            OutputQuantity = 1,
            CatastropheType = CatastropheType.Corruption,
            CatastropheCorruption = 8
        });

        // ==================== FIELD MEDICINE (3 recipes) ====================
        Register(new Recipe
        {
            RecipeId = "RCP_MED_BANDAGE",
            Name = "Field Bandage",
            Description = "Clean cloth strips prepared for wound dressing. Stops bleeding and prevents infection.",
            Trade = CraftingTrade.FieldMedicine,
            BaseDc = 2,
            Ingredients = new Dictionary<string, int>
            {
                { "clean_cloth", 2 }
            },
            OutputItemId = "bandage",
            OutputQuantity = 1
        });

        Register(new Recipe
        {
            RecipeId = "RCP_MED_SPLINT",
            Name = "Emergency Splint",
            Description = "Wood and cloth bound together to immobilize broken limbs. Crude but effective.",
            Trade = CraftingTrade.FieldMedicine,
            BaseDc = 3,
            Ingredients = new Dictionary<string, int>
            {
                { "scrap_wood", 1 },
                { "clean_cloth", 1 }
            },
            OutputItemId = "splint",
            OutputQuantity = 1
        });

        Register(new Recipe
        {
            RecipeId = "RCP_MED_KIT",
            Name = "Medkit",
            Description = "A comprehensive field medical kit containing bandages, antiseptic moss, and purified water.",
            Trade = CraftingTrade.FieldMedicine,
            BaseDc = 4,
            Ingredients = new Dictionary<string, int>
            {
                { "clean_cloth", 3 },
                { "blight_moss", 1 },
                { "clean_water", 1 }
            },
            OutputItemId = "medkit",
            OutputQuantity = 1
        });
    }
}
