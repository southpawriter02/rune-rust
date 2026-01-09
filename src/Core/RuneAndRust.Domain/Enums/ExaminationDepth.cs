namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Levels of detail when examining an object.
/// </summary>
public enum ExaminationDepth
{
    /// <summary>Quick glance, minimal detail (room entry).</summary>
    Glance,

    /// <summary>Standard look command.</summary>
    Look,

    /// <summary>Detailed examination.</summary>
    Examine
}
