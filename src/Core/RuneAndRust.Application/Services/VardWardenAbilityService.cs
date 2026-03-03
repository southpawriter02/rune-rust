using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Varð-Warden specialization ability execution.
/// Implements Tier 1 (Foundation), Tier 2 (Discipline), Tier 3 (Mastery),
/// and Capstone (Ultimate) ability logic for the Coherent Defensive Caster path.
/// </summary>
/// <remarks>
/// <para>The Varð-Warden specialization revolves around barriers, zones, ally buffs, and a once-per-expedition reaction capstone.
/// No Corruption mechanics—this is a Coherent path. Abilities protect allies, control the battlefield, and provide utility.</para>
///
/// <para>Tier 1 abilities:</para>
/// <list type="bullet">
/// <item>Sanctified Resolve (Passive): +1d10 WILL vs Push/Pull. No execute method.</item>
/// <item>Runic Barrier (Active): Create barrier 30-50 HP, 2-4 turns. 3 AP.</item>
/// <item>Consecrate Ground (Active): Create healing/damage zone. 3 AP.</item>
/// </list>
///
/// <para>Tier 2 abilities:</para>
/// <list type="bullet">
/// <item>Rune of Shielding (Active): Buff ally +Soak +corruption resist. 3 AP.</item>
/// <item>Reinforce Ward (Active): Heal barrier or boost zone. 2 AP.</item>
/// <item>Wardens Vigil (Passive): Row-wide Stress resistance. No execute method.</item>
/// </list>
///
/// <para>Tier 3 abilities:</para>
/// <list type="bullet">
/// <item>Glyph of Sanctuary (Active): Party temp HP + Stress immunity. 4 AP.</item>
/// <item>Aegis of Sanctity (Passive): Barrier reflection + zone cleanse. No execute method.</item>
/// </list>
///
/// <para>Capstone ability:</para>
/// <list type="bullet">
/// <item>Indomitable Bastion (Reaction): Negate fatal damage, once per expedition, 0 AP.</item>
/// </list>
///
/// <para>Critical design principle: No Corruption. Barriers and zones are deterministic.
/// The once-per-expedition capstone is tracked via HasUsedIndomitableBastionThisExpedition property.
/// Dice roll methods are marked <c>internal virtual</c> for unit test overriding via <c>TestVardWardenAbilityService</c>.</para>
/// </remarks>
public class VardWardenAbilityService : IVardWardenAbilityService
{
    // ===== Tier 1 Constants =====

    /// <summary>AP cost for Runic Barrier.</summary>
    private const int RunicBarrierApCost = 3;

    /// <summary>AP cost for Consecrate Ground.</summary>
    private const int ConsecrateGroundApCost = 3;

    // ===== Tier 2 Constants =====

    /// <summary>AP cost for Rune of Shielding.</summary>
    private const int RuneOfShieldingApCost = 3;

    /// <summary>AP cost for Reinforce Ward.</summary>
    private const int ReinforceWardApCost = 2;

    // ===== Tier 3 Constants =====

    /// <summary>AP cost for Glyph of Sanctuary.</summary>
    private const int GlyphOfSanctuaryApCost = 4;

    // ===== Capstone Constants =====

    /// <summary>AP cost for Indomitable Bastion (reaction ability, no AP cost).</summary>
    private const int IndomitableBastionApCost = 0;

    /// <summary>Barrier HP created by Runic Barrier at each rank.</summary>
    private static readonly int[] RunicBarrierHp = { 30, 40, 50 };

    /// <summary>Duration in turns for Runic Barrier at each rank.</summary>
    private static readonly int[] RunicBarrierDuration = { 2, 3, 4 };

    /// <summary>Soak bonus from Rune of Shielding at each rank.</summary>
    private static readonly int[] RuneOfShieldingSoakBonus = { 3, 5, 7 };

    /// <summary>Corruption resistance bonus percentage from Rune of Shielding at each rank.</summary>
    private static readonly int[] RuneOfShieldingCorruptionResistBonus = { 10, 15, 20 };

