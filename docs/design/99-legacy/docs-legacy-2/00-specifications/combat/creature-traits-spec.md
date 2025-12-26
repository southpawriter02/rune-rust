# Creature Traits System Specification

> **Specification ID**: SPEC-COMBAT-015
> **Status**: Draft
> **Domain**: Combat, Enemy Design
> **Related Systems**: Enemy Design, Combat Resolution, AI Behavior, Trauma Economy, Movement & Positioning

---

## Document Control

| Property | Value |
|----------|-------|
| **Specification ID** | SPEC-COMBAT-015 |
| **Title** | Creature Traits System |
| **Version** | 1.0.0 |
| **Status** | Draft |
| **Last Updated** | 2025-12-04 |
| **Author** | Game Design Team |
| **Reviewers** | Combat Designer, AI Engineer, Balance Team |
| **Stakeholders** | Enemy Designer, Narrative Designer, Implementation Team |

---

## Executive Summary

### Purpose

The **Creature Traits System** provides a modular, composable framework for defining unique creature behaviors, abilities, and mechanics in Rune & Rust. Rather than hardcoding special mechanics for each enemy type, this system allows designers to:

1. **Define reusable traits** that modify creature behavior, combat mechanics, and player interactions
2. **Compose unique creatures** by combining multiple traits
3. **Create thematic consistency** through trait categories aligned with Aethelgard's lore
4. **Balance incrementally** by tuning individual traits rather than entire creatures

### Scope

**In Scope**:
- Trait definition framework and data structures
- Trait categories aligned with game lore (Temporal, Corruption, Mechanical, Psychic, etc.)
- Individual trait specifications with mechanics and balance values
- Integration points with Combat Engine, AI System, Movement, and Trauma Economy
- Example creatures demonstrating trait composition
- Implementation architecture and service design

**Out of Scope**:
- Individual enemy stat blocks (see SPEC-COMBAT-012)
- AI decision-making algorithms (see SPEC-AI-001)
- Specific boss encounter scripts (see SPEC-COMBAT-005)
- Player character traits/abilities (separate system)

### Success Criteria

This specification is successful if:

1. **Designers can create unique creatures** by selecting traits from a catalog without code changes
2. **Traits compose predictably** - combining traits produces expected emergent behavior
3. **Combat feels varied** - trait-based creatures create distinct tactical challenges
4. **Balance is maintainable** - individual traits can be tuned without cascading effects
5. **Lore integration is seamless** - traits reinforce Aethelgard's thematic pillars

---

## Design Philosophy

### 1. Composability Over Inheritance

**Principle**: Creature uniqueness emerges from trait combinations, not deep inheritance hierarchies.

**Implementation**:
- Each creature has a `List<CreatureTrait>` defining its unique mechanics
- Traits are independent modules that hook into combat systems
- No trait assumes the presence of another trait
- Emergent behaviors arise from trait interactions

**Example**:
- **Chrono-Strider**: `[TemporalPhase, ChronoDistortion, TemporalPrescience, RandomBlink]`
- **Rust-Phantom**: `[Incorporeal, CorrosiveAura, FadingForm]`
- **Hive-Drone**: `[SwarmLink, SharedDamage, CollectiveTargeting]`

### 2. Thematic Grounding

**Principle**: Every trait connects to Aethelgard's lore pillars (Decay, Nordic Mythology, Body Horror, Technological Corruption).

**Implementation**:
- Traits organized into thematic categories
- Each trait has lore justification explaining its in-world origin
- Trait names use setting-appropriate terminology
- Visual/audio feedback reinforces thematic identity

**Example**:
- **Temporal traits** reflect Glitch-induced time anomalies
- **Corruption traits** reflect Runic Blight exposure
- **Mechanical traits** reflect Pre-Glitch automation decay
- **Psychic traits** reflect Forlorn entity mechanics

### 3. Tactical Clarity

**Principle**: Players should understand what a trait does through observation and be able to develop counterstrategies.

**Implementation**:
- Traits have clear, observable effects (visual indicators, combat log messages)
- Counter-play exists for every trait (no "unfun" mechanics)
- Trait effects are consistent and predictable
- Discovery is rewarded (first encounter teaches mechanic)

**Example**:
- **TemporalPhase**: Enemy visually flickers/phases when moving; player learns "don't rely on attacks of opportunity"
- **ChronoDistortion**: Stress gain shows in UI with temporal effect; player learns "kill quickly or suffer stress"

### 4. Bounded Complexity

**Principle**: Individual traits are simple; complexity emerges from composition.

**Implementation**:
- Each trait does ONE thing well
- Maximum 4-5 traits per creature (prevents cognitive overload)
- Trait effects are quantified with single values where possible
- No trait requires reading paragraphs to understand

**Example**:
- **Good**: "TemporalPrescience: +3 Evasion"
- **Bad**: "TemporalPrescience: +1 Evasion per stack of Temporal Energy, which accumulates when..."

---

## Trait Categories

Traits are organized into **12 thematic categories** reflecting Aethelgard's world-building:

### Category 1: Temporal Traits

**Theme**: Glitch-induced time anomalies, causality distortions, paradox effects
**Lore Origin**: The Ginnungagap Glitch created pockets of temporal instability; some entities exist partially outside normal time flow
**Visual Motif**: Flickering, afterimages, stuttering movement, clock/rune symbols

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **TemporalPhase** | Movement ignores attacks of opportunity; can move through occupied tiles | - | Entity phases between moments, appearing to teleport |
| **TemporalPrescience** | +X Evasion (anticipates incoming attacks) | +2 to +4 Evasion | Perceives attack trajectories before they occur |
| **ChronoDistortion** | Movement inflicts Psychic Stress to nearby characters | 2-5 Stress per move | Temporal wrongness causes existential dread |
| **RandomBlink** | AI behavior: randomly teleports to valid tile each turn | - | Unstable temporal anchor causes involuntary shifts |
| **TimeLoop** | On death, 25% chance to resurrect at 25% HP (once per combat) | 25% trigger | Caught in dying moment, occasionally escapes |
| **CausalEcho** | Attacks have 20% chance to hit twice (echo from alternate timeline) | 20% double-hit | Actions ripple across parallel moments |
| **TemporalStasis** | Can spend turn to become invulnerable until next turn (AI uses at low HP) | Full turn cost | Freezes self in temporal bubble |
| **Rewind** | Once per combat, can undo last action taken against it | 1 use | Minor temporal reversal |

---

### Category 2: Corruption Traits

