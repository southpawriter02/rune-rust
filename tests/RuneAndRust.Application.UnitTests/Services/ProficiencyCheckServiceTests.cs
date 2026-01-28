using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="ProficiencyCheckService"/> class.
/// </summary>
[TestFixture]
public class ProficiencyCheckServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Test Dependencies
    // ═══════════════════════════════════════════════════════════════════════════

    private Mock<IProficiencyEffectProvider> _mockEffectProvider = null!;
    private Mock<IProficiencyAcquisitionService> _mockAcquisitionService = null!;
    private Mock<ILogger<ProficiencyCheckService>> _mockLogger = null!;
    private ProficiencyCheckService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockEffectProvider = new Mock<IProficiencyEffectProvider>();
        _mockAcquisitionService = new Mock<IProficiencyAcquisitionService>();
        _mockLogger = new Mock<ILogger<ProficiencyCheckService>>();

        _service = new ProficiencyCheckService(
            _mockEffectProvider.Object,
            _mockAcquisitionService.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies constructor throws when effectProvider is null.
    /// </summary>
    [Test]
    public void Constructor_WithNullEffectProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ProficiencyCheckService(
            null!,
            _mockAcquisitionService.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("effectProvider");
    }

    /// <summary>
    /// Verifies constructor throws when acquisitionService is null.
    /// </summary>
    [Test]
    public void Constructor_WithNullAcquisitionService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ProficiencyCheckService(
            _mockEffectProvider.Object,
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("acquisitionService");
    }

    /// <summary>
    /// Verifies constructor throws when logger is null.
    /// </summary>
    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ProficiencyCheckService(
            _mockEffectProvider.Object,
            _mockAcquisitionService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetCombatModifiers Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies GetCombatModifiers returns correct modifiers for NonProficient.
    /// </summary>
    [Test]
    public void GetCombatModifiers_ForNonProficient_ReturnsNonProficientModifiers()
    {
        // Arrange - Category not in archetype = NonProficient
        var proficiencies = CreateProficienciesForCategory(
            WeaponCategory.Swords, isProficient: false);
        var effect = ProficiencyEffect.CreateNonProficient();

        _mockEffectProvider
            .Setup(p => p.GetEffect(WeaponProficiencyLevel.NonProficient))
            .Returns(effect);

        // Act
        var result = _service.GetCombatModifiers(proficiencies, WeaponCategory.Swords);

        // Assert
        result.ProficiencyLevel.Should().Be(WeaponProficiencyLevel.NonProficient);
        result.AttackModifier.Should().Be(-3);
        result.DamageModifier.Should().Be(-2);
        result.CanUseSpecialProperties.Should().BeFalse();
        result.UnlockedTechniqueLevel.Should().Be(TechniqueAccess.None);
    }

    /// <summary>
    /// Verifies GetCombatModifiers returns correct modifiers for Proficient.
    /// </summary>
    [Test]
    public void GetCombatModifiers_ForProficient_ReturnsProficientModifiers()
    {
        // Arrange - Category in archetype = Proficient
        var proficiencies = CreateProficienciesForCategory(
            WeaponCategory.Swords, isProficient: true);
        var effect = ProficiencyEffect.CreateProficient();

        _mockEffectProvider
            .Setup(p => p.GetEffect(WeaponProficiencyLevel.Proficient))
            .Returns(effect);

        // Act
        var result = _service.GetCombatModifiers(proficiencies, WeaponCategory.Swords);

        // Assert
        result.ProficiencyLevel.Should().Be(WeaponProficiencyLevel.Proficient);
        result.AttackModifier.Should().Be(0);
        result.DamageModifier.Should().Be(0);
        result.CanUseSpecialProperties.Should().BeTrue();
        result.UnlockedTechniqueLevel.Should().Be(TechniqueAccess.Basic);
    }

    /// <summary>
    /// Verifies GetCombatModifiers throws when proficiencies is null.
    /// </summary>
    [Test]
    public void GetCombatModifiers_WithNullProficiencies_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => _service.GetCombatModifiers(null!, WeaponCategory.Swords);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("proficiencies");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CanUseSpecialProperties Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies CanUseSpecialProperties returns false for NonProficient.
    /// </summary>
    [Test]
    public void CanUseSpecialProperties_ForNonProficient_ReturnsFalse()
    {
        // Arrange
        var proficiencies = CreateProficienciesForCategory(
            WeaponCategory.Axes, isProficient: false);

        _mockEffectProvider
            .Setup(p => p.CanUseSpecialProperties(WeaponProficiencyLevel.NonProficient))
            .Returns(false);

        // Act
        var result = _service.CanUseSpecialProperties(proficiencies, WeaponCategory.Axes);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies CanUseSpecialProperties returns true for Proficient.
    /// </summary>
    [Test]
    public void CanUseSpecialProperties_ForProficient_ReturnsTrue()
    {
        // Arrange
        var proficiencies = CreateProficienciesForCategory(
            WeaponCategory.Axes, isProficient: true);

        _mockEffectProvider
            .Setup(p => p.CanUseSpecialProperties(WeaponProficiencyLevel.Proficient))
            .Returns(true);

        // Act
        var result = _service.CanUseSpecialProperties(proficiencies, WeaponCategory.Axes);

        // Assert
        result.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CanUseTechnique Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies CanUseTechnique returns false when NonProficient attempts Basic technique.
    /// </summary>
    [Test]
    public void CanUseTechnique_WhenNonProficientAttemptsBasic_ReturnsFalse()
    {
        // Arrange
        var proficiencies = CreateProficienciesForCategory(
            WeaponCategory.Swords, isProficient: false);

        _mockEffectProvider
            .Setup(p => p.GetTechniqueAccess(WeaponProficiencyLevel.NonProficient))
            .Returns(TechniqueAccess.None);

        // Act
        var result = _service.CanUseTechnique(
            proficiencies,
            WeaponCategory.Swords,
            TechniqueAccess.Basic);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies CanUseTechnique returns true when Proficient attempts Basic technique.
    /// </summary>
    [Test]
    public void CanUseTechnique_WhenProficientAttemptsBasic_ReturnsTrue()
    {
        // Arrange
        var proficiencies = CreateProficienciesForCategory(
            WeaponCategory.Swords, isProficient: true);

        _mockEffectProvider
            .Setup(p => p.GetTechniqueAccess(WeaponProficiencyLevel.Proficient))
            .Returns(TechniqueAccess.Basic);

        // Act
        var result = _service.CanUseTechnique(
            proficiencies,
            WeaponCategory.Swords,
            TechniqueAccess.Basic);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies CanUseTechnique returns false when Proficient attempts Advanced technique.
    /// </summary>
    [Test]
    public void CanUseTechnique_WhenProficientAttemptsAdvanced_ReturnsFalse()
    {
        // Arrange
        var proficiencies = CreateProficienciesForCategory(
            WeaponCategory.Swords, isProficient: true);

        _mockEffectProvider
            .Setup(p => p.GetTechniqueAccess(WeaponProficiencyLevel.Proficient))
            .Returns(TechniqueAccess.Basic);

        // Act
        var result = _service.CanUseTechnique(
            proficiencies,
            WeaponCategory.Swords,
            TechniqueAccess.Advanced);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RecordCombatUsageAsync Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies RecordCombatUsageAsync delegates to acquisition service.
    /// </summary>
    [Test]
    public async Task RecordCombatUsageAsync_DelegatesToAcquisitionService()
    {
        // Arrange
        var proficiencies = CreateProficienciesForCategory(
            WeaponCategory.Swords, isProficient: true);

        var expectedResult = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            WeaponProficiencyLevel.Proficient,
            AcquisitionMethod.CombatExperience,
            AcquisitionCost.None);

        _mockAcquisitionService
            .Setup(s => s.RecordCombatExperienceAsync(
                proficiencies,
                WeaponCategory.Swords,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.RecordCombatUsageAsync(
            proficiencies, WeaponCategory.Swords);

        // Assert
        result.Should().Be(expectedResult);
        _mockAcquisitionService.Verify(
            s => s.RecordCombatExperienceAsync(
                proficiencies,
                WeaponCategory.Swords,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetProficiencyDescription Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies GetProficiencyDescription returns formatted description.
    /// </summary>
    [Test]
    public void GetProficiencyDescription_ForMaster_ContainsExpectedInfo()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateMaster();
        _mockEffectProvider
            .Setup(p => p.GetEffect(WeaponProficiencyLevel.Master))
            .Returns(effect);

        // Act
        var result = _service.GetProficiencyDescription(WeaponProficiencyLevel.Master);

        // Assert
        result.Should().Contain("Master");
        result.Should().Contain("+2");
        result.Should().Contain("+1");
        result.Should().Contain("signature techniques");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates CharacterProficiencies where the given category is either proficient or not.
    /// </summary>
    /// <remarks>
    /// CharacterProficiencies entities are initialized from archetypes, so categories
    /// start at either Proficient (in archetype) or NonProficient (not in archetype).
    /// To test higher levels (Expert, Master), you would need to advance the proficiency
    /// through the acquisition service.
    /// </remarks>
    private static CharacterProficiencies CreateProficienciesForCategory(
        WeaponCategory category,
        bool isProficient)
    {
        // If proficient, include the category in the archetype set
        // If not proficient, use a different category for the archetype
        var archetypeCategories = isProficient
            ? new List<WeaponCategory> { category }
            : new List<WeaponCategory> { GetDifferentCategory(category) };

        var archetypeSet = ArchetypeProficiencySet.Create(
            "test-archetype",
            "Test Archetype",
            "Test archetype for unit testing",
            archetypeCategories);

        return CharacterProficiencies.CreateFromArchetype(
            Guid.NewGuid(),
            archetypeSet,
            ProficiencyThresholds.Default);
    }

    /// <summary>
    /// Gets a different weapon category to use when we need a non-proficient setup.
    /// </summary>
    private static WeaponCategory GetDifferentCategory(WeaponCategory category)
    {
        // Just return a different category - Staves if not already Staves, else Swords
        return category == WeaponCategory.Staves
            ? WeaponCategory.Swords
            : WeaponCategory.Staves;
    }
}
