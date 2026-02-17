# v0.32.1: Database Schema & Room Templates

Type: Technical
Description: Database schema and 10 room templates for Jötunheim biome (7 Trunk, 3 Roots). Biome entry (biome_id: 7), 10 resources including Unblemished Jötun Plating.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.31.1 (Alfheim database reference)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.32: Jötunheim Biome Implementation (v0%2032%20J%C3%B6tunheim%20Biome%20Implementation%20ff37a5378c1a4e0ba7e00a263f293562.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.32.1-DATABASE

**Parent Specification:** v0.32 Jötunheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 10-14 hours

**Prerequisites:** v0.31.1 (Alfheim database structure as reference)

---

## I. Overview

This specification defines the complete database foundation for the Jötunheim biome, including table schema, room templates, resource drops, and biome status tracking. This is the first of four v0.32 sub-specifications and must be completed before environmental systems (v0.32.2), enemies (v0.32.3), or services (v0.32.4).

### Core Deliverables

- **Biomes table extension** with Jötunheim entry (biome_id: 7)
- **10+ Room Templates** (7 Trunk, 3 Roots - 70/30 split)
- **10 Resource Types** across 4 quality tiers
- **Biome_ResourceDrops** weighted spawn system
- **Complete SQL seeding scripts** for all content
- **Trunk/Roots verticality support** for mid-game positioning

---

## II. Database Schema

### A. Biomes Table Extension

**Existing Table Structure:**

```sql
CREATE TABLE IF NOT EXISTS Biomes (
    biome_id INTEGER PRIMARY KEY,
    biome_name TEXT NOT NULL UNIQUE,
    description TEXT,
    verticality_tier TEXT CHECK(verticality_tier IN ('Roots', 'Trunk', 'Canopy', 'Multiple')),
    ambient_condition_id INTEGER,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ambient_condition_id) REFERENCES Conditions(condition_id)
);
```

**New Entry:**

```sql
INSERT INTO Biomes (biome_id, biome_name, description, verticality_tier, ambient_condition_id)
VALUES (
    7,
    'Jotunheim', -- Internal ASCII storage
    'The Assembly Yards - colossal industrial manufacturing sector where Jötun-Forged were built. Vast factory floors, silent assembly lines, and the titanic corpses of dead metal giants. 800 years of rust and decay. No ambient condition - threats are physical and technological.',
    'Multiple', -- Can be Trunk or Roots
    NULL -- No ambient condition
);
```

**Display Layer Note:**

The database stores "Jotunheim" (ASCII), but the display layer renders "Jötunheim" (with ö) for all user-facing content.

---

### B. Biome_RoomTemplates Table

**Table Structure:**

```sql
CREATE TABLE IF NOT EXISTS Biome_RoomTemplates (
    template_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    template_name TEXT NOT NULL,
    description TEXT,
    verticality_tier TEXT CHECK(verticality_tier IN ('Roots', 'Trunk', 'Canopy')),
    size_category TEXT CHECK(size_category IN ('Small', 'Medium', 'Large')),
    spawn_weight REAL DEFAULT 1.0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id)
);
```

**10 Room Templates (7 Trunk, 3 Roots):**

### Trunk Templates (70% of spawn weight)

**1. Primary Assembly Line**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    7,
    'Primary Assembly Line',
    'A colossal, silent conveyor belt stretches into the darkness. Robotic arms hang motionless above the line, frozen mid-assembly. The floor is littered with half-finished components and 800 years of accumulated rust. The air smells of oil and ozone. This is where giants were born.',
    'Trunk',
    'Large',
    1.5
);
```

**2. Jötun Umbilical Gantry**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    7,
    'Jotun Umbilical Gantry',
    'The massive power and data cables that once fed a Jötun-Forged during construction hang like dead vines. The gantry structure creates extreme verticality - you can see dozens of meters up and down through the lattice of rusted steel. Some cables still hum with residual power.',
    'Trunk',
    'Medium',
    1.2
);
```

**3. Scrap Compactor Sector**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    7,
    'Scrap Compactor Sector',
    'Mountains of crushed and compacted metal dominate this sector. The compactor itself - a hydraulic press the size of a building - stands dormant. The ground is treacherous, shifting scrap that crunches and groans underfoot. Valuable components are mixed with worthless debris.',
    'Trunk',
    'Large',
    1.0
);
```

**4. Coolant Reservoir**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    7,
    'Coolant Reservoir',
    'A vast pool of stagnant, oily coolant fluid. The surface is covered with a rainbow-sheened film. Rusted catwalks provide precarious passage above the reservoir. The fluid is conductive - any electrical discharge here would be catastrophic. The smell is acrid and chemical.',
    'Trunk',
    'Medium',
    1.1
);
```

