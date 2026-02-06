# v0.19.x Room Template Biome Assignment

**Version:** 1.0
**Status:** Implementation Reference
**Last Updated:** 2026-02-04
**Purpose:** Complete room template registry with biome assignments from all realm lore
**Implementation Phase:** v0.19.1-v0.19.6

---

## 1. Overview

This document provides a comprehensive registry of all room templates derived from realm lore (§7 of each realm file), including their biome assignments, size classifications, and spawn/hazard specifications.

### 1.1 Template Size Classifications

| Size | Dimensions | Max Encounters | Max Hazards |
|------|-----------|----------------|-------------|
| Small | 1-2 tiles | 1 | 1 |
| Medium | 2-4 tiles | 2 | 2 |
| Large | 4-6 tiles | 3 | 3 |
| XLarge | 6-9 tiles | 4 | 4 |
| Huge | 9+ tiles | 5 | 5 |

---

## 2. Midgard Templates (midgard.md §7)

### 2.1 Greatwood Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Ridge Hold Clearing | Medium | Walled settlement perimeter | None | Humanoid (friendly) |
| Forest Trail | Small | Path through dense growth | Ambush | Blighted Beasts |
| Petrified Grove | Medium | Ancient ironwood ruins | Toxic Spores | Forlorn, Flora |
| Aether-Thistle Patch | Small | Glowing vegetation clearing | Blight Exposure | Blighted Beasts |
| Abandoned Farmstead | Large | Pre-Glitch agricultural ruins | Structural | Scavengers |
| Watchtower Ruins | Medium | Collapsed observation point | Falling Debris | Any |

### 2.2 Asgardian Scar Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Crater Edge | Medium | Impact zone perimeter | Reality Flux | Forlorn |
| Debris Field | Large | Scattered Asgard wreckage | Radiation, Unstable | Forlorn, Constructs |
| Fallen Spire | XLarge | Collapsed orbital structure | Structural Collapse | Forlorn, Boss |
| Blight-Storm Pocket | Medium | Concentrated corruption zone | Blight-Storm | Forlorn |
| Genius Loci Fragment | Huge | Boss encounter area | Reality Tear | Genius Loci |

### 2.3 Souring Mires Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Corduroy Span | Small | Plank crossing | Collapse Risk | None |
| Toxin Pool | Medium | Industrial runoff collection | Chemical Burn | Blighted Beasts |
| Ferry Landing | Medium | Guild-maintained crossing | None | Humanoid (Ferry Guild) |
| Skar-Horde Camp | Large | Raider encampment | Traps | Skar-Horde |
| Flooded Ruins | Large | Submerged pre-Glitch structures | Drowning | Blighted Beasts |

### 2.4 Serpent Fjords Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Rocky Shore | Medium | Coastal terrain | Wave Surge | Blighted Beasts |
| Fishing Dock | Medium | Working harbor | None | Humanoid (friendly) |
| Tidal Cave | Large | Sea-level cavern | Flooding | Hafgufa Brood |
| Shipwreck | Large | Beached vessel | Structural | Scavengers |
| Leviathan Warning Zone | XLarge | Deep water approach | Leviathan Strike | Boss |

---

## 3. Muspelheim Templates (muspelheim.md §7)

### 3.1 Slag Wastes Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Obsidian Field | Medium | Shattered volcanic glass | Heat, Sharp Terrain | Surtr's Scions |
| Thermal Refuge | Small | Cooled lava pocket | None | None |
| Way-Marker Station | Small | Navigation point | Heat | None |
| Slag Flow Edge | Medium | Active lava border | Intense Heat | Surtr's Scions |
| Collapsed Forge | Large | Ruined industrial site | Structural, Heat | Iron-Bane |

### 3.2 Gjöllflow Rivers Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Slag Crust Crossing | Medium | Hardened lava path | Crust Failure | Surtr's Scions |
| Observation Gantry | Medium | Platform over lava | Heat, Fall | None |
| Lava Tube | Large | Volcanic tunnel | Heat, Collapse | Surtr's Scions |
| Flow Confluence | XLarge | Rivers meeting | Extreme Heat | Boss |

### 3.3 Ashfall Drifts Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Ash Sea | Medium | Deep drift terrain | Burial, Respiratory | Slag-Swimmers |
| Buried Structure | Large | Ash-covered ruins | Collapse | Scavengers |
| Probe Station | Small | Navigation checkpoint | None | None |
| Drift Valley | Large | Wind-carved passage | Visibility | Any |

