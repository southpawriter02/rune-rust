// ═══════════════════════════════════════════════════════════════════════════════
// TooltipDtos.cs
// Data transfer objects for ability tree node tooltips.
// Version: 0.13.2d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

// ═══════════════════════════════════════════════════════════════════════════════
// TOOLTIP DISPLAY DTO
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Data transfer object for tooltip display content.
/// </summary>
/// <remarks>
/// <para>Contains all content needed to render a complete tooltip:</para>
/// <list type="bullet">
///   <item><description>Title: Ability name displayed prominently</description></item>
///   <item><description>Subtitle: Tier and cost information</description></item>
///   <item><description>Description: Ability description text</description></item>
///   <item><description>Sections: Details and requirements</description></item>
///   <item><description>Footer: Status and action prompt</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var tooltip = new TooltipDisplayDto(
///     Title: "POWER STRIKE",
///     Subtitle: "Tier 1 | Cost: 2 points",
///     Description: "A powerful strike that deals 150% weapon damage.",
///     Sections: new List&lt;TooltipSectionDto&gt; { detailsSection },
///     Footer: "Status: AVAILABLE - Press [U] to unlock");
/// </code>
/// </example>
/// <param name="Title">The tooltip title (ability name in uppercase).</param>
/// <param name="Subtitle">The subtitle containing tier and cost.</param>
/// <param name="Description">The ability description text.</param>
/// <param name="Sections">Content sections for details and requirements.</param>
/// <param name="Footer">The status and action footer line.</param>
public record TooltipDisplayDto(
    string Title,
    string? Subtitle,
    string? Description,
    IReadOnlyList<TooltipSectionDto> Sections,
    string? Footer);

// ═══════════════════════════════════════════════════════════════════════════════
// TOOLTIP SECTION DTO
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Data transfer object for a tooltip content section.
/// </summary>
/// <remarks>
/// <para>Sections organize tooltip content into logical groups:</para>
/// <list type="bullet">
///   <item><description>Details section: Cooldown, resource cost</description></item>
///   <item><description>Prerequisites section: Required nodes and progress</description></item>
/// </list>
/// <para>Each section can have an optional header and multiple lines.</para>
/// </remarks>
/// <example>
/// <code>
/// var prereqSection = new TooltipSectionDto(
///     Header: "Prerequisites:",
///     Lines: new List&lt;TooltipLineDto&gt;
///     {
///         new("[x] Cleave", "(Tier 2)", true),
///         new("[ ] Shield Bash", "(Tier 2)", false)
///     });
/// </code>
/// </example>
/// <param name="Header">Optional section header (e.g., "Prerequisites:").</param>
/// <param name="Lines">The lines in this section.</param>
public record TooltipSectionDto(
    string? Header,
    IReadOnlyList<TooltipLineDto> Lines);

// ═══════════════════════════════════════════════════════════════════════════════
// TOOLTIP LINE DTO
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Data transfer object for a single tooltip line.
/// </summary>
/// <remarks>
/// <para>Each line consists of a label and value pair.</para>
/// <para>For requirement lines, <see cref="IsSatisfied"/> controls color coding:</para>
/// <list type="bullet">
///   <item><description>True: Green or default color</description></item>
///   <item><description>False: Gray color</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var cooldownLine = new TooltipLineDto("Cooldown", "3 turns");
/// var prereqLine = new TooltipLineDto("[x] Cleave", "(Tier 2)", true);
/// </code>
/// </example>
/// <param name="Label">The line label (left side).</param>
/// <param name="Value">The line value (right side).</param>
/// <param name="IsSatisfied">Whether this requirement is satisfied (default true).</param>
public record TooltipLineDto(
    string Label,
    string Value,
    bool IsSatisfied = true);

// ═══════════════════════════════════════════════════════════════════════════════
// TOOLTIP POSITION
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Represents the screen position of a tooltip.
/// </summary>
/// <remarks>
/// <para>Position is calculated relative to the selected node:</para>
/// <list type="bullet">
///   <item><description>Default: Right of the node</description></item>
///   <item><description>Adjusted if tooltip would extend past screen edge</description></item>
/// </list>
/// </remarks>
/// <param name="X">The X coordinate (column) on screen.</param>
/// <param name="Y">The Y coordinate (row) on screen.</param>
public record TooltipPosition(int X, int Y);

// ═══════════════════════════════════════════════════════════════════════════════
// TALENT POINT DISPLAY DTO
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Data transfer object for talent point display.
/// </summary>
/// <remarks>
/// <para>Displays the player's talent point allocation:</para>
/// <list type="bullet">
///   <item><description>Available: Unspent points that can be allocated</description></item>
///   <item><description>Spent: Points already invested in abilities</description></item>
///   <item><description>Total: Sum of available and spent</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var pointDisplay = new TalentPointDisplayDto(Available: 3, Spent: 12, Total: 15);
/// // Output: "Talent Points: 3 Available | 12 Spent"
/// </code>
/// </example>
/// <param name="Available">The number of available (unspent) points.</param>
/// <param name="Spent">The number of spent points.</param>
/// <param name="Total">The total points earned (Available + Spent).</param>
public record TalentPointDisplayDto(
    int Available,
    int Spent,
    int Total);
