namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of concealment that can obscure a target from attackers.
/// Distinct from <see cref="CoverType"/> which provides physical defense bonuses.
/// </summary>
/// <remarks>
/// <para>Concealment affects attack accuracy by making targets harder to perceive,
/// unlike cover which provides a physical barrier. The Apex Predator ability
/// (Veiðimaðr Tier 3) denies all concealment benefits for targets with active Quarry Marks.</para>
/// <list type="bullet">
///   <item><description><see cref="None"/>: No concealment — target is fully visible.</description></item>
///   <item><description><see cref="LightObscurement"/>: Dim light, fog, light foliage — target partially hidden.</description></item>
///   <item><description><see cref="Invisibility"/>: Magical or alchemical invisibility — target unseen.</description></item>
///   <item><description><see cref="MagicalCamo"/>: Runic or seiðr-based camouflage — target blends with surroundings.</description></item>
///   <item><description><see cref="Hidden"/>: Stealth-based concealment — target has taken the Hide action.</description></item>
/// </list>
/// <para>Introduced in v0.20.7c for the Apex Predator passive ability.</para>
/// </remarks>
public enum ConcealmentType
{
    /// <summary>
    /// No concealment — the target is fully visible with no miss chance.
    /// </summary>
    None = 0,

    /// <summary>
    /// Light obscurement — dim light, thin fog, or light foliage partially hides the target.
    /// Examples: twilight shadows, morning mist, sparse undergrowth.
    /// </summary>
    LightObscurement = 1,

    /// <summary>
    /// Invisibility — magical or alchemical effect renders the target completely unseen.
    /// Examples: invisibility potions, cloaking runes, Myrk-gengr shadow abilities.
    /// </summary>
    Invisibility = 2,

    /// <summary>
    /// Magical camouflage — runic or seiðr-based effect causes the target to blend with surroundings.
    /// Examples: camouflage enchantments, adaptive cloaking, environmental blending spells.
    /// </summary>
    MagicalCamo = 3,

    /// <summary>
    /// Stealth-based concealment — the target has taken the Hide action and is using cover or shadows.
    /// Examples: crouching behind debris, hiding in darkness, using terrain for concealment.
    /// </summary>
    Hidden = 4
}
