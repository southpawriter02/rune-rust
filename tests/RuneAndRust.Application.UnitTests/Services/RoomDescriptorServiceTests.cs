using FluentAssertions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class RoomDescriptorServiceTests
{
    private Mock<IDescriptorRepository> _repositoryMock = null!;
    private RoomDescriptorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IDescriptorRepository>();
        _service = new RoomDescriptorService(_repositoryMock.Object);
    }

    [Test]
    public void GenerateRoomName_ReplacesModifierTokens()
    {
        // Arrange
        var template = new BaseDescriptorTemplate(
            "Test_Base",
            "The {Modifier} Chamber",
            "A {Modifier_Adj} room.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            1, 4);

        var modifier = new ThematicModifier
        {
            Name = "Rusted",
            PrimaryBiome = Biome.TheRoots,
            Adjective = "corroded",
            DetailFragment = "shows decay"
        };

        // Act
        var result = _service.GenerateRoomName(template, modifier);

        // Assert
        result.Should().Be("The Rusted Chamber");
    }

    [Test]
    public void GenerateRoomDescription_ReplacesAllModifierTokens()
    {
        // Arrange
        var template = new BaseDescriptorTemplate(
            "Test_Base",
            "Test Name",
            "A {Modifier_Adj} room that {Modifier_Detail}.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            1, 4);

        var modifier = new ThematicModifier
        {
            Name = "Frozen",
            PrimaryBiome = Biome.Niflheim,
            Adjective = "ice-covered",
            DetailFragment = "is encased in frost"
        };

        SetupEmptyFragments();

        // Act
        var result = _service.GenerateRoomDescription(
            template, modifier, Array.Empty<string>(), new Random(42));

        // Assert
        result.Should().Contain("ice-covered");
        result.Should().Contain("is encased in frost");
    }

    [Test]
    public void GenerateRoomDescription_FillsFragmentPlaceholders()
    {
        // Arrange
        var template = new BaseDescriptorTemplate(
            "Test_Base",
            "Test Name",
            "{Spatial_Descriptor}. {Architectural_Feature}.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            1, 4);

        var modifier = new ThematicModifier
        {
            Name = "Ancient",
            PrimaryBiome = Biome.Citadel,
            Adjective = "weathered",
            DetailFragment = "bears age"
        };

        // Setup empty fragments first, then override with specific ones
        SetupEmptyFragmentsForOtherCategories();

        var spatialFragments = new List<DescriptorFragment>
        {
            DescriptorFragment.CreateSpatial("The space is vast", weight: 2)
        };
        var architecturalFragments = new List<DescriptorFragment>
        {
            DescriptorFragment.CreateArchitectural("Stone walls rise high", ArchitecturalSubcategory.Wall, weight: 2)
        };

        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Spatial, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(spatialFragments);
        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Architectural, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(architecturalFragments);

        // Act
        var result = _service.GenerateRoomDescription(
            template, modifier, Array.Empty<string>(), new Random(42));

        // Assert
        result.Should().Contain("The space is vast");
        result.Should().Contain("Stone walls rise high");
    }

    [Test]
    public void GenerateRoomDescription_HandlesArticleTokens()
    {
        // Arrange
        var template = new BaseDescriptorTemplate(
            "Test_Base",
            "Test Name",
            "{Article_Cap} ancient room. {Article} icy corridor.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            1, 4);

        var modifier = new ThematicModifier
        {
            Name = "Test",
            PrimaryBiome = Biome.Citadel,
            Adjective = "old",
            DetailFragment = "test"
        };

        SetupEmptyFragments();

        // Act
        var result = _service.GenerateRoomDescription(
            template, modifier, Array.Empty<string>(), new Random(42));

        // Assert
        result.Should().Contain("An ancient room");
        result.Should().Contain("an icy corridor");
    }

    [Test]
    public void GenerateRoomDescription_IncludesRoomFunction()
    {
        // Arrange
        var template = new BaseDescriptorTemplate(
            "Test_Base",
            "Test Name",
            "A room. {Function}",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            1, 4);

        var modifier = new ThematicModifier
        {
            Name = "Test",
            PrimaryBiome = Biome.Citadel,
            Adjective = "old",
            DetailFragment = "test"
        };

        var function = new RoomFunction(
            "Pumping Station",
            "Massive pumps dominate the space.");

        SetupEmptyFragments();

        // Act
        var result = _service.GenerateRoomDescription(
            template, modifier, Array.Empty<string>(), new Random(42), function);

        // Assert
        result.Should().Contain("Massive pumps dominate the space");
    }

    [Test]
    public void GenerateRoomDescription_CleansUpDoubleSpaces()
    {
        // Arrange
        var template = new BaseDescriptorTemplate(
            "Test_Base",
            "Test Name",
            "A room.  Multiple  spaces.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            1, 4);

        var modifier = new ThematicModifier
        {
            Name = "Test",
            PrimaryBiome = Biome.Citadel,
            Adjective = "old",
            DetailFragment = "test"
        };

        SetupEmptyFragments();

        // Act
        var result = _service.GenerateRoomDescription(
            template, modifier, Array.Empty<string>(), new Random(42));

        // Assert
        result.Should().NotContain("  ");
    }

    [Test]
    public void GenerateRoomDescription_SameRandomSeed_ProducesSameResult()
    {
        // Arrange
        var template = new BaseDescriptorTemplate(
            "Test_Base",
            "Test Name",
            "{Spatial_Descriptor}. {Detail_1}.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            1, 4);

        var modifier = new ThematicModifier
        {
            Name = "Ancient",
            PrimaryBiome = Biome.Citadel,
            Adjective = "weathered",
            DetailFragment = "bears age"
        };

        // Setup empty fragments first, then override with specific ones
        SetupEmptyFragmentsForOtherCategories();

        var fragments = new List<DescriptorFragment>
        {
            DescriptorFragment.CreateSpatial("Option A", weight: 1),
            DescriptorFragment.CreateSpatial("Option B", weight: 1),
            DescriptorFragment.CreateSpatial("Option C", weight: 1)
        };

        var detailFragments = new List<DescriptorFragment>
        {
            DescriptorFragment.CreateDetail("Detail X", DetailSubcategory.Decay, weight: 1),
            DescriptorFragment.CreateDetail("Detail Y", DetailSubcategory.Decay, weight: 1)
        };

        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Spatial, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(fragments);
        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Detail, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(detailFragments);

        // Act
        var result1 = _service.GenerateRoomDescription(
            template, modifier, Array.Empty<string>(), new Random(12345));
        var result2 = _service.GenerateRoomDescription(
            template, modifier, Array.Empty<string>(), new Random(12345));

        // Assert
        result1.Should().Be(result2);
    }

    [Test]
    public void GenerateRoomDescription_DifferentSeeds_MayProduceDifferentResults()
    {
        // Arrange
        var template = new BaseDescriptorTemplate(
            "Test_Base",
            "Test Name",
            "{Spatial_Descriptor}.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            1, 4);

        var modifier = new ThematicModifier
        {
            Name = "Ancient",
            PrimaryBiome = Biome.Citadel,
            Adjective = "weathered",
            DetailFragment = "bears age"
        };

        // Setup empty fragments first, then override with specific ones
        SetupEmptyFragmentsForOtherCategories();

        var fragments = new List<DescriptorFragment>
        {
            DescriptorFragment.CreateSpatial("Option A", weight: 1),
            DescriptorFragment.CreateSpatial("Option B", weight: 1),
            DescriptorFragment.CreateSpatial("Option C", weight: 1),
            DescriptorFragment.CreateSpatial("Option D", weight: 1),
            DescriptorFragment.CreateSpatial("Option E", weight: 1)
        };

        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Spatial, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(fragments);

        // Act - Generate many results with different seeds
        var results = Enumerable.Range(0, 100)
            .Select(seed => _service.GenerateRoomDescription(
                template, modifier, Array.Empty<string>(), new Random(seed)))
            .ToList();

        // Assert - Should have some variety
        var distinctResults = results.Distinct().Count();
        distinctResults.Should().BeGreaterThan(1, "Different seeds should produce variety");
    }

    [Test]
    public void GenerateRoomDescription_WeightedSelection_RespectsWeights()
    {
        // Arrange
        var template = new BaseDescriptorTemplate(
            "Test_Base",
            "Test Name",
            "{Spatial_Descriptor}.",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            1, 4);

        var modifier = new ThematicModifier
        {
            Name = "Ancient",
            PrimaryBiome = Biome.Citadel,
            Adjective = "weathered",
            DetailFragment = "bears age"
        };

        // Setup empty fragments first, then override with specific ones
        SetupEmptyFragmentsForOtherCategories();

        // High weight option should be selected much more often
        var fragments = new List<DescriptorFragment>
        {
            DescriptorFragment.CreateSpatial("Heavy", weight: 100),
            DescriptorFragment.CreateSpatial("Light", weight: 1)
        };

        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Spatial, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(fragments);

        // Act - Generate many results
        var heavyCount = 0;
        for (var i = 0; i < 1000; i++)
        {
            var result = _service.GenerateRoomDescription(
                template, modifier, Array.Empty<string>(), new Random(i));
            if (result.Contains("Heavy"))
                heavyCount++;
        }

        // Assert - Heavy should be selected ~99% of the time (weight 100 out of 101)
        heavyCount.Should().BeGreaterThan(900, "High weight option should dominate");
    }

    [Test]
    public void Constructor_ThrowsOnNullRepository()
    {
        // Arrange & Act
        var act = () => new RoomDescriptorService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("repository");
    }

    [Test]
    public void GenerateRoomName_ThrowsOnNullTemplate()
    {
        // Arrange & Act
        var modifier = new ThematicModifier
        {
            Name = "Test",
            PrimaryBiome = Biome.Citadel,
            Adjective = "old",
            DetailFragment = "test"
        };

        var act = () => _service.GenerateRoomName(null!, modifier);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("template");
    }

    [Test]
    public void GenerateRoomName_ThrowsOnNullModifier()
    {
        // Arrange
        var template = new BaseDescriptorTemplate(
            "Test_Base",
            "Test Name",
            "Description",
            RoomArchetype.Chamber,
            RoomSize.Medium,
            1, 4);

        // Act
        var act = () => _service.GenerateRoomName(template, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("modifier");
    }

    private void SetupEmptyFragments()
    {
        _repositoryMock.Setup(r => r.GetFragments(It.IsAny<FragmentCategory>(), It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(new List<DescriptorFragment>());
    }

    private void SetupEmptyFragmentsForOtherCategories()
    {
        // Setup all fragment categories with empty lists as fallback
        // Tests that need specific fragments will override these
        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Direction, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(new List<DescriptorFragment>());
        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Atmospheric, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(new List<DescriptorFragment>());
        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Architectural, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(new List<DescriptorFragment>());
        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Detail, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(new List<DescriptorFragment>());
        _repositoryMock.Setup(r => r.GetFragments(FragmentCategory.Spatial, It.IsAny<Biome?>(), It.IsAny<IEnumerable<string>?>()))
            .Returns(new List<DescriptorFragment>());
    }
}
