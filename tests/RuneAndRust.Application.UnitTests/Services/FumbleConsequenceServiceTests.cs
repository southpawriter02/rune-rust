using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="FumbleConsequenceService"/> service.
/// </summary>
[TestFixture]
public class FumbleConsequenceServiceTests
{
    private IFumbleConsequenceRepository _repository = null!;
    private IFumbleConsequenceConfigurationProvider _configProvider = null!;
    private ILogger<FumbleConsequenceService> _logger = null!;
    private FumbleConsequenceService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IFumbleConsequenceRepository>();
        _configProvider = Substitute.For<IFumbleConsequenceConfigurationProvider>();
        _logger = Substitute.For<ILogger<FumbleConsequenceService>>();

        // Default configuration
        _configProvider
            .GetConfiguration(Arg.Any<FumbleType>())
            .Returns(new FumbleConsequenceConfiguration(
                Description: "Test description",
                Duration: null,
                RecoveryCondition: "test-condition"));

        _service = new FumbleConsequenceService(_repository, _configProvider, _logger);
    }

    [Test]
    public void CreateConsequence_CreatesAndAddsConsequence()
    {
        // Arrange
        const string characterId = "char-001";
        const string skillId = "persuasion";

        // Act
        var consequence = _service.CreateConsequence(characterId, skillId, null, null);

        // Assert
        consequence.Should().NotBeNull();
        consequence.CharacterId.Should().Be(characterId);
        consequence.SkillId.Should().Be(skillId);
        consequence.ConsequenceType.Should().Be(FumbleType.TrustShattered);
        consequence.IsActive.Should().BeTrue();

        _repository.Received(1).Add(Arg.Any<FumbleConsequence>());
    }

    [Test]
    public void CreateConsequence_DeterminesCorrectFumbleTypeFromSkillId()
    {
        // Arrange & Act
        var persuasion = _service.CreateConsequence("char", "persuasion", null, null);
        var deception = _service.CreateConsequence("char", "deception", null, null);
        var lockpicking = _service.CreateConsequence("char", "lockpicking", null, null);
        var stealth = _service.CreateConsequence("char", "stealth", null, null);

        // Assert
        persuasion.ConsequenceType.Should().Be(FumbleType.TrustShattered);
        deception.ConsequenceType.Should().Be(FumbleType.LieExposed);
        lockpicking.ConsequenceType.Should().Be(FumbleType.MechanismJammed);
        stealth.ConsequenceType.Should().Be(FumbleType.SystemWideAlert);
    }

    [Test]
    public void IsCheckBlocked_WhenTrustShatteredConsequenceExists_ReturnsTrue()
    {
        // Arrange
        const string characterId = "char-001";
        const string skillId = "persuasion";
        const string targetId = "npc-001";

        var consequence = new FumbleConsequence(
            "fc-001", characterId, skillId, FumbleType.TrustShattered,
            targetId, DateTime.UtcNow, null, "Trust shattered", null);

        _repository.GetActiveByCharacter(characterId).Returns(new List<FumbleConsequence> { consequence });

        // Act
        var isBlocked = _service.IsCheckBlocked(characterId, skillId, targetId);

        // Assert
        isBlocked.Should().BeTrue();
    }

    [Test]
    public void IsCheckBlocked_WhenNoBlockingConsequence_ReturnsFalse()
    {
        // Arrange
        _repository.GetActiveByCharacter("char-001").Returns(new List<FumbleConsequence>());

        // Act
        var isBlocked = _service.IsCheckBlocked("char-001", "persuasion", null);

        // Assert
        isBlocked.Should().BeFalse();
    }

    [Test]
    public void TryRecover_WhenConditionMet_DeactivatesAndReturnsTrue()
    {
        // Arrange
        var consequence = new FumbleConsequence(
            "fc-001", "char-001", "persuasion", FumbleType.TrustShattered,
            null, DateTime.UtcNow, null, "Test", "complete-quest");

        _repository.GetById("fc-001").Returns(consequence);

        // Act
        var recovered = _service.TryRecover("fc-001", new[] { "complete-quest" });

        // Assert
        recovered.Should().BeTrue();
        consequence.IsActive.Should().BeFalse();
        _repository.Received(1).Update(consequence);
    }

    [Test]
    public void TryRecover_WhenConditionNotMet_ReturnsFalse()
    {
        // Arrange
        var consequence = new FumbleConsequence(
            "fc-001", "char-001", "persuasion", FumbleType.TrustShattered,
            null, DateTime.UtcNow, null, "Test", "complete-quest");

        _repository.GetById("fc-001").Returns(consequence);

        // Act
        var recovered = _service.TryRecover("fc-001", new[] { "wrong-condition" });

        // Assert
        recovered.Should().BeFalse();
        consequence.IsActive.Should().BeTrue();
    }

    [Test]
    public void ProcessExpirations_DeactivatesExpiredConsequences()
    {
        // Arrange
        var expiredConsequence = new FumbleConsequence(
            "fc-001", "char-001", "persuasion", FumbleType.TrustShattered,
            null, DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1), "Test", null);

        var activeConsequence = new FumbleConsequence(
            "fc-002", "char-001", "deception", FumbleType.LieExposed,
            null, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), "Test", null);

        _repository.GetAllActive().Returns(new List<FumbleConsequence> { expiredConsequence, activeConsequence });

        // Act
        var expired = _service.ProcessExpirations(DateTime.UtcNow);

        // Assert
        expired.Should().HaveCount(1);
        expired[0].ConsequenceId.Should().Be("fc-001");
        expiredConsequence.IsActive.Should().BeFalse();
        activeConsequence.IsActive.Should().BeTrue();
    }

    [Test]
    public void GetTotalDicePenalty_SumsPenaltiesFromAffectingConsequences()
    {
        // Arrange
        var consequence = new FumbleConsequence(
            "fc-001", "char-001", "deception", FumbleType.LieExposed,
            null, DateTime.UtcNow, null, "Test", null);

        _repository.GetActiveByCharacter("char-001").Returns(new List<FumbleConsequence> { consequence });

        // Act
        var penalty = _service.GetTotalDicePenalty("char-001", "deception", null);

        // Assert
        penalty.Should().Be(-2); // LieExposed gives -2
    }

    [Test]
    public void GetTotalDcModifier_SumsModifiersFromAffectingConsequences()
    {
        // Arrange
        var consequence = new FumbleConsequence(
            "fc-001", "char-001", "lockpicking", FumbleType.MechanismJammed,
            null, DateTime.UtcNow, null, "Test", null);

        _repository.GetActiveByCharacter("char-001").Returns(new List<FumbleConsequence> { consequence });

        // Act
        var modifier = _service.GetTotalDcModifier("char-001", "lockpicking", null);

        // Assert
        modifier.Should().Be(2); // MechanismJammed gives +2
    }

    [Test]
    public void DeactivateConsequence_ManuallyDeactivates()
    {
        // Arrange
        var consequence = new FumbleConsequence(
            "fc-001", "char-001", "persuasion", FumbleType.TrustShattered,
            null, DateTime.UtcNow, null, "Test", null);

        _repository.GetById("fc-001").Returns(consequence);

        // Act
        _service.DeactivateConsequence("fc-001", "GM override");

        // Assert
        consequence.IsActive.Should().BeFalse();
        consequence.DeactivationReason.Should().Be("GM override");
        _repository.Received(1).Update(consequence);
    }
}
