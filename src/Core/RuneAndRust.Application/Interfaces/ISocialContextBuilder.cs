namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Fluent builder interface for constructing <see cref="SocialContext"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// The social context builder allows incremental construction of social interaction
/// contexts by chaining modifier methods. It follows the builder pattern from
/// <see cref="ISkillContextBuilder"/> adapted for social-specific factors.
/// </para>
/// </remarks>
public interface ISocialContextBuilder
{
    /// <summary>
    /// Sets the type of social interaction.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithInteractionType(SocialInteractionType interactionType);

    /// <summary>
    /// Sets the target NPC for the interaction.
    /// </summary>
    /// <param name="targetId">The NPC identifier.</param>
    /// <param name="disposition">The NPC's current disposition.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithTarget(string targetId, DispositionLevel disposition);

    /// <summary>
    /// Sets the target's faction, if any.
    /// </summary>
    /// <param name="factionId">The faction identifier.</param>
    /// <param name="playerStanding">The player's standing with this faction (-100 to +100).</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithFaction(string factionId, int playerStanding);

    /// <summary>
    /// Sets the cultural protocol to use.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithCulture(string cultureId);

    /// <summary>
    /// Sets the base difficulty class.
    /// </summary>
    /// <param name="baseDc">The base DC before modifiers.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithBaseDc(int baseDc);

    /// <summary>
    /// Adds a social modifier.
    /// </summary>
    /// <param name="modifier">The social modifier to add.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithSocialModifier(SocialModifier modifier);

    /// <summary>
    /// Adds an equipment modifier.
    /// </summary>
    /// <param name="modifier">The equipment modifier to add.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithEquipment(EquipmentModifier modifier);

    /// <summary>
    /// Adds a situational modifier.
    /// </summary>
    /// <param name="modifier">The situational modifier to add.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithSituation(SituationalModifier modifier);

    /// <summary>
    /// Adds a status effect affecting the check.
    /// </summary>
    /// <param name="statusId">The status effect identifier.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithAppliedStatus(string statusId);

    /// <summary>
    /// Marks the target as suspicious (Deception DC +4).
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder TargetIsSuspicious();

    /// <summary>
    /// Marks the target as trusting (Deception DC -4).
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder TargetIsTrusting();

    /// <summary>
    /// Sets argument alignment with NPC values.
    /// </summary>
    /// <param name="aligned">True if argument aligns with NPC beliefs.</param>
    /// <param name="reason">Optional reason for alignment/misalignment.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithArgumentAlignment(bool aligned, string? reason = null);

    /// <summary>
    /// Adds evidence supporting the argument.
    /// </summary>
    /// <param name="evidenceDescription">Description of the evidence.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithEvidence(string evidenceDescription);

    /// <summary>
    /// Sets strength comparison for intimidation.
    /// </summary>
    /// <param name="playerStronger">True if player is stronger than target.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithStrengthComparison(bool playerStronger);

    /// <summary>
    /// Marks that the target has backup nearby (Intimidation penalty).
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder TargetHasBackup();

    /// <summary>
    /// Marks that the player is wielding a visible artifact.
    /// </summary>
    /// <param name="artifactName">Name of the artifact.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WieldingArtifact(string artifactName);

    /// <summary>
    /// Adds player reputation modifier.
    /// </summary>
    /// <param name="status">The reputation status (e.g., "Honored", "Feared").</param>
    /// <param name="diceModifier">The dice modifier from this reputation.</param>
    /// <param name="appliesToTypes">Optional list of interaction types this applies to.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithReputation(string status, int diceModifier,
        IReadOnlyList<SocialInteractionType>? appliesToTypes = null);

    /// <summary>
    /// Marks the player as untrustworthy (all social checks DC +3).
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder IsUntrustworthy();

    /// <summary>
    /// Marks the target as stressed (Persuasion +1d10).
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder TargetIsStressed();

    /// <summary>
    /// Marks the target as feared (Persuasion +1d10).
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder TargetIsFeared();

    /// <summary>
    /// Adds cultural knowledge modifier.
    /// </summary>
    /// <param name="cultureName">The culture name.</param>
    /// <param name="fluent">True if fluent in the culture's cant.</param>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder WithCulturalKnowledge(string cultureName, bool fluent);

    /// <summary>
    /// Builds the final social context.
    /// </summary>
    /// <returns>An immutable <see cref="SocialContext"/>.</returns>
    SocialContext Build();

    /// <summary>
    /// Resets the builder to default state for reuse.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    ISocialContextBuilder Reset();
}
