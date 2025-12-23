# Milestone 4: The Survivor (Trauma, Crafting, & Environment)

This phase deepened gameplay mechanics beyond combat, introducing psychological horror, a survival economy, and a reactive world.

	•	v0.3.0: The Trauma Economy
	◦	v0.3.0a: The Weight (Psychic Stress) — Implemented the 0-100 Stress resource, WILL-based resolve checks, and the 6-tier stress status system (Stable to Breaking Point).
	◦	v0.3.0b: The Stain (Corruption) — Established the permanent Runic Blight Corruption mechanic, the 6-tier corruption classification, and the "Terminal Error" game-over state.
	◦	v0.3.0c: The Scar (Consequences) — Implemented the Breaking Point resolution event where maximum stress results in stabilization, permanent trauma acquisition, or catastrophe.
	•	v0.3.1: The Crafting Bench
	◦	v0.3.1a: The Blueprint (Core Engine) — Established the CraftingService, Recipe entity, and the static RecipeRegistry containing initial starter recipes.
	◦	v0.3.1b: The Tinkerer (Bodging & Medicine) — Implemented physical repair mechanics, item durability, salvage operations, and location-restricted medical crafting.
	◦	v0.3.1c: The Alchemist (Alchemy & Runeforging) — Implemented volatile trades with "Catastrophe" mechanics (explosions/corruption) and the ItemProperty system for runic enchantments.
	◦	v0.3.1d: Infrastructure — Established Docker-based PostgreSQL containers and EF Core design-time factories to support complex data persistence.
	◦	v0.3.1e: Validation — Added PostgreSQL integration tests to verify JSONB storage for complex crafting and trauma data.
	•	v0.3.2: Rest & Recovery
	◦	v0.3.2a: The Campfire (Core Logic) — Implemented the RestService to handle resource consumption (Rations/Water) and calculate HP/Stamina recovery based on attributes.
	◦	v0.3.2b: The Watch (Ambush System) — Added risk mechanics to wilderness resting, including "Camp Craft" skill mitigation and procedural ambush generation.
	◦	v0.3.2c: The Dawn (Integration) — Wired the rest and camp commands into the game loop and implemented the TUI feedback screen for recovery results.
	•	v0.3.3: Dynamic Environment
	◦	v0.3.3a: The Ground (Dynamic Hazards) — Implemented interactive battlefield objects (traps, vents) using the EffectScript engine to trigger damage or status effects.
	◦	v0.3.3b: The Air (Ambient Conditions) — Established room-wide persistent effects (e.g., Toxic Atmosphere) that apply passive penalties or turn-based ticks.
	◦	v0.3.3c: The Ecosystem (Integration) — Integrated hazards and conditions into the procedural dungeon generation pipeline based on Biome and Danger Level.
v0.3.4: The Gateway (Menu & Creation)
	•	[x] v0.3.4a: The Facade (Visuals) — Implemented the animated ASCII title screen with "glitch" effects and the main menu navigation loop.
	•	[x] v0.3.4b: The Forge (Wizard) — Created the split-screen character creation wizard featuring real-time attribute and derived stat previews.
	•	[x] v0.3.4c: The Prologue (Narrative) — Added the typewriter-style narrative engine to generate background-specific intro sequences.
v0.3.5: The HUD (Exploration View)
	•	[x] v0.3.5a: The Dashboard (Layout) — Established the persistent three-pane screen layout (Header, Body, Footer) and status bars.
	•	[x] v0.3.5b: The Cartographer (Minimap) — Implemented the dynamic 3x3 ASCII minimap with Fog of War and Z-level tracking.
	•	[x] v0.3.5c: The Surveyor (Room View) — Replaced raw text with formatted room panels featuring colored entity lists and exit indicators.
	•	[x] v0.3.5d: The Stabilizer — Addressed critical bugs regarding EF Core indexes and console mouse input interference.
v0.3.6: The Tactician (Combat View)
	•	[x] v0.3.6a: The Battlefield (Grid) — Visualized the Front/Back row combat system with a tactical grid layout.
	•	[x] v0.3.6b: The Timeline (Initiative) — Added a horizontal initiative timeline to project future turns and formatted the rich-text combat log.
	•	[x] v0.3.6c: The Telegraph (Intent) — Implemented enemy intent icons and status effect tracking based on player perception stats.
v0.3.7: The Ledger (Management UI)
	•	[x] v0.3.7a: The Pack (Inventory) — Built the split-screen inventory UI with equipment slots and visual burden meters.
	•	[x] v0.3.7b: The Bench (Crafting) — Created the tabbed crafting interface with recipe filtering and ingredient availability checks.
	•	[x] v0.3.7c: The Archive (Journal) — Implemented the Scavenger's Journal with tabbed navigation and the text redaction renderer.
v0.3.8: The Generator
	•	[x] v0.3.8: Dynamic Room Engine — Transitioned from test maps to template-based procedural generation with variable text substitution.
