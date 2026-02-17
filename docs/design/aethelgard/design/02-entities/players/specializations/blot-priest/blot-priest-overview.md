---
id: SPEC-BLOT-PRIEST-30001
title: "Blót-Priest"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Blót-Priest

**Archetype:** Mystic | **Path:** Heretical | **Role:** Sacrificial Healer / Bio-Aetheric Conduit

> *"The Conduit of a Sick World"*

---

## Identity

| Property | Value |
|----------|-------|
| **Name** | Blót-Priest |
| **Translation** | Old Norse: "Sacrifice-Priest" |
| **Archetype** | Mystic |
| **Path Type** | Heretical |
| **Role** | Sacrificial Healer / Bio-Aetheric Conduit |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | STURDINESS |
| **Resource** | Aether Pool (AP) OR Health Pool (HP) |
| **Trauma Risk** | **EXTREME** (highest Corruption generation) |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| **PP Cost** | 3 PP |
| **Minimum Legend** | 5 |
| **Prerequisites** | Mystic Archetype |
| **Exclusive With** | None |
| **Warning** | Heretical specialization—high Corruption risk |

---

## Design Philosophy

### Tagline
*"What would you sacrifice to save everyone?"*

### Core Fantasy
You are a practitioner of bio-Aetheric transduction who understands that the Aether is a poisoned well and that life force itself is a currency in a failing system. Your magic is not one of creation, but of transference—draining the corrupted life force of enemies, filtering it through the crucible of your own body, and offering it up as a desperate, tainted gift of healing to your allies.

You walk a razor's edge between martyrdom and monstrosity. Every act of healing is a willing acceptance of the world's sickness, a small, sacrificial death.

### What Blót-Priests ARE
- Sacrificial conduits who bypass chaotic Aether by using their own life force as a stable casting medium
- Biological system hackers using their body as a corruptible medium for power transfer
- High-risk healers whose every gift comes at a terrible price

### What Blót-Priests Are NOT
- ❌ Safe healers (they inflict Corruption on self AND allies)
- ❌ Front-line tanks (low HP, no heavy armor)
- ❌ Sustainable (Corruption spiral is inevitable)

### Mechanical Identity
1. **Sacrificial Casting**: Spend HP instead of AP to cast (2 HP per 1 AP, +1 Corruption)
2. **Life Siphon**: Offensive spells drain enemy HP and heal self (+1 Corruption per siphon)
3. **Blight Transference**: Healing allies transfers YOUR Corruption to THEM
4. **[Bloodied] Bonuses**: Multiple abilities gain power when below 50% HP

### Gameplay Feel
Desperate, sacrificial, morally complex. Every heal is a choice between your soul and theirs. Combat becomes a triage of Corruption management—who can afford to be tainted? The question every Blót-Priest must answer: Is winning this fight worth damning yourself and everyone you heal?

---

## v5.0 Setting Compliance: Technology, Not Magic

Blót-Priests are NOT fantasy blood mages. Their "magic" is bio-Aetheric transduction:

| Ability | Technical Explanation |
|---------|----------------------|
| Sacrificial Casting | Using organic tissue as biological antenna to bypass corrupted Aether pathways |
| Life Siphon | Extracting bio-electric energy from dying neural tissue (contaminated with Blight patterns) |
| Blight Transference | Passing Corruption through direct biological contact (blood-to-blood, cellular exchange) |
| Hemorrhaging Curse | Inducing cascading cellular breakdown via Aetheric resonance frequencies |

