# Alka-hestur (Combat Alchemist) — Specialization Specification v5.0

Type: Specialization
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-SPEC-ALKAHESTUR-v5.0
Primary Creed Affiliation: Independents
Proof-of-Concept Flag: Yes
Resource System: Stamina
Sub-Type: Utility
Sub-item: Alchemical Analysis I (Alchemical%20Analysis%20I%2064c9ad84f76c411fa224d157384a9cb7.md), Payload Strike (Payload%20Strike%20d56a513d53ca436a95e2a4867e17150b.md), Field Preparation (Field%20Preparation%20290cab38c0be499eaa28698ac220235b.md), Rack Expansion (Rack%20Expansion%203e1da722466f4b01a8cdd65589100d81.md), Targeted Injection (Targeted%20Injection%207a309a20178e460fbe16e094ddc06844.md), Cocktail Mixing (Cocktail%20Mixing%20554fd0608d0a4f978e76e8296d1eb8e2.md), Area Saturation (Area%20Saturation%20c5aaaaea4aec4d37b81986e00bcd5c93.md), Volatile Synthesis (Volatile%20Synthesis%20f9864d9718d149cd988924a0e3624715.md), Master Alchemist (Master%20Alchemist%20ec7e4bbcddb7490e8cbe644227df3f6c.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Validated: No

# Alka-hestur (Combat Alchemist) — Specialization Specification v5.0

## I. Core Philosophy: The Payload Technician

The **Alka-hestur** is the Skirmisher specialization that answers problems with chemistry delivered by lance. Where others rely on steel and strength, you trust cartridges, injectors, and the precise application of alchemical force. You are the payload alchemist who turns a single well-placed strike into a tactical reset.

To choose the Alka-hestur is to embrace the philosophy that **the right reagent solves any problem**. Your saga is written in EMP'd servitors, corroded armor plates, and enemies frozen mid-charge by cryo payloads. You don't fight with weapons—you fight with solutions.

---

## II. Player-Facing Presentation

| Attribute | Value |
| --- | --- |
| **Role** | Vulnerability Exploiter, Debuffer, Elemental Striker, Tactical Support |
| **Primary Attribute** | WITS (for alchemical knowledge and weakness identification) |
| **Secondary Attribute** | FINESSE (for precise payload delivery) |
| **Resource** | Stamina + Payload Charges |
| **Trauma Risk** | Low (volatile handling has minor risks) |

### Gameplay Feel

The Alka-hestur is an **adaptable damage dealer** who provides exactly the damage type or debuff the situation requires. Your operational loop is:

1. **Read**: Identify target weakness or tactical need
2. **Load**: Select appropriate payload from your rack
3. **Deliver**: Strike with alchemical lance to inject payload
4. **Assess**: Evaluate effect and adjust for next engagement

You operate on **rack logic**: pre-selecting a small, appropriate payload set before contact. Choice is made before the road, not in the chaos of combat. You carry the right four, then six payloads—never the whole workshop.

### The Trade-Off

Your versatility requires **preparation and components**. Without alchemical supplies, you're reduced to basic strikes. Payloads are consumed on use, creating resource management pressure. Your power is purchased at the bench before it's spent in the field.

---

## III. Mechanical Implementation

### A. Core Mechanic: Payload System

Payloads are alchemical cartridges loaded into your injector lance:

| Payload Type | Effect | Best Against |
| --- | --- | --- |
| **Ignition** | Fire damage + [Burning] DoT | Organic enemies, cold-resistant |
| **Cryo** | Ice damage + [Slowed] | Fast enemies, fire-resistant |
| **EMP** | Energy damage + [System Shock] | Mechanical/Undying |
| **Acidic** | Physical damage + [Corroded] (reduced Soak) | High-armor targets |
| **Concussive** | Physical damage + [Staggered] | Casters, chargers |
| **Smoke** | Obscures area + breaks targeting | Ranged enemies, retreat cover |
| **Marking** | No damage + reveals [Invisible] + tracks | Stealth enemies, crowd management |

### B. Core Mechanic: Rack Logic

You can carry a limited number of prepared payloads:

- **Base capacity**: 4 payload slots
- Rank progression increases to 6 slots
- Payloads are consumed on use
- Reloading requires bench access or field alchemy (higher tier)

### C. Core Mechanic: Vulnerability Exploitation

**Alchemical Analysis** identifies target weaknesses:

- Reveals elemental vulnerabilities and resistances
- Allows optimal payload selection
- Synergizes with party coordination (shared weakness information)

---

## IV. The Skill Tree: The Alchemist's Arsenal

### Tier 1 — Foundational Chemistry (3 PP each)

**Alchemical Analysis I** *(Passive)*

> *"You read the weakness before you load the answer. A twitch of the servos, a discoloration in the carapace, a hitch in the breath—you see what reagent will break them."*
> 
- +1d10 bonus to WITS checks for identifying creature vulnerabilities and resistances
- On successful analysis, learn one vulnerability or resistance
- **Rank 2**: +2d10; learn all vulnerabilities OR all resistances (choose one)
- **Rank 3**: +3d10; learn ALL vulnerabilities AND resistances; party members gain +1d10 damage vs analyzed target's vulnerabilities

**Payload Strike** *(Active — Standard Action)*

> *"You drive the lance home and trigger the injector. The payload enters the system. The reaction begins."*
> 
- Cost: 25 Stamina + 1 Payload Charge
- FINESSE attack dealing 2d8 base Physical damage
- Apply payload effect based on loaded cartridge type
- **Rank 2**: 3d8 base damage; payload effects last +1 turn
- **Rank 3**: 4d8 base damage; on critical hit, payload effect is doubled (damage or duration)
- Cooldown: None (limited by payload charges)

**Field Preparation** *(Active — 10 minutes, non-combat)*

> *"The bench is where battles are won. You measure reagents, seal cartridges, and check O-rings with practiced efficiency."*
> 
- Cost: 10 Stamina + Reagent Components
- Prepare payloads from raw materials during rest or downtime
- Base: Prepare up to 4 payloads per preparation session
- **Rank 2**: Prepare up to 6 payloads; preparation time reduced to 5 minutes
- **Rank 3**: Prepare up to 8 payloads; can prepare **during short rests**; 20% chance to create bonus payload from same materials

---

### Tier 2 — Advanced Payload Delivery (4 PP each, requires 8 PP in tree)

**Rack Expansion** *(Passive)*

> *"More solutions means more answers. Your bandolier grows heavier, but so does your versatility."*
> 
- Increase payload carrying capacity by 2 (total: 6 base)
- Can carry 2 of the same payload type (previously limited to 1 each)
- **Rank 2**: Capacity +4 (total: 8); can carry 3 of same type
- **Rank 3**: Capacity +6 (total: 10); can **quick-swap** loaded payload as Bonus Action once per turn

**Targeted Injection** *(Active — Standard Action)*

> *"You aim for the weak point—the joint seal, the exposed conduit, the gap in the carapace. Your payload bypasses armor entirely."*
> 
- Cost: 35 Stamina + 1 Payload Charge
- FINESSE attack dealing 3d8 base Physical damage
- This attack **ignores Soak** (armor penetration)
- Apply payload effect with +50% potency (damage or duration)
- **Rank 2**: 4d8 damage; +75% potency; target is [Vulnerable] for 1 turn
- **Rank 3**: 5d8 damage; +100% potency; if target has vulnerability to payload element, deal **triple** damage instead of double
- Cooldown: 3 turns

**Cocktail Mixing** *(Passive)*

> *"Why use one reagent when two creates something new? Your combination formulas produce effects no single cartridge can match."*
> 
- Can combine 2 payload types into a single **Cocktail Payload**
- Cocktail applies both effects but consumes both charges
- **Rank 2**: Cocktails deal +2d8 bonus damage from chemical reaction
- **Rank 3**: Can create **Triple Cocktails** (3 payloads combined); effects synergize (Fire + Acid = [Melting]: double armor reduction)

---

### Tier 3 — Mastery of Payload Alchemy (5 PP each, requires 16 PP in tree)

**Area Saturation** *(Active — Standard Action)*

> *"Sometimes the answer isn't a precise injection—it's a cloud, a splash, a wave of reagent that catches everything in the zone."*
> 
- Cost: 45 Stamina + 3 Payload Charges (same type)
- Affect all enemies in 3×3 area
- Deal 4d8 elemental damage (based on payload type)
- Apply payload effect to all targets
- **Rank 2**: 5d8 damage; 4×4 area; effects last +1 turn
- **Rank 3**: 6d8 damage; 5×5 area; can be used with Cocktail payloads (affects all with both effects); enemies in center take +50% damage
- Cooldown: 4 turns

**Volatile Synthesis** *(Active — Bonus Action)*

> *"In the press of combat, you cannot reach your bench. But you can improvise—dangerously, brilliantly, with materials at hand."*
> 
- Cost: 20 Stamina + 5 Psychic Stress (volatile handling)
- Create 1 payload of any type from ambient materials (no components required)
- Payload is **Unstable**: Must be used within 3 turns or it degrades
- **Rank 2**: Create 2 payloads; Stress reduced to 3; stable for 5 turns
- **Rank 3**: Create 3 payloads; Stress reduced to 2; payloads are **stable** (no degradation); can create **Cocktails** via Volatile Synthesis
- Cooldown: Once per combat

---

### Capstone — Master Alchemist (6 PP, requires 24 PP + any Tier 3)

> *"You have transcended the distinction between alchemist and weapon. Your touch delivers ruin calibrated to molecular precision. Every enemy has a solution—you carry all of them."*
> 

**Passive Component — Universal Reagent**:

- Your payloads deal +2d8 bonus damage of their element
- Payload effects last +2 turns baseline
- You can identify ALL creature vulnerabilities automatically (no check required)
- **Rank 2**: +3d8 bonus; enemies struck by your payloads have -2 to saves vs subsequent payload effects
- **Rank 3**: +4d8 bonus; once per combat, your payload can apply its effect **twice** (double duration and double damage over time)

**Active Component — Perfect Solution** *(Standard Action — Once per combat)*:

- Cost: 50 Stamina + 1 Payload Charge
- Analyze target and create **custom payload** specifically designed to exploit their vulnerabilities
- This attack automatically hits (no roll required)
- Deals 8d10 damage of target's weakest resistance type
- Applies [Perfect Exploitation]: Target takes +100% damage from all sources for 2 turns
- **Rank 2**: 10d10 damage; [Perfect Exploitation] lasts 3 turns
- **Rank 3**: 12d10 damage; if target is killed, you recover ALL expended payload charges from the precision synthesis

---

## V. Systemic Integration

### Trauma Economy

- **Coherent Specialization**: No heretical mechanics
- **Volatile Synthesis** is the only Psychic Stress source (field improvisation risk)
- **Low Risk**: Stable, preparation-focused specialization

### Situational Power Profile

**Optimal Conditions**:

- Encounters with varied enemy types requiring different damage types
- Prepared engagements where rack loading is optimized
- High-armor or elemental-vulnerable enemies
- Party compositions lacking elemental damage options

**Weakness Conditions**:

- Extended expeditions without resupply (component scarcity)
- Surprise encounters before loading appropriate payloads
- Enemies immune to all elemental damage
- Resource-denied scenarios

### Synergies

**Positive**:

- **Jötun-Reader**: Their analysis + your payloads = perfect targeting
- **Hlekkr-master**: Displace enemies into your Area Saturation zones
- **Skjaldmær**: Hold the line while you deliver precision payloads
- **Controllers**: Locked-down enemies can't dodge your injections

**Negative**:

- **Fast-paced parties**: May not allow preparation time
- **Resource-light campaigns**: Component scarcity limits effectiveness

---

## VI. Social Function (Layer 2)

The Alka-hestur serves critical functions beyond combat:

- **Civic Fire Safety**: Smoke and Marking payloads for crowd control
- **Custody Protocols**: Kaupmaðr logs batches to prevent panic rumors
- **Medical Support**: Some payloads (stimulants, antidotes) serve healing functions
- **Signal Coordination**: Marking dye confirms hits and manages battlefields through smoke

---

## VII. v5.0 Setting Compliance

✅ **Technology, Not Magic**: Alchemy is chemistry, not supernatural

✅ **Layer 2 Voice**: "Rack logic," "injector hygiene," "payload delivery," "reagent discipline"

✅ **Norse-Inspired**: Alka-hestur (alchemy-horse, i.e., mount/vehicle for alchemy)

✅ **Blight Integration**: Some payloads exploit Corruption; field synthesis carries CPS-adjacent risks