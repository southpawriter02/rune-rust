using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for NameGeneratorConfig (v0.0.9c).
/// </summary>
[TestFixture]
public class NameGeneratorConfigTests
{
    [Test]
    public void Default_HasValidPrefixesAndSuffixes()
    {
        var config = NameGeneratorConfig.Default;

        config.Prefixes.Should().HaveCountGreaterOrEqualTo(5);
        config.Suffixes.Should().HaveCountGreaterOrEqualTo(5);
        config.TitleFormat.Should().Be("{0} the {1}");
    }

    [Test]
    public void GenerateName_WithValidConfig_GeneratesName()
    {
        var config = new NameGeneratorConfig
        {
            Prefixes = ["Gr", "Kr"],
            Suffixes = ["ok", "ax"],
            TitleFormat = "{0} the {1}"
        };

        var name = config.GenerateName("Orc", new Random(42));

        name.Should().NotBeNullOrEmpty();
        name.Should().Contain("the Orc");
    }

    [Test]
    public void GenerateName_WithEmptyPrefixes_ReturnsMonsterTypeName()
    {
        var config = new NameGeneratorConfig
        {
            Prefixes = [],
            Suffixes = ["ok"],
            TitleFormat = "{0} the {1}"
        };

        var name = config.GenerateName("Orc");

        name.Should().Be("Orc");
    }

    [Test]
    public void GenerateName_WithEmptySuffixes_ReturnsMonsterTypeName()
    {
        var config = new NameGeneratorConfig
        {
            Prefixes = ["Gr"],
            Suffixes = [],
            TitleFormat = "{0} the {1}"
        };

        var name = config.GenerateName("Orc");

        name.Should().Be("Orc");
    }

    [Test]
    public void GenerateName_WithCustomTitleFormat_UsesCustomFormat()
    {
        var config = new NameGeneratorConfig
        {
            Prefixes = ["Test"],
            Suffixes = ["Name"],
            TitleFormat = "{0} the Mighty {1}"
        };

        var name = config.GenerateName("Orc", new Random(42));

        name.Should().Contain("the Mighty Orc");
    }
}
