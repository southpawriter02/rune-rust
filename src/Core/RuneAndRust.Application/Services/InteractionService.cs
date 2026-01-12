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
            InteractionType.Activate => Activate(obj),
            InteractionType.Deactivate => Deactivate(obj),
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

    private InteractionResult Activate(InteractiveObject obj)
    {
        if (obj.State == ObjectState.Active)
            return InteractionResult.Failed($"The {obj.Name} is already active.", InteractionType.Activate);

        if (!obj.TrySetState(ObjectState.Active))
            return InteractionResult.Failed($"The {obj.Name} cannot be activated.", InteractionType.Activate);

        _logger.LogDebug("Activated {Object}", obj.Name);
        return InteractionResult.Succeeded($"You activate the {obj.Name}.", InteractionType.Activate, ObjectState.Active);
    }

    private InteractionResult Deactivate(InteractiveObject obj)
    {
        if (obj.State == ObjectState.Inactive)
            return InteractionResult.Failed($"The {obj.Name} is already inactive.", InteractionType.Deactivate);

        if (!obj.TrySetState(ObjectState.Inactive))
            return InteractionResult.Failed($"The {obj.Name} cannot be deactivated.", InteractionType.Deactivate);

        _logger.LogDebug("Deactivated {Object}", obj.Name);
        return InteractionResult.Succeeded($"You deactivate the {obj.Name}.", InteractionType.Deactivate, ObjectState.Inactive);
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
}
