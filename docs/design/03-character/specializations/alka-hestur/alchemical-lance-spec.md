---
id: SPEC-EQUIPMENT-ALCHEMICAL-LANCE-29100
title: "Alchemical Lance Specification"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Alchemical Lance Specification

**Equipment Type:** Weapon | **Category:** Specialized Melee | **Restriction:** Alka-hestur Only

---

## Overview

The Alchemical Lance is a specially modified one-handed FINESSE weapon with an internal reservoir and injection mechanism. It functions as a standard melee weapon for basic attacks, but its true power is unlocked by loading Alchemical Cartridges (payloads).

---

## Weapon Statistics

| Property | Value |
|----------|-------|
| **Weapon Type** | One-handed Melee |
| **Damage** | 1d8 Physical |
| **Attribute** | FINESSE |
| **Range** | Melee (1 tile) |
| **Weight** | Medium |
| **Hands** | 1 |
| **Special** | Payload injection system |

---

## Physical Description

### L1 (Mythic Layer)
A needle of judgment, hollow and hungry. The saint's tool for delivering verdicts in liquid form.

### L2 (Diagnostic Layer)
A precision-engineered lance with integrated chemical delivery system. The shaft contains a pressurized reservoir, while the tip houses a retractable injection needle triggered on impact.

### L3 (Technical Layer)
```
Components:
├── Shaft (reinforced alloy, hollow core)
│   ├── Reservoir Chamber (50ml capacity)
│   ├── Pressure Regulator
│   └── Payload Feed Tube
├── Grip Assembly
│   ├── Trigger Mechanism
│   ├── Payload Selector (for quick-swap)
│   └── Rack Interface Port
└── Injection Head
    ├── Impact Sensor
    ├── Retractable Needle (tungsten-tipped)
    └── Dispersion Nozzle (for AoE mode)
```

---

## Core Mechanics

### Payload Loading

**Load Payload (Free Action)**
- Select one payload from your rack
- Payload is chambered in the lance reservoir
- Only one payload can be loaded at a time
- Can be done once per turn

```
TUI Display:
┌─────────────────────────────────────────┐
│ ALCHEMICAL LANCE                        │
│ ═══════════════════════════════════════ │
│ Loaded: [EMPTY]                         │
│                                         │
│ Available Payloads:                     │
│  [1] Ignition  ████░░ (4/6)            │
│  [2] Cryo      ██░░░░ (2/6)            │
│  [3] Acidic    ███░░░ (3/6)            │
│  [4] EMP       █░░░░░ (1/6)            │
│                                         │
│ Press [1-4] to load, [ESC] to cancel   │
└─────────────────────────────────────────┘
```

```
GUI Display:
┌──────────────────────────────────────────────────────────┐
│  ╔════════════════════════════════════════════════════╗  │
│  ║  ALCHEMICAL LANCE                    [LOAD]  [X]  ║  │
│  ╠════════════════════════════════════════════════════╣  │
│  ║                                                    ║  │
│  ║    ┌─────────────────────────────────────────┐    ║  │
│  ║    │         CURRENT PAYLOAD                 │    ║  │
│  ║    │                                         │    ║  │
│  ║    │         ╔═══════════════╗               │    ║  │
│  ║    │         ║    [EMPTY]    ║               │    ║  │
│  ║    │         ╚═══════════════╝               │    ║  │
│  ║    │                                         │    ║  │
│  ║    └─────────────────────────────────────────┘    ║  │
│  ║                                                    ║  │
│  ║    ┌─── PAYLOAD RACK ────────────────────────┐    ║  │
│  ║    │                                          │    ║  │
│  ║    │  🔥 Ignition [████░░] 4   Click to load │    ║  │
│  ║    │  ❄️  Cryo     [██░░░░] 2   Click to load │    ║  │
│  ║    │  ⚡ EMP      [█░░░░░] 1   Click to load │    ║  │
│  ║    │  💧 Acidic   [███░░░] 3   Click to load │    ║  │
│  ║    │  💥 Concuss. [░░░░░░] 0   (empty)       │    ║  │
│  ║    │                                          │    ║  │
│  ║    └──────────────────────────────────────────┘    ║  │
│  ║                                                    ║  │
│  ╚════════════════════════════════════════════════════╝  │
└──────────────────────────────────────────────────────────┘
```