    /// <summary>HP restored to barriers by Reinforce Ward at each rank.</summary>
    private static readonly int[] ReinforceWardBarrierHealing = { 15, 20, 25 };

    /// <summary>Zone effectiveness boost percentage by Reinforce Ward at each rank.</summary>
    private static readonly int[] ReinforceWardZoneBoost = { 50, 75, 100 };

    /// <summary>Temp HP per ally from Glyph of Sanctuary (3d10 base, computed per rank).</summary>
    private const int GlyphOfSanctuaryTempHpDice = 3; // 3d10 = 30 on average

    /// <summary>Duration in turns for Stress immunity from Glyph of Sanctuary.</summary>
    private const int GlyphOfSanctuaryStressImmunityDuration = 2;

    /// <summary>Barrier HP created by Indomitable Bastion on save.</summary>
    private const int IndomitableBastionBarrierHp = 30;

    // ===== PP Requirements =====

    /// <summary>PP threshold for Tier 2 ability unlock.</summary>
    private const int Tier2PpRequirement = 8;

    /// <summary>PP threshold for Tier 3 ability unlock.</summary>
    private const int Tier3PpRequirement = 16;

    /// <summary>PP threshold for Capstone ability unlock.</summary>
    private const int CapstonePpRequirement = 24;

    /// <summary>The specialization ID string for Varð-Warden.</summary>
    private const string VardWardenSpecId = "vard-warden";

    private readonly ILogger<VardWardenAbilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VardWardenAbilityService"/> class.
    /// </summary>
    /// <param name="logger">Logger for ability execution events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public VardWardenAbilityService(ILogger<VardWardenAbilityService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===== Tier 1: Runic Barrier (29011) =====

    /// <inheritdoc />
    public RunicBarrierResult? ExecuteRunicBarrier(Player player, int positionX, int positionY, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Guard chain: validate specialization → unlock → AP
        if (!ValidateSpecialization(player, "Runic Barrier")) return null;
        if (!ValidateAbilityUnlocked(player, VardWardenAbilityId.RunicBarrier, "Runic Barrier")) return null;
        if (!ValidateAP(player, RunicBarrierApCost, "Runic Barrier")) return null;

        player.CurrentAP -= RunicBarrierApCost;

        var barrierHp = RunicBarrierHp[rank - 1];
        var duration = RunicBarrierDuration[rank - 1];
        var barrierId = Guid.NewGuid();

        _logger.LogInformation(
            "VardWarden.RunicBarrier: Player {PlayerId} created barrier at ({X}, {Y}) with {HP} HP, {Duration} turns. Rank: {Rank}",
            player.Id, positionX, positionY, barrierHp, duration, rank);

        return new RunicBarrierResult
        {
            IsSuccess = true,
            Description = $"Runic Barrier created at ({positionX}, {positionY})",
            AetherSpent = RunicBarrierApCost,
            BarrierHp = barrierHp,
            Duration = duration,
            PositionX = positionX,
            PositionY = positionY,
            BarrierId = barrierId,
            AbilityRank = rank
        };
    }

    // ===== Tier 1: Consecrate Ground (29012) =====

