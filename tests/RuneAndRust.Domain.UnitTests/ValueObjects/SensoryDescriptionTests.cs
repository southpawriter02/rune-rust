using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class SensoryDescriptionTests
{
    [Test]
    public void SoundDescription_HasSounds_WhenDistantSet_ReturnsTrue()
    {
        var sound = new SoundDescription
        {
            Distant = "Echoes from afar",
            Nearby = null,
            Immediate = null
        };

        Assert.That(sound.HasSounds, Is.True);
    }

    [Test]
    public void SoundDescription_HasSounds_WhenAllEmpty_ReturnsFalse()
    {
        var sound = new SoundDescription
        {
            Distant = null,
            Nearby = "",
            Immediate = null
        };

        Assert.That(sound.HasSounds, Is.False);
    }

    [Test]
    public void SoundDescription_Primary_ReturnsNearbyFirst()
    {
        var sound = new SoundDescription
        {
            Distant = "Distant thunder",
            Nearby = "Water drips",
            Immediate = "Your heartbeat"
        };

        Assert.That(sound.Primary, Is.EqualTo("Water drips"));
    }

    [Test]
    public void SoundDescription_Primary_FallsBackToDistant()
    {
        var sound = new SoundDescription
        {
            Distant = "Distant thunder",
            Nearby = null,
            Immediate = null
        };

        Assert.That(sound.Primary, Is.EqualTo("Distant thunder"));
    }

    [Test]
    public void SoundDescription_ToFullDescription_CombinesAllLayers()
    {
        var sound = new SoundDescription
        {
            Distant = "Thunder rumbles.",
            Nearby = "Rain patters.",
            Immediate = "You breathe heavily."
        };

        var result = sound.ToFullDescription();

        Assert.That(result, Does.Contain("Thunder rumbles."));
        Assert.That(result, Does.Contain("Rain patters."));
        Assert.That(result, Does.Contain("You breathe heavily."));
    }

    [Test]
    public void SensoryDescription_HasDescriptions_WhenLightingSet_ReturnsTrue()
    {
        var sensory = new SensoryDescription
        {
            Lighting = new LightingDescription("Torchlight flickers", "torch", "dim"),
            Sounds = new SoundDescription(),
            Smell = new SmellDescription(),
            Temperature = string.Empty
        };

        Assert.That(sensory.HasDescriptions, Is.True);
    }

    [Test]
    public void SensoryDescription_HasDescriptions_WhenEmpty_ReturnsFalse()
    {
        var sensory = new SensoryDescription
        {
            Lighting = new LightingDescription(string.Empty, null, null),
            Sounds = new SoundDescription(),
            Smell = new SmellDescription(),
            Temperature = string.Empty
        };

        Assert.That(sensory.HasDescriptions, Is.False);
    }

    [Test]
    public void SensoryDescription_ToNarrative_CombinesSenses()
    {
        var sensory = new SensoryDescription
        {
            Lighting = new LightingDescription("Torchlight flickers.", "torch", "dim"),
            Sounds = new SoundDescription { Nearby = "Water drips." },
            Smell = new SmellDescription("Musty air.", SmellIntensity.Noticeable, "must"),
            Temperature = "Cool air chills you."
        };

        var result = sensory.ToNarrative();

        Assert.That(result, Does.Contain("Torchlight flickers."));
        Assert.That(result, Does.Contain("Water drips."));
        Assert.That(result, Does.Contain("Musty air."));
    }

    [Test]
    public void SensoryDescription_ToNarrative_RespectsMaxSenses()
    {
        var sensory = new SensoryDescription
        {
            Lighting = new LightingDescription("Torchlight flickers.", "torch", "dim"),
            Sounds = new SoundDescription { Nearby = "Water drips." },
            Smell = new SmellDescription("Musty air.", SmellIntensity.Noticeable, "must"),
            Temperature = "Cool air chills you."
        };

        var result = sensory.ToNarrative(2);

        // Should only include 2 senses
        var parts = result.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(parts.Length, Is.LessThanOrEqualTo(2));
    }

    [Test]
    public void SmellIntensity_HasCorrectValues()
    {
        Assert.That(SmellIntensity.Faint, Is.EqualTo((SmellIntensity)0));
        Assert.That(SmellIntensity.Noticeable, Is.EqualTo((SmellIntensity)1));
        Assert.That(SmellIntensity.Strong, Is.EqualTo((SmellIntensity)2));
        Assert.That(SmellIntensity.Overwhelming, Is.EqualTo((SmellIntensity)3));
    }

    [Test]
    public void LightingDescription_StoresAllProperties()
    {
        var lighting = new LightingDescription("Bright sunlight.", "sunlight", "bright");

        Assert.That(lighting.Description, Is.EqualTo("Bright sunlight."));
        Assert.That(lighting.LightSource, Is.EqualTo("sunlight"));
        Assert.That(lighting.DarknessLevel, Is.EqualTo("bright"));
    }

    [Test]
    public void SmellDescription_StoresAllProperties()
    {
        var smell = new SmellDescription("Sulfur stings.", SmellIntensity.Strong, "sulfur");

        Assert.That(smell.Description, Is.EqualTo("Sulfur stings."));
        Assert.That(smell.Intensity, Is.EqualTo(SmellIntensity.Strong));
        Assert.That(smell.SmellType, Is.EqualTo("sulfur"));
    }
}
