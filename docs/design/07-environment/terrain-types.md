---
id: SPEC-ENV-TERRAIN
title: "Terrain Types — Combat Grid Ground State"
version: 1.0
status: draft
last-updated: 2025-12-14
related-files:
  - path: "docs/07-environment/dynamic-hazards.md"
    status: Active
  - path: "docs/07-environment/room-engine/spatial-layout.md"
    status: Active
  - path: "docs/01-core/skills/acrobatics.md"
    status: Active
  - path: "docs/03-combat/combat-resolution.md"
    status: Active
---

# Terrain Types — Combat Grid Ground State

> *"The ground remembers what it was. This floor was once polished steel, now it's a mosaic of rust and ice. Every step is a negotiation with entropy."*

---

## 1. Core Philosophy: The State of the Grid

The **Terrain Types** system defines the fundamental state and properties of ground tiles on the combat grid. This is not about discrete objects like pillars or chasms, but about the very nature of the ground itself.

### 1.1 The Rendered Reality

Thematically, terrain represents the localized state of the world's **"physical rendering engine"**:

| Terrain State | System Metaphor |
|---------------|-----------------|
| Standard floor | Coherent, stable data |
| Mud/difficult terrain | "Laggy" sector, impedes processing |
| Acid pools | "Corrupted file" actively damaging access |
| Ice | Unstable physics layer |
| Flooded | Conductor waiting for voltage |

### 1.2 Design Principles

> [!IMPORTANT]
> **Terrain is passive, not triggered.** Unlike [Dynamic Hazards](dynamic-hazards.md), terrain effects apply automatically while a character occupies or moves through tiles.

**What Terrain Represents:**
- The physical state of the ground itself
- Persistent conditions that affect all occupants
- Tactical zones that reward positioning awareness
- Environmental storytelling through battlefield layout

**Terrain vs. Hazards:**
| Aspect | Terrain | Hazards |
|--------|---------|---------|
| **Activation** | Passive/automatic | Triggered by condition |
| **Nature** | Ground state | Discrete objects |
| **Lifecycle** | Persistent | Dormant → Triggered → Cooldown |
| **Example** | `[Flooded]` tiles | `[Steam Vent]` |

---

## 2. Terrain Type Catalog

### 2.1 Overview

| Terrain | Minimap | Primary Effect | Removable |
|---------|---------|----------------|-----------|
| `[Normal]` | `.` | No effect | N/A |
| `[Difficult]` | `~` | 2× Stamina for movement | No |
| `[Flooded]` | `w` | Difficult + Lightning vulnerability | Freeze |
| `[Obscuring]` | `%` | +2 Defense, -3d10 ranged | Disperse |
| `[Hazardous]` | `X` | End-of-turn damage | Immunity |
| `[Slippery]` | `s` | FINESSE check on mobility | Ignite → Burning |
| `[Burning]` | `^` | Fire damage per turn | Extinguish |
| `[Elevated]` | `+` | Height advantage | N/A |

---

## 3. Terrain Type Specifications

### 3.1 [Normal] — Standard Ground

| Property | Value |
|----------|-------|
| **Minimap Icon** | `.` (period) |
| **Effect** | None |
| **Description** | Stable, traversable ground |

The default state. No mechanical effects.

---

### 3.2 [Difficult Terrain]

| Property | Value |
|----------|-------|
| **Minimap Icon** | `~` (tilde) |
| **Effect** | 2× Stamina cost for all movement |
| **Examples** | Thick mud, deep snow, tangled roots, rubble fields |
| **Removable** | No |

> *"The ground here is a nightmare of broken pipes and twisted metal. Every step costs twice the effort."*

**Mechanical Effects:**
- **Movement Cost:** All movement actions (`move`, `glitch-dash`, `charge`) cost **double Stamina**
- **Combat Movement:** Free 5-ft step still allowed, but costs 5 Stamina instead of 0
- **Forced Movement:** `Push`/`Pull` effects move target only half distance

**Biome Distribution:**
| Biome | Difficulty Type | Frequency |
|-------|-----------------|-----------|
| The Roots | Rubble, debris | High |
| Niflheim | Deep snow, ice ridges | High |
| Vanaheim | Tangled undergrowth | Medium |
| Muspelheim | Ash drifts, slag | Medium |

