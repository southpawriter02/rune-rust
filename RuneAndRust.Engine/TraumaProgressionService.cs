using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages trauma progression and worsening over time
/// v0.15: Trauma Economy
/// </summary>
public class TraumaProgressionService
{
    private static readonly ILogger _log = Log.ForContext<TraumaProgressionService>();
    private readonly Random _rng;
    private const int DaysUntilProgressionCheck = 7; // Check every 7 days
    private const float ProgressionChance = 0.5f; // 50% chance to worsen

    public TraumaProgressionService(Random? rng = null)
    {
        _rng = rng ?? new Random();
    }

    /// <summary>
    /// Checks all traumas for progression (called when resting or on day transitions)
    /// </summary>
    /// <param name="character">The character whose traumas to check</param>
    /// <param name="daysElapsed">Number of days that have elapsed (usually 1)</param>
    /// <returns>List of traumas that progressed</returns>
    public List<Trauma> CheckTraumaProgression(PlayerCharacter character, int daysElapsed = 1)
    {
        var progressedTraumas = new List<Trauma>();

        foreach (var trauma in character.Traumas)
        {
            trauma.DaysSinceManagement += daysElapsed;

            // Check if ready for progression check
            if (trauma.DaysSinceManagement >= DaysUntilProgressionCheck && trauma.ProgressionLevel < 3)
            {
                // Roll for progression
                if (_rng.NextDouble() < ProgressionChance)
                {
                    ProgressTrauma(character, trauma);
                    progressedTraumas.Add(trauma);
                }
                else
                {
                    _log.Debug("Trauma did not progress: Character={CharacterName}, Trauma={TraumaId}, Roll failed",
                        character.Name, trauma.TraumaId);
                }
            }
        }

        if (progressedTraumas.Count > 0)
        {
            _log.Warning("Traumas progressed: Character={CharacterName}, Count={Count}",
                character.Name, progressedTraumas.Count);
        }

        return progressedTraumas;
    }

    /// <summary>
    /// Forces a trauma to worsen to the next progression level
    /// </summary>
    private void ProgressTrauma(PlayerCharacter character, Trauma trauma)
    {
        int oldLevel = trauma.ProgressionLevel;
        trauma.ProgressionLevel++;

        _log.Warning("Trauma progressed: Character={CharacterName}, Trauma={TraumaId}, OldLevel={OldLevel}, NewLevel={NewLevel}",
            character.Name, trauma.TraumaId, oldLevel, trauma.ProgressionLevel);

        // Update severity
        trauma.Severity = trauma.ProgressionLevel switch
        {
            1 => TraumaSeverity.Mild,
            2 => TraumaSeverity.Moderate,
            3 => TraumaSeverity.Severe,
            _ => TraumaSeverity.Severe
        };

        // Increase effects based on new level
        IncreaseTraumaEffects(trauma, trauma.ProgressionLevel);

        // Reset management counter (give player time to address it)
        trauma.DaysSinceManagement = 0;
    }

