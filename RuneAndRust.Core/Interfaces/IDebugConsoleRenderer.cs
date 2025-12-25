namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Interface for the debug console renderer (v0.3.17a).
/// Allows CommandParser to trigger the console without depending on Terminal layer.
/// </summary>
/// <remarks>See: SPEC-DEBUG-001 for Debug Console System design.</remarks>
public interface IDebugConsoleRenderer
{
    /// <summary>
    /// Runs the debug console in modal mode.
    /// Blocks until user exits with ~ or Escape.
    /// </summary>
    void Run();
}