---

### Unload Payload (Free Action)

- Remove currently loaded payload
- Payload returns to rack (not consumed)
- Allows loading different payload

---

### Quick-Swap (Bonus Action, Rank 3 Rack Expansion)

- Change loaded payload without standard action
- Old payload returns to rack
- New payload is chambered
- Once per turn limitation

```
TUI Display:
┌─────────────────────────────────────────┐
│ QUICK-SWAP                              │
│ ═══════════════════════════════════════ │
│ Current: [IGNITION] → Swap to:         │
│                                         │
│  [1] Cryo      ██░░░░ (2/6)            │
│  [2] Acidic    ███░░░ (3/6)            │
│  [3] EMP       █░░░░░ (1/6)            │
│                                         │
│ [BONUS ACTION REQUIRED]                 │
└─────────────────────────────────────────┘
```

---

### Cocktail Loading (Rank 2+ Cocktail Mixing)

- Combine 2-3 payloads into single load
- Consumes all combined charges
- Lance chamber displays cocktail composition

```
TUI Display:
┌─────────────────────────────────────────┐
│ COCKTAIL MIXING                         │
│ ═══════════════════════════════════════ │
│ Select payloads to combine:             │
│                                         │
│  [✓] Ignition  (Fire + [Burning])      │
│  [✓] Acidic    (Phys + [Corroded])     │
│  [ ] Cryo                               │
│                                         │
│ Cocktail Preview:                       │
│  → Fire + Physical damage               │
│  → [Burning] + [Corroded]               │
│  → SYNERGY: [Melting] (Soak -4)        │
│                                         │
│ Cost: 2 charges    [CONFIRM] [CANCEL]   │
└─────────────────────────────────────────┘
```

```
GUI Display:
┌──────────────────────────────────────────────────────────┐
│  ╔════════════════════════════════════════════════════╗  │
│  ║  COCKTAIL MIXING                     [MIX]   [X]  ║  │
│  ╠════════════════════════════════════════════════════╣  │
│  ║                                                    ║  │
│  ║   ┌────────────────────────────────────────────┐  ║  │
│  ║   │  DRAG PAYLOADS TO MIXING CHAMBER          │  ║  │
│  ║   │                                            │  ║  │
│  ║   │     ┌─────┐     ┌─────┐     ┌─────┐       │  ║  │
│  ║   │     │ 🔥  │  +  │ 💧  │  =  │ ??? │       │  ║  │
│  ║   │     │ IGN │     │ ACD │     │     │       │  ║  │
│  ║   │     └─────┘     └─────┘     └─────┘       │  ║  │
│  ║   │                                            │  ║  │
│  ║   │  SYNERGY DETECTED: [MELTING]              │  ║  │
│  ║   │  → Double armor reduction (Soak -4)       │  ║  │
│  ║   │                                            │  ║  │
│  ║   └────────────────────────────────────────────┘  ║  │
│  ║                                                    ║  │
│  ║   Charges Required: 2                             ║  │
│  ║   Effects: [Burning], [Corroded], [Melting]       ║  │
│  ║                                                    ║  │
│  ║        [ CONFIRM COCKTAIL ]  [ CANCEL ]           ║  │
│  ║                                                    ║  │
│  ╚════════════════════════════════════════════════════╝  │
└──────────────────────────────────────────────────────────┘
```

---

## Combat Interface

### TUI Combat Display

