// ------------------------------------------------------------------------------
// <copyright file="ToolRecipe.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the requirements for crafting an improvised tool.
// Recipes specify components needed, crafting difficulty, and the resulting tool.
// Part of v0.15.4g Improvised Tool Crafting System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines the requirements for crafting an improvised tool.
/// Recipes specify components needed, crafting difficulty, and the resulting tool.
/// </summary>
/// <remarks>
/// <para>
/// Each recipe defines:
/// <list type="bullet">
///   <item><description>Required components with quantities</description></item>
///   <item><description>Crafting DC (difficulty class)</description></item>
///   <item><description>Required attribute (WITS, FINESSE, or either)</description></item>
///   <item><description>The resulting tool type</description></item>
/// </list>
/// </para>
/// <para>
/// Recipes are accessed via static properties (ShimPicks, WireProbe, etc.)
/// or via the AllRecipes collection.
/// </para>
/// </remarks>
/// <param name="RecipeId">Unique identifier for the recipe.</param>
/// <param name="ToolName">Name of the tool this recipe produces.</param>
/// <param name="Description">Flavor description of the crafting process.</param>
/// <param name="RequiredComponents">List of components needed to craft.</param>
/// <param name="CraftDc">Difficulty class for the crafting check.</param>
/// <param name="RequiredAttribute">Primary attribute for the crafting check.</param>
/// <param name="ResultingToolType">The type of tool produced.</param>
public readonly record struct ToolRecipe(
    string RecipeId,
    string ToolName,
    string Description,
    IReadOnlyList<ComponentRequirement> RequiredComponents,
    int CraftDc,
    CraftingAttribute RequiredAttribute,
    ImprovisedToolType ResultingToolType)
{
    // -------------------------------------------------------------------------
    // Static Recipes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates the Shim Picks recipe.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Shim Picks are thin metal shims used to manipulate lock pins.
    /// </para>
    /// <para>
    /// Requirements: 3× Scrap Metal.
    /// Craft DC: 10 (WITS or FINESSE).
    /// Result: +1d10 to lockpicking, or +2d10 if crafted with critical success.
    /// </para>
    /// </remarks>
    public static ToolRecipe ShimPicks => new(
        RecipeId: "recipe_shim_picks",
        ToolName: "Shim Picks",
        Description: "Bend scrap metal into thin shims that slip between lock pins, " +
                    "allowing manipulation without proper lockpicks.",
        RequiredComponents: new List<ComponentRequirement>
        {
            new("Scrap Metal", ComponentType.Metal, 3)
        }.AsReadOnly(),
        CraftDc: 10,
        RequiredAttribute: CraftingAttribute.WitsOrFinesse,
        ResultingToolType: ImprovisedToolType.ShimPicks);

    /// <summary>
    /// Creates the Wire Probe recipe.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Wire Probe is a copper wire probe for manipulating terminal circuits.
    /// </para>
    /// <para>
    /// Requirements: 2× Copper Wire, 1× Handle.
    /// Craft DC: 12 (WITS or FINESSE).
    /// Result: +1d10 to terminal hacking, or +2d10 if crafted with critical success.
    /// </para>
    /// </remarks>
    public static ToolRecipe WireProbe => new(
        RecipeId: "recipe_wire_probe",
        ToolName: "Wire Probe",
        Description: "A carefully wrapped probe of copper wire attached to an insulated " +
                    "handle, used to manipulate exposed terminal circuits.",
        RequiredComponents: new List<ComponentRequirement>
        {
            new("Copper Wire", ComponentType.Wire, 2),
            new("Handle", ComponentType.Misc, 1)
        }.AsReadOnly(),
        CraftDc: 12,
        RequiredAttribute: CraftingAttribute.WitsOrFinesse,
        ResultingToolType: ImprovisedToolType.WireProbe);

    /// <summary>
    /// Creates the Glitch Trigger recipe.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Glitch Trigger is a crude device that induces glitched states in Old World machinery.
    /// </para>
    /// <para>
    /// Requirements: 1× Capacitor, 4× Wire.
    /// Craft DC: 14 (WITS).
    /// Result: +1d10 to glitch exploitation, or +2d10 if crafted with critical success.
    /// Special: Can force mechanism into [Glitched] state.
    /// </para>
    /// </remarks>
    public static ToolRecipe GlitchTrigger => new(
        RecipeId: "recipe_glitch_trigger",
        ToolName: "Glitch Trigger",
        Description: "A crude device that emits an electromagnetic pulse pattern " +
                    "known to induce glitched states in Old World machinery.",
        RequiredComponents: new List<ComponentRequirement>
        {
            new("Capacitor", ComponentType.Circuit, 1),
            new("Wire", ComponentType.Wire, 4)
        }.AsReadOnly(),
        CraftDc: 14,
        RequiredAttribute: CraftingAttribute.Wits,
        ResultingToolType: ImprovisedToolType.GlitchTrigger);

    /// <summary>
    /// Creates the Bypass Clamps recipe.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bypass Clamps are spring-loaded metal clips that bypass terminal access layers.
    /// </para>
    /// <para>
    /// Requirements: 4× Metal Clips.
    /// Craft DC: 12 (FINESSE).
    /// Result: +1d10 to terminal hacking, or +2d10 if crafted with critical success.
    /// Special: Skip Layer 1 of terminal infiltration.
    /// </para>
    /// </remarks>
    public static ToolRecipe BypassClamps => new(
        RecipeId: "recipe_bypass_clamps",
        ToolName: "Bypass Clamps",
        Description: "Spring-loaded metal clips that physically bypass the outer " +
                    "access layer of a terminal, connecting directly to inner circuits.",
        RequiredComponents: new List<ComponentRequirement>
        {
            new("Metal Clips", ComponentType.Metal, 4)
        }.AsReadOnly(),
        CraftDc: 12,
        RequiredAttribute: CraftingAttribute.Finesse,
        ResultingToolType: ImprovisedToolType.BypassClamps);

    /// <summary>
    /// Gets all available standard recipes.
    /// </summary>
    /// <remarks>
    /// This collection provides access to all craftable tool recipes.
    /// </remarks>
    public static IReadOnlyList<ToolRecipe> AllRecipes => new List<ToolRecipe>
    {
        ShimPicks,
        WireProbe,
        GlitchTrigger,
        BypassClamps
    }.AsReadOnly();

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the total component count required for this recipe.
    /// </summary>
    /// <remarks>
    /// This sums the quantity of all required components.
    /// </remarks>
    public int TotalComponentsRequired => RequiredComponents.Sum(c => c.Quantity);

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a display string for the recipe.
    /// </summary>
    /// <returns>A formatted multi-line description of the recipe.</returns>
    public string ToDisplayString()
    {
        var componentList = string.Join(", ",
            RequiredComponents.Select(c => $"{c.ComponentName} ×{c.Quantity}"));

        return $"""
            [{ToolName}] (DC {CraftDc}, {RequiredAttribute})
            Components: {componentList}
            {Description}
            """;
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"ToolRecipe[Id={RecipeId} Tool={ToolName} DC={CraftDc} " +
               $"Attr={RequiredAttribute} Components={TotalComponentsRequired}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}

/// <summary>
/// Specifies a single component requirement for a recipe.
/// </summary>
/// <remarks>
/// <para>
/// Component requirements define what materials are needed and in what quantity.
/// </para>
/// <para>
/// Component matching is done by name first, then by type for substitution possibilities.
/// </para>
/// </remarks>
/// <param name="ComponentName">Display name of the component.</param>
/// <param name="ComponentType">Type classification of the component.</param>
/// <param name="Quantity">Number of this component needed.</param>
public readonly record struct ComponentRequirement(
    string ComponentName,
    ComponentType ComponentType,
    int Quantity)
{
    /// <summary>
    /// Returns a display string for the requirement.
    /// </summary>
    /// <returns>A formatted requirement string (e.g., "Scrap Metal ×3").</returns>
    public string ToDisplayString()
    {
        return $"{ComponentName} ×{Quantity}";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
