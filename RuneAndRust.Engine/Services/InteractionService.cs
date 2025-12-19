using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements player-object interaction logic using WITS-based dice checks.
/// Handles examine, open, close, and search commands during exploration.
/// </summary>
public class InteractionService : IInteractionService
{
    private readonly ILogger<InteractionService> _logger;
    private readonly IInteractableObjectRepository _objectRepository;
    private readonly IDiceService _diceService;
    private readonly GameState _gameState;

    /// <summary>
    /// Net successes required for detailed tier (1+).
    /// </summary>
    private const int DetailedTierThreshold = 1;

    /// <summary>
    /// Net successes required for expert tier (3+).
    /// </summary>
    private const int ExpertTierThreshold = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="objectRepository">The repository for interactable objects.</param>
    /// <param name="diceService">The dice rolling service.</param>
    /// <param name="gameState">The current game state.</param>
    public InteractionService(
        ILogger<InteractionService> logger,
        IInteractableObjectRepository objectRepository,
        IDiceService diceService,
        GameState gameState)
    {
        _logger = logger;
        _objectRepository = objectRepository;
        _diceService = diceService;
        _gameState = gameState;
    }

    /// <inheritdoc/>
    public async Task<ExaminationResult> ExamineAsync(string targetName)
    {
        _logger.LogInformation("Examining object '{TargetName}'", targetName);

        // Validate game state
        if (_gameState.CurrentRoomId == null)
        {
            _logger.LogWarning("Examine attempted with no current room set");
            return ExaminationResult.NotFound(targetName);
        }

        if (_gameState.CurrentCharacter == null)
        {
            _logger.LogWarning("Examine attempted with no current character");
            return ExaminationResult.NotFound(targetName);
        }

        // Find the object in the current room
        var interactableObject = await _objectRepository.GetByNameInRoomAsync(
            _gameState.CurrentRoomId.Value,
            targetName);

        if (interactableObject == null)
        {
            _logger.LogDebug("Object '{TargetName}' not found in room {RoomId}",
                targetName, _gameState.CurrentRoomId.Value);
            return ExaminationResult.NotFound(targetName);
        }

        // Roll WITS dice pool for examination
        var wits = _gameState.CurrentCharacter.GetAttribute(CharacterAttribute.Wits);
        var diceResult = _diceService.Roll(wits, $"Examine {targetName}");

        var netSuccesses = diceResult.Successes - diceResult.Botches;
        _logger.LogDebug("Examination roll: {Successes} successes, {Botches} botches = {Net} net",
            diceResult.Successes, diceResult.Botches, netSuccesses);

        // Determine tier revealed
        int tierRevealed = CalculateTierRevealed(netSuccesses);

        // Check if new information was revealed
        bool newInfoRevealed = tierRevealed > interactableObject.HighestExaminationTier;

        // Update object state if new tier reached
        if (newInfoRevealed)
        {
            interactableObject.HighestExaminationTier = tierRevealed;
            interactableObject.HasBeenExamined = true;
            interactableObject.LastModified = DateTime.UtcNow;
            await _objectRepository.UpdateAsync(interactableObject);
            await _objectRepository.SaveChangesAsync();

            _logger.LogInformation("Object '{ObjectName}' examination tier updated to {Tier}",
                interactableObject.Name, tierRevealed);
        }

        // Build the combined description
        string description = BuildDescription(interactableObject, tierRevealed);

        var result = new ExaminationResult(
            Success: tierRevealed > 0,
            NetSuccesses: netSuccesses,
            TierRevealed: tierRevealed,
            Description: description,
            NewInfoRevealed: newInfoRevealed,
            Rolls: diceResult.Rolls
        );

        _logger.LogDebug("Examination complete: Success={Success}, Tier={Tier}, NewInfo={NewInfo}",
            result.Success, result.TierRevealed, result.NewInfoRevealed);

        return result;
    }