    /// <inheritdoc />
    public ConsecrateGroundResult? ExecuteConsecrateGround(Player player, int positionX, int positionY, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Consecrate Ground")) return null;
        if (!ValidateAbilityUnlocked(player, VardWardenAbilityId.ConsecrateGround, "Consecrate Ground")) return null;
        if (!ValidateAP(player, ConsecrateGroundApCost, "Consecrate Ground")) return null;

        player.CurrentAP -= ConsecrateGroundApCost;

        // Calculate heal/damage per turn based on rank
        var healPerTurn = rank switch
        {
            1 => RollD6(),                      // 1d6
            2 => RollD6() + 2,                  // 1d6+2
            3 => RollD6() + RollD6(),           // 2d6
            _ => RollD6()
        };

        var damagePerTurn = healPerTurn; // Same as healing
        var duration = 3 + (rank > 1 ? 1 : 0); // 3 turns base, 4 at R2+
        var radius = 2; // Standard zone radius
        var zoneId = Guid.NewGuid();

        _logger.LogInformation(
            "VardWarden.ConsecrateGround: Player {PlayerId} created zone at ({X}, {Y}) with {Heal} heal/{Damage} damage per turn, {Duration} turns. Rank: {Rank}",
            player.Id, positionX, positionY, healPerTurn, damagePerTurn, duration, rank);

        return new ConsecrateGroundResult
        {
            IsSuccess = true,
            Description = $"Consecrate Ground zone created at ({positionX}, {positionY})",
            AetherSpent = ConsecrateGroundApCost,
            HealPerTurn = healPerTurn,
            DamagePerTurn = damagePerTurn,
            Duration = duration,
            Radius = radius,
            PositionX = positionX,
            PositionY = positionY,
            ZoneId = zoneId,
            AbilityRank = rank
        };
    }

    // ===== Tier 2: Rune of Shielding (29013) =====

    /// <inheritdoc />
    public RuneOfShieldingResult? ExecuteRuneOfShielding(Player player, Guid allyId, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Rune of Shielding")) return null;
        if (!ValidateAbilityUnlocked(player, VardWardenAbilityId.RuneOfShielding, "Rune of Shielding")) return null;
        if (!ValidateTierRequirement(player, Tier2PpRequirement, "Rune of Shielding", 2)) return null;
        if (!ValidateAP(player, RuneOfShieldingApCost, "Rune of Shielding")) return null;

        player.CurrentAP -= RuneOfShieldingApCost;

        var soakBonus = RuneOfShieldingSoakBonus[rank - 1];
        var corruptionResistBonus = RuneOfShieldingCorruptionResistBonus[rank - 1];
        var duration = 4;

        _logger.LogInformation(
            "VardWarden.RuneOfShielding: Player {PlayerId} buffed ally {AllyId} with +{Soak} Soak, +{Resist}% Corruption resist. Rank: {Rank}",
            player.Id, allyId, soakBonus, corruptionResistBonus, rank);

        return new RuneOfShieldingResult
        {
            IsSuccess = true,
            Description = $"Rune of Shielding grants +{soakBonus} Soak, +{corruptionResistBonus}% Corruption resist",
            AetherSpent = RuneOfShieldingApCost,
            SoakBonus = soakBonus,
            CorruptionResistBonusPercent = corruptionResistBonus,
            TargetName = allyId.ToString(),
            Duration = duration,
            AbilityRank = rank
        };
    }

    // ===== Tier 2: Reinforce Ward (29014) =====

    /// <inheritdoc />
    public ReinforceWardResult? ExecuteReinforceWard(Player player, Guid barrierOrZoneId, bool isBarrier, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Reinforce Ward")) return null;
        if (!ValidateAbilityUnlocked(player, VardWardenAbilityId.ReinforceWard, "Reinforce Ward")) return null;
        if (!ValidateTierRequirement(player, Tier2PpRequirement, "Reinforce Ward", 2)) return null;
        if (!ValidateAP(player, ReinforceWardApCost, "Reinforce Ward")) return null;

        player.CurrentAP -= ReinforceWardApCost;

        if (isBarrier)
        {
            var hpRestored = ReinforceWardBarrierHealing[rank - 1];
            _logger.LogInformation(
                "VardWarden.ReinforceWard: Player {PlayerId} reinforced barrier {BarrierId} with {HP} HP. Rank: {Rank}",
                player.Id, barrierOrZoneId, hpRestored, rank);

            return new ReinforceWardResult
            {
                IsSuccess = true,
                Description = $"Reinforce Ward restores {hpRestored} HP to barrier",
                AetherSpent = ReinforceWardApCost,
                HpRestored = hpRestored,
                ZoneBoostPercent = null,
                TargetName = barrierOrZoneId.ToString(),
                IsBarrier = true,
                AbilityRank = rank
            };
        }
        else
        {
            var zoneBoostPercent = ReinforceWardZoneBoost[rank - 1];
            _logger.LogInformation(
                "VardWarden.ReinforceWard: Player {PlayerId} boosted zone {ZoneId} by +{Boost}%. Rank: {Rank}",
                player.Id, barrierOrZoneId, zoneBoostPercent, rank);

            return new ReinforceWardResult
            {
                IsSuccess = true,
                Description = $"Reinforce Ward boosts zone by +{zoneBoostPercent}%",
                AetherSpent = ReinforceWardApCost,
                HpRestored = null,
                ZoneBoostPercent = zoneBoostPercent,
                TargetName = barrierOrZoneId.ToString(),
                IsBarrier = false,
                AbilityRank = rank
            };
        }
    }

