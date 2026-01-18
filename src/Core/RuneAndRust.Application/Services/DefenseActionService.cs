namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Events;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service for managing defensive combat actions (Block, Dodge, Parry).
/// </summary>
/// <remarks>
/// <para>DefenseActionService provides:</para>
/// <list type="bullet">
///   <item><description>Eligibility checking for each defense type based on equipment and state</description></item>
///   <item><description>Execution of defense actions with dice rolling and damage calculation</description></item>
///   <item><description>Reaction management (consumption and reset)</description></item>
///   <item><description>Event publishing for combat log and UI integration</description></item>
/// </list>
/// <para>Defense mechanics follow these rules:</para>
/// <list type="bullet">
///   <item><description>Block: 50% damage reduction + shield bonus, no reaction cost</description></item>
///   <item><description>Dodge: 1d10+Finesse vs attack roll, avoid on success, costs reaction</description></item>
///   <item><description>Parry: 1d10+Finesse vs attack+2, counter on success, costs reaction</description></item>
/// </list>
/// </remarks>
public class DefenseActionService : IDefenseActionService
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Base damage reduction percentage for Block (50% = 0.5).
    /// </summary>
    private const float BlockBaseReduction = 0.5f;

    /// <summary>
    /// DC bonus added to attack roll for Parry difficulty.
    /// </summary>
    private const int ParryDcBonus = 2;

    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IDiceService _diceService;
    private readonly IGameEventLogger _eventLogger;
    private readonly ILogger<DefenseActionService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the DefenseActionService.
    /// </summary>
    /// <param name="diceService">Service for dice rolling.</param>
    /// <param name="eventLogger">Logger for game events.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
    public DefenseActionService(
        IDiceService diceService,
        IGameEventLogger eventLogger,
        ILogger<DefenseActionService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("DefenseActionService initialized with BlockBaseReduction={Reduction}, ParryDcBonus={DC}",
            BlockBaseReduction, ParryDcBonus);
    }

    // ═══════════════════════════════════════════════════════════════
    // ELIGIBILITY CHECKS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool CanBlock(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var hasShield = HasShieldEquipped(combatant);

        _logger.LogDebug("{Combatant} CanBlock check: HasShield={HasShield}, Result={CanBlock}",
            combatant.DisplayName, hasShield, hasShield);

        return hasShield;
    }

    /// <inheritdoc />
    public bool CanDodge(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var hasReaction = combatant.HasReaction;
        var hasHeavyArmor = HasHeavyArmor(combatant);
        var canDodge = hasReaction && !hasHeavyArmor;

        _logger.LogDebug("{Combatant} CanDodge check: HasReaction={HasReaction}, HeavyArmor={HeavyArmor}, Result={CanDodge}",
            combatant.DisplayName, hasReaction, hasHeavyArmor, canDodge);

        return canDodge;
    }

    /// <inheritdoc />
    public bool CanParry(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var hasReaction = combatant.HasReaction;
        var hasMeleeWeapon = HasMeleeWeapon(combatant);
        var canParry = hasReaction && hasMeleeWeapon;

        _logger.LogDebug("{Combatant} CanParry check: HasReaction={HasReaction}, MeleeWeapon={MeleeWeapon}, Result={CanParry}",
            combatant.DisplayName, hasReaction, hasMeleeWeapon, canParry);

        return canParry;
    }

    // ═══════════════════════════════════════════════════════════════
    // DEFENSE ACTIONS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public BlockResult UseBlock(Combatant combatant, int incomingDamage)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        _logger.LogDebug("{Combatant} attempting Block against {Damage} incoming damage",
            combatant.DisplayName, incomingDamage);

        // Check eligibility
        if (!CanBlock(combatant))
        {
            _logger.LogWarning("{Combatant} cannot Block: no shield equipped", combatant.DisplayName);
            return BlockResult.Failed("Cannot block without a shield equipped");
        }

        // Get shield bonus
        var shieldBonus = GetShieldBonus(combatant);

        // Calculate reduced damage: (incoming * 50%) - shield bonus, minimum 0
        var reducedByPercent = (int)(incomingDamage * BlockBaseReduction);
        var finalDamage = Math.Max(0, reducedByPercent - shieldBonus);
        var damagePrevented = incomingDamage - finalDamage;

        _logger.LogInformation(
            "{Combatant} BLOCKED: {Incoming} damage → {Final} damage ({Prevented} prevented, {Reduction:P0} base + {Shield} shield bonus)",
            combatant.DisplayName, incomingDamage, finalDamage, damagePrevented, BlockBaseReduction, shieldBonus);

        // Log event
        _eventLogger.LogCombat(
            "Block",
            $"{combatant.DisplayName} blocked {damagePrevented} damage with shield",
            data: new Dictionary<string, object>
            {
                ["defenderId"] = combatant.Id,
                ["incomingDamage"] = incomingDamage,
                ["finalDamage"] = finalDamage,
                ["damagePrevented"] = damagePrevented,
                ["shieldBonus"] = shieldBonus
            });

        return BlockResult.Success(finalDamage, damagePrevented, shieldBonus);
    }

    /// <inheritdoc />
    public DodgeResult UseDodge(Combatant combatant, int attackRoll)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        _logger.LogDebug("{Combatant} attempting Dodge against attack roll {AttackRoll}",
            combatant.DisplayName, attackRoll);

        // Check eligibility
        if (!combatant.HasReaction)
        {
            _logger.LogWarning("{Combatant} cannot Dodge: no reaction available", combatant.DisplayName);
            return DodgeResult.NotAllowed("No reaction available");
        }

        if (HasHeavyArmor(combatant))
        {
            _logger.LogWarning("{Combatant} cannot Dodge: wearing heavy armor", combatant.DisplayName);
            return DodgeResult.NotAllowed("Cannot dodge while wearing heavy armor");
        }

        // Consume reaction
        combatant.UseReaction();

        _logger.LogDebug("{Combatant} reaction consumed for Dodge", combatant.DisplayName);

        // Log reaction used event
        _eventLogger.LogCombat(
            "ReactionUsed",
            $"{combatant.DisplayName} used reaction for Dodge",
            data: new Dictionary<string, object>
            {
                ["combatantId"] = combatant.Id,
                ["actionType"] = DefenseActionType.Dodge.ToString()
            });

        // Roll dodge: 1d10 + Finesse modifier
        var finesseModifier = GetFinesseModifier(combatant);
        var roll = _diceService.Roll("1d10");
        var dodgeRoll = roll.Total + finesseModifier;
        var success = dodgeRoll >= attackRoll;

        _logger.LogInformation(
            "{Combatant} DODGE: rolled {Roll} (1d10={RawRoll} + {Finesse} Finesse) vs {Attack} → {Result}",
            combatant.DisplayName, dodgeRoll, roll.Total, finesseModifier, attackRoll,
            success ? "SUCCESS - Attack avoided!" : "FAILED - Attack hits");

        // Log event
        _eventLogger.LogCombat(
            success ? "DodgeSuccess" : "DodgeFailed",
            success
                ? $"{combatant.DisplayName} dodged the attack! (rolled {dodgeRoll} vs {attackRoll})"
                : $"{combatant.DisplayName} failed to dodge (rolled {dodgeRoll} vs {attackRoll})",
            data: new Dictionary<string, object>
            {
                ["defenderId"] = combatant.Id,
                ["dodgeRoll"] = dodgeRoll,
                ["attackRoll"] = attackRoll,
                ["success"] = success
            });

        return success
            ? DodgeResult.Success(dodgeRoll, attackRoll)
            : DodgeResult.Failure(dodgeRoll, attackRoll);
    }

    /// <inheritdoc />
    public ParryResult UseParry(Combatant combatant, Combatant attacker, int attackRoll)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        ArgumentNullException.ThrowIfNull(attacker);

        _logger.LogDebug("{Combatant} attempting Parry against {Attacker}'s attack roll {AttackRoll}",
            combatant.DisplayName, attacker.DisplayName, attackRoll);

        // Check eligibility
        if (!combatant.HasReaction)
        {
            _logger.LogWarning("{Combatant} cannot Parry: no reaction available", combatant.DisplayName);
            return ParryResult.NotAllowed("No reaction available");
        }

        if (!HasMeleeWeapon(combatant))
        {
            _logger.LogWarning("{Combatant} cannot Parry: no melee weapon equipped", combatant.DisplayName);
            return ParryResult.NotAllowed("Parry requires a melee weapon equipped");
        }

        // Consume reaction
        combatant.UseReaction();

        _logger.LogDebug("{Combatant} reaction consumed for Parry", combatant.DisplayName);

        // Log reaction used event
        _eventLogger.LogCombat(
            "ReactionUsed",
            $"{combatant.DisplayName} used reaction for Parry",
            data: new Dictionary<string, object>
            {
                ["combatantId"] = combatant.Id,
                ["actionType"] = DefenseActionType.Parry.ToString()
            });

        // Roll parry: 1d10 + Finesse modifier vs attack + DC bonus
        var finesseModifier = GetFinesseModifier(combatant);
        var roll = _diceService.Roll("1d10");
        var parryRoll = roll.Total + finesseModifier;
        var parryDc = attackRoll + ParryDcBonus;
        var success = parryRoll >= parryDc;

        if (success)
        {
            // Counter-attack!
            var counterResult = ExecuteCounterAttack(combatant, attacker);

            _logger.LogInformation(
                "{Combatant} PARRY SUCCESS: rolled {Roll} (1d10={RawRoll} + {Finesse} Finesse) vs DC {DC} → Deflected and counter-attacked for {Damage} damage!",
                combatant.DisplayName, parryRoll, roll.Total, finesseModifier, parryDc, counterResult.Damage);

            // Log event
            _eventLogger.LogCombat(
                "ParrySuccess",
                $"{combatant.DisplayName} parried {attacker.DisplayName}'s attack and countered for {counterResult.Damage} damage!",
                data: new Dictionary<string, object>
                {
                    ["defenderId"] = combatant.Id,
                    ["attackerId"] = attacker.Id,
                    ["parryRoll"] = parryRoll,
                    ["dc"] = parryDc,
                    ["counterDamage"] = counterResult.Damage
                });

            return ParryResult.SuccessWithCounter(parryRoll, parryDc, counterResult);
        }
        else
        {
            _logger.LogInformation(
                "{Combatant} PARRY FAILED: rolled {Roll} (1d10={RawRoll} + {Finesse} Finesse) vs DC {DC} → Attack hits",
                combatant.DisplayName, parryRoll, roll.Total, finesseModifier, parryDc);

            // Log event
            _eventLogger.LogCombat(
                "ParryFailed",
                $"{combatant.DisplayName} failed to parry (rolled {parryRoll} vs DC {parryDc})",
                data: new Dictionary<string, object>
                {
                    ["defenderId"] = combatant.Id,
                    ["parryRoll"] = parryRoll,
                    ["dc"] = parryDc
                });

            return ParryResult.Failure(parryRoll, parryDc);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // REACTION MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool HasReaction(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        return combatant.HasReaction;
    }

    /// <inheritdoc />
    public void ResetReaction(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var hadReaction = combatant.HasReaction;
        combatant.ResetReaction();

        if (!hadReaction)
        {
            _logger.LogDebug("{Combatant} reaction reset", combatant.DisplayName);

            _eventLogger.LogCombat(
                "ReactionReset",
                $"{combatant.DisplayName}'s reaction restored",
                data: new Dictionary<string, object>
                {
                    ["combatantId"] = combatant.Id
                });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILITY
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<DefenseActionType> GetAvailableDefenses(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var available = new List<DefenseActionType>();

        if (CanBlock(combatant))
            available.Add(DefenseActionType.Block);
        if (CanDodge(combatant))
            available.Add(DefenseActionType.Dodge);
        if (CanParry(combatant))
            available.Add(DefenseActionType.Parry);

        _logger.LogDebug("{Combatant} available defenses: [{Defenses}]",
            combatant.DisplayName, string.Join(", ", available));

        return available;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the combatant has a shield equipped.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>True if a shield is equipped.</returns>
    private static bool HasShieldEquipped(Combatant combatant)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            return combatant.Player.IsSlotOccupied(EquipmentSlot.Shield);
        }

        // Monsters don't have shields in current implementation
        return false;
    }

    /// <summary>
    /// Gets the shield's defense bonus for the combatant.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>The shield defense bonus, or 0 if no shield equipped.</returns>
    private static int GetShieldBonus(Combatant combatant)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            var shield = combatant.Player.GetEquippedItem(EquipmentSlot.Shield);
            return shield?.DefenseBonus ?? 0;
        }

        return 0;
    }

    /// <summary>
    /// Checks if the combatant is wearing heavy armor.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>True if wearing heavy armor.</returns>
    private static bool HasHeavyArmor(Combatant combatant)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            var armor = combatant.Player.GetEquippedItem(EquipmentSlot.Armor);
            return armor?.ArmorType == ArmorType.Heavy;
        }

        // Check monster armor type if applicable (currently monsters don't have armor items)
        return false;
    }

    /// <summary>
    /// Checks if the combatant has a melee weapon equipped.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>True if a melee weapon is equipped.</returns>
    /// <remarks>
    /// All current weapon types (Sword, Axe, Dagger, Staff) are melee weapons.
    /// </remarks>
    private static bool HasMeleeWeapon(Combatant combatant)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            var weapon = combatant.Player.GetEquippedItem(EquipmentSlot.Weapon);
            // All current weapon types are melee (Sword, Axe, Dagger, Staff)
            return weapon?.IsWeapon == true;
        }

        // Monsters with attacks are considered to have melee capability
        if (combatant.IsMonster && combatant.Monster != null)
        {
            // Monsters have innate attacks
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the Finesse modifier for the combatant (used for Dodge/Parry rolls).
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>The Finesse-based modifier.</returns>
    /// <remarks>
    /// Uses Finesse attribute divided by 2 (matching the defense calculation in Stats).
    /// </remarks>
    private static int GetFinesseModifier(Combatant combatant)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            // Match defense calculation: Finesse / 2
            return combatant.Player.GetEffectiveAttributes().Finesse / 2;
        }

        // Monsters use their initiative modifier as a proxy for dexterity
        if (combatant.IsMonster && combatant.Monster != null)
        {
            return combatant.Monster.InitiativeModifier;
        }

        return 0;
    }

    /// <summary>
    /// Executes a counter-attack as part of a successful parry.
    /// </summary>
    /// <param name="attacker">The combatant making the counter-attack.</param>
    /// <param name="target">The target of the counter-attack.</param>
    /// <returns>The result of the counter-attack.</returns>
    private CounterAttackResult ExecuteCounterAttack(Combatant attacker, Combatant target)
    {
        // Roll attack: 1d10 + attack modifier
        var attackMod = GetAttackModifier(attacker);
        var attackRollResult = _diceService.Roll("1d10");
        var attackRoll = attackRollResult.Total + attackMod;

        // Determine target defense
        var targetDefense = GetDefenseValue(target);

        var isHit = attackRoll >= targetDefense;
        var isCritical = attackRollResult.Total >= 10; // Natural 10 is critical on d10, use 20 for d20

        if (isHit)
        {
            // Roll damage
            var damageDice = GetDamageDice(attacker);
            var damageRoll = _diceService.Roll(damageDice);
            var damage = isCritical ? damageRoll.Total * 2 : damageRoll.Total;

            _logger.LogDebug(
                "Counter-attack: {Attacker} rolled {Attack} vs {Defense}, HIT for {Damage} damage{Critical}",
                attacker.DisplayName, attackRoll, targetDefense, damage, isCritical ? " (CRITICAL!)" : "");

            return CounterAttackResult.Hit(attackRoll, damage, isCritical);
        }
        else
        {
            _logger.LogDebug(
                "Counter-attack: {Attacker} rolled {Attack} vs {Defense}, MISS",
                attacker.DisplayName, attackRoll, targetDefense);

            return CounterAttackResult.Miss(attackRoll);
        }
    }

    /// <summary>
    /// Gets the attack modifier for counter-attacks.
    /// </summary>
    private static int GetAttackModifier(Combatant combatant)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            return combatant.Player.Stats.Attack;
        }

        if (combatant.IsMonster && combatant.Monster != null)
        {
            return combatant.Monster.Stats.Attack;
        }

        return 0;
    }

    /// <summary>
    /// Gets the defense value for determining counter-attack hits.
    /// </summary>
    private static int GetDefenseValue(Combatant combatant)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            return 10 + combatant.Player.Stats.Defense;
        }

        if (combatant.IsMonster && combatant.Monster != null)
        {
            return 10 + combatant.Monster.Stats.Defense;
        }

        return 10;
    }

    /// <summary>
    /// Gets the damage dice for counter-attacks.
    /// </summary>
    private static string GetDamageDice(Combatant combatant)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            var weapon = combatant.Player.GetEquippedItem(EquipmentSlot.Weapon);
            return weapon?.DamageDice ?? "1d4"; // Unarmed fallback
        }

        if (combatant.IsMonster && combatant.Monster != null)
        {
            // Monsters use their attack stat to determine damage dice
            // Scale: Attack 5-8 = 1d6, 9-12 = 1d8, 13-16 = 1d10, 17+ = 2d6
            var attack = combatant.Monster.Stats.Attack;
            return attack switch
            {
                < 9 => "1d6",
                < 13 => "1d8",
                < 17 => "1d10",
                _ => "2d6"
            };
        }

        return "1d4";
    }
}
