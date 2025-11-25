using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of an Echo Chain operation
/// </summary>
public class EchoChainResult
{
    public bool Success { get; set; }
    public int ChainDamage { get; set; }
    public int TargetsHit { get; set; }
    public List<int> AffectedEnemyIds { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Result of a combat ability operation
/// </summary>
public class CombatResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Damage { get; set; }
    public List<int> AffectedEnemyIds { get; set; } = new();
}

/// <summary>
/// Echo Chain tracking data for a character
/// </summary>
public class EchoChainData
{
    public int Range { get; set; } = 1;
    public float DamageMultiplier { get; set; } = 0.5f;
    public int MaxTargets { get; set; } = 1;
}

/// <summary>
/// v0.28.2: Service for Echo-Caller specialization-specific abilities and mechanics.
/// Handles Scream of Silence, Phantom Menace, Reality Fracture, Echo Displacement, and Silence Made Weapon.
/// The psychic artillery who weaponizes echoes for crowd control and cascading damage.
/// </summary>
public class EchoCallerService
{
    private static readonly ILogger _log = Log.ForContext<EchoCallerService>();
    private readonly string _connectionString;
    private readonly DiceService _diceService;
    private readonly TraumaEconomyService _traumaService;

    public EchoCallerService(string connectionString)
    {
        _connectionString = connectionString;
        _diceService = new DiceService();
        _traumaService = new TraumaEconomyService();
        _log.Debug("EchoCallerService initialized");
    }

    #region Initialization

    /// <summary>
    /// Initialize Echo Chain tracking for a character (creates entry if doesn't exist)
    /// </summary>
    public bool InitializeEchoChains(int characterId)
    {
        using var operation = _log.BeginScope("InitializeEchoChains for character {CharacterId}", characterId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR IGNORE INTO Characters_EchoChains (
                    character_id, total_echoes_triggered, total_echo_chains_triggered,
                    echo_cascade_rank, echo_chain_range, echo_chain_damage_multiplier,
                    echo_chain_max_targets, silence_weapon_uses_this_combat,
                    created_at, updated_at
                ) VALUES (
                    @CharacterId, 0, 0, 0, 1, 0.5, 1, 0,
                    datetime('now'), datetime('now')
                )
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.ExecuteNonQuery();

            _log.Information("Echo Chains initialized for character {CharacterId}", characterId);
            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize Echo Chains for character {CharacterId}", characterId);
            return false;
        }
    }

    #endregion

    #region Tier 1 Abilities

