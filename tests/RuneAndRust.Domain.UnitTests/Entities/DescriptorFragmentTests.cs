using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class DescriptorFragmentTests
{
    [Test]
    public void CreateSpatial_CreatesFragmentWithCorrectCategory()
    {
        // Act
        var fragment = DescriptorFragment.CreateSpatial("The space is vast");

        // Assert
        fragment.Category.Should().Be(FragmentCategory.Spatial);
        fragment.Text.Should().Be("The space is vast");
        fragment.Subcategory.Should().BeNull();
    }

    [Test]
    public void CreateArchitectural_CreatesFragmentWithSubcategory()
    {
        // Act
        var fragment = DescriptorFragment.CreateArchitectural(
            "Stone walls rise high",
            ArchitecturalSubcategory.Wall);

        // Assert
        fragment.Category.Should().Be(FragmentCategory.Architectural);
        fragment.Subcategory.Should().Be("Wall");
    }

    [Test]
    public void CreateDetail_CreatesFragmentWithSubcategory()
    {
        // Act
        var fragment = DescriptorFragment.CreateDetail(
            "Rust covers everything",
            DetailSubcategory.Decay);

        // Assert
        fragment.Category.Should().Be(FragmentCategory.Detail);
        fragment.Subcategory.Should().Be("Decay");
    }

    [Test]
    public void CreateAtmospheric_CreatesFragmentWithSubcategory()
    {
        // Act
        var fragment = DescriptorFragment.CreateAtmospheric(
            "A musty smell fills the air",
            AtmosphericSubcategory.Smell);

        // Assert
        fragment.Category.Should().Be(FragmentCategory.Atmospheric);
        fragment.Subcategory.Should().Be("Smell");
    }

    [Test]
    public void CreateDirection_CreatesFragmentCorrectly()
    {
        // Act
        var fragment = DescriptorFragment.CreateDirection("into darkness ahead");

        // Assert
        fragment.Category.Should().Be(FragmentCategory.Direction);
        fragment.Text.Should().Be("into darkness ahead");
    }

    [Test]
    public void Constructor_SetsWeight()
    {
        // Act
        var fragment = DescriptorFragment.CreateSpatial("Test", weight: 5);

        // Assert
        fragment.Weight.Should().Be(5);
    }

    [Test]
    public void Constructor_SetsBiomeAffinity()
    {
        // Act
        var fragment = DescriptorFragment.CreateSpatial(
            "Ice formations",
            biomeAffinity: Biome.Niflheim);

        // Assert
        fragment.BiomeAffinity.Should().Be(Biome.Niflheim);
    }

    [Test]
    public void Constructor_SetsTags()
    {
        // Act
        var fragment = DescriptorFragment.CreateSpatial(
            "Cramped space",
            tags: ["Cramped", "Confined"]);

        // Assert
        fragment.Tags.Should().Contain("Cramped");
        fragment.Tags.Should().Contain("Confined");
    }

    [Test]
    public void Constructor_ThrowsOnEmptyText()
    {
        // Act
        var act = () => new DescriptorFragment(FragmentCategory.Spatial, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("text");
    }

    [Test]
    public void Constructor_ThrowsOnWeightLessThanOne()
    {
        // Act
        var act = () => new DescriptorFragment(
            FragmentCategory.Spatial,
            "Test",
            weight: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("weight");
    }

    [Test]
    public void MatchesBiome_ReturnsTrueForSameBiome()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial(
            "Test",
            biomeAffinity: Biome.Muspelheim);

        // Act & Assert
        fragment.MatchesBiome(Biome.Muspelheim).Should().BeTrue();
    }

    [Test]
    public void MatchesBiome_ReturnsFalseForDifferentBiome()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial(
            "Test",
            biomeAffinity: Biome.Muspelheim);

        // Act & Assert
        fragment.MatchesBiome(Biome.Niflheim).Should().BeFalse();
    }

    [Test]
    public void MatchesBiome_ReturnsTrueForNullAffinity()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial("Test");

        // Act & Assert
        fragment.MatchesBiome(Biome.Muspelheim).Should().BeTrue();
        fragment.MatchesBiome(Biome.Niflheim).Should().BeTrue();
    }

    [Test]
    public void MatchesTags_ReturnsTrueWhenNoTagsRequired()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial("Test", tags: ["SomeTag"]);

        // Act & Assert
        fragment.MatchesTags(null).Should().BeTrue();
        fragment.MatchesTags(Array.Empty<string>()).Should().BeTrue();
    }

    [Test]
    public void MatchesTags_ReturnsTrueWhenFragmentHasNoTags()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial("Test");

        // Act & Assert
        fragment.MatchesTags(new[] { "RequiredTag" }).Should().BeTrue();
    }

    [Test]
    public void MatchesTags_ReturnsTrueWhenTagsIntersect()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial("Test", tags: ["A", "B", "C"]);

        // Act & Assert
        fragment.MatchesTags(new[] { "B", "D" }).Should().BeTrue();
    }

    [Test]
    public void MatchesTags_ReturnsFalseWhenNoIntersection()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial("Test", tags: ["A", "B"]);

        // Act & Assert
        fragment.MatchesTags(new[] { "C", "D" }).Should().BeFalse();
    }

    [Test]
    public void AddTag_AddsNewTag()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial("Test");

        // Act
        fragment.AddTag("NewTag");

        // Assert
        fragment.Tags.Should().Contain("NewTag");
    }

    [Test]
    public void AddTag_IgnoresEmptyTag()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial("Test");
        var initialCount = fragment.Tags.Count;

        // Act
        fragment.AddTag("");
        fragment.AddTag("  ");

        // Assert
        fragment.Tags.Count.Should().Be(initialCount);
    }

    [Test]
    public void HasTag_ReturnsTrueForExistingTag()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial("Test", tags: ["MyTag"]);

        // Act & Assert
        fragment.HasTag("MyTag").Should().BeTrue();
    }

    [Test]
    public void HasTag_ReturnsFalseForMissingTag()
    {
        // Arrange
        var fragment = DescriptorFragment.CreateSpatial("Test", tags: ["OtherTag"]);

        // Act & Assert
        fragment.HasTag("MyTag").Should().BeFalse();
    }
}
