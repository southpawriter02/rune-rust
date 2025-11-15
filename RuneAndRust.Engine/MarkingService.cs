using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a marking operation
/// </summary>
public class MarkingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int BonusDamage { get; set; }
    public int AllyBonusDamage { get; set; }
    public int Duration { get; set; }
    public int StressCost { get; set; }
    public List<string> RevealedVulnerabilities { get; set; } = new();
    public string CorruptionLevel { get; set; } = string.Empty;
}

/// <summary>
/// v0.24.1: Service for managing Mark for Death targeting system
/// Used by Veiðimaðr (Hunter) specialization to designate priority targets
/// </summary>
public class MarkingService
{
    private static readonly ILogger _log = Log.ForContext<MarkingService>();
    private readonly CorruptionTrackingService _corruptionService;

    public MarkingService(string connectionString)
    {
        _corruptionService = new CorruptionTrackingService(connectionString);
        _log.Debug("MarkingService initialized");
    }

    /// <summary>
    /// Apply Mark for Death to a target
    /// </summary>
    public MarkingResult ApplyMarkForDeath(PlayerCharacter hunter, Enemy target, int abilityRank)
    {
        _log.Information("Applying Mark for Death: Hunter={Hunter}, Target={Target}, Rank={Rank}",
            hunter.Name, target.Name, abilityRank);

        try
        {
            // Calculate parameters based on rank
            int duration = abilityRank >= 2 ? 4 : 3;
            int bonusDamage = abilityRank switch
            {
                1 => 8,
                2 => 12,
                3 => 15,
                _ => 8
            };
            int stressCost = abilityRank switch
            {
                3 => 2,
                2 => 3,
                _ => 5
            };
            bool allyBonus = abilityRank >= 3;
            int allyBonusDamage = allyBonus ? 5 : 0;

            // Apply Psychic Stress to hunter
            hunter.PsychicStress = Math.Min(100, hunter.PsychicStress + stressCost);

            // Apply Marked status effect to target
            var markedEffect = new StatusEffect
            {
                Type = StatusEffectType.Marked,
                Duration = duration,
                SourceId = hunter.GetHashCode(), // Using character hash as ID
                Metadata = new Dictionary<string, object>
                {
                    { "BonusDamageFromSource", bonusDamage },
                    { "BonusDamageFromAllies", allyBonusDamage },
                    { "SourceName", hunter.Name }
                }
            };

            target.StatusEffects.Add(markedEffect);

            _log.Information(
                "Mark for Death applied: Duration={Duration}, BonusDamage={Bonus}, AllyBonus={AllyBonus}, StressCost={Stress}",
                duration, bonusDamage, allyBonusDamage, stressCost);

            var result = new MarkingResult
            {
                Success = true,
                Message = $"{target.Name} marked for death ({duration} turns)",
                BonusDamage = bonusDamage,
                AllyBonusDamage = allyBonusDamage,
                Duration = duration,
                StressCost = stressCost
            };

            // If Stalker of the Unseen passive is active, reveal vulnerabilities
            var stalkerRank = GetAbilityRank(hunter, "Stalker of the Unseen");
            if (stalkerRank > 0)
            {
                result = RevealVulnerabilities(hunter, target, stalkerRank, result);
            }

            return result;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error applying Mark for Death");
            return new MarkingResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Reveal target vulnerabilities when Stalker of the Unseen is active
    /// </summary>
    private MarkingResult RevealVulnerabilities(PlayerCharacter hunter, Enemy target, int stalkerRank, MarkingResult result)
    {
        // Determine how many vulnerabilities to reveal
        int vulnerabilitiesToReveal = stalkerRank switch
        {
            3 => int.MaxValue, // All vulnerabilities
            2 => 2,            // 2 vulnerabilities
            _ => 1             // 1 vulnerability
        };

        // Get corruption level
        var corruptionLevel = _corruptionService.GetEnemyCorruptionLevel(target);
        result.CorruptionLevel = corruptionLevel.ToString();

        // Reveal vulnerabilities (simulate - in real implementation would access enemy data)
        var revealed = new List<string>();

        // Example vulnerabilities (in real implementation, read from enemy data)
        var allVulnerabilities = new List<string> { "Fire", "Lightning", "Physical (Piercing)" };

        int count = Math.Min(vulnerabilitiesToReveal, allVulnerabilities.Count);
        revealed.AddRange(allVulnerabilities.Take(count));

        result.RevealedVulnerabilities = revealed;

        _log.Information(
            "Stalker of the Unseen: Revealed {Count} vulnerabilities: {Vulns}, Corruption: {Level}",
            revealed.Count, string.Join(", ", revealed), corruptionLevel);

        result.Message += $" | Vulnerabilities: {string.Join(", ", revealed)} | Corruption: {corruptionLevel}";

        return result;
    }

    /// <summary>
    /// Check if target is currently marked
    /// </summary>
    public bool IsMarked(Enemy target)
    {
        return target.StatusEffects.Any(e => e.Type == StatusEffectType.Marked);
    }

    /// <summary>
    /// Get bonus damage from marking (for the marker)
    /// </summary>
    public int GetMarkBonusDamage(Enemy target, PlayerCharacter attacker)
    {
        var markedEffect = target.StatusEffects.FirstOrDefault(e => e.Type == StatusEffectType.Marked);
        if (markedEffect == null) return 0;

        // Check if this attacker is the source of the mark
        int sourceId = markedEffect.SourceId;
        if (sourceId == attacker.GetHashCode())
        {
            return (int)(markedEffect.Metadata?["BonusDamageFromSource"] ?? 0);
        }

        // Otherwise, get ally bonus if available
        return (int)(markedEffect.Metadata?["BonusDamageFromAllies"] ?? 0);
    }

    /// <summary>
    /// Remove Mark for Death from target
    /// </summary>
    public void RemoveMark(Enemy target)
    {
        target.StatusEffects.RemoveAll(e => e.Type == StatusEffectType.Marked);
        _log.Debug("Mark for Death removed from {Target}", target.Name);
    }

    /// <summary>
    /// Get ability rank for a character (helper method)
    /// </summary>
    private int GetAbilityRank(PlayerCharacter character, string abilityName)
    {
        var ability = character.Abilities.FirstOrDefault(a => a.Name == abilityName);
        return ability?.CurrentRank ?? 0;
    }
}