    /// <summary>
    /// Execute Scream of Silence (Tier 1)
    /// Psychic damage with bonus vs Feared targets, Echo Chain spread at Rank 3
    /// </summary>
    public CombatResult CastScreamOfSilence(
        PlayerCharacter caster,
        Enemy target,
        int rank = 1)
    {
        using var operation = _log.BeginScope("CastScreamOfSilence: CasterID={CasterId}, TargetID={TargetId}, Rank={Rank}",
            caster.CharacterID, target.EnemyID, rank);

        try
        {
            // Calculate damage based on rank
            var (baseDice, fearBonusDice) = rank switch
            {
                1 => (3, 1),  // 3d8 base, +1d8 if Feared
                2 => (4, 2),  // 4d8 base, +2d8 if Feared
                3 => (5, 2),  // 5d8 base, +2d8 if Feared, Echo Chain
                _ => (3, 1)
            };

            // Roll base damage
            int damage = 0;
            for (int i = 0; i < baseDice; i++)
            {
                damage += _diceService.RollD8();
            }

            // Check if target is Feared (placeholder - would check actual status effects)
            bool isFeared = CheckIfFeared(target.EnemyID);
            if (isFeared)
            {
                int bonusDamage = 0;
                for (int i = 0; i < fearBonusDice; i++)
                {
                    bonusDamage += _diceService.RollD8();
                }
                damage += bonusDamage;

                _log.Information("Scream of Silence bonus damage: Target was Feared, +{Bonus} damage", bonusDamage);
            }

            // Apply damage (placeholder - would call actual combat service)
            ApplyDamage(target.EnemyID, damage, "Psychic");

            // Track echo
            IncrementEchoCount(caster.CharacterID);

            string message = $"Scream of Silence deals {damage} Psychic damage to {target.Name}!";

            // Rank 3: Echo Chain
            var affectedEnemies = new List<int> { target.EnemyID };
            if (rank >= 3)
            {
                var chainResult = TriggerEchoChain(caster.CharacterID, target.EnemyID, damage * 0.5f);
                if (chainResult.Success)
                {
                    message += $" [Echo Chain] Spreads to {chainResult.TargetsHit} adjacent enemy(ies) for {chainResult.ChainDamage} damage!";
                    affectedEnemies.AddRange(chainResult.AffectedEnemyIds);
                }
            }

            _log.Information("Scream of Silence cast: CasterID={CasterId}, TargetID={TargetId}, Damage={Damage}, WasFeared={Feared}",
                caster.CharacterID, target.EnemyID, damage, isFeared);

            return new CombatResult
            {
                Success = true,
                Message = message,
                Damage = damage,
                AffectedEnemyIds = affectedEnemies
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to cast Scream of Silence: CasterID={CasterId}", caster.CharacterID);
            throw;
        }
    }

    /// <summary>
    /// Execute Phantom Menace (Tier 1)
    /// Apply [Feared] debuff with Echo Chain at Rank 3
    /// </summary>
    public CombatResult CastPhantomMenace(
        PlayerCharacter caster,
        Enemy target,
        int rank = 1)
    {
        using var operation = _log.BeginScope("CastPhantomMenace: CasterID={CasterId}, TargetID={TargetId}, Rank={Rank}",
            caster.CharacterID, target.EnemyID, rank);

        try
        {
            // Calculate duration based on rank
            int fearDuration = rank >= 2 ? 3 : 2;

            // Apply [Feared] status effect (placeholder - would call actual status effect service)
            ApplyStatusEffect(target.EnemyID, "Feared", fearDuration);

            // Track echo
            IncrementEchoCount(caster.CharacterID);

            // Trigger Terror Feedback if unlocked (restore Aether)
            TriggerTerrorFeedback(caster.CharacterID);

            string message = $"Phantom Menace applies [Feared] to {target.Name} for {fearDuration} turns!";
            var affectedEnemies = new List<int> { target.EnemyID };

            // Rank 3: Echo Chain with 50% chance
            if (rank >= 3)
            {
                var random = new Random();
                if (random.NextDouble() <= 0.5)
                {
                    var adjacentEnemies = GetAdjacentEnemies(target.EnemyID, 1);
                    if (adjacentEnemies.Any())
                    {
                        var chainTarget = adjacentEnemies.First();
                        ApplyStatusEffect(chainTarget, "Feared", 2);
                        message += $" [Echo Chain] Spreads to adjacent enemy for 2 turns!";
                        affectedEnemies.Add(chainTarget);
                        IncrementEchoChainCount(caster.CharacterID);
                    }
                }
            }

            _log.Information("Phantom Menace cast: CasterID={CasterId}, TargetID={TargetId}, Duration={Duration}",
                caster.CharacterID, target.EnemyID, fearDuration);

            return new CombatResult
            {
                Success = true,
                Message = message,
                AffectedEnemyIds = affectedEnemies
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to cast Phantom Menace: CasterID={CasterId}", caster.CharacterID);
            throw;
        }
    }

    #endregion

    #region Tier 2 Abilities

    /// <summary>
    /// Execute Reality Fracture (Tier 2)
    /// Psychic damage + Disoriented + forced Push with Echo Chain
    /// </summary>
    public CombatResult CastRealityFracture(
        PlayerCharacter caster,
        Enemy target,
        string pushDirection,
        int rank = 1)
    {
        using var operation = _log.BeginScope("CastRealityFracture: CasterID={CasterId}, TargetID={TargetId}, Rank={Rank}",
            caster.CharacterID, target.EnemyID, rank);

        try
        {
            // Calculate effects based on rank
            var (damageDice, pushTiles) = rank switch
            {
                1 => (2, 2),  // 2d8 damage, Push 2 tiles
                2 => (3, 3),  // 3d8 damage, Push 3 tiles
                3 => (4, 3),  // 4d8 damage, Push 3 tiles, Echo Chain
                _ => (2, 2)
            };

            // Roll damage
            int damage = 0;
            for (int i = 0; i < damageDice; i++)
            {
                damage += _diceService.RollD8();
            }

            // Apply damage and effects
            ApplyDamage(target.EnemyID, damage, "Psychic");
            ApplyStatusEffect(target.EnemyID, "Disoriented", 2);
            PushEnemy(target.EnemyID, pushDirection, pushTiles);

            // Track echo
            IncrementEchoCount(caster.CharacterID);

            string message = $"Reality Fracture deals {damage} Psychic damage, applies [Disoriented], and Pushes {target.Name} {pushTiles} tiles {pushDirection}!";
            var affectedEnemies = new List<int> { target.EnemyID };

            // Rank 3: Echo Chain pushes adjacent enemy
            if (rank >= 3)
            {
                var adjacentEnemies = GetAdjacentEnemies(target.EnemyID, 1);
                if (adjacentEnemies.Any())
                {
                    var chainTarget = adjacentEnemies.First();
                    PushEnemy(chainTarget, pushDirection, 2);
                    message += $" [Echo Chain] Adjacent enemy also Pushed 2 tiles!";
                    affectedEnemies.Add(chainTarget);
                    IncrementEchoChainCount(caster.CharacterID);
                }
            }

            _log.Information("Reality Fracture cast: CasterID={CasterId}, TargetID={TargetId}, Damage={Damage}, Push={Tiles}",
                caster.CharacterID, target.EnemyID, damage, pushTiles);

            return new CombatResult
            {
                Success = true,
                Message = message,
                Damage = damage,
                AffectedEnemyIds = affectedEnemies
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to cast Reality Fracture: CasterID={CasterId}", caster.CharacterID);
            throw;
        }
    }

    /// <summary>
    /// Update Echo Cascade rank for a character
    /// This affects Echo Chain range, damage multiplier, and max targets
    /// </summary>
    public void UpdateEchoCascadeRank(int characterId, int rank)
    {
        using var operation = _log.BeginScope("UpdateEchoCascadeRank: CharacterId={CharacterId}, Rank={Rank}",
            characterId, rank);

        try
        {
            var (range, damageMultiplier, maxTargets) = rank switch
            {
                1 => (2, 0.6f, 1),  // Range 2 tiles, 60% damage, 1 target
                2 => (2, 0.7f, 1),  // Range 2 tiles, 70% damage, 1 target
                3 => (3, 0.8f, 2),  // Range 3 tiles, 80% damage, 2 targets
                _ => (1, 0.5f, 1)
            };

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_EchoChains
                SET echo_cascade_rank = @Rank,
                    echo_chain_range = @Range,
                    echo_chain_damage_multiplier = @Multiplier,
                    echo_chain_max_targets = @MaxTargets,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.Parameters.AddWithValue("@Rank", rank);
            command.Parameters.AddWithValue("@Range", range);
            command.Parameters.AddWithValue("@Multiplier", damageMultiplier);
            command.Parameters.AddWithValue("@MaxTargets", maxTargets);
            command.ExecuteNonQuery();

            _log.Information("Echo Cascade rank updated: CharacterId={CharacterId}, Rank={Rank}, Range={Range}, Multiplier={Multiplier}",
                characterId, rank, range, damageMultiplier);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update Echo Cascade rank: CharacterId={CharacterId}", characterId);
            throw;
        }
    }

    #endregion

    #region Tier 3 Abilities

    /// <summary>
    /// Execute Echo Displacement (Tier 3)
    /// Forced teleportation + Psychic damage + Disoriented with Stress cost
    /// </summary>
    public CombatResult CastEchoDisplacement(
        PlayerCharacter caster,
        Enemy target,
        int newX,
        int newY,
        int rank = 1)
    {
        using var operation = _log.BeginScope("CastEchoDisplacement: CasterID={CasterId}, TargetID={TargetId}, Rank={Rank}",
            caster.CharacterID, target.EnemyID, rank);

        try
        {
            // Calculate effects based on rank
            var (maxRange, damageDice, stressCost) = rank switch
            {
                1 => (5, 2, 5),   // 5 tiles, 2d8 damage, +5 Stress
                2 => (7, 3, 4),   // 7 tiles, 3d8 damage, +4 Stress
                3 => (10, 4, 3),  // 10 tiles, 4d8 damage, +3 Stress, Echo Chain
                _ => (5, 2, 5)
            };

            // Roll damage
            int damage = 0;
            for (int i = 0; i < damageDice; i++)
            {
                damage += _diceService.RollD8();
            }

            // Apply effects
            TeleportEnemy(target.EnemyID, newX, newY);
            ApplyDamage(target.EnemyID, damage, "Psychic");
            ApplyStatusEffect(target.EnemyID, "Disoriented", 1);

            // Apply Psychic Stress cost to caster
            var (stressGained, traumaAcquired) = _traumaService.AddStress(
                caster,
                stressCost,
                source: "Echo Displacement (forceful reality manipulation)",
                allowResolveCheck: false
            );

            // Track echo
            IncrementEchoCount(caster.CharacterID);

            string message = $"Echo Displacement teleports {target.Name}, deals {damage} Psychic damage, applies [Disoriented]! (+{stressGained} Psychic Stress to caster)";
            var affectedEnemies = new List<int> { target.EnemyID };

            // Rank 3: Echo Chain teleports adjacent enemy
            if (rank >= 3)
            {
                var adjacentEnemies = GetAdjacentEnemies(target.EnemyID, 1);
                if (adjacentEnemies.Any())
                {
                    var chainTarget = adjacentEnemies.First();
                    // Random tile within 3 tiles (placeholder coordinates)
                    TeleportEnemy(chainTarget, newX + 2, newY + 1);
                    message += $" [Echo Chain] Adjacent enemy also teleported!";
                    affectedEnemies.Add(chainTarget);
                    IncrementEchoChainCount(caster.CharacterID);
                }
            }

            if (traumaAcquired != null)
            {
                message += $" [BREAKING POINT - Trauma acquired: {traumaAcquired.Name}]";
            }

            _log.Information("Echo Displacement cast: CasterID={CasterId}, TargetID={TargetId}, Damage={Damage}, Stress={Stress}",
                caster.CharacterID, target.EnemyID, damage, stressGained);

            return new CombatResult
            {
                Success = true,
                Message = message,
                Damage = damage,
                AffectedEnemyIds = affectedEnemies
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to cast Echo Displacement: CasterID={CasterId}", caster.CharacterID);
            throw;
        }
    }

    #endregion

    #region Capstone: Silence Made Weapon

    /// <summary>
    /// Execute Silence Made Weapon (Capstone)
    /// Battlefield-wide psychic assault scaling with enemy debuffs
    /// </summary>
    public CombatResult CastSilenceMadeWeapon(
        PlayerCharacter caster,
        List<Enemy> allEnemies,
        int rank = 1)
    {
        using var operation = _log.BeginScope("CastSilenceMadeWeapon: CasterID={CasterId}, Rank={Rank}",
            caster.CharacterID, rank);

        try
        {
            // Check uses per combat
            int usesThisCombat = GetSilenceWeaponUses(caster.CharacterID);
            int maxUses = rank >= 3 ? 2 : 1;

            if (usesThisCombat >= maxUses)
            {
                _log.Warning("Silence Made Weapon already used maximum times this combat");
                return new CombatResult { Success = false, Message = "Already used this combat!" };
            }

            // Calculate parameters based on rank
            var (baseDice, bonusDicePerStatus, maxBonusDice, dc, stressCost) = rank switch
            {
                1 => (4, 1, 5, 16, 15),    // 4d10 base, +1d10 per status (max +5), DC 16, +15 Stress
                2 => (5, 1, 6, 17, 12),    // 5d10 base, +1d10 per status (max +6), DC 17, +12 Stress
                3 => (6, 2, 12, 18, 10),   // 6d10 base, +2d10 per status (max +12), DC 18, +10 Stress
                _ => (4, 1, 5, 16, 15)
            };

            // Count enemies with Fear/Disoriented
            int statusCount = 0;
            foreach (var enemy in allEnemies)
            {
                if (CheckIfFeared(enemy.EnemyID) || CheckIfDisoriented(enemy.EnemyID))
                {
                    statusCount++;
                }
            }

            // Calculate total damage dice
            int bonusDice = Math.Min(statusCount * bonusDicePerStatus, maxBonusDice);
            int totalDice = baseDice + bonusDice;

            var results = new List<string>();
            var affectedEnemies = new List<int>();

            // Apply to all enemies
            foreach (var enemy in allEnemies)
            {
                // Roll damage with d10
                int damage = 0;
                for (int i = 0; i < totalDice; i++)
                {
                    damage += _diceService.Roll(1, 10);
                }

                ApplyDamage(enemy.EnemyID, damage, "Psychic");
                affectedEnemies.Add(enemy.EnemyID);

                // WILL check for Fear
                var willValue = enemy.WILL > 0 ? enemy.WILL : 2;
                var checkPassed = _diceService.SkillCheck(willValue, dc);
                if (!checkPassed)
                {
                    ApplyStatusEffect(enemy.EnemyID, "Feared", 2);
                    results.Add($"{enemy.Name}: {damage} damage, Feared");
                }
                else
                {
                    results.Add($"{enemy.Name}: {damage} damage");
                }
            }

            // Apply Stress cost to caster
            var (stressGained, traumaAcquired) = _traumaService.AddStress(
                caster,
                stressCost,
                source: "Silence Made Weapon (channeling raw Great Silence)",
                allowResolveCheck: false
            );

            // Mark as used
            MarkSilenceWeaponUsed(caster.CharacterID);

            string message = $"SILENCE MADE WEAPON! {allEnemies.Count} enemies hit with {totalDice}d10 Psychic damage! (+{stressGained} Psychic Stress)";

            if (traumaAcquired != null)
            {
                message += $" [BREAKING POINT - Trauma acquired: {traumaAcquired.Name}]";
            }

            _log.Information("SILENCE MADE WEAPON: CasterID={CasterId}, Enemies={Count}, TotalDice={Dice}d10, StatusCount={Status}, Stress={Stress}",
                caster.CharacterID, allEnemies.Count, totalDice, statusCount, stressGained);

            return new CombatResult
            {
                Success = true,
                Message = message,
                AffectedEnemyIds = affectedEnemies
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to cast Silence Made Weapon: CasterID={CasterId}", caster.CharacterID);
            throw;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Trigger Echo Chain effect (cascading damage to adjacent enemies)
    /// </summary>
    private EchoChainResult TriggerEchoChain(int casterId, int originEnemyId, float baseDamage)
    {
        var echoData = GetEchoChainData(casterId);

        var adjacentEnemies = GetAdjacentEnemies(originEnemyId, echoData.Range);
        if (!adjacentEnemies.Any())
        {
            return new EchoChainResult { Success = false };
        }

        // Apply multiplier
        int finalDamage = (int)(baseDamage * echoData.DamageMultiplier);

        var targets = adjacentEnemies.Take(echoData.MaxTargets).ToList();
        foreach (var targetId in targets)
        {
            ApplyDamage(targetId, finalDamage, "Psychic");
        }

        IncrementEchoChainCount(casterId);

        _log.Information("Echo Chain triggered: OriginID={OriginId}, Targets={Count}, Damage={Damage}",
            originEnemyId, targets.Count, finalDamage);

        return new EchoChainResult
        {
            Success = true,
            ChainDamage = finalDamage,
            TargetsHit = targets.Count,
            AffectedEnemyIds = targets
        };
    }

    /// <summary>
    /// Get Echo Chain configuration data for a character
    /// </summary>
    private EchoChainData GetEchoChainData(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT echo_chain_range, echo_chain_damage_multiplier, echo_chain_max_targets
                FROM Characters_EchoChains
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new EchoChainData
                {
                    Range = reader.GetInt32(0),
                    DamageMultiplier = reader.GetFloat(1),
                    MaxTargets = reader.GetInt32(2)
                };
            }

            // Default values if not found
            return new EchoChainData();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get Echo Chain data for character {CharacterId}", characterId);
            return new EchoChainData();
        }
    }

    /// <summary>
    /// Increment total echoes triggered counter
    /// </summary>
    private void IncrementEchoCount(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_EchoChains
                SET total_echoes_triggered = total_echoes_triggered + 1,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to increment echo count for character {CharacterId}", characterId);
        }
    }

    /// <summary>
    /// Increment total echo chains triggered counter
    /// </summary>
    private void IncrementEchoChainCount(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_EchoChains
                SET total_echo_chains_triggered = total_echo_chains_triggered + 1,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to increment echo chain count for character {CharacterId}", characterId);
        }
    }

    /// <summary>
    /// Get number of times Silence Made Weapon has been used this combat
    /// </summary>
    private int GetSilenceWeaponUses(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT silence_weapon_uses_this_combat
                FROM Characters_EchoChains
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);

            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get Silence Weapon uses for character {CharacterId}", characterId);
            return 0;
        }
    }

    /// <summary>
    /// Mark Silence Made Weapon as used this combat
    /// </summary>
    private void MarkSilenceWeaponUsed(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_EchoChains
                SET silence_weapon_uses_this_combat = silence_weapon_uses_this_combat + 1,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to mark Silence Weapon as used for character {CharacterId}", characterId);
        }
    }

