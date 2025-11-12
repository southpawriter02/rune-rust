using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Tactical Rule: Long corridors have ambush positioning (v0.12)
/// Enemies positioned at far end to create tension
/// </summary>
public class LongCorridorAmbushRule : CoherentGlitchRule
{
    public LongCorridorAmbushRule()
    {
        RuleId = "long_corridor_ambush";
        Description = "Long corridors position enemies for ambush tactics";
        Priority = RulePriority.Medium;
        Type = RuleType.Tactical;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.Archetype == RoomArchetype.Corridor &&
               room.DormantProcesses.Any();
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Position enemies at far end of corridor
        foreach (var enemy in room.DormantProcesses)
        {
            // Far end of corridor
            enemy.SpawnPosition = new Vector2(9, 5);

            // Add behavior note
            enemy.BehaviorNote = "Positioned at the far end of the corridor. " +
                                "Will engage when player enters.";
        }

        // Add cover mid-corridor for tactical advancement
        var cover = new RubblePile
        {
            Position = new Vector2(5, 5),
            ProvidesTouchCover = true
        };

        cover.Description = "Debris providing cover in the middle of the corridor. " +
                          "Useful for breaking line of sight.";

        room.StaticTerrainFeatures.Add(cover);

        // Add tension to description
        room.Description += " The corridor stretches into darkness. " +
                          "Something moves at the far end.";

        _log.Information("Coherent Glitch Rule applied: LongCorridorAmbush, " +
            "Room={RoomId}, EnemiesRepositioned={Count}",
            room.RoomId, room.DormantProcesses.Count);
    }
}
