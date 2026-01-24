// ------------------------------------------------------------------------------
// <copyright file="IImprovisedToolCraftingService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for the improvised tool crafting system.
// Handles recipe lookup, component validation, crafting attempts, and tool usage.
// Part of v0.15.4g Improvised Tool Crafting System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service interface for the improvised tool crafting system.
/// Handles recipe lookup, component validation, crafting attempts, and tool usage.
/// </summary>
/// <remarks>
/// <para>
/// The improvised tool crafting system enables characters to create bypass tools
/// from salvaged components. This represents Aethelgard's cargo-cult approach to
/// Old World technology—survivors fashion crude but functional equipment from
/// incomprehensible machinery.
/// </para>
/// <para>
/// Key operations:
/// <list type="bullet">
///   <item><description>Recipe lookup: Get available tool recipes</description></item>
///   <item><description>Component validation: Check if crafting is possible</description></item>
///   <item><description>Crafting attempts: Roll to create tools with success/failure/fumble outcomes</description></item>
///   <item><description>Tool usage: Apply tool bonuses to bypass attempts</description></item>
///   <item><description>Salvage generation: Get components from critical bypass successes</description></item>
/// </list>
/// </para>
/// <para>
/// Crafting outcomes:
/// <list type="bullet">
///   <item><description>Critical Success (net ≥5): Quality tool (+2d10, 5 uses)</description></item>
///   <item><description>Success (net >0): Standard tool (+1d10, 3 uses)</description></item>
///   <item><description>Failure (net ≤0): Components consumed, no tool</description></item>
///   <item><description>Fumble (0 successes + botch): Components lost, 1d6 damage</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IImprovisedToolCraftingService
{
    // -------------------------------------------------------------------------
    // Recipe Queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets all available tool recipes.
    /// </summary>
    /// <returns>Collection of craftable tool recipes.</returns>
    /// <remarks>
    /// <para>
    /// Available recipes:
    /// <list type="bullet">
    ///   <item><description>Shim Picks: 3× Scrap Metal, DC 10, WITS or FINESSE</description></item>
    ///   <item><description>Wire Probe: 2× Copper Wire + 1× Handle, DC 12, WITS or FINESSE</description></item>
    ///   <item><description>Glitch Trigger: 1× Capacitor + 4× Wire, DC 14, WITS</description></item>
    ///   <item><description>Bypass Clamps: 4× Metal Clips, DC 12, FINESSE</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IReadOnlyList<ToolRecipe> GetAvailableRecipes();

    // -------------------------------------------------------------------------
    // Component Validation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Checks if a character has the components to craft a recipe.
    /// </summary>
    /// <param name="recipe">The recipe to check requirements for.</param>
    /// <param name="inventory">The character's component inventory.</param>
    /// <returns>Result indicating if crafting is possible and what's missing.</returns>
    /// <remarks>
    /// <para>
    /// Component matching:
    /// <list type="bullet">
    ///   <item><description>Exact name match preferred</description></item>
    ///   <item><description>Type match allows substitution (e.g., Metal for Misc)</description></item>
    ///   <item><description>Quantities must be satisfied</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method does not consume components—use <see cref="AttemptCraft"/>
    /// to actually attempt crafting and consume components.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when recipe or inventory is null.</exception>
    ComponentCheckResult CanCraft(ToolRecipe recipe, IReadOnlyList<CraftingComponent> inventory);

    // -------------------------------------------------------------------------
    // Crafting Operations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to craft an improvised tool.
    /// </summary>
    /// <param name="character">Character context with attributes and modifiers.</param>
    /// <param name="recipe">Recipe being crafted.</param>
    /// <param name="inventory">Available components (will be consumed on any attempt).</param>
    /// <returns>Result of the crafting attempt including tool created (if successful).</returns>
    /// <remarks>
    /// <para>
    /// Crafting procedure:
    /// <list type="number">
    ///   <item><description>Validate components are available</description></item>
    ///   <item><description>Calculate dice pool from attribute + modifiers</description></item>
    ///   <item><description>Roll crafting check vs. recipe DC</description></item>
    ///   <item><description>Determine outcome and create tool (if successful)</description></item>
    ///   <item><description>Consume components (on all outcomes except missing components)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Dice pool modifiers:
    /// <list type="bullet">
    ///   <item><description>+1d10: Has [Tinker's Toolkit]</description></item>
    ///   <item><description>+2d10: Has workshop access</description></item>
    ///   <item><description>-1d10: Under pressure (hostile environment or time limit)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when recipe or inventory is null.</exception>
    ImprovisedCraftResult AttemptCraft(
        CharacterCraftingContext character,
        ToolRecipe recipe,
        IReadOnlyList<CraftingComponent> inventory);

    // -------------------------------------------------------------------------
    // Tool Usage
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the dice pool bonus for using a tool on a specific bypass type.
    /// </summary>
    /// <param name="tool">The improvised tool.</param>
    /// <param name="bypassType">The type of bypass being attempted.</param>
    /// <returns>Dice pool bonus (0 if tool doesn't apply or is broken).</returns>
    /// <remarks>
    /// <para>
    /// Tool applicability:
    /// <list type="bullet">
    ///   <item><description>Shim Picks → Lockpicking</description></item>
    ///   <item><description>Wire Probe → Terminal Hacking</description></item>
    ///   <item><description>Glitch Trigger → Glitch Exploitation</description></item>
    ///   <item><description>Bypass Clamps → Terminal Hacking</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Returns 0 if:
    /// <list type="bullet">
    ///   <item><description>Tool type doesn't match bypass type</description></item>
    ///   <item><description>Tool is broken (UsesRemaining = 0)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    int GetToolBonus(ImprovisedTool tool, BypassType bypassType);

    /// <summary>
    /// Uses a tool for a bypass attempt, consuming one use and returning the modified tool.
    /// </summary>
    /// <param name="tool">The tool being used.</param>
    /// <param name="bypassType">The type of bypass being attempted.</param>
    /// <returns>Result including bonus applied, special effects, and modified tool state.</returns>
    /// <remarks>
    /// <para>
    /// Tool usage:
    /// <list type="bullet">
    ///   <item><description>Consumes one use of the tool</description></item>
    ///   <item><description>Returns modified tool with decremented uses</description></item>
    ///   <item><description>Returns null for ModifiedTool if tool breaks</description></item>
    ///   <item><description>Applies special effects if applicable</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Special effects:
    /// <list type="bullet">
    ///   <item><description>Glitch Trigger: Force mechanism into [Glitched] state</description></item>
    ///   <item><description>Bypass Clamps: Skip Layer 1 of terminal infiltration</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    ToolUsageResult UseTool(ImprovisedTool tool, BypassType bypassType);

    // -------------------------------------------------------------------------
    // Salvage Generation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets components that would be salvaged from a critical success on bypass.
    /// </summary>
    /// <param name="bypassType">Type of bypass that critically succeeded.</param>
    /// <param name="mechanismType">Type of mechanism bypassed (e.g., "lock", "terminal").</param>
    /// <returns>List of salvaged components.</returns>
    /// <remarks>
    /// <para>
    /// Salvage tables by bypass type:
    /// <list type="bullet">
    ///   <item><description>Lockpicking: High-Tension Spring, Scrap Metal (50% each)</description></item>
    ///   <item><description>Terminal Hacking: Circuit Fragment, Wire, Capacitor (16% chance for rare)</description></item>
    ///   <item><description>Trap Disarmament: Wire, Metal Clips</description></item>
    ///   <item><description>Glitch Exploitation: Circuit Fragment, Capacitor (50% chance for rare)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Component rarity:
    /// <list type="bullet">
    ///   <item><description>Common: Scrap Metal, Metal Clips, Wire, Copper Wire, Handle</description></item>
    ///   <item><description>Uncommon: High-Tension Spring, Circuit Fragment</description></item>
    ///   <item><description>Rare: Capacitor</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IReadOnlyList<CraftingComponent> GetSalvageFromCritical(BypassType bypassType, string mechanismType);
}