### 3.4 Hearths Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Hearth Plaza | Large | Settlement center | None | Humanoid (Hearth-Clan) |
| Dew-Chamber | Medium | Water collection | None | Humanoid (friendly) |
| Trade Hall | Large | Commerce area | None | Humanoid (mixed) |
| Lava-Tube Dwelling | Medium | Clan residence | None | Humanoid (friendly) |
| Clan Council | Large | Leadership chamber | None | Humanoid (leadership) |

---

## 4. Niflheim Templates (niflheim.md §7)

### 4.1 Permafrost Halls Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Frozen Corridor | Medium | Ice-coated passage | Cold, Slip | Frost Revenants |
| Cryo-Chamber Array | Large | Stasis pod rows | Cold | Einherjar (dormant) |
| Machinery Graveyard | Large | Frozen equipment | Cold, Unstable | Frost Revenants |
| Ice Bridge | Small | Spanning gap | Collapse, Cold | None |
| Preserved Lab | Medium | Frozen research area | Cold | Frost Revenants |

### 4.2 Hvergelmir Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Thermal Drain Approach | Medium | Cold intensifies | Ice-Debt | Frost Revenants |
| Ice Formation Forest | Large | Growing crystal structures | Cold, Entrapment | None |
| Hvergelmir's Maw | Huge | Central cold nexus | Extreme Cold | Hvergelmir Warden |
| Warden Chamber | XLarge | Boss encounter | Ice-Debt Max | Boss |

### 4.3 Archive Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Einherjar Gallery | Large | Frozen soldiers display | Cold, Psychic | Einherjar |
| Data Crystal Chamber | Medium | Information storage | Cold | None |
| Memory Archive | Large | Consciousness storage | Psychic | Einherjar |
| Archive Entrance | Medium | Guarded access | Cold | Einherjar |

---

## 5. Vanaheim Templates (vanaheim.md §7)

### 5.1 Canopy Sea Zone (Z=+2)

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Canopy Platform | Medium | Stable branch intersection | None | Blighted Beasts |
| Sky Looms Station | Large | Ferry anchor point | Predator | Humanoid (Sky Looms) |
| Mother Tree Chamber | Large | Hollow trunk interior | None | Grove-Wardens |
| Rope Bridge Network | Small | Inter-platform crossing | Fall | None |
| Grove-Clan Settlement | Large | Community platform | None | Humanoid (Grove-Clan) |

### 5.2 Gloom-Veil Zone (Z=+1 to Z=0)

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Vine Curtain Passage | Small | Limited visibility | Ambush | Weaponized Flora |
| Bioluminescent Grove | Medium | Navigation landmark | None | Data-Wraiths |
| Emergency Shelter | Small | Fortified platform | None | None |
| Data-Wraith Haunt | Medium | High cognitive hazard | Psychic | Data-Wraiths |
| Trunk Descent | Large | Vertical passage | Fall, Spores | Weaponized Flora |

### 5.3 Under-growth Zone (Z=-1 to Z=0)

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Biomass Shallows | Medium | Navigable swamp | Contamination | The Unraveled |
| Unraveled Territory | Large | Post-human domain | Red Contamination | The Unraveled |
| Root Network | Medium | Tangled passage | Entanglement | Weaponized Flora |
| Un-Womb Approach | XLarge | Boss encounter area | Max Contamination | Un-Womb Spawn |
| Idunn Vault Ruins | Huge | Archive access (theoretical) | All | Boss |

---

## 6. Alfheim Templates (alfheim.md §7)

### 6.1 Luminous Approaches Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Crystal Gateway | Medium | Entry point | Reality Flux | Dökkálfar |
| Light Garden | Large | Aetheric flora | Exposure | Aether Constructs |
| Dökkálfar Waystation | Medium | Guide services | None | Dökkálfar |
| Prism Corridor | Medium | Light-bending passage | Disorientation | None |

### 6.2 Prismatic Depths Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Color Storm Chamber | Large | Chromatic chaos | Reality Flux | Aether Entities |
| Refracted Hall | Medium | Multiple reflections | Cognitive | Dökkálfar |
| Aetheric Nexus | XLarge | Power concentration | Exposure Max | Boss |
| Memory Fragment | Medium | Reality echo | Psychic | Forlorn |

### 6.3 Dreaming Core Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Dream Boundary | Large | Reality/Dream threshold | Cognitive Max | Dream Entities |
| Echo Chamber | Medium | Repeated memories | Psychic | Forlorn |
| Core Approach | XLarge | Final approach | All Hazards | Boss |
| Dreaming Heart | Huge | Boss encounter | Reality Collapse | Boss |

---

## 7. Asgard Templates (asgard.md §7)

