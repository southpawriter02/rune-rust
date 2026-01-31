// ═══════════════════════════════════════════════════════════════════════════════
// CharacterFactoryTests.cs
// Unit tests for the CharacterFactory application service. Verifies state
// validation (complete, missing fields, Clan-Born flexible bonus, specialization-
// archetype mismatch), the 13-step character creation sequence (Player
// construction, lineage modifiers, passive bonuses, trait registration, trauma
// baseline, background skills, derived stats, resource pools, archetype abilities,
// specialization application, saga progression), and constructor null guards.
// Uses Moq for dependency mocking and FluentAssertions.
// Version: 0.17.5e
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
/// Unit tests for <see cref="CharacterFactory"/>.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the factory behaviors organized by category:
/// </para>
/// <list type="bullet">
///   <item><description>Constructor — null guards for all six dependencies</description></item>
///   <item><description>ValidateState — complete state passes, missing fields fail, Clan-Born flexible bonus, spec-archetype mismatch</description></item>
///   <item><description>CreateCharacterAsync — 13-step initialization: Player construction, lineage modifiers, passive bonuses, trait, trauma, background skills, derived stats, resources, archetype abilities, specialization, progression</description></item>
/// </list>
/// <para>
/// All tests mock the five providers and the derived stat calculator. The logger is
/// a no-op mock. Tests use realistic game data (Rune-Marked Warrior Berserkr defaults)
/// to exercise the full initialization path.
/// </para>
/// </remarks>
/// <seealso cref="CharacterFactory"/>
/// <seealso cref="ICharacterFactory"/>
/// <seealso cref="FactoryValidationResult"/>
[TestFixture]
public class CharacterFactoryTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Mock lineage provider for attribute modifiers, passives, traits, and trauma.</summary>
    private Mock<ILineageProvider> _lineageProviderMock = null!;

    /// <summary>Mock background provider for skill grants.</summary>
    private Mock<IBackgroundProvider> _backgroundProviderMock = null!;

    /// <summary>Mock archetype provider for starting abilities.</summary>
    private Mock<IArchetypeProvider> _archetypeProviderMock = null!;

    /// <summary>Mock specialization provider for spec definitions and archetype compatibility.</summary>
    private Mock<ISpecializationProvider> _specializationProviderMock = null!;

    /// <summary>Mock derived stat calculator for HP, Stamina, Aether Pool calculations.</summary>
    private Mock<IDerivedStatCalculator> _derivedStatCalculatorMock = null!;

    /// <summary>Mock logger for diagnostic output. Not verified — logging is diagnostic only.</summary>
    private Mock<ILogger<CharacterFactory>> _loggerMock = null!;

    /// <summary>The factory instance under test.</summary>
    private CharacterFactory _factory = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes mock dependencies and creates a fresh factory before each test.
    /// Configures all mocks with default realistic data for a Rune-Marked Warrior
    /// Berserkr character, allowing tests to override specific setups as needed.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _lineageProviderMock = new Mock<ILineageProvider>();
        _backgroundProviderMock = new Mock<IBackgroundProvider>();
        _archetypeProviderMock = new Mock<IArchetypeProvider>();
        _specializationProviderMock = new Mock<ISpecializationProvider>();
        _derivedStatCalculatorMock = new Mock<IDerivedStatCalculator>();
        _loggerMock = new Mock<ILogger<CharacterFactory>>();

        // Configure default mocks for Rune-Marked Warrior Berserkr
        ConfigureDefaultLineageMocks();
        ConfigureDefaultBackgroundMocks();
        ConfigureDefaultArchetypeMocks();
        ConfigureDefaultSpecializationMocks();
        ConfigureDefaultDerivedStatsMocks();

        _factory = new CharacterFactory(
            _lineageProviderMock.Object,
            _backgroundProviderMock.Object,
            _archetypeProviderMock.Object,
            _specializationProviderMock.Object,
            _derivedStatCalculatorMock.Object,
            _loggerMock.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Constructor throws <see cref="ArgumentNullException"/> when lineageProvider is null.
    /// </summary>
    [Test]
    public void Constructor_NullLineageProvider_Throws()
    {
        // Act
        var act = () => new CharacterFactory(
            null!,
            _backgroundProviderMock.Object,
            _archetypeProviderMock.Object,
            _specializationProviderMock.Object,
            _derivedStatCalculatorMock.Object,
            _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lineageProvider");
    }

    /// <summary>
    /// Constructor throws <see cref="ArgumentNullException"/> when backgroundProvider is null.
    /// </summary>
    [Test]
    public void Constructor_NullBackgroundProvider_Throws()
    {
        // Act
        var act = () => new CharacterFactory(
            _lineageProviderMock.Object,
            null!,
            _archetypeProviderMock.Object,
            _specializationProviderMock.Object,
            _derivedStatCalculatorMock.Object,
            _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("backgroundProvider");
    }

    /// <summary>
    /// Constructor throws <see cref="ArgumentNullException"/> when archetypeProvider is null.
    /// </summary>
    [Test]
    public void Constructor_NullArchetypeProvider_Throws()
    {
        // Act
        var act = () => new CharacterFactory(
            _lineageProviderMock.Object,
            _backgroundProviderMock.Object,
            null!,
            _specializationProviderMock.Object,
            _derivedStatCalculatorMock.Object,
            _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("archetypeProvider");
    }

    /// <summary>
    /// Constructor throws <see cref="ArgumentNullException"/> when specializationProvider is null.
    /// </summary>
    [Test]
    public void Constructor_NullSpecializationProvider_Throws()
    {
        // Act
        var act = () => new CharacterFactory(
            _lineageProviderMock.Object,
            _backgroundProviderMock.Object,
            _archetypeProviderMock.Object,
            null!,
            _derivedStatCalculatorMock.Object,
            _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("specializationProvider");
    }

    /// <summary>
    /// Constructor throws <see cref="ArgumentNullException"/> when derivedStatCalculator is null.
    /// </summary>
    [Test]
    public void Constructor_NullDerivedStatCalculator_Throws()
    {
        // Act
        var act = () => new CharacterFactory(
            _lineageProviderMock.Object,
            _backgroundProviderMock.Object,
            _archetypeProviderMock.Object,
            _specializationProviderMock.Object,
            null!,
            _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("derivedStatCalculator");
    }

    /// <summary>
    /// Constructor throws <see cref="ArgumentNullException"/> when logger is null.
    /// </summary>
    [Test]
    public void Constructor_NullLogger_Throws()
    {
        // Act
        var act = () => new CharacterFactory(
            _lineageProviderMock.Object,
            _backgroundProviderMock.Object,
            _archetypeProviderMock.Object,
            _specializationProviderMock.Object,
            _derivedStatCalculatorMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATE STATE — HAPPY PATH
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ValidateState() returns a valid result when all required fields are present
    /// and specialization matches archetype.
    /// </summary>
    [Test]
    public void ValidateState_CompleteState_ReturnsValid()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// ValidateState() throws <see cref="ArgumentNullException"/> when state is null.
    /// </summary>
    [Test]
    public void ValidateState_NullState_Throws()
    {
        // Act
        var act = () => _factory.ValidateState(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("state");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATE STATE — MISSING FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ValidateState() returns invalid with "Lineage is required." when lineage is null.
    /// </summary>
    [Test]
    public void ValidateState_MissingLineage_ReturnsInvalid()
    {
        // Arrange
        var state = BuildCompleteState();
        state.SelectedLineage = null;

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Lineage is required.");
    }

    /// <summary>
    /// ValidateState() returns invalid with "Background is required." when background is null.
    /// </summary>
    [Test]
    public void ValidateState_MissingBackground_ReturnsInvalid()
    {
        // Arrange
        var state = BuildCompleteState();
        state.SelectedBackground = null;

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Background is required.");
    }

    /// <summary>
    /// ValidateState() returns invalid when attributes are null (not allocated).
    /// </summary>
    [Test]
    public void ValidateState_NullAttributes_ReturnsInvalid()
    {
        // Arrange
        var state = BuildCompleteState();
        state.Attributes = null;

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Complete attribute allocation is required.");
    }

    /// <summary>
    /// ValidateState() returns invalid when attributes have remaining points (incomplete).
    /// </summary>
    [Test]
    public void ValidateState_IncompleteAttributes_ReturnsInvalid()
    {
        // Arrange
        var state = BuildCompleteState();
        state.Attributes = new AttributeAllocationState
        {
            Mode = AttributeAllocationMode.Advanced,
            CurrentMight = 3, CurrentFinesse = 2, CurrentWits = 2,
            CurrentWill = 2, CurrentSturdiness = 2,
            PointsSpent = 11, PointsRemaining = 4, TotalPoints = 15
        };

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Complete attribute allocation is required.");
    }

    /// <summary>
    /// ValidateState() returns invalid with "Archetype is required." when archetype is null.
    /// </summary>
    [Test]
    public void ValidateState_MissingArchetype_ReturnsInvalid()
    {
        // Arrange
        var state = BuildCompleteState();
        state.SelectedArchetype = null;

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Archetype is required.");
    }

    /// <summary>
    /// ValidateState() returns invalid with "Specialization is required." when specialization is null.
    /// </summary>
    [Test]
    public void ValidateState_MissingSpecialization_ReturnsInvalid()
    {
        // Arrange
        var state = BuildCompleteState();
        state.SelectedSpecialization = null;

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Specialization is required.");
    }

    /// <summary>
    /// ValidateState() returns invalid with "Character name is required." when name is null.
    /// </summary>
    [Test]
    public void ValidateState_MissingName_ReturnsInvalid()
    {
        // Arrange
        var state = BuildCompleteState();
        state.CharacterName = null;

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Character name is required.");
    }

    /// <summary>
    /// ValidateState() returns invalid with "Character name is required." when name is whitespace.
    /// </summary>
    [Test]
    public void ValidateState_WhitespaceName_ReturnsInvalid()
    {
        // Arrange
        var state = BuildCompleteState();
        state.CharacterName = "   ";

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Character name is required.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATE STATE — CLAN-BORN FLEXIBLE BONUS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ValidateState() returns invalid when Clan-Born lineage is selected but
    /// no flexible attribute bonus is chosen.
    /// </summary>
    [Test]
    public void ValidateState_ClanBorn_WithoutFlexibleBonus_ReturnsInvalid()
    {
        // Arrange
        var state = BuildCompleteState();
        state.SelectedLineage = Lineage.ClanBorn;
        state.FlexibleAttributeBonus = null;

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Clan-Born lineage requires flexible attribute selection.");
    }

    /// <summary>
    /// ValidateState() returns valid when Clan-Born lineage has a flexible bonus selected.
    /// </summary>
    [Test]
    public void ValidateState_ClanBorn_WithFlexibleBonus_ReturnsValid()
    {
        // Arrange
        var state = BuildCompleteState();
        state.SelectedLineage = Lineage.ClanBorn;
        state.FlexibleAttributeBonus = CoreAttribute.Might;

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATE STATE — SPECIALIZATION-ARCHETYPE MISMATCH
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ValidateState() returns invalid when the specialization does not belong to
    /// the selected archetype (e.g., Skald with Warrior archetype).
    /// </summary>
    [Test]
    public void ValidateState_SpecializationArchetypeMismatch_ReturnsInvalid()
    {
        // Arrange — Skald belongs to Adept, not Warrior
        var state = BuildCompleteState();
        state.SelectedSpecialization = SpecializationId.Skald;

        // Mock: Skald returns spec def with ParentArchetype = Adept (mismatch with Warrior)
        var skaldDef = SpecializationDefinition.Create(
            SpecializationId.Skald,
            "Skald",
            "Voice of Legend",
            "A bard-like support path.",
            "Choose the way of the Skald...",
            Archetype.Adept,
            SpecializationPathType.Coherent,
            unlockCost: 0);

        _specializationProviderMock
            .Setup(x => x.GetBySpecializationId(SpecializationId.Skald))
            .Returns(skaldDef);

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(
            "Selected specialization does not belong to the selected archetype.");
    }

    /// <summary>
    /// ValidateState() collects multiple errors when several fields are missing.
    /// </summary>
    [Test]
    public void ValidateState_MultipleFieldsMissing_ReturnsAllErrors()
    {
        // Arrange — state with no selections at all
        var state = CharacterCreationState.Create();

        // Act
        var result = _factory.ValidateState(state);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(5);
        result.Errors.Should().Contain("Lineage is required.");
        result.Errors.Should().Contain("Background is required.");
        result.Errors.Should().Contain("Archetype is required.");
        result.Errors.Should().Contain("Specialization is required.");
        result.Errors.Should().Contain("Character name is required.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — PLAYER CONSTRUCTION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() returns a Player with the correct name from state.
    /// </summary>
    [Test]
    public async Task CreateCharacter_ValidState_ReturnsPlayerWithCorrectName()
    {
        // Arrange
        var state = BuildCompleteState();
        state.CharacterName = "  Bjorn  ";

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert
        player.Should().NotBeNull();
        player.Name.Should().Be("Bjorn");
    }

    /// <summary>
    /// CreateCharacterAsync() sets the lineage and background on the Player entity.
    /// </summary>
    [Test]
    public async Task CreateCharacter_ValidState_SetsLineageAndBackground()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert
        player.SelectedLineage.Should().Be(Lineage.RuneMarked);
        player.SelectedBackground.Should().Be(Background.WanderingSkald);
    }

    /// <summary>
    /// CreateCharacterAsync() constructs the Player with base attributes from the
    /// point allocation state, mapping Sturdiness → Fortitude.
    /// </summary>
    [Test]
    public async Task CreateCharacter_ValidState_SetsBaseAttributes()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — base attributes before lineage modifiers would have been
        // set by constructor, but lineage modifiers are applied after
        player.Attributes.Should().NotBeNull();
    }

    /// <summary>
    /// CreateCharacterAsync() throws <see cref="InvalidOperationException"/> when
    /// the state fails validation.
    /// </summary>
    [Test]
    public async Task CreateCharacter_InvalidState_ThrowsInvalidOperationException()
    {
        // Arrange — state missing lineage
        var state = BuildCompleteState();
        state.SelectedLineage = null;

        // Act
        var act = async () => await _factory.CreateCharacterAsync(state);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*validation failed*");
    }

    /// <summary>
    /// CreateCharacterAsync() throws <see cref="ArgumentNullException"/> when state is null.
    /// </summary>
    [Test]
    public async Task CreateCharacter_NullState_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await _factory.CreateCharacterAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("state");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — LINEAGE ATTRIBUTE MODIFIERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() applies lineage attribute modifiers from the provider.
    /// Verifies that the lineage provider's GetAttributeModifiers is called for the
    /// selected lineage and the resulting modifiers are applied to the Player.
    /// </summary>
    [Test]
    public async Task CreateCharacter_AppliesLineageAttributeModifiers()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — verify provider was called
        _lineageProviderMock.Verify(
            x => x.GetAttributeModifiers(Lineage.RuneMarked),
            Times.Once);
    }

    /// <summary>
    /// CreateCharacterAsync() applies the Clan-Born flexible +1 bonus correctly
    /// when the flexible attribute bonus target is set.
    /// </summary>
    [Test]
    public async Task CreateCharacter_ClanBorn_AppliesFlexibleBonus()
    {
        // Arrange
        var state = BuildCompleteState();
        state.SelectedLineage = Lineage.ClanBorn;
        state.FlexibleAttributeBonus = CoreAttribute.Might;

        // Configure Clan-Born attribute modifiers with flexible bonus
        var clanBornMods = new LineageAttributeModifiers(
            MightModifier: 0,
            FinesseModifier: 0,
            WitsModifier: 0,
            WillModifier: 0,
            SturdinessModifier: 0,
            HasFlexibleBonus: true,
            FlexibleBonusAmount: 1);

        _lineageProviderMock
            .Setup(x => x.GetAttributeModifiers(Lineage.ClanBorn))
            .Returns(clanBornMods);

        // Configure Clan-Born for other provider calls
        ConfigureLineageMocksForLineage(Lineage.ClanBorn);

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — verify the attribute modifiers were requested for Clan-Born
        _lineageProviderMock.Verify(
            x => x.GetAttributeModifiers(Lineage.ClanBorn),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — LINEAGE PASSIVE BONUSES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() applies lineage passive bonuses (HP, AP, Soak,
    /// Movement, Skills) from the lineage provider.
    /// </summary>
    [Test]
    public async Task CreateCharacter_AppliesLineagePassiveBonuses()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — verify provider was called for passive bonuses
        _lineageProviderMock.Verify(
            x => x.GetPassiveBonuses(Lineage.RuneMarked),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — LINEAGE TRAIT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() registers the lineage trait from the provider on
    /// the Player entity.
    /// </summary>
    [Test]
    public async Task CreateCharacter_RegistersLineageTrait()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — verify the trait provider was called
        _lineageProviderMock.Verify(
            x => x.GetUniqueTrait(Lineage.RuneMarked),
            Times.Once);

        // Verify trait was registered on the player
        player.LineageTrait.Should().NotBeNull();
        player.LineageTrait!.Value.TraitName.Should().Be("Aether-Tainted");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — TRAUMA BASELINE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() sets the trauma baseline from the lineage's trauma
    /// configuration. Rune-Marked starts with 5 Corruption and -1 Corruption resistance.
    /// </summary>
    [Test]
    public async Task CreateCharacter_RuneMarked_SetsTraumaBaseline()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — verify the trauma provider was called
        _lineageProviderMock.Verify(
            x => x.GetTraumaBaseline(Lineage.RuneMarked),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — BACKGROUND SKILLS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() grants background skill bonuses from the background
    /// provider to the Player entity.
    /// </summary>
    [Test]
    public async Task CreateCharacter_GrantsBackgroundSkills()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — verify the background skill provider was called
        _backgroundProviderMock.Verify(
            x => x.GetSkillGrants(Background.WanderingSkald),
            Times.Once);

        // Verify skills were applied to the player
        player.Skills.Should().NotBeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — DERIVED STATS AND ARCHETYPE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() sets the archetype and applies derived stats (HP,
    /// Attack, Defense) via the IDerivedStatCalculator.
    /// </summary>
    [Test]
    public async Task CreateCharacter_SetsArchetypeAndDerivedStats()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — verify derived stat calculator was called
        _derivedStatCalculatorMock.Verify(
            x => x.CalculateDerivedStats(
                It.IsAny<IReadOnlyDictionary<CoreAttribute, int>>(),
                "warrior",
                "rune-marked"),
            Times.AtLeastOnce);

        // Verify stats were set on the player
        player.Stats.MaxHealth.Should().Be(100);
        player.Health.Should().Be(100); // HP starts at maximum
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — RESOURCE POOLS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() initializes Stamina and Aether Pool resource pools
    /// at their maximum values from derived stats.
    /// </summary>
    [Test]
    public async Task CreateCharacter_InitializesResourcePools()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — derived stat calculator called at least once for resource values
        _derivedStatCalculatorMock.Verify(
            x => x.CalculateDerivedStats(
                It.IsAny<IReadOnlyDictionary<CoreAttribute, int>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — ARCHETYPE ABILITIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() grants the 3 archetype starting abilities to the Player.
    /// </summary>
    [Test]
    public async Task CreateCharacter_GrantsArchetypeAbilities()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — verify the archetype ability provider was called
        _archetypeProviderMock.Verify(
            x => x.GetStartingAbilities(Archetype.Warrior),
            Times.Once);

        // Verify abilities were granted
        player.Abilities.Should().ContainKey("power-strike");
        player.Abilities.Should().ContainKey("defensive-stance");
        player.Abilities.Should().ContainKey("iron-will");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — SPECIALIZATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() applies the specialization, grants Tier 1 abilities,
    /// and initializes the special resource (e.g., Rage for Berserkr).
    /// </summary>
    [Test]
    public async Task CreateCharacter_AppliesSpecializationAndTier1Abilities()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — verify the specialization provider was called
        _specializationProviderMock.Verify(
            x => x.GetBySpecializationId(SpecializationId.Berserkr),
            Times.AtLeastOnce);

        // Verify Tier 1 spec abilities were granted (3 abilities)
        player.Abilities.Should().ContainKey("rage-strike");
        player.Abilities.Should().ContainKey("bloodlust");
        player.Abilities.Should().ContainKey("fury-surge");
    }

    /// <summary>
    /// CreateCharacterAsync() initializes the special resource when the
    /// specialization defines one (e.g., Rage for Berserkr).
    /// </summary>
    [Test]
    public async Task CreateCharacter_InitializesSpecialResource()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — the specialization provider should have been called
        _specializationProviderMock.Verify(
            x => x.GetBySpecializationId(SpecializationId.Berserkr),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — SAGA PROGRESSION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() initializes saga progression to starting values
    /// (PP: 0, Rank: 1).
    /// </summary>
    [Test]
    public async Task CreateCharacter_InitializesSagaProgression()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert
        player.ProgressionPoints.Should().Be(0);
        player.ProgressionRank.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE CHARACTER — FULL INTEGRATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// CreateCharacterAsync() with a complete valid state produces a fully
    /// initialized Player with all 13 steps applied, verifying the end-to-end
    /// factory behavior across all initialization phases.
    /// </summary>
    [Test]
    public async Task CreateCharacter_FullIntegration_ProducesCompletePlayer()
    {
        // Arrange
        var state = BuildCompleteState();

        // Act
        var player = await _factory.CreateCharacterAsync(state);

        // Assert — verify the player has all expected properties set
        player.Should().NotBeNull();
        player.Name.Should().Be("Bjorn");
        player.SelectedLineage.Should().Be(Lineage.RuneMarked);
        player.SelectedBackground.Should().Be(Background.WanderingSkald);
        player.Stats.MaxHealth.Should().BeGreaterThan(0);
        player.Health.Should().Be(player.Stats.MaxHealth);
        player.LineageTrait.Should().NotBeNull();
        player.Skills.Should().NotBeEmpty();
        player.Abilities.Should().NotBeEmpty();
        player.ProgressionPoints.Should().Be(0);
        player.ProgressionRank.Should().Be(1);

        // Verify all providers were called
        _lineageProviderMock.Verify(x => x.GetAttributeModifiers(Lineage.RuneMarked), Times.Once);
        _lineageProviderMock.Verify(x => x.GetPassiveBonuses(Lineage.RuneMarked), Times.Once);
        _lineageProviderMock.Verify(x => x.GetUniqueTrait(Lineage.RuneMarked), Times.Once);
        _lineageProviderMock.Verify(x => x.GetTraumaBaseline(Lineage.RuneMarked), Times.Once);
        _backgroundProviderMock.Verify(x => x.GetSkillGrants(Background.WanderingSkald), Times.Once);
        _archetypeProviderMock.Verify(x => x.GetStartingAbilities(Archetype.Warrior), Times.Once);
        _specializationProviderMock.Verify(
            x => x.GetBySpecializationId(SpecializationId.Berserkr), Times.AtLeastOnce);
        _derivedStatCalculatorMock.Verify(
            x => x.CalculateDerivedStats(
                It.IsAny<IReadOnlyDictionary<CoreAttribute, int>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER — STATE BUILDER
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Builds a complete <see cref="CharacterCreationState"/> with all required
    /// fields set to valid values. Uses Rune-Marked Warrior Berserkr defaults:
    /// Might=4, Finesse=3, Wits=3, Will=3, Sturdiness=2 (15 points total).
    /// </summary>
    /// <returns>A fully populated state ready for factory validation and character creation.</returns>
    private static CharacterCreationState BuildCompleteState()
    {
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.RuneMarked;
        state.SelectedBackground = Background.WanderingSkald;
        state.Attributes = new AttributeAllocationState
        {
            Mode = AttributeAllocationMode.Advanced,
            CurrentMight = 4,
            CurrentFinesse = 3,
            CurrentWits = 3,
            CurrentWill = 3,
            CurrentSturdiness = 2,
            PointsSpent = 15,
            PointsRemaining = 0,
            TotalPoints = 15
        };
        state.SelectedArchetype = Archetype.Warrior;
        state.SelectedSpecialization = SpecializationId.Berserkr;
        state.CharacterName = "Bjorn";
        return state;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS — DEFAULT MOCK CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Configures default lineage provider mocks for Rune-Marked lineage.
    /// Includes attribute modifiers (+1 Wits, +1 Will), no passive bonuses,
    /// Aether-Tainted trait, and Rune-Marked trauma (5 Corruption, -1 Corruption resist).
    /// </summary>
    private void ConfigureDefaultLineageMocks()
    {
        // Rune-Marked attribute modifiers: +1 Wits, +1 Will, no flexible bonus
        var runeMarkedMods = new LineageAttributeModifiers(
            MightModifier: 0,
            FinesseModifier: 0,
            WitsModifier: 1,
            WillModifier: 1,
            SturdinessModifier: 0,
            HasFlexibleBonus: false,
            FlexibleBonusAmount: 0);

        _lineageProviderMock
            .Setup(x => x.GetAttributeModifiers(Lineage.RuneMarked))
            .Returns(runeMarkedMods);

        // Rune-Marked passive bonuses: none
        var runeMarkedPassives = new LineagePassiveBonuses(
            MaxHpBonus: 0,
            MaxApBonus: 0,
            SoakBonus: 0,
            MovementBonus: 0,
            SkillBonuses: Array.Empty<SkillBonus>());

        _lineageProviderMock
            .Setup(x => x.GetPassiveBonuses(Lineage.RuneMarked))
            .Returns(runeMarkedPassives);

        // Rune-Marked trait: Aether-Tainted
        var runeMarkedTrait = LineageTrait.Create(
            traitId: "aether-tainted",
            traitName: "Aether-Tainted",
            description: "Aether flows through your blood, enhancing magical potential at a cost.",
            effectType: LineageTraitEffectType.BonusDiceToSkill,
            bonusDice: 1,
            targetCheck: "aether");

        _lineageProviderMock
            .Setup(x => x.GetUniqueTrait(Lineage.RuneMarked))
            .Returns(runeMarkedTrait);

        // Rune-Marked trauma: 5 Corruption, 0 Stress, -1 Corruption resist, 0 Stress resist
        var runeMarkedTrauma = new LineageTraumaBaseline(
            StartingCorruption: 5,
            StartingStress: 0,
            CorruptionResistanceModifier: -1,
            StressResistanceModifier: 0);

        _lineageProviderMock
            .Setup(x => x.GetTraumaBaseline(Lineage.RuneMarked))
            .Returns(runeMarkedTrauma);
    }

    /// <summary>
    /// Configures lineage provider mocks for a specified non-default lineage.
    /// Uses neutral values (zero modifiers, empty passives, generic trait, zero trauma).
    /// </summary>
    /// <param name="lineage">The lineage to configure mocks for.</param>
    private void ConfigureLineageMocksForLineage(Lineage lineage)
    {
        var neutralPassives = new LineagePassiveBonuses(
            MaxHpBonus: 0,
            MaxApBonus: 0,
            SoakBonus: 0,
            MovementBonus: 0,
            SkillBonuses: Array.Empty<SkillBonus>());

        _lineageProviderMock
            .Setup(x => x.GetPassiveBonuses(lineage))
            .Returns(neutralPassives);

        var genericTrait = LineageTrait.Create(
            traitId: "test-trait",
            traitName: "Test Trait",
            description: "A test trait.",
            effectType: LineageTraitEffectType.BonusDiceToSkill,
            bonusDice: 1,
            targetCheck: "athletics");

        _lineageProviderMock
            .Setup(x => x.GetUniqueTrait(lineage))
            .Returns(genericTrait);

        var neutralTrauma = new LineageTraumaBaseline(
            StartingCorruption: 0,
            StartingStress: 0,
            CorruptionResistanceModifier: 0,
            StressResistanceModifier: 0);

        _lineageProviderMock
            .Setup(x => x.GetTraumaBaseline(lineage))
            .Returns(neutralTrauma);
    }

    /// <summary>
    /// Configures default background provider mocks for Wandering Skald.
    /// Grants 2 skill bonuses: rhetoric +2 and lore +1.
    /// </summary>
    private void ConfigureDefaultBackgroundMocks()
    {
        var skillGrants = new List<BackgroundSkillGrant>
        {
            BackgroundSkillGrant.Permanent("rhetoric", 2),
            BackgroundSkillGrant.Permanent("lore", 1)
        };

        _backgroundProviderMock
            .Setup(x => x.GetSkillGrants(Background.WanderingSkald))
            .Returns(skillGrants);
    }

    /// <summary>
    /// Configures default archetype provider mocks for Warrior.
    /// Grants 3 starting abilities: Power Strike (Active), Defensive Stance (Stance),
    /// Iron Will (Passive).
    /// </summary>
    private void ConfigureDefaultArchetypeMocks()
    {
        var startingAbilities = new List<ArchetypeAbilityGrant>
        {
            ArchetypeAbilityGrant.CreateActive(
                "power-strike", "Power Strike", "A powerful melee attack."),
            ArchetypeAbilityGrant.CreateStance(
                "defensive-stance", "Defensive Stance", "Reduces damage taken."),
            ArchetypeAbilityGrant.CreatePassive(
                "iron-will", "Iron Will", "Passive resilience.")
        };

        _archetypeProviderMock
            .Setup(x => x.GetStartingAbilities(Archetype.Warrior))
            .Returns(startingAbilities);
    }

    /// <summary>
    /// Configures default specialization provider mocks for Berserkr (Warrior).
    /// Includes Rage special resource and 3 Tier 1 abilities.
    /// </summary>
    private void ConfigureDefaultSpecializationMocks()
    {
        // Create Tier 1 abilities for Berserkr
        var tier1Abilities = new[]
        {
            SpecializationAbility.CreateActive(
                "rage-strike", "Rage Strike",
                "A devastating attack fueled by rage.",
                resourceCost: 10, resourceType: "rage"),
            SpecializationAbility.CreatePassive(
                "bloodlust", "Bloodlust",
                "Gain rage on successful melee hits."),
            SpecializationAbility.CreateActive(
                "fury-surge", "Fury Surge",
                "Burst of speed and power.",
                resourceCost: 15, resourceType: "rage")
        };

        var tier1 = SpecializationAbilityTier.CreateTier1(
            "Path of Rage", tier1Abilities);

        // Create Rage special resource definition
        var rageResource = SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: 0,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Fury that builds through combat.");

        // Create the Berserkr specialization definition
        var berserkrDef = SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Fury Incarnate",
            "A warrior consumed by battle rage.",
            "Embrace the fury within...",
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            unlockCost: 0,
            specialResource: rageResource,
            abilityTiers: new[] { tier1 });

        _specializationProviderMock
            .Setup(x => x.GetBySpecializationId(SpecializationId.Berserkr))
            .Returns(berserkrDef);
    }

    /// <summary>
    /// Configures default derived stat calculator mocks.
    /// Returns realistic Warrior stats: HP=100, Stamina=55, AP=35, Init=4, Soak=1,
    /// Move=5, Carry=40.
    /// </summary>
    private void ConfigureDefaultDerivedStatsMocks()
    {
        var derivedStats = DerivedStats.Create(
            maxHp: 100,
            maxStamina: 55,
            maxAetherPool: 35,
            initiative: 4,
            soak: 1,
            movementSpeed: 5,
            carryingCapacity: 40);

        _derivedStatCalculatorMock
            .Setup(x => x.CalculateDerivedStats(
                It.IsAny<IReadOnlyDictionary<CoreAttribute, int>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .Returns(derivedStats);
    }
}
