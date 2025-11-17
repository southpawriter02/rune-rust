using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.35.3: Calculates environmental hazard density based on territorial control
/// Different factions affect hazard spawns and environmental danger levels
/// </summary>
public class HazardDensityModifier
{
    private static readonly ILogger _log = Log.ForContext<HazardDensityModifier>();

    // Base hazard density multiplier (1.0 = normal)
    private const double BASE_DENSITY = 1.0;

    /// <summary>
    /// Calculate hazard density multiplier for a sector
    /// Takes into account controlling faction and war status
    /// </summary>
    public double CalculateHazardDensity(string? controllingFaction, bool isWarZone)
    {
        // Faction-specific modifiers
        double factionModifier = GetFactionModifier(controllingFaction);

        // War zone modifier
        double warModifier = isWarZone ? 1.5 : 1.0; // +50% hazards in war zones

        double totalDensity = BASE_DENSITY * factionModifier * warModifier;

        _log.Debug(
            "Hazard density calculated: Faction={Faction}, War={IsWar}, Modifier={Modifier}x",
            controllingFaction ?? "None", isWarZone, totalDensity);

        return totalDensity;
    }

    /// <summary>
    /// Get faction-specific hazard density modifier
    /// </summary>
    private double GetFactionModifier(string? faction)
    {
        return faction switch
        {
            "GodSleeperCultists" => 1.25,  // +25% hazards (reality corruption from rituals)
            "Independents" => 0.85,        // -15% hazards (safer routes, better maintained)
            "IronBanes" => 0.90,           // -10% hazards (active patrols clear corruption)
            "JotunReaders" => 1.0,         // No change (research focus, not environmental)
            "RustClans" => 0.95,           // -5% hazards (trade routes better maintained)
            null => 1.0,                   // No faction control = normal hazards
            _ => 1.0
        };
    }

    /// <summary>
    /// Calculate hazard spawn chance based on density
    /// </summary>
    public double CalculateHazardSpawnChance(double baseSp awnChance, double densityMultiplier)
    {
        double modifiedChance = baseSpawnChance * densityMultiplier;

        // Clamp to reasonable limits (1% to 50%)
        modifiedChance = Math.Clamp(modifiedChance, 0.01, 0.50);

        _log.Debug("Hazard spawn chance: Base={Base}, Density={Density}x, Result={Result}",
            baseSpawnChance, densityMultiplier, modifiedChance);

        return modifiedChance;
    }

    /// <summary>
    /// Get hazard density tier description
    /// </summary>
    public string GetHazardDensityTier(double densityMultiplier)
    {
        return densityMultiplier switch
        {
            >= 1.40 => "Extreme",      // War zones with God-Sleepers
            >= 1.20 => "Very High",    // God-Sleeper control or contested wars
            >= 1.10 => "High",         // Standard war zones
            >= 0.95 => "Normal",       // Neutral or balanced control
            >= 0.85 => "Low",          // Iron-Banes or Independents
            _ => "Very Low"            // Heavily patrolled/maintained areas
        };
    }

    /// <summary>
    /// Calculate expected hazard count in a sector
    /// </summary>
    public int CalculateExpectedHazardCount(int baseCount, double densityMultiplier)
    {
        int modifiedCount = (int)(baseCount * densityMultiplier);

        _log.Debug("Expected hazard count: Base={Base}, Density={Density}x, Result={Result}",
            baseCount, densityMultiplier, modifiedCount);

        return Math.Max(0, modifiedCount);
    }

    /// <summary>
    /// Get environmental description based on hazard density
    /// </summary>
    public string GetEnvironmentalDescription(string? faction, bool isWarZone, double densityMultiplier)
    {
        if (isWarZone && faction == "GodSleeperCultists")
        {
            return "Reality corruption surges violently through the sector. The awakening rituals have destabilized coherence zones, spawning frequent hazards.";
        }

        if (isWarZone)
        {
            return "Collateral damage from ongoing warfare has significantly increased environmental hazards. Corrupted zones expand as reality coherence degrades.";
        }

        return faction switch
        {
            "GodSleeperCultists" => "Ritual activity causes periodic reality distortions. Hazard density elevated due to intentional coherence manipulation.",
            "IronBanes" => "Active patrols keep corruption at bay. Regular purges maintain lower hazard levels.",
            "Independents" => "Well-maintained routes and careful hazard management keep this sector relatively safe.",
            "RustClans" => "Trade route maintenance reduces hazard frequency along major paths.",
            "JotunReaders" => "Research facilities maintain standard environmental controls.",
            _ => "Standard hazard density for this sector type."
        };
    }

    /// <summary>
    /// Apply temporary density modifier from events
    /// </summary>
    public double ApplyEventModifier(double baseDensity, string eventType, int daysRemaining)
    {
        double eventModifier = eventType switch
        {
            "Catastrophe" => 1.5,          // +50% from catastrophe
            "Awakening_Ritual" => 1.3,     // +30% from awakening
            "Supply_Raid" => 1.1,          // +10% from disruption
            _ => 1.0                       // No modifier
        };

        // Event modifiers decay over time
        double decayFactor = daysRemaining > 0 ? 1.0 : 0.0;

        double totalDensity = baseDensity * (1.0 + (eventModifier - 1.0) * decayFactor);

        _log.Debug(
            "Event modifier applied: Event={EventType}, Days={Days}, Modifier={Modifier}x, Result={Result}x",
            eventType, daysRemaining, eventModifier, totalDensity);

        return totalDensity;
    }
}
