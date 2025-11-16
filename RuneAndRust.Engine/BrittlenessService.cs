using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.29.3: Brittleness Service (Updated v0.30.2 for Niflheim support)
/// Handles [Brittle] debuff mechanic for both Muspelheim and Niflheim biomes.
///
/// Muspelheim Mechanic:
/// 1. Enemy has Fire Resistance > 0%
/// 2. Enemy takes Ice damage
/// 3. Apply [Brittle] debuff for 2 turns
/// 4. While [Brittle]: Physical damage +50%
///
/// Niflheim Mechanic:
/// 1. Enemy has Ice Resistance > 0%
/// 2. Enemy takes Ice damage
/// 3. Apply [Brittle] debuff for 1 turn
/// 4. While [Brittle]: Physical damage +50%
/// </summary>
public class BrittlenessService
{
    private static readonly ILogger _log = Log.ForContext<BrittlenessService>();

    // Store enemy resistances (temp solution until full resistance system)
    private readonly Dictionary<int, Dictionary<string, int>> _enemyResistances = new();

    // Brittle duration constants
    private const int MUSPELHEIM_BRITTLE_DURATION = 2; // Fire-resistant enemies in Muspelheim
    private const int NIFLHEIM_BRITTLE_DURATION = 1;   // Ice-resistant enemies in Niflheim

    public BrittlenessService()
    {
        _log.Information("BrittlenessService initialized (supports Muspelheim and Niflheim)");
    }

    /// <summary>
    /// Set resistance for an enemy (temporary until full resistance system implemented)
    /// </summary>
    public void SetEnemyResistance(int enemyId, string damageType, int resistancePercent)
    {
        if (!_enemyResistances.ContainsKey(enemyId))
        {
            _enemyResistances[enemyId] = new Dictionary<string, int>();
        }

        _enemyResistances[enemyId][damageType] = resistancePercent;

        _log.Debug("Set {DamageType} resistance for enemy {EnemyId}: {Percent}%",
            damageType, enemyId, resistancePercent);
    }

    /// <summary>
    /// Get resistance percentage for an enemy
    /// </summary>
    public int GetEnemyResistance(int enemyId, string damageType)
    {
        if (_enemyResistances.TryGetValue(enemyId, out var resistances))
        {
            if (resistances.TryGetValue(damageType, out var resistance))
            {
                return resistance;
            }
        }

        return 0; // No resistance
    }

    /// <summary>
    /// Check if Ice damage should apply [Brittle] debuff to target.
    /// Applies if target has Fire Resistance > 0%.
    /// </summary>
    public void TryApplyBrittle(Enemy target, int iceDamageDealt)
    {
        if (iceDamageDealt <= 0)
        {
            _log.Debug("No Ice damage dealt, [Brittle] not applied to {Enemy}", target.Name);
            return;
        }

        int fireResistance = GetEnemyResistance(target.EnemyID, "Fire");

        if (fireResistance <= 0)
        {
            _log.Information(
                "{Target} has no Fire Resistance ({Percent}%), [Brittle] does not apply",
                target.Name,
                fireResistance
            );
            return;
        }

        _log.Information(
            "{Target} has {FireRes}% Fire Resistance, applying [Brittle]",
            target.Name,
            fireResistance
        );

        // Apply or refresh [Brittle] debuff (2 turns, +50% Physical damage)
        ApplyBrittleStatusEffect(target, duration: 2);

        _log.Information(
            "{Target} is now [Brittle] - Physical damage +50% for 2 turns",
            target.Name
        );
    }

    /// <summary>
    /// Apply [Brittle] status effect to enemy
    /// </summary>
    private void ApplyBrittleStatusEffect(Enemy target, int duration)
    {
        // Check if already has Brittle
        var existingBrittle = target.StatusEffects
            .FirstOrDefault(s => s.EffectType.Equals("Brittle", StringComparison.OrdinalIgnoreCase));

        if (existingBrittle != null)
        {
            // Refresh duration
            existingBrittle.DurationRemaining = duration;
            _log.Debug("[Brittle] duration refreshed to {Duration} turns", duration);
        }
        else
        {
            // Add new Brittle effect
            var brittleEffect = new StatusEffect
            {
                EffectInstanceID = GenerateEffectInstanceId(),
                TargetID = target.EnemyID,
                EffectType = "Brittle",
                StackCount = 1,
                DurationRemaining = duration,
                AppliedBy = -1, // System-applied
                AppliedAt = DateTime.UtcNow,
                Category = StatusEffectCategory.StatModification,
                CanStack = false,
                MaxStacks = 1
            };

            target.StatusEffects.Add(brittleEffect);
            _log.Debug("[Brittle] applied for {Duration} turns", duration);
        }
    }

    /// <summary>
    /// Calculate Physical damage bonus against [Brittle] target.
    /// Called after armor reduction.
    /// </summary>
    public int ApplyBrittleBonus(Enemy target, int basePhysicalDamage)
    {
        if (!HasStatusEffect(target, "Brittle"))
        {
            return basePhysicalDamage;
        }

        int bonusDamage = basePhysicalDamage / 2; // +50%
        int totalDamage = basePhysicalDamage + bonusDamage;

        _log.Information(
            "[Brittle] bonus on {Target}: {Base} Physical damage → {Total} (+{Bonus})",
            target.Name,
            basePhysicalDamage,
            totalDamage,
            bonusDamage
        );

        return totalDamage;
    }

