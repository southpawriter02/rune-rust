using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of executing a single dialogue effect.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public record DialogueEffectResult
{
    /// <summary>
    /// Whether the effect executed successfully.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The type of effect that was executed.
    /// </summary>
    public required DialogueEffectType EffectType { get; init; }

    /// <summary>
    /// A description of what the effect did.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Error message if the effect failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful effect result.
    /// </summary>
    public static DialogueEffectResult Successful(DialogueEffectType type, string description) => new()
    {
        Success = true,
        EffectType = type,
        Description = description
    };

    /// <summary>
    /// Creates a failed effect result.
    /// </summary>
    public static DialogueEffectResult Failed(DialogueEffectType type, string error) => new()
    {
        Success = false,
        EffectType = type,
        ErrorMessage = error
    };
}
