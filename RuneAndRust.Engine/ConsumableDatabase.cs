using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.16 Consumable Database
/// Manages expanded consumable library (potions, tools, provisions)
/// </summary>
public class ConsumableDatabase
{
    private readonly Dictionary<string, Consumable> _consumables = new();

    public ConsumableDatabase()
    {
        InitializeV016Consumables();
    }

    private void InitializeV016Consumables()
    {
        // 1. Soothing Herb Tea (Common)
        AddConsumable("soothing_herb_tea", new Consumable
        {
            Name = "Soothing Herb Tea",
            Description = "Warm. Bitter. Calming. Foraged herbs steeped in clean water.",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            HPRestore = 0,
            StaminaRestore = 0,
            StressRestore = 15,
            ClearsBleeding = false,
            ClearsPoison = false,
            ClearsDisease = false
        });

        // 2. Stimpack (Common)
        AddConsumable("stimpack", new Consumable
        {
            Name = "Stimpack",
            Description = "Pre-Glitch medical injection. Still works. Mostly. Stings going in.",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            HPRestore = 15, // 2d8+5 ≈ 15 avg
            StaminaRestore = 0,
            StressRestore = 0,
            ClearsBleeding = false,
            ClearsPoison = false,
            ClearsDisease = false,
            MasterworkBonusHP = 5
        });

        // 3. Ration Bar (Common)
        AddConsumable("ration_bar", new Consumable
        {
            Name = "Ration Bar",
            Description = "Nutritionally complete. Taste: debatable. Expires: never.",
            Type = ConsumableType.Provision,
            Quality = CraftQuality.Standard,
            HPRestore = 5, // 1d6
            StaminaRestore = 10,
            StressRestore = 0,
            ClearsBleeding = false,
            ClearsPoison = false,
            ClearsDisease = false
        });

        // 4. Corruption Suppressant (Uncommon)
        AddConsumable("corruption_suppressant", new Consumable
        {
            Name = "Corruption Suppressant",
            Description = "Experimental drug. Pushes back the wrongness. Tastes like metal and regret. Max 3 uses per day.",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            HPRestore = 0,
            StaminaRestore = 0,
            StressRestore = 0,
            ClearsBleeding = false,
            ClearsPoison = false,
            ClearsDisease = false
            // Special: Reduces Corruption by 10 (handled in game logic)
        });

        // 5. Stress Dampener Pill (Uncommon)
        AddConsumable("stress_dampener_pill", new Consumable
        {
            Name = "Stress Dampener Pill",
            Description = "Dulls the edge. Numbs the fear. Pre-Glitch psychotropic medication.",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            HPRestore = 0,
            StaminaRestore = 0,
            StressRestore = 20,
            ClearsBleeding = false,
            ClearsPoison = false,
            ClearsDisease = false
            // Special: +10% Stress resistance for 1 hour (handled in game logic)
        });

        // 6. Power Cell (Uncommon)
        AddConsumable("power_cell", new Consumable
        {
            Name = "Power Cell",
            Description = "Universal power source. Recharges energy weapons and equipment. Rare. Valuable.",
            Type = ConsumableType.Tool,
            Quality = CraftQuality.Standard,
            HPRestore = 0,
            StaminaRestore = 0,
            StressRestore = 0,
            ClearsBleeding = false,
            ClearsPoison = false,
            ClearsDisease = false
            // Special: Recharges energy weapons (handled in game logic)
        });

        // 7. Antifungal Injection (Uncommon)
        AddConsumable("antifungal_injection", new Consumable
        {
            Name = "Antifungal Injection",
            Description = "Symbiotic Plate hates this. Purges the infection. Burns like fire in your veins.",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            HPRestore = 0,
            StaminaRestore = 0,
            StressRestore = 0,
            ClearsBleeding = false,
            ClearsPoison = true,
            ClearsDisease = true
            // Special: Remove [Infected] status, prevent Infection for 2 hours (handled in game logic)
        });

        // 8. Combat Stimulant (Rare)
        AddConsumable("combat_stimulant", new Consumable
        {
            Name = "Combat Stimulant",
            Description = "Military-grade enhancement. +2 to all attributes for 5 turns. Powerful. Addictive. Costs 20 Stress afterward.",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            HPRestore = 0,
            StaminaRestore = 20,
            StressRestore = 0,
            ClearsBleeding = false,
            ClearsPoison = false,
            ClearsDisease = false
            // Special: +2 to all attributes for 5 turns, +20 Stress after (handled in game logic)
        });

        // 9. Restoration Serum (Rare)
        AddConsumable("restoration_serum", new Consumable
        {
            Name = "Restoration Serum",
            Description = "Pre-Glitch medical miracle. Nearly priceless. Heals almost anything.",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            HPRestore = 40, // 5d10 ≈ 40 avg
            StaminaRestore = 30,
            StressRestore = 0,
            ClearsBleeding = true,
            ClearsPoison = true,
            ClearsDisease = true,
            MasterworkBonusHP = 10,
            MasterworkBonusStamina = 10
        });

        // 10. Corruption Purge (Epic)
        AddConsumable("corruption_purge", new Consumable
        {
            Name = "Corruption Purge",
            Description = "Purging corruption is... traumatic. Removes alien influence violently. ALWAYS triggers a Breaking Point.",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Standard,
            HPRestore = 0,
            StaminaRestore = 0,
            StressRestore = 0,
            ClearsBleeding = false,
            ClearsPoison = false,
            ClearsDisease = false
            // Special: -30 Corruption, trigger Breaking Point (guaranteed Trauma) (handled in game logic)
        });

        // Masterwork versions
        AddConsumable("stimpack_masterwork", new Consumable
        {
            Name = "Stimpack",
            Description = "Pre-Glitch medical injection. Still works. Mostly. Stings going in. [Masterwork Quality]",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Masterwork,
            HPRestore = 15,
            StaminaRestore = 0,
            StressRestore = 0,
            ClearsBleeding = false,
            ClearsPoison = false,
            ClearsDisease = false,
            MasterworkBonusHP = 5
        });

        AddConsumable("restoration_serum_masterwork", new Consumable
        {
            Name = "Restoration Serum",
            Description = "Pre-Glitch medical miracle. Nearly priceless. Heals almost anything. [Masterwork Quality]",
            Type = ConsumableType.Medicine,
            Quality = CraftQuality.Masterwork,
            HPRestore = 40,
            StaminaRestore = 30,
            StressRestore = 0,
            ClearsBleeding = true,
            ClearsPoison = true,
            ClearsDisease = true,
            MasterworkBonusHP = 10,
            MasterworkBonusStamina = 10
        });
    }

    private void AddConsumable(string key, Consumable consumable)
    {
        _consumables[key] = consumable;
    }

    /// <summary>
    /// Get a consumable by key
    /// </summary>
    public Consumable? GetConsumable(string consumableKey)
    {
        return _consumables.GetValueOrDefault(consumableKey);
    }

    /// <summary>
    /// Get all consumables in the database
    /// </summary>
    public List<Consumable> GetAllConsumables()
    {
        return _consumables.Values.ToList();
    }

    /// <summary>
    /// Get consumables by type
    /// </summary>
    public List<Consumable> GetConsumablesByType(ConsumableType type)
    {
        return _consumables.Values
            .Where(c => c.Type == type)
            .ToList();
    }

    /// <summary>
    /// Get consumables by quality
    /// </summary>
    public List<Consumable> GetConsumablesByQuality(CraftQuality quality)
    {
        return _consumables.Values
            .Where(c => c.Quality == quality)
            .ToList();
    }
}
