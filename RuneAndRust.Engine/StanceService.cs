using RuneAndRust.Core;
using RuneAndRust.Core.CombatFlavor;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.21.1: Advanced Stance System Service
/// Manages stance state, transitions, and combat effect calculations.
/// Integrates with Trauma Economy for stress vectors.
/// v0.38.12: Integrated combat stance descriptors
/// </summary>
public class StanceService
{
    private static readonly ILogger _log = Log.ForContext<StanceService>();
    private readonly CombatFlavorTextService? _flavorTextService;

    public StanceService(CombatFlavorTextService? flavorTextService = null)
    {
        _flavorTextService = flavorTextService;
    }

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
        var oldStanceType = oldStance?.Type ?? StanceType.Calculated;

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
            StanceType.Aggressive => Stance.CreateAggressiveStance(),
            StanceType.Defensive => Stance.CreateDefensiveStance(),
            StanceType.Evasive => Stance.CreateEvasiveStance(),
            StanceType.Calculated => Stance.CreateCalculatedStance(),
            _ => Stance.CreateCalculatedStance()
        };

        // Apply stance change
        character.ActiveStance = newStance;
        character.StanceShiftsRemaining--;
        character.StanceTurnsInCurrent = 0; // Reset turn counter for new stance

        _log.Information("Character {CharacterName} changed stance from {OldStance} to {NewStance} (Shifts remaining: {ShiftsRemaining})",
            character.Name, oldStanceType, newStanceType, character.StanceShiftsRemaining);

        // Log to combat state [v0.38.12: Using stance descriptors]
        if (_flavorTextService != null && combatState != null)
        {
            // Determine situation context based on combat state
            string? situationContext = null;
            if (combatState.Enemies != null && combatState.Enemies.Count > 0)
            {
                var playerHP = (float)character.HP / character.MaxHP;
                var enemyCount = combatState.Enemies.Count(e => e.IsAlive);

                situationContext = enemyCount switch
                {
                    > 3 => "Surrounded",
                    > 1 => "Outnumbered",
                    _ when playerHP > 0.7f => "Winning",
                    _ when playerHP < 0.3f => "Losing",
                    _ => "EvenMatch"
                };
            }

            // Determine weapon configuration
            string? weaponConfig = null;
            if (character.EquippedWeapon != null && character.EquippedShield != null)
                weaponConfig = "SwordAndShield";
            else if (character.EquippedWeapon?.IsTwoHanded == true)
                weaponConfig = "TwoHanded";
            else if (character.EquippedWeapon != null)
                weaponConfig = "SingleWeapon";

            var stanceText = _flavorTextService.GenerateCombatStanceText(
                GetStanceName(newStanceType),
                "Entering",
                previousStance: GetStanceName(oldStanceType),
                situationContext: situationContext,
                weaponConfiguration: weaponConfig,
                variables: new Dictionary<string, string>
                {
                    {"ActorName", character.Name}
                });

            combatState.AddLogEntry($"  {stanceText}");
        }
        else
        {
            combatState?.AddLogEntry($"  Shifted to {GetStanceName(newStanceType)} stance (free action).");
        }

        LogStanceEffects(newStanceType, combatState);

        if (character.StanceShiftsRemaining == 0)
        {
            combatState?.AddLogEntry($"  [No more free stance shifts this turn]");
        }

        return true;
    }

    /// <summary>
    /// v2.0: Gets the display name for a stance type.
    /// </summary>
    public string GetStanceName(StanceType stanceType)
    {
        return stanceType switch
        {
            StanceType.Aggressive => "Aggressive",
            StanceType.Defensive => "Defensive",
            StanceType.Evasive => "Evasive",
            StanceType.Calculated => "Calculated",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// v2.0: Gets a description of stance effects for display.
    /// </summary>
    public string GetStanceDescription(StanceType stanceType)
    {
        return stanceType switch
        {
            StanceType.Aggressive => "Aggressive: +4 Damage, -3 Defense, -2 WILL checks",
            StanceType.Defensive => "Defensive: +2 Soak, +2 WILL checks, -25% Damage, -5 Stamina regen",
            StanceType.Evasive => "Evasive: +3 Defense, -50% Damage (Legacy)",
            StanceType.Calculated => "Calculated: No modifiers (balanced)",
            _ => "Unknown stance"
        };
    }

    /// <summary>
    /// v2.0: Logs stance effects to combat state.
    /// </summary>
    private void LogStanceEffects(StanceType stanceType, CombatState? combatState)
    {
        if (combatState == null) return;

        switch (stanceType)
        {
            case StanceType.Aggressive:
                combatState.AddLogEntry("    [+4 Damage | -3 Defense | -2 WILL checks]");
                break;
            case StanceType.Defensive:
                combatState.AddLogEntry("    [+2 Soak | +2 WILL checks | -25% Damage | -5 Stamina regen]");
                break;
            case StanceType.Evasive:
                combatState.AddLogEntry("    [+3 Defense | -50% Damage] (Legacy)");
                break;
            case StanceType.Calculated:
                combatState.AddLogEntry("    [Balanced interface - no modifiers]");
                break;
        }
    }

    /// <summary>
    /// v2.0: Gets flat damage bonus from stance.
    /// Aggressive: +4 flat damage added to all attacks.
    /// </summary>
    public int GetFlatDamageBonus(PlayerCharacter character)
    {
        return character.ActiveStance?.FlatDamageBonus ?? 0;
    }

    /// <summary>
    /// v2.0: Gets Defense Score modifier from stance.
    /// Aggressive: -3 Defense (more vulnerable)
    /// </summary>
    public int GetDefenseModifier(PlayerCharacter character)
    {
        return character.ActiveStance?.DefenseModifier ?? 0;
    }

    /// <summary>
    /// v2.0: Gets Soak bonus from stance.
    /// Defensive: +2 Soak (flat damage reduction)
    /// </summary>
    public int GetSoakBonus(PlayerCharacter character)
    {
        return character.ActiveStance?.SoakBonus ?? 0;
    }

    /// <summary>
    /// v2.0: Gets WILL check modifier from stance (for Trauma Economy).
    /// Aggressive: -2 WILL (vulnerable to psychic stress)
    /// Defensive: +2 WILL (reinforced coherence)
    /// </summary>
    public int GetWillModifier(PlayerCharacter character)
    {
        return character.ActiveStance?.WillModifier ?? 0;
    }

    /// <summary>
    /// v2.0: Gets Stamina regeneration penalty from stance.
    /// Defensive: -5 Stamina regen per turn
    /// </summary>
    public int GetStaminaRegenPenalty(PlayerCharacter character)
    {
        return character.ActiveStance?.StaminaRegenPenalty ?? 0;
    }

    /// <summary>
    /// v2.0: Applies stance damage multiplier to outgoing damage.
    /// Defensive: -25% damage output
    /// Evasive: -50% damage output (legacy)
    /// Called by CombatEngine after base damage + flat bonuses.
    /// </summary>
    public int ApplyDamageMultiplier(PlayerCharacter character, int baseDamage)
    {
        var stance = character.ActiveStance;
        if (stance == null) return baseDamage;

        var multiplier = stance.DamageMultiplier;
        if (multiplier == 1.0f) return baseDamage;

        int modifiedDamage = (int)Math.Round(baseDamage * multiplier);

        _log.Debug("Stance damage multiplier: {Multiplier:F2}x applied to {BaseDamage} damage = {ModifiedDamage} damage",
            multiplier, baseDamage, modifiedDamage);

        return Math.Max(0, modifiedDamage);
    }

    /// <summary>
    /// v2.0 SPEC: Checks if character is in a vulnerable stance (Aggressive).
    /// Being attacked while in Aggressive Stance: +8-10 stress (exposure vulnerability)
    /// </summary>
    /// <param name="character">The character to check</param>
    /// <param name="wasAttacked">Whether the character was just attacked</param>
    /// <returns>Stress amount to add (0 if not vulnerable)</returns>
    public int CheckStanceVulnerabilityStress(PlayerCharacter character, bool wasAttacked)
    {
        if (!wasAttacked) return 0;

        var stance = character.ActiveStance;
        if (stance == null || stance.Type != StanceType.Aggressive) return 0;

        // v2.0 SPEC: Aggressive stance vulnerability: +8-10 stress
        const int vulnerabilityStress = 8; // Using 8 (mid-range of 8-10)

        _log.Debug("Character {CharacterName} caught in Aggressive stance while being attacked, adding {Stress} stress",
            character.Name, vulnerabilityStress);

        return vulnerabilityStress;
    }

    /// <summary>
    /// v2.0: Calculates stress relief from perfect defensive timing.
    /// Successfully blocking major attack in Defensive Stance: -3 stress.
    /// </summary>
    /// <param name="character">The character using the stance</param>
    /// <param name="damageMitigated">Amount of damage mitigated by defensive stance</param>
    /// <returns>Stress reduction amount (negative value)</returns>
    public int CalculateDefensiveBlockStressRelief(PlayerCharacter character, int damageMitigated)
    {
        var stance = character.ActiveStance;
        if (stance == null || stance.Type != StanceType.Defensive) return 0;

        // v2.0 SPEC: Perfect defensive timing (blocking major attack): -3 stress
        if (damageMitigated >= 5)
        {
            const int stressRelief = -3; // Negative = stress reduction

            _log.Debug("Character {CharacterName} successfully blocked major attack in Defensive stance, reducing stress by {Relief}",
                character.Name, Math.Abs(stressRelief));

            return stressRelief;
        }

        return 0;
    }

    /// <summary>
    /// v2.0: Gets all available stances for UI display.
    /// </summary>
    public List<(StanceType Type, string Name, string Description)> GetAvailableStances()
    {
        return new List<(StanceType, string, string)>
        {
            (StanceType.Calculated, GetStanceName(StanceType.Calculated), GetStanceDescription(StanceType.Calculated)),
            (StanceType.Aggressive, GetStanceName(StanceType.Aggressive), GetStanceDescription(StanceType.Aggressive)),
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
    /// v2.0 SPEC: Calculates stress relief from stance mastery.
    /// Maintaining Aggressive Stance for 3+ turns: -5 stress (tactical confidence building)
    /// </summary>
    public int CheckStanceMasteryStressRelief(PlayerCharacter character)
    {
        // v2.0 SPEC: Maintaining Aggressive for 3+ turns reduces stress
        if (character.ActiveStance.Type == StanceType.Aggressive && character.StanceTurnsInCurrent >= 3)
        {
            const int masteryStressRelief = -5; // Negative = stress reduction

            _log.Debug("Character {CharacterName} has maintained Aggressive stance for {Turns} turns, granting {Relief} stress relief",
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
