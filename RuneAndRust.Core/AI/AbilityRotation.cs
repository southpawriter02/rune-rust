using System.Collections.Generic;

namespace RuneAndRust.Core.AI;

/// <summary>
/// Defines an ability rotation for a boss phase.
/// Bosses use abilities in intelligent, predictable sequences.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public class AbilityRotation
{
    /// <summary>
    /// Boss type ID.
    /// </summary>
    public int BossTypeId { get; set; }

    /// <summary>
    /// Phase this rotation applies to.
    /// </summary>
    public BossPhase Phase { get; set; }

    /// <summary>
    /// Ordered list of rotation steps.
    /// </summary>
    public List<RotationStep> Steps { get; set; } = new();

    /// <summary>
    /// Human-readable notes about this rotation.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// A single step in an ability rotation.
/// </summary>
public class RotationStep
{
    /// <summary>
    /// Database ID.
    /// </summary>
    public int RotationId { get; set; }

    /// <summary>
    /// Step number in the rotation (1-indexed).
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Primary ability ID to use.
    /// </summary>
    public int AbilityId { get; set; }

    /// <summary>
    /// Fallback ability ID if primary is not available.
    /// </summary>
    public int? FallbackAbilityId { get; set; }

    /// <summary>
    /// Designer notes for this step.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Created timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
