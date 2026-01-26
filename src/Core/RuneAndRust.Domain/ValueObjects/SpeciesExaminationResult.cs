namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Contains the result of examining a specific flora or fauna species,
/// including all revealed layers and any special discoveries.
/// </summary>
/// <remarks>
/// <para>
/// This extends the general ExaminationResult pattern from v0.15.6b with
/// species-specific data including alchemical uses and harvest information.
/// </para>
/// <para>
/// Layer thresholds follow the standard DC mappings:
/// - Layer 1 (Cursory): DC 0 (automatic)
/// - Layer 2 (Detailed): DC 12 (2 successes)
/// - Layer 3 (Expert): DC 18 (4 successes)
/// </para>
/// </remarks>
/// <param name="SpeciesId">The examined species descriptor ID.</param>
/// <param name="SpeciesName">The species common name.</param>
/// <param name="ScientificName">The Aethelgard Latin name, if known.</param>
/// <param name="Category">Whether this is Flora or Fauna.</param>
/// <param name="LayersRevealed">List of examination text for each revealed layer.</param>
/// <param name="HighestLayerReached">The maximum layer achieved (1, 2, or 3).</param>
/// <param name="SuccessCount">Number of successes rolled on the Wits check.</param>
/// <param name="AlchemicalUseRevealed">Alchemical use info if Layer 3 reached.</param>
/// <param name="HarvestInfoRevealed">Harvest DC and risk if Layer 3 reached on flora.</param>
public readonly record struct SpeciesExaminationResult(
    string SpeciesId,
    string SpeciesName,
    string? ScientificName,
    FloraFaunaCategory Category,
    IReadOnlyList<string> LayersRevealed,
    int HighestLayerReached,
    int SuccessCount,
    string? AlchemicalUseRevealed,
    HarvestableFlora? HarvestInfoRevealed)
{
    /// <summary>
    /// Gets whether expert-level information was revealed.
    /// </summary>
    public bool ReachedExpertLevel => HighestLayerReached >= 3;

    /// <summary>
    /// Gets whether detailed information was revealed.
    /// </summary>
    public bool ReachedDetailedLevel => HighestLayerReached >= 2;

    /// <summary>
    /// Gets whether alchemical uses were discovered.
    /// </summary>
    public bool DiscoveredAlchemicalUse => !string.IsNullOrEmpty(AlchemicalUseRevealed);

    /// <summary>
    /// Gets whether harvest information was revealed.
    /// </summary>
    public bool DiscoveredHarvestInfo => HarvestInfoRevealed.HasValue;

    /// <summary>
    /// Gets whether this is flora (can be harvested).
    /// </summary>
    public bool IsFlora => Category == FloraFaunaCategory.Flora;

    /// <summary>
    /// Gets whether this is fauna (cannot be harvested).
    /// </summary>
    public bool IsFauna => Category == FloraFaunaCategory.Fauna;

    /// <summary>
    /// Gets the examination header for display.
    /// </summary>
    /// <remarks>
    /// Scientific name is only included when expert level is reached.
    /// </remarks>
    /// <returns>The formatted header string.</returns>
    public string GetDisplayHeader()
    {
        if (!string.IsNullOrEmpty(ScientificName) && ReachedExpertLevel)
        {
            return $"{SpeciesName} ({ScientificName})";
        }
        return SpeciesName;
    }

    /// <summary>
    /// Gets the combined description text from all revealed layers.
    /// </summary>
    /// <returns>The concatenated description paragraphs.</returns>
    public string GetCombinedDescription()
    {
        if (LayersRevealed is null || LayersRevealed.Count == 0)
        {
            return string.Empty;
        }
        return string.Join(Environment.NewLine + Environment.NewLine, LayersRevealed);
    }

    /// <summary>
    /// Gets a summary string for logging and debugging.
    /// </summary>
    /// <returns>A formatted summary string.</returns>
    public override string ToString() =>
        $"SpeciesExaminationResult({SpeciesName}, Layer {HighestLayerReached}, {SuccessCount} successes)";
}
