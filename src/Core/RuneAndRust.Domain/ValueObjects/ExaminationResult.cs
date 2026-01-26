namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the complete result of examining an object.
/// </summary>
/// <remarks>
/// <para>
/// Contains the outcome of a WITS check against layer DCs, including:
/// </para>
/// <list type="bullet">
///   <item><description>WitsCheckResult - The number of successes rolled</description></item>
///   <item><description>HighestLayerUnlocked - Which layers were unlocked (1, 2, or 3)</description></item>
///   <item><description>CompositeDescription - Combined narrative from all unlocked layers</description></item>
///   <item><description>RevealedHint/RevealedSolutionId - Puzzle hint integration (v0.15.6e)</description></item>
///   <item><description>RevealedInteractions - New commands made available</description></item>
/// </list>
/// <para>
/// The composite description combines all unlocked layer texts into a
/// flowing narrative, while LayerTexts preserves individual layers for
/// debugging or special display needs.
/// </para>
/// </remarks>
/// <param name="ObjectId">The unique identifier of the examined object.</param>
/// <param name="ObjectName">The display name of the object.</param>
/// <param name="WitsCheckResult">The number of net successes from the WITS check.</param>
/// <param name="HighestLayerUnlocked">The highest layer unlocked (1, 2, or 3).</param>
/// <param name="CompositeDescription">All unlocked layers combined into narrative text.</param>
/// <param name="LayerTexts">Individual layer description texts.</param>
/// <param name="RevealedHint">Whether a puzzle hint was revealed (Layer 3 only).</param>
/// <param name="RevealedSolutionId">The puzzle solution ID if a hint was revealed.</param>
/// <param name="RevealedInteractions">New commands/interactions made available.</param>
public readonly record struct ExaminationResult(
    string ObjectId,
    string ObjectName,
    int WitsCheckResult,
    int HighestLayerUnlocked,
    string CompositeDescription,
    IReadOnlyList<string> LayerTexts,
    bool RevealedHint,
    string? RevealedSolutionId,
    IReadOnlyList<string> RevealedInteractions)
{
    /// <summary>
    /// Gets the examination layer enum value for the highest unlocked layer.
    /// </summary>
    public ExaminationLayer UnlockedLayer => (ExaminationLayer)HighestLayerUnlocked;

    /// <summary>
    /// Gets whether Layer 2 (Detailed) was unlocked.
    /// </summary>
    /// <remarks>
    /// True when the character achieved at least DC 12 on their WITS check,
    /// or had previous Layer 2 knowledge cached.
    /// </remarks>
    public bool HasDetailedKnowledge => HighestLayerUnlocked >= (int)ExaminationLayer.Detailed;

    /// <summary>
    /// Gets whether Layer 3 (Expert) was unlocked.
    /// </summary>
    /// <remarks>
    /// True when the character achieved at least DC 18 on their WITS check,
    /// or had previous Layer 3 knowledge cached.
    /// </remarks>
    public bool HasExpertKnowledge => HighestLayerUnlocked >= (int)ExaminationLayer.Expert;

    /// <summary>
    /// Gets whether any new interactions were revealed.
    /// </summary>
    /// <remarks>
    /// Interactions are typically revealed at Layer 3 (Expert) and may include
    /// new commands like "bypass", "disarm", or "activate".
    /// </remarks>
    public bool HasNewInteractions => RevealedInteractions.Count > 0;

    /// <summary>
    /// Gets the layer count (how many layers are included in the description).
    /// </summary>
    public int LayerCount => LayerTexts.Count;

    /// <summary>
    /// Creates a cursory-only result (Layer 1, no check required).
    /// </summary>
    /// <param name="objectId">The object identifier.</param>
    /// <param name="objectName">The object display name.</param>
    /// <param name="cursoryDescription">The Layer 1 description text.</param>
    /// <returns>An ExaminationResult with only Layer 1 unlocked.</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are null or whitespace.</exception>
    public static ExaminationResult CursoryOnly(
        string objectId,
        string objectName,
        string cursoryDescription)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cursoryDescription);

        return new ExaminationResult(
            objectId,
            objectName,
            WitsCheckResult: 0,
            HighestLayerUnlocked: (int)ExaminationLayer.Cursory,
            CompositeDescription: cursoryDescription,
            LayerTexts: new[] { cursoryDescription },
            RevealedHint: false,
            RevealedSolutionId: null,
            RevealedInteractions: Array.Empty<string>());
    }

    /// <summary>
    /// Creates a result with specified layers unlocked.
    /// </summary>
    /// <param name="objectId">The object identifier.</param>
    /// <param name="objectName">The object display name.</param>
    /// <param name="witsResult">The WITS check result (net successes).</param>
    /// <param name="highestLayer">The highest layer unlocked (1, 2, or 3).</param>
    /// <param name="layerTexts">The individual layer texts (must have at least highestLayer entries).</param>
    /// <param name="revealedHint">Whether a hint was revealed.</param>
    /// <param name="solutionId">The revealed solution ID (if any).</param>
    /// <param name="interactions">Revealed interactions (if any).</param>
    /// <returns>A new ExaminationResult with composed description.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when highestLayer is out of range.</exception>
    public static ExaminationResult Create(
        string objectId,
        string objectName,
        int witsResult,
        int highestLayer,
        IReadOnlyList<string> layerTexts,
        bool revealedHint = false,
        string? solutionId = null,
        IReadOnlyList<string>? interactions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectName);
        ArgumentNullException.ThrowIfNull(layerTexts);
        ArgumentOutOfRangeException.ThrowIfLessThan(highestLayer, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(highestLayer, 3);

        // Compose description from unlocked layers (take up to highestLayer entries)
        var textsToInclude = layerTexts.Take(highestLayer).ToList();
        var composite = string.Join("\n\n", textsToInclude);

        return new ExaminationResult(
            objectId,
            objectName,
            witsResult,
            highestLayer,
            composite,
            textsToInclude,
            revealedHint,
            solutionId,
            interactions ?? Array.Empty<string>());
    }

    /// <summary>
    /// Creates a display string summarizing the examination outcome.
    /// </summary>
    /// <returns>A formatted summary string for display or logging.</returns>
    public string ToSummaryString()
    {
        var layerName = UnlockedLayer switch
        {
            ExaminationLayer.Cursory => "Cursory",
            ExaminationLayer.Detailed => "Detailed",
            ExaminationLayer.Expert => "Expert",
            _ => "Unknown"
        };

        var summary = $"[{ObjectName}] - {layerName} Knowledge";

        if (WitsCheckResult > 0)
        {
            summary += $" (WITS check: {WitsCheckResult} successes)";
        }

        if (RevealedHint)
        {
            summary += " [Hint revealed!]";
        }

        if (HasNewInteractions)
        {
            summary += $" [New commands: {string.Join(", ", RevealedInteractions)}]";
        }

        return summary;
    }

    /// <summary>
    /// Returns a string representation of this result for debugging.
    /// </summary>
    /// <returns>A formatted string showing key result values.</returns>
    public override string ToString() =>
        $"ExaminationResult(Object={ObjectId}, Layer={HighestLayerUnlocked}, " +
        $"WITS={WitsCheckResult}, Layers={LayerCount}, Hint={RevealedHint})";
}
