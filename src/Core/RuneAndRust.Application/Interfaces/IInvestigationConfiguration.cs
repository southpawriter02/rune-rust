namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Configuration settings for investigation operations.
/// </summary>
public interface IInvestigationConfiguration
{
    /// <summary>
    /// Time in minutes to investigate a crime scene.
    /// </summary>
    /// <remarks>Default: 15 minutes.</remarks>
    int CrimeSceneTime { get; }

    /// <summary>
    /// Time in minutes to investigate remains.
    /// </summary>
    /// <remarks>Default: 10 minutes.</remarks>
    int RemainsTime { get; }

    /// <summary>
    /// Time in minutes to investigate wreckage.
    /// </summary>
    /// <remarks>Default: 15 minutes.</remarks>
    int WreckageTime { get; }

    /// <summary>
    /// Time in minutes to investigate a trail.
    /// </summary>
    /// <remarks>Default: 5 minutes.</remarks>
    int TrailTime { get; }

    /// <summary>
    /// Time in minutes to investigate a document.
    /// </summary>
    /// <remarks>Default: 5 minutes.</remarks>
    int DocumentTime { get; }

    /// <summary>
    /// Default time in minutes for unspecified investigation types.
    /// </summary>
    /// <remarks>Default: 10 minutes.</remarks>
    int DefaultTime { get; }

    /// <summary>
    /// Number of net successes required for expert-level success.
    /// Expert success reveals hidden connections.
    /// </summary>
    /// <remarks>Default: 5.</remarks>
    int ExpertSuccessThreshold { get; }

    /// <summary>
    /// Whether characters can re-investigate a target for additional clues.
    /// </summary>
    bool AllowReinvestigation { get; }

    /// <summary>
    /// Cooldown in minutes before a target can be reinvestigated.
    /// </summary>
    int ReinvestigationCooldown { get; }
}
