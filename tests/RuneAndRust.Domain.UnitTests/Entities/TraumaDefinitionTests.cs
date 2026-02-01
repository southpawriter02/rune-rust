namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TraumaDefinition"/> entity.
/// Verifies factory method, validation, and ToString formatting.
/// </summary>
[TestFixture]
public class TraumaDefinitionTests
{
    // -------------------------------------------------------------------------
    // Factory Method — Valid Creation
    // -------------------------------------------------------------------------

    [Test]
    public void Create_WithValidParameters_CreatesTraumaDefinition()
    {
        // Arrange
        var traumaId = "survivors-guilt";
        var name = "Survivor's Guilt";
        var type = TraumaType.Emotional;
        var description = "You carry the weight of those who didn't make it.";
        var flavorText = "Why them? Why not me?";

        // Act
        var trauma = TraumaDefinition.Create(
            traumaId: traumaId,
            name: name,
            type: type,
            description: description,
            flavorText: flavorText,
            isRetirementTrauma: true,
            retirementCondition: "On acquisition",
            isStackable: false,
            acquisitionSources: new[] { "AllyDeath" },
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Assert
        trauma.TraumaId.Should().Be(traumaId);
        trauma.Name.Should().Be(name);
        trauma.Type.Should().Be(TraumaType.Emotional);
        trauma.Description.Should().Be(description);
        trauma.FlavorText.Should().Be(flavorText);
        trauma.IsRetirementTrauma.Should().BeTrue();
        trauma.RetirementCondition.Should().Be("On acquisition");
        trauma.IsStackable.Should().BeFalse();
        trauma.AcquisitionSources.Should().ContainSingle("AllyDeath");
        trauma.Id.Should().NotBe(Guid.Empty);
    }

    [Test]
    public void Create_WithStackableTrauma_SetsStackableTrue()
    {
        // Arrange & Act
        var trauma = TraumaDefinition.Create(
            traumaId: "reality-doubt",
            name: "Reality Doubt",
            type: TraumaType.Existential,
            description: "You question what is real.",
            flavorText: "Was any of this ever real?",
            isRetirementTrauma: true,
            retirementCondition: "5+ instances",
            isStackable: true,
            acquisitionSources: new[] { "WitnessingHorror" },
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Assert
        trauma.IsStackable.Should().BeTrue();
    }

    [Test]
    public void Create_WithCorruptionTrauma_SetsTypeCorrectly()
    {
        // Arrange & Act
        var trauma = TraumaDefinition.Create(
            traumaId: "machine-affinity",
            name: "[MACHINE AFFINITY]",
            type: TraumaType.Corruption,
            description: "You feel kinship with the machine ghosts.",
            flavorText: "{ERROR: ORGANIC DESIGNATION DEPRECATED}",
            isRetirementTrauma: true,
            retirementCondition: "On acquisition",
            isStackable: false,
            acquisitionSources: new[] { "CorruptionThreshold75", "ForlornContact" },
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Assert
        trauma.Type.Should().Be(TraumaType.Corruption);
        trauma.Name.Should().Contain("[MACHINE AFFINITY]");
    }

    [Test]
    public void Create_NormalizesTraumaIdToLowercase()
    {
        // Arrange & Act
        var trauma = TraumaDefinition.Create(
            traumaId: "Survivors-Guilt",
            name: "Survivor's Guilt",
            type: TraumaType.Emotional,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: false,
            retirementCondition: null,
            isStackable: false,
            acquisitionSources: new List<string>(),
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Assert
        trauma.TraumaId.Should().Be("survivors-guilt");
    }

    [Test]
    public void Create_NonRetirementTrauma_AllowsNullCondition()
    {
        // Arrange & Act
        var trauma = TraumaDefinition.Create(
            traumaId: "combat-flashbacks",
            name: "Combat Flashbacks",
            type: TraumaType.Cognitive,
            description: "Sudden vivid memories of violence.",
            flavorText: "The screams never stop.",
            isRetirementTrauma: false,
            retirementCondition: null,
            isStackable: false,
            acquisitionSources: new[] { "NearDeathExperience" },
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Assert
        trauma.IsRetirementTrauma.Should().BeFalse();
        trauma.RetirementCondition.Should().BeNull();
    }

    [Test]
    public void Create_WithTriggersAndEffects_StoresCollections()
    {
        // Arrange
        var triggers = new List<TraumaTrigger>
        {
            TraumaTrigger.Create("OnRest", null, true, 2)
        };
        var effects = new List<TraumaEffect>
        {
            TraumaEffect.Create("Penalty", "social-skills", -3, null, "-3 to social")
        };

        // Act
        var trauma = TraumaDefinition.Create(
            traumaId: "touch-aversion",
            name: "Touch Aversion",
            type: TraumaType.Social,
            description: "Physical contact causes distress.",
            flavorText: "Don't touch me!",
            isRetirementTrauma: false,
            retirementCondition: null,
            isStackable: false,
            acquisitionSources: new List<string>(),
            triggers: triggers,
            effects: effects
        );

        // Assert
        trauma.Triggers.Should().HaveCount(1);
        trauma.Effects.Should().HaveCount(1);
        trauma.Triggers[0].TriggerType.Should().Be("OnRest");
        trauma.Effects[0].EffectType.Should().Be("Penalty");
    }

    // -------------------------------------------------------------------------
    // Factory Method — Validation Errors
    // -------------------------------------------------------------------------

    [Test]
    public void Create_WithEmptyTraumaId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaDefinition.Create(
            traumaId: "",
            name: "Test",
            type: TraumaType.Cognitive,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: false,
            retirementCondition: null,
            isStackable: false,
            acquisitionSources: new List<string>(),
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("traumaId");
    }

    [Test]
    public void Create_WithWhitespaceTraumaId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaDefinition.Create(
            traumaId: "   ",
            name: "Test",
            type: TraumaType.Cognitive,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: false,
            retirementCondition: null,
            isStackable: false,
            acquisitionSources: new List<string>(),
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("traumaId");
    }

    [Test]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaDefinition.Create(
            traumaId: "test-trauma",
            name: "",
            type: TraumaType.Cognitive,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: false,
            retirementCondition: null,
            isStackable: false,
            acquisitionSources: new List<string>(),
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Create_RetirementTraumaWithoutCondition_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaDefinition.Create(
            traumaId: "test",
            name: "Test Trauma",
            type: TraumaType.Corruption,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: true,
            retirementCondition: null,
            isStackable: false,
            acquisitionSources: new List<string>(),
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("retirementCondition");
    }

    [Test]
    public void Create_RetirementTraumaWithEmptyCondition_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaDefinition.Create(
            traumaId: "test",
            name: "Test Trauma",
            type: TraumaType.Corruption,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: true,
            retirementCondition: "   ",
            isStackable: false,
            acquisitionSources: new List<string>(),
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("retirementCondition");
    }

    // -------------------------------------------------------------------------
    // ToString Formatting
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_WithNonRetirementNonStackable_ReturnsBasicFormat()
    {
        // Arrange
        var trauma = TraumaDefinition.Create(
            traumaId: "combat-flashbacks",
            name: "Combat Flashbacks",
            type: TraumaType.Cognitive,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: false,
            retirementCondition: null,
            isStackable: false,
            acquisitionSources: new List<string>(),
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Act
        var result = trauma.ToString();

        // Assert
        result.Should().Be("Combat Flashbacks (Cognitive) [ID: combat-flashbacks]");
        result.Should().NotContain("RETIREMENT");
        result.Should().NotContain("STACKABLE");
    }

    [Test]
    public void ToString_WithRetirementTrauma_IncludesRetirementTag()
    {
        // Arrange
        var trauma = TraumaDefinition.Create(
            traumaId: "machine-affinity",
            name: "Machine Affinity",
            type: TraumaType.Corruption,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: true,
            retirementCondition: "On acquisition",
            isStackable: false,
            acquisitionSources: new List<string>(),
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Act
        var result = trauma.ToString();

        // Assert
        result.Should().Contain("[RETIREMENT]");
    }

    [Test]
    public void ToString_WithStackableTrauma_IncludesStackableTag()
    {
        // Arrange
        var trauma = TraumaDefinition.Create(
            traumaId: "reality-doubt",
            name: "Reality Doubt",
            type: TraumaType.Existential,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: true,
            retirementCondition: "5+ instances",
            isStackable: true,
            acquisitionSources: new List<string>(),
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );

        // Act
        var result = trauma.ToString();

        // Assert
        result.Should().Contain("[STACKABLE]");
        result.Should().Contain("[RETIREMENT]");
    }

    // -------------------------------------------------------------------------
    // Unique ID Generation
    // -------------------------------------------------------------------------

    [Test]
    public void Create_GeneratesUniqueId()
    {
        // Arrange & Act
        var trauma1 = CreateMinimalTrauma("trauma-1");
        var trauma2 = CreateMinimalTrauma("trauma-2");

        // Assert
        trauma1.Id.Should().NotBe(Guid.Empty);
        trauma2.Id.Should().NotBe(Guid.Empty);
        trauma1.Id.Should().NotBe(trauma2.Id);
    }

    // -------------------------------------------------------------------------
    // Null Collection Handling
    // -------------------------------------------------------------------------

    [Test]
    public void Create_WithNullCollections_UsesEmptyLists()
    {
        // Arrange & Act
        var trauma = TraumaDefinition.Create(
            traumaId: "test",
            name: "Test",
            type: TraumaType.Cognitive,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: false,
            retirementCondition: null,
            isStackable: false,
            acquisitionSources: null!,
            triggers: null!,
            effects: null!
        );

        // Assert
        trauma.AcquisitionSources.Should().NotBeNull();
        trauma.AcquisitionSources.Should().BeEmpty();
        trauma.Triggers.Should().NotBeNull();
        trauma.Triggers.Should().BeEmpty();
        trauma.Effects.Should().NotBeNull();
        trauma.Effects.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Helper Methods
    // -------------------------------------------------------------------------

    private static TraumaDefinition CreateMinimalTrauma(string traumaId)
    {
        return TraumaDefinition.Create(
            traumaId: traumaId,
            name: "Test",
            type: TraumaType.Cognitive,
            description: "Test",
            flavorText: "Test",
            isRetirementTrauma: false,
            retirementCondition: null,
            isStackable: false,
            acquisitionSources: new List<string>(),
            triggers: new List<TraumaTrigger>(),
            effects: new List<TraumaEffect>()
        );
    }
}
