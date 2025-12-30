using System.Text.Json.Serialization;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Conditions;

/// <summary>
/// Base class for all dialogue condition types.
/// Conditions determine whether a dialogue option is available.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(AttributeCondition), "attribute")]
[JsonDerivedType(typeof(LevelCondition), "level")]
[JsonDerivedType(typeof(ReputationCondition), "reputation")]
[JsonDerivedType(typeof(FlagCondition), "flag")]
[JsonDerivedType(typeof(ItemCondition), "item")]
[JsonDerivedType(typeof(SpecializationCondition), "specialization")]
[JsonDerivedType(typeof(NodeCondition), "node")]
[JsonDerivedType(typeof(SkillCheckCondition), "skillcheck")]
public abstract class DialogueCondition
{
    /// <summary>
    /// The type of this condition for polymorphic handling.
    /// </summary>
    public abstract DialogueConditionType Type { get; }

    /// <summary>
    /// If true, option is hidden when condition fails.
    /// If false, option shows as locked with requirement hint.
    /// </summary>
    public bool HideWhenFailed { get; set; } = false;

    /// <summary>
    /// Gets the display string shown when condition fails (e.g., "[WITS 6]").
    /// </summary>
    public abstract string GetDisplayHint();
}
