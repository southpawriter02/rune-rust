using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.31.2: Runic Instability Service
/// Handles [Runic Instability] ambient condition for Alfheim biome.
/// Processes Wild Magic Surges, Aether Pool amplification, and Psychic Stress feedback.
///
/// Responsibilities:
/// - Trigger Wild Magic Surges (25% chance on Mystic abilities)
/// - Generate random surge effects (damage, range, targets, duration ±50%)
/// - Apply Aether Pool amplification (+10% for Mystics in Alfheim)
/// - Apply psychic stress from surge feedback (+5 per surge)
/// </summary>
public class RunicInstabilityService
{
    private static readonly ILogger _log = Log.ForContext<RunicInstabilityService>();
    private readonly DiceService _diceService;

    /// <summary>
    /// v0.31.2 canonical: Wild Magic Surge chance on Mystic ability use
    /// </summary>
    private const double SURGE_CHANCE = 0.25; // 25%

    /// <summary>
    /// v0.31.2 canonical: Psychic stress per Wild Magic Surge
    /// </summary>
    private const int STRESS_PER_SURGE = 5;

    /// <summary>
    /// v0.31.2 canonical: Aether Pool amplification for Mystics in Alfheim
    /// </summary>
    private const double AETHER_POOL_BONUS = 0.10; // +10%

    public RunicInstabilityService(DiceService? diceService = null)
    {
        _diceService = diceService ?? new DiceService();
        _log.Information("RunicInstabilityService initialized");
    }

    #region Wild Magic Surge Detection

    /// <summary>
    /// Check if Mystic ability triggers Wild Magic Surge in Alfheim.
    /// Returns surge result if triggered, null otherwise.
    /// </summary>
    public WildMagicSurgeResult? TryTriggerWildMagicSurge(
        PlayerCharacter caster,
        string abilityName,
        int biomeId)
    {
        // Only applies in Alfheim (biome_id: 6)
        if (biomeId != 6)
        {
            return null;
        }

        // Only affects Mystic characters
        if (caster.Archetype != "Mystic")
        {
            return null;
        }

        // Roll for surge (25% chance)
        var roll = _diceService.RollPercentage();
        if (roll > SURGE_CHANCE)
        {
            _log.Debug("Wild Magic Surge check: {Caster} using {Ability} - No surge (roll: {Roll})",
                caster.Name, abilityName, roll);
            return null;
        }

        _log.Information(
            "Wild Magic Surge triggered: {Caster} using {Ability} (roll: {Roll} <= {Threshold})",
            caster.Name, abilityName, roll, SURGE_CHANCE);

        // Generate surge effect
        var surge = GenerateSurgeEffect(abilityName);

        // Apply Psychic Stress
        caster.PsychicStress += STRESS_PER_SURGE;

        _log.Information(
            "Wild Magic Surge: {Effect} on {Ability}, +{Stress} Psychic Stress ({Total} total stress)",
            surge.EffectDescription, abilityName, STRESS_PER_SURGE, caster.PsychicStress);

        return surge;
    }

    /// <summary>
    /// Generate random surge effect for ability.
    /// Surge types: Damage, Range, Targets, Duration (all ±50%)
    /// </summary>
    private WildMagicSurgeResult GenerateSurgeEffect(string abilityName)
    {
        // Roll 1d4 for surge type
        var typeRoll = _diceService.Roll(1, 4);

        // Roll 1d2 for direction (1 = decrease, 2 = increase)
        var directionRoll = _diceService.Roll(1, 2);
        var isIncrease = directionRoll == 2;
        var modifier = isIncrease ? +0.5 : -0.5;

        return typeRoll switch
        {
            1 => new WildMagicSurgeResult
            {
                Type = SurgeType.DamageModification,
                Modifier = modifier,
                EffectDescription = isIncrease
                    ? "Damage increased by 50%"
                    : "Damage reduced by 50%",
                NarrativeText = isIncrease
                    ? "⚡ The Aetheric field amplifies your weaving - destructive power surges!"
                    : "⚡ Reality distortion interferes - your energy dissipates chaotically"
            },
            2 => new WildMagicSurgeResult
            {
                Type = SurgeType.RangeModification,
                Modifier = isIncrease ? +1 : -1,
                EffectDescription = isIncrease
                    ? "Range increased by 1 tile"
                    : "Range decreased by 1 tile (minimum 1)",
                NarrativeText = isIncrease
                    ? "⚡ Your runes reach farther than intended - spatial parameters shifted!"
                    : "⚡ The effect collapses closer to you - dimensional boundaries compressed"
            },
            3 => new WildMagicSurgeResult
            {
                Type = SurgeType.TargetModification,
                Modifier = isIncrease ? +1 : -1,
                EffectDescription = isIncrease
                    ? "Affects 1 additional target"
                    : "Affects 1 fewer target (minimum 1)",
                NarrativeText = isIncrease
                    ? "⚡ The effect branches unexpectedly - quantum targeting expanded!"
                    : "⚡ Your focus narrows involuntarily - targeting parameters reduced"
            },
            4 => new WildMagicSurgeResult
            {
                Type = SurgeType.DurationModification,
                Modifier = modifier,
                EffectDescription = isIncrease
                    ? "Duration increased by 50%"
                    : "Duration reduced by 50% (minimum 1 turn)",
                NarrativeText = isIncrease
                    ? "⚡ Temporal distortion extends the effect - time flows strangely!"
                    : "⚡ Reality snaps back faster than expected - temporal coherence failing"
            },
            _ => throw new InvalidOperationException($"Invalid surge type roll: {typeRoll}")
        };
    }

