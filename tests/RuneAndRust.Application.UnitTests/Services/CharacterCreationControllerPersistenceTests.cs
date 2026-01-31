// ═══════════════════════════════════════════════════════════════════════════════
// CharacterCreationControllerPersistenceTests.cs
// Unit tests for the CharacterCreationController's persistence integration
// (v0.17.5g). Verifies that ConfirmCharacterAsync correctly interacts with
// IPlayerRepository for saving characters, handling name collisions, and
// gracefully degrading when no repository is available.
// Version: 0.17.5g
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="CharacterCreationController"/> persistence integration.
/// </summary>
/// <remarks>
/// <para>
/// Tests verify the v0.17.5g persistence workflow in <c>ConfirmCharacterAsync</c>:
/// </para>
/// <list type="bullet">
///   <item><description>Successful save — repository SaveAsync called, character returned</description></item>
///   <item><description>Save failure — repository returns error, controller propagates failure</description></item>
///   <item><description>No repository — persistence skipped, creation still succeeds</description></item>
///   <item><description>Name collision — ExistsWithNameAsync returns true, creation blocked</description></item>
/// </list>
/// <para>
/// All tests progress the controller through all 5 steps before calling
/// <c>ConfirmCharacterAsync</c> to ensure state completeness. Mock dependencies
/// are configured per test to isolate the persistence behavior.
/// </para>
/// </remarks>
/// <seealso cref="CharacterCreationController"/>
/// <seealso cref="IPlayerRepository"/>
[TestFixture]
public class CharacterCreationControllerPersistenceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Mock lineage provider.</summary>
    private Mock<ILineageProvider> _lineageProviderMock = null!;

    /// <summary>Mock background provider.</summary>
    private Mock<IBackgroundProvider> _backgroundProviderMock = null!;

    /// <summary>Mock archetype provider.</summary>
    private Mock<IArchetypeProvider> _archetypeProviderMock = null!;

    /// <summary>Mock specialization provider.</summary>
    private Mock<ISpecializationProvider> _specializationProviderMock = null!;

    /// <summary>Mock ViewModel builder returning Empty by default.</summary>
    private Mock<IViewModelBuilder> _viewModelBuilderMock = null!;

    /// <summary>Mock name validator — configured to return valid by default.</summary>
    private Mock<INameValidator> _nameValidatorMock = null!;

    /// <summary>Mock logger for diagnostics.</summary>
    private Mock<ILogger<CharacterCreationController>> _loggerMock = null!;

    /// <summary>Mock character factory — returns a Player by default.</summary>
    private Mock<ICharacterFactory> _characterFactoryMock = null!;

    /// <summary>Mock player repository for persistence tests.</summary>
    private Mock<IPlayerRepository> _playerRepositoryMock = null!;

    /// <summary>Test player returned by the character factory.</summary>
    private Player _testPlayer = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes all mock dependencies before each test. Configures the name
    /// validator to accept any name, the character factory to return a test player,
    /// and the repository to return success by default.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _lineageProviderMock = new Mock<ILineageProvider>();
        _backgroundProviderMock = new Mock<IBackgroundProvider>();
        _archetypeProviderMock = new Mock<IArchetypeProvider>();
        _specializationProviderMock = new Mock<ISpecializationProvider>();
        _viewModelBuilderMock = new Mock<IViewModelBuilder>();
        _nameValidatorMock = new Mock<INameValidator>();
        _loggerMock = new Mock<ILogger<CharacterCreationController>>();
        _characterFactoryMock = new Mock<ICharacterFactory>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();

        // Default: ViewModel builder returns Empty for any state
        _viewModelBuilderMock
            .Setup(x => x.Build(It.IsAny<CharacterCreationState>()))
            .Returns(CharacterCreationViewModel.Empty);

        // Default: name validator accepts any name
        _nameValidatorMock
            .Setup(x => x.Validate(It.IsAny<string?>()))
            .Returns(NameValidationResult.Valid());

        // Default: create a test player from factory
        _testPlayer = new Player("TestHero");
        _characterFactoryMock
            .Setup(x => x.CreateCharacterAsync(It.IsAny<CharacterCreationState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testPlayer);

        // Default: repository saves successfully
        _playerRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SaveResult.Succeeded(_testPlayer.Id));

        // Default: no name collision
        _playerRepositoryMock
            .Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Default: specialization provider returns matching spec for archetype
        _specializationProviderMock
            .Setup(x => x.GetByArchetype(It.IsAny<Archetype>()))
            .Returns(new[]
            {
                SpecializationDefinition.Create(
                    SpecializationId.Berserkr,
                    "Berserkr",
                    "Fury Unleashed",
                    "A rage-fueled combatant.",
                    "Channel primal fury.",
                    Archetype.Warrior,
                    SpecializationPathType.Heretical,
                    unlockCost: 0)
            });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a controller with all dependencies including factory and repository.
    /// </summary>
    private CharacterCreationController CreateControllerWithRepository()
    {
        return new CharacterCreationController(
            _lineageProviderMock.Object,
            _backgroundProviderMock.Object,
            _archetypeProviderMock.Object,
            _specializationProviderMock.Object,
            _viewModelBuilderMock.Object,
            _nameValidatorMock.Object,
            _loggerMock.Object,
            _characterFactoryMock.Object,
            _playerRepositoryMock.Object);
    }

    /// <summary>
    /// Creates a controller with factory but no repository (persistence skipped).
    /// </summary>
    private CharacterCreationController CreateControllerWithoutRepository()
    {
        return new CharacterCreationController(
            _lineageProviderMock.Object,
            _backgroundProviderMock.Object,
            _archetypeProviderMock.Object,
            _specializationProviderMock.Object,
            _viewModelBuilderMock.Object,
            _nameValidatorMock.Object,
            _loggerMock.Object,
            _characterFactoryMock.Object);
    }

    /// <summary>
    /// Advances the controller through all 5 steps to reach a complete state
    /// ready for <c>ConfirmCharacterAsync</c>.
    /// </summary>
    /// <param name="controller">The controller to advance.</param>
    private async Task AdvanceToSummaryAsync(CharacterCreationController controller)
    {
        controller.Initialize();

        // Step 1: Lineage (non-Clan-Born, no flexible bonus needed)
        await controller.SelectLineageAsync(Lineage.IronBlooded);

        // Step 2: Background
        await controller.SelectBackgroundAsync(Background.VillageSmith);

        // Step 3: Attributes (must be complete — 0 points remaining)
        var attributes = new AttributeAllocationState
        {
            Mode = AttributeAllocationMode.Advanced,
            CurrentMight = 4, CurrentFinesse = 3, CurrentWits = 3,
            CurrentWill = 3, CurrentSturdiness = 2,
            PointsSpent = 15, PointsRemaining = 0, TotalPoints = 15
        };
        await controller.ConfirmAttributesAsync(attributes);

        // Step 4: Archetype
        await controller.SelectArchetypeAsync(Archetype.Warrior);

        // Step 5: Specialization
        await controller.SelectSpecializationAsync(SpecializationId.Berserkr);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PERSISTENCE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ConfirmCharacterAsync calls SaveAsync on the repository
    /// when both factory and repository are available, and returns a success result.
    /// </summary>
    [Test]
    public async Task ConfirmCharacter_WithRepository_SavesPlayer()
    {
        // Arrange
        var controller = CreateControllerWithRepository();
        await AdvanceToSummaryAsync(controller);

        // Act
        var result = await controller.ConfirmCharacterAsync("TestHero");

        // Assert
        result.Success.Should().BeTrue();
        result.Character.Should().NotBeNull();
        result.Character!.Name.Should().Be("TestHero");

        _playerRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that ConfirmCharacterAsync returns a failure result when
    /// the repository's SaveAsync returns a failure (e.g., database error).
    /// </summary>
    [Test]
    public async Task ConfirmCharacter_SaveFails_ReturnsError()
    {
        // Arrange
        _playerRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SaveResult.Failed("Database write failed."));

        var controller = CreateControllerWithRepository();
        await AdvanceToSummaryAsync(controller);

        // Act
        var result = await controller.ConfirmCharacterAsync("TestHero");

        // Assert
        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that ConfirmCharacterAsync succeeds even when no repository
    /// is provided — persistence is simply skipped.
    /// </summary>
    [Test]
    public async Task ConfirmCharacter_WithoutRepository_SkipsPersistence()
    {
        // Arrange
        var controller = CreateControllerWithoutRepository();
        await AdvanceToSummaryAsync(controller);

        // Act
        var result = await controller.ConfirmCharacterAsync("TestHero");

        // Assert
        result.Success.Should().BeTrue();
        result.Character.Should().NotBeNull();

        // Repository was never called
        _playerRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that ConfirmCharacterAsync returns a failure when
    /// ExistsWithNameAsync indicates the name is already taken.
    /// </summary>
    [Test]
    public async Task ConfirmCharacter_NameCollision_ReturnsError()
    {
        // Arrange — name already exists
        _playerRepositoryMock
            .Setup(x => x.ExistsWithNameAsync("TestHero", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateControllerWithRepository();
        await AdvanceToSummaryAsync(controller);

        // Act
        var result = await controller.ConfirmCharacterAsync("TestHero");

        // Assert
        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Contains("TestHero"));

        // Factory should NOT have been called since name check failed first
        _characterFactoryMock.Verify(
            x => x.CreateCharacterAsync(It.IsAny<CharacterCreationState>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
