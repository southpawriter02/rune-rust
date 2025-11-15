using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.29.4: Orchestration service for Muspelheim biome.
/// Coordinates generation, hazards, enemies, and heat mechanics.
/// </summary>
public class MuspelheimBiomeService
{
    private static readonly ILogger _log = Log.ForContext<MuspelheimBiomeService>();

    private readonly MuspelheimDataRepository _dataRepository;
    private readonly IntenseHeatService _heatService;
    private readonly BrittlenessService _brittlenessService;
    private readonly DiceService _diceService;

    public MuspelheimBiomeService(
        MuspelheimDataRepository dataRepository,
        IntenseHeatService heatService,
        BrittlenessService brittlenessService,
        DiceService diceService)
    {
        _dataRepository = dataRepository;
        _heatService = heatService;
        _brittlenessService = brittlenessService;
        _diceService = diceService;

        _log.Information("MuspelheimBiomeService initialized");
    }

    #region Initialization

    /// <summary>
    /// Check if party is prepared for Muspelheim.
    /// Logs warnings if Fire Resistance is insufficient.
    /// </summary>
    public MuspelheimPreparednessReport CheckPartyPreparedness(List<PlayerCharacter> party)
    {
        _log.Information("Checking party preparedness for Muspelheim ({Count} characters)", party.Count);

        var report = new MuspelheimPreparednessReport
        {
            Characters = new List<CharacterPreparedness>()
        };

        foreach (var character in party)
        {
            // Note: GetFireResistancePercent() is a placeholder until full resistance system
            int fireResistance = 0; // TODO: Implement actual resistance lookup

            var charPrep = new CharacterPreparedness
            {
                CharacterName = character.Name,
                FireResistancePercent = fireResistance,
                Sturdiness = character.Attributes.Sturdiness,
                IsAdequatelyPrepared = fireResistance >= 50,
                RecommendedFireResistance = 50
            };

            if (fireResistance < 25)
            {
                charPrep.WarningLevel = "Critical";
                charPrep.WarningMessage = $"{character.Name} has CRITICAL lack of Fire Resistance ({fireResistance}%). Expect frequent deaths from [Intense Heat].";
                _log.Warning("{Character}: CRITICAL Fire Resistance ({Percent}%)", character.Name, fireResistance);
            }
            else if (fireResistance < 50)
            {
                charPrep.WarningLevel = "Warning";
                charPrep.WarningMessage = $"{character.Name} has low Fire Resistance ({fireResistance}%). Consider acquiring heat-resistant equipment.";
                _log.Warning("{Character}: Low Fire Resistance ({Percent}%)", character.Name, fireResistance);
            }
            else
            {
                charPrep.WarningLevel = "Adequate";
                charPrep.WarningMessage = $"{character.Name} is prepared for Muspelheim ({fireResistance}% Fire Resistance).";
                _log.Information("{Character}: Adequate Fire Resistance ({Percent}%)", character.Name, fireResistance);
            }

            report.Characters.Add(charPrep);
        }

        report.AverageFireResistance = report.Characters.Average(c => c.FireResistancePercent);
        report.PartyIsAdequatelyPrepared = report.AverageFireResistance >= 40;

        _log.Information("Party preparedness check complete: Avg Fire Resistance={Avg}%, Adequate={IsAdequate}",
            report.AverageFireResistance, report.PartyIsAdequatelyPrepared);

        return report;
    }

    #endregion

    #region Enemy Resistance Loading

    /// <summary>
    /// Load Fire/Ice resistances for a Muspelheim enemy into BrittlenessService.
    /// Called when enemy is spawned.
    /// </summary>
    public void LoadEnemyResistances(Enemy enemy, MuspelheimEnemySpawn spawnData)
    {
        var resistances = _dataRepository.GetEnemyResistances(spawnData.SpawnRulesJson);

        foreach (var kvp in resistances)
        {
            _brittlenessService.SetEnemyResistance(enemy.EnemyID, kvp.Key, kvp.Value);
            _log.Debug("Set {Enemy} resistance: {Type} = {Percent}%",
                enemy.Name, kvp.Key, kvp.Value);
        }

        // Check if enemy is Brittle-eligible
        bool eligible = _brittlenessService.IsBrittleEligible(enemy);
        if (eligible)
        {
            _log.Information("{Enemy} is [Brittle]-eligible (Fire Resistance > 0%)", enemy.Name);
        }
    }

