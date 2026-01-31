// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationApplicationServiceTests.cs
// Unit tests for SpecializationApplicationService (v0.17.4e).
// Tests cover constructor validation, specialization application, tier unlocks,
// validation, cost calculation, and available specialization queries.
// Version: 0.17.4e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="SpecializationApplicationService"/> (v0.17.4e).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Constructor null validation for ISpecializationProvider and ILogger dependencies</description></item>
///   <item><description>Successful first specialization application (free, 0 PP)</description></item>
///   <item><description>Successful additional specialization application (3 PP)</description></item>
///   <item><description>Tier 1 ability granting on apply</description></item>
///   <item><description>Special resource initialization for resource-bearing specializations</description></item>
///   <item><description>Heretical path detection and Corruption warning</description></item>
///   <item><description>Coherent path with no Corruption warning</description></item>
///   <item><description>Failure on archetype mismatch, null character, unknown specialization, duplicate</description></item>
///   <item><description>Tier 2 and Tier 3 unlock with prerequisites</description></item>
///   <item><description>Tier unlock failures: missing previous tier, insufficient PP/rank, already unlocked</description></item>
///   <item><description>Validation methods (CanApplySpecialization, CanUnlockTier)</description></item>
///   <item><description>GetAvailableSpecializations and GetNextSpecializationCost queries</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class SpecializationApplicationServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════════════════

    private Mock<ISpecializationProvider> _mockProvider = null!;
    private Mock<ILogger<SpecializationApplicationService>> _mockLogger = null!;
    private SpecializationApplicationService _service = null!;

    /// <summary>
    /// Sets up test dependencies before each test. Configures mock provider
    /// with Berserkr and Skald specialization data and creates the service instance.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockProvider = new Mock<ISpecializationProvider>();
        _mockLogger = new Mock<ILogger<SpecializationApplicationService>>();

        // Configure provider count for constructor logging
        _mockProvider.Setup(p => p.Count).Returns(17);

        // Set up default provider responses
        SetupBerserkrProvider();
        SetupSkaldProvider();
        SetupWarriorArchetypeSpecializations();

        _service = new SpecializationApplicationService(
            _mockProvider.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROVIDER SETUP HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Configures the mock provider with Berserkr specialization data.
    /// Berserkr is a Heretical Warrior specialization with Rage resource
    /// and 3 tiers of abilities.
    /// </summary>
    private void SetupBerserkrProvider()
    {
        var resource = SpecialResourceDefinition.Create(
            "rage", "Rage", 0, 100, 0, 0, 5,
            "Accumulated fury that powers devastating abilities");

        // Tier 1 abilities
        var tier1Abilities = new List<SpecializationAbility>
        {
            SpecializationAbility.CreateActive(
                "rage-strike", "Rage Strike",
                "A devastating rage-powered attack.", 20, "rage", 0, 5),
            SpecializationAbility.CreateActive(
                "blood-frenzy", "Blood Frenzy",
                "Enter a frenzy that increases attack speed.", 30, "rage", 3, 0),
            SpecializationAbility.CreatePassive(
                "primal-toughness", "Primal Toughness",
                "Gain bonus HP from Constitution.", 0)
        };

        var tier1 = SpecializationAbilityTier.CreateTier1("Primal Fury", tier1Abilities);

        // Tier 2 abilities
        var tier2Abilities = new List<SpecializationAbility>
        {
            SpecializationAbility.CreateActive(
                "berserker-charge", "Berserker Charge",
                "Rush forward dealing damage.", 40, "rage", 0, 10),
            SpecializationAbility.CreateActive(
                "rending-blow", "Rending Blow",
                "Tear into enemies causing bleeding.", 25, "rage", 2, 0),
            SpecializationAbility.CreatePassive(
                "rage-fueled", "Rage Fueled",
                "Gain bonus damage based on current Rage.", 0)
        };

        var tier2 = SpecializationAbilityTier.CreateTier2("Unleashed Beast", tier2Abilities);

        // Tier 3 abilities
        var tier3Abilities = new List<SpecializationAbility>
        {
            SpecializationAbility.CreateActive(
                "avatar-of-destruction", "Avatar of Destruction",
                "Transform into an unstoppable force.", 100, "rage", 0, 20),
            SpecializationAbility.CreatePassive(
                "endless-rage", "Endless Rage",
                "Rage no longer decays over time.", 0),
            SpecializationAbility.CreatePassive(
                "blood-scent", "Blood Scent",
                "Gain bonus Rage when enemies are below 50% HP.", 0)
        };

        var tier3 = SpecializationAbilityTier.CreateTier3("Avatar of Destruction", tier3Abilities);

        var tiers = new List<SpecializationAbilityTier> { tier1, tier2, tier3 };

        var definition = SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Fury Unleashed",
            "The Berserkr channels primal rage into devastating attacks.",
            "Embrace the chaos within.",
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            0,
            resource,
            tiers);

        _mockProvider.Setup(p => p.GetBySpecializationId(SpecializationId.Berserkr))
            .Returns(definition);
        _mockProvider.Setup(p => p.Exists(SpecializationId.Berserkr))
            .Returns(true);
    }

    /// <summary>
    /// Configures the mock provider with Skald specialization data.
    /// Skald is a Coherent Adept specialization without a special resource.
    /// </summary>
    private void SetupSkaldProvider()
    {
        var tier1Abilities = new List<SpecializationAbility>
        {
            SpecializationAbility.CreateActive(
                "inspiring-song", "Inspiring Song",
                "Inspire allies with song.", 0, "", 3, 0),
            SpecializationAbility.CreateActive(
                "discordant-note", "Discordant Note",
                "Disrupt enemies with a jarring note.", 0, "", 2, 0),
            SpecializationAbility.CreatePassive(
                "performers-presence", "Performer's Presence",
                "Allies gain morale from your presence.", 0)
        };

        var tier1 = SpecializationAbilityTier.CreateTier1("Voice of Legends", tier1Abilities);

        var definition = SpecializationDefinition.Create(
            SpecializationId.Skald,
            "Skald",
            "Voice of Legends",
            "The Skald inspires allies through song and story.",
            "Let your voice carry the weight of legend.",
            Archetype.Adept,
            SpecializationPathType.Coherent,
            0,
            null,
            new List<SpecializationAbilityTier> { tier1 });

        _mockProvider.Setup(p => p.GetBySpecializationId(SpecializationId.Skald))
            .Returns(definition);
        _mockProvider.Setup(p => p.Exists(SpecializationId.Skald))
            .Returns(true);
    }

    /// <summary>
    /// Configures the mock provider to return Warrior specializations.
    /// </summary>
    private void SetupWarriorArchetypeSpecializations()
    {
        var berserkr = _mockProvider.Object.GetBySpecializationId(SpecializationId.Berserkr);
        var warriorSpecs = new List<SpecializationDefinition>();
        if (berserkr != null) warriorSpecs.Add(berserkr);

        _mockProvider.Setup(p => p.GetByArchetype(Archetype.Warrior))
            .Returns(warriorSpecs);
        _mockProvider.Setup(p => p.GetByArchetype(Archetype.Adept))
            .Returns(new List<SpecializationDefinition>());
    }

    /// <summary>
    /// Creates a Warrior player for testing.
    /// </summary>
    private static Player CreateWarriorPlayer(int pp = 0, int rank = 1)
    {
        var player = new Player("TestWarrior");
        player.SetClass("warrior", "warrior");
        player.SetProgressionPoints(pp);
        player.SetProgressionRank(rank);
        return player;
    }

    /// <summary>
    /// Creates an Adept player for testing.
    /// </summary>
    private static Player CreateAdeptPlayer(int pp = 0, int rank = 1)
    {
        var player = new Player("TestAdept");
        player.SetClass("adept", "adept");
        player.SetProgressionPoints(pp);
        player.SetProgressionRank(rank);
        return player;
    }

    /// <summary>
    /// Creates a Warrior player with Berserkr specialization already applied.
    /// </summary>
    private static Player CreateWarriorWithBerserkr(int pp = 0, int rank = 1)
    {
        var player = CreateWarriorPlayer(pp, rank);
        player.AddSpecialization(SpecializationId.Berserkr);
        return player;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the constructor throws when the provider is null.
    /// </summary>
    [Test]
    public void Constructor_NullProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new SpecializationApplicationService(
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("provider");
    }

    /// <summary>
    /// Verifies that the constructor throws when the logger is null.
    /// </summary>
    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new SpecializationApplicationService(
            _mockProvider.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ApplySpecialization TESTS — SUCCESS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the first specialization is applied for free (0 PP).
    /// </summary>
    [Test]
    public void ApplySpecialization_FirstSpecialization_AppliesForFree()
    {
        // Arrange
        var player = CreateWarriorPlayer(pp: 0);

        // Act
        var result = _service.ApplySpecialization(player, SpecializationId.Berserkr);

        // Assert
        result.Success.Should().BeTrue();
        result.PpCost.Should().Be(0);
        player.HasSpecialization(SpecializationId.Berserkr).Should().BeTrue();
        player.SpecializationCount.Should().Be(1);
        player.ProgressionPoints.Should().Be(0);
    }

    /// <summary>
    /// Verifies that applying Berserkr grants 3 Tier 1 abilities.
    /// </summary>
    [Test]
    public void ApplySpecialization_Berserkr_GrantsTier1Abilities()
    {
        // Arrange
        var player = CreateWarriorPlayer();

        // Act
        var result = _service.ApplySpecialization(player, SpecializationId.Berserkr);

        // Assert
        result.Success.Should().BeTrue();
        result.GrantedAbilities.Should().HaveCount(3);
        player.HasAbility("rage-strike").Should().BeTrue();
        player.HasAbility("blood-frenzy").Should().BeTrue();
        player.HasAbility("primal-toughness").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that applying Berserkr initializes the Rage resource.
    /// </summary>
    [Test]
    public void ApplySpecialization_Berserkr_InitializesRageResource()
    {
        // Arrange
        var player = CreateWarriorPlayer();

        // Act
        var result = _service.ApplySpecialization(player, SpecializationId.Berserkr);

        // Assert
        result.Success.Should().BeTrue();
        result.SpecialResourceInitialized.Should().BeTrue();
        player.HasResource("rage").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a Heretical specialization returns a Corruption warning.
    /// </summary>
    [Test]
    public void ApplySpecialization_HereticalSpec_ReturnsCorruptionWarning()
    {
        // Arrange
        var player = CreateWarriorPlayer();

        // Act
        var result = _service.ApplySpecialization(player, SpecializationId.Berserkr);

        // Assert
        result.IsHeretical.Should().BeTrue();
        result.CorruptionWarning.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Verifies that a Coherent specialization has no Corruption warning.
    /// </summary>
    [Test]
    public void ApplySpecialization_CoherentSpec_NoCorruptionWarning()
    {
        // Arrange
        var player = CreateAdeptPlayer();

        // Act
        var result = _service.ApplySpecialization(player, SpecializationId.Skald);

        // Assert
        result.IsHeretical.Should().BeFalse();
        result.CorruptionWarning.Should().BeNull();
    }

    /// <summary>
    /// Verifies that a specialization without a resource succeeds without resource init.
    /// </summary>
    [Test]
    public void ApplySpecialization_SpecWithoutResource_SucceedsWithoutResourceInit()
    {
        // Arrange
        var player = CreateAdeptPlayer();

        // Act
        var result = _service.ApplySpecialization(player, SpecializationId.Skald);

        // Assert
        result.Success.Should().BeTrue();
        result.SpecialResourceInitialized.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that additional specializations cost 3 PP.
    /// </summary>
    [Test]
    public void ApplySpecialization_AdditionalSpecialization_Costs3PP()
    {
        // Arrange
        var player = CreateWarriorPlayer(pp: 5);
        // Apply first specialization (free)
        _service.ApplySpecialization(player, SpecializationId.Berserkr);

        // Set up Skjaldmaer as another Warrior spec
        var skjaldmaerResource = SpecialResourceDefinition.Create(
            "block_charges", "Block Charges", 0, 3, 3, 1, 0,
            "Consumable charges for powerful defensive abilities");
        var skjaldmaerTier1 = SpecializationAbilityTier.CreateTier1("Shield Basics",
            new List<SpecializationAbility>
            {
                SpecializationAbility.CreateActive("shield-wall", "Shield Wall", "Defense boost.", 1, "block_charges", 0, 0),
                SpecializationAbility.CreateActive("guard", "Guard", "Redirect attack.", 0, "", 3, 0),
                SpecializationAbility.CreatePassive("stalwart", "Stalwart", "+2 HP per STURDINESS.", 0)
            });
        var skjaldmaerDef = SpecializationDefinition.Create(
            SpecializationId.Skjaldmaer, "Skjaldmaer", "The Living Shield",
            "The Skjaldmaer stands between allies and danger.",
            "None shall fall while I stand.",
            Archetype.Warrior, SpecializationPathType.Coherent, 0,
            skjaldmaerResource,
            new List<SpecializationAbilityTier> { skjaldmaerTier1 });
        _mockProvider.Setup(p => p.GetBySpecializationId(SpecializationId.Skjaldmaer))
            .Returns(skjaldmaerDef);

        // Act
        var result = _service.ApplySpecialization(player, SpecializationId.Skjaldmaer);

        // Assert
        result.Success.Should().BeTrue();
        result.PpCost.Should().Be(3);
        player.ProgressionPoints.Should().Be(2); // 5 - 3 = 2
        player.SpecializationCount.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ApplySpecialization TESTS — FAILURE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies failure when character is null.
    /// </summary>
    [Test]
    public void ApplySpecialization_NullCharacter_Fails()
    {
        // Act
        var result = _service.ApplySpecialization(null!, SpecializationId.Berserkr);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("character is required");
    }

    /// <summary>
    /// Verifies failure when archetype doesn't match.
    /// </summary>
    [Test]
    public void ApplySpecialization_WrongArchetype_Fails()
    {
        // Arrange — Adept trying to get Warrior specialization
        var player = CreateAdeptPlayer();

        // Act
        var result = _service.ApplySpecialization(player, SpecializationId.Berserkr);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Warrior");
    }

    /// <summary>
    /// Verifies failure when specialization not found in provider.
    /// </summary>
    [Test]
    public void ApplySpecialization_UnknownSpecialization_Fails()
    {
        // Arrange
        var player = CreateWarriorPlayer();
        _mockProvider.Setup(p => p.GetBySpecializationId(SpecializationId.AtgeirWielder))
            .Returns((SpecializationDefinition?)null);

        // Act
        var result = _service.ApplySpecialization(player, SpecializationId.AtgeirWielder);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("not found");
    }

    /// <summary>
    /// Verifies failure when player already has the specialization.
    /// </summary>
    [Test]
    public void ApplySpecialization_AlreadyHasSpec_Fails()
    {
        // Arrange
        var player = CreateWarriorPlayer();
        _service.ApplySpecialization(player, SpecializationId.Berserkr);

        // Act — try to apply again
        var result = _service.ApplySpecialization(player, SpecializationId.Berserkr);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Already has");
    }

    /// <summary>
    /// Verifies failure when insufficient PP for additional specialization.
    /// </summary>
    [Test]
    public void ApplySpecialization_InsufficientPP_Fails()
    {
        // Arrange — player already has a spec but has 0 PP for second
        var player = CreateWarriorPlayer(pp: 0);
        player.AddSpecialization(SpecializationId.IronBane);

        // Setup Skjaldmaer
        var skjaldmaerTier1 = SpecializationAbilityTier.CreateTier1("Shield Basics",
            new List<SpecializationAbility>
            {
                SpecializationAbility.CreateActive("shield-wall", "Shield Wall", "Defense boost.", 1, "block_charges", 0, 0),
                SpecializationAbility.CreateActive("guard", "Guard", "Redirect attack.", 0, "", 3, 0),
                SpecializationAbility.CreatePassive("stalwart", "Stalwart", "+2 HP.", 0)
            });
        var skjaldmaerDef = SpecializationDefinition.Create(
            SpecializationId.Skjaldmaer, "Skjaldmaer", "The Living Shield",
            "Stand between allies and danger.", "None shall fall.",
            Archetype.Warrior, SpecializationPathType.Coherent, 0, null,
            new List<SpecializationAbilityTier> { skjaldmaerTier1 });
        _mockProvider.Setup(p => p.GetBySpecializationId(SpecializationId.Skjaldmaer))
            .Returns(skjaldmaerDef);

        // Act
        var result = _service.ApplySpecialization(player, SpecializationId.Skjaldmaer);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("PP");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UnlockTier TESTS — SUCCESS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that unlocking Tier 2 succeeds with prerequisites met.
    /// </summary>
    [Test]
    public void UnlockTier_Tier2WithPrerequisites_Succeeds()
    {
        // Arrange
        var player = CreateWarriorWithBerserkr(pp: 5, rank: 2);

        // Act
        var result = _service.UnlockTier(player, SpecializationId.Berserkr, 2);

        // Assert
        result.Success.Should().BeTrue();
        result.PpCost.Should().Be(2);
        result.GrantedAbilities.Should().HaveCount(3);
        player.HasUnlockedTier(SpecializationId.Berserkr, 2).Should().BeTrue();
        player.HasAbility("berserker-charge").Should().BeTrue();
        player.HasAbility("rending-blow").Should().BeTrue();
        player.HasAbility("rage-fueled").Should().BeTrue();
        player.ProgressionPoints.Should().Be(3); // 5 - 2 = 3
    }

    /// <summary>
    /// Verifies that unlocking Tier 3 succeeds with Tier 2 already unlocked.
    /// </summary>
    [Test]
    public void UnlockTier_Tier3WithPrerequisites_Succeeds()
    {
        // Arrange
        var player = CreateWarriorWithBerserkr(pp: 10, rank: 3);
        player.UnlockSpecializationTier(SpecializationId.Berserkr, 2);

        // Act
        var result = _service.UnlockTier(player, SpecializationId.Berserkr, 3);

        // Assert
        result.Success.Should().BeTrue();
        result.PpCost.Should().Be(3);
        result.GrantedAbilities.Should().HaveCount(3);
        player.HasUnlockedTier(SpecializationId.Berserkr, 3).Should().BeTrue();
        player.ProgressionPoints.Should().Be(7); // 10 - 3 = 7
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UnlockTier TESTS — FAILURE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies failure when trying to skip to Tier 3 without Tier 2.
    /// </summary>
    [Test]
    public void UnlockTier_MissingPreviousTier_Fails()
    {
        // Arrange — Tier 1 unlocked but not Tier 2
        var player = CreateWarriorWithBerserkr(pp: 10, rank: 3);

        // Act
        var result = _service.UnlockTier(player, SpecializationId.Berserkr, 3);

        // Assert
        result.Success.Should().BeFalse();
    }

    /// <summary>
    /// Verifies failure when player doesn't have the specialization.
    /// </summary>
    [Test]
    public void UnlockTier_NoSpecialization_Fails()
    {
        // Arrange
        var player = CreateWarriorPlayer(pp: 5, rank: 2);

        // Act
        var result = _service.UnlockTier(player, SpecializationId.Berserkr, 2);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Does not have");
    }

    /// <summary>
    /// Verifies failure when tier is already unlocked.
    /// </summary>
    [Test]
    public void UnlockTier_AlreadyUnlocked_Fails()
    {
        // Arrange — Tier 2 already unlocked
        var player = CreateWarriorWithBerserkr(pp: 5, rank: 2);
        player.UnlockSpecializationTier(SpecializationId.Berserkr, 2);

        // Act
        var result = _service.UnlockTier(player, SpecializationId.Berserkr, 2);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("already unlocked");
    }

    /// <summary>
    /// Verifies that invalid tier numbers throw ArgumentOutOfRangeException.
    /// </summary>
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(4)]
    public void UnlockTier_InvalidTierNumber_ThrowsArgumentOutOfRange(int tierNumber)
    {
        // Arrange
        var player = CreateWarriorWithBerserkr(pp: 5, rank: 2);

        // Act
        var act = () => _service.UnlockTier(player, SpecializationId.Berserkr, tierNumber);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that null player throws ArgumentNullException for UnlockTier.
    /// </summary>
    [Test]
    public void UnlockTier_NullPlayer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.UnlockTier(null!, SpecializationId.Berserkr, 2);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CanApplySpecialization TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies validation passes for a valid scenario.
    /// </summary>
    [Test]
    public void CanApplySpecialization_ValidScenario_ReturnsTrue()
    {
        // Arrange
        var player = CreateWarriorPlayer();

        // Act
        var (canApply, reason) = _service.CanApplySpecialization(player, SpecializationId.Berserkr);

        // Assert
        canApply.Should().BeTrue();
        reason.Should().BeNull();
    }

    /// <summary>
    /// Verifies validation fails for wrong archetype.
    /// </summary>
    [Test]
    public void CanApplySpecialization_WrongArchetype_ReturnsFalse()
    {
        // Arrange
        var player = CreateAdeptPlayer();

        // Act
        var (canApply, reason) = _service.CanApplySpecialization(player, SpecializationId.Berserkr);

        // Assert
        canApply.Should().BeFalse();
        reason.Should().Contain("Warrior");
    }

    /// <summary>
    /// Verifies validation fails when already has specialization.
    /// </summary>
    [Test]
    public void CanApplySpecialization_AlreadyHasSpec_ReturnsFalse()
    {
        // Arrange
        var player = CreateWarriorPlayer();
        player.AddSpecialization(SpecializationId.Berserkr);

        // Act
        var (canApply, reason) = _service.CanApplySpecialization(player, SpecializationId.Berserkr);

        // Assert
        canApply.Should().BeFalse();
        reason.Should().Contain("Already has");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CanUnlockTier TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies tier unlock validation passes for a valid scenario.
    /// </summary>
    [Test]
    public void CanUnlockTier_ValidScenario_ReturnsTrue()
    {
        // Arrange
        var player = CreateWarriorWithBerserkr(pp: 5, rank: 2);

        // Act
        var (canUnlock, reason) = _service.CanUnlockTier(
            player, SpecializationId.Berserkr, 2);

        // Assert
        canUnlock.Should().BeTrue();
        reason.Should().BeNull();
    }

    /// <summary>
    /// Verifies tier unlock validation fails when missing prerequisite tier.
    /// </summary>
    [Test]
    public void CanUnlockTier_MissingPrerequisite_ReturnsFalse()
    {
        // Arrange — trying Tier 3 without Tier 2
        var player = CreateWarriorWithBerserkr(pp: 5, rank: 3);

        // Act
        var (canUnlock, reason) = _service.CanUnlockTier(
            player, SpecializationId.Berserkr, 3);

        // Assert
        canUnlock.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetAvailableSpecializations TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies GetAvailableSpecializations returns archetype-appropriate specializations.
    /// </summary>
    [Test]
    public void GetAvailableSpecializations_ReturnsArchetypeSpecializations()
    {
        // Arrange
        var player = CreateWarriorPlayer();

        // Act
        var available = _service.GetAvailableSpecializations(player);

        // Assert
        available.Should().NotBeEmpty();
        _mockProvider.Verify(p => p.GetByArchetype(Archetype.Warrior), Times.Once);
    }

    /// <summary>
    /// Verifies GetAvailableSpecializations returns empty when player has no archetype.
    /// </summary>
    [Test]
    public void GetAvailableSpecializations_NoArchetype_ReturnsEmpty()
    {
        // Arrange — player without archetype
        var player = new Player("NoArchetype");

        // Act
        var available = _service.GetAvailableSpecializations(player);

        // Assert
        available.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetNextSpecializationCost TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies first specialization costs 0 PP.
    /// </summary>
    [Test]
    public void GetNextSpecializationCost_FirstSpec_ReturnsZero()
    {
        // Arrange
        var player = CreateWarriorPlayer();

        // Act
        var cost = _service.GetNextSpecializationCost(player);

        // Assert
        cost.Should().Be(0);
    }

    /// <summary>
    /// Verifies additional specializations cost 3 PP.
    /// </summary>
    [Test]
    public void GetNextSpecializationCost_AdditionalSpec_ReturnsThree()
    {
        // Arrange
        var player = CreateWarriorPlayer();
        player.AddSpecialization(SpecializationId.Berserkr);

        // Act
        var cost = _service.GetNextSpecializationCost(player);

        // Assert
        cost.Should().Be(3);
    }

    /// <summary>
    /// Verifies cost remains 3 PP for third specialization.
    /// </summary>
    [Test]
    public void GetNextSpecializationCost_MultipleSpecs_ReturnsThree()
    {
        // Arrange
        var player = CreateWarriorPlayer();
        player.AddSpecialization(SpecializationId.Berserkr);
        player.AddSpecialization(SpecializationId.Skjaldmaer);

        // Act
        var cost = _service.GetNextSpecializationCost(player);

        // Assert
        cost.Should().Be(3);
    }
}
