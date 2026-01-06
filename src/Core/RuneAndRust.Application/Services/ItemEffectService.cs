using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for applying item effects to players.
/// </summary>
public class ItemEffectService
{
    private readonly ILogger<ItemEffectService> _logger;

    /// <summary>
    /// Creates a new ItemEffectService instance.
    /// </summary>
    /// <param name="logger">The logger for service diagnostics.</param>
    public ItemEffectService(ILogger<ItemEffectService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Applies an item's effect to the player.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="player">The player receiving the effect.</param>
    /// <returns>A tuple with success status and description message.</returns>
    public (bool Success, string Message) ApplyEffect(Item item, Player player)
    {
        _logger.LogDebug(
            "Applying effect: {Effect} (Value: {Value}) from {ItemName} to {PlayerName}",
            item.Effect, item.EffectValue, item.Name, player.Name);

        return item.Effect switch
        {
            ItemEffect.Heal => ApplyHeal(item, player),
            ItemEffect.Damage => ApplyDamage(item, player),
            ItemEffect.BuffAttack => ApplyAttackBuff(item, player),
            ItemEffect.BuffDefense => ApplyDefenseBuff(item, player),
            ItemEffect.None => (false, $"The {item.Name} has no usable effect."),
            _ => (false, $"Unknown effect type for {item.Name}.")
        };
    }

    private (bool Success, string Message) ApplyHeal(Item item, Player player)
    {
        var previousHealth = player.Health;
        var actualHealed = player.Heal(item.EffectValue);

        _logger.LogInformation(
            "Heal applied: {PlayerName} healed {ActualHealed} HP ({PreviousHP} -> {CurrentHP}/{MaxHP})",
            player.Name, actualHealed, previousHealth, player.Health, player.Stats.MaxHealth);

        var verb = item.Name.ToLower().Contains("potion") ? "drink" : "use";
        return (true, $"You {verb} the {item.Name}. Restored {actualHealed} HP. ({player.Health}/{player.Stats.MaxHealth})");
    }

    private (bool Success, string Message) ApplyDamage(Item item, Player player)
    {
        var previousHealth = player.Health;
        var actualDamage = player.TakeDamage(item.EffectValue);

        _logger.LogWarning(
            "Damage applied: {PlayerName} took {ActualDamage} damage ({PreviousHP} -> {CurrentHP}/{MaxHP})",
            player.Name, actualDamage, previousHealth, player.Health, player.Stats.MaxHealth);

        return (true, $"The {item.Name} was cursed! You take {actualDamage} damage. ({player.Health}/{player.Stats.MaxHealth})");
    }

    private (bool Success, string Message) ApplyAttackBuff(Item item, Player player)
    {
        // Note: Full buff system will be implemented in v0.0.6 (Status Effects)
        // For now, log and return a placeholder message
        _logger.LogDebug("Attack buff applied (placeholder): +{Value} for {Duration} turns",
            item.EffectValue, item.EffectDuration);

        return (true, $"You feel stronger! (+{item.EffectValue} Attack for {item.EffectDuration} turns) [Buff system coming in v0.0.6]");
    }

    private (bool Success, string Message) ApplyDefenseBuff(Item item, Player player)
    {
        _logger.LogDebug("Defense buff applied (placeholder): +{Value} for {Duration} turns",
            item.EffectValue, item.EffectDuration);

        return (true, $"Your skin hardens! (+{item.EffectValue} Defense for {item.EffectDuration} turns) [Buff system coming in v0.0.6]");
    }
}
