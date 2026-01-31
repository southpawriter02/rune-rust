// ═══════════════════════════════════════════════════════════════════════════════
// RestType.cs
// Defines the types of rest that allow stress recovery. Each rest type has an
// associated recovery formula: Short (WILL × 2), Long (WILL × 5), Sanctuary
// (full reset to 0), and Milestone (fixed 25). Availability and limitations of
// each type are controlled by game rules.
// Version: 0.18.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of rest that allow stress recovery.
/// </summary>
/// <remarks>
/// <para>
/// Each rest type has an associated recovery formula:
/// <list type="bullet">
///   <item><description><see cref="Short"/>: WILL × 2 stress recovered</description></item>
///   <item><description><see cref="Long"/>: WILL × 5 stress recovered</description></item>
///   <item><description><see cref="Sanctuary"/>: Full stress reset to 0</description></item>
///   <item><description><see cref="Milestone"/>: Fixed 25 stress recovered</description></item>
/// </list>
/// </para>
/// <para>
/// The availability and limitations of each rest type are controlled
/// by game rules (e.g., Short Rest once per day, Sanctuary requires
/// specific safe locations).
/// </para>
/// <para>
/// Enum values are explicitly assigned (0-3) for stable serialization
/// and configuration references. These integer values must not be changed
/// once persisted.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.Extensions.RestTypeExtensions"/>
public enum RestType
{
    // ═══════════════════════════════════════════════════════════════════════════
    // REST TYPES (ordered 0-3 for stable serialization)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// A brief rest (10-30 minutes in-game).
    /// </summary>
    /// <remarks>
    /// Recovery formula: WILL × 2 stress reduced.
    /// Typically limited to once per day in expeditions.
    /// </remarks>
    Short = 0,

    /// <summary>
    /// An extended rest (6-8 hours in-game).
    /// </summary>
    /// <remarks>
    /// Recovery formula: WILL × 5 stress reduced.
    /// Provides significant recovery but not complete restoration.
    /// Requires a safe location.
    /// </remarks>
    Long = 1,

    /// <summary>
    /// Rest in a designated Sanctuary location.
    /// </summary>
    /// <remarks>
    /// Recovery formula: Full stress reset to 0.
    /// Sanctuary locations are rare safe havens in Aethelgard
    /// where characters can fully decompress.
    /// </remarks>
    Sanctuary = 2,

    /// <summary>
    /// Rest granted upon completing a major milestone.
    /// </summary>
    /// <remarks>
    /// Recovery formula: Fixed 25 stress reduced.
    /// Awarded for completing quest objectives, defeating bosses,
    /// or other significant achievements.
    /// </remarks>
    Milestone = 3
}
