namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a command/interaction unlocked through hint discovery,
/// enabling actions that weren't previously available.
/// </summary>
/// <remarks>
/// Unlocked interactions persist across the session and are checked when
/// parsing player commands. Commands that haven't been unlocked will not
/// be recognized until the appropriate hint is discovered.
/// </remarks>
/// <param name="InteractionId">Unique identifier for this interaction.</param>
/// <param name="CommandText">The command text to type (e.g., "override console").</param>
/// <param name="TargetObjectId">The object this interaction targets.</param>
/// <param name="UnlockedByHintId">The hint that unlocked this interaction.</param>
/// <param name="Description">A brief description of what this command does.</param>
public readonly record struct UnlockedInteraction(
    string InteractionId,
    string CommandText,
    string TargetObjectId,
    string UnlockedByHintId,
    string Description)
{
    /// <summary>
    /// Checks if this interaction matches the given command input.
    /// </summary>
    /// <param name="input">The player's command input.</param>
    /// <returns>True if the input matches this interaction's command.</returns>
    public bool MatchesCommand(string input) =>
        CommandText.Equals(input.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if this interaction targets the specified object.
    /// </summary>
    /// <param name="objectId">The object ID to check.</param>
    /// <returns>True if this interaction targets the object.</returns>
    public bool TargetsObject(string objectId) =>
        TargetObjectId.Equals(objectId, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a help-style display string for this interaction.
    /// </summary>
    /// <returns>A formatted string showing the command and its description.</returns>
    public string ToHelpString() => $"{CommandText} - {Description}";

    /// <summary>
    /// Gets a summary for logging.
    /// </summary>
    public override string ToString() =>
        $"UnlockedInteraction({InteractionId}: '{CommandText}' on {TargetObjectId})";
}
