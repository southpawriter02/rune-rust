namespace RuneAndRust.Domain.Enums;

/// <summary>
/// NPC disposition toward the player character.
/// </summary>
/// <remarks>
/// <para>
/// Disposition affects social skill checks like persuasion and negotiation
/// through dice pool modifiers in <see cref="ValueObjects.TargetModifier"/>.
/// </para>
/// <para>
/// Modifier values:
/// <list type="bullet">
///   <item><description>Friendly: +2d10</description></item>
///   <item><description>Neutral: +0d10</description></item>
///   <item><description>Suspicious: -2d10</description></item>
///   <item><description>Hostile: -2d10 (some approaches may be impossible)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum Disposition
{
    /// <summary>
    /// Friendly and receptive. +2d10 to persuasion.
    /// </summary>
    Friendly = 0,

    /// <summary>
    /// Neutral, no strong feelings. No modifier.
    /// </summary>
    Neutral = 1,

    /// <summary>
    /// Wary or distrustful. -2d10 to persuasion.
    /// </summary>
    Suspicious = 2,

    /// <summary>
    /// Actively opposed. -2d10 to persuasion, some approaches impossible.
    /// </summary>
    Hostile = 3
}
