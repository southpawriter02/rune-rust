namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of an Antidote Craft ability execution.
/// Records the crafting process, materials consumed, success status,
/// and the created Antidote item (if successful).
/// </summary>
/// <remarks>
/// <para>Antidote Craft is the Bone-Setter's Tier 2 crafting ability:</para>
/// <list type="bullet">
/// <item>Cost: 2 AP + 1 Herbs supply + crafting materials (2 Plant Fiber, 1 Mineral Powder)</item>
/// <item>Always succeeds (100% success rate) — no DC check required</item>
/// <item>Output quality: Min(Herbs quality + material bonus, 5)</item>
/// <item>Material bonus: +1 if all materials are Quality 3+, otherwise +0</item>
/// <item>Corruption: None (Coherent path)</item>
/// </list>
/// <para>Use <see cref="CreateSuccess"/> or <see cref="CreateFailure"/> factory methods
/// to construct result instances.</para>
/// </remarks>
public sealed record AntidoteCraftResult
{
    /// <summary>
    /// Whether the crafting was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Human-readable message describing the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Name of the recipe used for crafting (e.g., "Basic Antidote").
    /// </summary>
    public string RecipeUsed { get; init; } = string.Empty;

    /// <summary>
    /// Quality rating of the created Antidote (1–5).
    /// 0 if crafting failed (prerequisite failure).
    /// </summary>
    public int CraftedQuality { get; init; }

    /// <summary>
    /// Materials consumed in the crafting process.
    /// Maps material name to quantity consumed.
    /// Empty if no materials were consumed (prerequisite failure).
    /// </summary>
    public IReadOnlyDictionary<string, int> MaterialsConsumed { get; init; }
        = new Dictionary<string, int>().AsReadOnly();

    /// <summary>
    /// The created Antidote supply item.
    /// Null if crafting failed.
    /// </summary>
    public MedicalSupplyItem? CreatedAntidote { get; init; }

    /// <summary>
    /// Number of Medical Supplies in inventory after crafting.
    /// </summary>
    public int SuppliesRemaining { get; init; }

    /// <summary>
    /// Reason for failure if <see cref="Success"/> is false.
    /// Null on successful crafting.
    /// </summary>
    public string? FailureReason { get; init; }

    /// <summary>
    /// Creates a successful Antidote Craft result.
    /// </summary>
    /// <param name="recipeUsed">Name of the recipe used.</param>
    /// <param name="craftedQuality">Quality of the created Antidote (1–5).</param>
    /// <param name="materialsConsumed">Map of material name to quantity consumed.</param>
    /// <param name="createdAntidote">The created Antidote supply item.</param>
    /// <param name="suppliesRemaining">Supplies remaining after crafting.</param>
    /// <returns>A successful <see cref="AntidoteCraftResult"/>.</returns>
    public static AntidoteCraftResult CreateSuccess(
        string recipeUsed,
        int craftedQuality,
        Dictionary<string, int> materialsConsumed,
        MedicalSupplyItem createdAntidote,
        int suppliesRemaining)
    {
        return new AntidoteCraftResult
        {
            Success = true,
            Message = $"Successfully crafted {recipeUsed} (Quality {craftedQuality})",
            RecipeUsed = recipeUsed,
            CraftedQuality = craftedQuality,
            MaterialsConsumed = materialsConsumed.AsReadOnly(),
            CreatedAntidote = createdAntidote,
            SuppliesRemaining = suppliesRemaining,
            FailureReason = null
        };
    }

    /// <summary>
    /// Creates a failed Antidote Craft result due to missing prerequisites.
    /// </summary>
    /// <param name="recipeUsed">Name of the recipe attempted.</param>
    /// <param name="failureReason">Reason the crafting failed.</param>
    /// <param name="suppliesRemaining">Supplies remaining (unchanged).</param>
    /// <returns>A failed <see cref="AntidoteCraftResult"/>.</returns>
    public static AntidoteCraftResult CreateFailure(
        string recipeUsed,
        string failureReason,
        int suppliesRemaining)
    {
        return new AntidoteCraftResult
        {
            Success = false,
            Message = $"Failed to craft {recipeUsed}: {failureReason}",
            RecipeUsed = recipeUsed,
            CraftedQuality = 0,
            MaterialsConsumed = new Dictionary<string, int>().AsReadOnly(),
            CreatedAntidote = null,
            SuppliesRemaining = suppliesRemaining,
            FailureReason = failureReason
        };
    }

    /// <summary>
    /// Returns a formatted summary of materials consumed in the crafting process.
    /// </summary>
    /// <returns>
    /// A formatted string such as "Herbs(1) + Plant Fiber(2) + Mineral Powder(1)".
    /// Returns "None" if no materials were consumed.
    /// </returns>
    public string GetMaterialSummary()
    {
        if (MaterialsConsumed.Count == 0)
            return "None";

        return string.Join(" + ", MaterialsConsumed.Select(m => $"{m.Key}({m.Value})"));
    }
}