    /// <summary>
    /// Reset combat state (call at end of combat)
    /// </summary>
    public void ResetCombatState(int characterId)
    {
        using var operation = _log.BeginScope("ResetCombatState for character {CharacterId}", characterId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_EchoChains
                SET silence_weapon_uses_this_combat = 0,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.ExecuteNonQuery();

            _log.Information("Combat state reset for character {CharacterId}", characterId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to reset combat state for character {CharacterId}", characterId);
        }
    }

    /// <summary>
    /// Trigger Terror Feedback passive (restore Aether when applying Fear)
    /// </summary>
    private void TriggerTerrorFeedback(int characterId)
    {
        // Placeholder - would check if character has Terror Feedback unlocked and restore Aether
        // Based on rank: Rank 1 = 10 Aether, Rank 2 = 15 Aether, Rank 3 = 20 Aether + Empowered buff
        _log.Debug("Terror Feedback triggered for character {CharacterId}", characterId);
    }

    // Placeholder methods for integration with actual combat/status effect systems
    private bool CheckIfFeared(int enemyId) => false;
    private bool CheckIfDisoriented(int enemyId) => false;
    private void ApplyDamage(int enemyId, int damage, string damageType) { }
    private void ApplyStatusEffect(int enemyId, string effect, int duration) { }
    private void PushEnemy(int enemyId, string direction, int tiles) { }
    private void TeleportEnemy(int enemyId, int x, int y) { }
    private List<int> GetAdjacentEnemies(int enemyId, int range) => new List<int>();

    #endregion
}
