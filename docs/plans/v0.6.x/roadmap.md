# Milestone 7 Roadmap: The Adversary (v0.6.x)

> **Status:** Planning
> **Version Range:** v0.6.0 - v0.6.15
> **Theme:** Advanced Combat, AI, and Tactics

## Overview
Milestone 7 transforms combat from a simple exchange of blows into a tactical wargame. "The Adversary" introduces intelligent enemies that use squad tactics, legendary boss encounters with multi-phase mechanics, and the ability for the player to lead their own warband.

This cycle targets **16 versions** (v0.6.0 - v0.6.15) to cover the immense complexity of AI behavior trees and squad management.

---

## Part I: The Mind (AI & Tactics)
**Focus:** Making enemies smarter and more dangerous.

### [v0.6.0] The Trait System
*Procedural complexity.*
- **v0.6.0a: The Mutation (Affix)**
  - `CreatureTraitService` for applying prefixes/suffixes (e.g., "Burning", "Shielded").
  - Visual indicators for elite enemies.
- **v0.6.0b: The Adaptation (Resistance)**
  - Dynamic resistance building (e.g., enemy gains fire res after taking fire dmg).
  - Weakness exploitation logic.

### [v0.6.1] The Squad
*Group dynamics.*
- **v0.6.1a: The Formation (Positioning)**
  - Enemies preferring specific relative positions (Tank front, Healer back).
  - "Rally" behavior to regroup.
- **v0.6.1b: The Synergy (Combo)**
  - Enemies triggering combos off each other (e.g., Oil Pot + Fire Arrow).
  - "Help" call logic.

### [v0.6.2] Advanced Behavior
*Beyond attack and move.*
- **v0.6.2a: The Tree (AI)**
  - Implementation of Behavior Trees for complex decision making.
  - States: Patrol, Investigate, Hunt, Flee.
- **v0.6.2b: The Role (Archetypes)**
  - Specific AI profiles: Skirmisher (Hit & Run), Controller (Debuff), Support (Buff/Heal).

### [v0.6.3] The Environment
*Using the battlefield.*
- **v0.6.3a: The Cover (Mechanic)**
  - Light/Heavy cover bonuses to AC/Reflex.
  - Destructible terrain.
- **v0.6.3b: The Hazard (Interaction)**
  - AI pushing players into traps/pits.
  - Enemies using high ground.

### [v0.6.4] The Hunter
*Predatory AI.*
- **v0.6.4a: The Stalk (Behavior)**
  - Enemies tracking player scent/noise across rooms.
  - Ambush setup logic.
- **v0.6.4b: The Nemesis (Persistence)**
  - Enemies that escape returning later with better gear.
  - "Grudge" tracking.

---

## Part II: The Titan (Bosses)
**Focus:** Cinematic, multi-stage encounters.

### [v0.6.5] The Legend
*Boss framework.*
- **v0.6.5a: The Health Bar (UI)**
  - Multi-segment boss HP bars (Phase indicators).
  - Boss music triggers.
- **v0.6.5b: The Action (Legendary)**
  - "Legendary Actions" taken at end of player turns.
  - Lair Actions on Initiative 20.

### [v0.6.6] The Phase
*Evolving combat.*
- **v0.6.6a: The Transition (Event)**
  - Logic for triggering Phase 2 (e.g., "Armor Breaks", "Enrage").
  - Model/Sprite changes.
- **v0.6.6b: The Pattern (Telegraph)**
  - Complex attack patterns (Cone -> AoE -> Dash).
  - Telegraphing "Ultimate" attacks 1 turn in advance.

### [v0.6.7] The Minion
*Adds and control.*
- **v0.6.7a: The Summon (Spawn)**
  - Bosses spawning reinforcements.
  - Minion sacrifice mechanics (Boss eating minions for HP).
- **v0.6.7b: The Command (Buff)**
  - Bosses buffing all minions in room.
  - "Focus Fire" orders.

### [v0.6.8] The Arena
*Lair mechanics.*
- **v0.6.8a: The Lair (Hazards)**
  - Room-wide effects (e.g., rising lava, falling ceiling).
  - Interactive objects to disable boss shields.
- **v0.6.8b: The Escape (Chase)**
  - Boss chase sequences.
  - Timed objectives.

### [v0.6.9] The Loot
*Rewards of conquest.*
- **v0.6.9a: The Hoard (Drop)**
  - Boss-specific loot tables.
  - "Trophy" items for settlement display.
- **v0.6.9b: The Soul (Crafting)**
  - Boss souls used to craft unique weapons.
  - Unlockable titles.

---

## Part III: The Pack (Companions)
**Focus:** Leading a warband.

### [v0.6.10] The Ally
*Friendly AI.*
- **v0.6.10a: The Recruit (System)**
  - Hiring mercenaries at settlements.
  - Faction-specific companions.
- **v0.6.10b: The Sheet (Stats)**
  - Simplified character sheets for companions.
  - Equipment management for allies.

### [v0.6.11] The Command
*Tactical control.*
- **v0.6.11a: The Order (UI)**
  - Squad Command Interface (Attack, Defend, Move).
  - Setting engagement rules (Aggressive, Passive).
- **v0.6.11b: The Tactic (Formation)**
  - Setting marching order.
  - coordinated breaching.

### [v0.6.12] The Bond
*Social dynamics.*
- **v0.6.12a: The Loyalty (Stat)**
  - Tracking companion loyalty.
  - Desertion risk if unpaid or mistreated.
- **v0.6.12b: The Banter (Flavor)**
  - Inter-party dialogue triggers.
  - Campfire stories.

---

## Part IV: The Shadow (Stealth & Hunt)
**Focus:** Asymmetric warfare.

### [v0.6.13] The Shadow
*Stealth mechanics.*
- **v0.6.13a: The Hide (Action)**
  - Stealth vs Perception checks.
  - Light/Shadow visibility levels.
- **v0.6.13b: The Strike (Critical)**
  - Sneak Attack damage multipliers.
  - Assassination animations.

### [v0.6.14] The Bounty
*Hunting targets.*
- **v0.6.14a: The Contract (Item)**
  - Bounty writs with target details.
  - Tracking mechanics (footprints, witnesses).
- **v0.6.14b: The Capture (Non-Lethal)**
  - Non-lethal takedown weapons (nets, saps).
  - Transporting prisoners.

### [v0.6.15] The Warlord
*Final polish.*
- **v0.6.15a: The Banner (Aura)**
  - Player "Commander" auras buffing allies.
  - Morale system for mass combat.
- **v0.6.15b: The Gauntlet II (Integration)**
  - Full scale battle simulation tests.
  - Performance tuning for high entity counts.
