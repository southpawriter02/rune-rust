# v0.38: Descriptor Library & Content Database

Type: Technical
Description: Builds comprehensive descriptor library for procedural content generation with unified content database, room descriptions, environmental features, interactive objects, atmospheric descriptors, and loot templates. 14 child specifications, 60-80 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.10-v0.12 (Dynamic Room Engine), v0.29-v0.32 (Biome Implementations)
Implementation Difficulty: Very Complex
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.38.3: Interactive Object Repository (v0%2038%203%20Interactive%20Object%20Repository%205c7d98c706064bf6a80fe4884b73ecd1.md), v0.38.1: Room Description Library (v0%2038%201%20Room%20Description%20Library%209d6e2b68dce14be58c3a2d4cac8da5ad.md), v0.38.6: Combat & Action Flavor Text (v0%2038%206%20Combat%20&%20Action%20Flavor%20Text%20e6ccd00efda04c028597e25795875b33.md), v0.38.7: Ability & Galdr Flavor Text (v0%2038%207%20Ability%20&%20Galdr%20Flavor%20Text%20cf7408124c184f83b7417f004c4082b0.md), v0.38.8: Status Effects & Condition Descriptors (v0%2038%208%20Status%20Effects%20&%20Condition%20Descriptors%20252133e008fe4e35ab4eb43c523d2010.md), v0.38.9: Perception & Examination Descriptors (v0%2038%209%20Perception%20&%20Examination%20Descriptors%20689553ff002a4c3a98578ee4515350f4.md), v0.38.5: Loot & Resource Templates (v0%2038%205%20Loot%20&%20Resource%20Templates%2086c7a00fa10f457bbd8d739d98a1da91.md), v0.38.10: Skill Usage Flavor Text (v0%2038%2010%20Skill%20Usage%20Flavor%20Text%2003aa530035424b77831fbc91fe600f2a.md), v0.38.4: Atmospheric Descriptor System (v0%2038%204%20Atmospheric%20Descriptor%20System%20fb95f98ba9a24861841c4e0742c1367b.md), v0.38.2: Environmental Feature Catalog (v0%2038%202%20Environmental%20Feature%20Catalog%206ae105d2d4474d6e84171b7e52f79151.md), v0.38.13: Ambient Environmental Descriptors (v0%2038%2013%20Ambient%20Environmental%20Descriptors%202611f1ad55b84551a4fd8b584c97f760.md), v0.38.14: Trauma Manifestation Descriptors (v0%2038%2014%20Trauma%20Manifestation%20Descriptors%20f2b400b4ca9a43de970636228afa99e0.md), v0.38.12: Advanced Combat Mechanics Descriptors (v0%2038%2012%20Advanced%20Combat%20Mechanics%20Descriptors%20e3536a7bf4934641960bbbaab38e1c87.md), v0.38.11: NPC Descriptors & Dialogue Barks (v0%2038%2011%20NPC%20Descriptors%20&%20Dialogue%20Barks%20ff5ee375207c449b91d0f6e021bf9be6.md)
Template Validated: No
Voice Validated: No

**Status:** Design Phase

**Prerequisites:** v0.10-v0.12 (Dynamic Room Engine Complete), v0.29-v0.32 (Biome Implementations)

**Timeline:** 60-80 hours (8-10 weeks part-time)

**Goal:** Build comprehensive descriptor library for procedural content generation

**Philosophy:** Transform ad-hoc biome content into reusable, data-driven descriptor system

---

## I. Executive Summary

v0.38 creates a **comprehensive descriptor library** that powers the Dynamic Room Engine's procedural generation. Instead of building content piecemeal per biome (v0.29-v0.32), this version establishes a **unified content database** with reusable descriptors across all categories.

**What v0.38 Delivers:**

- Unified Descriptor Framework (parent specification)
- Room Description Library (v0.38.1)
- Environmental Feature Catalog (v0.38.2)
- Interactive Object Repository (v0.38.3)
- Atmospheric Descriptor System (v0.38.4)
- Loot & Resource Templates (v0.38.5)

**Strategic Purpose:**

The Dynamic Room Engine (v0.10-v0.12) relies on the `Biome_Elements` table to generate content. Currently, biome implementations (v0.29-v0.32) create these elements in isolation, leading to:

