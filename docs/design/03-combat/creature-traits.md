---
id: SPEC-COMBAT-015
title: "Creature Traits System"
version: 1.0.0
status: Draft
last-updated: 2025-12-10
related-files:
  - path: "RuneAndRust.Engine/CreatureTraitService.cs"
    status: Planned
  - path: "RuneAndRust.Core/CreatureTrait.cs"
    status: Planned
---

# Creature Traits System

> "The flesh is weak, but it is mutable. The steel is strong, but it can rust. In the glitch, both become something... else."

---

## 1. Overview

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-COMBAT-015` |
| Category | Combat, Enemy Design |
| Priority | High |
| Status | Draft |
| Domain | Combat, Enemy Design |

### 1.2 Core Philosophy

The **Creature Traits System** allows for the creation of unique, modular creatures by combining reusable behaviors and mechanics. Rather than hardcoding every enemy, we compose them from traits that align with Aethelgard's lore pillars: Decay, Nordic Mythology, Body Horror, and Technological Corruption.

**Design Pillars:**
1.  **Composability Over Inheritance**: Uniqueness emerges from trait combinations (e.g., `[TemporalPhase, ChronoDistortion]`).
2.  **Thematic Grounding**: Every trait has a lore origin (Glitch anomalies, Runic Blight, Mechanical decay).
3.  **Tactical Clarity**: Effects must be observable and have clear counter-play.
4.  **Bounded Complexity**: Traits do one thing well; complexity comes from the mix, not the individual trait.

---

## 2. Player Experience

### 2.1 Interaction

Players identify traits through visual motifs (flickering for Temporal, rust for Mechanical) and combat feedback. Understanding enemy traits is key to formulating a counter-strategy (e.g., "Don't bunch up against an Explosive enemy").

### 2.2 Key Features

-   **12 Thematic Categories**: Including Temporal, Corruption, Mechanical, Psychic, and Mobility.
-   **Emergent Gameplay**: Traits interact to create unique challenges (e.g., a "Teleporting" + "Explosive" enemy).
-   **Visual/Audio Cues**: Each trait has distinct feedback to signal its presence.

---

## 3. Trait Categories

### 3.1 Category 1: Temporal Traits (Glitch Anomalies)
*Motif: Flickering, afterimages, clock symbols.*

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

### 3.2 Category 2: Corruption Traits (Runic Blight)
*Motif: Glitching textures, static, red/black tendrils.*

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

### 3.3 Category 3: Mechanical Traits (Pre-Glitch Automation)
*Motif: Gears, pistons, rusted metal.*

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

### 3.4 Category 4: Psychic Traits (Forlorn Corruption)
*Motif: Distortion waves, screaming faces, purple energy.*

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

### 3.5 Category 5: Mobility Traits
*Motif: Flight, phasing, burrowing.*

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

### 3.6 Category 6: Defensive Traits
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

### 3.7 Category 7: Offensive Traits
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

### 3.8 Category 8: Unique/Exotic Traits
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

### 3.9 Category 9: Resistance Traits
*Theme: Damage type resistances, elemental affinities, vulnerability exploitation.*

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

### 3.10 Category 10: Strategy/AI Behavior Traits
*Theme: Tactical decision-making patterns, target prioritization.*

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

### 3.11 Category 11: Sensory Traits
*Theme: Perception abilities, detection methods, sensory limitations.*

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

### 3.12 Category 12: Combat Condition Traits
*Theme: Environmental adaptations, condition immunities, situational modifiers.*

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

## 4. Trait Interactions

### 4.1 Synergies
-   **TemporalPhase + ChronoDistortion**: Mobile stress applicator that is hard to pin down.
-   **ForlornAura + ThoughtLeech**: Self-sustaining through the stress it inflicts.
-   **Flight + Hit-and-Run**: Untouchable harasser.
-   **Blind + Tremorsense + SoundSensitive**: Hunts by vibration/sound; exploitable weakness.
-   **BloodScent + WeaknessSensor + Executioner**: Hunts wounded prey with lethal efficiency.

### 4.2 Conflicts (Avoid)
-   **Anchored + RandomBlink**: Contradictory movement logic.
-   **IronHeart + Vampiric**: Thematic clash (Machine vs. Life-drain).
-   **Camouflage + ForlornAura**: Hard to see vs obvious psychic presence.

### 4.3 Rules
1.  **Immunity Trumps Infliction**: Can't self-inflict ignored effects.
2.  **Numeric Stacking**: Bonuses stack additively.
3.  **Percentage Stacking**: Multiplicative.
4.  **Once-per-combat effects track independently**: TimeLoop and Rewind each get their own use.

---

## 5. Example Creatures

### Chrono-Strider (Medium Tier)
*Concept: Temporal anomaly.*
-   **Traits**: `TemporalPhase`, `TemporalPrescience`, `ChronoDistortion`, `RandomBlink`.
-   **Tactics**: Moves to maximize stress aura, hard to hit, blinks randomly.

### Rust-Phantom (High Tier)
*Concept: Symbiotic Plate colony ghost.*
-   **Traits**: `Phasing`, `BlightAura`, `CorrosiveTouch`, `Ethereal`.
-   **Tactics**: Phases through walls to spread corruption; incorporeal defense.

### Siege-Construct (Lethal Tier)
*Concept: Heavy automaton.*
-   **Traits**: `IronHeart`, `ArmoredPlating`, `Anchored`, `PowerSurge`.
-   **Tactics**: Immovable object, explodes on death.

### Grief-Feeder (High Tier)
*Concept: Forlorn psychic predator.*
-   **Traits**: `ForlornAura`, `ThoughtLeech`, `MindSpike`, `Whispers`.
-   **Tactics**: Sustains itself on stress inflicted; bypasses armor.

### Swarm-Mother (High Tier)
*Concept: Central node.*
-   **Traits**: `Networked`, `SplitOnDeath`, `EmergencyProtocol`, `SelfRepair`.
-   **Tactics**: Buffs allies, spawns adds, becomes harder to kill when low.

---

## 6. Technical Implementation

### 6.1 Data Model
```csharp
public enum CreatureTraitType { TemporalPhase, BlightAura, IronHeart, ... } // 100+ entries

