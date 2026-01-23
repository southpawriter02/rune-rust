using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration for a single step in a chained skill check.
/// </summary>
/// <remarks>
/// <para>
/// Each step defines what skill check is required, its difficulty, and whether
/// retries are allowed. Steps are processed sequentially; each must succeed
/// before the next is attempted.
/// </para>
/// </remarks>
/// <param name="StepId">Unique identifier for this step within the chain.</param>
/// <param name="Name">Display name for the step (e.g., "Access", "Authentication").</param>
/// <param name="Description">Description of what this step represents.</param>
/// <param name="SkillId">The skill used for this step's check.</param>
/// <param name="SubType">Optional skill subtype (for master ability filtering).</param>
/// <param name="DifficultyClass">The DC for this step.</param>
/// <param name="DifficultyName">Display name for the difficulty (e.g., "Moderate").</param>
/// <param name="MaxRetries">Number of retry attempts allowed (0 = no retries).</param>
/// <param name="Context">Optional skill context with modifiers for this step.</param>
/// <param name="FailureConsequence">Optional consequence type if this step fumbles.</param>
/// <param name="SuccessMessage">Optional message to display on step success.</param>
/// <param name="FailureMessage">Optional message to display on step failure.</param>
public readonly record struct ChainedCheckStep(
    string StepId,
    string Name,
    string Description,
    string SkillId,
    string? SubType,
    int DifficultyClass,
    string DifficultyName = "Custom",
    int MaxRetries = 0,
    SkillContext? Context = null,
    FumbleType? FailureConsequence = null,
    string? SuccessMessage = null,
    string? FailureMessage = null)
{
    /// <summary>
    /// Whether this step allows retry attempts on failure.
    /// </summary>
    public bool AllowsRetries => MaxRetries > 0;

    /// <summary>
    /// Creates a simple step with minimal configuration.
    /// </summary>
    /// <param name="stepId">Step identifier.</param>
    /// <param name="name">Step name.</param>
    /// <param name="skillId">Skill ID for the check.</param>
    /// <param name="dc">Difficulty class.</param>
    /// <param name="retries">Number of retries (default 0).</param>
    /// <returns>A new chained check step.</returns>
    public static ChainedCheckStep Create(
        string stepId,
        string name,
        string skillId,
        int dc,
        int retries = 0)
    {
        return new ChainedCheckStep(
            StepId: stepId,
            Name: name,
            Description: $"{name} ({skillId} DC {dc})",
            SkillId: skillId,
            SubType: null,
            DifficultyClass: dc,
            DifficultyName: dc switch
            {
                <= 1 => "Trivial",
                2 => "Easy",
                3 => "Moderate",
                4 => "Challenging",
                >= 5 => "Hard"
            },
            MaxRetries: retries);
    }

    /// <summary>
    /// Gets a display string for this step.
    /// </summary>
    public string ToDisplayString() =>
        $"{Name}: {SkillId} DC {DifficultyClass}" +
        (MaxRetries > 0 ? $" ({MaxRetries} retries)" : "");
}
