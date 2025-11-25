using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.32.4: Service for generating Jötun corpse terrain - fallen Jötun-Forged as multi-level battlefields.
/// Creates extreme verticality and iconic environmental storytelling.
/// Dormant giant corpses create elevated platforms, interior cavities, and psychic stress zones.
/// </summary>
public class JotunCorpseTerrainService
{
    private static readonly ILogger _log = Log.ForContext<JotunCorpseTerrainService>();

    private readonly DiceService _diceService;
    private readonly TraumaEconomyService _traumaEconomyService;

    private const int PROXIMITY_STRESS_PER_TURN = 2;

    public JotunCorpseTerrainService(
        DiceService diceService,
        TraumaEconomyService traumaEconomyService)
    {
        _diceService = diceService;
        _traumaEconomyService = traumaEconomyService;

        _log.Information("JotunCorpseTerrainService initialized - fallen giant terrain system active");
    }

    #region Corpse Layout Generation

    /// <summary>
    /// Generate Jötun corpse terrain layout in room.
    /// Creates multi-level battlefield from fallen giant chassis.
    /// </summary>
    public void GenerateCorpseLayout(GridState grid, string? forcedCorpseType = null)
    {
        _log.Information("Generating Jötun corpse terrain layout (grid size: {Width}x{Height})",
            grid.Width, grid.Height);

        // Determine corpse type and orientation
        var corpseType = forcedCorpseType ?? RollCorpseType();
        var orientation = _diceService.Roll(0, 3) * 90; // 0, 90, 180, 270 degrees

        _log.Debug("Corpse type: {Type}, Orientation: {Orientation}°", corpseType, orientation);

        // Place corpse features based on type
        switch (corpseType)
        {
            case "Hull Section":
                PlaceHullSection(grid, orientation);
                break;
            case "Limb Bridge":
                PlaceLimbBridge(grid, orientation);
                break;
            case "Interior Cavity":
                PlaceInteriorCavity(grid);
                break;
            default:
                _log.Warning("Unknown corpse type: {Type}, defaulting to Hull Section", corpseType);
                PlaceHullSection(grid, orientation);
                break;
        }

        // Mark all corpse tiles with Stress effect
        MarkCorpseTiles(grid);

        _log.Information("Jötun corpse terrain generated successfully: Type={Type}, Tiles={Count}",
            corpseType, CountCorpseTiles(grid));
    }

    /// <summary>
    /// Roll random corpse type for procedural generation.
    /// </summary>
    private string RollCorpseType()
    {
        var roll = _diceService.Roll(1, 100);
        return roll switch
        {
            <= 40 => "Hull Section",    // 40% - Large flat platform
            <= 75 => "Limb Bridge",     // 35% - Elevated bridge
            _ => "Interior Cavity"      // 25% - Cave-like interior
        };
    }

    #endregion

    #region Hull Section

    /// <summary>
    /// Place hull section - creates large flat platform (outer surface of fallen giant).
    /// Dimensions: 4x6 elevated platform at +2 Z-level.
    /// </summary>
    private void PlaceHullSection(GridState grid, int orientation)
    {
        _log.Debug("Placing Hull Section (4x6 platform, elevation +2)");

        // Create 4x6 elevated platform (hull outer surface)
        int startX = Math.Max(1, grid.Width / 2 - 2);
        int startY = Math.Max(1, grid.Height / 2 - 3);

        int tilesPlaced = 0;

        for (int x = 0; x < 4 && (startX + x) < grid.Width - 1; x++)
        {
            for (int y = 0; y < 6 && (startY + y) < grid.Height - 1; y++)
            {
                var tile = grid.GetTile(startX + x, startY + y);
                if (tile != null && tile.GetBattlefieldTile().IsPassable())
                {
                    tile.AddTerrain("Jotun Corpse Terrain");
                    tile.SetMetadata("CorpseFeature", "Hull");
                    tile.SetMetadata("Elevation", 2); // +2 Z-level
                    tile.SetMetadata("CorpseType", "Hull Section");
                    tilesPlaced++;
                }
            }
        }

        _log.Information("Hull Section placed: {Count} tiles at elevation +2", tilesPlaced);
    }

    #endregion

    #region Limb Bridge