    /// <summary>
    /// Create an enemy from spawn data.
    /// </summary>
    public Enemy CreateEnemyFromSpawn(MuspelheimEnemySpawn spawnData, int level)
    {
        // Basic enemy creation (simplified)
        var enemy = new Enemy
        {
            EnemyID = GenerateEnemyId(),
            Name = spawnData.EnemyName,
            Level = level,
            HP = CalculateEnemyHP(spawnData.EnemyType, level),
            MaxHP = CalculateEnemyHP(spawnData.EnemyType, level),
            StatusEffects = new List<StatusEffect>()
        };

        // Load resistances
        LoadEnemyResistances(enemy, spawnData);

        _log.Information("Created enemy: {Name} (Level {Level}, HP {HP})",
            enemy.Name, enemy.Level, enemy.HP);

        return enemy;
    }

    private int CalculateEnemyHP(string enemyType, int level)
    {
        // Simplified HP calculation
        int baseHP = enemyType switch
        {
            "Boss" => 200,
            "Construct" => 80,
            "Humanoid" => 60,
            "Undying" => 70,
            _ => 50
        };

        return baseHP + (level * 10);
    }

    private static int _nextEnemyId = 5000; // Start Muspelheim enemies at 5000
    private int GenerateEnemyId()
    {
        return System.Threading.Interlocked.Increment(ref _nextEnemyId);
    }

    #endregion

    #region Enemy Selection

    /// <summary>
    /// Select a random enemy based on spawn weights.
    /// </summary>
    public MuspelheimEnemySpawn SelectWeightedEnemy(List<MuspelheimEnemySpawn> eligibleEnemies, Random random)
    {
        int totalWeight = eligibleEnemies.Sum(e => e.SpawnWeight);
        int roll = random.Next(totalWeight);

        int cumulativeWeight = 0;
        foreach (var enemy in eligibleEnemies)
        {
            cumulativeWeight += enemy.SpawnWeight;
            if (roll < cumulativeWeight)
            {
                _log.Debug("Selected enemy: {Name} (weight {Weight}/{Total}, roll {Roll})",
                    enemy.EnemyName, enemy.SpawnWeight, totalWeight, roll);
                return enemy;
            }
        }

        // Fallback (should not happen)
        return eligibleEnemies.Last();
    }

    #endregion

    #region Heat Damage Integration

    /// <summary>
    /// Process [Intense Heat] damage for all characters at end of turn.
    /// Integrates with IntenseHeatService.
    /// </summary>
    public List<HeatDamageResult> ProcessEndOfTurnHeat(List<PlayerCharacter> characters)
    {
        _log.Information("Processing [Intense Heat] for {Count} characters", characters.Count);

        var results = new List<HeatDamageResult>();

        foreach (var character in characters)
        {
            var result = ProcessHeatForCharacter(character);
            results.Add(result);
        }

        int totalDamage = results.Sum(r => r.DamageDealt);
        int charactersAffected = results.Count(r => r.DamageDealt > 0);

        _log.Information("[Intense Heat] processing complete: {Affected}/{Total} characters took damage, total {TotalDamage} damage",
            charactersAffected, characters.Count, totalDamage);

        return results;
    }

    private HeatDamageResult ProcessHeatForCharacter(PlayerCharacter character)
    {
        var result = new HeatDamageResult
        {
            CharacterName = character.Name,
            SturdynessValue = character.Attributes.Sturdiness
        };

        // Make STURDINESS check (DC 12)
        var rollResult = _diceService.Roll(character.Attributes.Sturdiness);
        result.SuccessesRolled = rollResult.Successes;
        result.CheckPassed = rollResult.Successes >= 12;

        if (result.CheckPassed)
        {
            // Success: No damage
            result.DamageDealt = 0;
            result.Message = $"✓ {character.Name} resists [Intense Heat] (STURDINESS check passed)";

            _log.Information("{Character} passed [Intense Heat] check: {Successes} successes (DC 12)",
                character.Name, rollResult.Successes);
        }
        else
        {
            // Failure: Roll 2d6 Fire damage
            int rawDamage = _diceService.RollDamage(2, 6);

            // Apply Fire Resistance (placeholder - actual resistance system not yet implemented)
            int finalDamage = rawDamage; // TODO: Apply Fire Resistance

            // Apply damage
            character.HP = Math.Max(0, character.HP - finalDamage);

            result.DamageDealt = finalDamage;
            result.RawDamageRolled = rawDamage;
            result.Message = $"✗ {character.Name} fails [Intense Heat] check ({rollResult.Successes} successes)\n" +
                           $"   🔥 The searing heat overwhelms your defenses!\n" +
                           $"   Damage: 2d6 = {rawDamage}\n" +
                           $"   HP: {character.HP}/{character.MaxHP}";

            _log.Information("{Character} failed [Intense Heat] check: {Successes} successes, took {Damage} Fire damage (HP: {HP}/{MaxHP})",
                character.Name, rollResult.Successes, finalDamage, character.HP, character.MaxHP);

            // Check for death
            if (character.HP <= 0)
            {
                result.CharacterDied = true;
                result.Message += $"\n💀 {character.Name} has succumbed to the [Intense Heat]!";
                _log.Warning("{Character} died from [Intense Heat]", character.Name);
            }
        }

        return result;
    }

