# v0.19.x Descriptor System Integration

**Version:** 1.0
**Status:** Implementation Reference
**Last Updated:** 2026-02-04
**Purpose:** Specification for integrating biome thematic descriptors into room generation
**Implementation Phase:** v0.19.6b

---

## 1. Overview

The descriptor system provides procedural room description generation that reflects biome-specific themes, environmental conditions, and atmospheric elements. Each realm has a unique thematic modifier, color palette, and ambient sound library.

### 1.1 Lore Sources

| Realm | Lore Document | Descriptor Section |
|-------|--------------|-------------------|
| Midgard | `midgard.md` | §11 / §12 |
| Muspelheim | `muspelheim.md` | §12 |
| Niflheim | `niflheim.md` | §12 |
| Vanaheim | `vanaheim.md` | §12 |
| Alfheim | `alfheim.md` | §12 |
| Asgard | `asgard.md` | §12 |
| Svartalfheim | `svartalfheim.md` | §12 |
| Jötunheim | `jotunheim.md` | §12 |
| Helheim | `helheim.md` | §12 |

---

## 2. Biome Thematic Modifiers

### 2.1 Complete Modifier Registry

| Realm | Modifier Name | Adjective | Detail Fragment | Color Palette | Lore Ref |
|-------|--------------|-----------|-----------------|---------------|----------|
| **Midgard** | Tamed | "tamed" | "carved from catastrophe by stubborn survivors" | green-brown-gray-rust | midgard.md §12.1 |
| **Muspelheim** | Scorched | "scorched" | "radiates intense heat and shows signs of fire damage" | red-orange-black-rust | muspelheim.md §12.1 |
| **Niflheim** | Frozen | "frozen" | "encased in layers of ice, preserved in eternal cold" | white-blue-gray-black | niflheim.md §12.1 |
| **Vanaheim** | Overgrown | "overgrown" | "consumed by aggressive bio-engineered vegetation" | green-gold-purple-black | vanaheim.md §12.1 |
| **Alfheim** | Glimmering | "glimmering" | "warped by intense Aetheric saturation" | prismatic-white-gold-violet | alfheim.md §12.1 |
| **Asgard** | Shattered | "shattered" | "broken by orbital descent and temporal decay" | gold-black-red-void | asgard.md §12.1 |
| **Svartalfheim** | Forged | "forged" | "precision-crafted by Dvergr engineering" | orange-black-steel-copper | svartalfheim.md §12.1 |
| **Jötunheim** | Titanic | "titanic" | "built to a scale that dwarfs human comprehension" | rust-gray-brown-shadow | jotunheim.md §12.1 |
| **Helheim** | Gangrenous | "gangrenous" | "corroded by industrial waste and biological decay" | green-brown-gray-bile | helheim.md §12.1 |

### 2.2 Modifier Schema

```json
{
  "$schema": "biome-modifier.schema.json",
  "type": "object",
  "required": ["id", "name", "adjective", "detailFragment", "colorPalette", "ambientSounds"],
  "properties": {
    "id": { "type": "string" },
    "name": { "type": "string" },
    "adjective": { "type": "string" },
    "detailFragment": { "type": "string" },
    "colorPalette": {
      "type": "array",
      "items": { "type": "string" },
      "minItems": 4,
      "maxItems": 5
    },
    "ambientSounds": {
      "type": "array",
      "items": { "type": "string" },
      "minItems": 3
    },
    "loreReference": { "type": "string" }
  }
}
```

---

## 3. Ambient Sound Libraries

### 3.1 Per-Realm Sound Sets

