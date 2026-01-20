namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes status effects by their general nature.
/// </summary>
/// <remarks>
/// <para>Categories determine default behavior for cleansing and UI display.</para>
/// <para>Buffs are beneficial, Debuffs are harmful, Environmental are neutral conditions.</para>
/// </remarks>
public enum EffectCategory
{
    /// <summary>
    /// Negative effects that harm or hinder the target.
    /// Can be cleansed by purify effects.
    /// </summary>
    Debuff,

    /// <summary>
    /// Positive effects that help or enhance the target.
    /// Can be dispelled by enemy abilities.
    /// </summary>
    Buff,

    /// <summary>
    /// Effects from environmental conditions.
    /// May interact with other effects (e.g., Wet + Lightning).
    /// </summary>
    Environmental
}
