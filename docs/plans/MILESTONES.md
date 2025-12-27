# Rune & Rust Milestones

> **Last Updated:** 2025-12-22
> **Document Status:** Consolidated from individual milestone files

This document provides a comprehensive overview of all implementation milestones for Rune & Rust.

---

## Milestone Overview

| # | Name | Versions | Theme | Status |
|---|------|----------|-------|--------|
| 1 | The Walking Skeleton | v0.0.1 - v0.0.5 | Infrastructure | Complete |
| 2 | The Explorer | v0.1.0 - v0.1.3 | World & Interaction | Complete |
| 3 | The Warrior | v0.2.0 - v0.2.4 | Combat & AI | Complete |
| 4 | The Survivor | v0.3.0 - v0.3.3 | Trauma & Crafting | Complete |
| 4.5 | The Interface | v0.3.4 - v0.3.10 | Polish & UX | Complete |
| 4.8 | The Polish | v0.3.11 - v0.3.14 | Documentation & Testing | Planned |
| 4.9 | The Long Polish | v0.3.15 - v0.3.24 | Stability & Prep | Planned |
| 5 | The Saga & The Weaver | v0.4.0 - v0.4.15 | Progression & Magic | Planned |
| 6 | The Architect | v0.5.0 - v0.5.20 | Settlements & Economy | Planned |
| 7 | The Adversary | v0.6.0 - v0.6.15 | Advanced Combat | Planned |
| 8 | The World | v0.7.0 - v0.7.4 | Biome Expansion | Planned |
| 9 | The Deep | v0.8.0 - v0.8.4 | Deep Realms | Planned |
| 10 | The Sky | v0.9.0 - v0.9.4 | Endgame | Planned |
| 11 | The Interface | v1.0.0 - v1.0.4 | GUI Launch | Planned |
| 12 | The Legacy | v1.1.0 - v1.1.4 | Post-Launch | Planned |

---

## Milestone 1: The Walking Skeleton (Infrastructure)

**Goal:** A running application with architecture, logging, basic state, and persistence.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.0.1 | The Foundation | Solution scaffolding, Dependency Injection, Serilog integration |
| v0.0.2 | The Domain | Dice System (IDiceService), Attributes (MIGHT, WITS, etc.), basic Character entity |
| v0.0.3 | The Loop | Game Loop state machine (GamePhase), Command Parser, Input/Output abstraction |
| v0.0.4 | Persistence | PostgreSQL integration, EF Core DbContext, Save/Load services |
| v0.0.5 | Spatial Core | Room Engine with DungeonGraph/RoomTemplate, Navigation with Z-axis movement |

**Status:** Complete

---

## Milestone 2: The Explorer (World & Interaction)

**Goal:** A player can create a character, traverse a generated dungeon, and interact with objects.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.1.0 | The Survivor | Character Creation Wizard with Lineage/Background/Archetype, Derived Stats |
| v0.1.1 | Interaction | Verbs (Search, Open, Unlock, Examine), Three-Tier Composition description engine |
| v0.1.2 | Survival | Inventory System, Encumbrance ("Burden"), Equipment slots, Loot Tables |
| v0.1.3 | The Codex | Scavenger's Journal UI, Data Capture system for lore fragments |

**Status:** Complete

---

## Milestone 3: The Warrior (Combat & AI)

**Goal:** A player can engage in tactical turn-based combat, use abilities, and die.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.2.0 | Combat Core | Initiative, Turn Order queue, Action economy, Attack/Defend resolution |
| v0.2.1 | The Armory | Weapon damage dice, Armor soak, Status Effects (Bleed, Stun), DoT/HoT |
| v0.2.2 | The Bestiary | Enemy Factory with Threat Tiers, AI behavior trees, Elite traits |
| v0.2.3 | The Arsenal | Ability System with costs/cooldowns, EffectScript parser, Archetype kits |
| v0.2.4 | The Adversary | Enemy ability hydration, Utility-based AI selection, Telegraphed attacks |

**Status:** Complete

---

## Milestone 4: The Survivor (Trauma & Crafting)