**5. Fallen Einherjar Torso-Cave**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    7,
    'Fallen Einherjar Torso-Cave',
    'You stand inside the hollow chest cavity of a dormant Jötun-Forged. Its outer hull forms the ceiling and walls of this chamber. Massive cables and servo-mechanisms create a maze of obstacles. The low-level psychic broadcast from its corrupted logic core creates a constant, oppressive hum.',
    'Trunk',
    'Large',
    0.8 -- Rare, special encounter location
);
```

**6. Command Deck Wreckage**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    7,
    'Command Deck Wreckage',
    'The elevated control station where Pre-Glitch engineers oversaw the assembly process. Most of the floor has collapsed, leaving only narrow catwalks and precarious platforms. Holographic displays flicker with corrupted data. The view from here shows the staggering scale of the facility.',
    'Trunk',
    'Medium',
    0.9
);
```

**7. Shipping Container Maze**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    7,
    'Shipping Container Maze',
    'Thousands of standardized cargo containers are stacked in chaotic piles. The original organization has long since collapsed into a labyrinth of rusted metal corridors. Some containers have burst open, spilling their contents. Others are sealed tight - their contents a mystery.',
    'Trunk',
    'Medium',
    1.3
);
```

### Roots Templates (30% of spawn weight)

**8. Maintenance Tunnel Network**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    7,
    'Maintenance Tunnel Network',
    'Cramped, claustrophobic passages beneath the factory floor. These tunnels once allowed maintenance crews to access the facility s infrastructure. Now they are flooded with ankle-deep coolant and echoing with the sounds of failing machinery. Rusted Servitors still patrol here.',
    'Roots',
    'Small',
    0.6
);
```

**9. Power Distribution Core**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    7,
    'Power Distribution Core',
    'Deep below the factory, the primary power grid still functions. Massive transformers and conduits hum with dangerous energy. The walls are lined with warning signs in Pre-Glitch script. This is the source of the facility s live power conduits - and the most dangerous sector.',
    'Roots',
    'Medium',
    0.5 -- Rare, dangerous
);
```

**10. Waste Reclamation Chamber**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    7,
    'Waste Reclamation Chamber',
    'The lowest level, where industrial waste was processed and recycled. The chamber is filled with toxic sludge and piles of corroded metal. The air is thick with chemical fumes. Few come here willingly, but the waste contains rare components that were deemed too damaged to use.',
    'Roots',
    'Medium',
    0.4 -- Very rare
);
```

---

### C. Biome_ResourceDrops Table

**Table Structure:**

```sql
CREATE TABLE IF NOT EXISTS Biome_ResourceDrops (
    drop_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    resource_name TEXT NOT NULL,
    resource_type TEXT CHECK(resource_type IN ('Mechanical', 'Organic', 'Mineral', 'Elemental', 'Specialized', 'Legendary')),
    quality_tier INTEGER CHECK(quality_tier BETWEEN 1 AND 5),
    drop_weight REAL DEFAULT 1.0,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id)
);
```

**10 Resource Types:**

### Tier 1-2 (Common)

**1. Rusted Scrap Metal**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    7,
    'Rusted Scrap Metal',
    'Mechanical',
    1,
    2.0, -- Very common
    'Heavily corroded metal fragments from the assembly yard. Low quality but abundant. Essential for basic repairs and Tier 1 crafting. The rust can be sanded off with effort. Salvaged from shipping containers, structural debris, and fallen machinery.'
);
```

**2. Intact Servomotor**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    7,
    'Intact Servomotor',
    'Mechanical',
    2,
    1.5,
    'Functional motor assembly salvaged from dormant machinery. Still operates despite 800 years of disuse - testament to Pre-Glitch engineering. Used in Tier 2 mechanical crafting and equipment repairs. Tinkers prize these components.'
);
```

**3. Coolant Fluid**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    7,
    'Coolant Fluid',
    'Specialized',
    1,
    1.3,
    'Industrial coolant extracted from reservoir tanks. Conductive and mildly toxic. Used in alchemical brewing and as a reagent for electrical resistance treatments. Handle with care - prolonged exposure causes chemical burns.'
);
```

**4. Ball Bearings (Set)**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    7,
    'Ball Bearings',
    'Mechanical',
    1,
    1.4,
    'Precision-manufactured ball bearings from assembly line equipment. Surprisingly well-preserved despite the rust. Used in mechanical crafting and as a common reagent for movement-based equipment. Can also be scattered as improvised caltrops.'
);
```

