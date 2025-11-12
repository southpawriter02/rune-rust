using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Contextual Rule: Rooms with "geothermal" keywords get more steam vents (v0.12)
/// Environmental storytelling: thermal pumping stations would have steam hazards
/// </summary>
public class GeothermalSteamRule : CoherentGlitchRule
{
    private static readonly string[] GeothermalKeywords = new[]
    {
        "geothermal", "pumping station", "thermal", "heat exchange",
        "boiler", "steam", "coolant"
    };

    public GeothermalSteamRule()
    {
        RuleId = "geothermal_steam";
        Description = "Geothermal rooms have increased steam vent probability";
        Priority = RulePriority.Medium;
        Type = RuleType.Contextual;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        // Check room name/description for geothermal keywords
        return GeothermalKeywords.Any(k =>
            room.Name.Contains(k, StringComparison.OrdinalIgnoreCase) ||
            room.Description.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Increase [Steam Vent] weight significantly
        var steamVentElement = context.BiomeElements.GetElement("steam_vent");

        if (steamVentElement != null)
        {
            steamVentElement.Weight *= 3.0f;  // Triple the likelihood

            _log.Debug("Coherent Glitch Rule applied: GeothermalSteam, " +
                "Room={RoomId}, NewWeight={Weight}",
                room.RoomId, steamVentElement.Weight);
        }

        // Also add [Extreme Heat] ambient condition if not present
        if (!room.HasAmbientCondition("[Extreme Heat]"))
        {
            room.AmbientConditions.Add(new ExtremeHeatCondition());

            _log.Information("Coherent Glitch Rule added ambient condition: GeothermalSteam, " +
                "Room={RoomId}, Condition=[Extreme Heat]",
                room.RoomId);
        }
    }
}
