using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a permanent psychological trauma acquired at a Breaking Point.
/// Traumas alter gameplay through passive effects or triggered behaviors.
/// </summary>
public class Trauma
{
    /// <summary>
    /// Unique identifier for the trauma instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The trauma definition ID from the registry (e.g., "TRM_NYCTO").
    /// Used to look up the trauma's effects and behaviors.
    /// </summary>
    public string DefinitionId { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the trauma.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Narrative description of the trauma's effects.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The category of trauma (Phobia, Compulsion, Delusion, Somatic).
    /// </summary>
    public TraumaType Type { get; set; } = TraumaType.Phobia;

    /// <summary>
    /// Timestamp when the trauma was acquired.
    /// </summary>
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The source event that caused this trauma (e.g., "Breaking Point in Abandoned Shrine").
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Whether this trauma is currently active (some traumas can be suppressed temporarily).
    /// </summary>
    public bool IsActive { get; set; } = true;
}
