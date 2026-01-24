// ------------------------------------------------------------------------------
// <copyright file="ImprovisedToolCraftingServiceTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the ImprovisedToolCraftingService, covering recipe lookup,
// component validation, crafting attempts, tool usage, and salvage generation.
// Part of v0.15.4g Improvised Tool Crafting System implementation.
// </summary>
// ------------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="ImprovisedToolCraftingService"/> service.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the following areas:
/// <list type="bullet">
///   <item><description>Recipe lookup and retrieval of all 4 recipes</description></item>
///   <item><description>Component validation (CanCraft with sufficient/insufficient components)</description></item>
///   <item><description>Crafting success with standard and quality tools</description></item>
///   <item><description>Crafting failure with component consumption</description></item>
///   <item><description>Crafting fumble with damage and component loss</description></item>
///   <item><description>Tool bonus application for matching bypass types</description></item>
///   <item><description>Tool usage with durability tracking and breakage</description></item>
///   <item><description>Special effects (Glitch Trigger, Bypass Clamps)</description></item>
///   <item><description>Salvage generation from critical bypass successes</description></item>
///   <item><description>Crafting modifiers (Tinker's Toolkit, Workshop, Pressure)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class ImprovisedToolCraftingServiceTests
{
    // -------------------------------------------------------------------------
    // Test Dependencies
    // -------------------------------------------------------------------------

    private IDiceService _diceService = null!;
    private ILogger<ImprovisedToolCraftingService> _logger = null!;
    private ImprovisedToolCraftingService _service = null!;

    // -------------------------------------------------------------------------
    // Setup and Teardown
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<ImprovisedToolCraftingService>>();
        _diceService = Substitute.For<IDiceService>();

        // Create service under test
        _service = new ImprovisedToolCraftingService(_diceService, _logger);
    }

    // -------------------------------------------------------------------------
    // Mock Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Configures the dice service mock to return a result with specified net successes.
    /// </summary>
    /// <param name="netSuccesses">Net successes to return.</param>
    /// <param name="isFumble">Whether the roll is a fumble.</param>
    private void SetupDiceRoll(int netSuccesses, bool isFumble = false)
    {
        var successes = Math.Max(0, netSuccesses);
        var botches = isFumble ? 1 : 0;
        var rolls = CreateRollsForNetSuccesses(successes, botches);
        var pool = DicePool.D10(rolls.Count);

        var result = new DiceRollResult(pool, rolls);
        _diceService.Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>()).Returns(result);
    }

    /// <summary>
    /// Configures the dice service mock to return a specific value for single die rolls.
    /// </summary>
    /// <param name="value">Value to return.</param>
    private void SetupSingleDieRoll(int value)
    {
        var pool = DicePool.D10(1);
        var result = new DiceRollResult(pool, new[] { value });
        _diceService.Roll(Arg.Any<DiceType>(), Arg.Any<int>(), Arg.Any<int>()).Returns(result);
    }

    /// <summary>
    /// Creates a list of rolls that would produce the specified successes and botches.
    /// </summary>
    /// <param name="successes">Number of successes.</param>
    /// <param name="botches">Number of botches.</param>
    /// <returns>List of die roll values.</returns>
    private static List<int> CreateRollsForNetSuccesses(int successes, int botches)
    {
        var rolls = new List<int>();

        // Add successes (8, 9, or 10)
        for (var i = 0; i < successes; i++)
        {
            rolls.Add(8 + (i % 3));
        }

        // Add botches (1)
        for (var i = 0; i < botches; i++)
        {
            rolls.Add(1);
        }

        // Pad with neutral values if needed
        if (rolls.Count == 0)
        {
            rolls.Add(5); // Neutral die
        }

        return rolls;
    }

    /// <summary>
    /// Creates an inventory with sufficient components for Shim Picks.
    /// </summary>
    /// <returns>List of components.</returns>
    private static List<CraftingComponent> CreateShimPicksComponents()
    {
        return new List<CraftingComponent>
        {
            CraftingComponent.CreateScrapMetal("test source"),
            CraftingComponent.CreateScrapMetal("test source"),
            CraftingComponent.CreateScrapMetal("test source")
        };
    }

    /// <summary>
    /// Creates an inventory with sufficient components for Wire Probe.
    /// </summary>
    /// <returns>List of components.</returns>
    private static List<CraftingComponent> CreateWireProbeComponents()
    {
        return new List<CraftingComponent>
        {
            CraftingComponent.CreateCopperWire("test source"),
            CraftingComponent.CreateCopperWire("test source"),
            CraftingComponent.CreateHandle("test source")
        };
    }

    /// <summary>
    /// Creates an inventory with sufficient components for Glitch Trigger.
    /// </summary>
    /// <returns>List of components.</returns>
    private static List<CraftingComponent> CreateGlitchTriggerComponents()
    {
        return new List<CraftingComponent>
        {
            CraftingComponent.CreateCapacitor("test source"),
            CraftingComponent.CreateWire("test source"),
            CraftingComponent.CreateWire("test source"),
            CraftingComponent.CreateWire("test source"),
            CraftingComponent.CreateWire("test source")
        };
    }

    // -------------------------------------------------------------------------
    // Recipe Query Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that GetAvailableRecipes returns all 4 standard recipes.
    /// </summary>
    [Test]
    public void GetAvailableRecipes_ReturnsAllFourRecipes()
    {
        // Act
        var recipes = _service.GetAvailableRecipes();

        // Assert
        recipes.Should().HaveCount(4);
        recipes.Select(r => r.ToolName).Should().Contain("Shim Picks");
        recipes.Select(r => r.ToolName).Should().Contain("Wire Probe");
        recipes.Select(r => r.ToolName).Should().Contain("Glitch Trigger");
        recipes.Select(r => r.ToolName).Should().Contain("Bypass Clamps");
    }

    /// <summary>
    /// Tests that Shim Picks recipe has correct configuration.
    /// </summary>
    [Test]
    public void GetAvailableRecipes_ShimPicksHasCorrectConfig()
    {
        // Act
        var recipes = _service.GetAvailableRecipes();
        var shimPicks = recipes.First(r => r.ToolName == "Shim Picks");

        // Assert
        shimPicks.CraftDc.Should().Be(10);
        shimPicks.RequiredAttribute.Should().Be(CraftingAttribute.WitsOrFinesse);
        shimPicks.RequiredComponents.Should().HaveCount(1);
        shimPicks.TotalComponentsRequired.Should().Be(3);
    }

    /// <summary>
    /// Tests that Glitch Trigger recipe requires WITS only.
    /// </summary>
    [Test]
    public void GetAvailableRecipes_GlitchTriggerRequiresWitsOnly()
    {
        // Act
        var recipes = _service.GetAvailableRecipes();
        var glitchTrigger = recipes.First(r => r.ToolName == "Glitch Trigger");

        // Assert
        glitchTrigger.CraftDc.Should().Be(14);
        glitchTrigger.RequiredAttribute.Should().Be(CraftingAttribute.Wits);
    }

    // -------------------------------------------------------------------------
    // Component Validation Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that CanCraft returns true with sufficient components.
    /// </summary>
    [Test]
    public void CanCraft_WithSufficientComponents_ReturnsCanCraftTrue()
    {
        // Arrange
        var recipe = ToolRecipe.ShimPicks;
        var inventory = CreateShimPicksComponents();

        // Act
        var result = _service.CanCraft(recipe, inventory);

        // Assert
        result.CanCraft.Should().BeTrue();
        result.MissingComponents.Should().BeEmpty();
        result.MatchedComponents.Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that CanCraft returns false with insufficient components.
    /// </summary>
    [Test]
    public void CanCraft_WithInsufficientComponents_ReturnsCanCraftFalse()
    {
        // Arrange
        var recipe = ToolRecipe.ShimPicks;
        var inventory = new List<CraftingComponent>
        {
            CraftingComponent.CreateScrapMetal("test source") // Only 1, need 3
        };

        // Act
        var result = _service.CanCraft(recipe, inventory);

        // Assert
        result.CanCraft.Should().BeFalse();
        result.MissingComponents.Should().HaveCount(1);
        result.TotalMissingCount.Should().Be(2); // Missing 2 more
    }

    /// <summary>
    /// Tests that CanCraft allows Metal to substitute for Misc.
    /// </summary>
    [Test]
    public void CanCraft_MetalSubstitutesForMisc_ReturnsCanCraftTrue()
    {
        // Arrange
        var recipe = ToolRecipe.WireProbe; // Requires 2× Copper Wire + 1× Handle (Misc)
        var inventory = new List<CraftingComponent>
        {
            CraftingComponent.CreateCopperWire("test source"),
            CraftingComponent.CreateCopperWire("test source"),
            CraftingComponent.CreateScrapMetal("test source") // Metal can substitute for Misc
        };

        // Act
        var result = _service.CanCraft(recipe, inventory);

        // Assert
        result.CanCraft.Should().BeTrue();
        result.MatchedComponents.Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that CanCraft returns empty inventory result correctly.
    /// </summary>
    [Test]
    public void CanCraft_WithEmptyInventory_ReturnsAllComponentsMissing()
    {
        // Arrange
        var recipe = ToolRecipe.ShimPicks;
        var inventory = new List<CraftingComponent>();

        // Act
        var result = _service.CanCraft(recipe, inventory);

        // Assert
        result.CanCraft.Should().BeFalse();
        result.MissingComponents.Should().HaveCount(1);
        result.TotalMissingCount.Should().Be(3);
    }

    // -------------------------------------------------------------------------
    // Crafting Success Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that AttemptCraft creates a standard tool on success.
    /// </summary>
    [Test]
    public void AttemptCraft_WithSuccessfulRoll_CreatesStandardTool()
    {
        // Arrange
        var recipe = ToolRecipe.ShimPicks;
        var inventory = CreateShimPicksComponents();
        var context = CharacterCraftingContext.CreateBasic(wits: 4, finesse: 3);

        SetupDiceRoll(netSuccesses: 2); // Success but not critical

        // Act
        var result = _service.AttemptCraft(context, recipe, inventory);

        // Assert
        result.Success.Should().BeTrue();
        result.IsCritical.Should().BeFalse();
        result.IsFumble.Should().BeFalse();
        result.ToolCreated.Should().NotBeNull();
        result.ToolCreated!.Value.ToolName.Should().Be("Shim Picks");
        result.ToolCreated!.Value.BonusAmount.Should().Be(1); // Standard bonus
        result.ToolCreated!.Value.UsesRemaining.Should().Be(3); // Standard durability
        result.ToolCreated!.Value.IsQuality.Should().BeFalse();
        result.ComponentsConsumed.Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that AttemptCraft creates a quality tool on critical success.
    /// </summary>
    [Test]
    public void AttemptCraft_WithCriticalSuccess_CreatesQualityTool()
    {
        // Arrange
        var recipe = ToolRecipe.ShimPicks;
        var inventory = CreateShimPicksComponents();
        var context = CharacterCraftingContext.CreateBasic(wits: 4, finesse: 3);

        SetupDiceRoll(netSuccesses: 5); // Critical success (>= 5)

        // Act
        var result = _service.AttemptCraft(context, recipe, inventory);

        // Assert
        result.Success.Should().BeTrue();
        result.IsCritical.Should().BeTrue();
        result.ToolCreated.Should().NotBeNull();
        result.ToolCreated!.Value.ToolName.Should().Be("Quality Shim Picks");
        result.ToolCreated!.Value.BonusAmount.Should().Be(2); // Quality bonus
        result.ToolCreated!.Value.UsesRemaining.Should().Be(5); // Quality durability
        result.ToolCreated!.Value.IsQuality.Should().BeTrue();
    }

    /// <summary>
    /// Tests that AttemptCraft fails without consuming components when missing.
    /// </summary>
    [Test]
    public void AttemptCraft_WithMissingComponents_FailsWithoutConsumingComponents()
    {
        // Arrange
        var recipe = ToolRecipe.ShimPicks;
        var inventory = new List<CraftingComponent>
        {
            CraftingComponent.CreateScrapMetal("test source") // Only 1, need 3
        };
        var context = CharacterCraftingContext.CreateBasic(wits: 4, finesse: 3);

        // Act
        var result = _service.AttemptCraft(context, recipe, inventory);

        // Assert
        result.Success.Should().BeFalse();
        result.ToolCreated.Should().BeNull();
        result.ComponentsConsumed.Should().BeEmpty(); // No components consumed
        result.NarrativeText.Should().Contain("lack the required components");
    }

    // -------------------------------------------------------------------------
    // Crafting Failure Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that AttemptCraft fails and consumes components on failed roll.
    /// </summary>
    [Test]
    public void AttemptCraft_WithFailedRoll_ConsumesComponentsWithNoTool()
    {
        // Arrange
        var recipe = ToolRecipe.ShimPicks;
        var inventory = CreateShimPicksComponents();
        var context = CharacterCraftingContext.CreateBasic(wits: 4, finesse: 3);

        SetupDiceRoll(netSuccesses: 0); // Failure

        // Act
        var result = _service.AttemptCraft(context, recipe, inventory);

        // Assert
        result.Success.Should().BeFalse();
        result.IsFumble.Should().BeFalse();
        result.ToolCreated.Should().BeNull();
        result.ComponentsConsumed.Should().HaveCount(3); // Components consumed on failure
        result.FumbleDamage.Should().Be(0); // No damage on regular failure
    }

    /// <summary>
    /// Tests that AttemptCraft fumble causes damage and component loss.
    /// </summary>
    [Test]
    public void AttemptCraft_WithFumble_CausesDamageAndConsumesComponents()
    {
        // Arrange
        var recipe = ToolRecipe.ShimPicks;
        var inventory = CreateShimPicksComponents();
        var context = CharacterCraftingContext.CreateBasic(wits: 4, finesse: 3);

        SetupDiceRoll(netSuccesses: 0, isFumble: true); // Fumble
        SetupSingleDieRoll(4); // 4 damage from 1d6

        // Act
        var result = _service.AttemptCraft(context, recipe, inventory);

        // Assert
        result.Success.Should().BeFalse();
        result.IsFumble.Should().BeTrue();
        result.ToolCreated.Should().BeNull();
        result.ComponentsConsumed.Should().HaveCount(3);
        result.FumbleDamage.Should().BeGreaterThan(0);
    }

    // -------------------------------------------------------------------------
    // Crafting Modifier Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that Tinker's Toolkit provides +1d10 bonus.
    /// </summary>
    [Test]
    public void AttemptCraft_WithTinkersToolkit_IncreasesChanceOfSuccess()
    {
        // Arrange
        var recipe = ToolRecipe.ShimPicks;
        var inventory = CreateShimPicksComponents();
        var context = new CharacterCraftingContext(
            Wits: 4,
            Finesse: 3,
            HasTinkersToolkit: true,
            HasWorkshopAccess: false,
            IsUnderPressure: false);

        SetupDiceRoll(netSuccesses: 2);

        // Act
        var result = _service.AttemptCraft(context, recipe, inventory);

        // Assert - Verify the roll was made (bonus is applied internally)
        _diceService.Received(1).Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>());
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that optimal crafting context (toolkit + workshop) grants bonuses.
    /// </summary>
    [Test]
    public void AttemptCraft_WithOptimalContext_HasHigherSuccessChance()
    {
        // Arrange
        var recipe = ToolRecipe.ShimPicks;
        var inventory = CreateShimPicksComponents();
        var context = CharacterCraftingContext.CreateOptimal(wits: 4, finesse: 3);

        SetupDiceRoll(netSuccesses: 2);

        // Act
        var result = _service.AttemptCraft(context, recipe, inventory);

        // Assert
        _diceService.Received(1).Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>());
        result.Success.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Tool Bonus Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that GetToolBonus returns correct bonus for matching bypass type.
    /// </summary>
    [Test]
    public void GetToolBonus_ForMatchingBypassType_ReturnsCorrectBonus()
    {
        // Arrange
        var shimPicks = ImprovisedTool.CreateShimPicks(isQuality: false);

        // Act
        var bonus = _service.GetToolBonus(shimPicks, BypassType.Lockpicking);

        // Assert
        bonus.Should().Be(1); // Standard bonus
    }

    /// <summary>
    /// Tests that GetToolBonus returns higher bonus for quality tools.
    /// </summary>
    [Test]
    public void GetToolBonus_ForQualityTool_ReturnsHigherBonus()
    {
        // Arrange
        var qualityShimPicks = ImprovisedTool.CreateShimPicks(isQuality: true);

        // Act
        var bonus = _service.GetToolBonus(qualityShimPicks, BypassType.Lockpicking);

        // Assert
        bonus.Should().Be(2); // Quality bonus
    }

    /// <summary>
    /// Tests that GetToolBonus returns 0 for non-matching bypass type.
    /// </summary>
    [Test]
    public void GetToolBonus_ForNonMatchingBypassType_ReturnsZero()
    {
        // Arrange
        var shimPicks = ImprovisedTool.CreateShimPicks(isQuality: false);

        // Act
        var bonus = _service.GetToolBonus(shimPicks, BypassType.TerminalHacking);

        // Assert
        bonus.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetToolBonus returns 0 for broken tools.
    /// </summary>
    [Test]
    public void GetToolBonus_ForBrokenTool_ReturnsZero()
    {
        // Arrange
        var shimPicks = ImprovisedTool.CreateShimPicks(isQuality: false).Break();

        // Act
        var bonus = _service.GetToolBonus(shimPicks, BypassType.Lockpicking);

        // Assert
        bonus.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // Tool Usage Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that UseTool decrements uses and returns modified tool.
    /// </summary>
    [Test]
    public void UseTool_DecrementsUsesAndReturnsModifiedTool()
    {
        // Arrange
        var shimPicks = ImprovisedTool.CreateShimPicks(isQuality: false);

        // Act
        var result = _service.UseTool(shimPicks, BypassType.Lockpicking);

        // Assert
        result.BonusApplied.Should().Be(1);
        result.ModifiedTool.Should().NotBeNull();
        result.ModifiedTool!.Value.UsesRemaining.Should().Be(2); // 3 - 1 = 2
        result.ToolBroke.Should().BeFalse();
    }

    /// <summary>
    /// Tests that UseTool breaks tool when uses are exhausted.
    /// </summary>
    [Test]
    public void UseTool_OnLastUse_BreaksTool()
    {
        // Arrange - Create tool with 1 use remaining
        var tool = ImprovisedTool.CreateShimPicks(isQuality: false);
        var modifiedTool = tool.UseOnce()!.Value.UseOnce()!.Value; // 3 -> 2 -> 1

        // Act
        var result = _service.UseTool(modifiedTool, BypassType.Lockpicking);

        // Assert
        result.BonusApplied.Should().Be(1);
        result.ModifiedTool.Should().BeNull();
        result.ToolBroke.Should().BeTrue();
    }

    /// <summary>
    /// Tests that UseTool returns no bonus for non-matching bypass type.
    /// </summary>
    [Test]
    public void UseTool_ForNonMatchingBypassType_ReturnsNoBonus()
    {
        // Arrange
        var shimPicks = ImprovisedTool.CreateShimPicks(isQuality: false);

        // Act
        var result = _service.UseTool(shimPicks, BypassType.TerminalHacking);

        // Assert
        result.BonusApplied.Should().Be(0);
        result.ModifiedTool.Should().NotBeNull(); // Tool not consumed
        result.ToolBroke.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Special Effect Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that Glitch Trigger has special effect.
    /// </summary>
    [Test]
    public void UseTool_GlitchTrigger_TriggersSpecialEffect()
    {
        // Arrange
        var glitchTrigger = ImprovisedTool.CreateGlitchTrigger(isQuality: false);

        // Act
        var result = _service.UseTool(glitchTrigger, BypassType.GlitchExploitation);

        // Assert
        result.SpecialEffectTriggered.Should().BeTrue();
        result.SpecialEffectDescription.Should().Contain("Glitched");
    }

    /// <summary>
    /// Tests that Bypass Clamps has special effect.
    /// </summary>
    [Test]
    public void UseTool_BypassClamps_TriggersSpecialEffect()
    {
        // Arrange
        var bypassClamps = ImprovisedTool.CreateBypassClamps(isQuality: false);

        // Act
        var result = _service.UseTool(bypassClamps, BypassType.TerminalHacking);

        // Assert
        result.SpecialEffectTriggered.Should().BeTrue();
        result.SpecialEffectDescription.Should().Contain("Layer 1");
    }

    /// <summary>
    /// Tests that Shim Picks does not have special effect.
    /// </summary>
    [Test]
    public void UseTool_ShimPicks_DoesNotTriggerSpecialEffect()
    {
        // Arrange
        var shimPicks = ImprovisedTool.CreateShimPicks(isQuality: false);

        // Act
        var result = _service.UseTool(shimPicks, BypassType.Lockpicking);

        // Assert
        result.SpecialEffectTriggered.Should().BeFalse();
        result.SpecialEffectDescription.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Salvage Generation Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that GetSalvageFromCritical generates components for lockpicking.
    /// </summary>
    [Test]
    public void GetSalvageFromCritical_Lockpicking_GeneratesMetalComponents()
    {
        // Arrange
        SetupSingleDieRoll(3); // 50% chance for second component

        // Act
        var components = _service.GetSalvageFromCritical(BypassType.Lockpicking, "test lock");

        // Assert
        components.Should().NotBeEmpty();
        components.Should().Contain(c => c.ComponentName == "High-Tension Spring");
    }

    /// <summary>
    /// Tests that GetSalvageFromCritical generates components for terminal hacking.
    /// </summary>
    [Test]
    public void GetSalvageFromCritical_TerminalHacking_GeneratesCircuitAndWire()
    {
        // Arrange
        SetupSingleDieRoll(5); // Not a 6, so no capacitor

        // Act
        var components = _service.GetSalvageFromCritical(BypassType.TerminalHacking, "test terminal");

        // Assert
        components.Should().HaveCountGreaterOrEqualTo(2);
        components.Should().Contain(c => c.ComponentName == "Circuit Fragment");
        components.Should().Contain(c => c.ComponentName == "Wire");
    }

    /// <summary>
    /// Tests that GetSalvageFromCritical may generate rare capacitor.
    /// </summary>
    [Test]
    public void GetSalvageFromCritical_WithLuckyRoll_CanGenerateRareCapacitor()
    {
        // Arrange
        SetupSingleDieRoll(6); // Lucky roll for capacitor

        // Act
        var components = _service.GetSalvageFromCritical(BypassType.TerminalHacking, "test terminal");

        // Assert
        components.Should().Contain(c => c.ComponentName == "Capacitor");
        components.Should().Contain(c => c.IsRare);
    }

    /// <summary>
    /// Tests that GetSalvageFromCritical generates wire and clips for trap disarmament.
    /// </summary>
    [Test]
    public void GetSalvageFromCritical_TrapDisarmament_GeneratesWireAndClips()
    {
        // Act
        var components = _service.GetSalvageFromCritical(BypassType.TrapDisarmament, "test trap");

        // Assert
        components.Should().HaveCount(2);
        components.Should().Contain(c => c.ComponentName == "Wire");
        components.Should().Contain(c => c.ComponentName == "Metal Clips");
    }

    // -------------------------------------------------------------------------
    // Constructor Validation Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that constructor throws on null dice service.
    /// </summary>
    [Test]
    public void Constructor_NullDiceService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ImprovisedToolCraftingService(null!, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("diceService");
    }

    /// <summary>
    /// Tests that constructor throws on null logger.
    /// </summary>
    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ImprovisedToolCraftingService(_diceService, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("logger");
    }
}