    /// <inheritdoc/>
    public async Task<string> OpenAsync(string targetName)
    {
        _logger.LogInformation("Attempting to open '{TargetName}'", targetName);

        var validationResult = ValidateGameState("Open");
        if (validationResult != null) return validationResult;

        var interactableObject = await _objectRepository.GetByNameInRoomAsync(
            _gameState.CurrentRoomId!.Value,
            targetName);

        if (interactableObject == null)
        {
            _logger.LogDebug("Object '{TargetName}' not found for open command", targetName);
            return $"You don't see anything called '{targetName}' here.";
        }

        if (!interactableObject.IsContainer)
        {
            _logger.LogDebug("Object '{ObjectName}' is not a container", interactableObject.Name);
            return $"The {interactableObject.Name} cannot be opened.";
        }

        if (interactableObject.IsOpen)
        {
            _logger.LogDebug("Object '{ObjectName}' is already open", interactableObject.Name);
            return $"The {interactableObject.Name} is already open.";
        }

        if (interactableObject.IsLocked)
        {
            _logger.LogDebug("Object '{ObjectName}' is locked", interactableObject.Name);
            return $"The {interactableObject.Name} is locked. You need to unlock it first.";
        }

        // Open the container
        interactableObject.IsOpen = true;
        interactableObject.LastModified = DateTime.UtcNow;
        await _objectRepository.UpdateAsync(interactableObject);
        await _objectRepository.SaveChangesAsync();

        _logger.LogInformation("Opened container '{ObjectName}'", interactableObject.Name);

        return $"You open the {interactableObject.Name}. It creaks on rusted hinges.";
    }

    /// <inheritdoc/>
    public async Task<string> CloseAsync(string targetName)
    {
        _logger.LogInformation("Attempting to close '{TargetName}'", targetName);

        var validationResult = ValidateGameState("Close");
        if (validationResult != null) return validationResult;

        var interactableObject = await _objectRepository.GetByNameInRoomAsync(
            _gameState.CurrentRoomId!.Value,
            targetName);

        if (interactableObject == null)
        {
            _logger.LogDebug("Object '{TargetName}' not found for close command", targetName);
            return $"You don't see anything called '{targetName}' here.";
        }

        if (!interactableObject.IsContainer)
        {
            _logger.LogDebug("Object '{ObjectName}' is not a container", interactableObject.Name);
            return $"The {interactableObject.Name} cannot be closed.";
        }

        if (!interactableObject.IsOpen)
        {
            _logger.LogDebug("Object '{ObjectName}' is already closed", interactableObject.Name);
            return $"The {interactableObject.Name} is already closed.";
        }

        // Close the container
        interactableObject.IsOpen = false;
        interactableObject.LastModified = DateTime.UtcNow;
        await _objectRepository.UpdateAsync(interactableObject);
        await _objectRepository.SaveChangesAsync();

        _logger.LogInformation("Closed container '{ObjectName}'", interactableObject.Name);

        return $"You close the {interactableObject.Name}.";
    }

