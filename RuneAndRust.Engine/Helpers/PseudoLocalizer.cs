namespace RuneAndRust.Engine.Helpers;

/// <summary>
/// Transforms strings into pseudo-localized versions for QA testing (v0.3.15c - The Polyglot).
/// Applies diacritics and ~30% expansion to simulate translated text length.
/// </summary>
/// <remarks>
/// Pseudo-localization helps identify:
/// - Hardcoded strings that bypass localization
/// - UI elements that don't accommodate text expansion
/// - Character encoding issues with non-ASCII characters
/// </remarks>
public static class PseudoLocalizer
{
    private static readonly Dictionary<char, char> DiacriticMap = new()
    {
        ['a'] = 'á', ['e'] = 'é', ['i'] = 'í', ['o'] = 'ó', ['u'] = 'ú',
        ['A'] = 'Á', ['E'] = 'É', ['I'] = 'Í', ['O'] = 'Ó', ['U'] = 'Ú',
        ['c'] = 'ç', ['C'] = 'Ç', ['n'] = 'ñ', ['N'] = 'Ñ'
    };

    /// <summary>
    /// Transforms a string into its pseudo-localized version.
    /// Adds diacritics, expansion (~30%), and brackets for boundary detection.
    /// </summary>
    /// <param name="input">The original string to transform.</param>
    /// <returns>
    /// The pseudo-localized string with format: [tráñsfórmèd téxt___]
    /// Returns the input unchanged if null or empty.
    /// </returns>
    /// <example>
    /// "New Game" becomes "[Ñéw Gámé__]"
    /// "{0} min" becomes "[{0} míñ_]"
    /// </example>
    public static string Transform(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        result.Append('[');

        var i = 0;
        while (i < input.Length)
        {
            // Preserve format placeholders like {0}, {1:N2}, {name}
            if (input[i] == '{' && i + 1 < input.Length)
            {
                var end = input.IndexOf('}', i);
                if (end > i)
                {
                    result.Append(input.AsSpan(i, end - i + 1));
                    i = end + 1;
                    continue;
                }
            }

            result.Append(DiacriticMap.TryGetValue(input[i], out var replacement)
                ? replacement : input[i]);
            i++;
        }

        // Add ~30% expansion for length testing
        var expansionLength = Math.Max(1, (int)(input.Length * 0.3));
        result.Append(new string('_', expansionLength));
        result.Append(']');

        return result.ToString();
    }
}
