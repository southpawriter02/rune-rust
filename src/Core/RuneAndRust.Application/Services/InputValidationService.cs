using Microsoft.Extensions.Logging;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for validating and sanitizing user input.
/// </summary>
public class InputValidationService
{
    private readonly ILogger<InputValidationService> _logger;

    /// <summary>
    /// Creates a new InputValidationService instance.
    /// </summary>
    /// <param name="logger">The logger for service diagnostics.</param>
    public InputValidationService(ILogger<InputValidationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates a player name.
    /// </summary>
    /// <param name="name">The player name to validate.</param>
    /// <returns>A tuple indicating validity and an optional error message.</returns>
    public (bool IsValid, string? ErrorMessage) ValidatePlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogDebug("Player name validation failed: empty name");
            return (false, "Name cannot be empty.");
        }

        if (name.Length > 50)
        {
            _logger.LogDebug("Player name validation failed: name too long ({Length} chars)", name.Length);
            return (false, "Name cannot exceed 50 characters.");
        }

        if (name.Any(char.IsControl))
        {
            _logger.LogDebug("Player name validation failed: contains control characters");
            return (false, "Name contains invalid characters.");
        }

        _logger.LogDebug("Player name validation passed: {Name}", name);
        return (true, null);
    }

    /// <summary>
    /// Validates an item name argument.
    /// </summary>
    /// <param name="itemName">The item name to validate.</param>
    /// <returns>A tuple indicating validity and an optional error message.</returns>
    public (bool IsValid, string? ErrorMessage) ValidateItemName(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            _logger.LogDebug("Item name validation failed: empty name");
            return (false, "Item name cannot be empty.");
        }

        if (itemName.Length > 100)
        {
            _logger.LogDebug("Item name validation failed: name too long ({Length} chars)", itemName.Length);
            return (false, "Item name too long.");
        }

        return (true, null);
    }

    /// <summary>
    /// Sanitizes user input to prevent injection attacks.
    /// </summary>
    /// <param name="input">The input to sanitize.</param>
    /// <returns>The sanitized input string.</returns>
    public string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // Remove control characters except newlines
        var sanitized = new string(input.Where(c => !char.IsControl(c) || c == '\n' || c == '\r').ToArray());

        if (sanitized != input)
        {
            _logger.LogDebug("Input sanitized: removed {Count} characters", input.Length - sanitized.Length);
        }

        return sanitized;
    }
}
