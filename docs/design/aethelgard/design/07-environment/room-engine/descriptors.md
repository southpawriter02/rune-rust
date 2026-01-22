---
id: SPEC-ROOMENGINE-DESCRIPTORS
title: "Descriptor System — Three-Tier Composition"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "data/schemas/v0.38.0_descriptor_framework_schema.sql"
    status: Active
  - path: "data/schemas/v0.38.1_room_description_library_schema.sql"
    status: Active
  - path: "data/descriptors/v0.38.1_descriptor_fragments_content.sql"
    status: Active
---

# Descriptor System — Three-Tier Composition

---

## 1. Overview

The descriptor system generates **unique, contextually-appropriate text** for rooms, features, and objects using a three-tier composition model:

```
┌─────────────────────────────────────────────────────────────┐
│ TIER 1: Base Templates                                      │
│   Biome-agnostic archetypes (Corridor_Base, Chamber_Base)   │
└─────────────────────────────────────────────────────────────┘
                          +
┌─────────────────────────────────────────────────────────────┐
│ TIER 2: Thematic Modifiers                                  │
│   Biome-specific variations (Scorched, Frozen, Rusted)      │
└─────────────────────────────────────────────────────────────┘
                          =
┌─────────────────────────────────────────────────────────────┐
│ TIER 3: Composite Descriptors                               │
│   Final generated text with placeholders filled             │
└─────────────────────────────────────────────────────────────┘
```

**Result:** Millions of unique descriptions from combinatorial variety.

---

## 2. Tier 1 — Base Templates

### 2.1 Template Structure

Base templates are biome-agnostic archetypes with placeholder tokens:

```json
{
  "template_name": "Corridor_Base",
  "category": "Room",
  "archetype": "Corridor",
  "name_template": "The {Modifier} Corridor",
  "description_template": "{Article_Cap} {Modifier_Adj} corridor stretches {Direction_Descriptor}. {Architectural_Feature}. {Spatial_Descriptor}. {Detail_1}. {Modifier_Detail}.",
  "base_mechanics": {
    "size": "Small",
    "min_exits": 2,
    "max_exits": 2,
    "spawn_budget_multiplier": 0.8
  }
}
```

### 2.2 Room Archetypes (15+)

| Archetype | Size | Exits | Purpose |
|-----------|------|-------|---------|
| Entry_Hall_Base | Medium | 1-2 | Safe starting room |
| Corridor_Base | Small | 2 | Linear transit |
| Chamber_Base | Large | 1-4 | Combat/exploration |
| Junction_Base | Medium | 3-4 | Branching point |
| Boss_Arena_Base | XLarge | 1 | Boss encounters |
| Secret_Room_Base | Small | 1 | Hidden rewards |
| Vertical_Shaft_Base | Medium | 2 | Z-level transit |
| Maintenance_Hub_Base | Medium | 2-4 | Utility junction |
| Storage_Bay_Base | Large | 1-2 | Salvage opportunity |
| Observation_Platform_Base | Medium | 1-2 | Tactical vantage |
| Power_Station_Base | Large | 1-3 | Electrical hazards |
| Laboratory_Base | Medium | 1-2 | Research facility |
| Barracks_Base | Medium | 1-2 | Enemy density |
| Forge_Chamber_Base | Large | 1-2 | Fire hazards |
| Cryo_Vault_Base | Large | 1-2 | Cold hazards |

### 2.3 Placeholder Tokens

| Token | Description | Example |
|-------|-------------|---------|
| `{Modifier}` | Thematic modifier name | "Rusted" |
| `{Modifier_Adj}` | Modifier adjective | "corroded" |
| `{Modifier_Detail}` | Modifier detail fragment | "shows decay" |
| `{Spatial_Descriptor}` | Room size/feel | "The ceiling presses low" |
| `{Architectural_Feature}` | Wall/ceiling/floor | "Pipes snake overhead" |
| `{Detail_1}`, `{Detail_2}` | Ambient details | "Rust flakes fall" |
| `{Direction_Descriptor}` | Direction phrase | "into darkness ahead" |
| `{Atmospheric_Detail}` | Sensory element | "smells of rust" |
| `{Function}` | Room function | "Pumping Station" |

---

## 3. Tier 2 — Thematic Modifiers

### 3.1 Modifier Structure

Modifiers apply biome-specific theming:

```json
{
  "modifier_name": "Rusted",
  "primary_biome": "The_Roots",
  "adjective": "corroded",
  "detail_fragment": "shows centuries of oxidation and decay",
  "stat_modifiers": {"hp_multiplier": 0.7, "brittle": true},
  "color_palette": "rust-brown-grey",
  "ambient_sounds": ["creaking metal", "dripping water"]
}
```

### 3.2 Modifiers by Biome

| Biome | Modifier | Adjective | Detail Fragment |
|-------|----------|-----------|-----------------|
| The Roots | Rusted | corroded | shows centuries of oxidation |
| Muspelheim | Scorched | scorched | radiates intense heat |
| Niflheim | Frozen | ice-covered | encased in thick frost |
| Alfheim | Crystalline | crystalline | impossible formations |
| Jötunheim | Monolithic | monolithic | massive, inhuman scale |

