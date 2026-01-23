namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="VoiceGuidanceService"/>.
/// </summary>
[TestFixture]
public class VoiceGuidanceServiceTests
{
    private Mock<ILogger<VoiceGuidanceService>> _loggerMock = null!;
    private VoiceGuidanceService _service = null!;
    private string _configPath = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<VoiceGuidanceService>>();
        _configPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "config", "skill-descriptors.json");

        // Resolve to absolute path
        _configPath = Path.GetFullPath(_configPath);

        _service = new VoiceGuidanceService(_configPath, _loggerMock.Object);
    }

    [Test]
    public void GetDescriptor_ConfiguredSkill_ReturnsDescriptor()
    {
        // Act
        var descriptor = _service.GetDescriptor("lockpicking", DescriptorCategory.Competent);

        // Assert
        descriptor.HasContent.Should().BeTrue();
        descriptor.SkillId.Should().Be("lockpicking");
        descriptor.Category.Should().Be(DescriptorCategory.Competent);
    }

    [Test]
    public void GetDescriptor_UnconfiguredSkill_ReturnsFallback()
    {
        // Act
        var descriptor = _service.GetDescriptor("unknown-skill", DescriptorCategory.Competent);

        // Assert
        descriptor.HasContent.Should().BeTrue();
        descriptor.SkillId.Should().Be("generic");
    }

    [Test]
    public void GetDescriptor_FromOutcomeDetails_UsesCorrectCategory()
    {
        // Arrange
        var outcome = new OutcomeDetails(
            outcomeType: SkillOutcome.FullSuccess,
            margin: 1,
            isFumble: false,
            isCritical: false);
        // FullSuccess maps to Competent

        // Act
        var descriptor = _service.GetDescriptor("lockpicking", outcome);

        // Assert
        descriptor.Category.Should().Be(DescriptorCategory.Competent);
    }

    [Test]
    public void HasDescriptors_ConfiguredSkill_ReturnsTrue()
    {
        // Assert
        _service.HasDescriptors("lockpicking").Should().BeTrue();
        _service.HasDescriptors("persuasion").Should().BeTrue();
    }

    [Test]
    public void HasDescriptors_UnconfiguredSkill_ReturnsFalse()
    {
        // Assert
        _service.HasDescriptors("unknown-skill").Should().BeFalse();
    }

    [Test]
    public void GetAvailableCategories_ReturnsConfiguredCategories()
    {
        // Act
        var categories = _service.GetAvailableCategories("lockpicking");

        // Assert
        categories.Should().Contain(DescriptorCategory.Catastrophic);
        categories.Should().Contain(DescriptorCategory.Failed);
        categories.Should().Contain(DescriptorCategory.Competent);
        categories.Should().Contain(DescriptorCategory.Masterful);
    }

    [Test]
    public void GetPoolSize_ReturnsDescriptorCount()
    {
        // Act
        var poolSize = _service.GetPoolSize("lockpicking", DescriptorCategory.Competent);

        // Assert
        poolSize.Should().BeGreaterOrEqualTo(2);
    }

    [Test]
    public void GetDescriptor_WithCorruptionContext_MayReturnContextualDescriptor()
    {
        // Arrange
        var envMod = EnvironmentModifier.FromCorruption(CorruptionTier.Glitched);
        var context = new SkillContext(
            equipmentModifiers: Array.Empty<EquipmentModifier>(),
            situationalModifiers: Array.Empty<SituationalModifier>(),
            environmentModifiers: new[] { envMod },
            targetModifiers: Array.Empty<TargetModifier>(),
            appliedStatuses: Array.Empty<string>());

        // Act
        var descriptor = _service.GetDescriptor("lockpicking", DescriptorCategory.Catastrophic, context);

        // Assert
        descriptor.HasContent.Should().BeTrue();
        // May be contextual (glitched) or default depending on random selection
    }
}
