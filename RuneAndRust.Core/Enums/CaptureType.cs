namespace RuneAndRust.Core.Enums;

/// <summary>
/// Classifies how a data fragment was captured or recorded.
/// Affects sorting in the Codex UI and potential Legend rewards.
/// </summary>
/// <remarks>
/// Data Captures are lore fragments that compile into Codex Entries.
/// Unlike inventory items, they have no weight burden and exist
/// in a separate progression system (the Scavenger's Journal).
/// </remarks>
public enum CaptureType
{
    /// <summary>
    /// Readable notes, slates, torn pages, or written records.
    /// The most common capture type found on bodies and in containers.
    /// </summary>
    TextFragment = 0,

    /// <summary>
    /// Audio logs or recordings (transcribed for display).
    /// Often found on Pre-Glitch devices or Aesir artifacts.
    /// </summary>
    EchoRecording = 1,

    /// <summary>
    /// Schematics, diagrams, images, or other visual records.
    /// Provides technical or structural information.
    /// </summary>
    VisualRecord = 2,

    /// <summary>
    /// Biological samples, tissue analysis, or specimen data.
    /// Typically gathered from creatures or Blight-affected entities.
    /// </summary>
    Specimen = 3,

    /// <summary>
    /// Dialogue fragments, rumors, or oral traditions.
    /// Gathered through conversation or eavesdropping.
    /// </summary>
    OralHistory = 4,

    /// <summary>
    /// Magical or technological trace analysis.
    /// Decoded from runic inscriptions or residual energy patterns.
    /// </summary>
    RunicTrace = 5
}