**Theme**: Runic Blight infection, data corruption, reality degradation
**Lore Origin**: Exposure to paradoxical information from the Glitch corrupts both flesh and machine
**Visual Motif**: Glitching textures, static, red/black corruption tendrils, fragmented forms

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **BlightAura** | Inflicts +X Corruption per turn to characters within 2 tiles | 1-3 Corruption/turn | Radiates corrupted data/energy |
| **CorrosiveTouch** | Melee attacks inflict Corroded status (stacking armor reduction) | 1-2 stacks | Physical contact transfers corruption |
| **DataFragment** | On death, releases information burst: nearby characters gain +5 Stress OR +3 Corruption | AoE 2 tiles | Corrupted data disperses violently |
| **Glitched** | 15% chance each turn to perform random action instead of AI choice | 15% random | Corrupted decision matrices |
| **Infectious** | Characters who kill this enemy gain 1 Corruption | 1 Corruption | Corruption transfers at death |
| **RealityTear** | Special attack creates hazardous tile (3 turns duration) | Tile hazard | Presence destabilizes local reality |
| **Reforming** | Regenerates 5 HP per turn while below 50% HP | 5 HP/turn | Corruption rebuilds damaged systems |
| **VoidTouched** | Immune to Psychic damage; deals Psychic damage instead of Physical | Damage type swap | Exists partially in the Void |

---

### Category 3: Mechanical Traits

**Theme**: Pre-Glitch automation, Iron Heart power systems, machine logic
**Lore Origin**: Draugr-Pattern and Haugbui-Class automatons retain functional (if corrupted) mechanical systems
**Visual Motif**: Gears, pistons, steam vents, glowing power cores, rusted metal

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **IronHeart** | Immune to Bleeding, Poison; resistant to Psychic (-50% Psychic damage) | Immunity + 50% resist | Machine physiology lacks blood/mind |
| **ArmoredPlating** | +X Soak (flat damage reduction) | +2 to +4 Soak | Heavy armor plating |
| **Overcharge** | Can spend 10 HP to gain +2 damage dice on next attack | 10 HP cost | Diverts power to weapons |
| **EmergencyProtocol** | At <25% HP, gains +2 Defense and +1 damage for remainder of combat | Threshold trigger | Failsafe combat protocols activate |
| **ModularConstruction** | Takes 50% damage from first hit each combat (armor absorbs) | 50% first-hit reduction | Sacrificial plating |
| **SelfRepair** | Heals X HP at end of turn if didn't attack | 3-8 HP/turn | Maintenance subroutines |
| **PowerSurge** | On death, deals 2d6 Lightning damage to adjacent tiles | 2d6 AoE | Power core detonation |
| **Networked** | +1 Accuracy for each allied Mechanical creature within 3 tiles | +1 per ally | Shared targeting data |

---

### Category 4: Psychic Traits

**Theme**: Mental attacks, Forlorn corruption, trauma infliction
**Lore Origin**: Entities corrupted by Runic Blight often develop psychic capabilities as their consciousness fragments
**Visual Motif**: Distortion waves, screaming faces, purple/void energy, fragmented memories

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **ForlornAura** | Passive Psychic Stress aura: +X Stress/turn to characters within range | 2-5 Stress/turn, 2-4 tile range | Presence induces existential dread |
| **MindSpike** | Attack deals Psychic damage (ignores Soak) | Bypasses armor | Direct mental assault |
| **MemoryDrain** | Successful attack reduces target's next action's accuracy by 2 | -2 Accuracy debuff | Scrambles target's focus |
| **PsychicScream** | AoE attack: 1d6 Psychic damage + 5 Stress to all characters in 3-tile radius | 3-tile AoE | Overwhelming mental broadcast |
| **FearAura** | Characters entering combat must pass WILL check (DC 2) or suffer Frightened (1 turn) | DC 2 WILL | Radiates primal terror |
| **ThoughtLeech** | Heals HP equal to Stress inflicted | HP = Stress dealt | Feeds on mental anguish |
| **Hallucination** | 20% chance attacks against this creature target random ally instead | 20% redirect | Induces targeting confusion |
| **Whispers** | At end of each round, lowest-WILL character gains +3 Stress | 3 Stress/round | Constant psychic interference |

---

### Category 5: Mobility Traits

**Theme**: Movement advantages, positioning control, terrain interaction
**Lore Origin**: Various - flight systems, phase technology, enhanced locomotion
**Visual Motif**: Varies by specific trait

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **Flight** | Ignores ground-based hazards; +2 Defense vs melee attacks | +2 vs melee | Aerial mobility |
| **Burrowing** | Can move through Impassable terrain; emerges with +2 Accuracy on next attack | +2 Accuracy ambush | Subterranean movement |
| **Phasing** | Can move through occupied tiles and obstacles | - | Partial incorporeality |
| **Swiftness** | +2 tiles movement per turn | +2 movement | Enhanced speed |
| **Anchored** | Cannot be forcibly moved; immune to knockback/pull effects | Displacement immunity | Locked position |
| **Ambush** | If hidden at start of combat, first attack is automatic critical hit | Auto-crit from stealth | Ambush predator |
| **Hit-and-Run** | After attacking, may move 1 tile without provoking attacks of opportunity | Free disengage | Strike-and-retreat tactics |
| **Territorial** | +2 Accuracy and +2 Defense while in starting zone | +2/+2 in home zone | Defender advantage |

---

### Category 6: Defensive Traits

**Theme**: Damage mitigation, survival mechanics, defensive abilities
**Lore Origin**: Various - armor, regeneration, evasion systems
**Visual Motif**: Shields, barriers, regenerating tissue, deflection effects

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **Regeneration** | Heals X HP at start of turn | 3-10 HP/turn | Biological/nano repair |
| **DamageThreshold** | Ignores attacks dealing less than X damage | Threshold 3-5 | Only significant damage registers |
| **Reflective** | 20% of damage taken is dealt back to attacker | 20% reflect | Damage reflection system |
| **ShieldGenerator** | Starts combat with X temporary HP that doesn't regenerate | 10-20 temp HP | Energy shield |
| **LastStand** | Cannot be reduced below 1 HP for first 2 turns of combat | 2-turn immortality | Desperate survival |
| **AdaptiveArmor** | After taking damage of a type, gains +2 Soak vs that type | +2 Soak (adaptive) | Learns from damage |
| **Camouflage** | -2 Accuracy for ranged attacks against this creature | -2 vs ranged | Hard to target at range |
| **Unstoppable** | Immune to Stun, Root, and Slow effects | CC immunity | Cannot be impeded |

---

### Category 7: Offensive Traits

