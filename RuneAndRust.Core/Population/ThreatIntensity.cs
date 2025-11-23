namespace RuneAndRust.Core.Population;

/// <summary>
/// v0.39.3: Threat intensity classification for heatmap visualization
/// Maps threat count to intensity level for debugging and balance analysis
/// </summary>
public enum ThreatIntensity
{
    /// <summary>
    /// No threats - 0 threats
    /// Safe room for exploration and rest
    /// </summary>
    None,

    /// <summary>
    /// Low intensity - 1-2 threats
    /// Minor encounters, safe passage
    /// </summary>
    Low,

    /// <summary>
    /// Medium intensity - 3-4 threats
    /// Standard combat encounters
    /// </summary>
    Medium,

    /// <summary>
    /// High intensity - 5-7 threats
    /// Challenging battles requiring preparation
    /// </summary>
    High,

    /// <summary>
    /// Extreme intensity - 8+ threats
    /// Boss battles and climactic encounters
    /// </summary>
    Extreme
}