**Goal:** Deepen gameplay with survival mechanics, crafting, and psychological horror.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.3.0 | Trauma Economy | Psychic Stress (0-100), Corruption tiers, Breaking Points, permanent Traumas |
| v0.3.1 | Crafting Bench | 4 trades (Bodging, Alchemy, Runeforging, Field Medicine), Recipe discovery |
| v0.3.2 | Rest & Recovery | Sanctuary vs Wilderness Rest, Ambush Risk calculation, Resource recovery |
| v0.3.3 | Dynamic Environment | Dynamic Hazards (traps, vents), Ambient Conditions (toxic air, resonance) |

**Status:** Complete

---

## Milestone 4.5: The Interface (Polish)

**Goal:** Transform functional TUI into immersive terminal experience.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.3.4 | The Gateway | Animated ASCII menu, Rich character creation wizard, Narrative intros |
| v0.3.5 | The HUD | Persistent status bars, Minimap with Fog of War, Formatted room descriptions |
| v0.3.6 | The Tactician | Combat grid visualization, Initiative timeline, Enemy intent icons |
| v0.3.7 | The Ledger | Tabbed inventory, Crafting workstation UI, Interactive Journal |
| v0.3.8 | The Generator | Template-based room generation with variable substitution |
| v0.3.9 | The Feedback | Screen shake effects, Accessibility themes, Context-aware help |
| v0.3.10 | The Configurator | Settings persistence, Options UI, Key rebinding |

**Status:** Complete

---

## Milestone 4.8: The Polish & Preparation

**Goal:** Stabilize engine, add testing, prepare for content expansion.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.3.11 | The Archivist | In-game Field Guide, Auto-generated developer docs |
| v0.3.12 | The Gauntlet | End-to-end integration tests (Exploration, Combat, Persistence loops) |
| v0.3.13 | The Scales | Balance tuning via loot simulation and combat TTK analysis |
| v0.3.14 | The Experience | Theme standardization, Screen transition animations |

**Status:** Planned

---

## Milestone 4.9: The Long Polish

**Goal:** Final code cleanup and dependency updates before Saga.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.3.15 | The Scribe | Localization infrastructure, String externalization |
| v0.3.16 | The Sentinel | Global exception handler, Emergency save system |
| v0.3.17 | The Architect | Debug console overlay, Cheat commands for QA |
| v0.3.18 | The Auditor | Memory profiling, Pathfinding optimization |
| v0.3.19 | The Bard | Audio service framework, Combat sound triggers |
| v0.3.20 | The Cartographer II | Map annotations, Map export to file |
| v0.3.21 | The Steward | Save metadata preview, Rolling autosave backup |
| v0.3.22 | The Tactician II | Combat log filtering, Enemy inspection |
| v0.3.23 | The Gatekeeper | Event-driven input abstraction, Mouse support |
| v0.3.24 | The Precursor | Deprecation cleanup, Alpha release packaging |

**Status:** Planned

---

## Milestone 5: The Saga & The Weaver (v0.4.x)

**Goal:** Long-term playability through Progression and Magic systems.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.4.0 | Saga Progression | SagaService for Legend (XP), Milestone rewards, Progression Points (PP) |
| v0.4.1 | Specializations | Specialization unlock system, Tiered ability trees (T1-T3-Capstone) |
| v0.4.2 | Factions & Dialogue | FactionService with reputation, Node-based dialogue, Skill-gated options |
| v0.4.3 | Magic Core | AetherService for flux tracking, Spell entity, "Weaving" mechanic |
| v0.4.4 | Spells & Chants | Cast command, Multi-turn chants, Initial Mystic spell lists |
| v0.4.5 | Runic Inscription | Runeforging trade, Rune items, Equipment embossing with ItemProperty |
| v0.4.6 | The Glitch | WildMagicService, Paradox events based on Corruption/Stress |
| v0.4.7 | Magic UI | Grimoire tab, Chant progress bars, Rune effect indicators |
| v0.4.8 | The Stabilizer | Balance tuning, XP curve adjustments, Spell cost audits |
| v0.4.9 | The Resonance | Audio hooks for magic, VFX polish, particle grading |
| v0.4.10 | The Convergence | Full integration tests, Settlement seeding, v0.5.0 prep |
| v0.4.11 | The Library | Codex expansion, Research mechanics, Spell learning |
| v0.4.12 | The Mentor | Legendary trainers, Duel challenges, Capstone unlocks |
| v0.4.13 | The Artifact | Legendary items, Cursed relics, Purification rituals |
| v0.4.14 | The Balance II | Sim-based tuning, Build saving, Level-up polish |
| v0.4.15 | The Bridge | Pre-Settlement world events, Save migration prep |

