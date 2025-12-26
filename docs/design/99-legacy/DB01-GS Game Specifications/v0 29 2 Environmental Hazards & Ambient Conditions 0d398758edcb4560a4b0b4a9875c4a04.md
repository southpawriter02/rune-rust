# v0.29.2: Environmental Hazards & Ambient Conditions

Type: Technical
Description: Implements [Intense Heat] ambient condition (DC 12 STURDINESS, 2d6 Fire damage), 8+ hazard/terrain types, Biome_EnvironmentalFeatures table, and ConditionService integration.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.29.1 (Database Schema)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.29: Muspelheim Biome Implementation (v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.29.2-HAZARDS

**Status:** Design Complete — Ready for Phase 2 (Database Implementation)

**Timeline:** 8-12 hours

**Prerequisites:** v0.29.1 (Database Schema & Room Templates)

**Parent Spec:** v0.29: Muspelheim Biome Implementation[[1]](v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)

---

## I. Executive Summary

This specification defines **environmental hazards and ambient conditions** for the Muspelheim biome, including [Intense Heat] ambient condition, 8+ hazard types, terrain features, and integration with the Tactical Combat Grid (v0.20) and Environmental Combat (v0.22) systems.

### Scope

**1 Ambient Condition:**

- **[Intense Heat]** - STURDINESS Resolve check DC 12 every turn, 2d6 Fire damage on failure

**8+ Environmental Hazards:**

1. [Burning Ground] - Persistent Fire damage terrain
2. [Chasm/Lava River] - Instant death obstacles
3. [High-Pressure Steam Vent] - Burst damage + Disoriented
4. [Volatile Gas Pocket] - Explosive AoE hazard
5. [Scorched Metal Plating] - Movement impediment
6. [Molten Slag Pool] - Damage-over-time zone
7. [Collapsing Catwalk] - Structural failure hazard
8. [Thermal Mirage] - Vision/targeting penalty

**Technical Deliverables:**

- Biome_EnvironmentalFeatures table seeding (8+ hazard definitions)
- ConditionService extension for [Intense Heat]
- IntenseHeatService implementation
- Environmental hazard placement algorithms
- Integration with Tactical Grid tile system
- Serilog structured logging
- Unit test suite (~85% coverage)

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.29.2)

**Ambient Condition Implementation:**

- [Intense Heat] condition definition in database
- End-of-turn STURDINESS check system
- Fire damage calculation with Resistance
- Biome status tracking (heat_damage_taken)

**Environmental Hazards:**

- Complete hazard definitions (8+ types)
- Placement rules per room template hazard_density
- Tactical Grid integration (tile overlays)
- Destructible hazards (steam vents, gas pockets)
- Interaction with Environmental Combat system

**Service Implementation:**

- IntenseHeatService.ProcessHeatChecks()
- IntenseHeatService.ApplyFireResistance()
- HazardPlacementService.PlaceHazardsInRoom()
- Environmental Combat integration methods

**Testing:**

- Unit tests for heat checks (pass/fail scenarios)
- Edge cases (0% resistance, 100% immunity)
- Hazard placement validation
- Balance testing (damage rates feel fair)

**Quality:**

- v5.0 setting compliance (technological heat)
- ASCII-only entity names
- Serilog structured logging
- 80%+ unit test coverage

### ❌ Explicitly Out of Scope

- Enemy definitions (v0.29.3)
- Procedural generation algorithms (v0.29.4)
- Brittleness mechanic ([Brittle] debuff) (v0.29.3)
- Boss encounter mechanics (v0.29.3)
- UI/visual effects (separate phase)
- Audio design (separate phase)
- Advanced puzzle mechanics (pressure valves) (v0.34)

---

## III. [Intense Heat] Ambient Condition

### Condition Definition

**[Intense Heat]** is the mandatory ambient condition for Muspelheim. It affects all combatants (players and enemies) at the end of each turn.

**Mechanical Implementation:**

```
At end of each turn:
1. All combatants make STURDINESS Resolve check (DC 12)
2. On failure: Take 2d6 Fire damage
3. Apply Fire Resistance (if any)
4. Track damage in Characters_BiomeStatus.heat_damage_taken
5. Increment death counter on player death from heat
```

**v2.0 Canonical Values:**

- DC 12 STURDINESS check (preserved)
- 2d6 Fire damage on failure (preserved)
- No save on success (0 damage)

### Database Schema

```sql
-- Add [Intense Heat] to Conditions table
INSERT INTO Conditions (
    condition_id,
    condition_name,
    condition_type,
    condition_description,
    is_ambient,
    resolve_check_attribute,
    resolve_check_dc,
    damage_dice_count,
    damage_dice_size,
    damage_type,
    applies_every_turn,
    can_be_resisted,
    resistance_type,
    is_removable
) VALUES (
    1004,
    '[Intense Heat]',
    'Ambient',
    'Catastrophic thermal load from containment system failure. Mandatory STURDINESS check each turn or suffer Fire damage.',
    1,
    'STURDINESS',
    12,
    2,
    6,
    'Fire',
    1,
    1,
    'Fire',
    0
);

-- Link to Muspelheim biome (already done in v0.29.1)
-- Biomes.ambient_condition_id = 1004
```

**Column Notes:**

- `condition_id: 1004` - Follows sequence (1000-1999 reserved for ambient conditions)
- `is_ambient: 1` - Applied automatically to all combatants in biome
- `applies_every_turn: 1` - Triggers at end of each turn
- `can_be_resisted: 1` - Fire Resistance applies
- `is_removable: 0` - Cannot be cleansed/dispelled (environmental)

### Service Implementation: IntenseHeatService