**Theme**: Enhanced damage, special attack modifiers, aggression
**Lore Origin**: Various - weapon systems, predatory instincts, combat programming
**Visual Motif**: Glowing weapons, energy charges, aggressive stance

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **Brutal** | Critical hits deal triple damage instead of double | 3x crit | Devastating strikes |
| **Relentless** | Can attack twice per turn (second attack at -2 Accuracy) | Double attack (-2) | Unceasing assault |
| **Executioner** | +50% damage against targets below 25% HP | +50% vs wounded | Finisher instinct |
| **ArmorPiercing** | Attacks ignore X points of target's Soak | Ignore 2-4 Soak | Penetrating attacks |
| **Reach** | Can attack targets 2 tiles away with melee attacks | Extended range | Long limbs/weapons |
| **Sweeping** | Basic attacks hit all adjacent enemies | Cleave | Wide attack arc |
| **Enrage** | At <50% HP, +2 damage but -2 Defense | +2 dmg / -2 def | Wounded fury |
| **PredatorInstinct** | +2 Accuracy against isolated targets (no allies within 2 tiles) | +2 vs isolated | Pack hunter targeting |

---

### Category 8: Unique/Exotic Traits

**Theme**: One-of-a-kind mechanics that don't fit other categories
**Lore Origin**: Rare phenomena, boss-specific mechanics, experimental
**Visual Motif**: Varies dramatically

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **SplitOnDeath** | When killed, spawns 2 smaller versions (50% HP, 50% damage each) | 2 spawns | Reproducing entity |
| **MirrorImage** | Starts combat with 2 illusory duplicates (1 HP each, destroyed on hit) | 2 decoys | Holographic decoys |
| **Vampiric** | Heals 50% of damage dealt | 50% lifesteal | Drains life force |
| **Explosive** | On death, deals Xd6 damage to all characters within 2 tiles | 2d6-4d6 AoE | Volatile internals |
| **Resurrection** | If killed while ally is alive, returns at 50% HP after 2 turns | 2-turn revive | Necromantic link |
| **SymbioticLink** | Shares damage with linked ally (50% each) | Damage split | Bonded entities |
| **Berserk** | At <25% HP, attacks random target (including allies) | Random targeting | Loss of control |
| **PackTactics** | +1 Accuracy per ally attacking same target this round | Stacking accuracy | Coordinated assault |

---

### Category 9: Resistance Traits

**Theme**: Damage type resistances, elemental affinities, vulnerability exploitation
**Lore Origin**: Creatures adapted to specific environments, materials, or energy types develop corresponding resistances
**Visual Motif**: Elemental auras, material composition indicators, damage type icons

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **FireResistant** | Takes 50% damage from Fire; immune to Burning status | 50% Fire resist | Heat-adapted physiology or fire-resistant materials |
| **ColdResistant** | Takes 50% damage from Cold; immune to Frozen/Slowed from cold | 50% Cold resist | Cryo-adapted systems or antifreeze biology |
| **LightningResistant** | Takes 50% damage from Lightning; immune to Stunned from electrical | 50% Lightning resist | Grounded chassis or insulated biology |
| **PsychicResistant** | Takes 50% damage from Psychic; +2 to WILL saves vs mental effects | 50% Psychic resist | Fragmented consciousness or shielded mind |
| **PhysicalResistant** | Takes 75% damage from Physical attacks (slashing, piercing, bludgeoning) | 25% Physical resist | Dense material composition or hardened hide |
| **AcidResistant** | Takes 50% damage from Acid/Corrosion; immune to Corroded status | 50% Acid resist | Inert surface material or neutralizing secretions |
| **FireVulnerable** | Takes 150% damage from Fire; Burning lasts +1 turn | 150% Fire weakness | Flammable composition or heat-sensitive systems |
| **ColdVulnerable** | Takes 150% damage from Cold; Frozen/Slowed lasts +1 turn | 150% Cold weakness | Requires warmth to function |
| **LightningVulnerable** | Takes 150% damage from Lightning; auto-Stunned for 1 turn on Lightning hit | 150% Lightning weakness | Conductive materials or sensitive electronics |
| **HolyVulnerable** | Takes 200% damage from Holy/Radiant sources | 200% Holy weakness | Corrupted or undead nature |
| **ElementalAbsorption** | Choose element: damage of that type heals instead of hurting | Full absorption | Elemental affinity or energy converter |
| **AlloyedForm** | Immune to Corroded; +2 Soak vs Physical; vulnerable to Lightning (150%) | Mixed | Refined metal construction |
| **OrganicBane** | +50% damage against organic (non-Mechanical) targets | +50% vs organic | Anti-biological weaponry |
| **MechanicalBane** | +50% damage against Mechanical targets; attacks disable SelfRepair for 1 turn | +50% vs mechanical | EMP-enhanced or disruption attacks |

---

### Category 10: Strategy/AI Behavior Traits

**Theme**: Tactical decision-making patterns, target prioritization, self-preservation instincts
**Lore Origin**: Programming remnants, predatory instincts, learned survival behaviors
**Visual Motif**: Targeting indicators, behavior state icons, tactical overlays

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **Cowardly** | Flees combat (attempts to leave grid) when HP drops below 25% | Flee at 25% HP | Self-preservation overrides combat directives |
| **Skittish** | After taking damage, 40% chance to move away from attacker instead of acting | 40% flee on hit | Easily startled; prey instincts |
| **HealerHunter** | Prioritizes targeting characters with healing abilities; +2 Accuracy vs healers | +2 vs healers | Programmed to eliminate support threats |
| **ThreatFocused** | Always attacks the character who dealt most damage to it last round | Aggro mechanic | Vengeful targeting protocols |
| **WeaknessSensor** | Prioritizes targets below 50% HP; +1 Accuracy vs wounded targets | +1 vs wounded | Opportunistic predator |
| **ProtectorInstinct** | Moves to intercept attacks against allied creatures; can bodyblock | Bodyguard AI | Defensive programming or pack loyalty |
| **Opportunist** | Prioritizes targets affected by status effects; +2 damage vs debuffed targets | +2 vs debuffed | Exploits vulnerability |
| **CasterKiller** | Prioritizes targets with high WILL/WITS; interrupts abilities on hit (25% chance) | 25% interrupt | Anti-magic programming |
| **Berserker** | Never retreats; +1 damage per 10% HP lost (max +10 at 0% HP) | Scaling damage | Rage-fueled aggression |
| **PackLeader** | Allied creatures within 3 tiles gain +1 Accuracy; this creature acts last in initiative | +1 ally Accuracy | Tactical coordination |
| **Ambusher** | Will not engage until player moves adjacent OR player HP drops below 75% | Delayed engagement | Patient predator |
| **Territorial** | Will not leave starting zone; +3 Accuracy and +3 Defense while in starting zone | +3/+3 in zone | Area defender |
| **HitAndFade** | After attacking, attempts to move to maximum range; won't pursue fleeing targets | Kiting AI | Ranged harasser tactics |
| **Swarmer** | Attempts to surround isolated targets; +2 Accuracy when 2+ allies adjacent to target | +2 flanking bonus | Coordinated encirclement |
| **SelfDestructive** | At <10% HP, uses Explosive trait (if present) voluntarily; otherwise charges nearest enemy | Suicide attack | Desperation protocol |
| **Cautious** | Won't attack if it would provoke attack of opportunity; prefers ranged options | Risk-averse | Self-preservation priority |