They are biological system hackers who use their own body as a corruptible medium for power transfer. It is horrifying biotech, not sorcery.

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
                    [BLÓT-PRIEST SACRIFICIAL MASTERY]
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ SANGUINE        │ │ BLOOD           │ │ GIFT OF         │
│ PACT            │ │ SIPHON          │ │ VITAE           │
│ [Tier 1]        │ │ [Tier 1]        │ │ [Tier 1]        │
│ Passive         │ │ Active          │ │ Active          │
│ HP→AP casting   │ │ Damage+heal     │ │ Heal ally       │
│ +Corruption     │ │ 3d6→5d6         │ │ +Transfer Corr  │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ BLOOD           │ │ EXSANGUINATE    │ │ CRIMSON         │
│ WARD            │ │                 │ │ VIGOR           │
│ [Tier 2]        │ │ [Tier 2]        │ │ [Tier 2]        │
│ Active          │ │ Active          │ │ Passive         │
│ HP→Shield       │ │ DoT+Lifesteal   │ │ [Bloodied]      │
│ +Stress attacker│ │ +Corruption     │ │ bonuses         │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │ HEMORRHAGING    MARTYR'S    │
              │ CURSE           RESOLVE     │
              │ [Tier 3]        [Tier 3]    │
              │ DoT+anti-heal   +Soak when  │
              │ +lifesteal      [Bloodied]  │
              └──────────────┬──────────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │         HEARTSTOPPER        │
              │          [Capstone]         │
              │  Crimson Deluge (AoE Heal)  │
              │  OR Final Anathema (Execute)│
              │       Once Per Combat       │
              └─────────────────────────────┘
```

### Ability Index

| ID | Name | Tier | Type | PP | Summary |
|----|------|------|------|----|---------
| 30010 | [Sanguine Pact](abilities/sanguine-pact.md) | 1 | Passive | 3 | Unlocks Sacrificial Casting (HP→AP) |
| 30011 | [Blood Siphon](abilities/blood-siphon.md) | 1 | Active | 3 | 3d6→5d6 damage + lifesteal (+Corruption) |
| 30012 | [Gift of Vitae](abilities/gift-of-vitae.md) | 1 | Active | 3 | Heal ally 4d10→8d10, transfers Corruption |
| 30013 | [Blood Ward](abilities/blood-ward.md) | 2 | Active | 4 | HP→Shield (2.5-3.5× value) |
| 30014 | [Exsanguinate](abilities/exsanguinate.md) | 2 | Active | 4 | DoT curse + lifesteal (+Corruption/tick) |
| 30015 | [Crimson Vigor](abilities/crimson-vigor.md) | 2 | Passive | 4 | [Bloodied] bonuses to healing/siphon |
| 30016 | [Hemorrhaging Curse](abilities/hemorrhaging-curse.md) | 3 | Active | 5 | DoT + [Bleeding] + anti-healing debuff |
| 30017 | [Martyrs Resolve](abilities/martyrs-resolve.md) | 3 | Passive | 5 | +Soak, +Resolve when [Bloodied] |
| 30018 | [Heartstopper](abilities/heartstopper.md) | 4 | Active | 6 | AoE heal OR execute (once/combat) |

---

## Core Mechanics

### Sacrificial Casting (The Sanguine Sacrament)

The Blót-Priest can cast spells by spending HP instead of AP:

| Property | Value |
|----------|-------|
| Conversion Rate | 2 HP per 1 AP (base) |
| Corruption Cost | +1 per HP-cast |
| Limit | Cannot reduce HP below 1 |

**Rank Progression:**
- Rank 1: 2 HP per 1 AP
- Rank 2: 1.5 HP per 1 AP
- Rank 3: 1 HP per 1 AP, Corruption cost reduced to +0.5

**Strategic Value:**
- Functions when AP depleted
- Enables massive burst potential
- Creates resource flexibility

### Life Siphon (The Corrupted Harvest)

Offensive spells drain enemy HP and heal the caster—but the life force is Blighted:

```
OnLifeSiphon(Damage):
    HealAmount = Damage × SiphonPercent
    Caster.HP += HealAmount
    Caster.Corruption += 1
    Log("Life Siphon: +{HealAmount} HP, +1 Corruption")
