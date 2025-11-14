using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.21.1: Advanced Stance System Service
/// Manages stance state, transitions, and combat effect calculations.
/// Integrates with Trauma Economy for stress vectors.
/// </summary>
public class StanceService
{
    private static readonly ILogger _log = Log.ForContext<StanceService>();

    /// <summary>
    /// v0.21.1: Switches a character's active stance.
    /// First shift per turn is free, subsequent shifts would cost AP (not yet implemented).
    /// </summary>
    /// <param name="character">The character changing stance</param>
    /// <param name="newStanceType">The stance to switch to</param>
    /// <param name="combatState">Current combat state for logging</param>
    /// <returns>True if stance change was successful, false if invalid</returns>
    public bool SwitchStance(PlayerCharacter character, StanceType newStanceType, CombatState? combatState = null)
    {
        var oldStance = character.ActiveStance;
        var oldStanceType = oldStance?.Type ?? StanceType.Balanced;

        // Check if already in the requested stance
        if (oldStanceType == newStanceType)
        {
            _log.Debug("Character {CharacterName} is already in {StanceType} stance",
                character.Name, newStanceType);
            combatState?.AddLogEntry($"  You are already in {GetStanceName(newStanceType)} stance.");
            return false;
        }

        // v0.21.1: Check if shifts remaining (future: could cost AP if no shifts left)
        if (character.StanceShiftsRemaining <= 0)
        {
            _log.Debug("Character {CharacterName} has no stance shifts remaining this turn",
                character.Name);
            combatState?.AddLogEntry($"  No stance shifts remaining this turn!");
            return false;
        }

        // Create new stance instance
        Stance newStance = newStanceType switch
        {
            StanceType.Offensive => Stance.CreateOffensiveStance(),
            StanceType.Defensive => Stance.CreateDefensiveStance(),
            StanceType.Evasive => Stance.CreateEvasiveStance(),
            StanceType.Balanced => Stance.CreateBalancedStance(),
            _ => Stance.CreateBalancedStance()
        };

        // Apply stance change
        character.ActiveStance = newStance;
        character.StanceShiftsRemaining--;
        character.StanceTurnsInCurrent = 0; // Reset turn counter for new stance

        _log.Information("Character {CharacterName} changed stance from {OldStance} to {NewStance} (Shifts remaining: {ShiftsRemaining})",
            character.Name, oldStanceType, newStanceType, character.StanceShiftsRemaining);

        // Log to combat state
        combatState?.AddLogEntry($"  Shifted to {GetStanceName(newStanceType)} stance (free action).");
        LogStanceEffects(newStanceType, combatState);

        if (character.StanceShiftsRemaining == 0)
        {
            combatState?.AddLogEntry($"  [No more free stance shifts this turn]");
        }

        return true;
    }

