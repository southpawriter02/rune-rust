using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Wraps a Player or Monster with combat-specific state and initiative tracking.
/// </summary>
/// <remarks>
/// <para>Combatant serves as a unified interface for participants in combat,
/// abstracting whether the participant is a player or monster.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
/// <item>Track initiative for turn order</item>
/// <item>Track "has acted this round" state</item>
/// <item>Provide numbered display names for duplicate monsters</item>
/// <item>Delegate health checks to underlying entity</item>
/// </list>
/// </remarks>
public class Combatant : IEntity
{
    // ===== Properties =====

    /// <summary>
    /// Gets the unique identifier for this combatant instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the underlying Player entity, or null if this combatant is a monster.
    /// </summary>
    public Player? Player { get; private set; }

    /// <summary>
    /// Gets the underlying Monster entity, or null if this combatant is a player.
    /// </summary>
    public Monster? Monster { get; private set; }

    /// <summary>
    /// Gets the combatant's initiative roll result.
    /// </summary>
    public InitiativeRoll InitiativeRoll { get; private set; }

    /// <summary>
    /// Gets the Finesse value used for initiative tie-breaking.
    /// </summary>
    /// <remarks>
    /// For players, this is <see cref="PlayerAttributes.Finesse"/>.
    /// For monsters, this is <see cref="Monster.InitiativeModifier"/>.
    /// </remarks>
    public int Finesse { get; private set; }

    /// <summary>
    /// Gets the display name for this combatant.
    /// </summary>
    /// <remarks>
    /// For players: Player.Name (e.g., "Hero")
    /// For monsters: Monster.Name with number if duplicates (e.g., "Goblin 2")
    /// </remarks>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the display number for this combatant (1-based, for monsters only).
    /// </summary>
    /// <remarks>
    /// Null for players. For monsters, only set when there are multiple of the same type.
    /// A value of 0 indicates a unique monster type (no numbering needed).
    /// </remarks>
    public int? DisplayNumber { get; private set; }

    /// <summary>
    /// Gets or sets whether this combatant has acted this round.
    /// </summary>
    public bool HasActedThisRound { get; private set; }

    /// <summary>
    /// Gets whether this combatant has their reaction available for the current round.
    /// </summary>
    /// <remarks>
    /// <para>Reactions are consumed when using certain defensive actions like
    /// <see cref="Enums.DefenseActionType.Dodge"/> or <see cref="Enums.DefenseActionType.Parry"/>.</para>
    /// <para>Reactions are reset at the start of the combatant's turn via <see cref="ResetReaction"/>.</para>
    /// </remarks>
    public bool HasReaction { get; private set; } = true;

    // ===== Computed Properties =====

    /// <summary>
    /// Gets the total initiative value used for turn order sorting.
    /// </summary>
    public int Initiative => InitiativeRoll.Total;

    /// <summary>
    /// Gets whether this combatant is still active in combat (not defeated/fled).
    /// </summary>
    public bool IsActive => IsPlayer ? !Player!.IsDead : !Monster!.IsDefeated;

    /// <summary>
    /// Gets whether this combatant is the player.
    /// </summary>
    public bool IsPlayer => Player != null;

    /// <summary>
    /// Gets whether this combatant is a monster.
    /// </summary>
    public bool IsMonster => Monster != null;

    /// <summary>
    /// Gets the current health of the combatant.
    /// </summary>
    public int CurrentHealth => IsPlayer ? Player!.Health : Monster!.Health;

    /// <summary>
    /// Gets the maximum health of the combatant.
    /// </summary>
    public int MaxHealth => IsPlayer ? Player!.Stats.MaxHealth : Monster!.Stats.MaxHealth;

    /// <summary>
    /// Gets the health percentage (0.0 to 1.0) for display purposes.
    /// </summary>
    public float HealthPercentage => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

    // ===== AI Behavior Properties (v0.0.6b) =====

    /// <summary>
    /// Gets the AI behavior of this combatant (for monsters).
    /// </summary>
    /// <remarks>
    /// Returns null for player combatants.
    /// </remarks>
    public AIBehavior? Behavior => Monster?.Behavior;

    /// <summary>
    /// Gets whether this combatant can heal.
    /// </summary>
    public bool CanHeal => Monster?.CanHeal ?? false;

    /// <summary>
    /// Gets the heal amount for this combatant.
    /// </summary>
    public int? HealAmount => Monster?.HealAmount;

