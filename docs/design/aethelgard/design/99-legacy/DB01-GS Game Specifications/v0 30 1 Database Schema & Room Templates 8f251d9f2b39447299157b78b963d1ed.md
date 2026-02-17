# v0.30.1: Database Schema & Room Templates

Type: Technical
Description: Database schema and 8 room templates for Niflheim biome (4 Roots, 4 Canopy). Biome entry (biome_id: 5), dual verticality support, 9 resources across tiers 2-5.
Priority: Must-Have
Status: Implemented
Target Version: Alpha
Dependencies: v0.29.1 (Muspelheim database reference)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.30: Niflheim Biome Implementation (v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.30.1-DATABASE

**Parent Specification:** v0.30 Niflheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-12 hours

**Prerequisites:** v0.29.1 (Muspelheim database structure as reference)

---

## ✅ IMPLEMENTATION COMPLETE (2025-11-16)

**Status:** Implementation complete, all deliverables verified

**Actual Time:** ~8 hours

**File:** `Data/v0.30.1_niflheim_schema.sql`

**Deliverables Completed:**

- ✅ Biome entry created (biome_id: 5, dual verticality Roots/Canopy, level 7-12)
- ✅ 8 room templates with WFC adjacency rules (4 Roots, 4 Canopy)
- ✅ 9 resources across tiers 2-5 with drop rates
- ✅ Characters_BiomeStatus tracking extended

**Room Templates:**

- **Roots (4):** Cryo-Storage Bay, Coolant Pumping Station, Ice-Choked Maintenance Tunnel, The Frost-Giant Tomb
- **Canopy (4):** Flash-Frozen Skywalk, Rimed Laboratory, Atmospheric Control Chamber, High-Altitude Observatory

**Resources:**

- **Tier 2:** Cryo-Coolant Fluid, Frost-Lichen, Frozen Circuitry
- **Tier 3:** Ice-Bear Pelt, Supercooled Alloy
- **Tier 4:** Pristine Ice Core, Cryogenic Data-Slate
- **Tier 5 (Legendary):** Heart of the Frost-Giant, Eternal Frost Crystal

**Quality Gates Met:**

- ✅ v5.0 compliance (technology framing throughout)
- ✅ ASCII-only entity names
- ✅ WFC adjacency rules in JSON format
- ✅ Dual verticality support functional

---

## I. Overview

This specification defines the complete database foundation for the Niflheim biome, including table schema, room templates, resource drops, and biome status tracking. This is the first of four v0.30 sub-specifications and must be completed before environmental systems (v0.30.2), enemies (v0.30.3), or services (v0.30.4).

### Core Deliverables

- **Biomes table extension** with Niflheim entry (biome_id: 5)
- **8 Room Templates** (4 for [Roots], 4 for [Canopy])
- **9 Resource Types** across 4 quality tiers
- **Biome_ResourceDrops** weighted spawn system
- **Complete SQL seeding scripts** for all content
- **Dual verticality support** for [Roots] and [Canopy] generation

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
    5,
    'Niflheim',
    'The Cryo-Facilities - catastrophic coolant system failure has created permanent flash-frozen sectors. Deep cryogenic laboratories and high-altitude research outposts locked in absolute zero. Not natural winter, but industrial refrigeration run amok.',
    'Multiple', -- Can appear in both Roots and Canopy
    NULL -- ambient_condition_id will be set when [Frigid Cold] is created in v0.30.2
);
```

---

### B. Biome_RoomTemplates Table

**Table Structure:**

```sql
CREATE TABLE IF NOT EXISTS Biome_RoomTemplates (
    template_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    template_name TEXT NOT NULL,
    description TEXT,
    verticality_tier TEXT CHECK(verticality_tier IN ('Roots', 'Canopy')),
    size_category TEXT CHECK(size_category IN ('Small', 'Medium', 'Large')),
    spawn_weight REAL DEFAULT 1.0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id)
);
```

**8 Room Templates (4 Roots, 4 Canopy):**

### Roots Templates (Deep Cryogenic Facilities)

**1. Cryo-Storage Bay**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    5,
    'Cryo-Storage Bay',
    'Vast chambers lined with row upon row of cryogenic suspension pods, their contents frozen for eight centuries. Frost coats every surface in crystalline sheets. The air is so cold it feels like breathing shards of glass. Many pods have shattered, their occupants preserved in blocks of ice.',
    'Roots',
    'Large',
    1.2
);
```