### 3.3 Mechanical Effects

Modifiers can alter gameplay:

| Modifier | Effect |
|----------|--------|
| Rusted | Objects have 0.7× HP, brittle |
| Scorched | Fire aura (2 damage/turn) |
| Frozen | Slippery surfaces, cold aura |
| Crystalline | Light source, dazzle chance |
| Monolithic | Increased scale factor |

---

## 4. Tier 3 — Composite Descriptors

### 4.1 Composition Process

```csharp
public string GenerateDescription(
    BaseTemplate template,
    ThematicModifier modifier,
    DescriptorFragments fragments,
    Random rng)
{
    var description = template.DescriptionTemplate;
    
    // Apply modifier tokens
    description = description
        .Replace("{Modifier}", modifier.Name)
        .Replace("{Modifier_Adj}", modifier.Adjective)
        .Replace("{Modifier_Detail}", modifier.DetailFragment);
    
    // Fill fragment placeholders
    description = description
        .Replace("{Spatial_Descriptor}", 
            SelectFragment(fragments, "SpatialDescriptor", template.Size, rng))
        .Replace("{Architectural_Feature}",
            SelectFragment(fragments, "ArchitecturalFeature", modifier.Biome, rng))
        .Replace("{Detail_1}",
            SelectFragment(fragments, "Detail", modifier.Biome, rng))
        .Replace("{Atmospheric_Detail}",
            SelectFragment(fragments, "Atmospheric", modifier.Biome, rng));
    
    return description;
}
```

### 4.2 Example Composition

**Input:**
- Base: `Corridor_Base`
- Modifier: `Rusted` (The Roots)

**Output:**
> "A corroded corridor stretches before you, narrowing into darkness. Corroded metal plates form the walls, held together by massive rivets. The ceiling presses low overhead. Rust flakes fall like snow from the ceiling. The space shows centuries of oxidation and decay."

---

## 5. Descriptor Fragments

### 5.1 Fragment Categories

| Category | Subcategories | Count |
|----------|---------------|-------|
| SpatialDescriptor | Confined, Vast, Vertical, Cramped | 8+ |
| ArchitecturalFeature | Wall, Ceiling, Floor | 12+ |
| Detail | Decay, Runes, Activity, Ominous, Loot | 28+ |
| Atmospheric | Smell, Sound, Light, Temperature | 155+ |
| Direction | Forward, Branching, Vertical | 6+ |

### 5.2 Fragment Selection

Fragments are selected via **weighted random** with tag filtering:

```csharp
public string SelectFragment(
    string category,
    string[] tags,
    Random rng)
{
    var candidates = _fragments
        .Where(f => f.Category == category)
        .Where(f => f.Tags.Intersect(tags).Any())
        .ToList();
    
    // Weighted random selection
    var totalWeight = candidates.Sum(f => f.Weight);
    var roll = rng.NextDouble() * totalWeight;
    
    foreach (var fragment in candidates)
    {
        roll -= fragment.Weight;
        if (roll <= 0) return fragment.Text;
    }
    
    return candidates.LastOrDefault()?.Text ?? "";
}
```

### 5.3 Sample Fragments

**Spatial Descriptors:**
- "The ceiling presses low overhead, and the walls feel uncomfortably close"
- "The chamber is vast, its far walls barely visible in the dim light"
- "The space extends dramatically upward, disappearing into darkness"

**Architectural Features (Walls):**
- "Corroded metal plates form the walls, held together by massive rivets"
- "The walls are reinforced with massive girders and support struts"
- "Smooth, seamless walls suggest advanced pre-Glitch fabrication"

**Detail (Decay):**
- "Rust streaks mark the surfaces like old blood"
- "Corrosion has eaten through many of the structural supports"
- "Everything here shows signs of advanced degradation"

---

## 6. Room Function Variants

### 6.1 Function Categories

Functional chambers have specialized purpose descriptors:

| Function | Archetype | Biome Affinity | Detail |
|----------|-----------|----------------|--------|
| Pumping Station | Chamber | The Roots | Geothermal fluid pumps |
| Forge Hall | Chamber | Muspelheim | Metalworking foundry |
| Cryo Storage | Chamber | Niflheim | Cryogenic preservation |
| Research Lab | Chamber | Alfheim | Aetheric experiments |
| Assembly Floor | Chamber | Jötunheim | Massive construction |
| Training Hall | Chamber | Any | Combat exercises |
| Operations Nexus | Junction | Any | Command center |
| Reactor Core | Chamber | Any | Power generation |

### 6.2 Function Detail Fragments

```json
{
  "function_name": "Pumping Station",
  "function_detail": "Massive geothermal pumps once circulated superheated fluids through the facility. Now they groan with neglect, leaking scalding steam from corroded seals.",
  "biome_affinity": ["The_Roots", "Muspelheim"]
}
```

---

## 7. Data Schema Summary

### 7.1 Tables

