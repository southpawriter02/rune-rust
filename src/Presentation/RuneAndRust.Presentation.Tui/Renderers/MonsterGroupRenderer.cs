using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Handles text formatting and color selection for monster group display elements.
/// </summary>
/// <remarks>
/// <para>This renderer is stateless and handles all string formatting operations
/// for the monster group UI components. It reads configuration for symbols
/// and color thresholds.</para>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new MonsterGroupRenderer(config, logger);
/// var header = renderer.FormatGroupHeader("Goblin Warband", 60);
/// var card = renderer.FormatMemberCard(memberDto, 15);
/// var color = renderer.GetMemberHealthColor(75);
/// </code>
/// </example>
public class MonsterGroupRenderer
{
    private readonly MonsterGroupDisplayConfig _config;
    private readonly ILogger<MonsterGroupRenderer>? _logger;

    /// <summary>
    /// Creates a new instance of the MonsterGroupRenderer.
    /// </summary>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public MonsterGroupRenderer(
        MonsterGroupDisplayConfig? config = null,
        ILogger<MonsterGroupRenderer>? logger = null)
    {
        _config = config ?? MonsterGroupDisplayConfig.CreateDefault();
        _logger = logger;
    }

    #region Group Header Formatting

    /// <summary>
    /// Formats the group name as a centered header.
    /// </summary>
    /// <param name="groupName">The group display name.</param>
    /// <param name="totalWidth">The total width of the display area.</param>
    /// <returns>A centered, uppercase group name header.</returns>
    /// <example>
    /// <code>
    /// var header = renderer.FormatGroupHeader("Goblin Warband", 60);
    /// // Returns: "                    GOBLIN WARBAND                    "
    /// </code>
    /// </example>
    public string FormatGroupHeader(string groupName, int totalWidth)
    {
        ArgumentNullException.ThrowIfNull(groupName);

        var decoratedName = groupName.ToUpperInvariant();
        var padding = (totalWidth - decoratedName.Length) / 2;

        if (padding < 0) padding = 0;

        var result = decoratedName.PadLeft(padding + decoratedName.Length).PadRight(totalWidth);

        _logger?.LogDebug(
            "Formatted group header: '{Name}' -> '{Result}'",
            groupName, result.Trim());

        return result;
    }

    #endregion

    #region Member Card Formatting

    /// <summary>
    /// Formats a member card with name, role, and health.
    /// </summary>
    /// <param name="member">The member display data.</param>
    /// <param name="cardWidth">The width of the card.</param>
    /// <returns>A multi-line string representing the member card.</returns>
    /// <remarks>
    /// <para>Card structure:</para>
    /// <list type="bullet">
    ///   <item><description>Top border</description></item>
    ///   <item><description>Line 1: Leader badge or role</description></item>
    ///   <item><description>Line 2-3: Monster name (split if needed)</description></item>
    ///   <item><description>Line 4: Health text</description></item>
    ///   <item><description>Bottom border</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var card = renderer.FormatMemberCard(leaderMember, 15);
    /// // Returns multi-line card with leader badge
    /// </code>
    /// </example>
    public string FormatMemberCard(GroupMemberDisplayDto member, int cardWidth)
    {
        ArgumentNullException.ThrowIfNull(member);

        var innerWidth = cardWidth - 2; // Account for borders
        var symbols = _config.Symbols;

        var topBorder = $"{symbols.CardTopLeft}{new string(symbols.CardHorizontal, innerWidth)}{symbols.CardTopRight}";
        var bottomBorder = $"{symbols.CardBottomLeft}{new string(symbols.CardHorizontal, innerWidth)}{symbols.CardBottomRight}";

        // Line 1: Leader badge or role
        var line1Content = member.IsLeader
            ? $"{_config.Symbols.LeaderBadge} LEADER"
            : $"   {member.Role}";
        var line1 = FormatCardLine(line1Content, innerWidth);

        // Line 2-3: Monster name (split if needed)
        var nameParts = SplitName(member.MemberName, innerWidth - 4);
        var line2 = FormatCardLine($"   {nameParts.Part1}", innerWidth);
        var line3 = FormatCardLine($"   {nameParts.Part2}", innerWidth);

        // Line 4: Health
        var healthText = $"HP: {member.CurrentHealth}/{member.MaxHealth}";
        var line4 = FormatCardLine($" {healthText}", innerWidth);

        var result = string.Join("\n",
            topBorder,
            line1,
            line2,
            line3,
            line4,
            bottomBorder);

        _logger?.LogDebug(
            "Formatted member card for '{Name}' (Leader={IsLeader})",
            member.MemberName, member.IsLeader);

        return result;
    }

    /// <summary>
    /// Formats a compact health bar for a member card.
    /// </summary>
    /// <param name="current">Current health value.</param>
    /// <param name="max">Maximum health value.</param>
    /// <param name="barWidth">Width available for the bar (unused, kept for compatibility).</param>
    /// <returns>Formatted health text.</returns>
    public string FormatMemberHealthBar(int current, int max, int barWidth)
    {
        return $"HP: {current}/{max}";
    }

    /// <summary>
    /// Determines the health color based on health percentage.
    /// </summary>
    /// <param name="healthPercent">The current health percentage (0-100).</param>
    /// <returns>The console color for the health display.</returns>
    /// <remarks>
    /// <para>Color thresholds (default):</para>
    /// <list type="bullet">
    ///   <item><description>&gt;75%: Green</description></item>
    ///   <item><description>&gt;50%: Yellow</description></item>
    ///   <item><description>&gt;25%: DarkYellow (Orange)</description></item>
    ///   <item><description>&gt;0%: Red</description></item>
    ///   <item><description>0%: DarkRed</description></item>
    /// </list>
    /// </remarks>
    public ConsoleColor GetMemberHealthColor(int healthPercent)
    {
        // Thresholds are ordered descending by percent
        foreach (var threshold in _config.HealthColorThresholds.OrderByDescending(t => t.Percent))
        {
            if (healthPercent > threshold.Percent)
            {
                return threshold.Color;
            }
        }

        return _config.Colors.CriticalHealthColor;
    }

    #endregion

    #region Leader Badge

    /// <summary>
    /// Gets the leader badge symbol.
    /// </summary>
    /// <returns>The leader badge string (default "[*]").</returns>
    public string GetLeaderBadgeSymbol()
    {
        return _config.Symbols.LeaderBadge;
    }

    #endregion

    #region Tactic Formatting

    /// <summary>
    /// Formats a tactic description with role assignments.
    /// </summary>
    /// <param name="tacticDto">The tactic display data.</param>
    /// <returns>A formatted tactic description with tree-style role assignments.</returns>
    /// <example>
    /// <code>
    /// var description = renderer.FormatTacticDescription(tacticDto);
    /// // Returns:
    /// // "TACTIC: Flanking Assault"
    /// // "|-- Archers: Attack from range"
    /// // "+-- Chief: Engage in melee"
    /// </code>
    /// </example>
    public string FormatTacticDescription(TacticDisplayDto tacticDto)
    {
        ArgumentNullException.ThrowIfNull(tacticDto);

        var lines = new List<string>
        {
            $"TACTIC: {tacticDto.TacticName}"
        };

        for (var i = 0; i < tacticDto.RoleAssignments.Count; i++)
        {
            var assignment = tacticDto.RoleAssignments[i];
            var isLast = i == tacticDto.RoleAssignments.Count - 1;
            lines.Add(FormatRoleAssignment(assignment.RoleName, assignment.ActionDescription, isLast));
        }

        var result = string.Join("\n", lines);

        _logger?.LogDebug(
            "Formatted tactic description: '{TacticName}' with {RoleCount} roles",
            tacticDto.TacticName, tacticDto.RoleAssignments.Count);

        return result;
    }

    /// <summary>
    /// Formats a role assignment line.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <param name="action">The action description.</param>
    /// <param name="isLast">Whether this is the last role in the list.</param>
    /// <returns>Formatted role assignment line with tree prefix.</returns>
    public string FormatRoleAssignment(string roleName, string action, bool isLast)
    {
        var prefix = isLast ? _config.Symbols.TreeEnd : _config.Symbols.TreeBranch;
        return $"{prefix}{roleName}: {action}";
    }

    #endregion

    #region Synergy Formatting

    /// <summary>
    /// Formats synergy text for display.
    /// </summary>
    /// <param name="synergyDto">The synergy display data.</param>
    /// <returns>Formatted synergy text based on trigger state.</returns>
    /// <remarks>
    /// <para>Format varies by state:</para>
    /// <list type="bullet">
    ///   <item><description>Just triggered: "SYNERGY: {Name} triggered! ({Effect})"</description></item>
    ///   <item><description>Active: "Active: {Name} ({Effect})"</description></item>
    /// </list>
    /// </remarks>
    public string FormatSynergyText(SynergyDisplayDto synergyDto)
    {
        ArgumentNullException.ThrowIfNull(synergyDto);

        string result;
        if (synergyDto.IsTriggered)
        {
            result = $"SYNERGY: {synergyDto.SynergyName} triggered! ({synergyDto.EffectDescription})";
        }
        else
        {
            result = $"Active: {synergyDto.SynergyName} ({synergyDto.EffectDescription})";
        }

        _logger?.LogDebug(
            "Formatted synergy text: '{SynergyName}' (Triggered={IsTriggered})",
            synergyDto.SynergyName, synergyDto.IsTriggered);

        return result;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Formats a single line of card content with vertical borders.
    /// </summary>
    private string FormatCardLine(string content, int width)
    {
        if (content.Length > width)
        {
            content = content[..width];
        }

        return $"{_config.Symbols.CardVertical}{content.PadRight(width)}{_config.Symbols.CardVertical}";
    }

    /// <summary>
    /// Splits a name into two parts for card display.
    /// </summary>
    private static (string Part1, string Part2) SplitName(string name, int maxLength)
    {
        if (name.Length <= maxLength)
        {
            return (name, string.Empty);
        }

        // Try to split at a space
        var splitIndex = name.LastIndexOf(' ', Math.Min(maxLength, name.Length - 1));
        if (splitIndex > 0)
        {
            return (name[..splitIndex], name[(splitIndex + 1)..]);
        }

        // Force split at max length
        return (name[..maxLength], name[maxLength..]);
    }

    #endregion

    /// <summary>
    /// Gets the configuration for display settings.
    /// </summary>
    /// <returns>The current display configuration.</returns>
    public MonsterGroupDisplayConfig GetConfig() => _config;
}
