// ═══════════════════════════════════════════════════════════════════════════════
// AbilitySlotService.cs
// Service that manages the tiered ability slot structure for character
// specializations in the Aethelgard system.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages the tiered ability slot structure for character specializations,
/// including initialization, tier unlocking, and PP cost calculations.
/// </summary>
/// <remarks>
/// <para>
/// The ability slot system uses a fixed 4-tier layout:
/// </para>
/// <list type="bullet">
///   <item><description><b>Tier 1:</b> 3 slots — Free (0 PP), unlocked immediately</description></item>
///   <item><description><b>Tier 2:</b> 3 slots — 4 PP each, requires 8 PP invested</description></item>
///   <item><description><b>Tier 3:</b> 2 slots — 5 PP each, requires 16 PP invested</description></item>
///   <item><description><b>Capstone:</b> 1 slot — 6 PP, requires 24 PP invested</description></item>
/// </list>
/// <para>
/// Slot data is stored in-memory keyed by (CharacterId, SpecializationId).
/// PP investment tracking is deferred to v0.20.1+.
/// </para>
/// </remarks>
/// <seealso cref="IAbilitySlotService"/>
/// <seealso cref="AbilitySlotPreparation"/>
public sealed class AbilitySlotService : IAbilitySlotService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Standard Tier 1 slot count.</summary>
    private const int DefaultTier1Slots = 3;

    /// <summary>Standard Tier 2 slot count.</summary>
    private const int DefaultTier2Slots = 3;

    /// <summary>Standard Tier 3 slot count.</summary>
    private const int DefaultTier3Slots = 2;

    /// <summary>Standard Capstone slot count.</summary>
    private const int DefaultCapstoneSlots = 1;

    /// <summary>PP cost per Tier 2 slot.</summary>
    private const int Tier2CostPerSlot = 4;

    /// <summary>PP cost per Tier 3 slot.</summary>
    private const int Tier3CostPerSlot = 5;

    /// <summary>PP cost for Capstone slot.</summary>
    private const int CapstoneCostPerSlot = 6;

    /// <summary>PP investment required to unlock Tier 2.</summary>
    private const int Tier2PpThreshold = 8;

    /// <summary>PP investment required to unlock Tier 3.</summary>
    private const int Tier3PpThreshold = 16;

    /// <summary>PP investment required to unlock Capstone.</summary>
    private const int CapstonePpThreshold = 24;

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initialized ability slot preparations keyed by (CharacterId, SpecializationId).
    /// </summary>
    private readonly Dictionary<(Guid CharacterId, SpecializationId SpecId), AbilitySlotPreparation> _slots = new();

    /// <summary>
    /// PP investment tracking per (CharacterId, SpecializationId).
    /// In v0.20.1+ this will be persisted.
    /// </summary>
    private readonly Dictionary<(Guid CharacterId, SpecializationId SpecId), int> _ppInvested = new();

    /// <summary>
    /// Logger for structured diagnostic output.
    /// </summary>
    private readonly ILogger<AbilitySlotService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="AbilitySlotService"/> class.
    /// </summary>
    /// <param name="logger">
    /// The logger for diagnostic output. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public AbilitySlotService(ILogger<AbilitySlotService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _logger = logger;

        _logger.LogDebug("AbilitySlotService initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — IAbilitySlotService
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public AbilitySlotPreparation InitializeAbilitySlots(Guid characterId, SpecializationId specialization)
    {
        var key = (characterId, specialization);

        _logger.LogInformation(
            "Initializing ability slots. CharacterId={CharacterId}, " +
            "Specialization={Specialization}",
            characterId,
            specialization);

        var preparation = new AbilitySlotPreparation
        {
            CharacterId = characterId,
            SpecializationId = specialization,
            Tier1Slots = DefaultTier1Slots,
            Tier2Slots = DefaultTier2Slots,
            Tier3Slots = DefaultTier3Slots,
            CapstoneSlots = DefaultCapstoneSlots
        };

        _slots[key] = preparation;
        _ppInvested[key] = 0;

        _logger.LogInformation(
            "Ability slots initialized. CharacterId={CharacterId}, " +
            "Specialization={Specialization}, TotalSlots={TotalSlots}, " +
            "Layout=Tier1({Tier1})+Tier2({Tier2})+Tier3({Tier3})+Capstone({Capstone})",
            characterId,
            specialization,
            preparation.TotalSlots,
            preparation.Tier1Slots,
            preparation.Tier2Slots,
            preparation.Tier3Slots,
            preparation.CapstoneSlots);

        return preparation;
    }

    /// <inheritdoc />
    public AbilitySlotPreparation? GetAbilitySlots(Guid characterId, SpecializationId specialization)
    {
        var key = (characterId, specialization);

        if (_slots.TryGetValue(key, out var preparation))
        {
            _logger.LogDebug(
                "Retrieved ability slots. CharacterId={CharacterId}, " +
                "Specialization={Specialization}, TotalSlots={TotalSlots}",
                characterId,
                specialization,
                preparation.TotalSlots);

            return preparation;
        }

        _logger.LogDebug(
            "No ability slots found. CharacterId={CharacterId}, " +
            "Specialization={Specialization}",
            characterId,
            specialization);

        return null;
    }

    /// <inheritdoc />
    public bool IsTierUnlocked(Guid characterId, SpecializationId specialization, int tier)
    {
        var key = (characterId, specialization);
        var invested = _ppInvested.GetValueOrDefault(key, 0);

        var isUnlocked = tier switch
        {
            1 => true,
            2 => invested >= Tier2PpThreshold,
            3 => invested >= Tier3PpThreshold,
            4 => invested >= CapstonePpThreshold,
            _ => false
        };

        _logger.LogDebug(
            "Checked tier unlock status. CharacterId={CharacterId}, " +
            "Specialization={Specialization}, Tier={Tier}, " +
            "PpInvested={PpInvested}, IsUnlocked={IsUnlocked}",
            characterId,
            specialization,
            tier,
            invested,
            isUnlocked);

        return isUnlocked;
    }

    /// <inheritdoc />
    public int GetNextTierUnlockCost(Guid characterId, SpecializationId specialization)
    {
        var key = (characterId, specialization);
        var invested = _ppInvested.GetValueOrDefault(key, 0);

        int cost;
        int nextTier;

        if (invested < Tier2PpThreshold)
        {
            cost = Tier2CostPerSlot;
            nextTier = 2;
        }
        else if (invested < Tier3PpThreshold)
        {
            cost = Tier3CostPerSlot;
            nextTier = 3;
        }
        else if (invested < CapstonePpThreshold)
        {
            cost = CapstoneCostPerSlot;
            nextTier = 4;
        }
        else
        {
            _logger.LogDebug(
                "All tiers unlocked. CharacterId={CharacterId}, " +
                "Specialization={Specialization}",
                characterId,
                specialization);

            return 0;
        }

        _logger.LogDebug(
            "Calculated next tier unlock cost. CharacterId={CharacterId}, " +
            "Specialization={Specialization}, NextTier={NextTier}, " +
            "Cost={Cost}, PpInvested={PpInvested}",
            characterId,
            specialization,
            nextTier,
            cost,
            invested);

        return cost;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — PP TRACKING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Records PP investment for a character's specialization.
    /// Used to track tier unlocking progress.
    /// </summary>
    /// <param name="characterId">The character investing PP.</param>
    /// <param name="specialization">The specialization receiving investment.</param>
    /// <param name="ppAmount">The amount of PP to invest.</param>
    public void InvestPP(Guid characterId, SpecializationId specialization, int ppAmount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ppAmount);

        var key = (characterId, specialization);
        var current = _ppInvested.GetValueOrDefault(key, 0);
        _ppInvested[key] = current + ppAmount;

        _logger.LogInformation(
            "PP invested. CharacterId={CharacterId}, Specialization={Specialization}, " +
            "Amount={Amount}, TotalInvested={TotalInvested}",
            characterId,
            specialization,
            ppAmount,
            _ppInvested[key]);
    }
}