- ❌ Duplication ("Corroded Pillar" defined 4 times across biomes)
- ❌ Inconsistency (same concept, different mechanics)
- ❌ Maintenance burden (bug fix requires updating 4 biomes)
- ❌ Missed opportunities (generic descriptors locked to one biome)

**v0.38 Solution:**

Build a **master descriptor library** that biomes reference, not duplicate:

```
BEFORE (v0.29-v0.32):
Muspelheim defines: [Corroded Pillar] with Muspelheim-specific stats
Niflheim defines: [Ice-Covered Pillar] with Niflheim-specific stats  
Alfheim defines: [Crystalline Pillar] with Alfheim-specific stats

AFTER (v0.38):
Descriptor Library defines: [Pillar_Base] template
Biomes apply thematic variants:
- Muspelheim: [Pillar_Base] + [Scorched] + [Brittle]
- Niflheim: [Pillar_Base] + [Frozen] + [Slippery]
- Alfheim: [Pillar_Base] + [Crystalline] + [Glowing]
```

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.38)

- Descriptor Framework architecture
- Room Description Library (v0.38.1)
- Environmental Feature Catalog (v0.38.2)
- Interactive Object Repository (v0.38.3)
- Atmospheric Descriptor System (v0.38.4)
- Loot & Resource Templates (v0.38.5)
- Biome-agnostic base templates
- Thematic variant system
- Tag-based descriptor filtering
- Integration with existing Biome_Elements tables
- Database migration from v0.29-v0.32
- Unit tests (80%+ coverage)
- Serilog structured logging

### ❌ Explicitly Out of Scope

- New biome implementations (use existing v0.29-v0.32)
- Enemy/NPC descriptors (separate system)
- Combat mechanics changes (descriptors only)
- UI/rendering changes (data layer only)
- Quest system integration (defer to v0.38.6+)
- Save file format changes (backward compatible)

---

## III. Architecture Analysis

### Current Biome_Elements Structure (v0.10-v0.11)

From v0.10/v0.11 specifications:

```csharp
public class BiomeElement
{
    public string ElementName { get; set; }        // "Hissing Steam Pipe"
    public BiomeElementType ElementType { get; set; }  // DynamicHazard, etc.
    public float Weight { get; set; }              // Spawn probability
    public string AssociatedDataId { get; set; }   // ID of hazard/enemy/loot
    public string SpawnRules { get; set; }         // JSON constraints
}

public enum BiomeElementType
{
    RoomTemplate,
    DescriptionDetail,
    AmbientCondition,
    DormantProcess,      // Enemies
    DynamicHazard,       // Environmental dangers  
    StaticTerrain,       // Cover, obstacles
    LootNode,            // Resource veins, containers
    CoherentGlitchRule   // Environmental storytelling
}
```

**Analysis:**

- ✅ **Strengths:** Flexible, data-driven, weighted spawning
- ❌ **Weakness:** No template/variant system
- ❌ **Weakness:** AssociatedDataId couples tightly to specific implementations
- ❌ **Weakness:** No inheritance or composition model

### Current Biome Implementation Pattern (v0.29-v0.32)

From Muspelheim (v0.29) and Niflheim (v0.30):

```sql
-- Muspelheim defines:
Biome_RoomTemplates (biome_id, template_id, name, description, weight)
Biome_Environmental_Features (biome_id, feature_id, type, properties, weight)
Biome_Ambient_Conditions (biome_id, condition_name, effects, weight)

-- Niflheim defines (SAME TABLES, different data):
Biome_RoomTemplates (biome_id, template_id, name, description, weight) 
Biome_Environmental_Features (biome_id, feature_id, type, properties, weight)
Biome_Ambient_Conditions (biome_id, condition_name, effects, weight)
```

**Analysis:**

- ✅ **Strengths:** Per-biome customization, clear ownership
- ❌ **Weakness:** Massive duplication ("Pillar" defined 4+ times)
- ❌ **Weakness:** Cross-biome features require manual sync
- ❌ **Weakness:** Difficult to maintain consistency

---

## IV. v0.38 Framework Architecture

### A. Descriptor Hierarchy

**Three-tier model:**

