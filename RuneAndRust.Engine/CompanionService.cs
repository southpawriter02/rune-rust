using RuneAndRust.Core;
using RuneAndRust.Engine.Integration;
using Microsoft.Data.Sqlite;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.34.4: Companion Service - Primary orchestration layer
/// Integrates AI, recruitment, progression, and combat mechanics
/// Handles combat turns, System Crash mechanics, and direct commands
/// v0.35: Territory integration for companion reactions
/// </summary>
public class CompanionService
{
    private static readonly ILogger _log = Log.ForContext<CompanionService>();
    private readonly string _connectionString;
    private readonly CompanionAIService _aiService;
    private readonly RecruitmentService _recruitmentService;
    private readonly CompanionProgressionService _progressionService;
    private readonly CompanionTerritoryReactions? _territoryReactions; // v0.35

    // System Crash mechanics
    private const int SYSTEM_CRASH_PSYCHIC_STRESS = 10;
    private const double AFTER_COMBAT_RECOVERY_PERCENTAGE = 0.5; // 50% HP recovery

    public CompanionService(string connectionString, CompanionTerritoryReactions? territoryReactions = null)
    {
        _connectionString = connectionString;
        var coverService = new CoverService();
        var flankingService = new FlankingService();
        _aiService = new CompanionAIService(coverService, flankingService, Log.ForContext<CompanionAIService>());
        _recruitmentService = new RecruitmentService(connectionString);
        _progressionService = new CompanionProgressionService(connectionString);
        _territoryReactions = territoryReactions; // v0.35
    }

    // ============================================
    // COMBAT PROCESSING
    // ============================================

    /// <summary>
    /// Process a companion's turn in combat
    /// Called by CombatEngine during turn order processing
    /// </summary>
    public CompanionAction ProcessCompanionTurn(
        Companion companion,
        PlayerCharacter player,
        List<Enemy> enemies,
        BattlefieldGrid? grid = null)
    {
        _log.Information("Processing turn: CompanionId={CompanionId}, Stance={Stance}, HP={HP}/{MaxHP}",
            companion.CompanionID, companion.CurrentStance, companion.CurrentHitPoints, companion.MaxHitPoints);

        // Check if incapacitated (System Crash)
        if (companion.IsIncapacitated)
        {
            _log.Debug("Companion incapacitated, skipping turn: {CompanionName}", companion.DisplayName);
            return new CompanionAction
            {
                ActionType = "Wait",
                Reason = $"{companion.DisplayName} is incapacitated (System Crash)"
            };
        }

        // Passive stance with no explicit command: Skip turn
        if (companion.CurrentStance.Equals("passive", StringComparison.OrdinalIgnoreCase))
        {
            _log.Debug("Companion in Passive stance, waiting for commands: {CompanionName}", companion.DisplayName);
            return new CompanionAction
            {
                ActionType = "Wait",
                Reason = $"{companion.DisplayName} awaits orders (Passive stance)"
            };
        }

        // Select action via AI
        var action = _aiService.SelectAction(companion, player, enemies, grid);

        _log.Information("Companion action selected: {CompanionName} -> {ActionType} ({Reason})",
            companion.DisplayName, action.ActionType, action.Reason);

        return action;
    }

    /// <summary>
    /// Execute a companion's selected action
    /// Returns true if action succeeded, false otherwise
    /// </summary>
    public bool ExecuteCompanionAction(
        Companion companion,
        CompanionAction action,
        List<Enemy> enemies,
        PlayerCharacter player,
        BattlefieldGrid? grid = null)
    {
        _log.Debug("Executing action: {CompanionName} -> {ActionType}", companion.DisplayName, action.ActionType);

        switch (action.ActionType.ToLower())
        {
            case "attack":
                if (action.TargetEnemy == null)
                {
                    _log.Warning("Attack action has no target enemy");
                    return false;
                }
                return ExecuteAttack(companion, action.TargetEnemy);

            case "useability":
                if (action.AbilityName == null)
                {
                    _log.Warning("UseAbility action has no ability name");
                    return false;
                }
                return ExecuteAbility(companion, action.AbilityName, action.TargetEnemy, action.TargetSelf, player, enemies);

            case "move":
                if (action.TargetPosition == null || grid == null)
                {
                    _log.Warning("Move action has no target position or grid");
                    return false;
                }
                return ExecuteMove(companion, action.TargetPosition.Value, grid);

            case "wait":
                _log.Debug("Companion waiting: {CompanionName}", companion.DisplayName);
                return true;

            default:
                _log.Warning("Unknown action type: {ActionType}", action.ActionType);
                return false;
        }
    }