**2. Coolant Pumping Station**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    5,
    'Coolant Pumping Station',
    'Industrial-scale refrigeration equipment dominates the space. Massive pumps stand silent, their coolant lines ruptured and leaking liquid nitrogen in slow, frozen cascades. The floor is a treacherous sheet of ice. Pressure gauges are frozen at critical readings.',
    'Roots',
    'Medium',
    1.0
);
```

**3. Ice-Choked Maintenance Tunnel**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    5,
    'Ice-Choked Maintenance Tunnel',
    'A narrow service corridor where coolant breach turned moisture into ice stalagmites and stalactites. The passage is treacherous, forcing careful navigation between jagged formations. Frozen condensation makes every surface slick. Emergency lighting flickers through layers of frost.',
    'Roots',
    'Small',
    0.8
);
```

**4. The Frost-Giant Tomb**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    5,
    'The Frost-Giant Tomb',
    'A colossal chamber where a dormant Jötun-Forged warmachine stands frozen in place, its systems locked in cryogenic stasis. Ice has formed around its massive frame like a crystal sarcophagus. The silence here is absolute, broken only by the occasional crack of shifting ice.',
    'Roots',
    'Large',
    0.5 -- Rare, potential boss encounter location
);
```

### Canopy Templates (High-Altitude Flash-Frozen Facilities)

**5. Flash-Frozen Skywalk**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    5,
    'Flash-Frozen Skywalk',
    'A transparent bridge connecting high-altitude research platforms. When atmospheric shields failed, everything froze instantly. Pre-Glitch scientists are preserved mid-stride, their final moments captured in ice. The view beyond shows a world locked in winter. The bridge itself is dangerously slippery.',
    'Canopy',
    'Small',
    1.0
);
```

**6. Rimed Laboratory**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    5,
    'Rimed Laboratory',
    'A high-tech research facility where atmospheric failure flash-froze everything. Holographic displays are frozen mid-projection. Equipment is coated in thick layers of rime. Data terminals are locked solid. The researchers bodies are preserved at their workstations, a moment of panic captured forever.',
    'Canopy',
    'Medium',
    1.1
);
```

**7. Atmospheric Control Chamber**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    5,
    'Atmospheric Control Chamber',
    'The heart of the environmental failure. Massive climate regulators are locked in catastrophic failure states. Warning klaxons are frozen mid-blare. The breach point where external cold flooded in is clearly visible, a jagged tear in the walls ringed with ice formations.',
    'Canopy',
    'Large',
    0.9
);
```

**8. High-Altitude Observatory**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    5,
    'High-Altitude Observatory',
    'A research outpost with panoramic views of the frozen world. Observation equipment is coated in frost. The exposed location means extreme cold and wind chill. Ice crystals have formed intricate patterns across every surface, beautiful but deadly.',
    'Canopy',
    'Medium',
    0.8
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

**9 Resource Types:**

### Tier 2-3 (Common-Uncommon)

**1. Cryo-Coolant Fluid**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    5,
    'Cryo-Coolant Fluid',
    'Mechanical',
    2,
    1.5, -- Very common
    'Supercooled liquid nitrogen from ruptured coolant lines. Essential for thermal regulation equipment and cryogenic component crafting. Still viable after 800 years of system failure.'
);
```

**2. Frost-Lichen**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    5,
    'Frost-Lichen',
    'Organic',
    2,
    1.2,
    'Hardy organism that thrives in extreme cold. Glows with faint bioluminescence. Used in cold resistance potions and medicinal compounds. Grows on frozen machinery and ice formations.'
);
```

