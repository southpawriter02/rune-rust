// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationAbilityTierTests.cs
// Unit tests for the SpecializationAbilityTier value object, verifying factory
// method validation, convenience factories, computed properties, unlock checks,
// and display formatting.
// Version: 0.17.4c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class SpecializationAbilityTierTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a standard set of test abilities for tier construction.
    /// </summary>
    private static List<SpecializationAbility> CreateTestAbilities(int count = 3)
    {
        var abilities = new List<SpecializationAbility>();

        if (count >= 1)
        {
            abilities.Add(SpecializationAbility.CreateActive(
                "rage-strike", "Rage Strike",
                "Channel rage into a devastating blow.",
                resourceCost: 20, resourceType: "rage",
                corruptionRisk: 5));
        }

        if (count >= 2)
        {
            abilities.Add(SpecializationAbility.CreatePassive(
                "blood-frenzy", "Blood Frenzy",
                "When below 50% health, deal 10% increased damage."));
        }

        if (count >= 3)
        {
            abilities.Add(SpecializationAbility.CreateActive(
                "intimidating-presence", "Intimidating Presence",
                "Your fury terrifies nearby enemies.",
                resourceCost: 15, resourceType: "rage",
                cooldown: 3));
        }

        return abilities;
    }

    /// <summary>
    /// Creates a different set of abilities for tier 2 (no ID overlap with tier 1).
    /// </summary>
    private static List<SpecializationAbility> CreateTier2Abilities()
    {
        return new List<SpecializationAbility>
        {
            SpecializationAbility.CreateActive(
                "berserker-charge", "Berserker Charge",
                "Rush toward an enemy, stunning them on impact.",
                resourceCost: 30, resourceType: "rage",
                cooldown: 4, corruptionRisk: 10),
            SpecializationAbility.CreatePassive(
                "pain-is-power", "Pain is Power",
                "Convert 25% of damage taken into Rage."),
            SpecializationAbility.CreateActive(
                "reckless-swing", "Reckless Swing",
                "A powerful attack that leaves you vulnerable.",
                resourceCost: 25, resourceType: "rage",
                cooldown: 2, corruptionRisk: 5)
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE — VALID PARAMETERS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidTier1Parameters_CreatesTier()
    {
        // Arrange
        var abilities = CreateTestAbilities();

        // Act
        var tier = SpecializationAbilityTier.Create(
            tier: 1,
            displayName: "Primal Fury",
            unlockCost: 0,
            requiresPreviousTier: false,
            requiredRank: 1,
            abilities: abilities);

        // Assert
        tier.Tier.Should().Be(1);
        tier.DisplayName.Should().Be("Primal Fury");
        tier.UnlockCost.Should().Be(0);
        tier.RequiresPreviousTier.Should().BeFalse();
        tier.RequiredRank.Should().Be(1);
        tier.Abilities.Should().HaveCount(3);
    }

    [Test]
    public void Create_WithValidTier2Parameters_CreatesTier()
    {
        // Arrange
        var abilities = CreateTier2Abilities();

        // Act
        var tier = SpecializationAbilityTier.Create(
            tier: 2,
            displayName: "Unleashed Beast",
            unlockCost: 2,
            requiresPreviousTier: true,
            requiredRank: 2,
            abilities: abilities);

        // Assert
        tier.Tier.Should().Be(2);
        tier.DisplayName.Should().Be("Unleashed Beast");
        tier.UnlockCost.Should().Be(2);
        tier.RequiresPreviousTier.Should().BeTrue();
        tier.RequiredRank.Should().Be(2);
    }

    [Test]
    public void Create_WithValidTier3Parameters_CreatesTier()
    {
        // Arrange
        var abilities = CreateTestAbilities(2);

        // Act
        var tier = SpecializationAbilityTier.Create(
            tier: 3,
            displayName: "Avatar of Destruction",
            unlockCost: 3,
            requiresPreviousTier: true,
            requiredRank: 3,
            abilities: abilities);

        // Assert
        tier.Tier.Should().Be(3);
        tier.UnlockCost.Should().Be(3);
        tier.RequiresPreviousTier.Should().BeTrue();
        tier.RequiredRank.Should().Be(3);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONVENIENCE FACTORIES
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateTier1_SetsStandardTier1Parameters()
    {
        // Arrange
        var abilities = CreateTestAbilities();

        // Act
        var tier = SpecializationAbilityTier.CreateTier1("Primal Fury", abilities);

        // Assert
        tier.Tier.Should().Be(1);
        tier.UnlockCost.Should().Be(0);
        tier.RequiresPreviousTier.Should().BeFalse();
        tier.RequiredRank.Should().Be(1);
        tier.DisplayName.Should().Be("Primal Fury");
    }

    [Test]
    public void CreateTier2_SetsStandardTier2Parameters()
    {
        // Arrange
        var abilities = CreateTier2Abilities();

        // Act
        var tier = SpecializationAbilityTier.CreateTier2("Unleashed Beast", abilities);

        // Assert
        tier.Tier.Should().Be(2);
        tier.UnlockCost.Should().Be(2);
        tier.RequiresPreviousTier.Should().BeTrue();
        tier.RequiredRank.Should().Be(2);
        tier.DisplayName.Should().Be("Unleashed Beast");
    }

    [Test]
    public void CreateTier3_SetsStandardTier3Parameters()
    {
        // Arrange
        var abilities = CreateTestAbilities(2);

        // Act
        var tier = SpecializationAbilityTier.CreateTier3("Avatar of Destruction", abilities);

        // Assert
        tier.Tier.Should().Be(3);
        tier.UnlockCost.Should().Be(3);
        tier.RequiresPreviousTier.Should().BeTrue();
        tier.RequiredRank.Should().Be(3);
        tier.DisplayName.Should().Be("Avatar of Destruction");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION — TIER NUMBER RANGE
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_TierBelowRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 0,
            displayName: "Invalid",
            unlockCost: 0,
            requiresPreviousTier: false,
            requiredRank: 1,
            abilities: CreateTestAbilities());

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("tier");
    }

    [Test]
    public void Create_TierAboveRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 4,
            displayName: "Invalid",
            unlockCost: 0,
            requiresPreviousTier: true,
            requiredRank: 4,
            abilities: CreateTestAbilities());

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("tier");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION — DISPLAY NAME
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 1,
            displayName: null!,
            unlockCost: 0,
            requiresPreviousTier: false,
            requiredRank: 1,
            abilities: CreateTestAbilities());

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("displayName");
    }

    [Test]
    public void Create_WithEmptyDisplayName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 1,
            displayName: "",
            unlockCost: 0,
            requiresPreviousTier: false,
            requiredRank: 1,
            abilities: CreateTestAbilities());

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("displayName");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION — UNLOCK COST
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithNegativeUnlockCost_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 1,
            displayName: "Core",
            unlockCost: -1,
            requiresPreviousTier: false,
            requiredRank: 1,
            abilities: CreateTestAbilities());

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("unlockCost");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION — REQUIRED RANK
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithZeroRequiredRank_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 1,
            displayName: "Core",
            unlockCost: 0,
            requiresPreviousTier: false,
            requiredRank: 0,
            abilities: CreateTestAbilities());

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("requiredRank");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION — ABILITIES
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithNullAbilities_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 1,
            displayName: "Core",
            unlockCost: 0,
            requiresPreviousTier: false,
            requiredRank: 1,
            abilities: null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("abilities");
    }

    [Test]
    public void Create_WithEmptyAbilities_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 1,
            displayName: "Core",
            unlockCost: 0,
            requiresPreviousTier: false,
            requiredRank: 1,
            abilities: new List<SpecializationAbility>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("abilities");
    }

    [Test]
    public void Create_WithDuplicateAbilityIds_ThrowsArgumentException()
    {
        // Arrange
        var abilities = new List<SpecializationAbility>
        {
            SpecializationAbility.CreateActive(
                "rage-strike", "Rage Strike", "A blow.", 20, "rage"),
            SpecializationAbility.CreatePassive(
                "rage-strike", "Rage Strike Passive", "A passive blow.")
        };

        // Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 1,
            displayName: "Core",
            unlockCost: 0,
            requiresPreviousTier: false,
            requiredRank: 1,
            abilities: abilities);

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("abilities");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION — PREVIOUS TIER CONSTRAINTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_Tier1WithRequiresPreviousTier_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 1,
            displayName: "Core",
            unlockCost: 0,
            requiresPreviousTier: true,
            requiredRank: 1,
            abilities: CreateTestAbilities());

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("requiresPreviousTier");
    }

    [Test]
    public void Create_Tier2WithoutRequiresPreviousTier_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 2,
            displayName: "Advanced",
            unlockCost: 2,
            requiresPreviousTier: false,
            requiredRank: 2,
            abilities: CreateTier2Abilities());

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("requiresPreviousTier");
    }

    [Test]
    public void Create_Tier3WithoutRequiresPreviousTier_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecializationAbilityTier.Create(
            tier: 3,
            displayName: "Ultimate",
            unlockCost: 3,
            requiresPreviousTier: false,
            requiredRank: 3,
            abilities: CreateTestAbilities(2));

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("requiresPreviousTier");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void AbilityCount_ReturnsCorrectCount()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier1(
            "Core", CreateTestAbilities(3));

        // Assert
        tier.AbilityCount.Should().Be(3);
    }

    [Test]
    public void PassiveCount_ReturnsCorrectCount()
    {
        // Arrange — CreateTestAbilities(3) has 1 passive (blood-frenzy)
        var tier = SpecializationAbilityTier.CreateTier1(
            "Core", CreateTestAbilities(3));

        // Assert
        tier.PassiveCount.Should().Be(1);
    }

    [Test]
    public void ActiveCount_ReturnsCorrectCount()
    {
        // Arrange — CreateTestAbilities(3) has 2 active (rage-strike, intimidating-presence)
        var tier = SpecializationAbilityTier.CreateTier1(
            "Core", CreateTestAbilities(3));

        // Assert
        tier.ActiveCount.Should().Be(2);
    }

    [Test]
    public void AbilityCount_WithSingleAbility_ReturnsOne()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier1(
            "Core", CreateTestAbilities(1));

        // Assert
        tier.AbilityCount.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CAN UNLOCK
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CanUnlock_Tier1WithAllRequirementsMet_ReturnsTrue()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier1(
            "Core", CreateTestAbilities());

        // Act
        var (canUnlock, reason) = tier.CanUnlock(
            currentRank: 1,
            previousTierUnlocked: true,
            availablePp: 0);

        // Assert
        canUnlock.Should().BeTrue();
        reason.Should().BeNull();
    }

    [Test]
    public void CanUnlock_Tier2WithAllRequirementsMet_ReturnsTrue()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier2(
            "Advanced", CreateTier2Abilities());

        // Act
        var (canUnlock, reason) = tier.CanUnlock(
            currentRank: 2,
            previousTierUnlocked: true,
            availablePp: 5);

        // Assert
        canUnlock.Should().BeTrue();
        reason.Should().BeNull();
    }

    [Test]
    public void CanUnlock_Tier2WithExactPP_ReturnsTrue()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier2(
            "Advanced", CreateTier2Abilities());

        // Act
        var (canUnlock, reason) = tier.CanUnlock(
            currentRank: 2,
            previousTierUnlocked: true,
            availablePp: 2);

        // Assert
        canUnlock.Should().BeTrue();
        reason.Should().BeNull();
    }

    [Test]
    public void CanUnlock_WithoutPreviousTier_ReturnsFalse()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier2(
            "Advanced", CreateTier2Abilities());

        // Act
        var (canUnlock, reason) = tier.CanUnlock(
            currentRank: 2,
            previousTierUnlocked: false,
            availablePp: 5);

        // Assert
        canUnlock.Should().BeFalse();
        reason.Should().Contain("Tier 1 must be unlocked first");
    }

    [Test]
    public void CanUnlock_InsufficientRank_ReturnsFalse()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier2(
            "Advanced", CreateTier2Abilities());

        // Act
        var (canUnlock, reason) = tier.CanUnlock(
            currentRank: 1,
            previousTierUnlocked: true,
            availablePp: 5);

        // Assert
        canUnlock.Should().BeFalse();
        reason.Should().Contain("Requires Rank 2");
    }

    [Test]
    public void CanUnlock_InsufficientPP_ReturnsFalse()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier2(
            "Advanced", CreateTier2Abilities());

        // Act
        var (canUnlock, reason) = tier.CanUnlock(
            currentRank: 2,
            previousTierUnlocked: true,
            availablePp: 1);

        // Assert
        canUnlock.Should().BeFalse();
        reason.Should().Contain("Requires 2 PP");
        reason.Should().Contain("1 available");
    }

    [Test]
    public void CanUnlock_Tier3WithoutTier2_ReturnsFalse()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier3(
            "Ultimate", CreateTestAbilities(2));

        // Act
        var (canUnlock, reason) = tier.CanUnlock(
            currentRank: 3,
            previousTierUnlocked: false,
            availablePp: 10);

        // Assert
        canUnlock.Should().BeFalse();
        reason.Should().Contain("Tier 2 must be unlocked first");
    }

    [Test]
    public void CanUnlock_ChecksPreviousTierBeforeRank()
    {
        // Arrange — Both previous tier and rank fail, but previous tier should be checked first
        var tier = SpecializationAbilityTier.CreateTier2(
            "Advanced", CreateTier2Abilities());

        // Act
        var (canUnlock, reason) = tier.CanUnlock(
            currentRank: 1,
            previousTierUnlocked: false,
            availablePp: 0);

        // Assert
        canUnlock.Should().BeFalse();
        reason.Should().Contain("Tier 1 must be unlocked first");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier1(
            "Primal Fury", CreateTestAbilities());

        // Assert
        tier.ToString().Should().Be("Tier 1: Primal Fury (3 abilities, 0 PP)");
    }

    [Test]
    public void ToString_Tier2_ReturnsFormattedString()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier2(
            "Unleashed Beast", CreateTier2Abilities());

        // Assert
        tier.ToString().Should().Be("Tier 2: Unleashed Beast (3 abilities, 2 PP)");
    }

    [Test]
    public void GetShortDisplay_ReturnsFormattedString()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier1(
            "Primal Fury", CreateTestAbilities());

        // Assert
        tier.GetShortDisplay().Should().Be("Tier 1: Primal Fury — 3 abilities (0 PP)");
    }

    [Test]
    public void GetDetailDisplay_ContainsTierInfoAndAbilities()
    {
        // Arrange
        var tier = SpecializationAbilityTier.CreateTier2(
            "Unleashed Beast", CreateTier2Abilities());

        // Act
        var display = tier.GetDetailDisplay();

        // Assert
        display.Should().Contain("Tier 2: Unleashed Beast");
        display.Should().Contain("Unlock Cost: 2 PP");
        display.Should().Contain("Required Rank: 2");
        display.Should().Contain("Requires Previous Tier: Yes");
        display.Should().Contain("3 total");
        display.Should().Contain("Berserker Charge");
        display.Should().Contain("Pain is Power");
        display.Should().Contain("Reckless Swing");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NORMALIZATION
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_TrimsDisplayName()
    {
        // Arrange & Act
        var tier = SpecializationAbilityTier.Create(
            tier: 1,
            displayName: "  Primal Fury  ",
            unlockCost: 0,
            requiresPreviousTier: false,
            requiredRank: 1,
            abilities: CreateTestAbilities());

        // Assert
        tier.DisplayName.Should().Be("Primal Fury");
    }
}
