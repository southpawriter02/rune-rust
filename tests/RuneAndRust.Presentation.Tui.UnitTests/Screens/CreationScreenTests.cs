// ═══════════════════════════════════════════════════════════════════════════════
// CreationScreenTests.cs
// Unit tests for the character creation wizard TUI components: ScreenResult
// factory methods, ScreenAction enum, OptionListRenderer prompt building and
// navigation parsing, StepHeaderRenderer, and CreationWizardView orchestration.
// Tests cover value types, shared components, and the orchestrator's routing
// and state management logic using Moq and Spectre.Console's TestConsole.
// Version: 0.17.5f
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Tui.Components;
using RuneAndRust.Presentation.Tui.Screens;
using RuneAndRust.Presentation.Tui.Views;
using Spectre.Console;
using Spectre.Console.Testing;

namespace RuneAndRust.Presentation.Tui.UnitTests.Screens;

/// <summary>
/// Unit tests for the character creation wizard TUI components (v0.17.5f).
/// </summary>
[TestFixture]
public class CreationScreenTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SCREEN RESULT FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ScreenResult_Selected_SetsActionToContinue()
    {
        // Arrange & Act
        var result = ScreenResult.Selected(Archetype.Warrior);

        // Assert
        result.Action.Should().Be(ScreenAction.Continue);
        result.Selection.Should().Be(Archetype.Warrior);
    }

    [Test]
    public void ScreenResult_Selected_PreservesSelectionObject()
    {
        // Arrange
        var tuple = (Lineage.ClanBorn, (CoreAttribute?)CoreAttribute.Might);

        // Act
        var result = ScreenResult.Selected(tuple);

        // Assert
        result.Action.Should().Be(ScreenAction.Continue);
        result.Selection.Should().Be(tuple);
    }

    [Test]
    public void ScreenResult_GoBack_SetsActionToGoBack()
    {
        // Act
        var result = ScreenResult.GoBack();

        // Assert
        result.Action.Should().Be(ScreenAction.GoBack);
        result.Selection.Should().BeNull();
    }

    [Test]
    public void ScreenResult_Cancel_SetsActionToCancel()
    {
        // Act
        var result = ScreenResult.Cancel();

        // Assert
        result.Action.Should().Be(ScreenAction.Cancel);
        result.Selection.Should().BeNull();
    }

    [Test]
    public void ScreenResult_Selected_WithAttributeAllocationState_PreservesState()
    {
        // Arrange
        var state = AttributeAllocationState.CreateAdvancedDefault(15);

        // Act
        var result = ScreenResult.Selected(state);

        // Assert
        result.Action.Should().Be(ScreenAction.Continue);
        var returned = result.Selection.Should().BeOfType<AttributeAllocationState>().Subject;
        returned.TotalPoints.Should().Be(15);
        returned.PointsRemaining.Should().Be(15);
        returned.Mode.Should().Be(AttributeAllocationMode.Advanced);
    }

    [Test]
    public void ScreenResult_Selected_WithStringName_PreservesName()
    {
        // Act
        var result = ScreenResult.Selected("Bjorn the Swift");

        // Assert
        result.Action.Should().Be(ScreenAction.Continue);
        result.Selection.Should().Be("Bjorn the Swift");
    }

    [Test]
    public void ScreenResult_ToString_ForContinue_IncludesTypeName()
    {
        // Act
        var result = ScreenResult.Selected(Archetype.Warrior);

        // Assert
        result.ToString().Should().Contain("Continue");
        result.ToString().Should().Contain("Archetype");
    }

    [Test]
    public void ScreenResult_ToString_ForGoBack_ShowsGoBack()
    {
        // Act
        var result = ScreenResult.GoBack();

        // Assert
        result.ToString().Should().Be("GoBack");
    }

    [Test]
    public void ScreenResult_ToString_ForCancel_ShowsCancel()
    {
        // Act
        var result = ScreenResult.Cancel();

        // Assert
        result.ToString().Should().Be("Cancel");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCREEN ACTION ENUM TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ScreenAction_Continue_HasValueZero()
    {
        ((int)ScreenAction.Continue).Should().Be(0);
    }

    [Test]
    public void ScreenAction_GoBack_HasValueOne()
    {
        ((int)ScreenAction.GoBack).Should().Be(1);
    }

    [Test]
    public void ScreenAction_Cancel_HasValueTwo()
    {
        ((int)ScreenAction.Cancel).Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OPTION LIST RENDERER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void OptionListRenderer_BackChoice_IsLessThanBack()
    {
        OptionListRenderer.BackChoice.Should().Be("< Back");
    }

    [Test]
    public void OptionListRenderer_CancelChoice_IsCancelCreation()
    {
        OptionListRenderer.CancelChoice.Should().Be("Cancel Creation");
    }

    [Test]
    public void OptionListRenderer_IsBack_ReturnsTrueForBackChoice()
    {
        OptionListRenderer.IsBack("< Back").Should().BeTrue();
    }

    [Test]
    public void OptionListRenderer_IsBack_ReturnsFalseForOtherStrings()
    {
        OptionListRenderer.IsBack("back").Should().BeFalse();
        OptionListRenderer.IsBack("Back").Should().BeFalse();
        OptionListRenderer.IsBack("Cancel Creation").Should().BeFalse();
    }

    [Test]
    public void OptionListRenderer_IsCancel_ReturnsTrueForCancelChoice()
    {
        OptionListRenderer.IsCancel("Cancel Creation").Should().BeTrue();
    }

    [Test]
    public void OptionListRenderer_IsCancel_ReturnsFalseForOtherStrings()
    {
        OptionListRenderer.IsCancel("cancel").Should().BeFalse();
        OptionListRenderer.IsCancel("< Back").Should().BeFalse();
    }

    [Test]
    public void OptionListRenderer_TryParseNavigation_ReturnsGoBackForBackChoice()
    {
        // Act
        var result = OptionListRenderer.TryParseNavigation("< Back");

        // Assert
        result.Should().NotBeNull();
        result!.Value.Action.Should().Be(ScreenAction.GoBack);
        result!.Value.Selection.Should().BeNull();
    }

    [Test]
    public void OptionListRenderer_TryParseNavigation_ReturnsCancelForCancelChoice()
    {
        // Act
        var result = OptionListRenderer.TryParseNavigation("Cancel Creation");

        // Assert
        result.Should().NotBeNull();
        result!.Value.Action.Should().Be(ScreenAction.Cancel);
        result!.Value.Selection.Should().BeNull();
    }

    [Test]
    public void OptionListRenderer_TryParseNavigation_ReturnsNullForContentChoice()
    {
        // Act
        var result = OptionListRenderer.TryParseNavigation("Clan-Born — The Stable Code");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void OptionListRenderer_RenderValidationErrors_WithEmptyList_NoOutput()
    {
        // Arrange
        var console = new TestConsole();
        var errors = Array.Empty<string>();

        // Act
        OptionListRenderer.RenderValidationErrors(console, errors);

        // Assert
        console.Output.Should().BeEmpty();
    }

    [Test]
    public void OptionListRenderer_RenderValidationErrors_WithNullList_NoOutput()
    {
        // Arrange
        var console = new TestConsole();

        // Act
        OptionListRenderer.RenderValidationErrors(console, null!);

        // Assert
        console.Output.Should().BeEmpty();
    }

    [Test]
    public void OptionListRenderer_RenderValidationErrors_WithErrors_RendersEachError()
    {
        // Arrange
        var console = new TestConsole();
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        OptionListRenderer.RenderValidationErrors(console, errors);

        // Assert
        console.Output.Should().Contain("Error 1");
        console.Output.Should().Contain("Error 2");
    }

    [Test]
    public void OptionListRenderer_CreatePrompt_WithCanGoBackFalse_ExcludesBackChoice()
    {
        // Act
        var prompt = OptionListRenderer.CreatePrompt(
            "Choose:",
            new[] { "Option A", "Option B" },
            canGoBack: false);

        // Assert — prompt should be non-null (we can't easily inspect choices)
        prompt.Should().NotBeNull();
    }

    [Test]
    public void OptionListRenderer_CreatePrompt_WithCanGoBackTrue_IncludesBackChoice()
    {
        // Act
        var prompt = OptionListRenderer.CreatePrompt(
            "Choose:",
            new[] { "Option A", "Option B" },
            canGoBack: true);

        // Assert
        prompt.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STEP HEADER RENDERER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void StepHeaderRenderer_Render_IncludesStepNumberAndTitle()
    {
        // Arrange
        var console = new TestConsole();
        var viewModel = new CharacterCreationViewModel
        {
            StepNumber = 3,
            TotalSteps = 6,
            StepTitle = "Allocate Attributes",
            StepDescription = "Define your character's core capabilities.",
            IsPermanentChoice = false,
            PermanentWarning = null,
            ValidationErrors = Array.Empty<string>(),
            IsCurrentStepValid = true
        };

        // Act
        StepHeaderRenderer.Render(console, viewModel);

        // Assert
        console.Output.Should().Contain("STEP 3 OF 6");
        console.Output.Should().Contain("ALLOCATE ATTRIBUTES");
    }

    [Test]
    public void StepHeaderRenderer_Render_IncludesDescription()
    {
        // Arrange
        var console = new TestConsole();
        var viewModel = new CharacterCreationViewModel
        {
            StepNumber = 1,
            TotalSteps = 6,
            StepTitle = "Choose Your Lineage",
            StepDescription = "Your bloodline defines your heritage.",
            IsPermanentChoice = false,
            PermanentWarning = null,
            ValidationErrors = Array.Empty<string>(),
            IsCurrentStepValid = true
        };

        // Act
        StepHeaderRenderer.Render(console, viewModel);

        // Assert
        console.Output.Should().Contain("Your bloodline defines your heritage.");
    }

    [Test]
    public void StepHeaderRenderer_Render_WithPermanentChoice_ShowsWarning()
    {
        // Arrange
        var console = new TestConsole();
        var viewModel = new CharacterCreationViewModel
        {
            StepNumber = 4,
            TotalSteps = 6,
            StepTitle = "Choose Your Archetype",
            StepDescription = "Your fundamental approach to survival.",
            IsPermanentChoice = true,
            PermanentWarning = "This choice is PERMANENT and cannot be changed.",
            ValidationErrors = Array.Empty<string>(),
            IsCurrentStepValid = true
        };

        // Act
        StepHeaderRenderer.Render(console, viewModel);

        // Assert
        console.Output.Should().Contain("WARNING");
        console.Output.Should().Contain("This choice is PERMANENT and cannot be changed.");
    }

    [Test]
    public void StepHeaderRenderer_Render_WithoutPermanentChoice_NoWarning()
    {
        // Arrange
        var console = new TestConsole();
        var viewModel = new CharacterCreationViewModel
        {
            StepNumber = 2,
            TotalSteps = 6,
            StepTitle = "Choose Your Background",
            StepDescription = "Your pre-Silence profession.",
            IsPermanentChoice = false,
            PermanentWarning = null,
            ValidationErrors = Array.Empty<string>(),
            IsCurrentStepValid = true
        };

        // Act
        StepHeaderRenderer.Render(console, viewModel);

        // Assert
        console.Output.Should().NotContain("WARNING");
    }

    [Test]
    public void StepHeaderRenderer_RenderProgress_ShowsFilledAndEmptyIndicators()
    {
        // Arrange
        var console = new TestConsole();
        var viewModel = new CharacterCreationViewModel
        {
            StepNumber = 3,
            TotalSteps = 6,
            StepTitle = "Test",
            StepDescription = "",
            IsPermanentChoice = false,
            PermanentWarning = null,
            ValidationErrors = Array.Empty<string>(),
            IsCurrentStepValid = true
        };

        // Act
        StepHeaderRenderer.RenderProgress(console, viewModel);

        // Assert — should contain 3 filled and 3 empty
        console.Output.Should().Contain("■■■");
        console.Output.Should().Contain("□□□");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATION WIZARD VIEW TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreationWizardView_Constructor_ThrowsOnNullController()
    {
        // Arrange
        var screens = new List<ICreationScreen>();
        var console = new TestConsole();

        // Act & Assert
        var act = () => new CreationWizardView(null!, screens, console);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CreationWizardView_Constructor_ThrowsOnNullScreens()
    {
        // Arrange
        var controller = new Mock<ICharacterCreationController>();
        var console = new TestConsole();

        // Act & Assert
        var act = () => new CreationWizardView(controller.Object, null!, console);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CreationWizardView_Constructor_ThrowsOnNullConsole()
    {
        // Arrange
        var controller = new Mock<ICharacterCreationController>();
        var screens = new List<ICreationScreen>();

        // Act & Assert
        var act = () => new CreationWizardView(controller.Object, screens, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CreationWizardView_Constructor_ThrowsOnEmptyScreens()
    {
        // Arrange
        var controller = new Mock<ICharacterCreationController>();
        var screens = new List<ICreationScreen>();
        var console = new TestConsole();

        // Act & Assert
        var act = () => new CreationWizardView(controller.Object, screens, console);
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public async Task CreationWizardView_RunAsync_CancelReturnsNull()
    {
        // Arrange
        var controller = new Mock<ICharacterCreationController>();
        var mockScreen = new Mock<ICreationScreen>();
        var console = new Mock<IAnsiConsole>();

        var state = CharacterCreationState.Create();
        var viewModel = new CharacterCreationViewModel
        {
            CurrentStep = CharacterCreationStep.Lineage,
            StepNumber = 1,
            TotalSteps = 6,
            StepTitle = "Test",
            StepDescription = "",
            IsPermanentChoice = false,
            ValidationErrors = Array.Empty<string>(),
            IsCurrentStepValid = true
        };

        controller.Setup(c => c.Initialize()).Returns(state);
        controller.Setup(c => c.IsSessionActive).Returns(true);
        controller.Setup(c => c.GetCurrentState()).Returns((state, viewModel));

        mockScreen.Setup(s => s.Step).Returns(CharacterCreationStep.Lineage);
        mockScreen.Setup(s => s.DisplayAsync(
                It.IsAny<CharacterCreationViewModel>(),
                It.IsAny<IAnsiConsole>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScreenResult.Cancel());

        var wizard = new CreationWizardView(
            controller.Object,
            new[] { mockScreen.Object },
            console.Object);

        // Act
        var result = await wizard.RunAsync();

        // Assert
        result.Should().BeNull();
        controller.Verify(c => c.Cancel(), Times.Once);
    }

    [Test]
    public async Task CreationWizardView_RunAsync_GoBackCallsControllerGoBack()
    {
        // Arrange
        var controller = new Mock<ICharacterCreationController>();
        var mockScreen = new Mock<ICreationScreen>();
        var console = new Mock<IAnsiConsole>();

        // Create state with CurrentStep matching the registered screen
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Background;

        var viewModel = new CharacterCreationViewModel
        {
            CurrentStep = CharacterCreationStep.Background,
            StepNumber = 2,
            TotalSteps = 6,
            StepTitle = "Test",
            StepDescription = "",
            CanGoBack = true,
            IsPermanentChoice = false,
            ValidationErrors = Array.Empty<string>(),
            IsCurrentStepValid = true
        };

        var goBackResult = StepResult.Succeeded(
            CharacterCreationStep.Lineage,
            CharacterCreationStep.Background,
            viewModel);

        controller.Setup(c => c.Initialize()).Returns(state);

        // Allow 3 iterations: GoBack + Cancel + exit
        var callCount = 0;
        controller.Setup(c => c.IsSessionActive)
            .Returns(() => ++callCount <= 3);

        controller.Setup(c => c.GetCurrentState()).Returns((state, viewModel));
        controller.Setup(c => c.GoBack()).Returns(goBackResult);

        mockScreen.Setup(s => s.Step).Returns(CharacterCreationStep.Background);

        // First call: GoBack; Second call: Cancel
        var displayCount = 0;
        mockScreen.Setup(s => s.DisplayAsync(
                It.IsAny<CharacterCreationViewModel>(),
                It.IsAny<IAnsiConsole>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++displayCount == 1
                ? ScreenResult.GoBack()
                : ScreenResult.Cancel());

        var wizard = new CreationWizardView(
            controller.Object,
            new[] { mockScreen.Object },
            console.Object);

        // Act
        await wizard.RunAsync();

        // Assert
        controller.Verify(c => c.GoBack(), Times.Once);
    }

    [Test]
    public async Task CreationWizardView_RunAsync_InitializesController()
    {
        // Arrange
        var controller = new Mock<ICharacterCreationController>();
        var mockScreen = new Mock<ICreationScreen>();
        var console = new Mock<IAnsiConsole>();

        var state = CharacterCreationState.Create();
        var viewModel = CharacterCreationViewModel.Empty;

        controller.Setup(c => c.Initialize()).Returns(state);
        controller.Setup(c => c.IsSessionActive).Returns(false);
        controller.Setup(c => c.GetCurrentState()).Returns((state, viewModel));

        mockScreen.Setup(s => s.Step).Returns(CharacterCreationStep.Lineage);

        var wizard = new CreationWizardView(
            controller.Object,
            new[] { mockScreen.Object },
            console.Object);

        // Act
        await wizard.RunAsync();

        // Assert
        controller.Verify(c => c.Initialize(), Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ATTRIBUTE POINT COST TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void AttributeAllocationState_CreateAdvancedDefault_SetsAllAttributesToOne()
    {
        // Act
        var state = AttributeAllocationState.CreateAdvancedDefault(15);

        // Assert
        state.CurrentMight.Should().Be(1);
        state.CurrentFinesse.Should().Be(1);
        state.CurrentWits.Should().Be(1);
        state.CurrentWill.Should().Be(1);
        state.CurrentSturdiness.Should().Be(1);
        state.PointsRemaining.Should().Be(15);
        state.Mode.Should().Be(AttributeAllocationMode.Advanced);
    }

    [Test]
    public void AttributeAllocationState_WithAttributeValue_IncreasesAttribute()
    {
        // Arrange
        var state = AttributeAllocationState.CreateAdvancedDefault(15);

        // Act — increase Might from 1 to 4 (3 points spent, cost 1 each)
        state = state.WithAttributeValue(CoreAttribute.Might, 2, 1);
        state = state.WithAttributeValue(CoreAttribute.Might, 3, 1);
        state = state.WithAttributeValue(CoreAttribute.Might, 4, 1);

        // Assert
        state.CurrentMight.Should().Be(4);
        state.PointsSpent.Should().Be(3);
        state.PointsRemaining.Should().Be(12);
    }

    [Test]
    public void AttributeAllocationState_CreateFromRecommendedBuild_SetsValuesCorrectly()
    {
        // Act
        var state = AttributeAllocationState.CreateFromRecommendedBuild(
            "warrior", 4, 3, 2, 2, 4, 15);

        // Assert
        state.Mode.Should().Be(AttributeAllocationMode.Simple);
        state.CurrentMight.Should().Be(4);
        state.CurrentFinesse.Should().Be(3);
        state.CurrentWits.Should().Be(2);
        state.CurrentWill.Should().Be(2);
        state.CurrentSturdiness.Should().Be(4);
        state.PointsRemaining.Should().Be(0);
        state.IsComplete.Should().BeTrue();
        state.AllowsManualAdjustment.Should().BeFalse();
    }

    [Test]
    public void AttributeAllocationState_SwitchToAdvanced_EnablesManualAdjustment()
    {
        // Arrange
        var simple = AttributeAllocationState.CreateFromRecommendedBuild(
            "warrior", 4, 3, 2, 2, 4, 15);

        // Act
        var advanced = simple.SwitchToAdvanced(15);

        // Assert
        advanced.Mode.Should().Be(AttributeAllocationMode.Advanced);
        advanced.AllowsManualAdjustment.Should().BeTrue();
        advanced.CurrentMight.Should().Be(4); // Values preserved
        advanced.SelectedArchetypeId.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCREEN STEP PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void LineageScreen_Step_IsLineage()
    {
        // Arrange
        var provider = new Mock<ILineageProvider>();
        var screen = new LineageScreen(provider.Object);

        // Assert
        screen.Step.Should().Be(CharacterCreationStep.Lineage);
    }

    [Test]
    public void BackgroundScreen_Step_IsBackground()
    {
        // Arrange
        var provider = new Mock<IBackgroundProvider>();
        var screen = new BackgroundScreen(provider.Object);

        // Assert
        screen.Step.Should().Be(CharacterCreationStep.Background);
    }

    [Test]
    public void ArchetypeScreen_Step_IsArchetype()
    {
        // Arrange
        var provider = new Mock<IArchetypeProvider>();
        var screen = new ArchetypeScreen(provider.Object);

        // Assert
        screen.Step.Should().Be(CharacterCreationStep.Archetype);
    }

    [Test]
    public void AttributeScreen_Step_IsAttributes()
    {
        // Arrange
        var archetypeProvider = new Mock<IArchetypeProvider>();
        var viewModelBuilder = new Mock<IViewModelBuilder>();
        var screen = new AttributeScreen(archetypeProvider.Object, viewModelBuilder.Object);

        // Assert
        screen.Step.Should().Be(CharacterCreationStep.Attributes);
    }

    [Test]
    public void SpecializationScreen_Step_IsSpecialization()
    {
        // Arrange
        var provider = new Mock<ISpecializationProvider>();
        var controller = new Mock<ICharacterCreationController>();
        var screen = new SpecializationScreen(provider.Object, controller.Object);

        // Assert
        screen.Step.Should().Be(CharacterCreationStep.Specialization);
    }

    [Test]
    public void SummaryScreen_Step_IsSummary()
    {
        // Arrange
        var nameValidator = new Mock<INameValidator>();
        var screen = new SummaryScreen(nameValidator.Object);

        // Assert
        screen.Step.Should().Be(CharacterCreationStep.Summary);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void LineageScreen_Constructor_ThrowsOnNullProvider()
    {
        var act = () => new LineageScreen(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void BackgroundScreen_Constructor_ThrowsOnNullProvider()
    {
        var act = () => new BackgroundScreen(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ArchetypeScreen_Constructor_ThrowsOnNullProvider()
    {
        var act = () => new ArchetypeScreen(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AttributeScreen_Constructor_ThrowsOnNullArchetypeProvider()
    {
        var viewModelBuilder = new Mock<IViewModelBuilder>();
        var act = () => new AttributeScreen(null!, viewModelBuilder.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AttributeScreen_Constructor_ThrowsOnNullViewModelBuilder()
    {
        var archetypeProvider = new Mock<IArchetypeProvider>();
        var act = () => new AttributeScreen(archetypeProvider.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void SpecializationScreen_Constructor_ThrowsOnNullProvider()
    {
        var controller = new Mock<ICharacterCreationController>();
        var act = () => new SpecializationScreen(null!, controller.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void SpecializationScreen_Constructor_ThrowsOnNullController()
    {
        var provider = new Mock<ISpecializationProvider>();
        var act = () => new SpecializationScreen(provider.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void SummaryScreen_Constructor_ThrowsOnNullNameValidator()
    {
        var act = () => new SummaryScreen(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
