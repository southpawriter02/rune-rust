using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Tests.Infrastructure;

/// <summary>
/// A test implementation of IInputHandler that provides scripted input from a queue.
/// Used for E2E integration testing to simulate user input sequences.
/// </summary>
public class ScriptedInputHandler : IInputHandler
{
    private readonly Queue<string> _commandQueue;
    private readonly ILogger<ScriptedInputHandler> _logger;

    /// <summary>
    /// Captures all messages displayed during test execution.
    /// </summary>
    public List<string> OutputBuffer { get; } = new();

    /// <summary>
    /// Captures all error messages displayed during test execution.
    /// </summary>
    public List<string> ErrorBuffer { get; } = new();

    /// <summary>
    /// Captures all prompts that were shown to request input.
    /// </summary>
    public List<string> PromptBuffer { get; } = new();

    /// <summary>
    /// Gets the number of commands remaining in the queue.
    /// </summary>
    public int RemainingCommands => _commandQueue.Count;

    /// <summary>
    /// Indicates whether the script has been exhausted (no more commands).
    /// </summary>
    public bool IsScriptExhausted => _commandQueue.Count == 0;

    /// <summary>
    /// Initializes a new ScriptedInputHandler with a sequence of commands.
    /// </summary>
    /// <param name="commands">The commands to execute in order.</param>
    /// <param name="logger">Logger for traceability.</param>
    public ScriptedInputHandler(IEnumerable<string> commands, ILogger<ScriptedInputHandler> logger)
    {
        _commandQueue = new Queue<string>(commands);
        _logger = logger;

        _logger.LogInformation(
            "ScriptedInputHandler initialized with {CommandCount} commands",
            _commandQueue.Count);
    }

    /// <inheritdoc/>
    public string GetInput(string prompt)
    {
        PromptBuffer.Add(prompt);

        if (_commandQueue.Count == 0)
        {
            _logger.LogWarning(
                "Script exhausted at prompt: {Prompt}. Returning 'quit' to end test.",
                prompt);
            return "quit";
        }

        var command = _commandQueue.Dequeue();
        _logger.LogDebug(
            "Scripted input: '{Command}' (responding to: {Prompt}, {Remaining} commands remaining)",
            command, prompt, _commandQueue.Count);

        return command;
    }

    /// <inheritdoc/>
    public void DisplayMessage(string message)
    {
        OutputBuffer.Add(message);
        _logger.LogTrace("Output captured: {Message}", message);
    }

    /// <inheritdoc/>
    public void DisplayError(string message)
    {
        ErrorBuffer.Add(message);
        _logger.LogDebug("Error captured: {Message}", message);
    }

    /// <summary>
    /// Checks if any output line contains the specified text.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>True if any output line contains the text.</returns>
    public bool OutputContains(string text)
    {
        return OutputBuffer.Any(line => line.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if any error line contains the specified text.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>True if any error line contains the text.</returns>
    public bool ErrorContains(string text)
    {
        return ErrorBuffer.Any(line => line.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns all captured output as a single string.
    /// </summary>
    public string GetFullOutput()
    {
        return string.Join(Environment.NewLine, OutputBuffer);
    }

    /// <summary>
    /// Clears all captured buffers (output, errors, prompts).
    /// </summary>
    public void ClearBuffers()
    {
        OutputBuffer.Clear();
        ErrorBuffer.Clear();
        PromptBuffer.Clear();
        _logger.LogDebug("All buffers cleared");
    }
}
