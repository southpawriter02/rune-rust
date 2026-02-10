// ═══════════════════════════════════════════════════════════════════════════════
// LivingRuneEntity.cs
// Immutable value object representing an animated rune entity summoned by the
// Rúnasmiðr specialization's Living Runes ability. These constructs fight
// alongside the summoner, attacking the nearest enemy each turn.
// Version: 0.20.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an animated rune entity summoned by the Rúnasmiðr's Living Runes ability.
/// </summary>
/// <remarks>
/// <para>
/// Living Runes is a Tier 3 ability that summons 2 animated rune constructs to
/// fight alongside the summoner. Each entity attacks the nearest enemy automatically
/// and disappears when its duration ends or it is destroyed.
/// </para>
/// <para>
/// Key mechanics:
/// </para>
/// <list type="bullet">
///   <item><description><b>HP:</b> 10 (fixed, cannot be healed)</description></item>
///   <item><description><b>Defense:</b> 12</description></item>
///   <item><description><b>Attack Bonus:</b> +4</description></item>
///   <item><description><b>Damage:</b> 1d8 per hit (average 4.5)</description></item>
///   <item><description><b>Movement:</b> 4 spaces per turn</description></item>
///   <item><description><b>Duration:</b> 5 turns</description></item>
///   <item><description><b>Behavior:</b> Aggressive — attacks nearest enemy</description></item>
///   <item><description><b>Limit:</b> 2 entities per summoning</description></item>
/// </list>
/// <para>
/// This is an immutable value object — methods like <see cref="TakeDamage"/>,
/// <see cref="Tick"/>, and <see cref="MoveTo"/> return new instances via
/// <c>with</c> expressions. The original instance is never modified.
/// </para>
/// <para>
/// <b>Cost:</b> 4 AP, 3 Rune Charges.
/// <b>Tier:</b> 3 (requires 16 PP invested in Rúnasmiðr tree).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var rune = LivingRuneEntity.Create(ownerId, (5, 3));
/// // rune.CurrentHp = 10, rune.TurnsRemaining = 5
///
/// var damaged = rune.TakeDamage(4);
/// // damaged.CurrentHp = 6, damaged.IsDestroyed = false
///
/// var ticked = damaged.Tick();
/// // ticked.TurnsRemaining = 4
/// </code>
/// </example>
/// <seealso cref="EmpoweredRune"/>
/// <seealso cref="RunicTrap"/>
/// <seealso cref="RuneAndRust.Domain.Enums.RunasmidrAbilityId"/>
public sealed record LivingRuneEntity
{
    // ═══════ Constants ═══════

    /// <summary>
    /// Default maximum HP for a Living Rune entity.
    /// </summary>
    public const int DefaultMaxHp = 10;

    /// <summary>
    /// Default Defense value for a Living Rune entity.
    /// </summary>
    public const int DefaultDefense = 12;

    /// <summary>
    /// Default Attack Bonus for a Living Rune entity.
    /// </summary>
    public const int DefaultAttackBonus = 4;

    /// <summary>
    /// Default damage dice expression for a Living Rune entity (1d8).
    /// </summary>
    public const string DefaultDamageDice = "1d8";

    /// <summary>
    /// Default movement speed in spaces per turn.
    /// </summary>
    public const int DefaultMovement = 4;

    /// <summary>
    /// Default duration for Living Rune entities, in turns.
    /// </summary>
    public const int DefaultDuration = 5;

    /// <summary>
    /// Number of damage dice rolled per attack.
    /// </summary>
    public const int DamageDiceCount = 1;

    /// <summary>
    /// Number of sides on the damage die.
    /// </summary>
    public const int DamageDiceSides = 8;

    /// <summary>
    /// Number of entities summoned per Living Runes activation.
    /// </summary>
    public const int SummonCount = 2;

    /// <summary>
    /// Average damage per attack (1d8 = 4.5).
    /// </summary>
    public const decimal AverageDamagePerDie = 4.5m;

    // ═══════ Properties ═══════

    /// <summary>
    /// Gets the unique identifier for this Living Rune entity instance.
    /// </summary>
    public Guid EntityId { get; init; }

    /// <summary>
    /// Gets the ID of the character who summoned this entity.
    /// </summary>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the current hit points of this entity.
    /// </summary>
    /// <remarks>
    /// Ranges from 0 to <see cref="MaxHp"/>. Living Runes cannot be healed —
    /// once damaged, HP only decreases.
    /// </remarks>
    public int CurrentHp { get; init; }

    /// <summary>
    /// Gets the maximum hit points (always 10).
    /// </summary>
    public int MaxHp { get; init; } = DefaultMaxHp;

    /// <summary>
    /// Gets the Defense value (always 12).
    /// </summary>
    public int Defense { get; init; } = DefaultDefense;

    /// <summary>
    /// Gets the Attack Bonus (always +4).
    /// </summary>
    public int AttackBonus { get; init; } = DefaultAttackBonus;

    /// <summary>
    /// Gets the damage dice expression (always "1d8").
    /// </summary>
    public string DamageDice { get; init; } = DefaultDamageDice;

    /// <summary>
    /// Gets the movement speed in spaces per turn (always 4).
    /// </summary>
    public int Movement { get; init; } = DefaultMovement;

    /// <summary>
    /// Gets the grid position of this entity (X, Y coordinates).
    /// </summary>
    public (int X, int Y) Position { get; init; }

    /// <summary>
    /// Gets the remaining turns before this entity despawns naturally.
    /// </summary>
    public int TurnsRemaining { get; init; }

    /// <summary>
    /// Gets the original (maximum) duration in turns.
    /// </summary>
    public int OriginalDuration { get; init; }

