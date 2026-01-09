using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for TraitService (v0.0.9c).
/// </summary>
[TestFixture]
public class TraitServiceTests
{
    private Mock<IGameConfigurationProvider> _configProviderMock = null!;
    private Mock<ILogger<TraitService>> _loggerMock = null!;
    private TraitService _sut = null!;
    private List<MonsterTrait> _traits = null!;

    [SetUp]
    public void SetUp()
    {
        _configProviderMock = new Mock<IGameConfigurationProvider>();
        _loggerMock = new Mock<ILogger<TraitService>>();

        _traits = new List<MonsterTrait>
        {
            MonsterTrait.Create("regenerating", "Regenerating", "Heals each turn.", TraitEffect.Regeneration, 5),
            MonsterTrait.Create("flying", "Flying", "Harder to hit.", TraitEffect.Flying, 3),
            MonsterTrait.Create("venomous", "Venomous", "Poisons attacks.", TraitEffect.Venomous, 3),
            MonsterTrait.Create("armored", "Armored", "Extra defense.", TraitEffect.Armored, 3),
            MonsterTrait.Create("berserker", "Berserker", "Damage boost.", TraitEffect.Berserker, 50, 30)
        };

        _configProviderMock.Setup(x => x.GetTraits()).Returns(_traits);
        _configProviderMock.Setup(x => x.GetTraitById(It.IsAny<string>()))
            .Returns((string id) => _traits.FirstOrDefault(t => t.Id == id));

        _sut = new TraitService(_configProviderMock.Object, _loggerMock.Object);
    }

    [Test]
    public void GetTrait_WithValidId_ReturnsTrait()
    {
        var trait = _sut.GetTrait("armored");

        trait.Should().NotBeNull();
        trait!.Id.Should().Be("armored");
        trait.Effect.Should().Be(TraitEffect.Armored);
    }

    [Test]
    public void GetTrait_WithInvalidId_ReturnsNull()
    {
        var trait = _sut.GetTrait("invalid");

        trait.Should().BeNull();
    }

    [Test]
    public void GetAllTraits_ReturnsTraitsFromProvider()
    {
        var traits = _sut.GetAllTraits();

        traits.Should().HaveCount(5);
    }

    [Test]
    public void GetTraits_WithValidIds_ReturnsTraits()
    {
        var traits = _sut.GetTraits(["armored", "flying"]);

        traits.Should().HaveCount(2);
        traits.Select(t => t.Id).Should().BeEquivalentTo(["armored", "flying"]);
    }

    [Test]
    public void GetTraits_WithInvalidIds_SkipsInvalid()
    {
        var traits = _sut.GetTraits(["armored", "invalid", "flying"]);

        traits.Should().HaveCount(2);
    }

    [Test]
    public void SelectRandomTraits_ForCommonTier_ReturnsNoTraits()
    {
        var commonTier = TierDefinition.Create("common", "Common", null, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);

        var traits = _sut.SelectRandomTraits(["armored", "flying"], commonTier);

        traits.Should().BeEmpty();
    }

    [Test]
    public void SelectRandomTraits_ForNamedTier_ReturnsOneTrait()
    {
        var namedTier = TierDefinition.Create("named", "Named", null, 1.5f, 1.3f, 1.2f, 2.0f, 2.0f, "yellow", 20, true);

        var traits = _sut.SelectRandomTraits(["armored", "flying", "venomous"], namedTier);

        traits.Should().HaveCount(1);
    }

    [Test]
    public void SelectRandomTraits_ForEliteTier_ReturnsTwoTraits()
    {
        var eliteTier = TierDefinition.Create("elite", "Elite", "Elite", 2.0f, 1.5f, 1.5f, 3.0f, 3.0f);

        var traits = _sut.SelectRandomTraits(["armored", "flying", "venomous"], eliteTier);

        traits.Should().HaveCount(2);
    }

    [Test]
    public void SelectRandomTraits_ForBossTier_ReturnsThreeTraits()
    {
        var bossTier = TierDefinition.Create("boss", "Boss", "Boss", 5.0f, 2.0f, 2.0f, 10.0f, 5.0f);

        var traits = _sut.SelectRandomTraits(["armored", "flying", "venomous", "berserker"], bossTier);

        traits.Should().HaveCount(3);
    }

    [Test]
    public void SelectRandomTraits_WithFewerAvailableThanNeeded_ReturnsAllAvailable()
    {
        var eliteTier = TierDefinition.Create("elite", "Elite", "Elite", 2.0f, 1.5f, 1.5f, 3.0f, 3.0f);

        var traits = _sut.SelectRandomTraits(["armored"], eliteTier);

        traits.Should().HaveCount(1);
        traits.Should().Contain("armored");
    }
}
