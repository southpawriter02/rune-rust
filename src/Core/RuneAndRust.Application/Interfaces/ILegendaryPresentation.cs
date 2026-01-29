namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Interface for formatting legendary item presentations.
/// Provides methods for creating exciting drop announcements and detailed examination text.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the presentation logic for Myth-Forged (legendary) items,
/// enabling consistent formatting across TUI and GUI implementations while allowing
/// for future customization and theming.
/// </para>
/// <para>
/// Implementations should produce visually distinctive output that emphasizes the
/// special nature of legendary items. This includes decorative borders, atmospheric
/// text, and structured stat/effect displays.
/// </para>
/// <para>
/// The interface is designed for dependency injection, allowing presentation logic
/// to be mocked in unit tests and swapped for different implementations (e.g.,
/// plain text for TUI vs. rich markup for GUI).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Format a drop announcement
/// var announcement = presentation.FormatDropAnnouncement(dropEvent);
/// renderer.Display(announcement);
/// 
/// // Format examination text for detailed item view
/// var examination = presentation.FormatExaminationText(uniqueItem);
/// Console.WriteLine(examination);
/// </code>
/// </example>
/// <seealso cref="MythForgedDropEvent"/>
/// <seealso cref="UniqueItem"/>
public interface ILegendaryPresentation
{
    /// <summary>
    /// Formats a full drop announcement for display.
    /// </summary>
    /// <param name="dropEvent">The drop event context containing item and source information.</param>
    /// <returns>
    /// A formatted multi-line announcement string with borders, header,
    /// atmospheric text, item name, and stat summary.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The announcement includes:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Top border decoration</description></item>
    ///   <item><description>"✦ LEGENDARY DROP ✦" header</description></item>
    ///   <item><description>Atmospheric text based on drop source</description></item>
    ///   <item><description>Item header with tier prefix and name</description></item>
    ///   <item><description>Stat line summary</description></item>
    ///   <item><description>First drop message (if applicable)</description></item>
    ///   <item><description>Bottom border decoration</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var announcement = presentation.FormatDropAnnouncement(dropEvent);
    /// // ═══════════════════════════════════════════════
    /// // ✦ LEGENDARY DROP ✦
    /// // The air hums with ancient power...
    /// //
    /// // [Myth-Forged] Shadowfang Blade
    /// // +5 MIGHT | +2 AGI | +15 Damage
    /// // ═══════════════════════════════════════════════
    /// </code>
    /// </example>
    string FormatDropAnnouncement(MythForgedDropEvent dropEvent);

    /// <summary>
    /// Formats detailed examination text for a unique item.
    /// </summary>
    /// <param name="item">The unique item to examine.</param>
    /// <returns>
    /// A formatted multi-line examination string with complete item details,
    /// including lore, stats, effects, and requirements.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The examination text includes:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Item header with tier prefix</description></item>
    ///   <item><description>Flavor text (if available)</description></item>
    ///   <item><description>Description</description></item>
    ///   <item><description>Detailed stats section</description></item>
    ///   <item><description>Special effects section (if any)</description></item>
    ///   <item><description>Requirements (level, class affinities)</description></item>
    /// </list>
    /// </remarks>
    string FormatExaminationText(UniqueItem item);

    /// <summary>
    /// Formats the stat bonuses as a display line.
    /// </summary>
    /// <param name="stats">The item stats to format.</param>
    /// <returns>
    /// A formatted stat line with pipe separators (e.g., "+5 MIGHT | +2 AGI | +15 Damage"),
    /// or "No stat bonuses" if all stats are zero.
    /// </returns>
    /// <remarks>
    /// Only non-zero stats are included in the output. Stats are formatted with
    /// abbreviated names for compact display in drop announcements.
    /// </remarks>
    string FormatStatLine(ItemStats stats);

    /// <summary>
    /// Formats a list of special effects for display.
    /// </summary>
    /// <param name="effects">The special effects to format.</param>
    /// <returns>
    /// A formatted multi-line effect list with bullet points,
    /// or "No special effects" if the list is empty.
    /// </returns>
    /// <remarks>
    /// Each effect is formatted on its own line with a special character
    /// bullet (✦) followed by the effect name and description.
    /// </remarks>
    string FormatEffectList(IReadOnlyList<SpecialEffect> effects);

    /// <summary>
    /// Gets the announcement frame borders.
    /// </summary>
    /// <param name="width">
    /// The desired width of the frame in characters. Default is 47.
    /// </param>
    /// <returns>
    /// A tuple containing the top and bottom border strings,
    /// both of the specified width.
    /// </returns>
    /// <remarks>
    /// Border characters use the double-line box drawing character (═)
    /// for a premium, distinctive appearance.
    /// </remarks>
    (string Top, string Bottom) GetAnnouncementFrame(int width = 47);

    /// <summary>
    /// Formats the item header with tier prefix and name.
    /// </summary>
    /// <param name="item">The unique item to format.</param>
    /// <returns>
    /// A formatted header string (e.g., "[Myth-Forged] Shadowfang Blade").
    /// </returns>
    /// <remarks>
    /// The tier prefix is obtained from <see cref="Domain.Constants.TierColors.GetTierPrefix"/>
    /// based on the item's quality tier.
    /// </remarks>
    string FormatItemHeader(UniqueItem item);

    /// <summary>
    /// Formats class affinity requirements for display.
    /// </summary>
    /// <param name="classAffinities">List of class IDs that have affinity for the item.</param>
    /// <returns>
    /// A formatted class list with proper capitalization,
    /// or "All Classes" if the list is empty.
    /// </returns>
    /// <remarks>
    /// Class IDs are converted from lowercase kebab-case to title case
    /// for player-facing display (e.g., "warrior" → "Warrior").
    /// </remarks>
    string FormatClassAffinities(IReadOnlyList<string> classAffinities);
}