```csharp
using Serilog;
using RuneAndRust.Core.Models;
using [RuneAndRust.Services](http://RuneAndRust.Services).Combat;
using [RuneAndRust.Services](http://RuneAndRust.Services).Character;

namespace [RuneAndRust.Services](http://RuneAndRust.Services).Biomes
{
    public class IntenseHeatService
    {
        private readonly ILogger _log;
        private readonly ResolveCheckService _resolveCheckService;
        private readonly DamageService _damageService;
        private readonly BiomeStatusService _biomeStatusService;

        public IntenseHeatService(
            ILogger log,
            ResolveCheckService resolveCheckService,
            DamageService damageService,
            BiomeStatusService biomeStatusService)
        {
            _log = log;
            _resolveCheckService = resolveCheckService;
            _damageService = damageService;
            _biomeStatusService = biomeStatusService;
        }

        /// <summary>
        /// Process [Intense Heat] ambient condition for all combatants in Muspelheim.
        /// Called at end of each combat turn.
        /// </summary>
        public void ProcessEndOfTurnHeat(List<Combatant> combatants, int biomeId)
        {
            using (_log.BeginTimedOperation("Processing [Intense Heat] for {CombatantCount} combatants", combatants.Count))
            {
                foreach (var combatant in combatants)
                {
                    ProcessHeatCheckForCombatant(combatant, biomeId);
                }

                _log.Information("[Intense Heat] processing complete for turn");
            }
        }

        /// <summary>
        /// Process heat check for a single combatant.
        /// </summary>
        private void ProcessHeatCheckForCombatant(Combatant combatant, int biomeId)
        {
            const int DC = 12;
            const int DAMAGE_DICE_COUNT = 2;
            const int DAMAGE_DICE_SIZE = 6;

            // Make STURDINESS Resolve check
            var checkResult = _resolveCheckService.MakeResolveCheck(
                combatant,
                AttributeType.STURDINESS,
                DC
            );

            _log.Information(
                "[Intense Heat] check for {CombatantName}: {Result} (rolled {Roll} vs DC {DC})",
                [combatant.Name](http://combatant.Name),
                checkResult.Success ? "PASS" : "FAIL",
                checkResult.TotalRoll,
                DC
            );

            if (checkResult.Success)
            {
                // Success: No damage
                _log.Information("{CombatantName} resists [Intense Heat]", [combatant.Name](http://combatant.Name));
                return;
            }

            // Failure: Roll Fire damage
            int rawDamage = _damageService.RollDamage(DAMAGE_DICE_COUNT, DAMAGE_DICE_SIZE);
            _log.Information(
                "{CombatantName} fails heat check, raw damage: {RawDamage}",
                [combatant.Name](http://combatant.Name),
                rawDamage
            );

            // Apply Fire Resistance
            var finalDamage = ApplyFireResistance(combatant, rawDamage);

            // Apply damage
            _damageService.ApplyDamage(
                combatant,
                finalDamage,
                [DamageType.Fire](http://DamageType.Fire),
                "[Intense Heat]"
            );

            // Track statistics (player characters only)
            if (combatant is PlayerCharacter pc)
            {
                _biomeStatusService.IncrementHeatDamage(pc.CharacterId, biomeId, finalDamage);

                if (pc.CurrentHP <= 0)
                {
                    _biomeStatusService.IncrementHeatDeaths(pc.CharacterId, biomeId);
                    _log.Warning(
                        "{CharacterName} has died from [Intense Heat]",
                        [pc.Name](http://pc.Name)
                    );
                }
            }

            _log.Information(
                "{CombatantName} takes {FinalDamage} Fire damage from [Intense Heat] (HP: {CurrentHP}/{MaxHP})",
                [combatant.Name](http://combatant.Name),
                finalDamage,
                combatant.CurrentHP,
                combatant.MaxHP
            );
        }

        /// <summary>
        /// Apply Fire Resistance to raw damage.
        /// </summary>
        public int ApplyFireResistance(Combatant combatant, int rawDamage)
        {
            // Get Fire Resistance percentage (0-100)
            int resistancePercent = combatant.GetResistance([DamageType.Fire](http://DamageType.Fire));

            if (resistancePercent <= 0)
            {
                return rawDamage;
            }

            if (resistancePercent >= 100)
            {
                _log.Information(
                    "{CombatantName} is immune to Fire damage (100% resistance)",
                    [combatant.Name](http://combatant.Name)
                );
                return 0;
            }

            // Calculate reduced damage
            int reducedDamage = rawDamage - (rawDamage * resistancePercent / 100);

            _log.Information(
                "Fire Resistance {Percent}%: {RawDamage} reduced to {FinalDamage}",
                resistancePercent,
                rawDamage,
                reducedDamage
            );

            return reducedDamage;
        }

        /// <summary>
        /// Check if combatant has sufficient Fire Resistance for Muspelheim.
        /// Recommendation: 50%+ for comfortable exploration.
        /// </summary>
        public bool HasSufficientFireResistance(Combatant combatant, int threshold = 50)
        {
            int resistancePercent = combatant.GetResistance([DamageType.Fire](http://DamageType.Fire));
            bool sufficient = resistancePercent >= threshold;

            _log.Information(
                "{CombatantName} Fire Resistance: {Percent}% ({Status})",
                [combatant.Name](http://combatant.Name),
                resistancePercent,
                sufficient ? "Sufficient" : "Insufficient"
            );

            return sufficient;
        }
    }
}
```

### Integration with Combat System

**CombatService modification:**

```csharp
public void EndTurn(CombatEncounter encounter)
{
    // ... existing end-of-turn logic ...

    // Apply ambient conditions if in a biome with them
    if (encounter.CurrentBiomeId.HasValue)
    {
        var biome = _biomeService.GetBiome(encounter.CurrentBiomeId.Value);
        
        if (biome.AmbientConditionId.HasValue)
        {
            // Muspelheim: ambient_condition_id = 1004 ([Intense Heat])
            if (biome.AmbientConditionId.Value == 1004)
            {
                _intenseHeatService.ProcessEndOfTurnHeat(
                    encounter.AllCombatants,
                    biome.BiomeId
                );
            }
        }
    }

    // ... rest of end-of-turn logic ...
}
```

---

## IV. Environmental Hazards

### Hazard 1: [Burning Ground]

**Description:** Persistent flames or superheated metal plating. Deals Fire damage each turn to combatants standing on it.

