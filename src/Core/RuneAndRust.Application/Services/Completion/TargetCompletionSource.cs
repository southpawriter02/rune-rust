using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Application.Services.Completion;

/// <summary>
/// Provides target name completions for combat/interaction commands.
/// </summary>
public class TargetCompletionSource : ICompletionSource
{
    private static readonly HashSet<string> TargetCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "attack", "examine", "look", "talk", "interact", "target", "inspect"
    };
    
    /// <inheritdoc/>
    public string Category => "targets";
    
    /// <inheritdoc/>
    public int Priority => 90;
    
    /// <inheritdoc/>
    public bool AppliesTo(CompletionContext context)
    {
        return context.WordIndex >= 1 && 
               context.CommandName != null &&
               TargetCommands.Contains(context.CommandName);
    }
    
    /// <inheritdoc/>
    public IEnumerable<string> GetMatches(string prefix, CompletionContext context)
    {
        if (context.AvailableTargets == null)
            return Enumerable.Empty<string>();
        
        return context.AvailableTargets
            .Where(t => t.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t);
    }
}
