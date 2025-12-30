namespace RuneAndRust.Core.Enums;

/// <summary>
/// Faction disposition tiers based on reputation value.
/// Determines NPC behavior, dialogue availability, and trade access.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
public enum Disposition
{
    /// <summary>
    /// Reputation -100 to -50: Attack on sight, no trade, no dialogue.
    /// NPCs of this faction will initiate combat when encountered.
    /// </summary>
    Hated = 0,

    /// <summary>
    /// Reputation -49 to -10: Refuse dialogue, threatening barks.
    /// NPCs will not attack unprovoked but refuse all interaction.
    /// </summary>
    Hostile = 1,

    /// <summary>
    /// Reputation -9 to +9: Indifferent, basic services only.
    /// Standard interactions available, no bonuses or penalties.
    /// </summary>
    Neutral = 2,

    /// <summary>
    /// Reputation +10 to +49: Better prices, extra dialogue options.
    /// Faction members offer discounts and share additional information.
    /// </summary>
    Friendly = 3,

    /// <summary>
    /// Reputation +50 to +100: Unique items, special quests, faction benefits.
    /// Full access to faction resources and exclusive content.
    /// </summary>
    Exalted = 4
}
