using System.Text.Json.Serialization;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Effects;

/// <summary>
/// Base class for all dialogue effect types.
/// Effects are executed when a dialogue option is selected.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ModifyReputationEffect), "modifyreputation")]
[JsonDerivedType(typeof(GiveItemEffect), "giveitem")]
[JsonDerivedType(typeof(SetFlagEffect), "setflag")]
public abstract class DialogueEffect
{
    /// <summary>
    /// The type of this effect for polymorphic handling.
    /// </summary>
    public abstract DialogueEffectType Type { get; }

    /// <summary>
    /// Gets a human-readable description of this effect.
    /// </summary>
    public abstract string GetDescription();
}
