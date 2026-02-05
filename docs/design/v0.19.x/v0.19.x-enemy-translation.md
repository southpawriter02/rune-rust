# v0.19.x Enemy Translation Matrix

**Version:** 1.0
**Status:** Implementation Reference
**Last Updated:** 2026-02-04
**Purpose:** Maps lore-defined enemies to implementation-ready monster definitions

---

## 1. Overview

This document translates all enemy types defined in the lore documents into implementation-ready monster definitions with IDs, faction assignments, stats, and mechanics.

### 1.1 Lore Source Documents

| Realm | Lore Document | Enemy Section |
|-------|--------------|---------------|
| Midgard | `midgard.md` | §7 |
| Muspelheim | `muspelheim.md` | §7 |
| Niflheim | `niflheim.md` | §7 |
| Vanaheim | `vanaheim.md` | §7 |
| Alfheim | `alfheim.md` | §7 |
| Asgard | `asgard.md` | §7 |
| Svartalfheim | `svartalfheim.md` | §7 |
| Jötunheim | `jotunheim.md` | §7 |
| Helheim | `helheim.md` | §7 |

---

## 2. Faction Pool Registry

### 2.1 Complete Faction List

| Faction ID | Display Name | Primary Realms | Spawn Weight Pattern |
|------------|-------------|----------------|---------------------|
| `blighted-beasts` | Blighted Beasts | Midgard, Vanaheim, Fjords | 60/30/10 |
| `humanoid` | Humanoid Factions | Midgard, Svartalfheim | 70/30/0 or 60/40/0 |
| `forlorn` | The Forlorn | Midgard (Scar), Alfheim, Asgard | 60/30/10 |
| `constructs` | Constructs | Midgard, Jötunheim | 30/60/10 |
| `fire-forged` | Fire-Forged | Muspelheim | 60/30/10 |
| `frost-touched` | Frost-Touched | Niflheim | 60/30/10 |
| `corrupted-machinery` | Corrupted Machinery | Muspelheim, Niflheim, Helheim | 30/60/10 |
| `acid-adapted` | Acid-Adapted | Helheim | 60/30/10 |
| `weaponized-flora` | Weaponized Flora | Vanaheim | 60/30/10 |
| `aetheric-anomalies` | Aetheric Anomalies | Alfheim | 60/30/10 |
| `undying` | The Undying | Asgard, Niflheim | 60/30/10 |
| `corrupted-automata` | Corrupted Automata | Svartalfheim | 60/30/10 |
| `ancient-constructs` | Ancient Constructs | Jötunheim | 60/30/10 |
| `hel-walkers` | Hel-Walkers | Helheim | 10 (rare) |
| `silent-folk` | Silent Folk | Svartalfheim | 10 (rare) |
| `unraveled` | The Unraveled | Vanaheim | 10 (rare) |
| `temporal` | Temporal Entities | Alfheim | 10 (rare) |
| `data-wraith` | Data-Wraiths | Asgard | 10 (rare) |

---

## 3. Midgard Enemies

**Lore Source:** `midgard.md` §7

### 3.1 Blighted Beasts

| Monster ID | Lore Name | Tier | HP | AC | Speed | Primary Attack |
|------------|----------|------|----|----|-------|----------------|
| `ash-vargr` | Ash-Vargr | 2 | 22 | 13 | 40ft | Bite +4 (1d8+2) |
| `ash-vargr-alpha` | Ash-Vargr Pack Leader | 3 | 38 | 14 | 40ft | Bite +5 (1d10+3) |
| `ash-vargr-rabid` | Ash-Vargr (Rabid) | 2 | 18 | 12 | 50ft | Bite +5 (1d8+3) |
| `rune-bear` | Rune-Bear | 3 | 52 | 14 | 30ft | Claw +6 (2d6+4) |
| `rune-bear-elder` | Rune-Bear (Scarred Elder) | 4 | 85 | 15 | 30ft | Claw +7 (2d8+5) |
| `jarn-hjortr` | Járn-hjortr | 2 | 28 | 14 | 50ft | Gore +4 (1d10+2) |
| `hafgufa-spawn` | Hafgufa Spawn | 3 | 45 | 15 | 30ft (swim 60ft) | Tentacle +5 (2d6+3) |

**Special Abilities:**

