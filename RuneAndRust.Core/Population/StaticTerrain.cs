namespace RuneAndRust.Core.Population;

/// <summary>
/// Static terrain features in procedurally generated Sectors (v0.11)
/// Provides cover, blocks movement, or adds tactical complexity
/// </summary>
public class StaticTerrain
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Tactical Properties
    public bool BlocksMovement { get; set; } = false;
    public bool BlocksLineOfSight { get; set; } = false;
    public bool ProvidesTouchCover { get; set; } = false; // +2 DEF when adjacent
    public bool ProvidesFullCover { get; set; } = false; // Cannot be targeted

    // Positional Data
    public Vector2 Position { get; set; } = new Vector2(0, 0);

    // Coherent Glitch Context (v0.12)
    public bool IsFromCeilingCollapse { get; set; } = false; // Created by [Unstable Ceiling]
    public bool IsOrganized { get; set; } = false; // Arranged by Haugbui-Class
}

/// <summary>
/// [Rubble Pile] - Common debris terrain (v0.11)
/// </summary>
public class RubblePile : StaticTerrain
{
    public RubblePile()
    {
        Name = "Rubble Pile";
        Description = "Chunks of corroded metal and concrete litter the floor.";
        BlocksMovement = false; // Difficult terrain, not impassable
        ProvidesTouchCover = true; // +2 DEF
    }
}

/// <summary>
/// [Corroded Pillar] - Large structural terrain (v0.11)
/// </summary>
public class CorrodedPillar : StaticTerrain
{
    public CorrodedPillar()
    {
        Name = "Corroded Pillar";
        Description = "A massive support pillar, pitted with rust and decay.";
        BlocksMovement = false;
        BlocksLineOfSight = true;
        ProvidesFullCover = true;
    }
}

/// <summary>
/// [Machinery Wreckage] - Cover and flavor (v0.11)
/// </summary>
public class MachineryWreckage : StaticTerrain
{
    public MachineryWreckage()
    {
        Name = "Machinery Wreckage";
        Description = "The twisted remains of ancient industrial equipment.";
        BlocksMovement = false;
        ProvidesTouchCover = true;
    }

    public string? MachineryType { get; set; } = null; // "geothermal pump", "conveyor system"
}
