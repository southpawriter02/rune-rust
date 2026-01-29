namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using System.Text;

/// <summary>
/// Service for formatting legendary item presentations.
/// Provides rich text formatting for drop announcements and item examination.
/// </summary>
/// <remarks>
/// <para>
/// LegendaryPresentationService implements <see cref="ILegendaryPresentation"/>
/// to provide consistent, visually distinctive formatting for Myth-Forged items.
/// The service produces plain text output suitable for both TUI and as a base
/// for richer GUI rendering.
/// </para>
/// <para>
/// The service uses Unicode characters for visual appeal:
/// <list type="bullet">
///   <item><description>═ (double line) for borders</description></item>
///   <item><description>─ (single line) for dividers</description></item>
///   <item><description>✦ for legendary symbols and effect bullets</description></item>
///   <item><description>├─ and └─ for stat tree formatting</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var service = new LegendaryPresentationService(logger);
/// 
/// // Format a drop announcement
/// var announcement = service.FormatDropAnnouncement(dropEvent);
/// 
/// // Format detailed examination
/// var examination = service.FormatExaminationText(uniqueItem);
/// </code>
/// </example>
public class LegendaryPresentationService : ILegendaryPresentation
{
    #region Constants

    /// <summary>
    /// Double-line box drawing character for borders.
    /// </summary>
    private const char BorderChar = '═';

    /// <summary>
    /// Single-line box drawing character for dividers.
    /// </summary>
    private const char DividerChar = '─';

    /// <summary>
    /// Symbol used for legendary header and effect bullets.
    /// </summary>
    private const string LegendarySymbol = "✦";

    /// <summary>
    /// Bullet character for special effects.
    /// </summary>
    private const string EffectBullet = "✦";

    /// <summary>
    /// Tree branch character for non-final stat lines.
    /// </summary>
    private const string StatBullet = "├─";

    /// <summary>
    /// Tree end character for final stat lines.
    /// </summary>
    private const string StatBulletLast = "└─";

    /// <summary>
    /// Default width for announcement frames.
    /// </summary>
    private const int DefaultFrameWidth = 47;

    /// <summary>
    /// Width for examination dividers.
    /// </summary>
    private const int DividerWidth = 37;

    /// <summary>
    /// Width for examination double-line borders.
    /// </summary>
    private const int ExaminationBorderWidth = 54;

    #endregion

    #region Fields

    private readonly ILogger<LegendaryPresentationService>? _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new LegendaryPresentationService instance.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public LegendaryPresentationService(ILogger<LegendaryPresentationService>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug("LegendaryPresentationService initialized");
    }

    #endregion

    #region ILegendaryPresentation Implementation

    /// <inheritdoc />
    public string FormatDropAnnouncement(MythForgedDropEvent dropEvent)
    {
        _logger?.LogDebug(
            "Formatting drop announcement for item {ItemId}",
            dropEvent.Item.ItemId);

        var sb = new StringBuilder();
        var (top, bottom) = GetAnnouncementFrame();

        // Top border
        sb.AppendLine(top);

        // Header
        sb.AppendLine($"{LegendarySymbol} LEGENDARY DROP {LegendarySymbol}");

        // Atmospheric text
        sb.AppendLine(dropEvent.GetAtmosphericText());
        sb.AppendLine();

        // Item header
        sb.AppendLine(FormatItemHeader(dropEvent.Item));

        // Stat line
        sb.AppendLine(FormatStatLine(dropEvent.Item.Stats));

        // First drop message (if applicable)
        var firstDropMsg = dropEvent.GetFirstDropMessage();
        if (firstDropMsg != null)
        {
            sb.AppendLine();
            sb.AppendLine(firstDropMsg);

            _logger?.LogInformation(
                "First Myth-Forged drop of run included in announcement: {ItemId}",
                dropEvent.Item.ItemId);
        }

        // Bottom border
        sb.AppendLine(bottom);

        _logger?.LogDebug(
            "Completed drop announcement formatting for {ItemId}, {LineCount} lines",
            dropEvent.Item.ItemId,
            sb.ToString().Split('\n').Length);

        return sb.ToString();
    }

