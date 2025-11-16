using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.30.2: Frigid Cold Service
/// Handles [Frigid Cold] ambient condition for Niflheim biome.
/// Processes Ice Vulnerability, Critical Hit Slow, and Psychic Stress application.
///
/// Responsibilities:
/// - Apply Ice Vulnerability (+50%) to all combatants
/// - Process critical hit slow effect (2 turns of [Slowed])
/// - Apply psychic stress from environmental anxiety (+5 per combat)
/// </summary>
public class FrigidColdService
{
    private static readonly ILogger _log = Log.ForContext<FrigidColdService>();

    /// <summary>
    /// v0.30.2 canonical: Ice vulnerability percentage for all combatants in [Frigid Cold]
    /// </summary>
    private const int ICE_VULNERABILITY_PERCENT = 50;

    /// <summary>
    /// v0.30.2 canonical: Duration of [Slowed] status when critically hit
    /// </summary>
    private const int CRITICAL_HIT_SLOW_DURATION = 2;

    /// <summary>
    /// v0.30.2 canonical: Psychic stress per combat from environmental anxiety
    /// </summary>
    private const int PSYCHIC_STRESS_PER_COMBAT = 5;

    public FrigidColdService()
    {
        _log.Information("FrigidColdService initialized");
    }

    #region Ice Vulnerability Application

    /// <summary>
    /// Apply [Frigid Cold] Ice Vulnerability to a player character.
    /// Grants +50% vulnerability to Ice damage.
    /// </summary>
    public void ApplyFrigidCold(PlayerCharacter character)
    {
        // TODO: Implement when status effect system is complete
        // For now, log the application
        _log.Debug("Applied [Frigid Cold] to {Character}: Ice Vulnerability +{Percent}%",
            character.Name, ICE_VULNERABILITY_PERCENT);

        // Note: The actual Ice Vulnerability will be applied by the combat system
        // when calculating Ice damage. This service tracks the ambient condition.
    }

    /// <summary>
    /// Apply [Frigid Cold] Ice Vulnerability to an enemy.
    /// Grants +50% vulnerability to Ice damage.
    /// </summary>
    public void ApplyFrigidCold(Enemy enemy)
    {
        // TODO: Implement when status effect system is complete
        _log.Debug("Applied [Frigid Cold] to {Enemy}: Ice Vulnerability +{Percent}%",
            enemy.Name, ICE_VULNERABILITY_PERCENT);
    }

    /// <summary>
    /// Calculate Ice damage with Frigid Cold vulnerability applied.
    /// </summary>
    public int ApplyIceVulnerability(int baseDamage)
    {
        // Apply +50% vulnerability
        int amplifiedDamage = (int)(baseDamage * 1.5);

        _log.Debug("Ice Vulnerability applied: {Base} → {Amplified} (+50%)",
            baseDamage, amplifiedDamage);

        return amplifiedDamage;
    }

    /// <summary>
    /// Get Ice Vulnerability percentage for [Frigid Cold].
    /// </summary>
    public int GetIceVulnerabilityPercent()
    {
        return ICE_VULNERABILITY_PERCENT;
    }

    #endregion

    #region Critical Hit Slow

    /// <summary>
    /// Process critical hit in [Frigid Cold] environment.
    /// Critical hits apply [Slowed] status for 2 turns.
    /// </summary>
    public CriticalHitSlowResult ProcessCriticalHitSlow(PlayerCharacter target, string attackerName)
    {
        _log.Information("{Attacker} scored critical hit on {Target} in [Frigid Cold]",
            attackerName, target.Name);

        var result = new CriticalHitSlowResult
        {
            TargetName = target.Name,
            AttackerName = attackerName,
            SlowDuration = CRITICAL_HIT_SLOW_DURATION
        };

        // TODO: Apply [Slowed] status when status effect system is implemented
        // For now, track that the effect should be applied
        result.SlowApplied = true;
        result.Message = $"💥 Critical hit in [Frigid Cold]!\n" +
                       $"   {target.Name} is [Slowed] for {CRITICAL_HIT_SLOW_DURATION} turns (-50% Movement Speed)\n" +
                       $"   The extreme cold amplifies the shock of the blow.";

        _log.Information("{Target} is [Slowed] for {Duration} turns from critical hit in [Frigid Cold]",
            target.Name, CRITICAL_HIT_SLOW_DURATION);

        return result;
    }

