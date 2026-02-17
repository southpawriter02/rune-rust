# Milestone 5 Roadmap: The Saga & The Weaver (v0.4.x)

> **Status:** Planning
> **Version Range:** v0.4.0 - v0.4.17
> **Theme:** Progression, Magic, and World Depth

## Overview
Milestone 5 represents the transition of *Rune & Rust* from a survival simulator to a full-fledged RPG. This cycle introduces the two pillars of long-term engagement: **The Saga System** (Character Progression) and **The Weave** (Aetheric Magic).

The version cycle is extended to **v0.4.17** to ensure these complex systems are fully integrated, balanced, and polished before the introduction of Settlements in v0.5.0.

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

### [v0.4.3] The Weaver (Magic Core)
*The engine of Aetheric manipulation—Flux tracking, spell casting, and arcane consequence.*
- **v0.4.3a: The Aether (Flux State & Service)**
  - `FluxState` model tracking environmental volatility (0-100).
  - `AetherService` for Flux manipulation, dissipation, and threshold detection.
  - `FluxThreshold` enum: Safe, Elevated, Critical, Overload.
  - CombatService integration for end-of-round dissipation.
- **v0.4.3b: The Grimoire (Spell Entity & Repository)**
  - `Spell` entity with School, AP Cost, Flux Cost, Range, EffectScript.
  - `SpellSchool` enum: Destruction, Restoration, Alteration, Divination.
  - `SpellRepository` for data access and spell lookup.
- **v0.4.3c: The Incantation (Casting Engine)**
  - `MagicService` orchestrating spell execution pipeline.
  - `CastCommand` for player spell invocation.
  - Target validation, AP deduction, Flux accumulation.
  - `SpellCastEvent` for UI/audio hooks.
- **v0.4.3d: The Backlash (Risk & Corruption)**
  - `BacklashService` for risk calculation (Flux - 50 = risk %).
  - Severity tiers: Minor (1-10), Major (11-25), Catastrophic (26+).
  - `CorruptionState` tracking long-term magical exposure (0-100).
  - `AetherSickness` status effect implementation.
- **v0.4.3e: The Resonance (Seeding & TUI)**
  - `SpellSeeder` with 12 starter spells across 4 schools.
  - `FluxWidget` and `CorruptionWidget` for HUD display.
  - `SpellListRenderer` for spell selection interface.
  - `cast` command integration in GameLoop.

### [v0.4.4] The Mystic (Archetype Specialization)
*Where others see ruin, the Mystic sees resonance—channeling the Old Ones' legacy through Galdr and glyph.*
- **v0.4.4a: The Attunement (Aetheric Resonance)**
  - `ResonanceState` model tracking personal Flux attunement (0-100).
  - `ResonanceService` for attunement manipulation and potency calculation.
  - `CastingMode` enum: Quick (+15 Flux), Standard, Channeled (-10 Flux), Ritual.
  - Potency modifiers: Dim (-10%) → Steady (0%) → Bright (+15%) → Blazing (+30%) → Overflow (+50%).
- **v0.4.4b: The Galdr (Chanting System)**
  - `GaldrState` tracking active chants, turns remaining, accumulated power.
  - `GaldrService` for chant lifecycle: Start, Advance, Release, Interrupt.
  - Concentration checks (WILL-based) when damaged while chanting.
  - Interruption mechanics: movement, actions, damage, silence effects.
- **v0.4.4c: The Mastery (School Progression)**
  - `MasteryState` tracking XP per spell school.
  - `MasteryService` for tier progression: Novice → Adept → Journeyman → Expert → Archon.
  - Cost modifiers per tier (-5% to -25% AP costs).
  - Passive abilities unlocked at tier thresholds.
- **v0.4.4d: The Paradox (Mystic-Specific Backlash)**
  - `ParadoxService` for enhanced backlash when Flux + Resonance > 100.
  - `AethericOverflow` state: Resonance = 100 triggers power surge then discharge.
  - `SoulFracture` mechanic: permanent penalty from repeated Overflow.
  - Extended backlash tables for Mystic-exclusive consequences.
- **v0.4.4e: The Grimoire (Spell Tree & TUI)**
  - Mystic specialization tree with 15+ abilities.
  - `GrimoireRenderer` for spell tree visualization.
  - Quick-slot system for favorite spells.
  - `grimoire` command integration.

### [v0.4.5] The Warden (Combat Archetype)
*Where the Mystic bends reality, the Warden breaks bones—a master of shield and steel.*
- **v0.4.5a: The Bulwark (Defensive Stance System)**
  - `StanceState` model tracking active combat stance.
  - `StanceService` for stance switching and modifier application.
  - Defensive stances: Shield Wall (+Block, -Attack), Iron Guard (+Parry), Tortoise (+Armor, -Movement).
- **v0.4.5b: The Aegis (Shield Mechanics)**
  - Extended shield block mechanics beyond basic parry.
  - `ShieldBashAction` for offensive shield use.
  - Shield durability and breakage thresholds.
- **v0.4.5c: The Taunt (Threat Generation)**
  - `ThreatService` for managing enemy target priority.
  - `TauntAction` forcing enemies to target the Warden.
  - Threat decay and persistence mechanics.
- **v0.4.5d: The Vanguard (Specialization Tree)**
  - Warden specialization tree with 12+ abilities.
  - Capstone: "Unbreakable"—immunity to stagger for 3 turns.
  - TUI integration for stance and threat display.

