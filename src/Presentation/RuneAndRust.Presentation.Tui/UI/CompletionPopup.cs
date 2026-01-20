using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Displays completion suggestions as a popup.
/// </summary>
/// <remarks>
/// Features:
/// - Shows multiple suggestions for selection
/// - Keyboard navigation (up/down, tab)
/// - Highlight selected item
/// - Auto-hide on selection or escape
/// </remarks>
public class CompletionPopup
{
    private readonly ITerminalService _terminal;
    private readonly ILogger<CompletionPopup>? _logger;
    
    private int _selectedIndex;
    private IReadOnlyList<string> _suggestions = Array.Empty<string>();
    private (int X, int Y) _position;
    private int _width;
    
    /// <summary>
    /// Gets whether the popup is visible.
    /// </summary>
    public bool IsVisible { get; private set; }
    
    /// <summary>
    /// Gets the currently selected index.
    /// </summary>
    public int SelectedIndex => _selectedIndex;
    
    /// <summary>
    /// Gets the current suggestion count.
    /// </summary>
    public int SuggestionCount => _suggestions.Count;
    
    /// <summary>
    /// Initializes a new instance of <see cref="CompletionPopup"/>.
    /// </summary>
    public CompletionPopup(
        ITerminalService terminal,
        ILogger<CompletionPopup>? logger = null)
    {
        _terminal = terminal;
        _logger = logger;
    }
    
    /// <summary>
    /// Shows the popup with suggestions.
    /// </summary>
    /// <param name="suggestions">List of completion suggestions.</param>
    /// <param name="inputX">X position of input cursor.</param>
    /// <param name="inputY">Y position of input line.</param>
    public void Show(IReadOnlyList<string> suggestions, int inputX, int inputY)
    {
        if (suggestions.Count <= 1)
        {
            return;
        }
        
        _suggestions = suggestions;
        _selectedIndex = 0;
        _position = (inputX, inputY + 1);
        _width = Math.Max(20, suggestions.Max(s => s.Length) + 4);
        
        IsVisible = true;
        Render();
        
        _logger?.LogDebug("Completion popup shown with {Count} suggestions", 
            suggestions.Count);
    }
    
    /// <summary>
    /// Hides the popup.
    /// </summary>
    public void Hide()
    {
        if (!IsVisible) return;
        
        Clear();
        IsVisible = false;
        _suggestions = Array.Empty<string>();
        
        _logger?.LogDebug("Completion popup hidden");
    }
    
    /// <summary>
    /// Moves selection to previous item.
    /// </summary>
    public void SelectPrevious()
    {
        if (_suggestions.Count == 0) return;
        
        _selectedIndex = (_selectedIndex - 1 + _suggestions.Count) % _suggestions.Count;
        Render();
    }
    
    /// <summary>
    /// Moves selection to next item.
    /// </summary>
    public void SelectNext()
    {
        if (_suggestions.Count == 0) return;
        
        _selectedIndex = (_selectedIndex + 1) % _suggestions.Count;
        Render();
    }
    
    /// <summary>
    /// Gets the currently selected suggestion.
    /// </summary>
    /// <returns>Selected suggestion, or null if none.</returns>
    public string? GetSelected()
    {
        if (_suggestions.Count == 0 || _selectedIndex < 0 || _selectedIndex >= _suggestions.Count)
            return null;
        
        return _suggestions[_selectedIndex];
    }
    
    private void Render()
    {
        var maxDisplay = Math.Min(10, _suggestions.Count);
        var startIndex = Math.Max(0, _selectedIndex - maxDisplay / 2);
        startIndex = Math.Min(startIndex, _suggestions.Count - maxDisplay);
        
        for (var i = 0; i < maxDisplay; i++)
        {
            var suggestionIndex = startIndex + i;
            var suggestion = _suggestions[suggestionIndex];
            var isSelected = suggestionIndex == _selectedIndex;
            
            _terminal.SetCursorPosition(_position.X, _position.Y + i);
            
            var prevFg = Console.ForegroundColor;
            var prevBg = Console.BackgroundColor;
            
            if (isSelected)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Cyan;
            }
            
            var text = suggestion.PadRight(_width - 4);
            _terminal.Write($"│ {text} │");
            
            Console.ForegroundColor = prevFg;
            Console.BackgroundColor = prevBg;
        }
    }
    
    private void Clear()
    {
        var maxDisplay = Math.Min(10, _suggestions.Count);
        var blank = new string(' ', _width);
        
        for (var i = 0; i < maxDisplay; i++)
        {
            _terminal.SetCursorPosition(_position.X, _position.Y + i);
            _terminal.Write(blank);
        }
    }
}
