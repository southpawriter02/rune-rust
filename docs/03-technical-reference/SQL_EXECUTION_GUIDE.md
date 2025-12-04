# SQL Execution Guide
## Niflheim Biome Database Setup

**Version**: v0.30
**Purpose**: Populate full Niflheim biome content
**Time Required**: 5-10 minutes

---

## Prerequisites

- SQLite3 command-line tool installed
- Navigate to project root directory
- Database file will be created as `runeandrust.db`

---

## I. Quick Start (Recommended)

### Execute All SQL Files in Sequence

```bash
# Navigate to project root
cd /path/to/rune-rust

# Execute v0.30.1: Schema & Room Templates
sqlite3 runeandrust.db < Data/v0.30.1_niflheim_schema.sql

# Execute v0.30.2: Environmental Hazards
sqlite3 runeandrust.db < Data/v0.30.2_environmental_hazards.sql

# Execute v0.30.3: Enemy Definitions
sqlite3 runeandrust.db < Data/v0.30.3_enemy_definitions.sql
```

### Verification

```bash
# Verify all data loaded successfully
sqlite3 runeandrust.db <<EOF
SELECT 'Niflheim Biome:' as Label, COUNT(*) as Count
FROM Biomes WHERE biome_id = 5;

SELECT 'Room Templates:' as Label, COUNT(*) as Count
FROM Biome_RoomTemplates WHERE biome_id = 5;

SELECT 'Environmental Hazards:' as Label, COUNT(*) as Count
FROM Biome_EnvironmentalFeatures WHERE biome_id = 5;

SELECT 'Enemy Spawns:' as Label, COUNT(*) as Count
FROM Biome_EnemySpawns WHERE biome_id = 5;

SELECT 'Resources:' as Label, COUNT(*) as Count
FROM Biome_ResourceDrops WHERE biome_id = 5;

SELECT 'Conditions:' as Label, COUNT(*) as Count
FROM Conditions WHERE condition_id IN (105, 106);
EOF
```

**Expected Output**:
```
Label                    Count
--------------------     -----
Niflheim Biome:          1
Room Templates:          8
Environmental Hazards:   9
Enemy Spawns:            7
Resources:               9
Conditions:              2
```

---

## II. Detailed Step-by-Step

### Step 1: Create/Verify Database

```bash
# The database is auto-created on first run of the application
# Or create manually:
sqlite3 runeandrust.db "SELECT 1;"
```

### Step 2: Execute v0.30.1 (Schema & Templates)

```bash
sqlite3 runeandrust.db < Data/v0.30.1_niflheim_schema.sql
```

**What this adds**:
- Niflheim biome entry (biome_id: 5)
- 8 room templates:
  - 4 Roots templates (lower level)
  - 4 Canopy templates (upper level)
- 9 resources (Tier 2-5):
  - Cryo-Coolant Fluid, Frost-Lichen, Ice-Bear Pelt
  - Pristine Ice Core, Cryogenic Data-Slate
  - Heart of the Frost-Giant, Eternal Permafrost, Absolute Zero Fragment

**Verification**:
```bash
sqlite3 runeandrust.db "SELECT template_name FROM Biome_RoomTemplates WHERE biome_id = 5;"
```

**Expected**: 8 room names listed

---

### Step 3: Execute v0.30.2 (Environmental Hazards)

```bash
sqlite3 runeandrust.db < Data/v0.30.2_environmental_hazards.sql
```

**What this adds**:
- [Frigid Cold] ambient condition (condition_id: 105)
  - Ice Vulnerability (+50%)
  - Critical Hit Slow (2 turns)
  - Psychic Stress (+5 per combat)

- [Brittle] debuff condition (condition_id: 106)
  - Physical Vulnerability (+50%)
  - 1 turn duration (Niflheim variant)

- 9 environmental hazards:
  - Slippery Terrain (70% coverage - dominant)
  - Unstable Ceiling (Icicles)
  - Frozen Machinery (Cover)
  - Ice Boulders
  - Cryo-Vent
  - Brittle Ice Bridge
  - Frozen Corpse
  - Cryogenic Fog
  - Flash-Frozen Terminal

**Verification**:
```bash
sqlite3 runeandrust.db <<EOF
SELECT feature_name, hazard_density_category, tile_coverage_percent
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 5
ORDER BY tile_coverage_percent DESC;
EOF
```

