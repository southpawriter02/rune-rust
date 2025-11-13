using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Narrative Chain Rule: "The Failed Evacuation" story (v0.12)
/// Environmental storytelling: Workers tried to flee but didn't make it
/// </summary>
public class FailedEvacuationNarrativeRule : CoherentGlitchRule
{
    public FailedEvacuationNarrativeRule()
    {
        RuleId = "failed_evacuation_narrative";
        Description = "Creates environmental story of failed evacuation attempt";
        Priority = RulePriority.Low;
        Type = RuleType.Contextual;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        // Trigger in corridors near boss arenas (fleeing the catastrophe)
        if (room.Archetype != RoomArchetype.Corridor)
            return false;

        // Check if room is near boss (within 2 rooms)
        if (room.GeneratedNodeType == NodeType.Main)
        {
            // For now, trigger randomly (20% chance)
            // In full implementation, would check graph distance to boss
            return context.Rng.NextDouble() < 0.20;
        }

        return false;
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Narrative: Workers tried to flee the malfunctioning sector

        // 1. Add abandoned equipment (loot)
        var wreckage = new SalvageableWreckage
        {
            Type = WreckageType.Equipment,
            Quality = LootQuality.Uncommon,
            EstimatedCogsValue = 60,
            Position = new Vector2(
                context.Rng.Next(0, 10),
                context.Rng.Next(0, 10)
            )
        };

        wreckage.Description = "Scattered tools and equipment, abandoned mid-task. " +
                             "Someone left in a hurry.";

        room.LootNodes.Add(wreckage);

        // 2. Add personal effects (second loot node)
        var personalEffects = new SalvageableWreckage
        {
            Type = WreckageType.PersonalEffects,
            Quality = LootQuality.Common,
            EstimatedCogsValue = 25,
            Position = new Vector2(
                context.Rng.Next(0, 10),
                context.Rng.Next(0, 10)
            )
        };

        personalEffects.Description = "Personal belongings scattered across the floor. " +
                                     "A technician's datapad, its screen cracked and dark.";

        room.LootNodes.Add(personalEffects);

        // 3. Add environmental detail to description
        room.Description += " Drag marks mar the corroded floor, leading toward the exit. " +
                          "They stop abruptly at a dark stain. Whatever happened here, " +
                          "it happened fast.";

        // 4. Increase hostile enemy presence (what stopped them?)
        context.SpawnBudgetModifier += 2;

        // 5. Add rubble pile (partial collapse during evacuation)
        var rubble = new RubblePile
        {
            Position = new Vector2(
                context.Rng.Next(0, 10),
                context.Rng.Next(0, 10)
            )
        };

        rubble.Description = "Fresh debris partially blocking the corridor. " +
                           "This collapse happened during the evacuation.";

        room.StaticTerrain.Add(rubble);

        _log.Information("Coherent Glitch Narrative applied: FailedEvacuation, " +
            "Room={RoomId}, ElementsAdded={Count}",
            room.RoomId, 4);
    }
}
