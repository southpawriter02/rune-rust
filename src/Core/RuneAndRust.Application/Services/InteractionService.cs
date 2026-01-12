using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for handling object interactions.
/// </summary>
public class InteractionService : IInteractionService
{
    private readonly ILogger<InteractionService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    public InteractionService(
        ILogger<InteractionService>? logger = null,
        IGameEventLogger? eventLogger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InteractionService>.Instance;
        _eventLogger = eventLogger;
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

        _logger.LogDebug("Opened {Object}", obj.Name);
        _eventLogger?.Log(InteractionEvent.Interacted(obj.Id, obj.Name, InteractionType.Open, ObjectState.Open));
        return InteractionResult.Opened(obj.Name);
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
        _eventLogger?.Log(InteractionEvent.Interacted(obj.Id, obj.Name, InteractionType.Close, ObjectState.Closed));
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
    public InteractionType GetDefaultInteraction(InteractiveObject obj) =>
        obj.GetDefaultInteraction();

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
}