### 7.1 Debris Fields Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Orbital Wreckage | Large | Floating debris | Zero-G, Impact | Corrupted Constructs |
| Station Fragment | XLarge | Preserved section | CPS, Structural | Forlorn, Constructs |
| Navigation Hazard | Medium | Dense debris | Impact | None |
| Docking Bay Ruins | Large | Access point | Structural | Constructs |

### 7.2 Heimdallr Array Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Antenna Cluster | Large | Communication equipment | Energy Discharge | Constructs |
| Control Station | Medium | Operations center | CPS | Automated |
| Signal Source | XLarge | Transmission origin | CPS Max | Boss |
| Array Approach | Medium | Access corridor | CPS | Constructs |

### 7.3 Archive Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Data Vault | Large | Information storage | CPS, Temporal | Constructs |
| Odin Chamber | Huge | Central AI core | CPS Max, Temporal | ODIN Fragment |
| Memory Bank | Medium | Consciousness storage | Psychic | Forlorn |
| Processing Core | XLarge | Computation center | CPS | Boss |

---

## 8. Svartalfheim Templates (svartalfheim.md §7)

### 8.1 Guildlands Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Guild Hall | Large | Craftsman headquarters | None | Dvergr (friendly) |
| Forge Complex | XLarge | Industrial production | Heat | Dvergr, Constructs |
| Trade Plaza | Large | Commerce center | None | Mixed |
| Mining Gallery | Large | Resource extraction | Collapse | Dvergr, Constructs |
| Guild Highway | Medium | Main thoroughfare | None | Patrols |

### 8.2 Blackvein Depths Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Blight Vein | Medium | Corruption source | Blight Exposure | Blighted |
| Abandoned Mine | Large | Sealed operation | Collapse, Blight | Scavengers |
| Darkness Chamber | Medium | Total dark | Darkness | Skittering Horrors |
| Contamination Zone | Large | Active blight | Blight Max | Boss |

### 8.3 Crystal Grottos Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Crystal Garden | Large | Gem formations | Energy | Constructs |
| Resonance Chamber | Medium | Vibrating crystals | Sonic | None |
| Mining Face | Medium | Active extraction | Collapse | Dvergr |
| Grotto Cathedral | Huge | Massive cavern | All | Boss |

---

## 9. Jötunheim Templates (jotunheim.md §7)

### 9.1 Bone Yards Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Collapsed Corridor | Medium | Rusted industrial passage | Debris | Rust-Thralls |
| Giant Workshop | Large | Abandoned tools | Scale Fall | Rust-Thralls |
| Machinery Graveyard | Large | Skeletal machinery | Unstable | Collapsed Giants |
| Entry Gate | Medium | Scale transition | Fall | None |
| Bone Field | Large | Giant remains scattered | Debris | Scavengers |

### 9.2 Utgard's Shadow Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Utgard Street | Medium | Giant-scale urban ruins | Scale, Darkness | Loom-Weavers |
| Loom Platform | Large | Industrial work platform | Fall | Loom-Weavers |
| Collapsed Tower | XLarge | Fallen structure | Debris, Fall | Collapsed Giants |
| Shadow Passage | Medium | Dark corridor | Darkness | Rust-Thralls |

### 9.3 Grinding Hall Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Active Forge | Large | Functional machinery | Industrial, Heat | Gear-Horrors |
| Gear Chamber | XLarge | Massive mechanisms | Crushing | Gear-Horrors |
| Conveyor System | Large | Moving platforms | Fall, Crushing | Loom-Weavers |
| Overseer Throne | Huge | Boss encounter | All Industrial | Loom Overseer |
| Power Core | XLarge | Energy source | Industrial Max | Boss |

---

## 10. Helheim Templates (helheim.md §7)

### 10.1 Labyrinth Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Corroded Passage | Medium | Acid-damaged corridor | Acid, Structural | Decay Spawn |
| Junction Hub | Large | Multiple passages | Gas Pocket | Decay Spawn |
| Collapsed Section | Large | Blocked route | Structural | Scavengers |
| Acid Pool Room | Medium | Hazardous liquid | Acid | None |
| Rusted Gate | Small | Barrier passage | Structural | None |

### 10.2 Sump Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Chemical Lake | Large | Toxic liquid | Chemical, Gas | Decay Spawn |
| Gas Vent Field | Medium | Erupting toxins | Gas Max | None |
| Processing Plant | XLarge | Industrial ruins | All Chemical | Decay Constructs |
| Sump Heart | Huge | Boss encounter | Max Contamination | Boss |

### 10.3 Sunken Districts Zone

