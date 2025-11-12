using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Narrative Chain Rule: Chasms have broken infrastructure remnants (v0.12)
/// Environmental storytelling: "This bridge collapsed during the Glitch"
/// </summary>
public class ChasmInfrastructureRule : CoherentGlitchRule
{
    public ChasmInfrastructureRule()
    {
        RuleId = "chasm_infrastructure";
        Description = "Chasms have broken bridge/gantry remnants telling a story";
        Priority = RulePriority.Low;
        Type = RuleType.Contextual;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.DynamicHazards.Any(h => h is ChasmHazard);
    }

    public override void Apply(Room room, PopulationContext context)
    {
        var chasm = room.DynamicHazards.OfType<ChasmHazard>().First();

        // 1. Add broken gantry/bridge remnants
        var gantry = new MachineryWreckage
        {
            MachineryType = "collapsed gantry",
            Position = chasm.Position ?? new Vector2(5, 5),
            BlocksMovement = true
        };

        gantry.Name = "Collapsed Gantry";
        gantry.Description = "Twisted metal girders jut from the chasm's edge. " +
                           "This bridge failed catastrophically 800 years ago. " +
                           "Whatever stress caused it to fail must have been immense.";

        room.StaticTerrainFeatures.Add(gantry);

        // 2. Add rubble pile near chasm edge
        var rubble = new RubblePile
        {
            Position = new Vector2(
                (chasm.Position?.X ?? 5) + context.Rng.Next(-2, 3),
                (chasm.Position?.Y ?? 5) + context.Rng.Next(-2, 3)
            ),
            IsFromCeilingCollapse = false
        };

        rubble.Description = "Chunks of broken concrete and twisted metal from the collapsed bridge.";

        room.StaticTerrainFeatures.Add(rubble);

        // 3. Add salvageable wreckage (broken equipment that fell)
        if (context.Rng.NextDouble() < 0.6) // 60% chance
        {
            var wreckage = new SalvageableWreckage
            {
                Type = WreckageType.Machinery,
                Quality = LootQuality.Uncommon,
                EstimatedCogsValue = 55,
                Position = new Vector2(
                    (chasm.Position?.X ?? 5) + context.Rng.Next(-3, 4),
                    (chasm.Position?.Y ?? 5) + context.Rng.Next(-3, 4)
                )
            };

            wreckage.Description = "Equipment that fell from the gantry during the collapse. " +
                                 "Miraculously, some of it might still be salvageable.";

            room.LootNodes.Add(wreckage);
        }

        // 4. Add detail to room description
        room.Description += $" A {chasm.Width:F1}-meter chasm splits the floor, " +
                          "its edges jagged with the torn remnants of a gantry. " +
                          "Whatever event caused this catastrophic failure must have " +
                          "happened in the blink of an eye—800 years ago.";

        // 5. Update chasm description
        chasm.Description = $"A {chasm.Width:F1}-meter chasm carved into the floor, " +
                          "its bottom lost in absolute darkness. The twisted remains " +
                          "of a collapsed gantry bridge tell a story of catastrophic failure.";

        _log.Information("Coherent Glitch Narrative applied: ChasmInfrastructure, " +
            "Room={RoomId}, ChasmWidth={Width}",
            room.RoomId, chasm.Width);
    }
}
