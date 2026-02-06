# v0.19.x Zone-Specific Hazard Registry

**Version:** 1.0
**Status:** Implementation Reference
**Last Updated:** 2026-02-04
**Purpose:** Complete specification of all zone-specific hazards across realms

---

## 1. Overview

This document defines all zone-specific hazards that supplement the primary environmental conditions. These hazards are distinct from ambient conditions and trigger based on specific circumstances.

### 1.1 Hazard Categories

| Category | Trigger Type | Examples |
|----------|-------------|----------|
| **Ambient** | Per-turn automatic | IntenseHeat, ExtremeCold |
| **Contact** | Touch/collision | Rust-Rot Moss, Aether-Thistle |
| **Zone Entry** | Entering area | Blight-Storm, Temporal Pocket |
| **Proximity** | Distance-based | Gravitational Anomaly, Leviathan |
| **Time-Based** | Scheduled/periodic | Seasonal Flood, Elding-Storm |
| **Triggered** | Player action | Volatile Gas (ignition), Slag Collapse |

---

## 2. Midgard Hazards

**Lore Source:** `midgard.md` §9.2

### 2.1 Universal Hazards (All Zones)

#### Rust-Rot Moss
```json
{
  "id": "haz-rust-rot",
  "name": "Rust-Rot Moss",
  "category": "contact",
  "trigger": "Contact with metal equipment",
  "effect": "Equipment degradation",
  "dc": null,
  "damage": "1d4 durability to metal items",
  "frequency": "per contact",
  "mitigation": "Avoid contact; coating equipment with oil",
  "loreReference": "midgard.md §9.2",
  "zones": ["all-midgard"]
}
```

### 2.2 Greatwood Hazards

#### Aether-Thistle
```json
{
  "id": "haz-aether-thistle",
  "name": "Aether-Thistle",
  "category": "contact",
  "trigger": "Contact with thistle patches",
  "effect": "Psychic static interferes with concentration",
  "dc": null,
  "damage": "+5 Stress",
  "frequency": "per contact",
  "mitigation": "Avoid patches; thick gloves negate",
  "loreReference": "midgard.md §9.2",
  "zones": ["midgard-greatwood"]
}
```

### 2.3 Asgardian Scar Hazards

#### Blight-Storm
```json
{
  "id": "haz-blight-storm",
  "name": "Blight-Storm",
  "category": "zone-entry",
  "trigger": "Entering active storm zone",
  "effect": "Corruption accumulation + Disoriented status",
  "dc": 12,
  "attribute": "WILL",
  "damage": "2d6 Corruption on failed save",
  "statusOnFail": "Disoriented (disadvantage on navigation)",
  "frequency": "per storm exposure",
  "duration": "1d4 hours",
  "warningSign": "Sky turns green-black; static discharge on metal",
  "mitigation": "Shelter; Blight-ward amulet (DC -4)",
  "loreReference": "midgard.md §9.2",
  "zones": ["midgard-scar"]
}
```

#### Gravitational Anomaly (S2)
```json
{
  "id": "haz-grav-anomaly",
  "name": "Gravitational Anomaly (S2 Blight Effect)",
  "category": "proximity",
  "trigger": "Entering 20ft radius of anomaly center",
  "effect": "Forced movement toward center",
  "dc": 14,
  "attribute": "AGILITY",
  "damage": "Fall damage (height varies)",
  "onFail": "Pulled 10ft toward center; prone",
  "frequency": "start of turn in area",
  "mitigation": "Magnetic anchors; staying prone",
  "loreReference": "midgard.md §9.2",
  "zones": ["midgard-scar"]
}
```

#### Temporal Pocket (T1)
```json
{
  "id": "haz-temporal-pocket",
  "name": "Temporal Pocket (T1 Blight Effect)",
  "category": "zone-entry",
  "trigger": "Entering pocket boundary",
  "effect": "Time dilation; turn disruption",
  "dc": 12,
  "attribute": "WILL",
  "damage": "Memory fragment (lose 1d4 hours of recent memory)",
  "onFail": "Skip next turn (frozen in time)",
  "frequency": "on entry",
  "warningSign": "Visual distortion; echoes of movement",
  "mitigation": "Avoid; temporal anchor artifact",
  "loreReference": "midgard.md §9.2",
  "zones": ["midgard-scar"]
}
```