// =============================================================================
// Result Value Objects
// =============================================================================

/// <summary>
/// Character context for crafting checks.
/// Contains attributes and situational modifiers that affect crafting rolls.
/// </summary>
/// <remarks>
/// <para>
/// Attribute selection:
/// <list type="bullet">
///   <item><description>Shim Picks: Higher of WITS or FINESSE</description></item>
///   <item><description>Wire Probe: Higher of WITS or FINESSE</description></item>
///   <item><description>Glitch Trigger: WITS only</description></item>
///   <item><description>Bypass Clamps: FINESSE only</description></item>
/// </list>
/// </para>
/// <para>
/// Modifiers:
/// <list type="bullet">
///   <item><description>Tinker's Toolkit: +1d10 to crafting</description></item>
///   <item><description>Workshop Access: +2d10 to crafting</description></item>
///   <item><description>Under Pressure: -1d10 to crafting</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Wits">Character's WITS attribute score.</param>
/// <param name="Finesse">Character's FINESSE attribute score.</param>
/// <param name="HasTinkersToolkit">True if character has [Tinker's Toolkit] equipment.</param>
/// <param name="HasWorkshopAccess">True if in a proper workshop environment.</param>
/// <param name="IsUnderPressure">True if time pressure or hostile environment.</param>
public readonly record struct CharacterCraftingContext(
    int Wits,
    int Finesse,
    bool HasTinkersToolkit,
    bool HasWorkshopAccess,
    bool IsUnderPressure)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the higher of WITS or FINESSE for recipes requiring either.
    /// </summary>
    /// <remarks>
    /// Used for Shim Picks (DC 10) and Wire Probe (DC 12) recipes.
    /// </remarks>
    public int HigherAttribute => Math.Max(Wits, Finesse);

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a basic context with just attributes (no modifiers).
    /// </summary>
    /// <param name="wits">Character's WITS attribute.</param>
    /// <param name="finesse">Character's FINESSE attribute.</param>
    /// <returns>A new context with default modifier values.</returns>
    /// <remarks>
    /// Useful for testing or when modifiers are not applicable.
    /// </remarks>
    public static CharacterCraftingContext CreateBasic(int wits, int finesse)
    {
        return new CharacterCraftingContext(
            Wits: wits,
            Finesse: finesse,
            HasTinkersToolkit: false,
            HasWorkshopAccess: false,
            IsUnderPressure: false);
    }

    /// <summary>
    /// Creates a context with optimal crafting conditions.
    /// </summary>
    /// <param name="wits">Character's WITS attribute.</param>
    /// <param name="finesse">Character's FINESSE attribute.</param>
    /// <returns>A new context with toolkit and workshop access.</returns>
    /// <remarks>
    /// Grants +3d10 total bonus (+1 toolkit, +2 workshop).
    /// </remarks>
    public static CharacterCraftingContext CreateOptimal(int wits, int finesse)
    {
        return new CharacterCraftingContext(
            Wits: wits,
            Finesse: finesse,
            HasTinkersToolkit: true,
            HasWorkshopAccess: true,
            IsUnderPressure: false);
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a display string for the crafting context.
    /// </summary>
    /// <returns>A formatted description of attributes and modifiers.</returns>
    public string ToDisplayString()
    {
        var modifiers = new List<string>();
        if (HasTinkersToolkit)
        {
            modifiers.Add("+1d10 (Tinker's Toolkit)");
        }

        if (HasWorkshopAccess)
        {
            modifiers.Add("+2d10 (Workshop)");
        }

        if (IsUnderPressure)
        {
            modifiers.Add("-1d10 (Under Pressure)");
        }

        var modifierText = modifiers.Count > 0
            ? string.Join(", ", modifiers)
            : "None";

        return $"WITS: {Wits}, FINESSE: {Finesse}, Modifiers: {modifierText}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"CraftingContext[Wits={Wits} Finesse={Finesse} " +
               $"Toolkit={HasTinkersToolkit} Workshop={HasWorkshopAccess} " +
               $"Pressure={IsUnderPressure}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}

/// <summary>
/// Result of checking if a recipe can be crafted with available components.
/// </summary>
/// <remarks>
/// <para>
/// This result is returned by <see cref="IImprovisedToolCraftingService.CanCraft"/>
/// to indicate whether crafting is possible and what components are missing.
/// </para>
/// <para>
/// Properties:
/// <list type="bullet">
///   <item><description>CanCraft: True if all required components are available</description></item>
///   <item><description>MissingComponents: List of what's needed (empty if CanCraft is true)</description></item>
///   <item><description>MatchedComponents: Components that will be consumed</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="CanCraft">True if all components are available.</param>
/// <param name="MissingComponents">List of missing components with quantities needed.</param>
/// <param name="MatchedComponents">Components from inventory that will be used.</param>
public readonly record struct ComponentCheckResult(
    bool CanCraft,
    IReadOnlyList<ComponentRequirement> MissingComponents,
    IReadOnlyList<CraftingComponent> MatchedComponents)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the total number of missing components across all requirements.
    /// </summary>
    public int TotalMissingCount => MissingComponents.Sum(m => m.Quantity);

    /// <summary>
    /// Gets a value indicating whether any rare components are matched.
    /// </summary>
    public bool IncludesRareComponents => MatchedComponents.Any(c => c.IsRare);

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful check result (all components available).
    /// </summary>
    /// <param name="matchedComponents">The components that will be used.</param>
    /// <returns>A successful component check result.</returns>
    public static ComponentCheckResult CreateSuccess(IReadOnlyList<CraftingComponent> matchedComponents)
    {
        return new ComponentCheckResult(
            CanCraft: true,
            MissingComponents: Array.Empty<ComponentRequirement>(),
            MatchedComponents: matchedComponents);
    }

    /// <summary>
    /// Creates a failed check result (missing components).
    /// </summary>
    /// <param name="missingComponents">The missing component requirements.</param>
    /// <param name="partialMatches">Any components that were matched (but not enough).</param>
    /// <returns>A failed component check result.</returns>
    public static ComponentCheckResult CreateFailure(
        IReadOnlyList<ComponentRequirement> missingComponents,
        IReadOnlyList<CraftingComponent> partialMatches)
    {
        return new ComponentCheckResult(
            CanCraft: false,
            MissingComponents: missingComponents,
            MatchedComponents: partialMatches);
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a display string for the check result.
    /// </summary>
    /// <returns>A formatted description of the check result.</returns>
    public string ToDisplayString()
    {
        if (CanCraft)
        {
            var componentList = string.Join(", ",
                MatchedComponents.Select(c => c.ComponentName));
            return $"Ready to craft with: {componentList}";
        }

        var missingList = string.Join(", ",
            MissingComponents.Select(m => m.ToDisplayString()));
        return $"Missing components: {missingList}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"ComponentCheckResult[CanCraft={CanCraft} " +
               $"Missing={TotalMissingCount} Matched={MatchedComponents.Count}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}

/// <summary>
/// Result of a crafting attempt.
/// Contains outcome, created tool (if successful), consumed components, and narrative.
/// </summary>
/// <remarks>
/// <para>
/// Possible outcomes:
/// <list type="bullet">
///   <item><description>Success + IsCritical: Quality tool created (+2d10, 5 uses)</description></item>
///   <item><description>Success: Standard tool created (+1d10, 3 uses)</description></item>
///   <item><description>Failure: Components consumed, no tool created</description></item>
///   <item><description>IsFumble: Components consumed, 1d6 damage taken</description></item>
/// </list>
/// </para>
/// <para>
/// Note: Components are consumed on all outcomes except when the check fails
/// due to missing components (which is detected before rolling).
/// </para>
/// </remarks>
/// <param name="Success">True if crafting succeeded and a tool was created.</param>
/// <param name="IsCritical">True if crafting critically succeeded (quality tool).</param>
/// <param name="IsFumble">True if crafting fumbled (damage + components lost).</param>
/// <param name="ToolCreated">The created tool (null if failed or fumbled).</param>
/// <param name="ComponentsConsumed">Components that were used or lost.</param>
/// <param name="FumbleDamage">Damage taken on fumble (0 otherwise).</param>
/// <param name="NarrativeText">Descriptive text for the result.</param>
public readonly record struct ImprovisedCraftResult(
    bool Success,
    bool IsCritical,
    bool IsFumble,
    ImprovisedTool? ToolCreated,
    IReadOnlyList<CraftingComponent> ComponentsConsumed,
    int FumbleDamage,
    string NarrativeText)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this result has any negative consequences.
    /// </summary>
    public bool HasNegativeConsequence => IsFumble || (!Success && ComponentsConsumed.Count > 0);

    /// <summary>
    /// Gets a value indicating whether a quality tool was created.
    /// </summary>
    public bool IsQualityToolCreated => Success && IsCritical && ToolCreated?.IsQuality == true;

    /// <summary>
    /// Gets the total number of components consumed.
    /// </summary>
    public int TotalComponentsConsumed => ComponentsConsumed.Count;

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a success result with a standard tool.
    /// </summary>
    /// <param name="tool">The created tool.</param>
    /// <param name="consumedComponents">Components that were used.</param>
    /// <param name="narrativeText">Descriptive text.</param>
    /// <returns>A successful craft result.</returns>
    public static ImprovisedCraftResult CreateSuccess(
        ImprovisedTool tool,
        IReadOnlyList<CraftingComponent> consumedComponents,
        string narrativeText)
    {
        return new ImprovisedCraftResult(
            Success: true,
            IsCritical: false,
            IsFumble: false,
            ToolCreated: tool,
            ComponentsConsumed: consumedComponents,
            FumbleDamage: 0,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Creates a critical success result with a quality tool.
    /// </summary>
    /// <param name="tool">The created quality tool.</param>
    /// <param name="consumedComponents">Components that were used.</param>
    /// <param name="narrativeText">Descriptive text.</param>
    /// <returns>A critical success craft result.</returns>
    public static ImprovisedCraftResult CreateCriticalSuccess(
        ImprovisedTool tool,
        IReadOnlyList<CraftingComponent> consumedComponents,
        string narrativeText)
    {
        return new ImprovisedCraftResult(
            Success: true,
            IsCritical: true,
            IsFumble: false,
            ToolCreated: tool,
            ComponentsConsumed: consumedComponents,
            FumbleDamage: 0,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Creates a failure result (components lost, no tool).
    /// </summary>
    /// <param name="consumedComponents">Components that were lost.</param>
    /// <param name="narrativeText">Descriptive text.</param>
    /// <returns>A failed craft result.</returns>
    public static ImprovisedCraftResult CreateFailure(
        IReadOnlyList<CraftingComponent> consumedComponents,
        string narrativeText)
    {
        return new ImprovisedCraftResult(
            Success: false,
            IsCritical: false,
            IsFumble: false,
            ToolCreated: null,
            ComponentsConsumed: consumedComponents,
            FumbleDamage: 0,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Creates a fumble result (components lost, damage taken).
    /// </summary>
    /// <param name="consumedComponents">Components that were lost.</param>
    /// <param name="damage">Damage taken from the fumble.</param>
    /// <param name="narrativeText">Descriptive text.</param>
    /// <returns>A fumbled craft result.</returns>
    public static ImprovisedCraftResult CreateFumble(
        IReadOnlyList<CraftingComponent> consumedComponents,
        int damage,
        string narrativeText)
    {
        return new ImprovisedCraftResult(
            Success: false,
            IsCritical: false,
            IsFumble: true,
            ToolCreated: null,
            ComponentsConsumed: consumedComponents,
            FumbleDamage: damage,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Creates a result indicating missing components (no roll attempted).
    /// </summary>
    /// <param name="narrativeText">Descriptive text about missing components.</param>
    /// <returns>A result with no components consumed.</returns>
    public static ImprovisedCraftResult CreateMissingComponents(string narrativeText)
    {
        return new ImprovisedCraftResult(
            Success: false,
            IsCritical: false,
            IsFumble: false,
            ToolCreated: null,
            ComponentsConsumed: Array.Empty<CraftingComponent>(),
            FumbleDamage: 0,
            NarrativeText: narrativeText);
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a display string for the craft result.
    /// </summary>
    /// <returns>A formatted description of the result.</returns>
    public string ToDisplayString()
    {
        if (IsFumble)
        {
            return $"[FUMBLE] {NarrativeText} ({FumbleDamage} damage taken)";
        }

        if (Success)
        {
            var qualityTag = IsCritical ? " [Quality]" : "";
            return $"[SUCCESS{qualityTag}] {NarrativeText}";
        }

        return $"[FAILURE] {NarrativeText}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"CraftResult[Success={Success} Critical={IsCritical} Fumble={IsFumble} " +
               $"Tool={ToolCreated?.ToolName ?? "none"} Consumed={TotalComponentsConsumed} " +
               $"Damage={FumbleDamage}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}

/// <summary>
/// Result of using an improvised tool for a bypass attempt.
/// </summary>
/// <remarks>
/// <para>
/// Tool usage effects:
/// <list type="bullet">
///   <item><description>BonusApplied: +1d10 (standard) or +2d10 (quality) to bypass roll</description></item>
///   <item><description>Special effects may trigger for certain tools</description></item>
///   <item><description>One use is consumed from the tool</description></item>
///   <item><description>Tool breaks when last use is consumed</description></item>
/// </list>
/// </para>
/// <para>
/// Special effects by tool:
/// <list type="bullet">
///   <item><description>Glitch Trigger: Force mechanism into [Glitched] state</description></item>
///   <item><description>Bypass Clamps: Skip Layer 1 of terminal infiltration</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="BonusApplied">The dice pool bonus that was applied.</param>
/// <param name="SpecialEffectTriggered">True if a special effect was activated.</param>
/// <param name="SpecialEffectDescription">Description of the special effect (if any).</param>
/// <param name="ModifiedTool">The tool after use (null if broken).</param>
/// <param name="ToolBroke">True if the tool broke from this use.</param>
/// <param name="NarrativeText">Descriptive text about the tool usage.</param>
public readonly record struct ToolUsageResult(
    int BonusApplied,
    bool SpecialEffectTriggered,
    string? SpecialEffectDescription,
    ImprovisedTool? ModifiedTool,
    bool ToolBroke,
    string NarrativeText)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether any bonus was applied.
    /// </summary>
    public bool HasBonus => BonusApplied > 0;

    /// <summary>
    /// Gets the remaining uses on the tool (0 if broken or null).
    /// </summary>
    public int RemainingUses => ModifiedTool?.UsesRemaining ?? 0;

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a result for successful tool usage.
    /// </summary>
    /// <param name="bonus">The bonus applied.</param>
    /// <param name="modifiedTool">The tool after use.</param>
    /// <param name="narrativeText">Descriptive text.</param>
    /// <returns>A tool usage result.</returns>
    public static ToolUsageResult Create(
        int bonus,
        ImprovisedTool? modifiedTool,
        string narrativeText)
    {
        return new ToolUsageResult(
            BonusApplied: bonus,
            SpecialEffectTriggered: false,
            SpecialEffectDescription: null,
            ModifiedTool: modifiedTool,
            ToolBroke: modifiedTool == null,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Creates a result for tool usage with a special effect.
    /// </summary>
    /// <param name="bonus">The bonus applied.</param>
    /// <param name="modifiedTool">The tool after use.</param>
    /// <param name="specialEffect">Description of the special effect.</param>
    /// <param name="narrativeText">Descriptive text.</param>
    /// <returns>A tool usage result with special effect.</returns>
    public static ToolUsageResult CreateWithSpecialEffect(
        int bonus,
        ImprovisedTool? modifiedTool,
        string specialEffect,
        string narrativeText)
    {
        return new ToolUsageResult(
            BonusApplied: bonus,
            SpecialEffectTriggered: true,
            SpecialEffectDescription: specialEffect,
            ModifiedTool: modifiedTool,
            ToolBroke: modifiedTool == null,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Creates a result indicating the tool doesn't apply to this bypass type.
    /// </summary>
    /// <param name="tool">The tool that was attempted.</param>
    /// <param name="bypassType">The bypass type attempted.</param>
    /// <returns>A result with no bonus or effect.</returns>
    public static ToolUsageResult CreateNotApplicable(ImprovisedTool tool, BypassType bypassType)
    {
        return new ToolUsageResult(
            BonusApplied: 0,
            SpecialEffectTriggered: false,
            SpecialEffectDescription: null,
            ModifiedTool: tool,
            ToolBroke: false,
            NarrativeText: $"Your {tool.ToolName} doesn't help with {bypassType}.");
    }

    /// <summary>
    /// Creates a result indicating the tool is already broken.
    /// </summary>
    /// <param name="tool">The broken tool.</param>
    /// <returns>A result indicating the tool cannot be used.</returns>
    public static ToolUsageResult CreateBroken(ImprovisedTool tool)
    {
        return new ToolUsageResult(
            BonusApplied: 0,
            SpecialEffectTriggered: false,
            SpecialEffectDescription: null,
            ModifiedTool: null,
            ToolBroke: true,
            NarrativeText: $"Your {tool.ToolName} is broken and cannot be used.");
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a display string for the usage result.
    /// </summary>
    /// <returns>A formatted description of the result.</returns>
    public string ToDisplayString()
    {
        var lines = new List<string> { NarrativeText };

        if (HasBonus)
        {
            lines.Add($"Bonus: +{BonusApplied}d10");
        }

        if (SpecialEffectTriggered && !string.IsNullOrEmpty(SpecialEffectDescription))
        {
            lines.Add($"Special Effect: {SpecialEffectDescription}");
        }

        if (ToolBroke)
        {
            lines.Add("[Tool broke!]");
        }
        else if (ModifiedTool.HasValue)
        {
            lines.Add($"Uses remaining: {ModifiedTool.Value.UsesRemaining}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"ToolUsageResult[Bonus=+{BonusApplied}d10 Special={SpecialEffectTriggered} " +
               $"Broke={ToolBroke} Remaining={RemainingUses}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