**Status:** Planned

---

## Milestone 6: The Architect (v0.5.x)

**Goal:** Establish safe zones, commerce, and political interaction.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.5.0 | Settlement Engine | SettlementGenerator with districts, Enter/Exit commands |
| v0.5.1 | Trade & Merchants | TradeService, Merchant entities, Dynamic stock, Currency (Scrip) |
| v0.5.2 | Services & Guilds | Inns, Hospitals, Bounty Boards, Guild Contracts |
| v0.5.3 | The Workshop | Work orders, Bulk scrapping, Crafting commissions |
| v0.5.4 | The Law | Theft mechanics, Jail system, Crime consequences |
| v0.5.5 | Dynamic Population | NPC schedules, Day/Night cycles, Crowd density |
| v0.5.6 | Faction Influence | District control, Dynamic skirmishes, Propaganda |
| v0.5.7 | Advanced Reputation | Status perks, Favor currency, Social tiering |
| v0.5.8 | Info Economy | Rumors, Secrets, Codex integration, Library research |
| v0.5.9 | Political Events | Elections, Crisis events, Price fluctuations |
| v0.5.10 | The World Map | Hex-grid atlas, Biomes, Fog of War UI |
| v0.5.11 | The Journey | Travel supplies, Fatigue, Movement stances |
| v0.5.12 | Road Encounters | Ambush generation, Roadside POIs, Mini-dungeons |
| v0.5.13 | Trade Routes | Caravans, Supply lines, Interception missions |
| v0.5.14 | The Frontier | Campsites, Outposts, Stashes, Wilderness survival |
| v0.5.15 | The Festival | Seasonal events, Calendars, Minigames |
| v0.5.16 | The Underworld | Black Markets, Smuggling missions, Hidden vendors |
| v0.5.17 | The Diplomat | Alliances, Betrayals, State-level politics |
| v0.5.18 | The Census | NPC names/lineage, Memory persistence, Greeting logic |
| v0.5.19 | The Vault | Banking, Loans, Warehouse storage |
| v0.5.20 | The Foundation II | Settlement defense, Siege previews, Final economy balance |

**Status:** Planned

---

## Milestone 7: The Adversary (v0.6.x)

**Goal:** Deepen tactical complexity with advanced AI and boss mechanics.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.6.0 | Enemy Traits | CreatureTraitService, Affixes, Dynamic resistance |
| v0.6.1 | The Squad | Formations, Synergies, Combo attacks, Rally behavior |
| v0.6.2 | Advanced AI | Behavior Trees, Archetypes (Controller/Support), State machines |
| v0.6.3 | The Environment | Cover mechanics, Destructible terrain, Hazard usage |
| v0.6.4 | The Hunter | Stalking AI, Ambush logic, Nemesis persistence |
| v0.6.5 | The Legend | Boss UI, Legendary Actions, Phase indicators |
| v0.6.6 | The Phase | Phase transitions, Model changes, Telegraphs |
| v0.6.7 | The Minion | Summoning, Sacrifice mechanics, Command buffs |
| v0.6.8 | The Arena | Lair Actions, Room hazards, Chase sequences |
| v0.6.9 | The Loot | Boss specific drops, Trophies, Soul crafting |
| v0.6.10 | The Ally | Mercenary recruitment, Companion stats, Equipment |
| v0.6.11 | The Command | Squad UI, Tactics, Formations, Breach orders |
| v0.6.12 | The Bond | Loyalty tracking, Banter, Desertion risk |
| v0.6.13 | The Shadow | Stealth mechanics, Hide action, Sneak attacks |
| v0.6.14 | The Bounty | Bounty contracts, Tracking, Non-lethal capture |
| v0.6.15 | The Warlord | Commander auras, Mass combat morale, Battle sim |

