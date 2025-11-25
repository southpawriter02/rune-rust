using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using RuneAndRust.Engine.Spatial;
using Serilog;
using System.Diagnostics;
using PopulationRoomArchetype = RuneAndRust.Core.Population.RoomArchetype;

namespace RuneAndRust.Engine;

/// <summary>
/// Generates procedural dungeon layouts using graph-based algorithm (v0.10)
/// v0.11: Integrated with PopulationPipeline for enemy/hazard/loot spawning
/// v0.39.1: Integrated with SpatialLayoutService for 3D coordinate assignment
/// v0.39.2: Integrated with BiomeTransitionService for multi-biome sectors
/// v0.39.3: Integrated with ContentDensityService for balanced population
/// v0.39.4: Full integration of all v0.39 components with comprehensive testing
/// </summary>
public class DungeonGenerator
{
    private static readonly ILogger _log = Log.ForContext<DungeonGenerator>();
    private readonly TemplateLibrary _templateLibrary;

    // v0.11 services
    private readonly PopulationPipeline? _populationPipeline;
    private readonly AnchorInserter? _anchorInserter;

    // v0.39.1 services (3D Spatial)
    private readonly ISpatialLayoutService? _spatialLayoutService;
    private readonly ISpatialValidationService? _spatialValidationService;
    private readonly IVerticalTraversalService? _verticalTraversalService;

    // v0.39.2 services (Biome Transitions)
    private readonly BiomeTransitionService? _biomeTransitionService;
    private readonly BiomeBlendingService? _biomeBlendingService;
    private readonly EnvironmentalGradientService? _gradientService;

    // v0.39.3 services (Content Density)
    private readonly ContentDensityService? _contentDensityService;
    private readonly DensityClassificationService? _densityClassificationService;
    private readonly BudgetDistributionService? _budgetDistributionService;
    private readonly ThreatHeatmapService? _heatmapService;

    private Random _rng = null!;
    private int _seed;
    private BiomeDefinition? _currentBiome;

    public DungeonGenerator(
        TemplateLibrary templateLibrary,
        PopulationPipeline? populationPipeline = null,
        AnchorInserter? anchorInserter = null,
        ISpatialLayoutService? spatialLayoutService = null,
        ISpatialValidationService? spatialValidationService = null,
        IVerticalTraversalService? verticalTraversalService = null,
        BiomeTransitionService? biomeTransitionService = null,
        BiomeBlendingService? biomeBlendingService = null,
        EnvironmentalGradientService? gradientService = null,
        ContentDensityService? contentDensityService = null,
        DensityClassificationService? densityClassificationService = null,
        BudgetDistributionService? budgetDistributionService = null,
        ThreatHeatmapService? heatmapService = null)
    {
        _templateLibrary = templateLibrary;
        _populationPipeline = populationPipeline;
        _anchorInserter = anchorInserter;
        _spatialLayoutService = spatialLayoutService;
        _spatialValidationService = spatialValidationService;
        _verticalTraversalService = verticalTraversalService;
        _biomeTransitionService = biomeTransitionService;
        _biomeBlendingService = biomeBlendingService;
        _gradientService = gradientService;
        _contentDensityService = contentDensityService;
        _densityClassificationService = densityClassificationService;
        _budgetDistributionService = budgetDistributionService;
        _heatmapService = heatmapService;
    }

    /// <summary>
    /// Generates a complete dungeon graph from a seed
    /// </summary>
    public DungeonGraph Generate(int seed, int targetRoomCount = 7, BiomeDefinition? biome = null)
    {
        _seed = seed;
        _rng = new Random(seed);
        _currentBiome = biome;

        // Use biome parameters if provided
        if (biome != null)
        {
            targetRoomCount = _rng.Next(biome.MinRoomCount, biome.MaxRoomCount + 1);
            _log.Information("Generating dungeon: Seed={Seed}, Biome={Biome}, RoomCount={RoomCount}",
                seed, biome.Name, targetRoomCount);
        }
        else
        {
            _log.Information("Generating dungeon: Seed={Seed}, TargetRoomCount={TargetRoomCount} (no biome)",
                seed, targetRoomCount);
        }

        // Step 1: Create graph structure
        var graph = new DungeonGraph();

        // Step 2: Generate main path
        _log.Debug("Generating main path...");
        GenerateMainPath(graph, targetRoomCount);

        // Step 3: Add branching paths (optional)
        float branchProb = biome?.BranchingProbability ?? 0.6f;
        int branchCount = _rng.NextDouble() < branchProb ? _rng.Next(1, 3) : 0;
        if (branchCount > 0)
        {
            _log.Debug("Adding {BranchCount} branching paths...", branchCount);
            AddBranchingPaths(graph, branchCount);
        }

        // Step 4: Add secret rooms (optional)
        float secretProb = biome?.SecretRoomProbability ?? 0.3f;
        int secretCount = _rng.NextDouble() < secretProb ? 1 : 0;
        if (secretCount > 0)
        {
            _log.Debug("Adding {SecretCount} secret rooms...", secretCount);
            AddSecretRooms(graph, secretCount);
        }

        // Step 5: Calculate node depths
        CalculateNodeDepths(graph);

        // Step 6: Assign directions to edges
        _log.Debug("Assigning directions to edges...");
        var directionAssigner = new DirectionAssigner();
        directionAssigner.AssignDirections(graph, _rng);

        // Step 7: Validate connectivity
        var (isValid, errors) = graph.Validate();
        if (!isValid)
        {
            _log.Error("Dungeon generation failed validation: {Errors}", string.Join(", ", errors));
            throw new InvalidOperationException($"Dungeon generation failed: {string.Join(", ", errors)}");
        }

        var stats = graph.GetStatistics();
        _log.Information("Dungeon generated successfully: Seed={Seed}, Nodes={Nodes}, Edges={Edges}, Branches={Branches}, Secrets={Secrets}",
            seed, stats["TotalNodes"], stats["TotalEdges"], stats["BranchNodes"], stats["SecretNodes"]);

        return graph;
    }