```json
{
  "midgard": {
    "universal": ["wind through ruins", "distant livestock", "wolf howl"],
    "greatwood": ["rustling leaves", "bird calls", "branch creaking"],
    "scar": ["static discharge", "metal stress", "silence punctuated by echoes"],
    "mires": ["bubbling water", "insect buzz", "splash"],
    "fjords": ["wave crash", "seabird cry", "ship creak"]
  },
  "muspelheim": {
    "universal": ["crackling flames", "hissing steam", "metal groaning", "distant rumbling"],
    "slag": ["glass cracking", "heat shimmer hum"],
    "gjollflow": ["molten burble", "sizzling"],
    "ashfall": ["wind howl", "muffled footsteps", "drift shifting"],
    "hearths": ["forge clang", "voices", "water drip"]
  },
  "niflheim": {
    "universal": ["wind howling", "ice cracking", "distant echoes", "silence"],
    "permafrost": ["frost creep", "machinery groan"],
    "hvergelmir": ["thermal drain hum", "deep cold vibration"],
    "archive": ["crystal resonance", "whispered voices", "frozen breath"]
  },
  "vanaheim": {
    "universal": ["rustling leaves", "insect chorus", "dripping water", "creaking"],
    "canopy": ["bird calls", "wind through leaves", "distant falls"],
    "gloom": ["fungal pop", "creature movement", "humid drip"],
    "undergrowth": ["root shift", "spore release", "predator call"]
  },
  "alfheim": {
    "universal": ["crystalline chimes", "static hum", "whispers", "reality tear"],
    "luminous": ["light crackle", "echo distortion"],
    "prismatic": ["color shift hum", "spatial warble"],
    "dreaming": ["distant voices", "memory fragment", "time skip"]
  },
  "asgard": {
    "universal": ["silence", "distant signal", "metal stress", "void echo"],
    "spire": ["debris shift", "structural groan"],
    "heimdallr": ["signal pulse", "antenna hum"],
    "archive": ["data whisper", "power fluctuation", "Odin-tone"]
  },
  "svartalfheim": {
    "universal": ["hammer strikes", "bellows", "machinery", "darkness"],
    "guildlands": ["forge roar", "chain clank", "Dvergr voices"],
    "blackveins": ["dripping", "skittering", "absolute silence"],
    "grottos": ["crystal hum", "blight pulse"]
  },
  "jotunheim": {
    "universal": ["groaning metal", "distant crashes", "wind through giants"],
    "boneyards": ["metal settling", "rust flake"],
    "utgard": ["loom rumble", "titanic footstep echo"],
    "grinding": ["machinery thunder", "chain rattle"]
  },
  "helheim": {
    "universal": ["bubbling", "dripping", "hissing gas", "structural collapse"],
    "labyrinth": ["acid drip", "corroded metal creak"],
    "sump": ["chemical burble", "gas vent"],
    "sunken": ["flooding water", "structure groan"]
  }
}
```

---

## 4. Description Template System

### 4.1 Template Structure

Each zone has multiple description templates that can be populated with context-specific variables:

```
{modifier} {room_type} {specific_detail}. {environmental_observation}. {sensory_detail}.
```

**Variables:**
- `{modifier}` — Biome adjective
- `{room_type}` — Room template category
- `{specific_detail}` — Zone-specific feature
- `{environmental_observation}` — Condition-based detail
- `{sensory_detail}` — Sound/smell/feeling
- `{light}` — Light level descriptor
- `{temperature}` — Temperature descriptor

### 4.2 Sample Templates by Realm

#### Midgard Templates

```json
{
  "midgard-greatwood": [
    "A {modifier} clearing opens before the ferrocrete walls of a Ridge Hold. The watch-fire burns steady on the tower above. Beyond the gate, Grit-Corn fields stretch toward the treeline where feral designer trees have grown into dense, unpredictable wilderness.",
    "The Greatwood canopy closes overhead, filtering {light} through mutated leaves. The air tastes of spores and rot. Something large moves in the undergrowth.",
    "Petrified ironwood trunks support a {modifier} forest floor. Aether-Thistle patches glow faintly between the roots. The path forward is marked by old Hold trail signs, half-consumed by growth."
  ],
  "midgard-scar": [
    "The Scar perimeter stretches before you. Orbital debris juts from poisoned earth at impossible angles. The air tastes of ozone and ash. In the distance, Blight-Storm lightning illuminates the shattered silhouette of fallen Asgard infrastructure.",
    "Reality bleeds at the crater's edge. Colors shift. Gravity feels wrong. A Forlorn Echo drifts through the wreckage, its form flickering between states.",
    "The {modifier} impact zone shows the violence of Asgard's fall. Ferrocrete walls have been shattered. Pre-Glitch technology lies scattered, corrupted by Runic Blight. The Genius Loci's presence weighs on your mind."
  ],
  "midgard-mires": [
    "The corduroy span crosses the Mire in uneven planks. Toxic water laps at the edges, iridescent with industrial runoff from Jötunheim and Helheim. Ferry Guild markers indicate the next safe crossing—three hours north, if the flood holds.",
    "A {modifier} wetland spreads before you, choked with rust-colored reeds. The water is poison. The soil is worse. Skar-Horde totems mark territorial boundaries.",
    "The seasonal flood has retreated, leaving behind pools of concentrated toxin and the bones of those who didn't evacuate in time."
  ],
  "midgard-fjords": [
    "Rocky coast gives way to tidal pools. The fishing docks are built for speed evacuation—Hafgufa's Brood patrol these waters. Warning signs mark the shallows.",
    "The {modifier} fjord opens before you, salt air mixing with the chemical tang of poisoned seas. Serpent Fjord fishing boats cluster near the shore, their crews watching the water with practiced fear.",
    "A tidal cave opens in the cliff face. The locals avoid this place. Something large disturbed the shallows recently."
  ]
}
```