    /// <summary>
    /// Check if enemy qualifies for [Brittle] mechanic.
    /// </summary>
    public bool IsBrittleEligible(Enemy enemy)
    {
        int fireResistance = GetEnemyResistance(enemy.EnemyID, "Fire");
        bool eligible = fireResistance > 0;

        _log.Debug("{Enemy} Brittle eligibility: {Eligible} (Fire Resistance: {Percent}%)",
            enemy.Name, eligible, fireResistance);

        return eligible;
    }

    /// <summary>
    /// Check if enemy has a specific status effect
    /// </summary>
    private bool HasStatusEffect(Enemy enemy, string effectType)
    {
        return enemy.StatusEffects.Any(s =>
            s.EffectType.Equals(effectType, StringComparison.OrdinalIgnoreCase) &&
            s.DurationRemaining > 0);
    }

    /// <summary>
    /// Generate unique effect instance ID
    /// </summary>
    private static int _nextEffectInstanceId = 3000; // Start at 3000 for Muspelheim effects
    private int GenerateEffectInstanceId()
    {
        return System.Threading.Interlocked.Increment(ref _nextEffectInstanceId);
    }

    /// <summary>
    /// Get flavor text for Brittle application
    /// </summary>
    public string GetBrittleApplicationMessage(Enemy target)
    {
        return $"❄️ {target.Name}'s heat-resistant tissue flash-freezes, becoming [Brittle]!";
    }

    /// <summary>
    /// Get flavor text for Brittle bonus damage
    /// </summary>
    public string GetBrittleBonusMessage(Enemy target, int bonusDamage)
    {
        return $"💥 [Brittle] vulnerability: +{bonusDamage} Physical damage to {target.Name}!";
    }

    #region Niflheim Support (v0.30.2)

    /// <summary>
    /// v0.30.2: Check if enemy qualifies for [Brittle] in Niflheim.
    /// Requires Ice Resistance > 0%.
    /// </summary>
    public bool IsBrittleEligibleNiflheim(Enemy enemy)
    {
        int iceResistance = GetEnemyResistance(enemy.EnemyID, "Ice");
        bool eligible = iceResistance > 0;

        _log.Debug("{Enemy} Brittle eligibility (Niflheim): {Eligible} (Ice Resistance: {Percent}%)",
            enemy.Name, eligible, iceResistance);

        return eligible;
    }

    /// <summary>
    /// v0.30.2: Apply [Brittle] debuff to enemy (Niflheim variant, 1 turn duration).
    /// </summary>
    public void ApplyBrittle(Enemy enemy)
    {
        ApplyBrittleStatusEffect(enemy, duration: NIFLHEIM_BRITTLE_DURATION);

        _log.Information("{Enemy} is now [Brittle] - Physical damage +50% for {Duration} turn(s)",
            enemy.Name, NIFLHEIM_BRITTLE_DURATION);
    }

    /// <summary>
    /// v0.30.2: Apply [Brittle] debuff with specific duration.
    /// </summary>
    public void ApplyBrittle(Enemy enemy, int duration)
    {
        ApplyBrittleStatusEffect(enemy, duration);

        _log.Information("{Enemy} is now [Brittle] - Physical damage +50% for {Duration} turn(s)",
            enemy.Name, duration);
    }

    /// <summary>
    /// v0.30.2: Check if enemy currently has [Brittle] status.
    /// </summary>
    public bool HasBrittle(Enemy enemy)
    {
        bool hasBrittle = HasStatusEffect(enemy, "Brittle");

        _log.Debug("{Enemy} has [Brittle]: {HasBrittle}",
            enemy.Name, hasBrittle);

        return hasBrittle;
    }

    /// <summary>
    /// v0.30.2: Try to apply [Brittle] to enemy in Niflheim biome.
    /// Applies if target has Ice Resistance > 0% and took Ice damage.
    /// </summary>
    public void TryApplyBrittleNiflheim(Enemy target, int iceDamageDealt)
    {
        if (iceDamageDealt <= 0)
        {
            _log.Debug("No Ice damage dealt, [Brittle] not applied to {Enemy}", target.Name);
            return;
        }

        int iceResistance = GetEnemyResistance(target.EnemyID, "Ice");

        if (iceResistance <= 0)
        {
            _log.Information(
                "{Target} has no Ice Resistance ({Percent}%), [Brittle] does not apply",
                target.Name,
                iceResistance
            );
            return;
        }

        _log.Information(
            "{Target} has {IceRes}% Ice Resistance, applying [Brittle]",
            target.Name,
            iceResistance
        );

        // Apply or refresh [Brittle] debuff (1 turn for Niflheim)
        ApplyBrittle(target);

        _log.Information(
            "{Target} is now [Brittle] - Physical damage +50% for {Duration} turn",
            target.Name,
            NIFLHEIM_BRITTLE_DURATION
        );
    }

    #endregion
}