    /// <summary>
    /// Generates a complete playable dungeon (graph + instantiated rooms) from a seed
    /// v0.11: Now includes population (enemies, hazards, terrain, loot, conditions)
    /// v0.39.1: Now includes 3D spatial layout with vertical connections
    /// </summary>
    public Dungeon GenerateComplete(int seed, int dungeonId = 1, int targetRoomCount = 7, BiomeDefinition? biome = null)
    {
        // Step 1: Generate graph
        var graph = Generate(seed, targetRoomCount, biome);

        // Step 2 (v0.39.1): Convert graph to 3D spatial layout
        Dictionary<string, Core.Spatial.RoomPosition>? positions = null;
        List<Core.Spatial.VerticalConnection>? verticalConnections = null;

        if (_spatialLayoutService != null)
        {
            _log.Information("Converting graph to 3D spatial layout...");
            positions = _spatialLayoutService.ConvertGraphTo3DLayout(graph, seed);

            // Validate no overlaps
            var templates = graph.GetNodes().ToDictionary(
                n => n.Id.ToString(),
                n => n.Template);
            var noOverlaps = _spatialLayoutService.ValidateNoOverlaps(positions, templates);

            if (!noOverlaps)
            {
                _log.Warning("Room overlaps detected in spatial layout - proceeding anyway");
            }

            // Generate vertical connections
            verticalConnections = _spatialLayoutService.GenerateVerticalConnections(positions, _rng);

            _log.Information("3D layout complete: {RoomCount} rooms positioned, {ConnectionCount} vertical connections",
                positions.Count, verticalConnections.Count);

            // Run spatial validation
            if (_spatialValidationService != null)
            {
                var validationIssues = _spatialValidationService.ValidateSector(positions, verticalConnections, graph);

                var criticalIssues = validationIssues.Where(i => i.Severity == "Critical").ToList();
                if (criticalIssues.Any())
                {
                    _log.Error("Critical spatial validation issues detected: {IssueCount}", criticalIssues.Count);
                    foreach (var issue in criticalIssues)
                    {
                        _log.Error("  - {Issue}", issue.Description);
                    }
                    throw new InvalidOperationException($"Spatial validation failed with {criticalIssues.Count} critical issues");
                }

                var warnings = validationIssues.Where(i => i.Severity == "Warning").ToList();
                if (warnings.Any())
                {
                    _log.Warning("Spatial validation warnings: {WarningCount}", warnings.Count);
                }
            }
        }
        else
        {
            _log.Debug("Spatial layout service not available (v0.10 mode)");
        }

        // Step 3: Instantiate rooms
        _log.Debug("Instantiating rooms...");
        var instantiator = new RoomInstantiator();
        var dungeon = instantiator.Instantiate(graph, dungeonId, seed, positions, verticalConnections);

        // Set biome on dungeon
        if (biome != null)
        {
            dungeon.Biome = biome.BiomeId;
        }

        // Step 4: v0.11 - Populate rooms with enemies, hazards, terrain, loot, conditions
        if (_populationPipeline != null && biome != null)
        {
            _log.Information("Populating dungeon with v0.11 population pipeline...");
            _populationPipeline.PopulateDungeon(dungeon, biome, _rng);
        }
        else if (_populationPipeline == null)
        {
            _log.Debug("Population pipeline not available (v0.10 mode)");
        }

        _log.Information("Complete dungeon generated: DungeonId={DungeonId}, Seed={Seed}, Biome={Biome}, Rooms={RoomCount}",
            dungeonId, seed, dungeon.Biome, dungeon.TotalRoomCount);

        return dungeon;
    }

