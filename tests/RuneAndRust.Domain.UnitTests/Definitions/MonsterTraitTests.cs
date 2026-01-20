using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for MonsterTrait (v0.0.9c).
/// </summary>
[TestFixture]
public class MonsterTraitTests
{
    [Test]
    public void Create_WithValidParameters_ReturnsMonsterTrait()
    {
        var trait = MonsterTrait.Create(
            id: "regenerating",
            name: "Regenerating",
            description: "Heals each turn.",
            effect: TraitEffect.Regeneration,
            effectValue: 5,
            triggerThreshold: null,
            tags: ["healing", "passive"],
            color: "green",
            sortOrder: 0);

        trait.Should().NotBeNull();
        trait.Id.Should().Be("regenerating");
        trait.Name.Should().Be("Regenerating");
        trait.Description.Should().Be("Heals each turn.");
        trait.Effect.Should().Be(TraitEffect.Regeneration);
        trait.EffectValue.Should().Be(5);
        trait.TriggerThreshold.Should().BeNull();
        trait.Tags.Should().BeEquivalentTo(["healing", "passive"]);
        trait.Color.Should().Be("green");
        trait.SortOrder.Should().Be(0);
    }

    [Test]
    public void Create_WithTriggerThreshold_SetsThreshold()
    {
        var trait = MonsterTrait.Create(
            id: "berserker",
            name: "Berserker",
            description: "More damage when low HP.",
            effect: TraitEffect.Berserker,
            effectValue: 50,
            triggerThreshold: 30,
            tags: ["offensive"],
            color: "red");

        trait.TriggerThreshold.Should().Be(30);
    }

    [Test]
    public void Create_WithNullId_ThrowsArgumentException()
    {
        var act = () => MonsterTrait.Create(
            id: null!,
            name: "Test",
            description: "Test",
            effect: TraitEffect.None,
            effectValue: 0);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("id");
    }

    [Test]
    public void HasTag_WithMatchingTag_ReturnsTrue()
    {
        var trait = MonsterTrait.Create(
            id: "test",
            name: "Test",
            description: "Test",
            effect: TraitEffect.None,
            effectValue: 0,
            tags: ["healing", "passive"]);

        trait.HasTag("healing").Should().BeTrue();
        trait.HasTag("PASSIVE").Should().BeTrue();
    }

    [Test]
    public void HasTag_WithNonMatchingTag_ReturnsFalse()
    {
        var trait = MonsterTrait.Create(
            id: "test",
            name: "Test",
            description: "Test",
            effect: TraitEffect.None,
            effectValue: 0,
            tags: ["healing"]);

        trait.HasTag("offensive").Should().BeFalse();
    }
}
