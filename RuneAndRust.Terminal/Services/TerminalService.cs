namespace RuneAndRust.Terminal.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;

/// <summary>
/// Manages terminal state for mouse mode and other VT features.
/// </summary>
/// <remarks>
/// See: SPEC-INPUT-002 for Mouse Support System design.
/// v0.3.23c: Initial implementation.
/// </remarks>
public class TerminalService : ITerminalService, IDisposable
{
    private readonly ILogger<TerminalService> _logger;
    private bool _mouseEnabled = false;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminalService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public TerminalService(ILogger<TerminalService> logger)
    {
        _logger = logger;
        _logger.LogDebug("[Terminal] TerminalService initialized (v0.3.23c)");
    }

    /// <inheritdoc />
    public bool IsMouseEnabled => _mouseEnabled;

    /// <inheritdoc />
    public void EnableMouseMode()
    {
        if (_mouseEnabled)
        {
            _logger.LogTrace("[Terminal] Mouse mode already enabled, skipping");
            return;
        }

        if (!IsMouseSupported())
        {
            _logger.LogDebug("[Terminal] Mouse not supported in this terminal, skipping enable");
            return;
        }

        // Enable SGR Extended Mouse Mode
        // Mode 1000 = Report button events
        // Mode 1006 = SGR extended coordinates (supports > 223 columns)
        Console.Write("\x1b[?1000h"); // Enable button tracking
        Console.Write("\x1b[?1006h"); // Enable SGR extended coordinates

        _mouseEnabled = true;
        _logger.LogDebug("[Terminal] Mouse mode enabled (SGR Extended)");
    }

    /// <inheritdoc />
    public void DisableMouseMode()
    {
        if (!_mouseEnabled)
        {
            _logger.LogTrace("[Terminal] Mouse mode not enabled, skipping disable");
            return;
        }

        Console.Write("\x1b[?1006l"); // Disable SGR extended
        Console.Write("\x1b[?1000l"); // Disable button tracking

        _mouseEnabled = false;
        _logger.LogDebug("[Terminal] Mouse mode disabled");
    }

    /// <inheritdoc />
    public bool IsMouseSupported()
    {
        // Windows Terminal
        var wtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        if (!string.IsNullOrEmpty(wtSession))
        {
            _logger.LogTrace("[Terminal] Windows Terminal detected, mouse supported");
            return true;
        }

        // xterm-compatible terminals
        var term = Environment.GetEnvironmentVariable("TERM") ?? "";
        if (term.Contains("xterm") || term.Contains("screen") ||
            term.Contains("tmux") || term.Contains("alacritty") ||
            term.Contains("kitty"))
        {
            _logger.LogTrace("[Terminal] xterm-compatible terminal detected ({Term}), mouse supported", term);
            return true;
        }

        // iTerm2, VS Code, macOS Terminal
        var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        if (termProgram == "iTerm.app" || termProgram == "vscode" ||
            termProgram == "Apple_Terminal")
        {
            _logger.LogTrace("[Terminal] {TermProgram} detected, mouse supported", termProgram);
            return true;
        }

        _logger.LogTrace("[Terminal] No known mouse-compatible terminal detected");
        return false;
    }

    /// <summary>
    /// Disposes the terminal service, ensuring mouse mode is disabled.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Ensure mouse mode is disabled when disposing
            if (_mouseEnabled)
            {
                _logger.LogDebug("[Terminal] Disposing, disabling mouse mode");
                DisableMouseMode();
            }
        }

        _disposed = true;
    }
}