    /// <inheritdoc />
    public string FormatExaminationText(UniqueItem item)
    {
        _logger?.LogDebug(
            "Formatting examination text for item {ItemId}",
            item.ItemId);

        var sb = new StringBuilder();
        var divider = new string(DividerChar, DividerWidth);
        var doubleLine = new string(BorderChar, ExaminationBorderWidth);

        // Top border
        sb.AppendLine(doubleLine);
        sb.AppendLine();

        // Item header
        sb.AppendLine(FormatItemHeader(item));
        sb.AppendLine(divider);
        sb.AppendLine();

        // Flavor text (if available)
        if (!string.IsNullOrWhiteSpace(item.FlavorText))
        {
            sb.AppendLine($"\"{item.FlavorText}\"");
            sb.AppendLine();

            _logger?.LogDebug(
                "Included flavor text for {ItemId}",
                item.ItemId);
        }

        // Description
        sb.AppendLine(item.Description);
        sb.AppendLine();

        // Stats section
        sb.AppendLine(divider);
        sb.AppendLine("STATS");
        sb.AppendLine(FormatStatsDetailed(item.Stats));
        sb.AppendLine();

        // Effects section (if any)
        if (item.SpecialEffectIds.Count > 0)
        {
            sb.AppendLine(divider);
            sb.AppendLine("SPECIAL EFFECTS");
            sb.AppendLine(FormatEffectIds(item.SpecialEffectIds));
            sb.AppendLine();

            _logger?.LogDebug(
                "Included {EffectCount} special effects for {ItemId}",
                item.SpecialEffectIds.Count,
                item.ItemId);
        }

        // Requirements
        sb.AppendLine(divider);
        sb.AppendLine($"Requirements: Level {item.RequiredLevel} | {FormatClassAffinities(item.ClassAffinities)}");
        sb.AppendLine();

        // Bottom border
        sb.AppendLine(doubleLine);

        _logger?.LogDebug(
            "Completed examination text formatting for {ItemId}",
            item.ItemId);

        return sb.ToString();
    }

    /// <inheritdoc />
    public string FormatStatLine(ItemStats stats)
    {
        var parts = new List<string>();

        if (stats.Might != 0)
        {
            parts.Add(FormatSingleStat(stats.Might, "MIGHT"));
        }

        if (stats.Agility != 0)
        {
            parts.Add(FormatSingleStat(stats.Agility, "AGI"));
        }

        if (stats.Will != 0)
        {
            parts.Add(FormatSingleStat(stats.Will, "WILL"));
        }

        if (stats.Fortitude != 0)
        {
            parts.Add(FormatSingleStat(stats.Fortitude, "FORT"));
        }

        if (stats.Arcana != 0)
        {
            parts.Add(FormatSingleStat(stats.Arcana, "ARC"));
        }

        if (stats.BonusDamage != 0)
        {
            parts.Add(FormatSingleStat(stats.BonusDamage, "Damage"));
        }

        if (stats.BonusDefense != 0)
        {
            parts.Add(FormatSingleStat(stats.BonusDefense, "Defense"));
        }

        if (stats.BonusHealth != 0)
        {
            parts.Add(FormatSingleStat(stats.BonusHealth, "HP"));
        }

        var result = parts.Count > 0 ? string.Join(" | ", parts) : "No stat bonuses";

        _logger?.LogDebug(
            "Formatted stat line with {StatCount} non-zero stats",
            parts.Count);

        return result;
    }

    /// <inheritdoc />
    public string FormatEffectList(IReadOnlyList<SpecialEffect> effects)
    {
        if (effects.Count == 0)
        {
            _logger?.LogDebug("Effect list is empty, returning placeholder text");
            return "No special effects";
        }

        var sb = new StringBuilder();
        foreach (var effect in effects)
        {
            var effectName = FormatEffectName(effect.EffectType);
            sb.AppendLine($"{EffectBullet} {effectName} - {effect.Description}");
        }

        _logger?.LogDebug(
            "Formatted {Count} special effects for display",
            effects.Count);

        return sb.ToString().TrimEnd();
    }

    /// <inheritdoc />
    public (string Top, string Bottom) GetAnnouncementFrame(int width = DefaultFrameWidth)
    {
        var border = new string(BorderChar, width);

        _logger?.LogDebug(
            "Generated announcement frame with width {Width}",
            width);

        return (border, border);
    }

    /// <inheritdoc />
    public string FormatItemHeader(UniqueItem item)
    {
        var prefix = TierColors.GetTierPrefix(item.QualityTier);
        var header = $"{prefix} {item.Name}";

        _logger?.LogDebug(
            "Formatted item header: {Header}",
            header);

        return header;
    }