    /// <summary>
    /// Gets the display name for a stance type.
    /// </summary>
    public string GetStanceName(StanceType stanceType)
    {
        return stanceType switch
        {
            StanceType.Offensive => "Offensive",
            StanceType.Defensive => "Defensive",
            StanceType.Evasive => "Evasive",
            StanceType.Balanced => "Balanced",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a description of stance effects for display.
    /// </summary>
    public string GetStanceDescription(StanceType stanceType)
    {
        return stanceType switch
        {
            StanceType.Offensive => "Offensive: +15% Accuracy, -10% Mitigation, +5% Damage",
            StanceType.Defensive => "Defensive: -10% Accuracy, +15% Mitigation, -10% Damage",
            StanceType.Evasive => "Evasive: +3 Defense, -50% Damage (Legacy)",
            StanceType.Balanced => "Balanced: No modifiers",
            _ => "Unknown stance"
        };
    }

    /// <summary>
    /// Logs stance effects to combat state.
    /// </summary>
    private void LogStanceEffects(StanceType stanceType, CombatState? combatState)
    {
        if (combatState == null) return;

        switch (stanceType)
        {
            case StanceType.Offensive:
                combatState.AddLogEntry("    [+15% Accuracy | -10% Mitigation | +5% Damage]");
                break;
            case StanceType.Defensive:
                combatState.AddLogEntry("    [-10% Accuracy | +15% Mitigation | -10% Damage]");
                break;
            case StanceType.Evasive:
                combatState.AddLogEntry("    [+3 Defense | -50% Damage]");
                break;
            case StanceType.Balanced:
                combatState.AddLogEntry("    [No combat modifiers]");
                break;
        }
    }

    /// <summary>
    /// v0.21.1: Applies stance accuracy modifier to ability rolls.
    /// Called by CombatEngine during ability execution.
    /// </summary>
    /// <param name="character">The character using the ability</param>
    /// <param name="baseDice">Base dice pool before stance modifiers</param>
    /// <returns>Modified dice pool after stance accuracy modifier</returns>
    public int ApplyStanceAccuracyModifier(PlayerCharacter character, int baseDice)
    {
        var stance = character.ActiveStance;
        if (stance == null) return baseDice;

        var accuracyModifier = stance.AccuracyModifier;
        if (accuracyModifier == 1.0f) return baseDice;

        int modifiedDice = (int)Math.Round(baseDice * accuracyModifier);

        _log.Debug("Stance accuracy modifier: {Modifier:F2}x applied to {BaseDice} dice = {ModifiedDice} dice",
            accuracyModifier, baseDice, modifiedDice);

        return Math.Max(1, modifiedDice); // Always at least 1 die
    }

    /// <summary>
    /// v0.21.1: Applies stance damage modifier to outgoing damage.
    /// Called by CombatEngine after damage calculation.
    /// </summary>
    /// <param name="character">The character dealing damage</param>
    /// <param name="baseDamage">Base damage before stance modifiers</param>
    /// <returns>Modified damage after stance damage multiplier</returns>
    public int ApplyStanceDamageModifier(PlayerCharacter character, int baseDamage)
    {
        var stance = character.ActiveStance;
        if (stance == null) return baseDamage;

        var damageMultiplier = stance.DamageMultiplier;
        if (damageMultiplier == 1.0f) return baseDamage;

        int modifiedDamage = (int)Math.Round(baseDamage * damageMultiplier);

        _log.Debug("Stance damage modifier: {Multiplier:F2}x applied to {BaseDamage} damage = {ModifiedDamage} damage",
            damageMultiplier, baseDamage, modifiedDamage);

        return Math.Max(0, modifiedDamage); // Can't be negative
    }

    /// <summary>
    /// v0.21.1: Applies stance mitigation modifier to incoming damage.
    /// Higher mitigation = less damage taken (damage reduction).
    /// Called by EnemyAI when calculating damage to player.
    /// </summary>
    /// <param name="character">The character receiving damage</param>
    /// <param name="incomingDamage">Damage before stance mitigation</param>
    /// <returns>Reduced damage after stance mitigation modifier</returns>
    public int ApplyStanceMitigationModifier(PlayerCharacter character, int incomingDamage)
    {
        var stance = character.ActiveStance;
        if (stance == null) return incomingDamage;

        var mitigationModifier = stance.MitigationModifier;
        if (mitigationModifier == 1.0f) return incomingDamage;

        // Mitigation works inversely: higher mitigation = lower damage taken
        // 1.15 mitigation = 85% damage taken (15% reduction)
        // 0.90 mitigation = 110% damage taken (10% more vulnerable)
        float damageMultiplier = 2.0f - mitigationModifier; // Inverse relationship
        int modifiedDamage = (int)Math.Round(incomingDamage * damageMultiplier);

        _log.Debug("Stance mitigation modifier: {Mitigation:F2} applied to {IncomingDamage} damage = {ModifiedDamage} damage (multiplier: {DamageMultiplier:F2})",
            mitigationModifier, incomingDamage, modifiedDamage, damageMultiplier);

        return Math.Max(0, modifiedDamage); // Can't be negative
    }

    /// <summary>
    /// v0.21.1 SPEC: Checks if character is in a vulnerable stance (Offensive).
    /// Being attacked while in Offensive Stance: +8-10 stress (vulnerability punishment)
    /// </summary>
    /// <param name="character">The character to check</param>
    /// <param name="wasAttacked">Whether the character was just attacked</param>
    /// <returns>Stress amount to add (0 if not vulnerable)</returns>
    public int CheckStanceVulnerabilityStress(PlayerCharacter character, bool wasAttacked)
    {
        if (!wasAttacked) return 0;

        var stance = character.ActiveStance;
        if (stance == null || stance.Type != StanceType.Offensive) return 0;

        // v0.21.1 SPEC: Offensive stance vulnerability: +8-10 stress
        const int vulnerabilityStress = 8; // Using 8 (mid-range of 8-10)

        _log.Debug("Character {CharacterName} caught in Offensive stance while being attacked, adding {Stress} stress",
            character.Name, vulnerabilityStress);

        return vulnerabilityStress;
    }

    /// <summary>
    /// v0.21.1: Calculates stress relief from optimal stance usage.
    /// Successful defensive stance usage during combat reduces stress.
    /// </summary>
    /// <param name="character">The character using the stance</param>
    /// <param name="damageMitigated">Amount of damage mitigated by defensive stance</param>
    /// <returns>Stress reduction amount (negative value)</returns>
    public int CalculateStanceMasteryStressRelief(PlayerCharacter character, int damageMitigated)
    {
        var stance = character.ActiveStance;
        if (stance == null || stance.Type != StanceType.Defensive) return 0;

        // Defensive stance provides stress relief when it mitigates significant damage
        if (damageMitigated >= 5)
        {
            const int stressRelief = -3; // Negative = stress reduction

            _log.Debug("Character {CharacterName} successfully used Defensive stance to mitigate {Damage} damage, reducing stress by {Relief}",
                character.Name, damageMitigated, Math.Abs(stressRelief));

            return stressRelief;
        }

        return 0;
    }

    /// <summary>
    /// Gets all available stances as a list for UI display.
    /// </summary>
    public List<(StanceType Type, string Name, string Description)> GetAvailableStances()
    {
        return new List<(StanceType, string, string)>
        {
            (StanceType.Balanced, GetStanceName(StanceType.Balanced), GetStanceDescription(StanceType.Balanced)),
            (StanceType.Offensive, GetStanceName(StanceType.Offensive), GetStanceDescription(StanceType.Offensive)),
            (StanceType.Defensive, GetStanceName(StanceType.Defensive), GetStanceDescription(StanceType.Defensive)),
            (StanceType.Evasive, GetStanceName(StanceType.Evasive), GetStanceDescription(StanceType.Evasive))
        };
    }

    /// <summary>
    /// v0.21.1: Called at the start of a character's turn to reset stance shifts.
    /// </summary>
    public void OnTurnStart(PlayerCharacter character)
    {
        character.StanceShiftsRemaining = 1; // Reset to 1 free shift per turn

        _log.Debug("Turn start for {CharacterName}: Stance shifts reset to {ShiftsRemaining}",
            character.Name, character.StanceShiftsRemaining);
    }

    /// <summary>
    /// v0.21.1: Called at the end of a character's turn to increment stance duration.
    /// Tracks how long character has been in current stance for stress relief mechanics.
    /// </summary>
    public void OnTurnEnd(PlayerCharacter character)
    {
        character.StanceTurnsInCurrent++;

        _log.Debug("Turn end for {CharacterName}: {StanceTurnsInCurrent} turns in {StanceType}",
            character.Name, character.StanceTurnsInCurrent, character.ActiveStance.Type);
    }

    /// <summary>
    /// v0.21.1 SPEC: Calculates stress relief from stance mastery.
    /// Maintaining Offensive Stance for 3+ turns: -5 stress (confidence building)
    /// </summary>
    public int CheckStanceMasteryStressRelief(PlayerCharacter character)
    {
        // v0.21.1 SPEC: Maintaining Offensive for 3+ turns reduces stress
        if (character.ActiveStance.Type == StanceType.Offensive && character.StanceTurnsInCurrent >= 3)
        {
            const int masteryStressRelief = -5; // Negative = stress reduction

            _log.Debug("Character {CharacterName} has maintained Offensive stance for {Turns} turns, granting {Relief} stress relief",
                character.Name, character.StanceTurnsInCurrent, Math.Abs(masteryStressRelief));

            return masteryStressRelief;
        }

        return 0;
    }

    /// <summary>
    /// v0.21.1: Check if character can change stance (has shifts remaining).
    /// </summary>
    public bool CanChangeStance(PlayerCharacter character)
    {
        return character.StanceShiftsRemaining > 0;
    }
}