    /// <summary>
    /// Gets or sets whether this combatant is defending (damage reduction active).
    /// </summary>
    /// <remarks>
    /// When defending, incoming damage is reduced by 50%.
    /// Defending flag resets at the start of the combatant's next turn.
    /// </remarks>
    public bool IsDefending { get; private set; }

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Combatant() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a combatant for a player.
    /// </summary>
    /// <param name="player">The player entity to wrap.</param>
    /// <param name="initiative">The rolled initiative result.</param>
    /// <returns>A new combatant representing the player.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    public static Combatant ForPlayer(Player player, InitiativeRoll initiative)
    {
        ArgumentNullException.ThrowIfNull(player);

        return new Combatant
        {
            Id = Guid.NewGuid(),
            Player = player,
            Monster = null,
            InitiativeRoll = initiative,
            Finesse = player.Attributes.Finesse,
            DisplayName = player.Name,
            DisplayNumber = null,
            HasActedThisRound = false
        };
    }

    /// <summary>
    /// Creates a combatant for a monster.
    /// </summary>
    /// <param name="monster">The monster entity to wrap.</param>
    /// <param name="initiative">The rolled initiative result.</param>
    /// <param name="displayNumber">The display number for duplicate monsters (0 if unique).</param>
    /// <returns>A new combatant representing the monster.</returns>
    /// <exception cref="ArgumentNullException">Thrown when monster is null.</exception>
    /// <remarks>
    /// If displayNumber is 0, the monster name is shown without a number.
    /// If displayNumber is > 0, the name includes the number (e.g., "Goblin 2").
    /// </remarks>
    public static Combatant ForMonster(Monster monster, InitiativeRoll initiative, int displayNumber)
    {
        ArgumentNullException.ThrowIfNull(monster);

        var baseName = monster.Name;
        var displayName = displayNumber > 0 ? $"{baseName} {displayNumber}" : baseName;

        return new Combatant
        {
            Id = Guid.NewGuid(),
            Player = null,
            Monster = monster,
            InitiativeRoll = initiative,
            Finesse = monster.InitiativeModifier,
            DisplayName = displayName,
            DisplayNumber = displayNumber > 0 ? displayNumber : null,
            HasActedThisRound = false
        };
    }

    // ===== Methods =====

    /// <summary>
    /// Marks this combatant as having acted this round.
    /// </summary>
    /// <remarks>
    /// Called after a combatant completes their turn action.
    /// Reset at the start of each new round.
    /// </remarks>
    public void MarkActed()
    {
        HasActedThisRound = true;
    }

    /// <summary>
    /// Resets the acted flag for a new round.
    /// </summary>
    /// <remarks>
    /// Called by <see cref="CombatEncounter"/> at the start of each new round.
    /// </remarks>
    public void ResetActed()
    {
        HasActedThisRound = false;
    }

    /// <summary>
    /// Sets the defending state for this combatant.
    /// </summary>
    /// <param name="defending">True to enable defending, false to disable.</param>
    public void SetDefending(bool defending)
    {
        IsDefending = defending;
    }

    /// <summary>
    /// Clears temporary combat states (called at start of turn).
    /// </summary>
    /// <remarks>
    /// Resets defending state and restores the combatant's reaction.
    /// </remarks>
    public void ResetTurnState()
    {
        IsDefending = false;
        ResetReaction();
    }

    /// <summary>
    /// Consumes the combatant's reaction for defensive actions.
    /// </summary>
    /// <remarks>
    /// <para>Called when using reaction-based defensive actions such as
    /// <see cref="Enums.DefenseActionType.Dodge"/> or <see cref="Enums.DefenseActionType.Parry"/>.</para>
    /// <para>Once consumed, the reaction cannot be used again until reset at turn start.</para>
    /// </remarks>
    public void UseReaction()
    {
        HasReaction = false;
    }

    /// <summary>
    /// Resets the combatant's reaction to available.
    /// </summary>
    /// <remarks>
    /// Called at the start of the combatant's turn via <see cref="ResetTurnState"/>.
    /// </remarks>
    public void ResetReaction()
    {
        HasReaction = true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var status = IsActive ? $"HP: {CurrentHealth}/{MaxHealth}" : "Defeated";
        return $"{DisplayName} (Init: {Initiative}, {status})";
    }
}
