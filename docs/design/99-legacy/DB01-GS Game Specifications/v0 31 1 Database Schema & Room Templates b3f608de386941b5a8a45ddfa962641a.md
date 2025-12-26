# v0.31.1: Database Schema & Room Templates

Type: Technical
Description: Database schema and 8 room templates for Alfheim biome (Canopy only). Biome entry (biome_id: 6), 9 resources across tiers 2-5 including Fragment of the All-Rune.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.30.1 (Niflheim database reference)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.31: Alfheim Biome Implementation (v0%2031%20Alfheim%20Biome%20Implementation%20efa0af4639af46c19be493eb264c0489.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.31.1-DATABASE

**Parent Specification:** v0.31 Alfheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-12 hours

**Prerequisites:** v0.30.1 (Niflheim database structure as reference)

---

## I. Overview

This specification defines the complete database foundation for the Alfheim biome, including table schema, room templates, resource drops, and biome status tracking. This is the first of four v0.31 sub-specifications and must be completed before environmental systems (v0.31.2), enemies (v0.31.3), or services (v0.31.4).

### Core Deliverables

- **Biomes table extension** with Alfheim entry (biome_id: 6)
- **8 Room Templates** (all for [Canopy] - high-altitude facilities only)
- **9 Resource Types** across 4 quality tiers
- **Biome_ResourceDrops** weighted spawn system
- **Complete SQL seeding scripts** for all content
- **Canopy-exclusive verticality support** for endgame positioning

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
    6,
    'Alfheim',
    'The Aetheric Spires - catastrophic failure of Pre-Glitch reality manipulation research. High-altitude energy conduits and Aetheric laboratories leak raw, uncontrolled energy. Not natural lightning, but industrial-scale physics manipulation gone catastrophically wrong.',
    'Canopy', -- Exclusively high-altitude
    NULL -- ambient_condition_id will be set when [Runic Instability] is created in v0.31.2
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

**8 Room Templates (All Canopy):**

### Canopy Templates (High-Altitude Energy Facilities)

**1. Aetheric Conduit Nexus**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    6,
    'Aetheric Conduit Nexus',
    'A vast chamber where energy conduits converge. The air crackles with unstable Aetheric discharge, arcing between crystalline structures in chaotic patterns. Pre-Glitch containment fields flicker erratically, creating zones of warped physics. The constant hum of overloaded systems is deafening.',
    'Canopy',
    'Large',
    1.2
);
```

**2. Reality-Warping Chamber**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    6,
    'Reality-Warping Chamber',
    'An experimental laboratory where Pre-Glitch scientists attempted to manipulate spacetime fabric. The equipment here creates localized physics anomalies - gravity fluctuates randomly, time seems to stutter, and solid surfaces phase between states. The research logs are corrupted beyond recovery.',
    'Canopy',
    'Medium',
    1.0
);
```

**3. All-Rune Proving Ground**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    6,
    'All-Rune Proving Ground',
    'The site where researchers attempted to compile the paradoxical All-Rune. The center of the room contains a frozen moment - a crystallized Reality Glitch that pulses with impossible colors. The walls are scored with runic inscriptions that shift and change when observed. This is ground zero of the Great Silence.',
    'Canopy',
    'Large',
    0.5 -- Rare, potential boss encounter location
);
```

**4. Holographic Archive**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    6,
    'Holographic Archive',
    'Pre-Glitch data storage facility using holographic projection technology. The projectors have malfunctioned, creating ghostly, translucent images that flicker through centuries of accumulated data. Some holograms have gained a disturbing semblance of agency, replaying the moments before the crash in endless loops.',
    'Canopy',
    'Medium',
    1.1
);
```

**5. Paradox Containment Field**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    6,
    'Paradox Containment Field',
    'A sealed chamber designed to contain the aftermath of failed Aetheric experiments. The containment has partially breached, leaking causality violations into the surrounding space. Objects exist in multiple states simultaneously, and the distinction between past and present events is blurred.',
    'Canopy',
    'Small',
    0.8
);
```

**6. Unstable Platform Array**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    6,
    'Unstable Platform Array',
    'A series of floating platforms held aloft by malfunctioning anti-gravity generators. The platforms flicker between solid and incorporeal states at random intervals. Below is an endless drop - the original floor collapsed eight hundred years ago. Navigation requires careful timing and nerve.',
    'Canopy',
    'Medium',
    1.0
);
```