v0.3.9: The Feedback (FX & Polish)
	•	[x] v0.3.9a: The Impact (Visual FX) — Added screen border flashes for combat feedback (damage, crits) with accessibility toggles.
	•	[x] v0.3.9b: The Lens (Themes) — Implemented a centralized theme service supporting High Contrast and Colorblind palettes.
	•	[x] v0.3.9c: The Guide (Help) — Created the Context Help service for dynamic tips and the Input Configuration service for keybindings.
v0.3.10: The Configurator (Settings)
	•	[x] v0.3.10a: The Preferences — Established the SettingsService for persisting user configuration to JSON.
	•	[x] v0.3.10b: The Control Panel — Built the interactive Options UI for modifying audio, video, and gameplay settings.
	•	[x] v0.3.10c: The Keymaster — Implemented the "Controls" tab allowing interactive key rebinding with conflict detection.
v0.3.10: The Configurator (Settings & Persistence)
	•	[x] v0.3.10a: The Preferences (Settings Engine) — Implement SettingsService to serialize/deserialize game settings (Volume, Colors, TextSpeed) to JSON.
	•	[x] v0.3.10b: The Control Panel (Options UI) — Create the OptionsScreenRenderer with tabbed categories for Display, Audio, and Gameplay configuration.
	•	[x] v0.3.10c: The Keymaster (Input Rebinding UI) — Connect InputConfigurationService to the Options UI to allow interactive key remapping.
v0.3.11: The Archivist (Documentation & Help)
	•	[ ] v0.3.11a: The Living Guide (In-Game Help) — Connect the Journal's "Field Guide" to live game data to auto-generate entries for Status Effects and Mechanics.
	•	[ ] v0.3.11b: The Developer’s Handbook (External Docs) — Implement a CLI tool to parse Attributes and Components into external Markdown documentation.
v0.3.12: The Gauntlet (Automated User Journeys)
	•	[ ] v0.3.12a: The Explorer's Path (Exploration Loop) — Create integration tests simulating a full session of moving, looting, and inventory management.
	•	[ ] v0.3.12b: The Warrior's Trial (Combat Loop) — Create integration tests simulating combat entry, attacks, reactions, and victory/defeat states.
	•	[ ] v0.3.12c: The Survivor's Cycle (Persistence Loop) — Create integration tests verifying state exactness after modification, saving, rebooting, and loading.
v0.3.13: The Scales (Balance & Tuning)
	•	[ ] v0.3.13a: The Loot Audit — Build a simulation utility to run 10,000 loot drops and analyze Rarity distribution against target curves.
	•	[ ] v0.3.13b: The Combat Simulator — Build a headless combat simulator to analyze Time-To-Kill (TTK) and win rates between AI and Archetypes.
v0.3.14: The Experience (UX Polish)
	•	[ ] v0.3.14a: The Palette (Theme Standardization) — Audit all Renderers to enforce ThemeService usage and remove hardcoded ANSI colors.
	•	[ ] v0.3.14b: The Flow (Animations & Transitions) — Implement visual transition effects (dissolve, wipe) between Game Phases using AnsiConsole.Live.
	•	v0.3.15: The Scribe (Localization)
	◦	Scope: Extract hardcoded strings into resource files (Strings.en-US.json) and implement a LocaleService for dynamic language switching at runtime.
	•	v0.3.16: The Sentinel (Stability & Recovery)
	◦	Scope: Implement a global exception handler to capture stack traces and a "Black Box" emergency save system to preserve state during crashes.
	•	v0.3.17: The Architect (Debug Tools)
	◦	Scope: Add an in-game TUI overlay console for executing raw engine commands, enabling cheats like god mode, teleportation, and item spawning for QA.
	•	v0.3.18: The Auditor (Performance Tuning)
	◦	Scope: Audit memory usage for large object allocations and optimize NavigationService pathfinding algorithms for larger dungeon grids.
	•	v0.3.19: The Bard (Audio Framework)
	◦	Scope: Establish the IAudioService architecture and wire combat events (Hit, Miss, Crit) to sound triggers, preparing for future audio assets.
	•	v0.3.20: The Cartographer II (Map Polish)
	◦	Scope: Enhance the minimap with player annotations (/note) and an export utility to dump the current map state to a text file.
	•	v0.3.21: The Steward (Save Management)
	◦	Scope: Enrich save metadata with "screenshot" data (stats, location) for the load menu and implement a rolling backup system for autosaves.
	•	v0.3.22: The Tactician II (Combat Polish)
	◦	Scope: Refine the combat UI with log filtering toggles and an inspect command to view known enemy stats based on Jötun-Reader lore checks.
	•	v0.3.23: The Gatekeeper (Input Refactoring)
	◦	Scope: Abstract input logic into an event-driven ActionInput system to decouple it from Console.ReadKey, enabling potential mouse support.
	•	v0.3.24: The Precursor (Alpha Release Prep)
	◦	Scope: Remove deprecated test/mock code paths, finalize stability checks, and package the build for the v0.4.0 Alpha release.