---
id: SPEC-UI-CHARACTER-SHEET
title: "Character Sheet UI — TUI & GUI Specification"
version: 1.0
status: draft
last-updated: 2025-12-14
related-files:
  - path: "docs/01-core/character-creation.md"
    status: Reference
  - path: "docs/01-core/saga-system.md"
    status: Reference
  - path: "docs/01-core/trauma-economy.md"
    status: Reference
  - path: "docs/08-ui/tui-layout.md"
    status: Reference
---

# Character Sheet UI — TUI & GUI Specification

> *"Know thyself — your strengths, your scars, your limits. The ruin doesn't care what you think you are."*

---

## 1. Overview

This specification defines the terminal (TUI) and graphical (GUI) interfaces for the Character Sheet, displaying all attributes, derived stats, trauma meters, progression, abilities, and equipment summary.

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-UI-CHARACTER-SHEET` |
| Category | UI System |
| Priority | High |
| Status | Draft |

### 1.2 Character Sheet Sections

| Section | Contents |
|---------|----------|
| **Identity** | Name, lineage, archetype, specialization |
| **Attributes** | MIGHT, FINESSE, WITS, WILL, STURDINESS |
| **Resources** | HP, Stamina, AP (if applicable) |
| **Derived Stats** | Speed, Accuracy, Evasion, Defense, Soak |
| **Trauma** | Stress, Corruption, Traumas |
| **Progression** | Legend, PP, Milestone |
| **Abilities** | Unlocked abilities by tier |
| **Equipment** | Currently equipped items (summary) |

### 1.3 Attribute Colors

| Attribute | Color | Symbol |
|-----------|-------|--------|
| **MIGHT** | Red | `⚔` |
| **FINESSE** | Blue | `◇` |
| **WITS** | Yellow | `★` |
| **WILL** | Purple | `☽` |
| **STURDINESS** | Green | `■` |

---

## 2. TUI Character Sheet Layout

### 2.1 Full Character Sheet

Accessed via `character` or `C`:

```
┌─────────────────────────────────────────────────────────────────────┐
│  CHARACTER SHEET                                         [C]haracter│
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  SIGRUN WOLF-DAUGHTER                                               │
│  ═══════════════════════════════════════════════════════════════    │
│  Lineage: Vargr-Kin (+1 Movement, +1 Survival)                      │
│  Archetype: Warrior                                                 │
│  Specialization: Berserkr (Heretical)                               │
│                                                                     │
├───────────────────────────────────┬─────────────────────────────────┤
│  ATTRIBUTES                       │  RESOURCES                      │
│  ───────────                      │  ─────────                      │
│  ⚔ MIGHT       5                  │  HP:      ████████████████░░░░  │
│  ◇ FINESSE     3                  │           72/90                 │
│  ★ WITS        2                  │                                 │
│  ☽ WILL        2                  │  Stamina: █████████████████░░░  │
│  ■ STURDINESS  4                  │           68/80                 │
│                                   │                                 │
│                                   │  Fury:    ███████░░░░░░░░░░░░░  │
│                                   │           35/100                │
├───────────────────────────────────┼─────────────────────────────────┤
│  DERIVED STATS                    │  TRAUMA                         │
│  ─────────────                    │  ──────                         │
│  Speed:      4                    │  Stress:     ████░░░░░░░░░░░░░  │
│  Accuracy:   +3                   │              28/100             │
│  Evasion:    12                   │                                 │
│  Soak:       6                    │  Corruption: ██░░░░░░░░░░░░░░░  │
│  Initiative: +2                   │              12/100 (Permanent) │
│                                   │                                 │
│                                   │  Traumas: Flashbacks            │
├───────────────────────────────────┴─────────────────────────────────┤
│  PROGRESSION                           Legend: 1,250 / 2,000        │
│  ───────────                           ████████████░░░░░░░░ (63%)   │
│  Milestone: 3  |  PP Available: 2  |  Next Milestone: +2 PP         │
├─────────────────────────────────────────────────────────────────────┤
│  [A] Abilities  [E] Equipment  [T] Traumas  [P] Progression  [C]lose│
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 TUI Sections Breakdown