```json
{
  "ash-vargr": {
    "packTactics": "Advantage on attack if ally within 5ft of target",
    "resistances": { "cold": 25 },
    "vulnerabilities": ["fire"]
  },
  "ash-vargr-alpha": {
    "packTactics": true,
    "alphaHowl": "Bonus action: All Ash-Vargr within 60ft gain +1d4 damage for 1 minute",
    "resistances": { "cold": 25 },
    "vulnerabilities": ["fire"]
  },
  "ash-vargr-rabid": {
    "noMorale": "Never flees, immune to fear",
    "frenzy": "+2 damage when below half HP"
  },
  "rune-bear": {
    "aethericDischarge": "On hit: 1d4 Lightning damage (recharge 5-6)",
    "resistances": { "physical": 25 },
    "vulnerabilities": ["psychic"]
  },
  "rune-bear-elder": {
    "shockwaveRoar": "Action: All creatures within 30ft make DC 14 STURDINESS or knocked prone + 2d6 Thunder",
    "bossFlag": true
  },
  "jarn-hjortr": {
    "charge": "Move 20ft straight, Gore attack deals +2d6",
    "resistances": { "physical": 50 },
    "vulnerabilities": ["acid", "fire"]
  },
  "hafgufa-spawn": {
    "aquaticAmbush": "Advantage on attacks against surprised targets in water",
    "grapple": "On tentacle hit, target grappled (escape DC 14)",
    "resistances": { "cold": 50 },
    "vulnerabilities": ["fire", "poison"]
  }
}
```

### 3.2 Constructs

| Monster ID | Lore Name | Tier | HP | AC | Speed | Primary Attack |
|------------|----------|------|----|----|-------|----------------|
| `grit-golem` | Grit-Golem | 2 | 35 | 16 | 20ft | Slam +5 (1d10+3) |
| `ruin-mimic` | Ruin-Mimic | 2 | 28 | 15 | 15ft | Bite +4 (1d8+2) |

**Special Abilities:**

```json
{
  "grit-golem": {
    "corruptedDirective": "Attacks any creature entering 'protected' zone",
    "harvestGuard": "Cannot leave designated 10x10 area",
    "resistances": { "physical": 50 },
    "vulnerabilities": ["fire", "acid"],
    "immunities": ["poison", "psychic"]
  },
  "ruin-mimic": {
    "camouflage": "Advantage on Stealth while motionless; appears as debris",
    "adhesive": "On hit, target stuck until DC 12 MIGHT to escape",
    "resistances": { "physical": 50 },
    "vulnerabilities": ["fire"]
  }
}
```

### 3.3 The Forlorn (Scar Zone)

| Monster ID | Lore Name | Tier | HP | AC | Speed | Primary Attack |
|------------|----------|------|----|----|-------|----------------|
| `forlorn-echo` | Forlorn Echo | 2 | 24 | 12 | 30ft (fly) | Psychic Touch +4 (1d8 Psychic) |
| `genius-loci-fragment` | Genius Loci Fragment | 4 | 95 | 16 | 0ft (teleport 30ft) | Reality Tear +7 (2d10 Psychic) |

**Special Abilities:**

```json
{
  "forlorn-echo": {
    "incorporeal": "Can move through objects; resistance to non-magical physical",
    "psychicWail": "Creatures within 10ft make DC 12 WILL or +1d4 Stress",
    "resistances": { "physical": 75 },
    "vulnerabilities": ["psychic"]
  },
  "genius-loci-fragment": {
    "bossFlag": true,
    "odinProtocol": "Legendary actions (3/round): Teleport, Reality Tear, Data Spike",
    "realityWarp": "Lair action: Gravity reversal in 20ft radius",
    "phaseShift": "When below 50% HP, becomes partially incorporeal (50% miss chance)",
    "resistances": { "energy": 50 },
    "vulnerabilities": ["psychic"],
    "immunities": ["poison", "disease"]
  }
}
```

### 3.4 Humanoid

| Monster ID | Lore Name | Tier | HP | AC | Speed | Primary Attack |
|------------|----------|------|----|----|-------|----------------|
| `rust-clan-ambusher` | Rust-Clan Ambusher | 2 | 20 | 14 | 30ft | Crossbow +4 (1d8+2) |
| `skar-horde-raider` | Skar-Horde Raider | 2 | 26 | 13 | 30ft | Axe +4 (1d10+2) |
| `skar-horde-butcher` | Skar-Horde Butcher | 3 | 42 | 15 | 25ft | Great Cleaver +6 (2d6+4) |