    /// <summary>
    /// Increases the intensity of trauma effects based on progression level
    /// </summary>
    private void IncreaseTraumaEffects(Trauma trauma, int level)
    {
        float multiplier = level switch
        {
            1 => 1.0f,    // Base effects
            2 => 1.5f,    // 50% stronger
            3 => 2.0f,    // 100% stronger (double)
            _ => 2.0f
        };

        foreach (var effect in trauma.Effects)
        {
            switch (effect)
            {
                case StressMultiplierEffect stressEffect:
                    // Increase stress multiplier
                    var baseMultiplier = (stressEffect.Multiplier - 1.0f); // Get bonus part
                    stressEffect.Multiplier = 1.0f + (baseMultiplier * multiplier);
                    _log.Debug("Increased StressMultiplier: Trauma={TraumaId}, NewMultiplier={Multiplier}",
                        trauma.TraumaId, stressEffect.Multiplier);
                    break;

                case PassiveStressEffect passiveEffect:
                    // Increase passive stress
                    int baseStress = passiveEffect.StressPerTurn / (int)Math.Max(1, multiplier / 1.5f);
                    passiveEffect.StressPerTurn = (int)(baseStress * multiplier);
                    _log.Debug("Increased PassiveStress: Trauma={TraumaId}, NewStress={Stress}",
                        trauma.TraumaId, passiveEffect.StressPerTurn);
                    break;

                case AttributePenaltyEffect attrEffect:
                    // Increase attribute penalty
                    int basePenalty = attrEffect.Penalty / (int)Math.Max(1, multiplier / 1.5f);
                    attrEffect.Penalty = (int)(basePenalty * multiplier);
                    _log.Debug("Increased AttributePenalty: Trauma={TraumaId}, Attribute={Attr}, NewPenalty={Penalty}",
                        trauma.TraumaId, attrEffect.AttributeName, attrEffect.Penalty);
                    break;

                case RestRestrictionEffect restEffect:
                    // Worsen rest effectiveness
                    if (restEffect.EffectivenessMultiplier.HasValue)
                    {
                        restEffect.EffectivenessMultiplier = Math.Max(0.25f, restEffect.EffectivenessMultiplier.Value * 0.8f);
                        _log.Debug("Worsened RestRestriction: Trauma={TraumaId}, NewMultiplier={Multiplier}",
                            trauma.TraumaId, restEffect.EffectivenessMultiplier);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Gets progression narrative for display when a trauma worsens
    /// </summary>
    public string GetProgressionNarrative(Trauma trauma, int oldLevel, int newLevel)
    {
        var narrative = new System.Text.StringBuilder();

        narrative.AppendLine();
        narrative.AppendLine("═══════════════════════════════════════════════════════");
        narrative.AppendLine("              === TRAUMA PROGRESSION ===");
        narrative.AppendLine("═══════════════════════════════════════════════════════");
        narrative.AppendLine();
        narrative.AppendLine($"Your {trauma.Name} has worsened.");
        narrative.AppendLine();
        narrative.AppendLine($"  Progression: Level {oldLevel} → Level {newLevel}");
        narrative.AppendLine($"  Severity: {trauma.Severity}");
        narrative.AppendLine();

        // Show new effects
        narrative.AppendLine("  Current Effects:");
        foreach (var effect in trauma.Effects)
        {
            narrative.AppendLine($"    - {effect.GetDescription()}");
        }

        narrative.AppendLine();

        // Add flavor text based on trauma and level
        narrative.AppendLine(newLevel switch
        {
            2 => "  The weight of it grows heavier. Harder to ignore.",
            3 => "  It consumes everything. You can barely remember who you were before.",
            _ => "  The trauma deepens."
        });

        narrative.AppendLine();
        narrative.AppendLine("  You must address this trauma through rest or therapy");
        narrative.AppendLine("  to prevent further deterioration.");
        narrative.AppendLine("═══════════════════════════════════════════════════════");

        return narrative.ToString();
    }

    /// <summary>
    /// Gets a summary of all traumas and their progression status
    /// </summary>
    public string GetTraumaSummary(PlayerCharacter character)
    {
        if (character.Traumas.Count == 0)
        {
            return "No active traumas.";
        }

        var summary = new System.Text.StringBuilder();
        summary.AppendLine();
        summary.AppendLine("═══════════════ ACTIVE TRAUMAS ═══════════════");

        foreach (var trauma in character.Traumas)
        {
            summary.AppendLine();
            summary.AppendLine($"{trauma.Name}");
            summary.AppendLine($"  \"{trauma.Description}\"");
            summary.AppendLine($"  Level: {trauma.ProgressionLevel}/3 | Severity: {trauma.Severity}");
            summary.AppendLine($"  Days since managed: {trauma.DaysSinceManagement}");

            if (trauma.DaysSinceManagement >= DaysUntilProgressionCheck)
            {
                summary.AppendLine("  ⚠️ WARNING: At risk of progression!");
            }

            summary.AppendLine();
            summary.AppendLine("  Effects:");
            foreach (var effect in trauma.Effects)
            {
                summary.AppendLine($"    - {effect.GetDescription()}");
            }

            if (!string.IsNullOrEmpty(trauma.FlavorText))
            {
                summary.AppendLine();
                summary.AppendLine($"  \"{trauma.FlavorText}\"");
            }
        }

        summary.AppendLine();
        summary.AppendLine("════════════════════════════════════════════════");

        return summary.ToString();
    }

    /// <summary>
    /// Checks if any traumas are at risk of progression
    /// </summary>
    public bool AnyTraumasAtRisk(PlayerCharacter character)
    {
        return character.Traumas.Any(t =>
            t.DaysSinceManagement >= DaysUntilProgressionCheck &&
            t.ProgressionLevel < 3);
    }

    /// <summary>
    /// Gets count of traumas at risk of progression
    /// </summary>
    public int GetTraumasAtRiskCount(PlayerCharacter character)
    {
        return character.Traumas.Count(t =>
            t.DaysSinceManagement >= DaysUntilProgressionCheck &&
            t.ProgressionLevel < 3);
    }
}
