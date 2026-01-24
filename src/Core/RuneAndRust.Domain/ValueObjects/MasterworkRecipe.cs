// ------------------------------------------------------------------------------
// <copyright file="MasterworkRecipe.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents a masterwork tool recipe unlocked by the [Master Craftsman]
// ability, including required components, crafting DC, and tool properties.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a masterwork tool recipe for the [Master Craftsman] ability.
/// </summary>
/// <remarks>
/// <para>
/// Masterwork recipes are special crafting options available only to Scrap-Tinkers
/// with the [Master Craftsman] ability. These tools provide superior quality
/// (+1 bonus die) compared to standard improvised tools.
/// </para>
/// <para>
/// <b>Masterwork Tool Properties:</b>
/// <list type="bullet">
///   <item><description>+1 bonus die on bypass attempts</description></item>
///   <item><description>Higher durability (longer lifespan)</description></item>
///   <item><description>Require rare components to craft</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="RecipeId">Unique identifier for the recipe.</param>
/// <param name="Name">Display name of the masterwork tool.</param>
/// <param name="Description">Narrative description of the tool.</param>
/// <param name="BypassType">Which bypass system the tool assists.</param>
/// <param name="RequiredComponents">Components needed to craft the tool.</param>
/// <param name="CraftingDc">WITS DC to craft the tool.</param>
/// <param name="BonusDice">Number of bonus dice the tool provides.</param>
/// <param name="DurabilityUses">Number of uses before the tool degrades.</param>
public readonly record struct MasterworkRecipe(
    string RecipeId,
    string Name,
    string Description,
    BypassType BypassType,
    IReadOnlyList<string> RequiredComponents,
    int CraftingDc,
    int BonusDice,
    int DurabilityUses)
{
    // =========================================================================
    // COMPUTED PROPERTIES
    // =========================================================================

    /// <summary>
    /// Gets the number of components required.
    /// </summary>
    public int ComponentCount => RequiredComponents.Count;

    /// <summary>
    /// Gets a value indicating whether this is a high-tier recipe (DC 16+).
    /// </summary>
    public bool IsHighTier => CraftingDc >= 16;

    // =========================================================================
    // STATIC FACTORY METHODS
    // =========================================================================

    /// <summary>
    /// Creates the Masterwork Shim Picks recipe (lockpicking).
    /// </summary>
    /// <returns>The masterwork shim picks recipe.</returns>
    /// <remarks>
    /// <para>
    /// <b>Masterwork Shim Picks</b>: Precision-crafted lock picks made from
    /// salvaged Old World alloys. Provides +1 bonus die on lockpicking attempts.
    /// </para>
    /// <para>
    /// <b>Components:</b> Fine Wire, Precision Springs, Grip Material<br/>
    /// <b>Crafting DC:</b> 14<br/>
    /// <b>Durability:</b> 10 uses
    /// </para>
    /// </remarks>
    public static MasterworkRecipe ShimPicks() => new(
        RecipeId: "masterwork-shim-picks",
        Name: "Masterwork Shim Picks",
        Description: "Precision-crafted lock picks made from salvaged Old World alloys. " +
                     "The balance and flexibility of these tools is unmatched.",
        BypassType: BypassType.Lockpicking,
        RequiredComponents: new[] { "Fine Wire", "Precision Springs", "Grip Material" },
        CraftingDc: 14,
        BonusDice: 1,
        DurabilityUses: 10);

    /// <summary>
    /// Creates the Masterwork Wire Probe recipe (terminal hacking).
    /// </summary>
    /// <returns>The masterwork wire probe recipe.</returns>
    /// <remarks>
    /// <para>
    /// <b>Masterwork Wire Probe</b>: A delicate probe designed for interfacing
    /// with Old World terminals. Provides +1 bonus die on hacking attempts.
    /// </para>
    /// <para>
    /// <b>Components:</b> Conductive Filament, Insulated Housing, Circuit Fragment<br/>
    /// <b>Crafting DC:</b> 16<br/>
    /// <b>Durability:</b> 8 uses
    /// </para>
    /// </remarks>
    public static MasterworkRecipe WireProbe() => new(
        RecipeId: "masterwork-wire-probe",
        Name: "Masterwork Wire Probe",
        Description: "A delicate probe designed for interfacing with Old World terminals. " +
                     "Its conductive filaments can navigate the most complex circuits.",
        BypassType: BypassType.TerminalHacking,
        RequiredComponents: new[] { "Conductive Filament", "Insulated Housing", "Circuit Fragment" },
        CraftingDc: 16,
        BonusDice: 1,
        DurabilityUses: 8);

    /// <summary>
    /// Creates the Masterwork Trap Kit recipe (trap disarmament).
    /// </summary>
    /// <returns>The masterwork trap kit recipe.</returns>
    /// <remarks>
    /// <para>
    /// <b>Masterwork Trap Kit</b>: A comprehensive toolkit for neutralizing
    /// mechanical and magical traps. Provides +1 bonus die on disarmament attempts.
    /// </para>
    /// <para>
    /// <b>Components:</b> Thin Blades, Mirror Shard, Dampening Cloth<br/>
    /// <b>Crafting DC:</b> 15<br/>
    /// <b>Durability:</b> 6 uses
    /// </para>
    /// </remarks>
    public static MasterworkRecipe TrapKit() => new(
        RecipeId: "masterwork-trap-kit",
        Name: "Masterwork Trap Kit",
        Description: "A comprehensive toolkit for neutralizing mechanical and magical traps. " +
                     "Contains everything a professional needs to work safely.",
        BypassType: BypassType.TrapDisarmament,
        RequiredComponents: new[] { "Thin Blades", "Mirror Shard", "Dampening Cloth" },
        CraftingDc: 15,
        BonusDice: 1,
        DurabilityUses: 6);

    /// <summary>
    /// Creates the Masterwork Glitch Trigger recipe (glitch exploitation).
    /// </summary>
    /// <returns>The masterwork glitch trigger recipe.</returns>
    /// <remarks>
    /// <para>
    /// <b>Masterwork Glitch Trigger</b>: A device tuned to exploit Old World
    /// corruption patterns. Provides +1 bonus die on glitch exploitation attempts.
    /// </para>
    /// <para>
    /// <b>Components:</b> Corrupted Chip, Resonance Crystal, Null-Metal Casing<br/>
    /// <b>Crafting DC:</b> 18<br/>
    /// <b>Durability:</b> 4 uses
    /// </para>
    /// </remarks>
    public static MasterworkRecipe GlitchTrigger() => new(
        RecipeId: "masterwork-glitch-trigger",
        Name: "Masterwork Glitch Trigger",
        Description: "A device tuned to exploit Old World corruption patterns. " +
                     "The resonance crystal at its heart reads the machine's madness.",
        BypassType: BypassType.GlitchExploitation,
        RequiredComponents: new[] { "Corrupted Chip", "Resonance Crystal", "Null-Metal Casing" },
        CraftingDc: 18,
        BonusDice: 1,
        DurabilityUses: 4);

    // =========================================================================
    // STATIC QUERY METHODS
    // =========================================================================

    /// <summary>
    /// Gets all masterwork recipes.
    /// </summary>
    /// <returns>A list of all available masterwork recipes.</returns>
    public static IReadOnlyList<MasterworkRecipe> GetAll()
    {
        return new[]
        {
            ShimPicks(),
            WireProbe(),
            TrapKit(),
            GlitchTrigger()
        };
    }

    /// <summary>
    /// Gets masterwork recipes for a specific bypass type.
    /// </summary>
    /// <param name="bypassType">The bypass type to filter by.</param>
    /// <returns>Recipes that apply to the specified bypass type.</returns>
    public static IReadOnlyList<MasterworkRecipe> GetForBypassType(BypassType bypassType)
    {
        return GetAll().Where(r => r.BypassType == bypassType).ToArray();
    }

    /// <summary>
    /// Gets a recipe by its ID.
    /// </summary>
    /// <param name="recipeId">The recipe identifier.</param>
    /// <returns>The recipe, or null if not found.</returns>
    public static MasterworkRecipe? GetById(string recipeId)
    {
        return recipeId?.ToLowerInvariant() switch
        {
            "masterwork-shim-picks" => ShimPicks(),
            "masterwork-wire-probe" => WireProbe(),
            "masterwork-trap-kit" => TrapKit(),
            "masterwork-glitch-trigger" => GlitchTrigger(),
            _ => null
        };
    }

    // =========================================================================
    // DISPLAY METHODS
    // =========================================================================

    /// <summary>
    /// Creates a display string for the recipe.
    /// </summary>
    /// <returns>A formatted multi-line string showing recipe details.</returns>
    /// <example>
    /// <code>
    /// var recipe = MasterworkRecipe.ShimPicks();
    /// Console.WriteLine(recipe.ToDisplayString());
    /// // Output:
    /// // Masterwork Shim Picks
    /// // Type: Lockpicking | Crafting DC: 14
    /// // Bonus: +1d10 | Durability: 10 uses
    /// // Components: Fine Wire, Precision Springs, Grip Material
    /// // Precision-crafted lock picks made from salvaged Old World alloys...
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var lines = new List<string>
        {
            Name,
            $"Type: {BypassType} | Crafting DC: {CraftingDc}",
            $"Bonus: +{BonusDice}d10 | Durability: {DurabilityUses} uses",
            $"Components: {string.Join(", ", RequiredComponents)}",
            string.Empty,
            Description
        };

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns a compact string for logging.
    /// </summary>
    /// <returns>A single-line string for logging.</returns>
    public string ToLogString()
    {
        return $"MasterworkRecipe[{RecipeId}] {Name} DC:{CraftingDc} +{BonusDice}d10 ({DurabilityUses} uses)";
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
