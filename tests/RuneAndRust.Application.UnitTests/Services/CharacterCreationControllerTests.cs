// ═══════════════════════════════════════════════════════════════════════════════
// CharacterCreationControllerTests.cs
// Unit tests for the CharacterCreationController application service. Verifies
// session initialization, step selection transitions, navigation (GoBack),
// cancellation, and validation behaviors including Clan-Born flexible bonus
// requirements, specialization-archetype compatibility, and name validation
// integration. Uses Moq for dependency mocking and FluentAssertions.
// Version: 0.17.5d
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
/// Unit tests for <see cref="CharacterCreationController"/>.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the core controller behaviors organized by category:
/// </para>
/// <list type="bullet">
///   <item><description>Session lifecycle — initialize, cancel, active status</description></item>
///   <item><description>Step transitions — valid selections advance to next step</description></item>
///   <item><description>Validation — Clan-Born bonus, specialization compatibility, name validation</description></item>
///   <item><description>Navigation — GoBack preserves selections, cannot go back from first step</description></item>
/// </list>
/// <para>
/// All tests mock the seven required dependencies (four providers, ViewModel builder,
/// name validator, logger). The ViewModel builder returns <see cref="CharacterCreationViewModel.Empty"/>
/// by default since these tests focus on controller logic, not ViewModel content.
/// </para>
/// </remarks>
/// <seealso cref="CharacterCreationController"/>
/// <seealso cref="ICharacterCreationController"/>
/// <seealso cref="StepResult"/>
[TestFixture]
public class CharacterCreationControllerTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Mock lineage provider for validating lineage selections.</summary>
    private Mock<ILineageProvider> _lineageProviderMock = null!;

    /// <summary>Mock background provider for validating background selections.</summary>
    private Mock<IBackgroundProvider> _backgroundProviderMock = null!;

    /// <summary>Mock archetype provider for validating archetype selections.</summary>
    private Mock<IArchetypeProvider> _archetypeProviderMock = null!;

    /// <summary>Mock specialization provider for archetype compatibility checks.</summary>
    private Mock<ISpecializationProvider> _specializationProviderMock = null!;

    /// <summary>Mock ViewModel builder returning Empty by default.</summary>
    private Mock<IViewModelBuilder> _viewModelBuilderMock = null!;

    /// <summary>Mock name validator for character name validation.</summary>
    private Mock<INameValidator> _nameValidatorMock = null!;

    /// <summary>Mock logger for diagnostic output. Not verified — logging is diagnostic only.</summary>
    private Mock<ILogger<CharacterCreationController>> _loggerMock = null!;

    /// <summary>The controller instance under test.</summary>
    private CharacterCreationController _controller = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes mock dependencies and creates a fresh controller before each test.
    /// The ViewModel builder is configured to return <see cref="CharacterCreationViewModel.Empty"/>
    /// for any state input, isolating controller logic from ViewModel construction.
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

        // Default: ViewModel builder returns Empty for any state
        _viewModelBuilderMock
            .Setup(x => x.Build(It.IsAny<CharacterCreationState>()))
            .Returns(CharacterCreationViewModel.Empty);

        _controller = new CharacterCreationController(
            _lineageProviderMock.Object,
            _backgroundProviderMock.Object,
            _archetypeProviderMock.Object,
            _specializationProviderMock.Object,
            _viewModelBuilderMock.Object,
            _nameValidatorMock.Object,
            _loggerMock.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SESSION LIFECYCLE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initialize() creates a new session starting at the Lineage step with a
    /// unique SessionId and sets IsSessionActive to true.
    /// </summary>
    [Test]
    public void Initialize_CreatesNewSession_AtLineageStep()
    {
        // Act
        var state = _controller.Initialize();

        // Assert
        state.Should().NotBeNull();
        state.CurrentStep.Should().Be(CharacterCreationStep.Lineage);
        state.SessionId.Should().NotBeNullOrEmpty();
        _controller.IsSessionActive.Should().BeTrue();
    }

    /// <summary>
    /// Cancel() clears the session state and sets IsSessionActive to false.
    /// </summary>
    [Test]
    public void Cancel_ClearsSessionState()
    {
        // Arrange
        _controller.Initialize();

        // Act
        _controller.Cancel();

        // Assert
        _controller.IsSessionActive.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LINEAGE SELECTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// SelectLineageAsync() with Clan-Born lineage but no flexible bonus returns
    /// a failure result with an error about the required flexible attribute bonus.
    /// </summary>
    [Test]
    public async Task SelectLineage_ClanBorn_WithoutFlexibleBonus_Fails()
    {
        // Arrange
        _controller.Initialize();

        // Act
        var result = await _controller.SelectLineageAsync(Lineage.ClanBorn);

        // Assert
        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e =>
            e.Contains("flexible attribute bonus"));
    }

    /// <summary>
    /// SelectLineageAsync() with a valid non-Clan-Born lineage succeeds and
    /// advances the workflow to the Background step.
    /// </summary>
    [Test]
    public async Task SelectLineage_Valid_AdvancesToBackground()
    {
        // Arrange
        _controller.Initialize();

        // Act
        var result = await _controller.SelectLineageAsync(Lineage.RuneMarked);

        // Assert
        result.Success.Should().BeTrue();
        result.CurrentStep.Should().Be(CharacterCreationStep.Background);
        result.NextStep.Should().Be(CharacterCreationStep.Attributes);
    }

    /// <summary>
    /// SelectLineageAsync() with Clan-Born lineage and a valid flexible bonus
    /// succeeds and advances to the Background step.
    /// </summary>
    [Test]
    public async Task SelectLineage_ClanBorn_WithFlexibleBonus_Succeeds()
    {
        // Arrange
        _controller.Initialize();

        // Act
        var result = await _controller.SelectLineageAsync(Lineage.ClanBorn, CoreAttribute.Might);

        // Assert
        result.Success.Should().BeTrue();
        result.CurrentStep.Should().Be(CharacterCreationStep.Background);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SPECIALIZATION VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// SelectSpecializationAsync() fails when the specialization does not belong
    /// to the currently selected archetype.
    /// </summary>
    [Test]
    public async Task SelectSpecialization_InvalidForArchetype_Fails()
    {
        // Arrange — initialize and advance through lineage + background + attributes + archetype
        _controller.Initialize();
        await _controller.SelectLineageAsync(Lineage.RuneMarked);
        await _controller.SelectBackgroundAsync(Background.WanderingSkald);
        var completeAttributes = new AttributeAllocationState
        {
            Mode = AttributeAllocationMode.Advanced,
            CurrentMight = 4, CurrentFinesse = 3, CurrentWits = 3,
            CurrentWill = 3, CurrentSturdiness = 2,
            PointsSpent = 15, PointsRemaining = 0, TotalPoints = 15
        };
        await _controller.ConfirmAttributesAsync(completeAttributes);
        await _controller.SelectArchetypeAsync(Archetype.Warrior);

        // Mock: GetByArchetype returns empty list — no valid specializations
        _specializationProviderMock
            .Setup(x => x.GetByArchetype(Archetype.Warrior))
            .Returns(new List<SpecializationDefinition>());

        // Act
        var result = await _controller.SelectSpecializationAsync(SpecializationId.Skald);

        // Assert
        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e =>
            e.Contains("not available for the chosen archetype"));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NAVIGATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// GoBack() from the Background step returns to the Lineage step and
    /// reports success with the previous step as the current step.
    /// </summary>
    [Test]
    public async Task GoBack_FromBackground_ReturnsToLineage()
    {
        // Arrange — initialize and advance to Background
        _controller.Initialize();
        await _controller.SelectLineageAsync(Lineage.RuneMarked);

        // Act
        var result = _controller.GoBack();

        // Assert
        result.Success.Should().BeTrue();
        result.CurrentStep.Should().Be(CharacterCreationStep.Lineage);
    }

    /// <summary>
    /// GoBack() from the Lineage step (first step) returns a failure result
    /// since there is no previous step to navigate to.
    /// </summary>
    [Test]
    public void GoBack_FromLineage_Fails()
    {
        // Arrange
        _controller.Initialize();

        // Act
        var result = _controller.GoBack();

        // Assert
        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Already at first step.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CHARACTER CONFIRMATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ConfirmCharacterAsync() returns a failure result when the name validator
    /// rejects the provided name.
    /// </summary>
    [Test]
    public async Task ConfirmCharacter_InvalidName_ReturnsError()
    {
        // Arrange
        _controller.Initialize();
        _nameValidatorMock
            .Setup(x => x.Validate(It.IsAny<string?>()))
            .Returns(NameValidationResult.Invalid("Name is required."));

        // Act
        var result = await _controller.ConfirmCharacterAsync("");

        // Assert
        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Name is required.");
    }
}