---

### 3.3 [Flooded]

| Property | Value |
|----------|-------|
| **Minimap Icon** | `w` (water) |
| **Effect** | Difficult Terrain + Lightning Vulnerability |
| **Examples** | Puddles, shallow streams, coolant spills, flooded chambers |
| **Removable** | Freeze → [Slippery] |

> *"Murky water laps at your ankles. Somewhere above, you hear the crackle of exposed wiring. A deadly combination."*

**Mechanical Effects:**
- **Movement Cost:** 2× Stamina (inherits `[Difficult]`)
- **Lightning Vulnerability:** Characters in [Flooded] tiles take **2× damage** from Lightning sources
- **Chain Conduction:** Lightning damage in one [Flooded] tile spreads to ALL connected [Flooded] tiles

**Interactions:**
| Ability | Effect |
|---------|--------|
| Ice spell (Galdr-caster) | Freeze water → Convert to [Slippery] |
| Fire spell (sustained) | Evaporate → Convert to [Normal] (temporary) |
| Lightning attack | Chain to all connected [Flooded] tiles |

**Chain Lightning Example:**
```
[w][w][w][ ][ ]
[ ][w][w][w][ ]
[ ][ ][@][w][w]  ← Character @ casts lightning here
[ ][ ][ ][ ][ ]

Result: ALL 8 connected [w] tiles conduct the damage
```

---

### 3.4 [Obscuring Terrain]

| Property | Value |
|----------|-------|
| **Minimap Icon** | `%` (percent) |
| **Effect** | +2 Defense, -3d10 ranged accuracy |
| **Examples** | Thick fog, dense spore clouds, geothermal steam, smoke |
| **Removable** | Some abilities disperse |

> *"The air is thick with something you can't identify. Visibility drops to arm's length. In here, you're hidden—but so is everything else."*

**Mechanical Effects:**
- **Concealment:** All characters within gain **+2 Defense Score**
- **Ranged Penalty:** All ranged attacks into, out of, or through suffer **-3d10 Accuracy**
- **Stealth Bonus:** +2d10 to Acrobatics (Stealth) checks within
- **Line of Sight:** Cannot target specific enemies beyond 2 tiles

**Subtypes:**
| Subtype | Additional Effect |
|---------|-------------------|
| Spore Cloud | +1 Stress/turn for non-Myr-Stalker |
| Toxic Fog | 1d4 Poison damage/turn |
| Steam | Blocks thermal/infrared senses |
| Smoke | Temporary (dissipates in 3 rounds) |

---

### 3.5 [Hazardous Terrain]

| Property | Value |
|----------|-------|
| **Minimap Icon** | `X` (danger) |
| **Effect** | End-of-turn damage |
| **Examples** | Acid pools, toxin mires, razor shard fields |
| **Removable** | Specialization immunity only |

> *"The ground here glistens with something corrosive. You can smell the destruction from several feet away."*

**Mechanical Effects:**
- **Damage Trigger:** Character takes damage at **end of their turn** if standing on tile
- **Damage Amount:** Determined by hazard subtype (typically 2d6-3d8)
- **Damage Type:** Varies (Acid, Poison, Physical)

**Subtypes:**
| Subtype | Damage | Type | Biome |
|---------|--------|------|-------|
| Acid Pool | 3d6/turn | Acid | The Roots, Svartalfheim |
| Toxin Mire | 2d6/turn + [Poisoned] | Poison | Vanaheim, Muspelheim |
| Razor Shards | 2d8 on entry | Physical | The Roots, Jotunheim |
| Corrupted Ground | 1d4 + 1 Corruption | Metaphysical | Alfheim (corrupted) |

**Immunity:**
| Specialization | Immunity |
|----------------|----------|
| Myr-Stalker | Toxin Mire |
| Iron-Blooded | Acid Pool |
| Corruption-Touched | Corrupted Ground |

---

### 3.6 [Slippery Terrain]

| Property | Value |
|----------|-------|
| **Minimap Icon** | `s` (slip) |
| **Effect** | FINESSE check or [Knocked Down] |
| **Examples** | Ice patches, oil slicks, polished metal |
| **Removable** | Ignite oil → [Burning] |

> *"The surface gleams treacherously. One wrong step and you're on your back."*