```
Tier 1: BASE TEMPLATES (biome-agnostic archetypes)
  ├─ Room_Descriptor_Base
  ├─ Feature_Descriptor_Base  
  ├─ Object_Descriptor_Base
  ├─ Atmospheric_Descriptor_Base
  └─ Loot_Descriptor_Base

Tier 2: THEMATIC MODIFIERS (biome-specific variations)
  ├─ [Scorched] → Muspelheim
  ├─ [Frozen] → Niflheim
  ├─ [Crystalline] → Alfheim
  ├─ [Rusted] → The Roots
  └─ [Monolithic] → Jötunheim

Tier 3: COMPOSITE DESCRIPTORS (Base + Modifiers)
  ├─ [Pillar_Base] + [Scorched] = "Scorched Support Pillar"  
  ├─ [Pillar_Base] + [Frozen] = "Ice-Encased Pillar"
  └─ [Pillar_Base] + [Crystalline] = "Glowing Crystal Column"
```

### B. Database Schema (Master Tables)

```sql
-- =====================================================
-- TIER 1: BASE DESCRIPTOR TEMPLATES
-- =====================================================

CREATE TABLE Descriptor_Base_Templates (
    template_id INTEGER PRIMARY KEY AUTOINCREMENT,
    template_name TEXT NOT NULL UNIQUE,  -- "Pillar_Base", "Corridor_Base"
    category TEXT NOT NULL,  -- "Room", "Feature", "Object", "Atmospheric", "Loot"
    archetype TEXT NOT NULL,  -- "Cover", "Hazard", "Container", "Ambient"
    
    -- Mechanical properties (JSON)
    base_mechanics TEXT,  -- {"hp": 50, "soak": 10, "destructible": true}
    
    -- Description templates
    name_template TEXT,  -- "{Modifier} Support Pillar"
    description_template TEXT,  -- "A {Modifier_Adj} pillar {Modifier_Detail}."
    
    -- Metadata
    tags TEXT,  -- JSON array: ["Structure", "Cover", "Destructible"]
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    
    CHECK (category IN ('Room', 'Feature', 'Object', 'Atmospheric', 'Loot'))
);

-- =====================================================
-- TIER 2: THEMATIC MODIFIERS
-- =====================================================

CREATE TABLE Descriptor_Thematic_Modifiers (
    modifier_id INTEGER PRIMARY KEY AUTOINCREMENT,
    modifier_name TEXT NOT NULL UNIQUE,  -- "Scorched", "Frozen", "Rusted"
    primary_biome TEXT,  -- "Muspelheim", "Niflheim", "The_Roots"
    
    -- Modifier properties
    adjective TEXT,  -- "scorched", "ice-covered", "corroded"
    detail_fragment TEXT,  -- "radiates intense heat", "drips with meltwater"
    
    -- Mechanical modifiers (JSON)
    stat_modifiers TEXT,  -- {"fire_resistance": 0.5, "ice_vulnerability": 2.0}
    status_effects TEXT,  -- JSON array: [["Burning", 2]]
    
    -- Visual/atmospheric
    color_palette TEXT,  -- "red-orange-black"
    ambient_sounds TEXT,  -- JSON: ["hissing steam", "crackling flames"]
    
    CHECK (primary_biome IN ('The_Roots', 'Muspelheim', 'Niflheim', 'Alfheim', 'Jotunheim'))
);

-- =====================================================  
-- TIER 3: COMPOSITE DESCRIPTORS (instances)
-- =====================================================

CREATE TABLE Descriptor_Composites (
    composite_id INTEGER PRIMARY KEY AUTOINCREMENT,
    base_template_id INTEGER NOT NULL,
    modifier_id INTEGER,  -- NULL for unmodified base
    
    -- Generated properties
    final_name TEXT NOT NULL,  -- "Scorched Support Pillar"
    final_description TEXT NOT NULL,
    final_mechanics TEXT,  -- Merged base + modifier mechanics
    
    -- Spawn rules
    biome_restrictions TEXT,  -- JSON: ["Muspelheim", "The_Roots"]
    spawn_weight REAL DEFAULT 1.0,
    spawn_rules TEXT,  -- JSON: {"min_room_size": "Medium", "requires_tag": "Industrial"}
    
    FOREIGN KEY (base_template_id) REFERENCES Descriptor_Base_Templates(template_id),
    FOREIGN KEY (modifier_id) REFERENCES Descriptor_Thematic_Modifiers(modifier_id)
);

-- =====================================================
-- INTEGRATION: Biome_Elements (updated)
-- =====================================================

-- Extend existing Biome_Elements table
ALTER TABLE Biome_Elements ADD COLUMN composite_descriptor_id INTEGER;
ALTER TABLE Biome_Elements ADD COLUMN uses_composite BOOLEAN DEFAULT 0;

-- Migration: Biomes now reference composites instead of hardcoded data
-- AssociatedDataId can point to Descriptor_Composites OR legacy IDs
```

