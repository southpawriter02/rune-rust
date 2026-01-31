// ═══════════════════════════════════════════════════════════════════════════════
// CharacterCreationStep.cs
// Enum defining the six sequential steps in the character creation wizard.
// Each step represents a distinct phase where the player makes foundational
// choices for their character, from lineage selection through final confirmation.
// Version: 0.17.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the sequential steps in the character creation wizard.
/// Each step represents a distinct phase where the player makes foundational
/// choices for their character.
/// </summary>
/// <remarks>
/// <para>
/// The character creation workflow guides the player through six ordered steps,
/// beginning with lineage selection and ending with a summary/confirmation screen.
/// Each step collects one or more selections that define the character's identity,
/// abilities, and starting resources.
/// </para>
/// <para>
/// The six steps are:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="Lineage"/> (Step 1): Blood heritage — determines attribute
///       modifiers, passive bonuses, and Trauma Economy baseline.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Background"/> (Step 2): Pre-Silence profession — grants
///       starting skills and equipment.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Attributes"/> (Step 3): Point allocation — defines core
///       capabilities via Simple or Advanced mode.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Archetype"/> (Step 4): Combat role — PERMANENT CHOICE that
///       determines available specializations and starting abilities.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Specialization"/> (Step 5): Tactical identity — grants
///       Tier 1 abilities and optional special resource.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Summary"/> (Step 6): Review and confirmation — enter
///       character name and confirm all choices before persistence.
///     </description>
///   </item>
/// </list>
/// <para>
/// Enum values are explicitly assigned (0-5) matching the step order (1-6) to
/// enable arithmetic navigation via <c>GetNextStep()</c> and
/// <c>GetPreviousStep()</c> extension methods. These integer values must not be
/// changed once persisted.
/// </para>
/// <para>
/// Step 4 (Archetype) is the only permanent choice in the workflow. All other
/// selections can be changed by navigating back to the relevant step before
/// final confirmation.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.Entities.CharacterCreationState"/>
/// <seealso cref="RuneAndRust.Domain.Extensions.CharacterCreationStepExtensions"/>
public enum CharacterCreationStep
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CREATION STEPS (ordered 0-5 for arithmetic navigation)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Step 1: Select blood heritage (Clan-Born, Rune-Marked, Iron-Blooded, Vargr-Kin).
    /// Determines attribute modifiers, passive bonuses, and trauma baseline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The first step in character creation. The player selects one of four lineages,
    /// each providing unique attribute modifiers, a passive bonus, and a unique trait.
    /// Lineage also determines the character's starting Corruption and Stress values
    /// through the Trauma Economy integration.
    /// </para>
    /// <para>
    /// Clan-Born lineage requires an additional selection: a flexible +1 attribute
    /// bonus that the player assigns to any core attribute. This sub-selection must
    /// be completed before advancing to the next step.
    /// </para>
    /// <para>
    /// This is the first step in the workflow — backward navigation is not available.
    /// </para>
    /// </remarks>
    Lineage = 0,

    /// <summary>
    /// Step 2: Select pre-Silence profession (Village Smith, Wandering Skald, etc.).
    /// Grants starting skills and equipment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The player selects one of six backgrounds representing their character's
    /// profession before the Silence. Each background provides starting skill
    /// bonuses and starting equipment that give the character an initial toolkit
    /// for survival.
    /// </para>
    /// <para>
    /// Background is a non-permanent choice that can be changed by navigating back
    /// from later steps before final confirmation.
    /// </para>
    /// </remarks>
    Background = 1,

    /// <summary>
    /// Step 3: Allocate attribute points using Simple or Advanced mode.
    /// Determines core capabilities (Might, Finesse, Wits, Will, Sturdiness).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The player allocates attribute points across five core attributes using either:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Simple Mode</c>: Auto-applies the archetype's recommended build.
    ///       No manual adjustment required.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Advanced Mode</c>: Manual point-buy with 15 points (14 for Adept).
    ///       Points 9-10 cost 2 each; all others cost 1.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// The UI provides a live preview of derived stats (HP, Stamina, AP) as
    /// attribute values change.
    /// </para>
    /// </remarks>
    Attributes = 2,

    /// <summary>
    /// Step 4: Select combat role (Warrior, Skirmisher, Mystic, Adept).
    /// PERMANENT CHOICE — Cannot be changed after character creation.
    /// Determines available specializations and starting abilities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the only permanent choice in the character creation workflow.
    /// Once confirmed, the archetype cannot be changed. The UI displays a
    /// prominent warning before the player confirms their selection.
    /// </para>
    /// <para>
    /// Archetype selection determines:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Three starting abilities</description></item>
    ///   <item><description>HP, Stamina, and AP bonuses</description></item>
    ///   <item><description>Primary resource type (Stamina or Aether)</description></item>
    ///   <item><description>Available specializations in Step 5</description></item>
    /// </list>
    /// </remarks>
    Archetype = 3,

    /// <summary>
    /// Step 5: Select tactical identity from archetype's available options.
    /// First specialization is free; grants Tier 1 abilities and special resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The player selects one specialization from those available for their
    /// chosen archetype. The first specialization is free during character
    /// creation. Additional specializations can be unlocked during gameplay
    /// for 3 Progression Points each.
    /// </para>
    /// <para>
    /// Specialization grants:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Three Tier 1 abilities (unlocked immediately)</description></item>
    ///   <item><description>Optional special resource (e.g., Rage for Berserkr)</description></item>
    ///   <item><description>Path type classification (Coherent or Heretical)</description></item>
    /// </list>
    /// <para>
    /// Heretical specializations display a Corruption warning during selection.
    /// </para>
    /// </remarks>
    Specialization = 4,

    /// <summary>
    /// Step 6: Review all choices, enter character name, and confirm creation.
    /// Final step before character is persisted to database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The summary step displays all previous selections, calculated derived
    /// stats, granted abilities, and starting equipment. The player enters
    /// a character name (2-20 characters, letters/spaces/hyphens only) and
    /// confirms creation.
    /// </para>
    /// <para>
    /// On confirmation, the character is fully initialized with maximum
    /// resources, starting abilities, equipment, and saga progression set
    /// to zero. The character is then persisted to the database.
    /// </para>
    /// <para>
    /// This is the final step — forward navigation leads to character
    /// initialization and game start.
    /// </para>
    /// </remarks>
    Summary = 5
}