---

### Category 11: Sensory Traits

**Theme**: Perception abilities, detection methods, sensory limitations
**Lore Origin**: Specialized sensors, biological adaptations, damaged perception systems
**Visual Motif**: Sensor arrays, eye states, detection range indicators

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **Blind** | Cannot target specific enemies; attacks random target in range; immune to visual effects | Random targeting | Non-functional optical systems |
| **Blindsense** | Unaffected by darkness, invisibility, or visual concealment within 3 tiles | 3-tile detection | Echolocation or tremorsense |
| **SoundSensitive** | +2 Accuracy in normal conditions; Stunned for 1 turn by loud sounds (explosions, PsychicScream) | +2 Acc / Stun weakness | Hyperactive audio sensors |
| **SoundBlind** | Immune to sound-based attacks; cannot detect Stealthed enemies; -2 to Initiative | Deafness trade-off | Non-functional audio systems |
| **Tremorsense** | Detects all ground-based creatures within 4 tiles regardless of stealth; cannot detect Flying | Ground detection | Vibration sensors |
| **ThermalVision** | +2 Accuracy vs warm-blooded (organic) targets; -2 Accuracy vs Mechanical/cold targets | Conditional Accuracy | Heat-based targeting |
| **MotionDetection** | +3 Accuracy vs targets that moved last turn; -2 Accuracy vs stationary targets | Movement-based | Motion-tracking sensors |
| **Darkvision** | Unaffected by darkness penalties; can see in magical darkness | Darkness immunity | Enhanced optical sensors |
| **LightSensitive** | -2 Accuracy in bright light; Blinded (1 turn) by flash effects; +2 Accuracy in darkness | Light weakness | Adapted to low-light environments |
| **PsychicSense** | Detects all creatures within 5 tiles regardless of stealth/invisibility; ignores cover for targeting | Full awareness | Psychic perception |
| **Eyeless** | Immune to Blind status and gaze attacks; relies on other senses (specify: sound, smell, tremor) | Blind immunity | No optical dependency |
| **ScatterSense** | Can detect and target all enemies within 2 tiles simultaneously; AoE attacks don't require aiming | Multi-target awareness | 360-degree sensors |

---

### Category 12: Combat Condition Traits

**Theme**: Environmental adaptations, condition immunities, situational modifiers
**Lore Origin**: Specialized design, environmental adaptation, unique physiology
**Visual Motif**: Condition icons, environmental indicators, adaptation markers

| Trait | Effect | Balance Value | Lore Justification |
|-------|--------|---------------|-------------------|
| **Amphibious** | No penalties in water; can move through water tiles freely; +2 Defense in water | Water adaptation | Aquatic capability |
| **HeatAdapted** | No damage from hot terrain (lava proximity, burning ground); +2 Accuracy in hot environments | Heat immunity | Thermal regulation systems |
| **ColdAdapted** | No damage from cold terrain (freezing areas); immune to Slowed from cold; +2 Accuracy in cold | Cold immunity | Cryo-tolerance |
| **ToxinImmune** | Immune to Poison, Corroded, and environmental toxin damage | Full toxin immunity | Sealed systems or resistant biology |
| **RadiationImmune** | Immune to radiation damage and Irradiated status | Radiation immunity | Shielded or radiation-adapted |
| **VacuumSurvival** | Can function in zero-atmosphere; immune to suffocation/pressure effects | Vacuum capable | Sealed systems |
| **GravityAdapted** | Immune to knockback, pull, and gravity-based effects; +2 Defense vs falling | Gravity resistance | Magnetic locks or dense mass |
| **Buoyant** | Cannot enter water (floats on surface); +2 Defense vs ground-based attacks; -2 vs aerial | Surface-bound | Low density composition |
| **Grounded** | Immune to Lightning damage reflection; cannot be lifted/thrown; +2 Soak vs Lightning | Electrical grounding | Conductive grounding system |
| **Ethereal** | 50% chance to ignore terrain effects; can pass through thin walls; -2 vs holy/psychic damage | Partial phasing | Semi-corporeal existence |
| **SunlightWeakness** | In bright/natural light: -2 to all attributes; in darkness: +2 to all attributes | Light vulnerability | Evolved for darkness |
| **MoonlightStrength** | During night/under moonlight: +2 Accuracy, +2 damage; during day: -1 Accuracy | Time-based power | Lunar attunement |
| **StormBorn** | During storms/electrical hazards: +3 Accuracy, regenerate 5 HP/turn; calm weather: normal | Weather-dependent | Storm energy absorption |
| **BloodScent** | +2 Accuracy against Bleeding targets; can detect Bleeding creatures through walls within 5 tiles | Blood tracking | Olfactory hunting |
| **CorruptionSustained** | Heals 2 HP per point of Corruption on the battlefield; weakens (-2 all stats) in pure areas | Corruption-fed | Feeds on Blight energy |
| **HallowedBane** | Takes 2 damage per turn in consecrated/holy areas; +2 damage in corrupted areas | Holy vulnerability | Anti-divine nature |

---

## Trait Interactions

### Synergistic Combinations

Some trait combinations create emergent behaviors:

| Combination | Emergent Behavior | Example Creature |
|-------------|-------------------|------------------|
| TemporalPhase + ChronoDistortion | Mobile stress applicator that can't be pinned down | Chrono-Strider |
| ForlornAura + ThoughtLeech | Self-sustaining through stress it inflicts | Grief-Feeder |
| Regeneration + DamageThreshold | Nearly unkillable without burst damage | Unkillable Horror |
| Flight + Hit-and-Run | Untouchable harasser | Blight-Hawk |
| SplitOnDeath + Explosive | Chain reaction on death | Volatile Swarm |
| Networked + PackTactics | Devastating coordinated attacks | Servitor Legion |
| Blind + Tremorsense + SoundSensitive | Hunts by vibration/sound; exploitable weakness | Echo-Stalker |
| Cowardly + Explosive | Runs away then detonates at range | Fleeing Bomb-Drone |
| HealerHunter + Swiftness + ArmorPiercing | Rushes past frontline to assassinate support | Support-Killer Unit |
| FireResistant + HeatAdapted + StormBorn | Thrives in Muspelheim's volcanic environments | Magma Sentinel |
| Blind + PsychicSense + MindSpike | Cannot be hidden from; ignores physical defenses | Void Seer |
| BloodScent + WeaknessSensor + Executioner | Hunts wounded prey with lethal efficiency | Scent-Hunter |
| LightSensitive + Darkvision + SunlightWeakness | Nocturnal predator, weak in daylight | Night-Stalker |
| ThreatFocused + Berserker + Enrage | Becomes more dangerous as fight continues | Vengeance Construct |
| ProtectorInstinct + ShieldGenerator + Anchored | Immovable guardian that protects allies | Bastion Unit |
| MotionDetection + Cautious + HitAndFade | Sniper that punishes movement | Motion-Tracker Turret |