**3. Ice-Bear Pelt**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    5,
    'Ice-Bear Pelt',
    'Organic',
    3,
    0.8,
    'Thick white fur from ice-adapted predators. Provides exceptional thermal insulation. Used in cold resistance armor and thermal suits. The fur has remarkable heat retention properties.'
);
```

**4. Frozen Circuitry**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    5,
    'Frozen Circuitry',
    'Mechanical',
    2,
    1.0,
    'Electronic components preserved by extreme cold. Supercooled circuits operate with reduced resistance. Used in precision equipment and advanced sensors. Surprisingly functional despite 800 years of freezing.'
);
```

**5. Supercooled Alloy**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    5,
    'Supercooled Alloy',
    'Mineral',
    3,
    0.7,
    'Metal alloy that has undergone phase changes at cryogenic temperatures. Exhibits unusual strength and brittleness. Used in specialized weapon and armor crafting. The material structure is unique to extreme cold exposure.'
);
```

### Tier 4 (Rare)

**6. Pristine Ice Core**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    5,
    'Pristine Ice Core',
    'Specialized',
    4,
    0.4,
    'Perfectly formed ice crystal with unique molecular structure. Acts as runic catalyst for Ice-based effects. Found in the deepest cryogenic chambers. The crystal maintains absolute zero temperature indefinitely.'
);
```

**7. Cryogenic Data-Slate**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    5,
    'Cryogenic Data-Slate',
    'Specialized',
    4,
    0.3,
    'Pre-Glitch research data perfectly preserved by flash-freezing. Contains atmospheric control protocols and climate regulation data. Valuable to Jötun-Readers and scholars. The information is coherent and uncorrupted.'
);
```

### Tier 5 (Legendary)

**8. Heart of the Frost-Giant**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    5,
    'Heart of the Frost-Giant',
    'Legendary',
    5,
    0.05, -- Extremely rare, boss drop
    'The core reactor of a dormant Jötun-Forged warmachine that was flash-frozen. Contains a self-sustaining cryogenic field. Essential component for legendary crafting. Radiates absolute zero in a contained field. Only obtainable from Frost-Giant encounters.'
);
```

**9. Eternal Frost Crystal**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    5,
    'Eternal Frost Crystal',
    'Legendary',
    5,
    0.03, -- Ultra rare
    'A perfect crystalline formation found only in the deepest cryo-storage chambers. Maintains absolute zero temperature through unknown physics. Used in myth-forged equipment with permanent cold effects. The crystal seems to violate thermodynamic laws.'
);
```

---

### D. SQL Migration Script

**Complete migration script for v0.30.1:**

```sql
-- v0.30.1 Niflheim Biome Database Migration
-- Execution: sqlite3 your_database.db < Data/v0.30.1_niflheim_schema.sql

BEGIN TRANSACTION;

-- 1. Add Niflheim biome entry
INSERT INTO Biomes (biome_id, biome_name, description, verticality_tier, ambient_condition_id)
VALUES (
    5,
    'Niflheim',
    'The Cryo-Facilities - catastrophic coolant system failure has created permanent flash-frozen sectors. Deep cryogenic laboratories and high-altitude research outposts locked in absolute zero.',
    'Multiple',
    NULL
);

