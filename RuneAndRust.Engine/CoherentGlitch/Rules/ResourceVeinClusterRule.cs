using RuneAndRust.Core;
using RuneAndRust.Core.Descriptors;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Contextual Rule: Resource veins appear in clusters (v0.12)
/// Geological realism: ore deposits cluster together
/// </summary>
public class ResourceVeinClusterRule : CoherentGlitchRule
{
    public ResourceVeinClusterRule()
    {
        RuleId = "resource_vein_cluster";
        Description = "Resource veins spawn in clusters for realism";
        Priority = RulePriority.Low;
        Type = RuleType.Contextual;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.LootNodes.Any(l => l is ResourceVein);
    }

    public override void Apply(Room room, PopulationContext context)
    {
        var existingVeins = room.LootNodes.OfType<ResourceVein>().ToList();

        if (existingVeins.Count > 0)
        {
            // 50% chance to add 1-2 more veins nearby
            if (context.Rng.NextDouble() < 0.5)
            {
                int additionalVeins = context.Rng.Next(1, 3);

                for (int i = 0; i < additionalVeins; i++)
                {
                    var baseVein = existingVeins[context.Rng.Next(existingVeins.Count)];

                    var newVein = new ResourceVein
                    {
                        ResourceType = baseVein.ResourceType,
                        Quality = LootQuality.Common,
                        EstimatedCogsValue = 25,
                        Position = new Vector2(
                            baseVein.Position.X + context.Rng.Next(-2, 3),
                            baseVein.Position.Y + context.Rng.Next(-2, 3)
                        )
                    };

                    newVein.Description = $"Another deposit of {baseVein.ResourceType}, " +
                                        "part of a larger vein.";

                    room.LootNodes.Add(newVein);
                }

                // Add detail to room description
                room.Description += " Rich deposits of salvageable materials cluster " +
                                  "along the walls.";

                _log.Information("Coherent Glitch Rule applied: ResourceVeinCluster, " +
                    "Room={RoomId}, VeinsAdded={Count}",
                    room.RoomId, additionalVeins);
            }
        }
    }
}