**7. Crystalline Garden**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    6,
    'Crystalline Garden',
    'An observation dome where Aetheric energy has crystallized into bizarre, tree-like formations. These crystal structures resonate with each other, creating haunting harmonics. The growth patterns suggest something between geology and biology - reality itself attempting to heal by growing around the wound.',
    'Canopy',
    'Large',
    0.9
);
```

**8. Energy Regulator Station**

```sql
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES (
    6,
    'Energy Regulator Station',
    'The central control room for this sector s Aetheric power distribution. The regulators here are locked in catastrophic feedback loops, cycling between overload and collapse every few seconds. Touching any control surface risks severe electromagnetic exposure. The systems have been failing for centuries but will never fully shut down.',
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

**1. Aetheric Residue**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    6,
    'Aetheric Residue',
    'Elemental',
    2,
    1.5, -- Very common
    'Crystallized Aetheric energy from ruptured containment fields. Glows with unstable, multi-colored light. Essential component for Mystic ability enhancement and runic inscription. Still radiates detectable energy signatures after 800 years of discharge.'
);
```

**2. Energy Crystal Shard**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    6,
    'Energy Crystal Shard',
    'Mineral',
    2,
    1.2,
    'Fragment of crystalline structures formed by solidified Aetheric energy. Naturally resonates at specific frequencies. Used in precision equipment crafting and energy shielding components. The crystal lattice structure exhibits properties between mineral and energy state.'
);
```

**3. Unstable Aether Sample**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    6,
    'Unstable Aether Sample',
    'Specialized',
    3,
    0.8,
    'Contained sample of raw Aetheric energy extracted from active conduits. Highly volatile and requires specialized containment. Used in experimental Mystic equipment and advanced runic work. The energy signature fluctuates unpredictably - handle with extreme caution.'
);
```

**4. Holographic Data Fragment**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    6,
    'Holographic Data Fragment',
    'Mechanical',
    2,
    1.0,
    'Partially corrupted holographic storage medium from Pre-Glitch archives. Contains fragmentary data about Aetheric research protocols. Valuable to Jötun-Readers and scholars. The projection technology is remarkably intact despite eight centuries of system failure.'
);
```

**5. Paradox-Touched Component**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    6,
    'Paradox-Touched Component',
    'Specialized',
    3,
    0.7,
    'Mechanical component that has been exposed to causality violations and temporal anomalies. Exhibits unusual properties - exists in multiple quantum states simultaneously. Used in reality-manipulation equipment crafting. The physics around it feel fundamentally wrong.'
);
```

### Tier 4 (Rare)

**6. Pure Aether Shard**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    6,
    'Pure Aether Shard',
    'Specialized',
    4,
    0.4,
    'Perfectly crystallized Aetheric energy with stable resonance patterns. Acts as runic catalyst for Energy-based effects and Mystic ability amplification. Found only in the most stable areas of Alfheim. The shard maintains perfect energy equilibrium indefinitely - a small miracle of Pre-Glitch engineering.'
);
```

**7. Crystallized Aether**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    6,
    'Crystallized Aether',
    'Specialized',
    4,
    0.3,
    'Aetheric energy that has solidified into near-perfect crystal lattice structure. Contains immense stored energy in stable form. Essential for legendary Mystic equipment crafting. The crystallization process requires conditions that no longer exist - these are irreplaceable remnants.'
);
```

### Tier 5 (Legendary)

**8. Fragment of the All-Rune**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    6,
    'Fragment of the All-Rune',
    'Legendary',
    5,
    0.05, -- Extremely rare, boss drop or deepest chambers
    'A piece of the paradoxical rune that crashed reality. Contains crystallized fragments of the compiled paradox itself. Essential component for myth-forged Mystic equipment. Radiates impossible mathematics and contradictory energy signatures. Only obtainable from the All-Rune Proving Ground or All-Rune s Echo.'
);
```

**9. Reality Anchor Core**

```sql
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES (
    6,
    'Reality Anchor Core',
    'Legendary',
    5,
    0.03, -- Ultra rare
    'The stabilization core from a Pre-Glitch reality anchor device. One of the few remaining components that can impose coherence on chaotic Aetheric fields. Used in legendary equipment that protects against reality manipulation. The core maintains a bubble of stable physics around itself through unknown mechanisms.'
);
```

---

### D. SQL Migration Script

**Complete migration script for v0.31.1:**

