using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class ClassRequirementsTests
{
    [Test]
    public void HasRequirements_WhenEmpty_ReturnsFalse()
    {
        var requirements = new ClassRequirements();

        Assert.That(requirements.HasRequirements, Is.False);
    }

    [Test]
    public void HasRequirements_WithRaceIds_ReturnsTrue()
    {
        var requirements = new ClassRequirements
        {
            AllowedRaceIds = ["elf", "human"]
        };

        Assert.That(requirements.HasRequirements, Is.True);
    }

    [Test]
    public void Validate_AllowedRace_ReturnsValid()
    {
        var requirements = new ClassRequirements
        {
            AllowedRaceIds = ["elf", "human"]
        };
        var attrs = new Dictionary<string, int>();

        var result = requirements.Validate("human", attrs);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_DisallowedRace_ReturnsInvalid()
    {
        var requirements = new ClassRequirements
        {
            AllowedRaceIds = ["elf"]
        };
        var attrs = new Dictionary<string, int>();

        var result = requirements.Validate("human", attrs);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.FailureReasons, Has.Count.EqualTo(1));
    }

    [Test]
    public void Validate_AttributesMet_ReturnsValid()
    {
        var requirements = new ClassRequirements
        {
            MinimumAttributes = new Dictionary<string, int> { ["will"] = 12 }
        };
        var attrs = new Dictionary<string, int> { ["will"] = 14 };

        var result = requirements.Validate("human", attrs);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_AttributesNotMet_ReturnsInvalid()
    {
        var requirements = new ClassRequirements
        {
            MinimumAttributes = new Dictionary<string, int> { ["will"] = 12 }
        };
        var attrs = new Dictionary<string, int> { ["will"] = 10 };

        var result = requirements.Validate("human", attrs);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.FailureReasons[0], Does.Contain("will"));
    }
}
