namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Provides configuration data for fumble consequences.
/// </summary>
public interface IFumbleConsequenceConfigurationProvider
{
    /// <summary>
    /// Gets the configuration for a specific fumble type.
    /// </summary>
    /// <param name="fumbleType">The fumble type to get configuration for.</param>
    /// <returns>The configuration for the fumble type.</returns>
    FumbleConsequenceConfiguration GetConfiguration(FumbleType fumbleType);
}

/// <summary>
/// Configuration record for a fumble consequence type.
/// </summary>
/// <param name="Description">The human-readable description of the consequence.</param>
/// <param name="Duration">The optional duration before the consequence expires.</param>
/// <param name="RecoveryCondition">The optional condition identifier that allows recovery.</param>
public record FumbleConsequenceConfiguration(
    string Description,
    TimeSpan? Duration,
    string? RecoveryCondition);
