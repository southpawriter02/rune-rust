using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Narrative Chain Rule: "The Broken Maintenance Cycle" story (v0.12)
/// Haugbui-Class automaton stuck in endless maintenance loop for 800 years
/// </summary>
public class BrokenMaintenanceCycleRule : CoherentGlitchRule
{
    public BrokenMaintenanceCycleRule()
    {
        RuleId = "broken_maintenance_cycle";
        Description = "Creates environmental story of Haugbui stuck in maintenance loop";
        Priority = RulePriority.Low;
        Type = RuleType.Contextual;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        // Requires maintenance room with Haugbui-Class enemy
        if (!room.Name.Contains("maintenance", StringComparison.OrdinalIgnoreCase))
            return false;

        return /* room.Enemies /* DormantProcesses removed, using Enemies */.Any(e =>
            e.ProcessType.Contains("haugbui", StringComparison.OrdinalIgnoreCase) removed */ 0);
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Narrative: Haugbui-Class automaton stuck in maintenance loop

        var haugbui = room.Enemies /* DormantProcesses removed, using Enemies */
            .FirstOrDefault(e => e.ProcessType.Contains("haugbui", StringComparison.OrdinalIgnoreCase));

        if (haugbui != null)
        {
            // 1. Add piles of "organized" rubble (STACK obsession)
            int pileCount = context.Rng.Next(3, 6);

            for (int i = 0; i < pileCount; i++)
            {
                var rubble = new RubblePile
                {
                    IsOrganized = true,
                    Position = new Vector2(
                        context.Rng.Next(0, 10),
                        context.Rng.Next(0, 10)
                    )
                };

                rubble.Description = "Debris meticulously stacked into a perfect cube. " +
                                   "The work of an obsessive automaton. " +
                                   "It's been doing this for 800 years.";

                room.StaticTerrain.Add(rubble);
            }

            // 2. Add broken tools (what it's trying to maintain)
            var tools = new SalvageableWreckage
            {
                Type = WreckageType.Equipment,
                Quality = LootQuality.Common,
                EstimatedCogsValue = 35,
                Position = new Vector2(5, 5)
            };

            tools.Description = "Corroded maintenance tools, arranged in precise rows. " +
                              "The Haugbui has been organizing these for centuries.";

            room.LootNodes.Add(tools);

            // 3. Add machinery it's "maintaining"
            var machinery = new MachineryWreckage
            {
                MachineryType = "environmental systems",
                Position = new Vector2(7, 7)
            };

            machinery.Description = "Ancient environmental control systems, long dead. " +
                                  "The Haugbui continues its futile maintenance routine.";

            room.StaticTerrain.Add(machinery);

            // 4. Modify enemy behavior note
            haugbui.BehaviorNote = "Obsessively organizing debris. " +
                                  "Attacks if work is disturbed. " +
                                  "Has been performing this routine for 800 years.";

            // 5. Add environmental detail
            room.Description += " The chamber is unnaturally organized—" +
                              "every piece of debris stacked with geometric precision. " +
                              "A Haugbui-Class automaton moves methodically through " +
                              "its maintenance routine, oblivious to the passage of time.";

            _log.Information("Coherent Glitch Narrative applied: BrokenMaintenanceCycle, " +
                "Room={RoomId}, Haugbui={EnemyId}, OrganizedPiles={Count}",
                room.RoomId, haugbui.Id, pileCount);
        }
    }
}