```

**Design Note:** Each siphon inflicts +1 Corruption. The more you fight, the more monstrous you become.

### Blight Transference (The Tainted Gift)

The Blót-Priest's defining mechanic. When healing allies, they transfer a portion of their accumulated Corruption to the target:

```
OnHealAlly(HealAmount, CorruptionTransfer):
    Ally.HP += HealAmount
    Ally.Corruption += CorruptionTransfer
    Log("Gift of Vitae: {Ally} healed {HealAmount}, received {CorruptionTransfer} Corruption")
```

**Moral Implications:**
- Save dying ally now, corrupt them later?
- Spread Corruption across party evenly?
- Let someone die to avoid spreading Corruption?

### [Bloodied] Status Bonuses

Multiple abilities gain power when HP drops below 50%:

| Ability | [Bloodied] Bonus |
|---------|------------------|
| Crimson Vigor | +50-100% healing potency, +25-60% siphon |
| Martyr's Resolve | +5-10 Soak, +2-4d Resolve |
| Blood Siphon | +25% damage (indirect via Crimson Vigor) |

---

## Trauma Economy Profile

**EXTREME Heretical Specialization** — The Blót-Priest is the most Corruption-intensive specialization in the system:

| Action | Corruption Cost |
|--------|-----------------|
| Every HP-cast | +1 Corruption |
| Every Life Siphon | +1 Corruption |
| Gift of Vitae | Transfers 1 Corruption to ally |
| Exsanguinate (full duration) | +3 Corruption |
| Heartstopper: Crimson Deluge | +10 Corruption + 5 to each ally |
| Heartstopper: Final Anathema | +15 Corruption (after transfer) |

**The question is not "if" the Blót-Priest becomes corrupted, but "how fast" and "who else gets corrupted."**

---

## Situational Power Profile

### Optimal Conditions
- Desperate fights where burst healing is critical
- Target-rich environments (Life Siphon sustain)
- Parties that can afford Corruption spread
- Boss fights where Final Anathema can end the threat
- When low HP enables Crimson Vigor bonuses

### Weakness Conditions
- Long expeditions (Corruption spiral accelerates)
- Parties with low Corruption tolerance
- Single-enemy encounters (limited siphon targets)
- When party is already heavily corrupted
- Sustained combat without recovery opportunities

---

## Party Synergies

### Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Iron-Hearted** | Both thrive on edge of death—complementary low-HP gameplay |
| **Bone-Setter** | Can manage party Corruption spread; coherent healer backup |
| **Berserkr** | Both heretical; Blót-Priest sustains the Berserkr's reckless playstyle |
| **High-HP tanks** | Gift of Vitae scales with Max HP; can afford Corruption transfer |
| **Skjaldmær** | Can absorb Corruption transfers while protecting Priest |

### Negative Synergies

| Partner | Conflict |
|---------|----------|
| **Coherent-focused parties** | Corruption spread is unacceptable |
| **Multiple heretical specs** | Competing for Corruption tolerance |
| **Low-HP allies** | Can't survive Blight Transfer costs |
| **Corruption-sensitive builds** | Party members approaching thresholds |

---

## Balance Data

### Power Curve

| Level Range | Power Level | Notes |
|-------------|-------------|-------|
| 1-5 | High | Strongest early healer (at a price) |
| 6-10 | Very High | Crimson Vigor + Exsanguinate sustain |
| 11-15 | Extreme | Hemorrhaging Curse + Martyr's Resolve |
| 16+ | Extreme | Heartstopper can end or win fights |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Damage | 6/10 | Moderate through DoTs and siphon |
| Survivability | 4/10 | Low HP, but self-healing sustain |
| Support | 10/10 | Most powerful healer—at a price |
| Control | 4/10 | Anti-healing debuff only |
| Utility | 3/10 | Highly specialized role |

---

## Voice Guidance

### Tone Profile
- Grim, resigned, determined
- Speaks of sacrifice and price
- Understands the horror of their gift

### Example Quotes (NPC Flavor Text)
- *"My blood for your wounds. My soul for your survival."*
- *"The Blight flows through me. Better me than you... for now."*
- *"I don't heal. I redistribute suffering."*
- *"Every life I save costs a piece of mine. I've been saving lives for a long time."*

---

## Layered Interpretation (Lexicon)

| Layer | Interpretation |
|-------|----------------|
| L1 (Mythic) | Blood-priest who bargains with the dying world; the healer whose touch carries a curse |
| L2 (Diagnostic) | Bio-Aetheric transduction specialist; filters Blighted life force through their own body to transfer vitality at the cost of spiritual integrity |
| L3 (Technical) | Biological antenna bypassing corrupted Aether pathways; neural tissue extraction; cellular-level Corruption exchange via blood-to-blood contact |

---

## Phased Implementation Guide

### Phase 1: Foundation
- [ ] Implement Sacrificial Casting (HP→AP conversion)
- [ ] Add Corruption-on-cast tracking
- [ ] Implement [Bloodied] status detection

### Phase 2: Core Abilities
- [ ] Implement Blood Siphon (damage + lifesteal + Corruption)
- [ ] Implement Gift of Vitae (heal + Corruption transfer)
- [ ] Implement Sanguine Pact passive

### Phase 3: Advanced Systems
- [ ] Implement Blood Ward (HP→Shield)
- [ ] Implement Exsanguinate (DoT + lifesteal)
- [ ] Implement Crimson Vigor ([Bloodied] bonuses)

### Phase 4: Mastery
- [ ] Implement Hemorrhaging Curse (DoT + anti-heal)
- [ ] Implement Martyr's Resolve (defensive [Bloodied] bonuses)
- [ ] Implement Heartstopper (dual-mode capstone)

### Phase 5: Polish
- [ ] Add blood/sacrifice visual effects
- [ ] Implement Corruption transfer UI
- [ ] Test Corruption spiral balance

---

## Testing Requirements

### Unit Tests
- Sacrificial Casting HP→AP conversion
- Life Siphon heal percentage calculations
- Blight Transfer Corruption movement
- [Bloodied] threshold detection
- Heartstopper death defiance trigger

### Integration Tests
- Full combat with Corruption accumulation
- Party-wide Corruption spread via Gift of Vitae
- Crimson Vigor bonus application when Bloodied
- Heartstopper: Crimson Deluge party heal + Corruption

### Manual QA
- Verify Sacrificial Casting feels risky but rewarding
- Test Blight Transference moral weight
- Confirm Heartstopper feels like ultimate sacrifice

---

## Logging Requirements

### Event Templates

```
OnSacrificialCast:
  "[Character] sacrifices {HP} HP to cast (costs {AP} AP equivalent)"
  "+{Corruption} Corruption from blood magic"

OnBloodSiphon:
  "Blood Siphon: {Damage} damage to {Target}, healed {Amount} HP"
  "+1 Corruption from consuming Blighted life force"

OnGiftOfVitae:
  "Gift of Vitae: {Ally} healed for {Amount} HP"
  "{Ally} received {Corruption} Corruption (Blight Transfer)"

OnHeartstopper:
  "HEARTSTOPPER! {Mode} activated!"
  "Cost: {HPCost} HP ({Percent}% of current)"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Mystic Archetype](../../archetypes/mystic.md) | Parent archetype |
| [Aether Resource](../../../01-core/resources/aether.md) | Primary resource |
| [Corruption Resource](../../../01-core/resources/corruption.md) | Corruption mechanics |
| [Trauma Economy](../../../01-core/trauma-economy.md) | Full trauma system |

---

## Cross-References

| Specialization | Relationship |
|----------------|--------------|
| Vard-Warden | Coherent defensive mystic (opposite approach) |
| Rust-Witch | Heretical offensive mystic (shared corruption risk) |
| Bone-Setter | Coherent healer (backup when Corruption too high) |
| Seiðkona | Heretical divination (shared trauma economy) |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard creation |
