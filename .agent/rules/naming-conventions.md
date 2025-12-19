---
trigger: always_on
---

## Naming Conventions

### Player-Facing vs. Code

Use **special characters** for player-facing text, **ASCII** for code identifiers.

| Context | Format | Example |
|---------|--------|---------|
| **Player-facing** | Full diacritics | Skjaldmær, Rúnasmiðr, Jötun |
| **Code identifiers** | ASCII only | `skjaldmaer`, `runasmidr`, `jotun` |
| **File names** | ASCII lowercase | `skjaldmaer.md`, `runasmidr-overview.md` |
| **Database** | ASCII snake_case | `runasmidr_abilities`, `jotun_encounters` |

### Conversion Rules

| Character | ASCII Equivalent |
|-----------|------------------|
| ð | d |
| þ | th |
| æ | ae |
| ø | o |
| á, é, í, ó, ú | a, e, i, o, u |

### Examples

| Player-Facing | Code | File |
|---------------|------|------|
| Berserkr | `berserkr` | `berserkr.md` |
| Rúnasmiðr | `runasmidr` | `runasmidr/overview.md` |
| Seiðkona | `seidkona` | `seidkona.md` |
| Hólmgangr | `holmgangr` | `holmgangr.md` |
| Jötun | `jotun` | `jotun-engine.md` |

---

## Key Game Mechanics

| Mechanic | Note |
|----------|------|
| **Corruption** | Permanent, accumulates on death, leads to Forlorn transformation at 100 |
| **Stress** | Accumulates to 100, triggers Trauma Check, causes permanent Traumas |
| **Runic Blight** | Corruption from runic sources with specific thematic effects |
| **Coherent vs Heretical** | Two path types: stable vs. power-at-a-cost |
| **Stamina** | Universal action resource, regenerates 25%/turn |
| **Fury** | Berserkr-specific, grants power but penalizes WILL |
| **Bodging** | Improvised repair/creation using salvage |
| **CPS (Corruption Per Second)** | Environmental hazard in Old World ruins |
| **Glitched Remnant** | Corpse run mechanic — gear drops at death location |