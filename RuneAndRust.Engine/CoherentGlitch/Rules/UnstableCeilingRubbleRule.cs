using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Mandatory Rule: [Unstable Ceiling] ALWAYS spawns [Rubble Pile] below (v0.12)
/// Logical consistency: falling debris creates rubble on the floor
/// </summary>
public class UnstableCeilingRubbleRule : CoherentGlitchRule
{
    public UnstableCeilingRubbleRule()
    {
        RuleId = "unstable_ceiling_rubble";
        Description = "Unstable Ceiling hazard requires Rubble Pile on floor";
        Priority = RulePriority.Critical;
        Type = RuleType.Mandatory;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.DynamicHazards.Any(h => h is UnstableCeilingHazard);
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Only add rubble if it doesn't already exist
        if (!room.StaticTerrainFeatures.Any(t => t is RubblePile))
        {
            var rubble = new RubblePile
            {
                Position = DetermineRubblePosition(room),
                IsFromCeilingCollapse = true
            };

            rubble.Description = "Chunks of corroded metal and concrete, freshly fallen from above. " +
                                "The collapse is ongoing.";

            room.StaticTerrainFeatures.Add(rubble);

            _log.Information("Coherent Glitch Rule applied: UnstableCeilingRubble, Room={RoomId}",
                room.RoomId);
        }
    }

    private Vector2 DetermineRubblePosition(Room room)
    {
        // Position rubble near center of room (below ceiling hazard)
        // In full implementation, would use room layout data
        return new Vector2(5, 5);
    }
}
