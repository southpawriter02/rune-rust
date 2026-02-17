# Heart of Iron — Tier 3 Ability

Type: Ability
Priority: Nice-to-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-IRONBANE-HEARTOFIRON-v5.0
Mechanical Role: Tank/Durability
Parent item: Iron-Bane (Zealous Purifier) — Specialization Specification v5.0 (Iron-Bane%20(Zealous%20Purifier)%20%E2%80%94%20Specialization%20Spec%20c2718eab17e04443af19f9da976f4ad3.md)
Proof-of-Concept Flag: No
Resource System: Vengeance
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Core Identity

| Property | Value |
| --- | --- |
| **Ability Name** | Heart of Iron |
| **Specialization** | Iron-Bane (Zealous Purifier) |
| **Tier** | 3 (Extermination Mastery) |
| **Type** | Passive (Defensive Aura) |
| **PP Cost** | 5 PP |
| **Resource Cost** | None (Passive) |
| **Effect** | Fear immunity + ally aura |
| **Rank** | R3 only (no progression) |

---

## Description

The Iron-Bane has faced the worst the Undying can offer. They have watched Sentinels execute entire villages. They have heard the psychic screams of the tortured converted. They have stood before the Iron Hulks and *refused to break*.

Their heart is not merely brave—it is **forged**. Forged in the furnace of a hundred horrors, tempered by unwavering faith, hardened into something the Undying's terror cannot crack. When the machines broadcast their fear, the Iron-Bane stands unmoved, a beacon of defiance.

> *"I have seen your worst. I am still standing. Your fear means nothing to me."*
> 

---

## Mechanics

### Effect Summary (Rank 3 Only)

| Property | Value |
| --- | --- |
| **Fear Immunity** | Immune to [Fear] from Undying/Mechanical sources |
| **Aura of Courage** | Adjacent allies gain +2 dice to resist [Fear] and [Disoriented] |
| **Vengeance on Ally Fear** | +20 Vengeance when ally within 10m becomes [Feared] |
| **Aura Range** | Adjacent tiles (1 tile / ~2m) |

### Formula

```
FearImmunity:
  If Source.Faction ∈ {Undying, Mechanical}:
    [Fear] automatically fails (no roll required)
  Else:
    Normal WILL check applies

AuraOfCourage:
  For each Ally in AdjacentTiles:
    BonusDice += 2 for Resolve checks vs [Fear] or [Disoriented]

VengeanceOnAllyFear:
  If Ally.Distance <= 10m AND Ally.Status == [Feared]:
    GainVengeance(20)
```

### Resolution Pipeline

**Fear Immunity:**

1. **Fear Attempt:** Enemy attempts to inflict [Fear]
2. **Source Check:** Is source Undying/Mechanical?
3. **Immunity Applied:** If yes, [Fear] automatically fails (bypasses roll)
4. **Organic Sources:** Normal WILL check applies vs non-Undying

**Aura of Courage:**

1. **Ally Targeted:** Ally in adjacent tile faces [Fear] or [Disoriented]
2. **Aura Active:** Iron-Bane is conscious and adjacent
3. **Bonus Applied:** +2 dice added to ally's Resolve pool

**Vengeance Generation:**

1. **Ally Feared:** Any ally within 10m gains [Feared] status
2. **Trigger:** Iron-Bane gains +20 Vengeance immediately

---

## Worked Examples

### Example 1: Personal Fear Immunity

**Situation:** Iron Sentinel broadcasts [Fear] aura (DC 16)

```
Target: Grizelda (Heart of Iron)
Source: Iron Sentinel (Undying faction)

Resolution:
  Source Check: Undying ✓
  Heart of Iron: IMMUNE
  Result: [Fear] automatically fails — no roll needed

Combat Log: "The Sentinel's wail of despair washes over Grizelda...
            but her Heart of Iron is unmoved! (Immune to Fear)"
```

### Example 2: Aura Protecting Adjacent Ally

