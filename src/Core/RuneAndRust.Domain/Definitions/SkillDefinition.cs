namespace RuneAndRust.Domain.Definitions;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines a skill that can be checked against a difficulty class.
/// Skills are configured via JSON and linked to player attributes.
/// </summary>
/// <remarks>
/// <para>Each skill has a primary attribute (e.g., "finesse" for Lockpicking) that provides
/// a modifier bonus to the skill check roll.</para>
/// <para>Some skills have a secondary attribute that provides half its value as additional bonus.</para>
/// </remarks>
public class SkillDefinition
{
    /// <summary>
    /// Gets the unique identifier for this skill.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name shown to players.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the description explaining what this skill covers.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the primary attribute that modifies this skill.
    /// Must be one of: "might", "fortitude", "will", "wits", "finesse".
    /// </summary>
    public string PrimaryAttribute { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional secondary attribute that provides half bonus.
    /// </summary>
    public string? SecondaryAttribute { get; init; }

    /// <summary>
    /// Gets the base dice pool notation for this skill (e.g., "1d10", "2d6").
    /// </summary>
    public string BaseDicePool { get; init; } = "1d10";

    /// <summary>
    /// Gets whether this skill can be attempted without training.
    /// </summary>
    public bool AllowUntrained { get; init; } = true;

    /// <summary>
    /// Gets the penalty applied when attempting this skill untrained.
    /// </summary>
    public int UntrainedPenalty { get; init; } = 0;

    /// <summary>
    /// Gets the category for grouping skills in the UI.
    /// </summary>
    public string Category { get; init; } = "General";

    /// <summary>
    /// Gets tags for filtering and searching skills.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the sort order for display purposes.
    /// </summary>
    public int SortOrder { get; init; } = 0;

    /// <summary>
    /// Private constructor for JSON deserialization.
    /// </summary>
    private SkillDefinition() { }

    /// <summary>
    /// Creates a new skill definition with validation.
    /// </summary>
    public static SkillDefinition Create(
        string id,
        string name,
        string description,
        string primaryAttribute,
        string? secondaryAttribute = null,
        string baseDicePool = "1d10",
        bool allowUntrained = true,
        int untrainedPenalty = 0,
        string category = "General",
        IReadOnlyList<string>? tags = null,
        int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(primaryAttribute, nameof(primaryAttribute));

        var validAttributes = new[] { "might", "fortitude", "will", "wits", "finesse" };
        var normalizedPrimary = primaryAttribute.ToLowerInvariant();
        if (!validAttributes.Contains(normalizedPrimary))
        {
            throw new ArgumentException(
                $"Primary attribute must be one of: {string.Join(", ", validAttributes)}",
                nameof(primaryAttribute));
        }

        if (secondaryAttribute != null)
        {
            var normalizedSecondary = secondaryAttribute.ToLowerInvariant();
            if (!validAttributes.Contains(normalizedSecondary))
            {
                throw new ArgumentException(
                    $"Secondary attribute must be one of: {string.Join(", ", validAttributes)}",
                    nameof(secondaryAttribute));
            }
        }

        if (!string.IsNullOrEmpty(baseDicePool) && !DicePool.TryParse(baseDicePool, out _))
        {
            throw new ArgumentException(
                $"Invalid dice pool notation: {baseDicePool}",
                nameof(baseDicePool));
        }

        if (untrainedPenalty < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(untrainedPenalty),
                "Untrained penalty cannot be negative");
        }

        return new SkillDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            PrimaryAttribute = normalizedPrimary,
            SecondaryAttribute = secondaryAttribute?.ToLowerInvariant(),
            BaseDicePool = string.IsNullOrWhiteSpace(baseDicePool) ? "1d10" : baseDicePool,
            AllowUntrained = allowUntrained,
            UntrainedPenalty = untrainedPenalty,
            Category = category ?? "General",
            Tags = tags ?? Array.Empty<string>(),
            SortOrder = sortOrder
        };
    }

    /// <summary>
    /// Checks if this skill has the specified tag.
    /// </summary>
    public bool HasTag(string tag) =>
        Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if this skill belongs to the specified category.
    /// </summary>
    public bool IsInCategory(string category) =>
        Category.Equals(category, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if this skill has a secondary attribute modifier.
    /// </summary>
    public bool HasSecondaryAttribute => !string.IsNullOrEmpty(SecondaryAttribute);

    /// <summary>
    /// Returns true if this skill requires training.
    /// </summary>
    public bool RequiresTraining => !AllowUntrained || UntrainedPenalty > 0;

    // ===== Proficiency System (v0.4.3c) =====

    /// <summary>
    /// Gets the bonus granted per proficiency level.
    /// </summary>
    public int BonusPerLevel { get; init; } = 1;

    /// <summary>
    /// Gets the experience thresholds for each proficiency level.
    /// </summary>
    public IReadOnlyDictionary<Enums.SkillProficiency, int> ExperienceThresholds { get; init; }
        = GetDefaultThresholds();

    /// <summary>
    /// Gets the default experience thresholds for skill progression.
    /// </summary>
    private static Dictionary<Enums.SkillProficiency, int> GetDefaultThresholds() => new()
    {
        { Enums.SkillProficiency.Novice, 0 },
        { Enums.SkillProficiency.Apprentice, 50 },
        { Enums.SkillProficiency.Journeyman, 150 },
        { Enums.SkillProficiency.Expert, 350 },
        { Enums.SkillProficiency.Master, 700 }
    };

    /// <summary>
    /// Gets the bonus for a given proficiency level.
    /// </summary>
    /// <param name="proficiency">The proficiency level.</param>
    /// <returns>The skill bonus.</returns>
    public int GetProficiencyBonus(Enums.SkillProficiency proficiency) =>
        (int)proficiency * BonusPerLevel;

    /// <summary>
    /// Gets the proficiency level for a given experience amount.
    /// </summary>
    /// <param name="experience">The experience amount.</param>
    /// <returns>The proficiency level earned.</returns>
    public Enums.SkillProficiency GetProficiencyForExperience(int experience)
    {
        var result = Enums.SkillProficiency.Untrained;

        foreach (var kvp in ExperienceThresholds.OrderBy(k => k.Value))
        {
            if (experience >= kvp.Value)
                result = kvp.Key;
            else
                break;
        }

        return result;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({PrimaryAttribute})";
}