    #endregion

    #region Brittleness Integration

    /// <summary>
    /// Apply Ice damage to enemy and check for [Brittle] application.
    /// </summary>
    public BrittlenessResult ApplyIceDamageToEnemy(Enemy enemy, int iceDamage, string sourceName)
    {
        _log.Information("Applying {Damage} Ice damage to {Enemy} from {Source}",
            iceDamage, enemy.Name, sourceName);

        var result = new BrittlenessResult
        {
            EnemyName = enemy.Name,
            IceDamageDealt = iceDamage
        };

        // Apply damage to enemy
        int damageAfterResistance = ApplyIceResistance(enemy, iceDamage);
        enemy.HP = Math.Max(0, enemy.HP - damageAfterResistance);

        result.FinalDamageDealt = damageAfterResistance;

        _log.Information("{Enemy} took {FinalDamage} Ice damage ({RawDamage} before resistance, HP: {HP}/{MaxHP})",
            enemy.Name, damageAfterResistance, iceDamage, enemy.HP, enemy.MaxHP);

        // Try to apply [Brittle]
        _brittlenessService.TryApplyBrittle(enemy, damageAfterResistance);

        // Check if [Brittle] was applied
        bool hasBrittle = enemy.StatusEffects.Any(s =>
            s.EffectType.Equals("Brittle", StringComparison.OrdinalIgnoreCase) &&
            s.DurationRemaining > 0);

        result.BrittleApplied = hasBrittle;

        if (hasBrittle)
        {
            result.Message = _brittlenessService.GetBrittleApplicationMessage(enemy);
            _log.Information("{Enemy} is now [Brittle] for 2 turns", enemy.Name);
        }
        else
        {
            result.Message = $"{enemy.Name} is not eligible for [Brittle] (no Fire Resistance)";
        }

        return result;
    }

    /// <summary>
    /// Apply Physical damage to enemy, with [Brittle] bonus if applicable.
    /// </summary>
    public PhysicalDamageResult ApplyPhysicalDamageToEnemy(Enemy enemy, int physicalDamage, string sourceName)
    {
        _log.Information("Applying {Damage} Physical damage to {Enemy} from {Source}",
            physicalDamage, enemy.Name, sourceName);

        var result = new PhysicalDamageResult
        {
            EnemyName = enemy.Name,
            BaseDamage = physicalDamage
        };

        // Check for [Brittle] bonus
        int finalDamage = _brittlenessService.ApplyBrittleBonus(enemy, physicalDamage);
        result.FinalDamage = finalDamage;
        result.BrittleBonusApplied = finalDamage > physicalDamage;

        if (result.BrittleBonusApplied)
        {
            int bonusDamage = finalDamage - physicalDamage;
            result.Message = _brittlenessService.GetBrittleBonusMessage(enemy, bonusDamage);
            _log.Information("[Brittle] bonus: {Base} Physical damage → {Final} (+{Bonus})",
                physicalDamage, finalDamage, bonusDamage);
        }

        // Apply damage
        enemy.HP = Math.Max(0, enemy.HP - finalDamage);

        _log.Information("{Enemy} took {FinalDamage} Physical damage (HP: {HP}/{MaxHP})",
            enemy.Name, finalDamage, enemy.HP, enemy.MaxHP);

        result.EnemyDefeated = enemy.HP <= 0;

        return result;
    }

