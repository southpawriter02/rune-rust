using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Conditions;

/// <summary>
/// Condition that checks if a specialization node is unlocked.
/// Example: [Has: Rage ability] for ability-gated dialogue.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class NodeCondition : DialogueCondition
{
    /// <inheritdoc/>
    public override DialogueConditionType Type => DialogueConditionType.Node;

    /// <summary>
    /// The specialization node ID to check for.
    /// </summary>
    public Guid NodeId { get; set; }

    /// <summary>
    /// Display name for the hint (cached from node/ability).
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override string GetDisplayHint() => $"[Has: {NodeName}]";
}
