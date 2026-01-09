namespace RuneAndRust.Application.Logging;

/// <summary>
/// Standard log message templates for consistent formatting across services.
/// Uses structured logging with semantic property names.
/// </summary>
public static class LogTemplates
{
    // Service lifecycle
    public const string ServiceInitialized = "{ServiceName} initialized";
    public const string ServiceInitializedWithCount = "{ServiceName} initialized with {Count} {ItemType}";

    // Player actions
    public const string PlayerAction = "Player {PlayerName} {Action}";
    public const string PlayerActionWithTarget = "Player {PlayerName} {Action} {Target}";
    public const string PlayerActionResult = "Player {PlayerName} {Action}: {Result}";

    // Combat
    public const string CombatInitiated = "Combat initiated: {PlayerName} vs {MonsterName}";
    public const string CombatDamage = "{Attacker} dealt {Damage} damage to {Defender}";
    public const string CombatResult = "Combat resolved: {Outcome}";

    // State changes
    public const string StateChanged = "{EntityType} {EntityId} state changed: {OldState} -> {NewState}";
    public const string ResourceChanged = "{ResourceType} changed: {OldValue} -> {NewValue}";

    // Configuration
    public const string ConfigurationLoaded = "Loaded {Count} {ItemType} from {Source}";
    public const string ConfigurationNotFound = "Configuration file not found: {FilePath}. Using defaults.";
    public const string ConfigurationParseError = "Failed to parse configuration: {FilePath}";

    // Errors
    public const string OperationFailed = "{Operation} failed: {Reason}";
    public const string EntityNotFound = "{EntityType} not found: {EntityId}";
    public const string ValidationFailed = "Validation failed for {EntityType}: {Errors}";

    // Repository operations
    public const string RepositorySave = "Saved {EntityType} with ID {EntityId}";
    public const string RepositoryLoad = "Loaded {EntityType} with ID {EntityId}";
    public const string RepositoryDelete = "Deleted {EntityType} with ID {EntityId}";
    public const string RepositoryNotFound = "{EntityType} with ID {EntityId} not found in repository";
}