    private int ApplyIceResistance(Enemy enemy, int iceDamage)
    {
        int iceResistance = _brittlenessService.GetEnemyResistance(enemy.EnemyID, "Ice");

        if (iceResistance == 0)
        {
            return iceDamage;
        }

        // Negative resistance = vulnerability (takes MORE damage)
        if (iceResistance < 0)
        {
            int vulnerabilityPercent = Math.Abs(iceResistance);
            int bonusDamage = iceDamage * vulnerabilityPercent / 100;
            int totalDamage = iceDamage + bonusDamage;

            _log.Information("Ice Vulnerability {Percent}%: {RawDamage} → {TotalDamage} (+{Bonus})",
                vulnerabilityPercent, iceDamage, totalDamage, bonusDamage);

            return totalDamage;
        }

        // Positive resistance = reduces damage
        int reducedDamage = iceDamage - (iceDamage * iceResistance / 100);
        _log.Information("Ice Resistance {Percent}%: {RawDamage} → {ReducedDamage}",
            iceResistance, iceDamage, reducedDamage);

        return Math.Max(0, reducedDamage);
    }

    #endregion

    #region Procedural Generation

    /// <summary>
    /// Generate a complete Muspelheim sector using WFC constraints.
    /// </summary>
    public MuspelheimSector GenerateMuspelheimSector(int sectorDepth, int? seed = null)
    {
        using (_log.BeginTimedOperation("Generating Muspelheim sector at depth {Depth}", sectorDepth))
        {
            var random = seed.HasValue ? new Random(seed.Value) : new Random();

            _log.Information("Starting Muspelheim sector generation: Depth={Depth}, Seed={Seed}",
                sectorDepth, seed ?? -1);

            // Load room templates from database
            var templates = _dataRepository.GetRoomTemplates();
            _log.Information("Loaded {Count} room templates from database", templates.Count);

            // Generate room graph using WFC
            var roomGraph = GenerateRoomGraph(templates, sectorDepth, random);
            _log.Information("Generated room graph: {RoomCount} rooms", roomGraph.Rooms.Count);

            // Create Room instances from templates
            var rooms = new List<MuspelheimRoom>();
            foreach (var node in roomGraph.Rooms)
            {
                var room = CreateRoomFromTemplate(node.Template, node.Position, random);
                rooms.Add(room);
            }

            // Place hazards in each room
            foreach (var room in rooms)
            {
                PlaceHazardsInRoom(room, random);
            }

            // Place enemies in each room
            foreach (var room in rooms)
            {
                PlaceEnemiesInRoom(room, sectorDepth, random);
            }

            // Place resource nodes
            foreach (var room in rooms)
            {
                PlaceResourceNodes(room, random);
            }

            // Connect rooms
            ConnectRooms(rooms, roomGraph);

            var sector = new MuspelheimSector
            {
                Rooms = rooms,
                Depth = sectorDepth,
                EntranceRoomId = rooms.First(r => r.IsEntrance).RoomId,
                ExitRoomId = rooms.First(r => r.IsExit).RoomId,
                GeneratedAt = DateTime.UtcNow,
                Seed = seed ?? random.Next()
            };

            int totalHazards = rooms.Sum(r => r.Hazards.Count);
            int totalEnemies = rooms.Sum(r => r.Enemies.Count);
            int totalResources = rooms.Sum(r => r.ResourceNodes.Count);

            _log.Information(
                "Muspelheim sector generation complete: {RoomCount} rooms, {HazardCount} hazards, {EnemyCount} enemies, {ResourceCount} resources",
                sector.Rooms.Count, totalHazards, totalEnemies, totalResources);

            return sector;
        }
    }

