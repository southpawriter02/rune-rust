using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Provides tab completion using registered sources.
/// </summary>
/// <remarks>
/// Features:
/// - Pluggable completion sources with priorities
/// - Context-aware completion based on command type
/// - Case-insensitive matching by default
/// - Configurable max suggestions
/// </remarks>
public class TabCompletionService : ITabCompletionService
{
    private readonly List<ICompletionSource> _sources = new();
    private readonly TabCompletionSettings _settings;
    private readonly ILogger<TabCompletionService>? _logger;
    
    /// <inheritdoc/>
    public IReadOnlyList<ICompletionSource> Sources => _sources.AsReadOnly();
    
    /// <summary>
    /// Initializes a new instance with settings.
    /// </summary>
    public TabCompletionService(
        IOptions<TabCompletionSettings> settings,
        ILogger<TabCompletionService>? logger = null)
    {
        _settings = settings.Value;
        _logger = logger;
    }
    
    /// <summary>
    /// Initializes a new instance with default settings.
    /// </summary>
    public TabCompletionService(ILogger<TabCompletionService>? logger = null)
    {
        _settings = new TabCompletionSettings();
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public IReadOnlyList<string> GetCompletions(string partialInput, CompletionContext context)
    {
        if (!_settings.Enabled) return Array.Empty<string>();
        
        var matches = new List<string>();
        var comparison = _settings.CaseSensitive 
            ? StringComparison.Ordinal 
            : StringComparison.OrdinalIgnoreCase;
        
        // Query sources in priority order
        foreach (var source in _sources.OrderByDescending(s => s.Priority))
        {
            if (source.AppliesTo(context))
            {
                var sourceMatches = source.GetMatches(context.CurrentWord, context)
                    .Where(m => m.StartsWith(context.CurrentWord, comparison))
                    .ToList();
                
                matches.AddRange(sourceMatches);
                
                _logger?.LogDebug("Source '{Category}' provided {Count} matches", 
                    source.Category, sourceMatches.Count);
            }
        }
        
        // Limit and sort
        var result = matches
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => m)
            .Take(_settings.MaxSuggestions)
            .ToList();
        
        _logger?.LogDebug("Tab completion for '{Input}': {Count} matches found", 
            context.CurrentWord, result.Count);
        
        return result;
    }
    
    /// <inheritdoc/>
    public string? GetBestCompletion(string partialInput, CompletionContext context)
    {
        var matches = GetCompletions(partialInput, context);
        return matches.Count == 1 ? matches[0] : null;
    }
    
    /// <inheritdoc/>
    public string Complete(string partialInput, CompletionContext context)
    {
        var best = GetBestCompletion(partialInput, context);
        if (best == null) return partialInput;
        
        // Replace the current word with the completion
        var words = partialInput.Split(' ').ToList();
        if (words.Count > 0 && context.WordIndex < words.Count)
        {
            words[context.WordIndex] = best;
        }
        else
        {
            words.Add(best);
        }
        
        return string.Join(" ", words);
    }
    
    /// <inheritdoc/>
    public void RegisterSource(ICompletionSource source)
    {
        _sources.Add(source);
        _logger?.LogDebug("Registered completion source: {Category} (Priority: {Priority})", 
            source.Category, source.Priority);
    }
    
    /// <inheritdoc/>
    public void UnregisterSource(ICompletionSource source)
    {
        _sources.Remove(source);
        _logger?.LogDebug("Unregistered completion source: {Category}", source.Category);
    }
}