---

## 4. Muspelheim Enemies

**Lore Source:** `muspelheim.md` §7

### 4.1 Fire-Forged

| Monster ID | Lore Name | Tier | HP | AC | Speed | Primary Attack |
|------------|----------|------|----|----|-------|----------------|
| `surtr-scion` | Surtr's Scion | 3 | 45 | 15 | 40ft | Flame Tendril +5 (2d6 Fire) |
| `slag-elemental` | Slag Elemental | 3 | 55 | 14 | 20ft | Molten Slam +6 (2d8 Fire) |
| `pilot-light` | Pilot-Light | 2 | 18 | 13 | fly 40ft | Heat Aura (1d6 Fire/turn within 5ft) |
| `slag-swimmer` | Slag-Swimmer | 2 | 30 | 14 | burrow 30ft | Eruption +4 (1d10+2 Fire) |
| `forge-guardian` | Forge Guardian | 4 | 110 | 17 | 30ft | Forge Hammer +8 (2d10+5) |
| `surtr-herald` | Surtur's Herald | 5 | 180 | 18 | 40ft | Inferno Blade +10 (3d8+6 Fire) |

**Special Abilities:**

```json
{
  "surtr-scion": {
    "mobileExtension": "Controlled by Surtr AI; shares perception",
    "assimilationDirective": "On kill, absorbs target's metal equipment",
    "immunities": ["fire"],
    "vulnerabilities": ["ice"],
    "brittleOnIce": "Ice damage applies [Brittle]: +50% physical damage"
  },
  "slag-elemental": {
    "moltenForm": "Dissolves into slag pool on death (difficult terrain)",
    "reforming": "If slag pool not cooled, reforms in 1d4 rounds",
    "immunities": ["fire"],
    "vulnerabilities": ["ice"],
    "resistances": { "physical": 50 }
  },
  "pilot-light": {
    "sentientPlasma": "Unpredictable behavior; may not attack unless provoked",
    "physicalImmune": "Physical attacks pass through",
    "immunities": ["fire", "physical"],
    "vulnerabilities": ["ice"]
  },
  "slag-swimmer": {
    "burrowAmbush": "Surprise attack from beneath ash: +2d6 damage",
    "burrowEscape": "Can burrow as bonus action when below 25% HP",
    "resistances": { "fire": 75 },
    "vulnerabilities": ["ice"]
  },
  "forge-guardian": {
    "bossFlag": true,
    "surtrControl": "Direct AI control; tactical coordination",
    "forgeSmash": "Area attack: 15ft cone, DC 15 AGILITY or 3d6 Fire + prone",
    "resistances": { "fire": 75, "physical": 50 },
    "vulnerabilities": ["ice"]
  },
  "surtr-herald": {
    "bossFlag": true,
    "multiPhase": "Phase 1: Melee, Phase 2: Inferno Aura, Phase 3: Meltdown",
    "legendaryActions": 3,
    "infernoAura": "All creatures within 20ft take 2d6 Fire at start of their turn",
    "meltdown": "Below 25% HP: Explosion deals 6d6 Fire in 30ft radius"
  }
}
```

---

## 5. Niflheim Enemies

**Lore Source:** `niflheim.md` §7

### 5.1 Frost-Touched

| Monster ID | Lore Name | Tier | HP | AC | Speed | Primary Attack |
|------------|----------|------|----|----|-------|----------------|
| `frost-revenant` | Frost Revenant | 2 | 28 | 13 | 25ft | Frost Touch +4 (1d8 Cold) |
| `ice-elemental` | Ice Elemental | 3 | 50 | 15 | 25ft | Ice Spike +5 (2d6 Cold) |
| `hvergelmir-warden` | Hvergelmir Warden | 4 | 95 | 16 | 30ft | Glacial Strike +7 (2d8+4 Cold) |

### 5.2 The Undying (Einherjar)

| Monster ID | Lore Name | Tier | HP | AC | Speed | Primary Attack |
|------------|----------|------|----|----|-------|----------------|
| `frozen-einherjar` | Frozen Einherjar | 3 | 45 | 16 | 20ft | Frost Blade +5 (1d10+3 Cold) |
| `einherjar-commander` | Einherjar Commander | 4 | 85 | 17 | 25ft | Command Blade +7 (2d6+4) |

**Special Abilities:**

```json
{
  "frost-revenant": {
    "coldAura": "Creatures starting turn within 5ft take 1d4 Cold",
    "immunities": ["cold"],
    "vulnerabilities": ["fire"]
  },
  "ice-elemental": {
    "shatterForm": "On death, explodes: 10ft radius, DC 13 AGILITY or 2d6 Cold",
    "immunities": ["cold"],
    "vulnerabilities": ["fire"],
    "resistances": { "physical": 25 }
  },
  "hvergelmir-warden": {
    "bossFlag": true,
    "iceDebtAura": "Creatures within 30ft gain +1 Ice-Debt per turn",
    "glacialPrison": "Action: Target makes DC 15 STURDINESS or encased in ice (restrained)",
    "immunities": ["cold"],
    "vulnerabilities": ["fire"]
  },
  "frozen-einherjar": {
    "preservedConsciousness": "Retains pre-Glitch combat training",
    "frozenDuty": "Cannot be turned or controlled; fights per original directive",
    "resistances": { "cold": 50, "physical": 25 },
    "vulnerabilities": ["fire"]
  },
  "einherjar-commander": {
    "bossFlag": true,
    "tacticalAI": "Bonus action: Direct 2 Einherjar within 30ft to attack",
    "shieldWall": "Reaction: Grant +2 AC to adjacent Einherjar",
    "resistances": { "cold": 50, "physical": 25 },
    "vulnerabilities": ["fire"]
  }
}
```

---

## 6. Vanaheim Enemies

**Lore Source:** `vanaheim.md` §7

### 6.1 Weaponized Flora

| Monster ID | Lore Name | Tier | HP | AC | Speed | Primary Attack |
|------------|----------|------|----|----|-------|----------------|
| `spore-shambler` | Spore Shambler | 2 | 32 | 12 | 20ft | Tendril +4 (1d8+2 + spores) |
| `golden-bloom` | Golden Bloom | 3 | 40 | 14 | 0ft | Spore Cloud (area) |
| `canopy-stalker` | Canopy Stalker | 3 | 38 | 15 | climb 40ft | Vine Lash +5 (1d10+3) |
| `root-titan` | Root-Titan | 4 | 120 | 16 | 15ft | Crushing Root +8 (2d10+5) |

---

## 7. Implementation Notes

### 7.1 Monster Definition Schema

```json
{
  "$schema": "monster-definition.schema.json",
  "id": "string (monster-id)",
  "name": "string (display name)",
  "faction": "string (faction-id)",
  "tier": "number (1-5)",
  "hp": "number",
  "ac": "number",
  "speed": "string",
  "primaryAttack": {
    "name": "string",
    "modifier": "number",
    "damage": "string",
    "damageType": "string"
  },
  "specialAbilities": ["array of ability objects"],
  "resistances": {"damageType": "percent"},
  "vulnerabilities": ["array of damage types"],
  "immunities": ["array of damage types"],
  "loreReference": "string (document path and section)"
}
```

### 7.2 Tier Guidelines

| Tier | Challenge | Typical HP | Example |
|------|-----------|-----------|---------|
| 1 | Minion | 10-20 | Basic wildlife |
| 2 | Standard | 20-40 | Ash-Vargr, Grit-Golem |
| 3 | Elite | 40-60 | Rune-Bear, Slag Elemental |
| 4 | Boss | 80-120 | Genius Loci, Forge Guardian |
| 5 | Raid Boss | 150+ | Surtur's Herald |

---

## 8. Cross-Reference Index

| Monster ID | Realm | Zone(s) | Lore Section |
|------------|-------|---------|--------------|
| `ash-vargr` | Midgard | Greatwood | midgard.md §7.1 |
| `genius-loci-fragment` | Midgard | Scar | midgard.md §7.1 |
| `surtr-scion` | Muspelheim | Gjöllflow | muspelheim.md §7.1 |
| `forge-guardian` | Muspelheim | Core | muspelheim.md §7.1 |
| `frozen-einherjar` | Niflheim | Archive | niflheim.md §7.1 |
| `hvergelmir-warden` | Niflheim | Maw | niflheim.md §7.1 |

---

_This translation matrix provides implementation-ready monster definitions for all lore-defined enemies. All stats and abilities are derived from lore document specifications._
