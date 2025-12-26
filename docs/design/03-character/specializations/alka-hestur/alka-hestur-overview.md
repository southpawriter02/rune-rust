---
id: SPEC-ALKA-HESTUR-29001
title: "Alka-hestur"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Alka-hestur

**Archetype:** Skirmisher | **Path:** Coherent | **Role:** Combat Alchemist / Payload Specialist

> *"The Coherent Solution"*

---

## Identity

| Property | Value |
|----------|-------|
| **Name** | Alka-hestur |
| **Translation** | Old Norse: "Alchemy-Horse" (vehicle for chemical solutions) |
| **Archetype** | Skirmisher |
| **Path Type** | Coherent |
| **Role** | Combat Alchemist / Payload Specialist |
| **Primary Attribute** | FINESSE |
| **Secondary Attribute** | WITS |
| **Resource** | Stamina + Payload Charges |
| **Trauma Risk** | Low (only Volatile Synthesis generates Stress) |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| **PP Cost** | 3 PP |
| **Minimum Legend** | 5 |
| **Prerequisites** | None |
| **Exclusive With** | None |

---

## Design Philosophy

### Tagline
*"Every enemy has a solution—you carry all of them."*

### Core Fantasy
You are the pragmatic combat alchemist who answers paradox with chemistry delivered by lance. In a world defined by chaotic, glitching paradoxes of the Runic Blight, you reject heretical corruption in favor of preparation, analysis, and application. You see enemies not as monsters to be feared, but as systems to be analyzed and problems to be solved with the correct chemical payload.

### Mechanical Identity
1. **Alchemical Lance**: Specialized FINESSE weapon with internal reservoir and injection mechanism
2. **Payload System**: Consumable cartridges (Ignition, Cryo, EMP, Acidic, Concussive) loaded into lance
3. **Weakness Exploitation**: Identify vulnerabilities, then apply targeted chemical counters
4. **Preparation Economy**: Craft payloads during downtime, sustain through long expeditions

### Gameplay Loop
```
1. READ    — Identify target weakness or tactical need (Alchemical Analysis)
2. LOAD    — Select appropriate payload from rack (Free Action)
3. DELIVER — Strike with alchemical lance to inject payload (Payload Strike)
4. ASSESS  — Evaluate effect and adjust for next engagement
```

### Gameplay Feel
Methodical, analytical, satisfying. Combat becomes a puzzle of matching solutions to problems. The moment when you identify a weakness, load the perfect payload, and watch the enemy crumble to exactly the right chemical reaction—that's the Alka-hestur fantasy.

---

## Rank Progression

### Tree-Based Advancement
Abilities unlock through **prerequisite chains**, not PP purchase:

| Tier | PP Cost | Starting Rank | Rank Upgrades |
|------|---------|---------------|---------------|
| Tier 1 | 3 PP each | Rank 1 | → Rank 2 → Rank 3 |
| Tier 2 | 4 PP each | Rank 2 | → Rank 3 |
| Tier 3 | 5 PP each | No ranks | Full power when unlocked |
| Capstone | 6 PP | No ranks | Upgrades all Tier 1 & 2 to Rank 3 |

### Rank Unlock Requirements

| Rank | Requirement |
|------|-------------|
| Rank 2 | Unlock any Tier 2 ability in this specialization |
| Rank 3 | Unlock the Capstone ability |

---

## Ability Tree

```
                    [ALKA-HESTUR PAYLOAD MASTERY]
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ ALCHEMICAL      │ │ PAYLOAD         │ │ FIELD           │
│ ANALYSIS I      │ │ STRIKE          │ │ PREPARATION     │
│ [Tier 1]        │ │ [Tier 1]        │ │ [Tier 1]        │
│ Passive         │ │ Active          │ │ Active          │
│ ID weakness     │ │ Lance + payload │ │ Craft payloads  │
│ +1d10→+3d10     │ │ 2d8→4d8         │ │ 4→8 capacity    │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ RACK            │ │ TARGETED        │ │ COCKTAIL        │
│ EXPANSION       │ │ INJECTION       │ │ MIXING          │
│ [Tier 2]        │ │ [Tier 2]        │ │ [Tier 2]        │
│ Passive         │ │ Active          │ │ Passive         │
│ +2→+6 capacity  │ │ Ignore Soak     │ │ Combine 2→3     │
│ Quick-swap      │ │ 3d8→5d8         │ │ payloads        │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │ AREA          VOLATILE      │
              │ SATURATION    SYNTHESIS     │
              │ [Tier 3]      [Tier 3]      │
              │ AoE payload   Create        │
              │ 4d8→6d8       payloads      │
              └──────────────┬──────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │       MASTER ALCHEMIST      │
              │          [Capstone]         │
              │  Universal Reagent (Passive)│
              │  Perfect Solution (Active)  │
              │       Once Per Combat       │
              └─────────────────────────────┘
```

