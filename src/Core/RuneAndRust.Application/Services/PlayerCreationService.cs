using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using BackgroundDefinition = RuneAndRust.Domain.Definitions.BackgroundDefinition;
using RuneAndRust.Domain.Validation;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for orchestrating player character creation.
/// </summary>
public class PlayerCreationService
{
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<PlayerCreationService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    public PlayerCreationService(
        IGameConfigurationProvider configProvider,
        ILogger<PlayerCreationService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;
        _logger.LogDebug("PlayerCreationService initialized");
    }

    /// <summary>
    /// Gets all available races for character creation.
    /// </summary>
    public IReadOnlyList<RaceDefinition> GetAvailableRaces()
    {
        return _configProvider.GetRaces()
            .Where(r => r.IsPlayable)
            .OrderBy(r => r.SortOrder)
            .ToList();
    }

    /// <summary>
    /// Gets all available backgrounds for character creation.
    /// </summary>
    public IReadOnlyList<BackgroundDefinition> GetAvailableBackgrounds()
    {
        return _configProvider.GetBackgrounds()
            .Where(b => b.IsPlayable)
            .OrderBy(b => b.Category)
            .ThenBy(b => b.SortOrder)
            .ToList();
    }

    /// <summary>
    /// Gets attribute definitions for display.
    /// </summary>
    public IReadOnlyList<AttributeDefinition> GetAttributeDefinitions()
    {
        return _configProvider.GetAttributes()
            .OrderBy(a => a.SortOrder)
            .ToList();
    }

    /// <summary>
    /// Gets the point-buy rules for attribute allocation.
    /// </summary>
    public PointBuyRules GetPointBuyRules()
    {
        return _configProvider.GetPointBuyRules();
    }

    /// <summary>
    /// Validates a player name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <returns>Validation result with success status and optional error message.</returns>
    public (bool IsValid, string? ErrorMessage) ValidateName(string? name)
    {
        var result = PlayerNameValidator.Validate(name);
        if (!result.IsValid)
        {
            _logger.LogDebug("Name validation failed: {Error}", result.ErrorMessage);
        }
        return result;
    }

    /// <summary>
    /// Normalizes a valid player name.
    /// </summary>
    /// <param name="name">The name to normalize.</param>
    /// <returns>Normalized name with trimmed whitespace.</returns>
    public string NormalizeName(string name)
    {
        return PlayerNameValidator.Normalize(name);
    }

    /// <summary>
    /// Validates attribute allocation against point-buy rules.
    /// </summary>
    /// <param name="attributes">The attributes to validate.</param>
    /// <param name="raceId">The selected race for modifier calculation.</param>
    /// <returns>Validation result with remaining points or error.</returns>
    public (bool IsValid, int PointsUsed, int PointsRemaining, string? ErrorMessage) ValidateAttributes(
        PlayerAttributes attributes,
        string raceId)
    {
        var rules = _configProvider.GetPointBuyRules();
        var pointsUsed = attributes.CalculatePointCost();
        var pointsRemaining = rules.StartingPoints - pointsUsed;

        if (pointsUsed > rules.StartingPoints)
        {
            return (false, pointsUsed, pointsRemaining,
                $"Exceeded point budget. Used {pointsUsed}, allowed {rules.StartingPoints}.");
        }

        // Validate individual attribute ranges
        var race = _configProvider.GetRaceById(raceId);
        var finalAttributes = race != null
            ? attributes.WithModifiers(race.AttributeModifiers)
            : attributes;

        // Check each attribute is within valid range
        foreach (var attr in new[] { "might", "fortitude", "will", "wits", "finesse" })
        {
            var baseValue = attributes.GetByName(attr);
            if (baseValue < rules.MinimumAttribute || baseValue > rules.MaximumAttribute)
            {
                return (false, pointsUsed, pointsRemaining,
                    $"Attribute {attr} must be between {rules.MinimumAttribute} and {rules.MaximumAttribute}.");
            }

            var finalValue = finalAttributes.GetByName(attr);
            if (finalValue > rules.MaximumAfterRacial)
            {
                return (false, pointsUsed, pointsRemaining,
                    $"Attribute {attr} exceeds maximum after racial modifiers ({rules.MaximumAfterRacial}).");
            }
        }

        _logger.LogDebug("Attributes validated: {PointsUsed}/{PointsTotal} points used",
            pointsUsed, rules.StartingPoints);

        return (true, pointsUsed, pointsRemaining, null);
    }

