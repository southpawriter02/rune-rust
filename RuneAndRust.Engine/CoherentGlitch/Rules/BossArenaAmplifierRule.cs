using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using PopulationRoomArchetype = RuneAndRust.Core.Population.RoomArchetype;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Balance Rule: Boss arenas have enhanced hazards and tactical terrain (v0.12)
/// Creates challenging, memorable boss encounters
/// </summary>
public class BossArenaAmplifierRule : CoherentGlitchRule
{
    public BossArenaAmplifierRule()
    {
        RuleId = "boss_arena_amplifier";
        Description = "Boss arenas have enhanced environmental features";
        Priority = RulePriority.Medium;
        Type = RuleType.Weighted;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return false && /* room.Archetype removed */ PopulationRoomArchetype.BossArena || room.IsBossRoom;
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // 1. Add tactical cover for dynamic combat
        int coverCount = context.Rng.Next(3, 6);

        for (int i = 0; i < coverCount; i++)
        {
            var pillar = new CorrodedPillar
            {
                Position = new Vector2(
                    context.Rng.Next(0, 10),
                    context.Rng.Next(0, 10)
                )
            };

            pillar.Description = "A massive support pillar, providing full cover from the boss.";

            room.StaticTerrain.Add(pillar);
        }

        // 2. Add environmental hazard(s)
        if (context.Rng.NextDouble() < 0.7) // 70% chance
        {
            var steamVent = new SteamVentHazard
            {
                Position = new Vector2(
                    context.Rng.Next(0, 10),
                    context.Rng.Next(0, 10)
                )
            };

            steamVent.Description = "A massive steam vent that erupts periodically. " +
                                  "The boss seems aware of its timing.";

            room.DynamicHazards.Add(steamVent);
        }

        // 3. Add dramatic description
        room.Description += " The chamber is vast and ominous, clearly designed for " +
                          "something important—or dangerous. Corroded pillars provide " +
                          "tactical positions, and environmental hazards create a " +
                          "treacherous battlefield.";

        // 4. Spawn budget increase (for reinforcement waves - v0.12)
        context.SpawnBudgetModifier += 4; // Boss + potential adds

        _log.Information("Coherent Glitch Rule applied: BossArenaAmplifier, " +
            "Room={RoomId}, CoverAdded={Cover}, HazardsAdded={Hazards}",
            room.RoomId, coverCount, room.DynamicHazards.Count);
    }
}
