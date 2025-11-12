using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch;

/// <summary>
/// Context passed through population and Coherent Glitch rule systems (v0.11/v0.12)
/// </summary>
public class PopulationContext
{
    // Generation State
    public Random Rng { get; set; } = new Random();
    public int Seed { get; set; } = 0;
    public string BiomeId { get; set; } = string.Empty;

    // Biome Element Tables
    public BiomeElementPool BiomeElements { get; set; } = new BiomeElementPool();

    // Spawn Budget (v0.11)
    public int SpawnBudget { get; set; } = 10; // Base budget for enemy spawns
    public int SpawnBudgetModifier { get; set; } = 0; // Adjustments from rules

    // Loot Economy (v0.11)
    public int LootBudget { get; set; } = 100; // Base Cogs value per room
    public double LootMultiplier { get; set; } = 1.0; // Multiplier from rules

    // Coherent Glitch Tracking (v0.12)
    public int TotalRulesFired { get; set; } = 0;
    public Dictionary<string, int> RuleFireCounts { get; set; } = new Dictionary<string, int>();

    // Dungeon Context
    public Dungeon? CurrentDungeon { get; set; } = null;
    public Room? CurrentRoom { get; set; } = null;

    /// <summary>
    /// Gets effective spawn budget for current room
    /// </summary>
    public int GetEffectiveSpawnBudget()
    {
        return Math.Max(0, SpawnBudget + SpawnBudgetModifier);
    }

    /// <summary>
    /// Gets effective loot budget for current room
    /// </summary>
    public int GetEffectiveLootBudget()
    {
        return (int)(LootBudget * LootMultiplier);
    }
}

/// <summary>
/// Pool of biome elements with weighted selection (v0.11/v0.12)
/// </summary>
public class BiomeElementPool
{
    private List<BiomeElementEntry> _elements = new List<BiomeElementEntry>();
    private HashSet<string> _excludedElements = new HashSet<string>();

    /// <summary>
    /// Adds an element to the pool
    /// </summary>
    public void AddElement(string elementId, BiomeElementType type, float weight, object? data = null)
    {
        _elements.Add(new BiomeElementEntry
        {
            ElementId = elementId,
            Type = type,
            Weight = weight,
            Data = data
        });
    }

    /// <summary>
    /// Gets an element by ID
    /// </summary>
    public BiomeElementEntry? GetElement(string elementId)
    {
        return _elements.FirstOrDefault(e => e.ElementId == elementId);
    }

    /// <summary>
    /// Modifies an element's weight (Coherent Glitch rule effect)
    /// </summary>
    public void ModifyWeight(string elementId, float multiplier)
    {
        var element = GetElement(elementId);
        if (element != null)
        {
            element.Weight *= multiplier;
        }
    }

    /// <summary>
    /// Excludes an element from spawning (Coherent Glitch rule effect)
    /// </summary>
    public void ExcludeElement(string elementId)
    {
        _excludedElements.Add(elementId);
    }

    /// <summary>
    /// Selects a random element by type using weighted selection
    /// </summary>
    public BiomeElementEntry? SelectRandom(BiomeElementType type, Random rng)
    {
        var eligible = _elements
            .Where(e => e.Type == type && !_excludedElements.Contains(e.ElementId) && e.Weight > 0)
            .ToList();

        if (eligible.Count == 0)
            return null;

        float totalWeight = eligible.Sum(e => e.Weight);
        float roll = (float)(rng.NextDouble() * totalWeight);
        float cumulative = 0;

        foreach (var element in eligible)
        {
            cumulative += element.Weight;
            if (roll <= cumulative)
            {
                return element;
            }
        }

        return eligible.Last(); // Fallback
    }

    /// <summary>
    /// Gets all elements of a specific type
    /// </summary>
    public List<BiomeElementEntry> GetElementsByType(BiomeElementType type)
    {
        return _elements
            .Where(e => e.Type == type && !_excludedElements.Contains(e.ElementId))
            .ToList();
    }
}

/// <summary>
/// Individual biome element entry (v0.11)
/// </summary>
public class BiomeElementEntry
{
    public string ElementId { get; set; } = string.Empty;
    public BiomeElementType Type { get; set; } = BiomeElementType.DescriptionDetail;
    public float Weight { get; set; } = 1.0f;
    public object? Data { get; set; } = null; // Additional data (enemy templates, etc.)
}