**Database Definition:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id,
    feature_name,
    feature_type,
    feature_description,
    damage_per_turn,
    damage_type,
    tile_coverage_percent,
    is_destructible,
    blocks_movement,
    blocks_line_of_sight,
    hazard_density_category
) VALUES (
    4,
    '[Burning Ground]',
    'Terrain',
    'Flames or superheated metal. Deals Fire damage each turn to those standing on it.',
    8, -- 1d8 average
    'Fire',
    15, -- 15% of room tiles
    0,
    0,
    0,
    'Medium'
);
```

**Mechanical Implementation:**

- **Damage:** 1d8 Fire damage per turn
- **Trigger:** At end of turn if combatant is standing on tile
- **Resistance:** Fire Resistance applies
- **Coverage:** ~15% of room tiles (scattered patches)
- **Placement logic:** Clusters near lava rivers, forges, breaches
- **Visual:** Orange/red tile overlay

**Code Example:**

```csharp
public void ApplyBurningGroundDamage(Combatant combatant, TacticalTile tile)
{
    if (tile.HasFeature("[Burning Ground]"))
    {
        int damage = _dice.Roll(1, 8);
        int finalDamage = _intenseHeatService.ApplyFireResistance(combatant, damage);
        
        _damageService.ApplyDamage(
            combatant,
            finalDamage,
            [DamageType.Fire](http://DamageType.Fire),
            "[Burning Ground]"
        );
        
        _log.Information(
            "{Combatant} takes {Damage} Fire damage from [Burning Ground]",
            [combatant.Name](http://combatant.Name),
            finalDamage
        );
    }
}
```

### Hazard 2: [Chasm/Lava River]

**Description:** Impassable molten slag river or structural collapse exposing magma below. Instant death if pushed/moved into.

**Database Definition:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id,
    feature_name,
    feature_type,
    feature_description,
    damage_per_turn,
    damage_type,
    tile_coverage_percent,
    is_destructible,
    blocks_movement,
    blocks_line_of_sight,
    hazard_density_category,
    special_rules
) VALUES (
    4,
    '[Chasm/Lava River]',
    'Obstacle',
    'Impassable molten slag river. Instant death if pushed/moved into. Controllers dream of this.',
    999, -- Instant death marker
    'Fire',
    10, -- 10% of room tiles
    0,
    1,
    0,
    'High',
    'instant_death_on_enter'
);
```

**Mechanical Implementation:**

- **Effect:** Instant death (HP = 0) on entering tile
- **Movement:** Blocks all movement
- **Forced Movement:** Push/Pull abilities can push enemies into lava
- **Line of Sight:** Does not block (players can see across)
- **Coverage:** ~10% of room tiles (bisects rooms or borders)
- **Placement logic:** Linear features (rivers) or pit borders
- **Visual:** Bright orange/yellow glow, animated

**Code Example:**

```csharp
public bool TryMoveToTile(Combatant combatant, TacticalTile targetTile)
{
    if (targetTile.HasFeature("[Chasm/Lava River]"))
    {
        _log.Warning(
            "{Combatant} pushed into [Lava River] - instant death",
            [combatant.Name](http://combatant.Name)
        );
        
        _damageService.ApplyDamage(
            combatant,
            combatant.CurrentHP, // Instant death
            [DamageType.Fire](http://DamageType.Fire),
            "[Chasm/Lava River]"
        );
        
        return false; // Movement fails (corpse doesn't occupy tile)
    }
    
    // ... normal movement logic ...
}
```

**Tactical Value:**

- **Controller's Playground:** Push/Pull abilities become devastating
- **Positioning Importance:** Stay away from lava edges
- **Risk/Reward:** Crossing catwalks for positioning advantage

### Hazard 3: [High-Pressure Steam Vent]

**Description:** Pressure release valve venting superheated steam. Deals burst damage + Disoriented status.

**Database Definition:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id,
    feature_name,
    feature_type,
    feature_description,
    damage_per_turn,
    damage_type,
    tile_coverage_percent,
    is_destructible,
    blocks_movement,
    blocks_line_of_sight,
    hazard_density_category,
    special_rules
) VALUES (
    4,
    '[High-Pressure Steam Vent]',
    'Dynamic',
    'Pressure valve venting superheated steam. Deals burst damage + Disoriented. Destructible via Environmental Combat.',
    16, -- 2d8 average
    'Fire',
    5, -- 5% of room tiles
    1,
    0,
    1, -- Blocks line of sight when venting
    'High',
    'applies_disoriented,destructible'
);
```

**Mechanical Implementation:**

- **Damage:** 2d8 Fire damage (burst)
- **Status Effect:** [Disoriented] for 2 turns (-2 to attack rolls, -10 ft movement)
- **Trigger:** Random (30% chance per turn) or when destructible element hit
- **Destructible:** Can be destroyed via Environmental Combat attacks
- **Coverage:** ~5% of room tiles (vents at walls/pipes)
- **Line of Sight:** Blocks LoS while venting (steam cloud)
- **Visual:** Steam jet animation + sound effect

**Code Example:**

```csharp
public void TriggerSteamVent(TacticalTile ventTile)
{
    // Check if vent triggers this turn
    if (_random.NextDouble() > 0.30) return;

    _log.Information("[High-Pressure Steam Vent] triggered at {Position}", ventTile.Position);

    // Get all combatants in 1-tile radius
    var affectedCombatants = _tacticalGridService.GetCombatantsInRadius(
        ventTile.Position,
        radius: 1
    );

    foreach (var combatant in affectedCombatants)
    {
        // Roll damage
        int damage = _dice.Roll(2, 8);
        int finalDamage = _intenseHeatService.ApplyFireResistance(combatant, damage);

        _damageService.ApplyDamage(
            combatant,
            finalDamage,
            [DamageType.Fire](http://DamageType.Fire),
            "[High-Pressure Steam Vent]"
        );

        // Apply Disoriented
        _statusEffectService.ApplyStatusEffect(
            combatant,
            "Disoriented",
            duration: 2
        );

        _log.Information(
            "{Combatant} hit by steam vent: {Damage} damage + Disoriented",
            [combatant.Name](http://combatant.Name),
            finalDamage
        );
    }
}
```

### Hazard 4: [Volatile Gas Pocket]

**Description:** Pockets of combustible gas. Explodes when Fire damage dealt nearby, causing AoE damage.

**Database Definition:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id,
    feature_name,
    feature_type,
    feature_description,
    damage_per_turn,
    damage_type,
    tile_coverage_percent,
    is_destructible,
    blocks_movement,
    blocks_line_of_sight,
    hazard_density_category,
    special_rules
) VALUES (
    4,
    '[Volatile Gas Pocket]',
    'Explosive',
    'Combustible gas pocket. Explodes when Fire damage dealt nearby (3-tile radius), causing 4d6 AoE Fire damage.',
    24, -- 4d6 average
    'Fire',
    3, -- 3% of room tiles
    1,
    0,
    0,
    'Extreme',
    'chain_reaction,aoe_radius_3'
);
```

**Mechanical Implementation:**

- **Trigger:** Any Fire damage within 3 tiles
- **Damage:** 4d6 Fire damage (AoE, 3-tile radius)
- **Chain Reaction:** Can trigger adjacent gas pockets
- **One-Time:** Pocket destroyed after explosion
- **Coverage:** ~3% of room tiles (rare but deadly)
- **Placement logic:** Clusters in poorly ventilated areas
- **Visual:** Shimmering air + explosion animation

**Code Example:**

```csharp
public void CheckForGasExplosion(TacticalTile damageTile, DamageType damageType)
{
    if (damageType != [DamageType.Fire](http://DamageType.Fire)) return;

    // Find all gas pockets within 3 tiles
    var gasPockets = _tacticalGridService.GetTilesInRadius(
        damageTile.Position,
        radius: 3
    ).Where(t => t.HasFeature("[Volatile Gas Pocket]")).ToList();

    foreach (var pocket in gasPockets)
    {
        _log.Warning("[Volatile Gas Pocket] ignited at {Position}", pocket.Position);

        // Get all combatants in explosion radius
        var victims = _tacticalGridService.GetCombatantsInRadius(
            pocket.Position,
            radius: 3
        );

        foreach (var victim in victims)
        {
            int damage = _dice.Roll(4, 6);
            int finalDamage = _intenseHeatService.ApplyFireResistance(victim, damage);

            _damageService.ApplyDamage(
                victim,
                finalDamage,
                [DamageType.Fire](http://DamageType.Fire),
                "[Volatile Gas Pocket]"
            );

            _log.Information(
                "{Victim} caught in gas explosion: {Damage} Fire damage",
                [victim.Name](http://victim.Name),
                finalDamage
            );
        }

        // Remove gas pocket (destroyed)
        pocket.RemoveFeature("[Volatile Gas Pocket]");

        // Chain reaction check (recursive)
        CheckForGasExplosion(pocket, [DamageType.Fire](http://DamageType.Fire));
    }
}
```

**Tactical Considerations:**

- **Double-Edged Sword:** Can harm players if triggered accidentally
- **Controller Synergy:** Intentionally trigger for AoE damage
- **Positioning:** Stay away from clusters
- **Fire Resistance:** Essential for intentional triggers

### Hazard 5: [Scorched Metal Plating]

**Description:** Heat-warped structural plating. Impedes movement but doesn't deal damage.

**Database Definition:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id,
    feature_name,
    feature_type,
    feature_description,
    damage_per_turn,
    damage_type,
    tile_coverage_percent,
    is_destructible,
    blocks_movement,
    blocks_line_of_sight,
    hazard_density_category,
    special_rules
) VALUES (
    4,
    '[Scorched Metal Plating]',
    'Terrain',
    'Heat-warped structural plating. Movement cost doubled (10 ft becomes 5 ft effective). No damage.',
    0,
    NULL,
    20, -- 20% of room tiles
    0,
    0,
    0,
    'Low',
    'movement_cost_doubled'
);
```

**Mechanical Implementation:**

- **Effect:** Movement cost doubled (10 ft movement = 5 ft actual)
- **No Damage:** Safe to stand on, just slow
- **Coverage:** ~20% of room tiles (largest coverage)
- **Placement logic:** Scattered throughout, especially near heat sources
- **Visual:** Blackened/warped metal texture

### Hazard 6: [Molten Slag Pool]

**Description:** Shallow pools of cooling slag. Deals damage and applies [Slowed].

**Database Definition:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id,
    feature_name,
    feature_type,
    feature_description,
    damage_per_turn,
    damage_type,
    tile_coverage_percent,
    is_destructible,
    blocks_movement,
    blocks_line_of_sight,
    hazard_density_category,
    special_rules
) VALUES (
    4,
    '[Molten Slag Pool]',
    'Terrain',
    'Shallow pool of cooling slag. Deals Fire damage and applies [Slowed] (-50% movement speed).',
    4, -- 1d4 average
    'Fire',
    8, -- 8% of room tiles
    0,
    0,
    0,
    'Medium',
    'applies_slowed'
);
```

**Mechanical Implementation:**

- **Damage:** 1d4 Fire damage per turn (minor)
- **Status Effect:** [Slowed] while standing in pool (-50% movement speed)
- **Coverage:** ~8% of room tiles
- **Placement logic:** Low-lying areas, drainage channels
- **Visual:** Orange/yellow liquid texture

### Hazard 7: [Collapsing Catwalk]

**Description:** Structurally unstable walkway. Random chance of collapse each turn.

**Database Definition:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id,
    feature_name,
    feature_type,
    feature_description,
    damage_per_turn,
    damage_type,
    tile_coverage_percent,
    is_destructible,
    blocks_movement,
    blocks_line_of_sight,
    hazard_density_category,
    special_rules
) VALUES (
    4,
    '[Collapsing Catwalk]',
    'Dynamic',
    'Unstable walkway. 20% chance per turn of collapse if occupied. Combatant falls to [Chasm] below.',
    999, -- Instant death if falls
    'Physical',
    5, -- 5% of room tiles
    0,
    0,
    0,
    'Extreme',
    'collapse_chance_20,fall_to_chasm'
);
```

**Mechanical Implementation:**

- **Trigger:** 20% chance per turn if combatant standing on tile
- **Effect:** Combatant falls into [Chasm] below (instant death)
- **One-Time:** Catwalk tile becomes [Chasm] after collapse
- **Coverage:** ~5% of room tiles (narrow walkways)
- **Placement logic:** Bridges over lava/chasms
- **Visual:** Rusted/damaged catwalk with warning signs

### Hazard 8: [Thermal Mirage]

**Description:** Heat shimmer distorts vision and targeting.

**Database Definition:**

```sql
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id,
    feature_name,
    feature_type,
    feature_description,
    damage_per_turn,
    damage_type,
    tile_coverage_percent,
    is_destructible,
    blocks_movement,
    blocks_line_of_sight,
    hazard_density_category,
    special_rules
) VALUES (
    4,
    '[Thermal Mirage]',
    'Vision',
    'Heat shimmer distorts vision. Ranged attacks through affected tiles suffer -2 penalty.',
    0,
    NULL,
    25, -- 25% of room tiles
    0,
    0,
    0,
    'Low',
    'ranged_attack_penalty_2'
);
```

**Mechanical Implementation:**

- **Effect:** Ranged attacks through affected tiles: -2 penalty
- **No Damage:** Pure targeting penalty
- **Coverage:** ~25% of room tiles (largest coverage)
- **Placement logic:** Anywhere with extreme heat, especially near lava
- **Visual:** Shimmering air effect overlay

---

## V. Complete Environmental Features Table

```sql
-- =====================================================
-- v0.29.2: Environmental Hazards & Ambient Conditions
-- =====================================================
-- Create Biome_EnvironmentalFeatures table
-- =====================================================