**Mechanical Effects:**
- **No Extra Stamina Cost:** Unlike [Difficult], normal movement is free
- **Trigger Condition:** Check required when:
  - Performing `dash`, `charge`, or mobility abilities
  - Being forcibly moved (`Push`/`Pull`)
  - Attempting to change direction sharply

**FINESSE Check:**
| Condition | DC |
|-----------|-----|
| Standard movement | No check |
| Dash/Charge | DC 12 |
| Forced movement | DC 14 |
| Combat action on ice | DC 10 |

**Failure:** Character gains **[Knocked Down]** status:
- Prone until action spent to stand
- -2 Defense while prone
- Melee attackers gain +2d10

**Interactions:**
| Action | Result |
|--------|--------|
| Fire attack on oil | Convert to [Burning] for 3 rounds |
| Ice spell on water | Convert [Flooded] → [Slippery] |
| Salt/sand application | Remove [Slippery] |

---

### 3.7 [Burning Ground]

| Property | Value |
|----------|-------|
| **Minimap Icon** | `^` (flame) |
| **Effect** | Fire damage per turn |
| **Examples** | Ignited oil, coal seam fires, lava adjacent |
| **Removable** | Extinguish (3 rounds natural) |

> *"Flames lick across the floor. The heat is oppressive, the smoke choking. Standing here is not an option."*

**Mechanical Effects:**
- **Damage:** 3d6 Fire damage at **start of turn** while on tile
- **Ignition Risk:** Characters may catch fire ([Burning] status, WILL save DC 14)
- **Light Source:** Provides illumination in 2-tile radius

**Duration:**
| Source | Duration |
|--------|----------|
| Ignited oil | 3 rounds |
| Ignited debris | 5 rounds |
| Coal seam/lava adjacent | Permanent |

**Extinguishing:**
- Water/ice attack: Immediate
- Smothering: 1 action, ends fire
- Natural: Burns out after duration

---

### 3.8 [Elevated Terrain]

| Property | Value |
|----------|-------|
| **Minimap Icon** | `+` (plus) |
| **Effect** | Height advantage |
| **Examples** | Raised platforms, catwalks, rubble mounds |
| **Removable** | N/A (structural) |

> *"From up here, the battlefield spreads before you. Every target is exposed. This is where you want to be."*

**Mechanical Effects:**
- **Height Advantage:** +1d10 to ranged attacks targeting lower elevation
- **Cover Bonus:** +1 Defense against ranged from below
- **Movement:** Requires Climb check (Acrobatics) to ascend/descend

**Integration with Acrobatics:**
| Action | Check |
|--------|-------|
| Climb up | Acrobatics DC 10-14 |
| Jump down (1 level) | No check, 1d6 damage |
| Leap across gap | Per Acrobatics leaping rules |

---

## 4. Movement Cost Summary

| Terrain | Base Movement | Dash/Charge | Forced Movement |
|---------|---------------|-------------|-----------------|
| [Normal] | 1× Stamina | 1× | Full distance |
| [Difficult] | 2× Stamina | 2× | Half distance |
| [Flooded] | 2× Stamina | 2× | Half distance |
| [Obscuring] | 1× Stamina | 1× | Full distance |
| [Hazardous] | 1× Stamina | 1× | Full distance |
| [Slippery] | 1× Stamina | Check req'd | Check req'd |
| [Burning] | 1× Stamina | 1× | Full distance |
| [Elevated] | Climb check | N/A | N/A |

---

## 5. Vulnerability & Damage Modifiers

### 5.1 Elemental Vulnerabilities

| Terrain | Vulnerability |
|---------|---------------|
| [Flooded] | Lightning damage ×2 |
| [Burning] | Cold damage ×0.5 (fire resists) |
| [Slippery] (ice) | Fire damage may remove |

### 5.2 Status Effect Interactions

| Status | Terrain | Interaction |
|--------|---------|-------------|
| [Burning] (character) | [Flooded] | Extinguished |
| [Frozen] | [Burning Ground] | Thaws, takes damage |
| [Oiled] | [Burning Ground] | Instant ignition |

---

## 6. Biome Terrain Distribution

### 6.1 Default Terrain by Biome