| Section | Location | Key Info |
|---------|----------|----------|
| **Header** | Top | Name, Lineage, Archetype, Specialization |
| **Attributes** | Left-center | 5 core attributes with symbols |
| **Resources** | Right-center | HP, Stamina, specialty resource |
| **Derived Stats** | Left-bottom | Speed, Accuracy, Evasion, Soak |
| **Trauma** | Right-bottom | Stress, Corruption, active Traumas |
| **Progression** | Bottom | Legend bar, Milestone, PP |
| **Commands** | Footer | Navigation to sub-screens |

---

## 3. Attributes Display

### 3.1 Core Attributes Table

| Attribute | Description | Derived Effects |
|-----------|-------------|-----------------|
| **MIGHT** | Physical power | Melee damage, carry capacity |
| **FINESSE** | Agility, precision | Ranged accuracy, evasion |
| **WITS** | Perception, knowledge | Initiative, crafting, lore |
| **WILL** | Mental fortitude | Stress resistance, AP |
| **STURDINESS** | Endurance | Max HP, Stamina, Soak |

### 3.2 Attribute Display Format

```
⚔ MIGHT       5  (+2 from equipment)
◇ FINESSE     3
★ WITS        2
☽ WILL        2  (-1 from Fury)
■ STURDINESS  4
```

### 3.3 Attribute Modifiers

Display modifiers from:
- Equipment bonuses
- Status effects
- Specialty resources (e.g., Fury reduces WILL)
- Traumas

---

## 4. Resources Display

### 4.1 Resource Bars

```
HP:      ████████████████░░░░  72/90
         [Current Color: Green/Yellow/Red based on %]

Stamina: █████████████████░░░  68/80
         [Current Color: Blue]

Fury:    ███████░░░░░░░░░░░░░  35/100  (Berserkr only)
         [Current Color: Orange]
```

### 4.2 Resource Color Thresholds (HP)

| Percent | Color | Warning |
|---------|-------|---------|
| 100-50% | Green | — |
| 50-25% | Yellow | "Wounded" |
| 25-1% | Red | "Critical" |
| 0% | Dark Red | "Dying" |

### 4.3 Specialty Resources

| Specialization | Resource | Display |
|----------------|----------|---------|
| **Berserkr** | Fury | Orange bar (0-100) |
| **Skjaldmaer** | Block Charges | Icons: `◆◆◆◇◇` |
| **Seiðkona** | Aether Pool | Purple bar |

---

## 5. Trauma Display

### 5.1 Stress Meter

```
Stress:     ████████░░░░░░░░░░  45/100
            [Yellow → Orange → Red as increases]
            
            Threshold warning: 75 (Trauma Check at 100)
```

### 5.2 Corruption Meter

```
Corruption: ██████░░░░░░░░░░░░  32/100 (Permanent)
            [Dark red, never decreases]
            
            WARNING: At 100, you become Forlorn
```

### 5.3 Active Traumas

```
TRAUMAS:
─────────
• Flashbacks — WILL checks at sudden loud noises
• Trembling Hands — -1 FINESSE for precision tasks
```

---

## 6. TUI Abilities Sub-Screen

Accessed via `A` from Character Sheet:

```
┌─────────────────────────────────────────────────────────────────────┐
│  ABILITIES — SIGRUN WOLF-DAUGHTER                                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  WARRIOR (Base)                                                     │
│  ─────────────                                                      │
│  • Strike — Basic melee attack                                      │
│  • Defensive Stance — Reduce damage, reduce mobility                │
│  • Warrior's Vigor — Recover Stamina                                │
│                                                                     │
│  ═══════════════════════════════════════════════════════════════    │
│                                                                     │
│  BERSERKR (Specialization)                                          │
│  ─────────────────────────                                          │
│                                                                     │
│  TIER 1 (Unlocked)                                                  │
│  • Wild Swing ★★★ — High damage, self-damage                        │
│  • Primal Vigor ★★☆ — Fury → HP conversion                          │
│  • Blood-Fueled ★☆☆ — Damage taken → Fury                           │
│                                                                     │
│  TIER 2 (Unlocked)                                                  │
│  • Reckless Assault ★★☆ — Multi-attack                              │
│  • Hemorrhaging Strike ★☆☆ — Bleed application                      │
│                                                                     │
│  TIER 3 (Locked — Requires 6 PP invested)                           │
│  • Death or Glory — Unavailable                                     │
│  • Unleashed Roar — Unavailable                                     │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│  ★ = Rank (max 3)  |  PP Available: 2  |  [U] Upgrade  [C] Close    │
└─────────────────────────────────────────────────────────────────────┘
```