| Table | Purpose | Records |
|-------|---------|---------|
| Descriptor_Base_Templates | Tier 1 templates | 18+ |
| Descriptor_Thematic_Modifiers | Tier 2 modifiers | 5 |
| Descriptor_Composites | Tier 3 cached results | Generated |
| Descriptor_Fragments | Reusable text snippets | 60+ |
| Room_Function_Variants | Chamber functions | 18+ |

### 7.2 Schema References

- **Framework:** [v0.38.0_descriptor_framework_schema.sql](../../../data/schemas/v0.38.0_descriptor_framework_schema.sql)
- **Room Library:** [v0.38.1_room_description_library_schema.sql](../../../data/schemas/v0.38.1_room_description_library_schema.sql)
- **Fragments:** [v0.38.1_descriptor_fragments_content.sql](../../../data/descriptors/v0.38.1_descriptor_fragments_content.sql)
- **Atmospheric:** [v0.38.4_atmospheric_descriptors.sql](../../../data/descriptors/v0.38.4_atmospheric_descriptors.sql)


---

## 8. Extended Descriptor Libraries

Beyond rooms, the system includes descriptors for:

| Version | Category | Content |
|---------|----------|---------|
| v0.38.2 | Environmental Features | Hazard modifiers |
| v0.38.3 | Interactive Objects | Containers, terminals |
| v0.38.4 | Atmospheric | 155 sensory descriptors |
| v0.38.5 | Resource Nodes | Ore veins, salvage |
| v0.38.6 | Combat Flavor | Enemy voices, actions |
| v0.38.7 | Galdr/Magic | Spell manifestations |
| v0.38.8 | Status Effects | Effect text |
| v0.38.9 | Examination | Perception descriptors (see [examination.md](examination.md)) |
| v0.38.10 | Skill Usage | Skill action text |
| v0.38.11 | NPC Barks | Dialogue snippets |
| v0.38.12 | Combat Mechanics | Attack/defense text |
| v0.38.13 | Ambient Environmental | Background atmosphere |
| v0.38.14 | Trauma | Breaking point, recovery |

**Total: 1,000+ descriptors across 14 categories**

---

## 9. Service Interface

```csharp
public interface IRoomDescriptorService
{
    string GenerateRoomName(Room room);
    string GenerateRoomDescription(Room room);
    string GenerateFeatureDescription(Feature feature, string biome);
}

public interface IDescriptorRepository
{
    BaseTemplate GetBaseTemplate(string archetype);
    ThematicModifier GetModifier(string biome);
    IReadOnlyList<Fragment> GetFragments(string category, string[] tags);
    RoomFunction GetFunction(string functionName);
}
```

---

## 10. Variety Calculation

**Combinatorial variety:**

```
Base Templates:    18
Modifiers:         5
Functions:         18
Spatial Fragments: 8
Arch Fragments:    12
Detail Fragments:  28
Atmospheric:       155

Minimum combinations: 18 × 5 × 8 × 12 × 28 × 155 = ~47 million
```

## 11. Phased Implementation Guide

### Phase 1: Data Models
- [ ] **DTOs**: Define `BaseTemplate`, `ThematicModifier`, `Fragment` records.
- [ ] **JSON**: Create initial JSON files for 3 base templates (Corridor, Chamber, Hall).

### Phase 2: Core Logic
- [ ] **Service**: Implement `RoomDescriptorService.GenerateRoomDescription()`.
- [ ] **Composition**: Implement token replacement logic (`{Modifier}`, `{Detail}`).
- [ ] **Selection**: Implement weighted random fragment selection.

### Phase 3: Content Loading
- [ ] **Repository**: implement `DescriptorRepository` with SQL data loading.
- [ ] **Caching**: Implement simple caching for frequently used fragments.

### Phase 4: Integration
- [ ] **Room Engine**: Connect to `DungeonGenerator`.
- [ ] **CLI**: Add `Describe` command to test output generation.

---

## 12. Testing Requirements

### 12.1 Unit Tests
- [ ] **Replacement**: "Hello {Name}" + "World" -> "Hello World".
- [ ] **Missing Token**: "Hello {Missing}" -> Throws or returns placeholder error.
- [ ] **Weighting**: Run 1000 selections, verify distribution matches weights (+- 5%).
- [ ] **Context**: Filter by Biome="Ice" returns only Ice fragments.

### 12.2 Integration Tests
- [ ] **Full Gen**: Generate 50 rooms, verify no crashes or empty strings.
- [ ] **Database**: Verify all SQL files load correctly on startup.

### 12.3 Manual QA
- [ ] **Grammar**: Check generated sentences for article usage ("A/An").
- [ ] **Tone**: Verify horror/cyberpunk tone consistency.

---

## 13. Logging Requirements

**Reference:** [logging.md](../../00-project/logging.md)

### 13.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Generate | Info | "Generated description for {RoomID} using {Template}." | `RoomID`, `Template` |
| Missing | Warn | "Missing fragment for category {Category} in context {Context}." | `Category`, `Context` |
| Load | Info | "Loaded {Count} descriptor fragments." | `Count` |

---

## 14. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Phased Guide, Testing, and Logging |
