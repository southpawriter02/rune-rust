using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Bone-Setter Medical Supplies resource management.
/// Manages supply validation, spending, acquisition, and healing bonus calculation.
/// </summary>
/// <remarks>
/// <para>Named <c>BoneSetterMedicalSuppliesService</c> to coexist alongside any future
/// general-purpose inventory or medical services.</para>
/// <para>Key differences from <see cref="BerserkrRageService"/>:</para>
/// <list type="bullet">
/// <item>Immutable operations — all spend/add methods create new <see cref="MedicalSuppliesResource"/>
/// instances and update the player's reference via <see cref="Player.SetMedicalSupplies"/></item>
/// <item>No automatic regeneration — supplies are finite consumables</item>
/// <item>Quality-based bonuses — each supply's quality rating (1–5) affects healing</item>
/// <item>No threshold transitions — unlike Rage, supplies have no level boundaries</item>
/// <item>No Corruption interaction — the Bone-Setter follows the Coherent path</item>
/// </list>
/// </remarks>
public class BoneSetterMedicalSuppliesService : IBoneSetterMedicalSuppliesService
{
    /// <summary>
    /// The specialization ID string for Bone-Setter.
    /// </summary>
    private const string BoneSetterSpecId = "bone-setter";

    private readonly ILogger<BoneSetterMedicalSuppliesService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoneSetterMedicalSuppliesService"/> class.
    /// </summary>
    /// <param name="logger">Logger for supply management events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public BoneSetterMedicalSuppliesService(ILogger<BoneSetterMedicalSuppliesService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void InitializeSupplies(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Initialize empty Medical Supplies inventory with default capacity
        player.InitializeMedicalSupplies();