### 6.1 Ability Entry Format

```
• Ability Name ★★☆ — Brief description
  [Rank 2/3 | Cost: 30 Stamina | Cooldown: 2 turns]
```

### 6.2 Rank Display

| Rank | Display | Meaning |
|------|---------|---------|
| Rank 1 | `★☆☆` | Base ability unlocked |
| Rank 2 | `★★☆` | +2 PP invested |
| Rank 3 | `★★★` | +3 PP invested (max) |

---

## 7. TUI Equipment Sub-Screen

Accessed via `E` from Character Sheet:

```
┌─────────────────────────────────────────────────────────────────────┐
│  EQUIPPED — SIGRUN WOLF-DAUGHTER                                    │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  WEAPONS                           ARMOR                            │
│  ───────                           ─────                            │
│  Right Hand: Iron Axe (+8 damage)  Helmet: Leather Cap (+2 Soak)    │
│  Left Hand:  Wooden Shield (+15%)  Body:   Chainmail (+6 Soak)      │
│  Ranged:     (empty)               Gloves: Leather (+1 Soak)        │
│                                    Boots:  Iron Boots (+2 Soak)     │
│                                    Belt:   Utility Belt             │
│                                    Cloak:  (empty)                  │
│                                                                     │
│  ACCESSORIES                                                        │
│  ───────────                                                        │
│  Left Bracer:  Rune-Carved Bracer (+1 MIGHT)                        │
│  Right Bracer: (empty)                                              │
│  Left Ring:    (empty)                                              │
│  Right Ring:   Ring of Vigor (+5 Max HP)                            │
│  Jewel:        (empty)                                              │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│  Total Soak: +11  |  Total Bonuses: +1 MIGHT, +5 HP                 │
├─────────────────────────────────────────────────────────────────────┤
│  [I] Full Inventory  [C] Close                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 8. TUI Progression Sub-Screen

Accessed via `P` from Character Sheet:

```
┌─────────────────────────────────────────────────────────────────────┐
│  SAGA PROGRESSION — SIGRUN WOLF-DAUGHTER                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  LEGEND                                                             │
│  ──────                                                             │
│  Current: 1,250                                                     │
│  To Next Milestone: 2,000                                           │
│                                                                     │
│  Progress: ████████████░░░░░░░░ 63%                                 │
│                                                                     │
│  ═══════════════════════════════════════════════════════════════    │
│                                                                     │
│  MILESTONES ACHIEVED                                                │
│  ───────────────────                                                │
│  ★ Milestone 1 (500 Legend)   — +2 PP earned                        │
│  ★ Milestone 2 (1,000 Legend) — +2 PP earned                        │
│  ★ Milestone 3 (2,000 Legend) — +2 PP earned                        │
│  ☐ Milestone 4 (4,000 Legend) — Next reward: +2 PP                  │
│                                                                     │
│  ═══════════════════════════════════════════════════════════════    │
│                                                                     │
│  PROGRESSION POINTS                                                 │
│  ──────────────────                                                 │
│  Total Earned:  6 PP                                                │
│  Total Spent:   4 PP                                                │
│  Available:     2 PP                                                │
│                                                                     │
│  [S] Spend PP  [C] Close                                            │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 9. TUI Traumas Sub-Screen

Accessed via `T` from Character Sheet:

