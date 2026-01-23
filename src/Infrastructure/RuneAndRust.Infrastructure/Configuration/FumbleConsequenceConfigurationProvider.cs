namespace RuneAndRust.Infrastructure.Configuration;

using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Provides default configuration for fumble consequences.
/// </summary>
/// <remarks>
/// This is a hardcoded implementation. In a production environment,
/// this would be loaded from fumble-consequences.json configuration file.
/// </remarks>
public sealed class FumbleConsequenceConfigurationProvider : IFumbleConsequenceConfigurationProvider
{
    private static readonly Dictionary<FumbleType, FumbleConsequenceConfiguration> Configurations = new()
    {
        [FumbleType.TrustShattered] = new FumbleConsequenceConfiguration(
            Description: "Your words have shattered any trust this person had in you.",
            Duration: null,
            RecoveryCondition: "complete-favor-quest"),

        [FumbleType.LieExposed] = new FumbleConsequenceConfiguration(
            Description: "Your deception has been exposed, and word has spread throughout the faction.",
            Duration: null,
            RecoveryCondition: "improve-faction-standing"),

        [FumbleType.ChallengeAccepted] = new FumbleConsequenceConfiguration(
            Description: "Your target has accepted your challenge and attacks with renewed fury.",
            Duration: null,
            RecoveryCondition: "win-or-escape-combat"),

        [FumbleType.MechanismJammed] = new FumbleConsequenceConfiguration(
            Description: "The lock mechanism has jammed, making future attempts significantly more difficult.",
            Duration: null,
            RecoveryCondition: "find-alternate-entry"),

        [FumbleType.SystemLockout] = new FumbleConsequenceConfiguration(
            Description: "The terminal has locked you out and triggered a security alert.",
            Duration: null,
            RecoveryCondition: "find-admin-credentials"),

        [FumbleType.ForcedExecution] = new FumbleConsequenceConfiguration(
            Description: "The trap triggers immediately, catching you in the blast.",
            Duration: null,
            RecoveryCondition: null),

        [FumbleType.TheSlip] = new FumbleConsequenceConfiguration(
            Description: "You lose your grip and fall, taking damage and stress.",
            Duration: null,
            RecoveryCondition: null),

        [FumbleType.TheLongFall] = new FumbleConsequenceConfiguration(
            Description: "Your landing is catastrophic, dealing extra damage and leaving you disoriented.",
            Duration: TimeSpan.FromMinutes(2),
            RecoveryCondition: null),

        [FumbleType.SystemWideAlert] = new FumbleConsequenceConfiguration(
            Description: "You've alerted all nearby enemies, who now converge on your location.",
            Duration: null,
            RecoveryCondition: "defeat-or-escape-enemies")
    };

    /// <inheritdoc />
    public FumbleConsequenceConfiguration GetConfiguration(FumbleType fumbleType)
    {
        if (Configurations.TryGetValue(fumbleType, out var config))
        {
            return config;
        }

        // Default configuration for unknown fumble types
        return new FumbleConsequenceConfiguration(
            Description: "An unexpected fumble has occurred.",
            Duration: null,
            RecoveryCondition: null);
    }
}