    /// <summary>
    /// Apply damage to a companion
    /// Triggers System Crash if HP reaches 0
    /// </summary>
    public void ApplyCompanionDamage(Companion companion, int damage, PlayerCharacter player)
    {
        var oldHP = companion.CurrentHitPoints;
        companion.CurrentHitPoints = Math.Max(0, companion.CurrentHitPoints - damage);

        _log.Information("Companion damaged: {CompanionName} took {Damage} damage ({OldHP} -> {NewHP})",
            companion.DisplayName, damage, oldHP, companion.CurrentHitPoints);

        // Check for System Crash
        if (companion.CurrentHitPoints == 0 && !companion.IsIncapacitated)
        {
            HandleSystemCrash(companion, player);
        }

        // Update database
        UpdateCompanionHP(companion);
    }

    // ============================================
    // SYSTEM CRASH & RECOVERY
    // ============================================

    /// <summary>
    /// System Crash: Companion reaches 0 HP
    /// - Mark incapacitated
    /// - Apply +10 Psychic Stress to player (Trauma Economy integration)
    /// - Remove from combat grid
    /// </summary>
    public void HandleSystemCrash(Companion companion, PlayerCharacter player)
    {
        companion.IsIncapacitated = true;
        companion.CurrentHitPoints = 0;

        // Apply Psychic Stress to player
        player.PsychicStress += SYSTEM_CRASH_PSYCHIC_STRESS;

        _log.Warning("SYSTEM CRASH: {CompanionName} incapacitated. Player +{Stress} Psychic Stress (now {Total})",
            companion.DisplayName, SYSTEM_CRASH_PSYCHIC_STRESS, player.PsychicStress);

        // Update database
        UpdateCompanionHP(companion);
    }

    /// <summary>
    /// After-combat recovery: Restore companions to 50% HP
    /// Called after combat ends successfully
    /// </summary>
    public void RecoverCompanion(Companion companion, int characterId)
    {
        if (!companion.IsIncapacitated)
        {
            _log.Debug("Companion not incapacitated, skipping recovery: {CompanionName}", companion.DisplayName);
            return;
        }

        var recoveredHP = (int)(companion.MaxHitPoints * AFTER_COMBAT_RECOVERY_PERCENTAGE);
        companion.CurrentHitPoints = recoveredHP;
        companion.IsIncapacitated = false;

        // Full resource recovery
        companion.CurrentStamina = companion.MaxStamina;
        companion.CurrentAetherPool = companion.MaxAetherPool;

        _log.Information("Companion recovered: {CompanionName} restored to {HP} HP (50% of max)",
            companion.DisplayName, recoveredHP);

        // Update database
        UpdateCompanionHP(companion);
        UpdateCompanionResources(companion, characterId);
    }

    /// <summary>
    /// Mid-dungeon revival: Revive companion with healing ability
    /// Used by Bone-Setter abilities like "Field Repair"
    /// </summary>
    public void ReviveCompanion(Companion companion, int healAmount, int characterId, BattlefieldGrid? grid = null, GridPosition? position = null)
    {
        if (!companion.IsIncapacitated)
        {
            _log.Debug("Companion not incapacitated, cannot revive: {CompanionName}", companion.DisplayName);
            return;
        }

        companion.IsIncapacitated = false;
        companion.CurrentHitPoints = Math.Min(healAmount, companion.MaxHitPoints);

        _log.Information("Companion revived: {CompanionName} restored to {HP} HP",
            companion.DisplayName, companion.CurrentHitPoints);

        // Return to combat grid if position provided
        if (grid != null && position != null)
        {
            companion.Position = position;
            _log.Debug("Companion returned to grid at {Position}", position);
        }

        // Update database
        UpdateCompanionHP(companion);
    }

    /// <summary>
    /// Sanctuary recovery: Full HP/Stamina/Aether restore
    /// Called when player rests at a Sanctuary
    /// </summary>
    public void SanctuaryRecovery(Companion companion, int characterId)
    {
        companion.CurrentHitPoints = companion.MaxHitPoints;
        companion.CurrentStamina = companion.MaxStamina;
        companion.CurrentAetherPool = companion.MaxAetherPool;
        companion.IsIncapacitated = false;

        _log.Information("Companion fully recovered at Sanctuary: {CompanionName}", companion.DisplayName);

        // Update database
        UpdateCompanionHP(companion);
        UpdateCompanionResources(companion, characterId);
    }

    // ============================================
    // DIRECT COMMAND INTEGRATION
    // ============================================

    /// <summary>
    /// Execute a direct command to a companion
    /// Called by CompanionCommands parser
    /// </summary>
    public CompanionAction CommandCompanion(
        Companion companion,
        string abilityName,
        Enemy? targetEnemy,
        List<Enemy> allEnemies,
        PlayerCharacter player)
    {
        _log.Information("Direct command: {CompanionName} -> {AbilityName} on {Target}",
            companion.DisplayName, abilityName, targetEnemy?.Name ?? "self");

        // Find ability by name (fuzzy match)
        var ability = companion.Abilities.FirstOrDefault(a =>
            a.AbilityName.Equals(abilityName, StringComparison.OrdinalIgnoreCase) ||
            a.AbilityName.Replace(" ", "").Equals(abilityName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));

        if (ability == null)
        {
            _log.Warning("Ability not found: {AbilityName} for {CompanionName}",
                abilityName, companion.DisplayName);
            return new CompanionAction
            {
                ActionType = "Wait",
                Reason = $"Unknown ability: {abilityName}"
            };
        }

        // Check resource cost
        if (!CanAffordAbility(companion, ability))
        {
            _log.Warning("Insufficient resources: {CompanionName} cannot afford {AbilityName}",
                companion.DisplayName, abilityName);
            return new CompanionAction
            {
                ActionType = "Wait",
                Reason = $"Insufficient {ability.ResourceCostType} for {abilityName}"
            };
        }

        // Return action
        return new CompanionAction
        {
            ActionType = "UseAbility",
            AbilityName = ability.AbilityName,
            TargetEnemy = targetEnemy,
            TargetSelf = (targetEnemy == null && ability.TargetType == "self"),
            Reason = $"Direct command: {abilityName}"
        };
    }

    /// <summary>
    /// Change companion AI stance
    /// </summary>
    public bool ChangeStance(Companion companion, string newStance, int characterId)
    {
        var validStances = new[] { "aggressive", "defensive", "passive" };
        if (!validStances.Contains(newStance.ToLower()))
        {
            _log.Warning("Invalid stance: {Stance}", newStance);
            return false;
        }

        var oldStance = companion.CurrentStance;
        companion.CurrentStance = newStance.ToLower();

        _log.Information("Stance changed: {CompanionName} {OldStance} -> {NewStance}",
            companion.DisplayName, oldStance, newStance);

        // Update database
        UpdateCompanionStance(companion, characterId);
        return true;
    }

    // ============================================
    // PARTY MANAGEMENT
    // ============================================

    /// <summary>
    /// Get all active party companions for a character
    /// </summary>
    public List<Companion> GetPartyCompanions(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT c.companion_id, c.companion_name, c.display_name, c.archetype, c.combat_role,
                   c.base_might, c.base_finesse, c.base_sturdiness, c.base_wits, c.base_will,
                   c.base_max_hp, c.base_defense, c.base_soak, c.resource_type, c.base_max_resource,
                   c.default_stance, c.starting_abilities,
                   cc.current_hp, cc.current_resource, cc.current_stance
            FROM Companions c
            INNER JOIN Characters_Companions cc ON c.companion_id = cc.companion_id
            WHERE cc.character_id = @charId AND cc.is_in_party = 1
        ";
        command.Parameters.AddWithValue("@charId", characterId);

        var companions = new List<Companion>();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var companion = new Companion
            {
                CompanionID = reader.GetInt32(0),
                CompanionName = reader.GetString(1),
                DisplayName = reader.GetString(2),
                Archetype = reader.GetString(3),
                CombatRole = reader.GetString(4),
                BaseAttributes = new Attributes
                {
                    Might = reader.GetInt32(5),
                    Finesse = reader.GetInt32(6),
                    Sturdiness = reader.GetInt32(7),
                    Wits = reader.GetInt32(8),
                    Will = reader.GetInt32(9)
                },
                MaxHitPoints = reader.GetInt32(10), // Base stats - will be scaled
                Defense = reader.GetInt32(11),
                Soak = reader.GetInt32(12),
                ResourceType = reader.GetString(13),
                MaxStamina = reader.GetString(13) == "Stamina" ? reader.GetInt32(14) : 0,
                MaxAetherPool = reader.GetString(13) == "Aether Pool" ? reader.GetInt32(14) : 0,
                CurrentStance = reader.GetString(15),
                CurrentHitPoints = reader.GetInt32(17),
                CurrentStamina = reader.GetString(13) == "Stamina" ? reader.GetInt32(18) : 0,
                CurrentAetherPool = reader.GetString(13) == "Aether Pool" ? reader.GetInt32(18) : 0
            };

            // Load abilities
            companion.Abilities = LoadCompanionAbilities(companion.CompanionID, characterId, connection);

            // Apply stat scaling
            var scaledStats = _progressionService.CalculateScaledStats(characterId, companion.CompanionID);
            ApplyScaledStats(companion, scaledStats);

            companions.Add(companion);
        }

        _log.Debug("Retrieved {Count} active party companions for CharacterId={CharacterId}",
            companions.Count, characterId);

