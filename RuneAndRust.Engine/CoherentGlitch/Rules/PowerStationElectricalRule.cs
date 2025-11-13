using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Contextual Rule: Power stations have higher electrical hazard density (v0.12)
/// Logical consistency: power infrastructure = electrical hazards
/// </summary>
public class PowerStationElectricalRule : CoherentGlitchRule
{
    private static readonly string[] PowerKeywords = new[]
    {
        "power", "electrical", "generator", "reactor", "substation",
        "transformer", "conduit", "grid"
    };

    public PowerStationElectricalRule()
    {
        RuleId = "power_station_electrical";
        Description = "Power stations have increased electrical hazard probability";
        Priority = RulePriority.Medium;
        Type = RuleType.Contextual;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return PowerKeywords.Any(k =>
            room.Name.Contains(k, StringComparison.OrdinalIgnoreCase) ||
            room.Description.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Increase [Live Power Conduit] weight significantly
        context.BiomeElements.ModifyWeight("live_power_conduit", 3.0f);

        // Add machinery wreckage (broken power equipment)
        var wreckage = new MachineryWreckage
        {
            MachineryType = "power distribution equipment",
            Position = new Vector2(
                context.Rng.Next(0, 10),
                context.Rng.Next(0, 10)
            )
        };

        wreckage.Description = "The shattered remains of a power distribution node. " +
                             "Exposed wiring crackles ominously.";

        room.StaticTerrain.Add(wreckage);

        // Add ambient description
        room.Description += " The air smells of ozone and burnt insulation. " +
                          "Emergency power systems spark and flicker erratically.";

        _log.Information("Coherent Glitch Rule applied: PowerStationElectrical, " +
            "Room={RoomId}",
            room.RoomId);
    }
}