### C. Descriptor Service Architecture

```csharp
public interface IDescriptorService
{
    // Tier 1: Base templates
    DescriptorBaseTemplate GetBaseTemplate(string templateName);
    List<DescriptorBaseTemplate> GetBaseTemplatesByCategory(string category);
    
    // Tier 2: Modifiers
    ThematicModifier GetModifier(string modifierName);
    List<ThematicModifier> GetModifiersForBiome(string biomeName);
    
    // Tier 3: Composites
    DescriptorComposite ComposeDescriptor(string baseTemplateName, string modifierName);
    DescriptorComposite GetComposite(int compositeId);
    
    // Query
    List<DescriptorComposite> QueryDescriptors(DescriptorQuery query);
}

public class DescriptorQuery
{
    public string Category { get; set; }  // "Feature"
    public string Archetype { get; set; }  // "Cover"
    public string Biome { get; set; }  // "Muspelheim"
    public List<string> RequiredTags { get; set; }  // ["Destructible"]
    public List<string> ExcludedTags { get; set; }  
}
```

---

## V. Child Specifications Overview

### v0.38.1: Room Description Library

**Focus:** Room templates, corridors, chambers, architectural archetypes

**Base Templates:**

- Corridor_Base
- Chamber_Base
- Junction_Base
- Entry_Hall_Base
- Boss_Arena_Base

**Descriptors:**

- Architectural elements (walls, ceilings, floors)
- Spatial descriptors (claustrophobic, vast, vertical)
- Degradation states (pristine, decayed, collapsed)

**Thematic Variants:**

- [Geothermal] → hissing pipes, steam vents
- [Frozen] → ice sheets, frost patterns
- [Crystalline] → glowing formations

### v0.38.2: Environmental Feature Catalog

**Focus:** Static terrain, dynamic hazards, navigational obstacles

**Base Templates:**

- Pillar_Base (cover)
- Chasm_Base (obstacle)
- Elevation_Base (tactical)
- Hazard_Zone_Base (danger area)

**Descriptors:**

- Cover quality (partial/full)
- Obstacle traversal
- Hazard mechanics (damage, conditions)

**Thematic Variants:**

- [Lava] → Chasm_Base becomes lava river
- [Electrified] → Floor_Base becomes shocking trap

### v0.38.3: Interactive Object Repository

**Focus:** Levers, doors, containers, investigatable objects

**Base Templates:**

- Lever_Base
- Door_Base
- Container_Base
- Corpse_Base
- Data_Slate_Base

**Descriptors:**

- Interaction types (pull, open, search)
- Skill checks (WITS, MIGHT)
- Consequences (unlock, spawn, reveal)

### v0.38.4: Atmospheric Descriptor System

**Focus:** Ambient conditions, sensory details, environmental mood

**Base Templates:**

- Lighting_Base
- Sound_Base
- Smell_Base
- Temperature_Base
- Aetheric_Presence_Base

**Descriptors:**

- Light levels (dim, flickering, total darkness)
- Soundscapes (hissing, groaning, silence)
- Olfactory (ozone, rust, decay)
- Thermal (scorching, freezing, comfortable)
- Psychic pressure (resonance, instability)

### v0.38.5: Loot & Resource Templates

**Focus:** Resource nodes, containers, reward structures

**Base Templates:**

- Ore_Vein_Base
- Salvage_Wreck_Base
- Container_Loot_Base
- Boss_Drop_Base

**Descriptors:**

- Resource types (metals, organics, Aetheric)
- Rarity tiers (common, uncommon, rare)
- Extraction mechanics (mining, salvaging)

---

## VI. Migration Strategy

### Phase 1: Extract Patterns (from v0.29-v0.32)