    /// <summary>
    /// Gets the timestamp when this entity was summoned.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets whether this entity is still active in combat.
    /// </summary>
    /// <remarks>
    /// An entity becomes inactive when it is destroyed (HP ≤ 0)
    /// or when its duration expires (TurnsRemaining ≤ 0).
    /// </remarks>
    public bool IsActive { get; init; }

    // ═══════ Computed Properties ═══════

    /// <summary>
    /// Gets whether this entity has been destroyed (HP ≤ 0).
    /// </summary>
    public bool IsDestroyed => CurrentHp <= 0;

    /// <summary>
    /// Gets whether this entity has expired naturally (duration ended, but not destroyed).
    /// </summary>
    public bool IsExpired => TurnsRemaining <= 0 && !IsDestroyed;

    // ═══════ Factory Methods ═══════

    /// <summary>
    /// Creates a new Living Rune entity at the specified position with default stats.
    /// </summary>
    /// <param name="ownerId">ID of the character summoning this entity.</param>
    /// <param name="position">Grid position (X, Y) for the entity.</param>
    /// <returns>A new LivingRuneEntity instance with default stats and full duration.</returns>
    public static LivingRuneEntity Create(Guid ownerId, (int X, int Y) position)
    {
        return new LivingRuneEntity
        {
            EntityId = Guid.NewGuid(),
            OwnerId = ownerId,
            CurrentHp = DefaultMaxHp,
            MaxHp = DefaultMaxHp,
            Defense = DefaultDefense,
            AttackBonus = DefaultAttackBonus,
            DamageDice = DefaultDamageDice,
            Movement = DefaultMovement,
            Position = position,
            TurnsRemaining = DefaultDuration,
            OriginalDuration = DefaultDuration,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    // ═══════ State Transition Methods ═══════

    /// <summary>
    /// Applies damage to this Living Rune entity.
    /// </summary>
    /// <param name="damage">Amount of damage to apply. Must be non-negative.</param>
    /// <returns>
    /// A new instance with reduced HP. If HP reaches 0, <see cref="IsDestroyed"/>
    /// returns <c>true</c> and <see cref="IsActive"/> is set to <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="damage"/> is negative.
    /// </exception>
    public LivingRuneEntity TakeDamage(int damage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(damage);

        var newHp = Math.Max(0, CurrentHp - damage);
        return this with
        {
            CurrentHp = newHp,
            IsActive = newHp > 0 && TurnsRemaining > 0
        };
    }

    /// <summary>
    /// Advances this entity by one turn, decrementing the remaining duration.
    /// </summary>
    /// <returns>
    /// A new instance with decremented duration (minimum 0). If duration reaches 0,
    /// <see cref="IsActive"/> is set to <c>false</c>.
    /// </returns>
    public LivingRuneEntity Tick()
    {
        var newTurns = Math.Max(0, TurnsRemaining - 1);
        return this with
        {
            TurnsRemaining = newTurns,
            IsActive = CurrentHp > 0 && newTurns > 0
        };
    }

    /// <summary>
    /// Moves this entity to a new grid position.
    /// </summary>
    /// <param name="newPosition">The target grid position (X, Y).</param>
    /// <returns>A new instance at the updated position.</returns>
    public LivingRuneEntity MoveTo((int X, int Y) newPosition)
    {
        return this with { Position = newPosition };
    }

    /// <summary>
    /// Rolls attack damage for this entity (1d8).
    /// </summary>
    /// <param name="random">
    /// Random instance for dice rolling. If null, uses <see cref="Random.Shared"/>.
    /// </param>
    /// <returns>The damage rolled (1–8).</returns>
    public int RollAttackDamage(Random? random = null)
    {
        var rng = random ?? Random.Shared;
        var totalDamage = 0;
        for (var i = 0; i < DamageDiceCount; i++)
        {
            totalDamage += rng.Next(1, DamageDiceSides + 1);
        }

        return totalDamage;
    }

    // ═══════ Display Methods ═══════

    /// <summary>
    /// Gets the average damage per attack (1d8 = 4.5 average).
    /// </summary>
    /// <returns>The average damage as a decimal value.</returns>
    public decimal GetAverageDamage() => AverageDamagePerDie;

    /// <summary>
    /// Gets a human-readable status display for combat UI.
    /// </summary>
    /// <returns>
    /// A formatted string such as "Living Rune █████░░░░░ 5/10 HP (3 turns)".
    /// </returns>
    public string GetStatusDisplay()
    {
        var hpBar = GetHpBar();
        var status = IsDestroyed ? "DESTROYED" : !IsActive ? "EXPIRED" : "ACTIVE";
        return $"Living Rune [{status}] {hpBar} {CurrentHp}/{MaxHp} HP ({TurnsRemaining} turns)";
    }

    /// <summary>
    /// Returns a human-readable representation of this Living Rune entity.
    /// </summary>
    public override string ToString()
    {
        var status = IsDestroyed ? "DESTROYED" : !IsActive ? "EXPIRED" : "ACTIVE";
        return $"Living Rune [{status}] at ({Position.X}, {Position.Y}): " +
               $"{CurrentHp}/{MaxHp} HP, +{AttackBonus} ATK, {DamageDice} DMG, " +
               $"DEF {Defense} ({TurnsRemaining}/{OriginalDuration} turns)";
    }

    // ═══════ Private Helpers ═══════

    /// <summary>
    /// Generates a visual HP bar using block characters.
    /// </summary>
    private string GetHpBar(int width = 10)
    {
        var filled = (int)Math.Ceiling((double)CurrentHp / MaxHp * width);
        return new string('█', filled) + new string('░', width - filled);
    }
}
