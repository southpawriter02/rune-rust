namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the category of a permanent psychological trauma.
/// Traumas are acquired when a character fails a Breaking Point resolve check.
/// </summary>
public enum TraumaType
{
    /// <summary>
    /// Fear-based trauma. Triggered by specific environmental conditions.
    /// Example: Nyctophobia (fear of darkness).
    /// </summary>
    Phobia = 0,

    /// <summary>
    /// Compulsive behavior trauma. Forces specific actions in certain situations.
    /// Example: Pyromaniac (compulsion to start fires).
    /// </summary>
    Compulsion = 1,

    /// <summary>
    /// Distorted perception trauma. Affects how the character perceives reality.
    /// Example: Paranoia (distrust of allies).
    /// </summary>
    Delusion = 2,

    /// <summary>
    /// Physical manifestation of psychological trauma.
    /// Example: The Shakes (tremors affecting dexterity).
    /// </summary>
    Somatic = 3
}
