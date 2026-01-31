// ═══════════════════════════════════════════════════════════════════════════════
// ViewModelBuilderTests.cs
// Unit tests for the ViewModelBuilder application service. Verifies ViewModel
// construction from CharacterCreationState including step information, navigation
// state, selection display names, preview data (derived stats, abilities,
// equipment), and validation status. Uses Moq for provider dependencies and
// FluentAssertions for readable assertions.
// Version: 0.17.5b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
/// Unit tests for <see cref="ViewModelBuilder"/>.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover all major scenarios of the Build() and BuildDerivedStatsPreview() methods:
/// </para>
/// <list type="bullet">
///   <item><description>Step information (title, description, step number, total steps)</description></item>
///   <item><description>Navigation state (back, forward, permanent choice warning)</description></item>
///   <item><description>Selection display names from providers</description></item>
///   <item><description>Derived stats preview with lineage and archetype bonuses</description></item>
///   <item><description>Abilities preview from archetype and specialization</description></item>
///   <item><description>Equipment preview from background</description></item>
///   <item><description>Validation errors and step validity</description></item>
///   <item><description>Null/missing data graceful degradation</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ViewModelBuilderTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    private Mock<ILineageProvider> _mockLineageProvider = null!;
    private Mock<IBackgroundProvider> _mockBackgroundProvider = null!;
    private Mock<IArchetypeProvider> _mockArchetypeProvider = null!;
    private Mock<ISpecializationProvider> _mockSpecializationProvider = null!;
    private Mock<IDerivedStatCalculator> _mockDerivedStatCalculator = null!;
    private ViewModelBuilder _builder = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes mock providers and creates the ViewModelBuilder for each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLineageProvider = new Mock<ILineageProvider>();
        _mockBackgroundProvider = new Mock<IBackgroundProvider>();
        _mockArchetypeProvider = new Mock<IArchetypeProvider>();
        _mockSpecializationProvider = new Mock<ISpecializationProvider>();
        _mockDerivedStatCalculator = new Mock<IDerivedStatCalculator>();

        _builder = new ViewModelBuilder(
            _mockLineageProvider.Object,
            _mockBackgroundProvider.Object,
            _mockArchetypeProvider.Object,
            _mockSpecializationProvider.Object,
            _mockDerivedStatCalculator.Object,
            NullLogger<ViewModelBuilder>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NULL GUARD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() throws ArgumentNullException when state is null.
    /// </summary>
    [Test]
    public void Build_NullState_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => _builder.Build(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("state");
    }

    /// <summary>
    /// BuildDerivedStatsPreview() throws ArgumentNullException when state is null.
    /// </summary>
    [Test]
    public void BuildDerivedStatsPreview_NullState_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => _builder.BuildDerivedStatsPreview(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("state");
    }

    /// <summary>
    /// Constructor throws ArgumentNullException when any required provider is null.
    /// </summary>
    [Test]
    public void Constructor_NullLineageProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ViewModelBuilder(
            null!,
            _mockBackgroundProvider.Object,
            _mockArchetypeProvider.Object,
            _mockSpecializationProvider.Object,
            _mockDerivedStatCalculator.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lineageProvider");
    }

    /// <summary>
    /// Constructor throws ArgumentNullException when derived stat calculator is null.
    /// </summary>
    [Test]
    public void Constructor_NullDerivedStatCalculator_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ViewModelBuilder(
            _mockLineageProvider.Object,
            _mockBackgroundProvider.Object,
            _mockArchetypeProvider.Object,
            _mockSpecializationProvider.Object,
            null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("derivedStatCalculator");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STEP INFORMATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() populates step information for the Lineage step (Step 1).
    /// Verifies StepTitle, StepDescription, StepNumber, and TotalSteps.
    /// </summary>
    [Test]
    public void Build_LineageStep_PopulatesStepInformation()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act
        var viewModel = _builder.Build(state);

        // Assert — Step 1 is Lineage
        viewModel.CurrentStep.Should().Be(CharacterCreationStep.Lineage);
        viewModel.StepTitle.Should().Be("Choose Your Lineage");
        viewModel.StepDescription.Should().NotBeNullOrEmpty();
        viewModel.StepNumber.Should().Be(1);
        viewModel.TotalSteps.Should().Be(6);
    }

    /// <summary>
    /// Build() sets correct step number for each step in the workflow.
    /// </summary>
    /// <param name="step">The step to set on the creation state.</param>
    /// <param name="expectedStepNumber">The expected 1-based step number.</param>
    [TestCase(CharacterCreationStep.Lineage, 1)]
    [TestCase(CharacterCreationStep.Background, 2)]
    [TestCase(CharacterCreationStep.Attributes, 3)]
    [TestCase(CharacterCreationStep.Archetype, 4)]
    [TestCase(CharacterCreationStep.Specialization, 5)]
    [TestCase(CharacterCreationStep.Summary, 6)]
    public void Build_EachStep_SetsCorrectStepNumber(CharacterCreationStep step, int expectedStepNumber)
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = step;

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.StepNumber.Should().Be(expectedStepNumber);
        viewModel.TotalSteps.Should().Be(6);
    }

    /// <summary>
    /// Build() computes the ProgressIndicator correctly for each step.
    /// </summary>
    [Test]
    public void Build_Step3_ProgressIndicatorFormatted()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Attributes;

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.ProgressIndicator.Should().Be("Step 3 of 6");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NAVIGATION STATE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() sets CanGoBack to false for Step 1 (Lineage).
    /// </summary>
    [Test]
    public void Build_LineageStep_CanGoBackIsFalse()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.CanGoBack.Should().BeFalse();
    }

    /// <summary>
    /// Build() sets CanGoBack to true for steps after Lineage.
    /// </summary>
    [TestCase(CharacterCreationStep.Background)]
    [TestCase(CharacterCreationStep.Attributes)]
    [TestCase(CharacterCreationStep.Archetype)]
    [TestCase(CharacterCreationStep.Specialization)]
    [TestCase(CharacterCreationStep.Summary)]
    public void Build_StepsAfterLineage_CanGoBackIsTrue(CharacterCreationStep step)
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = step;

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.CanGoBack.Should().BeTrue();
    }

    /// <summary>
    /// Build() sets CanGoForward to false when the current step is not complete.
    /// </summary>
    [Test]
    public void Build_IncompleteStep_CanGoForwardIsFalse()
    {
        // Arrange — fresh state has no lineage selected
        var state = CharacterCreationState.Create();

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.CanGoForward.Should().BeFalse();
    }

    /// <summary>
    /// Build() sets CanGoForward to true when the lineage step is complete
    /// (lineage selected with no Clan-Born flexible bonus requirement).
    /// </summary>
    [Test]
    public void Build_CompleteLineageStep_CanGoForwardIsTrue()
    {
        // Arrange — select a lineage that does NOT require flexible bonus (RuneMarked)
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.RuneMarked;

        SetupLineageProvider(Lineage.RuneMarked, "Rune-Marked", hpBonus: 0, apBonus: 5);

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.CanGoForward.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PERMANENT CHOICE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() sets IsPermanentChoice and PermanentWarning for the Archetype step.
    /// </summary>
    [Test]
    public void Build_ArchetypeStep_SetsPermanentChoice()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Archetype;

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.IsPermanentChoice.Should().BeTrue();
        viewModel.PermanentWarning.Should().NotBeNullOrEmpty();
        viewModel.PermanentWarning.Should().Contain("PERMANENT");
    }

    /// <summary>
    /// Build() does not set permanent choice for non-Archetype steps.
    /// </summary>
    [TestCase(CharacterCreationStep.Lineage)]
    [TestCase(CharacterCreationStep.Background)]
    [TestCase(CharacterCreationStep.Attributes)]
    [TestCase(CharacterCreationStep.Specialization)]
    [TestCase(CharacterCreationStep.Summary)]
    public void Build_NonArchetypeStep_NoPermanentChoice(CharacterCreationStep step)
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = step;

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.IsPermanentChoice.Should().BeFalse();
        viewModel.PermanentWarning.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SELECTION DISPLAY NAME TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() resolves lineage display name from ILineageProvider.
    /// </summary>
    [Test]
    public void Build_WithLineageSelected_ResolvesDisplayName()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.ClanBorn;
        state.FlexibleAttributeBonus = CoreAttribute.Might;

        SetupLineageProvider(Lineage.ClanBorn, "Clan-Born", hpBonus: 5, apBonus: 0);

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.SelectedLineageName.Should().Be("Clan-Born");
    }

    /// <summary>
    /// Build() resolves background display name from IBackgroundProvider.
    /// </summary>
    [Test]
    public void Build_WithBackgroundSelected_ResolvesDisplayName()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Background;
        state.SelectedBackground = Background.VillageSmith;

        SetupBackgroundProvider(Background.VillageSmith, "Village Smith");

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.SelectedBackgroundName.Should().Be("Village Smith");
    }

    /// <summary>
    /// Build() resolves archetype display name from IArchetypeProvider.
    /// </summary>
    [Test]
    public void Build_WithArchetypeSelected_ResolvesDisplayName()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Archetype;
        state.SelectedArchetype = Archetype.Warrior;

        SetupArchetypeProvider(Archetype.Warrior, "Warrior");

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.SelectedArchetypeName.Should().Be("Warrior");
    }

    /// <summary>
    /// Build() resolves specialization display name from ISpecializationProvider.
    /// </summary>
    [Test]
    public void Build_WithSpecializationSelected_ResolvesDisplayName()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Specialization;
        state.SelectedArchetype = Archetype.Warrior;
        state.SelectedSpecialization = SpecializationId.Berserkr;

        SetupArchetypeProvider(Archetype.Warrior, "Warrior");
        SetupSpecializationProvider(SpecializationId.Berserkr, "Berserkr", Archetype.Warrior);

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.SelectedSpecializationName.Should().Be("Berserkr");
    }

    /// <summary>
    /// Build() returns null display names when no selections have been made.
    /// </summary>
    [Test]
    public void Build_NoSelections_AllDisplayNamesAreNull()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.SelectedLineageName.Should().BeNull();
        viewModel.SelectedBackgroundName.Should().BeNull();
        viewModel.SelectedArchetypeName.Should().BeNull();
        viewModel.SelectedSpecializationName.Should().BeNull();
        viewModel.CharacterName.Should().BeNull();
        viewModel.HasAnySelections.Should().BeFalse();
    }

    /// <summary>
    /// Build() includes the character name from state when set.
    /// </summary>
    [Test]
    public void Build_WithCharacterName_IncludesNameInViewModel()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Summary;
        state.CharacterName = "Thorvald";

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.CharacterName.Should().Be("Thorvald");
        viewModel.HasAnySelections.Should().BeTrue();
    }

    /// <summary>
    /// Build() gracefully handles missing provider definitions by returning null display names.
    /// </summary>
    [Test]
    public void Build_MissingLineageDefinition_ReturnsNullDisplayName()
    {
        // Arrange — lineage selected but provider returns null
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.ClanBorn;
        state.FlexibleAttributeBonus = CoreAttribute.Might;

        _mockLineageProvider.Setup(p => p.GetLineage(Lineage.ClanBorn))
            .Returns((LineageDefinition?)null);

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.SelectedLineageName.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED STATS PREVIEW TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() returns null DerivedStatsPreview when no attributes are allocated.
    /// </summary>
    [Test]
    public void Build_NoAttributes_DerivedStatsPreviewIsNull()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.DerivedStatsPreview.Should().BeNull();
    }

    /// <summary>
    /// Build() populates DerivedStatsPreview when attributes are allocated.
    /// Verifies the calculator is called and bonuses are resolved from providers.
    /// </summary>
    [Test]
    public void Build_WithAttributes_PopulatesDerivedStatsPreview()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Attributes;
        state.SelectedLineage = Lineage.ClanBorn;
        state.FlexibleAttributeBonus = CoreAttribute.Might;
        state.SelectedArchetype = Archetype.Warrior;
        state.Attributes = AttributeAllocationState.CreateAdvancedDefault(15);

        SetupLineageProvider(Lineage.ClanBorn, "Clan-Born", hpBonus: 5, apBonus: 0);
        SetupArchetypeProvider(Archetype.Warrior, "Warrior");

        _mockArchetypeProvider.Setup(p => p.GetResourceBonuses(Archetype.Warrior))
            .Returns(ArchetypeResourceBonuses.Warrior);

        // Calculator returns stats for the attributes + archetype + lineage
        _mockDerivedStatCalculator.Setup(c => c.GetPreview(
                It.IsAny<AttributeAllocationState>(),
                "warrior",
                "clan-born"))
            .Returns(DerivedStats.Create(
                maxHp: 104, maxStamina: 55, maxAetherPool: 30,
                initiative: 3, soak: 2, movementSpeed: 5, carryingCapacity: 40));

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.DerivedStatsPreview.Should().NotBeNull();
        viewModel.DerivedStatsPreview!.Value.MaxHp.Should().Be(104);
        viewModel.DerivedStatsPreview.Value.MaxStamina.Should().Be(55);
        viewModel.DerivedStatsPreview.Value.MaxAp.Should().Be(30); // MaxAetherPool → MaxAp
        viewModel.DerivedStatsPreview.Value.HpFromLineage.Should().Be(5);
        viewModel.DerivedStatsPreview.Value.HpFromArchetype.Should().Be(49); // Warrior
        viewModel.DerivedStatsPreview.Value.ApFromLineage.Should().Be(0);
        viewModel.DerivedStatsPreview.Value.ApFromArchetype.Should().Be(0); // Warrior has no AP bonus
        viewModel.DerivedStatsPreview.Value.StaminaFromArchetype.Should().Be(5); // Warrior
    }

    /// <summary>
    /// BuildDerivedStatsPreview() returns DerivedStatsPreview.Empty when no attributes allocated.
    /// </summary>
    [Test]
    public void BuildDerivedStatsPreview_NoAttributes_ReturnsEmpty()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act
        var preview = _builder.BuildDerivedStatsPreview(state);

        // Assert
        preview.HasValues.Should().BeFalse();
        preview.MaxHp.Should().Be(0);
        preview.MaxStamina.Should().Be(0);
        preview.MaxAp.Should().Be(0);
    }

    /// <summary>
    /// BuildDerivedStatsPreview() calculates stats with Rune-Marked lineage AP bonus.
    /// </summary>
    [Test]
    public void BuildDerivedStatsPreview_RuneMarkedMystic_IncludesApBonuses()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.RuneMarked;
        state.SelectedArchetype = Archetype.Mystic;
        state.Attributes = AttributeAllocationState.CreateAdvancedDefault(15);

        SetupLineageProvider(Lineage.RuneMarked, "Rune-Marked", hpBonus: 0, apBonus: 5);

        _mockArchetypeProvider.Setup(p => p.GetResourceBonuses(Archetype.Mystic))
            .Returns(ArchetypeResourceBonuses.Mystic);

        _mockDerivedStatCalculator.Setup(c => c.GetPreview(
                It.IsAny<AttributeAllocationState>(),
                "mystic",
                "rune-marked"))
            .Returns(DerivedStats.Create(
                maxHp: 80, maxStamina: 30, maxAetherPool: 60,
                initiative: 4, soak: 1, movementSpeed: 5, carryingCapacity: 30));

        // Act
        var preview = _builder.BuildDerivedStatsPreview(state);

        // Assert
        preview.HasValues.Should().BeTrue();
        preview.MaxHp.Should().Be(80);
        preview.MaxAp.Should().Be(60);
        preview.ApFromLineage.Should().Be(5);
        preview.ApFromArchetype.Should().Be(20); // Mystic
        preview.HpFromArchetype.Should().Be(20); // Mystic
        preview.StaminaFromArchetype.Should().Be(0); // Mystic has no stamina bonus
    }

    /// <summary>
    /// BuildDerivedStatsPreview() works without archetype or lineage selected.
    /// </summary>
    [Test]
    public void BuildDerivedStatsPreview_AttributesOnly_UsesNoBonuses()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.Attributes = AttributeAllocationState.CreateAdvancedDefault(15);

        _mockDerivedStatCalculator.Setup(c => c.GetPreview(
                It.IsAny<AttributeAllocationState>(),
                null,
                null))
            .Returns(DerivedStats.Create(
                maxHp: 60, maxStamina: 30, maxAetherPool: 15,
                initiative: 2, soak: 1, movementSpeed: 5, carryingCapacity: 30));

        // Act
        var preview = _builder.BuildDerivedStatsPreview(state);

        // Assert
        preview.HasValues.Should().BeTrue();
        preview.MaxHp.Should().Be(60);
        preview.HpFromLineage.Should().Be(0);
        preview.HpFromArchetype.Should().Be(0);
        preview.ApFromLineage.Should().Be(0);
        preview.ApFromArchetype.Should().Be(0);
        preview.StaminaFromArchetype.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ABILITIES PREVIEW TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() returns null AbilitiesPreview when no archetype is selected.
    /// </summary>
    [Test]
    public void Build_NoArchetype_AbilitiesPreviewIsNull()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.AbilitiesPreview.Should().BeNull();
    }

    /// <summary>
    /// Build() populates archetype abilities from IArchetypeProvider.GetStartingAbilities().
    /// </summary>
    [Test]
    public void Build_WithArchetype_PopulatesArchetypeAbilities()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Archetype;
        state.SelectedArchetype = Archetype.Warrior;

        SetupArchetypeProvider(Archetype.Warrior, "Warrior");

        var startingAbilities = new List<ArchetypeAbilityGrant>
        {
            ArchetypeAbilityGrant.CreateActive("power-strike", "Power Strike", "A powerful melee attack."),
            ArchetypeAbilityGrant.CreateStance("defensive-stance", "Defensive Stance", "Reduce damage taken."),
            ArchetypeAbilityGrant.CreatePassive("iron-will", "Iron Will", "Resist mental effects.")
        };
        _mockArchetypeProvider.Setup(p => p.GetStartingAbilities(Archetype.Warrior))
            .Returns(startingAbilities.AsReadOnly());

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.AbilitiesPreview.Should().NotBeNull();
        viewModel.AbilitiesPreview!.Value.ArchetypeAbilities.Should().HaveCount(3);
        viewModel.AbilitiesPreview.Value.ArchetypeAbilities.Should()
            .Contain("Power Strike")
            .And.Contain("Defensive Stance")
            .And.Contain("Iron Will");
        viewModel.AbilitiesPreview.Value.SpecializationAbilities.Should().BeEmpty();
    }

    /// <summary>
    /// Build() populates both archetype and specialization Tier 1 abilities.
    /// </summary>
    [Test]
    public void Build_WithArchetypeAndSpecialization_PopulatesBothAbilityLists()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Specialization;
        state.SelectedArchetype = Archetype.Warrior;
        state.SelectedSpecialization = SpecializationId.Berserkr;

        SetupArchetypeProvider(Archetype.Warrior, "Warrior");

        var startingAbilities = new List<ArchetypeAbilityGrant>
        {
            ArchetypeAbilityGrant.CreateActive("power-strike", "Power Strike", "A powerful melee attack."),
            ArchetypeAbilityGrant.CreateStance("defensive-stance", "Defensive Stance", "Reduce damage taken."),
            ArchetypeAbilityGrant.CreatePassive("iron-will", "Iron Will", "Resist mental effects.")
        };
        _mockArchetypeProvider.Setup(p => p.GetStartingAbilities(Archetype.Warrior))
            .Returns(startingAbilities.AsReadOnly());

        // Create specialization with Tier 1 abilities
        var tier1Abilities = new[]
        {
            SpecializationAbility.CreateActive("rage-strike", "Rage Strike",
                "Channel rage into a devastating blow.", resourceCost: 20, resourceType: "rage"),
            SpecializationAbility.CreatePassive("blood-fury", "Blood Fury",
                "Gain rage when taking damage.")
        };
        var tier1 = SpecializationAbilityTier.CreateTier1("Core Techniques", tier1Abilities);

        var specDef = SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Fury Unleashed",
            "Warriors consumed by battle rage.",
            "Embrace the fury of the berserker.",
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            unlockCost: 0,
            abilityTiers: new[] { tier1 });

        _mockSpecializationProvider.Setup(p => p.GetBySpecializationId(SpecializationId.Berserkr))
            .Returns(specDef);

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.AbilitiesPreview.Should().NotBeNull();
        viewModel.AbilitiesPreview!.Value.ArchetypeAbilities.Should().HaveCount(3);
        viewModel.AbilitiesPreview.Value.SpecializationAbilities.Should().HaveCount(2);
        viewModel.AbilitiesPreview.Value.SpecializationAbilities.Should()
            .Contain("Rage Strike")
            .And.Contain("Blood Fury");
        viewModel.AbilitiesPreview.Value.HasSpecializationAbilities.Should().BeTrue();
        viewModel.AbilitiesPreview.Value.TotalCount.Should().Be(5);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EQUIPMENT PREVIEW TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() returns null EquipmentPreview when no background is selected.
    /// </summary>
    [Test]
    public void Build_NoBackground_EquipmentPreviewIsNull()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.EquipmentPreview.Should().BeNull();
    }

    /// <summary>
    /// Build() populates equipment preview from background equipment grants.
    /// Verifies item name transformation (kebab-case to title case), quantity,
    /// and item type derivation.
    /// </summary>
    [Test]
    public void Build_WithBackground_PopulatesEquipmentPreview()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Background;
        state.SelectedBackground = Background.VillageSmith;

        var equipmentGrants = new List<BackgroundEquipmentGrant>
        {
            BackgroundEquipmentGrant.Equipped("smiths-hammer", EquipmentSlot.Weapon),
            BackgroundEquipmentGrant.Equipped("leather-apron", EquipmentSlot.Armor),
            BackgroundEquipmentGrant.Consumable("bandages", 3)
        };

        var backgroundDef = BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "You worked the forge.",
            "The ring of hammer on anvil was your morning song.",
            "Blacksmith",
            "Respected craftsperson",
            equipmentGrants: equipmentGrants);

        _mockBackgroundProvider.Setup(p => p.GetBackground(Background.VillageSmith))
            .Returns(backgroundDef);

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.EquipmentPreview.Should().NotBeNull();
        viewModel.EquipmentPreview!.Value.FromBackground.Should().Be("Village Smith");
        viewModel.EquipmentPreview.Value.Items.Should().HaveCount(3);
        viewModel.EquipmentPreview.Value.HasItems.Should().BeTrue();

        // Verify item name transformation: kebab-case → title case
        viewModel.EquipmentPreview.Value.Items[0].Name.Should().Be("Smiths Hammer");
        viewModel.EquipmentPreview.Value.Items[0].ItemType.Should().Be("Weapon");
        viewModel.EquipmentPreview.Value.Items[0].Quantity.Should().Be(1);

        viewModel.EquipmentPreview.Value.Items[1].Name.Should().Be("Leather Apron");
        viewModel.EquipmentPreview.Value.Items[1].ItemType.Should().Be("Armor");

        // Consumable: non-equipped, quantity > 1 → "Consumable"
        viewModel.EquipmentPreview.Value.Items[2].Name.Should().Be("Bandages");
        viewModel.EquipmentPreview.Value.Items[2].ItemType.Should().Be("Consumable");
        viewModel.EquipmentPreview.Value.Items[2].Quantity.Should().Be(3);
    }

    /// <summary>
    /// Build() derives "Item" type for non-equipped, single-quantity items.
    /// </summary>
    [Test]
    public void Build_InventoryItem_DerivesItemType()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Background;
        state.SelectedBackground = Background.RuinDelver;

        var equipmentGrants = new List<BackgroundEquipmentGrant>
        {
            BackgroundEquipmentGrant.Inventory("climbing-rope", 1)
        };

        var backgroundDef = BackgroundDefinition.Create(
            Background.RuinDelver,
            "Ruin Delver",
            "You explored ancient ruins.",
            "The deep places call to you.",
            "Explorer",
            "Tolerated outsider",
            equipmentGrants: equipmentGrants);

        _mockBackgroundProvider.Setup(p => p.GetBackground(Background.RuinDelver))
            .Returns(backgroundDef);

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.EquipmentPreview.Should().NotBeNull();
        viewModel.EquipmentPreview!.Value.Items[0].Name.Should().Be("Climbing Rope");
        viewModel.EquipmentPreview.Value.Items[0].ItemType.Should().Be("Item");
        viewModel.EquipmentPreview.Value.Items[0].Quantity.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() sets IsCurrentStepValid to true when step is complete with no validation errors.
    /// </summary>
    [Test]
    public void Build_CompleteStepNoErrors_IsCurrentStepValidTrue()
    {
        // Arrange — RuneMarked lineage is complete immediately (no flexible bonus needed)
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.RuneMarked;

        SetupLineageProvider(Lineage.RuneMarked, "Rune-Marked", hpBonus: 0, apBonus: 5);

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.IsCurrentStepValid.Should().BeTrue();
        viewModel.ValidationErrors.Should().BeEmpty();
    }

    /// <summary>
    /// Build() sets IsCurrentStepValid to false when step is incomplete.
    /// </summary>
    [Test]
    public void Build_IncompleteStep_IsCurrentStepValidFalse()
    {
        // Arrange — no lineage selected
        var state = CharacterCreationState.Create();

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.IsCurrentStepValid.Should().BeFalse();
    }

    /// <summary>
    /// Build() includes validation errors from the state in the ViewModel.
    /// </summary>
    [Test]
    public void Build_WithValidationErrors_IncludesErrorsInViewModel()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.ValidationErrors.Add("Name must be at least 2 characters.");
        state.ValidationErrors.Add("Name contains invalid characters.");

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.ValidationErrors.Should().HaveCount(2);
        viewModel.ValidationErrors.Should().Contain("Name must be at least 2 characters.");
        viewModel.ValidationErrors.Should().Contain("Name contains invalid characters.");
        viewModel.IsCurrentStepValid.Should().BeFalse();
    }

    /// <summary>
    /// Build() sets IsCurrentStepValid to false when step is complete but has validation errors.
    /// </summary>
    [Test]
    public void Build_CompleteStepWithErrors_IsCurrentStepValidFalse()
    {
        // Arrange — lineage selected but validation errors added externally
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.RuneMarked;
        state.ValidationErrors.Add("Some external validation error");

        SetupLineageProvider(Lineage.RuneMarked, "Rune-Marked", hpBonus: 0, apBonus: 5);

        // Act
        var viewModel = _builder.Build(state);

        // Assert — step is complete but has errors → not valid
        viewModel.IsCurrentStepValid.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PREVIEW DATA COMPUTED PROPERTIES TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() sets HasPreviewData to false when no preview data is available.
    /// </summary>
    [Test]
    public void Build_NoPreviewData_HasPreviewDataIsFalse()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.HasPreviewData.Should().BeFalse();
    }

    /// <summary>
    /// Build() sets HasPreviewData to true when equipment preview is populated.
    /// </summary>
    [Test]
    public void Build_WithEquipmentPreview_HasPreviewDataIsTrue()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedBackground = Background.ClanGuard;

        var backgroundDef = BackgroundDefinition.Create(
            Background.ClanGuard,
            "Clan Guard",
            "You stood watch.",
            "Steel and vigilance were your companions.",
            "Guard",
            "Trusted protector",
            equipmentGrants: new List<BackgroundEquipmentGrant>
            {
                BackgroundEquipmentGrant.Equipped("shield", EquipmentSlot.Shield)
            });

        _mockBackgroundProvider.Setup(p => p.GetBackground(Background.ClanGuard))
            .Returns(backgroundDef);

        // Act
        var viewModel = _builder.Build(state);

        // Assert
        viewModel.HasPreviewData.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FULL WORKFLOW INTEGRATION TEST
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Build() assembles a complete ViewModel with all selections and preview data
    /// for a fully-configured character at the Summary step.
    /// </summary>
    [Test]
    public void Build_SummaryStep_FullyPopulatedViewModel()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Summary;
        state.SelectedLineage = Lineage.ClanBorn;
        state.FlexibleAttributeBonus = CoreAttribute.Might;
        state.SelectedBackground = Background.VillageSmith;
        state.SelectedArchetype = Archetype.Warrior;
        state.SelectedSpecialization = SpecializationId.Berserkr;
        state.CharacterName = "Thorvald";
        state.Attributes = AttributeAllocationState.CreateAdvancedDefault(15);

        // Setup all providers
        SetupLineageProvider(Lineage.ClanBorn, "Clan-Born", hpBonus: 5, apBonus: 0);
        SetupArchetypeProvider(Archetype.Warrior, "Warrior");

        _mockArchetypeProvider.Setup(p => p.GetResourceBonuses(Archetype.Warrior))
            .Returns(ArchetypeResourceBonuses.Warrior);

        _mockArchetypeProvider.Setup(p => p.GetStartingAbilities(Archetype.Warrior))
            .Returns(new List<ArchetypeAbilityGrant>
            {
                ArchetypeAbilityGrant.CreateActive("power-strike", "Power Strike", "Attack."),
                ArchetypeAbilityGrant.CreateStance("defensive-stance", "Defensive Stance", "Defend."),
                ArchetypeAbilityGrant.CreatePassive("iron-will", "Iron Will", "Resist.")
            }.AsReadOnly());

        var tier1Abilities = new[]
        {
            SpecializationAbility.CreateActive("rage-strike", "Rage Strike",
                "Rage attack.", resourceCost: 20, resourceType: "rage")
        };
        var tier1 = SpecializationAbilityTier.CreateTier1("Core Techniques", tier1Abilities);
        var specDef = SpecializationDefinition.Create(
            SpecializationId.Berserkr, "Berserkr", "Fury Unleashed",
            "Warriors consumed by battle rage.", "Embrace fury.",
            Archetype.Warrior, SpecializationPathType.Heretical,
            unlockCost: 0, abilityTiers: new[] { tier1 });
        _mockSpecializationProvider.Setup(p => p.GetBySpecializationId(SpecializationId.Berserkr))
            .Returns(specDef);

        var backgroundDef = BackgroundDefinition.Create(
            Background.VillageSmith, "Village Smith", "Forge worker.",
            "Hammer song.", "Blacksmith", "Respected",
            equipmentGrants: new List<BackgroundEquipmentGrant>
            {
                BackgroundEquipmentGrant.Equipped("smiths-hammer", EquipmentSlot.Weapon),
                BackgroundEquipmentGrant.Equipped("leather-apron", EquipmentSlot.Armor)
            });
        _mockBackgroundProvider.Setup(p => p.GetBackground(Background.VillageSmith))
            .Returns(backgroundDef);

        _mockDerivedStatCalculator.Setup(c => c.GetPreview(
                It.IsAny<AttributeAllocationState>(), "warrior", "clan-born"))
            .Returns(DerivedStats.Create(
                maxHp: 104, maxStamina: 55, maxAetherPool: 0,
                initiative: 3, soak: 2, movementSpeed: 5, carryingCapacity: 40));

        // Act
        var viewModel = _builder.Build(state);

        // Assert — Step info
        viewModel.CurrentStep.Should().Be(CharacterCreationStep.Summary);
        viewModel.StepNumber.Should().Be(6);
        viewModel.CanGoBack.Should().BeTrue();
        viewModel.IsPermanentChoice.Should().BeFalse();

        // Assert — Selections
        viewModel.SelectedLineageName.Should().Be("Clan-Born");
        viewModel.SelectedBackgroundName.Should().Be("Village Smith");
        viewModel.SelectedArchetypeName.Should().Be("Warrior");
        viewModel.SelectedSpecializationName.Should().Be("Berserkr");
        viewModel.CharacterName.Should().Be("Thorvald");
        viewModel.HasAnySelections.Should().BeTrue();

        // Assert — Derived stats
        viewModel.DerivedStatsPreview.Should().NotBeNull();
        viewModel.DerivedStatsPreview!.Value.MaxHp.Should().Be(104);

        // Assert — Abilities
        viewModel.AbilitiesPreview.Should().NotBeNull();
        viewModel.AbilitiesPreview!.Value.ArchetypeAbilities.Should().HaveCount(3);
        viewModel.AbilitiesPreview.Value.SpecializationAbilities.Should().HaveCount(1);

        // Assert — Equipment
        viewModel.EquipmentPreview.Should().NotBeNull();
        viewModel.EquipmentPreview!.Value.Items.Should().HaveCount(2);
        viewModel.EquipmentPreview.Value.FromBackground.Should().Be("Village Smith");

        // Assert — Preview data
        viewModel.HasPreviewData.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets up the mock lineage provider to return a lineage definition with the
    /// specified display name and bonus values.
    /// </summary>
    /// <param name="lineage">The lineage to mock.</param>
    /// <param name="displayName">The display name to return.</param>
    /// <param name="hpBonus">The HP bonus from lineage passive bonuses.</param>
    /// <param name="apBonus">The AP bonus from lineage passive bonuses.</param>
    private void SetupLineageProvider(Lineage lineage, string displayName, int hpBonus, int apBonus)
    {
        var attributeModifiers = lineage switch
        {
            Lineage.ClanBorn => LineageAttributeModifiers.ClanBorn,
            Lineage.RuneMarked => LineageAttributeModifiers.RuneMarked,
            Lineage.IronBlooded => LineageAttributeModifiers.IronBlooded,
            Lineage.VargrKin => LineageAttributeModifiers.VargrKin,
            _ => LineageAttributeModifiers.ClanBorn
        };

        var definition = LineageDefinition.Create(
            lineage,
            displayName,
            $"Description for {displayName}.",
            $"Selection text for {displayName}.",
            attributeModifiers,
            LineagePassiveBonuses.Create(hpBonus, apBonus, 0, 0),
            lineage == Lineage.RuneMarked ? LineageTrait.AetherTainted : LineageTrait.None,
            LineageTraumaBaseline.None);

        _mockLineageProvider.Setup(p => p.GetLineage(lineage))
            .Returns(definition);
    }

    /// <summary>
    /// Sets up the mock background provider to return a background definition
    /// with no equipment (for display name resolution only).
    /// </summary>
    /// <param name="background">The background to mock.</param>
    /// <param name="displayName">The display name to return.</param>
    private void SetupBackgroundProvider(Background background, string displayName)
    {
        var definition = BackgroundDefinition.Create(
            background,
            displayName,
            $"Description for {displayName}.",
            $"Selection text for {displayName}.",
            "Profession",
            "Standing");

        _mockBackgroundProvider.Setup(p => p.GetBackground(background))
            .Returns(definition);
    }

    /// <summary>
    /// Sets up the mock archetype provider to return an archetype definition
    /// and configures default resource bonuses and empty starting abilities.
    /// </summary>
    /// <param name="archetype">The archetype to mock.</param>
    /// <param name="displayName">The display name to return.</param>
    private void SetupArchetypeProvider(Archetype archetype, string displayName)
    {
        var primaryResource = archetype == Archetype.Mystic
            ? ResourceType.AetherPool
            : ResourceType.Stamina;

        var definition = ArchetypeDefinition.Create(
            archetype,
            displayName,
            $"{displayName} tagline",
            $"Description for {displayName}.",
            $"Selection text for {displayName}.",
            "Melee DPS",
            primaryResource,
            "Playstyle description.");

        _mockArchetypeProvider.Setup(p => p.GetArchetype(archetype))
            .Returns(definition);

        // Default resource bonuses (can be overridden per test)
        var bonuses = archetype switch
        {
            Archetype.Warrior => ArchetypeResourceBonuses.Warrior,
            Archetype.Skirmisher => ArchetypeResourceBonuses.Skirmisher,
            Archetype.Mystic => ArchetypeResourceBonuses.Mystic,
            Archetype.Adept => ArchetypeResourceBonuses.Adept,
            _ => ArchetypeResourceBonuses.None
        };
        _mockArchetypeProvider.Setup(p => p.GetResourceBonuses(archetype))
            .Returns(bonuses);

        // Default empty abilities (can be overridden per test)
        _mockArchetypeProvider.Setup(p => p.GetStartingAbilities(archetype))
            .Returns(new List<ArchetypeAbilityGrant>().AsReadOnly());
    }

    /// <summary>
    /// Sets up the mock specialization provider to return a specialization definition
    /// with no ability tiers (for display name resolution only).
    /// </summary>
    /// <param name="specId">The specialization ID to mock.</param>
    /// <param name="displayName">The display name to return.</param>
    /// <param name="parentArchetype">The parent archetype for validation.</param>
    private void SetupSpecializationProvider(
        SpecializationId specId, string displayName, Archetype parentArchetype)
    {
        var definition = SpecializationDefinition.Create(
            specId,
            displayName,
            $"{displayName} tagline",
            $"Description for {displayName}.",
            $"Selection text for {displayName}.",
            parentArchetype,
            SpecializationPathType.Heretical,
            unlockCost: 0);

        _mockSpecializationProvider.Setup(p => p.GetBySpecializationId(specId))
            .Returns(definition);
    }
}
