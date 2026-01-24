// ------------------------------------------------------------------------------
// <copyright file="BypassSpecializationAbility.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents a specialization ability for the System Bypass skill system,
// including its type, trigger conditions, effects, and prerequisites.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a specialization ability for the System Bypass skill system.
/// </summary>
/// <remarks>
/// <para>
/// Specialization abilities are unique bonuses granted to characters based on
/// their archetype. Each ability has a type (Passive, Triggered, UniqueAction),
/// trigger conditions, and specific effects.
/// </para>
/// <para>
/// <b>Ability Categories:</b>
/// <list type="bullet">
///   <item><description><b>Passive:</b> Always active when conditions met</description></item>
///   <item><description><b>Triggered:</b> Activates on specific outcomes</description></item>
///   <item><description><b>UniqueAction:</b> Grants new actions</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="AbilityId">Unique identifier for the ability (e.g., "master-craftsman").</param>
/// <param name="Name">Display name of the ability (e.g., "[Master Craftsman]").</param>
/// <param name="Description">Narrative description of what the ability does.</param>
/// <param name="Specialization">The specialization that grants this ability.</param>
/// <param name="AbilityType">Category of ability (Passive, Triggered, UniqueAction).</param>
/// <param name="TriggerCondition">When the ability activates.</param>
/// <param name="EffectType">What type of effect the ability applies.</param>
/// <param name="EffectMagnitude">Numeric value for the effect (e.g., DC reduction amount).</param>
/// <param name="RequiredBypassType">Which bypass systems this ability applies to.</param>
/// <param name="CheckDc">DC for unique actions that require a check (0 if none).</param>
/// <param name="CheckAttribute">Attribute used for unique action checks (e.g., "WITS").</param>
public readonly record struct BypassSpecializationAbility(
    string AbilityId,
    string Name,
    string Description,
    BypassSpecialization Specialization,
    BypassAbilityType AbilityType,
    BypassTriggerCondition TriggerCondition,
    BypassEffectType EffectType,
    int EffectMagnitude,
    BypassType RequiredBypassType,
    int CheckDc,
    string CheckAttribute)
{
    // =========================================================================
    // COMPUTED PROPERTIES
    // =========================================================================

    /// <summary>
    /// Gets a value indicating whether this ability is passive (always active).
    /// </summary>
    public bool IsPassive => AbilityType == BypassAbilityType.Passive;

    /// <summary>
    /// Gets a value indicating whether this ability triggers on specific outcomes.
    /// </summary>
    public bool IsTriggered => AbilityType == BypassAbilityType.Triggered;

    /// <summary>
    /// Gets a value indicating whether this ability grants a unique action.
    /// </summary>
    public bool IsUniqueAction => AbilityType == BypassAbilityType.UniqueAction;

    /// <summary>
    /// Gets a value indicating whether this ability requires a check to use.
    /// </summary>
    public bool RequiresCheck => CheckDc > 0;

    /// <summary>
    /// Gets a value indicating whether this ability applies to all bypass types.
    /// </summary>
    public bool AppliesToAllBypasses => RequiredBypassType == BypassType.None;

    // =========================================================================
    // STATIC FACTORY METHODS - SCRAP-TINKER ABILITIES
    // =========================================================================

    /// <summary>
    /// Creates the [Master Craftsman] ability for Scrap-Tinker specialization.
    /// </summary>
    /// <returns>The Master Craftsman ability definition.</returns>
    /// <remarks>
    /// <para>
    /// <b>[Master Craftsman]</b>: Unlocks masterwork tool recipes, allowing
    /// the Scrap-Tinker to craft tools with superior quality (+1 bonus die).
    /// </para>
    /// <para>
    /// <b>Type:</b> Triggered (on craft attempt)<br/>
    /// <b>Effect:</b> Unlock masterwork recipes<br/>
    /// <b>Bypass Focus:</b> Lockpicking, Tool Crafting
    /// </para>
    /// </remarks>
    public static BypassSpecializationAbility MasterCraftsman() => new(
        AbilityId: "master-craftsman",
        Name: "[Master Craftsman]",
        Description: "Your intimate knowledge of mechanisms allows you to craft " +
                     "[Masterwork Tools] from rare components. These tools provide " +
                     "+1 bonus die on bypass attempts.",
        Specialization: BypassSpecialization.ScrapTinker,
        AbilityType: BypassAbilityType.Triggered,
        TriggerCondition: BypassTriggerCondition.OnCraftAttempt,
        EffectType: BypassEffectType.UnlockRecipes,
        EffectMagnitude: 1, // +1 bonus die from masterwork quality
        RequiredBypassType: BypassType.Lockpicking,
        CheckDc: 0,
        CheckAttribute: string.Empty);

    /// <summary>
    /// Creates the [Relock] ability for Scrap-Tinker specialization.
    /// </summary>
    /// <returns>The Relock ability definition.</returns>
    /// <remarks>
    /// <para>
    /// <b>[Relock]</b>: After picking a lock, the Scrap-Tinker can re-lock it
    /// behind them to slow pursuers. Requires a WITS check against DC 12.
    /// </para>
    /// <para>
    /// <b>Type:</b> Unique Action<br/>
    /// <b>Check:</b> WITS DC 12<br/>
    /// <b>Effect:</b> Lock returns to locked state<br/>
    /// <b>Bypass Focus:</b> Lockpicking
    /// </para>
    /// </remarks>
    public static BypassSpecializationAbility Relock() => new(
        AbilityId: "relock",
        Name: "[Relock]",
        Description: "After picking a lock, you can re-engage the mechanism behind you. " +
                     "A successful WITS check (DC 12) returns the lock to its secured state, " +
                     "slowing any pursuers.",
        Specialization: BypassSpecialization.ScrapTinker,
        AbilityType: BypassAbilityType.UniqueAction,
        TriggerCondition: BypassTriggerCondition.ManualActivation,
        EffectType: BypassEffectType.UniqueAction,
        EffectMagnitude: 0,
        RequiredBypassType: BypassType.Lockpicking,
        CheckDc: 12,
        CheckAttribute: "WITS");

    // =========================================================================
    // STATIC FACTORY METHODS - RUIN-STALKER ABILITIES
    // =========================================================================

    /// <summary>
    /// Creates the [Trap Artist] ability for Ruin-Stalker specialization.
    /// </summary>
    /// <returns>The Trap Artist ability definition.</returns>
    /// <remarks>
    /// <para>
    /// <b>[Trap Artist]</b>: After disabling a trap, the Ruin-Stalker can
    /// re-arm it as a defensive measure. Requires a WITS check against DC 14.
    /// </para>
    /// <para>
    /// <b>Type:</b> Unique Action<br/>
    /// <b>Check:</b> WITS DC 14<br/>
    /// <b>Effect:</b> Trap becomes armed and controlled by party<br/>
    /// <b>Bypass Focus:</b> Trap Disarmament
    /// </para>
    /// </remarks>
    public static BypassSpecializationAbility TrapArtist() => new(
        AbilityId: "trap-artist",
        Name: "[Trap Artist]",
        Description: "After disabling a trap, you can re-arm it as a defensive measure. " +
                     "A successful WITS check (DC 14) sets the trap to trigger on your enemies instead. " +
                     "The trap remains under your control until triggered or disarmed.",
        Specialization: BypassSpecialization.RuinStalker,
        AbilityType: BypassAbilityType.UniqueAction,
        TriggerCondition: BypassTriggerCondition.ManualActivation,
        EffectType: BypassEffectType.UniqueAction,
        EffectMagnitude: 0,
        RequiredBypassType: BypassType.TrapDisarmament,
        CheckDc: 14,
        CheckAttribute: "WITS");

    /// <summary>
    /// Creates the [Sixth Sense] ability for Ruin-Stalker specialization.
    /// </summary>
    /// <returns>The Sixth Sense ability definition.</returns>
    /// <remarks>
    /// <para>
    /// <b>[Sixth Sense]</b>: The Ruin-Stalker automatically detects traps
    /// within 10 feet without requiring a Perception check.
    /// </para>
    /// <para>
    /// <b>Type:</b> Passive<br/>
    /// <b>Trigger:</b> On movement<br/>
    /// <b>Effect:</b> Auto-detect traps within 10 feet<br/>
    /// <b>Bypass Focus:</b> Trap Disarmament
    /// </para>
    /// </remarks>
    public static BypassSpecializationAbility SixthSense() => new(
        AbilityId: "sixth-sense",
        Name: "[Sixth Sense]",
        Description: "Your experience navigating trapped ruins has sharpened your instincts. " +
                     "You automatically detect any trap within 10 feet without requiring a check. " +
                     "The trap's location is revealed, but not its exact mechanism.",
        Specialization: BypassSpecialization.RuinStalker,
        AbilityType: BypassAbilityType.Passive,
        TriggerCondition: BypassTriggerCondition.OnMovement,
        EffectType: BypassEffectType.AutoDetection,
        EffectMagnitude: 10, // 10 feet radius
        RequiredBypassType: BypassType.TrapDisarmament,
        CheckDc: 0,
        CheckAttribute: string.Empty);

    // =========================================================================
    // STATIC FACTORY METHODS - JOTUN-READER ABILITIES
    // =========================================================================

    /// <summary>
    /// Creates the [Deep Access] ability for Jötun-Reader specialization.
    /// </summary>
    /// <returns>The Deep Access ability definition.</returns>
    /// <remarks>
    /// <para>
    /// <b>[Deep Access]</b>: When the Jötun-Reader successfully hacks a terminal,
    /// they automatically gain Admin-Level access instead of basic access.
    /// </para>
    /// <para>
    /// <b>Type:</b> Triggered (on terminal hack success)<br/>
    /// <b>Effect:</b> Upgrade access level to Admin<br/>
    /// <b>Bypass Focus:</b> Terminal Hacking
    /// </para>
    /// </remarks>
    public static BypassSpecializationAbility DeepAccess() => new(
        AbilityId: "deep-access",
        Name: "[Deep Access]",
        Description: "Your communion with Old World technology runs deep. " +
                     "When you successfully hack a terminal, you automatically gain " +
                     "[Admin-Level] access, bypassing user-level restrictions entirely.",
        Specialization: BypassSpecialization.JotunReader,
        AbilityType: BypassAbilityType.Triggered,
        TriggerCondition: BypassTriggerCondition.OnTerminalHackSuccess,
        EffectType: BypassEffectType.UpgradeResult,
        EffectMagnitude: 2, // Access level upgrade (Basic=0, User=1, Admin=2)
        RequiredBypassType: BypassType.TerminalHacking,
        CheckDc: 0,
        CheckAttribute: string.Empty);

    /// <summary>
    /// Creates the [Pattern Recognition] ability for Jötun-Reader specialization.
    /// </summary>
    /// <returns>The Pattern Recognition ability definition.</returns>
    /// <remarks>
    /// <para>
    /// <b>[Pattern Recognition]</b>: The Jötun-Reader's familiarity with corrupted
    /// technology reduces the DC penalty from [Glitched] status by 2.
    /// </para>
    /// <para>
    /// <b>Type:</b> Passive<br/>
    /// <b>Trigger:</b> On glitched interaction<br/>
    /// <b>Effect:</b> Reduce [Glitched] DC penalty by 2<br/>
    /// <b>Bypass Focus:</b> Glitch Exploitation
    /// </para>
    /// </remarks>
    public static BypassSpecializationAbility PatternRecognition() => new(
        AbilityId: "pattern-recognition",
        Name: "[Pattern Recognition]",
        Description: "You've learned to read the chaos in corrupted technology. " +
                     "The DC penalty from [Glitched] status is reduced by 2 for all bypass attempts. " +
                     "What others see as madness, you see as patterns.",
        Specialization: BypassSpecialization.JotunReader,
        AbilityType: BypassAbilityType.Passive,
        TriggerCondition: BypassTriggerCondition.OnGlitchedInteraction,
        EffectType: BypassEffectType.DcReduction,
        EffectMagnitude: 2, // DC reduction amount
        RequiredBypassType: BypassType.GlitchExploitation,
        CheckDc: 0,
        CheckAttribute: string.Empty);

    // =========================================================================
    // STATIC FACTORY METHODS - GANTRY-RUNNER ABILITIES
    // =========================================================================

    /// <summary>
    /// Creates the [Fast Pick] ability for Gantry-Runner specialization.
    /// </summary>
    /// <returns>The Fast Pick ability definition.</returns>
    /// <remarks>
    /// <para>
    /// <b>[Fast Pick]</b>: The Gantry-Runner's practiced speed reduces bypass
    /// time by 1 round (minimum 1 round).
    /// </para>
    /// <para>
    /// <b>Type:</b> Passive<br/>
    /// <b>Trigger:</b> On bypass attempt<br/>
    /// <b>Effect:</b> Reduce time by 1 round<br/>
    /// <b>Bypass Focus:</b> All bypass types
    /// </para>
    /// </remarks>
    public static BypassSpecializationAbility FastPick() => new(
        AbilityId: "fast-pick",
        Name: "[Fast Pick]",
        Description: "Speed is survival on the gantries. You've learned to work fast without sacrificing precision. " +
                     "All bypass attempts take 1 fewer round to complete (minimum 1 round).",
        Specialization: BypassSpecialization.GantryRunner,
        AbilityType: BypassAbilityType.Passive,
        TriggerCondition: BypassTriggerCondition.OnBypassAttempt,
        EffectType: BypassEffectType.TimeReduction,
        EffectMagnitude: 1, // Rounds reduced
        RequiredBypassType: BypassType.None, // Applies to all
        CheckDc: 0,
        CheckAttribute: string.Empty);

    /// <summary>
    /// Creates the [Bypass Under Fire] ability for Gantry-Runner specialization.
    /// </summary>
    /// <returns>The Bypass Under Fire ability definition.</returns>
    /// <remarks>
    /// <para>
    /// <b>[Bypass Under Fire]</b>: The Gantry-Runner ignores penalties that would
    /// normally apply when attempting bypass while in combat or under threat.
    /// </para>
    /// <para>
    /// <b>Type:</b> Passive<br/>
    /// <b>Trigger:</b> On bypass while in danger<br/>
    /// <b>Effect:</b> Negate combat/danger penalties<br/>
    /// <b>Bypass Focus:</b> All bypass types
    /// </para>
    /// </remarks>
    public static BypassSpecializationAbility BypassUnderFire() => new(
        AbilityId: "bypass-under-fire",
        Name: "[Bypass Under Fire]",
        Description: "Danger sharpens your focus instead of dulling it. " +
                     "You ignore all penalties that would apply when attempting bypass " +
                     "while in combat, under threat, or being observed by enemies.",
        Specialization: BypassSpecialization.GantryRunner,
        AbilityType: BypassAbilityType.Passive,
        TriggerCondition: BypassTriggerCondition.OnBypassInDanger,
        EffectType: BypassEffectType.PenaltyNegation,
        EffectMagnitude: 0, // Full negation, not a specific amount
        RequiredBypassType: BypassType.None, // Applies to all
        CheckDc: 0,
        CheckAttribute: string.Empty);

    // =========================================================================
    // STATIC QUERY METHODS
    // =========================================================================

    /// <summary>
    /// Gets all abilities for a given specialization.
    /// </summary>
    /// <param name="specialization">The specialization to query.</param>
    /// <returns>A list of abilities for the specialization.</returns>
    /// <remarks>
    /// Returns an empty list for <see cref="BypassSpecialization.None"/>.
    /// </remarks>
    public static IReadOnlyList<BypassSpecializationAbility> GetAbilitiesFor(
        BypassSpecialization specialization)
    {
        return specialization switch
        {
            BypassSpecialization.ScrapTinker => new[] { MasterCraftsman(), Relock() },
            BypassSpecialization.RuinStalker => new[] { TrapArtist(), SixthSense() },
            BypassSpecialization.JotunReader => new[] { DeepAccess(), PatternRecognition() },
            BypassSpecialization.GantryRunner => new[] { FastPick(), BypassUnderFire() },
            _ => Array.Empty<BypassSpecializationAbility>()
        };
    }

    /// <summary>
    /// Gets all defined bypass specialization abilities.
    /// </summary>
    /// <returns>A list of all 8 abilities across all specializations.</returns>
    public static IReadOnlyList<BypassSpecializationAbility> GetAll()
    {
        return new[]
        {
            // Scrap-Tinker
            MasterCraftsman(),
            Relock(),
            // Ruin-Stalker
            TrapArtist(),
            SixthSense(),
            // Jötun-Reader
            DeepAccess(),
            PatternRecognition(),
            // Gantry-Runner
            FastPick(),
            BypassUnderFire()
        };
    }

    /// <summary>
    /// Gets an ability by its ID.
    /// </summary>
    /// <param name="abilityId">The ability identifier.</param>
    /// <returns>The ability definition, or null if not found.</returns>
    public static BypassSpecializationAbility? GetById(string abilityId)
    {
        return abilityId?.ToLowerInvariant() switch
        {
            "master-craftsman" => MasterCraftsman(),
            "relock" => Relock(),
            "trap-artist" => TrapArtist(),
            "sixth-sense" => SixthSense(),
            "deep-access" => DeepAccess(),
            "pattern-recognition" => PatternRecognition(),
            "fast-pick" => FastPick(),
            "bypass-under-fire" => BypassUnderFire(),
            _ => null
        };
    }

    // =========================================================================
    // DISPLAY METHODS
    // =========================================================================

    /// <summary>
    /// Creates a display string for the ability.
    /// </summary>
    /// <returns>A formatted multi-line string showing ability details.</returns>
    /// <example>
    /// <code>
    /// var ability = BypassSpecializationAbility.SixthSense();
    /// Console.WriteLine(ability.ToDisplayString());
    /// // Output:
    /// // [Sixth Sense] (Ruin-Stalker)
    /// // Type: Passive | Trigger: On Movement
    /// // Effect: Auto-Detection (10 ft radius)
    /// // Your experience navigating trapped ruins has sharpened your instincts...
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var lines = new List<string>
        {
            $"{Name} ({Specialization})",
            $"Type: {AbilityType} | Trigger: {FormatTriggerCondition()}",
            $"Effect: {FormatEffect()}"
        };

        if (RequiresCheck)
        {
            lines.Add($"Check: {CheckAttribute} DC {CheckDc}");
        }

        if (RequiredBypassType != BypassType.None)
        {
            lines.Add($"Applies to: {RequiredBypassType}");
        }
        else
        {
            lines.Add("Applies to: All bypass types");
        }

        lines.Add(string.Empty);
        lines.Add(Description);

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns the ability name for display.
    /// </summary>
    /// <returns>The ability name.</returns>
    public override string ToString() => Name;

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    /// <summary>
    /// Formats the trigger condition for display.
    /// </summary>
    private string FormatTriggerCondition()
    {
        return TriggerCondition switch
        {
            BypassTriggerCondition.ManualActivation => "Manual Activation",
            BypassTriggerCondition.OnMovement => "On Movement",
            BypassTriggerCondition.OnBypassAttempt => "On Bypass Attempt",
            BypassTriggerCondition.OnBypassInDanger => "On Bypass Under Threat",
            BypassTriggerCondition.OnGlitchedInteraction => "On Glitched Interaction",
            BypassTriggerCondition.OnTerminalHackSuccess => "On Terminal Hack Success",
            BypassTriggerCondition.OnCraftAttempt => "On Craft Attempt",
            _ => TriggerCondition.ToString()
        };
    }

    /// <summary>
    /// Formats the effect for display.
    /// </summary>
    private string FormatEffect()
    {
        return EffectType switch
        {
            BypassEffectType.AutoDetection => $"Auto-Detection ({EffectMagnitude} ft radius)",
            BypassEffectType.DcReduction => $"DC Reduction (-{EffectMagnitude})",
            BypassEffectType.TimeReduction => $"Time Reduction (-{EffectMagnitude} rounds)",
            BypassEffectType.PenaltyNegation => "Penalty Negation",
            BypassEffectType.UpgradeResult => "Upgrade Result",
            BypassEffectType.UnlockRecipes => "Unlock Masterwork Recipes",
            BypassEffectType.UniqueAction => "Unique Action",
            _ => EffectType.ToString()
        };
    }
}