CREATE TABLE IF NOT EXISTS Biome_EnvironmentalFeatures (
    feature_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    feature_name TEXT NOT NULL,
    feature_type TEXT CHECK(feature_type IN ('Terrain', 'Obstacle', 'Dynamic', 'Explosive', 'Vision')),
    feature_description TEXT,
    damage_per_turn INTEGER DEFAULT 0,
    damage_type TEXT CHECK(damage_type IN ('Fire', 'Ice', 'Lightning', 'Poison', 'Physical', 'Psychic', NULL)),
    tile_coverage_percent INTEGER DEFAULT 10,
    is_destructible INTEGER DEFAULT 0,
    blocks_movement INTEGER DEFAULT 0,
    blocks_line_of_sight INTEGER DEFAULT 0,
    hazard_density_category TEXT CHECK(hazard_density_category IN ('Low', 'Medium', 'High', 'Extreme')),
    special_rules TEXT, -- Comma-separated tags
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_biome_features_biome ON Biome_EnvironmentalFeatures(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_features_type ON Biome_EnvironmentalFeatures(feature_type);
CREATE INDEX IF NOT EXISTS idx_biome_features_density ON Biome_EnvironmentalFeatures(hazard_density_category);

-- =====================================================
-- Seed [Intense Heat] Ambient Condition
-- =====================================================

INSERT INTO Conditions (
    condition_id,
    condition_name,
    condition_type,
    condition_description,
    is_ambient,
    resolve_check_attribute,
    resolve_check_dc,
    damage_dice_count,
    damage_dice_size,
    damage_type,
    applies_every_turn,
    can_be_resisted,
    resistance_type,
    is_removable
) VALUES (
    1004,
    '[Intense Heat]',
    'Ambient',
    'Catastrophic thermal load from containment system failure. Mandatory STURDINESS check each turn or suffer Fire damage.',
    1,
    'STURDINESS',
    12,
    2,
    6,
    'Fire',
    1,
    1,
    'Fire',
    0
);

-- =====================================================
-- Seed Environmental Hazards
-- =====================================================

-- Hazard 1: Burning Ground
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Burning Ground]', 'Terrain',
    'Flames or superheated metal. Deals Fire damage each turn to those standing on it.',
    8, 'Fire', 15, 0, 0, 0, 'Medium', 'persistent_fire'
);

