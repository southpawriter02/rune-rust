namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the gameplay context for random number generation seeding behavior.
/// </summary>
/// <remarks>
/// <para>
/// Different game contexts have different seeding requirements:
/// <list type="bullet">
///   <item><description>Combat: Locked seed prevents save-scumming during fights</description></item>
///   <item><description>Exploration: Fresh seed on load allows retry on reload</description></item>
///   <item><description>Crafting: Locked seed per session for fair crafting</description></item>
///   <item><description>Dialogue: Fresh seed for varied conversation outcomes</description></item>
/// </list>
/// </para>
/// <para>
/// The context determines whether seeds are locked (deterministic within session)
/// or fresh (randomized on game load).
/// </para>
/// </remarks>
public enum RngContext
{
    /// <summary>
    /// Combat encounters with locked seed per encounter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Seed is locked when combat begins and remains constant until combat ends.
    /// This prevents players from save-scumming to get better attack rolls.
    /// </para>
    /// <para>
    /// Reloading a save during combat produces the same roll sequence.
    /// </para>
    /// </remarks>
    Combat,

    /// <summary>
    /// Exploration and dungeon traversal with fresh seed on load.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A new random seed is generated each time the game loads.
    /// This allows players to retry trap checks, random encounters,
    /// and loot rolls by reloading a save.
    /// </para>
    /// <para>
    /// Provides a more forgiving experience for exploration mistakes.
    /// </para>
    /// </remarks>
    Exploration,

    /// <summary>
    /// Crafting and item creation with locked seed per session.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Seed is locked when a crafting session begins. Quality rolls,
    /// bonus attribute rolls, and success chances are deterministic.
    /// </para>
    /// <para>
    /// Prevents save-scumming for perfect crafting results while
    /// still allowing different outcomes between sessions.
    /// </para>
    /// </remarks>
    Crafting,

    /// <summary>
    /// Dialogue and social interactions with fresh seed on load.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Fresh seed allows varied dialogue outcomes on reload.
    /// Persuasion checks, deception rolls, and NPC reactions
    /// can differ each attempt.
    /// </para>
    /// <para>
    /// Provides a more narrative-friendly experience where
    /// conversation replays feel natural.
    /// </para>
    /// </remarks>
    Dialogue,

    /// <summary>
    /// Default context when no specific context is active.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses the current active context's seeding behavior.
    /// If no context is active, behaves like Exploration (fresh seed).
    /// </para>
    /// <para>
    /// Used as a fallback when context is not explicitly specified.
    /// </para>
    /// </remarks>
    Default
}