### [v0.4.6] The Shadow (Stealth Archetype)
*Unseen, unheard, unforgotten—the Shadow strikes from darkness and vanishes like smoke.*
- **v0.4.6a: The Veil (Stealth System)**
  - `StealthState` model tracking detection level (0-100).
  - `StealthService` for visibility calculation and detection checks.
  - Environmental modifiers: light level, noise, cover.
- **v0.4.6b: The Strike (Ambush Mechanics)**
  - `AmbushService` for surprise attack bonuses.
  - Critical hit multipliers from stealth (1.5x → 2x).
  - Backstab positioning requirements.
- **v0.4.6c: The Escape (Disengage System)**
  - `DisengageAction` for combat exit without opportunity attacks.
  - Smoke bomb consumables for emergency concealment.
  - Re-stealth mechanics during combat.
- **v0.4.6d: The Assassin (Specialization Tree)**
  - Shadow specialization tree with 12+ abilities.
  - Capstone: "Death's Whisper"—guaranteed critical from stealth.
  - TUI integration for stealth meter and detection indicators.

### [v0.4.7] Runic Inscription
*Permanent magic bound to steel.*
- **v0.4.7a: The Rune (Item Properties)**
  - Extension of `ItemProperty` system to support active effects.
  - Socket system for weapons/armor.
- **v0.4.7b: The Forge (Crafting UI)**
  - "Runeforging" trade interface.
  - Resource requirements (Dust + Sigil + Item).

### [v0.4.8] The Glitch
*When magic breaks reality.*
- **v0.4.8a: The Paradox (Event System)**
  - Procedural events triggered by high Flux/Corruption.
  - Visual glitches (text scrambling) in TUI.
- **v0.4.8b: The Wilds (Random Tables)**
  - Wild Magic surge tables (Good/Bad/Weird effects).
  - Summoning accidents (Hostile entities spawning).

### [v0.4.9] Magic UI
*Visualizing the invisible.*
- **v0.4.9a: The Sight (HUD)**
  - Flux meter display in HUD.
  - Active spell effect indicators.
- **v0.4.9b: The Grimoire (Menu)**
  - Dedicated spell management tab.
  - Rune resonance display (matching runes for bonuses).

---

## Part III: The Echo (Stabilization)
**Focus:** Integration, balancing, and preparing for the world expansion.

### [v0.4.10] The Stabilizer
*Balancing the scales of power.*
- **v0.4.10a: The Audit (Balance Pass)**
  - Tuning XP curves based on simulation.
  - Adjusting Spell Costs vs. Damage output.
  - Archetype balance review (Mystic vs Warden vs Shadow).
- **v0.4.10b: The Fix (Bug Squashing)**
  - Focused sprint on regression testing for Saga/Magic interactions.
  - Archetype interaction edge cases.

### [v0.4.11] The Resonance
*Polishing the experience.*
- **v0.4.11a: The Sound (Audio Triggers)**
  - Hooking up audio events for Level Up, Spell Cast, and Fizzle.
  - Archetype-specific audio cues (shield clash, stealth ambush).
- **v0.4.11b: The Feedback (VFX)**
  - Screen shake intensity tuning for high-power spells.
  - Particle color grading for different magic schools.

### [v0.4.12] The Convergence
*Final integration and golden master for 0.5.0.*
- **v0.4.12a: The Gauntlet (Integration Tests)**
  - Full playthrough automation script (Level 1 to Specialization).
  - Multi-archetype progression paths.
- **v0.4.12b: The Precursor (Settlement Seeds)**
  - Seeding "Dormant" settlement locations for v0.5.0.
  - Migration scripts for save compatibility.

### [v0.4.13] The Library
*Codifying the arcane.*
- **v0.4.13a: The Archive (Content)**
  - Expansion of Codex entries for all Spells and Specializations.
  - Unlocking lore by mastering abilities.
- **v0.4.13b: The Research (Mechanic)**
  - System for deciphering ancient scrolls (Intelligence checks).
  - Learning new spells from items instead of trainers.

### [v0.4.14] The Mentor
*Advanced training.*
- **v0.4.14a: The Master (NPCs)**
  - Legendary trainer NPCs who teach Capstone abilities.
  - Requirement for specific reputation to unlock training.
- **v0.4.14b: The Duel (Test)**
  - 1v1 combat challenges to prove worthiness for Specialization unlocks.

### [v0.4.15] The Artifact
*Items of power.*
- **v0.4.15a: The Relic (Item Type)**
  - Unique legendary items with bound spells.
  - Lore descriptions tied to game history.
- **v0.4.15b: The Curse (Mechanic)**
  - Negative properties on powerful items (e.g., drains Sanity).
  - Purification rituals.

### [v0.4.16] The Balance II
*Fine-tuning for the long haul.*
- **v0.4.16a: The Sim (Data)**
  - Automated combat logs analysis for Level 10+ characters.
  - Nerfing/Buffing outliers in Specialization trees.
- **v0.4.16b: The Polish (UX)**
  - Streamlining the level-up process UI.
  - Adding "Build" saving/loading for ease of testing.

### [v0.4.17] The Bridge
*Preparing for civilization.*
- **v0.4.17a: The Prelude (Event)**
  - World event hinting at the opening of Settlement gates.
  - Rumors of trade routes appearing in dialogue.
- **v0.4.17b: The Snapshot (Save)**
  - Creating a stable "Pre-Settlement" save state format.