### Conflicting Combinations (Avoid)

Some combinations are mechanically contradictory or narratively inconsistent:

| Combination | Conflict | Resolution |
|-------------|----------|------------|
| Anchored + RandomBlink | Cannot be moved vs constantly moves | Don't combine |
| IronHeart + Vampiric | Machine vs life-draining | Choose one thematic direction |
| Camouflage + ForlornAura | Hard to see vs obvious psychic presence | ForlornAura overrides stealth |
| Flight + Burrowing | Aerial vs subterranean | Pick one movement mode |

### Trait Interaction Rules

1. **Immunity trumps infliction**: If a creature is immune to an effect, it cannot inflict that effect on itself through traits
2. **Numeric bonuses stack**: Multiple sources of +Accuracy, +Defense, etc. stack additively
3. **Percentage bonuses multiply**: 50% lifesteal + 20% reflect = separate calculations, not combined
4. **Once-per-combat effects track independently**: TimeLoop and Rewind each get their own use
5. **AoE effects don't self-target**: ChronoDistortion doesn't stress the creature using it

---

## Example Creatures

### Chrono-Strider

**Concept**: A temporal anomaly given physical form—an entity that exists between moments, appearing to stutter through reality. Its movements cause existential dread in observers as their minds struggle to process impossible motion.

**Lore**: Born from severe Glitch exposure, Chrono-Striders are believed to be the remnants of Pre-Glitch maintenance workers caught in temporal loops during the Ginnungagap event. They now patrol the spaces between seconds, their humanity long since eroded.

**Traits**:
| Trait | Effect | Tactical Implication |
|-------|--------|---------------------|
| **TemporalPhase** | Ignores attacks of opportunity; moves through occupied tiles | Cannot be pinned down; positioning denial ineffective |
| **TemporalPrescience** | +3 Evasion | Harder to hit; requires high Accuracy or debuffs |
| **ChronoDistortion** | Movement inflicts 3 Stress to characters within 2 tiles | Encourages aggressive engagement; punishes drawn-out fights |
| **RandomBlink** | Teleports to random valid tile each turn | Unpredictable positioning; AoE attacks valuable |

**Base Stats** (Medium Tier):
- HP: 35
- Damage: 2d6 (Temporal Strike - Psychic damage)
- Attributes: MIGHT 2, FINESSE 4, WITS 3, WILL 4, STURDINESS 2
- Defense Bonus: +3 (from TemporalPrescience)
- Legend Value: 45 (includes +15% for trait complexity)

**AI Archetype**: Tactical (uses movement to maximize stress application)

**AI Behavior Pattern**:
```
Phase 1 (HP > 50%):
- 40% Move to maximize ChronoDistortion coverage, then attack
- 30% Attack highest-threat target
- 30% RandomBlink triggers (involuntary)

Phase 2 (HP ≤ 50%):
- 50% Focus attacks on lowest-WILL target
- 30% Defensive movement (maximize distance while applying stress)
- 20% RandomBlink triggers
```

**Counter-Play**:
- **Stun/Root**: Prevents movement, negating ChronoDistortion
- **High WILL builds**: Resist stress accumulation
- **Burst damage**: End fight quickly before stress accumulates
- **AoE attacks**: RandomBlink makes single-target unreliable

**Encounter Design Notes**:
- Solo: Manageable stress application (~15-25 total Stress over 5-turn fight)
- Pair: Stress doubles; consider pairing with non-Forlorn enemy for variety
- Avoid: 3+ Chrono-Striders (stress becomes overwhelming)

---

### Rust-Phantom

**Concept**: A Symbiotic Plate colony that has consumed its host entirely, leaving only a corroded humanoid shell animated by malevolent machine intelligence. It phases between solid and incorporeal states, spreading corruption through mere proximity.

**Lore**: When Symbiotic Plate achieves total host integration, the biological matter decays while the machine consciousness persists. Rust-Phantoms are the final stage—neither living nor truly machine, existing in a liminal state between material and void.

**Traits**:
| Trait | Effect | Tactical Implication |
|-------|--------|---------------------|
| **Phasing** | Moves through occupied tiles and obstacles | Ignores formation; can reach backline |
| **BlightAura** | +2 Corruption/turn within 2 tiles | Corruption pressure in prolonged fights |
| **CorrosiveTouch** | Melee attacks inflict 1 Corroded stack | Armor degradation over time |
| **FadingForm** | 25% chance to negate physical damage (incorporeal flicker) | Unreliable physical attacks |

