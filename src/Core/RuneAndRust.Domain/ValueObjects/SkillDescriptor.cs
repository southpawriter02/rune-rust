namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a narrative descriptor for a skill check outcome.
/// </summary>
/// <remarks>
/// <para>
/// Voice guidance provides flavor text that varies by skill, outcome category,
/// and context. Descriptors are selected from pools of text to provide variety
/// while maintaining thematic consistency.
/// </para>
/// <para>
/// Descriptor pools are organized by:
/// <list type="bullet">
///   <item>Skill ID: Each skill has its own descriptor pools</item>
///   <item>Category: Catastrophic, Failed, Marginal, Competent, Impressive, Masterful</item>
///   <item>Context: Optional variants for corruption, surface type, etc.</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="SkillId">The ID of the skill this descriptor is for.</param>
/// <param name="Category">The descriptor category (maps from SkillOutcome).</param>
/// <param name="Text">The narrative text to display.</param>
/// <param name="IsContextual">Whether this descriptor is context-specific.</param>
/// <param name="ContextType">The type of context that influenced selection (e.g., "glitched", "hostile").</param>
public readonly record struct SkillDescriptor(
    string SkillId,
    DescriptorCategory Category,
    string Text,
    bool IsContextual = false,
    string? ContextType = null)
{
    /// <summary>
    /// Gets a value indicating whether this is a fallback (generic) descriptor.
    /// </summary>
    public bool IsFallback => !IsContextual && string.IsNullOrEmpty(ContextType);

    /// <summary>
    /// Gets a value indicating whether this descriptor has content.
    /// </summary>
    public bool HasContent => !string.IsNullOrWhiteSpace(Text);

    /// <summary>
    /// Creates an empty descriptor when no text is available.
    /// </summary>
    /// <param name="skillId">The skill ID.</param>
    /// <param name="category">The descriptor category.</param>
    /// <returns>An empty descriptor.</returns>
    public static SkillDescriptor Empty(string skillId, DescriptorCategory category)
    {
        return new SkillDescriptor(
            SkillId: skillId,
            Category: category,
            Text: string.Empty);
    }

    /// <summary>
    /// Creates a generic fallback descriptor.
    /// </summary>
    /// <param name="category">The descriptor category.</param>
    /// <returns>A generic descriptor for the category.</returns>
    public static SkillDescriptor GenericFallback(DescriptorCategory category)
    {
        var text = category switch
        {
            DescriptorCategory.Catastrophic => "Something goes terribly wrong.",
            DescriptorCategory.Failed => "The attempt fails.",
            DescriptorCategory.Marginal => "You barely succeed.",
            DescriptorCategory.Competent => "You succeed.",
            DescriptorCategory.Impressive => "You succeed impressively.",
            DescriptorCategory.Masterful => "You achieve a masterful success.",
            _ => "The check is resolved."
        };

        return new SkillDescriptor(
            SkillId: "generic",
            Category: category,
            Text: text);
    }

    /// <summary>
    /// Returns the descriptor text, suitable for display.
    /// </summary>
    /// <returns>The narrative text.</returns>
    public override string ToString() => Text;
}
