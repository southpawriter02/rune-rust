using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Presentation.Input;

/// <summary>
/// Handles user input with command history navigation support.
/// </summary>
/// <remarks>
/// Features:
/// - Up/Down arrow history navigation
/// - Left/Right cursor movement
/// - Home/End support
/// - Backspace/Delete editing
/// - Current input preservation during navigation
/// </remarks>
public class InputHandler
{
    private readonly ICommandHistoryService _historyService;
    private readonly ITerminalService _terminal;
    private readonly ILogger<InputHandler>? _logger;
    
    private string _currentInput = "";
    private int _cursorPosition;
    
    private const string Prompt = "> ";
    
    /// <summary>
    /// Initializes a new instance of <see cref="InputHandler"/>.
    /// </summary>
    public InputHandler(
        ICommandHistoryService historyService,
        ITerminalService terminal,
        ILogger<InputHandler>? logger = null)
    {
        _historyService = historyService;
        _terminal = terminal;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets the current input text.
    /// </summary>
    public string CurrentInput => _currentInput;
    
    /// <summary>
    /// Gets the cursor position within the input.
    /// </summary>
    public int CursorPosition => _cursorPosition;
    
    /// <summary>
    /// Handles a key press.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <returns>The completed command if Enter was pressed, null otherwise.</returns>
    public string? HandleKey(ConsoleKeyInfo key)
    {
        return key.Key switch
        {
            ConsoleKey.Enter => HandleEnter(),
            ConsoleKey.UpArrow => HandleUpArrow(),
            ConsoleKey.DownArrow => HandleDownArrow(),
            ConsoleKey.Backspace => HandleBackspace(),
            ConsoleKey.Delete => HandleDelete(),
            ConsoleKey.LeftArrow => HandleLeftArrow(),
            ConsoleKey.RightArrow => HandleRightArrow(),
            ConsoleKey.Home => HandleHome(),
            ConsoleKey.End => HandleEnd(),
            ConsoleKey.Escape => HandleEscape(),
            _ => HandleCharacter(key.KeyChar)
        };
    }
    
    /// <summary>
    /// Resets the input state.
    /// </summary>
    public void Reset()
    {
        _currentInput = "";
        _cursorPosition = 0;
        _historyService.ResetNavigation();
    }
    
    private string? HandleEnter()
    {
        var command = _currentInput.Trim();
        
        if (!string.IsNullOrEmpty(command))
        {
            _historyService.Add(command);
        }
        
        _historyService.ResetNavigation();
        _logger?.LogDebug("Command entered: '{Command}'", command);
        
        var result = _currentInput;
        Reset();
        return result;
    }
    
    private string? HandleUpArrow()
    {
        // Save current input before first navigation
        if (!_historyService.IsNavigating)
        {
            _historyService.SaveCurrentInput(_currentInput);
        }
        
        var previous = _historyService.GetPrevious();
        if (previous != null)
        {
            SetInput(previous);
            _logger?.LogDebug("Recalled from history: '{Command}'", previous);
        }
        
        return null;
    }
    
    private string? HandleDownArrow()
    {
        var next = _historyService.GetNext();
        if (next != null)
        {
            SetInput(next);
            _logger?.LogDebug("Navigated forward to: '{Command}'", next);
        }
        
        return null;
    }
    
    private void SetInput(string text)
    {
        _currentInput = text;
        _cursorPosition = text.Length;
    }
    
    private string? HandleCharacter(char c)
    {
        if (char.IsControl(c)) return null;
        
        // If navigating history, any character input resets navigation
        if (_historyService.IsNavigating)
        {
            _historyService.ResetNavigation();
        }
        
        if (_cursorPosition < _currentInput.Length)
        {
            _currentInput = _currentInput.Insert(_cursorPosition, c.ToString());
        }
        else
        {
            _currentInput += c;
        }
        
        _cursorPosition++;
        return null;
    }
    
    private string? HandleBackspace()
    {
        if (_cursorPosition > 0)
        {
            _currentInput = _currentInput.Remove(_cursorPosition - 1, 1);
            _cursorPosition--;
        }
        return null;
    }
    
    private string? HandleDelete()
    {
        if (_cursorPosition < _currentInput.Length)
        {
            _currentInput = _currentInput.Remove(_cursorPosition, 1);
        }
        return null;
    }
    
    private string? HandleLeftArrow()
    {
        if (_cursorPosition > 0)
        {
            _cursorPosition--;
        }
        return null;
    }
    
    private string? HandleRightArrow()
    {
        if (_cursorPosition < _currentInput.Length)
        {
            _cursorPosition++;
        }
        return null;
    }
    
    private string? HandleHome()
    {
        _cursorPosition = 0;
        return null;
    }
    
    private string? HandleEnd()
    {
        _cursorPosition = _currentInput.Length;
        return null;
    }
    
    private string? HandleEscape()
    {
        _historyService.ResetNavigation();
        Reset();
        return null;
    }
}