    /// <summary>
    /// Process critical hit in [Frigid Cold] environment (enemy target).
    /// Critical hits apply [Slowed] status for 2 turns.
    /// </summary>
    public CriticalHitSlowResult ProcessCriticalHitSlow(Enemy target, string attackerName)
    {
        _log.Information("{Attacker} scored critical hit on {Target} in [Frigid Cold]",
            attackerName, target.Name);

        var result = new CriticalHitSlowResult
        {
            TargetName = target.Name,
            AttackerName = attackerName,
            SlowDuration = CRITICAL_HIT_SLOW_DURATION
        };

        // TODO: Apply [Slowed] status when status effect system is implemented
        result.SlowApplied = true;
        result.Message = $"💥 Critical hit in [Frigid Cold]!\n" +
                       $"   {target.Name} is [Slowed] for {CRITICAL_HIT_SLOW_DURATION} turns\n" +
                       $"   The extreme cold amplifies the shock of the blow.";

        _log.Information("{Target} is [Slowed] for {Duration} turns from critical hit in [Frigid Cold]",
            target.Name, CRITICAL_HIT_SLOW_DURATION);

        return result;
    }

    #endregion

    #region Psychic Stress

    /// <summary>
    /// Apply environmental psychic stress from [Frigid Cold] at end of combat.
    /// +5 Psychic Stress per combat encounter.
    /// </summary>
    public List<PsychicStressResult> ApplyEnvironmentalStress(List<PlayerCharacter> characters)
    {
        _log.Information("Applying [Frigid Cold] environmental stress to {Count} characters",
            characters.Count);

        var results = new List<PsychicStressResult>();

        foreach (var character in characters)
        {
            var result = new PsychicStressResult
            {
                CharacterName = character.Name,
                PreviousStress = character.PsychicStress
            };

            // Apply +5 Psychic Stress
            character.PsychicStress += PSYCHIC_STRESS_PER_COMBAT;

            result.StressGained = PSYCHIC_STRESS_PER_COMBAT;
            result.CurrentStress = character.PsychicStress;
            result.Message = $"{character.Name} gains +{PSYCHIC_STRESS_PER_COMBAT} Psychic Stress from [Frigid Cold] exposure\n" +
                           $"   Environmental anxiety from the extreme cold weighs on the mind.\n" +
                           $"   Psychic Stress: {result.PreviousStress} → {result.CurrentStress}";

            results.Add(result);

            _log.Information("{Character} gains +{Stress} Psychic Stress from [Frigid Cold] ({Previous} → {Current})",
                character.Name, PSYCHIC_STRESS_PER_COMBAT, result.PreviousStress, result.CurrentStress);
        }

        return results;
    }

    /// <summary>
    /// Get Psychic Stress per combat for [Frigid Cold].
    /// </summary>
    public int GetPsychicStressPerCombat()
    {
        return PSYCHIC_STRESS_PER_COMBAT;
    }

    #endregion

    #region Status Messages

    /// <summary>
    /// Get flavor text for [Frigid Cold] condition.
    /// </summary>
    public string GetFrigidColdStatusMessage()
    {
        return $"❄️ [Frigid Cold] - Ambient Condition\n" +
               $"   All combatants are Vulnerable to Ice damage (+{ICE_VULNERABILITY_PERCENT}%)\n" +
               $"   Critical hits inflict [Slowed] for {CRITICAL_HIT_SLOW_DURATION} turns\n" +
               $"   +{PSYCHIC_STRESS_PER_COMBAT} Psychic Stress per combat (environmental anxiety)";
    }

    /// <summary>
    /// Get warning message for characters entering Niflheim.
    /// </summary>
    public string GetWarningMessage()
    {
        return "⚠️ Entering [Frigid Cold] environment (Niflheim)\n" +
               "   The absolute zero temperature bypasses all standard thermal protection.\n" +
               $"   All combatants take +{ICE_VULNERABILITY_PERCENT}% Ice damage.\n" +
               "   Ice-based attacks are especially devastating in this environment.";
    }

    #endregion
}

#region Result Data Transfer Objects

public class CriticalHitSlowResult
{
    public string TargetName { get; set; } = string.Empty;
    public string AttackerName { get; set; } = string.Empty;
    public bool SlowApplied { get; set; }
    public int SlowDuration { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PsychicStressResult
{
    public string CharacterName { get; set; } = string.Empty;
    public int PreviousStress { get; set; }
    public int StressGained { get; set; }
    public int CurrentStress { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion
