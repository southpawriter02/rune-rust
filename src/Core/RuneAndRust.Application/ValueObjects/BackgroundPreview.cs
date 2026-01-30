// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundPreview.cs
// Value object providing a preview of a background for character creation UI,
// including pre-formatted strings suitable for direct display.
// Version: 0.17.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Provides a preview of a background for the character creation UI.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BackgroundPreview"/> contains pre-formatted strings suitable for direct
/// display in the character creation interface. It aggregates information from
/// <see cref="RuneAndRust.Domain.Entities.BackgroundDefinition"/> into a UI-ready format,
/// avoiding the need for presentation logic in the view layer.
/// </para>
/// <para>
/// <strong>Usage:</strong> Call <see cref="RuneAndRust.Application.Interfaces.IBackgroundApplicationService.GetBackgroundPreview"/>
/// to generate a preview for a specific background, or
/// <see cref="RuneAndRust.Application.Interfaces.IBackgroundApplicationService.GetAllBackgroundPreviews"/>
/// to generate previews for all available backgrounds.
/// </para>
/// <para>
/// <strong>Not-Found Case:</strong> Use <see cref="Empty"/> to create a placeholder preview
/// when a background cannot be found. This prevents null propagation in the UI layer.
/// </para>
/// </remarks>
/// <seealso cref="BackgroundApplicationResult"/>
public readonly record struct BackgroundPreview
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the background enum value.
    /// </summary>
    /// <value>
    /// The <see cref="Background"/> enum value identifying this background.
    /// </value>
    public Background BackgroundId { get; init; }

    /// <summary>
    /// Gets the display name for the background (e.g., "Village Smith").
    /// </summary>
    /// <value>
    /// A human-readable name suitable for display in selection lists and headers.
    /// </value>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the full description of the background.
    /// </summary>
    /// <value>
    /// A brief narrative description of what this background represents
    /// (e.g., "You worked the forge, shaping metal into tools of war and peace").
    /// </value>
    public string Description { get; init; }

    /// <summary>
    /// Gets the selection text for the creation UI.
    /// </summary>
    /// <value>
    /// An evocative second-person narrative passage displayed when the player
    /// considers this background during character creation. Written to help
    /// players connect with the character's backstory.
    /// </value>
    public string SelectionText { get; init; }

    /// <summary>
    /// Gets the character's profession before the Great Silence.
    /// </summary>
    /// <value>
    /// A short description of what the character did pre-Silence
    /// (e.g., "Blacksmith and metalworker").
    /// </value>
    public string ProfessionBefore { get; init; }

    /// <summary>
    /// Gets how society views this background.
    /// </summary>
    /// <value>
    /// A description of the character's social standing based on their profession
    /// (e.g., "Respected craftsperson, essential to any settlement").
    /// </value>
    public string SocialStanding { get; init; }

    /// <summary>
    /// Gets a formatted summary of skill grants (e.g., "craft +2, might +1").
    /// </summary>
    /// <value>
    /// A pre-formatted string listing all skill bonuses from this background,
    /// suitable for direct display in the selection panel.
    /// </value>
    public string SkillSummary { get; init; }

    /// <summary>
    /// Gets a formatted summary of equipment grants.
    /// </summary>
    /// <value>
    /// A pre-formatted string listing all starting equipment from this background,
    /// suitable for direct display in the selection panel.
    /// </value>
    public string EquipmentSummary { get; init; }

    /// <summary>
    /// Gets the number of narrative hooks available for this background.
    /// </summary>
    /// <value>
    /// The count of narrative hooks (typically 3), which influence dialogue
    /// options, quest availability, and NPC interactions.
    /// </value>
    public int NarrativeHookCount { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an empty preview for when a background is not found.
    /// </summary>
    /// <param name="backgroundId">The background that was not found.</param>
    /// <returns>
    /// A <see cref="BackgroundPreview"/> with default placeholder values,
    /// indicating the background could not be located in the provider.
    /// </returns>
    /// <remarks>
    /// This factory method prevents null propagation in the UI layer by returning
    /// a valid struct with sensible defaults. The display name is set to "Unknown"
    /// and the description to "Background not found".
    /// </remarks>
    /// <example>
    /// <code>
    /// var preview = BackgroundPreview.Empty(Background.VillageSmith);
    /// // preview.DisplayName == "Unknown"
    /// // preview.SkillSummary == "None"
    /// </code>
    /// </example>
    public static BackgroundPreview Empty(Background backgroundId) =>
        new()
        {
            BackgroundId = backgroundId,
            DisplayName = "Unknown",
            Description = "Background not found",
            SelectionText = string.Empty,
            ProfessionBefore = string.Empty,
            SocialStanding = string.Empty,
            SkillSummary = "None",
            EquipmentSummary = "None",
            NarrativeHookCount = 0
        };
}