```sql
-- v0.31.1 Alfheim Biome Database Migration
-- Execution: sqlite3 your_database.db < Data/v0.31.1_alfheim_schema.sql

BEGIN TRANSACTION;

-- 1. Add Alfheim biome entry
INSERT INTO Biomes (biome_id, biome_name, description, verticality_tier, ambient_condition_id)
VALUES (
    6,
    'Alfheim',
    'The Aetheric Spires - catastrophic failure of Pre-Glitch reality manipulation research. High-altitude energy conduits and Aetheric laboratories leak raw, uncontrolled energy.',
    'Canopy',
    NULL
);

-- 2. Add 8 room templates (all Canopy)
INSERT INTO Biome_RoomTemplates (biome_id, template_name, description, verticality_tier, size_category, spawn_weight)
VALUES 
-- CANOPY TEMPLATES
(6, 'Aetheric Conduit Nexus', 'A vast chamber where energy conduits converge. The air crackles with unstable Aetheric discharge...', 'Canopy', 'Large', 1.2),
(6, 'Reality-Warping Chamber', 'An experimental laboratory where Pre-Glitch scientists attempted to manipulate spacetime fabric...', 'Canopy', 'Medium', 1.0),
(6, 'All-Rune Proving Ground', 'The site where researchers attempted to compile the paradoxical All-Rune...', 'Canopy', 'Large', 0.5),
(6, 'Holographic Archive', 'Pre-Glitch data storage facility using holographic projection technology...', 'Canopy', 'Medium', 1.1),
(6, 'Paradox Containment Field', 'A sealed chamber designed to contain the aftermath of failed Aetheric experiments...', 'Canopy', 'Small', 0.8),
(6, 'Unstable Platform Array', 'A series of floating platforms held aloft by malfunctioning anti-gravity generators...', 'Canopy', 'Medium', 1.0),
(6, 'Crystalline Garden', 'An observation dome where Aetheric energy has crystallized into bizarre formations...', 'Canopy', 'Large', 0.9),
(6, 'Energy Regulator Station', 'The central control room for this sector s Aetheric power distribution...', 'Canopy', 'Medium', 0.8);

-- 3. Add 9 resource types
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_type, quality_tier, drop_weight, description)
VALUES
-- TIER 2-3
(6, 'Aetheric Residue', 'Elemental', 2, 1.5, 'Crystallized Aetheric energy from ruptured containment fields...'),
(6, 'Energy Crystal Shard', 'Mineral', 2, 1.2, 'Fragment of crystalline structures formed by solidified Aetheric energy...'),
(6, 'Unstable Aether Sample', 'Specialized', 3, 0.8, 'Contained sample of raw Aetheric energy extracted from active conduits...'),
(6, 'Holographic Data Fragment', 'Mechanical', 2, 1.0, 'Partially corrupted holographic storage medium from Pre-Glitch archives...'),
(6, 'Paradox-Touched Component', 'Specialized', 3, 0.7, 'Mechanical component that has been exposed to causality violations...'),
-- TIER 4
(6, 'Pure Aether Shard', 'Specialized', 4, 0.4, 'Perfectly crystallized Aetheric energy with stable resonance patterns...'),
(6, 'Crystallized Aether', 'Specialized', 4, 0.3, 'Aetheric energy that has solidified into near-perfect crystal lattice structure...'),
-- TIER 5
(6, 'Fragment of the All-Rune', 'Legendary', 5, 0.05, 'A piece of the paradoxical rune that crashed reality...'),
(6, 'Reality Anchor Core', 'Legendary', 5, 0.03, 'The stabilization core from a Pre-Glitch reality anchor device...');

COMMIT;
```

---

## III. Canopy-Exclusive Verticality

### Verticality Tier Rationale

Unlike Muspelheim (Roots-only) or Niflheim (both Roots and Canopy), Alfheim is **exclusively Canopy (100% chance)**:

**[Canopy] - High-Altitude Aetheric Facilities (100% chance)**

- Energy research laboratories
- Reality manipulation facilities
- Aetheric power conduits
- All-Rune Proving Ground locations
- Observatory and experimental platforms

**Why Canopy-Only:**

- Aetheric research required altitude (thinner atmosphere, less interference)
- Energy facilities needed direct access to atmospheric Aether
- Reality manipulation experiments were isolated at highest levels
- Endgame positioning: Alfheim is late-game, high-risk, high-reward
- Thematically: "Sky-labs" and "floating platforms" don't exist underground

**Implementation Note:**

The BiomeGenerationService will always use verticality_tier = "Canopy" when generating Alfheim sectors. No random selection needed.

---

## IV. v5.0 Setting Compliance

### Technology, Not Magic

**All room descriptions emphasize:**

