using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Encapsulates all context and modifiers for a climbing attempt.
/// </summary>
/// <remarks>
/// <para>
/// ClimbContext aggregates factors affecting a climb:
/// <list type="bullet">
///   <item><description>Total height determines stages required (1-3)</description></item>
///   <item><description>Surface type provides dice or DC modifiers</description></item>
///   <item><description>Equipment bonuses (climbing gear, grappling hook)</description></item>
///   <item><description>Armor penalties (medium: -1d10, heavy: -4d10)</description></item>
/// </list>
/// </para>
/// <para>
/// Stage Calculation:
/// <list type="bullet">
///   <item><description>1-20ft: 1 stage</description></item>
///   <item><description>21-40ft: 2 stages</description></item>
///   <item><description>41+ft: 3 stages</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="TotalHeight">The total height to climb in feet.</param>
/// <param name="SurfaceType">The surface type affecting the climb.</param>
/// <param name="BaseDc">The base difficulty class before modifiers.</param>
/// <param name="EquipmentDiceModifier">Dice modifier from climbing equipment.</param>
/// <param name="ArmorPenalty">Dice penalty from worn armor (negative value).</param>
/// <param name="EquipmentDescription">Optional description of equipment used.</param>
/// <param name="ArmorDescription">Optional description of armor worn.</param>
public readonly record struct ClimbContext(
    int TotalHeight,
    SurfaceType SurfaceType,
    int BaseDc,
    int EquipmentDiceModifier = 0,
    int ArmorPenalty = 0,
    string? EquipmentDescription = null,
    string? ArmorDescription = null)
{
    /// <summary>
    /// Gets the number of stages required to complete this climb.
    /// </summary>
    /// <remarks>
    /// Height thresholds:
    /// <list type="bullet">
    ///   <item><description>≤0ft: 0 stages (invalid climb)</description></item>
    ///   <item><description>1-20ft: 1 stage</description></item>
    ///   <item><description>21-40ft: 2 stages</description></item>
    ///   <item><description>41+ft: 3 stages (maximum)</description></item>
    /// </list>
    /// </remarks>
    public int StagesRequired => TotalHeight switch
    {
        <= 0 => 0,
        <= 20 => 1,
        <= 40 => 2,
        _ => 3
    };

    /// <summary>
    /// Gets the dice modifier from the surface type.
    /// </summary>
    /// <remarks>
    /// Returns 0 for surfaces that use DC modifiers (e.g., Glitched).
    /// </remarks>
    public int SurfaceDiceModifier => SurfaceType.GetDiceModifier();

    /// <summary>
    /// Gets the DC modifier from the surface type.
    /// </summary>
    /// <remarks>
    /// Only Glitched surfaces apply DC modifiers (+2).
    /// </remarks>
    public int SurfaceDcModifier => SurfaceType.GetDcModifier();

    /// <summary>
    /// Gets the total dice pool modification from all sources.
    /// </summary>
    /// <remarks>
    /// Sum of: surface modifier + equipment modifier + armor penalty.
    /// This value is added to the base dice pool for skill checks.
    /// </remarks>
    public int TotalDiceModifier => SurfaceDiceModifier + EquipmentDiceModifier + ArmorPenalty;

    /// <summary>
    /// Gets the effective DC after applying surface modifiers.
    /// </summary>
    /// <remarks>
    /// Base DC + surface DC modifier (only applies for Glitched surfaces).
    /// </remarks>
    public int EffectiveDc => BaseDc + SurfaceDcModifier;

    /// <summary>
    /// Gets the target height at a specific stage.
    /// </summary>
    /// <param name="stageNumber">The 1-based stage number.</param>
    /// <returns>The height reached upon completing the stage.</returns>
    /// <example>
    /// For a 50ft climb:
    /// - Stage 1: 20ft
    /// - Stage 2: 40ft
    /// - Stage 3: 50ft (destination)
    /// </example>
    public int GetHeightAtStage(int stageNumber)
    {
        return stageNumber switch
        {
            1 => Math.Min(20, TotalHeight),
            2 => Math.Min(40, TotalHeight),
            3 => TotalHeight,
            _ => 0
        };
    }

    /// <summary>
    /// Creates a ClimbingStage for the specified stage number.
    /// </summary>
    /// <param name="stageNumber">The 1-based stage number to create.</param>
    /// <returns>A new ClimbingStage with context-derived values.</returns>
    public ClimbingStage CreateStage(int stageNumber)
    {
        return new ClimbingStage(
            StageNumber: stageNumber,
            HeightReached: GetHeightAtStage(stageNumber),
            SurfaceType: SurfaceType,
            StageDc: EffectiveDc);
    }

    /// <summary>
    /// Returns a human-readable description of the climb context.
    /// </summary>
    /// <returns>Formatted context description for display.</returns>
    public string ToDescription()
    {
        var parts = new List<string>
        {
            $"Height: {TotalHeight}ft ({StagesRequired} stages)",
            $"Surface: {SurfaceType.GetDescription()}",
            $"DC: {EffectiveDc}"
        };

        if (EquipmentDiceModifier != 0)
        {
            var equipDesc = EquipmentDescription ?? "Equipment";
            var sign = EquipmentDiceModifier > 0 ? "+" : "";
            parts.Add($"{equipDesc}: {sign}{EquipmentDiceModifier}d10");
        }

        if (ArmorPenalty != 0)
        {
            var armorDesc = ArmorDescription ?? "Armor";
            parts.Add($"{armorDesc}: {ArmorPenalty}d10");
        }

        if (TotalDiceModifier != 0)
        {
            var sign = TotalDiceModifier > 0 ? "+" : "";
            parts.Add($"Net Modifier: {sign}{TotalDiceModifier}d10");
        }

        return string.Join(", ", parts);
    }

    /// <inheritdoc/>
    public override string ToString() => ToDescription();

    /// <summary>
    /// Creates a ClimbContext with standard default values.
    /// </summary>
    /// <param name="height">The total height to climb in feet.</param>
    /// <param name="surfaceType">The surface type.</param>
    /// <param name="baseDc">Base difficulty class (default: 2).</param>
    /// <returns>A new ClimbContext with the specified values.</returns>
    public static ClimbContext Create(int height, SurfaceType surfaceType, int baseDc = 2)
    {
        return new ClimbContext(
            TotalHeight: height,
            SurfaceType: surfaceType,
            BaseDc: baseDc);
    }

    /// <summary>
    /// Creates a ClimbContext with equipment and armor modifiers calculated.
    /// </summary>
    /// <param name="height">The total height to climb in feet.</param>
    /// <param name="surfaceType">The surface type.</param>
    /// <param name="baseDc">Base difficulty class.</param>
    /// <param name="hasClimbingGear">Whether climbing gear is equipped.</param>
    /// <param name="hasGrapplingHook">Whether a grappling hook is available.</param>
    /// <param name="armorWeight">The weight category of worn armor.</param>
    /// <returns>A new ClimbContext with all modifiers calculated.</returns>
    /// <remarks>
    /// Equipment modifiers:
    /// <list type="bullet">
    ///   <item><description>Climbing Gear: +1d10</description></item>
    ///   <item><description>Grappling Hook: +2d10</description></item>
    /// </list>
    /// Armor penalties:
    /// <list type="bullet">
    ///   <item><description>None/Light: 0</description></item>
    ///   <item><description>Medium: -1d10</description></item>
    ///   <item><description>Heavy: -4d10</description></item>
    /// </list>
    /// </remarks>
    public static ClimbContext CreateWithEquipment(
        int height,
        SurfaceType surfaceType,
        int baseDc,
        bool hasClimbingGear = false,
        bool hasGrapplingHook = false,
        string armorWeight = "none")
    {
        // Calculate equipment bonus
        var equipmentBonus = 0;
        var equipmentDesc = new List<string>();

        if (hasClimbingGear)
        {
            equipmentBonus += 1;
            equipmentDesc.Add("Climbing Gear");
        }

        if (hasGrapplingHook)
        {
            equipmentBonus += 2;
            equipmentDesc.Add("Grappling Hook");
        }

        // Calculate armor penalty
        var armorPenalty = armorWeight.ToLowerInvariant() switch
        {
            "none" => 0,
            "light" => 0,
            "medium" => -1,
            "heavy" => -4,
            _ => 0
        };

        var armorDesc = armorWeight.ToLowerInvariant() switch
        {
            "medium" => "Medium Armor",
            "heavy" => "Heavy Armor",
            _ => null
        };

        return new ClimbContext(
            TotalHeight: height,
            SurfaceType: surfaceType,
            BaseDc: baseDc,
            EquipmentDiceModifier: equipmentBonus,
            ArmorPenalty: armorPenalty,
            EquipmentDescription: equipmentDesc.Count > 0 ? string.Join(" + ", equipmentDesc) : null,
            ArmorDescription: armorDesc);
    }
}