**5. Hydraulic Cylinder**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    7,
    'Hydraulic Cylinder',
    'Mechanical',
    2,
    0.9,
    'Heavy-duty hydraulic component from industrial machinery. Contains pressurized fluid - exercise caution when disassembling. Used in crafting high-force mechanical equipment and siege weapons. The pressure system is remarkably intact.'
);
```

**6. Power Relay Circuit**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    7,
    'Power Relay Circuit',
    'Specialized',
    2,
    0.8,
    'Electronic circuit board from power distribution systems. Still functional - the solid-state components have survived the centuries. Essential for electrical equipment crafting and power management systems. Tinkers can repurpose these for various applications.'
);
```

### Tier 3 (Rare)

**7. Unblemished Jötun Plating**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    7,
    'Unblemished Jotun Plating',
    'Mechanical',
    3,
    0.3, -- Rare
    'Pristine hull plating salvaged from intact sections of a dormant Jötun-Forged. Lightweight yet incredibly durable - the alloy composition is a lost art. Used in high-tier armor crafting and legendary equipment. Each piece must be carefully extracted without damaging the giant.'
);
```

**8. Industrial Servo Actuator**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    7,
    'Industrial Servo Actuator',
    'Mechanical',
    3,
    0.4,
    'High-precision actuator from assembly line robotics. Capable of micro-adjustments at massive scale. Used in advanced mechanical crafting and prosthetic equipment. The control algorithms are partially corrupted but still salvageable.'
);
```

### Tier 4 (Legendary)

**9. Uncorrupted Power Coil**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    7,
    'Uncorrupted Power Coil',
    'Legendary',
    4,
    0.05, -- Extremely rare
    'A perfectly preserved power coil from the deepest power distribution cores. Contains a stable, high-density energy field. Essential component for myth-forged electrical weapons and legendary armor. Only found in the most dangerous sectors - the power stations that still function.'
);
```

**10. Jötun Logic Core Fragment**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    7,
    'Jotun Logic Core Fragment',
    'Legendary',
    4,
    0.03, -- Ultra rare
    'A shard of a Jötun-Forged central processing unit. Contains fragments of corrupted but immensely powerful computational logic. Used in legendary equipment crafting and advanced runic inscription. The psychic broadcast from this fragment is palpable - handle with caution.'
);
```

---

### D. SQL Migration Script

**Complete migration script for v0.32.1:**

```sql
-- v0.32.1 Jötunheim Biome Database Migration
-- Execution: sqlite3 your_database.db < Data/v0.32.1_jotunheim_schema.sql

BEGIN TRANSACTION;

-- 1. Add Jötunheim biome entry
INSERT INTO Biomes (biome_id, biome_name, description, verticality_tier, ambient_condition_id)
VALUES (
    7,
    'Jotunheim',
    'The Assembly Yards - colossal industrial manufacturing sector where Jötun-Forged were built. Vast factory floors, silent assembly lines, and the titanic corpses of dead metal giants.',
    'Multiple',
    NULL
);

-- 2. Add 10 room templates (7 Trunk, 3 Roots)
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES 
-- TRUNK TEMPLATES (70%)
(7, 'Primary Assembly Line', 'A colossal, silent conveyor belt stretches into the darkness...', 'Trunk', 'Large', 1.5),
(7, 'Jotun Umbilical Gantry', 'The massive power and data cables that once fed a Jötun-Forged...', 'Trunk', 'Medium', 1.2),
(7, 'Scrap Compactor Sector', 'Mountains of crushed and compacted metal dominate this sector...', 'Trunk', 'Large', 1.0),
(7, 'Coolant Reservoir', 'A vast pool of stagnant, oily coolant fluid...', 'Trunk', 'Medium', 1.1),
(7, 'Fallen Einherjar Torso-Cave', 'You stand inside the hollow chest cavity of a dormant Jötun-Forged...', 'Trunk', 'Large', 0.8),
(7, 'Command Deck Wreckage', 'The elevated control station where Pre-Glitch engineers oversaw...', 'Trunk', 'Medium', 0.9),
(7, 'Shipping Container Maze', 'Thousands of standardized cargo containers are stacked...', 'Trunk', 'Medium', 1.3),
-- ROOTS TEMPLATES (30%)
(7, 'Maintenance Tunnel Network', 'Cramped, claustrophobic passages beneath the factory floor...', 'Roots', 'Small', 0.6),
(7, 'Power Distribution Core', 'Deep below the factory, the primary power grid still functions...', 'Roots', 'Medium', 0.5),
(7, 'Waste Reclamation Chamber', 'The lowest level, where industrial waste was processed...', 'Roots', 'Medium', 0.4);

-- 3. Add 10 resource types
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES
-- TIER 1-2
(7, 'Rusted Scrap Metal', 'Mechanical', 1, 2.0, 'Heavily corroded metal fragments from the assembly yard...'),
(7, 'Intact Servomotor', 'Mechanical', 2, 1.5, 'Functional motor assembly salvaged from dormant machinery...'),
(7, 'Coolant Fluid', 'Specialized', 1, 1.3, 'Industrial coolant extracted from reservoir tanks...'),
(7, 'Ball Bearings', 'Mechanical', 1, 1.4, 'Precision-manufactured ball bearings from assembly line equipment...'),
(7, 'Hydraulic Cylinder', 'Mechanical', 2, 0.9, 'Heavy-duty hydraulic component from industrial machinery...'),
(7, 'Power Relay Circuit', 'Specialized', 2, 0.8, 'Electronic circuit board from power distribution systems...'),
-- TIER 3
(7, 'Unblemished Jotun Plating', 'Mechanical', 3, 0.3, 'Pristine hull plating salvaged from intact sections of a dormant Jötun-Forged...'),
(7, 'Industrial Servo Actuator', 'Mechanical', 3, 0.4, 'High-precision actuator from assembly line robotics...'),
-- TIER 4
(7, 'Uncorrupted Power Coil', 'Legendary', 4, 0.05, 'A perfectly preserved power coil from the deepest power distribution cores...'),
(7, 'Jotun Logic Core Fragment', 'Legendary', 4, 0.03, 'A shard of a Jötun-Forged central processing unit...');

COMMIT;
```

