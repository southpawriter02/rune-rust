namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration for a sequence-based puzzle requiring ordered activation.
/// </summary>
/// <remarks>
/// <para>
/// Sequence puzzles require activating objects (levers, buttons, symbols) in a specific order.
/// Each step corresponds to an InteractiveObject in the room, and validation tracks progress.
/// </para>
/// <list type="bullet">
///   <item><description>Steps must be activated in exact order</description></item>
///   <item><description>Wrong steps can optionally reset progress</description></item>
///   <item><description>Links to InteractiveObject IDs for activation</description></item>
/// </list>
/// </remarks>
public class SequencePuzzle
{
    // ===== Core Properties =====

    /// <summary>
    /// Gets the ordered list of step IDs that must be activated.
    /// </summary>
    public IReadOnlyList<string> RequiredSequence { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Gets descriptions for each step (for feedback).
    /// </summary>
    public IReadOnlyDictionary<string, string> StepDescriptions { get; private set; }
        = new Dictionary<string, string>();

    /// <summary>
    /// Gets whether wrong steps reset the entire sequence.
    /// </summary>
    public bool ResetOnWrongStep { get; private set; } = true;

    /// <summary>
    /// Gets whether steps must be consecutive (no delays).
    /// </summary>
    public bool RequiresConsecutiveSteps { get; private set; }

    /// <summary>
    /// Gets the max turns between steps (0 = no limit).
    /// </summary>
    public int MaxTurnsBetweenSteps { get; private set; }

    /// <summary>
    /// Gets associated interactive object IDs for each step.
    /// </summary>
    public IReadOnlyDictionary<string, Guid> StepObjectIds { get; private set; }
        = new Dictionary<string, Guid>();

    // ===== Computed Properties =====

    /// <summary>
    /// Gets the total number of steps in the sequence.
    /// </summary>
    public int TotalSteps => RequiredSequence.Count;

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private SequencePuzzle() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a sequence puzzle configuration.
    /// </summary>
    /// <param name="sequence">The ordered step IDs.</param>
    /// <param name="stepDescriptions">Optional feedback per step.</param>
    /// <param name="resetOnWrongStep">Whether wrong steps reset progress.</param>
    /// <param name="requiresConsecutive">Whether steps must be consecutive.</param>
    /// <param name="maxTurnsBetween">Maximum turns between steps.</param>
    /// <param name="stepObjectIds">Map of step IDs to object IDs.</param>
    /// <returns>A new SequencePuzzle instance.</returns>
    public static SequencePuzzle Create(
        IEnumerable<string> sequence,
        IDictionary<string, string>? stepDescriptions = null,
        bool resetOnWrongStep = true,
        bool requiresConsecutive = false,
        int maxTurnsBetween = 0,
        IDictionary<string, Guid>? stepObjectIds = null)
    {
        var sequenceList = sequence.ToList();
        ArgumentOutOfRangeException.ThrowIfZero(sequenceList.Count);

        return new SequencePuzzle
        {
            RequiredSequence = sequenceList,
            StepDescriptions = stepDescriptions?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                ?? new Dictionary<string, string>(),
            ResetOnWrongStep = resetOnWrongStep,
            RequiresConsecutiveSteps = requiresConsecutive,
            MaxTurnsBetweenSteps = Math.Max(0, maxTurnsBetween),
            StepObjectIds = stepObjectIds?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                ?? new Dictionary<string, Guid>()
        };
    }

    // ===== Validation Methods =====

    /// <summary>
    /// Validates if a step is the correct next step.
    /// </summary>
    /// <param name="completedSteps">Steps already completed.</param>
    /// <param name="stepId">The step being attempted.</param>
    /// <returns>True if this is the correct next step.</returns>
    public bool IsCorrectNextStep(IReadOnlyList<string> completedSteps, string stepId)
    {
        if (completedSteps.Count >= RequiredSequence.Count)
            return false;

        return RequiredSequence[completedSteps.Count].Equals(stepId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the sequence is complete.
    /// </summary>
    /// <param name="completedSteps">Steps completed so far.</param>
    /// <returns>True if all steps completed in order.</returns>
    public bool IsComplete(IReadOnlyList<string> completedSteps)
    {
        if (completedSteps.Count != RequiredSequence.Count)
            return false;

        for (int i = 0; i < completedSteps.Count; i++)
        {
            if (!completedSteps[i].Equals(RequiredSequence[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Gets the number of remaining steps.
    /// </summary>
    /// <param name="completedSteps">Steps completed so far.</param>
    /// <returns>Number of steps remaining.</returns>
    public int GetRemainingSteps(IReadOnlyList<string> completedSteps)
    {
        return Math.Max(0, RequiredSequence.Count - completedSteps.Count);
    }

    // ===== Helper Methods =====

    /// <summary>
    /// Gets the description for a step.
    /// </summary>
    /// <param name="stepId">The step ID.</param>
    /// <returns>The step description, or a default message.</returns>
    public string GetStepDescription(string stepId)
    {
        return StepDescriptions.TryGetValue(stepId, out var desc)
            ? desc
            : $"Step '{stepId}' activated.";
    }

    /// <summary>
    /// Gets the interactive object ID for a step.
    /// </summary>
    /// <param name="stepId">The step ID.</param>
    /// <returns>The object ID, or null if not mapped.</returns>
    public Guid? GetStepObjectId(string stepId)
    {
        return StepObjectIds.TryGetValue(stepId, out var id) ? id : null;
    }

    /// <summary>
    /// Finds the step ID associated with an object.
    /// </summary>
    /// <param name="objectId">The object ID.</param>
    /// <returns>The step ID, or null if not found.</returns>
    public string? GetStepIdForObject(Guid objectId)
    {
        return StepObjectIds.FirstOrDefault(kvp => kvp.Value == objectId).Key;
    }

    /// <summary>
    /// Gets the expected next step ID.
    /// </summary>
    /// <param name="completedSteps">Steps completed so far.</param>
    /// <returns>The next expected step ID, or null if complete.</returns>
    public string? GetNextExpectedStep(IReadOnlyList<string> completedSteps)
    {
        if (completedSteps.Count >= RequiredSequence.Count)
            return null;

        return RequiredSequence[completedSteps.Count];
    }

    /// <summary>
    /// Returns a string representation of this sequence puzzle.
    /// </summary>
    public override string ToString() =>
        $"SequencePuzzle({TotalSteps} steps, ResetOnWrongStep={ResetOnWrongStep})";
}
