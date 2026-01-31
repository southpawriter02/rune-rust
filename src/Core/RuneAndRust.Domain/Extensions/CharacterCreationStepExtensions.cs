// ------------------------------------------------------------------------------
// <copyright file="CharacterCreationStepExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for CharacterCreationStep enum providing step ordering,
// navigation logic, permanent choice identification, display names, and
// thematic descriptions for the six-step character creation workflow.
// Part of v0.17.5a Creation Step Enum & State implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="CharacterCreationStep"/> enum.
/// Provides step order, navigation logic, and display information.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods support the character creation workflow by providing:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       Step ordering: 1-based step numbers for UI display and total step count.
///     </description>
///   </item>
///   <item>
///     <description>
///       Navigation control: Forward/backward step traversal and back-navigation
///       eligibility (disabled for Step 1).
///     </description>
///   </item>
///   <item>
///     <description>
///       Permanent choice identification: Only the Archetype step (Step 4) is
///       flagged as permanent, triggering a UI warning before confirmation.
///     </description>
///   </item>
///   <item>
///     <description>
///       Display information: Human-readable step names and atmospheric descriptions
///       matching the creation-workflow.json configuration.
///     </description>
///   </item>
/// </list>
/// <para>
/// Extension methods provide default values. The <c>creation-workflow.json</c>
/// configuration file is the authoritative source for display text and navigation
/// rules, but these methods serve as fallback defaults when configuration is
/// unavailable.
/// </para>
/// </remarks>
/// <seealso cref="CharacterCreationStep"/>
/// <seealso cref="RuneAndRust.Domain.Entities.CharacterCreationState"/>
public static class CharacterCreationStepExtensions
{
    // -------------------------------------------------------------------------
    // Step Order Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the 1-based step number for display purposes.
    /// </summary>
    /// <param name="step">The creation step.</param>
    /// <returns>
    /// Step number in the range 1-6, where 1 is Lineage and 6 is Summary.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Converts the 0-based enum value to a 1-based step number for human-friendly
    /// display in the creation wizard UI (e.g., "Step 3 of 6: Allocate Attributes").
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var step = CharacterCreationStep.Attributes;
    /// int number = step.GetStepNumber(); // 3
    /// </code>
    /// </example>
    public static int GetStepNumber(this CharacterCreationStep step) => (int)step + 1;

    /// <summary>
    /// Gets the total number of steps in the creation workflow.
    /// </summary>
    /// <returns>
    /// Total step count: 6 (Lineage, Background, Attributes, Archetype,
    /// Specialization, Summary).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a static utility method (not an extension method) that returns the
    /// fixed total of 6 steps. Used alongside <see cref="GetStepNumber"/> for
    /// progress display (e.g., "Step 3 of 6").
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// int total = CharacterCreationStepExtensions.GetTotalSteps(); // 6
    /// </code>
    /// </example>
    public static int GetTotalSteps() => 6;

    // -------------------------------------------------------------------------
    // Navigation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines if navigation backward is allowed from this step.
    /// </summary>
    /// <param name="step">The creation step.</param>
    /// <returns>
    /// <c>true</c> if the player can navigate to the previous step;
    /// <c>false</c> for Step 1 (Lineage), which is the first step.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Backward navigation is allowed from all steps except Lineage (Step 1),
    /// which has no preceding step. Navigating back preserves all existing
    /// selections — no data is lost when going backward.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool canBack = CharacterCreationStep.Lineage.CanGoBack();     // false
    /// bool canBack2 = CharacterCreationStep.Background.CanGoBack(); // true
    /// </code>
    /// </example>
    public static bool CanGoBack(this CharacterCreationStep step) =>
        step != CharacterCreationStep.Lineage;

    /// <summary>
    /// Determines if this step represents a permanent, irreversible choice.
    /// </summary>
    /// <param name="step">The creation step.</param>
    /// <returns>
    /// <c>true</c> if this step's selection is permanent and cannot be changed
    /// after character creation; <c>false</c> otherwise. Only the Archetype
    /// step (Step 4) is permanent.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Only <see cref="CharacterCreationStep.Archetype"/> returns <c>true</c>.
    /// When the UI detects a permanent step, it displays a prominent warning
    /// (e.g., "This choice is PERMANENT and cannot be changed after character
    /// creation.") before the player confirms their selection.
    /// </para>
    /// <para>
    /// All other steps allow changes via backward navigation before final
    /// confirmation at the Summary step.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool permanent = CharacterCreationStep.Archetype.IsPermanentChoice();       // true
    /// bool notPermanent = CharacterCreationStep.Specialization.IsPermanentChoice(); // false
    /// </code>
    /// </example>
    public static bool IsPermanentChoice(this CharacterCreationStep step) =>
        step == CharacterCreationStep.Archetype;

