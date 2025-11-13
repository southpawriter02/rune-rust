using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.CoherentGlitch;

/// <summary>
/// Base class for Coherent Glitch environmental storytelling rules (v0.12)
/// "The engine's generation is not purely random; it is logically chaotic."
/// </summary>
public abstract class CoherentGlitchRule
{
    protected static readonly ILogger _log = Log.ForContext<CoherentGlitchRule>();

    public string RuleId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RulePriority Priority { get; set; } = RulePriority.Medium;
    public RuleType Type { get; set; } = RuleType.Contextual;

    /// <summary>
    /// Evaluate if this rule should apply to the given room
    /// </summary>
    public abstract bool ShouldApply(Room room, PopulationContext context);

    /// <summary>
    /// Apply the rule's effects to the room
    /// </summary>
    public abstract void Apply(Room room, PopulationContext context);

    /// <summary>
    /// Execute the rule: evaluate and apply if conditions met
    /// </summary>
    public bool Execute(Room room, PopulationContext context)
    {
        try
        {
            if (ShouldApply(room, context))
            {
                _log.Debug("Coherent Glitch Rule firing: Rule={RuleId}, Room={RoomId}",
                    RuleId, room.RoomId);

                Apply(room, context);

                // Track rule application
                // room.CoherentGlitchRulesFired++; // Property removed in v0.11 refactor
                context.TotalRulesFired++;

                if (!context.RuleFireCounts.ContainsKey(RuleId))
                {
                    context.RuleFireCounts[RuleId] = 0;
                }
                context.RuleFireCounts[RuleId]++;

                _log.Information("Coherent Glitch Rule applied: Rule={RuleId}, Room={RoomId}, Type={RuleType}",
                    RuleId, room.RoomId, Type);

                return true;
            }

            _log.Debug("Coherent Glitch Rule evaluated (no match): Rule={RuleId}, Room={RoomId}",
                RuleId, room.RoomId);

            return false;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Coherent Glitch Rule failed: Rule={RuleId}, Room={RoomId}, Error={Error}",
                RuleId, room.RoomId, ex.Message);
            return false;
        }
    }
}

/// <summary>
/// Rule priority determines execution order (v0.12)
/// </summary>
public enum RulePriority
{
    Critical = 0,    // Must apply first (structure, mandatory relationships)
    High = 1,        // Safety and exclusions
    Medium = 2,      // Thematic coherence
    Low = 3          // Polish and aesthetics
}

/// <summary>
/// Rule type classification (v0.12)
/// </summary>
public enum RuleType
{
    Mandatory,       // Must fire if conditions met
    Weighted,        // Modifies spawn probabilities
    Exclusion,       // Prevents invalid combinations
    Contextual,      // Based on room properties
    Tactical         // Gameplay positioning
}
