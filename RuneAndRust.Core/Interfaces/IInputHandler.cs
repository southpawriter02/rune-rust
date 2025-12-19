namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for handling user input.
/// Abstracted to allow different implementations (Terminal, GUI, Test Mock).
/// </summary>
public interface IInputHandler
{
    /// <summary>
    /// Prompts the user and waits for string input.
    /// </summary>
    /// <param name="prompt">The text to display to the user.</param>
    /// <returns>The raw input string from the user.</returns>
    string GetInput(string prompt);

    /// <summary>
    /// Displays a message to the user without expecting input.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void DisplayMessage(string message);

    /// <summary>
    /// Displays an error message to the user.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    void DisplayError(string message);
}