```
┌─────────────────────────────────────────────────────────────────┐
│ COMBAT - Your Turn                                              │
│ ═══════════════════════════════════════════════════════════════ │
│                                                                 │
│ [Alka-hestur]  HP: 45/60  Stamina: 75/100  Stress: 2/20        │
│                                                                 │
│ ┌─ ALCHEMICAL LANCE ─────────────────────────────────────────┐ │
│ │ Loaded: [ACIDIC] 💧                                        │ │
│ │ Rack: [🔥4] [❄️2] [⚡1] [💧2] [💥0]  Total: 9/10           │ │
│ └────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ACTIONS:                                                        │
│  [P] Payload Strike      (25 Stam + 1 Charge)                  │
│  [T] Targeted Injection  (35 Stam + 1 Charge) [READY]          │
│  [A] Area Saturation     (45 Stam + 3 Charges) [CD: 2]         │
│  [L] Load Payload        (Free Action)                         │
│  [S] Quick-Swap          (Bonus Action)                        │
│  [C] Create Cocktail     (Free Action)                         │
│                                                                 │
│ Target: [Armored Sentinel]                                      │
│  → Analyzed: Vulnerable to ENERGY, Resistant to PHYSICAL       │
│  → Recommendation: Load EMP for +100% damage                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### GUI Combat Display

```
┌────────────────────────────────────────────────────────────────────────┐
│                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐  │
│  │                         BATTLEFIELD                              │  │
│  │    ┌───┬───┬───┬───┬───┐                                        │  │
│  │    │   │ E │   │ E │   │  E = Enemy                             │  │
│  │    ├───┼───┼───┼───┼───┤  P = Player (Alka-hestur)             │  │
│  │    │   │   │ T │   │   │  T = Target (highlighted)              │  │
│  │    ├───┼───┼───┼───┼───┤                                        │  │
│  │    │ A │   │ P │   │ A │  A = Ally                              │  │
│  │    └───┴───┴───┴───┴───┘                                        │  │
│  └─────────────────────────────────────────────────────────────────┘  │
│                                                                        │
│  ┌─── CHARACTER STATUS ──────────────────────────────────────────┐    │
│  │  [████████░░] HP: 45/60    [███████░░░] Stamina: 75/100      │    │
│  │  [█░░░░░░░░░] Stress: 2/20                                   │    │
│  └───────────────────────────────────────────────────────────────┘    │
│                                                                        │
│  ┌─── ALCHEMICAL LANCE ──────────────────────────────────────────┐    │
│  │                                                                │    │
│  │  LOADED: ╔═══════════════╗   RACK:                            │    │
│  │          ║   💧 ACIDIC   ║   🔥×4  ❄️×2  ⚡×1  💧×2  💥×0     │    │
│  │          ╚═══════════════╝   [9/10 charges]                   │    │
│  │                                                                │    │
│  │  [LOAD]  [SWAP]  [COCKTAIL]                                   │    │
│  │                                                                │    │
│  └────────────────────────────────────────────────────────────────┘    │
│                                                                        │
│  ┌─── TARGET INFO ───────────────────────────────────────────────┐    │
│  │  ARMORED SENTINEL                                             │    │
│  │  HP: ████████░░ 80/100    Soak: 5                            │    │
│  │                                                                │    │
│  │  ⚠️  VULNERABLE: Energy                                       │    │
│  │  🛡️  RESISTANT: Physical                                      │    │
│  │                                                                │    │
│  │  💡 RECOMMENDATION: Switch to EMP payload                     │    │
│  └────────────────────────────────────────────────────────────────┘    │
│                                                                        │
│  ┌─── ABILITIES ─────────────────────────────────────────────────┐    │
│  │                                                                │    │
│  │  [PAYLOAD STRIKE]  [TARGETED INJECTION]  [AREA SATURATION]   │    │
│  │   25 Stam + 1 chg   35 Stam + 1 chg       45 Stam + 3 chg    │    │
│  │   Ready             Ready                  CD: 2 turns        │    │
│  │                                                                │    │
│  └────────────────────────────────────────────────────────────────┘    │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

---

## Injection Mechanics

### Standard Injection (Payload Strike, Targeted Injection)

1. Lance tip contacts target
2. Impact sensor triggers
3. Needle extends into target (armor penetration for Targeted Injection)
4. Reservoir pressure releases payload
5. Chemical reaction begins in target

```
Animation Sequence (GUI):
Frame 1: Lance thrust animation
Frame 2: Contact highlight on target
Frame 3: Payload color pulse (fire=red, ice=blue, etc.)
Frame 4: Status effect icon appears above target
Frame 5: Damage number floats up
```

### Area Injection (Area Saturation)