#### Muspelheim Templates

```json
{
  "muspelheim-slag": [
    "A {modifier} forge hall dominates this space. Rivers of molten slag cut through blackened infrastructure. The air shimmers with superheated mirage-distortions. Somewhere in the gloom, metal groans under thermal stress.",
    "The Slag Wastes extend to the horizon—a sea of shattered obsidian under a rust-colored sky. Heat rises in visible waves. Nothing moves. Nothing grows. Nothing survives here that doesn't have to.",
    "Volcanic glass crunches underfoot. The {modifier} terrain offers no shade, no shelter, no mercy. Emergency way-markers indicate the next thermal refuge is two kilometers east."
  ],
  "muspelheim-gjollflow": [
    "The Gjöllflow cuts across your path, a slow river of molten slag and industrial runoff. The far bank shimmers through the heat haze. A slag crust has formed at the narrowest point—maybe stable enough to cross. Maybe not.",
    "An observation gantry extends over the {modifier} lava river. The platform's thermal shielding has partially failed. Below, Surtr's Scions patrol the molten surface.",
    "The crossing protocol is clear: probe the crust, listen for hollow-ring resonance, abort on two consecutive failures. The alternative is instant incineration."
  ],
  "muspelheim-ashfall": [
    "The Ashfall Drifts swallow the world in rust-colored silence. Visibility drops to thirty meters. The ash shifts beneath your feet—stable here, but drifts have been known to swallow travelers without warning.",
    "A {modifier} sea of soot stretches in every direction. Somewhere beneath the surface, Slag-Swimmers hunt. The probe-and-advance protocol is your only defense.",
    "The ash tastes of fire and chemicals. Unprotected breathing is not recommended. Your equipment will need maintenance within four hours."
  ],
  "muspelheim-hearths": [
    "The Hearth opens before you—an island of survivable warmth in an ocean of fire. Hearth-Clan children watch from cooled lava-tube entrances. The Dew-Chamber hums with life-sustaining condensation.",
    "Glimmerheim's main plaza is carved from basalt. Trade-Speakers negotiate with Dvergr caravans. The watch-fire on the council hall burns low—Elding-Storm on the horizon.",
    "A {modifier} settlement clings to this stable vent. The Hearth-Clans have survived here for generations, reverent of fire's fickle nature, dependent on their Dew-Chambers."
  ]
}
```

#### Niflheim Templates

```json
{
  "niflheim-permafrost": [
    "The Permafrost Halls stretch into blue-white darkness. Ice coats every surface—walls, machinery, the preserved bodies of those who ventured too deep. Your breath fogs. The cold is survivable. For now.",
    "A {modifier} corridor extends before you, lined with frozen machinery from the Age of Forging. Frost Revenants patrol the far end, their forms barely distinguishable from the ice.",
    "Cryo-chambers line the walls, their occupants preserved in perfect stasis. Some have been here since before the Glitch. Some are more recent."
  ],
  "niflheim-hvergelmir": [
    "Hvergelmir's Maw opens before you—the heart of cold, the thermal drain that feeds Niflheim's eternal winter. You feel the Ice-Debt begin to accumulate the moment you cross the threshold.",
    "The temperature drops to impossible depths. Heat is drawn from your body, your equipment, your soul. The {modifier} core of Niflheim hungers.",
    "Ice formations grow in real-time, reaching toward any source of warmth. The Hvergelmir Warden waits at the center, wreathed in frost that burns."
  ],
  "niflheim-archive": [
    "The Ice Archives preserve the Einherjar—soldiers of the old Aesir, frozen in the moment of their last duty. Their consciousness lingers, bleeding into the ice. You feel their memories pressing against your mind.",
    "Rows of {modifier} soldiers stand in eternal attention, their eyes tracking your movement. They are not hostile. They are not friendly. They are preserved.",
    "Data crystals grow from the walls like frozen flowers, containing information from before the Glitch. The Einherjar guarded this knowledge. They guard it still."
  ]
}
```

