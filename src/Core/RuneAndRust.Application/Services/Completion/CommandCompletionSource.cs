using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Application.Services.Completion;

/// <summary>
/// Provides command name completions.
/// </summary>
public class CommandCompletionSource : ICompletionSource
{
    private readonly IReadOnlyList<string> _commands;
    
    /// <inheritdoc/>
    public string Category => "commands";
    
    /// <inheritdoc/>
    public int Priority => 100;  // Highest for first word
    
    /// <summary>
    /// Initializes a new instance with available commands.
    /// </summary>
    public CommandCompletionSource(IEnumerable<string> availableCommands)
    {
        _commands = availableCommands.ToList();
    }
    
    /// <inheritdoc/>
    public bool AppliesTo(CompletionContext context)
    {
        // Only complete commands at word index 0
        return context.WordIndex == 0;
    }
    
    /// <inheritdoc/>
    public IEnumerable<string> GetMatches(string prefix, CompletionContext context)
    {
        return _commands
            .Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c);
    }
}
