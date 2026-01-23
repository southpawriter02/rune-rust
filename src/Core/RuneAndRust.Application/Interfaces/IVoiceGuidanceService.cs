namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides narrative descriptors for skill check outcomes.
/// </summary>
/// <remarks>
/// <para>
/// Voice guidance enriches skill check feedback with thematic, skill-specific
/// narrative text. Each skill has pools of descriptors for each outcome category,
/// with optional context-specific variants.
/// </para>
/// <para>
/// Descriptor selection considers:
/// <list type="bullet">
///   <item>Skill identity: Lockpicking has different flavor than persuasion</item>
///   <item>Outcome severity: Fumbles vs critical successes</item>
///   <item>Context: Corruption, target disposition, surface type, etc.</item>
/// </list>
/// </para>
/// </remarks>
public interface IVoiceGuidanceService
{
    /// <summary>
    /// Gets a narrative descriptor for a skill check outcome.
    /// </summary>
    /// <param name="skillId">The ID of the skill used.</param>
    /// <param name="category">The outcome category for descriptor lookup.</param>
    /// <param name="context">Optional skill context for context-specific variants.</param>
    /// <returns>A <see cref="SkillDescriptor"/> with narrative text.</returns>
    SkillDescriptor GetDescriptor(
        string skillId,
        DescriptorCategory category,
        SkillContext? context = null);

    /// <summary>
    /// Gets a descriptor using the outcome details to determine category.
    /// </summary>
    /// <param name="skillId">The ID of the skill used.</param>
    /// <param name="outcomeDetails">The outcome details from the skill check.</param>
    /// <param name="context">Optional skill context for context-specific variants.</param>
    /// <returns>A <see cref="SkillDescriptor"/> with narrative text.</returns>
    SkillDescriptor GetDescriptor(
        string skillId,
        OutcomeDetails outcomeDetails,
        SkillContext? context = null);

    /// <summary>
    /// Gets all available descriptor categories for a skill.
    /// </summary>
    /// <param name="skillId">The ID of the skill.</param>
    /// <returns>A collection of categories that have descriptors configured.</returns>
    IReadOnlyList<DescriptorCategory> GetAvailableCategories(string skillId);

    /// <summary>
    /// Checks if a skill has any descriptors configured.
    /// </summary>
    /// <param name="skillId">The ID of the skill.</param>
    /// <returns>True if the skill has descriptors configured.</returns>
    bool HasDescriptors(string skillId);

    /// <summary>
    /// Gets the count of descriptors in a pool.
    /// </summary>
    /// <param name="skillId">The ID of the skill.</param>
    /// <param name="category">The descriptor category.</param>
    /// <returns>The number of available descriptors in the pool.</returns>
    int GetPoolSize(string skillId, DescriptorCategory category);
}
