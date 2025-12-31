using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a magical spell that can be cast by characters.
/// Spells have schools, costs, ranges, and targeting requirements.
/// </summary>
/// <remarks>
/// See: v0.4.3b (The Grimoire) for implementation details.
///
/// Spells connect to the Flux system via FluxCost:
/// - Casting spells adds flux to the environment (v0.4.3a)
/// - Higher flux levels increase magical instability
/// </remarks>
public class Spell
{
    #region Identity

    /// <summary>
    /// Gets or sets the unique identifier for this spell.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the display name of the spell.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the spell's effects.
    /// Should be written in AAM-VOICE Layer 2 (Diagnostic) style.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    #endregion

    #region Classification

    /// <summary>
    /// Gets or sets the magical school this spell belongs to.
    /// </summary>
    public SpellSchool School { get; set; } = SpellSchool.Destruction;

    /// <summary>
    /// Gets or sets what targets this spell can affect.
    /// </summary>
    public SpellTargetType TargetType { get; set; } = SpellTargetType.SingleEnemy;

    /// <summary>
    /// Gets or sets the range at which this spell can be cast.
    /// </summary>
    public SpellRange Range { get; set; } = SpellRange.Medium;

    #endregion

    #region Costs

    /// <summary>
    /// Gets or sets the Action Point cost to cast this spell.
    /// Characters must have sufficient AP to cast.
    /// </summary>
    public int ApCost { get; set; } = 2;

    /// <summary>
    /// Gets or sets the Flux cost when casting this spell.
    /// Adds to environmental flux (v0.4.3a integration).
    /// </summary>
    public int FluxCost { get; set; } = 5;

    #endregion

    #region Power

    /// <summary>
    /// Gets or sets the base power value for damage/healing calculations.
    /// Modified by caster attributes and equipment.
    /// </summary>
    public int BasePower { get; set; } = 10;

    /// <summary>
    /// Gets or sets the script identifier for applying spell effects.
    /// References effect scripts in the SpellEffectSystem (v0.4.3c).
    /// </summary>
    public string? EffectScript { get; set; }

    #endregion

    #region Casting Mechanics

    /// <summary>
    /// Gets or sets the number of turns required to charge before casting.
    /// 0 = instant cast, 1+ = must spend that many turns charging.
    /// </summary>
    public int ChargeTurns { get; set; } = 0;

    /// <summary>
    /// Gets or sets the message displayed when the spell begins charging.
    /// Shown to other combatants as a telegraph.
    /// </summary>
    public string? TelegraphMessage { get; set; }

    /// <summary>
    /// Gets or sets whether this spell requires concentration to maintain.
    /// Concentrated spells end if the caster takes damage or casts another spell.
    /// </summary>
    public bool RequiresConcentration { get; set; } = false;

    #endregion

    #region Requirements

    /// <summary>
    /// Gets or sets the minimum tier required to learn this spell.
    /// 1 = novice, 2 = apprentice, 3 = journeyman, 4 = master.
    /// </summary>
    public int Tier { get; set; } = 1;

    /// <summary>
    /// Gets or sets the archetype that can learn this spell.
    /// Null means any archetype can learn it.
    /// </summary>
    public ArchetypeType? Archetype { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// Gets or sets the timestamp when this spell was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this spell was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets whether this spell is an instant cast (no charge time).
    /// </summary>
    public bool IsInstantCast => ChargeTurns == 0;

    /// <summary>
    /// Gets whether this spell requires charging before casting.
    /// </summary>
    public bool IsChargedSpell => ChargeTurns > 0;

    /// <summary>
    /// Gets whether this spell is restricted to a specific archetype.
    /// </summary>
    public bool IsArchetypeRestricted => Archetype.HasValue;

    /// <summary>
    /// Gets the total cost as a combined AP + Flux value for comparison.
    /// </summary>
    public int TotalCost => ApCost + FluxCost;

    #endregion
}
