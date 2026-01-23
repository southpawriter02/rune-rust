// ------------------------------------------------------------------------------
// <copyright file="InterrogationContext.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Contains all context needed to resolve an interrogation round,
// including character attributes, method selection, and modifiers.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Contains all context needed to resolve an interrogation round.
/// </summary>
/// <remarks>
/// <para>
/// The InterrogationContext aggregates all information required to conduct
/// a single round of interrogation, including:
/// <list type="bullet">
///   <item><description>The interrogation state (subject resistance, history)</description></item>
///   <item><description>The selected method and attribute choice</description></item>
///   <item><description>Interrogator's skills and attributes</description></item>
///   <item><description>Subject's defensive attributes (WITS for Deception)</description></item>
///   <item><description>Available resources and modifiers</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record InterrogationContext
{
    /// <summary>
    /// Gets the base social context for the interaction.
    /// </summary>
    /// <remarks>
    /// Provides the underlying social context including disposition levels
    /// and other social interaction parameters.
    /// </remarks>
    public required SocialContext BaseContext { get; init; }

    /// <summary>
    /// Gets the current state of the interrogation.
    /// </summary>
    /// <remarks>
    /// Contains the subject's remaining resistance, round history,
    /// methods used, and cumulative effects.
    /// </remarks>
    public required InterrogationState InterrogationState { get; init; }

    /// <summary>
    /// Gets the method selected for this round.
    /// </summary>
    public required InterrogationMethod SelectedMethod { get; init; }

    /// <summary>
    /// Gets the interrogator's relevant attribute value (WILL or MIGHT).
    /// </summary>
    /// <remarks>
    /// The attribute used depends on the method and the UseMight flag.
    /// BadCop and Torture can use either MIGHT or WILL.
    /// </remarks>
    public required int InterrogatorAttribute { get; init; }

    /// <summary>
    /// Gets the interrogator's Rhetoric skill level.
    /// </summary>
    /// <remarks>
    /// Added to attribute for dice pool calculation (except Torture,
    /// which uses raw attribute only).
    /// </remarks>
    public required int InterrogatorRhetoric { get; init; }

    /// <summary>
    /// Gets the subject's WITS attribute (for Deception defense).
    /// </summary>
    /// <remarks>
    /// Used when calculating the opposed DC for Deception method.
    /// Subjects with high WITS are harder to deceive.
    /// </remarks>
    public required int SubjectWits { get; init; }

    /// <summary>
    /// Gets the available resources for Bribery (gold amount).
    /// </summary>
    /// <remarks>
    /// If AvailableGold is less than the bribery cost for the subject's
    /// resistance level, Bribery method cannot be used.
    /// </remarks>
    public int AvailableGold { get; init; }

    /// <summary>
    /// Gets any bonus dice from items, abilities, or circumstances.
    /// </summary>
    /// <remarks>
    /// Examples of bonus dice sources:
    /// <list type="bullet">
    ///   <item><description>[Enhanced Interrogation] ability: +2d10</description></item>
    ///   <item><description>Torture tools item: +1d10</description></item>
    ///   <item><description>Environmental factors: varies</description></item>
    /// </list>
    /// </remarks>
    public int BonusDice { get; init; }

    /// <summary>
    /// Gets any DC modifiers from circumstances.
    /// </summary>
    /// <remarks>
    /// Positive values increase difficulty, negative values decrease it.
    /// Examples:
    /// <list type="bullet">
    ///   <item><description>Subject is injured: -2 DC</description></item>
    ///   <item><description>Subject is well-rested: +2 DC</description></item>
    ///   <item><description>Prior torture: -1 DC</description></item>
    /// </list>
    /// </remarks>
    public int DcModifier { get; init; }

    /// <summary>
    /// Gets a value indicating whether to use MIGHT instead of WILL for BadCop/Torture.
    /// </summary>
    /// <remarks>
    /// When true and the method supports it, MIGHT is used instead of WILL.
    /// This allows physically imposing characters to leverage their strength.
    /// </remarks>
    public bool UseMight { get; init; }

    /// <summary>
    /// Calculates the effective DC for the current method.
    /// </summary>
    /// <returns>The DC that must be met for success.</returns>
    /// <remarks>
    /// <para>
    /// DC calculation varies by method:
    /// <list type="bullet">
    ///   <item><description>Good Cop: 14 + DcModifier</description></item>
    ///   <item><description>Bad Cop: 12 + DcModifier</description></item>
    ///   <item><description>Deception: 16 + SubjectWits + DcModifier (opposed)</description></item>
    ///   <item><description>Bribery: 10 + DcModifier</description></item>
    ///   <item><description>Torture: SubjectWill × 2 + DcModifier</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int CalculateEffectiveDc()
    {
        var baseDc = SelectedMethod.GetBaseDc();

        // Torture uses special DC calculation
        if (SelectedMethod == InterrogationMethod.Torture)
        {
            baseDc = InterrogationState.SubjectWill * 2;
        }
        // Deception is opposed by subject's WITS
        else if (SelectedMethod == InterrogationMethod.Deception)
        {
            baseDc += SubjectWits;
        }

        return baseDc + DcModifier;
    }

    /// <summary>
    /// Calculates the dice pool for the current method.
    /// </summary>
    /// <returns>The number of dice to roll.</returns>
    /// <remarks>
    /// <para>
    /// Dice pool calculation:
    /// <list type="bullet">
    ///   <item><description>Torture: Attribute only (no skill)</description></item>
    ///   <item><description>Other methods: Attribute + Rhetoric + BonusDice</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int CalculateDicePool()
    {
        int basePool;

        if (SelectedMethod == InterrogationMethod.Torture)
        {
            // Torture uses raw attribute only - no skill contribution
            basePool = InterrogatorAttribute;
        }
        else
        {
            // Other methods use Attribute + Rhetoric
            basePool = InterrogatorAttribute + InterrogatorRhetoric;
        }

        // Add bonus dice
        basePool += BonusDice;

        // Ensure minimum of 1 die
        return Math.Max(1, basePool);
    }

    /// <summary>
    /// Gets whether Bribery can be used with current resources.
    /// </summary>
    /// <returns>True if sufficient gold is available for Bribery.</returns>
    public bool CanAffordBribery()
    {
        var briberyCost = InterrogationState.ResistanceLevel.GetBaseBriberyCost();
        return AvailableGold >= briberyCost;
    }

    /// <summary>
    /// Gets a breakdown of all modifiers affecting this round.
    /// </summary>
    /// <returns>A multi-line string describing all modifiers.</returns>
    public string ToModifierBreakdown()
    {
        var lines = new List<string>
        {
            $"Method: {SelectedMethod.GetDisplayName()}",
            $"Attribute ({SelectedMethod.GetAttribute(UseMight)}): {InterrogatorAttribute}"
        };

        if (SelectedMethod != InterrogationMethod.Torture)
        {
            lines.Add($"Rhetoric: +{InterrogatorRhetoric}");
        }

        if (BonusDice != 0)
        {
            lines.Add($"Bonus Dice: {BonusDice:+0;-0}");
        }

        lines.Add($"Dice Pool: {CalculateDicePool()}d10");
        lines.Add($"Base DC: {SelectedMethod.GetBaseDc()}");

        if (SelectedMethod == InterrogationMethod.Deception)
        {
            lines.Add($"Subject WITS: +{SubjectWits}");
        }

        if (SelectedMethod == InterrogationMethod.Torture)
        {
            lines.Add($"Torture DC (WILL × 2): {InterrogationState.SubjectWill * 2}");
        }

        if (DcModifier != 0)
        {
            lines.Add($"DC Modifier: {DcModifier:+0;-0}");
        }

        lines.Add($"Effective DC: {CalculateEffectiveDc()}");
        lines.Add($"Reliability: {SelectedMethod.GetReliabilityPercent()}%");
        lines.Add($"Duration: {SelectedMethod.GetRoundDurationMinutes()} min");

        if (SelectedMethod.RequiresResources())
        {
            lines.Add($"Cost: ~{InterrogationState.ResistanceLevel.GetBaseBriberyCost()} gold");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Creates a minimal context for testing or simple scenarios.
    /// </summary>
    /// <param name="state">The interrogation state.</param>
    /// <param name="method">The method to use.</param>
    /// <param name="attribute">The interrogator's attribute value.</param>
    /// <param name="rhetoric">The interrogator's Rhetoric skill.</param>
    /// <returns>A new InterrogationContext with minimal configuration.</returns>
    public static InterrogationContext CreateMinimal(
        InterrogationState state,
        InterrogationMethod method,
        int attribute = 5,
        int rhetoric = 3)
    {
        return new InterrogationContext
        {
            BaseContext = SocialContext.CreateMinimal("test-subject", SocialInteractionType.Interrogation),
            InterrogationState = state,
            SelectedMethod = method,
            InterrogatorAttribute = attribute,
            InterrogatorRhetoric = rhetoric,
            SubjectWits = 4,
            AvailableGold = 500,
            BonusDice = 0,
            DcModifier = 0,
            UseMight = false
        };
    }
}
