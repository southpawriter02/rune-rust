using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Application.Services.Completion;

/// <summary>
/// Provides inventory item completions.
/// </summary>
public class ItemCompletionSource : ICompletionSource
{
    private static readonly HashSet<string> ItemCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "use", "equip", "unequip", "drop", "examine", "inspect"
    };
    
    /// <inheritdoc/>
    public string Category => "items";
    
    /// <inheritdoc/>
    public int Priority => 90;
    
    /// <inheritdoc/>
    public bool AppliesTo(CompletionContext context)
    {
        return context.WordIndex >= 1 && 
               context.CommandName != null &&
               ItemCommands.Contains(context.CommandName);
    }
    
    /// <inheritdoc/>
    public IEnumerable<string> GetMatches(string prefix, CompletionContext context)
    {
        if (context.InventoryItems == null)
            return Enumerable.Empty<string>();
        
        return context.InventoryItems
            .Where(i => i.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i);
    }
}