    /// <inheritdoc />
    public string FormatClassAffinities(IReadOnlyList<string> classAffinities)
    {
        if (classAffinities.Count == 0)
        {
            _logger?.LogDebug("No class affinities specified, returning 'All Classes'");
            return "All Classes";
        }

        var formattedClasses = classAffinities
            .Select(FormatClassName)
            .ToList();

        var result = string.Join(", ", formattedClasses);

        _logger?.LogDebug(
            "Formatted {Count} class affinities: {Classes}",
            classAffinities.Count,
            result);

        return result;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Formats a single stat value with sign and abbreviation.
    /// </summary>
    /// <param name="value">The stat value.</param>
    /// <param name="abbreviation">The stat abbreviation.</param>
    /// <returns>Formatted stat string (e.g., "+5 MIGHT").</returns>
    private static string FormatSingleStat(int value, string abbreviation)
    {
        var sign = value >= 0 ? "+" : "";
        return $"{sign}{value} {abbreviation}";
    }

    /// <summary>
    /// Formats stats in detailed tree-style format for examination text.
    /// </summary>
    /// <param name="stats">The item stats to format.</param>
    /// <returns>Multi-line formatted stats with tree bullets.</returns>
    private string FormatStatsDetailed(ItemStats stats)
    {
        var lines = new List<string>();

        if (stats.Might != 0)
        {
            lines.Add($"Might:        {FormatStatValue(stats.Might)}");
        }

        if (stats.Agility != 0)
        {
            lines.Add($"Agility:      {FormatStatValue(stats.Agility)}");
        }

        if (stats.Will != 0)
        {
            lines.Add($"Will:         {FormatStatValue(stats.Will)}");
        }

        if (stats.Fortitude != 0)
        {
            lines.Add($"Fortitude:    {FormatStatValue(stats.Fortitude)}");
        }

        if (stats.Arcana != 0)
        {
            lines.Add($"Arcana:       {FormatStatValue(stats.Arcana)}");
        }

        if (stats.BonusHealth != 0)
        {
            lines.Add($"Bonus Health: {FormatStatValue(stats.BonusHealth)}");
        }

        if (stats.BonusDamage != 0)
        {
            lines.Add($"Bonus Damage: {FormatStatValue(stats.BonusDamage)}");
        }

        if (stats.BonusDefense != 0)
        {
            lines.Add($"Bonus Defense: {FormatStatValue(stats.BonusDefense)}");
        }

        if (lines.Count == 0)
        {
            return $"{StatBulletLast} No stat bonuses";
        }

        var sb = new StringBuilder();
        for (int i = 0; i < lines.Count; i++)
        {
            var bullet = i == lines.Count - 1 ? StatBulletLast : StatBullet;
            sb.AppendLine($"{bullet} {lines[i]}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Formats a stat value with sign prefix.
    /// </summary>
    /// <param name="value">The stat value.</param>
    /// <returns>Formatted value string (e.g., "+5").</returns>
    private static string FormatStatValue(int value)
    {
        var sign = value >= 0 ? "+" : "";
        return $"{sign}{value}";
    }

    /// <summary>
    /// Formats effect IDs for display (converts kebab-case to title case).
    /// </summary>
    /// <param name="effectIds">The effect IDs to format.</param>
    /// <returns>Multi-line formatted effect IDs.</returns>
    private string FormatEffectIds(IReadOnlyList<string> effectIds)
    {
        var sb = new StringBuilder();
        foreach (var effectId in effectIds)
        {
            // Format effect ID to display name (e.g., "life-steal" → "Life Steal")
            var displayName = FormatKebabCaseToTitle(effectId);
            sb.AppendLine($"{EffectBullet} {displayName}");
        }

        _logger?.LogDebug(
            "Formatted {Count} effect IDs for examination display",
            effectIds.Count);

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Converts a kebab-case string to title case.
    /// </summary>
    /// <param name="kebabCase">The kebab-case input string.</param>
    /// <returns>Title case output (e.g., "life-steal" → "Life Steal").</returns>
    private static string FormatKebabCaseToTitle(string kebabCase)
    {
        if (string.IsNullOrEmpty(kebabCase))
        {
            return string.Empty;
        }

        var words = kebabCase.Split('-');
        var titleWords = words.Select(word =>
            word.Length > 0
                ? char.ToUpper(word[0]) + word[1..]
                : word);

        return string.Join(" ", titleWords);
    }

    /// <summary>
    /// Formats a class ID for display.
    /// </summary>
    /// <param name="classId">The class ID to format.</param>
    /// <returns>Formatted class name with proper capitalization.</returns>
    private static string FormatClassName(string classId)
    {
        if (string.IsNullOrEmpty(classId))
        {
            return string.Empty;
        }

        // Handle kebab-case class IDs
        if (classId.Contains('-'))
        {
            return FormatKebabCaseToTitle(classId);
        }

        // Simple capitalization for single-word class IDs
        return char.ToUpper(classId[0]) + classId[1..];
    }

    /// <summary>
    /// Gets the display name for a special effect type.
    /// </summary>
    /// <param name="effectType">The effect type to format.</param>
    /// <returns>Player-friendly effect name.</returns>
    private static string FormatEffectName(SpecialEffectType effectType) =>
        effectType switch
        {
            SpecialEffectType.IgnoreArmor => "Armor Pierce",
            SpecialEffectType.LifeSteal => "Life Steal",
            SpecialEffectType.Cleave => "Cleave",
            SpecialEffectType.Phase => "Phase Strike",
            SpecialEffectType.Reflect => "Damage Reflect",
            SpecialEffectType.FireDamage => "Fire Damage",
            SpecialEffectType.IceDamage => "Ice Damage",
            SpecialEffectType.LightningDamage => "Lightning Damage",
            SpecialEffectType.Slow => "Slow",
            SpecialEffectType.AutoHide => "Shadow Step",
            SpecialEffectType.Detection => "True Sight",
            SpecialEffectType.CriticalBonus => "Critical Edge",
            SpecialEffectType.DamageReduction => "Damage Reduction",
            SpecialEffectType.FearAura => "Fear Aura",
            _ => effectType.ToString()
        };

    #endregion
}
