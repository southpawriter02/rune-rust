// ------------------------------------------------------------------------------
// <copyright file="RhetoricSpecializationSystemTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the Rhetoric Specialization System including specialization
// abilities, ability effects, and extension methods.
// Part of v0.15.3i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.UnitTests.Services;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for the Rhetoric Specialization System components.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the required acceptance criteria from the design specification:
/// </para>
/// <list type="number">
///   <item>
///     <description>RhetoricSpecializationAbility correctly maps to archetypes and effects</description>
///   </item>
///   <item>
///     <description>Extension methods return correct values for all abilities</description>
///   </item>
///   <item>
///     <description>SpecializationAbilityEffect factory methods produce correct effects</description>
///   </item>
///   <item>
///     <description>GetAbilitiesForArchetype returns correct abilities per archetype</description>
///   </item>
///   <item>
///     <description>Dice bonuses, auto-success thresholds, and activation DCs are correct</description>
///   </item>
///   <item>
///     <description>Effect computed properties work correctly</description>
///   </item>
/// </list>
/// </remarks>
[TestFixture]
public class RhetoricSpecializationSystemTests
{
    #region Test 1: RhetoricSpecializationAbility GetDisplayName

    /// <summary>
    /// Verifies that all abilities have non-empty display names.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason)]
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority)]
    [TestCase(RhetoricSpecializationAbility.InspiringWords)]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes)]
    [TestCase(RhetoricSpecializationAbility.SilverTongue)]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies)]
    [TestCase(RhetoricSpecializationAbility.MaintainCover)]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments)]
    public void GetDisplayName_ReturnsNonEmpty(RhetoricSpecializationAbility ability)
    {
        // Act
        var displayName = ability.GetDisplayName();

        // Assert
        displayName.Should().NotBeNullOrWhiteSpace(
            because: $"{ability} should have a display name defined");
    }

    /// <summary>
    /// Verifies that display names match expected values.
    /// </summary>
    /// <param name="ability">The rhetoric specialization ability.</param>
    /// <param name="expectedName">The expected display name.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason, "Voice of Reason")]
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority, "Scholarly Authority")]
    [TestCase(RhetoricSpecializationAbility.InspiringWords, "Inspiring Words")]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes, "Saga of Heroes")]
    [TestCase(RhetoricSpecializationAbility.SilverTongue, "Silver Tongue")]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies, "Sniff Out Lies")]
    [TestCase(RhetoricSpecializationAbility.MaintainCover, "Maintain Cover")]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments, "Forge Documents")]
    public void GetDisplayName_ReturnsExpectedValue(
        RhetoricSpecializationAbility ability,
        string expectedName)
    {
        // Act
        var displayName = ability.GetDisplayName();

        // Assert
        displayName.Should().Be(expectedName,
            because: $"{ability} should have display name '{expectedName}'");
    }

    #endregion

    #region Test 2: Archetype Mappings

    /// <summary>
    /// Verifies that Thul abilities are correctly mapped.
    /// </summary>
    /// <param name="ability">A Thul ability.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason)]
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority)]
    public void GetArchetypeId_ThulAbilities_ReturnsThul(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var archetypeId = ability.GetArchetypeId();

        // Assert
        archetypeId.Should().Be("thul",
            because: $"{ability} is a Thul ability");
    }

    /// <summary>
    /// Verifies that Skald abilities are correctly mapped.
    /// </summary>
    /// <param name="ability">A Skald ability.</param>
    [TestCase(RhetoricSpecializationAbility.InspiringWords)]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes)]
    public void GetArchetypeId_SkaldAbilities_ReturnsSkald(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var archetypeId = ability.GetArchetypeId();

        // Assert
        archetypeId.Should().Be("skald",
            because: $"{ability} is a Skald ability");
    }

    /// <summary>
    /// Verifies that Kupmadr abilities are correctly mapped.
    /// </summary>
    /// <param name="ability">A Kupmadr ability.</param>
    [TestCase(RhetoricSpecializationAbility.SilverTongue)]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies)]
    public void GetArchetypeId_KupmadrAbilities_ReturnsKupmadr(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var archetypeId = ability.GetArchetypeId();

        // Assert
        archetypeId.Should().Be("kupmadr",
            because: $"{ability} is a Kupmaðr ability");
    }

    /// <summary>
    /// Verifies that Myrk-gengr abilities are correctly mapped.
    /// </summary>
    /// <param name="ability">A Myrk-gengr ability.</param>
    [TestCase(RhetoricSpecializationAbility.MaintainCover)]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments)]
    public void GetArchetypeId_MyrkGengrAbilities_ReturnsMyrkGengr(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var archetypeId = ability.GetArchetypeId();

        // Assert
        archetypeId.Should().Be("myrk-gengr",
            because: $"{ability} is a Myrk-gengr ability");
    }

    /// <summary>
    /// Verifies that GetAbilitiesForArchetype returns correct abilities for Thul.
    /// </summary>
    [Test]
    public void GetAbilitiesForArchetype_Thul_ReturnsThulAbilities()
    {
        // Act
        var abilities = RhetoricSpecializationAbilityExtensions.GetAbilitiesForArchetype("thul");

        // Assert
        abilities.Should().HaveCount(2);
        abilities.Should().Contain(RhetoricSpecializationAbility.VoiceOfReason);
        abilities.Should().Contain(RhetoricSpecializationAbility.ScholarlyAuthority);
    }

    /// <summary>
    /// Verifies that GetAbilitiesForArchetype returns correct abilities for Skald.
    /// </summary>
    [Test]
    public void GetAbilitiesForArchetype_Skald_ReturnsSkaldAbilities()
    {
        // Act
        var abilities = RhetoricSpecializationAbilityExtensions.GetAbilitiesForArchetype("skald");

        // Assert
        abilities.Should().HaveCount(2);
        abilities.Should().Contain(RhetoricSpecializationAbility.InspiringWords);
        abilities.Should().Contain(RhetoricSpecializationAbility.SagaOfHeroes);
    }

    /// <summary>
    /// Verifies that GetAbilitiesForArchetype returns correct abilities for Kupmadr.
    /// </summary>
    [Test]
    public void GetAbilitiesForArchetype_Kupmadr_ReturnsKupmadrAbilities()
    {
        // Act
        var abilities = RhetoricSpecializationAbilityExtensions.GetAbilitiesForArchetype("kupmadr");

        // Assert
        abilities.Should().HaveCount(2);
        abilities.Should().Contain(RhetoricSpecializationAbility.SilverTongue);
        abilities.Should().Contain(RhetoricSpecializationAbility.SniffOutLies);
    }

    /// <summary>
    /// Verifies that GetAbilitiesForArchetype returns correct abilities for Myrk-gengr.
    /// </summary>
    [Test]
    public void GetAbilitiesForArchetype_MyrkGengr_ReturnsMyrkGengrAbilities()
    {
        // Act
        var abilities = RhetoricSpecializationAbilityExtensions.GetAbilitiesForArchetype("myrk-gengr");

        // Assert
        abilities.Should().HaveCount(2);
        abilities.Should().Contain(RhetoricSpecializationAbility.MaintainCover);
        abilities.Should().Contain(RhetoricSpecializationAbility.ForgeDocuments);
    }

    /// <summary>
    /// Verifies that GetAbilitiesForArchetype returns empty for unknown archetype.
    /// </summary>
    [Test]
    public void GetAbilitiesForArchetype_UnknownArchetype_ReturnsEmpty()
    {
        // Act
        var abilities = RhetoricSpecializationAbilityExtensions.GetAbilitiesForArchetype("unknown");

        // Assert
        abilities.Should().BeEmpty(
            because: "unknown archetype should not have any abilities");
    }

    /// <summary>
    /// Verifies that GetAbilitiesForArchetype handles null input gracefully.
    /// </summary>
    [Test]
    public void GetAbilitiesForArchetype_Null_ReturnsEmpty()
    {
        // Act
        var abilities = RhetoricSpecializationAbilityExtensions.GetAbilitiesForArchetype(null!);

        // Assert
        abilities.Should().BeEmpty(
            because: "null archetype should return empty array");
    }

    /// <summary>
    /// Verifies that GetAbilitiesForArchetype is case-insensitive.
    /// </summary>
    [TestCase("THUL")]
    [TestCase("Thul")]
    [TestCase("thul")]
    [TestCase("SKALD")]
    [TestCase("Skald")]
    public void GetAbilitiesForArchetype_IsCaseInsensitive(string archetypeId)
    {
        // Act
        var abilities = RhetoricSpecializationAbilityExtensions.GetAbilitiesForArchetype(archetypeId);

        // Assert
        abilities.Should().HaveCount(2,
            because: "archetype lookup should be case-insensitive");
    }

    #endregion

    #region Test 3: Dice Bonus Values

    /// <summary>
    /// Verifies that GetDiceBonus returns correct values for dice-bonus abilities.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    /// <param name="expectedBonus">The expected dice bonus.</param>
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority, 2)]
    [TestCase(RhetoricSpecializationAbility.InspiringWords, 1)]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies, 2)]
    [TestCase(RhetoricSpecializationAbility.MaintainCover, 2)]
    public void GetDiceBonus_DiceBonusAbilities_ReturnsCorrectValue(
        RhetoricSpecializationAbility ability,
        int expectedBonus)
    {
        // Act
        var bonus = ability.GetDiceBonus();

        // Assert
        bonus.Should().Be(expectedBonus,
            because: $"{ability} should grant +{expectedBonus}d10");
    }

    /// <summary>
    /// Verifies that GetDiceBonus returns 0 for non-dice-bonus abilities.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason)]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes)]
    [TestCase(RhetoricSpecializationAbility.SilverTongue)]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments)]
    public void GetDiceBonus_NonDiceBonusAbilities_ReturnsZero(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var bonus = ability.GetDiceBonus();

        // Assert
        bonus.Should().Be(0,
            because: $"{ability} does not provide dice bonus directly");
    }

    /// <summary>
    /// Verifies that ModifiesDicePool returns true for dice bonus abilities.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority)]
    [TestCase(RhetoricSpecializationAbility.InspiringWords)]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies)]
    [TestCase(RhetoricSpecializationAbility.MaintainCover)]
    public void ModifiesDicePool_DiceBonusAbilities_ReturnsTrue(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var modifiesDicePool = ability.ModifiesDicePool();

        // Assert
        modifiesDicePool.Should().BeTrue(
            because: $"{ability} adds bonus dice to the pool");
    }

    #endregion

    #region Test 4: Auto-Success Threshold

    /// <summary>
    /// Verifies that Silver Tongue has the correct auto-success threshold.
    /// </summary>
    [Test]
    public void SilverTongue_GetAutoSuccessThreshold_Returns12()
    {
        // Arrange
        var ability = RhetoricSpecializationAbility.SilverTongue;

        // Act
        var threshold = ability.GetAutoSuccessThreshold();

        // Assert
        threshold.Should().Be(12,
            because: "Silver Tongue auto-succeeds on DC ≤ 12");
    }

    /// <summary>
    /// Verifies that CanBypassCheck returns true only for Silver Tongue.
    /// </summary>
    [Test]
    public void CanBypassCheck_SilverTongue_ReturnsTrue()
    {
        // Act
        var canBypass = RhetoricSpecializationAbility.SilverTongue.CanBypassCheck();

        // Assert
        canBypass.Should().BeTrue(
            because: "Silver Tongue can auto-succeed on low DC negotiations");
    }

    /// <summary>
    /// Verifies that CanBypassCheck returns false for other abilities.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason)]
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority)]
    [TestCase(RhetoricSpecializationAbility.InspiringWords)]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes)]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies)]
    [TestCase(RhetoricSpecializationAbility.MaintainCover)]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments)]
    public void CanBypassCheck_NonAutoSuccessAbilities_ReturnsFalse(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var canBypass = ability.CanBypassCheck();

        // Assert
        canBypass.Should().BeFalse(
            because: $"{ability} cannot bypass checks");
    }

    #endregion

    #region Test 5: Activation DC

    /// <summary>
    /// Verifies that Inspiring Words has DC 12 activation.
    /// </summary>
    [Test]
    public void InspiringWords_GetActivationDc_Returns12()
    {
        // Act
        var dc = RhetoricSpecializationAbility.InspiringWords.GetActivationDc();

        // Assert
        dc.Should().Be(12,
            because: "Inspiring Words requires DC 12 Rhetoric check to activate");
    }

    /// <summary>
    /// Verifies that Saga of Heroes has DC 10 activation.
    /// </summary>
    [Test]
    public void SagaOfHeroes_GetActivationDc_Returns10()
    {
        // Act
        var dc = RhetoricSpecializationAbility.SagaOfHeroes.GetActivationDc();

        // Assert
        dc.Should().Be(10,
            because: "Saga of Heroes requires DC 10 Rhetoric check to activate");
    }

    /// <summary>
    /// Verifies that RequiresActivationCheck identifies correct abilities.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    [TestCase(RhetoricSpecializationAbility.InspiringWords)]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes)]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments)]
    public void RequiresActivationCheck_ActivationAbilities_ReturnsTrue(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var requires = ability.RequiresActivationCheck();

        // Assert
        requires.Should().BeTrue(
            because: $"{ability} requires a skill check to activate");
    }

    #endregion

    #region Test 6: Party-Affecting Abilities

    /// <summary>
    /// Verifies that AffectsParty returns true for party-wide abilities.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    [TestCase(RhetoricSpecializationAbility.InspiringWords)]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes)]
    public void AffectsParty_PartyWideAbilities_ReturnsTrue(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var affectsParty = ability.AffectsParty();

        // Assert
        affectsParty.Should().BeTrue(
            because: $"{ability} can affect party members");
    }

    /// <summary>
    /// Verifies that AffectsParty returns false for single-target abilities.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason)]
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority)]
    [TestCase(RhetoricSpecializationAbility.SilverTongue)]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies)]
    [TestCase(RhetoricSpecializationAbility.MaintainCover)]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments)]
    public void AffectsParty_SingleTargetAbilities_ReturnsFalse(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var affectsParty = ability.AffectsParty();

        // Assert
        affectsParty.Should().BeFalse(
            because: $"{ability} only affects the character");
    }

    #endregion

    #region Test 7: Outcome Modification

    /// <summary>
    /// Verifies that Voice of Reason modifies outcomes.
    /// </summary>
    [Test]
    public void VoiceOfReason_ModifiesOutcome_ReturnsTrue()
    {
        // Act
        var modifies = RhetoricSpecializationAbility.VoiceOfReason.ModifiesOutcome();

        // Assert
        modifies.Should().BeTrue(
            because: "Voice of Reason prevents option locking on failure");
    }

    /// <summary>
    /// Verifies that Maintain Cover modifies outcomes (fumble downgrade).
    /// </summary>
    [Test]
    public void MaintainCover_ModifiesOutcome_ReturnsTrue()
    {
        // Act
        var modifies = RhetoricSpecializationAbility.MaintainCover.ModifiesOutcome();

        // Assert
        modifies.Should().BeTrue(
            because: "Maintain Cover downgrades fumble consequences");
    }

    #endregion

    #region Test 8: Asset Creation

    /// <summary>
    /// Verifies that only ForgeDocuments creates assets.
    /// </summary>
    [Test]
    public void ForgeDocuments_CreatesAsset_ReturnsTrue()
    {
        // Act
        var creates = RhetoricSpecializationAbility.ForgeDocuments.CreatesAsset();

        // Assert
        creates.Should().BeTrue(
            because: "Forge Documents creates forged document assets");
    }

    /// <summary>
    /// Verifies that other abilities don't create assets.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason)]
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority)]
    [TestCase(RhetoricSpecializationAbility.InspiringWords)]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes)]
    [TestCase(RhetoricSpecializationAbility.SilverTongue)]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies)]
    [TestCase(RhetoricSpecializationAbility.MaintainCover)]
    public void CreatesAsset_NonAssetAbilities_ReturnsFalse(
        RhetoricSpecializationAbility ability)
    {
        // Act
        var creates = ability.CreatesAsset();

        // Assert
        creates.Should().BeFalse(
            because: $"{ability} does not create assets");
    }

    #endregion

    #region Test 9: Stress Reduction

    /// <summary>
    /// Verifies that Saga of Heroes provides stress reduction.
    /// </summary>
    [Test]
    public void SagaOfHeroes_GetBaseStressReduction_ReturnsPositive()
    {
        // Act
        var reduction = RhetoricSpecializationAbility.SagaOfHeroes.GetBaseStressReduction();

        // Assert
        reduction.Should().BeGreaterThan(0,
            because: "Saga of Heroes provides party stress reduction");
    }

    /// <summary>
    /// Verifies that Maintain Cover provides stress reduction.
    /// </summary>
    [Test]
    public void MaintainCover_GetBaseStressReduction_Returns1()
    {
        // Act
        var reduction = RhetoricSpecializationAbility.MaintainCover.GetBaseStressReduction();

        // Assert
        reduction.Should().Be(1,
            because: "Maintain Cover reduces stress by 1 when cover is challenged");
    }

    #endregion

    #region Test 10: Ability Types

    /// <summary>
    /// Verifies that GetAbilityType returns correct types.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    /// <param name="expectedType">The expected ability type.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason, "Outcome Modification")]
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority, "Dice Bonus")]
    [TestCase(RhetoricSpecializationAbility.InspiringWords, "Party Buff")]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes, "Stress Relief")]
    [TestCase(RhetoricSpecializationAbility.SilverTongue, "Auto-Success")]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies, "Dice Bonus")]
    [TestCase(RhetoricSpecializationAbility.MaintainCover, "Composite")]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments, "Asset Creation")]
    public void GetAbilityType_ReturnsCorrectType(
        RhetoricSpecializationAbility ability,
        string expectedType)
    {
        // Act
        var type = ability.GetAbilityType();

        // Assert
        type.Should().Be(expectedType,
            because: $"{ability} is categorized as {expectedType}");
    }

    #endregion

    #region Test 11: SpecializationAbilityEffect Factory Methods

    /// <summary>
    /// Verifies that DiceBonusEffect creates correct effect.
    /// </summary>
    [Test]
    public void DiceBonusEffect_CreatesEffectWithBonus()
    {
        // Act
        var effect = SpecializationAbilityEffect.DiceBonusEffect(
            RhetoricSpecializationAbility.ScholarlyAuthority,
            2,
            "Test description");

        // Assert
        effect.Ability.Should().Be(RhetoricSpecializationAbility.ScholarlyAuthority);
        effect.DiceBonus.Should().Be(2);
        effect.ModifiesDicePool.Should().BeTrue();
        effect.HasEffect.Should().BeTrue();
        effect.AutoSuccess.Should().BeFalse();
        effect.PreventOptionLock.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that AutoSucceedEffect creates correct effect.
    /// </summary>
    [Test]
    public void AutoSucceedEffect_CreatesEffectWithThreshold()
    {
        // Act
        var effect = SpecializationAbilityEffect.AutoSucceedEffect(
            RhetoricSpecializationAbility.SilverTongue,
            12,
            "Test description");

        // Assert
        effect.Ability.Should().Be(RhetoricSpecializationAbility.SilverTongue);
        effect.AutoSuccess.Should().BeTrue();
        effect.AutoSuccessThreshold.Should().Be(12);
        effect.BypassesCheck.Should().BeTrue();
        effect.HasEffect.Should().BeTrue();
        effect.DiceBonus.Should().Be(0);
    }

    /// <summary>
    /// Verifies that PreventLockEffect creates correct effect.
    /// </summary>
    [Test]
    public void PreventLockEffect_CreatesEffectThatPreventsLocking()
    {
        // Act
        var effect = SpecializationAbilityEffect.PreventLockEffect(
            RhetoricSpecializationAbility.VoiceOfReason,
            "Test description");

        // Assert
        effect.Ability.Should().Be(RhetoricSpecializationAbility.VoiceOfReason);
        effect.PreventOptionLock.Should().BeTrue();
        effect.ModifiesOutcome.Should().BeTrue();
        effect.HasEffect.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that StressReliefEffect creates party-wide effect.
    /// </summary>
    [Test]
    public void StressReliefEffect_CreatesPartyWideEffect()
    {
        // Act
        var effect = SpecializationAbilityEffect.StressReliefEffect(
            RhetoricSpecializationAbility.SagaOfHeroes,
            2,
            "Test description");

        // Assert
        effect.Ability.Should().Be(RhetoricSpecializationAbility.SagaOfHeroes);
        effect.StressReduction.Should().Be(2);
        effect.IsPartyWide.Should().BeTrue();
        effect.AffectsParty.Should().BeTrue();
        effect.ReducesStress.Should().BeTrue();
        effect.HasEffect.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that InspirationEffect creates effect with duration.
    /// </summary>
    [Test]
    public void InspirationEffect_CreatesEffectWithDuration()
    {
        // Act
        var effect = SpecializationAbilityEffect.InspirationEffect(
            1,
            false,
            "Test description");

        // Assert
        effect.Ability.Should().Be(RhetoricSpecializationAbility.InspiringWords);
        effect.DiceBonus.Should().Be(1);
        effect.Duration.Should().Be(TimeSpan.FromMinutes(10));
        effect.HasDuration.Should().BeTrue();
        effect.IsPartyWide.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that InspirationEffect with partyWide=true creates party-wide effect.
    /// </summary>
    [Test]
    public void InspirationEffect_PartyWide_CreatesPartyWideEffect()
    {
        // Act
        var effect = SpecializationAbilityEffect.InspirationEffect(
            1,
            true,
            "Test description");

        // Assert
        effect.IsPartyWide.Should().BeTrue();
        effect.AffectsParty.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that MaintainCoverEffect creates composite effect.
    /// </summary>
    [Test]
    public void MaintainCoverEffect_CreatesCompositeEffect()
    {
        // Act
        var effect = SpecializationAbilityEffect.MaintainCoverEffect("Test description");

        // Assert
        effect.Ability.Should().Be(RhetoricSpecializationAbility.MaintainCover);
        effect.DiceBonus.Should().Be(2);
        effect.StressReduction.Should().Be(1);
        effect.FumbleDowngrade.Should().BeTrue();
        effect.ModifiesDicePool.Should().BeTrue();
        effect.ModifiesOutcome.Should().BeTrue();
        effect.ReducesStress.Should().BeTrue();
        effect.HasEffect.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ForgeryEffect creates asset-creating effect.
    /// </summary>
    [Test]
    public void ForgeryEffect_CreatesAssetCreatingEffect()
    {
        // Act
        var effect = SpecializationAbilityEffect.ForgeryEffect(3, "Test description");

        // Assert
        effect.Ability.Should().Be(RhetoricSpecializationAbility.ForgeDocuments);
        effect.CreatesAsset.Should().BeTrue();
        effect.AssetType.Should().Be("ForgedDocument");
        effect.AssetQuality.Should().Be(3);
        effect.ProducesAsset.Should().BeTrue();
        effect.HasEffect.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that None effect has no effect.
    /// </summary>
    [Test]
    public void None_HasNoEffect()
    {
        // Act
        var effect = SpecializationAbilityEffect.None;

        // Assert
        effect.HasEffect.Should().BeFalse();
        effect.ModifiesDicePool.Should().BeFalse();
        effect.ModifiesOutcome.Should().BeFalse();
        effect.BypassesCheck.Should().BeFalse();
        effect.ReducesStress.Should().BeFalse();
        effect.ProducesAsset.Should().BeFalse();
    }

    #endregion

    #region Test 12: Effect Display Methods

    /// <summary>
    /// Verifies that ToDisplayString returns correct output for dice bonus effect.
    /// </summary>
    [Test]
    public void ToDisplayString_DiceBonusEffect_IncludesDiceBonus()
    {
        // Arrange
        var effect = SpecializationAbilityEffect.DiceBonusEffect(
            RhetoricSpecializationAbility.ScholarlyAuthority,
            2,
            "Test");

        // Act
        var display = effect.ToDisplayString();

        // Assert
        display.Should().Contain("+2d10");
    }

    /// <summary>
    /// Verifies that ToDisplayString returns correct output for auto-success effect.
    /// </summary>
    [Test]
    public void ToDisplayString_AutoSuccessEffect_IncludesThreshold()
    {
        // Arrange
        var effect = SpecializationAbilityEffect.AutoSucceedEffect(
            RhetoricSpecializationAbility.SilverTongue,
            12,
            "Test");

        // Act
        var display = effect.ToDisplayString();

        // Assert
        display.Should().Contain("Auto-success");
        display.Should().Contain("12");
    }

    /// <summary>
    /// Verifies that ToDisplayString returns "No effect" for None effect.
    /// </summary>
    [Test]
    public void ToDisplayString_None_ReturnsNoEffect()
    {
        // Act
        var display = SpecializationAbilityEffect.None.ToDisplayString();

        // Assert
        display.Should().Be("No effect");
    }

    /// <summary>
    /// Verifies that ToLogString includes ability name.
    /// </summary>
    [Test]
    public void ToLogString_IncludesAbilityName()
    {
        // Arrange
        var effect = SpecializationAbilityEffect.DiceBonusEffect(
            RhetoricSpecializationAbility.ScholarlyAuthority,
            2,
            "Test");

        // Act
        var log = effect.ToLogString();

        // Assert
        log.Should().Contain("ScholarlyAuthority");
    }

    #endregion

    #region Test 13: Color and Icon Hints

    /// <summary>
    /// Verifies that GetColorHint returns archetype-consistent colors.
    /// </summary>
    [Test]
    public void GetColorHint_ThulAbilities_ReturnBlue()
    {
        // Act & Assert
        RhetoricSpecializationAbility.VoiceOfReason.GetColorHint()
            .Should().Be("Blue");
        RhetoricSpecializationAbility.ScholarlyAuthority.GetColorHint()
            .Should().Be("Blue");
    }

    /// <summary>
    /// Verifies that GetColorHint returns archetype-consistent colors for Skald.
    /// </summary>
    [Test]
    public void GetColorHint_SkaldAbilities_ReturnGold()
    {
        // Act & Assert
        RhetoricSpecializationAbility.InspiringWords.GetColorHint()
            .Should().Be("Gold");
        RhetoricSpecializationAbility.SagaOfHeroes.GetColorHint()
            .Should().Be("Gold");
    }

    /// <summary>
    /// Verifies that GetIconHint returns non-empty values for all abilities.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason)]
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority)]
    [TestCase(RhetoricSpecializationAbility.InspiringWords)]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes)]
    [TestCase(RhetoricSpecializationAbility.SilverTongue)]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies)]
    [TestCase(RhetoricSpecializationAbility.MaintainCover)]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments)]
    public void GetIconHint_ReturnsNonEmpty(RhetoricSpecializationAbility ability)
    {
        // Act
        var icon = ability.GetIconHint();

        // Assert
        icon.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Test 14: Flavor Text

    /// <summary>
    /// Verifies that all abilities have flavor text.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason)]
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority)]
    [TestCase(RhetoricSpecializationAbility.InspiringWords)]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes)]
    [TestCase(RhetoricSpecializationAbility.SilverTongue)]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies)]
    [TestCase(RhetoricSpecializationAbility.MaintainCover)]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments)]
    public void GetFlavorText_ReturnsNonEmpty(RhetoricSpecializationAbility ability)
    {
        // Act
        var flavorText = ability.GetFlavorText();

        // Assert
        flavorText.Should().NotBeNullOrWhiteSpace(
            because: $"{ability} should have flavor text for narrative display");
    }

    #endregion

    #region Test 15: Description

    /// <summary>
    /// Verifies that all abilities have descriptions.
    /// </summary>
    /// <param name="ability">The ability to test.</param>
    [TestCase(RhetoricSpecializationAbility.VoiceOfReason)]
    [TestCase(RhetoricSpecializationAbility.ScholarlyAuthority)]
    [TestCase(RhetoricSpecializationAbility.InspiringWords)]
    [TestCase(RhetoricSpecializationAbility.SagaOfHeroes)]
    [TestCase(RhetoricSpecializationAbility.SilverTongue)]
    [TestCase(RhetoricSpecializationAbility.SniffOutLies)]
    [TestCase(RhetoricSpecializationAbility.MaintainCover)]
    [TestCase(RhetoricSpecializationAbility.ForgeDocuments)]
    public void GetDescription_ReturnsNonEmpty(RhetoricSpecializationAbility ability)
    {
        // Act
        var description = ability.GetDescription();

        // Assert
        description.Should().NotBeNullOrWhiteSpace(
            because: $"{ability} should have a description for tooltips");
    }

    #endregion

    #region Test 16: Enum Values

    /// <summary>
    /// Verifies that enum values are as expected.
    /// </summary>
    [Test]
    public void EnumValues_HaveExpectedOrdinalValues()
    {
        // Assert
        ((int)RhetoricSpecializationAbility.VoiceOfReason).Should().Be(0);
        ((int)RhetoricSpecializationAbility.ScholarlyAuthority).Should().Be(1);
        ((int)RhetoricSpecializationAbility.InspiringWords).Should().Be(2);
        ((int)RhetoricSpecializationAbility.SagaOfHeroes).Should().Be(3);
        ((int)RhetoricSpecializationAbility.SilverTongue).Should().Be(4);
        ((int)RhetoricSpecializationAbility.SniffOutLies).Should().Be(5);
        ((int)RhetoricSpecializationAbility.MaintainCover).Should().Be(6);
        ((int)RhetoricSpecializationAbility.ForgeDocuments).Should().Be(7);
    }

    /// <summary>
    /// Verifies that there are exactly 8 abilities (2 per archetype).
    /// </summary>
    [Test]
    public void Enum_HasExactly8Values()
    {
        // Act
        var count = Enum.GetValues<RhetoricSpecializationAbility>().Length;

        // Assert
        count.Should().Be(8,
            because: "there are 4 archetypes with 2 abilities each");
    }

    #endregion
}
