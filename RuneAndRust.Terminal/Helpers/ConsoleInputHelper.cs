namespace RuneAndRust.Terminal.Helpers;

/// <summary>
/// Helper for reading console input while filtering out mouse events (v0.3.5c).
/// Some terminals send mouse tracking escape sequences that Console.ReadKey()
/// interprets as key presses, causing unwanted screen refreshes.
/// </summary>
public static class ConsoleInputHelper
{
    /// <summary>
    /// Waits for a valid key press, filtering out mouse events and escape sequences.
    /// </summary>
    /// <param name="intercept">If true, the key is not displayed in the console.</param>
    /// <returns>The ConsoleKeyInfo for the pressed key.</returns>
    public static ConsoleKeyInfo ReadKeyFiltered(bool intercept = true)
    {
        while (true)
        {
            var key = Console.ReadKey(intercept);

            // Filter out escape sequences from mouse events
            // Mouse events typically start with Escape (27) followed by '[' and 'M'
            // They can also produce keys with KeyChar 0 and no valid ConsoleKey
            if (key.Key == ConsoleKey.Escape)
            {
                // Drain any remaining escape sequence characters
                DrainEscapeSequence();
                continue;
            }

            // Filter out null/empty key events that can come from mouse tracking
            if (key.KeyChar == '\0' && key.Key == 0)
            {
                continue;
            }

            // Valid key press
            return key;
        }
    }

    /// <summary>
    /// Waits for any valid key press (for "Press any key to continue" prompts).
    /// </summary>
    public static void WaitForKeyPress()
    {
        ReadKeyFiltered(intercept: true);
    }

    /// <summary>
    /// Drains any remaining characters from an escape sequence.
    /// </summary>
    private static void DrainEscapeSequence()
    {
        // Give a brief moment for escape sequence to arrive
        Thread.Sleep(10);

        // Drain any buffered characters from the escape sequence
        while (Console.KeyAvailable)
        {
            Console.ReadKey(intercept: true);
        }
    }
}