| Biome | Primary Terrain | Secondary | Rare |
|-------|-----------------|-----------|------|
| **The Roots** | Difficult (rubble) | Flooded | Hazardous (acid) |
| **Muspelheim** | Burning | Hazardous (lava) | Obscuring (smoke) |
| **Niflheim** | Slippery (ice) | Difficult (snow) | Obscuring (blizzard) |
| **Vanaheim** | Difficult (growth) | Obscuring (spores) | Hazardous (toxin) |
| **Svartalfheim** | Hazardous (acid) | Obscuring (darkness) | Slippery (oil) |
| **Alfheim** | Normal/Elevated | Obscuring (light) | Hazardous (corruption) |
| **Jotunheim** | Elevated (ruins) | Difficult (debris) | Slippery (frost) |
| **Midgard** | Normal | Flooded | Difficult (mud) |

### 6.2 Room Type Modifiers

| Room Type | Terrain Tendency |
|-----------|------------------|
| Entry Hall | Minimal terrain |
| Corridor | Linear difficult/hazardous |
| Combat Arena | Mixed tactical terrain |
| Boss Chamber | Signature terrain + hazards |
| Workshop | Slippery (oil), hazardous (acid) |
| Flooded Section | Flooded dominant |

---

## 7. Dynamic Terrain

### 7.1 Terrain Creation Abilities

| Ability | Creates | Duration |
|---------|---------|----------|
| `Ice Storm` (Galdr-caster) | [Slippery] 3×3 area | 3 rounds |
| `Oil Flask` (Brewmaster) | [Slippery] 2×2 area | Until ignited |
| `Wall of Fire` (Galdr-caster) | [Burning] line | Concentration |
| `Spore Burst` (Myr-Stalker) | [Obscuring] 2×2 area | 2 rounds |
| `Acid Vial` (Alchemist) | [Hazardous] single tile | 5 rounds |

### 7.2 Terrain Removal

| Method | Affects | Result |
|--------|---------|--------|
| Fire attack | [Flooded] | Evaporates (temporary) |
| Fire attack | [Slippery] (oil) | → [Burning] |
| Ice attack | [Flooded] | → [Slippery] |
| Ice attack | [Burning] | Extinguishes |
| Wind ability | [Obscuring] | Disperses (1 round) |

---

## 8. AI Terrain Awareness

### 8.1 AI Behavior by Intelligence

| Intelligence | Terrain Awareness |
|--------------|-------------------|
| **Low (1-3)** | Ignores terrain; walks through anything |
| **Medium (4-6)** | Avoids [Hazardous]; uses [Obscuring] for cover |
| **High (7-9)** | Actively pushes players into bad terrain |
| **Brilliant (10+)** | Creates terrain combos; uses Lightning on [Flooded] |

### 8.2 AI Tactical Priorities

| Situation | AI Action |
|-----------|-----------|
| Player in [Flooded] | Prioritize Lightning attacks |
| Player on [Slippery] | Use Push abilities |
| [Burning] between AI and player | Seek alternate route |
| [Obscuring] available | Move into for defense |

---

## 9. Technical Implementation

### 9.1 Data Model

```csharp
public enum TerrainType
{
    Normal,
    Difficult,
    Flooded,
    Obscuring,
    Hazardous,
    Slippery,
    Burning,
    Elevated
}

public record GridTile
{
    public Position Position { get; init; }
    public TerrainType Terrain { get; set; }
    public TerrainSubtype? Subtype { get; set; }
    public int Elevation { get; set; }
    public int RemainingDuration { get; set; }  // For temporary terrain
    public DamageType? HazardDamageType { get; set; }
    public int HazardDamage { get; set; }
}

public enum TerrainSubtype
{
    // Difficult
    Rubble, DeepSnow, TangledRoots, Mud,
    // Flooded
    Water, Coolant, Blood,
    // Obscuring
    Fog, Spores, Steam, Smoke,
    // Hazardous
    Acid, Toxin, RazorShards, Corruption,
    // Slippery
    Ice, Oil, PolishedMetal,
    // Burning
    OilFire, CoalSeam, Lava
}
```

### 9.2 Service Interface

