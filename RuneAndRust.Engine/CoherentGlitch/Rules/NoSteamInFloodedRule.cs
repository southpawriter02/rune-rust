using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Exclusion Rule: [Steam Vents] cannot coexist with [Flooded] (v0.12)
/// Physical impossibility: steam vents don't work underwater
/// </summary>
public class NoSteamInFloodedRule : CoherentGlitchRule
{
    public NoSteamInFloodedRule()
    {
        RuleId = "no_steam_in_flooded";
        Description = "Steam vents cannot spawn in flooded rooms";
        Priority = RulePriority.High;
        Type = RuleType.Exclusion;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.AmbientConditions.Any() /* TODO: filter by type */;
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Remove [Steam Vent] from spawn pool
        context.BiomeElements.ExcludeElement("steam_vent");

        // Remove any already-spawned steam vents
        var steamVents = room.DynamicHazards.OfType<SteamVentHazard>().ToList();

        foreach (var vent in steamVents)
        {
            room.DynamicHazards.Remove(vent);

            _log.Warning("Coherent Glitch Rule removed invalid hazard: NoSteamInFlooded, " +
                "Room={RoomId}, RemovedHazard={HazardId}",
                room.RoomId, vent.Id);
        }

        if (steamVents.Any())
        {
            _log.Information("Coherent Glitch Rule applied: NoSteamInFlooded, " +
                "Room={RoomId}, RemovedVents={Count}",
                room.RoomId, steamVents.Count);
        }
    }
}
