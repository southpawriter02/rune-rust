namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of methods used to bring unconscious characters back to consciousness.
/// Determines mechanical effects and narrative flavor for resurrection events.
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.6c as part of the Bone-Setter's Resuscitate (Tier 3)
/// and Miracle Worker (Capstone) abilities.</para>
/// <para>Each method has different mechanical outcomes:</para>
/// <list type="bullet">
/// <item><see cref="SkillBasedResuscitation"/>: Target restored to 1 HP (barely conscious)</item>
/// <item><see cref="MiracleIntervention"/>: Target restored to full HP with conditions cleared</item>
/// <item><see cref="NaturalRecovery"/>: Future use — natural healing over time</item>
/// <item><see cref="DivineGrace"/>: Future use — magical/divine intervention</item>
/// </list>
/// </remarks>
public enum ResurrectionMethod
{
    /// <summary>
    /// Resuscitate ability — emergency revival via Bone-Setter medical skill.
    /// Brings target to 1 HP (barely conscious, extremely vulnerable).
    /// Costs 4 AP and 2 Medical Supplies. No cooldown (unlimited uses).
    /// </summary>
    SkillBasedResuscitation = 0,

    /// <summary>
    /// Miracle Worker capstone — full restoration and condition clearing.
    /// Brings target to full HP with all negative conditions removed.
    /// Costs 5 AP. Once per long rest.
    /// </summary>
    MiracleIntervention = 1,

    /// <summary>
    /// Natural recovery — target recovers via rest or natural healing.
    /// Reserved for future implementation.
    /// </summary>
    NaturalRecovery = 2,

    /// <summary>
    /// Divine or magical intervention from other specializations.
    /// Reserved for future implementation (e.g., Seiðkona, Echo-Caller).
    /// </summary>
    DivineGrace = 3
}
