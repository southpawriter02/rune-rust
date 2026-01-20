namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for the lexicon system containing term definitions.
/// </summary>
public class LexiconConfiguration
{
    /// <summary>
    /// Available context types for term selection.
    /// </summary>
    public IReadOnlyList<LexiconContext> Contexts { get; init; } = [];

    /// <summary>
    /// Term definitions keyed by term ID.
    /// </summary>
    public IReadOnlyDictionary<string, TermDefinition> Terms { get; init; } =
        new Dictionary<string, TermDefinition>();
}

/// <summary>
/// A context type for contextual synonym selection.
/// </summary>
public class LexiconContext
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// A term definition with synonyms and contextual variants.
/// </summary>
public class TermDefinition
{
    /// <summary>
    /// The default term used when synonyms are disabled.
    /// </summary>
    public string Default { get; init; } = string.Empty;

    /// <summary>
    /// General-purpose synonyms.
    /// </summary>
    public IReadOnlyList<string> Synonyms { get; init; } = [];

    /// <summary>
    /// Context-specific synonym lists.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Contextual { get; init; } =
        new Dictionary<string, IReadOnlyList<string>>();

    /// <summary>
    /// Probability weights for synonym selection.
    /// </summary>
    public IReadOnlyDictionary<string, int> Weights { get; init; } =
        new Dictionary<string, int>();

    /// <summary>
    /// Value-based variations (for damage, quantities, conditions).
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Severity { get; init; } =
        new Dictionary<string, IReadOnlyList<string>>();
}