### 2.4 Souring Mires Hazards

#### Toxic Runoff
```json
{
  "id": "haz-toxic-runoff",
  "name": "Toxic Runoff",
  "category": "contact",
  "trigger": "Immersion in water",
  "effect": "Poison damage + [Poisoned] status",
  "dc": 14,
  "attribute": "STURDINESS",
  "damage": "2d8 Poison",
  "statusOnFail": "Poisoned (1 hour)",
  "frequency": "per immersion",
  "mitigation": "Hazmat gear; staying on paths",
  "loreReference": "midgard.md §9.2",
  "zones": ["midgard-mires"]
}
```

#### Seasonal Flood
```json
{
  "id": "haz-seasonal-flood",
  "name": "Seasonal Flood",
  "category": "time-based",
  "trigger": "Seasonal conditions (spring thaw)",
  "effect": "Route closure; drowning hazard",
  "dc": null,
  "damage": "Drowning if caught in flood",
  "duration": "2-4 weeks",
  "prediction": "Ferry Guild maintains flood calendar",
  "mitigation": "Avoid during flood season; high-ground routes",
  "loreReference": "midgard.md §9.2",
  "zones": ["midgard-mires"]
}
```

### 2.5 Serpent Fjords Hazards

#### Leviathan Strike
```json
{
  "id": "haz-leviathan-strike",
  "name": "Leviathan Strike (Hafgufa's Brood)",
  "category": "proximity",
  "trigger": "Proximity to water (within 15ft)",
  "effect": "Massive tentacle strike from below",
  "dc": 14,
  "attribute": "AGILITY",
  "damage": "4d8 Physical",
  "onFail": "Grappled; pulled toward water",
  "frequency": "Random (1-in-6 per 10 minutes near water)",
  "warningSign": "Water disturbance; seabirds fleeing",
  "mitigation": "Stay away from water edge; lookout watches",
  "loreReference": "midgard.md §9.2",
  "zones": ["midgard-fjords"]
}
```

---

## 3. Muspelheim Hazards

**Lore Source:** `muspelheim.md` §10.2

### 3.1 Universal Hazards (All Zones)

#### Steam Vent
```json
{
  "id": "haz-steam-vent",
  "name": "Steam Vent",
  "category": "proximity",
  "trigger": "Proximity to vent (10ft) OR timer (d6 rounds)",
  "effect": "Fire damage + Disoriented status",
  "dc": 12,
  "attribute": "AGILITY",
  "damage": "2d8 Fire",
  "statusOnFail": "Disoriented (1 round)",
  "frequency": "Erupts every 1d6 rounds",
  "warningSign": "Hissing sound; steam wisps",
  "mitigation": "Detect pattern; maintain distance",
  "loreReference": "muspelheim.md §10.2",
  "zones": ["all-muspelheim"]
}
```

#### Volatile Gas Pocket
```json
{
  "id": "haz-volatile-gas",
  "name": "Volatile Gas Pocket",
  "category": "triggered",
  "trigger": "Ignition source (fire spell, torch, spark)",
  "effect": "AoE explosion",
  "dc": null,
  "damage": "4d6 Fire (20ft radius)",
  "save": "AGILITY DC 14 for half",
  "frequency": "One-time per pocket",
  "warningSign": "Sulfur smell; shimmering air",
  "detection": "DC 12 Perception to notice",
  "mitigation": "No open flames; detect before entering",
  "loreReference": "muspelheim.md §10.2",
  "zones": ["all-muspelheim"]
}
```

### 3.2 Gjöllflow Rivers Hazards

