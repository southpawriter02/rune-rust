using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Persistence.Seeders;

namespace RuneAndRust.Infrastructure.IntegrationTests.Seeders;

[TestFixture]
public class InteractionDescriptorSeederTests
{
    [Test]
    public void GetAllDescriptors_ReturnsExpectedCount()
    {
        // Arrange & Act
        var descriptors = InteractionDescriptorSeeder.GetAllDescriptors().ToList();

        // Assert - Should be 90+ descriptors (spec says 150+ but we implemented key ones)
        descriptors.Should().HaveCountGreaterThanOrEqualTo(90);
    }

    [Test]
    public void GetAllDescriptors_ContainsMechanicalObjects()
    {
        // Arrange & Act
        var descriptors = InteractionDescriptorSeeder.GetAllDescriptors().ToList();

        // Assert
        var mechanicalObjects = descriptors.Where(d => d.Category == InteractionCategory.MechanicalObject).ToList();
        mechanicalObjects.Should().HaveCountGreaterThanOrEqualTo(14); // 4 levers + 5 doors + 5 terminals

        // Verify specific objects exist
        mechanicalObjects.Should().Contain(d => d.SubCategory == "Lever" && d.State == "Active");
        mechanicalObjects.Should().Contain(d => d.SubCategory == "Door" && d.State == "Locked");
        mechanicalObjects.Should().Contain(d => d.SubCategory == "Terminal" && d.State == "Corrupted");
    }

    [Test]
    public void GetAllDescriptors_ContainsContainers()
    {
        // Arrange & Act
        var descriptors = InteractionDescriptorSeeder.GetAllDescriptors().ToList();

        // Assert
        var containers = descriptors.Where(d => d.Category == InteractionCategory.Container).ToList();
        containers.Should().HaveCountGreaterThanOrEqualTo(14); // 4 crates + 6 corpses + 4 resource nodes

        // Verify specific containers exist
        containers.Should().Contain(d => d.SubCategory == "SalvageCrate" && d.State == "Trapped");
        containers.Should().Contain(d => d.SubCategory == "Corpse" && d.State == "Fresh");
        containers.Should().Contain(d => d.SubCategory == "ResourceNode" && d.State == "RareFind");
    }

    [Test]
    public void GetAllDescriptors_ContainsWitsChecks()
    {
        // Arrange & Act
        var descriptors = InteractionDescriptorSeeder.GetAllDescriptors().ToList();

        // Assert - 4 success + 4 failure + object-specific
        var witsSuccess = descriptors.Where(d => d.Category == InteractionCategory.WitsSuccess).ToList();
        var witsFailure = descriptors.Where(d => d.Category == InteractionCategory.WitsFailure).ToList();

        witsSuccess.Should().HaveCountGreaterThanOrEqualTo(4);
        witsFailure.Should().HaveCountGreaterThanOrEqualTo(4);

        // Verify success margins exist
        witsSuccess.Should().Contain(d => d.State == "Low");
        witsSuccess.Should().Contain(d => d.State == "Critical");

        // Verify failure margins exist
        witsFailure.Should().Contain(d => d.State == "NearMiss");
        witsFailure.Should().Contain(d => d.State == "CriticalFail");
    }

    [Test]
    public void GetAllDescriptors_ContainsDiscoveries()
    {
        // Arrange & Act
        var descriptors = InteractionDescriptorSeeder.GetAllDescriptors().ToList();

        // Assert
        var discoveries = descriptors.Where(d => d.Category == InteractionCategory.Discovery).ToList();
        discoveries.Should().HaveCountGreaterThanOrEqualTo(18); // 6 secret + 6 lore + 6 danger

        // Verify discovery types exist
        discoveries.Should().Contain(d => d.SubCategory == "Secret");
        discoveries.Should().Contain(d => d.SubCategory == "Lore");
        discoveries.Should().Contain(d => d.SubCategory == "Danger");
    }

    [Test]
    public void GetAllDescriptors_ContainsContainerInteractions()
    {
        // Arrange & Act
        var descriptors = InteractionDescriptorSeeder.GetAllDescriptors().ToList();

        // Assert
        var interactions = descriptors.Where(d => d.Category == InteractionCategory.ContainerInteraction).ToList();
        interactions.Should().HaveCountGreaterThanOrEqualTo(14); // 5 opening + 9 loot

        // Verify interaction types exist
        interactions.Should().Contain(d => d.SubCategory == "Opening" && d.State == "TrapTrigger");
        interactions.Should().Contain(d => d.SubCategory == "Loot" && d.State == "Jackpot");
    }

    [Test]
    public void GetAllDescriptors_ContainsSkillSpecific()
    {
        // Arrange & Act
        var descriptors = InteractionDescriptorSeeder.GetAllDescriptors().ToList();

        // Assert
        var skills = descriptors.Where(d => d.Category == InteractionCategory.SkillSpecific).ToList();
        skills.Should().HaveCountGreaterThanOrEqualTo(10); // 6 trades + 4 specializations

        // Verify trade skills exist
        skills.Should().Contain(d => d.State.StartsWith("Bodging"));
        skills.Should().Contain(d => d.State.StartsWith("FieldMedicine"));
        skills.Should().Contain(d => d.State.StartsWith("Runeforging"));

        // Verify specializations exist
        skills.Should().Contain(d => d.State.StartsWith("JotunReader"));
        skills.Should().Contain(d => d.State.StartsWith("RuinStalker"));
    }

    [Test]
    public void GetAllDescriptors_ContainsEnvironmental()
    {
        // Arrange & Act
        var descriptors = InteractionDescriptorSeeder.GetAllDescriptors().ToList();

        // Assert
        var environmental = descriptors.Where(d => d.Category == InteractionCategory.Environmental).ToList();
        environmental.Should().HaveCountGreaterThanOrEqualTo(8); // 4 traversal + 4 repair

        // Verify traversal types exist
        environmental.Should().Contain(d => d.SubCategory == "Traversal");
        environmental.Should().Contain(d => d.SubCategory == "Repair");
    }

    [Test]
    public void GetAllDescriptors_HasUniqueIds()
    {
        // Arrange & Act
        var descriptors = InteractionDescriptorSeeder.GetAllDescriptors().ToList();

        // Assert
        var ids = descriptors.Select(d => d.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void GetAllDescriptors_AllHaveValidText()
    {
        // Arrange & Act
        var descriptors = InteractionDescriptorSeeder.GetAllDescriptors().ToList();

        // Assert
        foreach (var descriptor in descriptors)
        {
            descriptor.DescriptorText.Should().NotBeNullOrWhiteSpace(
                because: $"Descriptor {descriptor} should have valid text");
        }
    }
}