---

## III. Trunk/Roots Verticality Split

### Verticality Tier Rationale

Unlike previous biomes with single verticality tiers, Jötunheim uses **Multiple** to support both Trunk and Roots:

**[Trunk] - Factory Floor Level (70% chance)**

- Primary assembly lines and gantries
- Fallen Jötun-Forged corpse terrain
- Shipping container storage
- Command deck structures
- Coolant reservoirs (surface level)

**[Roots] - Maintenance/Utility Level (30% chance)**

- Maintenance tunnel networks
- Power distribution cores
- Waste reclamation chambers
- Deep coolant pumping stations

**Why 70/30 Split:**

- Trunk represents the main factory floor (primary gameplay space)
- Roots represent dangerous utility areas (high-risk, high-reward)
- Matches v2.0 "primarily Trunk, with frequent descents into Roots"
- Creates variety without diluting the "massive scale" fantasy

**Implementation Note:**

The BiomeGenerationService will roll for verticality per room:

- 70% chance: Select from Trunk templates
- 30% chance: Select from Roots templates

---

## IV. v5.0 Setting Compliance

### Technology, Not Mythology

**All room descriptions emphasize:**

- **Industrial manufacturing** (not divine forges)
- **Pre-Glitch engineering** (not mystical craftsmanship)
- **800 years of rust and decay** (not eternal monuments)
- **Corrupted industrial systems** (not cursed machinery)

### Voice Layer Examples

**❌ v2.0 Language:**

- "Divine assembly grounds"
- "Where metal gods were born"
- "Sacred forge of the titans"

**✅ v5.0 Language:**

- "Industrial manufacturing sector"
- "Where Jötun-Forged terraforming units were assembled"
- "Pre-Glitch mega-scale construction facility"

### ASCII Compliance

**Internal Storage (Database):**

- "Jotunheim" (ASCII)
- "Jotun-Forged" (ASCII)
- "Jotun Umbilical Gantry" (ASCII)
- "Jotun Logic Core Fragment" (ASCII)

**Display Layer (User-Facing):**

- "Jötunheim" (with ö)
- "Jötun-Forged" (with ö)
- "Jötun Umbilical Gantry" (with ö)
- "Jötun Logic Core Fragment" (with ö)

---

## V. Testing Requirements

### Database Integrity Tests

**Test 1: Biome Entry Exists**

```sql
SELECT COUNT(*) FROM Biomes WHERE biome_id = 7;
-- Expected: 1
```

**Test 2: Room Template Count**

```sql
SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 7;
-- Expected: 10
```

**Test 3: Verticality Distribution**

