// ═══════════════════════════════════════════════════════════════════════════════
// LineagePassiveBonuses.cs
// Value object containing passive stat and skill bonuses for a lineage.
// Version: 0.17.0b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains passive stat and skill bonuses granted by a lineage.
/// </summary>
/// <remarks>
/// <para>
/// LineagePassiveBonuses represents the inherent advantages of a bloodline heritage.
/// Unlike attribute modifiers which affect core stats during point-buy, passive bonuses
/// directly modify derived stats (Max HP, Max AP, Soak, Movement) and skill ranks.
/// </para>
/// <para>
/// Each lineage provides one stat bonus and one skill bonus, creating distinct
/// mechanical identities:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Lineage</term>
///     <description>Passive Bonuses</description>
///   </listheader>
///   <item>
///     <term>Clan-Born</term>
///     <description>+5 Max HP, +1 Social (resilient diplomats)</description>
///   </item>
///   <item>
///     <term>Rune-Marked</term>
///     <description>+5 Max AP, +1 Lore (mystical scholars)</description>
///   </item>
///   <item>
///     <term>Iron-Blooded</term>
///     <description>+2 Soak, +1 Craft (tough artisans)</description>
///   </item>
///   <item>
///     <term>Vargr-Kin</term>
///     <description>+1 Movement, +1 Survival (swift hunters)</description>
///   </item>
/// </list>
/// <para>
/// Passive bonuses are applied during character creation (v0.17.0f) and persist
/// throughout the character's life. They are added to derived stat calculations
/// performed by the DerivedStatCalculator (v0.17.2).
/// </para>
/// </remarks>
/// <param name="MaxHpBonus">Bonus added to Max HP calculation.</param>
/// <param name="MaxApBonus">Bonus added to Max AP (Aether Pool) calculation.</param>
/// <param name="SoakBonus">Bonus added to damage reduction.</param>
/// <param name="MovementBonus">Bonus added to movement range in tiles.</param>
/// <param name="SkillBonuses">List of skill bonuses granted by this lineage.</param>
/// <seealso cref="SkillBonus"/>
/// <seealso cref="Entities.LineageDefinition"/>
public readonly record struct LineagePassiveBonuses(
    int MaxHpBonus,
    int MaxApBonus,
    int SoakBonus,
    int MovementBonus,
    IReadOnlyList<SkillBonus> SkillBonuses)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this lineage grants any HP bonus.
    /// </summary>
    /// <value><c>true</c> if <see cref="MaxHpBonus"/> is greater than zero.</value>
    public bool HasHpBonus => MaxHpBonus > 0;

    /// <summary>
    /// Gets whether this lineage grants any AP bonus.
    /// </summary>
    /// <value><c>true</c> if <see cref="MaxApBonus"/> is greater than zero.</value>
    public bool HasApBonus => MaxApBonus > 0;

    /// <summary>
    /// Gets whether this lineage grants any Soak bonus.
    /// </summary>
    /// <value><c>true</c> if <see cref="SoakBonus"/> is greater than zero.</value>
    public bool HasSoakBonus => SoakBonus > 0;

    /// <summary>
    /// Gets whether this lineage grants any Movement bonus.
    /// </summary>
    /// <value><c>true</c> if <see cref="MovementBonus"/> is greater than zero.</value>
    public bool HasMovementBonus => MovementBonus > 0;

    /// <summary>
    /// Gets whether this lineage grants any skill bonuses.
    /// </summary>
    /// <value><c>true</c> if <see cref="SkillBonuses"/> contains at least one entry.</value>
    public bool HasSkillBonuses => SkillBonuses.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this lineage has any passive bonuses at all.
    /// </summary>
    /// <value>
    /// <c>true</c> if any stat or skill bonus is present; otherwise, <c>false</c>.
    /// </value>
    public bool HasAnyBonuses =>
        HasHpBonus || HasApBonus || HasSoakBonus || HasMovementBonus || HasSkillBonuses;

    // ═══════════════════════════════════════════════════════════════════════════
    // METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the skill bonus amount for a specific skill, if any.
    /// </summary>
    /// <param name="skillId">The skill identifier to look up.</param>
    /// <returns>
    /// The bonus amount for the specified skill, or 0 if no bonus exists.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="skillId"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// The lookup is case-insensitive. For example, "Social", "SOCIAL", and "social"
    /// will all match a skill bonus defined for "social".
    /// </remarks>
    /// <example>
    /// <code>
    /// var clanBorn = LineagePassiveBonuses.ClanBorn;
    /// 
    /// var socialBonus = clanBorn.GetSkillBonus("social");  // Returns 1
    /// var loreBonus = clanBorn.GetSkillBonus("lore");      // Returns 0
    /// </code>
    /// </example>
    public int GetSkillBonus(string skillId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));

        var normalizedId = skillId.ToLowerInvariant();

        foreach (var skillBonus in SkillBonuses)
        {
            if (skillBonus.NormalizedSkillId.Equals(normalizedId, StringComparison.Ordinal))
            {
                return skillBonus.BonusAmount;
            }
        }

        return 0;
    }

    /// <summary>
    /// Checks if this lineage grants a bonus to the specified skill.
    /// </summary>
    /// <param name="skillId">The skill identifier to check.</param>
    /// <returns>
    /// <c>true</c> if a bonus exists for the skill; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Returns <c>false</c> for null or whitespace skill IDs without throwing.
    /// </remarks>
    public bool HasBonusForSkill(string? skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
        {
            return false;
        }

        var normalizedId = skillId.ToLowerInvariant();
        return SkillBonuses.Any(sb =>
            sb.NormalizedSkillId.Equals(normalizedId, StringComparison.Ordinal));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="LineagePassiveBonuses"/> with validation.
    /// </summary>
    /// <param name="maxHpBonus">HP bonus (must be non-negative).</param>
    /// <param name="maxApBonus">AP bonus (must be non-negative).</param>
    /// <param name="soakBonus">Soak bonus (must be non-negative).</param>
    /// <param name="movementBonus">Movement bonus (must be non-negative).</param>
    /// <param name="skillBonuses">Skill bonuses (can be null or empty).</param>
    /// <returns>A new <see cref="LineagePassiveBonuses"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any stat bonus is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create custom passive bonuses
    /// var bonuses = LineagePassiveBonuses.Create(
    ///     maxHpBonus: 5,
    ///     maxApBonus: 0,
    ///     soakBonus: 0,
    ///     movementBonus: 0,
    ///     skillBonuses: new[] { SkillBonus.Create("social", 1) });
    /// </code>
    /// </example>
    public static LineagePassiveBonuses Create(
        int maxHpBonus,
        int maxApBonus,
        int soakBonus,
        int movementBonus,
        IReadOnlyList<SkillBonus>? skillBonuses = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxHpBonus, nameof(maxHpBonus));
        ArgumentOutOfRangeException.ThrowIfNegative(maxApBonus, nameof(maxApBonus));
        ArgumentOutOfRangeException.ThrowIfNegative(soakBonus, nameof(soakBonus));
        ArgumentOutOfRangeException.ThrowIfNegative(movementBonus, nameof(movementBonus));

        return new LineagePassiveBonuses(
            maxHpBonus,
            maxApBonus,
            soakBonus,
            movementBonus,
            skillBonuses ?? Array.Empty<SkillBonus>());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC FACTORY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the standard passive bonuses for Clan-Born lineage.
    /// </summary>
    /// <value>
    /// Bonuses: +5 Max HP, +1 Social skill bonus.
    /// </value>
    /// <remarks>
    /// Clan-Born are resilient community builders with enhanced health
    /// and natural social aptitude from generations of clan governance.
    /// </remarks>
    public static LineagePassiveBonuses ClanBorn => Create(
        maxHpBonus: 5,
        maxApBonus: 0,
        soakBonus: 0,
        movementBonus: 0,
        skillBonuses: new[] { SkillBonus.Create("social", 1) });

    /// <summary>
    /// Gets the standard passive bonuses for Rune-Marked lineage.
    /// </summary>
    /// <value>
    /// Bonuses: +5 Max AP, +1 Lore skill bonus.
    /// </value>
    /// <remarks>
    /// Rune-Marked are mystical scholars with enhanced Aether reserves
    /// and deep knowledge of the arcane from their tainted heritage.
    /// </remarks>
    public static LineagePassiveBonuses RuneMarked => Create(
        maxHpBonus: 0,
        maxApBonus: 5,
        soakBonus: 0,
        movementBonus: 0,
        skillBonuses: new[] { SkillBonus.Create("lore", 1) });

    /// <summary>
    /// Gets the standard passive bonuses for Iron-Blooded lineage.
    /// </summary>
    /// <value>
    /// Bonuses: +2 Soak, +1 Craft skill bonus.
    /// </value>
    /// <remarks>
    /// Iron-Blooded are tough artisans with natural damage resistance
    /// and crafting aptitude from generations of forge work.
    /// </remarks>
    public static LineagePassiveBonuses IronBlooded => Create(
        maxHpBonus: 0,
        maxApBonus: 0,
        soakBonus: 2,
        movementBonus: 0,
        skillBonuses: new[] { SkillBonus.Create("craft", 1) });

    /// <summary>
    /// Gets the standard passive bonuses for Vargr-Kin lineage.
    /// </summary>
    /// <value>
    /// Bonuses: +1 Movement, +1 Survival skill bonus.
    /// </value>
    /// <remarks>
    /// Vargr-Kin are swift hunters with enhanced movement speed
    /// and survival instincts from their primal wolf-spirit heritage.
    /// </remarks>
    public static LineagePassiveBonuses VargrKin => Create(
        maxHpBonus: 0,
        maxApBonus: 0,
        soakBonus: 0,
        movementBonus: 1,
        skillBonuses: new[] { SkillBonus.Create("survival", 1) });

    /// <summary>
    /// Gets an empty passive bonuses instance (no bonuses).
    /// </summary>
    /// <value>
    /// An instance with all bonuses set to zero and no skill bonuses.
    /// </value>
    /// <remarks>
    /// Use this for testing or when a lineage should have no passive bonuses.
    /// </remarks>
    public static LineagePassiveBonuses None => Create(0, 0, 0, 0);

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING OVERRIDE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of all passive bonuses.
    /// </summary>
    /// <returns>
    /// A comma-separated list of non-zero bonuses, or "No passive bonuses"
    /// if no bonuses are present.
    /// </returns>
    /// <example>
    /// <code>
    /// var clanBorn = LineagePassiveBonuses.ClanBorn;
    /// Console.WriteLine(clanBorn.ToString());
    /// // Output: "+5 Max HP, social +1"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var parts = new List<string>();

        if (MaxHpBonus > 0)
        {
            parts.Add($"+{MaxHpBonus} Max HP");
        }

        if (MaxApBonus > 0)
        {
            parts.Add($"+{MaxApBonus} Max AP");
        }

        if (SoakBonus > 0)
        {
            parts.Add($"+{SoakBonus} Soak");
        }

        if (MovementBonus > 0)
        {
            parts.Add($"+{MovementBonus} Movement");
        }

        foreach (var skill in SkillBonuses)
        {
            parts.Add(skill.ToString());
        }

        return parts.Count > 0
            ? string.Join(", ", parts)
            : "No passive bonuses";
    }
}