    /// <summary>
    /// Gets the next step in the workflow, if available.
    /// </summary>
    /// <param name="step">The current step.</param>
    /// <returns>
    /// The next <see cref="CharacterCreationStep"/> in sequence, or <c>null</c>
    /// if the current step is Summary (the final step).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Uses arithmetic on the enum's integer value to determine the next step.
    /// Returns <c>null</c> from Summary because there is no step after the
    /// final confirmation screen.
    /// </para>
    /// <para>
    /// Note: This method only determines the next step — it does not validate
    /// whether the current step's requirements are met. Use
    /// <see cref="RuneAndRust.Domain.Entities.CharacterCreationState.TryAdvance"/>
    /// for validated navigation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var next = CharacterCreationStep.Lineage.GetNextStep();  // Background
    /// var last = CharacterCreationStep.Summary.GetNextStep();   // null
    /// </code>
    /// </example>
    public static CharacterCreationStep? GetNextStep(this CharacterCreationStep step) =>
        step == CharacterCreationStep.Summary ? null : (CharacterCreationStep)((int)step + 1);

    /// <summary>
    /// Gets the previous step in the workflow, if available.
    /// </summary>
    /// <param name="step">The current step.</param>
    /// <returns>
    /// The previous <see cref="CharacterCreationStep"/> in sequence, or <c>null</c>
    /// if the current step is Lineage (the first step).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Uses arithmetic on the enum's integer value to determine the previous step.
    /// Returns <c>null</c> from Lineage because there is no step before the
    /// first selection screen.
    /// </para>
    /// <para>
    /// Note: This method only determines the previous step — it does not clear
    /// or modify any existing selections. Use
    /// <see cref="RuneAndRust.Domain.Entities.CharacterCreationState.TryGoBack"/>
    /// for validated backward navigation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var prev = CharacterCreationStep.Background.GetPreviousStep(); // Lineage
    /// var first = CharacterCreationStep.Lineage.GetPreviousStep();   // null
    /// </code>
    /// </example>
    public static CharacterCreationStep? GetPreviousStep(this CharacterCreationStep step) =>
        step == CharacterCreationStep.Lineage ? null : (CharacterCreationStep)((int)step - 1);

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the display name for this step.
    /// </summary>
    /// <param name="step">The creation step.</param>
    /// <returns>
    /// A human-readable step name suitable for UI headers and navigation labels.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Display names are imperative phrases matching the thematic tone of
    /// Aethelgard's character creation. These serve as defaults; the
    /// <c>creation-workflow.json</c> configuration provides the authoritative
    /// display names that can be customized or localized.
    /// </para>
    /// <para>
    /// Display names by step:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Lineage: "Choose Your Lineage"</description></item>
    ///   <item><description>Background: "Choose Your Background"</description></item>
    ///   <item><description>Attributes: "Allocate Attributes"</description></item>
    ///   <item><description>Archetype: "Choose Your Archetype"</description></item>
    ///   <item><description>Specialization: "Choose Your Specialization"</description></item>
    ///   <item><description>Summary: "Confirm Your Survivor"</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// string name = CharacterCreationStep.Archetype.GetDisplayName();
    /// // "Choose Your Archetype"
    /// </code>
    /// </example>
    public static string GetDisplayName(this CharacterCreationStep step) => step switch
    {
        CharacterCreationStep.Lineage => "Choose Your Lineage",
        CharacterCreationStep.Background => "Choose Your Background",
        CharacterCreationStep.Attributes => "Allocate Attributes",
        CharacterCreationStep.Archetype => "Choose Your Archetype",
        CharacterCreationStep.Specialization => "Choose Your Specialization",
        CharacterCreationStep.Summary => "Confirm Your Survivor",
        _ => step.ToString()
    };

    /// <summary>
    /// Gets the thematic description for this step.
    /// </summary>
    /// <param name="step">The creation step.</param>
    /// <returns>
    /// An atmospheric description string matching Aethelgard's narrative tone,
    /// suitable for display below the step title in the creation wizard.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Descriptions provide atmospheric context for each step, grounding the
    /// mechanical choice in the world of Aethelgard. These serve as defaults;
    /// the <c>creation-workflow.json</c> configuration provides the authoritative
    /// descriptions that can be customized or localized.
    /// </para>
    /// <para>
    /// Descriptions by step:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Lineage: "Your bloodline carries echoes of the world before."</description></item>
    ///   <item><description>Background: "What were you before the world broke?"</description></item>
    ///   <item><description>Attributes: "Define your character's core capabilities."</description></item>
    ///   <item><description>Archetype: "Your fundamental approach to survival."</description></item>
    ///   <item><description>Specialization: "Your tactical identity and unique abilities."</description></item>
    ///   <item><description>Summary: "Review your choices and begin your saga."</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// string desc = CharacterCreationStep.Lineage.GetDescription();
    /// // "Your bloodline carries echoes of the world before."
    /// </code>
    /// </example>
    public static string GetDescription(this CharacterCreationStep step) => step switch
    {
        CharacterCreationStep.Lineage => "Your bloodline carries echoes of the world before.",
        CharacterCreationStep.Background => "What were you before the world broke?",
        CharacterCreationStep.Attributes => "Define your character's core capabilities.",
        CharacterCreationStep.Archetype => "Your fundamental approach to survival.",
        CharacterCreationStep.Specialization => "Your tactical identity and unique abilities.",
        CharacterCreationStep.Summary => "Review your choices and begin your saga.",
        _ => string.Empty
    };
}
