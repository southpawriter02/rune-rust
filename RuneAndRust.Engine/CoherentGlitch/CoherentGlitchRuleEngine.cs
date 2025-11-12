using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.CoherentGlitch;

/// <summary>
/// Manages execution of Coherent Glitch rules for environmental storytelling (v0.12)
/// </summary>
public class CoherentGlitchRuleEngine
{
    private static readonly ILogger _log = Log.ForContext<CoherentGlitchRuleEngine>();
    private List<CoherentGlitchRule> _rules = new List<CoherentGlitchRule>();

    public CoherentGlitchRuleEngine()
    {
        // Register all rules
        RegisterRules();
    }

    /// <summary>
    /// Registers all Coherent Glitch rules for a biome
    /// </summary>
    private void RegisterRules()
    {
        // Mandatory Rules (Priority.Critical)
        _rules.Add(new Rules.UnstableCeilingRubbleRule());

        // Weighted Rules (Priority.High)
        _rules.Add(new Rules.FloodedElectricalDangerRule());
        _rules.Add(new Rules.DarknessStressAmplifierRule());

        // Exclusion Rules (Priority.High)
        _rules.Add(new Rules.NoSteamInFloodedRule());
        _rules.Add(new Rules.EntryHallSafetyRule());

        // Contextual Rules (Priority.Medium)
        _rules.Add(new Rules.GeothermalSteamRule());
        _rules.Add(new Rules.MaintenanceHubOrganizationRule());
        _rules.Add(new Rules.PowerStationElectricalRule());

        // Tactical Rules (Priority.Medium)
        _rules.Add(new Rules.TacticalCoverPlacementRule());
        _rules.Add(new Rules.LongCorridorAmbushRule());

        // Balance Rules (Priority.Medium)
        _rules.Add(new Rules.SecretRoomRewardRule());
        _rules.Add(new Rules.BossArenaAmplifierRule());

        // Narrative Chain Rules (Priority.Low)
        _rules.Add(new Rules.FailedEvacuationNarrativeRule());
        _rules.Add(new Rules.BrokenMaintenanceCycleRule());
        _rules.Add(new Rules.ChasmInfrastructureRule());

        // Polish Rules (Priority.Low)
        _rules.Add(new Rules.HiddenContainerDiscoveryRule());
        _rules.Add(new Rules.ResourceVeinClusterRule());

        _log.Information("Coherent Glitch Rule Engine initialized: {RuleCount} rules registered", _rules.Count);
    }

    /// <summary>
    /// Applies all Coherent Glitch rules to a room
    /// </summary>
    public void ApplyRules(Room room, PopulationContext context)
    {
        context.CurrentRoom = room;

        using (_log.BeginScope("CoherentGlitchRules-{RoomId}", room.RoomId))
        {
            _log.Debug("Applying Coherent Glitch rules: Room={RoomId}, RulesAvailable={RuleCount}",
                room.RoomId, _rules.Count);

            // Group rules by priority
            var rulesByPriority = _rules
                .GroupBy(r => r.Priority)
                .OrderBy(g => g.Key)
                .ToList();

            int totalFired = 0;

            // Execute rules in priority order
            foreach (var group in rulesByPriority)
            {
                foreach (var rule in group)
                {
                    if (rule.Execute(room, context))
                    {
                        totalFired++;
                    }
                }
            }

            _log.Information("Coherent Glitch rules applied: Room={RoomId}, RulesFired={RulesFired}/{TotalRules}",
                room.RoomId, totalFired, _rules.Count);
        }
    }

    /// <summary>
    /// Applies rules to all rooms in a dungeon
    /// </summary>
    public void ApplyRulesToDungeon(Dungeon dungeon, PopulationContext context)
    {
        context.CurrentDungeon = dungeon;

        using (_log.BeginScope("CoherentGlitch-Dungeon-{DungeonId}", dungeon.DungeonId))
        {
            _log.Information("Applying Coherent Glitch rules to dungeon: DungeonId={DungeonId}, Rooms={RoomCount}",
                dungeon.DungeonId, dungeon.TotalRoomCount);

            foreach (var room in dungeon.Rooms.Values)
            {
                ApplyRules(room, context);
            }

            _log.Information("Coherent Glitch rules complete: DungeonId={DungeonId}, TotalRulesFired={TotalRulesFired}",
                dungeon.DungeonId, context.TotalRulesFired);

            // Log rule fire statistics
            foreach (var kvp in context.RuleFireCounts.OrderByDescending(x => x.Value))
            {
                _log.Debug("Rule fire count: Rule={RuleId}, Count={Count}",
                    kvp.Key, kvp.Value);
            }
        }
    }

    /// <summary>
    /// Gets all registered rules
    /// </summary>
    public IReadOnlyList<CoherentGlitchRule> GetRules()
    {
        return _rules.AsReadOnly();
    }
}
