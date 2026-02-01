// ═══════════════════════════════════════════════════════════════════════════════
// ThresholdEffectDto.cs
// DTO representing a corruption threshold effect from JSON configuration.
// Each threshold (25, 50, 75, 100) can trigger UI warnings, faction reputation
// locks, trauma acquisition, or the Terminal Error check. Loaded from
// config/corruption-sources.json for data-driven threshold effect resolution.
// Version: 0.18.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// DTO representing an effect triggered when a corruption threshold is crossed.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from the <c>thresholdEffects</c> section of <c>config/corruption-sources.json</c>.
/// Each threshold (25, 50, 75, 100) defines a milestone in the character's corruption
/// progression with both mechanical and narrative consequences.
/// </para>
/// <para>
/// <strong>Threshold Effects:</strong>
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Threshold</term>
/// <description>Default Effect</description>
/// </listheader>
/// <item><term>25</term><description>UI warning indicator displayed (<see cref="UiWarning"/> = true)</description></item>
/// <item><term>50</term><description>Human faction reputation locked (<see cref="FactionLock"/> = true)</description></item>
/// <item><term>75</term><description>Acquire MACHINE AFFINITY trauma (<see cref="TraumaId"/> = "machine-affinity")</description></item>
/// <item><term>100</term><description>Terminal Error check triggered (<see cref="TerminalError"/> = true)</description></item>
/// </list>
/// <para>
/// <strong>Configuration Example:</strong>
/// </para>
/// <code>
/// {
///   "description": "Acquire [MACHINE AFFINITY] trauma",
///   "traumaId": "machine-affinity"
/// }
/// </code>
/// </remarks>
/// <seealso cref="CorruptionConfigurationDto"/>
public sealed record ThresholdEffectDto
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the narrative description shown to the player when this threshold is crossed.
    /// </summary>
    /// <remarks>
    /// Displayed as a notification or dialog when the character's corruption reaches
    /// this threshold for the first time (e.g., "You feel the Blight's touch...",
    /// "Terminal Error - Character transformation").
    /// </remarks>
    /// <value>A narrative string describing the threshold effect.</value>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets whether to display a persistent UI warning indicator.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, a visual warning is displayed on the character sheet and HUD
    /// to alert the player that corruption is becoming significant. Used at the 25%
    /// threshold as an early warning system.
    /// </remarks>
    /// <value><c>true</c> to display UI warning; <c>false</c> otherwise.</value>
    [JsonPropertyName("uiWarning")]
    public bool UiWarning { get; init; }

    /// <summary>
    /// Gets whether to lock certain faction reputation gains.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, human faction reputation can no longer increase. The character
    /// is too corrupted for normal human society to accept. Used at the 50% threshold.
    /// </remarks>
    /// <value><c>true</c> to lock faction reputation; <c>false</c> otherwise.</value>
    [JsonPropertyName("factionLock")]
    public bool FactionLock { get; init; }

    /// <summary>
    /// Gets the ID of a permanent trauma to acquire at this threshold.
    /// </summary>
    /// <remarks>
    /// References a <c>TraumaDefinition</c> from v0.18.3. When not <c>null</c>, the character
    /// automatically acquires the specified permanent trauma upon crossing this threshold.
    /// Used at the 75% threshold for "machine-affinity" trauma.
    /// </remarks>
    /// <value>A kebab-case trauma ID string, or <c>null</c> if no trauma is triggered.</value>
    [JsonPropertyName("traumaId")]
    public string? TraumaId { get; init; }

    /// <summary>
    /// Gets whether this threshold triggers the Terminal Error check.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, the character must pass a WILL-based Terminal Error survival check.
    /// Failure means the character becomes Forlorn and is lost. Used at the 100% threshold.
    /// The Terminal Error DC is defined by <c>CorruptionService.TerminalErrorDc</c> (default: 3).
    /// </remarks>
    /// <value><c>true</c> to trigger Terminal Error; <c>false</c> otherwise.</value>
    [JsonPropertyName("terminalError")]
    public bool TerminalError { get; init; }
}