    /// <summary>
    /// v0.11: Generates a complete dungeon from a DungeonBlueprint (with Quest Anchor support)
    /// </summary>
    public Dungeon GenerateFromBlueprint(DungeonBlueprint blueprint, int dungeonId, BiomeDefinition biome)
    {
        // Step 1: Validate blueprint
        var (isValid, errors) = blueprint.Validate();
        if (!isValid)
        {
            _log.Error("Blueprint validation failed: {Errors}", string.Join(", ", errors));
            throw new ArgumentException($"Invalid blueprint: {string.Join(", ", errors)}");
        }

        _seed = blueprint.Seed;
        _rng = new Random(blueprint.Seed);
        _currentBiome = biome;

        _log.Information("Generating dungeon from blueprint: Seed={Seed}, TargetRooms={RoomCount}, Anchors={AnchorCount}",
            blueprint.Seed, blueprint.TargetRoomCount, blueprint.RequiredAnchors.Count);

        // Step 2: Generate base graph
        var graph = Generate(blueprint.Seed, blueprint.TargetRoomCount, biome);

        // Step 3: Insert Quest Anchors (if anchor inserter available)
        if (_anchorInserter != null && blueprint.RequiredAnchors.Count > 0)
        {
            _log.Information("Inserting {AnchorCount} Quest Anchors into graph", blueprint.RequiredAnchors.Count);
            _anchorInserter.InsertAnchors(graph, blueprint, _rng);
        }
        else if (blueprint.RequiredAnchors.Count > 0)
        {
            _log.Warning("Blueprint has {AnchorCount} Quest Anchors but no AnchorInserter provided - anchors will be skipped",
                blueprint.RequiredAnchors.Count);
        }

        // Step 4: Instantiate rooms
        _log.Debug("Instantiating rooms...");
        var instantiator = new RoomInstantiator();
        var dungeon = instantiator.Instantiate(graph, dungeonId, blueprint.Seed);
        dungeon.Biome = biome.BiomeId;

        // Step 5: Populate rooms (skip handcrafted Quest Anchor rooms)
        if (_populationPipeline != null)
        {
            _log.Information("Populating dungeon with v0.11 population pipeline...");
            _populationPipeline.PopulateDungeon(dungeon, biome, _rng);
        }

        _log.Information("Complete dungeon generated from blueprint: DungeonId={DungeonId}, Seed={Seed}, Rooms={RoomCount}, Anchors={AnchorCount}",
            dungeonId, blueprint.Seed, dungeon.TotalRoomCount, blueprint.RequiredAnchors.Count);

        return dungeon;
    }

