using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Effects;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for executing dialogue effects on characters.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public class DialogueEffectExecutor : IDialogueEffectExecutor
{
    private readonly IFactionService _factionService;
    private readonly IInventoryService _inventoryService;
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<DialogueEffectExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the DialogueEffectExecutor.
    /// </summary>
    public DialogueEffectExecutor(
        IFactionService factionService,
        IInventoryService inventoryService,
        IItemRepository itemRepository,
        ILogger<DialogueEffectExecutor> logger)
    {
        _factionService = factionService ?? throw new ArgumentNullException(nameof(factionService));
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<DialogueEffectResult> ExecuteEffectAsync(
        Character character,
        DialogueEffect effect,
        GameState gameState)
    {
        _logger.LogDebug(
            "Executing dialogue effect {EffectType} for character {CharacterId}",
            effect.Type,
            character.Id);

        try
        {
            return effect switch
            {
                ModifyReputationEffect repEffect => await ExecuteModifyReputationAsync(character, repEffect),
                GiveItemEffect itemEffect => await ExecuteGiveItemAsync(character, itemEffect),
                SetFlagEffect flagEffect => ExecuteSetFlag(flagEffect, gameState),
                _ => DialogueEffectResult.Failed(effect.Type, $"Unknown effect type: {effect.Type}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error executing dialogue effect {EffectType} for character {CharacterId}",
                effect.Type,
                character.Id);

            return DialogueEffectResult.Failed(effect.Type, ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DialogueEffectResult>> ExecuteEffectsAsync(
        Character character,
        IEnumerable<DialogueEffect> effects,
        GameState gameState)
    {
        var results = new List<DialogueEffectResult>();

        foreach (var effect in effects)
        {
            var result = await ExecuteEffectAsync(character, effect, gameState);
            results.Add(result);

            _logger.LogDebug(
                "Effect {EffectType} result: {Success} - {Description}",
                effect.Type,
                result.Success,
                result.Success ? result.Description : result.ErrorMessage);
        }

        return results;
    }

    /// <summary>
    /// Executes a ModifyReputation effect.
    /// </summary>
    private async Task<DialogueEffectResult> ExecuteModifyReputationAsync(
        Character character,
        ModifyReputationEffect effect)
    {
        _logger.LogDebug(
            "Modifying reputation with {Faction} by {Amount} for character {CharacterId}",
            effect.Faction,
            effect.Amount,
            character.Id);

        var result = await _factionService.ModifyReputationAsync(
            character,
            effect.Faction,
            effect.Amount,
            "dialogue effect");

        if (result.Success)
        {
            var description = $"Reputation with {effect.Faction} changed by {effect.Amount} " +
                              $"({result.OldValue} -> {result.NewValue})";

            _logger.LogInformation(
                "Reputation modified: {Description}",
                description);

            return DialogueEffectResult.Successful(DialogueEffectType.ModifyReputation, description);
        }

        return DialogueEffectResult.Failed(
            DialogueEffectType.ModifyReputation,
            result.Message ?? "Failed to modify reputation");
    }

    /// <summary>
    /// Executes a GiveItem effect.
    /// </summary>
    private async Task<DialogueEffectResult> ExecuteGiveItemAsync(
        Character character,
        GiveItemEffect effect)
    {
        _logger.LogDebug(
            "Giving item {ItemId} x{Quantity} to character {CharacterId}",
            effect.ItemId,
            effect.Quantity,
            character.Id);

        // Get the item from the repository
        var item = await _itemRepository.GetByIdAsync(effect.ItemId);

        if (item == null)
        {
            _logger.LogWarning("Item {ItemId} not found for GiveItem effect", effect.ItemId);
            return DialogueEffectResult.Failed(
                DialogueEffectType.GiveItem,
                $"Item '{effect.ItemName}' not found");
        }

        var result = await _inventoryService.AddItemAsync(character, item, effect.Quantity);

        if (result.Success)
        {
            var description = effect.Quantity > 1
                ? $"Received {item.Name} x{effect.Quantity}"
                : $"Received {item.Name}";

            _logger.LogInformation(
                "Item given: {Description} to character {CharacterId}",
                description,
                character.Id);

            return DialogueEffectResult.Successful(DialogueEffectType.GiveItem, description);
        }

        return DialogueEffectResult.Failed(DialogueEffectType.GiveItem, result.Message);
    }

    /// <summary>
    /// Executes a SetFlag effect.
    /// </summary>
    private DialogueEffectResult ExecuteSetFlag(SetFlagEffect effect, GameState gameState)
    {
        _logger.LogDebug(
            "Setting flag {FlagKey} to {Value}",
            effect.FlagKey,
            effect.Value);

        gameState.SetFlag(effect.FlagKey, effect.Value);

        var description = effect.Value
            ? $"Flag '{effect.FlagKey}' set"
            : $"Flag '{effect.FlagKey}' cleared";

        _logger.LogInformation("Flag modified: {Description}", description);

        return DialogueEffectResult.Successful(DialogueEffectType.SetFlag, description);
    }
}