```
Audit existing biomes:
- List all RoomTemplates
- List all Environmental_Features
- List all Ambient_Conditions
- List all Object definitions

Identify duplicates:
- "Pillar" appears in 4 biomes → Extract to Pillar_Base
- "Corroded" appears in 3 biomes → Extract to [Rusted] modifier
- "Steam Vent" appears in 2 biomes → Extract to Hazard_Zone_Base + [Geothermal]
```

### Phase 2: Create Base Templates

```sql
-- Example: Extract "Pillar" pattern
INSERT INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags
) VALUES (
    'Pillar_Base', 'Feature', 'Cover',
    '{"hp": 50, "soak": 8, "cover_quality": "Full", "destructible": true}',
    '{Modifier} Support Pillar',
    'A {Modifier_Adj} pillar that {Modifier_Detail}. It provides full cover.',
    '["Structure", "Cover", "Destructible"]'
);
```

### Phase 3: Create Modifiers

```sql
-- Example: Extract "Scorched" pattern from Muspelheim
INSERT INTO Descriptor_Thematic_Modifiers (
    modifier_name, primary_biome, adjective, detail_fragment,
    stat_modifiers, color_palette
) VALUES (
    'Scorched', 'Muspelheim', 'scorched',
    'radiates intense heat and shows signs of fire damage',
    '{"fire_resistance": 0.5, "fire_vulnerability": 0.0}',
    'red-orange-black'
);
```

### Phase 4: Generate Composites

```sql
-- Auto-generate composites from base + modifier combinations
INSERT INTO Descriptor_Composites (
    base_template_id, modifier_id,
    final_name, final_description, final_mechanics,
    biome_restrictions
)
SELECT 
    bt.template_id,
    tm.modifier_id,
    REPLACE([bt.name](http://bt.name)_template, '{Modifier}', tm.modifier_name),
    REPLACE(REPLACE(bt.description_template, 
        '{Modifier_Adj}', tm.adjective),
        '{Modifier_Detail}', tm.detail_fragment),
    MergeMechanics(bt.base_mechanics, tm.stat_modifiers),  -- Function
    '["' || tm.primary_biome || '"]'
FROM Descriptor_Base_Templates bt
CROSS JOIN Descriptor_Thematic_Modifiers tm;
```

### Phase 5: Update Biome_Elements

```sql
-- Update existing Biome_Elements to reference composites
UPDATE Biome_Elements
SET 
    composite_descriptor_id = (
        SELECT composite_id FROM Descriptor_Composites
        WHERE final_name = Biome_Elements.ElementName
    ),
    uses_composite = 1
WHERE ElementName IN (
    SELECT final_name FROM Descriptor_Composites
);
```

---

## VII. Integration with Dynamic Room Engine

### Updated DynamicRoomEngine Population

```csharp
public class DynamicRoomEngine
{
    private readonly IDescriptorService _descriptorService;
    
    public void PopulateRoomWithFeatures(Room room, BiomeDefinition biome)
    {
        // BEFORE (v0.11):
        // var elements = _biomeElementTable.GetElements(biome.BiomeId, "StaticTerrain");
        
        // AFTER (v0.38):
        var query = new DescriptorQuery
        {
            Category = "Feature",
            Archetype = "Cover",
            Biome = biome.BiomeId,
            RequiredTags = room.Tags  // Match room context
        };
        
        var descriptors = _descriptorService.QueryDescriptors(query);
        
        foreach (var descriptor in WeightedSelect(descriptors))
        {
            var feature = InstantiateFeature(descriptor);
            room.StaticTerrain.Add(feature);
        }
    }
    
    private StaticTerrainFeature InstantiateFeature(DescriptorComposite descriptor)
    {
        // Parse final_mechanics JSON
        var mechanics = JsonSerializer.Deserialize<FeatureMechanics>(
            descriptor.FinalMechanics);
        
        return new StaticTerrainFeature
        {
            Name = descriptor.FinalName,
            Description = descriptor.FinalDescription,
            HP = mechanics.HP,
            Soak = mechanics.Soak,
            CoverQuality = mechanics.CoverQuality,
            IsDestructible = mechanics.Destructible
        };
    }
}
```

---

## VIII. Success Criteria

**v0.38 is DONE when:**

### Framework (Parent Spec)

