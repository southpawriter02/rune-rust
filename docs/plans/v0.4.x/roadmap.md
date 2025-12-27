# Milestone 5 Roadmap: The Saga & The Weaver (v0.4.x)

> **Status:** Planning
> **Version Range:** v0.4.0 - v0.4.15
> **Theme:** Progression, Magic, and World Depth

## Overview
Milestone 5 represents the transition of *Rune & Rust* from a survival simulator to a full-fledged RPG. This cycle introduces the two pillars of long-term engagement: **The Saga System** (Character Progression) and **The Weave** (Aetheric Magic).

The version cycle is extended to **v0.4.15** to ensure these complex systems are fully integrated, balanced, and polished before the introduction of Settlements in v0.5.0.

---

## Part I: The Saga (Progression)
**Focus:** Character growth, specialization, and social standing.

### [v0.4.0] The Saga System
*The foundation of leveling and attributes.*
- **v0.4.0a: The Chronicle (Legend Engine)**
  - Core `SagaService` implementation.
  - Legend (XP) accumulation logic.
  - Fibonacci-based threshold calculation (500, 600, 700...).
- **v0.4.0b: The Choice (Spending Logic)**
  - `ProgressionService` for spending PP (Progression Points).
  - Attribute upgrades (Cost: 3 PP).
  - Skill rank upgrades (Cost: 2 PP).
- **v0.4.0c: The Reflection (UI)**
  - "Saga" tab in the Rest Menu (Sanctuary only).
  - Visual display of Attribute growth and next level thresholds.

### [v0.4.1] Specializations
*Defining the character's role through unique ability trees.*
- **v0.4.1a: The Tree (Data Structure)**
  - JSON definitions for Specialization Trees (e.g., `specs/specializations/berserkr.json`).
  - Prerequisite logic (Parent nodes, Attribute gates).
- **v0.4.1b: The Unlock (Acquisition)**
  - Logic for unlocking Specializations via items (Codex fragments) or Trainers.
  - "Capstone" ability logic (Tier 3).
- **v0.4.1c: The Path (Visuals)**
  - Tree visualization in TUI (ASCII branching lines).
  - Tooltips for locked/unlocked abilities.

### [v0.4.2] Factions & Dialogue
*The social context of the world.*
- **v0.4.2a: The Reputation (FactionService)**
  - Tracking disposition (0-100) for key factions (Aesir, Vanir, Jotnar).
  - Reputation tiers: Hated, Suspicious, Neutral, Friendly, Exalted.
- **v0.4.2b: The Voice (Dialogue Engine)**
  - Node-based dialogue parser (JSON).
  - Requirement checks (e.g., `Requires: { "Skill": "Skald", "Rank": 2 }`).
- **v0.4.2c: The Parley (Interaction)**
  - Dialogue UI with numbered responses.
  - Skill-gated choice highlighting.

---

## Part II: The Weaver (Magic)
**Focus:** High-risk, high-reward Aetheric manipulation.

### [v0.4.3] Magic Core
*The engine of Aetheric manipulation.*
- **v0.4.3a: The Aether (Flux Mechanics)**
  - `AetherService` for tracking local Flux levels.
  - Flux accumulation per cast vs. dissipation over time.
- **v0.4.3b: The Weave (Spell Engine)**
  - `Spell` entity definition and `Cast` verb.
  - Target selection for non-combat spells.
- **v0.4.3c: The Backlash (Risk System)**
  - Corruption checks when casting above Flux threshold.
  - "Aether Sickness" status effect implementation.

### [v0.4.4] Spells & Chants
*The arsenal of the mystic.*
- **v0.4.4a: The Chant (Multi-turn Actions)**
  - State machine for multi-turn casting (Preparing -> Chanting -> Unleashed).
  - Interruption logic (taking damage while chanting).
- **v0.4.4b: The Grimoire (Data)**
  - Implementation of Tier 1 Spells (e.g., *Spark*, *Mend*, *Ward*).
  - Spellbook item handling and memory slots.