-- 2. Add 8 room templates (4 Roots, 4 Canopy)
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES 
-- ROOTS TEMPLATES
(5, 'Cryo-Storage Bay', 'Vast chambers lined with row upon row of cryogenic suspension pods...', 'Roots', 'Large', 1.2),
(5, 'Coolant Pumping Station', 'Industrial-scale refrigeration equipment dominates the space...', 'Roots', 'Medium', 1.0),
(5, 'Ice-Choked Maintenance Tunnel', 'A narrow service corridor where coolant breach turned moisture into ice...', 'Roots', 'Small', 0.8),
(5, 'The Frost-Giant Tomb', 'A colossal chamber where a dormant Jötun-Forged warmachine stands frozen...', 'Roots', 'Large', 0.5),
-- CANOPY TEMPLATES
(5, 'Flash-Frozen Skywalk', 'A transparent bridge connecting high-altitude research platforms...', 'Canopy', 'Small', 1.0),
(5, 'Rimed Laboratory', 'A high-tech research facility where atmospheric failure flash-froze everything...', 'Canopy', 'Medium', 1.1),
(5, 'Atmospheric Control Chamber', 'The heart of the environmental failure. Massive climate regulators...', 'Canopy', 'Large', 0.9),
(5, 'High-Altitude Observatory', 'A research outpost with panoramic views of the frozen world...', 'Canopy', 'Medium', 0.8);

-- 3. Add 9 resource types
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES
-- TIER 2-3
(5, 'Cryo-Coolant Fluid', 'Mechanical', 2, 1.5, 'Supercooled liquid nitrogen from ruptured coolant lines...'),
(5, 'Frost-Lichen', 'Organic', 2, 1.2, 'Hardy organism that thrives in extreme cold...'),
(5, 'Ice-Bear Pelt', 'Organic', 3, 0.8, 'Thick white fur from ice-adapted predators...'),
(5, 'Frozen Circuitry', 'Mechanical', 2, 1.0, 'Electronic components preserved by extreme cold...'),
(5, 'Supercooled Alloy', 'Mineral', 3, 0.7, 'Metal alloy that has undergone phase changes at cryogenic temperatures...'),
-- TIER 4
(5, 'Pristine Ice Core', 'Specialized', 4, 0.4, 'Perfectly formed ice crystal with unique molecular structure...'),
(5, 'Cryogenic Data-Slate', 'Specialized', 4, 0.3, 'Pre-Glitch research data perfectly preserved by flash-freezing...'),
-- TIER 5
(5, 'Heart of the Frost-Giant', 'Legendary', 5, 0.05, 'The core reactor of a dormant Jötun-Forged warmachine...'),
(5, 'Eternal Frost Crystal', 'Legendary', 5, 0.03, 'A perfect crystalline formation found only in the deepest cryo-storage chambers...');

