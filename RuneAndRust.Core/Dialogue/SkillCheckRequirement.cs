namespace RuneAndRust.Core.Dialogue;

/// <summary>
/// Represents a skill or attribute check requirement for dialogue options (v0.8)
/// </summary>
public class SkillCheckRequirement
{
    public string Attribute { get; set; } = string.Empty; // WITS, WILL, MIGHT, FINESSE, STURDINESS
    public int TargetValue { get; set; } = 0;
    public Specialization? Skill { get; set; } = null; // Optional skill requirement
    public int SkillRanks { get; set; } = 0; // Required specialization level
}