    /// <summary>
    /// Place limb bridge - creates elevated walkway (fallen arm/leg spanning room).
    /// Dimensions: 2-tile-wide bridge across room at +3 Z-level.
    /// </summary>
    private void PlaceLimbBridge(GridState grid, int orientation)
    {
        _log.Debug("Placing Limb Bridge (2-tile-wide bridge, elevation +3)");

        // Create 2-tile-wide bridge across room
        int midY = grid.Height / 2;
        int tilesPlaced = 0;

        for (int x = 2; x < grid.Width - 2; x++)
        {
            for (int dy = -1; dy <= 0; dy++)
            {
                var yPos = midY + dy;
                if (yPos >= 1 && yPos < grid.Height - 1)
                {
                    var tile = grid.GetTile(x, yPos);
                    if (tile != null && tile.GetBattlefieldTile().IsPassable())
                    {
                        tile.AddTerrain("Jotun Corpse Terrain");
                        tile.SetMetadata("CorpseFeature", "Limb");
                        tile.SetMetadata("Elevation", 3); // +3 Z-level (elevated bridge)
                        tile.SetMetadata("CorpseType", "Limb Bridge");
                        tilesPlaced++;
                    }
                }
            }
        }

        _log.Information("Limb Bridge placed: {Count} tiles at elevation +3", tilesPlaced);
    }

    #endregion

    #region Interior Cavity