**Expected**: Slippery Terrain at top with 70% coverage

---

### Step 4: Execute v0.30.3 (Enemy Definitions)

```bash
sqlite3 runeandrust.db < Data/v0.30.3_enemy_definitions.sql
```

**What this adds**:
- 5 enemy types with 7 spawn variants:
  - Frost-Rimed Undying (Roots + Canopy variants)
  - Cryo-Drone (Roots + Canopy variants)
  - Ice-Adapted Beast (Roots + Canopy variants)
  - Frost-Giant (Boss, Both tiers)
  - Forlorn Echo (Both tiers)

- JSON spawn rules including:
  - HP, resistances, vulnerabilities
  - Abilities and passives
  - Loot tables
  - Tags (ice_walker, brittle_eligible, etc.)

**Verification**:
```bash
sqlite3 runeandrust.db <<EOF
SELECT enemy_name, enemy_type, verticality_tier, spawn_weight
FROM Biome_EnemySpawns
WHERE biome_id = 5
ORDER BY spawn_weight DESC;
EOF
```

**Expected**: 7 rows, Frost-Rimed Undying with highest spawn_weight

---

## III. Troubleshooting

### Issue: "Error: no such table: Biomes"

**Cause**: Database tables not created yet

**Solution**: Run the application once to initialize database
```bash
dotnet run --project RuneAndRust.ConsoleApp
# Tables will be auto-created on first run
# Then execute SQL files
```

---

### Issue: "Error: UNIQUE constraint failed"

**Cause**: Data already inserted

**Solution**: This is expected behavior with `INSERT OR IGNORE`
- Data will not be duplicated
- Safe to re-run SQL files

---

### Issue: "File not found: Data/v0.30.1_niflheim_schema.sql"

**Cause**: Wrong directory

**Solution**: Ensure you're in project root
```bash
pwd
# Should show: /path/to/rune-rust

ls Data/v0.30.1_niflheim_schema.sql
# Should exist
```

---

## IV. Alternative: Manual SQL Execution

### Using SQLite3 Interactive Mode

```bash
sqlite3 runeandrust.db
```

```sql
-- Then paste contents of each SQL file manually
.read Data/v0.30.1_niflheim_schema.sql
.read Data/v0.30.2_environmental_hazards.sql
.read Data/v0.30.3_enemy_definitions.sql

-- Verify
SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 5;

-- Exit
.quit
```

---

## V. Database Schema Reference

### Key Tables

**Biomes**
- Stores biome metadata
- biome_id = 5 (Niflheim)
- Links to ambient_condition_id = 105 ([Frigid Cold])

**Biome_RoomTemplates**
- Procedural room templates
- Filtered by biome_id and z_level (Roots/Canopy)
- Used for WFC (Wave Function Collapse) generation

**Biome_EnvironmentalFeatures**
- Environmental hazards
- Filtered by hazard_density_category
- Spawned based on tile_coverage_percent

**Biome_EnemySpawns**
- Enemy spawn definitions
- Weighted random selection (spawn_weight)
- JSON spawn_rules_json contains full enemy data

**Biome_ResourceDrops**
- Loot table
- Weighted random selection (weight)
- Filtered by resource_tier

**Conditions**
- Status effects and ambient conditions
- condition_id 105 = [Frigid Cold]
- condition_id 106 = [Brittle]

**Condition_Effects**
- Individual effects for conditions
- Links to Conditions via condition_id

---

## VI. Post-Execution Checklist

After executing all SQL files:

- [ ] All 3 SQL files executed without errors
- [ ] Verification queries show expected counts
- [ ] Biome entry exists (biome_id = 5)
- [ ] 8 room templates loaded
- [ ] 9 environmental hazards loaded
- [ ] 7 enemy spawn variants loaded
- [ ] 9 resources loaded
- [ ] 2 conditions loaded ([Frigid Cold], [Brittle])
- [ ] Application runs without database errors
- [ ] Niflheim rooms can be generated (if generation implemented)

---

## VII. Data Integrity Checks

### Comprehensive Validation

