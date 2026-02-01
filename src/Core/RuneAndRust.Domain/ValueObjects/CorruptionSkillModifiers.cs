// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionSkillModifiers.cs
// Immutable value object representing the stage-based skill check modifiers
// derived from a character's Runic Blight Corruption level. Encapsulates the
// tech bonus, social penalty, and faction reputation lock status as a single
// composite object for consumption by the skill check system and UI layer.
// Version: 0.18.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the stage-based skill check modifiers from corruption.
/// </summary>
/// <remarks>
/// <para>
/// Corruption grants a paradoxical benefit: as characters become more tainted
/// by the Blight, they gain an intuitive understanding of Aethelgard's runic
/// technology at the cost of social standing. This manifests as a Tech bonus
/// and Social penalty that scale with corruption stage.
/// </para>
/// <para>
/// Stage-based modifier table:
/// <list type="table">
///   <listheader>
///     <term>Stage</term>
///     <description>Modifiers</description>
///   </listheader>
///   <item>
///     <term>Uncorrupted (0-19)</term>
///     <description>+0 Tech, -0 Social, No faction lock</description>
///   </item>
///   <item>
///     <term>Tainted (20-39)</term>
///     <description>+1 Tech, -1 Social, No faction lock</description>
///   </item>
///   <item>
///     <term>Infected (40-59)</term>
///     <description>+2 Tech, -2 Social, Faction locked</description>
///   </item>
///   <item>
///     <term>Blighted (60-79)</term>
///     <description>+2 Tech, -2 Social, Faction locked</description>
///   </item>
///   <item>
///     <term>Corrupted (80-99)</term>
///     <description>+2 Tech, -2 Social, Faction locked</description>
///   </item>
///   <item>
///     <term>Consumed (100)</term>
///     <description>N/A — character is lost (Terminal Error)</description>
///   </item>
/// </list>
/// </para>
/// <para>
/// This composite value object is returned by
/// <c>ICorruptionService.GetSkillModifiers</c> and consumed by the skill check
/// system to apply corruption-based modifiers to relevant checks.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var modifiers = CorruptionSkillModifiers.Create(
///     techBonus: 2,
///     socialPenalty: -2,
///     factionLocked: true);
///
/// if (isTechCheck)
///     dicePool += modifiers.TechBonus;
/// if (isSocialCheck)
///     dicePool += modifiers.SocialPenalty; // Negative value, reduces pool
/// if (modifiers.FactionLocked)
///     Console.WriteLine("Human faction reputation gains are blocked.");
/// </code>
/// </example>
/// <seealso cref="Entities.CorruptionTracker"/>
/// <seealso cref="CorruptionStage"/>
/// <seealso cref="CorruptionState"/>
public readonly record struct CorruptionSkillModifiers
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Stored
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the tech skill bonus from corruption.
    /// </summary>
    /// <value>
    /// A non-negative integer bonus (0, +1, or +2) applied to technology-related
    /// skill checks. Higher corruption grants better intuitive understanding of
    /// runic technology. Caps at +2 from the Infected stage onward.
    /// </value>
    /// <remarks>
    /// The tech bonus represents the character's growing affinity with the
    /// corrupted runic machinery of Aethelgard. As the Blight integrates with
    /// the character's essence, they gain an uncanny understanding of the
    /// technology that created the Blight itself.
    /// </remarks>
    public int TechBonus { get; }

    /// <summary>
    /// Gets the social skill penalty from corruption.
    /// </summary>
    /// <value>
    /// A non-positive integer penalty (0, -1, or -2) applied to social
    /// interaction skill checks. Higher corruption makes the character visibly
    /// tainted, causing NPCs to react with fear or disgust. Caps at -2 from
    /// the Infected stage onward.
    /// </value>
    /// <remarks>
    /// The social penalty reflects the visible signs of Blight contamination:
    /// glowing veins, metallic patches on the skin, and an unsettling aura
    /// that even non-sensitive individuals can perceive.
    /// </remarks>
    public int SocialPenalty { get; }

    /// <summary>
    /// Gets whether the character's faction reputation is locked.
    /// </summary>
    /// <value>
    /// <c>true</c> if the character's corruption is 50 or higher, meaning
    /// they can no longer gain reputation with human factions; <c>false</c>
    /// if reputation gains are still possible.
    /// </value>
    /// <remarks>
    /// <para>
    /// Faction lock occurs at the Infected stage (corruption >= 50). At this
    /// point, the character's corruption is visible enough that human factions
    /// refuse to deepen their alliance. Existing reputation is maintained but
    /// cannot be increased.
    /// </para>
    /// <para>
    /// Machine/Blight-aligned factions are NOT affected by this lock and may
    /// even respond more favorably to corrupted characters (handled by the
    /// faction system, not by this modifier).
    /// </para>
    /// </remarks>
    public bool FactionLocked { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Arrow-Expression (derived from stored properties)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the character has any active corruption modifiers.
    /// </summary>
    /// <value>
    /// <c>true</c> if any modifier is non-zero or faction is locked;
    /// <c>false</c> if the character is at the Uncorrupted stage with no
    /// modifiers applied.
    /// </value>
    public bool HasModifiers => TechBonus != 0 || SocialPenalty != 0 || FactionLocked;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR (private — use factory methods)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor to enforce factory pattern.
    /// </summary>
    /// <param name="techBonus">The tech skill bonus (0, +1, or +2).</param>
    /// <param name="socialPenalty">The social skill penalty (0, -1, or -2).</param>
    /// <param name="factionLocked">Whether faction reputation is locked.</param>
    private CorruptionSkillModifiers(
        int techBonus,
        int socialPenalty,
        bool factionLocked)
    {
        TechBonus = techBonus;
        SocialPenalty = socialPenalty;
        FactionLocked = factionLocked;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a corruption skill modifiers result.
    /// </summary>
    /// <param name="techBonus">The tech skill bonus from corruption stage.</param>
    /// <param name="socialPenalty">The social skill penalty from corruption stage.</param>
    /// <param name="factionLocked">Whether human faction reputation gains are locked.</param>
    /// <returns>
    /// A new <see cref="CorruptionSkillModifiers"/> with the specified modifiers.
    /// </returns>
    /// <example>
    /// <code>
    /// // Tainted stage modifiers
    /// var tainted = CorruptionSkillModifiers.Create(
    ///     techBonus: 1,
    ///     socialPenalty: -1,
    ///     factionLocked: false);
    ///
    /// // Infected stage modifiers
    /// var infected = CorruptionSkillModifiers.Create(
    ///     techBonus: 2,
    ///     socialPenalty: -2,
    ///     factionLocked: true);
    /// </code>
    /// </example>
    public static CorruptionSkillModifiers Create(
        int techBonus,
        int socialPenalty,
        bool factionLocked) =>
        new(techBonus, socialPenalty, factionLocked);

    /// <summary>
    /// Returns modifiers for an uncorrupted character (no modifiers).
    /// </summary>
    /// <value>
    /// A <see cref="CorruptionSkillModifiers"/> with all zero values and
    /// no faction lock, representing a character with no Blight contamination.
    /// </value>
    public static CorruptionSkillModifiers None => new(0, 0, false);

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the corruption skill modifiers for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the tech bonus, social penalty, and faction
    /// lock status. Returns "No modifiers" for uncorrupted characters.
    /// </returns>
    /// <example>
    /// <code>
    /// var modifiers = CorruptionSkillModifiers.Create(2, -2, true);
    /// modifiers.ToString();
    /// // Returns "Corruption Modifiers: Tech +2, Social -2 [FACTION LOCKED]"
    ///
    /// var none = CorruptionSkillModifiers.None;
    /// none.ToString();
    /// // Returns "Corruption Modifiers: No modifiers"
    /// </code>
    /// </example>
    public override string ToString() =>
        HasModifiers
            ? $"Corruption Modifiers: Tech {(TechBonus >= 0 ? "+" : "")}{TechBonus}, Social {SocialPenalty}" +
              (FactionLocked ? " [FACTION LOCKED]" : "")
            : "Corruption Modifiers: No modifiers";
}