-- Hazard 2: Chasm/Lava River
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Chasm/Lava River]', 'Obstacle',
    'Impassable molten slag river. Instant death if pushed/moved into. Controllers dream of this.',
    999, 'Fire', 10, 0, 1, 0, 'High', 'instant_death_on_enter'
);

-- Hazard 3: High-Pressure Steam Vent
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[High-Pressure Steam Vent]', 'Dynamic',
    'Pressure valve venting superheated steam. Deals burst damage + Disoriented. Destructible via Environmental Combat.',
    16, 'Fire', 5, 1, 0, 1, 'High', 'applies_disoriented,destructible'
);

-- Hazard 4: Volatile Gas Pocket
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Volatile Gas Pocket]', 'Explosive',
    'Combustible gas pocket. Explodes when Fire damage dealt nearby (3-tile radius), causing 4d6 AoE Fire damage.',
    24, 'Fire', 3, 1, 0, 0, 'Extreme', 'chain_reaction,aoe_radius_3'
);

-- Hazard 5: Scorched Metal Plating
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Scorched Metal Plating]', 'Terrain',
    'Heat-warped structural plating. Movement cost doubled (10 ft becomes 5 ft effective). No damage.',
    0, NULL, 20, 0, 0, 0, 'Low', 'movement_cost_doubled'
);

-- Hazard 6: Molten Slag Pool
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Molten Slag Pool]', 'Terrain',
    'Shallow pool of cooling slag. Deals Fire damage and applies [Slowed] (-50% movement speed).',
    4, 'Fire', 8, 0, 0, 0, 'Medium', 'applies_slowed'
);

