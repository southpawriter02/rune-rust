# Chains of Decay — Tier 3 Ability

Type: Ability
Priority: Nice-to-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-IRONBANE-CHAINSOFDECAY-v5.0
Mechanical Role: Controller/Debuffer, Damage Dealer
Parent item: Iron-Bane (Zealous Purifier) — Specialization Specification v5.0 (Iron-Bane%20(Zealous%20Purifier)%20%E2%80%94%20Specialization%20Spec%20c2718eab17e04443af19f9da976f4ad3.md)
Proof-of-Concept Flag: No
Resource System: Stamina, Vengeance
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Core Identity

| Property | Value |
| --- | --- |
| **Ability Name** | Chains of Decay |
| **Specialization** | Iron-Bane (Zealous Purifier) |
| **Tier** | 3 (Extermination Mastery) |
| **Type** | Active (AoE Attack + Debuff) |
| **PP Cost** | 5 PP |
| **Stamina Cost** | 50 |
| **Vengeance Cost** | 40 |
| **Target** | All enemies in Front Row (3m radius) |
| **Rank** | R3 only (no progression) |

---

## Description

The Iron-Bane plants their blade in the earth and speaks the **Litany of Ruination**—the most sacred prayer of their order. Holy power surges through the sanctified steel and into the corrupted machines around them.

From the point of impact, chains of rust-light snake outward, connecting every piece of Undying metal in range. Where the light touches, the corrosion begins—not mere surface rust, but deep structural decay that spreads from one machine to the next like a divine plague.

> *"Let their corruption consume them. Let their strength become their grave."*
> 

---

## Mechanics

### Effect Summary (Rank 3 Only)

| Property | Value |
| --- | --- |
| **Damage** | 5d10 + MIGHT Physical (auto-hit) |
| **[Corroded]** | -4 Soak for 4 rounds (Undying only) |
| **[Slowed]** | 2 rounds (Undying only) |
| **Target Area** | All enemies in Front Row / 3m radius |
| **Attack Type** | Auto-hit (ground eruption, cannot be dodged) |

### Formula

```
Damage = 5d10 + MIGHT (to each target)
CorrodedEffect = -4 Soak, 4 rounds
SlowedEffect = -2 Movement, skip reactions, 2 rounds

Targeting:
  Targets = All enemies in EnemyFrontRow OR within 3m radius
  AutoHit = true (no attack roll required)

Conditions:
  [Corroded] only applies to Target.Faction ∈ {Undying, Mechanical}
  [Slowed] only applies to Target.Faction ∈ {Undying, Mechanical}
  Damage applies to ALL targets regardless of faction
```

### Resolution Pipeline

1. **Resource Deduction:** Spend 50 Stamina + 40 Vengeance
2. **Target Identification:** All enemies in Front Row / 3m radius
3. **Damage Application:** Apply 5d10 + MIGHT to each target (auto-hit)
4. **Faction Check (per target):** For each Undying/Mechanical target:
    - Apply [Corroded] for 4 rounds
    - Apply [Slowed] for 2 rounds
5. **Non-Undying:** Damage only; no debuffs applied

---

## Worked Examples

### Example 1: Pure Undying Front Row

**Situation:** Grizelda (MIGHT 4, Vengeance 60) faces 3 Iron Husks in Front Row

```
Cost: 50 Stamina, 40 Vengeance → Vengeance now 20
Targets: 3 Iron Husks (all Undying)

Damage Roll: 5d10 + 4 = [8, 6, 9, 4, 7] + 4 = 38 Physical
  Iron Husk 1: 38 - 8 (Soak) = 30 damage
  Iron Husk 2: 38 - 8 (Soak) = 30 damage  
  Iron Husk 3: 38 - 8 (Soak) = 30 damage

Debuffs Applied (all 3 targets):
  [Corroded]: -4 Soak for 4 rounds
  [Slowed]: Movement halved, no reactions for 2 rounds

Post-Chains Soak: 8 - 4 = 4 each
Party follow-up attacks deal +4 damage per hit to each target
```