**Situation:** Forlorn targets Grizelda's adjacent ally with [Fear]

```
Target: Erik (WILL 2, adjacent to Grizelda)
Attacker: Forlorn
Effect: [Fear] DC 14

Erik's Resolve Pool:
  Base WILL: 2
  Heart of Iron Aura: +2 dice
  Total Pool: 4d10 vs DC 14

Roll: [9, 7, 8, 5] → 3 successes
Result: SUCCESS — Erik resists (would have failed without aura)

Combat Log: "Grizelda's courageous presence bolsters Erik's resolve!"
```

### Example 3: Vengeance from Ally's Suffering

**Situation:** Distant ally (8m away) becomes [Feared]

```
Event: Skald ally fails Fear resist, becomes [Feared]
Distance: 8m (within 10m range)
Grizelda's Vengeance: 35

Trigger: Heart of Iron Vengeance clause
Result: +20 Vengeance → Grizelda now at 55 Vengeance

Combat Log: "Grizelda's fury grows as she watches her ally falter!
            She gains 20 Vengeance!"

Note: Iron-Bane's righteous anger at seeing allies suffer fuels 
      her crusade — defensive failure becomes offensive fuel
```

---

## Failure Modes

| Failure Type | Result |
| --- | --- |
| **Organic [Fear] Source** | Immunity does NOT apply; normal WILL check required |
| **Ally Out of Range** | Aura only affects adjacent tiles; 2+ tiles away = no bonus |
| **Iron-Bane Unconscious** | Aura deactivates while incapacitated |
| **[Disoriented] Self** | Not immune to [Disoriented] personally (only [Fear]) |
| **No Allies Feared** | Vengeance clause only triggers on ally failure |

---

## Tactical Applications

1. **Psychic Anchor:** Immune to Undying fear; hold the line where others break
2. **Formation Play:** Position allies adjacent for +2 dice Fear resistance
3. **Vengeance Engine:** Ally failures fuel your offensive capabilities
4. **Aggro Magnet:** Enemies can't break your resolve — draw psychic attacks
5. **Anti-Forlorn:** Hard counter to fear-heavy biomes and encounters

---

## Integration Notes

### Synergies

- **Indomitable Will I:** Stacking psychic defenses create impenetrable mental fortress
- **Righteous Retribution:** Fear attempts still trigger reflection (enemy tries, immune, reflected)
- **Party Formation:** Skald + Iron-Bane shield wall = party-wide Fear resistance
- **Vengeance Spenders:** Ally suffering → Vengeance → Corrosive Strike/Annihilate

### Anti-Synergies

- **Non-Undying Campaigns:** Immunity only vs Undying/Mechanical fear sources
- **Solo Play:** No allies = no aura benefit, no Vengeance from ally fear
- **Non-Psychic Enemies:** Physical-only threats bypass this entirely
- **Scattered Party:** Aura requires adjacency; spread formation = limited coverage

### Design Note: Partial vs Full Immunity

Heart of Iron grants immunity specifically to **Undying/Mechanical** fear sources. This preserves challenge from:

- Organic horrors (Blighted Beasts with fear abilities)
- Human psychological warfare (Thul rhetoric)
- Magical fear effects (non-machine sources)

The Iron-Bane's conviction is specifically tuned to resist machine terror — their crusade has hardened them against that specific threat.

### Combat Log Examples

```
> The Forlorn's wail of despair washes over Grizelda...
> Her Heart of Iron is unmoved! (Immune to Fear)
```

```
> The Forlorn targets Erik with its terrifying presence...
> Grizelda's courageous presence bolsters Erik's will!
> (Aura of Courage: +2 dice to resist)
> Erik's Resolve Check: Success!
```

```
> The Cultist's fear aura overwhelms the Skald...
> The Skald is now [Feared]!
> Grizelda's fury grows watching her ally suffer!
> She gains 20 Vengeance!
```