- [ ]  Descriptor_Base_Templates table created
- [ ]  Descriptor_Thematic_Modifiers table created
- [ ]  Descriptor_Composites table created
- [ ]  IDescriptorService interface defined
- [ ]  DescriptorService implementation complete
- [ ]  Query system functional
- [ ]  Migration scripts from v0.29-v0.32

### Room Description Library (v0.38.1)

- [ ]  15+ room base templates
- [ ]  20+ architectural descriptors
- [ ]  5+ thematic modifiers
- [ ]  50+ composite room descriptors

### Environmental Feature Catalog (v0.38.2)

- [ ]  10+ feature base templates
- [ ]  25+ feature descriptors
- [ ]  5+ hazard archetypes
- [ ]  40+ composite features

### Interactive Object Repository (v0.38.3)

- [ ]  8+ object base templates
- [ ]  15+ object descriptors
- [ ]  30+ composite objects

### Atmospheric Descriptor System (v0.38.4)

- [ ]  5+ atmospheric categories
- [ ]  30+ sensory descriptors
- [ ]  50+ composite atmospherics

### Loot & Resource Templates (v0.38.5)

- [ ]  6+ loot base templates
- [ ]  20+ resource descriptors
- [ ]  40+ composite loot nodes

### Integration

- [ ]  Dynamic Room Engine uses descriptor queries
- [ ]  All v0.29-v0.32 biomes migrated
- [ ]  Biome_Elements references composites
- [ ]  Backward compatible with existing saves
- [ ]  80%+ unit test coverage
- [ ]  Serilog logging throughout
- [ ]  No gameplay regressions

---

## IX. Timeline & Roadmap

**Phase 1: Framework (v0.38 Parent)** — 12-15 hours

- Database schema
- Service architecture
- Migration tools

**Phase 2: Room Descriptions (v0.38.1)** — 10-12 hours

- Extract room patterns
- Create base templates
- Generate composites

**Phase 3: Environmental Features (v0.38.2)** — 10-12 hours

- Extract feature patterns
- Hazard templates
- Terrain composites

**Phase 4: Interactive Objects (v0.38.3)** — 8-10 hours

- Extract object patterns
- Interaction mechanics
- Object composites

**Phase 5: Atmospheric System (v0.38.4)** — 8-10 hours

- Sensory descriptors
- Ambient conditions
- Atmospheric composites

**Phase 6: Loot Templates (v0.38.5)** — 8-10 hours

- Resource patterns
- Loot mechanics
- Reward composites

**Phase 7: Integration & Testing** — 8-12 hours

- Dynamic Room Engine integration
- Migration from v0.29-v0.32
- Testing and validation

**Total: 64-81 hours**

---

## X. Benefits

### For Development

- ✅ **DRY Principle:** Define once, use everywhere
- ✅ **Maintainability:** Fix bugs in one place
- ✅ **Extensibility:** New biomes trivial to add
- ✅ **Consistency:** Uniform mechanics across biomes

### For Gameplay

- ✅ **Variety:** Mix-and-match descriptors = exponential content
- ✅ **Coherence:** Consistent rules preserve "Coherent Glitch"
- ✅ **Balance:** Centralized tuning
- ✅ **Replayability:** More content permutations

### For Content Creation

- ✅ **Speed:** New biome = select descriptors, not write from scratch
- ✅ **Quality:** Refined templates > ad-hoc per-biome definitions
- ✅ **Creativity:** Focus on thematic modifiers, not mechanics

---

## XI. After v0.38 Ships

**You'll Have:**

- ✅ Comprehensive descriptor library (200+ descriptors)
- ✅ Reusable content templates
- ✅ Tag-based query system
- ✅ Thematic variant framework
- ✅ Migrated biomes using composites
- ✅ Foundation for infinite content expansion

**Next Steps:**

- **v0.39:** Enemy Descriptor Library (separate system)
- **v0.40:** Quest Anchor Templates
- **v0.41:** Biome Mixing & Transition Zones

**The descriptor library transforms content creation from "build everything" to "compose from library."**

---

**Ready to build the future of procedural content.**

[v0.38.3: Interactive Object Repository](v0%2038%20Descriptor%20Library%20&%20Content%20Database/v0%2038%203%20Interactive%20Object%20Repository%20e256b353218647da8d1e18cbcf550626.md)