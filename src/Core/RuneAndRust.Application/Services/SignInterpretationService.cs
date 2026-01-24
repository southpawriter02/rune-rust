using System.Globalization;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service implementation for scavenger sign interpretation in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// Implements the Scavenger Sign Reading mechanics for the Wasteland Survival skill.
/// Allows characters to interpret markings left by wasteland factions to gain information
/// about territory boundaries, dangers, hidden caches, safe paths, and more.
/// </para>
/// <para>
/// DC calculation:
/// <list type="bullet">
///   <item><description>Base DC: From sign type (10, 12, or 14)</description></item>
///   <item><description>Unknown faction: +4 DC modifier</description></item>
///   <item><description>Sign age: +0 (Fresh/Recent), +1 (Old), +2 (Faded), +4 (Ancient)</description></item>
/// </list>
/// </para>
/// <para>
/// Interpretation outcomes:
/// <list type="bullet">
///   <item><description>Success (net >= DC): Sign type and meaning revealed</description></item>
///   <item><description>Critical (net >= 5): Additional context provided</description></item>
///   <item><description>Failure (net &lt; DC): No information gained</description></item>
///   <item><description>Fumble (0 successes + botch): Misinterpretation with false info</description></item>
/// </list>
/// </para>
/// </remarks>
public class SignInterpretationService : ISignInterpretationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - SKILL IDENTIFICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The skill ID for Wasteland Survival used in sign interpretation checks.
    /// </summary>
    private const string WastelandSurvivalSkillId = "wasteland-survival";

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - BASE INTERPRETATION DCS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Base DC for Territory Marker signs (relatively standardized).
    /// </summary>
    private const int TerritoryMarkerBaseDc = 10;

    /// <summary>
    /// Base DC for Warning Sign signs (requires cultural knowledge).
    /// </summary>
    private const int WarningSignBaseDc = 12;

    /// <summary>
    /// Base DC for Cache Indicator signs (deliberately cryptic).
    /// </summary>
    private const int CacheIndicatorBaseDc = 14;

    /// <summary>
    /// Base DC for Trail Blaze signs (universal, quick to read).
    /// </summary>
    private const int TrailBlazeBaseDc = 10;

    /// <summary>
    /// Base DC for Hunt Marker signs (specialized terminology).
    /// </summary>
    private const int HuntMarkerBaseDc = 14;

    /// <summary>
    /// Base DC for Taboo Sign signs (important but clear).
    /// </summary>
    private const int TabooSignBaseDc = 12;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - DC MODIFIERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// DC modifier applied when the sign's faction is unknown to the player.
    /// </summary>
    private const int UnknownFactionDcModifier = 4;

    /// <summary>
    /// DC modifier for Fresh sign age (no modifier).
    /// </summary>
    private const int FreshAgeDcModifier = 0;

    /// <summary>
    /// DC modifier for Recent sign age (no modifier).
    /// </summary>
    private const int RecentAgeDcModifier = 0;

    /// <summary>
    /// DC modifier for Old sign age.
    /// </summary>
    private const int OldAgeDcModifier = 1;

    /// <summary>
    /// DC modifier for Faded sign age.
    /// </summary>
    private const int FadedAgeDcModifier = 2;

    /// <summary>
    /// DC modifier for Ancient sign age.
    /// </summary>
    private const int AncientAgeDcModifier = 4;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - THRESHOLDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Net successes required for critical success (additional context revealed).
    /// </summary>
    private const int CriticalSuccessThreshold = 5;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - MAJOR FACTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Major factions that are known to all players.
    /// </summary>
    private static readonly HashSet<string> MajorFactionIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "iron-covenant",
        "rust-walkers",
        "silent-ones",
        "verdant-circle",
        "ash-born"
    };

    /// <summary>
    /// Display names for major factions.
    /// </summary>
    private static readonly Dictionary<string, string> FactionDisplayNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["iron-covenant"] = "Iron Covenant",
            ["rust-walkers"] = "Rust Walkers",
            ["silent-ones"] = "Silent Ones",
            ["verdant-circle"] = "Verdant Circle",
            ["ash-born"] = "Ash-Born"
        };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly SkillCheckService _skillCheckService;
    private readonly ILogger<SignInterpretationService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the SignInterpretationService.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is null.
    /// </exception>
    public SignInterpretationService(
        SkillCheckService skillCheckService,
        ILogger<SignInterpretationService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("SignInterpretationService initialized successfully");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SIGN INTERPRETATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public SignInterpretationResult InterpretSign(Player player, ScavengerSign sign)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Check if interpretation can proceed
        if (!CanInterpret(player))
        {
            var reason = GetInterpretationBlockedReason(player);
            _logger.LogWarning(
                "Interpretation blocked for PlayerId={PlayerId}: {Reason}",
                player.Id, reason);

            return SignInterpretationResult.Failure(0, 0, $"Interpretation blocked: {reason}");
        }

        // Calculate DC
        var factionKnown = IsFactionKnown(player, sign.FactionId);
        var dc = GetSignDc(player, sign);

        _logger.LogInformation(
            "PlayerId={PlayerId} attempting to interpret {SignType} from faction '{FactionId}' " +
            "(Age={Age}, FactionKnown={FactionKnown}, DC={Dc})",
            player.Id, sign.SignType, sign.FactionId, sign.Age, factionKnown, dc);

        // Perform the skill check
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            WastelandSurvivalSkillId,
            dc,
            $"Interpret {sign.SignType.GetDisplayName()}",
            AdvantageType.Normal,
            0);

        var netSuccesses = checkResult.NetSuccesses;
        var rollDetails = BuildRollDetails(checkResult, sign, dc);

        _logger.LogDebug(
            "Skill check result for PlayerId={PlayerId}: " +
            "NetSuccesses={NetSuccesses}, TotalSuccesses={TotalSuccesses}, " +
            "TotalBotches={TotalBotches}, IsFumble={IsFumble}",
            player.Id, netSuccesses,
            checkResult.DiceResult.TotalSuccesses,
            checkResult.DiceResult.TotalBotches,
            checkResult.IsFumble);

        // Handle fumble (misinterpretation)
        if (checkResult.IsFumble)
        {
            _logger.LogWarning(
                "PlayerId={PlayerId} FUMBLED interpretation of {SignType}: Misinterpretation!",
                player.Id, sign.SignType);

            var falseMeaning = GetMisinterpretation(sign.SignType);
            return SignInterpretationResult.Misinterpretation(
                sign.SignType,
                falseMeaning,
                GetFactionDisplayName(sign.FactionId),
                dc,
                rollDetails);
        }

        // Check for success
        if (netSuccesses >= dc)
        {
            var isCritical = netSuccesses >= CriticalSuccessThreshold;
            var factionDisplayName = GetFactionDisplayName(sign.FactionId);

            _logger.LogInformation(
                "PlayerId={PlayerId} successfully interpreted {SignType}: " +
                "{NetSuccesses} successes vs DC {Dc}{Critical}",
                player.Id, sign.SignType, netSuccesses, dc,
                isCritical ? " [CRITICAL]" : "");

            // If faction was unknown, player now learns about it
            if (!factionKnown)
            {
                player.AddKnownFaction(sign.FactionId);
                _logger.LogInformation(
                    "PlayerId={PlayerId} learned about faction '{FactionId}'",
                    player.Id, sign.FactionId);
            }

            // Create success result
            if (isCritical)
            {
                var additionalContext = GetCriticalContext(sign.SignType);
                return SignInterpretationResult.CriticalSuccess(
                    sign.SignType,
                    sign.Meaning,
                    factionDisplayName,
                    factionKnown,
                    additionalContext,
                    netSuccesses,
                    dc,
                    rollDetails);
            }

            return SignInterpretationResult.Success(
                sign.SignType,
                sign.Meaning,
                factionDisplayName,
                factionKnown,
                netSuccesses,
                dc,
                rollDetails);
        }

        // Failure
        _logger.LogDebug(
            "PlayerId={PlayerId} failed to interpret {SignType}: " +
            "{NetSuccesses} successes vs DC {Dc}",
            player.Id, sign.SignType, netSuccesses, dc);

        return SignInterpretationResult.Failure(netSuccesses, dc, rollDetails);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DC CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int GetSignDc(Player player, ScavengerSign sign)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var baseDc = GetBaseDc(sign.SignType);
        var ageModifier = GetAgeDcModifier(sign.Age);
        var factionModifier = IsFactionKnown(player, sign.FactionId) ? 0 : UnknownFactionDcModifier;

        var totalDc = baseDc + ageModifier + factionModifier;

        _logger.LogDebug(
            "DC calculation for {SignType}: Base={BaseDc} + Age={AgeModifier} + " +
            "Faction={FactionModifier} = {TotalDc}",
            sign.SignType, baseDc, ageModifier, factionModifier, totalDc);

        return totalDc;
    }

    /// <inheritdoc/>
    public int GetBaseDc(ScavengerSignType signType)
    {
        var dc = signType switch
        {
            ScavengerSignType.TerritoryMarker => TerritoryMarkerBaseDc,
            ScavengerSignType.WarningSign => WarningSignBaseDc,
            ScavengerSignType.CacheIndicator => CacheIndicatorBaseDc,
            ScavengerSignType.TrailBlaze => TrailBlazeBaseDc,
            ScavengerSignType.HuntMarker => HuntMarkerBaseDc,
            ScavengerSignType.TabooSign => TabooSignBaseDc,
            _ => WarningSignBaseDc // Default to moderate DC
        };

        _logger.LogDebug("Base DC for {SignType}: {Dc}", signType, dc);

        return dc;
    }

    /// <inheritdoc/>
    public int GetAgeDcModifier(SignAge age)
    {
        var modifier = age switch
        {
            SignAge.Fresh => FreshAgeDcModifier,
            SignAge.Recent => RecentAgeDcModifier,
            SignAge.Old => OldAgeDcModifier,
            SignAge.Faded => FadedAgeDcModifier,
            SignAge.Ancient => AncientAgeDcModifier,
            _ => FreshAgeDcModifier
        };

        _logger.LogDebug("Age DC modifier for {Age}: +{Modifier}", age, modifier);

        return modifier;
    }

    /// <inheritdoc/>
    public int GetUnknownFactionModifier() => UnknownFactionDcModifier;

    // ═══════════════════════════════════════════════════════════════════════════
    // SIGN MEANING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public string GetSignMeaning(ScavengerSignType signType)
    {
        return signType.GetSuccessMeaning();
    }

    /// <inheritdoc/>
    public string GetCriticalContext(ScavengerSignType signType)
    {
        return signType.GetCriticalContext();
    }

    /// <inheritdoc/>
    public string GetMisinterpretation(ScavengerSignType signType)
    {
        return signType.GetMisinterpretation();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTION KNOWLEDGE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool IsFactionKnown(Player player, string factionId)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        if (string.IsNullOrWhiteSpace(factionId))
        {
            _logger.LogWarning("IsFactionKnown called with null or empty factionId");
            return false;
        }

        // Major factions are always known
        if (MajorFactionIds.Contains(factionId))
        {
            _logger.LogDebug(
                "Faction '{FactionId}' is a major faction (always known)",
                factionId);
            return true;
        }

        // Check player's learned factions
        var isKnown = player.KnowsFaction(factionId);

        _logger.LogDebug(
            "Faction '{FactionId}' known to PlayerId={PlayerId}: {IsKnown}",
            factionId, player.Id, isKnown);

        return isKnown;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SIGN INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public string GetSignDisplayName(ScavengerSignType signType)
    {
        return signType.GetDisplayName();
    }

    /// <inheritdoc/>
    public string GetSignDescription(ScavengerSignType signType)
    {
        return signType.GetDescription();
    }

    /// <inheritdoc/>
    public string GetFactionDisplayName(string factionId)
    {
        if (string.IsNullOrWhiteSpace(factionId))
        {
            return "Unknown Faction";
        }

        // Check known faction names
        if (FactionDisplayNames.TryGetValue(factionId, out var displayName))
        {
            return displayName;
        }

        // Convert kebab-case to title case for unknown factions
        return ConvertKebabCaseToTitleCase(factionId);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SIGN AGE INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public string GetAgeDisplayString(SignAge age)
    {
        return age.ToDisplayString();
    }

    /// <inheritdoc/>
    public string? GetReliabilityWarning(SignAge age)
    {
        return age.GetReliabilityWarning();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERPRETATION PREREQUISITES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool CanInterpret(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Check for blocking status effects
        // Note: This would need integration with a status effect service
        // For now, we assume interpretation is allowed unless explicitly blocked

        _logger.LogDebug(
            "Interpretation allowed for PlayerId={PlayerId}",
            player.Id);

        return true;
    }

    /// <inheritdoc/>
    public string? GetInterpretationBlockedReason(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Check for blocking conditions
        // Would check for Blinded or other relevant statuses

        return null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Builds a human-readable roll details string for the interpretation result.
    /// </summary>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="sign">The sign being interpreted.</param>
    /// <param name="dc">The interpretation DC.</param>
    /// <returns>A formatted string describing the roll details.</returns>
    private static string BuildRollDetails(SkillCheckResult checkResult, ScavengerSign sign, int dc)
    {
        return $"Roll: {checkResult.DiceResult.TotalSuccesses} successes, " +
               $"{checkResult.DiceResult.TotalBotches} botches, " +
               $"Net: {checkResult.NetSuccesses} | " +
               $"DC: {dc} ({sign.SignType.GetDisplayName()}, {sign.Age.GetDisplayName()})";
    }

    /// <summary>
    /// Converts a kebab-case string to title case.
    /// </summary>
    /// <param name="kebabCase">The kebab-case string (e.g., "iron-covenant").</param>
    /// <returns>The title case string (e.g., "Iron Covenant").</returns>
    private static string ConvertKebabCaseToTitleCase(string kebabCase)
    {
        if (string.IsNullOrWhiteSpace(kebabCase))
        {
            return string.Empty;
        }

        var words = kebabCase.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var textInfo = CultureInfo.InvariantCulture.TextInfo;

        for (var i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = textInfo.ToTitleCase(words[i].ToLowerInvariant());
            }
        }

        return string.Join(" ", words);
    }
}
