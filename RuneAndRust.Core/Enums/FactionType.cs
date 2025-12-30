namespace RuneAndRust.Core.Enums;

/// <summary>
/// Canonical faction identifiers in Aethelgard.
/// Each faction has distinct culture, territory, and player interaction patterns.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
public enum FactionType
{
    /// <summary>
    /// Scavenger clans of the upper ruins.
    /// Pragmatic survivors who trade in salvage and information.
    /// Default disposition: Neutral.
    /// </summary>
    IronBanes = 0,

    /// <summary>
    /// Deep-dwelling master smiths and artificers.
    /// Secretive craftsmen who guard Pre-Glitch forging secrets.
    /// Default disposition: Neutral.
    /// </summary>
    Dvergr = 1,

    /// <summary>
    /// Cultists devoted to the Glitch.
    /// Believe the Glitch is divine transformation, not corruption.
    /// Default disposition: Hostile (starts at -25).
    /// </summary>
    TheBound = 2,

    /// <summary>
    /// Mysterious masked traders with unknown allegiances.
    /// Deal in rare artifacts and forbidden knowledge.
    /// Default disposition: Neutral.
    /// </summary>
    TheFaceless = 3
}