### Ability Index

| ID | Name | Tier | Type | PP | Summary |
|----|------|------|------|----|---------
| 29010 | [Alchemical Analysis I](abilities/alchemical-analysis-i.md) | 1 | Passive | 3 | +1d10→+3d10 WITS to identify weaknesses |
| 29011 | [Payload Strike](abilities/payload-strike.md) | 1 | Active | 3 | 2d8→4d8 + payload effect delivery |
| 29012 | [Field Preparation](abilities/field-preparation.md) | 1 | Active | 3 | Craft 4→8 payloads during rest |
| 29013 | [Rack Expansion](abilities/rack-expansion.md) | 2 | Passive | 4 | +2→+6 payload capacity |
| 29014 | [Targeted Injection](abilities/targeted-injection.md) | 2 | Active | 4 | 3d8→5d8 ignores Soak, +potency |
| 29015 | [Cocktail Mixing](abilities/cocktail-mixing.md) | 2 | Passive | 4 | Combine 2→3 payloads into one |
| 29016 | [Area Saturation](abilities/area-saturation.md) | 3 | Active | 5 | AoE payload (3×3→5×5) |
| 29017 | [Volatile Synthesis](abilities/volatile-synthesis.md) | 3 | Active | 5 | Create payloads mid-combat (+Stress) |
| 29018 | [Master Alchemist](abilities/master-alchemist.md) | 4 | Mixed | 6 | Universal Reagent + Perfect Solution |

---

## Core Mechanics

### The Alchemical Lance

A specially modified one-handed FINESSE weapon with internal reservoir and injection mechanism:

| Property | Value |
|----------|-------|
| Weapon Type | One-handed Melee |
| Attribute | FINESSE |
| Base Damage | 1d8 Physical |
| Special | Payload injection system |

See [Alchemical Lance Specification](alchemical-lance-spec.md) for full details.

### Payload System

Payloads are consumable cartridges loaded into the lance:

| Payload Type | Damage Type | Status Effect | Optimal Target |
|--------------|-------------|---------------|----------------|
| Ignition | Fire | [Burning] DoT | Organic, cold-resistant |
| Cryo | Ice | [Slowed] | Fast enemies, fire-resistant |
| EMP | Energy | [System Shock] | Mechanical/Undying |
| Acidic | Physical | [Corroded] (Soak reduction) | High-armor targets |
| Concussive | Physical | [Staggered] | Casters, chargers |

### Rack Capacity

| Progression | Capacity | Notes |
|-------------|----------|-------|
| Base | 4 slots | Starting capacity |
| Rack Expansion R2 | 6 slots | +2 base |
| Rack Expansion R3 | 8 slots | +4 base |
| Rack Expansion R3 | 10 slots | +6 base |

### Payload Loading

- **Load Payload**: Free Action (once per turn)
- **Quick-Swap**: Bonus Action (Rack Expansion R3)
- **Unload**: Free Action

---

## Payload Effect Reference

### Ignition Payload
```
Damage: +1d6 Fire
Status: [Burning] - 1d4 Fire damage per turn (3 turns)
Counter: Extinguish (water, smothering)
```

### Cryo Payload
```
Damage: +1d6 Ice
Status: [Slowed] - Movement halved, -1d10 to Initiative (2 turns)
Counter: Heat source removes early
```

### EMP Payload
```
Damage: +1d6 Energy
Status: [System Shock] - Mechanical enemies skip next turn
Counter: Shielded electronics immune
```

### Acidic Payload
```
Damage: +1d6 Physical (corrosive)
Status: [Corroded] - Soak reduced by 2 (3 turns)
Counter: Acid-resistant materials
```

### Concussive Payload
```
Damage: +1d6 Physical (impact)
Status: [Staggered] - Next action has -2d10 penalty
Counter: Braced/anchored targets resist
```

---

## Situational Power Profile

### Optimal Conditions
- Enemies with known vulnerabilities
- Preparation time before combat
- Heavily armored targets (Acidic)
- Mixed enemy compositions (payload variety)
- Boss fights with multiple phases

### Weakness Conditions
- Surprise encounters (no time to load)
- Resource denial/item destruction
- Enemies immune to all elemental damage
- Extended combat without rest (payload depletion)
- Anti-magic fields (affects some payloads)

---

## Party Synergies

### Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Jötun-Reader** | Their analysis + your payloads = perfect targeting |
| **Hlekkr-Master** | Displace enemies into Area Saturation zones |
| **Skjaldmær** | Hold the line while you deliver precision strikes |
| **Burst Dealers** | Perfect Solution enables massive burst windows |
| **Ruin-Stalker** | Scout ahead, report enemy types for payload prep |

