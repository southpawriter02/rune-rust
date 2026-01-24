// ------------------------------------------------------------------------------
// <copyright file="ImprovisedToolCraftingService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service for handling improvised tool crafting operations.
// Manages recipe lookup, component validation, crafting attempts, and tool usage
// for the Aethelgard scavenger crafting system.
// Part of v0.15.4g Improvised Tool Crafting System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for handling improvised tool crafting operations.
/// </summary>
/// <remarks>
/// <para>
/// The improvised tool crafting system enables characters to create bypass tools
/// from salvaged components. This represents Aethelgard's cargo-cult approach to
/// Old World technology—survivors fashion crude but functional equipment from
/// incomprehensible machinery.
/// </para>
/// <para>
/// This service manages:
/// <list type="bullet">
///   <item><description>Recipe lookup and validation</description></item>
///   <item><description>Component matching and availability checks</description></item>
///   <item><description>Crafting attempts with dice rolls and modifiers</description></item>
///   <item><description>Tool usage with durability tracking</description></item>
///   <item><description>Salvage generation from critical bypass successes</description></item>
/// </list>
/// </para>
/// <para>
/// Crafting outcomes:
/// <list type="bullet">
///   <item><description>Critical Success (net ≥5): Quality tool (+2d10, 5 uses)</description></item>
///   <item><description>Success (net &gt;0): Standard tool (+1d10, 3 uses)</description></item>
///   <item><description>Failure (net ≤0): Components consumed, no tool created</description></item>
///   <item><description>Fumble (0 successes + botch): Components lost, 1d6 damage</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ImprovisedToolCraftingService : IImprovisedToolCraftingService
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// Dice pool bonus for having [Tinker's Toolkit].
    /// </summary>
    private const int TinkersToolkitBonus = 1;

    /// <summary>
    /// Dice pool bonus for workshop access.
    /// </summary>
    private const int WorkshopBonus = 2;

    /// <summary>
    /// Dice pool penalty for being under pressure.
    /// </summary>
    private const int PressurePenalty = 1;

    /// <summary>
    /// Damage dice on fumble (1d6).
    /// </summary>
    private const int FumbleDamageDieSize = 6;

    /// <summary>
    /// Net successes threshold for critical success (quality tool).
    /// </summary>
    private const int CriticalSuccessThreshold = 5;

    /// <summary>
    /// Minimum dice in crafting pool.
    /// </summary>
    private const int MinimumDicePool = 1;

    // -------------------------------------------------------------------------
    // Salvage Narratives
    // -------------------------------------------------------------------------

    /// <summary>
    /// Narrative text for salvage from different bypass types.
    /// </summary>
    private static readonly IReadOnlyDictionary<BypassType, string> SalvageNarratives =
        new Dictionary<BypassType, string>
        {
            { BypassType.Lockpicking, "As you work the lock, you recognize useful springs and metal pieces that can be salvaged." },
            { BypassType.TerminalHacking, "While navigating the terminal circuits, you identify recoverable components in the wiring." },
            { BypassType.TrapDisarmament, "Disarming the trap reveals salvageable wire and metal clips within its mechanism." },
            { BypassType.GlitchExploitation, "The glitched device yields fragments of circuitry that survived its corruption." }
        };

    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------

    private readonly IDiceService _diceService;
    private readonly ILogger<ImprovisedToolCraftingService> _logger;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ImprovisedToolCraftingService"/> class.
    /// </summary>
    /// <param name="diceService">Service for rolling dice.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public ImprovisedToolCraftingService(
        IDiceService diceService,
        ILogger<ImprovisedToolCraftingService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("ImprovisedToolCraftingService initialized");
    }

    // -------------------------------------------------------------------------
    // Recipe Queries
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public IReadOnlyList<ToolRecipe> GetAvailableRecipes()
    {
        _logger.LogDebug("Getting available recipes: Count={Count}", ToolRecipe.AllRecipes.Count);

        return ToolRecipe.AllRecipes;
    }

    // -------------------------------------------------------------------------
    // Component Validation
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public ComponentCheckResult CanCraft(ToolRecipe recipe, IReadOnlyList<CraftingComponent> inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        _logger.LogDebug(
            "Checking components for {Recipe}: InventoryCount={InventoryCount}, RequiredComponents={RequiredCount}",
            recipe.ToolName,
            inventory.Count,
            recipe.RequiredComponents.Count);

        var missing = new List<ComponentRequirement>();
        var matched = new List<CraftingComponent>();
        var remainingInventory = new List<CraftingComponent>(inventory);

        // Process each requirement
        foreach (var requirement in recipe.RequiredComponents)
        {
            var available = FindMatchingComponents(
                remainingInventory,
                requirement,
                out var usedIndices);

            if (available.Count < requirement.Quantity)
            {
                // Not enough components - record what's missing
                var shortfall = requirement.Quantity - available.Count;
                missing.Add(requirement with { Quantity = shortfall });

                _logger.LogDebug(
                    "Missing component for {Recipe}: {ComponentName} - need {Shortfall} more",
                    recipe.ToolName,
                    requirement.ComponentName,
                    shortfall);
            }

            // Add matched components and remove them from remaining inventory
            matched.AddRange(available);
            foreach (var index in usedIndices.OrderByDescending(i => i))
            {
                remainingInventory.RemoveAt(index);
            }
        }

        var canCraft = missing.Count == 0;

        _logger.LogInformation(
            "Component check for {Recipe}: CanCraft={CanCraft}, Missing={MissingCount}, Matched={MatchedCount}",
            recipe.ToolName,
            canCraft,
            missing.Count,
            matched.Count);

        return new ComponentCheckResult(
            CanCraft: canCraft,
            MissingComponents: missing.AsReadOnly(),
            MatchedComponents: matched.AsReadOnly());
    }

    // -------------------------------------------------------------------------
    // Crafting Operations
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public ImprovisedCraftResult AttemptCraft(
        CharacterCraftingContext character,
        ToolRecipe recipe,
        IReadOnlyList<CraftingComponent> inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        _logger.LogInformation(
            "Attempting craft: Recipe={Recipe}, Character={Character}",
            recipe.ToolName,
            character.ToLogString());

        // First check components
        var componentCheck = CanCraft(recipe, inventory);
        if (!componentCheck.CanCraft)
        {
            _logger.LogWarning(
                "Craft attempt blocked - missing components for {Recipe}: {MissingList}",
                recipe.ToolName,
                string.Join(", ", componentCheck.MissingComponents.Select(m => m.ToDisplayString())));

            return ImprovisedCraftResult.CreateMissingComponents(
                "You lack the required components to craft this tool.");
        }

        // Calculate dice pool
        var attribute = GetCraftingAttribute(character, recipe.RequiredAttribute);
        var dicePool = CalculateDicePool(character, attribute);

        _logger.LogDebug(
            "Crafting {Recipe}: BaseAttribute={Attribute}, TotalPool={Pool}, DC={DC}",
            recipe.ToolName,
            attribute,
            dicePool,
            recipe.CraftDc);

        // Roll crafting check
        var rollResult = _diceService.Roll(DicePool.D10(dicePool));

        _logger.LogDebug(
            "Craft roll result: Successes={Successes}, Botches={Botches}, Net={Net}, IsFumble={IsFumble}",
            rollResult.TotalSuccesses,
            rollResult.TotalBotches,
            rollResult.NetSuccesses,
            rollResult.IsFumble);

        // Handle fumble (0 successes + at least 1 botch)
        if (rollResult.IsFumble)
        {
            return HandleCraftingFumble(recipe, componentCheck.MatchedComponents);
        }

        // Handle failure (net successes <= 0)
        if (rollResult.NetSuccesses <= 0)
        {
            return HandleCraftingFailure(recipe, componentCheck.MatchedComponents, rollResult.NetSuccesses);
        }

        // Handle success
        var isCritical = rollResult.NetSuccesses >= CriticalSuccessThreshold;
        return HandleCraftingSuccess(recipe, componentCheck.MatchedComponents, rollResult.NetSuccesses, isCritical);
    }

    // -------------------------------------------------------------------------
    // Tool Usage
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public int GetToolBonus(ImprovisedTool tool, BypassType bypassType)
    {
        if (!tool.IsUsable)
        {
            _logger.LogDebug(
                "Tool bonus check: {Tool} is broken, returning 0",
                tool.ToolName);
            return 0;
        }

        if (tool.AssistsWithBypassType != bypassType)
        {
            _logger.LogDebug(
                "Tool bonus check: {Tool} does not assist with {BypassType}, returning 0",
                tool.ToolName,
                bypassType);
            return 0;
        }

        _logger.LogDebug(
            "Tool bonus check: {Tool} provides +{Bonus}d10 for {BypassType}",
            tool.ToolName,
            tool.BonusAmount,
            bypassType);

        return tool.BonusAmount;
    }

    /// <inheritdoc />
    public ToolUsageResult UseTool(ImprovisedTool tool, BypassType bypassType)
    {
        _logger.LogInformation(
            "Using tool: {Tool} for {BypassType}, UsesRemaining={Uses}",
            tool.ToolName,
            bypassType,
            tool.UsesRemaining);

        // Check if tool is already broken
        if (!tool.IsUsable)
        {
            _logger.LogWarning(
                "Tool usage blocked: {Tool} is broken",
                tool.ToolName);

            return ToolUsageResult.CreateBroken(tool);
        }

        // Check if tool applies to this bypass type
        var bonus = GetToolBonus(tool, bypassType);
        if (bonus == 0)
        {
            _logger.LogDebug(
                "Tool usage: {Tool} does not apply to {BypassType}",
                tool.ToolName,
                bypassType);

            return ToolUsageResult.CreateNotApplicable(tool, bypassType);
        }

        // Consume one use
        var modifiedTool = tool.UseOnce();
        var broke = modifiedTool == null;

        _logger.LogInformation(
            "Tool used: {Tool}, Bonus=+{Bonus}d10, Broke={Broke}, RemainingUses={Uses}",
            tool.ToolName,
            bonus,
            broke,
            modifiedTool?.UsesRemaining ?? 0);

        // Check for special effect
        if (tool.HasSpecialEffect)
        {
            _logger.LogInformation(
                "Special effect triggered: {Tool} - {Effect}",
                tool.ToolName,
                tool.SpecialEffect);

            return ToolUsageResult.CreateWithSpecialEffect(
                bonus,
                modifiedTool,
                tool.SpecialEffect!,
                GenerateUsageNarrative(tool, bonus, broke, hasSpecialEffect: true));
        }

        return ToolUsageResult.Create(
            bonus,
            modifiedTool,
            GenerateUsageNarrative(tool, bonus, broke, hasSpecialEffect: false));
    }

    // -------------------------------------------------------------------------
    // Salvage Generation
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public IReadOnlyList<CraftingComponent> GetSalvageFromCritical(
        BypassType bypassType,
        string mechanismType)
    {
        _logger.LogInformation(
            "Generating salvage: BypassType={BypassType}, MechanismType={MechanismType}",
            bypassType,
            mechanismType);

        var components = new List<CraftingComponent>();
        var source = $"salvaged from {mechanismType}";

        // Determine salvage based on bypass type
        switch (bypassType)
        {
            case BypassType.Lockpicking:
                components.Add(CraftingComponent.CreateHighTensionSpring(source));
                // 50% chance for Scrap Metal (roll 1d4 >= 3)
                if (RollD4() >= 3)
                {
                    components.Add(CraftingComponent.CreateScrapMetal(source));
                }

                break;

            case BypassType.TerminalHacking:
                components.Add(CraftingComponent.CreateCircuitFragment(source));
                components.Add(CraftingComponent.CreateWire(source));
                // 16% chance for Capacitor (rare) - roll 1d6 == 6
                if (RollD6() == 6)
                {
                    components.Add(CraftingComponent.CreateCapacitor(source));
                    _logger.LogInformation(
                        "Rare salvage! Capacitor acquired from {MechanismType}",
                        mechanismType);
                }

                break;

            case BypassType.TrapDisarmament:
                components.Add(CraftingComponent.CreateWire(source));
                components.Add(CraftingComponent.CreateMetalClips(source));
                break;

            case BypassType.GlitchExploitation:
                components.Add(CraftingComponent.CreateCircuitFragment(source));
                // 50% chance for Capacitor (roll 1d4 >= 3)
                if (RollD4() >= 3)
                {
                    components.Add(CraftingComponent.CreateCapacitor(source));
                    _logger.LogInformation(
                        "Rare salvage! Capacitor acquired from glitched {MechanismType}",
                        mechanismType);
                }

                break;

            default:
                // Default fallback - basic scrap
                components.Add(CraftingComponent.CreateScrapMetal(source));
                break;
        }

        _logger.LogInformation(
            "Salvage generated: BypassType={BypassType}, Components=[{ComponentList}]",
            bypassType,
            string.Join(", ", components.Select(c => c.ComponentName)));

        return components.AsReadOnly();
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Component Matching
    // -------------------------------------------------------------------------

    /// <summary>
    /// Finds components in inventory that match a requirement.
    /// </summary>
    /// <param name="inventory">Available components.</param>
    /// <param name="requirement">The requirement to match.</param>
    /// <param name="usedIndices">Output parameter for indices of used components.</param>
    /// <returns>List of matching components (up to required quantity).</returns>
    /// <remarks>
    /// <para>
    /// Component matching priority:
    /// <list type="number">
    ///   <item><description>Exact name match</description></item>
    ///   <item><description>Type match with substitution allowed</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private static List<CraftingComponent> FindMatchingComponents(
        List<CraftingComponent> inventory,
        ComponentRequirement requirement,
        out List<int> usedIndices)
    {
        usedIndices = new List<int>();
        var matched = new List<CraftingComponent>();

        // First pass: exact name matches
        for (var i = 0; i < inventory.Count && matched.Count < requirement.Quantity; i++)
        {
            if (usedIndices.Contains(i))
            {
                continue;
            }

            if (inventory[i].ComponentName == requirement.ComponentName)
            {
                matched.Add(inventory[i]);
                usedIndices.Add(i);
            }
        }

        // Second pass: type matches with substitution
        for (var i = 0; i < inventory.Count && matched.Count < requirement.Quantity; i++)
        {
            if (usedIndices.Contains(i))
            {
                continue;
            }

            if (inventory[i].CanSubstituteFor(requirement.ComponentType))
            {
                matched.Add(inventory[i]);
                usedIndices.Add(i);
            }
        }

        return matched;
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Attribute and Dice Calculation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the appropriate attribute value for crafting based on recipe requirements.
    /// </summary>
    /// <param name="character">Character crafting context.</param>
    /// <param name="required">Required attribute for the recipe.</param>
    /// <returns>The attribute value to use for the crafting check.</returns>
    private int GetCraftingAttribute(CharacterCraftingContext character, CraftingAttribute required)
    {
        return required switch
        {
            CraftingAttribute.Wits => character.Wits,
            CraftingAttribute.Finesse => character.Finesse,
            CraftingAttribute.WitsOrFinesse => character.HigherAttribute,
            _ => character.HigherAttribute
        };
    }

    /// <summary>
    /// Calculates the total dice pool for a crafting attempt.
    /// </summary>
    /// <param name="character">Character crafting context.</param>
    /// <param name="baseAttribute">Base attribute value.</param>
    /// <returns>Total dice pool size (minimum 1).</returns>
    /// <remarks>
    /// <para>
    /// Dice pool modifiers:
    /// <list type="bullet">
    ///   <item><description>+1: Tinker's Toolkit</description></item>
    ///   <item><description>+2: Workshop Access</description></item>
    ///   <item><description>-1: Under Pressure</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private int CalculateDicePool(CharacterCraftingContext character, int baseAttribute)
    {
        var pool = baseAttribute;

        if (character.HasTinkersToolkit)
        {
            pool += TinkersToolkitBonus;
            _logger.LogDebug("Tinker's Toolkit bonus applied: +{Bonus}d10", TinkersToolkitBonus);
        }

        if (character.HasWorkshopAccess)
        {
            pool += WorkshopBonus;
            _logger.LogDebug("Workshop bonus applied: +{Bonus}d10", WorkshopBonus);
        }

        if (character.IsUnderPressure)
        {
            pool -= PressurePenalty;
            _logger.LogDebug("Pressure penalty applied: -{Penalty}d10", PressurePenalty);
        }

        return Math.Max(MinimumDicePool, pool);
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Outcome Handling
    // -------------------------------------------------------------------------

    /// <summary>
    /// Handles a fumble outcome during crafting.
    /// </summary>
    /// <param name="recipe">The recipe being crafted.</param>
    /// <param name="consumedComponents">Components that will be lost.</param>
    /// <returns>A fumble craft result with damage.</returns>
    private ImprovisedCraftResult HandleCraftingFumble(
        ToolRecipe recipe,
        IReadOnlyList<CraftingComponent> consumedComponents)
    {
        // Roll 1d6 for fumble damage
        var damage = RollD6();

        _logger.LogWarning(
            "Crafting FUMBLE for {Recipe}: Components lost, {Damage} damage taken",
            recipe.ToolName,
            damage);

        return ImprovisedCraftResult.CreateFumble(
            consumedComponents,
            damage,
            GenerateFumbleNarrative(recipe, damage));
    }

    /// <summary>
    /// Handles a failure outcome during crafting.
    /// </summary>
    /// <param name="recipe">The recipe being crafted.</param>
    /// <param name="consumedComponents">Components that will be lost.</param>
    /// <param name="netSuccesses">Net successes from the roll (≤0).</param>
    /// <returns>A failure craft result.</returns>
    private ImprovisedCraftResult HandleCraftingFailure(
        ToolRecipe recipe,
        IReadOnlyList<CraftingComponent> consumedComponents,
        int netSuccesses)
    {
        _logger.LogInformation(
            "Crafting FAILED for {Recipe}: NetSuccesses={Net}, Components lost",
            recipe.ToolName,
            netSuccesses);

        return ImprovisedCraftResult.CreateFailure(
            consumedComponents,
            GenerateFailureNarrative(recipe));
    }

    /// <summary>
    /// Handles a success outcome during crafting.
    /// </summary>
    /// <param name="recipe">The recipe being crafted.</param>
    /// <param name="consumedComponents">Components that were used.</param>
    /// <param name="netSuccesses">Net successes from the roll.</param>
    /// <param name="isCritical">Whether this is a critical success (quality tool).</param>
    /// <returns>A success craft result with the created tool.</returns>
    private ImprovisedCraftResult HandleCraftingSuccess(
        ToolRecipe recipe,
        IReadOnlyList<CraftingComponent> consumedComponents,
        int netSuccesses,
        bool isCritical)
    {
        var tool = CreateTool(recipe.ResultingToolType, isCritical);

        _logger.LogInformation(
            "Crafting SUCCESS for {Recipe}: Critical={Critical}, Tool={Tool}, NetSuccesses={Net}",
            recipe.ToolName,
            isCritical,
            tool.ToolName,
            netSuccesses);

        if (isCritical)
        {
            return ImprovisedCraftResult.CreateCriticalSuccess(
                tool,
                consumedComponents,
                GenerateSuccessNarrative(recipe, tool, isCritical: true));
        }

        return ImprovisedCraftResult.CreateSuccess(
            tool,
            consumedComponents,
            GenerateSuccessNarrative(recipe, tool, isCritical: false));
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Tool Creation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a tool of the specified type.
    /// </summary>
    /// <param name="toolType">Type of tool to create.</param>
    /// <param name="isQuality">Whether to create a quality tool.</param>
    /// <returns>A new improvised tool.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for unknown tool types.</exception>
    private static ImprovisedTool CreateTool(ImprovisedToolType toolType, bool isQuality)
    {
        return toolType switch
        {
            ImprovisedToolType.ShimPicks => ImprovisedTool.CreateShimPicks(isQuality),
            ImprovisedToolType.WireProbe => ImprovisedTool.CreateWireProbe(isQuality),
            ImprovisedToolType.GlitchTrigger => ImprovisedTool.CreateGlitchTrigger(isQuality),
            ImprovisedToolType.BypassClamps => ImprovisedTool.CreateBypassClamps(isQuality),
            _ => throw new ArgumentOutOfRangeException(
                nameof(toolType),
                toolType,
                "Unknown improvised tool type.")
        };
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Dice Rolling
    // -------------------------------------------------------------------------

    /// <summary>
    /// Rolls 1d4 and returns the result.
    /// </summary>
    /// <returns>Result between 1 and 4.</returns>
    private int RollD4()
    {
        var result = _diceService.Roll(DiceType.D4).Total;
        _logger.LogDebug("Rolled 1d4: {Result}", result);
        return result;
    }

    /// <summary>
    /// Rolls 1d6 and returns the result.
    /// </summary>
    /// <returns>Result between 1 and 6.</returns>
    private int RollD6()
    {
        var result = _diceService.Roll(DiceType.D6).Total;
        _logger.LogDebug("Rolled 1d6: {Result}", result);
        return result;
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Narrative Generation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Generates narrative text for a successful crafting attempt.
    /// </summary>
    /// <param name="recipe">The recipe that was crafted.</param>
    /// <param name="tool">The tool that was created.</param>
    /// <param name="isCritical">Whether this was a critical success.</param>
    /// <returns>Narrative description of the success.</returns>
    private static string GenerateSuccessNarrative(ToolRecipe recipe, ImprovisedTool tool, bool isCritical)
    {
        if (isCritical)
        {
            return $"Your hands move with practiced precision. The components click together " +
                   $"perfectly—the {tool.ToolName} is of exceptional quality! " +
                   $"This tool will serve you well for {ImprovisedTool.QualityDurability} uses " +
                   $"and grants +{tool.BonusAmount}d10.";
        }

        return $"After careful work, you've fashioned a serviceable {tool.ToolName}. " +
               $"It's rough around the edges but functional—good for {ImprovisedTool.StandardDurability} uses " +
               $"with a +{tool.BonusAmount}d10 bonus.";
    }

    /// <summary>
    /// Generates narrative text for a failed crafting attempt.
    /// </summary>
    /// <param name="recipe">The recipe that failed.</param>
    /// <returns>Narrative description of the failure.</returns>
    private static string GenerateFailureNarrative(ToolRecipe recipe)
    {
        return $"Despite your efforts, the {recipe.ToolName} won't come together properly. " +
               "The components break apart as you work, their salvaged nature proving " +
               "too damaged to form anything useful. Your materials are lost.";
    }

    /// <summary>
    /// Generates narrative text for a fumbled crafting attempt.
    /// </summary>
    /// <param name="recipe">The recipe that fumbled.</param>
    /// <param name="damage">Damage taken from the fumble.</param>
    /// <returns>Narrative description of the fumble.</returns>
    private static string GenerateFumbleNarrative(ToolRecipe recipe, int damage)
    {
        return $"A spark flies as components short-circuit! The {recipe.ToolName} attempt " +
               $"fails catastrophically—sharp metal and crackling electricity catch you " +
               $"off guard. You take {damage} damage from cuts and burns. " +
               "The components are utterly ruined.";
    }

    /// <summary>
    /// Generates narrative text for using a tool.
    /// </summary>
    /// <param name="tool">The tool being used.</param>
    /// <param name="bonus">Bonus provided by the tool.</param>
    /// <param name="broke">Whether the tool broke from this use.</param>
    /// <param name="hasSpecialEffect">Whether a special effect was triggered.</param>
    /// <returns>Narrative description of the tool usage.</returns>
    private static string GenerateUsageNarrative(
        ImprovisedTool tool,
        int bonus,
        bool broke,
        bool hasSpecialEffect)
    {
        var baseText = $"You apply your {tool.ToolName} with practiced care (+{bonus}d10).";

        if (hasSpecialEffect)
        {
            baseText += $" {tool.SpecialEffect}!";
        }

        if (broke)
        {
            return baseText + " The tool crumbles in your hands—its improvised construction " +
                   "finally giving way. The tool is broken.";
        }

        return baseText + $" The tool holds together—{tool.UsesRemaining - 1} uses remain.";
    }
}
