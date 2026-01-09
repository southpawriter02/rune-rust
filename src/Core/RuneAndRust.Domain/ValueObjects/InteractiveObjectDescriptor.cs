namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// A complete description of an interactive object based on type and state.
/// </summary>
/// <remarks>
/// Provides progressive detail based on examination depth, with each level
/// revealing more information about the object.
/// </remarks>
public readonly record struct InteractiveObjectDescriptor
{
    /// <summary>
    /// The type of interactive object.
    /// </summary>
    public InteractiveObjectType ObjectType { get; init; }

    /// <summary>
    /// The current state of the object.
    /// </summary>
    public ObjectState State { get; init; }

    /// <summary>
    /// Brief description shown on room entry (glance level).
    /// </summary>
    public string GlanceDescription { get; init; }

    /// <summary>
    /// Standard description shown with look command.
    /// </summary>
    public string LookDescription { get; init; }

    /// <summary>
    /// Detailed description shown with examine command.
    /// </summary>
    public string ExamineDescription { get; init; }

    /// <summary>
    /// Optional hint about interaction possibilities.
    /// </summary>
    public string? InteractionHint { get; init; }

    /// <summary>
    /// Gets the description appropriate for the examination depth.
    /// </summary>
    /// <param name="depth">The examination depth level.</param>
    /// <returns>The appropriate description text.</returns>
    public string GetDescription(ExaminationDepth depth)
    {
        return depth switch
        {
            ExaminationDepth.Glance => GlanceDescription,
            ExaminationDepth.Look => LookDescription,
            ExaminationDepth.Examine => BuildExamineDescription(),
            _ => LookDescription
        };
    }

    private string BuildExamineDescription()
    {
        var result = ExamineDescription;
        if (!string.IsNullOrEmpty(InteractionHint))
        {
            result += $" {InteractionHint}";
        }
        return result;
    }
}