- **Aetheric research failures** (not supernatural magic)
- **Containment breach and energy leakage** (not elemental curses)
- **Reality manipulation systems malfunction** (not wizardry)
- **Physics experiments gone wrong** (not mystical portals)

### Voice Layer Examples

**❌ v2.0 Language:**

- "Magical energy vortex"
- "Spellcasting research chamber"
- "Cursed reality tear"

**✅ v5.0 Language:**

- "Aetheric containment breach creates energy vortex"
- "Reality manipulation experimental chamber"
- "System crash manifestation - localized physics failure"

### ASCII Compliance

All entity names verified ASCII-only:

- ✅ "Alfheim" (no special characters)
- ✅ "All-Rune" (hyphen is ASCII)
- ✅ "Aether-Vulture" (hyphen is ASCII)
- ✅ "Reality Anchor Core" (all ASCII)

---

## V. Testing Requirements

### Database Integrity Tests

**Test 1: Biome Entry Exists**

```sql
SELECT COUNT(*) FROM Biomes WHERE biome_id = 6;
-- Expected: 1
```

**Test 2: Room Template Count**

```sql
SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 6;
-- Expected: 8 (all Canopy)
```

**Test 3: Verticality Verification**

```sql
SELECT verticality_tier, COUNT(*) 
FROM Biome_RoomTemplates 
WHERE biome_id = 6 
GROUP BY verticality_tier;
-- Expected: Canopy: 8 (no other tiers)
```

**Test 4: Resource Drop Count**

```sql
SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 6;
-- Expected: 9
```

**Test 5: Quality Tier Distribution**

```sql
SELECT quality_tier, COUNT(*) 
FROM Biome_ResourceDrops 
WHERE biome_id = 6 
GROUP BY quality_tier;
-- Expected: Tier 2: 3, Tier 3: 2, Tier 4: 2, Tier 5: 2
```

**Test 6: Drop Weight Validation**

```sql
SELECT resource_name, drop_weight 
FROM Biome_ResourceDrops 
WHERE biome_id = 6 
ORDER BY drop_weight DESC;
-- Expected: Aetheric Residue highest (1.5), legendaries lowest (0.05, 0.03)
```

---

## VI. Success Criteria

### Functional Requirements

- [ ]  Biomes table contains Alfheim entry (biome_id: 6)
- [ ]  8 room templates seeded (all Canopy)
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
- [ ]  Verticality tier correctly assigned (Canopy only)
- [ ]  Quality tiers match resource rarity

---

## VII. Deployment Instructions

### Step 1: Backup Database

```bash
cp your_database.db your_database_backup_pre_v0.31.1.db
```

### Step 2: Run Migration

```bash
sqlite3 your_database.db < Data/v0.31.1_alfheim_schema.sql
```

### Step 3: Verify Data

```bash
sqlite3 your_database.db
> SELECT * FROM Biomes WHERE biome_id = 6;
> SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 6;
> SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 6;
> .quit
```

### Step 4: Run Integrity Tests

```bash
dotnet test --filter "FullyQualifiedName~Alfheim.DatabaseIntegrityTests"
```

---

## VIII. Next Steps

Once v0.31.1 is complete and database seeding verified:

**Proceed to v0.31.2:** Environmental Hazards & Ambient Conditions

- [Runic Instability] ambient condition (Wild Magic Surges)
- [Psychic Resonance] high-intensity implementation
- 8+ hazard type definitions
- Reality Tear positioning system
- Integration with ConditionService and Trauma Economy

---

## IX. Related Documents

**Parent Specification:**

- v0.31: Alfheim Biome Implementation[[1]](v0%2031%20Alfheim%20Biome%20Implementation%20efa0af4639af46c19be493eb264c0489.md)

**Reference Implementation:**

- v0.29.1: Muspelheim Database Schema[[2]](v0%2029%201%20Database%20Schema%20&%20Room%20Templates%204459437f44df4cb2aa9f3c4a71efe23d.md)
- v0.30.1: Niflheim Database Schema[[3]](v0%2030%201%20Database%20Schema%20&%20Room%20Templates%208f251d9f2b39447299157b78b963d1ed.md)

**Prerequisites:**

- v0.10-v0.12: Dynamic Room Engine[[4]](https://www.notion.so/v0-10-Dynamic-Room-Engine-Core-f6b2626559d844d78fc65f1fe2c30798?pvs=21)
- Master Roadmap[[5]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)

---

**Database foundation complete. Proceed to environmental systems (v0.31.2).**