using RuneAndRust.Core.Enums;

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
/// <param name="PlayerAbilities">List of player abilities with hotkeys and status.</param>
public record CombatViewModel(
    int RoundNumber,
    string ActiveCombatantName,
    List<CombatantView> TurnOrder,
    List<string> CombatLog,
    PlayerStatsView PlayerStats,
    List<AbilityView>? PlayerAbilities = null,
    // Row-grouped combatants for grid display (v0.3.6a)
    List<CombatantView>? PlayerFrontRow = null,
    List<CombatantView>? PlayerBackRow = null,
    List<CombatantView>? EnemyFrontRow = null,
    List<CombatantView>? EnemyBackRow = null
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
    string InitiativeDisplay,
    // Row system (v0.3.6a)
    RowPosition Row = RowPosition.Front,
    bool IsTargeted = false
);

/// <summary>
/// Player statistics for the combat header display.
/// </summary>
/// <param name="CurrentHp">Player's current HP.</param>
/// <param name="MaxHp">Player's maximum HP.</param>
/// <param name="CurrentStamina">Player's current stamina.</param>
/// <param name="MaxStamina">Player's maximum stamina.</param>
/// <param name="CurrentStress">Player's current psychic stress (0-100).</param>
/// <param name="MaxStress">Player's maximum stress (always 100).</param>
/// <param name="CurrentCorruption">Player's current Runic Blight corruption (0-100).</param>
/// <param name="MaxCorruption">Player's maximum corruption (always 100).</param>
public record PlayerStatsView(
    int CurrentHp,
    int MaxHp,
    int CurrentStamina,
    int MaxStamina,
    int CurrentStress,
    int MaxStress,
    int CurrentCorruption,
    int MaxCorruption
);

/// <summary>
/// Display-ready ability information for combat UI rendering.
/// </summary>
/// <param name="Hotkey">The 1-based hotkey index for this ability.</param>
/// <param name="Name">Display name of the ability.</param>
/// <param name="CostDisplay">Formatted cost string (e.g., "35 STA" or "15 AP").</param>
/// <param name="CooldownRemaining">Turns remaining on cooldown, or 0 if ready.</param>
/// <param name="IsUsable">Whether the ability can currently be used.</param>
public record AbilityView(
    int Hotkey,
    string Name,
    string CostDisplay,
    int CooldownRemaining,
    bool IsUsable
);
