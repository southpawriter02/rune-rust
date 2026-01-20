using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class ResourceChangeTests
{
    [Test]
    public void Delta_ReturnsCorrectDifference()
    {
        var change = new ResourceChange("mana", 50, 60, ResourceChangeType.Regeneration);

        Assert.That(change.Delta, Is.EqualTo(10));
    }

    [Test]
    public void IsGain_ReturnsTrue_WhenDeltaPositive()
    {
        var change = new ResourceChange("mana", 50, 60, ResourceChangeType.Regeneration);

        Assert.That(change.IsGain, Is.True);
        Assert.That(change.IsLoss, Is.False);
    }

    [Test]
    public void IsLoss_ReturnsTrue_WhenDeltaNegative()
    {
        var change = new ResourceChange("rage", 50, 40, ResourceChangeType.Decay);

        Assert.That(change.IsLoss, Is.True);
        Assert.That(change.IsGain, Is.False);
    }

    [Test]
    public void ResourceChangeResult_HasChanges_WhenNotEmpty()
    {
        var changes = new List<ResourceChange>
        {
            new("mana", 50, 60, ResourceChangeType.Regeneration)
        };
        var result = new ResourceChangeResult(changes);

        Assert.That(result.HasChanges, Is.True);
    }
}
