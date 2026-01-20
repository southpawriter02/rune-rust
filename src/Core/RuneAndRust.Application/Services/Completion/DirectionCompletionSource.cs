using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Application.Services.Completion;

/// <summary>
/// Provides direction completions for movement commands.
/// </summary>
public class DirectionCompletionSource : ICompletionSource
{
    private static readonly string[] AllDirections = 
        { "north", "south", "east", "west", "up", "down", "n", "s", "e", "w", "u", "d" };
    
    private static readonly HashSet<string> MovementCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "go", "move", "walk", "run"
    };
    
    /// <inheritdoc/>
    public string Category => "directions";
    
    /// <inheritdoc/>
    public int Priority => 90;
    
    /// <inheritdoc/>
    public bool AppliesTo(CompletionContext context)
    {
        return context.WordIndex >= 1 && 
               context.CommandName != null &&
               MovementCommands.Contains(context.CommandName);
    }
    
    /// <inheritdoc/>
    public IEnumerable<string> GetMatches(string prefix, CompletionContext context)
    {
        // Prefer available exits, fall back to all directions
        var directions = context.AvailableExits?.ToArray() ?? AllDirections;
        
        return directions
            .Where(d => d.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(d => d);
    }
}