| Template | Size | Description | Hazards | Encounters |
|----------|------|-------------|---------|------------|
| Flooded Street | Large | Submerged urban | Drowning, Acid | Decay Spawn |
| Waterlogged Building | Large | Partially flooded | Structural | Scavengers |
| Sewage Flow | Medium | Waste passage | Disease, Gas | Decay Spawn |
| Collapsed Dam | XLarge | Flood source | Flooding | Boss |

---

## 11. Template Assignment Schema

```json
{
  "$schema": "room-template.schema.json",
  "type": "object",
  "required": ["id", "name", "size", "realm", "zone", "hazards", "encounters"],
  "properties": {
    "id": { "type": "string" },
    "name": { "type": "string" },
    "size": {
      "type": "string",
      "enum": ["Small", "Medium", "Large", "XLarge", "Huge"]
    },
    "realm": { "type": "string" },
    "zone": { "type": "string" },
    "description": { "type": "string" },
    "hazards": {
      "type": "array",
      "items": { "type": "string" }
    },
    "encounters": {
      "type": "array",
      "items": { "type": "string" }
    },
    "isBossRoom": { "type": "boolean", "default": false },
    "isSettlement": { "type": "boolean", "default": false },
    "verticalLevel": { "type": "integer" },
    "loreReference": { "type": "string" }
  }
}
```

---

## 12. Implementation Interface

```csharp
public interface IRoomTemplateService
{
    RoomTemplate GetTemplate(string templateId);
    IReadOnlyList<RoomTemplate> GetTemplatesForZone(RealmId realm, string zoneId);
    IReadOnlyList<RoomTemplate> GetTemplatesBySize(TemplateSize size);
    IReadOnlyList<RoomTemplate> GetBossTemplates(RealmId realm);
    IReadOnlyList<RoomTemplate> GetSettlementTemplates(RealmId realm);
    RoomTemplate GetRandomTemplate(BiomeZone zone, TemplateSize? preferredSize = null);
}

public record RoomTemplate(
    string Id,
    string Name,
    TemplateSize Size,
    RealmId Realm,
    string ZoneId,
    string Description,
    string[] Hazards,
    string[] EncounterTypes,
    bool IsBossRoom,
    bool IsSettlement,
    int? VerticalLevel,
    string LoreReference);

public enum TemplateSize
{
    Small,
    Medium,
    Large,
    XLarge,
    Huge
}
```

---

## 13. Template Statistics

| Realm | Small | Medium | Large | XLarge | Huge | Total |
|-------|-------|--------|-------|--------|------|-------|
| Midgard | 4 | 8 | 6 | 1 | 1 | 20 |
| Muspelheim | 3 | 6 | 7 | 1 | 0 | 17 |
| Niflheim | 1 | 5 | 5 | 1 | 1 | 13 |
| Vanaheim | 2 | 5 | 5 | 1 | 1 | 14 |
| Alfheim | 0 | 5 | 4 | 2 | 1 | 12 |
| Asgard | 0 | 4 | 4 | 3 | 1 | 12 |
| Svartalfheim | 0 | 4 | 5 | 1 | 1 | 11 |
| Jötunheim | 0 | 4 | 5 | 2 | 1 | 12 |
| Helheim | 1 | 4 | 5 | 2 | 1 | 13 |
| **Total** | **11** | **45** | **46** | **14** | **8** | **124** |

---

## 14. Acceptance Criteria

- [ ] All 124 templates defined in configuration
- [ ] Template-to-zone mappings complete
- [ ] Size distribution balanced per realm
- [ ] Hazard assignments consistent with realm themes
- [ ] Encounter types aligned with enemy spawn pools
- [ ] Boss templates identified for all realms
- [ ] Settlement templates identified where applicable
- [ ] Vertical level assigned for Vanaheim templates
- [ ] ~12 unit tests pass

---

## 15. Lore Cross-Reference Index

| Realm | Lore Section | Template Count |
|-------|--------------|----------------|
| Midgard | midgard.md §7 | 20 |
| Muspelheim | muspelheim.md §7 | 17 |
| Niflheim | niflheim.md §7 | 13 |
| Vanaheim | vanaheim.md §7 | 14 |
| Alfheim | alfheim.md §7 | 12 |
| Asgard | asgard.md §7 | 12 |
| Svartalfheim | svartalfheim.md §7 | 11 |
| Jötunheim | jotunheim.md §7 | 12 |
| Helheim | helheim.md §7 | 13 |

---

_This room template registry provides complete biome assignment specifications derived from all realm lore documents, enabling procedural dungeon generation with lore-compliant room distribution._