```csharp
public interface ITerrainService
{
    TerrainType GetTerrainAt(Position pos);
    int CalculateMovementCost(Character character, Position from, Position to);
    bool RequiresAcrobaticsCheck(Character character, Position pos, MovementType movement);
    void ApplyEndOfTurnEffects(Character character);
    void CreateTerrain(Position pos, TerrainType type, int duration = -1);
    void RemoveTerrain(Position pos);
    IEnumerable<Position> GetConnectedTiles(Position pos, TerrainType type);
}
```

### 9.3 Integration Points

```csharp
// MovementService.cs
public int CalculateStaminaCost(Character character, Position target)
{
    var baseCost = _movementCostTable[character.MovementType];
    var terrain = _terrainService.GetTerrainAt(target);
    
    return terrain switch
    {
        TerrainType.Difficult or TerrainType.Flooded => baseCost * 2,
        _ => baseCost
    };
}

// CombatEngine.cs - End of Turn
public void ProcessEndOfTurn(Character character)
{
    _terrainService.ApplyEndOfTurnEffects(character);
    // ... other end-of-turn processing
}

// DamageService.cs - Vulnerability
public int CalculateDamage(Character target, int baseDamage, DamageType type)
{
    var terrain = _terrainService.GetTerrainAt(target.Position);
    
    if (terrain == TerrainType.Flooded && type == DamageType.Lightning)
        baseDamage *= 2;
    
    return baseDamage;
}
```

---

## 10. Phased Implementation Guide

### Phase 1: Core Data
- [ ] Implement `TerrainType` and `TerrainSubtype` enums
- [ ] Add `Terrain` property to `GridTile`
- [ ] Implement `ITerrainService` interface

### Phase 2: Movement Integration
- [ ] Hook Stamina calculation to terrain
- [ ] Implement [Slippery] FINESSE checks
- [ ] Integrate with Acrobatics climbing for [Elevated]

### Phase 3: Combat Integration
- [ ] End-of-turn [Hazardous] damage
- [ ] [Flooded] Lightning vulnerability
- [ ] [Burning] ignition checks

### Phase 4: Dynamic Terrain
- [ ] Terrain creation abilities
- [ ] Terrain transformation (freeze, ignite)
- [ ] Duration tracking and cleanup

### Phase 5: AI & UI
- [ ] AI terrain awareness by intelligence
- [ ] Minimap terrain icons
- [ ] Terrain tooltips

---

## 11. Testing Requirements

### 11.1 Unit Tests
- [ ] Movement cost doubles on [Difficult]
- [ ] Lightning damage doubles on [Flooded]
- [ ] [Slippery] triggers FINESSE check on dash
- [ ] [Hazardous] applies damage at end of turn
- [ ] Terrain chain (freeze [Flooded] → [Slippery])

### 11.2 Integration Tests
- [ ] Full movement path through mixed terrain
- [ ] AI avoids [Hazardous] terrain
- [ ] Ability creates terrain → terrain affects combat

### 11.3 Manual QA
- [ ] Minimap displays all terrain types correctly
- [ ] Terrain tooltips show accurate information
- [ ] Visual feedback for terrain effects

---

## 12. Voice Guidance

### 12.1 Tone Profile

| Property | Value |
|----------|-------|
| **Theme** | Environmental storytelling, tactical positioning |
| **Tone** | Descriptive, warning, tactical |
| **Key Words** | Ground, footing, unstable, corroded, frozen, burning |

### 12.2 Narrative Examples

| Terrain | Look Description |
|---------|------------------|
| [Difficult] | "The floor is a treacherous maze of twisted pipes and broken machinery. Every step requires careful placement." |
| [Flooded] | "Murky water laps at ankle height. Somewhere above, exposed cables spark ominously." |
| [Slippery] | "A thin sheen of ice covers the floor, glittering treacherously in the dim light." |
| [Burning] | "Flames dance across the floor where spilled fuel has ignited. The heat is oppressive." |

---

## 13. Related Specifications

| Document | Purpose |
|----------|---------|
| [Dynamic Hazards](dynamic-hazards.md) | Triggered battlefield objects |
| [Acrobatics](../01-core/skills/acrobatics.md) | Movement checks |
| [Combat Resolution](../03-combat/combat-resolution.md) | Damage calculation |
| [Spatial Layout](room-engine/spatial-layout.md) | Grid coordinates |
| [Biomes Overview](biomes/overview.md) | Terrain distribution |

---

## 14. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |
