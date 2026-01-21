using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Renders the monster group display showing group composition,
/// member health bars, tactics, and leader designation.
/// </summary>
/// <remarks>
/// <para>The monster group view is displayed during combat when fighting
/// coordinated enemy groups. It shows:</para>
/// <list type="bullet">
///   <item><description>Group name header</description></item>
///   <item><description>Member cards with health bars</description></item>
///   <item><description>Current tactic indicator</description></item>
///   <item><description>Leader badge and morale hints</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var groupView = new MonsterGroupView(renderer, terminal, tacticIndicator, leaderBadge, config, logger);
/// 
/// // Render complete group
/// var dto = new MonsterGroupDisplayDto(...);
/// groupView.RenderGroup(dto);
/// 
/// // Update individual member health
/// groupView.ShowMemberHealth(memberDto);
/// 
/// // Clear display
/// groupView.Clear();
/// </code>
/// </example>
public class MonsterGroupView
{
    private readonly MonsterGroupRenderer _renderer;
    private readonly ITerminalService _terminalService;
    private readonly TacticIndicator _tacticIndicator;
    private readonly LeaderBadge _leaderBadge;
    private readonly MonsterGroupDisplayConfig _config;
    private readonly ILogger<MonsterGroupView>? _logger;

    private MonsterGroupDisplayDto? _currentGroup;
    private IReadOnlyList<GroupMemberDisplayDto> _members = Array.Empty<GroupMemberDisplayDto>();

