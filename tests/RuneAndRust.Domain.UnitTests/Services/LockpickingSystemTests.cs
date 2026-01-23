// ------------------------------------------------------------------------------
// <copyright file="LockpickingSystemTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the Lockpicking System including lock type DCs, tool quality
// modifiers, salvage components, and LockContext behavior.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.UnitTests.Services;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for the Lockpicking System components.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the core acceptance criteria:
/// </para>
/// <list type="number">
///   <item>
///     <description>Lock type sets correct DC (ImprovisedLatch 6, SimpleLock 10, StandardLock 14, etc.)</description>
///   </item>
///   <item>
///     <description>Tool quality provides correct dice modifiers (-2 to +2)</description>
///   </item>
///   <item>
///     <description>Corruption modifies DC correctly (+2 Glitched, +4 Blighted, +6 Resonance)</description>
///   </item>
///   <item>
///     <description>Jammed status adds +2 DC penalty</description>
///   </item>
///   <item>
///     <description>Salvageable components have correct rarity by lock type</description>
///   </item>
/// </list>
/// </remarks>
[TestFixture]
public class LockpickingSystemTests
{
    #region Test 1: Lock Type Base DC Values

    /// <summary>
    /// Verifies that each lock type returns the correct base DC.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <param name="expectedDc">The expected base difficulty class.</param>
    [TestCase(LockType.ImprovisedLatch, 6)]
    [TestCase(LockType.SimpleLock, 10)]
    [TestCase(LockType.StandardLock, 14)]
    [TestCase(LockType.ComplexLock, 18)]
    [TestCase(LockType.MasterLock, 22)]
    [TestCase(LockType.JotunForged, 26)]
    public void LockType_GetBaseDc_ReturnsCorrectDc(LockType lockType, int expectedDc)
    {
        // Act
        var baseDc = lockType.GetBaseDc();

        // Assert
        baseDc.Should().Be(expectedDc, because: $"{lockType} should have DC {expectedDc}");
    }

    /// <summary>
    /// Verifies that all lock types have unique DC values.
    /// </summary>
    [Test]
    public void LockType_AllTypesHaveUniqueDcValues()
    {
        // Arrange
        var lockTypes = Enum.GetValues<LockType>();
        var dcValues = new List<int>();

        // Act
        foreach (var lockType in lockTypes)
        {
            dcValues.Add(lockType.GetBaseDc());
        }

        // Assert
        dcValues.Should().OnlyHaveUniqueItems(because: "each lock type should have a unique DC");
    }

    /// <summary>
    /// Verifies that DC values scale progressively from ImprovisedLatch to JotunForged.
    /// </summary>
    [Test]
    public void LockType_DcScalesProgressively()
    {
        // Act
        var improvisedDc = LockType.ImprovisedLatch.GetBaseDc();
        var simpleDc = LockType.SimpleLock.GetBaseDc();
        var standardDc = LockType.StandardLock.GetBaseDc();
        var complexDc = LockType.ComplexLock.GetBaseDc();
        var masterDc = LockType.MasterLock.GetBaseDc();
        var jotunDc = LockType.JotunForged.GetBaseDc();

        // Assert
        improvisedDc.Should().BeLessThan(simpleDc);
        simpleDc.Should().BeLessThan(standardDc);
        standardDc.Should().BeLessThan(complexDc);
        complexDc.Should().BeLessThan(masterDc);
        masterDc.Should().BeLessThan(jotunDc);
    }

