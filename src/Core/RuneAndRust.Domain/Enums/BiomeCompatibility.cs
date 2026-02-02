namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the adjacency compatibility between two realms.
/// </summary>
/// <remarks>
/// <para>
/// BiomeCompatibility determines how realm pairs can be placed in dungeon generation.
/// Some realms can directly neighbor, others require transitional buffer zones,
/// and incompatible pairs cannot be adjacent under any circumstances.
/// </para>
/// <para>
/// Critical Incompatibilities:
/// <list type="bullet">
/// <item>Muspelheim (Fire) ↔ Niflheim (Ice) — Elemental opposition</item>
/// <item>Muspelheim (Fire) ↔ Vanaheim (Bio) — Extreme heat destroys organic matter</item>
/// </list>
/// </para>
/// </remarks>
public enum BiomeCompatibility
{
    /// <summary>
    /// Realms can directly neighbor without transition zones.
    /// </summary>
    /// <remarks>
    /// The biomes are environmentally similar enough that direct adjacency
    /// makes narrative and mechanical sense. No buffer rooms required.
    /// </remarks>
    Compatible = 0,

    /// <summary>
    /// Realms can neighbor but require 1-3 transition rooms.
    /// </summary>
    /// <remarks>
    /// The biomes have environmental differences that require gradual
    /// transition. Transition rooms blend properties from both realms.
    /// </remarks>
    RequiresTransition = 1,

    /// <summary>
    /// Realms cannot neighbor under any circumstances.
    /// </summary>
    /// <remarks>
    /// The biomes have fundamentally incompatible environmental properties.
    /// Sector generation must place other realms between them.
    /// </remarks>
    Incompatible = 2
}
