using RuneAndRust.Core;
using RuneAndRust.Core.Descriptors;
using PopulationRoomArchetype = RuneAndRust.Core.Descriptors.RoomArchetype;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Exclusion Rule: Entry Halls must be safe for new players (v0.12)
/// Balance tuning: reduce spawn budget and exclude champions
/// </summary>
public class EntryHallSafetyRule : CoherentGlitchRule
{
    public EntryHallSafetyRule()
    {
        RuleId = "entry_hall_safety";
        Description = "Entry Halls have reduced enemy spawns and no champions";
        Priority = RulePriority.High;
        Type = RuleType.Exclusion;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.Archetype == PopulationRoomArchetype.EntryHall || room.IsStartRoom;
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Reduce spawn budget significantly
        context.SpawnBudgetModifier -= 3; // Was -2, now -3 (v0.12 balance tuning)

        _log.Debug("Coherent Glitch Rule applied: EntryHallSafety, " +
            "Room={RoomId}, SpawnBudgetModifier={Modifier}",
            room.RoomId, context.SpawnBudgetModifier);

        // Remove all champion-tier enemies
        var champions = room.Enemies /* DormantProcesses removed, using Enemies */
            .Where(e => e.ThreatLevel >= ThreatLevel.High || e.IsChampion)
            .ToList();

        foreach (var champion in champions)
        {
            room.Enemies /* DormantProcesses removed, using Enemies */.Remove(champion);

            _log.Warning("Coherent Glitch Rule removed champion from entry hall: " +
                "Room={RoomId}, Enemy={EnemyType}",
                room.RoomId, champion.ProcessType);
        }

        if (champions.Any())
        {
            _log.Information("Coherent Glitch Rule applied: EntryHallSafety, " +
                "Room={RoomId}, RemovedChampions={Count}",
                room.RoomId, champions.Count);
        }
    }
}