COMMIT;
```

---

## III. Dual Verticality Support

### Verticality Tier Selection Logic

Unlike Muspelheim (Roots-only), Niflheim can appear in both [Roots] and [Canopy]:

**[Roots] - Deep Cryogenic Facilities (60% chance)**

- Cryo-storage chambers
- Coolant pumping stations
- Underground laboratories
- Frost-Giant tomb locations

**[Canopy] - High-Altitude Flash-Frozen Facilities (40% chance)**

- Sky bridges and exposed platforms
- Research outposts
- Atmospheric control chambers
- Observatory stations

**Implementation Note:**

The BiomeGenerationService will randomly select verticality tier when generating Niflheim sectors, then filter room templates by matching verticality_tier value.

---

## IV. v5.0 Setting Compliance

### Technology, Not Magic

**All room descriptions emphasize:**

- **Coolant system failures** (not supernatural ice)
- **Flash-freezing from atmospheric shield collapse** (not winter magic)
- **Cryogenic equipment malfunction** (not ice curses)
- **Industrial refrigeration** (not elemental forces)

### Voice Layer Examples

**❌ v2.0 Language:**

- "Magical ice formations"
- "Frost magic permeates the air"
- "Cursed winter realm"

**✅ v5.0 Language:**

- "Coolant breach created ice formations"
- "Atmospheric shield failure flash-froze everything"
- "Cryogenic system malfunction locked the sector at absolute zero"

### ASCII Compliance

All entity names verified ASCII-only:

- ✅ "Niflheim" (no special characters)
- ✅ "Frost-Giant" (hyphen is ASCII)
- ✅ "Cryo-Drone" (hyphen is ASCII)
- ✅ "Jötun-Forged" (ö is handled at display layer)

---

## V. Testing Requirements

### Database Integrity Tests

**Test 1: Biome Entry Exists**

```sql
SELECT COUNT(*) FROM Biomes WHERE biome_id = 5;
-- Expected: 1
```

**Test 2: Room Template Count**

```sql
SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 5;
-- Expected: 8 (4 Roots, 4 Canopy)
```

**Test 3: Verticality Distribution**

```sql
SELECT verticality_tier, COUNT(*) 
FROM Biome_RoomTemplates 
WHERE biome_id = 5 
GROUP BY verticality_tier;
-- Expected: Roots: 4, Canopy: 4
```

**Test 4: Resource Drop Count**

```sql
SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 5;
-- Expected: 9
```

**Test 5: Quality Tier Distribution**

```sql
SELECT quality_tier, COUNT(*) 
FROM Biome_ResourceDrops 
WHERE biome_id = 5 
GROUP BY quality_tier;
-- Expected: Tier 2: 3, Tier 3: 2, Tier 4: 2, Tier 5: 2
```

**Test 6: Drop Weight Validation**

```sql
SELECT resource_name, drop_weight 
FROM Biome_ResourceDrops 
WHERE biome_id = 5 
ORDER BY drop_weight DESC;
-- Expected: Cryo-Coolant Fluid highest (1.5), legendaries lowest (0.05, 0.03)
```

---

## VI. Success Criteria

### Functional Requirements

- [ ]  Biomes table contains Niflheim entry (biome_id: 5)
- [ ]  8 room templates seeded (4 Roots, 4 Canopy)
- [ ]  All templates have valid spawn weights
- [ ]  9 resource types seeded across 4 tiers
- [ ]  Resource drop weights correctly calibrated
- [ ]  SQL migration script executes without errors
- [ ]  All foreign key constraints satisfied

### Quality Gates

- [ ]  All descriptions use v5.0 voice (technology, not magic)
- [ ]  Entity names are ASCII-compliant
- [ ]  Database integrity tests pass
- [ ]  No NULL values in required fields
- [ ]  Spawn weights sum appropriately per category

### Data Validation

- [ ]  Room descriptions are 2-4 sentences each
- [ ]  Resource descriptions explain crafting usage
- [ ]  Legendary resources reference boss/rare sources
- [ ]  Verticality tiers correctly assigned
- [ ]  Quality tiers match resource rarity

---

## VII. Deployment Instructions

### Step 1: Backup Database

```bash
cp your_database.db your_database_backup_pre_v0.30.1.db
```

### Step 2: Run Migration

```bash
sqlite3 your_database.db < Data/v0.30.1_niflheim_schema.sql
```

### Step 3: Verify Data

```bash
sqlite3 your_database.db
> SELECT * FROM Biomes WHERE biome_id = 5;
> SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 5;
> SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 5;
> .quit
```

### Step 4: Run Integrity Tests

```bash
dotnet test --filter "FullyQualifiedName~Niflheim.DatabaseIntegrityTests"
```

---

## VIII. Next Steps

Once v0.30.1 is complete and database seeding verified:

**Proceed to v0.30.2:** Environmental Hazards & Ambient Conditions

- [Frigid Cold] ambient condition
- [Slippery Terrain] mechanics
- 8+ hazard type definitions
- Integration with ConditionService

---

## IX. Related Documents

**Parent Specification:**

- v0.30: Niflheim Biome Implementation[[1]](v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)

**Reference Implementation:**

- v0.29.1: Muspelheim Database Schema[[2]](v0%2029%201%20Database%20Schema%20&%20Room%20Templates%204459437f44df4cb2aa9f3c4a71efe23d.md)

**Prerequisites:**

- v0.10-v0.12: Dynamic Room Engine[[3]](https://www.notion.so/v0-10-Dynamic-Room-Engine-Core-f6b2626559d844d78fc65f1fe2c30798?pvs=21)
- Master Roadmap[[4]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)

---

**Database foundation complete. Proceed to environmental systems (v0.30.2).**