    /// <summary>
    /// Creates a new instance of the MonsterGroupView.
    /// </summary>
    /// <param name="renderer">The renderer for formatting group elements.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="tacticIndicator">The tactic indicator component.</param>
    /// <param name="leaderBadge">The leader badge component.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="renderer"/>, <paramref name="terminalService"/>,
    /// <paramref name="tacticIndicator"/>, or <paramref name="leaderBadge"/> is null.
    /// </exception>
    public MonsterGroupView(
        MonsterGroupRenderer renderer,
        ITerminalService terminalService,
        TacticIndicator tacticIndicator,
        LeaderBadge leaderBadge,
        MonsterGroupDisplayConfig? config = null,
        ILogger<MonsterGroupView>? logger = null)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _tacticIndicator = tacticIndicator ?? throw new ArgumentNullException(nameof(tacticIndicator));
        _leaderBadge = leaderBadge ?? throw new ArgumentNullException(nameof(leaderBadge));
        _config = config ?? MonsterGroupDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug(
            "MonsterGroupView initialized at position ({X}, {Y})",
            _config.StartX, _config.StartY);
    }

    #region Public Methods

    /// <summary>
    /// Renders the complete monster group display.
    /// </summary>
    /// <param name="groupDto">The monster group display data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="groupDto"/> is null.</exception>
    /// <remarks>
    /// <para>Renders the following elements in order:</para>
    /// <list type="number">
    ///   <item><description>Group header with name</description></item>
    ///   <item><description>Member cards in horizontal layout</description></item>
    ///   <item><description>Current tactic indicator</description></item>
    ///   <item><description>Morale hint (if leader exists)</description></item>
    /// </list>
    /// </remarks>
    public void RenderGroup(MonsterGroupDisplayDto groupDto)
    {
        _currentGroup = groupDto ?? throw new ArgumentNullException(nameof(groupDto));
        _members = groupDto.Members;

        _logger?.LogDebug(
            "Rendering monster group '{GroupName}' with {MemberCount} members",
            groupDto.GroupName,
            groupDto.Members.Count);

        // Render group header with name
        RenderGroupHeader(groupDto.GroupName);

        // Render member cards in horizontal layout
        RenderMemberCards(groupDto.Members);

        // Render current tactic
        if (groupDto.CurrentTactic != null)
        {
            _tacticIndicator.RenderTactic(groupDto.CurrentTactic);
        }

        // Render morale/synergy hint if leader exists
        if (groupDto.HasLeader)
        {
            RenderMoraleHint(groupDto.LeaderRole);
        }

        _logger?.LogInformation(
            "Monster group '{GroupName}' displayed with {MemberCount} members, HasLeader={HasLeader}",
            groupDto.GroupName, groupDto.Members.Count, groupDto.HasLeader);
    }

    /// <summary>
    /// Renders individual member health bar update.
    /// </summary>
    /// <param name="memberDto">The member display data.</param>
    /// <remarks>
    /// Updates only the health bar portion of the member's card.
    /// If the member is not found in the current group, logs a warning.
    /// </remarks>
    public void ShowMemberHealth(GroupMemberDisplayDto memberDto)
    {
        ArgumentNullException.ThrowIfNull(memberDto);

        var memberList = _members.ToList();
        var cardIndex = memberList.FindIndex(m => m.MonsterId == memberDto.MonsterId);
        if (cardIndex < 0)
        {
            _logger?.LogWarning("Member {MonsterId} not found in group", memberDto.MonsterId);
            return;
        }

        var cardX = CalculateMemberCardX(cardIndex);
        var healthY = _config.StartY + _config.MemberCardStartRow + 4; // Line 4 of card

        var healthText = _renderer.FormatMemberHealthBar(
            memberDto.CurrentHealth,
            memberDto.MaxHealth,
            _config.MemberCardWidth - 4);

        var healthColor = _renderer.GetMemberHealthColor(memberDto.HealthPercent);

        _terminalService.WriteColoredAt(cardX + 2, healthY, $" {healthText} ", healthColor);

        _logger?.LogDebug(
            "Updated health for {MemberName}: {Current}/{Max} ({Percent}%)",
            memberDto.MemberName,
            memberDto.CurrentHealth,
            memberDto.MaxHealth,
            memberDto.HealthPercent);
    }

    /// <summary>
    /// Highlights the leader member with the leader badge.
    /// </summary>
    /// <param name="memberDto">The leader member display data.</param>
    /// <remarks>
    /// If the member is not the leader, logs a warning and returns.
    /// </remarks>
    public void HighlightLeader(GroupMemberDisplayDto memberDto)
    {
        ArgumentNullException.ThrowIfNull(memberDto);

        if (!memberDto.IsLeader)
        {
            _logger?.LogWarning("Member {MemberName} is not the leader", memberDto.MemberName);
            return;
        }

        var leaderStatus = new LeaderStatusDto(
            LeaderName: memberDto.MemberName,
            LeaderRole: memberDto.Role,
            IsAlive: memberDto.IsAlive,
            IsDefeated: !memberDto.IsAlive,
            MoraleEffects: Array.Empty<string>());

        _leaderBadge.RenderBadge(leaderStatus);

        _logger?.LogDebug("Highlighted leader: {LeaderName}", memberDto.MemberName);
    }

    /// <summary>
    /// Displays a morale effect notification.
    /// </summary>
    /// <param name="synergyDto">The synergy/morale effect data.</param>
    /// <remarks>
    /// Synergy effects are displayed in the morale effect row area
    /// using the synergy color from configuration.
    /// </remarks>
    public void ShowMoraleEffect(SynergyDisplayDto? synergyDto)
    {
        if (synergyDto == null) return;

        var effectY = _config.StartY + _config.MoraleEffectRow;
        var effectText = _renderer.FormatSynergyText(synergyDto);

        _terminalService.WriteColoredAt(
            _config.StartX,
            effectY,
            effectText,
            _config.Colors.SynergyColor);

        _logger?.LogDebug(
            "Displayed morale effect: {SynergyName}",
            synergyDto.SynergyName);
    }

    /// <summary>
    /// Clears the monster group display from the terminal.
    /// </summary>
    /// <remarks>
    /// Resets internal state and clears the entire display area,
    /// including tactic indicator and leader badge components.
    /// </remarks>
    public void Clear()
    {
        _currentGroup = null;
        _members = Array.Empty<GroupMemberDisplayDto>();

        // Clear the display area
        var clearLine = new string(' ', _config.TotalWidth);
        for (var row = 0; row < _config.TotalHeight; row++)
        {
            _terminalService.WriteAt(_config.StartX, _config.StartY + row, clearLine);
        }

        // Clear sub-components
        _tacticIndicator.Clear();
        _leaderBadge.Clear();

        _logger?.LogDebug("Cleared monster group display");
    }

    /// <summary>
    /// Gets the current group ID, if any.
    /// </summary>
    public Guid? CurrentGroupId => _currentGroup?.GroupId;

    /// <summary>
    /// Gets the number of currently displayed members.
    /// </summary>
    public int DisplayedMemberCount => _members.Count;

    #endregion

    #region Private Rendering Methods

    /// <summary>
    /// Renders the group name header.
    /// </summary>
    private void RenderGroupHeader(string groupName)
    {
        var header = _renderer.FormatGroupHeader(groupName, _config.TotalWidth);
        var headerColor = _config.Colors.HeaderColor;

        _terminalService.WriteColoredAt(
            _config.StartX,
            _config.StartY,
            header,
            headerColor);
    }

    /// <summary>
    /// Renders all member cards in horizontal layout.
    /// </summary>
    private void RenderMemberCards(IReadOnlyList<GroupMemberDisplayDto> members)
    {
        for (var i = 0; i < members.Count; i++)
        {
            var member = members[i];
            var cardX = CalculateMemberCardX(i);

            RenderSingleMemberCard(member, cardX);
        }
    }

    /// <summary>
    /// Renders a single member card at the specified position.
    /// </summary>
    private void RenderSingleMemberCard(GroupMemberDisplayDto member, int cardX)
    {
        var cardY = _config.StartY + _config.MemberCardStartRow;
        var card = _renderer.FormatMemberCard(member, _config.MemberCardWidth);

        // Card has multiple lines - render each
        var lines = card.Split('\n');
        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];
            var lineColor = GetLineColor(member, lineIndex);

            _terminalService.WriteColoredAt(cardX, cardY + lineIndex, line, lineColor);
        }
    }

    /// <summary>
    /// Determines the color for a card line based on content.
    /// </summary>
    private ConsoleColor GetLineColor(GroupMemberDisplayDto member, int lineIndex)
    {
        // Line 1 (index 1): Leader badge or role header
        if (lineIndex == 1 && member.IsLeader)
        {
            return _config.Colors.LeaderColor;
        }

        // Line 4 (index 4): Health bar
        if (lineIndex == 4)
        {
            return _renderer.GetMemberHealthColor(member.HealthPercent);
        }

        return _config.Colors.DefaultColor;
    }

    /// <summary>
    /// Renders the morale hint below member cards.
    /// </summary>
    private void RenderMoraleHint(string leaderRole)
    {
        var hintY = _config.StartY + _config.MoraleHintRow;
        var hintText = $"Kill the {leaderRole} to break morale!";

        _terminalService.WriteColoredAt(
            _config.StartX + _config.TextIndent,
            hintY,
            hintText,
            _config.Colors.HintColor);
    }

    /// <summary>
    /// Calculates the X position for a member card.
    /// </summary>
    /// <param name="cardIndex">Zero-based index of the card.</param>
    /// <returns>X coordinate for the card.</returns>
    private int CalculateMemberCardX(int cardIndex)
    {
        return _config.StartX +
               _config.TextIndent +
               (cardIndex * (_config.MemberCardWidth + _config.CardSpacing));
    }

    #endregion
}
