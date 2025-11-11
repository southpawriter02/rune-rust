namespace RuneAndRust.Core;

/// <summary>
/// Types of crafting components for Field Medicine
/// </summary>
public enum ComponentType
{
    // Field Medicine Components (Bone-Setter specialty)
    CommonHerb,      // Basic medicinal plants
    CleanCloth,      // Sterile bandages, cloth strips
    Suture,          // Thread, wire for stitching
    Antiseptic,      // Alcohol, cleaning agents
    Splint,          // Wood, metal rods for setting bones
    Stimulant        // Caffeine, adrenaline compounds (rare)
}

/// <summary>
/// Represents a crafting component used in recipes
/// </summary>
public class CraftingComponent
{
    public ComponentType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Creates a component by type
    /// </summary>
    public static CraftingComponent Create(ComponentType type)
    {
        return type switch
        {
            ComponentType.CommonHerb => new CraftingComponent
            {
                Type = ComponentType.CommonHerb,
                Name = "Common Herb",
                Description = "Medicinal plants salvaged from ruins"
            },
            ComponentType.CleanCloth => new CraftingComponent
            {
                Type = ComponentType.CleanCloth,
                Name = "Clean Cloth",
                Description = "Sterilized bandages and cloth strips"
            },
            ComponentType.Suture => new CraftingComponent
            {
                Type = ComponentType.Suture,
                Name = "Suture",
                Description = "Thread and wire for stitching wounds"
            },
            ComponentType.Antiseptic => new CraftingComponent
            {
                Type = ComponentType.Antiseptic,
                Name = "Antiseptic",
                Description = "Alcohol and cleaning agents"
            },
            ComponentType.Splint => new CraftingComponent
            {
                Type = ComponentType.Splint,
                Name = "Splint Material",
                Description = "Wood or metal rods for setting bones"
            },
            ComponentType.Stimulant => new CraftingComponent
            {
                Type = ComponentType.Stimulant,
                Name = "Stimulant",
                Description = "Rare adrenaline compounds"
            },
            _ => throw new ArgumentException($"Unknown component type: {type}")
        };
    }
}
