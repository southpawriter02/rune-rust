using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for TierService (v0.0.9c).
/// </summary>
[TestFixture]
public class TierServiceTests
{
    private Mock<IGameConfigurationProvider> _configProviderMock = null!;
    private Mock<ILogger<TierService>> _loggerMock = null!;
    private TierService _sut = null!;
    private List<TierDefinition> _tiers = null!;

    [SetUp]
    public void SetUp()
    {
        _configProviderMock = new Mock<IGameConfigurationProvider>();
        _loggerMock = new Mock<ILogger<TierService>>();

        _tiers = new List<TierDefinition>
        {
            TierDefinition.Create("common", "Common", null, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, "white", 70, false, 0),
            TierDefinition.Create("named", "Named", null, 1.5f, 1.3f, 1.2f, 2.0f, 2.0f, "yellow", 20, true, 1),
            TierDefinition.Create("elite", "Elite", "Elite", 2.0f, 1.5f, 1.5f, 3.0f, 3.0f, "orange3", 8, false, 2),
            TierDefinition.Create("boss", "Boss", "Boss", 5.0f, 2.0f, 2.0f, 10.0f, 5.0f, "red", 2, false, 3)
        };

        _configProviderMock.Setup(x => x.GetTiers()).Returns(_tiers);
        _configProviderMock.Setup(x => x.GetTierById(It.IsAny<string>()))
            .Returns((string id) => _tiers.FirstOrDefault(t => t.Id == id));

        _sut = new TierService(_configProviderMock.Object, _loggerMock.Object);
    }

    [Test]
    public void GetTier_WithValidId_ReturnsTier()
    {
        var tier = _sut.GetTier("elite");

        tier.Should().NotBeNull();
        tier!.Id.Should().Be("elite");
        tier.Name.Should().Be("Elite");
    }

    [Test]
    public void GetTier_WithInvalidId_ReturnsNull()
    {
        var tier = _sut.GetTier("invalid");

        tier.Should().BeNull();
    }

    [Test]
    public void GetTier_WithNullId_ReturnsNull()
    {
        var tier = _sut.GetTier(null!);

        tier.Should().BeNull();
    }

    [Test]
    public void GetAllTiers_ReturnsTiersFromProvider()
    {
        var tiers = _sut.GetAllTiers();

        tiers.Should().HaveCount(4);
        tiers.Select(t => t.Id).Should().BeEquivalentTo(["common", "named", "elite", "boss"]);
    }

    [Test]
    public void GetDefaultTier_ReturnsCommonTier()
    {
        var tier = _sut.GetDefaultTier();

        tier.Id.Should().Be("common");
    }

    [Test]
    public void SelectRandomTier_WithEmptyList_ReturnsDefaultTier()
    {
        var tier = _sut.SelectRandomTier([]);

        tier.Id.Should().Be("common");
    }

    [Test]
    public void SelectRandomTier_WithSingleTier_ReturnsThatTier()
    {
        var tier = _sut.SelectRandomTier(["elite"]);

        tier.Id.Should().Be("elite");
    }

    [Test]
    public void SelectRandomTier_WithMultipleTiers_ReturnsValidTier()
    {
        var tier = _sut.SelectRandomTier(["common", "elite"]);

        tier.Id.Should().BeOneOf("common", "elite");
    }
}
