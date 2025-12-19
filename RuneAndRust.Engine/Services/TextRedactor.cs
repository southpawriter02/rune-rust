using System.Text;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for redacting text based on completion percentage.
/// Creates a fragmented knowledge effect for incomplete Codex entries.
/// Uses stable pseudo-random masking so the same text at the same completion level
/// always produces the same output.
/// </summary>
public class TextRedactor
{
    /// <summary>
    /// Spectre.Console markup for redacted text blocks.
    /// </summary>
    private const string RedactedBlock = "[grey]████[/]";

    /// <summary>
    /// Message shown when entry is completely unknown.
    /// </summary>
    private const string FullyRedactedMessage = "[REDACTED]";

    /// <summary>
    /// Redacts text based on completion percentage.
    /// Uses word-level masking with stable pseudo-random selection.
    /// </summary>
    /// <param name="fullText">The complete text to redact.</param>
    /// <param name="completionPct">Completion percentage (0-100).</param>
    /// <returns>Text with masked words based on completion.</returns>
    public string RedactText(string fullText, int completionPct)
    {
        // Handle edge cases
        if (completionPct >= 100)
        {
            return fullText;
        }

        if (completionPct <= 0)
        {
            return FullyRedactedMessage;
        }

        if (string.IsNullOrWhiteSpace(fullText))
        {
            return fullText;
        }

        var words = fullText.Split(' ');
        var result = new StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            // Stable pseudo-random based on word index
            // Formula ensures same word is always visible/hidden at same completion level
            // Multiplier 7 and offset 13 are primes to reduce patterns
            bool visible = ((i * 7 + 13) % 100) < completionPct;

            if (visible)
            {
                result.Append(words[i]);
            }
            else
            {
                result.Append(RedactedBlock);
            }

            // Add space between words (except after last word)
            if (i < words.Length - 1)
            {
                result.Append(' ');
            }
        }

        return result.ToString();
    }
}
