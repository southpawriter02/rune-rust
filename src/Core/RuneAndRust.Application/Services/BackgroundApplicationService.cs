// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundApplicationService.cs
// Service that orchestrates applying background grants (skills, equipment)
// to characters during creation.
// Version: 0.17.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Applies backgrounds to characters during creation, granting skill bonuses
/// and starting equipment.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BackgroundApplicationService"/> is the primary orchestrator for applying
/// all background components to a <see cref="Player"/> entity. It retrieves background
/// definitions from <see cref="IBackgroundProvider"/> and applies them in the following order:
/// </para>
/// <list type="number">
///   <item><description>Retrieve background definition from provider</description></item>
///   <item><description>Apply skill grants (Permanent bonuses to character skills)</description></item>
///   <item><description>Apply equipment grants (create items, add to inventory, auto-equip)</description></item>
///   <item><description>Set the background identifier on the character entity</description></item>
/// </list>
/// <para>
/// <strong>Skill Application:</strong> Each background grants a primary skill (+2) and
/// secondary skill (+1), both as Permanent grants. Skills are applied via
/// <see cref="Player.ModifySkill"/> which creates the skill if it doesn't exist.
/// </para>
/// <para>
/// <strong>Equipment Application:</strong> Equipment items are created as <see cref="Item"/>
/// instances, added to the character's <see cref="Inventory"/> via <c>TryAdd</c>, and
/// optionally auto-equipped to their designated <see cref="EquipmentSlot"/> via <c>TryEquip</c>.
/// </para>
/// <para>
/// <strong>Idempotency:</strong> The service does not currently check for duplicate application.
/// That responsibility belongs to the character creation workflow (v0.17.5) which ensures
/// each step is only applied once.
/// </para>
/// </remarks>
/// <seealso cref="IBackgroundApplicationService"/>
/// <seealso cref="IBackgroundProvider"/>
/// <seealso cref="BackgroundApplicationResult"/>
public class BackgroundApplicationService : IBackgroundApplicationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for accessing background definitions from configuration.
    /// </summary>
    private readonly IBackgroundProvider _backgroundProvider;

    /// <summary>
    /// Logger for structured diagnostic output.
    /// </summary>
    private readonly ILogger<BackgroundApplicationService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundApplicationService"/> class.
    /// </summary>
    /// <param name="backgroundProvider">
    /// The background provider for accessing background definitions.
    /// Must not be null.
    /// </param>
    /// <param name="logger">
    /// The logger for diagnostic output. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="backgroundProvider"/> or <paramref name="logger"/> is null.
    /// </exception>
    public BackgroundApplicationService(
        IBackgroundProvider backgroundProvider,
        ILogger<BackgroundApplicationService> logger)
    {
        ArgumentNullException.ThrowIfNull(backgroundProvider, nameof(backgroundProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _backgroundProvider = backgroundProvider;
        _logger = logger;

        _logger.LogDebug(
            "BackgroundApplicationService initialized with IBackgroundProvider");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public BackgroundApplicationResult ApplyBackgroundToCharacter(
        Player character,
        Background background)
    {
        _logger.LogInformation(
            "Beginning background application. Background={Background}, " +
            "CharacterId={CharacterId}, CharacterName={CharacterName}",
            background,
            character?.Id.ToString() ?? "null",
            character?.Name ?? "null");

        // Step 1: Validate character is not null
        if (character == null)
        {
            _logger.LogWarning(
                "Background application failed: character is null. Background={Background}",
                background);
            return BackgroundApplicationResult.Failed(
                background, "Character cannot be null");
        }

        // Step 2: Get background definition from provider
        var definition = _backgroundProvider.GetBackground(background);
        if (definition == null)
        {
            _logger.LogWarning(
                "Background application failed: background not found. " +
                "Background={Background}, CharacterId={CharacterId}",
                background,
                character.Id);
            return BackgroundApplicationResult.Failed(
                background,
                $"Background '{background}' not found in provider");
        }

        _logger.LogDebug(
            "Retrieved background definition. Background={Background}, " +
            "DisplayName={DisplayName}, DefinitionId={DefinitionId}, " +
            "SkillGrantCount={SkillGrantCount}, EquipmentGrantCount={EquipmentGrantCount}",
            background,
            definition.DisplayName,
            definition.Id,
            definition.SkillGrants.Count,
            definition.EquipmentGrants.Count);

        var grantedSkills = new List<GrantedSkill>();
        var grantedEquipment = new List<GrantedEquipment>();

        // Step 3: Apply skill grants
        _logger.LogDebug(
            "Applying {SkillGrantCount} skill grants for background {Background}",
            definition.SkillGrants.Count,
            background);

        foreach (var skillGrant in definition.SkillGrants)
        {
            ApplySkillGrant(character, skillGrant);
            grantedSkills.Add(new GrantedSkill(
                skillGrant.SkillId,
                skillGrant.BonusAmount,
                skillGrant.GrantType));

            _logger.LogDebug(
                "Applied skill grant: SkillId={SkillId}, Amount={Amount:+#;-#;0}, " +
                "GrantType={GrantType}, Background={Background}",
                skillGrant.SkillId,
                skillGrant.BonusAmount,
                skillGrant.GrantType,
                background);
        }

        // Step 4: Apply equipment grants
        _logger.LogDebug(
            "Applying {EquipmentGrantCount} equipment grants for background {Background}",
            definition.EquipmentGrants.Count,
            background);

        foreach (var equipGrant in definition.EquipmentGrants)
        {
            var wasEquipped = ApplyEquipmentGrant(character, equipGrant);
            grantedEquipment.Add(new GrantedEquipment(
                equipGrant.ItemId,
                equipGrant.Quantity,
                wasEquipped,
                equipGrant.Slot));

            _logger.LogDebug(
                "Applied equipment grant: ItemId={ItemId}, Quantity={Quantity}, " +
                "IsEquipped={IsEquipped}, WasEquipped={WasEquipped}, " +
                "Slot={Slot}, Background={Background}",
                equipGrant.ItemId,
                equipGrant.Quantity,
                equipGrant.IsEquipped,
                wasEquipped,
                equipGrant.Slot?.ToString() ?? "None",
                background);
        }

        // Step 5: Set character's background
        character.SetBackground(background);

        _logger.LogDebug(
            "Set background on character. CharacterId={CharacterId}, " +
            "Background={Background}, HasBackground={HasBackground}",
            character.Id,
            background,
            character.HasBackground);

        _logger.LogInformation(
            "Successfully applied background {Background} to character {CharacterName}. " +
            "SkillsGranted={SkillCount} ({SkillSummary}), " +
            "EquipmentGranted={EquipCount} ({EquipmentSummary}), " +
            "CharacterId={CharacterId}",
            background,
            character.Name,
            grantedSkills.Count,
            string.Join(", ", grantedSkills.Select(s => s.ToString())),
            grantedEquipment.Count,
            string.Join(", ", grantedEquipment.Select(e => e.ToString())),
            character.Id);

        return BackgroundApplicationResult.Succeeded(
            background,
            grantedSkills,
            grantedEquipment,
            $"Applied {definition.DisplayName} background successfully");
    }

    /// <inheritdoc />
    public BackgroundPreview GetBackgroundPreview(Background background)
    {
        _logger.LogDebug(
            "Generating background preview. Background={Background}",
            background);

        var definition = _backgroundProvider.GetBackground(background);
        if (definition == null)
        {
            _logger.LogWarning(
                "Cannot generate preview: background {Background} not found in provider",
                background);
            return BackgroundPreview.Empty(background);
        }

        var preview = new BackgroundPreview
        {
            BackgroundId = background,
            DisplayName = definition.DisplayName,
            Description = definition.Description,
            SelectionText = definition.SelectionText,
            ProfessionBefore = definition.ProfessionBefore,
            SocialStanding = definition.SocialStanding,
            SkillSummary = definition.GetSkillGrantSummary(),
            EquipmentSummary = definition.GetEquipmentGrantSummary(),
            NarrativeHookCount = definition.NarrativeHooks.Count
        };

        _logger.LogDebug(
            "Generated background preview. Background={Background}, " +
            "DisplayName={DisplayName}, SkillSummary={SkillSummary}, " +
            "EquipmentSummary={EquipmentSummary}, NarrativeHookCount={NarrativeHookCount}",
            background,
            preview.DisplayName,
            preview.SkillSummary,
            preview.EquipmentSummary,
            preview.NarrativeHookCount);

        return preview;
    }

    /// <inheritdoc />
    public IReadOnlyList<BackgroundPreview> GetAllBackgroundPreviews()
    {
        _logger.LogDebug("Generating previews for all backgrounds");

        var backgrounds = _backgroundProvider.GetAllBackgrounds();

        _logger.LogDebug(
            "Retrieved {BackgroundCount} backgrounds from provider for preview generation",
            backgrounds.Count);

        var previews = backgrounds
            .Select(def => GetBackgroundPreview(def.BackgroundId))
            .ToList();

        _logger.LogDebug(
            "Generated {PreviewCount} background previews",
            previews.Count);

        return previews;
    }

    /// <inheritdoc />
    public bool CanApplyBackground(Background background)
    {
        var exists = _backgroundProvider.HasBackground(background);

        _logger.LogDebug(
            "Checked background availability. Background={Background}, " +
            "Exists={Exists}",
            background,
            exists);

        return exists;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies a single skill grant to the character.
    /// </summary>
    /// <param name="character">The player character to modify.</param>
    /// <param name="grant">The skill grant to apply.</param>
    /// <remarks>
    /// <para>
    /// Delegates to <see cref="Player.ModifySkill"/> which creates the skill if it
    /// doesn't exist and records the bonus. The grant type determines how the bonus
    /// is applied:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="SkillGrantType.Permanent"/>: Added to base skill value via ModifySkill</description></item>
    ///   <item><description><see cref="SkillGrantType.StartingBonus"/>: Applied as starting bonus via ModifySkill</description></item>
    ///   <item><description><see cref="SkillGrantType.Proficiency"/>: Logged but not currently applied (future implementation)</description></item>
    /// </list>
    /// </remarks>
    private void ApplySkillGrant(Player character, BackgroundSkillGrant grant)
    {
        switch (grant.GrantType)
        {
            case SkillGrantType.Permanent:
                // Apply permanent skill bonus to the character
                character.ModifySkill(grant.SkillId, grant.BonusAmount);
                _logger.LogDebug(
                    "Applied permanent skill grant. SkillId={SkillId}, " +
                    "BonusAmount={BonusAmount:+#;-#;0}",
                    grant.SkillId,
                    grant.BonusAmount);
                break;

            case SkillGrantType.StartingBonus:
                // Starting bonuses are applied via ModifySkill as well;
                // the distinction is tracked in the grant type for future use
                character.ModifySkill(grant.SkillId, grant.BonusAmount);
                _logger.LogDebug(
                    "Applied starting skill bonus. SkillId={SkillId}, " +
                    "BonusAmount={BonusAmount:+#;-#;0}",
                    grant.SkillId,
                    grant.BonusAmount);
                break;

            case SkillGrantType.Proficiency:
                // Proficiency grants unlock skill use without penalty;
                // currently tracked but not mechanically applied (future implementation)
                _logger.LogDebug(
                    "Proficiency grant noted for future implementation. " +
                    "SkillId={SkillId}",
                    grant.SkillId);
                break;

            default:
                _logger.LogWarning(
                    "Unknown skill grant type encountered. SkillId={SkillId}, " +
                    "GrantType={GrantType}, BonusAmount={BonusAmount}",
                    grant.SkillId,
                    grant.GrantType,
                    grant.BonusAmount);
                break;
        }
    }

    /// <summary>
    /// Applies a single equipment grant to the character by creating an item,
    /// adding it to inventory, and optionally equipping it.
    /// </summary>
    /// <param name="character">The player character to modify.</param>
    /// <param name="grant">The equipment grant to apply.</param>
    /// <returns>
    /// <c>true</c> if the item was auto-equipped to a slot;
    /// <c>false</c> if it was only placed in inventory.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Items are created using the <see cref="Item"/> constructor with minimal
    /// properties. The item type is inferred from the equipment slot:
    /// <list type="bullet">
    ///   <item><description>Weapon slot → <see cref="ItemType.Weapon"/></description></item>
    ///   <item><description>Armor/Shield/Helmet/Boots slot → <see cref="ItemType.Armor"/></description></item>
    ///   <item><description>No slot → <see cref="ItemType.Misc"/> (general inventory items)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// For stackable items (quantity > 1), individual items are created and added
    /// separately to the inventory. This matches the existing inventory model which
    /// tracks individual item instances.
    /// </para>
    /// </remarks>
    private bool ApplyEquipmentGrant(Player character, BackgroundEquipmentGrant grant)
    {
        // Determine item type based on equipment slot
        var itemType = DetermineItemType(grant.Slot);

        _logger.LogDebug(
            "Creating item for equipment grant. ItemId={ItemId}, Quantity={Quantity}, " +
            "ItemType={ItemType}, IsEquipped={IsEquipped}, Slot={Slot}",
            grant.ItemId,
            grant.Quantity,
            itemType,
            grant.IsEquipped,
            grant.Slot?.ToString() ?? "None");

        // Create and add items to inventory
        // For quantity > 1, we create individual instances (matching the inventory model)
        for (var i = 0; i < grant.Quantity; i++)
        {
            var item = new Item(
                name: FormatItemName(grant.ItemId),
                description: $"Starting equipment from background",
                type: itemType,
                equipmentSlot: grant.Slot);

            var added = character.Inventory.TryAdd(item);
            if (!added)
            {
                _logger.LogWarning(
                    "Failed to add item to inventory (inventory may be full). " +
                    "ItemId={ItemId}, ItemName={ItemName}, Iteration={Iteration}",
                    grant.ItemId,
                    item.Name,
                    i + 1);
                continue;
            }

            _logger.LogDebug(
                "Added item to inventory. ItemId={ItemId}, ItemName={ItemName}, " +
                "InventoryItemId={InventoryItemId}, Iteration={Iteration}/{Total}",
                grant.ItemId,
                item.Name,
                item.Id,
                i + 1,
                grant.Quantity);

            // Auto-equip the first item if marked as equipped and slot is available
            if (i == 0 && grant.IsEquipped && grant.Slot.HasValue)
            {
                var equipped = character.TryEquip(item);
                if (equipped)
                {
                    _logger.LogDebug(
                        "Auto-equipped item to slot. ItemId={ItemId}, " +
                        "ItemName={ItemName}, Slot={Slot}",
                        grant.ItemId,
                        item.Name,
                        grant.Slot.Value);
                    return true;
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to auto-equip item (slot may be occupied). " +
                        "ItemId={ItemId}, ItemName={ItemName}, Slot={Slot}",
                        grant.ItemId,
                        item.Name,
                        grant.Slot.Value);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines the appropriate <see cref="ItemType"/> based on the equipment slot.
    /// </summary>
    /// <param name="slot">The equipment slot, or <c>null</c> for inventory-only items.</param>
    /// <returns>
    /// <see cref="ItemType.Weapon"/> for Weapon slots,
    /// <see cref="ItemType.Armor"/> for defensive slots (Armor, Shield, Helmet, Boots),
    /// or <see cref="ItemType.Misc"/> for items without a slot.
    /// </returns>
    private static ItemType DetermineItemType(EquipmentSlot? slot) =>
        slot switch
        {
            EquipmentSlot.Weapon => ItemType.Weapon,
            EquipmentSlot.Armor => ItemType.Armor,
            EquipmentSlot.Shield => ItemType.Armor,
            EquipmentSlot.Helmet => ItemType.Armor,
            EquipmentSlot.Boots => ItemType.Armor,
            _ => ItemType.Misc
        };

    /// <summary>
    /// Formats a kebab-case item ID into a human-readable display name.
    /// </summary>
    /// <param name="itemId">The item ID in kebab-case (e.g., "smiths-hammer").</param>
    /// <returns>
    /// A title-cased display name (e.g., "Smiths Hammer").
    /// </returns>
    /// <remarks>
    /// Replaces hyphens with spaces and title-cases each word. This provides a
    /// reasonable display name for items created from configuration data.
    /// </remarks>
    private static string FormatItemName(string itemId)
    {
        // Convert kebab-case to title case: "smiths-hammer" → "Smiths Hammer"
        var words = itemId.Split('-');
        return string.Join(" ", words.Select(w =>
            w.Length > 0
                ? char.ToUpperInvariant(w[0]) + w[1..]
                : w));
    }
}