    // ===== Tier 3: Glyph of Sanctuary (29016) =====

    /// <inheritdoc />
    public GlyphOfSanctuaryResult? ExecuteGlyphOfSanctuary(Player player, IReadOnlyList<Guid> allyIds, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(allyIds);

        if (!ValidateSpecialization(player, "Glyph of Sanctuary")) return null;
        if (!ValidateAbilityUnlocked(player, VardWardenAbilityId.GlyphOfSanctuary, "Glyph of Sanctuary")) return null;
        if (!ValidateTierRequirement(player, Tier3PpRequirement, "Glyph of Sanctuary", 3)) return null;
        if (!ValidateAP(player, GlyphOfSanctuaryApCost, "Glyph of Sanctuary")) return null;

        player.CurrentAP -= GlyphOfSanctuaryApCost;

        // Temp HP: 3d10 per ally
        var tempHpPerAlly = RollD10() + RollD10() + RollD10();
        var alliesAffected = allyIds.Count;
        var allyNames = allyIds.Select(id => id.ToString()).ToArray();

        _logger.LogInformation(
            "VardWarden.GlyphOfSanctuary: Player {PlayerId} granted {TempHp} temp HP to {Count} allies, " +
            "Stress immunity for {Duration} turns. Rank: {Rank}",
            player.Id, tempHpPerAlly, alliesAffected, GlyphOfSanctuaryStressImmunityDuration, rank);

        return new GlyphOfSanctuaryResult
        {
            IsSuccess = true,
            Description = $"Glyph of Sanctuary grants {tempHpPerAlly} temp HP and Stress immunity",
            AetherSpent = GlyphOfSanctuaryApCost,
            TempHpPerAlly = tempHpPerAlly,
            AlliesAffected = alliesAffected,
            StressImmunityDuration = GlyphOfSanctuaryStressImmunityDuration,
            AffectedAllyNames = allyNames,
            AbilityRank = rank
        };
    }

    // ===== Capstone: Indomitable Bastion (29018) =====

    /// <inheritdoc />
    public IndomitableBastionResult? ExecuteIndomitableBastion(Player player, Guid allyId, int incomingDamage, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Indomitable Bastion")) return null;
        if (!ValidateAbilityUnlocked(player, VardWardenAbilityId.IndomitableBastion, "Indomitable Bastion")) return null;
        if (!ValidateTierRequirement(player, CapstonePpRequirement, "Indomitable Bastion", 4)) return null;

        // Check if already used this expedition
        if (player.HasUsedIndomitableBastionThisExpedition)
        {
            _logger.LogWarning(
                "Indomitable Bastion failed: {Player} ({PlayerId}) already used this ability once per expedition",
                player.Name, player.Id);
            return null;
        }

        // Mark as used
        player.HasUsedIndomitableBastionThisExpedition = true;

        var barrierCreated = true;
        var damageNegated = incomingDamage;

        _logger.LogInformation(
            "VardWarden.IndomitableBastion: Player {PlayerId} negated {Damage} fatal damage to ally {AllyId}. " +
            "Created {BarrierHp} HP barrier. Once-per-expedition charge used. Rank: {Rank}",
            player.Id, damageNegated, allyId, IndomitableBastionBarrierHp, rank);

        return new IndomitableBastionResult
        {
            IsSuccess = true,
            Description = $"Indomitable Bastion negates {damageNegated} fatal damage",
            DamageNegated = damageNegated,
            BarrierCreated = barrierCreated,
            SavedAllyName = allyId.ToString(),
            UsedExpeditionCharge = true
        };
    }