-- Hazard 7: Collapsing Catwalk
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Collapsing Catwalk]', 'Dynamic',
    'Unstable walkway. 20% chance per turn of collapse if occupied. Combatant falls to [Chasm] below.',
    999, 'Physical', 5, 0, 0, 0, 'Extreme', 'collapse_chance_20,fall_to_chasm'
);

-- Hazard 8: Thermal Mirage
INSERT INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Thermal Mirage]', 'Vision',
    'Heat shimmer distorts vision. Ranged attacks through affected tiles suffer -2 penalty.',
    0, NULL, 25, 0, 0, 0, 'Low', 'ranged_attack_penalty_2'
);
```

---

## VI. Hazard Placement System

### HazardPlacementService

```csharp
using Serilog;
using RuneAndRust.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace [RuneAndRust.Services](http://RuneAndRust.Services).Biomes
{
    public class HazardPlacementService
    {
        private readonly ILogger _log;
        private readonly Random _random;

        public HazardPlacementService(ILogger log)
        {
            _log = log;
            _random = new Random();
        }

        /// <summary>
        /// Place environmental hazards in a room based on template hazard_density.
        /// </summary>
        public void PlaceHazardsInRoom(
            Room room,
            RoomTemplate template,
            List<EnvironmentalFeature> availableHazards)
        {
            using (_log.BeginTimedOperation(
                "Placing hazards in {RoomName} (density: {Density})",
                template.TemplateName,
                template.HazardDensity))
            {
                // Filter hazards by density category
                var eligibleHazards = availableHazards
                    .Where(h => IsHazardEligible(h, template.HazardDensity))
                    .ToList();

                _log.Information(
                    "Found {Count} eligible hazards for density {Density}",
                    eligibleHazards.Count,
                    template.HazardDensity
                );

                // Place each hazard type based on coverage percentage
                foreach (var hazard in eligibleHazards)
                {
                    PlaceHazardType(room, hazard);
                }

                _log.Information(
                    "Hazard placement complete: {TotalTiles} hazard tiles",
                    room.Tiles.Count(t => t.HasHazard)
                );
            }
        }

        /// <summary>
        /// Check if hazard is eligible for given density category.
        /// </summary>
        private bool IsHazardEligible(EnvironmentalFeature hazard, string templateDensity)
        {
            // Density hierarchy: None < Low < Medium < High < Extreme
            var densityLevels = new Dictionary<string, int>
            {
                { "None", 0 },
                { "Low", 1 },
                { "Medium", 2 },
                { "High", 3 },
                { "Extreme", 4 }
            };

            int templateLevel = densityLevels[templateDensity];
            int hazardLevel = densityLevels[hazard.HazardDensityCategory];

            // Template must be at or above hazard's density level
            return templateLevel >= hazardLevel;
        }

        /// <summary>
        /// Place a specific hazard type in the room.
        /// </summary>
        private void PlaceHazardType(Room room, EnvironmentalFeature hazard)
        {
            int totalTiles = room.Tiles.Count;
            int targetTiles = (int)(totalTiles * (hazard.TileCoveragePercent / 100.0));

            _log.Information(
                "Placing {Hazard}: target {TargetTiles}/{TotalTiles} tiles ({Percent}%)",
                hazard.FeatureName,
                targetTiles,
                totalTiles,
                hazard.TileCoveragePercent
            );

            // Special placement logic by type
            switch (hazard.FeatureType)
            {
                case "Obstacle":
                    PlaceLinearObstacle(room, hazard, targetTiles);
                    break;
                case "Terrain":
                    PlaceScatteredTerrain(room, hazard, targetTiles);
                    break;
                case "Dynamic":
                    PlaceWallFeatures(room, hazard, targetTiles);
                    break;
                case "Explosive":
                    PlaceClusters(room, hazard, targetTiles);
                    break;
                case "Vision":
                    PlaceNearHeatSources(room, hazard, targetTiles);
                    break;
            }
        }

        /// <summary>
        /// Place linear obstacle (e.g., lava river bisecting room).
        /// </summary>
        private void PlaceLinearObstacle(Room room, EnvironmentalFeature hazard, int targetTiles)
        {
            // Choose random direction (horizontal or vertical)
            bool horizontal = _[random.Next](http://random.Next)(2) == 0;
            int tilesPlaced = 0;

            if (horizontal)
            {
                // Bisect room horizontally
                int centerY = room.Height / 2;
                for (int x = 0; x < room.Width && tilesPlaced < targetTiles; x++)
                {
                    var tile = room.GetTile(x, centerY);
                    if (tile != null && !tile.HasHazard)
                    {
                        tile.AddFeature(hazard);
                        tilesPlaced++;
                    }
                }
            }
            else
            {
                // Bisect room vertically
                int centerX = room.Width / 2;
                for (int y = 0; y < room.Height && tilesPlaced < targetTiles; y++)
                {
                    var tile = room.GetTile(centerX, y);
                    if (tile != null && !tile.HasHazard)
                    {
                        tile.AddFeature(hazard);
                        tilesPlaced++;
                    }
                }
            }

            _log.Information(
                "Placed {Count} {Hazard} tiles (linear {Direction})",
                tilesPlaced,
                hazard.FeatureName,
                horizontal ? "horizontal" : "vertical"
            );
        }

        /// <summary>
        /// Place scattered terrain hazards (e.g., burning ground patches).
        /// </summary>
        private void PlaceScatteredTerrain(Room room, EnvironmentalFeature hazard, int targetTiles)
        {
            int tilesPlaced = 0;
            var availableTiles = room.Tiles.Where(t => !t.HasHazard).ToList();

            while (tilesPlaced < targetTiles && availableTiles.Count > 0)
            {
                // Pick random tile
                var tile = availableTiles[_[random.Next](http://random.Next)(availableTiles.Count)];
                tile.AddFeature(hazard);
                availableTiles.Remove(tile);
                tilesPlaced++;

                // 60% chance to place adjacent tile (create clusters)
                if (_random.NextDouble() < 0.60 && tilesPlaced < targetTiles)
                {
                    var adjacentTiles = room.GetAdjacentTiles(tile)
                        .Where(t => !t.HasHazard && availableTiles.Contains(t))
                        .ToList();

                    if (adjacentTiles.Any())
                    {
                        var adjTile = adjacentTiles[_[random.Next](http://random.Next)(adjacentTiles.Count)];
                        adjTile.AddFeature(hazard);
                        availableTiles.Remove(adjTile);
                        tilesPlaced++;
                    }
                }
            }

            _log.Information(
                "Placed {Count} {Hazard} tiles (scattered)",
                tilesPlaced,
                hazard.FeatureName
            );
        }

        // Additional placement methods: PlaceWallFeatures(), PlaceClusters(), PlaceNearHeatSources()
        // ... (similar patterns) ...
    }
}
```

---

## VII. Unit Tests

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using [RuneAndRust.Services](http://RuneAndRust.Services).Biomes;
using RuneAndRust.Core.Models;
using System.Collections.Generic;

namespace RuneAndRust.Tests.Biomes
{
    [TestClass]
    public class IntenseHeatServiceTests
    {
        private Mock<ILogger> _mockLog;
        private Mock<ResolveCheckService> _mockResolveCheckService;
        private Mock<DamageService> _mockDamageService;
        private Mock<BiomeStatusService> _mockBiomeStatusService;
        private IntenseHeatService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockLog = new Mock<ILogger>();
            _mockResolveCheckService = new Mock<ResolveCheckService>();
            _mockDamageService = new Mock<DamageService>();
            _mockBiomeStatusService = new Mock<BiomeStatusService>();

            _service = new IntenseHeatService(
                _mockLog.Object,
                _mockResolveCheckService.Object,
                _mockDamageService.Object,
                _mockBiomeStatusService.Object
            );
        }

        [TestMethod]
        public void IntenseHeat_SuccessfulCheck_NoDamage()
        {
            // Arrange
            var combatant = CreateCombatant("Test Warrior", sturdiness: 4);
            
            _mockResolveCheckService
                .Setup(s => s.MakeResolveCheck(combatant, AttributeType.STURDINESS, 12))
                .Returns(new ResolveCheckResult { Success = true, TotalRoll = 15 });

            // Act
            _service.ProcessEndOfTurnHeat(new List<Combatant> { combatant }, biomeId: 4);

            // Assert
            _mockDamageService.Verify(
                s => s.ApplyDamage(It.IsAny<Combatant>(), It.IsAny<int>(), It.IsAny<DamageType>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [TestMethod]
        public void IntenseHeat_FailedCheck_AppliesDamage()
        {
            // Arrange
            var combatant = CreateCombatant("Test Warrior", sturdiness: 1);
            combatant.SetResistance([DamageType.Fire](http://DamageType.Fire), 0); // No resistance

            _mockResolveCheckService
                .Setup(s => s.MakeResolveCheck(combatant, AttributeType.STURDINESS, 12))
                .Returns(new ResolveCheckResult { Success = false, TotalRoll = 8 });

            _mockDamageService
                .Setup(s => s.RollDamage(2, 6))
                .Returns(7); // Average roll

            // Act
            _service.ProcessEndOfTurnHeat(new List<Combatant> { combatant }, biomeId: 4);

            // Assert
            _mockDamageService.Verify(
                s => s.ApplyDamage(combatant, 7, [DamageType.Fire](http://DamageType.Fire), "[Intense Heat]"),
                Times.Once
            );
        }

        [TestMethod]
        public void ApplyFireResistance_50Percent_ReducesDamageByHalf()
        {
            // Arrange
            var combatant = CreateCombatant("Fire Resistant Warrior");
            combatant.SetResistance([DamageType.Fire](http://DamageType.Fire), 50);

            // Act
            int finalDamage = _service.ApplyFireResistance(combatant, rawDamage: 10);

            // Assert
            Assert.AreEqual(5, finalDamage);
        }

        [TestMethod]
        public void ApplyFireResistance_100Percent_ImmuneToDamage()
        {
            // Arrange
            var combatant = CreateCombatant("Fire Immune Warrior");
            combatant.SetResistance([DamageType.Fire](http://DamageType.Fire), 100);

            // Act
            int finalDamage = _service.ApplyFireResistance(combatant, rawDamage: 10);

            // Assert
            Assert.AreEqual(0, finalDamage);
        }

        [TestMethod]
        public void ApplyFireResistance_0Percent_FullDamage()
        {
            // Arrange
            var combatant = CreateCombatant("Unprotected Warrior");
            combatant.SetResistance([DamageType.Fire](http://DamageType.Fire), 0);

            // Act
            int finalDamage = _service.ApplyFireResistance(combatant, rawDamage: 10);

            // Assert
            Assert.AreEqual(10, finalDamage);
        }

        [TestMethod]
        public void HasSufficientFireResistance_50Percent_ReturnsTrue()
        {
            // Arrange
            var combatant = CreateCombatant("Prepared Warrior");
            combatant.SetResistance([DamageType.Fire](http://DamageType.Fire), 50);

            // Act
            bool sufficient = _service.HasSufficientFireResistance(combatant, threshold: 50);

            // Assert
            Assert.IsTrue(sufficient);
        }

        [TestMethod]
        public void HasSufficientFireResistance_25Percent_ReturnsFalse()
        {
            // Arrange
            var combatant = CreateCombatant("Underprepared Warrior");
            combatant.SetResistance([DamageType.Fire](http://DamageType.Fire), 25);

            // Act
            bool sufficient = _service.HasSufficientFireResistance(combatant, threshold: 50);

            // Assert
            Assert.IsFalse(sufficient);
        }

        [TestMethod]
        public void IntenseHeat_MultipleCombatants_ProcessesAll()
        {
            // Arrange
            var combatants = new List<Combatant>
            {
                CreateCombatant("Warrior 1"),
                CreateCombatant("Warrior 2"),
                CreateCombatant("Warrior 3")
            };

            _mockResolveCheckService
                .Setup(s => s.MakeResolveCheck(It.IsAny<Combatant>(), AttributeType.STURDINESS, 12))
                .Returns(new ResolveCheckResult { Success = false, TotalRoll = 8 });

            _mockDamageService
                .Setup(s => s.RollDamage(2, 6))
                .Returns(7);

            // Act
            _service.ProcessEndOfTurnHeat(combatants, biomeId: 4);

            // Assert
            _mockDamageService.Verify(
                s => s.ApplyDamage(It.IsAny<Combatant>(), It.IsAny<int>(), [DamageType.Fire](http://DamageType.Fire), "[Intense Heat]"),
                Times.Exactly(3)
            );
        }

        [TestMethod]
        public void IntenseHeat_PlayerDeath_IncrementsDeathCounter()
        {
            // Arrange
            var player = CreatePlayerCharacter("Doomed Warrior", currentHP: 5);
            player.SetResistance([DamageType.Fire](http://DamageType.Fire), 0);

            _mockResolveCheckService
                .Setup(s => s.MakeResolveCheck(player, AttributeType.STURDINESS, 12))
                .Returns(new ResolveCheckResult { Success = false, TotalRoll = 8 });

            _mockDamageService
                .Setup(s => s.RollDamage(2, 6))
                .Returns(10); // Lethal damage

            _mockDamageService
                .Setup(s => s.ApplyDamage(player, 10, [DamageType.Fire](http://DamageType.Fire), "[Intense Heat]"))
                .Callback(() => player.CurrentHP = 0); // Simulate death

            // Act
            _service.ProcessEndOfTurnHeat(new List<Combatant> { player }, biomeId: 4);

            // Assert
            _mockBiomeStatusService.Verify(
                s => s.IncrementHeatDeaths(player.CharacterId, 4),
                Times.Once
            );
        }

        [TestMethod]
        public void IntenseHeat_TracksHeatDamage_InBiomeStatus()
        {
            // Arrange
            var player = CreatePlayerCharacter("Test Warrior");
            player.SetResistance([DamageType.Fire](http://DamageType.Fire), 0);

            _mockResolveCheckService
                .Setup(s => s.MakeResolveCheck(player, AttributeType.STURDINESS, 12))
                .Returns(new ResolveCheckResult { Success = false, TotalRoll = 8 });

            _mockDamageService
                .Setup(s => s.RollDamage(2, 6))
                .Returns(7);

            // Act
            _service.ProcessEndOfTurnHeat(new List<Combatant> { player }, biomeId: 4);

            // Assert
            _mockBiomeStatusService.Verify(
                s => s.IncrementHeatDamage(player.CharacterId, 4, 7),
                Times.Once
            );
        }

        private Combatant CreateCombatant(string name, int sturdiness = 2)
        {
            return new Combatant
            {
                Name = name,
                MaxHP = 50,
                CurrentHP = 50,
                Attributes = new Attributes { STURDINESS = sturdiness }
            };
        }

        private PlayerCharacter CreatePlayerCharacter(string name, int currentHP = 50)
        {
            return new PlayerCharacter
            {
                CharacterId = 1,
                Name = name,
                MaxHP = 50,
                CurrentHP = currentHP,
                Attributes = new Attributes { STURDINESS = 2 }
            };
        }
    }
}
```

---

## VIII. v5.0 Setting Compliance

✅ **Technology, Not Magic:**

- "Thermal regulation failure," "containment breach," "heat exchanger malfunction"
- Steam vents are "pressure relief valves," not elemental magic
- Gas pockets are "combustible industrial byproducts," not supernatural
- All damage is physics-based (thermal energy transfer)

✅ **Layer 2 Voice:**

- "Superheated steam," "thermal load," "ablative shielding"
- "Structural instability," "molten slag," "heat-warped plating"
- Industrial disaster terminology throughout

✅ **ASCII-Only Entity Names:**

- All hazard names use ASCII brackets: [Intense Heat], [Burning Ground]
- No special characters in database entity names
- "Jötun-Forged" stored as "Jotun-Forged" (display-only Unicode)

✅ **v2.0 Canonical Accuracy:**

- [Intense Heat] DC 12 STURDINESS check preserved
- 2d6 Fire damage on failure preserved
- Fire Resistance as survival necessity preserved
- Lava rivers as tactical hazards preserved

---

## IX. Known Limitations

1. **No Enemy Fire Resistance Data:** Forge-Hardened enemies defined in v0.29.3
2. **No Procedural Generation:** Room generation algorithms in v0.29.4
3. **No UI/Visual Effects:** Hazard rendering separate phase
4. **Simplified Hazard Placement:** Advanced WFC-based placement in v0.29.4
5. **No Brittleness Mechanic Yet:** [Brittle] debuff (Ice → Physical vuln) in v0.29.3

---

## X. Success Criteria

- [ ]  [Intense Heat] condition defined in database
- [ ]  All 8 environmental hazards defined
- [ ]  IntenseHeatService implemented with logging
- [ ]  HazardPlacementService implemented
- [ ]  Unit tests achieve 85%+ coverage
- [ ]  Integration with Tactical Grid verified
- [ ]  v5.0 setting compliance confirmed
- [ ]  ASCII-only entity names confirmed

---

## XI. Related Documents

**Parent:** v0.29: Muspelheim Biome Implementation[[1]](v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)

**Previous:** v0.29.1: Database Schema & Room Templates[[2]](v0%2029%201%20Database%20Schema%20&%20Room%20Templates%204459437f44df4cb2aa9f3c4a71efe23d.md)

**Next:** v0.29.3: Enemy Definitions & Spawn System

**Canonical:** v2.0 Muspelheim Biome[[3]](https://www.notion.so/Feature-Specification-The-Muspelheim-Biome-2a355eb312da80cdab65de771b57e414?pvs=21)

**Requirements:** MANDATORY[[4]](https://www.notion.so/MANDATORY-Specification-Requirements-Quality-Standards-a40022443c8b4abfb8cbd4882839447d?pvs=21)

---

**Phase 1 (Hazard Design): COMPLETE ✓**

**Phase 2 (Database Implementation): Ready to begin**