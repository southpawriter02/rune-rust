using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Stores interaction text descriptors for objects, examination results,
/// discovery moments, and skill-specific interactions.
/// </summary>
public class InteractionDescriptor : IEntity
{
    public Guid Id { get; private set; }
    public InteractionCategory Category { get; private set; }
    public string SubCategory { get; private set; }
    public string State { get; private set; }
    public string DescriptorText { get; private set; }
    public Biome? BiomeAffinity { get; private set; }
    public int Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private InteractionDescriptor()
    {
        SubCategory = null!;
        State = null!;
        DescriptorText = null!;
    } // For EF Core

    public InteractionDescriptor(
        InteractionCategory category,
        string subCategory,
        string state,
        string descriptorText,
        Biome? biomeAffinity = null,
        int weight = 1)
    {
        if (string.IsNullOrWhiteSpace(subCategory))
            throw new ArgumentException("SubCategory cannot be empty", nameof(subCategory));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));
        if (string.IsNullOrWhiteSpace(descriptorText))
            throw new ArgumentException("Descriptor text cannot be empty", nameof(descriptorText));
        if (weight < 1)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be at least 1");

        Id = Guid.NewGuid();
        Category = category;
        SubCategory = subCategory;
        State = state;
        DescriptorText = descriptorText;
        BiomeAffinity = biomeAffinity;
        Weight = weight;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Constructor for seeding with known Id.
    /// </summary>
    public InteractionDescriptor(
        Guid id,
        InteractionCategory category,
        string subCategory,
        string state,
        string descriptorText,
        Biome? biomeAffinity = null,
        int weight = 1)
        : this(category, subCategory, state, descriptorText, biomeAffinity, weight)
    {
        Id = id;
    }

    // Factory methods for different descriptor types

    public static InteractionDescriptor CreateMechanicalObject(
        string objectType,
        string state,
        string descriptorText,
        Biome? biomeAffinity = null) =>
        new(InteractionCategory.MechanicalObject, objectType, state, descriptorText, biomeAffinity);

    public static InteractionDescriptor CreateContainer(
        string containerType,
        string state,
        string descriptorText,
        Biome? biomeAffinity = null) =>
        new(InteractionCategory.Container, containerType, state, descriptorText, biomeAffinity);

    public static InteractionDescriptor CreateWitsSuccess(
        string margin,
        string descriptorText) =>
        new(InteractionCategory.WitsSuccess, "WitsCheck", margin, descriptorText);

    public static InteractionDescriptor CreateWitsFailure(
        string severity,
        string descriptorText) =>
        new(InteractionCategory.WitsFailure, "WitsCheck", severity, descriptorText);

    public static InteractionDescriptor CreateDiscovery(
        string discoveryType,
        string subType,
        string descriptorText,
        Biome? biomeAffinity = null) =>
        new(InteractionCategory.Discovery, discoveryType, subType, descriptorText, biomeAffinity);

    public static InteractionDescriptor CreateContainerInteraction(
        string actionType,
        string subType,
        string descriptorText) =>
        new(InteractionCategory.ContainerInteraction, actionType, subType, descriptorText);

    public static InteractionDescriptor CreateSkillSpecific(
        string skillType,
        string skillName,
        string descriptorText) =>
        new(InteractionCategory.SkillSpecific, skillType, skillName, descriptorText);

    public static InteractionDescriptor CreateEnvironmental(
        string actionType,
        string result,
        string descriptorText) =>
        new(InteractionCategory.Environmental, actionType, result, descriptorText);

    public override string ToString() => $"{Category}/{SubCategory}/{State}";
}