1. Dispersion nozzle activates
2. Full reservoir contents expelled
3. Payload creates area cloud/splash
4. All enemies in area affected

```
Animation Sequence (GUI):
Frame 1: Lance held overhead
Frame 2: Nozzle opens, payload sprays
Frame 3: Area highlight (3x3 to 5x5)
Frame 4: Status effects on all targets
Frame 5: Multiple damage numbers
```

---

## Payload Visualization

### Color Coding

| Payload | Primary Color | Icon | Effect Color |
|---------|---------------|------|--------------|
| Ignition | Orange/Red | 🔥 | Flame particles |
| Cryo | Light Blue | ❄️ | Ice crystals |
| EMP | Electric Blue | ⚡ | Lightning arcs |
| Acidic | Green | 💧 | Dripping effect |
| Concussive | Gray/White | 💥 | Shockwave rings |

### Loaded Indicator

**TUI:** `Loaded: [IGNITION] 🔥`

**GUI:** Lance glows with payload color, animated particles

### Unstable Payload Indicator

**TUI:** `Loaded: [ACIDIC*] 💧 (UNSTABLE - 2 turns)`

**GUI:** Payload icon pulses/flickers, warning border

---

## Error States

### No Payload Loaded

```
TUI:
> Payload Strike
ERROR: No payload loaded! Load a payload first.
[L] Load Payload
```

```
GUI:
┌─────────────────────────────┐
│  ⚠️  NO PAYLOAD LOADED      │
│                             │
│  Load a payload to use      │
│  this ability.              │
│                             │
│  [LOAD PAYLOAD]  [CANCEL]   │
└─────────────────────────────┘
```

### Insufficient Charges

```
TUI:
> Area Saturation
ERROR: Requires 3 charges of same type.
You have: Ignition×2, Cryo×1
```

```
GUI:
┌─────────────────────────────────────┐
│  ⚠️  INSUFFICIENT CHARGES           │
│                                     │
│  Area Saturation requires           │
│  3 charges of the same type.        │
│                                     │
│  Current: 🔥×2  ❄️×1  ⚡×1           │
│                                     │
│  [OK]                               │
└─────────────────────────────────────┘
```

### Cooldown Active

```
TUI:
> Targeted Injection
COOLDOWN: 2 turns remaining

GUI:
Ability button grayed out with "CD: 2" overlay
```

---

## Accessibility Features

### TUI Accommodations
- All payload types have distinct letter codes (I, C, E, A, X)
- ASCII-only alternative icons available
- High-contrast mode for payload bars
- Screen reader compatible descriptions

### GUI Accommodations
- Colorblind-friendly palette option (patterns + colors)
- Icon-only mode (no reliance on color alone)
- Scalable UI elements
- Tooltip descriptions on hover

---

## Implementation Notes

### Data Structure

```
AlchemicalLance {
    BaseWeapon: {
        Damage: "1d8",
        Type: "Physical",
        Attribute: "FINESSE"
    },
    PayloadSystem: {
        CurrentPayload: Payload | null,
        ReservoirCapacity: 1,
        InjectionReady: boolean
    },
    Rack: {
        Capacity: 4-10 (based on Rack Expansion),
        Contents: Payload[],
        MaxSameType: 1-3 (based on Rack Expansion)
    }
}

Payload {
    Type: "Ignition" | "Cryo" | "EMP" | "Acidic" | "Concussive",
    Element: DamageType,
    Status: StatusEffect,
    Duration: number,
    Unstable: boolean,
    DegradeTimer: number | null,
    IsCocktail: boolean,
    CocktailComponents: Payload[] | null
}
```

### Event Hooks

```
OnPayloadLoad(payload): void
OnPayloadUnload(payload): void
OnPayloadConsume(payload, ability): void
OnCocktailCreate(components): Payload
OnUnstableDegrade(payload): void
OnQuickSwap(oldPayload, newPayload): void
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Alka-hestur Overview](overview.md) | Parent specialization |
| [Payload Strike](abilities/payload-strike.md) | Primary delivery ability |
| [Rack Expansion](abilities/rack-expansion.md) | Capacity system |
| [Cocktail Mixing](abilities/cocktail-mixing.md) | Combination system |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |
