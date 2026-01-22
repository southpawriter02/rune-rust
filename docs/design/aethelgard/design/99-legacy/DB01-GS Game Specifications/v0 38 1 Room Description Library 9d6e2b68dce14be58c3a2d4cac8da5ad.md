# v0.38.1: Room Description Library

Description: 15+ room base templates, 20+ architectural descriptors, 5+ thematic modifiers, 50+ composites
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.38, v0.10
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.38: Descriptor Library & Content Database (v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Specification:** [v0.38: Descriptor Library & Content Database](v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)

**Status:** Design Phase

**Timeline:** 10-12 hours

**Goal:** Build comprehensive room template library for Dynamic Room Engine

**Philosophy:** Define reusable architectural archetypes with thematic variants

---

## I. Purpose

v0.38.1 creates the **Room Description Library**, the foundational descriptor set for the Dynamic Room Engine's layout generation (v0.10). This library provides:

- **15+ Base Room Templates** (corridors, chambers, junctions)
- **20+ Architectural Descriptors** (walls, ceilings, spatial qualities)
- **5+ Thematic Modifiers** (geothermal, frozen, industrial)
- **50+ Composite Room Descriptors** (base + modifier combinations)

**Strategic Function:**

Currently, biome implementations (v0.29-v0.32) define rooms independently:

- Muspelheim: "The Scorched Corridor" (hardcoded)
- Niflheim: "The Frozen Passage" (hardcoded)
- Result: 4 biomes × 8 rooms = 32 separate definitions

**v0.38.1 transforms this to:**

- Base: `Corridor_Base` template
- Modifiers: `[Scorched]`, `[Frozen]`, `[Rusted]`, `[Crystalline]`
- Result: 1 base × 5 modifiers = 5 composites (covers all biomes)

---

## II. The Rule: What's In vs. Out

### ✅ In Scope

- Room base templates (15+)
- Architectural descriptors (walls, floors, ceilings)
- Spatial descriptors (claustrophobic, vast, vertical)
- Degradation states (pristine, decayed, collapsed)
- Thematic modifiers (5+)
- Room archetype system
- Composite generation logic
- Integration with v0.10 DungeonGenerator
- Database schema (Descriptor_Base_Templates rows)
- Unit tests (80%+ coverage)
- Serilog logging

### ❌ Explicitly Out of Scope

- Enemy placement (v0.11, separate system)
- Hazard placement (v0.38.2)
- Loot placement (v0.38.5)
- Interactive objects (v0.38.3)
- Ambient conditions (v0.38.4)
- Quest anchor templates (defer to v0.40)
- UI/rendering changes

---

## III. Room Archetype Taxonomy

### A. Five Core Archetypes

From v0.10 specification:

```csharp
public enum RoomArchetype
{
    EntryHall,      // Starting rooms (safer, tutorial)
    Corridor,       // Connectors (linear, transit)
    Chamber,        // Large rooms (combat, exploration)
    Junction,       // Branching points (3+ exits)
    BossArena,      // Final rooms (climactic encounters)
    SecretRoom      // Hidden/optional (rewards)
}
```

**Archetype Properties:**

| Archetype | Size | Min Exits | Max Exits | Purpose |
| --- | --- | --- | --- | --- |
| Entry Hall | Medium | 1 | 2 | Safe start, orientation |
| Corridor | Small | 2 | 2 | Linear transit |
| Chamber | Large | 1 | 4 | Combat/exploration hub |
| Junction | Medium | 3 | 4 | Branching decisions |
| Boss Arena | Large | 1 | 1 | Climactic encounter |
| Secret Room | Small-Med | 1 | 1 | Hidden rewards |

---

## IV. Base Template Definitions

### Template 1: Entry_Hall_Base

**Archetype:** EntryHall

**Category:** Room

**Tags:** `["Starting", "Safe", "Orientation"]`

**Base Mechanics:**

```json
{
  "size": "Medium",
  "min_exits": 1,
  "max_exits": 2,
  "spawn_budget_multiplier": 0.5,
  "ambient_danger_level": "Low"
}
```

**Name Template:**

`"The {Modifier} Entry Hall"`

**Description Template:**

```
"You enter {Article} {Modifier_Adj} entry hall. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Detail_2}. The air {Atmospheric_Detail}."
```

**Example Composites:**

- Entry_Hall_Base + [Rusted] = "The Rusted Entry Hall"
- Entry_Hall_Base + [Frozen] = "The Frozen Entry Hall"
- Entry_Hall_Base + [Scorched] = "The Scorched Entry Hall"

---

### Template 2: Corridor_Base

**Archetype:** Corridor

**Category:** Room

**Tags:** `["Transit", "Linear", "Narrow"]`

**Base Mechanics:**

```json
{
  "size": "Small",
  "min_exits": 2,
  "max_exits": 2,
  "spawn_budget_multiplier": 0.8,
  "ambient_danger_level": "Medium"
}
```

**Name Templates:**

```json
[
  "The {Modifier} Corridor",
  "The {Modifier} Passage",
  "The {Modifier} Hallway"
]
```

**Description Template:**

```
"{Article_Cap} {Modifier_Adj} corridor stretches {Direction_Descriptor}. {Architectural_Feature}. {Spatial_Descriptor}. {Detail_1}. {Modifier_Detail}."
```

**Example Composites:**

- Corridor_Base + [Rusted] + "Choked" variant = "The Rust-Choked Corridor"
- Corridor_Base + [Frozen] = "The Frozen Passage"
- Corridor_Base + [Crystalline] = "The Crystalline Hallway"

---

### Template 3: Chamber_Base

**Archetype:** Chamber

**Category:** Room

**Tags:** `["Large", "Combat", "Exploration"]`

**Base Mechanics:**

```json
{
  "size": "Large",
  "min_exits": 1,
  "max_exits": 4,
  "spawn_budget_multiplier": 1.2,
  "ambient_danger_level": "High"
}
```

**Name Templates:**

```json
[
  "The {Modifier} {Function} Chamber",
  "The {Modifier} {Function} Hall",
  "The {Function} {Modifier}"
]
```

**Function Variants:**

- "Pumping Station" (geothermal, industrial)
- "Storage Bay" (industrial, roots)
- "Observation Dome" (Alfheim, canopy)
- "Forge Hall" (Muspelheim)
- "Cryo Chamber" (Niflheim)

**Description Template:**

```
"{Article_Cap} {Modifier_Adj} {Function} dominates this space. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Detail_2}. {Function_Detail}. {Modifier_Detail}."
```

---

### Template 4: Junction_Base

**Archetype:** Junction

**Category:** Room

**Tags:** `["Branching", "Decision", "Navigation"]`

**Base Mechanics:**

```json
{
  "size": "Medium",
  "min_exits": 3,
  "max_exits": 4,
  "spawn_budget_multiplier": 1.0,
  "ambient_danger_level": "Medium"
}
```

**Name Templates:**

```json
[
  "The {Modifier} Junction",
  "The {Modifier} Crossroads",
  "The {Modifier} Hub"
]
```

**Description Template:**

```
"Multiple passages converge at this {Modifier_Adj} junction. {Spatial_Descriptor}. {Architectural_Feature}. {Exit_Description}. {Detail_1}."
```

---

### Template 5: Boss_Arena_Base

**Archetype:** BossArena

**Category:** Room

**Tags:** `["Climactic", "Large", "Arena"]`

**Base Mechanics:**

```json
{
  "size": "Large",
  "min_exits": 1,
  "max_exits": 1,
  "spawn_budget_multiplier": 0.0,
  "ambient_danger_level": "Extreme",
  "boss_spawn_required": true
}
```

**Name Templates:**

```json
[
  "The {Modifier} Arena",
  "The {Function} Core",
  "The Heart of {Location}"
]
```

**Description Template:**

```
"{Article_Cap} vast {Modifier_Adj} chamber opens before you. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Detail_2}. {Ominous_Detail}. This is clearly a place of significance."
```

---

### Template 6-15: Additional Base Templates

**6. Secret_Room_Base** - Hidden rewards

**7. Vertical_Shaft_Base** - Canopy/Roots transit

**8. Maintenance_Hub_Base** - Industrial function rooms

**9. Storage_Bay_Base** - Supply/resource rooms

**10. Observation_Platform_Base** - Elevated vantage

**11. Power_Station_Base** - Energy infrastructure

**12. Laboratory_Base** - Research/Alfheim

**13. Barracks_Base** - Military/security

**14. Forge_Chamber_Base** - Muspelheim specific

**15. Cryo_Vault_Base** - Niflheim specific

---

## V. Architectural Descriptor Components

### A. Spatial Descriptors

**Purpose:** Convey room dimensions and feel

**Descriptor Set:**

```json
{
  "claustrophobic": "The ceiling presses low overhead, and the walls feel uncomfortably close.",
  "vast": "The chamber is vast, its far walls barely visible in the dim light.",
  "vertical": "The space extends dramatically upward, disappearing into darkness above.",
  "cramped": "There's barely room to maneuver in this tight space.",
  "cavernous": "The room is cavernous, your footsteps echoing into the distance.",
  "narrow": "The passage is narrow, forcing single-file movement.",
  "sprawling": "The chamber sprawls in multiple directions, irregular and expansive."
}
```

### B. Architectural Features

**Purpose:** Describe structural elements

**Feature Categories:**

1. **Walls:**

```json
[
  "Corroded metal plates form the walls",
  "The walls are reinforced with massive girders",
  "Smooth, seamless walls suggest advanced fabrication",
  "The walls are rough-hewn stone, ancient and weathered"
]
```

1. **Ceilings:**

```json
[
  "The ceiling is a tangle of exposed conduits and pipes",
  "A vaulted ceiling arches overhead",
  "The ceiling has partially collapsed, revealing the structure above",
  "The ceiling is studded with defunct light panels"
]
```

1. **Floors:**

```json
[
  "The floor is corrugated metal grating",
  "Smooth stone flags pave the floor",
  "The floor is littered with debris and rubble",
  "Industrial tiles, cracked and discolored, cover the floor"
]
```

### C. Detail Fragments

**Purpose:** Add environmental storytelling

**Detail Categories:**

1. **Decay Signs:**

```json
[
  "Rust streaks mark the surfaces like old blood",
  "Corrosion has eaten through many of the structural supports",
  "The walls are pitted and scarred by centuries",
  "Everything here shows signs of advanced degradation"
]
```

1. **Runic Elements:**

```json
[
  "Runic glyphs flicker weakly on the walls, their light stuttering",
  "Inscribed runes pulse with an irregular rhythm",
  "The walls bear ancient runic inscriptions, now barely legible",
  "Glowing sigils cast eerie shadows"
]
```

1. **Activity Signs:**

```json
[
  "Fresh tracks mar the dust on the floor",
  "Something has passed through here recently",
  "The debris has been disturbed",
  "Evidence of habitation—or something worse—is present"
]
```

---

## VI. Thematic Modifiers

### Modifier 1: [Rusted] (The Roots)

**Primary Biome:** The_Roots

**Adjective:** "corroded"

**Color Palette:** "brown-orange-gray"

**Detail Fragments:**

```json
[
  "shows centuries of oxidation and decay",
  "is crusted with layers of rust",
  "bears the unmistakable marks of moisture and time",
  "has degraded to the point of near-failure"
]
```

**Atmospheric Details:**

```json
[
  "smells of rust and stale water",
  "is thick with the metallic tang of corrosion",
  "carries the scent of decay"
]
```

**Ambient Sounds:**

```json
[
  "dripping water echoing in the distance",
  "the groan of stressed metal",
  "hissing steam escaping from fractured pipes"
]
```

---

### Modifier 2: [Scorched] (Muspelheim)

**Primary Biome:** Muspelheim

**Adjective:** "scorched"

**Color Palette:** "red-orange-black"

**Detail Fragments:**

```json
[
  "radiates intense heat, making the air shimmer",
  "shows signs of extreme thermal damage",
  "is blackened by fire and superheated gases",
  "glows faintly with residual heat"
]
```

**Atmospheric Details:**

```json
[
  "is thick with the smell of brimstone and superheated metal",
  "burns your lungs with each breath",
  "shimmers with oppressive heat"
]
```

**Ambient Sounds:**

```json
[
  "the low rumble of flowing lava",
  "hissing steam vents",
  "the crackle of flames"
]
```

---

### Modifier 3: [Frozen] (Niflheim)

**Primary Biome:** Niflheim

**Adjective:** "ice-covered"

**Color Palette:** "white-blue-gray"

**Detail Fragments:**

```json
[
  "is encased in thick sheets of ancient ice",
  "drips with meltwater from thawing frost",
  "shows signs of centuries of freeze-thaw cycles",
  "is slick with a treacherous layer of ice"
]
```

**Atmospheric Details:**

```json
[
  "is bone-chillingly cold",
  "carries the sharp scent of ozone and frozen moisture",
  "fogs with your breath"
]
```

**Ambient Sounds:**

```json
[
  "the creak and crack of shifting ice",
  "howling wind through frozen passages",
  "the drip of melting frost"
]
```

---

### Modifier 4: [Crystalline] (Alfheim)

**Primary Biome:** Alfheim

**Adjective:** "crystalline"

**Color Palette:** "multi-colored-iridescent"

**Detail Fragments:**

```json
[
  "has crystallized into bizarre, impossible formations",
  "pulses with unstable Aetheric energy",
  "shifts and flickers as if not entirely real",
  "defies conventional physics"
]
```

**Atmospheric Details:**

```json
[
  "crackles with uncontrolled Aether",
  "makes your hair stand on end",
  "distorts reality at the edges of perception"
]
```

**Ambient Sounds:**

```json
[
  "the high-pitched shriek of the Cursed Choir",
  "reality itself seems to hum and vibrate",
  "discordant chimes of crystalline structures"
]
```

---

### Modifier 5: [Monolithic] (Jötunheim)

**Primary Biome:** Jotunheim

**Adjective:** "monolithic"

**Color Palette:** "gray-steel-dark"

**Detail Fragments:**

```json
[
  "is constructed on a massive, inhuman scale",
  "shows signs of industrial decay",
  "echoes with the ghost of vast machinery",
  "was built for beings far larger than humans"
]
```

**Atmospheric Details:**

```json
[
  "echoes with emptiness",
  "carries the smell of rust and old industry",
  "is oppressively silent"
]
```

**Ambient Sounds:**

```json
[
  "distant metallic clanging",
  "the groan of enormous structures",
  "echoes of titanic footsteps"
]
```

---

## VII. Database Schema Implementation

### SQL: Insert Base Templates

```sql
-- =====================================================
-- ENTRY HALL BASE TEMPLATE
-- =====================================================

INSERT INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags
) VALUES (
    'Entry_Hall_Base',
    'Room',
    'EntryHall',
    '{
      "size": "Medium",
      "min_exits": 1,
      "max_exits": 2,
      "spawn_budget_multiplier": 0.5,
      "ambient_danger_level": "Low"
    }',
    'The {Modifier} Entry Hall',
    'You enter {Article} {Modifier_Adj} entry hall. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Detail_2}. The air {Atmospheric_Detail}.',
    '["Starting", "Safe", "Orientation"]'
);

-- =====================================================
-- CORRIDOR BASE TEMPLATE
-- =====================================================

INSERT INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags
) VALUES (
    'Corridor_Base',
    'Room',
    'Corridor',
    '{
      "size": "Small",
      "min_exits": 2,
      "max_exits": 2,
      "spawn_budget_multiplier": 0.8,
      "ambient_danger_level": "Medium"
    }',
    'The {Modifier} Corridor',
    '{Article_Cap} {Modifier_Adj} corridor stretches {Direction_Descriptor}. {Architectural_Feature}. {Spatial_Descriptor}. {Detail_1}. {Modifier_Detail}.',
    '["Transit", "Linear", "Narrow"]'
);

-- =====================================================
-- CHAMBER BASE TEMPLATE  
-- =====================================================

INSERT INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags
) VALUES (
    'Chamber_Base',
    'Room',
    'Chamber',
    '{
      "size": "Large",
      "min_exits": 1,
      "max_exits": 4,
      "spawn_budget_multiplier": 1.2,
      "ambient_danger_level": "High"
    }',
    'The {Modifier} {Function} Chamber',
    '{Article_Cap} {Modifier_Adj} {Function} dominates this space. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Detail_2}. {Function_Detail}. {Modifier_Detail}.',
    '["Large", "Combat", "Exploration"]'
);

-- Continue for remaining 12 base templates...
```

### SQL: Insert Thematic Modifiers

```sql
-- =====================================================
-- RUSTED MODIFIER (The Roots)
-- =====================================================

INSERT INTO Descriptor_Thematic_Modifiers (
    modifier_name,
    primary_biome,
    adjective,
    detail_fragment,
    stat_modifiers,
    status_effects,
    color_palette,
    ambient_sounds
) VALUES (
    'Rusted',
    'The_Roots',
    'corroded',
    'shows centuries of oxidation and decay',
    '{}',  -- No stat modifiers for rooms
    '[]',  -- No status effects
    'brown-orange-gray',
    '["dripping water echoing", "groan of stressed metal", "hissing steam"]'
);

-- =====================================================
-- SCORCHED MODIFIER (Muspelheim)
-- =====================================================

INSERT INTO Descriptor_Thematic_Modifiers (
    modifier_name,
    primary_biome,
    adjective,
    detail_fragment,
    stat_modifiers,
    status_effects,
    color_palette,
    ambient_sounds
) VALUES (
    'Scorched',
    'Muspelheim',
    'scorched',
    'radiates intense heat, making the air shimmer',
    '{"ambient_fire_damage": 1}',
    '[]',
    'red-orange-black',
    '["low rumble of lava", "hissing steam vents", "crackle of flames"]'
);

-- Continue for remaining 3 modifiers...
```

---

## VIII. Service Implementation

### RoomDescriptorService.cs

```csharp
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);

public class RoomDescriptorService : IRoomDescriptorService
{
    private readonly IDescriptorRepository _repository;
    private readonly ILogger<RoomDescriptorService> _logger;
    private readonly Random _random;
    
    public RoomDescriptorService(
        IDescriptorRepository repository,
        ILogger<RoomDescriptorService> logger)
    {
        _repository = repository;
        _logger = logger;
        _random = new Random();
    }
    
    /// <summary>
    /// Generate a room description from base template + modifier.
    /// </summary>
    public string GenerateRoomDescription(
        string baseTemplateName,
        string modifierName)
    {
        try
        {
            // Load base template
            var baseTemplate = _repository.GetBaseTemplate(baseTemplateName);
            if (baseTemplate == null)
            {
                _logger.LogError(
                    "Base template not found: {TemplateName}",
                    baseTemplateName);
                return "ERROR: Template not found";
            }
            
            // Load modifier
            var modifier = _repository.GetModifier(modifierName);
            if (modifier == null)
            {
                _logger.LogError(
                    "Modifier not found: {ModifierName}",
                    modifierName);
                return "ERROR: Modifier not found";
            }
            
            // Generate description
            var description = ProcessDescriptionTemplate(
                baseTemplate.DescriptionTemplate,
                baseTemplate,
                modifier);
            
            _logger.LogDebug(
                "Generated room description: Base={Base}, Modifier={Modifier}",
                baseTemplateName,
                modifierName);
            
            return description;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error generating room description: Base={Base}, Modifier={Modifier}",
                baseTemplateName,
                modifierName);
            throw;
        }
    }
    
    /// <summary>
    /// Process template string, replacing placeholders.
    /// </summary>
    private string ProcessDescriptionTemplate(
        string template,
        DescriptorBaseTemplate baseTemplate,
        ThematicModifier modifier)
    {
        var result = template;
        
        // Replace modifier placeholders
        result = result.Replace("{Modifier}", modifier.ModifierName);
        result = result.Replace("{Modifier_Adj}", modifier.Adjective);
        result = result.Replace("{Modifier_Detail}", modifier.DetailFragment);
        
        // Replace article
        var article = IsVowel(modifier.Adjective[0]) ? "an" : "a";
        result = result.Replace("{Article}", article);
        result = result.Replace("{Article_Cap}", 
            char.ToUpper(article[0]) + article.Substring(1));
        
        // Replace spatial descriptor (random)
        var spatialDescriptor = GetRandomSpatialDescriptor(baseTemplate);
        result = result.Replace("{Spatial_Descriptor}", spatialDescriptor);
        
        // Replace architectural feature (random)
        var archFeature = GetRandomArchitecturalFeature(baseTemplate);
        result = result.Replace("{Architectural_Feature}", archFeature);
        
        // Replace details (random)
        var detail1 = GetRandomDetail(baseTemplate, modifier);
        var detail2 = GetRandomDetail(baseTemplate, modifier, exclude: detail1);
        result = result.Replace("{Detail_1}", detail1);
        result = result.Replace("{Detail_2}", detail2);
        
        // Replace atmospheric detail (from modifier)
        var atmoDetail = GetRandomAtmosphericDetail(modifier);
        result = result.Replace("{Atmospheric_Detail}", atmoDetail);
        
        return result;
    }
    
    private bool IsVowel(char c)
    {
        return "aeiouAEIOU".Contains(c);
    }
    
    private string GetRandomSpatialDescriptor(DescriptorBaseTemplate template)
    {
        // Query descriptor fragments by tag
        var descriptors = _repository.GetDescriptorFragments(
            "SpatialDescriptor",
            template.Tags);
        
        if (!descriptors.Any())
            return "The space extends before you";
        
        return descriptors[_[random.Next](http://random.Next)(descriptors.Count)];
    }
    
    private string GetRandomArchitecturalFeature(DescriptorBaseTemplate template)
    {
        var features = _repository.GetDescriptorFragments(
            "ArchitecturalFeature",
            template.Tags);
        
        if (!features.Any())
            return "The architecture is unremarkable";
        
        return features[_[random.Next](http://random.Next)(features.Count)];
    }
    
    private string GetRandomDetail(
        DescriptorBaseTemplate template,
        ThematicModifier modifier,
        string exclude = null)
    {
        var details = _repository.GetDescriptorFragments(
            "Detail",
            template.Tags.Concat(new[] { modifier.PrimaryBiome }).ToList());
        
        if (exclude != null)
            details = details.Where(d => d != exclude).ToList();
        
        if (!details.Any())
            return "Details are nondescript";
        
        return details[_[random.Next](http://random.Next)(details.Count)];
    }
    
    private string GetRandomAtmosphericDetail(ThematicModifier modifier)
    {
        var atmoDetails = JsonSerializer.Deserialize<List<string>>(
            modifier.AmbientSounds ?? "[]");
        
        if (!atmoDetails.Any())
            return "is still";
        
        return atmoDetails[_[random.Next](http://random.Next)(atmoDetails.Count)];
    }
}
```

---

## IX. Integration with v0.10 DungeonGenerator

### Updated Room Instantiation

```csharp
public class DungeonGenerator
{
    private readonly IRoomDescriptorService _roomDescriptorService;
    
    private Room InstantiateRoom(
        DungeonNode node,
        BiomeDefinition biome,
        int dungeonId)
    {
        // BEFORE (v0.10):
        // var name = template.GenerateName(rng);
        // var description = template.GenerateDescription(rng);
        
        // AFTER (v0.38.1):
        var baseTemplate = GetBaseTemplateForArchetype(node.Archetype);
        var modifier = GetModifierForBiome(biome.BiomeId);
        
        var name = _roomDescriptorService.GenerateRoomName(
            baseTemplate,
            modifier);
        
        var description = _roomDescriptorService.GenerateRoomDescription(
            baseTemplate,
            modifier);
        
        return new Room
        {
            Id = $"room_dungeon{dungeonId}_node{[node.Id](http://node.Id)}",
            TemplateId = baseTemplate,
            Name = name,
            Description = description,
            Archetype = node.Archetype,
            Size = GetRoomSize(node.Archetype),
            Exits = BuildExitsFromNode(node),
            SecretExits = BuildSecretExits(node)
        };
    }
    
    private string GetBaseTemplateForArchetype(RoomArchetype archetype)
    {
        return archetype switch
        {
            RoomArchetype.EntryHall => "Entry_Hall_Base",
            RoomArchetype.Corridor => "Corridor_Base",
            RoomArchetype.Chamber => "Chamber_Base",
            RoomArchetype.Junction => "Junction_Base",
            RoomArchetype.BossArena => "Boss_Arena_Base",
            RoomArchetype.SecretRoom => "Secret_Room_Base",
            _ => "Chamber_Base"
        };
    }
    
    private string GetModifierForBiome(string biomeId)
    {
        return biomeId switch
        {
            "The_Roots" => "Rusted",
            "Muspelheim" => "Scorched",
            "Niflheim" => "Frozen",
            "Alfheim" => "Crystalline",
            "Jotunheim" => "Monolithic",
            _ => "Rusted"
        };
    }
}
```

---

## X. Success Criteria

**v0.38.1 is DONE when:**

### Base Templates

- [ ]  15+ room base templates in Descriptor_Base_Templates
- [ ]  All 6 core archetypes covered
- [ ]  Base mechanics JSON valid
- [ ]  Name templates defined
- [ ]  Description templates defined
- [ ]  Tags properly assigned

### Architectural Descriptors

- [ ]  20+ descriptor fragments in repository
- [ ]  Spatial descriptors (7+)
- [ ]  Architectural features (walls, ceilings, floors)
- [ ]  Detail fragments (decay, runes, activity)
- [ ]  Tagged for filtering

### Thematic Modifiers

- [ ]  5+ modifiers in Descriptor_Thematic_Modifiers
- [ ]  All 5 core biomes covered
- [ ]  Detail fragments defined
- [ ]  Atmospheric details defined
- [ ]  Ambient sounds defined

### Composite Generation

- [ ]  50+ composites auto-generated
- [ ]  Base + modifier combinations
- [ ]  Descriptions render correctly
- [ ]  No placeholder text in output

### Service Implementation

- [ ]  RoomDescriptorService complete
- [ ]  IDescriptorRepository interface
- [ ]  Template processing logic
- [ ]  Random selection logic
- [ ]  Serilog logging throughout

### Integration

- [ ]  DungeonGenerator uses descriptor service
- [ ]  v0.10 room instantiation updated
- [ ]  Backward compatible
- [ ]  No gameplay regressions

### Testing

- [ ]  Unit tests (80%+ coverage)
- [ ]  Template processing tests
- [ ]  Composite generation tests
- [ ]  Integration tests with v0.10
- [ ]  Generated rooms validated

---

## XI. Implementation Roadmap

**Phase 1: Database Schema** — 2 hours

- Create base template rows (15+)
- Create modifier rows (5+)
- Create descriptor fragment support tables

**Phase 2: Service Implementation** — 4 hours

- RoomDescriptorService
- Template processing logic
- Random selection algorithms

**Phase 3: Descriptor Content** — 3 hours

- Spatial descriptors (7+)
- Architectural features (12+)
- Detail fragments (20+)

**Phase 4: Integration** — 2 hours

- Update DungeonGenerator
- Update Room instantiation
- Test with v0.10

**Phase 5: Testing & Validation** — 1 hour

- Unit tests
- Integration tests
- Generated room validation

**Total: 12 hours**

---

## XII. Example Generated Rooms

### Example 1: Rusted Entry Hall

**Base:** Entry_Hall_Base

**Modifier:** [Rusted]

**Generated Name:** "The Corroded Entry Hall"

**Generated Description:**

> "You enter a corroded entry hall. The ceiling presses low overhead, and the walls feel uncomfortably close. Corroded metal plates form the walls. Rust streaks mark the surfaces like old blood. Runic glyphs flicker weakly on the walls, their light stuttering. The air smells of rust and stale water."
> 

---

### Example 2: Scorched Corridor

**Base:** Corridor_Base

**Modifier:** [Scorched]

**Generated Name:** "The Scorched Passage"

**Generated Description:**

> "A scorched corridor stretches before you, narrowing into darkness. The walls are blackened by extreme heat. The passage is narrow, forcing single-file movement. The floor is warped and discolored by thermal stress. The air is thick with the smell of brimstone and superheated metal."
> 

---

### Example 3: Frozen Chamber

**Base:** Chamber_Base + "Cryo Chamber" function

**Modifier:** [Frozen]

**Generated Name:** "The Frozen Cryo Chamber"

**Generated Description:**

> "An ice-covered cryo chamber dominates this space. The chamber is vast, its far walls barely visible in the dim light. The ceiling is studded with defunct light panels. Everything is encased in thick sheets of ancient ice. Frost patterns spiral across the walls in impossible geometries. The cryo pods lining the walls are dark and silent. The air is bone-chillingly cold."
> 

---

**Ready to implement the Room Description Library.**