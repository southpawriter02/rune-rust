using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Tactical Rule: Cover is positioned near enemy spawns for tactical gameplay (v0.12)
/// Creates interesting combat scenarios where player can use environment
/// </summary>
public class TacticalCoverPlacementRule : CoherentGlitchRule
{
    public TacticalCoverPlacementRule()
    {
        RuleId = "tactical_cover_placement";
        Description = "Cover terrain positioned tactically relative to enemies";
        Priority = RulePriority.Medium;
        Type = RuleType.Tactical;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.DormantProcesses.Any();
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Find enemy positions
        var enemyPositions = room.DormantProcesses
            .Where(e => e.SpawnPosition.HasValue)
            .Select(e => e.SpawnPosition!.Value)
            .ToList();

        if (enemyPositions.Count == 0)
            return;

        // Find existing cover terrain
        var coverTerrain = room.StaticTerrainFeatures
            .Where(t => t.ProvidesTouchCover)
            .ToList();

        if (coverTerrain.Count == 0)
        {
            // No cover exists - spawn some near enemies
            int coverToSpawn = Math.Min(2, enemyPositions.Count);

            for (int i = 0; i < coverToSpawn; i++)
            {
                var enemyPos = enemyPositions[context.Rng.Next(enemyPositions.Count)];
                var coverPos = FindTacticalCoverPosition(enemyPos, room, context.Rng);

                var cover = new RubblePile
                {
                    Position = coverPos,
                    ProvidesTouchCover = true
                };

                cover.Description = "Debris strategically positioned for cover. " +
                                  "Almost as if someone placed it here deliberately.";

                room.StaticTerrainFeatures.Add(cover);

                _log.Debug("Coherent Glitch Rule spawned tactical cover: " +
                    "Room={RoomId}, CoverPos={Position}",
                    room.RoomId, coverPos);
            }

            _log.Information("Coherent Glitch Rule applied: TacticalCoverPlacement, " +
                "Room={RoomId}, CoverSpawned={Count}",
                room.RoomId, coverToSpawn);
        }
        else
        {
            // Reposition existing cover for better tactics
            int repositioned = 0;

            foreach (var cover in coverTerrain.Take(2))
            {
                var enemyPos = enemyPositions[context.Rng.Next(enemyPositions.Count)];
                var newPos = FindTacticalCoverPosition(enemyPos, room, context.Rng);

                cover.Position = newPos;
                repositioned++;

                _log.Debug("Coherent Glitch Rule repositioned cover: " +
                    "Room={RoomId}, NewPos={Position}",
                    room.RoomId, newPos);
            }

            if (repositioned > 0)
            {
                _log.Information("Coherent Glitch Rule applied: TacticalCoverPlacement, " +
                    "Room={RoomId}, CoverRepositioned={Count}",
                    room.RoomId, repositioned);
            }
        }
    }

    /// <summary>
    /// Finds a good tactical position for cover near an enemy
    /// </summary>
    private Vector2 FindTacticalCoverPosition(Vector2 enemyPos, Room room, Random rng)
    {
        // Place cover 3-5 meters from enemy (mid-range distance)
        float distance = 3.0f + (float)(rng.NextDouble() * 2.0f);
        float angle = (float)(rng.NextDouble() * Math.PI * 2);

        return new Vector2(
            enemyPos.X + distance * (float)Math.Cos(angle),
            enemyPos.Y + distance * (float)Math.Sin(angle)
        );
    }
}