---

## 5. Dynamic Generation System

### 5.1 Description Generation Interface

```csharp
public interface IDescriptorService
{
    string GenerateRoomDescription(Room room, DescriptorContext context);
    string[] GetAmbientSounds(BiomeZone zone);
    string GetTemperatureDescriptor(float celsius);
    string GetLightDescriptor(float lightLevel);
    string GetModifierAdjective(RealmId realm);
}

public class DescriptorContext
{
    public BiomeDefinition Biome { get; set; }
    public BiomeZone Zone { get; set; }
    public RoomTemplate Template { get; set; }
    public EnvironmentalCondition[] ActiveConditions { get; set; }
    public TimeOfDay TimeOfDay { get; set; }
    public WeatherCondition Weather { get; set; }
}
```

### 5.2 Temperature Descriptors

| Range (°C) | Descriptor |
|------------|------------|
| < -50 | "killing cold" |
| -50 to -20 | "bitter cold" |
| -20 to 0 | "freezing" |
| 0 to 10 | "cold" |
| 10 to 20 | "cool" |
| 20 to 30 | "warm" |
| 30 to 50 | "hot" |
| 50 to 80 | "sweltering" |
| 80 to 120 | "scorching" |
| > 120 | "lethal heat" |

### 5.3 Light Level Descriptors

| Range (0-1) | Descriptor |
|-------------|------------|
| 0.0 - 0.1 | "absolute darkness" |
| 0.1 - 0.3 | "deep shadow" |
| 0.3 - 0.5 | "dim light" |
| 0.5 - 0.7 | "moderate light" |
| 0.7 - 0.9 | "bright light" |
| 0.9 - 1.0 | "blinding light" |

---

## 6. Implementation Requirements

### 6.1 Configuration Files

| File | Purpose |
|------|---------|
| `config/descriptors/biome-modifiers.json` | Thematic modifier definitions |
| `config/descriptors/ambient-sounds.json` | Sound library per zone |
| `config/descriptors/templates/{realm}.json` | Description templates per realm |
| `config/descriptors/temperature-descriptors.json` | Temperature phrase mapping |
| `config/descriptors/light-descriptors.json` | Light level phrase mapping |

### 6.2 Acceptance Criteria

- [ ] All 9 realms have thematic modifiers defined
- [ ] All zones have at least 3 description templates
- [ ] Ambient sound library complete for all zones
- [ ] Temperature descriptors cover full range
- [ ] Light descriptors cover full range
- [ ] IDescriptorService generates valid descriptions
- [ ] Descriptions incorporate active environmental conditions
- [ ] Color palette available for UI theming
- [ ] ~12 unit tests pass

---

## 7. Cross-Reference Index

| Realm | Modifier | Lore Section |
|-------|----------|--------------|
| Midgard | Tamed | midgard.md §12 |
| Muspelheim | Scorched | muspelheim.md §12 |
| Niflheim | Frozen | niflheim.md §12 |
| Vanaheim | Overgrown | vanaheim.md §12 |
| Alfheim | Glimmering | alfheim.md §12 |
| Asgard | Shattered | asgard.md §12 |
| Svartalfheim | Forged | svartalfheim.md §12 |
| Jötunheim | Titanic | jotunheim.md §12 |
| Helheim | Gangrenous | helheim.md §12 |

---

_This descriptor system specification enables procedural room description generation with full lore compliance. All templates and modifiers are derived from canonical lore documents._