    /// <summary>
    /// Place interior cavity - creates cave-like space inside corpse chassis.
    /// Dimensions: 3x4 enclosed area at ground level.
    /// </summary>
    private void PlaceInteriorCavity(GridState grid)
    {
        _log.Debug("Placing Interior Cavity (3x4 enclosed area, ground level)");

        // Create enclosed space (3x4 area) - inside the giant's torso
        int centerX = grid.Width / 2;
        int centerY = grid.Height / 2;
        int tilesPlaced = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -2; y <= 1; y++)
            {
                var xPos = centerX + x;
                var yPos = centerY + y;

                if (xPos >= 1 && xPos < grid.Width - 1 && yPos >= 1 && yPos < grid.Height - 1)
                {
                    var tile = grid.GetTile(xPos, yPos);
                    if (tile != null && tile.GetBattlefieldTile().IsPassable())
                    {
                        tile.AddTerrain("Jotun Corpse Terrain");
                        tile.SetMetadata("CorpseFeature", "Interior");
                        tile.SetMetadata("Elevation", 0); // Ground level, but enclosed
                        tile.SetMetadata("CorpseType", "Interior Cavity");
                        tile.SetMetadata("IsEnclosed", true);
                        tilesPlaced++;
                    }
                }
            }
        }

        _log.Information("Interior Cavity placed: {Count} tiles (enclosed interior)", tilesPlaced);
    }

    #endregion

    #region Stress Effects

    /// <summary>
    /// Mark all corpse tiles and apply passive Stress effect metadata.
    /// Characters on these tiles gain +2 Psychic Stress per turn.
    /// </summary>
    private void MarkCorpseTiles(GridState grid)
    {
        int markedCount = 0;

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                var tile = grid.Tiles[x, y];
                if (tile.HasTerrain("Jotun Corpse Terrain"))
                {
                    tile.SetMetadata("AppliesProximityStress", true);
                    tile.SetMetadata("StressPerTurn", PROXIMITY_STRESS_PER_TURN);
                    tile.SetMetadata("StressSource", "Jötun proximity - corrupted logic core broadcast");
                    markedCount++;
                }
            }
        }

        _log.Information("Marked {Count} tiles as Jötun corpse terrain (applies +{Stress} Psychic Stress/turn)",
            markedCount, PROXIMITY_STRESS_PER_TURN);
    }

    /// <summary>
    /// Apply passive Psychic Stress to character on Jötun corpse terrain.
    /// Called by JotunheimBiomeService during turn processing.
    /// </summary>
    public void ApplyCorpseProximityStress(PlayerCharacter character, GridTile tile)
    {
        if (!tile.HasTerrain("Jotun Corpse Terrain"))
        {
            return;
        }

        var stressAmount = tile.GetMetadata<int>("StressPerTurn");
        var stressSource = tile.GetMetadata<string>("StressSource") ?? "Jötun proximity";

        _traumaEconomyService.AddStress(character, stressAmount, stressSource);

        _log.Debug("{Character} on Jötun corpse terrain - applied +{Stress} Psychic Stress",
            character.Name, stressAmount);
    }

    #endregion

    #region Utility

    /// <summary>
    /// Count tiles with Jötun corpse terrain in grid.
    /// </summary>
    private int CountCorpseTiles(GridState grid)
    {
        int count = 0;
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                if (grid.Tiles[x, y].HasTerrain("Jotun Corpse Terrain"))
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// Get corpse terrain type at tile (Hull Section, Limb Bridge, Interior Cavity).
    /// Returns null if tile is not Jötun corpse terrain.
    /// </summary>
    public string? GetCorpseTerrainType(GridTile tile)
    {
        if (!tile.HasTerrain("Jotun Corpse Terrain"))
        {
            return null;
        }

        return tile.GetMetadata<string>("CorpseType");
    }

    /// <summary>
    /// Get elevation of corpse terrain tile.
    /// Returns 0 for Interior Cavity, 2 for Hull Section, 3 for Limb Bridge.
    /// </summary>
    public int GetCorpseTerrainElevation(GridTile tile)
    {
        if (!tile.HasTerrain("Jotun Corpse Terrain"))
        {
            return 0;
        }

        return tile.GetMetadata<int>("Elevation");
    }

    /// <summary>
    /// Check if tile is inside Jötun corpse (Interior Cavity type).
    /// </summary>
    public bool IsInsideCorpse(GridTile tile)
    {
        if (!tile.HasTerrain("Jotun Corpse Terrain"))
        {
            return false;
        }

        var corpseFeature = tile.GetMetadata<string>("CorpseFeature");
        return corpseFeature == "Interior";
    }

    /// <summary>
    /// Get count of corpse terrain tiles in grid.
    /// </summary>
    public int GetCorpseTerrainCount(GridState grid)
    {
        return CountCorpseTiles(grid);
    }

    /// <summary>
    /// Generate corpse terrain report for battlefield.
    /// </summary>
    public JotunCorpseTerrainReport GenerateTerrainReport(GridState grid)
    {
        int totalCorpseTiles = 0;
        int hullSectionTiles = 0;
        int limbBridgeTiles = 0;
        int interiorCavityTiles = 0;
        int totalElevation = 0;

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                var tile = grid.Tiles[x, y];
                if (tile.HasTerrain("Jotun Corpse Terrain"))
                {
                    totalCorpseTiles++;
                    totalElevation += tile.GetMetadata<int>("Elevation");

                    var corpseFeature = tile.GetMetadata<string>("CorpseFeature");
                    if (corpseFeature == "Hull")
                        hullSectionTiles++;
                    else if (corpseFeature == "Limb")
                        limbBridgeTiles++;
                    else if (corpseFeature == "Interior")
                        interiorCavityTiles++;
                }
            }
        }

        var report = new JotunCorpseTerrainReport
        {
            TotalCorpseTiles = totalCorpseTiles,
            HullSectionTiles = hullSectionTiles,
            LimbBridgeTiles = limbBridgeTiles,
            InteriorCavityTiles = interiorCavityTiles,
            AverageElevation = totalCorpseTiles > 0 ? (double)totalElevation / totalCorpseTiles : 0,
            HasCorpseTerrain = totalCorpseTiles > 0
        };

        _log.Debug("Corpse terrain report: {Report}", report);

        return report;
    }

    #endregion
}

#region Data Transfer Objects

/// <summary>
/// Report of Jötun corpse terrain status on battlefield.
/// </summary>
public class JotunCorpseTerrainReport
{
    public int TotalCorpseTiles { get; set; }
    public int HullSectionTiles { get; set; }
    public int LimbBridgeTiles { get; set; }
    public int InteriorCavityTiles { get; set; }
    public double AverageElevation { get; set; }
    public bool HasCorpseTerrain { get; set; }

    public override string ToString()
    {
        if (!HasCorpseTerrain)
        {
            return "No Jötun corpse terrain present";
        }

        return $"Jötun Corpse Terrain: {TotalCorpseTiles} tiles total " +
               $"(Hull: {HullSectionTiles}, Limb: {LimbBridgeTiles}, Interior: {InteriorCavityTiles}), " +
               $"Avg Elevation: {AverageElevation:F1}";
    }
}

#endregion