### Example 2: Mixed Front Row

**Situation:** 2 Undying + 1 Blighted Beast in Front Row

```
Cost: 50 Stamina, 40 Vengeance
Targets: Rusted Warden (Undying), Iron Husk (Undying), Ash-Vargr (Beast)

Damage: 5d10 + 4 = 34 Physical to ALL three targets

Debuff Application:
  Rusted Warden: [Corroded] ✓, [Slowed] ✓
  Iron Husk: [Corroded] ✓, [Slowed] ✓
  Ash-Vargr: NO debuffs (organic faction)

Result: Damage to all, debuffs only to Undying
```

### Example 3: Boss Setup Combo

**Situation:** Setting up Iron Hulk boss for party burst

```
Round 1: Chains of Decay
  Cost: 50 Stamina, 40 Vengeance
  Damage: 5d10 + 4 = 36 to Iron Hulk
  [Corroded]: -4 Soak (boss now at 12 - 4 = 8)
  [Slowed]: Boss loses reactions, halved movement

Round 2: Corrosive Strike (stacking)
  [Corroded] Stack 2: -4 additional Soak
  Boss Soak: 12 - 8 = 4

Round 2 (Party): Coordinated burst
  All party Physical attacks now deal +8 damage vs boss
  Boss cannot react (Slowed) to interrupt burst
```

---

## Failure Modes

| Failure Type | Result |
| --- | --- |
| **Insufficient Resources** | Cannot activate; need 50 Stamina AND 40 Vengeance |
| **No Targets in Range** | Resources spent, no effect (poor positioning) |
| **All Non-Undying** | Damage applies but no debuffs — inefficient use |
| **Self-Centered Risk** | Iron-Bane must be in melee range; vulnerable to counter-attacks |
| **Corroded Cap** | If targets already have 2 stacks, only refreshes duration |

---

## Tactical Applications

1. **Wave Clear:** Damage + debuff entire enemy formation in one action
2. **Party Setup:** Mass [Corroded] enables devastating party follow-up
3. **Control:** [Slowed] removes enemy reactions and limits repositioning
4. **Corroded Stacking:** Combines with Corrosive Strike for -8 Soak on priority target
5. **Auto-Hit Reliability:** Ground eruption cannot miss — guaranteed value

---

## Integration Notes

### Synergies

- **Corrosive Strike:** Stack [Corroded] for maximum Soak reduction (-8)
- **Purging Alacrity:** AoE debuff = guaranteed Defense buff trigger
- **Party Burst:** All physical dealers benefit from mass Soak reduction
- **Annihilate Iron Heart:** Multiple [Corroded] + [Bloodied] targets = execution parade

### Anti-Synergies

- **Non-Undying Campaigns:** Damage works but no debuffs — 90 resources for just AoE damage
- **Scattered Enemies:** Requires clustered front row; back row untouched
- **Vengeance Hungry:** 40 Vengeance is significant — competes with Annihilate (75)
- **Positioning Risk:** Self-centered AoE puts Iron-Bane in danger

### The [Slowed] Status Effect

```
[Slowed]
- Movement speed halved
- Cannot take Reactions (Opportunity Attacks, Interrupts)
- Duration: 2 rounds
- Cannot stack (refreshes duration)
```

### Combat Log Example

```
> Grizelda slams her hammer into the ground, unleashing the 
> Chains of Decay!
> Ethereal, rusting chains erupt from the floor, lashing at 
> the enemy frontline!
> The Rusted Warden takes 34 Physical damage and is now 
> [Corroded] and [Slowed]!
> The Iron Husk takes 34 Physical damage and is now 
> [Corroded] and [Slowed]!
> The Rival Raider takes 34 Physical damage!
> (Raider is organic — no debuffs applied)
```