**Status:** Planned

---

## Milestone 8: The World (Biome Expansion)

**Goal:** Implement unique environmental mechanics for major realms.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.7.0 | Muspelheim | Heat mechanic, Lava hazards, Surtr-pattern enemies |
| v0.7.1 | Niflheim | Cold mechanic, Freezing hazards, Cryo-preservation lore |
| v0.7.2 | Jotunheim | Industrial biomes, Conveyor belt movement, Construct enemies |
| v0.7.3 | Alfheim | Glimmer madness (CPS Stage 2), Crystalline geometry |
| v0.7.4 | Vanaheim | Gene-Storm mutation hazards, Vertical canopy exploration |

**Status:** Planned

---

## Milestone 9: The Deep (Deep Realms)

**Goal:** Implement complex industrial and toxic underground realms.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.8.0 | The Deep Framework | Depth mechanic (Z -1 to -3), Acoustic Stealth, Darkness hazards |
| v0.8.1 | Svartalfheim (Zone) | Light-Crystal illumination, Dvergr city-states, Guild vendors |
| v0.8.2 | Svartalfheim (Economy) | Pure Steel refining, Deep Gate fast-travel, Trade Route missions |
| v0.8.3 | Helheim (Zone) | Toxicity dynamics, Waste Processing hazards, Rusting Labyrinth |
| v0.8.4 | The Sunken Sectors | Submersible vehicles, Acid lakes, Hafgufa naval combat |

**Status:** Planned

---

## Milestone 10: The Sky (Endgame)

**Goal:** The orbital realm of Asgard and resolution of the Trauma Economy.

| Version | Codename | Scope |
|---------|----------|-------|
| v0.9.0 | Asgard (Zone) | Orbital physics (Zero-G), Vacuum exposure, Pristine loot tables |
| v0.9.1 | The Genius Loci | Architectural AI (rearranging rooms), O.D.I.N. defense protocols |
| v0.9.2 | Valhalla Archives | Memory-Dive mechanics, Consciousness Upload hazards |
| v0.9.3 | The Heimdallr Signal | Type Omega CPS hazards, Reality Tearing effects |
| v0.9.4 | The Counter-Rune | Jormungandr Protocol, Choice to stabilize or accelerate Glitch |

**Status:** Planned

---

## Milestone 11: The Interface (1.0 Launch)

**Goal:** Transition from Terminal to full Avalonia GUI.

| Version | Codename | Scope |
|---------|----------|-------|
| v1.0.0 | GUI Foundation | AvaloniaUI window, Visual Inventory (drag-drop), Paper Doll |
| v1.0.1 | Audio & Atmosphere | AudioService, Biome-specific ambience, SFX triggers |
| v1.0.2 | Visual FX | Shader effects for Runic Blight, Particle Systems for spells |
| v1.0.3 | Accessibility | High-contrast modes, Screen-reader support, String externalization |
| v1.0.4 | The Gold Release | Final balance pass, Achievements, Ironman save mode |

**Status:** Planned

---

## Milestone 12: The Legacy (Post-Launch)

**Goal:** Replayability and systems extending beyond a single campaign.

| Version | Codename | Scope |
|---------|----------|-------|
| v1.1.0 | New Game+ | LegacyRegistry for completed runs, Ancestral Traits unlock |
| v1.1.1 | Faction Warfare | Dynamic Territory Control based on player contracts |
| v1.1.2 | Settlement Building | Player-owned Ridge Hold or Bunker with repair/staffing |
| v1.1.3 | The Endless Deep | Infinite procedural Abyssal Zone dungeon |
| v1.1.4 | Community Cycles | Daily Runs with fixed seeds, Legend score leaderboards |

**Status:** Planned

---

*Consolidated from milestone-1.md through milestone-13.md*
*Generated by The Architect - Rune & Rust Development*
