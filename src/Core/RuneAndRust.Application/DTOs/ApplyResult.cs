namespace RuneAndRust.Application.DTOs;

using RuneAndRust.Domain.Entities;

/// <summary>
/// Result of attempting to apply a status effect.
/// </summary>
public record ApplyResult
{
    /// <summary>Whether the effect was applied or modified.</summary>
    public bool WasApplied { get; init; }

    /// <summary>The type of result.</summary>
    public ApplyResultType ResultType { get; init; }

    /// <summary>Optional message describing the result.</summary>
    public string? Message { get; init; }

    /// <summary>The active effect instance (if applied).</summary>
    public ActiveStatusEffect? ActiveEffect { get; init; }

    /// <summary>Creates a success result for a new effect.</summary>
    public static ApplyResult Success(ActiveStatusEffect effect)
        => new() { WasApplied = true, ResultType = ApplyResultType.Applied, ActiveEffect = effect };

    /// <summary>Creates a result for refreshed duration.</summary>
    public static ApplyResult Refreshed(ActiveStatusEffect effect)
        => new() { WasApplied = true, ResultType = ApplyResultType.Refreshed, ActiveEffect = effect };

    /// <summary>Creates a result for added stacks.</summary>
    public static ApplyResult Stacked(ActiveStatusEffect effect)
        => new() { WasApplied = true, ResultType = ApplyResultType.Stacked, ActiveEffect = effect };

    /// <summary>Creates a result for replaced effect.</summary>
    public static ApplyResult Replaced(ActiveStatusEffect effect)
        => new() { WasApplied = true, ResultType = ApplyResultType.Replaced, ActiveEffect = effect };

    /// <summary>Creates a result when at max stacks.</summary>
    public static ApplyResult AtMaxStacks(ActiveStatusEffect effect)
        => new()
        {
            WasApplied = true,
            ResultType = ApplyResultType.AtMaxStacks,
            ActiveEffect = effect,
            Message = "Effect is at maximum stacks, duration refreshed"
        };

    /// <summary>Creates a failed result.</summary>
    public static ApplyResult Failed(string reason)
        => new() { WasApplied = false, ResultType = ApplyResultType.Failed, Message = reason };

    /// <summary>Creates an immunity result.</summary>
    public static ApplyResult Immune(string effectId)
        => new()
        {
            WasApplied = false,
            ResultType = ApplyResultType.Immune,
            Message = $"Target is immune to {effectId}"
        };

    /// <summary>Creates a blocked result.</summary>
    public static ApplyResult Blocked(string effectId)
        => new()
        {
            WasApplied = false,
            ResultType = ApplyResultType.Blocked,
            Message = $"Effect {effectId} was blocked"
        };
}

/// <summary>
/// Types of apply result outcomes.
/// </summary>
public enum ApplyResultType
{
    /// <summary>New effect applied.</summary>
    Applied,

    /// <summary>Duration refreshed.</summary>
    Refreshed,

    /// <summary>Stacks added.</summary>
    Stacked,

    /// <summary>Effect replaced.</summary>
    Replaced,

    /// <summary>At max stacks, duration refreshed.</summary>
    AtMaxStacks,

    /// <summary>Application failed.</summary>
    Failed,

    /// <summary>Target is immune.</summary>
    Immune,

    /// <summary>Effect was blocked.</summary>
    Blocked
}
