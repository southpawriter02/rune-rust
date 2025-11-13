using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Weighted Rule: [Flooded] rooms enhance electrical hazards (v0.12)
/// Water conducts electricity - logical environmental interaction
/// </summary>
public class FloodedElectricalDangerRule : CoherentGlitchRule
{
    public FloodedElectricalDangerRule()
    {
        RuleId = "flooded_electrical_danger";
        Description = "Flooded condition enhances electrical hazards";
        Priority = RulePriority.High;
        Type = RuleType.Weighted;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.AmbientConditions.Any() /* TODO: filter by type */;
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // 1. Increase [Live Power Conduit] spawn weight
        var powerConduitElement = context.BiomeElements.GetElement("live_power_conduit");

        if (powerConduitElement != null)
        {
            powerConduitElement.Weight *= 2.5f;  // Was 0.20, now 0.50

            _log.Debug("Coherent Glitch Rule applied: FloodedElectricalDanger, " +
                "Room={RoomId}, NewWeight={Weight}",
                room.RoomId, powerConduitElement.Weight);
        }

        // 2. Enhance existing power conduits
        foreach (var hazard in room.DynamicHazards.OfType<LivePowerConduitHazard>())
        {
            hazard.DamageMultiplier = 2.0; // Double damage
            hazard.RangeMultiplier = 1.5; // +50% range (water conducts)
            hazard.IsFloodedEnhanced = true;
            hazard.IsEnhancedByRule = true;

            hazard.Description = "Exposed electrical wiring crackles violently, its charge " +
                               "conducted through the ankle-deep water. Extremely dangerous.";

            _log.Information("Coherent Glitch Rule enhanced hazard: FloodedElectrical, " +
                "Hazard={HazardId}, DamageMult={DamageMult}",
                hazard.Id, hazard.DamageMultiplier);
        }

        // 3. Add description detail to room
        if (!room.Description.Contains("electrical"))
        {
            room.Description += " The stagnant water conducts residual electrical charge, " +
                              "creating a lethal hazard.";
        }
    }
}
