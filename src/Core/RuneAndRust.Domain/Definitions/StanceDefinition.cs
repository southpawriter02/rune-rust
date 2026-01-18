using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a combat stance with its associated stat modifiers.
/// </summary>
/// <remarks>
/// <para>StanceDefinition is an immutable domain entity that describes how a combat stance
/// modifies a combatant's stats when active.</para>
/// <para>Stances are loaded from JSON configuration and matched to the <see cref="CombatStance"/> enum.</para>
/// <para>Key characteristics:</para>
/// <list type="bullet">
///   <item><description>Attack bonus/penalty applied to attack rolls</description></item>
///   <item><description>Damage bonus/penalty as dice notation (e.g., "1d4" or "-1d4")</description></item>
///   <item><description>Defense bonus/penalty applied to armor class</description></item>
///   <item><description>Save bonus/penalty applied to all saving throws</description></item>
/// </list>
/// </remarks>
public class StanceDefinition : IEntity
{
    // ===== Properties =====

    /// <summary>
    /// Gets the unique identifier for this stance definition instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique string identifier for this stance (e.g., "aggressive", "defensive").
    /// </summary>
    /// <remarks>
    /// Used for configuration loading and modifier source tracking.
    /// Always stored in lowercase.
    /// </remarks>
    public string StanceId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the stance (e.g., "Aggressive Stance").
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the detailed description of the stance's effects and recommended usage.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets whether this is the default stance for new combatants.
    /// </summary>
    /// <remarks>
    /// Only one stance should be marked as default (typically Balanced).
    /// </remarks>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Gets the bonus or penalty applied to attack rolls while in this stance.
    /// </summary>
    /// <remarks>
    /// Positive values increase attack chance, negative values decrease it.
    /// Example: +2 for Aggressive, -2 for Defensive, 0 for Balanced.
    /// </remarks>
    public int AttackBonus { get; private set; }

    /// <summary>
    /// Gets the damage bonus or penalty as dice notation.
    /// </summary>
    /// <remarks>
    /// <para>Can be positive (e.g., "1d4") or negative (e.g., "-1d4").</para>
    /// <para>Null indicates no damage modification.</para>
    /// <para>Rolled and added to base weapon damage on successful hits.</para>
    /// </remarks>
    public string? DamageBonus { get; private set; }

    /// <summary>
    /// Gets the bonus or penalty applied to defense (armor class) while in this stance.
    /// </summary>
    /// <remarks>
    /// Positive values make the combatant harder to hit, negative values easier.
    /// Example: -2 for Aggressive, +2 for Defensive, 0 for Balanced.
    /// </remarks>
    public int DefenseBonus { get; private set; }

    /// <summary>
    /// Gets the bonus or penalty applied to all saving throws while in this stance.
    /// </summary>
    /// <remarks>
    /// Applied to fortitude, reflex, and will saves equally.
    /// Example: -1 for Aggressive, +2 for Defensive, 0 for Balanced.
    /// </remarks>
    public int SaveBonus { get; private set; }

    /// <summary>
    /// Gets the path to the stance's icon asset.
    /// </summary>
    /// <remarks>
    /// Used for UI display. May be null if no custom icon is defined.
    /// </remarks>
    public string? IconPath { get; private set; }

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core and serialization.
    /// </summary>
    private StanceDefinition() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a new stance definition with the specified properties.
    /// </summary>
    /// <param name="stanceId">Unique string identifier for the stance (will be lowercased).</param>
    /// <param name="name">Display name of the stance.</param>
    /// <param name="description">Detailed description of the stance's effects.</param>
    /// <param name="attackBonus">Bonus/penalty to attack rolls (default: 0).</param>
    /// <param name="damageBonus">Dice notation for damage bonus (e.g., "1d4", "-1d4", or null).</param>
    /// <param name="defenseBonus">Bonus/penalty to defense (default: 0).</param>
    /// <param name="saveBonus">Bonus/penalty to all saves (default: 0).</param>
    /// <param name="isDefault">Whether this is the default stance (default: false).</param>
    /// <param name="iconPath">Path to the stance icon (optional).</param>
    /// <returns>A new StanceDefinition instance.</returns>
    /// <exception cref="ArgumentException">Thrown when stanceId or name is null or whitespace.</exception>
    public static StanceDefinition Create(
        string stanceId,
        string name,
        string description,
        int attackBonus = 0,
        string? damageBonus = null,
        int defenseBonus = 0,
        int saveBonus = 0,
        bool isDefault = false,
        string? iconPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stanceId, nameof(stanceId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new StanceDefinition
        {
            Id = Guid.NewGuid(),
            StanceId = stanceId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            AttackBonus = attackBonus,
            DamageBonus = damageBonus,
            DefenseBonus = defenseBonus,
            SaveBonus = saveBonus,
            IsDefault = isDefault,
            IconPath = iconPath
        };
    }

    // ===== Methods =====

    /// <summary>
    /// Converts this stance definition to the corresponding <see cref="CombatStance"/> enum value.
    /// </summary>
    /// <returns>The matching CombatStance enum value, or Balanced if no match found.</returns>
    /// <remarks>
    /// Matches are case-insensitive based on the StanceId property.
    /// </remarks>
    public CombatStance ToCombatStance()
    {
        return StanceId switch
        {
            "aggressive" => CombatStance.Aggressive,
            "defensive" => CombatStance.Defensive,
            "balanced" => CombatStance.Balanced,
            _ => CombatStance.Balanced
        };
    }

    /// <summary>
    /// Gets the modifier source identifier for tracking stance-applied stat modifiers.
    /// </summary>
    /// <returns>A source string in the format "stance:{stanceId}".</returns>
    /// <remarks>
    /// Used by the StanceService to track and remove stance modifiers when switching stances.
    /// </remarks>
    public string GetModifierSource() => $"stance:{StanceId}";

    /// <summary>
    /// Determines whether this stance has any stat modifiers.
    /// </summary>
    /// <returns>True if any modifier is non-zero, false otherwise.</returns>
    public bool HasModifiers()
    {
        return AttackBonus != 0
            || !string.IsNullOrEmpty(DamageBonus)
            || DefenseBonus != 0
            || SaveBonus != 0;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({StanceId})";
}
