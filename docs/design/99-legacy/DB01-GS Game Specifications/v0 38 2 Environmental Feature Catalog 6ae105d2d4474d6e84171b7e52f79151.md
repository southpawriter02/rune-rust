# v0.38.2: Environmental Feature Catalog

Description: 10+ feature base templates, 25+ descriptors, 5+ hazard archetypes, 40+ composite features
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.38, v0.11, v0.22
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.38: Descriptor Library & Content Database (v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Specification:** [v0.38: Descriptor Library & Content Database](v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)

**Status:** Design Phase

**Timeline:** 10-12 hours

**Goal:** Build comprehensive environmental feature library for Dynamic Room Engine

**Philosophy:** Define reusable hazards, terrain, and obstacles with biome-specific variants

---

## I. Purpose

v0.38.2 creates the **Environmental Feature Catalog**, providing standardized descriptors for all non-room environmental elements in the Dynamic Room Engine (v0.11). This catalog provides:

- **10+ Base Feature Templates** (cover, hazards, obstacles)
- **25+ Feature Descriptors** (mechanics, interactions)
- **5+ Hazard Archetypes** (damage types, activation patterns)
- **40+ Composite Features** (base + modifier combinations)

**Strategic Function:**

Currently, biome implementations define environmental features independently:

- Muspelheim: "[Lava River]" with fire damage mechanics
- Niflheim: "[Frozen Chasm]" with ice damage mechanics
- Result: Similar concepts (chasms) defined separately

**v0.38.2 transforms this to:**

- Base: `Chasm_Base` template (impassable obstacle, fall damage)
- Modifiers: `[Lava_Filled]`, `[Frozen]`, `[Void]`
- Result: 1 base × 3+ modifiers = reusable chasm variants

---

## II. The Rule: What's In vs. Out

### ✅ In Scope

- **Static Terrain:** Cover, obstacles, elevation, impassable barriers
- **Dynamic Hazards:** Damage-dealing environmental dangers
- **Activation Patterns:** Turn-based, proximity-based, triggered
- **Mechanical Properties:** HP, soak, damage dice, conditions
- **Base templates (10+)**
- **Thematic modifiers (5+)** (reuse from v0.38.1)
- **Composite generation**
- **Integration with v0.11 population system**
- **Database schema**
- **Unit tests (80%+ coverage)**
- **Serilog logging**

### ❌ Explicitly Out of Scope

- Enemy placement (v0.11, separate system)
- Interactive objects (v0.38.3)
- Loot nodes (v0.38.5)
- Ambient conditions (v0.38.4)
- Quest-specific features (defer to v0.40)
- Combat mechanics changes (descriptors only)
- UI/rendering changes

---

## III. Feature Type Taxonomy

### A. Three Core Categories

From v0.11 and v0.22 specifications:

```csharp
public enum EnvironmentalFeatureType
{
    StaticTerrain,      // Permanent features (cover, chasms)
    DynamicHazard,      // Active dangers (steam vents, electrical)
    NavigationalObstacle // Movement restrictions (rubble, elevation)
}
```

### B. Static Terrain Archetypes

**1. Cover (Tactical Positioning)**

- **Light Cover:** -2 dice to hit, no LoS block
- **Heavy Cover:** -4 dice to hit, blocks LoS
- **Examples:** Pillars, crates, bulkheads

**2. Obstacles (Movement)**

- **Impassable:** Cannot cross (chasms, walls)
- **Difficult Terrain:** +2 movement cost (rubble, debris)
- **Examples:** Chasms, collapsed structures, debris piles

**3. Elevation (Tactical Advantage)**

- **High Ground:** +1d to ranged attacks
- **Access Cost:** 3+ movement to reach
- **Examples:** Platforms, catwalks, ledges

### C. Dynamic Hazard Archetypes

**1. Damage-Over-Time (Persistent)**

- **Activation:** Every turn, start/end of turn
- **Examples:** [Burning Ground], [Toxic Haze], [Radiation Zone]

**2. Triggered (Proximity/Action)**

- **Activation:** Enter tile, loud action, weight trigger
- **Examples:** [Live Power Conduit], [Pressure Plate], [Unstable Ceiling]

**3. Periodic (Timed)**

- **Activation:** Every N turns
- **Examples:** [Steam Vent] (every 3 turns), [Flame Jet] (every 2 turns)

**4. Area Effect (Multi-Tile)**

- **Pattern:** Line, cone, circle
- **Examples:** [Steam Vent] (3×3), [Lava River] (line), [Gas Cloud] (expanding)

---

## IV. Base Template Definitions

### Static Terrain Templates

### Template 1: Pillar_Base (Cover)

**Category:** Feature

**Archetype:** Cover

**Tags:** `["Structure", "Cover", "Destructible"]`

**Base Mechanics:**

```json
{
  "hp": 50,
  "soak": 8,
  "cover_quality": "Heavy",
  "cover_bonus": -4,
  "blocks_los": true,
  "destructible": true,
  "tiles_occupied": 1
}
```

**Name Template:**

`"{Modifier} Support Pillar"`

**Description Template:**

```
"A {Modifier_Adj} pillar {Modifier_Detail}. It provides heavy cover."
```

**Example Composites:**

- Pillar_Base + [Rusted] = "Corroded Support Pillar"
- Pillar_Base + [Scorched] = "Scorched Support Pillar"
- Pillar_Base + [Crystalline] = "Crystalline Column"

---

### Template 2: Chasm_Base (Obstacle)

**Category:** Feature

**Archetype:** Obstacle

**Tags:** `["Impassable", "Dangerous", "Environmental"]`

**Base Mechanics:**

```json
{
  "impassable": true,
  "fall_damage": "6d6",
  "damage_type": "Physical",
  "requires_acrobatics": false,
  "tiles_width": 2,
  "tactical_divider": true
}
```

**Name Template:**

`"{Modifier} Chasm"`

**Description Template:**

```
"A {Modifier_Adj} chasm {Modifier_Detail}. Falling into it would be fatal."
```

**Example Composites:**

- Chasm_Base + [Lava_Filled] = "Lava River" (+ 8d6 Fire damage)
- Chasm_Base + [Frozen] = "Frozen Chasm" (+ Slippery edges)
- Chasm_Base + [Void] = "Reality Tear" (+ Psychic damage)

---

### Template 3: Elevation_Base (Tactical)

**Category:** Feature

**Archetype:** Tactical

**Tags:** `["Elevation", "Advantage", "Tactical"]`

**Base Mechanics:**

```json
{
  "elevation_bonus": "+1d",
  "applies_to": "Ranged",
  "climb_cost": 3,
  "requires_check": false,
  "tiles_occupied": 4,
  "provides_cover": true
}
```

**Name Template:**

`"{Modifier} Platform"`

**Description Template:**

```
"A {Modifier_Adj} raised platform {Modifier_Detail}. It offers a tactical vantage point."
```

---

### Template 4: Rubble_Pile_Base (Difficult Terrain)

**Category:** Feature

**Archetype:** Obstacle

**Tags:** `["Difficult_Terrain", "Cover", "Environmental"]`

**Base Mechanics:**

```json
{
  "movement_cost_modifier": 2,
  "cover_quality": "Light",
  "cover_bonus": -2,
  "blocks_los": false,
  "destructible": false,
  "tiles_occupied": 2
}
```

**Name Template:**

`"{Modifier} Rubble Pile"`

**Description Template:**

```
"A pile of {Modifier_Adj} rubble {Modifier_Detail}. Crossing it will slow movement."
```

**Coherent Glitch Rule:**

Rubble_Pile_Base **MUST** spawn beneath any [Unstable_Ceiling] hazard (environmental storytelling: ceiling was already dropping debris).

---

### Dynamic Hazard Templates

### Template 5: Steam_Vent_Base (Periodic Hazard)

**Category:** Feature

**Archetype:** DynamicHazard

**Tags:** `["Hazard", "Periodic", "Area_Effect"]`

**Base Mechanics:**

```json
{
  "damage": "2d6",
  "damage_type": "Fire",
  "activation_frequency": 3,
  "activation_type": "Periodic",
  "area_pattern": "3x3",
  "status_effect": null,
  "warning_turn": true
}
```

**Name Template:**

`"{Modifier} Steam Vent"`

**Description Template:**

```
"A fractured pipe vents {Modifier_Adj} steam periodically. It erupts every 3 turns."
```

**Example Composites:**

- Steam_Vent_Base + [Geothermal] = "Geothermal Steam Vent"
- Steam_Vent_Base + [Corrupted] = "Toxic Steam Vent" (+ [Poisoned] status)

---

### Template 6: Power_Conduit_Base (Proximity Hazard)

**Category:** Feature

**Archetype:** DynamicHazard

**Tags:** `["Hazard", "Proximity", "Electrical"]`

**Base Mechanics:**

```json
{
  "damage": "3d6",
  "damage_type": "Lightning",
  "activation_range": 2,
  "activation_type": "Proximity",
  "status_effect": ["Stunned", 1],
  "enhanced_by": ["Flooded"]
}
```

**Name Template:**

`"Live Power Conduit"`

**Description Template:**

```
"An exposed power conduit arcs with {Modifier_Adj} electricity. Approach with caution."
```

**Coherent Glitch Rule:**

If room has [Flooded] ambient condition:

- Damage increases to 6d6
- Activation range increases to 4 tiles
- Entire flooded area conducts electricity

---

### Template 7: Unstable_Ceiling_Base (Triggered Hazard)

**Category:** Feature

**Archetype:** DynamicHazard

**Tags:** `["Hazard", "Triggered", "One_Time", "Area_Effect"]`

**Base Mechanics:**

```json
{
  "damage": "4d6",
  "damage_type": "Physical",
  "triggers": ["Explosion", "Heavy_Attack", "Loud_Action"],
  "area_pattern": "All_Combatants",
  "one_time": true,
  "creates_terrain": "Rubble_Pile"
}
```

**Name Template:**

`"Unstable Ceiling"`

**Description Template:**

```
"The {Modifier_Adj} ceiling shows dangerous signs of instability. Loud actions may trigger a collapse."
```

**Coherent Glitch Rule:**

When Unstable_Ceiling activates:

1. Deal 4d6 Physical damage to all combatants
2. Create Rubble_Pile terrain in random tiles
3. Hazard is destroyed (one-time)

---

### Template 8: Burning_Ground_Base (Persistent Hazard)

**Category:** Feature

**Archetype:** DynamicHazard

**Tags:** `["Hazard", "Persistent", "Fire"]`

**Base Mechanics:**

```json
{
  "damage": "2d6",
  "damage_type": "Fire",
  "activation_timing": "End_Of_Turn",
  "tiles_affected": 4,
  "status_effect": ["Burning", 2],
  "spread_chance": 0.1
}
```

**Name Template:**

`"{Modifier} Burning Ground"`

**Description Template:**

```
"The ground here burns with {Modifier_Adj} flames. Standing in it deals fire damage."
```

---

### Template 9: Toxic_Haze_Base (Area Hazard)

**Category:** Feature

**Archetype:** DynamicHazard

**Tags:** `["Hazard", "Persistent", "Poison", "Area_Effect"]`

**Base Mechanics:**

```json
{
  "damage": "1d4",
  "damage_type": "Poison",
  "activation_timing": "Start_Of_Turn",
  "area_pattern": "Room_Wide",
  "status_effect": ["Poisoned", 0.25],
  "accuracy_penalty": -1
}
```

**Name Template:**

`"{Modifier} Toxic Haze"`

**Description Template:**

```
"A {Modifier_Adj} haze fills the air. Breathing it causes damage and impairs accuracy."
```

---

### Template 10: Electrified_Floor_Base (Movement Hazard)

**Category:** Feature

**Archetype:** DynamicHazard

**Tags:** `["Hazard", "Movement_Triggered", "Electrical"]`

**Base Mechanics:**

```json
{
  "damage": "3d6",
  "damage_type": "Lightning",
  "activation_type": "Movement",
  "tiles_affected": 6,
  "status_effect": ["Stunned", 0.2]
}
```

**Name Template:**

`"Electrified Floor"`

**Description Template:**

```
"The {Modifier_Adj} floor pulses with electrical current. Moving across it triggers shocks."
```

---

## V. Thematic Modifiers (Reused from v0.38.1)

### Modifier Application by Feature Type

**[Rusted] (The Roots):**

- **Applies to:** Pillars, Platforms, Rubble
- **Stat Modifiers:** -20% HP (structural weakness)
- **Visual:** Brown-orange corrosion, rust streaks

**[Scorched] (Muspelheim):**

- **Applies to:** All terrain, hazards
- **Stat Modifiers:** Fire resistance 0.5, adds ambient heat damage
- **Visual:** Blackened surfaces, glowing edges

**[Frozen] (Niflheim):**

- **Applies to:** All terrain, water features
- **Stat Modifiers:** Slippery (+1 movement cost), Ice damage type
- **Visual:** Ice coating, frost patterns

**[Crystalline] (Alfheim):**

- **Applies to:** Structures, elevation
- **Stat Modifiers:** Unstable (random property shifts)
- **Visual:** Glowing formations, semi-transparent

**[Monolithic] (Jötunheim):**

- **Applies to:** Structures, industrial features
- **Stat Modifiers:** +50% HP, +50% Soak (massive construction)
- **Visual:** Oversized, industrial decay

---

## VI. Special Modifiers for Hazards

### Modifier 6: [Lava_Filled] (Muspelheim-specific)

**Primary Application:** Chasm_Base

**Damage Modifier:** +8d6 Fire

**Visual:** Flowing molten rock, red-orange glow

**Composite:**

```json
{
  "base": "Chasm_Base",
  "modifier": "Lava_Filled",
  "final_name": "Lava River",
  "final_mechanics": {
    "impassable": true,
    "fall_damage": "6d6 Physical + 8d6 Fire",
    "ambient_heat_range": 2,
    "ambient_heat_damage": "1d4 Fire per turn"
  }
}
```

---

### Modifier 7: [Geothermal] (The Roots-specific)

**Primary Application:** Steam_Vent_Base

**Damage Type:** Fire

**Visual:** Hissing pipes, condensation

**Composite:**

```json
{
  "base": "Steam_Vent_Base",
  "modifier": "Geothermal",
  "final_name": "Geothermal Steam Vent",
  "final_mechanics": {
    "damage": "2d6 Fire",
    "activation_frequency": 3,
    "warning_turn": true,
    "area_pattern": "3x3"
  }
}
```

---

### Modifier 8: [Void] (Alfheim-specific)

**Primary Application:** Chasm_Base

**Damage Modifier:** Reality tears, Psychic damage

**Visual:** Flickering void, non-Euclidean geometry

**Composite:**

```json
{
  "base": "Chasm_Base",
  "modifier": "Void",
  "final_name": "Reality Tear",
  "final_mechanics": {
    "impassable": true,
    "fall_damage": "6d6 Psychic",
    "proximity_stress": "+2 Psychic Stress per turn within 3 tiles",
    "unstable": "May shift position between turns"
  }
}
```

---

## VII. Database Schema Implementation

### SQL: Insert Feature Base Templates

```sql
-- =====================================================
-- PILLAR BASE (Cover)
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
    'Pillar_Base',
    'Feature',
    'Cover',
    '{
      "hp": 50,
      "soak": 8,
      "cover_quality": "Heavy",
      "cover_bonus": -4,
      "blocks_los": true,
      "destructible": true,
      "tiles_occupied": 1
    }',
    '{Modifier} Support Pillar',
    'A {Modifier_Adj} pillar {Modifier_Detail}. It provides heavy cover.',
    '["Structure", "Cover", "Destructible"]'
);

-- =====================================================
-- CHASM BASE (Obstacle)
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
    'Chasm_Base',
    'Feature',
    'Obstacle',
    '{
      "impassable": true,
      "fall_damage": "6d6",
      "damage_type": "Physical",
      "requires_acrobatics": false,
      "tiles_width": 2,
      "tactical_divider": true
    }',
    '{Modifier} Chasm',
    'A {Modifier_Adj} chasm {Modifier_Detail}. Falling into it would be fatal.',
    '["Impassable", "Dangerous", "Environmental"]'
);

-- Continue for remaining 8 feature templates...
```

### SQL: Insert Hazard-Specific Modifiers

```sql
-- =====================================================
-- LAVA_FILLED MODIFIER (Muspelheim)
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
    'Lava_Filled',
    'Muspelheim',
    'lava-filled',
    'flows with molten rock that glows red-orange',
    '{"fall_damage_bonus": "8d6", "damage_type": "Fire", "ambient_heat_range": 2}',
    '[["Burning", 2]]',
    'red-orange-black',
    '["low rumble of flowing lava", "crackling heat"]'
);

-- =====================================================
-- GEOTHERMAL MODIFIER (The Roots)
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
    'Geothermal',
    'The_Roots',
    'geothermal',
    'vents superheated steam from fractured pipes',
    '{}',
    '[]',
    'gray-white-orange',
    '["hissing steam", "groaning pipes"]'
);

-- Continue for remaining hazard modifiers...
```

---

## VIII. Service Implementation

### EnvironmentalFeatureService.cs

```csharp
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);

public class EnvironmentalFeatureService : IEnvironmentalFeatureService
{
    private readonly IDescriptorRepository _repository;
    private readonly ILogger<EnvironmentalFeatureService> _logger;
    
    public EnvironmentalFeatureService(
        IDescriptorRepository repository,
        ILogger<EnvironmentalFeatureService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    /// <summary>
    /// Generate environmental feature from base template + modifier.
    /// </summary>
    public StaticTerrainFeature GenerateStaticTerrain(
        string baseTemplateName,
        string modifierName)
    {
        try
        {
            var baseTemplate = _repository.GetBaseTemplate(baseTemplateName);
            var modifier = _repository.GetModifier(modifierName);
            
            if (baseTemplate == null || modifier == null)
            {
                _logger.LogError(
                    "Template or modifier not found: Base={Base}, Modifier={Modifier}",
                    baseTemplateName,
                    modifierName);
                throw new ArgumentException("Invalid template or modifier");
            }
            
            // Parse base mechanics
            var baseMechanics = JsonSerializer.Deserialize<FeatureMechanics>(
                baseTemplate.BaseMechanics);
            
            // Apply modifier stat adjustments
            var finalMechanics = ApplyModifierToMechanics(baseMechanics, modifier);
            
            // Generate name and description
            var name = GenerateName(baseTemplate.NameTemplate, modifier);
            var description = GenerateDescription(
                baseTemplate.DescriptionTemplate,
                baseTemplate,
                modifier);
            
            _logger.LogDebug(
                "Generated static terrain: {Name}",
                name);
            
            return new StaticTerrainFeature
            {
                Name = name,
                Description = description,
                HP = finalMechanics.HP,
                Soak = finalMechanics.Soak,
                CoverQuality = finalMechanics.CoverQuality,
                CoverBonus = finalMechanics.CoverBonus,
                BlocksLoS = finalMechanics.BlocksLoS,
                IsDestructible = finalMechanics.Destructible,
                TilesOccupied = finalMechanics.TilesOccupied
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error generating static terrain: Base={Base}, Modifier={Modifier}",
                baseTemplateName,
                modifierName);
            throw;
        }
    }
    
    /// <summary>
    /// Generate dynamic hazard from base template + modifier.
    /// </summary>
    public DynamicHazard GenerateDynamicHazard(
        string baseTemplateName,
        string modifierName)
    {
        try
        {
            var baseTemplate = _repository.GetBaseTemplate(baseTemplateName);
            var modifier = _repository.GetModifier(modifierName);
            
            if (baseTemplate == null || modifier == null)
            {
                _logger.LogError(
                    "Template or modifier not found: Base={Base}, Modifier={Modifier}",
                    baseTemplateName,
                    modifierName);
                throw new ArgumentException("Invalid template or modifier");
            }
            
            var baseMechanics = JsonSerializer.Deserialize<HazardMechanics>(
                baseTemplate.BaseMechanics);
            
            var finalMechanics = ApplyModifierToHazard(baseMechanics, modifier);
            
            var name = GenerateName(baseTemplate.NameTemplate, modifier);
            var description = GenerateDescription(
                baseTemplate.DescriptionTemplate,
                baseTemplate,
                modifier);
            
            _logger.LogDebug(
                "Generated dynamic hazard: {Name}, Type={Type}",
                name,
                finalMechanics.DamageType);
            
            return new DynamicHazard
            {
                Name = name,
                Description = description,
                Damage = finalMechanics.Damage,
                DamageType = finalMechanics.DamageType,
                ActivationType = finalMechanics.ActivationType,
                ActivationFrequency = finalMechanics.ActivationFrequency,
                ActivationRange = finalMechanics.ActivationRange,
                AreaPattern = finalMechanics.AreaPattern,
                StatusEffect = finalMechanics.StatusEffect
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error generating dynamic hazard: Base={Base}, Modifier={Modifier}",
                baseTemplateName,
                modifierName);
            throw;
        }
    }
    
    private FeatureMechanics ApplyModifierToMechanics(
        FeatureMechanics baseMechanics,
        ThematicModifier modifier)
    {
        var result = baseMechanics.Clone();
        
        // Parse modifier stat adjustments
        if (!string.IsNullOrEmpty(modifier.StatModifiers))
        {
            var modifiers = JsonSerializer.Deserialize<Dictionary<string, object>>(
                modifier.StatModifiers);
            
            // Apply HP modifier if present
            if (modifiers.ContainsKey("hp_multiplier"))
            {
                var multiplier = Convert.ToDouble(modifiers["hp_multiplier"]);
                result.HP = (int)(result.HP * multiplier);
            }
            
            // Apply Soak modifier if present
            if (modifiers.ContainsKey("soak_multiplier"))
            {
                var multiplier = Convert.ToDouble(modifiers["soak_multiplier"]);
                result.Soak = (int)(result.Soak * multiplier);
            }
        }
        
        return result;
    }
    
    private HazardMechanics ApplyModifierToHazard(
        HazardMechanics baseMechanics,
        ThematicModifier modifier)
    {
        var result = baseMechanics.Clone();
        
        // Parse modifier hazard adjustments
        if (!string.IsNullOrEmpty(modifier.StatModifiers))
        {
            var modifiers = JsonSerializer.Deserialize<Dictionary<string, object>>(
                modifier.StatModifiers);
            
            // Damage type override
            if (modifiers.ContainsKey("damage_type"))
            {
                result.DamageType = modifiers["damage_type"].ToString();
            }
            
            // Damage bonus
            if (modifiers.ContainsKey("damage_bonus"))
            {
                result.Damage += " + " + modifiers["damage_bonus"].ToString();
            }
            
            // Activation range modifier
            if (modifiers.ContainsKey("activation_range_multiplier"))
            {
                var multiplier = Convert.ToDouble(modifiers["activation_range_multiplier"]);
                result.ActivationRange = (int)(result.ActivationRange * multiplier);
            }
        }
        
        return result;
    }
    
    private string GenerateName(string template, ThematicModifier modifier)
    {
        return template
            .Replace("{Modifier}", modifier.ModifierName)
            .Replace("{Modifier_Adj}", modifier.Adjective);
    }
    
    private string GenerateDescription(
        string template,
        DescriptorBaseTemplate baseTemplate,
        ThematicModifier modifier)
    {
        return template
            .Replace("{Modifier_Adj}", modifier.Adjective)
            .Replace("{Modifier_Detail}", modifier.DetailFragment);
    }
}
```

---

## IX. Integration with v0.11 Population System

### Updated DynamicRoomEngine.PopulateRoom

```csharp
public class DynamicRoomEngine
{
    private readonly IEnvironmentalFeatureService _featureService;
    
    public void PopulateRoomWithFeatures(
        Room room,
        BiomeDefinition biome,
        Random rng)
    {
        // BEFORE (v0.11):
        // var elements = _biomeElementTable.GetElements(biome.BiomeId, "StaticTerrain");
        // foreach (var element in WeightedSelect(elements))
        // {
        //     var feature = new StaticTerrainFeature { Name = [element.Name](http://element.Name) };
        //     room.StaticTerrain.Add(feature);
        // }
        
        // AFTER (v0.38.2):
        var query = new DescriptorQuery
        {
            Category = "Feature",
            Archetype = "Cover",
            Biome = biome.BiomeId,
            RequiredTags = room.Tags
        };
        
        var descriptors = _descriptorService.QueryDescriptors(query);
        var selectedDescriptors = WeightedSelect(descriptors, count: 2, rng);
        
        foreach (var descriptor in selectedDescriptors)
        {
            var baseTemplate = descriptor.BaseTemplateName;
            var modifier = descriptor.ModifierName;
            
            var feature = _featureService.GenerateStaticTerrain(
                baseTemplate,
                modifier);
            
            room.StaticTerrain.Add(feature);
            
            _logger.LogDebug(
                "Placed feature in room: Room={RoomId}, Feature={Feature}",
                [room.Id](http://room.Id),
                [feature.Name](http://feature.Name));
        }
    }
    
    public void PopulateRoomWithHazards(
        Room room,
        BiomeDefinition biome,
        Random rng)
    {
        // Query hazard descriptors
        var query = new DescriptorQuery
        {
            Category = "Feature",
            Archetype = "DynamicHazard",
            Biome = biome.BiomeId
        };
        
        var descriptors = _descriptorService.QueryDescriptors(query);
        var selectedDescriptors = WeightedSelect(descriptors, count: 1, rng);
        
        foreach (var descriptor in selectedDescriptors)
        {
            var hazard = _featureService.GenerateDynamicHazard(
                descriptor.BaseTemplateName,
                descriptor.ModifierName);
            
            room.DynamicHazards.Add(hazard);
            
            // Apply Coherent Glitch rules
            ApplyCoherentGlitchRules(room, hazard);
            
            _logger.LogDebug(
                "Placed hazard in room: Room={RoomId}, Hazard={Hazard}",
                [room.Id](http://room.Id),
                [hazard.Name](http://hazard.Name));
        }
    }
    
    private void ApplyCoherentGlitchRules(Room room, DynamicHazard hazard)
    {
        // Rule: Unstable Ceiling → Create Rubble Pile
        if ([hazard.Name](http://hazard.Name).Contains("Unstable Ceiling"))
        {
            var rubble = _featureService.GenerateStaticTerrain(
                "Rubble_Pile_Base",
                GetModifierForBiome(room.Biome));
            
            room.StaticTerrain.Add(rubble);
            
            _logger.LogDebug(
                "Coherent Glitch: Added Rubble Pile beneath Unstable Ceiling");
        }
        
        // Rule: Power Conduit + Flooded → Enhanced damage
        if ([hazard.Name](http://hazard.Name).Contains("Power Conduit") &&
            room.AmbientConditions.Contains("Flooded"))
        {
            hazard.Damage = "6d6";  // Doubled from 3d6
            hazard.ActivationRange = 4;  // Increased from 2
            
            _logger.LogDebug(
                "Coherent Glitch: Enhanced Power Conduit due to Flooded condition");
        }
    }
}
```

---

## X. Success Criteria

**v0.38.2 is DONE when:**

### Base Templates

- [ ]  10+ feature base templates
- [ ]  Static Terrain archetypes (Cover, Obstacle, Elevation)
- [ ]  Dynamic Hazard archetypes (Periodic, Proximity, Triggered)
- [ ]  Base mechanics JSON valid
- [ ]  All templates in Descriptor_Base_Templates

### Thematic Modifiers

- [ ]  5+ core modifiers (reused from v0.38.1)
- [ ]  3+ hazard-specific modifiers ([Lava_Filled], [Geothermal], [Void])
- [ ]  Stat modifier rules defined
- [ ]  Damage type modifiers

### Feature Descriptors

- [ ]  25+ feature descriptors
- [ ]  Cover mechanics (Light/Heavy)
- [ ]  Obstacle mechanics (Impassable/Difficult Terrain)
- [ ]  Hazard activation patterns

### Composite Generation

- [ ]  40+ composites auto-generated
- [ ]  Base + modifier combinations
- [ ]  Mechanics merge correctly
- [ ]  No invalid stat combinations

### Coherent Glitch Rules

- [ ]  Unstable Ceiling → Rubble Pile
- [ ]  Power Conduit + Flooded → Enhanced damage
- [ ]  Rules fire automatically
- [ ]  Logged correctly

### Service Implementation

- [ ]  EnvironmentalFeatureService complete
- [ ]  GenerateStaticTerrain() functional
- [ ]  GenerateDynamicHazard() functional
- [ ]  Stat modifier application logic
- [ ]  Serilog logging throughout

### Integration

- [ ]  v0.11 PopulateRoom uses descriptor service
- [ ]  Weighted selection works
- [ ]  Features instantiate correctly
- [ ]  Backward compatible

### Testing

- [ ]  Unit tests (80%+ coverage)
- [ ]  Template processing tests
- [ ]  Stat modifier tests
- [ ]  Coherent Glitch rule tests
- [ ]  Integration tests with v0.11

---

## XI. Implementation Roadmap

**Phase 1: Database Schema** — 2 hours

- Feature base templates (10+)
- Hazard-specific modifiers (3+)

**Phase 2: Service Implementation** — 4 hours

- EnvironmentalFeatureService
- Stat modifier application logic
- Mechanic merging algorithms

**Phase 3: Coherent Glitch Rules** — 2 hours

- Rule detection logic
- Auto-application on population
- Rule validation

**Phase 4: Integration** — 3 hours

- Update v0.11 PopulateRoom
- Test feature instantiation
- Test hazard activation

**Phase 5: Testing & Validation** — 1 hour

- Unit tests
- Integration tests
- Generated feature validation

**Total: 12 hours**

---

## XII. Example Generated Features

### Example 1: Corroded Support Pillar

**Base:** Pillar_Base

**Modifier:** [Rusted]

**Final Mechanics:**

```json
{
  "hp": 40,
  "soak": 6,
  "cover_quality": "Heavy",
  "cover_bonus": -4,
  "blocks_los": true,
  "destructible": true
}
```

**Description:**

> "A corroded pillar shows centuries of oxidation and decay. It provides heavy cover."
> 

---

### Example 2: Lava River

**Base:** Chasm_Base

**Modifier:** [Lava_Filled]

**Final Mechanics:**

```json
{
  "impassable": true,
  "fall_damage": "6d6 Physical + 8d6 Fire",
  "ambient_heat_range": 2,
  "ambient_heat_damage": "1d4 Fire per turn"
}
```

**Description:**

> "A lava-filled chasm flows with molten rock that glows red-orange. Falling into it would be fatal."
> 

---

### Example 3: Geothermal Steam Vent

**Base:** Steam_Vent_Base

**Modifier:** [Geothermal]

**Final Mechanics:**

```json
{
  "damage": "2d6 Fire",
  "activation_frequency": 3,
  "activation_type": "Periodic",
  "area_pattern": "3x3",
  "warning_turn": true
}
```

**Description:**

> "A fractured pipe vents geothermal steam periodically. It erupts every 3 turns."
> 

**Gameplay:**

Turn 1: Warning message ("The vent hisses ominously")

Turn 2: Warning intensifies

Turn 3: ERUPTION → 2d6 Fire damage to 3×3 area

---

**Ready to implement the Environmental Feature Catalog.**