### [v0.4.5] Runic Inscription
*Permanent magic bound to steel.*
- **v0.4.5a: The Rune (Item Properties)**
  - Extension of `ItemProperty` system to support active effects.
  - Socket system for weapons/armor.
- **v0.4.5b: The Forge (Crafting UI)**
  - "Runeforging" trade interface.
  - Resource requirements (Dust + Sigil + Item).

### [v0.4.6] The Glitch
*When magic breaks reality.*
- **v0.4.6a: The Paradox (Event System)**
  - Procedural events triggered by high Flux/Corruption.
  - Visual glitches (text scrambling) in TUI.
- **v0.4.6b: The Wilds (Random Tables)**
  - Wild Magic surge tables (Good/Bad/Weird effects).
  - Summoning accidents (Hostile entities spawning).

### [v0.4.7] Magic UI
*Visualizing the invisible.*
- **v0.4.7a: The Sight (HUD)**
  - Flux meter display in HUD.
  - Active spell effect indicators.
- **v0.4.7b: The Grimoire (Menu)**
  - Dedicated spell management tab.
  - Rune resonance display (matching runes for bonuses).

---

## Part III: The Echo (Stabilization)
**Focus:** Integration, balancing, and preparing for the world expansion.

### [v0.4.8] The Stabilizer
*Balancing the scales of power.*
- **v0.4.8a: The Audit (Balance Pass)**
  - Tuning XP curves based on simulation.
  - Adjusting Spell Costs vs. Damage output.
- **v0.4.8b: The Fix (Bug Squashing)**
  - Focused sprint on regression testing for Saga/Magic interactions.

### [v0.4.9] The Resonance
*Polishing the experience.*
- **v0.4.9a: The Sound (Audio Triggers)**
  - Hooking up audio events for Level Up, Spell Cast, and Fizzle.
- **v0.4.9b: The Feedback (VFX)**
  - Screen shake intensity tuning for high-power spells.
  - Particle color grading for different magic schools.

### [v0.4.10] The Convergence
*Final integration and golden master for 0.5.0.*
- **v0.4.10a: The Gauntlet (Integration Tests)**
  - Full playthrough automation script (Level 1 to Specialization).
- **v0.4.10b: The Precursor (Settlement Seeds)**
  - Seeding "Dormant" settlement locations for v0.5.0.
  - Migration scripts for save compatibility.

### [v0.4.11] The Library
*Codifying the arcane.*
- **v0.4.11a: The Archive (Content)**
  - Expansion of Codex entries for all Spells and Specializations.
  - Unlocking lore by mastering abilities.
- **v0.4.11b: The Research (Mechanic)**
  - System for deciphering ancient scrolls (Intelligence checks).
  - Learning new spells from items instead of trainers.

### [v0.4.12] The Mentor
*Advanced training.*
- **v0.4.12a: The Master (NPCs)**
  - Legendary trainer NPCs who teach Capstone abilities.
  - Requirement for specific reputation to unlock training.
- **v0.4.12b: The Duel (Test)**
  - 1v1 combat challenges to prove worthiness for Specialization unlocks.

### [v0.4.13] The Artifact
*Items of power.*
- **v0.4.13a: The Relic (Item Type)**
  - Unique legendary items with bound spells.
  - Lore descriptions tied to game history.
- **v0.4.13b: The Curse (Mechanic)**
  - Negative properties on powerful items (e.g., drains Sanity).
  - Purification rituals.

### [v0.4.14] The Balance II
*Fine-tuning for the long haul.*
- **v0.4.14a: The Sim (Data)**
  - Automated combat logs analysis for Level 10+ characters.
  - Nerfing/Buffing outliers in Specialization trees.
- **v0.4.14b: The Polish (UX)**
  - streamlining the level-up process UI.
  - Adding "Build" saving/loading for ease of testing.

### [v0.4.15] The Bridge
*Preparing for civilization.*
- **v0.4.15a: The Prelude (Event)**
  - World event hinting at the opening of Settlement gates.
  - Rumors of trade routes appearing in dialogue.
- **v0.4.15b: The Snapshot (Save)**
  - Creating a stable "Pre-Settlement" save state format.