#### Slag Crust Collapse
```json
{
  "id": "haz-slag-collapse",
  "name": "Slag Crust Collapse",
  "category": "triggered",
  "trigger": "Failed probe check OR weight exceeds threshold",
  "effect": "Fall into molten slag",
  "dc": 12,
  "attribute": "AGILITY",
  "damage": "Instant death (lava immersion)",
  "probeProtocol": {
    "check": "AGILITY DC 12 per section",
    "abortCondition": ">2 consecutive hollow-ring detections",
    "emergencySpan": "Available at crossing points"
  },
  "warningSign": "Hollow-ring resonance on probe",
  "mitigation": "Proper probe testing; emergency spans",
  "loreReference": "muspelheim.md §10.2",
  "zones": ["muspelheim-gjollflow"]
}
```

### 3.3 Ashfall Drifts Hazards

#### Ashfall Drift Collapse
```json
{
  "id": "haz-ashfall-collapse",
  "name": "Ashfall Drift Collapse",
  "category": "triggered",
  "trigger": "Weight on unstable drift OR rapid movement",
  "effect": "Buried in ash",
  "dc": 14,
  "attribute": "MIGHT",
  "damage": "Suffocation (1d4 rounds to escape)",
  "onFail": "Buried; restrained; suffocating",
  "frequency": "Per unstable drift crossed",
  "warningSign": "Soft, shifting ground; recent disturbance",
  "mitigation": "Probe-and-advance protocol; roped travel",
  "loreReference": "muspelheim.md §10.2",
  "zones": ["muspelheim-ashfall"]
}
```

### 3.4 Elding-Storm (Weather Hazard)

```json
{
  "id": "haz-elding-storm",
  "name": "Elding-Storm Cell",
  "category": "time-based",
  "trigger": "Weather pattern; Surtr AI threat response",
  "effect": "Fire + Lightning damage; zero visibility",
  "dc": null,
  "damage": "3d6 Fire + 2d6 Lightning per round exposed",
  "duration": "1d4 hours (can extend near Forge Core)",
  "warningSign": [
    "Ash color shifts rust to black",
    "Temperature spike (+10°C in 10 min)",
    "Pilot-Lights become agitated",
    "Static discharge on metal"
  ],
  "visibility": "Zero (heavily obscured)",
  "mitigation": "Shelter immediately; Hearth protection",
  "loreReference": "muspelheim.md §10.3",
  "zones": ["all-muspelheim"]
}
```

---

## 4. Niflheim Hazards

**Lore Source:** `niflheim.md` §10.2

### 4.1 Universal Hazards

#### Ice Crack
```json
{
  "id": "haz-ice-crack",
  "name": "Ice Crack",
  "category": "triggered",
  "trigger": "Weight on weakened ice",
  "effect": "Fall into crevasse",
  "dc": 12,
  "attribute": "AGILITY",
  "damage": "Fall damage (2d6 to 6d6 based on depth)",
  "onFail": "Fall; potentially trapped",
  "detection": "DC 14 Perception to notice weak ice",
  "mitigation": "Roped travel; probe protocol",
  "loreReference": "niflheim.md §10.2",
  "zones": ["all-niflheim"]
}
```

#### Frostbite Accumulation
```json
{
  "id": "haz-frost-bite",
  "name": "Frostbite Accumulation",
  "category": "ambient",
  "trigger": "Failed ExtremeCold save",
  "effect": "Cumulative penalty stacking",
  "dc": null,
  "damage": "-1 to all checks per stack (max -5)",
  "clearCondition": "Long rest in warm environment; healing magic",
  "mitigation": "Thermal gear; regular warming",
  "loreReference": "niflheim.md §10.2",
  "zones": ["all-niflheim"]
}
```

### 4.2 Hvergelmir's Maw Hazards

#### Ice-Debt
```json
{
  "id": "haz-ice-debt",
  "name": "Ice-Debt Accumulation",
  "category": "ambient",
  "trigger": "Each turn spent in Hvergelmir's Maw",
  "effect": "Progressive freezing status",
  "accumulation": "+1 Ice-Debt per turn",
  "thresholds": {
    "3": "[Slowed] status (-10ft movement)",
    "5": "[Frozen] status (incapacitated)",
    "7": "Preserved (death, but body intact)"
  },
  "clearing": {
    "heatSource": "-1 per turn adjacent to fire",
    "fireSpell": "-2 per casting on target",
    "exitZone": "-1 per turn outside Maw",
    "warmingKit": "-3 immediate (consumable)"
  },
  "loreReference": "niflheim.md §4.2",
  "zones": ["niflheim-hvergelmir"]
}
```

