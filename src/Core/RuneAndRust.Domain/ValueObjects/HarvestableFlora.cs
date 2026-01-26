namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a harvestable flora instance with harvest difficulty and risk information.
/// </summary>
/// <remarks>
/// <para>
/// This value object is returned by flora/fauna services to provide players with
/// harvest opportunities and the information needed to assess risk vs. reward.
/// </para>
/// <para>
/// Harvest DC determines the skill check difficulty, while HarvestRisk describes
/// potential negative consequences of failed harvest attempts.
/// </para>
/// </remarks>
/// <param name="SpeciesId">The species descriptor ID for reference.</param>
/// <param name="SpeciesName">The common name for display.</param>
/// <param name="HarvestDc">The DC required for successful harvest.</param>
/// <param name="HarvestRisk">Description of what happens on failed harvest, if any.</param>
/// <param name="AlchemicalUse">What the harvested material can be used for.</param>
public readonly record struct HarvestableFlora(
    string SpeciesId,
    string SpeciesName,
    int HarvestDc,
    string? HarvestRisk,
    string? AlchemicalUse)
{
    /// <summary>
    /// Gets whether attempting to harvest this flora carries risk.
    /// </summary>
    /// <remarks>
    /// Risky flora may cause damage, status effects, or other negative
    /// consequences when harvest fails.
    /// </remarks>
    public bool IsRisky => !string.IsNullOrEmpty(HarvestRisk);

    /// <summary>
    /// Gets whether this flora has known alchemical applications.
    /// </summary>
    public bool HasAlchemicalUse => !string.IsNullOrEmpty(AlchemicalUse);

    /// <summary>
    /// Creates a display string for the harvest opportunity.
    /// </summary>
    /// <returns>A formatted string describing the harvest option with DC and risk.</returns>
    /// <example>
    /// "Luminous Shelf Fungus (DC 8)"
    /// "Rust-Eater Moss (DC 12) [Risk: Acidic residue]"
    /// </example>
    public string ToDisplayString()
    {
        var riskWarning = IsRisky ? $" [Risk: {HarvestRisk}]" : "";
        return $"{SpeciesName} (DC {HarvestDc}){riskWarning}";
    }

    /// <summary>
    /// Creates a detailed display string including alchemical use.
    /// </summary>
    /// <returns>A multi-line formatted string with full harvest details.</returns>
    public string ToDetailedString()
    {
        var lines = new List<string>
        {
            $"• {SpeciesName} (DC {HarvestDc}){(IsRisky ? $" [Risk: {HarvestRisk}]" : "")}"
        };

        if (HasAlchemicalUse)
        {
            lines.Add($"  Use: {AlchemicalUse}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