**Base Stats** (High Tier):
- HP: 55
- Damage: 2d6+2 (Corroding Strike - Physical + Corroded application)
- Attributes: MIGHT 3, FINESSE 3, WITS 2, WILL 4, STURDINESS 2
- Soak: 0 (incorporeal, doesn't need armor)
- Legend Value: 60

**AI Archetype**: Aggressive (prioritizes melee engagement for CorrosiveTouch)

**Counter-Play**:
- **Psychic/Energy damage**: Bypasses FadingForm (only negates physical)
- **Keep distance**: BlightAura has limited range
- **Kill quickly**: Prevents Corruption accumulation
- **Armor-piercing not needed**: Low Soak, but FadingForm compensates

---

### Siege-Construct

**Concept**: A massive Haugbui-Class construction automaton repurposed for warfare. Slow but devastating, it anchors positions and methodically destroys anything in its path.

**Lore**: Originally designed to demolish unstable structures in the lower decks, Siege-Constructs have had their targeting parameters corrupted to include "organic obstacles." Their Iron Hearts still burn with Pre-Glitch power, making them nearly unstoppable once engaged.

**Traits**:
| Trait | Effect | Tactical Implication |
|-------|--------|---------------------|
| **IronHeart** | Immune to Bleeding, Poison; 50% Psychic resistance | Status effects limited; physical damage preferred |
| **ArmoredPlating** | +4 Soak | High sustained damage needed |
| **Anchored** | Cannot be moved; immune to knockback | Positioning permanent; plan accordingly |
| **PowerSurge** | On death, 2d6 Lightning to adjacent tiles | Don't cluster when killing |

**Base Stats** (Lethal Tier):
- HP: 85
- Damage: 3d6+2 (Demolition Strike - Physical, Sweeping)
- Attributes: MIGHT 6, FINESSE 1, WITS 0, WILL 0, STURDINESS 6
- Soak: 4 (from ArmoredPlating)
- Legend Value: 90

**AI Archetype**: Defensive (holds position, punishes approach)

**Counter-Play**:
- **Kiting**: Anchored means it can't pursue; ranged attacks from safety
- **Armor-piercing**: Essential for reasonable TTK
- **DoT effects**: Bleeding/Poison don't work, but Corroded does
- **Spread on death**: Avoid PowerSurge damage

---

### Grief-Feeder

**Concept**: A Forlorn entity that has evolved to sustain itself on psychic anguish. It cultivates despair in its prey, feeding on the resulting stress to fuel its own regeneration.

**Lore**: Some Forlorn entities discover they can metabolize the psychic energy released by traumatized minds. Grief-Feeders seek out settlements and isolated travelers, tormenting them for days before finally consuming their broken psyches entirely.

**Traits**:
| Trait | Effect | Tactical Implication |
|-------|--------|---------------------|
| **ForlornAura** | +4 Stress/turn within 3 tiles | High stress pressure |
| **ThoughtLeech** | Heals HP equal to Stress inflicted | Self-sustaining in prolonged fights |
| **MindSpike** | Attacks deal Psychic damage (ignores Soak) | Armor doesn't help |
| **Whispers** | Lowest-WILL character gains +3 Stress at round end | Targets weak-willed |

**Base Stats** (High Tier):
- HP: 50
- Damage: 2d6 (Mind Spike - Psychic)
- Attributes: MIGHT 1, FINESSE 3, WITS 4, WILL 6, STURDINESS 2
- Defense Bonus: +2
- Legend Value: 70 (includes Forlorn bonus)
- IsForlorn: true

**AI Archetype**: Control (maximizes stress application over direct damage)

**Counter-Play**:
- **High WILL party**: Reduces stress intake, limiting healing
- **Burst damage**: Kill before ThoughtLeech sustains it
- **Silence effects**: Prevents psychic abilities
- **Focus fire**: Don't let it sustain through multiple targets

---

### Swarm-Mother

**Concept**: A central node in a Servitor Swarm network, this entity coordinates and empowers lesser machines while spawning replacements for destroyed units.

**Lore**: Pre-Glitch maintenance systems used hierarchical control structures. Swarm-Mothers are corrupted master nodes that have begun treating all non-networked entities as contamination to be purged. They endlessly produce lesser servitors from scavenged materials.

**Traits**:
| Trait | Effect | Tactical Implication |
|-------|--------|---------------------|
| **Networked** | +1 Accuracy per allied Mechanical creature within 3 tiles | Dangerous with minions; kill adds first |
| **SplitOnDeath** | Spawns 2 Corrupted Servitors (15 HP each) on death | Fight continues after "kill" |
| **EmergencyProtocol** | At <25% HP, +2 Defense and +1 damage | Becomes harder to finish |
| **SelfRepair** | Heals 5 HP/turn if didn't attack | Punishes passive play |

**Base Stats** (High Tier):
- HP: 65
- Damage: 2d6 (Coordinator Strike - targets marked enemy for +2 ally Accuracy)
- Attributes: MIGHT 3, FINESSE 2, WITS 4, WILL 1, STURDINESS 4
- Defense Bonus: +1
- Legend Value: 75

**AI Archetype**: Support (buffs allies, sustains self)

**Counter-Play**:
- **Kill adds first**: Removes Networked bonus
- **Sustained pressure**: Prevents SelfRepair healing
- **Prepare for spawns**: Have resources for 2 Servitors after kill
- **AoE**: Efficient against spawned adds

---

## Implementation Architecture

### Data Structures

#### CreatureTrait Enum

```csharp
public enum CreatureTrait
{
    // Temporal Traits (100-199)
    TemporalPhase = 100,
    TemporalPrescience = 101,
    ChronoDistortion = 102,
    RandomBlink = 103,
    TimeLoop = 104,
    CausalEcho = 105,
    TemporalStasis = 106,
    Rewind = 107,

    // Corruption Traits (200-299)
    BlightAura = 200,
    CorrosiveTouch = 201,
    DataFragment = 202,
    Glitched = 203,
    Infectious = 204,
    RealityTear = 205,
    Reforming = 206,
    VoidTouched = 207,

    // Mechanical Traits (300-399)
    IronHeart = 300,
    ArmoredPlating = 301,
    Overcharge = 302,
    EmergencyProtocol = 303,
    ModularConstruction = 304,
    SelfRepair = 305,
    PowerSurge = 306,
    Networked = 307,

    // Psychic Traits (400-499)
    ForlornAura = 400,
    MindSpike = 401,
    MemoryDrain = 402,
    PsychicScream = 403,
    FearAura = 404,
    ThoughtLeech = 405,
    Hallucination = 406,
    Whispers = 407,

    // Mobility Traits (500-599)
    Flight = 500,
    Burrowing = 501,
    Phasing = 502,
    Swiftness = 503,
    Anchored = 504,
    Ambush = 505,
    HitAndRun = 506,
    Territorial = 507,

    // Defensive Traits (600-699)
    Regeneration = 600,
    DamageThreshold = 601,
    Reflective = 602,
    ShieldGenerator = 603,
    LastStand = 604,
    AdaptiveArmor = 605,
    Camouflage = 606,
    Unstoppable = 607,

    // Offensive Traits (700-799)
    Brutal = 700,
    Relentless = 701,
    Executioner = 702,
    ArmorPiercing = 703,
    Reach = 704,
    Sweeping = 705,
    Enrage = 706,
    PredatorInstinct = 707,

    // Unique Traits (800-899)
    SplitOnDeath = 800,
    MirrorImage = 801,
    Vampiric = 802,
    Explosive = 803,
    Resurrection = 804,
    SymbioticLink = 805,
    Berserk = 806,
    PackTactics = 807,

    // Resistance Traits (900-999)
    FireResistant = 900,
    ColdResistant = 901,
    LightningResistant = 902,
    PsychicResistant = 903,
    PhysicalResistant = 904,
    AcidResistant = 905,
    FireVulnerable = 906,
    ColdVulnerable = 907,
    LightningVulnerable = 908,
    HolyVulnerable = 909,
    ElementalAbsorption = 910,
    AlloyedForm = 911,
    OrganicBane = 912,
    MechanicalBane = 913,

    // Strategy/AI Behavior Traits (1000-1099)
    Cowardly = 1000,
    Skittish = 1001,
    HealerHunter = 1002,
    ThreatFocused = 1003,
    WeaknessSensor = 1004,
    ProtectorInstinct = 1005,
    Opportunist = 1006,
    CasterKiller = 1007,
    BerserkerAI = 1008,  // Note: Different from Berserk (806) which is HP-triggered
    PackLeader = 1009,
    AmbusherAI = 1010,   // Note: Different from Ambush (505) which is stealth-based
    TerritorialAI = 1011, // Note: Different from Territorial (507) which is stat-based
    HitAndFade = 1012,
    Swarmer = 1013,
    SelfDestructive = 1014,
    Cautious = 1015,

    // Sensory Traits (1100-1199)
    Blind = 1100,
    Blindsense = 1101,
    SoundSensitive = 1102,
    SoundBlind = 1103,
    Tremorsense = 1104,
    ThermalVision = 1105,
    MotionDetection = 1106,
    Darkvision = 1107,
    LightSensitive = 1108,
    PsychicSense = 1109,
    Eyeless = 1110,
    ScatterSense = 1111,

    // Combat Condition Traits (1200-1299)
    Amphibious = 1200,
    HeatAdapted = 1201,
    ColdAdapted = 1202,
    ToxinImmune = 1203,
    RadiationImmune = 1204,
    VacuumSurvival = 1205,
    GravityAdapted = 1206,
    Buoyant = 1207,
    Grounded = 1208,
    Ethereal = 1209,
    SunlightWeakness = 1210,
    MoonlightStrength = 1211,
    StormBorn = 1212,
    BloodScent = 1213,
    CorruptionSustained = 1214,
    HallowedBane = 1215
}
```

#### TraitConfiguration Class

```csharp
public class TraitConfiguration
{
    public CreatureTrait Trait { get; set; }

    // Numeric parameters (trait-specific interpretation)
    public int PrimaryValue { get; set; }      // e.g., +3 Evasion, 5 HP/turn
    public int SecondaryValue { get; set; }    // e.g., range, duration
    public float Percentage { get; set; }       // e.g., 25% chance, 50% reduction

    // Trait-specific metadata
    public Dictionary<string, object> Metadata { get; set; }
}
```

#### Enemy Class Extension

```csharp
public class Enemy
{
    // ... existing properties ...

    /// <summary>
    /// Collection of creature traits that modify this enemy's behavior and mechanics.
    /// Traits are processed by CreatureTraitService during combat.
    /// </summary>
    public List<TraitConfiguration> Traits { get; set; } = new();

    /// <summary>
    /// Quick lookup for trait presence (computed from Traits list).
    /// </summary>
    public bool HasTrait(CreatureTrait trait) =>
        Traits.Any(t => t.Trait == trait);

    /// <summary>
    /// Get configuration for a specific trait.
    /// </summary>
    public TraitConfiguration? GetTraitConfig(CreatureTrait trait) =>
        Traits.FirstOrDefault(t => t.Trait == trait);
}
```

### Service Architecture

#### ICreatureTraitService

```csharp
public interface ICreatureTraitService
{
    // Combat event hooks
    void OnCombatStart(Enemy enemy, CombatState state);
    void OnTurnStart(Enemy enemy, CombatState state);
    void OnTurnEnd(Enemy enemy, CombatState state);
    void OnMovement(Enemy enemy, GridPosition from, GridPosition to, CombatState state);
    void OnAttackAttempt(Enemy attacker, ITarget target, ref AttackContext context);
    void OnAttackReceived(Enemy defender, IAttacker attacker, ref DamageContext context);
    void OnDeath(Enemy enemy, CombatState state);

    // Stat modifiers
    int GetEvasionModifier(Enemy enemy);
    int GetAccuracyModifier(Enemy enemy, ITarget target, CombatState state);
    int GetDamageModifier(Enemy enemy, ITarget target);
    int GetSoakModifier(Enemy enemy);

    // Movement modifiers
    bool IgnoresAttacksOfOpportunity(Enemy enemy);
    bool CanMoveThrough(Enemy enemy, GridPosition position, CombatState state);
    int GetMovementBonus(Enemy enemy);

    // Status immunities
    bool IsImmuneToStatus(Enemy enemy, StatusEffectType status);
    float GetDamageTypeResistance(Enemy enemy, DamageType damageType);
}
```

#### Integration Points

**CombatEngine Integration**:
```csharp
// In CombatEngine.ProcessMovement()
if (!_traitService.IgnoresAttacksOfOpportunity(enemy))
{
    ProcessAttacksOfOpportunity(enemy, path);
}

// In CombatEngine.ProcessAttack()
var evasionBonus = _traitService.GetEvasionModifier(target as Enemy);
defensePool += evasionBonus;
```

**TraumaEconomyService Integration**:
```csharp
// In TraumaEconomyService.ProcessTurnStress()
foreach (var enemy in combatState.Enemies.Where(e => e.HasTrait(CreatureTrait.ChronoDistortion)))
{
    if (enemy.MovedThisTurn)
    {
        var config = enemy.GetTraitConfig(CreatureTrait.ChronoDistortion);
        int stressAmount = config?.PrimaryValue ?? 3;
        int range = config?.SecondaryValue ?? 2;

        ApplyStressInRadius(enemy.Position, range, stressAmount, combatState);
    }
}
```

**EnemyAI Integration**:
```csharp
// In EnemyAI.DetermineAction()
if (enemy.HasTrait(CreatureTrait.RandomBlink))
{
    // 30% chance to involuntarily blink
    if (_random.Next(100) < 30)
    {
        return new BlinkAction(GetRandomValidPosition(enemy, combatState));
    }
}
```

---

## Balance Guidelines

### Trait Point Budget

Each creature has a **Trait Point Budget** based on its threat tier:

| Threat Tier | Trait Point Budget | Max Traits | Example |
|-------------|-------------------|------------|---------|
| Low | 4-6 points | 2 traits | Scrap-Hound: Swiftness (2) + HitAndRun (2) |
| Medium | 6-10 points | 3 traits | Chrono-Strider: TemporalPhase (3) + ChronoDistortion (3) + TemporalPrescience (2) |
| High | 10-14 points | 4 traits | Rust-Phantom: Phasing (2) + BlightAura (4) + CorrosiveTouch (3) + FadingForm (3) |
| Lethal | 14-18 points | 4-5 traits | Siege-Construct: IronHeart (4) + ArmoredPlating (4) + Anchored (2) + PowerSurge (3) |
| Boss | 18-25 points | 5+ traits | Phase-based trait activation |

### Trait Point Costs

| Trait | Point Cost | Justification |
|-------|------------|---------------|
| **High Impact** (5+ points) | | |
| ForlornAura (4 Stress, 3 range) | 6 | Major stress pressure |
| Regeneration (10 HP/turn) | 5 | Significantly extends TTK |
| Relentless | 5 | Doubles damage output |
| **Medium Impact** (3-4 points) | | |
| TemporalPhase | 4 | Negates positioning strategy |
| BlightAura | 4 | Corruption pressure |
| ArmoredPlating (+4 Soak) | 4 | +60% effective HP |
| IronHeart | 4 | Multiple immunities |
| **Low Impact** (1-2 points) | | |
| Swiftness | 2 | Positioning advantage |
| HitAndRun | 2 | Tactical flexibility |
| Territorial | 2 | Conditional bonus |
| TemporalPrescience (+2) | 2 | Moderate evasion |

### Legend Value Adjustment

Creatures with traits should have adjusted Legend values:

```
Adjusted_Legend = Base_Legend × (1 + (Total_Trait_Points × 0.03))

Example: Chrono-Strider
- Base Legend (Medium, 35 HP): 35
- Trait Points: 8 (TemporalPhase 3 + ChronoDistortion 3 + TemporalPrescience 2)
- Adjustment: 35 × (1 + 0.24) = 43.4 ≈ 45 Legend
```

---

## Testing Requirements

### Unit Tests

Each trait requires:
1. **Activation test**: Trait effect triggers under correct conditions
2. **Value test**: Numeric effects match configuration
3. **Interaction test**: Trait works with combat systems (damage, movement, status)
4. **Edge case test**: Behavior when trait conditions can't be met

### Integration Tests

Trait combinations require:
1. **Synergy test**: Combined traits produce expected emergent behavior
2. **Conflict test**: Contradictory traits resolve correctly
3. **Performance test**: Multiple trait-bearing creatures don't cause slowdown

### Balance Tests

New creatures require:
1. **TTK validation**: Time-to-kill within tier expectations (±20%)
2. **Stress validation**: Total stress infliction within budget (15-50 for Medium tier)
3. **Counter-play validation**: At least 2 viable counter-strategies exist
4. **Fun validation**: Playtest confirms engaging (not frustrating) combat

---

## Appendix A: Trait Quick Reference

### By Tactical Role

**Anti-Positioning**: TemporalPhase, Phasing, Flight, Burrowing, RandomBlink, Ethereal
**Stress Applicators**: ChronoDistortion, ForlornAura, Whispers, FearAura, DataFragment
**Damage Mitigation**: ArmoredPlating, Regeneration, DamageThreshold, ShieldGenerator, FadingForm, PhysicalResistant
**Damage Amplification**: Brutal, Relentless, Executioner, Enrage, Overcharge, OrganicBane, MechanicalBane
**Status Immunity**: IronHeart, Unstoppable, VoidTouched, ToxinImmune, RadiationImmune
**Sustain**: SelfRepair, ThoughtLeech, Vampiric, Reforming, ElementalAbsorption, CorruptionSustained
**Spawning**: SplitOnDeath, MirrorImage, Resurrection
**Target Priority**: HealerHunter, ThreatFocused, WeaknessSensor, CasterKiller, Opportunist
**Self-Preservation**: Cowardly, Skittish, Cautious, HitAndFade
**Detection/Sensing**: Blindsense, Tremorsense, ThermalVision, PsychicSense, BloodScent, MotionDetection
**Environmental**: HeatAdapted, ColdAdapted, Amphibious, StormBorn, SunlightWeakness

### By Counter-Play

**Requires Burst Damage**: Regeneration, ThoughtLeech, Reforming, TimeLoop, CorruptionSustained
**Requires Status Effects**: Unstoppable (can't be CC'd), IronHeart (needs alternatives)
**Requires High WILL**: ForlornAura, ChronoDistortion, Whispers, FearAura
**Requires Positioning**: Territorial, Anchored, Networked, TerritorialAI
**Requires AoE**: RandomBlink, MirrorImage, SplitOnDeath, ScatterSense
**Requires Armor-Piercing**: ArmoredPlating, AdaptiveArmor, PhysicalResistant, AlloyedForm
**Requires Specific Damage Type**: FireResistant (use non-Fire), ColdResistant (use non-Cold), etc.
**Exploit Sensory Weakness**: SoundSensitive (use loud abilities), LightSensitive (use flash), Blind (stay still)
**Exploit Environmental**: SunlightWeakness (fight in daylight), HallowedBane (use holy ground)
**Kite/Distance**: Cowardly (let them flee), HitAndFade (close gap), Territorial (leave their zone)

### By Damage Type Interaction

**Fire**:
- Resistant: FireResistant, HeatAdapted
- Vulnerable: FireVulnerable, ColdAdapted (thematically)
- Absorbs: ElementalAbsorption (Fire)

**Cold**:
- Resistant: ColdResistant, ColdAdapted
- Vulnerable: ColdVulnerable, HeatAdapted (thematically)
- Absorbs: ElementalAbsorption (Cold)

**Lightning**:
- Resistant: LightningResistant, Grounded
- Vulnerable: LightningVulnerable, AlloyedForm
- Absorbs: ElementalAbsorption (Lightning), StormBorn

**Psychic**:
- Resistant: PsychicResistant, IronHeart (50%)
- Vulnerable: Ethereal (-2 vs psychic)
- Absorbs: VoidTouched (immune)

**Physical**:
- Resistant: PhysicalResistant, ArmoredPlating, AlloyedForm
- Vulnerable: Ethereal (vs holy)
- Mitigates: Soak, DamageThreshold

**Holy/Radiant**:
- Vulnerable: HolyVulnerable, HallowedBane, Ethereal
- Resistant: (none by default - holy is meant to be effective vs corrupted)

### By AI Behavior Pattern

**Aggressive (Attack-Focused)**:
- BerserkerAI, ThreatFocused, Swarmer, SelfDestructive

**Defensive (Protection-Focused)**:
- ProtectorInstinct, Cautious, TerritorialAI, Anchored

**Opportunistic (Exploit Weakness)**:
- WeaknessSensor, Opportunist, Executioner, BloodScent

**Assassination (Priority Targeting)**:
- HealerHunter, CasterKiller, PredatorInstinct

**Evasive (Avoid Damage)**:
- Cowardly, Skittish, HitAndFade, Cautious

**Ambush (Wait and Strike)**:
- AmbusherAI, Ambush, MotionDetection

---

## Appendix B: Future Trait Ideas

Reserved for future implementation:

### Environmental Interaction
- **PyroAbsorption**: Heals from Fire damage; immune to burning terrain
- **CryoAura**: Creates difficult terrain in adjacent tiles
- **ElectricConduction**: Lightning damage chains to nearby enemies

### Advanced AI Behaviors
- **Tactical Retreat**: Flees at <25% HP; returns with reinforcements
- **Sacrifice**: Can kill allied creature to fully heal
- **Mimicry**: Copies last ability used by player

### Boss-Specific
- **PhaseImmunity**: Immune to damage until phase trigger
- **RaidBoss**: Requires multiple simultaneous attacks to damage
- **Mythic**: Cannot be killed; must be sealed/banished

---

**End of Specification**

---

*For trait implementation*: See `RuneAndRust.Engine/CreatureTraitService.cs`
*For creature examples*: See creature bestiary (this document, Example Creatures section)
*For balance testing*: See `/docs/templates/trait-balance-worksheet.md` (pending)
