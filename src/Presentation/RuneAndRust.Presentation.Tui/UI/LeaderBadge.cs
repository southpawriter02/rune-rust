using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Renders the leader badge and handles leader defeat notifications.
/// </summary>
/// <remarks>
/// <para>The leader badge:</para>
/// <list type="bullet">
///   <item><description>Appears on the leader's member card with a [*] symbol</description></item>
///   <item><description>Displays leader-provided bonuses to the group</description></item>
///   <item><description>Shows a notification box when the leader is defeated</description></item>
/// </list>
/// <para>When the leader is defeated, a prominent effect box appears showing
/// the morale impact on remaining group members.</para>
/// </remarks>
/// <example>
/// <code>
/// var badge = new LeaderBadge(renderer, terminal, config, logger);
/// 
/// // Render badge for living leader
/// var status = new LeaderStatusDto("Goblin Chief", "leader", true, false, Array.Empty&lt;string&gt;());
/// badge.RenderBadge(status);
/// 
/// // Show leader defeated effect
/// var defeatedStatus = new LeaderStatusDto("Goblin Chief", "leader", false, true, moraleEffects);
/// badge.ShowLeaderDefeated(defeatedStatus);
/// </code>
/// </example>
public class LeaderBadge
{
    private readonly MonsterGroupRenderer _renderer;
    private readonly ITerminalService _terminalService;
    private readonly MonsterGroupDisplayConfig _config;
    private readonly ILogger<LeaderBadge>? _logger;

    private LeaderStatusDto? _currentStatus;