### Negative Synergies

| Partner | Conflict |
|---------|----------|
| **Aggressive rushers** | May engage before you can analyze/load |
| **Chaotic tacticians** | Unpredictable movement disrupts positioning |

---

## Balance Data

### Power Curve

| Level Range | Power Level | Notes |
|-------------|-------------|-------|
| 1-5 | Medium | Basic payload delivery |
| 6-10 | High | Rack Expansion + Targeted Injection |
| 11-15 | Very High | Area Saturation + Cocktails |
| 16+ | Extreme | Perfect Solution deletes priority targets |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Damage | 8/10 | Excellent single-target, good AoE |
| Survivability | 5/10 | Skirmisher baseline, no self-healing |
| Support | 6/10 | Weakness identification helps party |
| Control | 7/10 | Status effects via payloads |
| Utility | 8/10 | Crafting, analysis, adaptability |

---

## Voice Guidance

### Tone Profile
- Analytical, methodical, confident
- Speaks in terms of problems and solutions
- Calm certainty in the face of horror

### Example Quotes (NPC Flavor Text)
- *"Let me see what breaks you."*
- *"Acidic. Definitely acidic. That armor won't help."*
- *"The bench is where battles are won. Combat is just... delivery."*
- *"Every monster is a problem. Every problem has a solution. I carry solutions."*

---

## Layered Interpretation (Lexicon)

| Layer | Interpretation |
|-------|----------------|
| L1 (Mythic) | Phial-saint with a solving spear; the blow that knows what breaks you |
| L2 (Diagnostic) | Technician of elemental payloads; reads weaknesses and selects cartridges to unmake them |
| L3 (Technical) | Chemical kinetics, materials transfer, and injector timing coupled to positional delivery |

---

## Phased Implementation Guide

### Phase 1: Foundation
- [ ] Implement Payload Charge resource tracking
- [ ] Create Alchemical Lance weapon type
- [ ] Implement payload loading/unloading system
- [ ] Add 5 base payload types with effects

### Phase 2: Core Abilities
- [ ] Implement Alchemical Analysis (WITS check + weakness reveal)
- [ ] Implement Payload Strike (base delivery mechanism)
- [ ] Implement Field Preparation (downtime crafting)

### Phase 3: Advanced Systems
- [ ] Implement Rack Expansion (capacity increase)
- [ ] Implement Targeted Injection (armor penetration)
- [ ] Implement Cocktail Mixing (payload combination)

### Phase 4: Mastery
- [ ] Implement Area Saturation (AoE conversion)
- [ ] Implement Volatile Synthesis (combat crafting + Stress)
- [ ] Implement Perfect Solution (capstone execution)

### Phase 5: Polish
- [ ] Add payload visual indicators (TUI/GUI)
- [ ] Implement rack UI display
- [ ] Test payload combination edge cases
- [ ] Balance payload damage values

---

## Testing Requirements

### Unit Tests
- Payload charge consumption/restoration
- Lance damage calculation with payloads
- Rack capacity limits
- Cocktail combination validation

### Integration Tests
- Full combat with payload rotation
- Weakness identification → payload selection flow
- Area Saturation targeting
- Perfect Solution auto-hit mechanics

### Manual QA
- Verify payload selection feels responsive
- Test cocktail combinations feel impactful
- Confirm Perfect Solution feels capstone-worthy

---

## Logging Requirements

### Event Templates

```
OnPayloadLoad:
  "[Character] loads {PayloadType} into alchemical lance"

OnPayloadStrike:
  "Payload Strike: {Damage} damage + [{Status}] applied!"

OnWeaknessIdentified:
  "Alchemical Analysis: {Target} vulnerable to {Element}!"

OnPerfectSolution:
  "PERFECT SOLUTION! Custom payload deals {Damage} damage!"
  "{Target} suffers [Perfect Exploitation]!"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skirmisher Archetype](../../archetypes/skirmisher.md) | Parent archetype |
| [Stamina Resource](../../../01-core/resources/stamina.md) | Primary resource |
| [Alchemical Lance Specification](alchemical-lance-spec.md) | Weapon details |
| [Alchemy Crafting](../../../04-systems/crafting/alchemy.md) | Crafting integration |

---

## Cross-References

| Specialization | Relationship |
|----------------|--------------|
| Jötun-Reader | Weakness identification synergy |
| Iron-Bane | Anti-Undying (EMP payload overlap) |
| Rust-Witch | Armor-shredding (Acidic payload overlap) |
| Ruin-Stalker | Survivalist resource management |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard creation |