        _logger.LogInformation(
            "Medical Supplies initialized: {Player} ({PlayerId}) inventory set to " +
            "{Current}/{Max} (empty)",
            player.Name, player.Id, 0, MedicalSuppliesResource.DefaultMaxCapacity);
    }

    /// <inheritdoc />
    public MedicalSuppliesResource? GetSupplies(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.MedicalSupplies;
    }

    /// <inheritdoc />
    public bool ValidateSupplyAvailability(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.MedicalSupplies == null)
        {
            _logger.LogWarning(
                "Supply validation failed: {Player} ({PlayerId}) has no Medical Supplies " +
                "resource initialized",
                player.Name, player.Id);
            return false;
        }

        var hasSupplies = player.MedicalSupplies.GetTotalSupplyCount() > 0;

        if (!hasSupplies)
        {
            _logger.LogWarning(
                "Supply validation failed: {Player} ({PlayerId}) has no supplies remaining. " +
                "Inventory: {Summary}",
                player.Name, player.Id, player.MedicalSupplies.GetInventorySummary());
        }

        return hasSupplies;
    }

    /// <inheritdoc />
    public bool ValidateSupplyAvailability(Player player, MedicalSupplyType type)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.MedicalSupplies == null)
        {
            _logger.LogWarning(
                "Supply validation failed: {Player} ({PlayerId}) has no Medical Supplies " +
                "resource initialized",
                player.Name, player.Id);
            return false;
        }

        var hasType = player.MedicalSupplies.HasSupply(type);

        if (!hasType)
        {
            _logger.LogWarning(
                "Supply validation failed: {Player} ({PlayerId}) has no {SupplyType} available. " +
                "Inventory: {Summary}",
                player.Name, player.Id, type, player.MedicalSupplies.GetInventorySummary());
        }

        return hasType;
    }

    /// <inheritdoc />
    public MedicalSupplyItem? SpendSupply(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.MedicalSupplies == null)
        {
            _logger.LogWarning(
                "SpendSupply failed: {Player} ({PlayerId}) has no Medical Supplies " +
                "resource initialized",
                player.Name, player.Id);
            return null;
        }

        if (player.MedicalSupplies.GetTotalSupplyCount() == 0)
        {
            _logger.LogWarning(
                "SpendSupply failed: {Player} ({PlayerId}) has no supplies remaining. " +
                "Inventory: {Summary}",
                player.Name, player.Id, player.MedicalSupplies.GetInventorySummary());
            return null;
        }

        // Spend lowest quality supply first (immutable — returns new instance)
        var (newResource, spentItem) = player.MedicalSupplies.SpendAnySupply();

        // Swap the player's immutable reference
        player.SetMedicalSupplies(newResource);

        _logger.LogInformation(
            "Supply spent: {Player} ({PlayerId}) consumed {SupplyName} ({SupplyType}, " +
            "Quality: {Quality}). Remaining: {Summary}",
            player.Name, player.Id, spentItem.Name, spentItem.SupplyType,
            spentItem.Quality, newResource.GetInventorySummary());

        return spentItem;
    }

    /// <inheritdoc />
    public MedicalSupplyItem? SpendSupply(Player player, MedicalSupplyType type)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.MedicalSupplies == null)
        {
            _logger.LogWarning(
                "SpendSupply failed: {Player} ({PlayerId}) has no Medical Supplies " +
                "resource initialized",
                player.Name, player.Id);
            return null;
        }

        if (!player.MedicalSupplies.HasSupply(type))
        {
            _logger.LogWarning(
                "SpendSupply failed: {Player} ({PlayerId}) has no {SupplyType} available. " +
                "Inventory: {Summary}",
                player.Name, player.Id, type, player.MedicalSupplies.GetInventorySummary());
            return null;
        }

        // Get the supply that will be consumed (for logging before removal)
        var supplyToSpend = player.MedicalSupplies.Supplies
            .First(s => s.SupplyType == type);

        // Spend by type (immutable — returns new instance)
        var newResource = player.MedicalSupplies.SpendSupply(type);

        // Swap the player's immutable reference
        player.SetMedicalSupplies(newResource);

        _logger.LogInformation(
            "Supply spent: {Player} ({PlayerId}) consumed {SupplyName} ({SupplyType}, " +
            "Quality: {Quality}). Remaining: {Summary}",
            player.Name, player.Id, supplyToSpend.Name, supplyToSpend.SupplyType,
            supplyToSpend.Quality, newResource.GetInventorySummary());

        return supplyToSpend;
    }

    /// <inheritdoc />
    public bool AddSupply(Player player, MedicalSupplyItem item)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(item);

        if (player.MedicalSupplies == null)
        {
            _logger.LogWarning(
                "AddSupply failed: {Player} ({PlayerId}) has no Medical Supplies " +
                "resource initialized",
                player.Name, player.Id);
            return false;
        }

        if (!player.MedicalSupplies.CanCarryMore())
        {
            _logger.LogWarning(
                "AddSupply failed: {Player} ({PlayerId}) inventory is at maximum capacity " +
                "({Current}/{Max}). Cannot add {SupplyName}",
                player.Name, player.Id,
                player.MedicalSupplies.GetTotalSupplyCount(),
                player.MedicalSupplies.MaxCarryCapacity,
                item.Name);
            return false;
        }

        // Add supply (immutable — returns new instance)
        var newResource = player.MedicalSupplies.AddSupply(item);

        // Swap the player's immutable reference
        player.SetMedicalSupplies(newResource);

        _logger.LogInformation(
            "Supply added: {Player} ({PlayerId}) acquired {SupplyName} ({SupplyType}, " +
            "Quality: {Quality}, Source: {Source}). Inventory: {Summary}",
            player.Name, player.Id, item.Name, item.SupplyType,
            item.Quality, item.Source, newResource.GetInventorySummary());

        return true;
    }

    /// <inheritdoc />
    public int CalculateQualityBonus(MedicalSupplyItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.GetHealingBonus();
    }

    /// <inheritdoc />
    public MedicalSupplyItem? GetHighestQualitySupply(Player player, MedicalSupplyType type)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.MedicalSupplies?.GetHighestQualitySupply(type);
    }

    /// <inheritdoc />
    public MedicalSupplyItem? GetHighestQualitySupply(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.MedicalSupplies?.GetHighestQualitySupply();
    }
}