    // ===== Zone Processing =====

    /// <inheritdoc />
    public ConsecratedZoneTickResult ProcessZoneTick(
        IReadOnlyList<Guid> alliesInZone,
        IReadOnlyList<Guid> enemiesInZone,
        bool isConsecratedGround,
        int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(alliesInZone);
        ArgumentNullException.ThrowIfNull(enemiesInZone);

        if (!isConsecratedGround)
        {
            // Non-consecrated zones don't tick (or have different mechanics)
            return new ConsecratedZoneTickResult
            {
                IsSuccess = true,
                Description = "Zone tick: no effect",
                HealAmount = 0,
                DamageAmount = 0,
                TargetsAffected = 0,
                AffectedTargetNames = Array.Empty<string>()
            };
        }

        // Calculate heal/damage per tick based on rank
        var healAmount = rank switch
        {
            1 => RollD6(),                      // 1d6
            2 => RollD6() + 2,                  // 1d6+2
            3 => RollD6() + RollD6(),           // 2d6
            _ => RollD6()
        };

        var damageAmount = healAmount;
        var allyNames = alliesInZone.Select(id => id.ToString()).ToArray();
        var enemyNames = enemiesInZone.Select(id => id.ToString()).ToArray();
        var totalTargetsAffected = alliesInZone.Count + enemiesInZone.Count;
        var targetNames = alliesInZone.Concat(enemiesInZone).Select(id => id.ToString()).ToArray();

        _logger.LogInformation(
            "VardWarden.ProcessZoneTick: Consecrated zone heals {Allies} allies for {Heal} HP, " +
            "damages {Enemies} enemies for {Damage} HP. Rank: {Rank}",
            alliesInZone.Count, healAmount, enemiesInZone.Count, damageAmount, rank);

        return new ConsecratedZoneTickResult
        {
            IsSuccess = true,
            Description = $"Consecrated zone heals {alliesInZone.Count} allies, damages {enemiesInZone.Count} enemies",
            HealAmount = healAmount,
            DamageAmount = damageAmount,
            TargetsAffected = totalTargetsAffected,
            AffectedTargetNames = targetNames
        };
    }

    // ===== Utility Methods =====

    /// <inheritdoc />
    public string GetAbilityReadiness(Player player, VardWardenAbilityId abilityId)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsVardWarden(player))
            return "Requires Varð-Warden specialization";

        if (!player.HasVardWardenAbilityUnlocked(abilityId))
            return $"{abilityId} is not unlocked";

        // Capstone once-per-expedition check
        if (abilityId == VardWardenAbilityId.IndomitableBastion && player.HasUsedIndomitableBastionThisExpedition)
            return "Indomitable Bastion already used this expedition";

        var ppRequired = GetPpRequiredForAbility(abilityId);
        if (ppRequired > 0)
        {
            var ppInvested = player.GetVardWardenPPInvested();
            if (ppInvested < ppRequired)
                return $"Insufficient PP ({ppInvested}/{ppRequired})";
        }

        var apCost = GetApCostForAbility(abilityId);
        if (apCost > 0 && player.CurrentAP < apCost)
            return $"Insufficient AP ({player.CurrentAP}/{apCost})";

        return "Ready";
    }

    // ===== Validation Helpers =====

    /// <summary>
    /// Validates that the player has the Varð-Warden specialization.
    /// </summary>
    private bool ValidateSpecialization(Player player, string abilityName)
    {
        if (IsVardWarden(player)) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) is not a Varð-Warden (has: {Spec})",
            abilityName, player.Name, player.Id, player.SpecializationId ?? "none");
        return false;
    }

    /// <summary>
    /// Validates that the player has unlocked the specified ability.
    /// </summary>
    private bool ValidateAbilityUnlocked(Player player, VardWardenAbilityId abilityId, string abilityName)
    {
        if (player.HasVardWardenAbilityUnlocked(abilityId)) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) has not unlocked {AbilityId}",
            abilityName, player.Name, player.Id, abilityId);
        return false;
    }

    /// <summary>
    /// Validates that the player meets the PP investment requirement for a tier.
    /// </summary>
    private bool ValidateTierRequirement(Player player, int requiredPP, string abilityName, int tier)
    {
        var invested = player.GetVardWardenPPInvested();
        if (invested >= requiredPP) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) needs {Required} PP invested for Tier {Tier}, " +
            "has {Invested}",
            abilityName, player.Name, player.Id, requiredPP, tier, invested);
        return false;
    }

    /// <summary>
    /// Validates that the player has enough AP for the ability.
    /// </summary>
    private bool ValidateAP(Player player, int requiredAP, string abilityName)
    {
        if (player.CurrentAP >= requiredAP) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) has insufficient AP (need {Required}, have {Available})",
            abilityName, player.Name, player.Id, requiredAP, player.CurrentAP);
        return false;
    }

    /// <summary>
    /// Checks if the player is a Varð-Warden (case-insensitive comparison).
    /// </summary>
    private static bool IsVardWarden(Player player)
    {
        return string.Equals(player.SpecializationId, VardWardenSpecId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the AP cost for a given ability.
    /// </summary>
    private static int GetApCostForAbility(VardWardenAbilityId abilityId)
    {
        return abilityId switch
        {
            VardWardenAbilityId.RunicBarrier => RunicBarrierApCost,
            VardWardenAbilityId.ConsecrateGround => ConsecrateGroundApCost,
            VardWardenAbilityId.RuneOfShielding => RuneOfShieldingApCost,
            VardWardenAbilityId.ReinforceWard => ReinforceWardApCost,
            VardWardenAbilityId.GlyphOfSanctuary => GlyphOfSanctuaryApCost,
            VardWardenAbilityId.IndomitableBastion => IndomitableBastionApCost,
            _ => 0 // Passive abilities have no AP cost
        };
    }

    /// <summary>
    /// Gets the PP requirement for a given ability (0 if Tier 1, 8 for Tier 2, 16 for Tier 3, 24 for Capstone).
    /// </summary>
    private static int GetPpRequiredForAbility(VardWardenAbilityId abilityId)
    {
        return abilityId switch
        {
            // Tier 1: no requirement
            VardWardenAbilityId.SanctifiedResolve => 0,
            VardWardenAbilityId.RunicBarrier => 0,
            VardWardenAbilityId.ConsecrateGround => 0,
            // Tier 2: 8 PP
            VardWardenAbilityId.RuneOfShielding => Tier2PpRequirement,
            VardWardenAbilityId.ReinforceWard => Tier2PpRequirement,
            VardWardenAbilityId.WardensVigil => Tier2PpRequirement,
            // Tier 3: 16 PP
            VardWardenAbilityId.GlyphOfSanctuary => Tier3PpRequirement,
            VardWardenAbilityId.AegisOfSanctity => Tier3PpRequirement,
            // Capstone: 24 PP
            VardWardenAbilityId.IndomitableBastion => CapstonePpRequirement,
            _ => 0
        };
    }

    // ===== Dice Roll Methods (internal virtual for test overriding) =====

    /// <summary>
    /// Rolls a single d6 (1-6). Override in test subclass for deterministic results.
    /// </summary>
    internal virtual int RollD6()
    {
        return Random.Shared.Next(1, 7);
    }

    /// <summary>
    /// Rolls a single d10 (1-10). Override in test subclass for deterministic results.
    /// </summary>
    internal virtual int RollD10()
    {
        return Random.Shared.Next(1, 11);
    }
}
