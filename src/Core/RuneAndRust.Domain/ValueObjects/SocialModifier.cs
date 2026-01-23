namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Represents a modifier specific to social interactions.
/// </summary>
/// <remarks>
/// <para>
/// Social modifiers capture factors unique to social checks that don't fit
/// into equipment, situational, or environmental categories. These include
/// argument alignment, target state, faction relationships, and cultural factors.
/// </para>
/// </remarks>
/// <param name="Source">The source of this modifier (e.g., "Value Alignment", "Faction Standing").</param>
/// <param name="Description">Human-readable description of the modifier.</param>
/// <param name="DiceModifier">Bonus or penalty to dice pool.</param>
/// <param name="DcModifier">Bonus or penalty to difficulty class.</param>
/// <param name="ModifierType">The category of social modifier.</param>
/// <param name="AppliesToInteractionTypes">Which interaction types this modifier affects.</param>
public readonly record struct SocialModifier(
    string Source,
    string Description,
    int DiceModifier,
    int DcModifier,
    SocialModifierType ModifierType,
    IReadOnlyList<SocialInteractionType>? AppliesToInteractionTypes = null) : ISkillModifier
{
    /// <summary>
    /// Gets the modifier category.
    /// </summary>
    public ModifierCategory Category => ModifierCategory.Social;

    /// <summary>
    /// Gets whether this modifier applies to a specific interaction type.
    /// </summary>
    /// <param name="interactionType">The interaction type to check.</param>
    /// <returns>True if the modifier applies.</returns>
    /// <remarks>
    /// If no specific types are listed, the modifier applies to all interaction types.
    /// </remarks>
    public bool AppliesTo(SocialInteractionType interactionType)
    {
        // If no specific types are listed, it applies to all
        if (AppliesToInteractionTypes == null || AppliesToInteractionTypes.Count == 0)
            return true;

        return AppliesToInteractionTypes.Contains(interactionType);
    }

    /// <summary>
    /// Creates a faction standing modifier.
    /// </summary>
    /// <param name="factionId">The faction identifier.</param>
    /// <param name="diceModifier">The dice modifier from faction standing.</param>
    /// <param name="standing">The standing description (e.g., "Honored", "Hostile").</param>
    /// <returns>A faction standing social modifier.</returns>
    public static SocialModifier FactionStanding(string factionId, int diceModifier, string standing)
    {
        return new SocialModifier(
            Source: "Faction Standing",
            Description: $"{standing} with {factionId}",
            DiceModifier: diceModifier,
            DcModifier: 0,
            ModifierType: SocialModifierType.FactionRelation);
    }

    /// <summary>
    /// Creates an argument alignment modifier.
    /// </summary>
    /// <param name="aligned">True if the argument aligns with NPC values.</param>
    /// <param name="reason">Optional reason for alignment/misalignment.</param>
    /// <returns>An argument alignment social modifier.</returns>
    /// <remarks>
    /// Aligned arguments provide +1d10, contradicting arguments provide -1d10.
    /// Only applies to Persuasion checks.
    /// </remarks>
    public static SocialModifier ArgumentAlignment(bool aligned, string? reason = null)
    {
        var desc = aligned
            ? $"Argument aligns with NPC values{(reason != null ? $": {reason}" : "")}"
            : $"Argument contradicts NPC values{(reason != null ? $": {reason}" : "")}";

        return new SocialModifier(
            Source: "Argument Alignment",
            Description: desc,
            DiceModifier: aligned ? 1 : -1,
            DcModifier: 0,
            ModifierType: SocialModifierType.ArgumentQuality,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Persuasion });
    }

    /// <summary>
    /// Creates an evidence modifier.
    /// </summary>
    /// <param name="evidenceDescription">Description of the evidence provided.</param>
    /// <returns>An evidence social modifier providing +2d10.</returns>
    /// <remarks>
    /// Evidence that supports the player's argument provides +2d10.
    /// Only applies to Persuasion checks.
    /// </remarks>
    public static SocialModifier Evidence(string evidenceDescription)
    {
        return new SocialModifier(
            Source: "Evidence",
            Description: evidenceDescription,
            DiceModifier: 2,
            DcModifier: 0,
            ModifierType: SocialModifierType.Evidence,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Persuasion });
    }

    /// <summary>
    /// Creates a suspicion modifier for deception checks.
    /// </summary>
    /// <returns>A target suspicion modifier adding DC +4.</returns>
    /// <remarks>
    /// When the target is already suspicious of the player, Deception DC increases by 4.
    /// </remarks>
    public static SocialModifier Suspicious()
    {
        return new SocialModifier(
            Source: "Target Suspicion",
            Description: "Target is suspicious",
            DiceModifier: 0,
            DcModifier: 4,
            ModifierType: SocialModifierType.TargetState,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Deception });
    }

    /// <summary>
    /// Creates a trusting modifier for deception checks.
    /// </summary>
    /// <returns>A target trust modifier reducing DC by 4.</returns>
    /// <remarks>
    /// When the target is trusting of the player, Deception DC decreases by 4.
    /// </remarks>
    public static SocialModifier Trusting()
    {
        return new SocialModifier(
            Source: "Target Trust",
            Description: "Target is trusting",
            DiceModifier: 0,
            DcModifier: -4,
            ModifierType: SocialModifierType.TargetState,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Deception });
    }

    /// <summary>
    /// Creates a strength comparison modifier for intimidation.
    /// </summary>
    /// <param name="playerStronger">True if the player is stronger than the target.</param>
    /// <returns>A strength comparison modifier for Intimidation.</returns>
    /// <remarks>
    /// Being stronger than the target provides +1d10, being weaker provides -1d10.
    /// </remarks>
    public static SocialModifier StrengthComparison(bool playerStronger)
    {
        return new SocialModifier(
            Source: "Strength Comparison",
            Description: playerStronger ? "Target is weaker" : "Target is stronger",
            DiceModifier: playerStronger ? 1 : -1,
            DcModifier: 0,
            ModifierType: SocialModifierType.TargetState,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Intimidation });
    }

    /// <summary>
    /// Creates a reputation modifier.
    /// </summary>
    /// <param name="reputationStatus">The reputation status (e.g., "Honored", "Feared").</param>
    /// <param name="diceModifier">The dice modifier from this reputation.</param>
    /// <param name="appliesToTypes">Optional list of interaction types this applies to.</param>
    /// <returns>A reputation social modifier.</returns>
    public static SocialModifier Reputation(string reputationStatus, int diceModifier,
        IReadOnlyList<SocialInteractionType>? appliesToTypes = null)
    {
        return new SocialModifier(
            Source: "Reputation",
            Description: $"[{reputationStatus}] reputation",
            DiceModifier: diceModifier,
            DcModifier: 0,
            ModifierType: SocialModifierType.Reputation,
            AppliesToInteractionTypes: appliesToTypes);
    }

    /// <summary>
    /// Creates an untrustworthy flag modifier.
    /// </summary>
    /// <returns>An untrustworthy modifier adding DC +3 to all social checks.</returns>
    /// <remarks>
    /// The [Untrustworthy] flag is typically applied after a fumbled Deception check.
    /// It persists until the player completes a recovery action.
    /// </remarks>
    public static SocialModifier Untrustworthy()
    {
        return new SocialModifier(
            Source: "Untrustworthy",
            Description: "[Untrustworthy] reputation",
            DiceModifier: 0,
            DcModifier: 3,
            ModifierType: SocialModifierType.Reputation);
    }

    /// <summary>
    /// Creates a backup presence modifier for intimidation.
    /// </summary>
    /// <returns>A backup presence modifier applying -1d10 to Intimidation.</returns>
    /// <remarks>
    /// When the target has allies nearby, they are harder to intimidate.
    /// </remarks>
    public static SocialModifier HasBackup()
    {
        return new SocialModifier(
            Source: "Backup Present",
            Description: "Target has backup nearby",
            DiceModifier: -1,
            DcModifier: 0,
            ModifierType: SocialModifierType.TargetState,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Intimidation });
    }

    /// <summary>
    /// Creates an intimidating ally modifier.
    /// </summary>
    /// <param name="allyName">Name of the intimidating ally.</param>
    /// <returns>An intimidating ally modifier providing +1d10.</returns>
    /// <remarks>
    /// Having an intimidating companion present enhances Intimidation checks.
    /// </remarks>
    public static SocialModifier IntimidatingAlly(string allyName)
    {
        return new SocialModifier(
            Source: "Intimidating Ally",
            Description: $"{allyName} present",
            DiceModifier: 1,
            DcModifier: 0,
            ModifierType: SocialModifierType.TargetState,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Intimidation });
    }

    /// <summary>
    /// Creates an artifact modifier for intimidation.
    /// </summary>
    /// <param name="artifactName">Name of the wielded artifact.</param>
    /// <returns>An artifact modifier providing +1d10 to Intimidation.</returns>
    /// <remarks>
    /// Wielding a visibly powerful artifact enhances Intimidation.
    /// </remarks>
    public static SocialModifier WieldingArtifact(string artifactName)
    {
        return new SocialModifier(
            Source: "Wielding Artifact",
            Description: $"Displaying {artifactName}",
            DiceModifier: 1,
            DcModifier: 0,
            ModifierType: SocialModifierType.Equipment,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Intimidation });
    }

    /// <summary>
    /// Creates a cultural knowledge modifier.
    /// </summary>
    /// <param name="cultureName">The culture name.</param>
    /// <param name="fluent">True if fluent in the culture's cant.</param>
    /// <returns>A cultural knowledge modifier.</returns>
    /// <remarks>
    /// Fluent knowledge of a culture's cant provides +1d10, no knowledge provides -1d10.
    /// </remarks>
    public static SocialModifier CulturalKnowledge(string cultureName, bool fluent)
    {
        return new SocialModifier(
            Source: "Cultural Knowledge",
            Description: fluent ? $"Fluent in {cultureName} cant" : $"No knowledge of {cultureName} cant",
            DiceModifier: fluent ? 1 : -1,
            DcModifier: 0,
            ModifierType: SocialModifierType.Cultural,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Protocol });
    }

    /// <summary>
    /// Creates a target stressed modifier.
    /// </summary>
    /// <returns>A target stressed modifier providing +1d10 to Persuasion.</returns>
    /// <remarks>
    /// Stressed or frightened targets are easier to persuade.
    /// </remarks>
    public static SocialModifier TargetStressed()
    {
        return new SocialModifier(
            Source: "Target State",
            Description: "Target is [Stressed]",
            DiceModifier: 1,
            DcModifier: 0,
            ModifierType: SocialModifierType.TargetState,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Persuasion });
    }

    /// <summary>
    /// Creates a target feared modifier.
    /// </summary>
    /// <returns>A target feared modifier providing +1d10 to Persuasion.</returns>
    /// <remarks>
    /// Frightened targets are more easily persuaded.
    /// </remarks>
    public static SocialModifier TargetFeared()
    {
        return new SocialModifier(
            Source: "Target State",
            Description: "Target is [Feared]",
            DiceModifier: 1,
            DcModifier: 0,
            ModifierType: SocialModifierType.TargetState,
            AppliesToInteractionTypes: new[] { SocialInteractionType.Persuasion });
    }

    /// <summary>
    /// Creates a custom social modifier.
    /// </summary>
    /// <param name="source">The source of the modifier.</param>
    /// <param name="description">Description of the modifier.</param>
    /// <param name="diceModifier">Dice pool modifier.</param>
    /// <param name="dcModifier">DC modifier.</param>
    /// <param name="modifierType">The type of social modifier.</param>
    /// <param name="appliesToTypes">Optional specific interaction types.</param>
    /// <returns>A custom social modifier.</returns>
    public static SocialModifier Custom(
        string source,
        string description,
        int diceModifier,
        int dcModifier = 0,
        SocialModifierType modifierType = SocialModifierType.TargetState,
        IReadOnlyList<SocialInteractionType>? appliesToTypes = null)
    {
        return new SocialModifier(
            Source: source,
            Description: description,
            DiceModifier: diceModifier,
            DcModifier: dcModifier,
            ModifierType: modifierType,
            AppliesToInteractionTypes: appliesToTypes);
    }

    /// <summary>
    /// Returns a short description for UI display.
    /// </summary>
    /// <returns>A formatted string showing the modifier effect.</returns>
    public string ToShortDescription()
    {
        var parts = new List<string> { Description };

        if (DiceModifier != 0)
        {
            var diceStr = DiceModifier > 0 ? $"+{DiceModifier}d10" : $"{DiceModifier}d10";
            parts.Add($"({diceStr})");
        }

        if (DcModifier != 0)
        {
            var dcStr = DcModifier > 0 ? $"DC +{DcModifier}" : $"DC {DcModifier}";
            parts.Add($"({dcStr})");
        }

        return string.Join(" ", parts);
    }

    /// <inheritdoc/>
    public override string ToString() => ToShortDescription();
}
