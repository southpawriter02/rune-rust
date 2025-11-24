namespace RuneAndRust.Core;

/// <summary>
/// Unified wrapper for combat units (PlayerCharacter, Enemy, Companion).
/// Provides common interface for HP, position, status effects, and combat properties.
/// Used by v0.43.6+ Desktop UI for rendering HP bars, status effects, and animations.
/// </summary>
public class Combatant
{
    /// <summary>
    /// The underlying character object (PlayerCharacter, Enemy, or Companion).
    /// </summary>
    public object Character { get; set; }

    /// <summary>
    /// Current position on the tactical grid.
    /// </summary>
    public GridPosition? CurrentPosition
    {
        get => Character switch
        {
            PlayerCharacter pc => GetPlayerPosition(pc),
            Enemy e => e.Position,
            Companion c => c.Position,
            _ => null
        };
        set
        {
            switch (Character)
            {
                case PlayerCharacter pc:
                    SetPlayerPosition(pc, value);
                    break;
                case Enemy e:
                    e.Position = value;
                    break;
                case Companion c:
                    c.Position = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Active status effects on this combatant.
    /// </summary>
    public List<StatusEffect> ActiveEffects { get; set; } = new();

    /// <summary>
    /// Current hit points.
    /// </summary>
    public int CurrentHP => Character switch
    {
        PlayerCharacter pc => pc.HP,
        Enemy e => e.HP,
        Companion c => c.HP,
        _ => 0
    };

    /// <summary>
    /// Maximum hit points.
    /// </summary>
    public int MaxHP => Character switch
    {
        PlayerCharacter pc => pc.MaxHP,
        Enemy e => e.MaxHP,
        Companion c => c.MaxHP,
        _ => 0
    };

    /// <summary>
    /// Whether this combatant is alive.
    /// </summary>
    public bool IsAlive => CurrentHP > 0;

    /// <summary>
    /// Character name for display.
    /// </summary>
    public string Name => Character switch
    {
        PlayerCharacter pc => pc.Name,
        Enemy e => e.Name,
        Companion c => c.Name,
        _ => "Unknown"
    };

    /// <summary>
    /// Whether this is the player character.
    /// </summary>
    public bool IsPlayer => Character is PlayerCharacter;

    /// <summary>
    /// Whether this is a companion.
    /// </summary>
    public bool IsCompanion => Character is Companion;

    /// <summary>
    /// Whether this is an enemy.
    /// </summary>
    public bool IsEnemy => Character is Enemy;

    // Constructors
    public Combatant(PlayerCharacter player)
    {
        Character = player ?? throw new ArgumentNullException(nameof(player));
    }

    public Combatant(Enemy enemy)
    {
        Character = enemy ?? throw new ArgumentNullException(nameof(enemy));
    }

    public Combatant(Companion companion)
    {
        Character = companion ?? throw new ArgumentNullException(nameof(companion));
    }

    // Helper methods for PlayerCharacter position (stored externally in CombatState)
    // These will be set by CombatState/CombatEngine
    private static GridPosition? _playerPosition = null;

    private static GridPosition? GetPlayerPosition(PlayerCharacter pc)
    {
        // Position is tracked in CombatState for player
        // This is a workaround - ideally PlayerCharacter should have Position property
        return _playerPosition;
    }

    private static void SetPlayerPosition(PlayerCharacter pc, GridPosition? position)
    {
        _playerPosition = position;
    }

    /// <summary>
    /// Static method to set player position (called by CombatState/CombatEngine).
    /// </summary>
    public static void SetPlayerPositionStatic(GridPosition? position)
    {
        _playerPosition = position;
    }

    public override string ToString()
    {
        return $"{Name} ({CurrentHP}/{MaxHP} HP) at {CurrentPosition}";
    }
}