```
┌─────────────────────────────────────────────────────────────────────┐
│  TRAUMAS — SIGRUN WOLF-DAUGHTER                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ACTIVE TRAUMAS (2/4 slots)                                         │
│  ══════════════════════════                                         │
│                                                                     │
│  [1] FLASHBACKS                                                     │
│      Trigger: Sudden loud noises, explosions                        │
│      Effect: Must pass WILL check or lose action                    │
│      Acquired: The Iron Crypts (Day 12)                             │
│                                                                     │
│  [2] TREMBLING HANDS                                                │
│      Trigger: Precision tasks (lockpicking, surgery)                │
│      Effect: -1 FINESSE for affected tasks                          │
│      Acquired: Rust Lord encounter (Day 15)                         │
│                                                                     │
│  ═══════════════════════════════════════════════════════════════    │
│                                                                     │
│  STRESS RECOVERY                                                    │
│  ────────────────                                                   │
│  Current Stress: 28/100                                             │
│  Recovery Rate: -5 per rest in safe zone                            │
│  Next Trauma Check: At 100 Stress                                   │
│                                                                     │
│  > [!WARNING]                                                       │
│  > At 4 Traumas, next Trauma Check is LETHAL.                       │
│                                                                     │
│  [C] Close                                                          │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 10. GUI Character Sheet Panel

### 10.1 Layout

```
┌───────────────────────────────────────────────────────────────────────┐
│  CHARACTER SHEET                                              [X]    │
├───────────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  SIGRUN WOLF-DAUGHTER                               │
│  │             │  Vargr-Kin • Warrior • Berserkr                     │
│  │  [Portrait] │                                                      │
│  │             │  ═══════════════════════════════════════════        │
│  └─────────────┘                                                      │
├───────────────────────────────────────────────────────────────────────┤
│  [Attributes] [Resources] [Abilities] [Equipment] [Progression]      │
├───────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌─────────────────────────────┐  ┌─────────────────────────────┐    │
│  │  ATTRIBUTES                 │  │  RESOURCES                  │    │
│  │  ─────────────              │  │  ─────────────              │    │
│  │  ⚔ MIGHT       5            │  │  HP      ████████████░░     │    │
│  │  ◇ FINESSE     3            │  │          72/90              │    │
│  │  ★ WITS        2            │  │                             │    │
│  │  ☽ WILL        2            │  │  Stamina ██████████████░    │    │
│  │  ■ STURDINESS  4            │  │          68/80              │    │
│  │                             │  │                             │    │
│  │  ─────────────────────────  │  │  Fury    ██████░░░░░░░░░    │    │
│  │  Speed: 4  Accuracy: +3     │  │          35/100             │    │
│  │  Evasion: 12  Soak: 6       │  │                             │    │
│  └─────────────────────────────┘  └─────────────────────────────┘    │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │  TRAUMA                                                        │  │
│  │  ─────                                                         │  │
│  │  Stress:     ████████░░░░░░░░  45/100                          │  │
│  │  Corruption: ██████░░░░░░░░░░  32/100 (Permanent)              │  │
│  │  Traumas:    Flashbacks, Trembling Hands                       │  │
│  └────────────────────────────────────────────────────────────────┘  │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │  LEGEND: 1,250 / 2,000  ████████████░░░░░░░░ (63%)             │  │
│  │  Milestone: 3  |  PP Available: 2                              │  │
│  └────────────────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────────────┘
```

### 10.2 GUI Tab System

| Tab | Contents |
|-----|----------|
| **Attributes** | Core stats, derived stats |
| **Resources** | HP, Stamina, specialty resource, stress, corruption |
| **Abilities** | Full ability tree with upgrade interface |
| **Equipment** | Visual equipment slots with drag/drop |
| **Progression** | Legend bar, milestones, PP spending |

### 10.3 Interactive Elements

| Element | Click | Hover |
|---------|-------|-------|
| Attribute | — | Show formula/modifiers |
| Resource bar | — | Show regeneration rate |
| Ability | Open upgrade dialog | Show full description |
| Trauma | Open trauma details | Show trigger/effect |
| Legend bar | — | Show milestone rewards |

---

## 11. CharacterSheetViewModel

### 11.1 Interface

```csharp
public interface ICharacterSheetViewModel
{
    // Identity
    string CharacterName { get; }
    string LineageName { get; }
    string ArchetypeName { get; }
    string SpecializationName { get; }
    
    // Attributes
    int Might { get; }
    int Finesse { get; }
    int Wits { get; }
    int Will { get; }
    int Sturdiness { get; }
    
    // Attribute Modifiers
    IReadOnlyList<AttributeModifier> AttributeModifiers { get; }
    
    // Resources
    int CurrentHp { get; }
    int MaxHp { get; }
    int CurrentStamina { get; }
    int MaxStamina { get; }
    int? CurrentSpecialtyResource { get; }
    int? MaxSpecialtyResource { get; }
    string? SpecialtyResourceName { get; }
    
    // Derived Stats
    int Speed { get; }
    int Accuracy { get; }
    int Evasion { get; }
    int Soak { get; }
    int Initiative { get; }
    