        return companions;
    }

    /// <summary>
    /// Get a single companion by ID (for command parsing)
    /// </summary>
    public Companion? GetCompanionById(int characterId, int companionId)
    {
        var companions = GetPartyCompanions(characterId);
        return companions.FirstOrDefault(c => c.CompanionID == companionId);
    }

    /// <summary>
    /// Get companion by name (fuzzy match for command parsing)
    /// </summary>
    public Companion? GetCompanionByName(int characterId, string name)
    {
        var companions = GetPartyCompanions(characterId);

        // Exact match on display name
        var exact = companions.FirstOrDefault(c =>
            c.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;

        // Partial match (e.g., "Kara" matches "Kara Ironbreaker")
        var partial = companions.FirstOrDefault(c =>
            c.DisplayName.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (partial != null) return partial;

        // Match on companion_name (ASCII)
        return companions.FirstOrDefault(c =>
            c.CompanionName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    // ============================================
    // COMBAT ACTION EXECUTION (PRIVATE HELPERS)
    // ============================================

    private bool ExecuteAttack(Companion companion, Enemy target)
    {
        // Basic attack damage: 1d6 + attribute bonus
        var damageRoll = new Random().Next(1, 7);
        var attributeBonus = companion.BaseMight; // Simplified - would use GetAttributeValue
        var totalDamage = damageRoll + (attributeBonus / 2); // Attribute modifier

        target.HP = Math.Max(0, target.HP - totalDamage);

        _log.Information("Companion attack: {CompanionName} hit {Target} for {Damage} damage ({TargetHP} HP remaining)",
            companion.DisplayName, target.Name, totalDamage, target.HP);

        return true;
    }

    private bool ExecuteAbility(
        Companion companion,
        string abilityName,
        Enemy? targetEnemy,
        bool targetSelf,
        PlayerCharacter player,
        List<Enemy> enemies)
    {
        var ability = companion.Abilities.FirstOrDefault(a =>
            a.AbilityName.Equals(abilityName, StringComparison.OrdinalIgnoreCase));

        if (ability == null)
        {
            _log.Warning("Ability not found: {AbilityName}", abilityName);
            return false;
        }

        // Check and consume resources
        if (!ConsumeAbilityResources(companion, ability))
        {
            _log.Warning("Failed to consume resources for ability: {AbilityName}", abilityName);
            return false;
        }

        _log.Information("Companion ability: {CompanionName} used {AbilityName} (cost: {Cost} {ResourceType})",
            companion.DisplayName, abilityName, ability.ResourceCost, ability.ResourceCostType);

        // Ability execution would be handled by combat engine
        // This is a placeholder for the orchestration layer
        return true;
    }

    private bool ExecuteMove(Companion companion, GridPosition targetPosition, BattlefieldGrid grid)
    {
        var oldPosition = companion.Position;
        companion.Position = targetPosition;

        _log.Information("Companion moved: {CompanionName} from {OldPos} to {NewPos}",
            companion.DisplayName, oldPosition, targetPosition);

        return true;
    }

    private bool CanAffordAbility(Companion companion, CompanionAbility ability)
    {
        if (ability.ResourceCostType == "Stamina")
        {
            return companion.CurrentStamina >= ability.ResourceCost;
        }
        else if (ability.ResourceCostType == "Aether Pool")
        {
            return companion.CurrentAetherPool >= ability.ResourceCost;
        }
        return true; // No resource cost
    }

    private bool ConsumeAbilityResources(Companion companion, CompanionAbility ability)
    {
        if (!CanAffordAbility(companion, ability))
        {
            return false;
        }

        if (ability.ResourceCostType == "Stamina")
        {
            companion.CurrentStamina -= ability.ResourceCost;
        }
        else if (ability.ResourceCostType == "Aether Pool")
        {
            companion.CurrentAetherPool -= ability.ResourceCost;
        }

        return true;
    }

    // ============================================
    // DATABASE HELPERS
    // ============================================

    private void UpdateCompanionHP(Companion companion)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters_Companions
            SET current_hp = @hp, updated_at = @updatedAt
            WHERE companion_id = @companionId
        ";
        command.Parameters.AddWithValue("@hp", companion.CurrentHitPoints);
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@companionId", companion.CompanionID);
        command.ExecuteNonQuery();
    }

    private void UpdateCompanionResources(Companion companion, int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var resource = companion.ResourceType == "Stamina" ? companion.CurrentStamina : companion.CurrentAetherPool;

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters_Companions
            SET current_resource = @resource, updated_at = @updatedAt
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        command.Parameters.AddWithValue("@resource", resource);
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companion.CompanionID);
        command.ExecuteNonQuery();
    }

    private void UpdateCompanionStance(Companion companion, int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters_Companions
            SET current_stance = @stance, updated_at = @updatedAt
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        command.Parameters.AddWithValue("@stance", companion.CurrentStance);
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companion.CompanionID);
        command.ExecuteNonQuery();
    }

    private List<CompanionAbility> LoadCompanionAbilities(int companionId, int characterId, SqliteConnection connection)
    {
        // Get unlocked abilities from Companion_Progression
        var progressionCommand = connection.CreateCommand();
        progressionCommand.CommandText = @"
            SELECT unlocked_abilities FROM Companion_Progression
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        progressionCommand.Parameters.AddWithValue("@charId", characterId);
        progressionCommand.Parameters.AddWithValue("@companionId", companionId);

        var unlockedJson = (string?)progressionCommand.ExecuteScalar();
        var unlockedIds = JsonSerializer.Deserialize<List<int>>(unlockedJson ?? "[]") ?? new List<int>();

        // Load abilities from database
        var abilityCommand = connection.CreateCommand();
        abilityCommand.CommandText = @"
            SELECT ability_id, ability_name, owner, description, resource_cost_type, resource_cost,
                   target_type, range_type, range_tiles, damage_type, duration_turns,
                   special_effects, conditions, ability_category
            FROM Companion_Abilities
            WHERE owner = (SELECT companion_name FROM Companions WHERE companion_id = @companionId)
        ";
        abilityCommand.Parameters.AddWithValue("@companionId", companionId);

        var abilities = new List<CompanionAbility>();
        using var reader = abilityCommand.ExecuteReader();

        while (reader.Read())
        {
            var abilityId = reader.GetInt32(0);

            // Only include unlocked abilities
            if (!unlockedIds.Contains(abilityId))
                continue;

            abilities.Add(new CompanionAbility
            {
                AbilityID = abilityId,
                AbilityName = reader.GetString(1),
                Owner = reader.GetString(2),
                Description = reader.GetString(3),
                ResourceCostType = reader.IsDBNull(4) ? null : reader.GetString(4),
                ResourceCost = reader.GetInt32(5),
                TargetType = reader.GetString(6),
                RangeType = reader.GetString(7),
                RangeTiles = reader.GetInt32(8),
                DamageType = reader.IsDBNull(9) ? null : reader.GetString(9),
                DurationTurns = reader.GetInt32(10),
                SpecialEffects = reader.IsDBNull(11) ? null : reader.GetString(11),
                Conditions = reader.IsDBNull(12) ? null : reader.GetString(12),
                AbilityCategory = reader.GetString(13)
            });
        }

        return abilities;
    }

    private void ApplyScaledStats(Companion companion, CompanionScaledStats scaledStats)
    {
        // Apply scaled attributes
        companion.BaseAttributes.Might = scaledStats.Might;
        companion.BaseAttributes.Finesse = scaledStats.Finesse;
        companion.BaseAttributes.Sturdiness = scaledStats.Sturdiness;
        companion.BaseAttributes.Wits = scaledStats.Wits;
        companion.BaseAttributes.Will = scaledStats.Will;

        // Apply scaled resources
        companion.MaxHitPoints = scaledStats.MaxHP;
        companion.Defense = scaledStats.Defense;
        companion.Soak = scaledStats.Soak;

        if (companion.ResourceType == "Stamina")
        {
            companion.MaxStamina = scaledStats.MaxResource;
        }
        else if (companion.ResourceType == "Aether Pool")
        {
            companion.MaxAetherPool = scaledStats.MaxResource;
        }
    }

    // ============================================
    // v0.35: TERRITORY INTEGRATION
    // ============================================

    /// <summary>
    /// v0.35: Process companion reaction when entering a sector
    /// Returns dialogue and any buff/debuff to apply
    /// </summary>
    public (string dialogue, string? buffName, int duration, int value) OnCompanionEnterSector(
        Companion companion,
        int sectorId)
    {
        if (_territoryReactions == null)
            return ("", null, 0, 0);

        try
        {
            var reaction = _territoryReactions.GetCompanionReaction(companion, sectorId);

            _log.Information(
                "Companion {Name} entered sector {SectorId}: {Dialogue}",
                companion.CompanionName, sectorId, reaction.dialogue);

            return reaction;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get companion territory reaction");
            return ("", null, 0, 0);
        }
    }

    /// <summary>
    /// v0.35: Get companion comment on current territory status
    /// </summary>
    public string GetCompanionTerritoryComment(Companion companion, int sectorId)
    {
        if (_territoryReactions == null)
            return "";

        try
        {
            return _territoryReactions.GetCompanionComment(companion, sectorId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get companion territory comment");
            return "";
        }
    }
}
