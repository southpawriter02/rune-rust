// ------------------------------------------------------------------------------
// <copyright file="TerminalContext.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Contextual information for a terminal hacking attempt including terminal type,
// security level, corruption, and tool quality.
// Part of v0.15.4b Terminal Hacking System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contextual information for a terminal hacking attempt.
/// </summary>
/// <remarks>
/// <para>
/// Aggregates all factors affecting terminal infiltration checks including
/// terminal type, security level, corruption, and tool quality.
/// </para>
/// <para>
/// Layer DCs are calculated as:
/// <code>
/// Layer 1 DC = TerminalType base DC + corruption modifier + failure modifier
/// Layer 2 DC = Layer 1 DC + security level modifier
/// Layer 3 DC = Data type DC
/// </code>
/// </para>
/// </remarks>
public sealed class TerminalContext
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// DC modifier applied per previous Layer 1 failure.
    /// </summary>
    public const int FailureDcPenaltyPerAttempt = 2;

    /// <summary>
    /// Default DC used for Glitched Manifold terminals.
    /// </summary>
    public const int GlitchedManifoldDefaultDc = 14;

    /// <summary>
    /// Minimum DC for Glitched Manifold terminals.
    /// </summary>
    public const int GlitchedManifoldMinDc = 12;

    /// <summary>
    /// Maximum DC for Glitched Manifold terminals.
    /// </summary>
    public const int GlitchedManifoldMaxDc = 20;

    // -------------------------------------------------------------------------
    // Core Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the type/classification of the terminal.
    /// </summary>
    public TerminalType TerminalType { get; }

    /// <summary>
    /// Gets the corruption level affecting the terminal.
    /// </summary>
    /// <remarks>
    /// Normal: No modifier.
    /// Glitched: +2 DC.
    /// Blighted: +4 DC.
    /// </remarks>
    public CorruptionTier CorruptionLevel { get; }

    /// <summary>
    /// Gets the security level for Layer 2 authentication.
    /// </summary>
    public SecurityLevel SecurityLevel { get; }

    /// <summary>
    /// Gets the type of data being sought (for Layer 3).
    /// </summary>
    public DataType? TargetDataType { get; }

    /// <summary>
    /// Gets the quality of hacking tools being used.
    /// </summary>
    public ToolQuality ToolQuality { get; }

    /// <summary>
    /// Gets the number of previous failed Layer 1 attempts.
    /// </summary>
    public int PreviousLayer1Failures { get; }

    /// <summary>
    /// Gets the unique identifier of the terminal entity.
    /// </summary>
    public string TerminalId { get; }

    /// <summary>
    /// Gets the display name of the terminal for UI messages.
    /// </summary>
    public string? TerminalName { get; }

    // -------------------------------------------------------------------------
    // Computed DC Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the base difficulty class from the terminal type.
    /// </summary>
    public int BaseDc => TerminalType == TerminalType.GlitchedManifold
        ? GetGlitchedManifoldDc()
        : (int)TerminalType;

    /// <summary>
    /// Gets the DC modifier from corruption level.
    /// </summary>
    public int CorruptionDcModifier => CorruptionLevel switch
    {
        CorruptionTier.Normal => 0,
        CorruptionTier.Glitched => 2,
        CorruptionTier.Blighted => 4,
        _ => 0
    };

    /// <summary>
    /// Gets the DC modifier from Layer 1 failures.
    /// </summary>
    public int FailureDcModifier => PreviousLayer1Failures * FailureDcPenaltyPerAttempt;

    /// <summary>
    /// Gets the Layer 1 (Access) effective DC.
    /// </summary>
    public int Layer1Dc => BaseDc + CorruptionDcModifier + FailureDcModifier;

    /// <summary>
    /// Gets the DC modifier from security level.
    /// </summary>
    public int SecurityDcModifier => (int)SecurityLevel;

    /// <summary>
    /// Gets the Layer 2 (Authentication) effective DC.
    /// </summary>
    public int Layer2Dc => Layer1Dc + SecurityDcModifier;

    /// <summary>
    /// Gets the Layer 3 (Navigation) DC based on data type.
    /// </summary>
    public int Layer3Dc => TargetDataType.HasValue
        ? (int)TargetDataType.Value
        : (int)DataType.InternalDocuments; // Default to internal documents

    // -------------------------------------------------------------------------
    // Computed Dice Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the dice pool modifier from tool quality.
    /// </summary>
    public int ToolDiceModifier => ToolQuality switch
    {
        ToolQuality.BareHands => -2,
        ToolQuality.Improvised => 0,
        ToolQuality.Proper => 1,
        ToolQuality.Masterwork => 2,
        _ => 0
    };

    // -------------------------------------------------------------------------
    // Time Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the time in rounds for Layer 1 based on terminal type.
    /// </summary>
    public int Layer1TimeRounds => TerminalType switch
    {
        TerminalType.CivilianDataPort => 1,
        TerminalType.CorporateMainframe => 1,
        TerminalType.SecurityHub => 2,
        TerminalType.MilitaryServer => 3,
        TerminalType.JotunArchive => 5,
        TerminalType.GlitchedManifold => Random.Shared.Next(1, 4), // 1-3 rounds
        _ => 1
    };

    // -------------------------------------------------------------------------
    // ICE Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the ICE rating for this terminal type.
    /// </summary>
    /// <remarks>
    /// ICE mechanics detailed in v0.15.4c.
    /// </remarks>
    public int IceRating => TerminalType switch
    {
        TerminalType.CivilianDataPort => 0,  // No ICE
        TerminalType.CorporateMainframe => 12, // Passive
        TerminalType.SecurityHub => 16,       // Active
        TerminalType.MilitaryServer => 20,    // Active + Lethal
        TerminalType.JotunArchive => 24,      // Lethal
        TerminalType.GlitchedManifold => Random.Shared.Next(12, 25), // Variable
        _ => 0
    };

    // -------------------------------------------------------------------------
    // Display Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the display name of the terminal, defaulting to the terminal type if no name is set.
    /// </summary>
    public string DisplayName => TerminalName ?? TerminalType.ToString();

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new terminal context.
    /// </summary>
    /// <param name="terminalId">Unique identifier for the terminal.</param>
    /// <param name="terminalType">Classification of the terminal.</param>
    /// <param name="corruptionLevel">Corruption tier affecting the terminal.</param>
    /// <param name="securityLevel">Security level for Layer 2 authentication.</param>
    /// <param name="targetDataType">Type of data being sought (for Layer 3).</param>
    /// <param name="toolQuality">Quality of tools being used.</param>
    /// <param name="previousLayer1Failures">Number of prior Layer 1 failed attempts.</param>
    /// <param name="terminalName">Optional display name.</param>
    /// <exception cref="ArgumentNullException">Thrown when terminalId is null.</exception>
    public TerminalContext(
        string terminalId,
        TerminalType terminalType,
        CorruptionTier corruptionLevel = CorruptionTier.Normal,
        SecurityLevel securityLevel = SecurityLevel.PasswordOnly,
        DataType? targetDataType = null,
        ToolQuality toolQuality = ToolQuality.Improvised,
        int previousLayer1Failures = 0,
        string? terminalName = null)
    {
        TerminalId = terminalId ?? throw new ArgumentNullException(nameof(terminalId));
        TerminalType = terminalType;
        CorruptionLevel = corruptionLevel;
        SecurityLevel = securityLevel;
        TargetDataType = targetDataType;
        ToolQuality = toolQuality;
        PreviousLayer1Failures = Math.Max(0, previousLayer1Failures);
        TerminalName = terminalName;
    }

    // -------------------------------------------------------------------------
    // Mutation Methods (Create New Instances)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a copy with incremented Layer 1 failures.
    /// </summary>
    /// <returns>A new TerminalContext with PreviousLayer1Failures + 1.</returns>
    public TerminalContext WithLayer1Failure()
    {
        return new TerminalContext(
            TerminalId,
            TerminalType,
            CorruptionLevel,
            SecurityLevel,
            TargetDataType,
            ToolQuality,
            PreviousLayer1Failures + 1,
            TerminalName);
    }

    /// <summary>
    /// Creates a copy with specified target data type.
    /// </summary>
    /// <param name="dataType">The target data type.</param>
    /// <returns>A new TerminalContext with the specified data type.</returns>
    public TerminalContext WithTargetData(DataType dataType)
    {
        return new TerminalContext(
            TerminalId,
            TerminalType,
            CorruptionLevel,
            SecurityLevel,
            dataType,
            ToolQuality,
            PreviousLayer1Failures,
            TerminalName);
    }

    /// <summary>
    /// Creates a copy with a different tool quality.
    /// </summary>
    /// <param name="newToolQuality">The new tool quality.</param>
    /// <returns>A new TerminalContext with the updated tool quality.</returns>
    public TerminalContext WithToolQuality(ToolQuality newToolQuality)
    {
        return new TerminalContext(
            TerminalId,
            TerminalType,
            CorruptionLevel,
            SecurityLevel,
            TargetDataType,
            newToolQuality,
            PreviousLayer1Failures,
            TerminalName);
    }

    // -------------------------------------------------------------------------
    // Conversion Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Converts this context to a SkillContext for skill check integration.
    /// </summary>
    /// <param name="layer">The infiltration layer being attempted.</param>
    /// <returns>A SkillContext with equipment and environment modifiers applied.</returns>
    /// <remarks>
    /// <para>
    /// The conversion creates modifiers for:
    /// <list type="bullet">
    ///   <item><description>Tool quality as equipment modifier (dice bonus/penalty)</description></item>
    ///   <item><description>Corruption level as environment modifier (DC increase)</description></item>
    ///   <item><description>Failure modifier as situational (Layer 1 only, DC increase)</description></item>
    ///   <item><description>Security modifier as situational (Layer 2 only, DC increase)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public SkillContext ToSkillContext(InfiltrationLayer layer)
    {
        var equipmentModifiers = new List<EquipmentModifier>();
        var situationalModifiers = new List<SituationalModifier>();
        var environmentModifiers = new List<EnvironmentModifier>();

        // Add tool modifier as equipment
        if (ToolDiceModifier != 0)
        {
            var toolName = ToolQuality switch
            {
                ToolQuality.BareHands => "Bare Hands",
                ToolQuality.Improvised => "Improvised Probe",
                ToolQuality.Proper => "Wire Probe",
                ToolQuality.Masterwork => "Masterwork Interface",
                _ => "Unknown Tools"
            };

            equipmentModifiers.Add(new EquipmentModifier(
                EquipmentId: $"tool-{ToolQuality.ToString().ToLowerInvariant()}",
                EquipmentName: toolName,
                DiceModifier: ToolDiceModifier,
                DcModifier: 0,
                EquipmentCategory: EquipmentCategory.Tool,
                RequiredForCheck: false));
        }

        // Add corruption modifier as environment
        if (CorruptionLevel != CorruptionTier.Normal)
        {
            environmentModifiers.Add(EnvironmentModifier.FromCorruption(CorruptionLevel));
        }

        // Add failure modifier as situational (Layer 1 only)
        if (layer == InfiltrationLayer.Layer1_Access && FailureDcModifier != 0)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "previous-access-failures",
                Name: "Previous Access Failures",
                DiceModifier: 0,
                DcModifier: FailureDcModifier,
                Source: $"{PreviousLayer1Failures} failed attempt(s)",
                Duration: ModifierDuration.Persistent,
                IsStackable: false,
                Description: $"Security protocols tightened after {PreviousLayer1Failures} failed access attempt(s)."));
        }

        // Add security modifier (Layer 2 only)
        if (layer == InfiltrationLayer.Layer2_Authentication && SecurityDcModifier != 0)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: $"security-{SecurityLevel.ToString().ToLowerInvariant()}",
                Name: $"Security Level ({SecurityLevel})",
                DiceModifier: 0,
                DcModifier: SecurityDcModifier,
                Source: GetSecurityDescription(),
                Duration: ModifierDuration.Persistent,
                IsStackable: false,
                Description: GetSecurityDescription()));
        }

        return new SkillContext(
            equipmentModifiers.AsReadOnly(),
            situationalModifiers.AsReadOnly(),
            environmentModifiers.AsReadOnly(),
            Array.Empty<TargetModifier>(),
            Array.Empty<string>());
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods
    // -------------------------------------------------------------------------

    private int GetGlitchedManifoldDc()
    {
        // Glitched Manifold has variable DC based on glitch phase
        // Determined dynamically when encountered
        return Random.Shared.Next(GlitchedManifoldMinDc, GlitchedManifoldMaxDc + 1);
    }

    private string GetSecurityDescription()
    {
        return SecurityLevel switch
        {
            SecurityLevel.PasswordOnly => "Simple password authentication",
            SecurityLevel.Biometric => "Biometric verification required",
            SecurityLevel.MultiFactor => "Multi-factor authentication",
            SecurityLevel.JotunLocked => "Ancient Jötun authentication protocols",
            _ => "Unknown security"
        };
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override string ToString()
    {
        var parts = new List<string>
        {
            $"{TerminalType} (Layer DCs: {Layer1Dc}/{Layer2Dc}/{Layer3Dc})"
        };

        if (CorruptionLevel != CorruptionTier.Normal)
            parts.Add($"[{CorruptionLevel}]");

        parts.Add($"Security: {SecurityLevel}");
        var diceModStr = ToolDiceModifier >= 0 ? $"+{ToolDiceModifier}" : $"{ToolDiceModifier}";
        parts.Add($"Tools: {ToolQuality} ({diceModStr}d10)");

        if (IceRating > 0)
            parts.Add($"ICE: {IceRating}");

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Returns a compact summary for logging.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"Terminal[{TerminalId}:{TerminalType}] " +
               $"DC={Layer1Dc}/{Layer2Dc}/{Layer3Dc} " +
               $"tools={ToolQuality} security={SecurityLevel} " +
               $"corruption={CorruptionLevel} failures={PreviousLayer1Failures}";
    }

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a simple terminal context with default settings.
    /// </summary>
    /// <param name="terminalId">Unique identifier for the terminal.</param>
    /// <param name="terminalType">Classification of the terminal.</param>
    /// <param name="toolQuality">Quality of tools to use.</param>
    /// <returns>A new TerminalContext with default security and no corruption.</returns>
    public static TerminalContext Create(
        string terminalId,
        TerminalType terminalType,
        ToolQuality toolQuality = ToolQuality.Proper)
    {
        return new TerminalContext(terminalId, terminalType, toolQuality: toolQuality);
    }

    /// <summary>
    /// Creates a corrupted terminal context.
    /// </summary>
    /// <param name="terminalId">Unique identifier for the terminal.</param>
    /// <param name="terminalType">Classification of the terminal.</param>
    /// <param name="corruption">Corruption tier.</param>
    /// <param name="toolQuality">Quality of tools to use.</param>
    /// <returns>A new TerminalContext with the specified corruption.</returns>
    public static TerminalContext CreateCorrupted(
        string terminalId,
        TerminalType terminalType,
        CorruptionTier corruption,
        ToolQuality toolQuality = ToolQuality.Proper)
    {
        return new TerminalContext(terminalId, terminalType, corruption, toolQuality: toolQuality);
    }

    /// <summary>
    /// Creates a secured terminal context.
    /// </summary>
    /// <param name="terminalId">Unique identifier for the terminal.</param>
    /// <param name="terminalType">Classification of the terminal.</param>
    /// <param name="securityLevel">Security level for authentication.</param>
    /// <param name="toolQuality">Quality of tools to use.</param>
    /// <returns>A new TerminalContext with the specified security level.</returns>
    public static TerminalContext CreateSecured(
        string terminalId,
        TerminalType terminalType,
        SecurityLevel securityLevel,
        ToolQuality toolQuality = ToolQuality.Proper)
    {
        return new TerminalContext(terminalId, terminalType, securityLevel: securityLevel, toolQuality: toolQuality);
    }
}