    // Trauma
    int CurrentStress { get; }
    int MaxStress { get; }
    int Corruption { get; }
    int MaxCorruption { get; }
    IReadOnlyList<TraumaViewModel> ActiveTraumas { get; }
    
    // Progression
    int LegendPoints { get; }
    int LegendToNextMilestone { get; }
    int CurrentMilestone { get; }
    int AvailablePp { get; }
    int TotalPpEarned { get; }
    int TotalPpSpent { get; }
    
    // Abilities
    IReadOnlyList<AbilityViewModel> BaseAbilities { get; }
    IReadOnlyList<AbilityViewModel> SpecializationAbilities { get; }
    
    // Equipment Summary
    IReadOnlyDictionary<EquipmentSlot, ItemViewModel?> EquippedItems { get; }
    int TotalSoak { get; }
    
    // Commands
    ICommand OpenAbilitiesCommand { get; }
    ICommand OpenEquipmentCommand { get; }
    ICommand OpenProgressionCommand { get; }
    ICommand OpenTraumasCommand { get; }
    ICommand CloseCommand { get; }
}

public record AttributeModifier(string Source, string Attribute, int Value);
public record TraumaViewModel(string Name, string Trigger, string Effect, string AcquiredAt);
public record AbilityViewModel(string Name, int Rank, int MaxRank, string Description, int Tier);
```

---

## 12. Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `C` | Open/close character sheet |
| `A` | View abilities (from character sheet) |
| `E` | View equipment (from character sheet) |
| `T` | View traumas (from character sheet) |
| `P` | View progression (from character sheet) |
| `Esc` | Close current view |

---

## 13. Configuration

| Setting | Default | Options |
|---------|---------|---------|
| `ShowAttributeModifiers` | true | true/false |
| `ShowDerivedFormulas` | false | true/false |
| `ResourceBarStyle` | `bar` | bar/numeric/both |
| `TraumaWarnings` | true | true/false |

---

## 14. Implementation Status

| Component | TUI Status | GUI Status |
|-----------|------------|------------|
| Main character sheet | ❌ Planned | ❌ Planned |
| Attributes display | ❌ Planned | ❌ Planned |
| Resources display | ❌ Planned | ❌ Planned |
| Derived stats | ❌ Planned | ❌ Planned |
| Trauma display | ❌ Planned | ❌ Planned |
| Progression display | ❌ Planned | ❌ Planned |
| Abilities sub-screen | ❌ Planned | ❌ Planned |
| Equipment sub-screen | ❌ Planned | ❌ Planned |
| Traumas sub-screen | ❌ Planned | ❌ Planned |
| CharacterSheetViewModel | ❌ Planned | ❌ Planned |

---

## 15. Phased Implementation Guide

### Phase 1: Core Display
- [ ] Main character sheet layout
- [ ] Identity section (name, lineage, archetype, specialization)
- [ ] Attributes with symbols and values
- [ ] Resource bars (HP, Stamina)

### Phase 2: Derived & Trauma
- [ ] Derived stats calculation display
- [ ] Stress and Corruption meters
- [ ] Active traumas list

### Phase 3: Sub-Screens
- [ ] Abilities sub-screen with tiers and ranks
- [ ] Equipment summary sub-screen
- [ ] Progression sub-screen with milestones

### Phase 4: GUI
- [ ] CharacterSheetViewModel
- [ ] Tab-based navigation
- [ ] Interactive elements (hover, click)
- [ ] Character portrait support

---

## 16. Testing Requirements

### 16.1 TUI Tests
- [ ] All attributes display correctly
- [ ] Resource bars update in real-time
- [ ] Trauma list reflects actual traumas
- [ ] Progression shows correct milestone

### 16.2 GUI Tests
- [ ] Tab switching works
- [ ] Hover tooltips appear
- [ ] Equipment summary links to inventory

### 16.3 Integration Tests
- [ ] Attribute modifiers from equipment reflected
- [ ] Stress changes update display
- [ ] PP spending updates available PP

---

## 17. Related Specifications

| Spec | Relationship |
|------|--------------|
| [character-creation.md](../01-core/character-creation.md) | Starting values |
| [saga-system.md](../01-core/saga-system.md) | Legend, PP, Milestones |
| [trauma-economy.md](../01-core/trauma-economy.md) | Stress, Corruption, Traumas |
| [inventory-ui.md](inventory-ui.md) | Equipment slot alignment |

---

## 18. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |
