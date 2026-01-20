namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Context information for tab completion.
/// </summary>
/// <param name="FullInput">The complete input text.</param>
/// <param name="CurrentWord">The word being completed.</param>
/// <param name="WordIndex">Index of current word (0 = command, 1+ = args).</param>
/// <param name="CommandName">The command name if known.</param>
/// <param name="AvailableTargets">Valid targets in current context.</param>
/// <param name="InventoryItems">Items the player has.</param>
/// <param name="AvailableExits">Available directions from current room.</param>
public record CompletionContext(
    string FullInput,
    string CurrentWord,
    int WordIndex,
    string? CommandName,
    IReadOnlyList<string>? AvailableTargets = null,
    IReadOnlyList<string>? InventoryItems = null,
    IReadOnlyList<string>? AvailableExits = null)
{
    /// <summary>
    /// Creates a context from an input string.
    /// </summary>
    public static CompletionContext FromInput(
        string input,
        IReadOnlyList<string>? targets = null,
        IReadOnlyList<string>? items = null,
        IReadOnlyList<string>? exits = null)
    {
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentWord = words.Length > 0 ? words[^1] : "";
        var commandName = words.Length > 0 ? words[0] : null;
        
        // If input ends with space, we're starting a new word
        var wordIndex = input.EndsWith(' ') ? words.Length : Math.Max(0, words.Length - 1);
        if (input.EndsWith(' ')) currentWord = "";
        
        return new CompletionContext(
            input,
            currentWord,
            wordIndex,
            wordIndex > 0 ? commandName : null,
            targets,
            items,
            exits);
    }
}