public class TraitConfiguration {
    public CreatureTraitType Trait;
    public int PrimaryValue; // e.g., +3 or Range 2
    public int SecondaryValue; // e.g., Duration 3
}

public class EnemyTraits {
    public List<TraitConfiguration> Traits = new();
    public bool HasTrait(CreatureTraitType t) => Traits.Any(tr => tr.Trait == t);
}
```

### 6.2 Service Interface
```csharp
public interface ICreatureTraitService {
    // Hooks
    void OnCombatStart(Enemy enemy, CombatState state);
    void OnTurnStart(Enemy enemy, CombatState state);
    void OnAttackAttempt(Enemy attacker, CombatParticipant target, AttackResult result);
    void OnDefense(Enemy defender, CombatParticipant attacker, DamageResult result);
    
    // Modifiers
    int GetEvasionModifier(Enemy enemy);
    int GetSoakModifier(Enemy enemy, DamageType type);
    bool CheckImmunities(Enemy enemy, StatusEffect effect);
}
```

---

## 7. Phased Implementation Guide

### Phase 1: Core System
- [ ] **Registry**: Implement `CreatureTraitType` enum and `TraitConfiguration` class.
- [ ] **Service**: Create `TraitService` and basic hooks (`OnTurnStart`).
- [ ] **Stat Traits**: Implement simple stat modifiers (e.g., `ArmoredPlating` -> Soak +2).

### Phase 2: Combat Flow Hooks
- [ ] **OnAttack**: Implement offensive traits (`CorrosiveTouch`, `Executioner`).
- [ ] **OnDefend**: Implement defensive traits (`Reflective`, `TemporalStasis`).
- [ ] **OnDeath**: Implement death triggers (`Explosive`, `SplitOnDeath`).

### Phase 3: Complex Behaviours
- [ ] **Auras**: Implement Turn End/Start proximity checks (`BlightAura`).
- [ ] **Movement**: Implement `TemporalPhase` (ignore AoO) and `Burrowing`.
- [ ] **AI Hooks**: Ensure `HealerHunter` influences AI Target Selection.

### Phase 4: UI & Feedback
- [ ] **Icons**: Trait icons on Enemy Unit Frames.
- [ ] **Tooltips**: Hover over enemy shows known traits (Lore check?).
- [ ] **Visuals**: Corruption fx for BlightAura, Flicker fx for Temporal.

---

## 8. Balance Guidelines

### 8.1 Point Budget
-   **Low**: 4-6 points (2 traits)
-   **Medium**: 6-10 points (3 traits)
-   **High**: 10-14 points (4 traits)
-   **Lethal/Boss**: 14+ points (4-5+ traits)

### 8.2 Cost Examples
-   **High Impact (5-6)**: ForlornAura, Regeneration, Relentless.
-   **Medium Impact (3-4)**: TemporalPhase, BlightAura, ArmoredPlating.
-   **Low Impact (1-2)**: Swiftness, HitAndRun.

---

## 9. Voice Guidance

**Reference:** [npc-flavor.md](../../.templates/flavor-text/npc-flavor.md)

### 9.1 System Tone
**Layer 2 Diagnostic Voice**: Clinical, suspicious, analyzing deviations.

### 9.2 Feedback Text Examples
- **Temporal**: "Chronometric instability detected. Target is... shifting between moments."
- **Corruption**: "Alert: Code-rot present. Target exhibits signs of Blight infection."
- **Mechanical**: "Ancient systems active. Iron Heart output at 120%."

---

## 10. Testing Requirements

### 10.1 Unit Tests
- [ ] **Stat Mod**: ArmoredPlating +3 -> GetSoak returns +3.
- [ ] **Immunity**: IronHeart vs Poison -> IsImmune returns true.
- [ ] **Trigger**: Explosive on Death -> Calls Damage Service.
- [ ] **Stacking**: 2x Traits with +2 Accuracy -> +4 Total (or non-stacking if rule).

### 10.2 Integration Tests
- [ ] **Aura**: End turn near BlightAura -> Corruption increases.
- [ ] **Reflect**: Attack Reflective Enemy -> Attacker takes 20% dmg.
- [ ] **Split**: Kill Splitter -> 2 Spawns appear at valid tiles.

### 10.3 Manual QA
- [ ] **Visual**: Flickering effect on Temporal enemies.
- [ ] **Log**: "Explosive trait detonated dealing 12 dmg!"

---

## 11. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 11.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Trait Trigger | Info | "{Enemy}'s {Trait} activated: {Effect}." | `Enemy`, `Trait`, `Effect` |
| Trait Resist | Info | "{Enemy}'s {Trait} prevented {Status}." | `Enemy`, `Trait`, `Status` |
| Aura Tick | Debug | "{Trait} aura affected {Count} targets." | `Trait`, `Count` |

---

## 12. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-COMBAT-012` | [Enemy Design System](enemy-design.md) — Uses traits for enemy definitions |
| `SPEC-COMBAT-016` | [Encounter Generation System](encounter-generation.md) — Trait assignment during spawning |
| `SPEC-COMBAT-019` | [Elite & Champion Mechanics](elite-mechanics.md) — Champion affixes draw from trait pool |
| `SPEC-AI-001` | Enemy AI System — Trait-driven behavior patterns |
| `SPEC-COMBAT-001` | Combat Resolution — Trait effects in combat |

---

## 13. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-12-10 | Initial specification |