    /// <summary>
    /// Creates a new instance of the LeaderBadge.
    /// </summary>
    /// <param name="renderer">The renderer for formatting badge elements.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="renderer"/> or <paramref name="terminalService"/> is null.
    /// </exception>
    public LeaderBadge(
        MonsterGroupRenderer renderer,
        ITerminalService terminalService,
        MonsterGroupDisplayConfig? config = null,
        ILogger<LeaderBadge>? logger = null)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? MonsterGroupDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug("LeaderBadge initialized");
    }

    #region Public Methods

    /// <summary>
    /// Renders the leader badge on a member card.
    /// </summary>
    /// <param name="leaderStatus">The leader status data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="leaderStatus"/> is null.</exception>
    /// <remarks>
    /// The badge is rendered as part of the member card.
    /// If the leader is not alive, the badge is hidden.
    /// </remarks>
    public void RenderBadge(LeaderStatusDto leaderStatus)
    {
        _currentStatus = leaderStatus ?? throw new ArgumentNullException(nameof(leaderStatus));

        if (!leaderStatus.IsAlive)
        {
            _logger?.LogDebug("Leader {LeaderName} is not alive, badge hidden", leaderStatus.LeaderName);
            return;
        }

        var badgeSymbol = _renderer.GetLeaderBadgeSymbol();

        _logger?.LogDebug(
            "Rendering leader badge for {LeaderName} ({BadgeSymbol})",
            leaderStatus.LeaderName,
            badgeSymbol);
    }

    /// <summary>
    /// Shows the leader bonus information.
    /// </summary>
    /// <param name="bonuses">The list of bonus descriptions.</param>
    /// <remarks>
    /// <para>Bonuses are displayed in the morale effect area:</para>
    /// <example>
    /// Leader provides: +1 attack to all allies
    ///                  +2 damage on flanking hits
    /// </example>
    /// </remarks>
    public void ShowLeaderBonus(IEnumerable<string>? bonuses)
    {
        if (bonuses == null || !bonuses.Any())
        {
            _logger?.LogDebug("No leader bonuses to display");
            return;
        }

        var bonusList = bonuses.ToList();
        _logger?.LogDebug("Displaying {Count} leader bonuses", bonusList.Count);

        // Bonuses are displayed in the synergy/morale area
        var bonusY = _config.StartY + _config.MoraleEffectRow;

        for (var i = 0; i < bonusList.Count; i++)
        {
            var prefix = i == 0 ? "Leader provides: " : "                 ";
            var bonusText = $"{prefix}{bonusList[i]}";

            _terminalService.WriteColoredAt(
                _config.StartX + _config.TextIndent,
                bonusY + i,
                bonusText,
                _config.Colors.LeaderColor);
        }
    }

    /// <summary>
    /// Shows the leader defeated effect box.
    /// </summary>
    /// <param name="leaderStatus">The leader status data with morale effects.</param>
    /// <remarks>
    /// <para>Displays a prominent notification box showing:</para>
    /// <list type="bullet">
    ///   <item><description>Leader name and defeat message</description></item>
    ///   <item><description>Morale broken notification</description></item>
    ///   <item><description>Negative effects applied to remaining members</description></item>
    /// </list>
    /// </remarks>
    public void ShowLeaderDefeated(LeaderStatusDto leaderStatus)
    {
        ArgumentNullException.ThrowIfNull(leaderStatus);

        if (!leaderStatus.IsDefeated)
        {
            _logger?.LogWarning("ShowLeaderDefeated called but leader is not defeated");
            return;
        }

        _currentStatus = leaderStatus;

        _logger?.LogDebug(
            "Showing leader defeated effect for {LeaderName}",
            leaderStatus.LeaderName);

        // Render the effect box
        RenderLeaderDefeatedBox(leaderStatus);

        _logger?.LogInformation(
            "Leader defeated effect displayed for {LeaderName} with {EffectCount} morale effects",
            leaderStatus.LeaderName, leaderStatus.MoraleEffects.Count);
    }

    /// <summary>
    /// Clears the leader badge display.
    /// </summary>
    /// <remarks>
    /// Resets internal state and clears any effect box if displayed.
    /// </remarks>
    public void Clear()
    {
        _currentStatus = null;

        // Clear any effect box if displayed
        ClearEffectBox();

        _logger?.LogDebug("Cleared leader badge display");
    }

    /// <summary>
    /// Gets the current leader name, or null if none tracked.
    /// </summary>
    public string? CurrentLeaderName => _currentStatus?.LeaderName;

    /// <summary>
    /// Gets whether the current leader is alive.
    /// </summary>
    public bool IsLeaderAlive => _currentStatus?.IsAlive ?? false;

    #endregion

    #region Private Methods

    /// <summary>
    /// Renders the leader defeated effect box.
    /// </summary>
    private void RenderLeaderDefeatedBox(LeaderStatusDto leaderStatus)
    {
        var boxWidth = _config.EffectBox.Width;
        var boxX = _config.StartX + (_config.TotalWidth - boxWidth) / 2;
        var boxY = _config.StartY + _config.EffectBox.StartRow;

        // Box header
        var topBorder = "+" + new string('=', boxWidth - 2) + "+";
        var titleLine = FormatCenteredLine("[!]  LEADER DEFEATED", boxWidth);
        var divider = "+" + new string('=', boxWidth - 2) + "+";

        _terminalService.WriteColoredAt(boxX, boxY, topBorder, _config.Colors.EffectBoxBorderColor);
        _terminalService.WriteColoredAt(boxX, boxY + 1, titleLine, _config.Colors.EffectBoxTitleColor);
        _terminalService.WriteColoredAt(boxX, boxY + 2, divider, _config.Colors.EffectBoxBorderColor);

        // Content
        var defeatedText = $"{leaderStatus.LeaderName} has fallen!";
        _terminalService.WriteColoredAt(boxX, boxY + 3, FormatCenteredLine("", boxWidth), _config.Colors.DefaultColor);
        _terminalService.WriteColoredAt(boxX, boxY + 4, FormatCenteredLine(defeatedText, boxWidth), _config.Colors.EffectBoxContentColor);
        _terminalService.WriteColoredAt(boxX, boxY + 5, FormatCenteredLine("", boxWidth), _config.Colors.DefaultColor);
        _terminalService.WriteColoredAt(boxX, boxY + 6, FormatCenteredLine("Group morale broken:", boxWidth), _config.Colors.EffectBoxContentColor);

        // Morale effects
        var effectY = boxY + 7;
        foreach (var effect in leaderStatus.MoraleEffects)
        {
            var effectLine = FormatCenteredLine($"|-- {effect}", boxWidth);
            _terminalService.WriteColoredAt(boxX, effectY++, effectLine, _config.Colors.NegativeEffectColor);
        }

        // Bottom border
        var bottomY = effectY;
        _terminalService.WriteColoredAt(boxX, bottomY, "+" + new string('=', boxWidth - 2) + "+", _config.Colors.EffectBoxBorderColor);
    }

    /// <summary>
    /// Clears the effect box area.
    /// </summary>
    private void ClearEffectBox()
    {
        var boxWidth = _config.EffectBox.Width;
        var boxX = _config.StartX + (_config.TotalWidth - boxWidth) / 2;
        var boxY = _config.StartY + _config.EffectBox.StartRow;
        var boxHeight = _config.EffectBox.Height;

        var clearLine = new string(' ', boxWidth);

        for (var i = 0; i < boxHeight; i++)
        {
            _terminalService.WriteAt(boxX, boxY + i, clearLine);
        }
    }

    /// <summary>
    /// Formats text centered within a line with box borders.
    /// </summary>
    private static string FormatCenteredLine(string text, int width)
    {
        var innerWidth = width - 4; // Account for "| " and " |"
        if (text.Length >= innerWidth)
        {
            return $"| {text[..innerWidth]} |";
        }

        var padding = (innerWidth - text.Length) / 2;
        var paddedText = text.PadLeft(padding + text.Length).PadRight(innerWidth);
        return $"| {paddedText} |";
    }

    #endregion
}