    /// <summary>
    /// v0.39.4: Full integrated pipeline with all v0.39 components
    /// Generates a complete dungeon using the 6-phase pipeline:
    /// 1. Layout Generation (v0.10)
    /// 2. 3D Spatial Layout (v0.39.1)
    /// 3. Biome Transitions (v0.39.2)
    /// 4. Content Density (v0.39.3)
    /// 5. Population (v0.11 modified)
    /// 6. Validation & Finalization
    /// </summary>
    public Dungeon GenerateWithFullPipeline(
        int seed,
        int dungeonId,
        int targetRoomCount = 7,
        BiomeDefinition? biome = null,
        List<BiomeDefinition>? additionalBiomes = null)
    {
        var stopwatch = Stopwatch.StartNew();
        _seed = seed;
        _rng = new Random(seed);
        _currentBiome = biome;

        var biomeIds = new List<string>();
        if (biome != null) biomeIds.Add(biome.BiomeId);
        if (additionalBiomes != null) biomeIds.AddRange(additionalBiomes.Select(b => b.BiomeId));

        _log.Information(
            "Sector generation started: Seed={Seed}, Biome={Biome}, AdditionalBiomes={Additional}, Difficulty={Difficulty}, TargetRooms={Rooms}",
            seed, biome?.BiomeId ?? "none", additionalBiomes?.Count ?? 0, DifficultyTier.Normal, targetRoomCount);

        try
        {
            // ===== PHASE 1: LAYOUT GENERATION (v0.10) =====
            _log.Debug("PHASE 1: Generating graph layout...");
            var graph = Generate(seed, targetRoomCount, biome);
            _log.Information("Graph generated: {NodeCount} nodes", graph.NodeCount);

            // ===== PHASE 2: 3D SPATIAL LAYOUT (v0.39.1) =====
            _log.Debug("PHASE 2: Converting to 3D spatial layout...");
            Dictionary<string, Core.Spatial.RoomPosition>? positions = null;
            List<Core.Spatial.VerticalConnection>? verticalConnections = null;

            if (_spatialLayoutService != null)
            {
                positions = _spatialLayoutService.ConvertGraphTo3DLayout(graph, seed);

                // Validate no overlaps
                var templates = graph.GetNodes().ToDictionary(
                    n => n.Id.ToString(),
                    n => n.Template);
                var noOverlaps = _spatialLayoutService.ValidateNoOverlaps(positions, templates);

                if (!noOverlaps)
                {
                    _log.Warning("Room overlaps detected in spatial layout - attempting to resolve");
                }

                // Generate vertical connections
                verticalConnections = _spatialLayoutService.GenerateVerticalConnections(positions, _rng);

                _log.Information("3D layout complete: {RoomCount} rooms positioned, {ConnectionCount} vertical connections",
                    positions.Count, verticalConnections.Count);

                // Run spatial validation
                if (_spatialValidationService != null)
                {
                    var validationIssues = _spatialValidationService.ValidateSector(positions, verticalConnections, graph);

                    var criticalIssues = validationIssues.Where(i => i.Severity == "Critical").ToList();
                    if (criticalIssues.Any())
                    {
                        _log.Error("Critical spatial validation issues detected: {IssueCount}", criticalIssues.Count);
                        foreach (var issue in criticalIssues)
                        {
                            _log.Error("  - {Issue}", issue.Description);
                        }
                        throw new InvalidOperationException($"Spatial validation failed with {criticalIssues.Count} critical issues");
                    }

                    var warnings = validationIssues.Where(i => i.Severity == "Warning").ToList();
                    if (warnings.Any())
                    {
                        _log.Warning("Spatial validation warnings: {WarningCount}", warnings.Count);
                    }
                }
            }
            else
            {
                _log.Debug("Spatial layout service not available (v0.10 mode)");
            }

            // ===== PHASE 3: INSTANTIATE ROOMS =====
            _log.Debug("PHASE 3: Instantiating rooms...");
            var instantiator = new RoomInstantiator();
            var dungeon = instantiator.Instantiate(graph, dungeonId, seed, positions, verticalConnections);

            // Set biome on dungeon
            if (biome != null)
            {
                dungeon.Biome = biome.BiomeId;
            }

            // ===== PHASE 4: BIOME TRANSITIONS (v0.39.2) =====
            if (additionalBiomes != null && additionalBiomes.Any() &&
                _biomeTransitionService != null &&
                biome != null)
            {
                _log.Debug("PHASE 4: Applying biome transitions...");
                ApplyBiomeTransitions(dungeon, biome, additionalBiomes, _rng);
                _log.Information("Biome transitions applied: {BiomeCount} biomes", additionalBiomes.Count + 1);
            }
            else
            {
                _log.Debug("PHASE 4: Skipped (single biome or service not available)");
            }

            // ===== PHASE 5: CONTENT DENSITY (v0.39.3) =====
            if (_contentDensityService != null &&
                _densityClassificationService != null &&
                _budgetDistributionService != null &&
                biome != null)
            {
                _log.Debug("PHASE 5: Calculating content density budgets...");

                var globalBudget = _contentDensityService.CalculateGlobalBudget(
                    dungeon.TotalRoomCount,
                    DifficultyTier.Normal,
                    biome.BiomeId);

                var roomsList = dungeon.Rooms.Values.ToList();
                var densityMap = _densityClassificationService.ClassifyRooms(roomsList, _rng);

                var populationPlan = _budgetDistributionService.DistributeBudget(
                    globalBudget, densityMap, _rng);

                _log.Information(
                    "Population plan created: {Enemies} enemies, {Hazards} hazards allocated across {Rooms} rooms",
                    populationPlan.TotalEnemiesAllocated,
                    populationPlan.TotalHazardsAllocated,
                    dungeon.TotalRoomCount);

                // ===== PHASE 6: POPULATION (v0.11 modified with budgets) =====
                _log.Debug("PHASE 6: Populating rooms with budget constraints...");
                PopulateRoomsWithBudgets(dungeon, populationPlan, biome, _rng);

                // ===== PHASE 7: VALIDATION & FINALIZATION =====
                _log.Debug("PHASE 7: Final validation...");
                if (_heatmapService != null)
                {
                    var heatmap = _heatmapService.GenerateHeatmap(dungeon, populationPlan);
                    _heatmapService.LogHeatmapStatistics(heatmap, dungeonId.ToString());
                }

                ValidateFinalDungeon(dungeon);
            }
            else if (_populationPipeline != null && biome != null)
            {
                // Fallback to old population pipeline if v0.39.3 services not available
                _log.Information("Content density services not available, using legacy population pipeline");
                _populationPipeline.PopulateDungeon(dungeon, biome, _rng);
            }
            else
            {
                _log.Debug("Population skipped (no services available)");
            }

            stopwatch.Stop();
            _log.Information(
                "Sector generation complete: Seed={Seed}, Time={Duration}ms, Rooms={RoomCount}, Pipeline=v0.39.4-full",
                seed, stopwatch.ElapsedMilliseconds, dungeon.TotalRoomCount);

            return dungeon;
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Sector generation failed: Seed={Seed}, Error={Error}",
                seed, ex.Message);
            throw;
        }
    }

    #region v0.39.2: Biome Transition Methods

    /// <summary>
    /// Applies biome transitions to a dungeon with multiple biomes
    /// </summary>
    private void ApplyBiomeTransitions(
        Dungeon dungeon,
        BiomeDefinition primaryBiome,
        List<BiomeDefinition> additionalBiomes,
        Random rng)
    {
        if (_biomeTransitionService == null)
        {
            _log.Warning("BiomeTransitionService not available, skipping transitions");
            return;
        }

        // For now, simple implementation: blend primary with first additional biome
        // More sophisticated multi-biome blending can be added later
        if (additionalBiomes.Count > 0)
        {
            var secondaryBiome = additionalBiomes[0];

            // Check compatibility
            if (!_biomeTransitionService.CanBiomesBeAdjacent(primaryBiome.BiomeId, secondaryBiome.BiomeId))
            {
                _log.Error("Biomes {BiomeA} and {BiomeB} are incompatible, skipping transition",
                    primaryBiome.BiomeId, secondaryBiome.BiomeId);
                return;
            }

            // Determine transition room count (use 20-30% of rooms for transition)
            var transitionCount = Math.Max(1, dungeon.TotalRoomCount / 4);
            transitionCount = _biomeTransitionService.GetOptimalTransitionCount(
                primaryBiome.BiomeId, secondaryBiome.BiomeId, rng);

            _log.Information("Creating {Count} transition rooms between {BiomeA} and {BiomeB}",
                transitionCount, primaryBiome.BiomeId, secondaryBiome.BiomeId);

            // Apply blending to rooms in the middle section
            var roomsList = dungeon.Rooms.Values.OrderBy(r => ((Core.Spatial.RoomPosition?)r.Position)?.X ?? 0).ToList();
            var startIndex = roomsList.Count / 2 - transitionCount / 2;
            var endIndex = startIndex + transitionCount;

            for (int i = startIndex; i < endIndex && i < roomsList.Count; i++)
            {
                var room = roomsList[i];
                var progress = (float)(i - startIndex) / transitionCount;

                room.SecondaryBiome = secondaryBiome.BiomeId;
                room.BiomeBlendRatio = progress;

                // Apply gradients if available
                if (_gradientService != null)
                {
                    _gradientService.ApplyGradients(room, primaryBiome.BiomeId, secondaryBiome.BiomeId, progress);
                }

                _log.Debug("Applied biome blend to room {RoomId}: {Blend:P0} {SecondaryBiome}",
                    room.RoomId, progress, secondaryBiome.BiomeId);
            }
        }
    }

    #endregion

    #region v0.39.3: Budget-Based Population Methods

    /// <summary>
    /// Populates rooms using budget allocations from the population plan
    /// Integrates with existing v0.11 spawners which respect allocated budgets
    /// </summary>
    private void PopulateRoomsWithBudgets(
        Dungeon dungeon,
        SectorPopulationPlan plan,
        BiomeDefinition biome,
        Random rng)
    {
        // Step 1: Set allocated budgets on all rooms
        foreach (var room in dungeon.Rooms.Values)
        {
            if (plan.RoomAllocations.TryGetValue(room.RoomId, out var allocation))
            {
                room.AllocatedEnemyBudget = allocation.AllocatedEnemies;
                room.AllocatedHazardBudget = allocation.AllocatedHazards;
                room.AllocatedLootBudget = allocation.AllocatedLoot;
                room.DensityClassification = allocation.Density;

                _log.Debug(
                    "Room {RoomId} budgets set: Enemies={Enemies}, Hazards={Hazards}, Loot={Loot}, Density={Density}",
                    room.RoomId, allocation.AllocatedEnemies, allocation.AllocatedHazards,
                    allocation.AllocatedLoot, allocation.Density);
            }
            else
            {
                _log.Debug("Room {RoomId} has no allocation, will remain empty", room.RoomId);
            }
        }

        // Step 2: Use PopulationPipeline if available (it respects allocated budgets)
        if (_populationPipeline != null)
        {
            _log.Information("Using PopulationPipeline with v0.39.3 budget constraints");

            foreach (var room in dungeon.Rooms.Values)
            {
                if (room.IsHandcrafted)
                {
                    _log.Debug("Skipping handcrafted room {RoomId}", room.RoomId);
                    continue;
                }

                // PopulateRoom will check AllocatedEnemyBudget, AllocatedHazardBudget, etc.
                _populationPipeline.PopulateRoom(room, biome, rng);
            }
        }
        else if (biome.Elements != null)
        {
            // Fallback: Use biome element tables directly
            _log.Information("PopulationPipeline not available, using biome element tables directly");

            foreach (var room in dungeon.Rooms.Values)
            {
                if (!plan.RoomAllocations.TryGetValue(room.RoomId, out var allocation))
                    continue;

                SpawnEnemiesFromBiomeElements(room, allocation.AllocatedEnemies, biome, rng);
                SpawnHazardsFromBiomeElements(room, allocation.AllocatedHazards, biome, rng);
                SpawnLootFromBiomeElements(room, allocation.AllocatedLoot, biome, rng);
            }
        }
        else
        {
            // Last resort: Use placeholder spawning for testing without full biome data
            _log.Warning("Neither PopulationPipeline nor BiomeElements available, using placeholder spawning");

            foreach (var room in dungeon.Rooms.Values)
            {
                if (!plan.RoomAllocations.TryGetValue(room.RoomId, out var allocation))
                    continue;

                SpawnPlaceholderContent(room, allocation, biome, rng);
            }
        }
    }

    /// <summary>
    /// Spawns enemies using biome element tables (fallback when PopulationPipeline unavailable)
    /// </summary>
    private void SpawnEnemiesFromBiomeElements(Room room, int budget, BiomeDefinition biome, Random rng)
    {
        if (budget <= 0 || biome.Elements == null) return;

        var availableEnemies = biome.Elements.GetEligibleElements(
            BiomeElementType.DormantProcess, room, rng);

        if (availableEnemies.Count == 0)
        {
            _log.Debug("No eligible enemies for room {RoomId}", room.RoomId);
            return;
        }

        int spawnedCount = 0;
        while (budget > 0 && availableEnemies.Count > 0)
        {
            var selected = biome.Elements.WeightedRandomSelection(availableEnemies, rng);
            if (selected == null) break;

            if (selected.SpawnCost > budget)
            {
                availableEnemies = availableEnemies.Where(e => e.SpawnCost <= budget).ToList();
                continue;
            }

            // Create simplified enemy spawn
            room.Enemies.Add(new Enemy
            {
                EnemyID = int.Parse((selected.AssociatedDataId ?? "0")),
                Name = selected.ElementName,
                Level = 1,
                HP = 50,
                MaxHP = 50
            });

            budget -= selected.SpawnCost;
            spawnedCount++;
        }

        _log.Debug("Spawned {Count} enemies in room {RoomId}", spawnedCount, room.RoomId);
    }

    /// <summary>
    /// Spawns hazards using biome element tables (fallback when PopulationPipeline unavailable)
    /// </summary>
    private void SpawnHazardsFromBiomeElements(Room room, int count, BiomeDefinition biome, Random rng)
    {
        if (count <= 0 || biome.Elements == null) return;

        var availableHazards = biome.Elements.GetEligibleElements(
            BiomeElementType.DynamicHazard, room, rng);

        if (availableHazards.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            var selected = biome.Elements.WeightedRandomSelection(availableHazards, rng);
            if (selected == null) break;

            room.Hazards.Add(new Core.Population.HazardSpawn
            {
                HazardId = selected.AssociatedDataId ?? selected.ElementName
            });

            // Remove to avoid duplicates
            availableHazards = availableHazards.Where(h => h.ElementName != selected.ElementName).ToList();
        }
    }

    /// <summary>
    /// Spawns loot using biome element tables (fallback when PopulationPipeline unavailable)
    /// </summary>
    private void SpawnLootFromBiomeElements(Room room, int count, BiomeDefinition biome, Random rng)
    {
        if (count <= 0 || biome.Elements == null) return;

        var availableLoot = biome.Elements.GetEligibleElements(
            BiomeElementType.LootNode, room, rng);

        if (availableLoot.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            var selected = biome.Elements.WeightedRandomSelection(availableLoot, rng);
            if (selected == null) break;

            room.Loot.Add(new Core.Population.ResourceVein
            {
                Id = selected.AssociatedDataId ?? selected.ElementName,
                Quality = LootQuality.Common // Could be enhanced based on element data
            });
        }
    }

    /// <summary>
    /// Placeholder spawning for testing without full biome data
    /// Only used when neither PopulationPipeline nor BiomeElements are available
    /// </summary>
    private void SpawnPlaceholderContent(Room room, RoomAllocation allocation, BiomeDefinition biome, Random rng)
    {
        for (int i = 0; i < allocation.AllocatedEnemies; i++)
        {
            room.Enemies.Add(new Enemy
            {
                EnemyID = i,
                Name = $"placeholder_enemy_{biome.BiomeId}_{i}",
                Level = 1,
                HP = 50,
                MaxHP = 50
            });
        }

        for (int i = 0; i < allocation.AllocatedHazards; i++)
        {
            room.Hazards.Add(new Core.Population.HazardSpawn
            {
                HazardId = $"placeholder_hazard_{biome.BiomeId}_{i}"
            });
        }

        for (int i = 0; i < allocation.AllocatedLoot; i++)
        {
            room.Loot.Add(new Core.Population.ResourceVein
            {
                Id = $"placeholder_loot_{biome.BiomeId}_{i}",
                Quality = LootQuality.Common
            });
        }
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates the final dungeon for quality assurance
    /// </summary>
    private void ValidateFinalDungeon(Dungeon dungeon)
    {
        var issues = new List<string>();

        // Check that all rooms have content or are intentionally empty
        foreach (var room in dungeon.Rooms.Values)
        {
            var hasContent = room.Enemies.Any() || room.Hazards.Any() || room.Loot.Any();
            var isBreatherRoom = room.Archetype == PopulationRoomArchetype.EntryHall ||
                                room.Archetype == PopulationRoomArchetype.SecretRoom;

            if (!hasContent && !isBreatherRoom && room.IsBossRoom == false)
            {
                _log.Debug("Room {RoomId} is empty (may be breather room)", room.RoomId);
            }
        }

        // Check that boss room exists and has content
        var bossRoom = dungeon.Rooms.Values.FirstOrDefault(r => r.IsBossRoom);
        if (bossRoom != null)
        {
            if (bossRoom.Enemies.Count == 0)
            {
                issues.Add("Boss room has no enemies");
            }
        }
        else
        {
            issues.Add("No boss room found");
        }

        if (issues.Any())
        {
            _log.Warning("Dungeon validation issues: {Issues}", string.Join(", ", issues));
        }
        else
        {
            _log.Information("Dungeon validation passed");
        }
    }

    #endregion

    #region Main Path Generation

    /// <summary>
    /// Generates the main path: Start -> N rooms -> Boss
    /// </summary>
    private void GenerateMainPath(DungeonGraph graph, int targetRoomCount)
    {
        // Create start node (Entry Hall)
        var startTemplate = SelectTemplateByArchetype(Core.RoomArchetype.EntryHall);
        var startNode = CreateNode(startTemplate, NodeType.Start, "Start Room");
        graph.AddNode(startNode);

        // Generate intermediate rooms (targetRoomCount - 2, excluding start and boss)
        var currentNode = startNode;
        int intermediateRoomCount = Math.Max(3, targetRoomCount - 2);

        for (int i = 0; i < intermediateRoomCount; i++)
        {
            var nextTemplate = SelectNextTemplate(currentNode, graph);
            var nextNode = CreateNode(nextTemplate, NodeType.Main, $"Main Path Room {i + 1}");
            graph.AddNode(nextNode);
            graph.AddEdge(currentNode, nextNode);
            currentNode = nextNode;

            _log.Debug("Added main path node: {NodeId} ({TemplateId})", nextNode.Id, nextTemplate.TemplateId);
        }

        // Create boss node
        var bossTemplate = SelectTemplateByArchetype(Core.RoomArchetype.BossArena);
        var bossNode = CreateNode(bossTemplate, NodeType.Boss, "Boss Room");
        graph.AddNode(bossNode);
        graph.AddEdge(currentNode, bossNode);

        _log.Debug("Main path complete: {RoomCount} rooms", graph.NodeCount);
    }

    #endregion

    #region Branching Paths

    /// <summary>
    /// Adds branching paths that split from and optionally rejoin the main path
    /// </summary>
    private void AddBranchingPaths(DungeonGraph graph, int branchCount)
    {
        for (int i = 0; i < branchCount; i++)
        {
            // Pick random node on main path (not start or boss) as branch point
            var mainNodes = graph.GetMainPathNodes()
                .Where(n => n.Type != NodeType.Start && n.Type != NodeType.Boss)
                .ToList();

            if (mainNodes.Count == 0)
            {
                _log.Warning("Cannot add branch: no suitable branch points");
                continue;
            }

            var branchPoint = mainNodes[_rng.Next(mainNodes.Count)];

            // Create 1-2 room branch
            int branchLength = _rng.Next(1, 3);
            var branchNodes = new List<DungeonNode>();

            DungeonNode? previousNode = branchPoint;
            for (int j = 0; j < branchLength; j++)
            {
                var template = SelectNextTemplate(previousNode, graph);
                var branchNode = CreateNode(template, NodeType.Branch, $"Branch {i + 1} Room {j + 1}");
                graph.AddNode(branchNode);
                graph.AddEdge(previousNode, branchNode);
                branchNodes.Add(branchNode);
                previousNode = branchNode;

                _log.Debug("Added branch node: {NodeId} ({TemplateId}) from {BranchPoint}",
                    branchNode.Id, template.TemplateId, branchPoint.Id);
            }

            // 50% chance: branch rejoins main path (creates loop)
            // 50% chance: branch is dead end (optional exploration)
            if (_rng.NextDouble() < 0.5)
            {
                var rejoinPoint = FindRejoinPoint(graph, branchPoint);
                if (rejoinPoint != null && branchNodes.Count > 0)
                {
                    graph.AddEdge(branchNodes[^1], rejoinPoint);
                    _log.Debug("Branch rejoins at node {RejoinId}", rejoinPoint.Id);
                }
            }
            else
            {
                _log.Debug("Branch is dead-end (optional exploration)");
            }
        }
    }

    /// <summary>
    /// Finds a suitable rejoin point on the main path after the branch point
    /// </summary>
    private DungeonNode? FindRejoinPoint(DungeonGraph graph, DungeonNode branchPoint)
    {
        var mainPath = graph.GetMainPath();
        if (mainPath == null) return null;

        // Find branch point index in main path
        int branchIndex = mainPath.IndexOf(branchPoint);
        if (branchIndex == -1 || branchIndex >= mainPath.Count - 2)
            return null;

        // Rejoin at a later point on the main path (at least 2 nodes ahead)
        var eligibleRejoinPoints = mainPath
            .Skip(branchIndex + 2)
            .Where(n => n.Type != NodeType.Boss)
            .ToList();

        if (eligibleRejoinPoints.Count == 0)
            return null;

        return eligibleRejoinPoints[_rng.Next(eligibleRejoinPoints.Count)];
    }

    #endregion

    #region Secret Rooms

    /// <summary>
    /// Adds secret rooms connected via hidden passages
    /// </summary>
    private void AddSecretRooms(DungeonGraph graph, int secretCount)
    {
        for (int i = 0; i < secretCount; i++)
        {
            // Pick random non-boss room as parent
            var eligibleParents = graph.GetNodes()
                .Where(n => n.Type != NodeType.Boss && n.Type != NodeType.Secret)
                .ToList();

            if (eligibleParents.Count == 0)
            {
                _log.Warning("Cannot add secret room: no suitable parent nodes");
                continue;
            }

            var parentNode = eligibleParents[_rng.Next(eligibleParents.Count)];

            // Create secret room
            var template = SelectTemplateByArchetype(Core.RoomArchetype.SecretRoom);
            var secretNode = CreateNode(template, NodeType.Secret, $"Secret Room {i + 1}");
            graph.AddNode(secretNode);

            // Connect with SECRET edge type
            graph.AddEdge(parentNode, secretNode, EdgeType.Secret);

            _log.Debug("Added secret room: {NodeId} ({TemplateId}) connected to {ParentId}",
                secretNode.Id, template.TemplateId, parentNode.Id);
        }
    }

    #endregion

    #region Template Selection

    /// <summary>
    /// Selects the next template based on current node and graph state
    /// </summary>
    private RoomTemplate SelectNextTemplate(DungeonNode currentNode, DungeonGraph graph)
    {
        // Get valid next archetypes based on current template's connection rules
        var validArchetypes = currentNode.Template.ValidConnections;

        // Filter by what hasn't been used recently (avoid repetition)
        var recentTemplates = GetRecentTemplates(graph, windowSize: 3);
        var availableTemplates = new List<RoomTemplate>();

        foreach (var archetype in validArchetypes)
        {
            // Skip BossArena (only used explicitly for final room)
            if (archetype == Core.RoomArchetype.BossArena)
                continue;

            var templates = _templateLibrary.GetTemplatesByArchetype(archetype);

            // Prefer templates not recently used
            var freshTemplates = templates
                .Where(t => !recentTemplates.Contains(t.TemplateId))
                .ToList();

            availableTemplates.AddRange(freshTemplates.Count > 0 ? freshTemplates : templates);
        }

        // Fallback: if no valid templates, use any chamber/corridor
        if (availableTemplates.Count == 0)
        {
            _log.Warning("No valid templates found, falling back to Corridor");
            availableTemplates.AddRange(_templateLibrary.GetTemplatesByArchetype(Core.RoomArchetype.Corridor));
        }

        if (availableTemplates.Count == 0)
        {
            throw new InvalidOperationException("Template library has no valid templates for generation");
        }

        // Weight by difficulty curve (future enhancement - for now just random)
        return availableTemplates[_rng.Next(availableTemplates.Count)];
    }

    /// <summary>
    /// Selects a template by archetype (for start/boss rooms)
    /// </summary>
    private RoomTemplate SelectTemplateByArchetype(Core.RoomArchetype archetype)
    {
        var template = _templateLibrary.GetRandomTemplate(_rng, archetype);

        if (template == null)
        {
            throw new InvalidOperationException($"No templates found for archetype: {archetype}");
        }

        return template;
    }

    /// <summary>
    /// Gets template IDs used in the last N nodes
    /// </summary>
    private HashSet<string> GetRecentTemplates(DungeonGraph graph, int windowSize)
    {
        var nodes = graph.GetNodes();
        return nodes
            .TakeLast(windowSize)
            .Select(n => n.Template.TemplateId)
            .ToHashSet();
    }

    #endregion

    #region Node Creation & Utilities

    /// <summary>
    /// Creates a dungeon node from a template
    /// </summary>
    private DungeonNode CreateNode(RoomTemplate template, NodeType type, string name)
    {
        return new DungeonNode
        {
            Template = template,
            Type = type,
            Name = name,
            Depth = 0 // Will be calculated later
        };
    }

    /// <summary>
    /// Calculates depth (distance from start) for all nodes using BFS
    /// </summary>
    private void CalculateNodeDepths(DungeonGraph graph)
    {
        var startNode = graph.StartNode;
        if (startNode == null) return;

        var visited = new HashSet<DungeonNode>();
        var queue = new Queue<(DungeonNode Node, int Depth)>();

        queue.Enqueue((startNode, 0));
        visited.Add(startNode);
        startNode.Depth = 0;

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();

            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    neighbor.Depth = depth + 1;
                    queue.Enqueue((neighbor, depth + 1));
                }
            }
        }

        _log.Debug("Node depths calculated. Max depth: {MaxDepth}",
            graph.GetNodes().Max(n => n.Depth));
    }

    #endregion
}