    /// <summary>
    /// Generate room connectivity graph using WFC constraints.
    /// </summary>
    private MuspelheimRoomGraph GenerateRoomGraph(
        List<MuspelheimRoomTemplate> templates,
        int sectorDepth,
        Random random)
    {
        // Determine room count (6-12 rooms based on depth)
        int baseRoomCount = 6 + (sectorDepth / 2); // Increases with depth
        int variability = random.Next(-2, 3);
        int totalRooms = Math.Clamp(baseRoomCount + variability, 6, 12);

        _log.Information("Generating room graph with {Count} rooms for depth {Depth}",
            totalRooms, sectorDepth);

        var graph = new MuspelheimRoomGraph();

        // Step 1: Select entrance room
        var entranceTemplates = templates.Where(t => t.CanBeEntrance).ToList();
        if (!entranceTemplates.Any())
        {
            throw new InvalidOperationException("No entrance templates available");
        }

        var entranceTemplate = entranceTemplates[random.Next(entranceTemplates.Count)];
        var entranceNode = new MuspelheimRoomNode
        {
            Template = entranceTemplate,
            Position = new Vector2Int(0, 0),
            IsEntrance = true,
            RoomId = GenerateRoomId()
        };
        graph.Rooms.Add(entranceNode);

        _log.Debug("Selected entrance: {RoomName}", entranceTemplate.TemplateName);

        // Step 2: Use WFC to place remaining rooms
        var queue = new Queue<MuspelheimRoomNode>();
        queue.Enqueue(entranceNode);

        while (queue.Count > 0 && graph.Rooms.Count < totalRooms)
        {
            var currentNode = queue.Dequeue();

            // Determine connections needed
            int connectionsNeeded = random.Next(
                currentNode.Template.MinConnections,
                currentNode.Template.MaxConnections + 1
            );

            for (int i = 0; i < connectionsNeeded && graph.Rooms.Count < totalRooms; i++)
            {
                // Get eligible templates based on WFC adjacency rules
                var eligibleTemplates = GetEligibleAdjacentTemplates(
                    currentNode.Template,
                    templates,
                    graph
                );

                if (!eligibleTemplates.Any())
                {
                    _log.Debug("No eligible templates for adjacency, skipping connection");
                    continue;
                }

                var nextTemplate = SelectWeightedTemplate(eligibleTemplates, random);
                var nextPosition = GetAdjacentPosition(currentNode.Position, graph, random);

                var nextNode = new MuspelheimRoomNode
                {
                    Template = nextTemplate,
                    Position = nextPosition,
                    RoomId = GenerateRoomId()
                };

                graph.Rooms.Add(nextNode);
                graph.Connections.Add((currentNode.RoomId, nextNode.RoomId));

                _log.Debug("Connected {FromRoom} → {ToRoom}",
                    currentNode.Template.TemplateName, nextTemplate.TemplateName);

                queue.Enqueue(nextNode);
            }
        }

        // Step 3: Mark exit room
        var exitCandidates = graph.Rooms
            .Where(r => r.Template.CanBeExit && !r.IsEntrance)
            .ToList();

        if (exitCandidates.Any())
        {
            var exitNode = exitCandidates[random.Next(exitCandidates.Count)];
            exitNode.IsExit = true;
            _log.Debug("Selected exit: {RoomName}", exitNode.Template.TemplateName);
        }
        else
        {
            // Fallback: Use last room as exit
            var lastRoom = graph.Rooms.Last();
            lastRoom.IsExit = true;
            _log.Warning("No explicit exit candidates, using last room: {RoomName}",
                lastRoom.Template.TemplateName);
        }

        // Step 4: Identify boss room (Containment Breach Zone)
        var bossRoom = graph.Rooms.FirstOrDefault(r =>
            r.Template.TemplateName == "Containment Breach Zone");
        if (bossRoom != null)
        {
            bossRoom.IsBossRoom = true;
            _log.Information("Boss room identified: Containment Breach Zone");
        }

        _log.Information(
            "Room graph generated: {RoomCount} rooms, {ConnectionCount} connections",
            graph.Rooms.Count, graph.Connections.Count);

        return graph;
    }

    /// <summary>
    /// Get templates that can be adjacent based on WFC rules.
    /// </summary>
    private List<MuspelheimRoomTemplate> GetEligibleAdjacentTemplates(
        MuspelheimRoomTemplate currentTemplate,
        List<MuspelheimRoomTemplate> allTemplates,
        MuspelheimRoomGraph graph)
    {
        var adjacencyRules = ParseAdjacencyRules(currentTemplate.WfcAdjacencyRules);

        var eligible = allTemplates.Where(t =>
        {
            // Check if allowed
            if (adjacencyRules.Allow.Any() && !adjacencyRules.Allow.Contains(t.TemplateName))
            {
                return false;
            }

            // Check if forbidden
            if (adjacencyRules.Forbid.Contains(t.TemplateName))
            {
                return false;
            }

            // Prevent duplicate room types (optional constraint)
            int existingCount = graph.Rooms.Count(r => r.Template.TemplateName == t.TemplateName);
            if (existingCount >= 2) // Max 2 of same room type
            {
                return false;
            }

            return true;
        }).ToList();

        return eligible;
    }

