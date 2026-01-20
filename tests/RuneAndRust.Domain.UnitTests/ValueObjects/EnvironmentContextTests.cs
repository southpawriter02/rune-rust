using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class EnvironmentContextTests
{
    [Test]
    public void GetValue_WhenCategoryExists_ReturnsValue()
    {
        var values = new Dictionary<string, string>
        {
            ["biome"] = "cave",
            ["climate"] = "cold"
        };
        var context = new EnvironmentContext(values, []);

        Assert.That(context.GetValue("biome"), Is.EqualTo("cave"));
        Assert.That(context.GetValue("climate"), Is.EqualTo("cold"));
    }

    [Test]
    public void GetValue_WhenCategoryMissing_ReturnsNull()
    {
        var values = new Dictionary<string, string>
        {
            ["biome"] = "cave"
        };
        var context = new EnvironmentContext(values, []);

        Assert.That(context.GetValue("lighting"), Is.Null);
    }

    [Test]
    public void WithValue_AddsNewCategory_PreservesExisting()
    {
        var values = new Dictionary<string, string>
        {
            ["biome"] = "cave"
        };
        var context = new EnvironmentContext(values, []);

        var updated = context.WithValue("climate", "cold");

        Assert.That(updated.GetValue("biome"), Is.EqualTo("cave"));
        Assert.That(updated.GetValue("climate"), Is.EqualTo("cold"));
        // Original unchanged (immutability)
        Assert.That(context.GetValue("climate"), Is.Null);
    }

    [Test]
    public void Biome_Property_ReturnsBiomeValue()
    {
        var values = new Dictionary<string, string>
        {
            ["biome"] = "volcanic",
            ["climate"] = "scorching"
        };
        var context = new EnvironmentContext(values, []);

        Assert.That(context.Biome, Is.EqualTo("volcanic"));
        Assert.That(context.Climate, Is.EqualTo("scorching"));
    }

    [Test]
    public void HasCategory_WhenExists_ReturnsTrue()
    {
        var values = new Dictionary<string, string>
        {
            ["biome"] = "cave",
            ["lighting"] = "dim"
        };
        var context = new EnvironmentContext(values, []);

        Assert.That(context.HasCategory("biome"), Is.True);
        Assert.That(context.HasCategory("lighting"), Is.True);
        Assert.That(context.HasCategory("condition"), Is.False);
    }

    [Test]
    public void DerivedTags_PreservesTags()
    {
        var values = new Dictionary<string, string> { ["biome"] = "cave" };
        var tags = new List<string> { "underground", "natural", "stone" };
        var context = new EnvironmentContext(values, tags);

        Assert.That(context.DerivedTags, Is.EqualTo(tags));
    }

    [Test]
    public void WithDerivedTags_ReplacesExistingTags()
    {
        var context = new EnvironmentContext(
            new Dictionary<string, string> { ["biome"] = "cave" },
            ["old_tag"]);

        var updated = context.WithDerivedTags(["new_tag_1", "new_tag_2"]);

        Assert.That(updated.DerivedTags, Is.EquivalentTo(new[] { "new_tag_1", "new_tag_2" }));
        Assert.That(context.DerivedTags, Is.EquivalentTo(new[] { "old_tag" })); // Original unchanged
    }

    [Test]
    public void DefaultConstructor_CreatesEmptyContext()
    {
        var context = new EnvironmentContext();

        Assert.That(context.CategoryValues, Is.Empty);
        Assert.That(context.DerivedTags, Is.Empty);
        Assert.That(context.Biome, Is.Null);
    }

    [Test]
    public void ToString_ReturnsReadableFormat()
    {
        var values = new Dictionary<string, string>
        {
            ["biome"] = "cave",
            ["climate"] = "cold"
        };
        var context = new EnvironmentContext(values, []);

        var result = context.ToString();

        Assert.That(result, Does.Contain("biome:cave"));
        Assert.That(result, Does.Contain("climate:cold"));
    }

    [Test]
    public void ConvenienceProperties_ReturnCorrectValues()
    {
        var values = new Dictionary<string, string>
        {
            ["biome"] = "dungeon",
            ["climate"] = "temperate",
            ["lighting"] = "dim",
            ["era"] = "ancient",
            ["condition"] = "collapsed"
        };
        var context = new EnvironmentContext(values, []);

        Assert.That(context.Biome, Is.EqualTo("dungeon"));
        Assert.That(context.Climate, Is.EqualTo("temperate"));
        Assert.That(context.Lighting, Is.EqualTo("dim"));
        Assert.That(context.Era, Is.EqualTo("ancient"));
        Assert.That(context.Condition, Is.EqualTo("collapsed"));
    }
}
