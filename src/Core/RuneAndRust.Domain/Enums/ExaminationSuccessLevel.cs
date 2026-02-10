// ═══════════════════════════════════════════════════════════════════════════════
// ExaminationSuccessLevel.cs
// Defines success tiers for examination attempts, mapping to the number of
// information layers revealed by the Perception Layer system.
// Version: 0.20.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Success levels for examination attempts, determining the depth of
/// information revealed through the Perception Layer system.
/// </summary>
/// <remarks>
/// <para>
/// Each level maps to a number of information layers revealed:
/// </para>
/// <list type="bullet">
///   <item><description><b>Failure (0):</b> No information gained</description></item>
///   <item><description><b>Partial (1):</b> Layer 1 only — basic observation</description></item>
///   <item><description><b>Success (2):</b> Layers 1–2 — functional understanding</description></item>
///   <item><description><b>Expert (3):</b> Layers 1–3 — detailed analysis</description></item>
///   <item><description><b>Master (4):</b> All layers — complete comprehension + bonus lore</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ValueObjects.ExaminationResult"/>
public enum ExaminationSuccessLevel
{
    /// <summary>
    /// Examination failed. No information gained.
    /// </summary>
    Failure = 0,

    /// <summary>
    /// Partial success. Layer 1 only — basic physical observation.
    /// </summary>
    Partial = 1,

    /// <summary>
    /// Standard success. Layers 1–2 — functional understanding of the object.
    /// </summary>
    Success = 2,

    /// <summary>
    /// Expert success. Layers 1–3 — detailed technical analysis.
    /// </summary>
    Expert = 3,

    /// <summary>
    /// Master success. All layers revealed plus bonus lore and +2 Lore Insight.
    /// </summary>
    Master = 4
}