    /// <summary>
    /// Verifies that each lock type has a non-empty display name.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    [TestCase(LockType.ImprovisedLatch)]
    [TestCase(LockType.SimpleLock)]
    [TestCase(LockType.StandardLock)]
    [TestCase(LockType.ComplexLock)]
    [TestCase(LockType.MasterLock)]
    [TestCase(LockType.JotunForged)]
    public void LockType_GetDisplayName_ReturnsNonEmpty(LockType lockType)
    {
        // Act
        var displayName = lockType.GetDisplayName();

        // Assert
        displayName.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Test 2: Tool Quality Dice Modifiers

    /// <summary>
    /// Verifies that each tool quality returns the correct dice modifier.
    /// </summary>
    /// <param name="quality">The tool quality.</param>
    /// <param name="expectedModifier">The expected dice modifier.</param>
    [TestCase(ToolQuality.BareHands, -2)]
    [TestCase(ToolQuality.Improvised, 0)]
    [TestCase(ToolQuality.Proper, 1)]
    [TestCase(ToolQuality.Masterwork, 2)]
    public void ToolQuality_GetDiceModifier_ReturnsCorrectModifier(
        ToolQuality quality,
        int expectedModifier)
    {
        // Act
        var diceModifier = quality.GetDiceModifier();

        // Assert
        diceModifier.Should().Be(expectedModifier,
            because: $"{quality} should have dice modifier {expectedModifier}");
    }

    /// <summary>
    /// Verifies that bare hands can only attempt locks with DC less than 10.
    /// </summary>
    [TestCase(LockType.ImprovisedLatch, true)]
    [TestCase(LockType.SimpleLock, false)]
    [TestCase(LockType.StandardLock, false)]
    [TestCase(LockType.ComplexLock, false)]
    [TestCase(LockType.MasterLock, false)]
    [TestCase(LockType.JotunForged, false)]
    public void ToolQuality_BareHands_CanOnlyAttemptLowDcLocks(LockType lockType, bool canAttempt)
    {
        // Act
        var result = ToolQuality.BareHands.CanAttemptLock(lockType);

        // Assert
        result.Should().Be(canAttempt,
            because: $"bare hands {(canAttempt ? "can" : "cannot")} attempt {lockType}");
    }

    /// <summary>
    /// Verifies that improvised tools can break on fumble.
    /// </summary>
    [Test]
    public void ToolQuality_Improvised_CanBreakOnFumble()
    {
        // Act
        var canBreak = ToolQuality.Improvised.CanBreakOnFumble();

        // Assert
        canBreak.Should().BeTrue(because: "improvised tools should break on fumble");
    }

    /// <summary>
    /// Verifies that proper and masterwork tools do not break on fumble.
    /// </summary>
    [TestCase(ToolQuality.Proper)]
    [TestCase(ToolQuality.Masterwork)]
    public void ToolQuality_QualityTools_DoNotBreakOnFumble(ToolQuality quality)
    {
        // Act
        var canBreak = quality.CanBreakOnFumble();

        // Assert
        canBreak.Should().BeFalse(because: $"{quality} tools should not break on fumble");
    }

    #endregion

    #region Test 3: LockContext Effective DC Calculation

    /// <summary>
    /// Verifies that LockContext calculates effective DC correctly without modifiers.
    /// </summary>
    [Test]
    public void LockContext_EffectiveDc_EqualsBaseDc_WhenNoModifiers()
    {
        // Arrange
        var context = new LockContext(
            lockId: "test-lock-1",
            lockType: LockType.StandardLock,
            corruptionLevel: CorruptionTier.Normal,
            toolQuality: ToolQuality.Proper,
            previousAttempts: 0,
            isJammed: false);

        // Act & Assert
        context.EffectiveDc.Should().Be(14, because: "StandardLock has base DC 14");
        context.BaseDc.Should().Be(14);
        context.CorruptionDcModifier.Should().Be(0);
        context.JammedDcModifier.Should().Be(0);
    }

    /// <summary>
    /// Verifies that corruption adds correct DC modifier.
    /// </summary>
    /// <param name="corruption">The corruption tier.</param>
    /// <param name="expectedModifier">The expected DC modifier.</param>
    [TestCase(CorruptionTier.Normal, 0)]
    [TestCase(CorruptionTier.Glitched, 2)]
    [TestCase(CorruptionTier.Blighted, 4)]
    [TestCase(CorruptionTier.Resonance, 6)]
    public void LockContext_CorruptionDcModifier_ReturnsCorrectValue(
        CorruptionTier corruption,
        int expectedModifier)
    {
        // Arrange
        var context = new LockContext(
            lockId: "test-lock-2",
            lockType: LockType.SimpleLock,
            corruptionLevel: corruption);

        // Act & Assert
        context.CorruptionDcModifier.Should().Be(expectedModifier);
        context.EffectiveDc.Should().Be(10 + expectedModifier);
    }

    /// <summary>
    /// Verifies that jammed status adds +2 DC.
    /// </summary>
    [Test]
    public void LockContext_JammedDcModifier_AddsTwoDc()
    {
        // Arrange
        var context = new LockContext(
            lockId: "test-lock-3",
            lockType: LockType.SimpleLock,
            corruptionLevel: CorruptionTier.Normal,
            isJammed: true);

        // Act & Assert
        context.JammedDcModifier.Should().Be(2);
        context.EffectiveDc.Should().Be(12, because: "SimpleLock DC 10 + Jammed +2 = 12");
    }

    /// <summary>
    /// Verifies that corruption and jammed stack correctly.
    /// </summary>
    [Test]
    public void LockContext_EffectiveDc_StacksCorruptionAndJammed()
    {
        // Arrange
        var context = new LockContext(
            lockId: "test-lock-4",
            lockType: LockType.ComplexLock, // DC 18
            corruptionLevel: CorruptionTier.Blighted, // +4
            isJammed: true); // +2

        // Act & Assert
        context.BaseDc.Should().Be(18);
        context.CorruptionDcModifier.Should().Be(4);
        context.JammedDcModifier.Should().Be(2);
        context.EffectiveDc.Should().Be(24, because: "DC 18 + Blighted +4 + Jammed +2 = 24");
    }

    #endregion

    #region Test 4: LockContext Tool Requirement Validation

    /// <summary>
    /// Verifies that locks with DC 10+ require tools.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <param name="requiresTools">Whether tools are required.</param>
    [TestCase(LockType.ImprovisedLatch, false)]
    [TestCase(LockType.SimpleLock, true)]
    [TestCase(LockType.StandardLock, true)]
    [TestCase(LockType.ComplexLock, true)]
    [TestCase(LockType.MasterLock, true)]
    [TestCase(LockType.JotunForged, true)]
    public void LockContext_RequiresTools_CorrectForLockType(LockType lockType, bool requiresTools)
    {
        // Arrange
        var context = new LockContext("test-lock", lockType);

        // Act & Assert
        context.RequiresTools.Should().Be(requiresTools);
    }

    /// <summary>
    /// Verifies that HasRequiredTools is correct based on tool quality.
    /// </summary>
    [Test]
    public void LockContext_HasRequiredTools_TrueForImprovisedOnSimpleLock()
    {
        // Arrange
        var context = new LockContext(
            lockId: "test-lock",
            lockType: LockType.SimpleLock,
            toolQuality: ToolQuality.Improvised);

        // Act & Assert
        context.RequiresTools.Should().BeTrue();
        context.HasRequiredTools.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that bare hands fails tool requirement for DC 10+ locks.
    /// </summary>
    [Test]
    public void LockContext_HasRequiredTools_FalseForBareHandsOnSimpleLock()
    {
        // Arrange
        var context = new LockContext(
            lockId: "test-lock",
            lockType: LockType.SimpleLock,
            toolQuality: ToolQuality.BareHands);

        // Act & Assert
        context.RequiresTools.Should().BeTrue();
        context.HasRequiredTools.Should().BeFalse();
    }

    #endregion

    #region Test 5: LockContext Immutable State Changes

    /// <summary>
    /// Verifies that WithJammed creates a new context with jammed status.
    /// </summary>
    [Test]
    public void LockContext_WithJammed_CreatesNewContextWithJammedTrue()
    {
        // Arrange
        var original = new LockContext(
            lockId: "test-lock",
            lockType: LockType.StandardLock,
            isJammed: false);

        // Act
        var jammed = original.WithJammed();

        // Assert
        original.IsJammed.Should().BeFalse(because: "original should be unchanged");
        jammed.IsJammed.Should().BeTrue(because: "new context should be jammed");
        jammed.LockId.Should().Be(original.LockId);
        jammed.LockType.Should().Be(original.LockType);
    }

    /// <summary>
    /// Verifies that WithFailedAttempt increments the attempt counter.
    /// </summary>
    [Test]
    public void LockContext_WithFailedAttempt_IncrementsAttemptCount()
    {
        // Arrange
        var original = new LockContext(
            lockId: "test-lock",
            lockType: LockType.StandardLock,
            previousAttempts: 2);

        // Act
        var afterFail = original.WithFailedAttempt();

        // Assert
        original.PreviousAttempts.Should().Be(2);
        afterFail.PreviousAttempts.Should().Be(3);
    }

    /// <summary>
    /// Verifies that WithToolQuality changes the tool quality.
    /// </summary>
    [Test]
    public void LockContext_WithToolQuality_ChangesToolQuality()
    {
        // Arrange
        var original = new LockContext(
            lockId: "test-lock",
            lockType: LockType.StandardLock,
            toolQuality: ToolQuality.Improvised);

        // Act
        var upgraded = original.WithToolQuality(ToolQuality.Masterwork);

        // Assert
        original.ToolQuality.Should().Be(ToolQuality.Improvised);
        upgraded.ToolQuality.Should().Be(ToolQuality.Masterwork);
    }

    #endregion

    #region Test 6: Salvageable Component Factory Methods

    /// <summary>
    /// Verifies that each component factory method creates valid components.
    /// </summary>
    [Test]
    public void SalvageableComponent_FactoryMethods_CreateValidComponents()
    {
        // Act
        var wireBundle = SalvageableComponent.WireBundle();
        var smallSpring = SalvageableComponent.SmallSpring();
        var highTensionSpring = SalvageableComponent.HighTensionSpring();
        var pinSet = SalvageableComponent.PinSet();
        var circuitFragment = SalvageableComponent.CircuitFragment();
        var powerCell = SalvageableComponent.PowerCellFragment();
        var encryptionChip = SalvageableComponent.EncryptionChip();
        var biometricSensor = SalvageableComponent.BiometricSensor();

        // Assert
        wireBundle.ComponentId.Should().Be("wire-bundle");
        wireBundle.Rarity.Should().Be(ItemRarity.Common);

        highTensionSpring.Rarity.Should().Be(ItemRarity.Uncommon);
        pinSet.Rarity.Should().Be(ItemRarity.Uncommon);

        circuitFragment.Rarity.Should().Be(ItemRarity.Rare);
        powerCell.Rarity.Should().Be(ItemRarity.Rare);

        encryptionChip.Rarity.Should().Be(ItemRarity.Legendary);
        biometricSensor.Rarity.Should().Be(ItemRarity.Legendary);
    }

    /// <summary>
    /// Verifies that component base values scale with rarity.
    /// </summary>
    [Test]
    public void SalvageableComponent_BaseValue_ScalesWithRarity()
    {
        // Arrange
        var common = SalvageableComponent.WireBundle();
        var uncommon = SalvageableComponent.HighTensionSpring();
        var rare = SalvageableComponent.CircuitFragment();
        var legendary = SalvageableComponent.EncryptionChip();

        // Assert
        common.BaseValue.Should().BeLessThan(uncommon.BaseValue);
        uncommon.BaseValue.Should().BeLessThan(rare.BaseValue);
        rare.BaseValue.Should().BeLessThan(legendary.BaseValue);
    }

    /// <summary>
    /// Verifies that TotalValue applies rarity multiplier.
    /// </summary>
    [Test]
    public void SalvageableComponent_TotalValue_AppliesRarityMultiplier()
    {
        // Arrange
        var common = SalvageableComponent.WireBundle(); // BaseValue 2, Common x1
        var legendary = SalvageableComponent.EncryptionChip(); // BaseValue 100, Legendary x25

        // Act & Assert
        common.TotalValue.Should().Be(2); // 2 * 1.0
        legendary.TotalValue.Should().Be(2500); // 100 * 25.0
    }

    /// <summary>
    /// Verifies that components with quantity multiply correctly.
    /// </summary>
    [Test]
    public void SalvageableComponent_WithQuantity_MultipliesTotalValue()
    {
        // Arrange
        var single = SalvageableComponent.WireBundle(1);
        var triple = SalvageableComponent.WireBundle(3);

        // Act & Assert
        single.TotalValue.Should().Be(2);
        triple.TotalValue.Should().Be(6);
    }

    #endregion

    #region Test 7: Item Rarity Extensions

    /// <summary>
    /// Verifies that each rarity has the correct value multiplier.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <param name="expectedMultiplier">The expected multiplier.</param>
    [TestCase(ItemRarity.Common, 1.0f)]
    [TestCase(ItemRarity.Uncommon, 2.0f)]
    [TestCase(ItemRarity.Rare, 5.0f)]
    [TestCase(ItemRarity.Epic, 10.0f)]
    [TestCase(ItemRarity.Legendary, 25.0f)]
    public void ItemRarity_GetValueMultiplier_ReturnsCorrectValue(
        ItemRarity rarity,
        float expectedMultiplier)
    {
        // Act
        var multiplier = rarity.GetValueMultiplier();

        // Assert
        multiplier.Should().Be(expectedMultiplier);
    }

    /// <summary>
    /// Verifies that each rarity has a non-empty display name.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    [TestCase(ItemRarity.Common)]
    [TestCase(ItemRarity.Uncommon)]
    [TestCase(ItemRarity.Rare)]
    [TestCase(ItemRarity.Epic)]
    [TestCase(ItemRarity.Legendary)]
    public void ItemRarity_GetDisplayName_ReturnsNonEmpty(ItemRarity rarity)
    {
        // Act
        var displayName = rarity.GetDisplayName();

        // Assert
        displayName.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Verifies that lock types return correct typical salvage rarity.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <param name="expectedRarity">The expected salvage rarity.</param>
    [TestCase(LockType.ImprovisedLatch, ItemRarity.Common)]
    [TestCase(LockType.SimpleLock, ItemRarity.Uncommon)]
    [TestCase(LockType.StandardLock, ItemRarity.Uncommon)]
    [TestCase(LockType.ComplexLock, ItemRarity.Rare)]
    [TestCase(LockType.MasterLock, ItemRarity.Rare)]
    [TestCase(LockType.JotunForged, ItemRarity.Legendary)]
    public void ItemRarity_GetTypicalSalvageRarity_CorrectForLockType(
        LockType lockType,
        ItemRarity expectedRarity)
    {
        // Act
        var rarity = lockType.GetTypicalSalvageRarity();

        // Assert
        rarity.Should().Be(expectedRarity);
    }

    #endregion

    #region Test 8: LockpickingResult Factory Methods

    /// <summary>
    /// Verifies that Success factory creates correct result.
    /// </summary>
    [Test]
    public void LockpickingResult_Success_CreatesCorrectResult()
    {
        // Arrange
        var context = new LockContext("test-lock", LockType.SimpleLock);
        var diceResult = CreateDiceResult(successes: 3, botches: 0);
        var checkResult = CreateSkillCheckResult(diceResult, dc: 10, outcome: SkillOutcome.FullSuccess);

        // Act
        var result = LockpickingResult.Success(
            SkillOutcome.FullSuccess,
            context,
            checkResult,
            "Lock opens");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Outcome.Should().Be(SkillOutcome.FullSuccess);
        result.IsCriticalSuccess.Should().BeFalse();
        result.IsFumble.Should().BeFalse();
        result.HasSalvage.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that critical success can include salvage.
    /// </summary>
    [Test]
    public void LockpickingResult_CriticalSuccess_IncludesSalvage()
    {
        // Arrange
        var context = new LockContext("test-lock", LockType.SimpleLock);
        var diceResult = CreateDiceResult(successes: 6, botches: 0);
        var checkResult = CreateSkillCheckResult(diceResult, dc: 10, outcome: SkillOutcome.CriticalSuccess);
        var salvage = SalvageableComponent.PinSet();

        // Act
        var result = LockpickingResult.Success(
            SkillOutcome.CriticalSuccess,
            context,
            checkResult,
            "Lock opens with salvage",
            salvage);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsCriticalSuccess.Should().BeTrue();
        result.HasSalvage.Should().BeTrue();
        result.SalvagedComponent.Should().NotBeNull();
        result.SalvagedComponent!.Value.ComponentId.Should().Be("pin-set");
    }

    /// <summary>
    /// Verifies that Failure factory creates correct result.
    /// </summary>
    [Test]
    public void LockpickingResult_Failure_CreatesCorrectResult()
    {
        // Arrange
        var context = new LockContext("test-lock", LockType.ComplexLock);
        var diceResult = CreateDiceResult(successes: 1, botches: 0);
        var checkResult = CreateSkillCheckResult(diceResult, dc: 18, outcome: SkillOutcome.Failure);

        // Act
        var result = LockpickingResult.Failure(
            SkillOutcome.Failure,
            context,
            checkResult,
            "Lock remains closed");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(SkillOutcome.Failure);
        result.IsFumble.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Blocked factory creates correct result.
    /// </summary>
    [Test]
    public void LockpickingResult_Blocked_CreatesCorrectResult()
    {
        // Arrange
        var context = new LockContext(
            lockId: "test-lock",
            lockType: LockType.StandardLock,
            toolQuality: ToolQuality.BareHands);

        // Act
        var result = LockpickingResult.Blocked(context, "You need tools");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.NarrativeDescription.Should().Be("You need tools");
    }

    #endregion

    #region Test 9: LockContext SkillContext Conversion

    /// <summary>
    /// Verifies that ToSkillContext includes tool modifier.
    /// </summary>
    [Test]
    public void LockContext_ToSkillContext_IncludesToolModifier()
    {
        // Arrange
        var context = new LockContext(
            lockId: "test-lock",
            lockType: LockType.StandardLock,
            toolQuality: ToolQuality.Masterwork);

        // Act
        var skillContext = context.ToSkillContext();

        // Assert
        skillContext.TotalDiceModifier.Should().Be(2, because: "Masterwork tools add +2 dice");
    }

    /// <summary>
    /// Verifies that ToSkillContext includes corruption modifier.
    /// </summary>
    [Test]
    public void LockContext_ToSkillContext_IncludesCorruptionModifier()
    {
        // Arrange
        var context = new LockContext(
            lockId: "test-lock",
            lockType: LockType.StandardLock,
            corruptionLevel: CorruptionTier.Blighted,
            toolQuality: ToolQuality.Proper);

        // Act
        var skillContext = context.ToSkillContext();

        // Assert
        skillContext.TotalDcModifier.Should().Be(4, because: "Blighted adds +4 DC");
    }

    /// <summary>
    /// Verifies that ToSkillContext includes jammed modifier.
    /// </summary>
    [Test]
    public void LockContext_ToSkillContext_IncludesJammedModifier()
    {
        // Arrange
        var context = new LockContext(
            lockId: "test-lock",
            lockType: LockType.StandardLock,
            toolQuality: ToolQuality.Proper,
            isJammed: true);

        // Act
        var skillContext = context.ToSkillContext();

        // Assert
        skillContext.TotalDcModifier.Should().Be(2, because: "Jammed adds +2 DC");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a dice roll result for testing.
    /// </summary>
    private static DiceRollResult CreateDiceResult(int successes, int botches)
    {
        // Create test rolls array that matches requested successes/botches
        var rolls = new List<int>();

        // Add success rolls (8-10)
        for (var i = 0; i < successes; i++)
            rolls.Add(8 + (i % 3)); // 8, 9, 10, 8, 9...

        // Add botch rolls (1)
        for (var i = 0; i < botches; i++)
            rolls.Add(1);

        // Add neutral rolls (2-7)
        rolls.Add(5);
        rolls.Add(4);

        return new DiceRollResult(
            pool: DicePool.D10(rolls.Count),
            rolls: rolls);
    }

    /// <summary>
    /// Creates a skill check result for testing.
    /// </summary>
    private static SkillCheckResult CreateSkillCheckResult(
        DiceRollResult diceResult,
        int dc,
        SkillOutcome outcome)
    {
        return new SkillCheckResult(
            skillId: "lockpicking",
            skillName: "Lockpicking",
            diceResult: diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: dc,
            difficultyName: "Test Lock");
    }

    #endregion
}
