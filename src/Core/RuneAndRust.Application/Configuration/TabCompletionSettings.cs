namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Settings for tab completion behavior.
/// </summary>
public record TabCompletionSettings
{
    /// <summary>
    /// Gets or sets whether tab completion is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
    
    /// <summary>
    /// Gets or sets whether matching is case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; init; } = false;
    
    /// <summary>
    /// Gets or sets the maximum number of suggestions to show.
    /// </summary>
    public int MaxSuggestions { get; init; } = 10;
    
    /// <summary>
    /// Gets or sets whether to show the completion popup.
    /// </summary>
    public bool ShowPopup { get; init; } = true;
}