    #endregion

    #region Aether Pool Amplification

    /// <summary>
    /// Apply Aether Pool amplification for Mystics entering Alfheim.
    /// Grants +10% base Aether Pool capacity.
    /// </summary>
    public void ApplyAetherPoolAmplification(PlayerCharacter character)
    {
        if (character.Archetype != "Mystic")
        {
            _log.Debug("Aether Pool amplification skipped: {Character} is not a Mystic",
                character.Name);
            return;
        }

        // Calculate +10% bonus
        var basePool = character.BaseAetherPool; // Assuming this property exists
        var amplification = (int)(basePool * AETHER_POOL_BONUS);

        // Apply bonus
        character.MaxAetherPool += amplification;
        character.CurrentAetherPool += amplification; // Grant the extra capacity immediately

        _log.Information(
            "{Character} Aether Pool amplified in Alfheim: +{Bonus} (+10%) - New max: {NewMax}",
            character.Name, amplification, character.MaxAetherPool);
    }

    /// <summary>
    /// Remove Aether Pool amplification when leaving Alfheim.
    /// </summary>
    public void RemoveAetherPoolAmplification(PlayerCharacter character)
    {
        if (character.Archetype != "Mystic")
        {
            return;
        }

        var basePool = character.BaseAetherPool;
        var amplification = (int)(basePool * AETHER_POOL_BONUS);

        character.MaxAetherPool -= amplification;

        // Don't reduce current pool below new max
        if (character.CurrentAetherPool > character.MaxAetherPool)
        {
            character.CurrentAetherPool = character.MaxAetherPool;
        }

        _log.Information(
            "{Character} Aether Pool amplification removed: -{Bonus} - New max: {NewMax}",
            character.Name, amplification, character.MaxAetherPool);
    }

    #endregion

    #region Surge Application Helpers

    /// <summary>
    /// Apply surge modification to damage value.
    /// </summary>
    public int ApplySurgeToDamage(int baseDamage, WildMagicSurgeResult surge)
    {
        if (surge.Type != SurgeType.DamageModification)
        {
            return baseDamage;
        }

        var modifiedDamage = (int)(baseDamage * (1.0 + surge.Modifier));
        modifiedDamage = Math.Max(1, modifiedDamage); // Minimum 1 damage

        _log.Debug("Surge damage modification: {Base} → {Modified} ({Modifier:P0})",
            baseDamage, modifiedDamage, surge.Modifier);

        return modifiedDamage;
    }

    /// <summary>
    /// Apply surge modification to range value.
    /// </summary>
    public int ApplySurgeToRange(int baseRange, WildMagicSurgeResult surge)
    {
        if (surge.Type != SurgeType.RangeModification)
        {
            return baseRange;
        }

        var modifiedRange = baseRange + (int)surge.Modifier;
        modifiedRange = Math.Max(1, modifiedRange); // Minimum 1 tile

        _log.Debug("Surge range modification: {Base} → {Modified} ({Modifier:+0;-0})",
            baseRange, modifiedRange, (int)surge.Modifier);

        return modifiedRange;
    }

    /// <summary>
    /// Apply surge modification to target count.
    /// </summary>
    public int ApplySurgeToTargets(int baseTargets, WildMagicSurgeResult surge)
    {
        if (surge.Type != SurgeType.TargetModification)
        {
            return baseTargets;
        }

        var modifiedTargets = baseTargets + (int)surge.Modifier;
        modifiedTargets = Math.Max(1, modifiedTargets); // Minimum 1 target

        _log.Debug("Surge target modification: {Base} → {Modified} ({Modifier:+0;-0})",
            baseTargets, modifiedTargets, (int)surge.Modifier);

        return modifiedTargets;
    }

    /// <summary>
    /// Apply surge modification to duration value.
    /// </summary>
    public int ApplySurgeToDuration(int baseDuration, WildMagicSurgeResult surge)
    {
        if (surge.Type != SurgeType.DurationModification)
        {
            return baseDuration;
        }

        var modifiedDuration = (int)(baseDuration * (1.0 + surge.Modifier));
        modifiedDuration = Math.Max(1, modifiedDuration); // Minimum 1 turn

        _log.Debug("Surge duration modification: {Base} → {Modified} ({Modifier:P0})",
            baseDuration, modifiedDuration, surge.Modifier);

        return modifiedDuration;
    }

    #endregion
}

#region Data Transfer Objects

/// <summary>
/// Result of a Wild Magic Surge event
/// </summary>
public class WildMagicSurgeResult
{
    public SurgeType Type { get; set; }
    public double Modifier { get; set; }
    public string EffectDescription { get; set; } = string.Empty;
    public string NarrativeText { get; set; } = string.Empty;
}

/// <summary>
/// Types of Wild Magic Surge modifications
/// </summary>
public enum SurgeType
{
    DamageModification,
    RangeModification,
    TargetModification,
    DurationModification
}

#endregion