```bash
sqlite3 runeandrust.db <<'EOF'
-- Check for orphaned data
SELECT 'Orphaned Room Templates' as Check, COUNT(*) as Issues
FROM Biome_RoomTemplates
WHERE biome_id NOT IN (SELECT biome_id FROM Biomes);

SELECT 'Orphaned Hazards' as Check, COUNT(*) as Issues
FROM Biome_EnvironmentalFeatures
WHERE biome_id NOT IN (SELECT biome_id FROM Biomes);

SELECT 'Orphaned Enemies' as Check, COUNT(*) as Issues
FROM Biome_EnemySpawns
WHERE biome_id NOT IN (SELECT biome_id FROM Biomes);

SELECT 'Orphaned Resources' as Check, COUNT(*) as Issues
FROM Biome_ResourceDrops
WHERE biome_id NOT IN (SELECT biome_id FROM Biomes);

-- Check for missing ambient condition link
SELECT 'Missing Ambient Condition' as Check, COUNT(*) as Issues
FROM Biomes
WHERE biome_id = 5 AND ambient_condition_id IS NULL;

-- Verify JSON validity (basic check)
SELECT 'Invalid JSON in Enemy Spawns' as Check, COUNT(*) as Issues
FROM Biome_EnemySpawns
WHERE biome_id = 5
  AND (spawn_rules_json IS NULL OR spawn_rules_json = '');

-- Check verticality distribution
SELECT 'Verticality Distribution' as Check, verticality_tier, COUNT(*) as Count
FROM Biome_RoomTemplates
WHERE biome_id = 5
GROUP BY verticality_tier;

-- Check enemy level ranges
SELECT 'Enemy Level Range Issues' as Check, COUNT(*) as Issues
FROM Biome_EnemySpawns
WHERE biome_id = 5
  AND min_level > max_level;
EOF
```

**Expected**: All "Issues" counts should be 0

---

## VIII. Backup & Rollback

### Backup Before Execution

```bash
# Create backup
cp runeandrust.db runeandrust.db.backup

# Execute SQL files
sqlite3 runeandrust.db < Data/v0.30.1_niflheim_schema.sql
# ... etc

# If something goes wrong, restore:
mv runeandrust.db.backup runeandrust.db
```

### Rollback Niflheim Data

```bash
sqlite3 runeandrust.db <<'EOF'
-- Remove all Niflheim data
DELETE FROM Biome_ResourceDrops WHERE biome_id = 5;
DELETE FROM Biome_EnemySpawns WHERE biome_id = 5;
DELETE FROM Biome_EnvironmentalFeatures WHERE biome_id = 5;
DELETE FROM Biome_RoomTemplates WHERE biome_id = 5;
DELETE FROM Condition_Effects WHERE condition_id IN (105, 106);
DELETE FROM Conditions WHERE condition_id IN (105, 106);
UPDATE Biomes SET ambient_condition_id = NULL WHERE biome_id = 5;
DELETE FROM Biomes WHERE biome_id = 5;
EOF
```

---

## IX. Production Deployment

### For Production Databases

1. **Test on Development Database First**
2. **Create Full Database Backup**
3. **Execute in Transaction** (if needed):

```bash
sqlite3 runeandrust.db <<'EOF'
BEGIN TRANSACTION;

-- Execute SQL file contents here
.read Data/v0.30.1_niflheim_schema.sql
.read Data/v0.30.2_environmental_hazards.sql
.read Data/v0.30.3_enemy_definitions.sql

-- Verify critical counts
SELECT CASE
    WHEN (SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 5) = 8
     AND (SELECT COUNT(*) FROM Biome_EnemySpawns WHERE biome_id = 5) = 7
     AND (SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 5) = 9
    THEN 'COMMIT'
    ELSE 'ROLLBACK'
END as Action;

-- If all counts correct, manually COMMIT
COMMIT;

-- If anything wrong, manually ROLLBACK
-- ROLLBACK;
EOF
```

---

## X. Next Steps

After successful SQL execution:

1. **Test Application Startup**: Verify no database errors in logs
2. **Test Room Generation**: Generate Niflheim rooms if system supports it
3. **Test Enemy Spawning**: Spawn Niflheim enemies
4. **Test Resource Drops**: Verify loot tables work
5. **Test Combat**: Enter Niflheim combat, verify mechanics
6. **Integration**: Follow v0.30.5_INTEGRATION_GUIDE.md for CombatEngine/MovementService hooks

---

**SQL Execution Complete!**

Your Niflheim biome is now fully populated in the database and ready for integration with the game systems.
