namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration for procedural monster name generation.
/// </summary>
/// <remarks>
/// Used by Named tier monsters to generate unique names like "Grok the Orc".
/// The format string uses {0} for the generated name and {1} for the monster type.
/// </remarks>
public readonly record struct NameGeneratorConfig
{
    /// <summary>
    /// Gets the list of possible first name prefixes.
    /// </summary>
    /// <example>["Gr", "Kr", "Th", "Br", "Sk"]</example>
    public IReadOnlyList<string> Prefixes { get; init; }

    /// <summary>
    /// Gets the list of possible name suffixes.
    /// </summary>
    /// <example>["ok", "ag", "ul", "ax", "or"]</example>
    public IReadOnlyList<string> Suffixes { get; init; }

    /// <summary>
    /// Gets the title format string.
    /// </summary>
    /// <remarks>
    /// {0} = generated name, {1} = monster type name.
    /// </remarks>
    /// <example>"{0} the {1}"</example>
    public string TitleFormat { get; init; }

    /// <summary>
    /// Creates a default name generator config with standard fantasy name components.
    /// </summary>
    public static NameGeneratorConfig Default => new()
    {
        Prefixes = ["Gr", "Kr", "Th", "Br", "Sk", "Dr", "Bl", "Tr", "Zr", "Vr"],
        Suffixes = ["ok", "ag", "ul", "ax", "or", "ek", "im", "an", "ur", "ash"],
        TitleFormat = "{0} the {1}"
    };

    /// <summary>
    /// Generates a random name using this configuration.
    /// </summary>
    /// <param name="monsterTypeName">The base monster type name (e.g., "Orc").</param>
    /// <param name="random">Random number generator. Uses Random.Shared if null.</param>
    /// <returns>A generated name like "Grok the Orc".</returns>
    public string GenerateName(string monsterTypeName, Random? random = null)
    {
        random ??= Random.Shared;

        // Handle empty prefix/suffix lists gracefully
        if (Prefixes == null || Prefixes.Count == 0 ||
            Suffixes == null || Suffixes.Count == 0)
        {
            return monsterTypeName;
        }

        var prefix = Prefixes[random.Next(Prefixes.Count)];
        var suffix = Suffixes[random.Next(Suffixes.Count)];
        var generatedName = prefix + suffix;

        // Use default format if TitleFormat is empty
        var format = string.IsNullOrWhiteSpace(TitleFormat) ? "{0} the {1}" : TitleFormat;

        return string.Format(format, generatedName, monsterTypeName);
    }
}
