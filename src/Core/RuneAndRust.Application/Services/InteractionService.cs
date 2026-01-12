using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for handling object interactions.
/// </summary>
public class InteractionService : IInteractionService
{
    private readonly ILogger<InteractionService> _logger;

    public InteractionService(ILogger<InteractionService>? logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InteractionService>.Instance;
    }

    /// <inheritdoc/>
    public InteractionResult Interact(InteractiveObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        var defaultType = obj.GetDefaultInteraction();
        return Interact(obj, defaultType);
    }

    /// <inheritdoc/>
    public InteractionResult Interact(InteractiveObject obj, InteractionType type)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (obj.State == ObjectState.Broken)
            return InteractionResult.Failed($"The {obj.Name} is broken and cannot be used.", type);

        if (obj.State == ObjectState.Destroyed)
            return InteractionResult.Failed($"The {obj.Name} has been destroyed.", type);

        if (!obj.CanPerformInteraction(type))
            return InteractionResult.Failed($"You cannot {type.ToString().ToLowerInvariant()} the {obj.Name}.", type);

        return type switch
        {
            InteractionType.Open => Open(obj),
            InteractionType.Close => Close(obj),
            InteractionType.Examine => Examine(obj),
            InteractionType.Activate => ActivateInternal(obj),
            InteractionType.Deactivate => DeactivateInternal(obj),
            _ => InteractionResult.Succeeded($"You interact with the {obj.Name}.", type)
        };
    }

    /// <inheritdoc/>
    public InteractionResult Open(InteractiveObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (obj.State == ObjectState.Open)
            return InteractionResult.Failed($"The {obj.Name} is already open.", InteractionType.Open);

        if (obj.State == ObjectState.Locked)
            return InteractionResult.Failed($"The {obj.Name} is locked.", InteractionType.Open);

        if (!obj.TrySetState(ObjectState.Open))
            return InteractionResult.Failed($"The {obj.Name} cannot be opened.", InteractionType.Open);

        _logger.LogInformation("Opened {ObjectName}", obj.Name);

        // Show container contents if applicable
        var message = $"You open the {obj.Name}.";
        if (obj.IsContainer && obj.ContainerInventory != null)
        {
            message += "\n\n" + obj.ContainerInventory.GetContentsDescription();
        }

        return InteractionResult.Succeeded(message, InteractionType.Open, ObjectState.Open);
    }

    /// <inheritdoc/>
    public InteractionResult Close(InteractiveObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (obj.State == ObjectState.Closed)
            return InteractionResult.Failed($"The {obj.Name} is already closed.", InteractionType.Close);

        if (!obj.TrySetState(ObjectState.Closed))
            return InteractionResult.Failed($"The {obj.Name} cannot be closed.", InteractionType.Close);

        _logger.LogDebug("Closed {Object}", obj.Name);
        return InteractionResult.Closed(obj.Name);
    }

    /// <inheritdoc/>
    public InteractionResult Examine(InteractiveObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var stateDesc = obj.State switch
        {
            ObjectState.Open => "It is open.",
            ObjectState.Closed => "It is closed.",
            ObjectState.Locked => "It is locked.",
            ObjectState.Active => "It is active.",
            ObjectState.Inactive => "It is inactive.",
            ObjectState.Broken => "It is broken.",
            ObjectState.Destroyed => "It has been destroyed.",
            _ => ""
        };

        var fullDesc = string.IsNullOrEmpty(obj.Description)
            ? $"{obj.Name}. {stateDesc}"
            : $"{obj.Name}\n{obj.Description} {stateDesc}";

        return InteractionResult.Examined(fullDesc);
    }

    /// <inheritdoc/>
    public InteractiveObject? FindObject(IEnumerable<InteractiveObject> objects, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return null;
        return objects.FirstOrDefault(o => o.MatchesKeyword(keyword));
    }

    /// <inheritdoc/>
    public InteractiveObject? FindObject(Room room, string keyword)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (string.IsNullOrWhiteSpace(keyword)) return null;

        var obj = room.GetInteractableByKeyword(keyword);

        if (obj != null)
        {
            _logger.LogDebug("Found object {ObjectName} by keyword '{Keyword}'", obj.Name, keyword);
        }
        else
        {
            _logger.LogDebug("No object found for keyword '{Keyword}'", keyword);
        }

        return obj;
    }

    /// <inheritdoc/>
    public InteractionType GetDefaultInteraction(InteractiveObject obj) =>
        obj.GetDefaultInteraction();

    /// <inheritdoc/>
    public string GetRoomObjectsDescription(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var visibleObjects = room.GetVisibleInteractables().ToList();
        if (!visibleObjects.Any())
        {
            return string.Empty;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("You notice:");

        foreach (var obj in visibleObjects)
        {
            var stateInfo = GetStateDisplayText(obj.State);
            var blockingInfo = obj.IsCurrentlyBlocking && obj.BlockedDirection.HasValue
                ? $" - blocking {obj.BlockedDirection.Value.ToString().ToLowerInvariant()}"
                : string.Empty;

            sb.AppendLine($"  {obj.Name}{stateInfo}{blockingInfo}");
        }

        return sb.ToString().TrimEnd();
    }

    private static string GetStateDisplayText(ObjectState state)
    {
        return state switch
        {
            ObjectState.Open => " (open)",
            ObjectState.Closed => " (closed)",
            ObjectState.Locked => " (locked)",
            ObjectState.Active => " (active)",
            ObjectState.Inactive => " (inactive)",
            ObjectState.Lit => " (lit)",
            ObjectState.Unlit => " (unlit)",
            ObjectState.Broken => " (broken)",
            ObjectState.Destroyed => " (destroyed)",
            ObjectState.Up => " (up)",
            ObjectState.Down => " (down)",
            _ => string.Empty
        };
    }

    // ===== Private Activation Helpers (v0.4.0c Updated) =====

    /// <summary>
    /// Internal activate without room for effect resolution.
    /// </summary>
    private InteractionResult ActivateInternal(InteractiveObject obj)
    {
        if (obj.State == ObjectState.Active)
        {
            _logger.LogDebug("{Object} already active", obj.Name);
            return InteractionResult.Failed($"The {obj.Name} is already active.", InteractionType.Activate);
        }

        if (!obj.Activate())
        {
            _logger.LogDebug("{Object} cannot be activated", obj.Name);
            return InteractionResult.Failed($"The {obj.Name} cannot be activated.", InteractionType.Activate);
        }

        var message = GetActivationMessage(obj);
        _logger.LogInformation("Activated {Object}, IsButton={IsButton}", obj.Name, obj.IsButton);
        return InteractionResult.Succeeded(message, InteractionType.Activate, ObjectState.Active);
    }

    /// <summary>
    /// Internal deactivate without room for effect resolution.
    /// </summary>
    private InteractionResult DeactivateInternal(InteractiveObject obj)
    {
        if (obj.State != ObjectState.Active)
        {
            _logger.LogDebug("{Object} is not active", obj.Name);
            return InteractionResult.Failed($"The {obj.Name} is not active.", InteractionType.Deactivate);
        }

        if (obj.IsButton)
        {
            _logger.LogDebug("{Object} is a button and cannot be manually deactivated", obj.Name);
            return InteractionResult.Failed($"The {obj.Name} will reset automatically.", InteractionType.Deactivate);
        }

        if (!obj.Deactivate())
        {
            _logger.LogDebug("{Object} cannot be deactivated", obj.Name);
            return InteractionResult.Failed($"The {obj.Name} cannot be deactivated.", InteractionType.Deactivate);
        }

        var message = GetDeactivationMessage(obj);
        _logger.LogInformation("Deactivated {Object}", obj.Name);
        return InteractionResult.Succeeded(message, InteractionType.Deactivate, ObjectState.Inactive);
    }

    /// <summary>
    /// Gets a themed activation message based on object type.
    /// </summary>
    private static string GetActivationMessage(InteractiveObject obj)
    {
        return obj.ObjectType switch
        {
            InteractiveObjectType.Lever => $"You pull the {obj.Name}. It locks into position with a heavy click.",
            InteractiveObjectType.Button => $"You press the {obj.Name}. It depresses with a satisfying click.",
            _ => $"You activate the {obj.Name}."
        };
    }

    /// <summary>
    /// Gets a themed deactivation message based on object type.
    /// </summary>
    private static string GetDeactivationMessage(InteractiveObject obj)
    {
        return obj.ObjectType switch
        {
            InteractiveObjectType.Lever => $"You push the {obj.Name} back. It returns to its original position.",
            _ => $"You deactivate the {obj.Name}."
        };
    }

    // ===== Lock Methods (v0.4.0b) =====

    /// <inheritdoc/>
    public UnlockResult UnlockWithKey(InteractiveObject obj, Player player)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug("Attempting to unlock {ObjectName} with key", obj.Name);

        if (!obj.IsLocked)
            return UnlockResult.NotLocked(obj.Name);

        if (!obj.HasLock)
            return UnlockResult.NotLocked(obj.Name);

        // Find a matching key in player's inventory
        var matchingKey = player.Inventory.Items
            .FirstOrDefault(i => i.IsKey && i.KeyId != null && obj.Lock.KeyMatches(i.KeyId));

        if (matchingKey == null)
        {
            _logger.LogDebug("No matching key found for {ObjectName}", obj.Name);
            return UnlockResult.NoKey(obj.Name);
        }

        // Unlock with the key
        if (!obj.TryUnlockWithKey(matchingKey.KeyId!))
            return UnlockResult.Failed($"The key doesn't fit this lock.");

        var consumed = false;
        if (matchingKey.IsKeyConsumedOnUse)
        {
            player.Inventory.Remove(matchingKey);
            consumed = true;
            _logger.LogInformation("Key {KeyName} consumed after unlocking {ObjectName}",
                matchingKey.Name, obj.Name);
        }

        _logger.LogInformation("Unlocked {ObjectName} with {KeyName}", obj.Name, matchingKey.Name);

        return UnlockResult.SuccessWithKey(
            $"You use the {matchingKey.Name} to unlock the {obj.Name}.",
            matchingKey.Name,
            consumed);
    }

    /// <inheritdoc/>
    public UnlockResult PickLock(InteractiveObject obj, Player player)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug("Attempting to pick lock on {ObjectName}", obj.Name);

        if (!obj.IsLocked)
            return UnlockResult.NotLocked(obj.Name);

        if (!obj.HasLock)
            return UnlockResult.NotLocked(obj.Name);

        if (!obj.Lock.IsLockpickable)
        {
            _logger.LogDebug("Lock on {ObjectName} cannot be picked", obj.Name);
            return UnlockResult.CannotPick(obj.Name);
        }

        // Simple skill check simulation - in real implementation use SkillCheckService
        // For now, we'll just unlock if lockpickable to allow testing
        obj.Unlock();
        _logger.LogInformation("Successfully picked lock on {ObjectName}", obj.Name);

        return UnlockResult.SuccessWithLockpick(
            $"You carefully work the lock on the {obj.Name}. After a moment, it clicks open.",
            obj.Lock.LockpickDC,
            obj.Lock.LockpickDC);
    }

    /// <inheritdoc/>
    public InteractionResult Lock(InteractiveObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (obj.IsLocked)
            return InteractionResult.Failed($"The {obj.Name} is already locked.", InteractionType.Lock);

        if (!obj.HasLock)
            return InteractionResult.Failed($"The {obj.Name} doesn't have a lock.", InteractionType.Lock);

        if (!obj.Lock.CanRelock)
            return InteractionResult.Failed($"This lock cannot be relocked.", InteractionType.Lock);

        if (obj.State != ObjectState.Closed)
            return InteractionResult.Failed($"You need to close the {obj.Name} before locking it.", InteractionType.Lock);

        if (!obj.TryLock())
            return InteractionResult.Failed($"The {obj.Name} cannot be locked.", InteractionType.Lock);

        _logger.LogInformation("Locked {ObjectName}", obj.Name);

        return InteractionResult.Succeeded(
            $"You lock the {obj.Name}.",
            InteractionType.Lock,
            ObjectState.Locked);
    }

    // ===== Container Methods (v0.4.0b) =====

    /// <inheritdoc/>
    public InteractionResult TakeFromContainer(InteractiveObject container, string itemName, Player player)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentException.ThrowIfNullOrWhiteSpace(itemName);
        ArgumentNullException.ThrowIfNull(player);

        if (!container.IsContainer)
            return InteractionResult.Failed($"The {container.Name} is not a container.", InteractionType.Take);

        if (container.State != ObjectState.Open)
            return InteractionResult.Failed($"The {container.Name} is not open.", InteractionType.Take);

        var item = container.ContainerInventory!.GetItemByPartialName(itemName);
        if (item == null)
            return InteractionResult.Failed($"There is no '{itemName}' in the {container.Name}.", InteractionType.Take);

        if (player.Inventory.IsFull)
            return InteractionResult.Failed("Your inventory is full.", InteractionType.Take);

        container.ContainerInventory.RemoveItem(item);
        player.Inventory.TryAdd(item);

        _logger.LogInformation("Took {ItemName} from {ContainerName}", item.Name, container.Name);

        return InteractionResult.Succeeded(
            $"You take the {item.Name} from the {container.Name}.",
            InteractionType.Take);
    }

    /// <inheritdoc/>
    public InteractionResult TakeAllFromContainer(InteractiveObject container, Player player)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(player);

        if (!container.IsContainer)
            return InteractionResult.Failed($"The {container.Name} is not a container.", InteractionType.Take);

        if (container.State != ObjectState.Open)
            return InteractionResult.Failed($"The {container.Name} is not open.", InteractionType.Take);

        if (container.ContainerInventory!.IsEmpty)
            return InteractionResult.Failed($"The {container.Name} is empty.", InteractionType.Take);

        var takenItems = new List<string>();
        var leftBehind = new List<string>();

        foreach (var item in container.ContainerInventory.Items.ToList())
        {
            if (player.Inventory.IsFull)
                leftBehind.Add(item.Name);
            else
            {
                container.ContainerInventory.RemoveItem(item);
                player.Inventory.TryAdd(item);
                takenItems.Add(item.Name);
            }
        }

        if (takenItems.Count == 0)
            return InteractionResult.Failed("Your inventory is full.", InteractionType.Take);

        _logger.LogInformation("Took {Count} items from {ContainerName}", takenItems.Count, container.Name);

        var message = "You take:\n" + string.Join("\n", takenItems.Select(n => $"  - {n}"));

        if (leftBehind.Count > 0)
        {
            message += "\n\nYour inventory is full. Left behind:\n" +
                       string.Join("\n", leftBehind.Select(n => $"  - {n}"));
        }
        else if (container.ContainerInventory.IsEmpty)
        {
            message += $"\n\nThe {container.Name} is now empty.";
        }

        return InteractionResult.Succeeded(message, InteractionType.Take);
    }

    /// <inheritdoc/>
    public InteractionResult PutInContainer(InteractiveObject container, string itemName, Player player)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentException.ThrowIfNullOrWhiteSpace(itemName);
        ArgumentNullException.ThrowIfNull(player);

        if (!container.IsContainer)
            return InteractionResult.Failed($"You can't put things in the {container.Name}.", InteractionType.Put);

        if (container.State != ObjectState.Open)
            return InteractionResult.Failed($"The {container.Name} is not open.", InteractionType.Put);

        var item = player.Inventory.GetByName(itemName);
        if (item == null)
            return InteractionResult.Failed($"You don't have '{itemName}' in your inventory.", InteractionType.Put);

        if (container.ContainerInventory!.IsFull)
            return InteractionResult.Failed($"The {container.Name} is full.", InteractionType.Put);

        player.Inventory.Remove(item);
        container.ContainerInventory.TryAddItem(item);

        _logger.LogInformation("Put {ItemName} in {ContainerName}", item.Name, container.Name);

        return InteractionResult.Succeeded(
            $"You put the {item.Name} in the {container.Name}.",
            InteractionType.Put);
    }

    /// <inheritdoc/>
    public string GetContainerContents(InteractiveObject container)
    {
        if (!container.IsContainer || container.ContainerInventory == null)
            return $"The {container.Name} is not a container.";

        if (container.State != ObjectState.Open)
            return $"The {container.Name} is closed.";

        return container.ContainerInventory.GetContentsDescription();
    }

    // ===== Activation Methods (v0.4.0c) =====

    /// <inheritdoc/>
    public InteractionResult Activate(InteractiveObject obj, Room room)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(room);

        _logger.LogDebug("Activating {Object} in room {Room}", obj.Name, room.Name);

        var result = ActivateInternal(obj);

        if (result.Success)
        {
            // Resolve triggered effects
            var effectMessages = ResolveEffectsForState(obj, ObjectState.Active, room);
            if (effectMessages.Count > 0)
            {
                var combinedMessage = result.Message + "\n\n" + string.Join("\n", effectMessages);
                return result with { Message = combinedMessage };
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public InteractionResult Deactivate(InteractiveObject obj, Room room)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(room);

        _logger.LogDebug("Deactivating {Object} in room {Room}", obj.Name, room.Name);

        var result = DeactivateInternal(obj);

        if (result.Success)
        {
            // Resolve triggered effects
            var effectMessages = ResolveEffectsForState(obj, ObjectState.Inactive, room);
            if (effectMessages.Count > 0)
            {
                var combinedMessage = result.Message + "\n\n" + string.Join("\n", effectMessages);
                return result with { Message = combinedMessage };
            }
        }

        return result;
    }

    // ===== Destruction Methods (v0.4.0c) =====

    /// <inheritdoc/>
    public ObjectDamageResult AttackObject(InteractiveObject obj, int damage, string? damageType, Room room)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(room);

        _logger.LogDebug("Attacking {Object} with {Damage} {DamageType} damage",
            obj.Name, damage, damageType ?? "untyped");

        if (!obj.IsDestructible)
        {
            _logger.LogDebug("{Object} is not destructible", obj.Name);
            return ObjectDamageResult.NotDestructible(obj.Name);
        }

        if (obj.IsDestroyed)
        {
            _logger.LogDebug("{Object} is already destroyed", obj.Name);
            return ObjectDamageResult.AlreadyDestroyed(obj.Name);
        }

        // Check for immunity before dealing damage
        if (!string.IsNullOrEmpty(damageType) && obj.Destructible!.IsImmuneTo(damageType))
        {
            _logger.LogDebug("{Object} is immune to {DamageType}", obj.Name, damageType);
            return ObjectDamageResult.Immune(obj.Name, damageType);
        }

        var wasBlockingBefore = obj.IsCurrentlyBlocking;
        var actualDamage = obj.TakeDamage(damage, damageType);
        var wasDestroyed = obj.IsDestroyed;

        _logger.LogInformation("Dealt {ActualDamage} damage to {Object}, destroyed={Destroyed}",
            actualDamage, obj.Name, wasDestroyed);

        if (wasDestroyed)
        {
            var droppedItems = DropContainerContents(obj, room);
            var passageCleared = wasBlockingBefore && !obj.IsCurrentlyBlocking;

            // Handle vulnerability messaging
            if (!string.IsNullOrEmpty(damageType) && obj.Destructible!.IsVulnerableTo(damageType))
            {
                return ObjectDamageResult.Vulnerable(
                    obj.Name, actualDamage, damageType,
                    destroyed: true, droppedItems: droppedItems, passageCleared: passageCleared);
            }

            return ObjectDamageResult.Destroyed(obj.Name, actualDamage, droppedItems, passageCleared);
        }

        // Not destroyed - build appropriate message
        var condition = obj.Destructible!.GetConditionDescription();
        var damageMessage = $"You strike the {obj.Name} for {actualDamage} damage. It looks {condition}.";

        if (!string.IsNullOrEmpty(damageType))
        {
            if (obj.Destructible.IsVulnerableTo(damageType))
            {
                return ObjectDamageResult.Vulnerable(obj.Name, actualDamage, damageType);
            }
            else if (obj.Destructible.IsResistantTo(damageType))
            {
                return ObjectDamageResult.Resisted(obj.Name, actualDamage, damageType);
            }
        }

        return ObjectDamageResult.Hit(damageMessage, actualDamage);
    }

    /// <summary>
    /// Drops container contents to the room when destroyed.
    /// </summary>
    private List<Item> DropContainerContents(InteractiveObject obj, Room room)
    {
        if (!obj.IsContainer || obj.ContainerInventory == null || obj.ContainerInventory.IsEmpty)
            return [];

        var items = obj.ContainerInventory.TakeAll().ToList();
        _logger.LogInformation("Dropped {Count} items from destroyed {Object}", items.Count, obj.Name);

        // Note: In a full implementation, items would be added to room floor
        // For now, we just return them in the result
        return items;
    }

    // ===== Turn Processing Methods (v0.4.0c) =====

    /// <inheritdoc/>
    public IEnumerable<string> ProcessRoomTurnTick(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var messages = new List<string>();

        foreach (var obj in room.Interactables)
        {
            if (obj.ProcessTurnTick())
            {
                _logger.LogDebug("{Object} reset to inactive", obj.Name);
                var message = obj.ObjectType switch
                {
                    InteractiveObjectType.Button => $"The {obj.Name} clicks back into place.",
                    _ => $"The {obj.Name} deactivates."
                };
                messages.Add(message);

                // Resolve effects for the reset (entering Inactive state)
                var effectMessages = ResolveEffectsForState(obj, ObjectState.Inactive, room);
                messages.AddRange(effectMessages);
            }
        }

        return messages;
    }

    // ===== Effect Methods (v0.4.0c) =====

    /// <inheritdoc/>
    public IEnumerable<ObjectEffect> GetPendingEffects(InteractiveObject obj, ObjectState newState)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return obj.GetTriggeredEffects(newState);
    }

    /// <inheritdoc/>
    public EffectTriggerResult ResolveEffect(ObjectEffect effect, Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        _logger.LogDebug("Resolving effect {EffectType} on target {Target}",
            effect.Type, effect.TargetObjectId);

        // Handle message-only effects
        if (effect.Type == EffectType.Message)
        {
            _logger.LogDebug("Message effect: {Message}", effect.EffectMessage);
            return EffectTriggerResult.MessageDisplayed(effect);
        }

        // Find target object by definition ID
        var target = room.Interactables.FirstOrDefault(o =>
            o.DefinitionId.Equals(effect.TargetObjectId, StringComparison.OrdinalIgnoreCase));

        if (target == null)
        {
            _logger.LogDebug("Effect target not found: {Target}", effect.TargetObjectId);
            return EffectTriggerResult.TargetNotFound(effect);
        }

        var message = effect.EffectMessage ?? GetDefaultEffectMessage(effect.Type, target);

        // Apply effect based on type
        var (success, newState) = ApplyEffect(effect.Type, target);

        if (success)
        {
            _logger.LogInformation("Effect {EffectType} applied to {Target}, new state: {NewState}",
                effect.Type, target.Name, newState);
            return EffectTriggerResult.Succeeded(effect, message, newState);
        }

        _logger.LogDebug("Effect {EffectType} failed on {Target}", effect.Type, target.Name);
        return EffectTriggerResult.Failed(effect, $"The effect on {target.Name} failed.");
    }

    /// <summary>
    /// Applies an effect to a target object.
    /// </summary>
    private static (bool Success, ObjectState? NewState) ApplyEffect(EffectType type, InteractiveObject target)
    {
        return type switch
        {
            EffectType.OpenTarget => ApplyOpenEffect(target),
            EffectType.CloseTarget => ApplyCloseEffect(target),
            EffectType.UnlockTarget => ApplyUnlockEffect(target),
            EffectType.LockTarget => ApplyLockEffect(target),
            EffectType.ToggleTarget => ApplyToggleEffect(target),
            EffectType.ActivateTarget => ApplyActivateEffect(target),
            EffectType.DeactivateTarget => ApplyDeactivateEffect(target),
            EffectType.DestroyTarget => ApplyDestroyEffect(target),
            EffectType.RevealTarget => ApplyRevealEffect(target),
            _ => (false, null)
        };
    }

    private static (bool, ObjectState?) ApplyOpenEffect(InteractiveObject target)
    {
        if (target.State == ObjectState.Open) return (true, ObjectState.Open);
        if (target.State == ObjectState.Locked) return (false, null);
        return target.TrySetState(ObjectState.Open) ? (true, ObjectState.Open) : (false, null);
    }

    private static (bool, ObjectState?) ApplyCloseEffect(InteractiveObject target)
    {
        if (target.State == ObjectState.Closed) return (true, ObjectState.Closed);
        return target.TrySetState(ObjectState.Closed) ? (true, ObjectState.Closed) : (false, null);
    }

    private static (bool, ObjectState?) ApplyUnlockEffect(InteractiveObject target)
    {
        if (!target.IsLocked) return (true, target.State);
        return target.Unlock() ? (true, ObjectState.Closed) : (false, null);
    }

    private static (bool, ObjectState?) ApplyLockEffect(InteractiveObject target)
    {
        if (target.IsLocked) return (true, ObjectState.Locked);
        return target.TryLock() ? (true, ObjectState.Locked) : (false, null);
    }

    private static (bool, ObjectState?) ApplyToggleEffect(InteractiveObject target)
    {
        var newState = target.Toggle();
        return newState.HasValue ? (true, newState) : (false, null);
    }

    private static (bool, ObjectState?) ApplyActivateEffect(InteractiveObject target)
    {
        if (target.State == ObjectState.Active) return (true, ObjectState.Active);
        return target.Activate() ? (true, ObjectState.Active) : (false, null);
    }

    private static (bool, ObjectState?) ApplyDeactivateEffect(InteractiveObject target)
    {
        if (target.State == ObjectState.Inactive) return (true, ObjectState.Inactive);
        return target.Deactivate() ? (true, ObjectState.Inactive) : (false, null);
    }

    private static (bool, ObjectState?) ApplyDestroyEffect(InteractiveObject target)
    {
        target.Destroy();
        return (true, ObjectState.Destroyed);
    }

    private static (bool, ObjectState?) ApplyRevealEffect(InteractiveObject target)
    {
        target.SetVisibility(true);
        return (true, target.State);
    }

    /// <summary>
    /// Gets a default message for an effect type.
    /// </summary>
    private static string GetDefaultEffectMessage(EffectType type, InteractiveObject target)
    {
        return type switch
        {
            EffectType.OpenTarget => $"The {target.Name} opens.",
            EffectType.CloseTarget => $"The {target.Name} closes.",
            EffectType.UnlockTarget => $"The {target.Name} unlocks with a click.",
            EffectType.LockTarget => $"The {target.Name} locks.",
            EffectType.ToggleTarget => $"The {target.Name} toggles.",
            EffectType.ActivateTarget => $"The {target.Name} activates.",
            EffectType.DeactivateTarget => $"The {target.Name} deactivates.",
            EffectType.DestroyTarget => $"The {target.Name} is destroyed!",
            EffectType.RevealTarget => $"A hidden {target.Name} is revealed!",
            _ => $"Something happens to the {target.Name}."
        };
    }

    /// <summary>
    /// Resolves all effects for an object entering a new state.
    /// </summary>
    private List<string> ResolveEffectsForState(InteractiveObject obj, ObjectState newState, Room room)
    {
        var messages = new List<string>();
        var effects = obj.GetTriggeredEffects(newState);

        foreach (var effect in effects)
        {
            if (!effect.IsImmediate)
            {
                _logger.LogDebug("Skipping delayed effect: {Effect}", effect);
                continue;
            }

            var result = ResolveEffect(effect, room);
            if (!string.IsNullOrEmpty(result.Message))
            {
                messages.Add(result.Message);
            }
        }

        return messages;
    }
}
