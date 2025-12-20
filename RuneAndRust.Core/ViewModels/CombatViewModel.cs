namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// Immutable snapshot of combat state for UI rendering.
/// Transforms raw CombatState into display-ready data.
/// </summary>
/// <param name="RoundNumber">The current combat round.</param>
/// <param name="ActiveCombatantName">Name of the combatant whose turn it is.</param>
/// <param name="TurnOrder">Ordered list of combatants with display information.</param>
/// <param name="CombatLog">Rolling buffer of recent combat events (max 10).</param>
/// <param name="PlayerStats">Player's HP and Stamina for header display.</param>
public record CombatViewModel(
    int RoundNumber,
    string ActiveCombatantName,
    List<CombatantView> TurnOrder,
    List<string> CombatLog,
    PlayerStatsView PlayerStats
);

/// <summary>
/// Display-ready combatant information for turn order rendering.
/// </summary>
/// <param name="Id">Unique identifier for the combatant.</param>
/// <param name="Name">Display name of the combatant.</param>
/// <param name="IsPlayer">Whether this combatant is the player character.</param>
/// <param name="IsActive">Whether it is currently this combatant's turn.</param>
/// <param name="HealthStatus">Health display: "75/100" for player, "Healthy"/"Wounded"/"Critical" for enemies.</param>
/// <param name="StatusEffects">Placeholder for status effect icons (future expansion).</param>
/// <param name="InitiativeDisplay">Initiative value as string for table display.</param>
public record CombatantView(
    Guid Id,
    string Name,
    bool IsPlayer,
    bool IsActive,
    string HealthStatus,
    string StatusEffects,
    string InitiativeDisplay
);

/// <summary>
/// Player statistics for the combat header display.
/// </summary>
/// <param name="CurrentHp">Player's current HP.</param>
/// <param name="MaxHp">Player's maximum HP.</param>
/// <param name="CurrentStamina">Player's current stamina.</param>
/// <param name="MaxStamina">Player's maximum stamina.</param>
public record PlayerStatsView(
    int CurrentHp,
    int MaxHp,
    int CurrentStamina,
    int MaxStamina
);