```sql
SELECT verticality_tier, COUNT(*), SUM(spawn_weight) 
FROM Biome_RoomTemplates 
WHERE biome_id = 7 
GROUP BY verticality_tier;
-- Expected: Trunk: 7 templates (~70% weight), Roots: 3 templates (~30% weight)
```

**Test 4: Resource Drop Count**

```sql
SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 7;
-- Expected: 10
```

**Test 5: Quality Tier Distribution**

```sql
SELECT quality_tier, COUNT(*) 
FROM Biome_ResourceDrops 
WHERE biome_id = 7 
GROUP BY quality_tier;
-- Expected: Tier 1: 3, Tier 2: 3, Tier 3: 2, Tier 4: 2
```

**Test 6: Mechanical Type Dominance**

```sql
SELECT resource_type, COUNT(*) 
FROM Biome_ResourceDrops 
WHERE biome_id = 7 
GROUP BY resource_type;
-- Expected: Mechanical: 6+, Specialized: 2-3, Legendary: 2
```

---

## VI. Success Criteria

### Functional Requirements

- [ ]  Biomes table contains Jötunheim entry (biome_id: 7)
- [ ]  10 room templates seeded (7 Trunk, 3 Roots)
- [ ]  All templates have valid spawn weights
- [ ]  Spawn weight distribution creates 70/30 Trunk/Roots split
- [ ]  10 resource types seeded across 4 tiers
- [ ]  Resource drop weights correctly calibrated
- [ ]  SQL migration script executes without errors
- [ ]  All foreign key constraints satisfied

### Quality Gates

- [ ]  All descriptions use v5.0 voice (industrial, not mythological)
- [ ]  Entity names are ASCII-compliant internally
- [ ]  Database integrity tests pass
- [ ]  No NULL values in required fields
- [ ]  Spawn weights sum appropriately per category

### Data Validation

- [ ]  Room descriptions are 2-4 sentences each
- [ ]  Resource descriptions explain crafting usage
- [ ]  Legendary resources reference dangerous locations
- [ ]  Verticality tier correctly assigned (Trunk/Roots split)
- [ ]  Quality tiers match resource rarity
- [ ]  Mechanical components are dominant resource type

---

## VII. Deployment Instructions

### Step 1: Backup Database

```bash
cp your_database.db your_database_backup_pre_v0.32.1.db
```

### Step 2: Run Migration

```bash
sqlite3 your_database.db < Data/v0.32.1_jotunheim_schema.sql
```

### Step 3: Verify Data

```bash
sqlite3 your_database.db
> SELECT * FROM Biomes WHERE biome_id = 7;
> SELECT COUNT(*), verticality_tier FROM Biome_RoomTemplates WHERE biome_id = 7 GROUP BY verticality_tier;
> SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 7;
> .quit
```

### Step 4: Run Integrity Tests

```bash
dotnet test --filter "FullyQualifiedName~Jotunheim.DatabaseIntegrityTests"
```

---

## VIII. Next Steps

Once v0.32.1 is complete and database seeding verified:

**Proceed to v0.32.2:** Environmental Hazards & Industrial Terrain

- [Live Power Conduit] signature hazard (interacts with [Flooded])
- [High-Pressure Steam Vent] implementation
- [Unstable Ceiling/Wall] structural collapse
- 10+ hazard type definitions
- Integration with Environmental Combat

---

## IX. Related Documents

**Parent Specification:**

- v0.32: Jötunheim Biome Implementation[[1]](v0%2032%20J%C3%B6tunheim%20Biome%20Implementation%20ff37a5378c1a4e0ba7e00a263f293562.md)

**Reference Implementation:**

- v0.29.1: Muspelheim Database Schema[[2]](v0%2029%201%20Database%20Schema%20&%20Room%20Templates%204459437f44df4cb2aa9f3c4a71efe23d.md)
- v0.30.1: Niflheim Database Schema[[3]](v0%2030%201%20Database%20Schema%20&%20Room%20Templates%208f251d9f2b39447299157b78b963d1ed.md)
- v0.31.1: Alfheim Database Schema[[4]](v0%2031%201%20Database%20Schema%20&%20Room%20Templates%20b3f608de386941b5a8a45ddfa962641a.md)

**Prerequisites:**

- v0.10-v0.12: Dynamic Room Engine[[5]](https://www.notion.so/v0-10-Dynamic-Room-Engine-Core-f6b2626559d844d78fc65f1fe2c30798?pvs=21)
- Master Roadmap[[6]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)

---

**Database foundation complete. Proceed to environmental systems (v0.32.2).**