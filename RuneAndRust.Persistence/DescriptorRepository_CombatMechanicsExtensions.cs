using Microsoft.Data.Sqlite;
using RuneAndRust.Core.CombatFlavor;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.12: Advanced Combat Mechanics Descriptors extensions for DescriptorRepository
/// Provides database access to defensive action, stance, critical hit, fumble, and maneuver descriptors
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _combatMechanicsLog = Log.ForContext<DescriptorRepository>();

    #region Defensive Action Descriptors

    /// <summary>
    /// Gets defensive action descriptors with optional filtering
    /// </summary>
    public List<DefensiveActionDescriptor> GetDefensiveActionDescriptors(
        string? actionType = null,
        string? outcomeType = null,
        string? weaponType = null,
        string? attackIntensity = null,
        string? environmentContext = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, action_type, outcome_type, weapon_type,
                attack_intensity, environment_context,
                descriptor_text, weight, is_active, tags
            FROM Combat_Defensive_Action_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(actionType))
        {
            sql += " AND action_type = $actionType";
            command.Parameters.AddWithValue("$actionType", actionType);
        }

        if (!string.IsNullOrEmpty(outcomeType))
        {
            sql += " AND outcome_type = $outcomeType";
            command.Parameters.AddWithValue("$outcomeType", outcomeType);
        }

        if (!string.IsNullOrEmpty(weaponType))
        {
            sql += " AND (weapon_type = $weaponType OR weapon_type IS NULL)";
            command.Parameters.AddWithValue("$weaponType", weaponType);
        }

        if (!string.IsNullOrEmpty(attackIntensity))
        {
            sql += " AND (attack_intensity = $attackIntensity OR attack_intensity IS NULL)";
            command.Parameters.AddWithValue("$attackIntensity", attackIntensity);
        }

        if (!string.IsNullOrEmpty(environmentContext))
        {
            sql += " AND (environment_context = $environmentContext OR environment_context IS NULL)";
            command.Parameters.AddWithValue("$environmentContext", environmentContext);
        }

        command.CommandText = sql;

        var descriptors = new List<DefensiveActionDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapDefensiveActionDescriptor(reader));
        }

        _combatMechanicsLog.Debug(
            "Loaded {Count} defensive action descriptors (Action: {ActionType}, Outcome: {OutcomeType})",
            descriptors.Count, actionType, outcomeType);

        return descriptors;
    }

    /// <summary>
    /// Gets a random defensive action descriptor matching the criteria
    /// </summary>
    public DefensiveActionDescriptor? GetRandomDefensiveActionDescriptor(
        string actionType,
        string outcomeType,
        string? weaponType = null,
        string? attackIntensity = null,
        string? environmentContext = null)
    {
        var descriptors = GetDefensiveActionDescriptors(
            actionType, outcomeType, weaponType, attackIntensity, environmentContext);

        if (descriptors.Count == 0)
        {
            // Fallback: try without weapon type
            if (!string.IsNullOrEmpty(weaponType))
            {
                descriptors = GetDefensiveActionDescriptors(
                    actionType, outcomeType, null, attackIntensity, environmentContext);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without attack intensity
            if (!string.IsNullOrEmpty(attackIntensity))
            {
                descriptors = GetDefensiveActionDescriptors(
                    actionType, outcomeType, weaponType, null, environmentContext);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without environment context
            if (!string.IsNullOrEmpty(environmentContext))
            {
                descriptors = GetDefensiveActionDescriptors(
                    actionType, outcomeType, weaponType, attackIntensity, null);
            }
        }

        if (descriptors.Count == 0)
        {
            _combatMechanicsLog.Warning(
                "No defensive action descriptors found for: {ActionType} {OutcomeType}",
                actionType, outcomeType);
            return null;
        }

        return SelectWeightedRandom(descriptors);
    }

    private DefensiveActionDescriptor MapDefensiveActionDescriptor(SqliteDataReader reader)
    {
        return new DefensiveActionDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            ActionType = reader.GetString(1),
            OutcomeType = reader.GetString(2),
            WeaponType = reader.IsDBNull(3) ? null : reader.GetString(3),
            AttackIntensity = reader.IsDBNull(4) ? null : reader.GetString(4),
            EnvironmentContext = reader.IsDBNull(5) ? null : reader.GetString(5),
            DescriptorText = reader.GetString(6),
            Weight = reader.GetFloat(7),
            IsActive = reader.GetInt32(8) == 1,
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    #endregion

    #region Combat Stance Descriptors

    /// <summary>
    /// Gets combat stance descriptors with optional filtering
    /// </summary>
    public List<CombatStanceDescriptor> GetCombatStanceDescriptors(
        string? stanceType = null,
        string? descriptionMoment = null,
        string? previousStance = null,
        string? situationContext = null,
        string? weaponConfiguration = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, stance_type, description_moment, previous_stance,
                situation_context, weapon_configuration,
                descriptor_text, weight, is_active, tags
            FROM Combat_Stance_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(stanceType))
        {
            sql += " AND stance_type = $stanceType";
            command.Parameters.AddWithValue("$stanceType", stanceType);
        }

        if (!string.IsNullOrEmpty(descriptionMoment))
        {
            sql += " AND description_moment = $descriptionMoment";
            command.Parameters.AddWithValue("$descriptionMoment", descriptionMoment);
        }

        if (!string.IsNullOrEmpty(previousStance))
        {
            sql += " AND (previous_stance = $previousStance OR previous_stance IS NULL)";
            command.Parameters.AddWithValue("$previousStance", previousStance);
        }

        if (!string.IsNullOrEmpty(situationContext))
        {
            sql += " AND (situation_context = $situationContext OR situation_context IS NULL)";
            command.Parameters.AddWithValue("$situationContext", situationContext);
        }

        if (!string.IsNullOrEmpty(weaponConfiguration))
        {
            sql += " AND (weapon_configuration = $weaponConfiguration OR weapon_configuration IS NULL)";
            command.Parameters.AddWithValue("$weaponConfiguration", weaponConfiguration);
        }

        command.CommandText = sql;

        var descriptors = new List<CombatStanceDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapCombatStanceDescriptor(reader));
        }

        _combatMechanicsLog.Debug(
            "Loaded {Count} combat stance descriptors (Stance: {StanceType}, Moment: {DescriptionMoment})",
            descriptors.Count, stanceType, descriptionMoment);

        return descriptors;
    }

    /// <summary>
    /// Gets a random combat stance descriptor matching the criteria
    /// </summary>
    public CombatStanceDescriptor? GetRandomCombatStanceDescriptor(
        string stanceType,
        string descriptionMoment,
        string? previousStance = null,
        string? situationContext = null,
        string? weaponConfiguration = null)
    {
        var descriptors = GetCombatStanceDescriptors(
            stanceType, descriptionMoment, previousStance, situationContext, weaponConfiguration);

        if (descriptors.Count == 0)
        {
            // Fallback: try without situation context
            if (!string.IsNullOrEmpty(situationContext))
            {
                descriptors = GetCombatStanceDescriptors(
                    stanceType, descriptionMoment, previousStance, null, weaponConfiguration);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without weapon configuration
            if (!string.IsNullOrEmpty(weaponConfiguration))
            {
                descriptors = GetCombatStanceDescriptors(
                    stanceType, descriptionMoment, previousStance, situationContext, null);
            }
        }

        if (descriptors.Count == 0)
        {
            _combatMechanicsLog.Warning(
                "No combat stance descriptors found for: {StanceType} {DescriptionMoment}",
                stanceType, descriptionMoment);
            return null;
        }

        return SelectWeightedRandom(descriptors);
    }

    private CombatStanceDescriptor MapCombatStanceDescriptor(SqliteDataReader reader)
    {
        return new CombatStanceDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            StanceType = reader.GetString(1),
            DescriptionMoment = reader.GetString(2),
            PreviousStance = reader.IsDBNull(3) ? null : reader.GetString(3),
            SituationContext = reader.IsDBNull(4) ? null : reader.GetString(4),
            WeaponConfiguration = reader.IsDBNull(5) ? null : reader.GetString(5),
            DescriptorText = reader.GetString(6),
            Weight = reader.GetFloat(7),
            IsActive = reader.GetInt32(8) == 1,
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    #endregion

    #region Critical Hit Descriptors

    /// <summary>
    /// Gets critical hit descriptors with optional filtering
    /// </summary>
    public List<CriticalHitDescriptor> GetCriticalHitDescriptors(
        string? attackCategory = null,
        string? damageType = null,
        string? weaponOrSpellType = null,
        string? targetType = null,
        string? specialEffect = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, attack_category, damage_type, weapon_or_spell_type,
                target_type, special_effect,
                descriptor_text, weight, is_active, tags
            FROM Combat_Critical_Hit_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(attackCategory))
        {
            sql += " AND attack_category = $attackCategory";
            command.Parameters.AddWithValue("$attackCategory", attackCategory);
        }

        if (!string.IsNullOrEmpty(damageType))
        {
            sql += " AND damage_type = $damageType";
            command.Parameters.AddWithValue("$damageType", damageType);
        }

        if (!string.IsNullOrEmpty(weaponOrSpellType))
        {
            sql += " AND (weapon_or_spell_type = $weaponOrSpellType OR weapon_or_spell_type IS NULL)";
            command.Parameters.AddWithValue("$weaponOrSpellType", weaponOrSpellType);
        }

        if (!string.IsNullOrEmpty(targetType))
        {
            sql += " AND (target_type = $targetType OR target_type IS NULL)";
            command.Parameters.AddWithValue("$targetType", targetType);
        }

        if (!string.IsNullOrEmpty(specialEffect))
        {
            sql += " AND (special_effect = $specialEffect OR special_effect IS NULL)";
            command.Parameters.AddWithValue("$specialEffect", specialEffect);
        }

        command.CommandText = sql;

        var descriptors = new List<CriticalHitDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapCriticalHitDescriptor(reader));
        }

        _combatMechanicsLog.Debug(
            "Loaded {Count} critical hit descriptors (Category: {AttackCategory}, DamageType: {DamageType})",
            descriptors.Count, attackCategory, damageType);

        return descriptors;
    }

    /// <summary>
    /// Gets a random critical hit descriptor matching the criteria
    /// </summary>
    public CriticalHitDescriptor? GetRandomCriticalHitDescriptor(
        string attackCategory,
        string damageType,
        string? weaponOrSpellType = null,
        string? targetType = null,
        string? specialEffect = null)
    {
        var descriptors = GetCriticalHitDescriptors(
            attackCategory, damageType, weaponOrSpellType, targetType, specialEffect);

        if (descriptors.Count == 0)
        {
            // Fallback: try without weapon/spell type
            if (!string.IsNullOrEmpty(weaponOrSpellType))
            {
                descriptors = GetCriticalHitDescriptors(
                    attackCategory, damageType, null, targetType, specialEffect);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without target type
            if (!string.IsNullOrEmpty(targetType))
            {
                descriptors = GetCriticalHitDescriptors(
                    attackCategory, damageType, weaponOrSpellType, null, specialEffect);
            }
        }

        if (descriptors.Count == 0)
        {
            _combatMechanicsLog.Warning(
                "No critical hit descriptors found for: {AttackCategory} {DamageType}",
                attackCategory, damageType);
            return null;
        }

        return SelectWeightedRandom(descriptors);
    }

    private CriticalHitDescriptor MapCriticalHitDescriptor(SqliteDataReader reader)
    {
        return new CriticalHitDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            AttackCategory = reader.GetString(1),
            DamageType = reader.GetString(2),
            WeaponOrSpellType = reader.IsDBNull(3) ? null : reader.GetString(3),
            TargetType = reader.IsDBNull(4) ? null : reader.GetString(4),
            SpecialEffect = reader.IsDBNull(5) ? null : reader.GetString(5),
            DescriptorText = reader.GetString(6),
            Weight = reader.GetFloat(7),
            IsActive = reader.GetInt32(8) == 1,
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    #endregion

    #region Fumble Descriptors

    /// <summary>
    /// Gets fumble descriptors with optional filtering
    /// </summary>
    public List<FumbleDescriptor> GetFumbleDescriptors(
        string? fumbleCategory = null,
        string? fumbleType = null,
        string? equipmentType = null,
        string? severity = null,
        string? environmentFactor = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, fumble_category, fumble_type, equipment_type,
                severity, environment_factor,
                descriptor_text, weight, is_active, tags
            FROM Combat_Fumble_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(fumbleCategory))
        {
            sql += " AND fumble_category = $fumbleCategory";
            command.Parameters.AddWithValue("$fumbleCategory", fumbleCategory);
        }

        if (!string.IsNullOrEmpty(fumbleType))
        {
            sql += " AND fumble_type = $fumbleType";
            command.Parameters.AddWithValue("$fumbleType", fumbleType);
        }

        if (!string.IsNullOrEmpty(equipmentType))
        {
            sql += " AND (equipment_type = $equipmentType OR equipment_type IS NULL)";
            command.Parameters.AddWithValue("$equipmentType", equipmentType);
        }

        if (!string.IsNullOrEmpty(severity))
        {
            sql += " AND (severity = $severity OR severity IS NULL)";
            command.Parameters.AddWithValue("$severity", severity);
        }

        if (!string.IsNullOrEmpty(environmentFactor))
        {
            sql += " AND (environment_factor = $environmentFactor OR environment_factor IS NULL)";
            command.Parameters.AddWithValue("$environmentFactor", environmentFactor);
        }

        command.CommandText = sql;

        var descriptors = new List<FumbleDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapFumbleDescriptor(reader));
        }

        _combatMechanicsLog.Debug(
            "Loaded {Count} fumble descriptors (Category: {FumbleCategory}, Type: {FumbleType})",
            descriptors.Count, fumbleCategory, fumbleType);

        return descriptors;
    }

    /// <summary>
    /// Gets a random fumble descriptor matching the criteria
    /// </summary>
    public FumbleDescriptor? GetRandomFumbleDescriptor(
        string fumbleCategory,
        string fumbleType,
        string? equipmentType = null,
        string? severity = null,
        string? environmentFactor = null)
    {
        var descriptors = GetFumbleDescriptors(
            fumbleCategory, fumbleType, equipmentType, severity, environmentFactor);

        if (descriptors.Count == 0)
        {
            // Fallback: try without equipment type
            if (!string.IsNullOrEmpty(equipmentType))
            {
                descriptors = GetFumbleDescriptors(
                    fumbleCategory, fumbleType, null, severity, environmentFactor);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without severity
            if (!string.IsNullOrEmpty(severity))
            {
                descriptors = GetFumbleDescriptors(
                    fumbleCategory, fumbleType, equipmentType, null, environmentFactor);
            }
        }

        if (descriptors.Count == 0)
        {
            _combatMechanicsLog.Warning(
                "No fumble descriptors found for: {FumbleCategory} {FumbleType}",
                fumbleCategory, fumbleType);
            return null;
        }

        return SelectWeightedRandom(descriptors);
    }

    private FumbleDescriptor MapFumbleDescriptor(SqliteDataReader reader)
    {
        return new FumbleDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            FumbleCategory = reader.GetString(1),
            FumbleType = reader.GetString(2),
            EquipmentType = reader.IsDBNull(3) ? null : reader.GetString(3),
            Severity = reader.IsDBNull(4) ? null : reader.GetString(4),
            EnvironmentFactor = reader.IsDBNull(5) ? null : reader.GetString(5),
            DescriptorText = reader.GetString(6),
            Weight = reader.GetFloat(7),
            IsActive = reader.GetInt32(8) == 1,
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    #endregion

    #region Combat Maneuver Descriptors

    /// <summary>
    /// Gets combat maneuver descriptors with optional filtering
    /// </summary>
    public List<CombatManeuverDescriptor> GetCombatManeuverDescriptors(
        string? maneuverType = null,
        string? outcomeType = null,
        string? weaponType = null,
        string? targetType = null,
        string? environmentContext = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, maneuver_type, outcome_type, weapon_type,
                target_type, environment_context,
                descriptor_text, weight, is_active, tags
            FROM Combat_Maneuver_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(maneuverType))
        {
            sql += " AND maneuver_type = $maneuverType";
            command.Parameters.AddWithValue("$maneuverType", maneuverType);
        }

        if (!string.IsNullOrEmpty(outcomeType))
        {
            sql += " AND outcome_type = $outcomeType";
            command.Parameters.AddWithValue("$outcomeType", outcomeType);
        }

        if (!string.IsNullOrEmpty(weaponType))
        {
            sql += " AND (weapon_type = $weaponType OR weapon_type IS NULL)";
            command.Parameters.AddWithValue("$weaponType", weaponType);
        }

        if (!string.IsNullOrEmpty(targetType))
        {
            sql += " AND (target_type = $targetType OR target_type IS NULL)";
            command.Parameters.AddWithValue("$targetType", targetType);
        }

        if (!string.IsNullOrEmpty(environmentContext))
        {
            sql += " AND (environment_context = $environmentContext OR environment_context IS NULL)";
            command.Parameters.AddWithValue("$environmentContext", environmentContext);
        }

        command.CommandText = sql;

        var descriptors = new List<CombatManeuverDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapCombatManeuverDescriptor(reader));
        }

        _combatMechanicsLog.Debug(
            "Loaded {Count} combat maneuver descriptors (Maneuver: {ManeuverType}, Outcome: {OutcomeType})",
            descriptors.Count, maneuverType, outcomeType);

        return descriptors;
    }

    /// <summary>
    /// Gets a random combat maneuver descriptor matching the criteria
    /// </summary>
    public CombatManeuverDescriptor? GetRandomCombatManeuverDescriptor(
        string maneuverType,
        string outcomeType,
        string? weaponType = null,
        string? targetType = null,
        string? environmentContext = null)
    {
        var descriptors = GetCombatManeuverDescriptors(
            maneuverType, outcomeType, weaponType, targetType, environmentContext);

        if (descriptors.Count == 0)
        {
            // Fallback: try without weapon type
            if (!string.IsNullOrEmpty(weaponType))
            {
                descriptors = GetCombatManeuverDescriptors(
                    maneuverType, outcomeType, null, targetType, environmentContext);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without target type
            if (!string.IsNullOrEmpty(targetType))
            {
                descriptors = GetCombatManeuverDescriptors(
                    maneuverType, outcomeType, weaponType, null, environmentContext);
            }
        }

        if (descriptors.Count == 0)
        {
            _combatMechanicsLog.Warning(
                "No combat maneuver descriptors found for: {ManeuverType} {OutcomeType}",
                maneuverType, outcomeType);
            return null;
        }

        return SelectWeightedRandom(descriptors);
    }

    private CombatManeuverDescriptor MapCombatManeuverDescriptor(SqliteDataReader reader)
    {
        return new CombatManeuverDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            ManeuverType = reader.GetString(1),
            OutcomeType = reader.GetString(2),
            WeaponType = reader.IsDBNull(3) ? null : reader.GetString(3),
            TargetType = reader.IsDBNull(4) ? null : reader.GetString(4),
            EnvironmentContext = reader.IsDBNull(5) ? null : reader.GetString(5),
            DescriptorText = reader.GetString(6),
            Weight = reader.GetFloat(7),
            IsActive = reader.GetInt32(8) == 1,
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Selects a random descriptor from a weighted list
    /// </summary>
    private T? SelectWeightedRandom<T>(List<T> descriptors) where T : class
    {
        if (descriptors.Count == 0)
            return null;

        // Extract weight using reflection
        var weightProperty = typeof(T).GetProperty("Weight");
        if (weightProperty == null)
            return descriptors[new Random().Next(descriptors.Count)];

        var totalWeight = descriptors.Sum(d => (float)(weightProperty.GetValue(d) ?? 1.0f));
        var random = new Random().NextDouble() * totalWeight;
        var cumulativeWeight = 0.0;

        foreach (var descriptor in descriptors)
        {
            var weight = (float)(weightProperty.GetValue(descriptor) ?? 1.0f);
            cumulativeWeight += weight;
            if (random <= cumulativeWeight)
            {
                return descriptor;
            }
        }

        return descriptors.Last();
    }

    /// <summary>
    /// Gets statistics for combat mechanics descriptors
    /// </summary>
    public (int DefensiveActions, int Stances, int CriticalHits, int Fumbles, int Maneuvers) GetCombatMechanicsStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var defensiveActions = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Combat_Defensive_Action_Descriptors WHERE is_active = 1");
        var stances = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Combat_Stance_Descriptors WHERE is_active = 1");
        var criticalHits = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Combat_Critical_Hit_Descriptors WHERE is_active = 1");
        var fumbles = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Combat_Fumble_Descriptors WHERE is_active = 1");
        var maneuvers = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Combat_Maneuver_Descriptors WHERE is_active = 1");

        return (defensiveActions, stances, criticalHits, fumbles, maneuvers);
    }

    private int ExecuteScalarInt(SqliteConnection connection, string sql)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = command.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    #endregion
}