    /// <summary>
    /// Parse WFC adjacency rules from JSON.
    /// </summary>
    private WfcAdjacencyRules ParseAdjacencyRules(string json)
    {
        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            var rules = new WfcAdjacencyRules();

            if (root.TryGetProperty("allow", out var allowElement))
            {
                rules.Allow = allowElement.EnumerateArray()
                    .Select(e => e.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (root.TryGetProperty("forbid", out var forbidElement))
            {
                rules.Forbid = forbidElement.EnumerateArray()
                    .Select(e => e.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            return rules;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to parse WFC adjacency rules: {Json}", json);
            return new WfcAdjacencyRules(); // Empty rules (allow all)
        }
    }

    /// <summary>
    /// Select template based on enemy spawn weight.
    /// </summary>
    private MuspelheimRoomTemplate SelectWeightedTemplate(
        List<MuspelheimRoomTemplate> templates,
        Random random)
    {
        int totalWeight = templates.Sum(t => t.EnemySpawnWeight);
        int roll = random.Next(totalWeight);

        int cumulativeWeight = 0;
        foreach (var template in templates)
        {
            cumulativeWeight += template.EnemySpawnWeight;
            if (roll < cumulativeWeight)
            {
                return template;
            }
        }

        return templates.Last();
    }

    /// <summary>
    /// Get adjacent position that doesn't collide with existing rooms.
    /// </summary>
    private Vector2Int GetAdjacentPosition(
        Vector2Int currentPos,
        MuspelheimRoomGraph graph,
        Random random)
    {
        var directions = new[] {
            new Vector2Int(1, 0),  // East
            new Vector2Int(-1, 0), // West
            new Vector2Int(0, 1),  // North
            new Vector2Int(0, -1)  // South
        };

        // Shuffle directions
        var shuffledDirections = directions.OrderBy(_ => random.Next()).ToArray();

        foreach (var dir in shuffledDirections)
        {
            var newPos = new Vector2Int(currentPos.X + dir.X, currentPos.Y + dir.Y);

            // Check if position is already occupied
            if (!graph.Rooms.Any(r => r.Position.X == newPos.X && r.Position.Y == newPos.Y))
            {
                return newPos;
            }
        }

        // Fallback: Use first available direction (may overlap)
        return new Vector2Int(currentPos.X + directions[0].X, currentPos.Y + directions[0].Y);
    }

    /// <summary>
    /// Create a Room instance from a template.
    /// </summary>
    private MuspelheimRoom CreateRoomFromTemplate(
        MuspelheimRoomTemplate template,
        Vector2Int position,
        Random random)
    {
        return new MuspelheimRoom
        {
            RoomId = GenerateRoomId(),
            Name = template.TemplateName,
            Description = template.Description,
            RoomSize = template.RoomSize,
            HazardDensity = template.HazardDensity,
            Position = position,
            Hazards = new List<PlacedHazard>(),
            Enemies = new List<Enemy>(),
            ResourceNodes = new List<ResourceNode>(),
            Connections = new List<string>()
        };
    }

    /// <summary>
    /// Place hazards in a room based on template hazard_density.
    /// </summary>
    private void PlaceHazardsInRoom(MuspelheimRoom room, Random random)
    {
        var eligibleHazards = _dataRepository.GetEnvironmentalHazards(room.HazardDensity);

        _log.Debug("Placing hazards in {RoomName}: {EligibleCount} eligible hazards (density: {Density})",
            room.Name, eligibleHazards.Count, room.HazardDensity);

        foreach (var hazard in eligibleHazards)
        {
            // Check spawn probability based on tile coverage
            double spawnChance = hazard.TileCoveragePercent / 100.0;

            if (random.NextDouble() < spawnChance)
            {
                var placedHazard = new PlacedHazard
                {
                    HazardName = hazard.FeatureName,
                    HazardType = hazard.FeatureType,
                    DamagePerTurn = hazard.DamagePerTurn,
                    DamageType = hazard.DamageType,
                    IsDestructible = hazard.IsDestructible,
                    BlocksMovement = hazard.BlocksMovement,
                    BlocksLineOfSight = hazard.BlocksLineOfSight,
                    SpecialRules = hazard.SpecialRules
                };

                room.Hazards.Add(placedHazard);

                _log.Debug("Placed hazard: {HazardName} in {RoomName}",
                    hazard.FeatureName, room.Name);
            }
        }

        _log.Information("{RoomName}: Placed {Count} hazards", room.Name, room.Hazards.Count);
    }

    /// <summary>
    /// Place enemies in a room based on spawn weights.
    /// </summary>
    private void PlaceEnemiesInRoom(MuspelheimRoom room, int sectorDepth, Random random)
    {
        // Boss room special handling
        if (room.Name == "Containment Breach Zone")
        {
            PlaceBossEnemy(room, sectorDepth);
            return;
        }

        // Calculate enemy count based on room size
        int enemyCount = CalculateEnemyCount(room.RoomSize, random);

        _log.Debug("Placing {Count} enemies in {RoomName} (size: {Size})",
            enemyCount, room.Name, room.RoomSize);

        var eligibleEnemies = _dataRepository.GetEnemySpawns(
            minLevel: sectorDepth,
            maxLevel: sectorDepth + 2
        ).Where(e => e.EnemyType != "Boss").ToList(); // Exclude boss

        for (int i = 0; i < enemyCount; i++)
        {
            if (!eligibleEnemies.Any()) break;

            var spawnData = SelectWeightedEnemy(eligibleEnemies, random);
            var enemy = CreateEnemyFromSpawn(spawnData, sectorDepth);

            room.Enemies.Add(enemy);
        }

        _log.Information("{RoomName}: Placed {Count} enemies", room.Name, room.Enemies.Count);
    }

    /// <summary>
    /// Place boss enemy (Surtur's Herald) in boss room.
    /// </summary>
    private void PlaceBossEnemy(MuspelheimRoom room, int sectorDepth)
    {
        var bossSpawn = _dataRepository.GetBossSpawn();
        if (bossSpawn == null)
        {
            _log.Warning("No boss spawn data found for Surtur's Herald");
            return;
        }

        var boss = CreateEnemyFromSpawn(bossSpawn, level: 12); // Always level 12
        room.Enemies.Add(boss);

        _log.Information("Placed boss: {BossName} in {RoomName}", boss.Name, room.Name);
    }

    /// <summary>
    /// Calculate enemy count based on room size.
    /// </summary>
    private int CalculateEnemyCount(string roomSize, Random random)
    {
        return roomSize switch
        {
            "Small" => random.Next(1, 3),   // 1-2 enemies
            "Medium" => random.Next(2, 4),  // 2-3 enemies
            "Large" => random.Next(3, 5),   // 3-4 enemies
            "XLarge" => random.Next(4, 6),  // 4-5 enemies
            _ => random.Next(2, 4)          // Default 2-3
        };
    }

    /// <summary>
    /// Place resource nodes in a room.
    /// </summary>
    private void PlaceResourceNodes(MuspelheimRoom room, Random random)
    {
        var template = room.Template;
        if (template == null) return;

        // Check if room gets a resource node
        if (random.NextDouble() > template.ResourceSpawnChance)
        {
            return; // No resource in this room
        }

        var resources = _dataRepository.GetResourceDrops();

        // Select resource based on weight
        int totalWeight = resources.Sum(r => r.Weight);
        int roll = random.Next(totalWeight);

        int cumulativeWeight = 0;
        foreach (var resource in resources)
        {
            cumulativeWeight += resource.Weight;
            if (roll < cumulativeWeight)
            {
                // Determine quantity
                int quantity = random.Next(resource.MinQuantity, resource.MaxQuantity + 1);

                var node = new ResourceNode
                {
                    ResourceName = resource.ResourceName,
                    Quantity = quantity,
                    RequiresSpecialNode = resource.RequiresSpecialNode,
                    IsDiscovered = !resource.RequiresSpecialNode // Special nodes hidden by default
                };

                room.ResourceNodes.Add(node);

                _log.Debug("Placed resource: {ResourceName} x{Quantity} in {RoomName}",
                    resource.ResourceName, quantity, room.Name);
                break;
            }
        }

        _log.Information("{RoomName}: Placed {Count} resource nodes",
            room.Name, room.ResourceNodes.Count);
    }

    /// <summary>
    /// Connect rooms based on graph connections.
    /// </summary>
    private void ConnectRooms(List<MuspelheimRoom> rooms, MuspelheimRoomGraph graph)
    {
        foreach (var (fromId, toId) in graph.Connections)
        {
            var fromRoom = rooms.FirstOrDefault(r => r.RoomId == fromId);
            var toRoom = rooms.FirstOrDefault(r => r.RoomId == toId);

            if (fromRoom != null && toRoom != null)
            {
                fromRoom.Connections.Add(toRoom.RoomId);
                toRoom.Connections.Add(fromRoom.RoomId); // Bidirectional
            }
        }

        _log.Information("Connected {Count} room pairs", graph.Connections.Count);
    }

    private static int _nextRoomId = 10000; // Start Muspelheim rooms at 10000
    private string GenerateRoomId()
    {
        return $"muspelheim_room_{System.Threading.Interlocked.Increment(ref _nextRoomId)}";
    }

    #endregion

    #region Data Access Helpers

    /// <summary>
    /// Get all room templates for Muspelheim
    /// </summary>
    public List<MuspelheimRoomTemplate> GetRoomTemplates()
    {
        return _dataRepository.GetRoomTemplates();
    }

    /// <summary>
    /// Get enemy spawns for a specific level range
    /// </summary>
    public List<MuspelheimEnemySpawn> GetEnemySpawnsForLevel(int level)
    {
        return _dataRepository.GetEnemySpawns(minLevel: level - 1, maxLevel: level + 1);
    }

    /// <summary>
    /// Get environmental hazards for a hazard density level
    /// </summary>
    public List<MuspelheimHazard> GetHazardsForDensity(string hazardDensity)
    {
        return _dataRepository.GetEnvironmentalHazards(hazardDensity);
    }

    /// <summary>
    /// Get biome metadata
    /// </summary>
    public MuspelheimBiomeMetadata GetBiomeMetadata()
    {
        return _dataRepository.GetBiomeMetadata();
    }

    #endregion
}

#region Result Data Transfer Objects

public class MuspelheimPreparednessReport
{
    public List<CharacterPreparedness> Characters { get; set; } = new();
    public double AverageFireResistance { get; set; }
    public bool PartyIsAdequatelyPrepared { get; set; }
}

public class CharacterPreparedness
{
    public string CharacterName { get; set; } = string.Empty;
    public int FireResistancePercent { get; set; }
    public int Sturdiness { get; set; }
    public bool IsAdequatelyPrepared { get; set; }
    public int RecommendedFireResistance { get; set; }
    public string WarningLevel { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
}

public class HeatDamageResult
{
    public string CharacterName { get; set; } = string.Empty;
    public int SturdynessValue { get; set; }
    public int SuccessesRolled { get; set; }
    public bool CheckPassed { get; set; }
    public int RawDamageRolled { get; set; }
    public int DamageDealt { get; set; }
    public bool CharacterDied { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BrittlenessResult
{
    public string EnemyName { get; set; } = string.Empty;
    public int IceDamageDealt { get; set; }
    public int FinalDamageDealt { get; set; }
    public bool BrittleApplied { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PhysicalDamageResult
{
    public string EnemyName { get; set; } = string.Empty;
    public int BaseDamage { get; set; }
    public int FinalDamage { get; set; }
    public bool BrittleBonusApplied { get; set; }
    public bool EnemyDefeated { get; set; }
    public string Message { get; set; } = string.Empty;
}

// Procedural Generation DTOs

public class MuspelheimSector
{
    public List<MuspelheimRoom> Rooms { get; set; } = new();
    public int Depth { get; set; }
    public string EntranceRoomId { get; set; } = string.Empty;
    public string ExitRoomId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public int Seed { get; set; }
}

public class MuspelheimRoom
{
    public string RoomId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RoomSize { get; set; } = string.Empty;
    public string HazardDensity { get; set; } = string.Empty;
    public Vector2Int Position { get; set; }
    public bool IsEntrance { get; set; }
    public bool IsExit { get; set; }
    public bool IsBossRoom { get; set; }
    public List<PlacedHazard> Hazards { get; set; } = new();
    public List<Enemy> Enemies { get; set; } = new();
    public List<ResourceNode> ResourceNodes { get; set; } = new();
    public List<string> Connections { get; set; } = new();
    public MuspelheimRoomTemplate? Template { get; set; }
}

public class MuspelheimRoomGraph
{
    public List<MuspelheimRoomNode> Rooms { get; set; } = new();
    public List<(string FromId, string ToId)> Connections { get; set; } = new();
}

public class MuspelheimRoomNode
{
    public MuspelheimRoomTemplate Template { get; set; } = null!;
    public Vector2Int Position { get; set; }
    public string RoomId { get; set; } = string.Empty;
    public bool IsEntrance { get; set; }
    public bool IsExit { get; set; }
    public bool IsBossRoom { get; set; }
}

public class WfcAdjacencyRules
{
    public List<string> Allow { get; set; } = new();
    public List<string> Forbid { get; set; } = new();
}

public class PlacedHazard
{
    public string HazardName { get; set; } = string.Empty;
    public string HazardType { get; set; } = string.Empty;
    public int DamagePerTurn { get; set; }
    public string? DamageType { get; set; }
    public bool IsDestructible { get; set; }
    public bool BlocksMovement { get; set; }
    public bool BlocksLineOfSight { get; set; }
    public string SpecialRules { get; set; } = string.Empty;
}

public class ResourceNode
{
    public string ResourceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool RequiresSpecialNode { get; set; }
    public bool IsDiscovered { get; set; }
}

public struct Vector2Int
{
    public int X { get; set; }
    public int Y { get; set; }

    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }
}

#endregion
