namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes skill check outcomes for voice guidance descriptor lookup.
/// </summary>
/// <remarks>
/// <para>
/// Maps to pools of narrative text that provide flavor descriptions for skill outcomes.
/// Each category has skill-specific descriptor pools in the voice guidance system.
/// </para>
/// <para>
/// Categories align with <see cref="SkillOutcome"/> but provide a semantic layer
/// for the voice guidance system to select appropriate narrative text.
/// </para>
/// </remarks>
public enum DescriptorCategory
{
    /// <summary>
    /// Catastrophic failure - fumble descriptors with consequence narratives.
    /// </summary>
    Catastrophic = 0,

    /// <summary>
    /// Standard failure - unsuccessful attempt descriptors.
    /// </summary>
    Failed = 1,

    /// <summary>
    /// Marginal success - barely succeeded, complications possible.
    /// </summary>
    Marginal = 2,

    /// <summary>
    /// Full success - competent execution descriptors.
    /// </summary>
    Competent = 3,

    /// <summary>
    /// Exceptional success - impressive performance descriptors.
    /// </summary>
    Impressive = 4,

    /// <summary>
    /// Critical success - masterful execution descriptors.
    /// </summary>
    Masterful = 5
}