    /// <inheritdoc/>
    public async Task<string> SearchAsync()
    {
        _logger.LogInformation("Searching current room");

        var validationResult = ValidateGameState("Search");
        if (validationResult != null) return validationResult;

        // Roll WITS dice pool for search
        var wits = _gameState.CurrentCharacter!.GetAttribute(CharacterAttribute.Wits);
        var diceResult = _diceService.Roll(wits, "Search room");

        var netSuccesses = diceResult.Successes - diceResult.Botches;
        _logger.LogDebug("Search roll: {Successes} successes, {Botches} botches = {Net} net",
            diceResult.Successes, diceResult.Botches, netSuccesses);

        // Get all objects in room
        var objects = await _objectRepository.GetByRoomIdAsync(_gameState.CurrentRoomId!.Value);
        var objectList = objects.ToList();

        if (!objectList.Any())
        {
            return "Your search reveals nothing of interest. The room appears empty of notable objects.";
        }

        // Reveal objects based on search success
        var foundObjects = new List<string>();
        foreach (var obj in objectList)
        {
            // Always find visible objects; success reveals hidden details
            foundObjects.Add(obj.Name);
        }

        if (netSuccesses < 0)
        {
            _logger.LogDebug("Search fumbled - only basic objects revealed");
            return "Your hasty search disturbs the dust, revealing little of value. " +
                   $"You notice: {string.Join(", ", foundObjects.Take(1))}.";
        }

        if (netSuccesses == 0)
        {
            return $"You search the area and find: {string.Join(", ", foundObjects)}.";
        }

        // Successful search provides more detail
        var containerCount = objectList.Count(o => o.IsContainer);
        var lockedCount = objectList.Count(o => o.IsLocked);

        var result = $"Your careful search reveals: {string.Join(", ", foundObjects)}.";

        if (containerCount > 0 && netSuccesses >= DetailedTierThreshold)
        {
            var containerNote = containerCount == 1
                ? "One appears to be a container."
                : $"{containerCount} appear to be containers.";
            result += $" {containerNote}";
        }

        if (lockedCount > 0 && netSuccesses >= ExpertTierThreshold)
        {
            var lockedNote = lockedCount == 1
                ? "You notice one is secured with a lock."
                : $"You notice {lockedCount} are secured with locks.";
            result += $" {lockedNote}";
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InteractableObject>> GetVisibleObjectsAsync()
    {
        _logger.LogDebug("Getting visible objects in current room");

        if (_gameState.CurrentRoomId == null)
        {
            _logger.LogWarning("GetVisibleObjects called with no current room");
            return Enumerable.Empty<InteractableObject>();
        }

        var objects = await _objectRepository.GetByRoomIdAsync(_gameState.CurrentRoomId.Value);

        _logger.LogDebug("Found {Count} visible objects in room", objects.Count());

        return objects;
    }

    /// <inheritdoc/>
    public async Task<string> ListObjectsAsync()
    {
        _logger.LogDebug("Listing objects in current room");

        var objects = await GetVisibleObjectsAsync();
        var objectList = objects.ToList();

        if (!objectList.Any())
        {
            return "You see nothing of particular interest in this area.";
        }

        var names = objectList.Select(o => o.Name).ToList();

        if (names.Count == 1)
        {
            return $"You notice: {names[0]}.";
        }

        var lastItem = names.Last();
        var otherItems = string.Join(", ", names.Take(names.Count - 1));
        return $"You notice: {otherItems}, and {lastItem}.";
    }

    /// <summary>
    /// Calculates the tier revealed based on net successes.
    /// </summary>
    private static int CalculateTierRevealed(int netSuccesses)
    {
        if (netSuccesses >= ExpertTierThreshold)
            return 2; // Expert tier
        if (netSuccesses >= DetailedTierThreshold)
            return 1; // Detailed tier
        return 0; // Base only
    }

    /// <summary>
    /// Builds the combined description string based on revealed tier.
    /// </summary>
    private static string BuildDescription(InteractableObject obj, int tierRevealed)
    {
        var parts = new List<string> { obj.Description };

        if (tierRevealed >= 1 && !string.IsNullOrWhiteSpace(obj.DetailedDescription))
        {
            parts.Add(obj.DetailedDescription);
        }

        if (tierRevealed >= 2 && !string.IsNullOrWhiteSpace(obj.ExpertDescription))
        {
            parts.Add(obj.ExpertDescription);
        }

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Validates that the game state is ready for interaction commands.
    /// </summary>
    private string? ValidateGameState(string action)
    {
        if (_gameState.CurrentRoomId == null)
        {
            _logger.LogWarning("{Action} attempted with no current room set", action);
            return "You must be in a room to do that.";
        }

        if (_gameState.CurrentCharacter == null)
        {
            _logger.LogWarning("{Action} attempted with no current character", action);
            return "You must have an active character to do that.";
        }

        return null;
    }
}
