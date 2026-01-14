using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Infrastructure.Services;

/// <summary>
/// Console-based terminal service implementation.
/// </summary>
/// <remarks>
/// Wraps System.Console to provide:
/// <list type="bullet">
/// <item><description>Terminal size detection</description></item>
/// <item><description>Cursor position control</description></item>
/// <item><description>Resize event polling (since Console lacks native resize events)</description></item>
/// <item><description>Color and Unicode capability detection</description></item>
/// </list>
/// </remarks>
public class TerminalService : ITerminalService, IDisposable
{
    private readonly ILogger<TerminalService> _logger;
    private readonly Timer? _resizePoller;
    private (int Width, int Height) _lastSize;
    private bool _disposed;
    
    private const int ResizePollIntervalMs = 250;
    
    /// <inheritdoc/>
    public event Action<(int Width, int Height)>? OnResize;
    
    /// <inheritdoc/>
    public bool SupportsColor { get; }
    
    /// <inheritdoc/>
    public bool SupportsUnicode { get; }
    
    /// <summary>
    /// Initializes a new instance of <see cref="TerminalService"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    public TerminalService(ILogger<TerminalService> logger)
    {
        _logger = logger;
        _lastSize = GetSize();
        
        // Detect capabilities
        SupportsColor = DetectColorSupport();
        SupportsUnicode = DetectUnicodeSupport();
        
        // Start resize polling (Console doesn't have native resize events)
        _resizePoller = new Timer(CheckForResize, null, 
            ResizePollIntervalMs, ResizePollIntervalMs);
        
        _logger.LogDebug("TerminalService initialized: {Width}x{Height}, Color={Color}, Unicode={Unicode}",
            _lastSize.Width, _lastSize.Height, SupportsColor, SupportsUnicode);
    }
    
    /// <inheritdoc/>
    public (int Width, int Height) GetSize()
    {
        try
        {
            return (Console.WindowWidth, Console.WindowHeight);
        }
        catch (IOException)
        {
            // Fallback for redirected/piped console
            _logger.LogDebug("Console output redirected, using fallback size 80x24");
            return (80, 24);
        }
    }
    
    /// <inheritdoc/>
    public void SetCursorPosition(int x, int y)
    {
        try
        {
            var size = GetSize();
            // Clamp to valid range
            var safeX = Math.Clamp(x, 0, size.Width - 1);
            var safeY = Math.Clamp(y, 0, size.Height - 1);
            Console.SetCursorPosition(safeX, safeY);
        }
        catch (ArgumentOutOfRangeException)
        {
            _logger.LogWarning("Invalid cursor position: ({X}, {Y})", x, y);
        }
    }
    
    /// <inheritdoc/>
    public void Write(string text)
    {
        Console.Write(text);
    }
    
    /// <inheritdoc/>
    public void WriteLine(string text)
    {
        Console.WriteLine(text);
    }
    
    /// <inheritdoc/>
    public void Clear()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Ignore if output is redirected
            _logger.LogDebug("Cannot clear redirected console output");
        }
    }
    
    /// <inheritdoc/>
    public void ClearRegion(int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;
            
        var blank = new string(' ', width);
        for (var row = 0; row < height; row++)
        {
            SetCursorPosition(x, y + row);
            Write(blank);
        }
    }
    
    /// <summary>
    /// Polls for terminal resize events.
    /// </summary>
    private void CheckForResize(object? state)
    {
        if (_disposed) return;
        
        var currentSize = GetSize();
        if (currentSize != _lastSize)
        {
            _logger.LogDebug("Terminal resized: {OldWidth}x{OldHeight} -> {NewWidth}x{NewHeight}",
                _lastSize.Width, _lastSize.Height, currentSize.Width, currentSize.Height);
            
            _lastSize = currentSize;
            OnResize?.Invoke(currentSize);
        }
    }
    
    /// <summary>
    /// Detects if the terminal supports color output.
    /// </summary>
    private static bool DetectColorSupport()
    {
        // Check environment variables for color support
        var term = Environment.GetEnvironmentVariable("TERM") ?? "";
        var colorTerm = Environment.GetEnvironmentVariable("COLORTERM") ?? "";
        
        // Most modern terminals support color
        return !Console.IsOutputRedirected && 
               (term.Contains("color", StringComparison.OrdinalIgnoreCase) || 
                term.Contains("xterm", StringComparison.OrdinalIgnoreCase) || 
                term.Contains("256", StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrEmpty(colorTerm) ||
                OperatingSystem.IsWindows()); // Windows Terminal supports color
    }
    
    /// <summary>
    /// Detects if the terminal supports Unicode output.
    /// </summary>
    private static bool DetectUnicodeSupport()
    {
        try
        {
            var encodingName = Console.OutputEncoding.EncodingName;
            return encodingName.Contains("Unicode", StringComparison.OrdinalIgnoreCase) ||
                   encodingName.Contains("UTF", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
    
    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _resizePoller?.Dispose();
        GC.SuppressFinalize(this);
    }
}
