namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object for faction definitions loaded from config/factions.json.
/// </summary>
/// <remarks>
/// <para>Represents a single faction in the Aethelgard world. Each faction has a unique ID,
/// display name, philosophy, primary location, and relationships with other factions
/// (allies and enemies).</para>
///
/// <para>The 5 major factions per design doc (v1.2):</para>
/// <list type="bullet">
///   <item><description>iron-banes — Anti-Undying zealots (Trunk/Roots patrols)</description></item>
///   <item><description>god-sleeper-cultists — Jötun-Forged worshippers (Jötunheim temples)</description></item>
///   <item><description>jotun-readers — Pre-Glitch scholars (Alfheim, terminals)</description></item>
///   <item><description>rust-clans — Midgard survivors (Midgard, trade outposts)</description></item>
///   <item><description>independents — Unaffiliated (Anywhere)</description></item>
/// </list>
/// </remarks>
public record FactionDefinitionDto
{
    /// <summary>
    /// Gets the unique faction identifier (e.g., "iron-banes").
    /// </summary>
    /// <remarks>
    /// Used as the key in reputation dictionaries. Case-insensitive matching.
    /// Must match the faction IDs used in quest definition <c>ReputationChanges</c> dictionaries.
    /// </remarks>
    public string FactionId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name (e.g., "Iron-Banes").
    /// </summary>
    /// <remarks>
    /// Used in UI messages like "+25 Iron-Bane Reputation" and
    /// "Your standing with Iron-Banes is now Friendly!"
    /// </remarks>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the faction's philosophy/belief description.
    /// </summary>
    /// <remarks>
    /// Shown in faction information panels and dialogue context.
    /// </remarks>
    public string Philosophy { get; init; } = string.Empty;

    /// <summary>
    /// Gets the faction's primary location in the game world.
    /// </summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>
    /// Gets the IDs of factions allied with this faction.
    /// </summary>
    /// <remarks>
    /// Allied factions have a "Friendly" default relationship per design doc Section 4.1.
    /// Example: Iron-Banes are allied with Rust-Clans.
    /// </remarks>
    public IReadOnlyList<string> Allies { get; init; } = [];

    /// <summary>
    /// Gets the IDs of factions hostile to this faction.
    /// </summary>
    /// <remarks>
    /// Enemy factions have a "Hostile" default relationship per design doc Section 4.1.
    /// Example: Iron-Banes are enemies with God-Sleeper Cultists.
    /// </remarks>
    public IReadOnlyList<string> Enemies { get; init; } = [];

    /// <summary>
    /// Gets the default starting reputation value for new players.
    /// </summary>
    /// <remarks>
    /// Typically 0 (Neutral). Can be overridden per faction if the design requires
    /// players to start with a non-neutral standing.
    /// </remarks>
    public int DefaultReputation { get; init; } = 0;
}