    /// <summary>
    /// Creates a new player character with the specified options.
    /// </summary>
    /// <param name="name">The player name.</param>
    /// <param name="raceId">The selected race ID.</param>
    /// <param name="backgroundId">The selected background ID.</param>
    /// <param name="baseAttributes">The base attributes before modifiers.</param>
    /// <param name="description">Optional character description.</param>
    /// <returns>The created player entity.</returns>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public Player CreateCharacter(
        string name,
        string raceId,
        string backgroundId,
        PlayerAttributes baseAttributes,
        string description = "")
    {
        // Validate name
        var nameResult = ValidateName(name);
        if (!nameResult.IsValid)
        {
            throw new ArgumentException(nameResult.ErrorMessage, nameof(name));
        }

        // Validate race exists
        var race = _configProvider.GetRaceById(raceId);
        if (race == null || !race.IsPlayable)
        {
            throw new ArgumentException($"Invalid race: {raceId}", nameof(raceId));
        }

        // Validate background exists
        var background = _configProvider.GetBackgroundById(backgroundId);
        if (background == null || !background.IsPlayable)
        {
            throw new ArgumentException($"Invalid background: {backgroundId}", nameof(backgroundId));
        }

        // Validate attributes
        var attrResult = ValidateAttributes(baseAttributes, raceId);
        if (!attrResult.IsValid)
        {
            throw new ArgumentException(attrResult.ErrorMessage, nameof(baseAttributes));
        }

        // Apply racial modifiers
        var finalAttributes = baseAttributes.WithModifiers(race.AttributeModifiers);

        // Apply background bonuses
        finalAttributes = finalAttributes.WithModifiers(background.AttributeBonuses);

        // Normalize name
        var normalizedName = NormalizeName(name);

        // Create the player
        var player = new Player(
            normalizedName,
            raceId,
            backgroundId,
            finalAttributes,
            description);

        _logger.LogInformation(
            "Created character: {Name} ({Race} {Background}), Attributes: {Attributes}",
            normalizedName, race.Name, background.Name, finalAttributes);

        _eventLogger?.LogCharacter("CharacterCreated", $"Created {normalizedName}",
            data: new Dictionary<string, object>
            {
                ["name"] = normalizedName,
                ["raceId"] = raceId,
                ["raceName"] = race.Name,
                ["backgroundId"] = backgroundId,
                ["backgroundName"] = background.Name,
                ["might"] = finalAttributes.Might,
                ["fortitude"] = finalAttributes.Fortitude,
                ["will"] = finalAttributes.Will,
                ["wits"] = finalAttributes.Wits,
                ["finesse"] = finalAttributes.Finesse
            });

        return player;
    }

    /// <summary>
    /// Calculates final attributes with all modifiers applied.
    /// </summary>
    /// <param name="baseAttributes">Base attributes from point-buy.</param>
    /// <param name="raceId">The selected race.</param>
    /// <param name="backgroundId">The selected background.</param>
    /// <returns>Final attributes with all modifiers.</returns>
    public PlayerAttributes CalculateFinalAttributes(
        PlayerAttributes baseAttributes,
        string raceId,
        string backgroundId)
    {
        var result = baseAttributes;

        var race = _configProvider.GetRaceById(raceId);
        if (race != null)
        {
            result = result.WithModifiers(race.AttributeModifiers);
        }

        var background = _configProvider.GetBackgroundById(backgroundId);
        if (background != null)
        {
            result = result.WithModifiers(background.AttributeBonuses);
        }

        return result;
    }
}
