// ------------------------------------------------------------------------------
// <copyright file="IceResolutionResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The complete result of resolving an ICE encounter, including all
// mechanical consequences.
// Part of v0.15.4c ICE Countermeasures implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// The complete result of resolving an ICE encounter, including all
/// mechanical consequences.
/// </summary>
/// <remarks>
/// <para>
/// This value object captures every outcome detail from an ICE resolution,
/// including damage, stress, lockout duration, alert changes, and narrative.
/// </para>
/// <para>
/// Use the static factory methods to create appropriately-configured results
/// for each ICE type and outcome combination.
/// </para>
/// </remarks>
/// <param name="Encounter">The ICE encounter that was resolved.</param>
/// <param name="Outcome">The outcome of the resolution (CharacterWon, IceWon, Evaded).</param>
/// <param name="CharacterRoll">The character's roll result (net successes).</param>
/// <param name="IceDc">The ICE's effective DC (for contested) or save DC (for Lethal).</param>
/// <param name="PsychicDamage">Psychic damage dealt (Lethal ICE only).</param>
/// <param name="StressGained">Stress points gained.</param>
/// <param name="LocationRevealed">Whether the character's location was revealed (Passive ICE).</param>
/// <param name="ForcedDisconnect">Whether the character was disconnected (Active/Lethal).</param>
/// <param name="LockoutDuration">Lockout duration in minutes (0 = no lockout, -1 = permanent).</param>
/// <param name="AlertLevelChange">Change to terminal alert level.</param>
/// <param name="BonusDiceGranted">Bonus dice for next layer check (on Active ICE victory).</param>
/// <param name="NarrativeDescription">Flavor text describing the encounter outcome.</param>
public readonly record struct IceResolutionResult(
    IceEncounter Encounter,
    IceResolutionOutcome Outcome,
    int CharacterRoll,
    int IceDc,
    int PsychicDamage,
    int StressGained,
    bool LocationRevealed,
    bool ForcedDisconnect,
    int LockoutDuration,
    int AlertLevelChange,
    int BonusDiceGranted,
    string NarrativeDescription)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Whether the character won this encounter.
    /// </summary>
    /// <remarks>
    /// Character victory indicates successful defense against the ICE,
    /// though consequences may still apply (e.g., Lethal ICE save success
    /// still inflicts stress).
    /// </remarks>
    public bool CharacterWon => Outcome is IceResolutionOutcome.CharacterWon
                                        or IceResolutionOutcome.Evaded;

    /// <summary>
    /// Whether this result includes damage that must be applied.
    /// </summary>
    public bool HasDamage => PsychicDamage > 0;

    /// <summary>
    /// Whether this result includes stress that must be applied.
    /// </summary>
    public bool HasStress => StressGained > 0;

    /// <summary>
    /// Whether the terminal is permanently locked out.
    /// </summary>
    /// <remarks>
    /// Permanent lockout occurs when Lethal ICE save fails. The terminal
    /// can never be accessed again by this character.
    /// </remarks>
    public bool IsPermanentLockout => LockoutDuration == -1;

    /// <summary>
    /// Whether this result has a temporary lockout duration.
    /// </summary>
    public bool HasTemporaryLockout => LockoutDuration > 0;

    // -------------------------------------------------------------------------
    // Passive ICE Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful resolution for Passive ICE (evaded trace).
    /// </summary>
    /// <param name="encounter">The ICE encounter being resolved.</param>
    /// <param name="characterRoll">The character's net successes.</param>
    /// <param name="iceDc">The ICE's difficulty class.</param>
    /// <returns>An IceResolutionResult for successful trace evasion.</returns>
    /// <remarks>
    /// On Passive ICE evasion:
    /// <list type="bullet">
    ///   <item><description>No location revealed</description></item>
    ///   <item><description>No alert level change</description></item>
    ///   <item><description>Character may continue infiltration</description></item>
    /// </list>
    /// </remarks>
    public static IceResolutionResult PassiveEvaded(
        IceEncounter encounter,
        int characterRoll,
        int iceDc)
    {
        return new IceResolutionResult(
            Encounter: encounter.WithOutcome(IceResolutionOutcome.Evaded),
            Outcome: IceResolutionOutcome.Evaded,
            CharacterRoll: characterRoll,
            IceDc: iceDc,
            PsychicDamage: 0,
            StressGained: 0,
            LocationRevealed: false,
            ForcedDisconnect: false,
            LockoutDuration: 0,
            AlertLevelChange: 0,
            BonusDiceGranted: 0,
            NarrativeDescription: "You slip through the trace like smoke through fingers. " +
                                  "The guardian program spins in confusion, losing your trail."
        );
    }

    /// <summary>
    /// Creates a failed resolution for Passive ICE (location revealed).
    /// </summary>
    /// <param name="encounter">The ICE encounter being resolved.</param>
    /// <param name="characterRoll">The character's net successes.</param>
    /// <param name="iceDc">The ICE's difficulty class.</param>
    /// <returns>An IceResolutionResult for failed trace evasion.</returns>
    /// <remarks>
    /// On Passive ICE victory:
    /// <list type="bullet">
    ///   <item><description>Character's physical location broadcast to security</description></item>
    ///   <item><description>Alert level +2</description></item>
    ///   <item><description>Character may continue infiltration (but guards alerted)</description></item>
    /// </list>
    /// </remarks>
    public static IceResolutionResult PassiveFailed(
        IceEncounter encounter,
        int characterRoll,
        int iceDc)
    {
        return new IceResolutionResult(
            Encounter: encounter.WithOutcome(IceResolutionOutcome.IceWon),
            Outcome: IceResolutionOutcome.IceWon,
            CharacterRoll: characterRoll,
            IceDc: iceDc,
            PsychicDamage: 0,
            StressGained: 0,
            LocationRevealed: true,
            ForcedDisconnect: false,
            LockoutDuration: 0,
            AlertLevelChange: 2,
            BonusDiceGranted: 0,
            NarrativeDescription: "The ICE locks onto your neural signature. Your location " +
                                  "pulses on security displays. They know where you are."
        );
    }

    // -------------------------------------------------------------------------
    // Active ICE Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful resolution for Active ICE (ICE disabled).
    /// </summary>
    /// <param name="encounter">The ICE encounter being resolved.</param>
    /// <param name="characterRoll">The character's net successes.</param>
    /// <param name="iceDc">The ICE's difficulty class.</param>
    /// <returns>An IceResolutionResult for defeating Active ICE.</returns>
    /// <remarks>
    /// On Active ICE defeat:
    /// <list type="bullet">
    ///   <item><description>ICE program disabled (won't reactivate)</description></item>
    ///   <item><description>+1 bonus die on next layer check</description></item>
    ///   <item><description>Character may continue infiltration</description></item>
    /// </list>
    /// </remarks>
    public static IceResolutionResult ActiveDefeated(
        IceEncounter encounter,
        int characterRoll,
        int iceDc)
    {
        return new IceResolutionResult(
            Encounter: encounter.WithOutcome(IceResolutionOutcome.CharacterWon),
            Outcome: IceResolutionOutcome.CharacterWon,
            CharacterRoll: characterRoll,
            IceDc: iceDc,
            PsychicDamage: 0,
            StressGained: 0,
            LocationRevealed: false,
            ForcedDisconnect: false,
            LockoutDuration: 0,
            AlertLevelChange: 0,
            BonusDiceGranted: 1,
            NarrativeDescription: "The ICE lunges—but you're faster. Your countermeasures " +
                                  "fragment the program into harmless data streams. The path is clear."
        );
    }

    /// <summary>
    /// Creates a failed resolution for Active ICE (forced disconnect).
    /// </summary>
    /// <param name="encounter">The ICE encounter being resolved.</param>
    /// <param name="characterRoll">The character's net successes.</param>
    /// <param name="iceDc">The ICE's difficulty class.</param>
    /// <returns>An IceResolutionResult for being defeated by Active ICE.</returns>
    /// <remarks>
    /// On Active ICE victory:
    /// <list type="bullet">
    ///   <item><description>Connection severed immediately</description></item>
    ///   <item><description>Terminal locked for 1 minute</description></item>
    ///   <item><description>Alert level +1</description></item>
    ///   <item><description>Must restart from Layer 1 after lockout</description></item>
    /// </list>
    /// </remarks>
    public static IceResolutionResult ActiveFailed(
        IceEncounter encounter,
        int characterRoll,
        int iceDc)
    {
        return new IceResolutionResult(
            Encounter: encounter.WithOutcome(IceResolutionOutcome.IceWon),
            Outcome: IceResolutionOutcome.IceWon,
            CharacterRoll: characterRoll,
            IceDc: iceDc,
            PsychicDamage: 0,
            StressGained: 0,
            LocationRevealed: false,
            ForcedDisconnect: true,
            LockoutDuration: 1,
            AlertLevelChange: 1,
            BonusDiceGranted: 0,
            NarrativeDescription: "The ICE strikes like a viper. Your connection shatters—you're " +
                                  "thrown back to reality, neural interfaces screaming."
        );
    }

    // -------------------------------------------------------------------------
    // Lethal ICE Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful save resolution for Lethal ICE (partial escape).
    /// </summary>
    /// <param name="encounter">The ICE encounter being resolved.</param>
    /// <param name="characterRoll">The character's save result (net successes).</param>
    /// <param name="stressRoll">The rolled stress amount (1d6).</param>
    /// <returns>An IceResolutionResult for surviving Lethal ICE.</returns>
    /// <remarks>
    /// On Lethal ICE save success:
    /// <list type="bullet">
    ///   <item><description>Auto-disconnect from terminal</description></item>
    ///   <item><description>1d6 stress gained</description></item>
    ///   <item><description>Terminal locked for 1 minute</description></item>
    ///   <item><description>May retry after lockout</description></item>
    /// </list>
    /// </remarks>
    public static IceResolutionResult LethalSaved(
        IceEncounter encounter,
        int characterRoll,
        int stressRoll)
    {
        return new IceResolutionResult(
            Encounter: encounter.WithOutcome(IceResolutionOutcome.CharacterWon),
            Outcome: IceResolutionOutcome.CharacterWon,
            CharacterRoll: characterRoll,
            IceDc: 16, // Fixed DC for Lethal ICE
            PsychicDamage: 0,
            StressGained: stressRoll, // 1d6
            LocationRevealed: false,
            ForcedDisconnect: true,
            LockoutDuration: 1,
            AlertLevelChange: 0,
            BonusDiceGranted: 0,
            NarrativeDescription: "You feel the neural strike incoming—ancient malice reaching for your mind. " +
                                  "You tear yourself free just in time, gasping, shaking. The connection severs."
        );
    }

    /// <summary>
    /// Creates a failed save resolution for Lethal ICE (full neural strike).
    /// </summary>
    /// <param name="encounter">The ICE encounter being resolved.</param>
    /// <param name="characterRoll">The character's save result (net successes).</param>
    /// <param name="damageRoll">The rolled psychic damage amount (3d10).</param>
    /// <param name="stressRoll">The rolled stress amount (2d6).</param>
    /// <returns>An IceResolutionResult for failing against Lethal ICE.</returns>
    /// <remarks>
    /// On Lethal ICE save failure:
    /// <list type="bullet">
    ///   <item><description>3d10 psychic damage</description></item>
    ///   <item><description>2d6 stress gained</description></item>
    ///   <item><description>Forced disconnect</description></item>
    ///   <item><description>Terminal permanently locked out</description></item>
    ///   <item><description>Alert level +2</description></item>
    /// </list>
    /// </remarks>
    public static IceResolutionResult LethalFailed(
        IceEncounter encounter,
        int characterRoll,
        int damageRoll,
        int stressRoll)
    {
        return new IceResolutionResult(
            Encounter: encounter.WithOutcome(IceResolutionOutcome.IceWon),
            Outcome: IceResolutionOutcome.IceWon,
            CharacterRoll: characterRoll,
            IceDc: 16, // Fixed DC for Lethal ICE
            PsychicDamage: damageRoll, // 3d10
            StressGained: stressRoll, // 2d6
            LocationRevealed: false,
            ForcedDisconnect: true,
            LockoutDuration: -1, // Permanent
            AlertLevelChange: 2,
            BonusDiceGranted: 0,
            NarrativeDescription: "The Lethal ICE bypasses every defense. Ancient code burns through your neural " +
                                  "pathways like acid. You scream as your vision fills with static and blood."
        );
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the resolution result.
    /// </summary>
    /// <returns>A human-readable summary of the resolution.</returns>
    public string ToDisplayString()
    {
        var outcomeStr = CharacterWon ? "SUCCESS" : "FAILURE";
        var consequences = new List<string>();

        if (HasDamage)
            consequences.Add($"{PsychicDamage} psychic damage");
        if (HasStress)
            consequences.Add($"{StressGained} stress");
        if (LocationRevealed)
            consequences.Add("location revealed");
        if (ForcedDisconnect)
            consequences.Add("disconnected");
        if (IsPermanentLockout)
            consequences.Add("permanent lockout");
        else if (HasTemporaryLockout)
            consequences.Add($"{LockoutDuration} min lockout");
        if (AlertLevelChange > 0)
            consequences.Add($"alert +{AlertLevelChange}");
        if (BonusDiceGranted > 0)
            consequences.Add($"+{BonusDiceGranted}d10 bonus");

        var consequenceStr = consequences.Count > 0
            ? $" [{string.Join(", ", consequences)}]"
            : "";

        return $"ICE Resolution: {outcomeStr} (Roll {CharacterRoll} vs DC {IceDc}){consequenceStr}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"IceResult[{Encounter.EncounterId[..8]}] " +
               $"Outcome={Outcome} Roll={CharacterRoll} DC={IceDc} " +
               $"Dmg={PsychicDamage} Stress={StressGained} " +
               $"Lockout={LockoutDuration} Alert={AlertLevelChange}";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
