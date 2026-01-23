// ------------------------------------------------------------------------------
// <copyright file="ArgumentAlignment.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Evaluates how well an argument aligns with NPC values and beliefs.
// Part of v0.15.3b Persuasion System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Evaluates how well an argument aligns with NPC values and beliefs.
/// </summary>
/// <remarks>
/// <para>
/// Argument alignment provides dice modifiers based on whether the player's
/// reasoning appeals to or contradicts the NPC's core values and motivations.
/// </para>
/// <para>
/// NPCs have defined value sets (e.g., "profit-minded", "risk-averse",
/// "respects loyalty") that can be matched against argument themes.
/// </para>
/// <para>
/// +1d10 for aligned arguments, -1d10 for contradicting arguments.
/// </para>
/// </remarks>
/// <param name="IsAligned">Whether the argument aligns with NPC values.</param>
/// <param name="IsContradicting">Whether the argument contradicts NPC values.</param>
/// <param name="MatchedValue">The NPC value that was matched, if any.</param>
/// <param name="ContradictedValue">The NPC value that was contradicted, if any.</param>
/// <param name="Reason">Explanation of the alignment evaluation.</param>
public readonly record struct ArgumentAlignment(
    bool IsAligned,
    bool IsContradicting,
    string? MatchedValue,
    string? ContradictedValue,
    string? Reason)
{
    /// <summary>
    /// Gets the dice modifier based on alignment.
    /// </summary>
    /// <remarks>
    /// Returns +1 if aligned, -1 if contradicting, 0 otherwise.
    /// These are mutually exclusive in normal circumstances.
    /// </remarks>
    public int DiceModifier
    {
        get
        {
            if (IsAligned) return 1;
            if (IsContradicting) return -1;
            return 0;
        }
    }

    /// <summary>
    /// Gets whether this alignment provides any modifier.
    /// </summary>
    public bool HasModifier => IsAligned || IsContradicting;

    /// <summary>
    /// Creates a neutral alignment (no match or contradiction).
    /// </summary>
    /// <returns>A neutral ArgumentAlignment with no modifiers.</returns>
    public static ArgumentAlignment CreateNeutral()
    {
        return new ArgumentAlignment(
            IsAligned: false,
            IsContradicting: false,
            MatchedValue: null,
            ContradictedValue: null,
            Reason: "Argument does not strongly relate to NPC values");
    }

    /// <summary>
    /// Creates an aligned argument.
    /// </summary>
    /// <param name="matchedValue">The NPC value that was matched.</param>
    /// <param name="reason">Explanation of why the argument aligns.</param>
    /// <returns>An ArgumentAlignment with +1d10 modifier.</returns>
    public static ArgumentAlignment CreateAligned(string matchedValue, string? reason = null)
    {
        return new ArgumentAlignment(
            IsAligned: true,
            IsContradicting: false,
            MatchedValue: matchedValue,
            ContradictedValue: null,
            Reason: reason ?? $"Argument appeals to NPC's '{matchedValue}' value");
    }

    /// <summary>
    /// Creates a contradicting argument.
    /// </summary>
    /// <param name="contradictedValue">The NPC value that was contradicted.</param>
    /// <param name="reason">Explanation of why the argument contradicts.</param>
    /// <returns>An ArgumentAlignment with -1d10 modifier.</returns>
    public static ArgumentAlignment CreateContradicting(string contradictedValue, string? reason = null)
    {
        return new ArgumentAlignment(
            IsAligned: false,
            IsContradicting: true,
            MatchedValue: null,
            ContradictedValue: contradictedValue,
            Reason: reason ?? $"Argument conflicts with NPC's '{contradictedValue}' value");
    }

    /// <summary>
    /// Evaluates argument alignment against a set of NPC values.
    /// </summary>
    /// <param name="argumentThemes">The themes present in the player's argument.</param>
    /// <param name="npcValues">The NPC's core values and motivations.</param>
    /// <param name="npcDislikes">Values the NPC opposes or dislikes.</param>
    /// <returns>An ArgumentAlignment based on the evaluation.</returns>
    /// <remarks>
    /// Contradictions take priority over alignments. If an argument contains
    /// both a value match and a dislike match, the contradiction is returned.
    /// </remarks>
    public static ArgumentAlignment Evaluate(
        IEnumerable<string> argumentThemes,
        IEnumerable<string> npcValues,
        IEnumerable<string> npcDislikes)
    {
        var themes = argumentThemes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var values = npcValues.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var dislikes = npcDislikes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Check for contradictions first (they take priority)
        var contradiction = themes.Intersect(dislikes, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        if (contradiction != null)
        {
            return CreateContradicting(contradiction);
        }

        // Check for alignment
        var match = themes.Intersect(values, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        if (match != null)
        {
            return CreateAligned(match);
        }

        return CreateNeutral();
    }

    /// <summary>
    /// Gets a human-readable description of the alignment.
    /// </summary>
    /// <returns>A string describing the alignment and its effect.</returns>
    public string ToDescription()
    {
        if (IsAligned)
        {
            return $"Aligned (+1d10): Appeals to '{MatchedValue}'";
        }
        if (IsContradicting)
        {
            return $"Contradicts (-1d10): Conflicts with '{ContradictedValue}'";
        }
        return "Neutral: No strong value alignment";
    }

    /// <inheritdoc/>
    public override string ToString() => ToDescription();
}
