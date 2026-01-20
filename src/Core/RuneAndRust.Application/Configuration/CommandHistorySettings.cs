namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Settings for command history behavior.
/// </summary>
public record CommandHistorySettings
{
    /// <summary>
    /// Gets or sets whether command history is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
    
    /// <summary>
    /// Gets or sets the maximum number of history entries.
    /// </summary>
    public int MaxEntries { get; init; } = 100;
    
    /// <summary>
    /// Gets or sets whether to exclude empty commands.
    /// </summary>
    public bool ExcludeEmpty { get; init; } = true;
    
    /// <summary>
    /// Gets or sets whether to move duplicates to front instead of ignoring.
    /// </summary>
    public bool MoveDuplicatesToFront { get; init; } = true;
}
