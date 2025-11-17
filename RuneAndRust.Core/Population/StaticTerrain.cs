using RuneAndRust.Core;

namespace RuneAndRust.Core.Population;

/// <summary>
/// Static terrain features in procedurally generated Sectors (v0.11)
/// Provides cover, blocks movement, or adds tactical complexity
/// </summary>
public class StaticTerrain
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TerrainId { get; set; } = string.Empty; // Alias for Core compatibility
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StaticTerrainType Type { get; set; } // Terrain type classification

    // Tactical Properties
    public bool BlocksMovement { get; set; } = false;
    public bool BlocksLineOfSight { get; set; } = false;
    public bool ProvidesTouchCover { get; set; } = false; // +2 DEF when adjacent
    public bool ProvidesFullCover { get; set; } = false; // Cannot be targeted

    // Destructibility
    public bool IsDestructible { get; set; } = false;
    public int HP { get; set; } = 0;

    // Positional Data
    public Vector2 Position { get; set; } = new Vector2(0, 0);

    // Coherent Glitch Context (v0.12)
    public bool IsFromCeilingCollapse { get; set; } = false; // Created by [Unstable Ceiling]
    public bool IsOrganized { get; set; } = false; // Arranged by Haugbui-Class

    // v0.37.1: Investigation and Search properties
    public string TerrainName => Name; // Alias for command system
    public string FlavorText => Description; // Alias for command system
    public bool IsInteractive { get; set; } = false;
    public int InvestigationDC { get; set; } = 2;
    public string? InvestigationSuccessText { get; set; } = null;
    public string? InvestigationFailureText { get; set; } = null;
    public bool HasBeenInvestigated { get; set; } = false;
    public bool IsSearchable { get; set; } = false;
    public bool HasBeenSearched { get; set; } = false;
    public bool ContainsLoot { get; set; } = false;
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
        Type = StaticTerrainType.RubblePile;
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
        Type = StaticTerrainType.CollapsedPillar;
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
        Type = StaticTerrainType.CollapseDebris; // Closest match for wreckage
    }

    public string? MachineryType { get; set; } = null; // "geothermal pump", "conveyor system"
}

/// <summary>
/// [Rusted Bulkhead] - Solid cover (v0.11)
/// </summary>
public class RustedBulkhead : StaticTerrain
{
    public RustedBulkhead()
    {
        Name = "Rusted Bulkhead";
        Description = "A corroded blast door provides solid cover despite centuries of decay.";
        BlocksMovement = true;
        BlocksLineOfSight = true;
        ProvidesFullCover = true;
        Type = StaticTerrainType.RustedBulkhead;
    }
}

/// <summary>
/// [Chasm] - Traversal hazard terrain (v0.11)
/// </summary>
public class ChasmTerrain : StaticTerrain
{
    public ChasmTerrain()
    {
        Name = "Chasm";
        Description = "A gaping hole in the floor plunges into darkness. The edges are unstable.";
        BlocksMovement = true;
        Type = StaticTerrainType.Chasm;
    }

    public bool RequiresSkillCheck { get; set; } = true;
    public int SkillCheckDC { get; set; } = 12;
}

/// <summary>
/// [Elevated Platform] - Tactical high ground (v0.11)
/// </summary>
public class ElevatedPlatform : StaticTerrain
{
    public ElevatedPlatform()
    {
        Name = "Elevated Platform";
        Description = "A raised maintenance walkway provides tactical high ground.";
        BlocksMovement = false;
        Type = StaticTerrainType.ElevatedPlatform;
    }

    public bool RequiresClimbing { get; set; } = true;
    public int ClimbDC { get; set; } = 10;
}