#### Avalanche
```json
{
  "id": "haz-avalanche",
  "name": "Avalanche",
  "category": "triggered",
  "trigger": "Loud noise; explosion; structural damage",
  "effect": "Massive ice/snow cascade",
  "dc": 14,
  "attribute": "AGILITY",
  "damage": "4d6 Physical; buried",
  "onFail": "Buried; restrained; suffocating",
  "areaEffect": "60ft cone from trigger point",
  "mitigation": "Silence; avoid triggering",
  "loreReference": "niflheim.md §10.2",
  "zones": ["niflheim-hvergelmir", "niflheim-permafrost"]
}
```

### 4.3 Ice Archives Hazards

#### Cognitive Echo
```json
{
  "id": "haz-cognitive-echo",
  "name": "Cognitive Echo (Einherjar Presence)",
  "category": "proximity",
  "trigger": "Within 30ft of frozen Einherjar",
  "effect": "Psychic intrusion from preserved consciousness",
  "dc": 12,
  "attribute": "WILL",
  "damage": "+1d6 Stress",
  "onFail": "Glimpse of Einherjar's final moments",
  "frequency": "Per new Einherjar encountered",
  "mitigation": "Mental shielding; avoiding eye contact",
  "loreReference": "niflheim.md §10.2",
  "zones": ["niflheim-archive"]
}
```

---

## 5. Implementation Schema

### 5.1 Hazard Definition Schema

```json
{
  "$schema": "hazard-definition.schema.json",
  "type": "object",
  "required": ["id", "name", "category", "trigger", "effect"],
  "properties": {
    "id": { "type": "string", "pattern": "^haz-[a-z-]+$" },
    "name": { "type": "string" },
    "category": {
      "type": "string",
      "enum": ["ambient", "contact", "zone-entry", "proximity", "time-based", "triggered"]
    },
    "trigger": { "type": "string" },
    "effect": { "type": "string" },
    "dc": { "type": ["number", "null"] },
    "attribute": {
      "type": "string",
      "enum": ["MIGHT", "AGILITY", "STURDINESS", "WILL"]
    },
    "damage": { "type": "string" },
    "statusOnFail": { "type": "string" },
    "frequency": { "type": "string" },
    "mitigation": { "type": "string" },
    "warningSign": { "type": ["string", "array"] },
    "loreReference": { "type": "string" },
    "zones": { "type": "array", "items": { "type": "string" } }
  }
}
```

### 5.2 Hazard Trigger Interface

```csharp
public interface IHazardTriggerService
{
    bool CheckHazardTrigger(Hazard hazard, TriggerContext context);
    HazardResult ApplyHazard(Hazard hazard, Character target);
    IEnumerable<Hazard> GetActiveHazards(BiomeZone zone);
    IEnumerable<Hazard> GetProximityHazards(Room room, Position position);
}
```

---

## 6. Cross-Reference Index

| Hazard ID | Realm | Zone(s) | Lore Section |
|-----------|-------|---------|--------------|
| `haz-rust-rot` | Midgard | All | midgard.md §9.2 |
| `haz-blight-storm` | Midgard | Scar | midgard.md §9.2 |
| `haz-leviathan-strike` | Midgard | Fjords | midgard.md §9.2 |
| `haz-steam-vent` | Muspelheim | All | muspelheim.md §10.2 |
| `haz-slag-collapse` | Muspelheim | Gjöllflow | muspelheim.md §10.2 |
| `haz-elding-storm` | Muspelheim | All | muspelheim.md §10.3 |
| `haz-ice-debt` | Niflheim | Hvergelmir | niflheim.md §4.2 |
| `haz-cognitive-echo` | Niflheim | Archive | niflheim.md §10.2 |

---

_This hazard registry provides complete specifications for all zone-specific hazards. All mechanics are derived from lore document specifications._
