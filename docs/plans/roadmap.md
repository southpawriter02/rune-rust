Milestone 1: The Walking Skeleton (Infrastructure)

Current Status: In Planning/Early Dev Goal: A running application with architecture, logging, basic state, and persistence.
	•	v0.0.1: The Foundation (Completed Plan)
	◦	Solution scaffolding, Dependency Injection, Serilog integration.
	•	v0.0.2: The Domain (Completed Plan)
	◦	Dice System (IDiceService), Attributes (MIGHT, WITS, etc.), and basic Character entity.
	•	v0.0.3: The Loop (Completed Plan)
	◦	Game Loop state machine (GamePhase), Command Parser, Input/Output abstraction.
	•	v0.0.4: Persistence (Completed Plan)
	◦	PostgreSQL integration, EF Core DbContext, Save/Load services.
	•	v0.0.5: Spatial Core
	◦	Room Engine: Implement DungeonGraph and RoomTemplate to generate a connected grid.
	◦	Navigation: Implement GoCommand and Z-axis (Vertical) movement logic.

Milestone 2: The Explorer (World & Interaction)

Goal: A player can create a character, traverse a generated dungeon, and interact with objects.
	•	v0.1.0: Character Creation
	◦	Creation Wizard: Implement the CharacterCreationController to handle Lineage, Background, and Archetype selection.
	◦	Derived Stats: Implement formulas for HP (Sturdiness), Stamina (Finesse), and AP (Will).
	•	v0.1.1: The Interaction Layer
	◦	Verbs: Implement Search, Open, Unlock, and Examine commands.
	◦	Description System: Implement the "Three-Tier Composition" engine to generate procedural descriptions for rooms and objects.
	•	v0.1.2: Inventory & Economy
	◦	Inventory System: Implement InventoryService, Encumbrance ("Burden"), and equipment slots.
	◦	Loot Tables: Implement LootService with Quality Tiers (Jury-Rigged to Myth-Forged).
	•	v0.1.3: Data Captures (The Codex)
	◦	Journal: Implement the Scavenger's Journal UI and the Data Capture system (lore fragments, echo recordings).
	◦	Why here? This provides the reward loop for exploration before combat is added.

Milestone 3: The Warrior (Combat & AI)

Goal: A player can engage in tactical turn-based combat, use abilities, and die.
	•	v0.2.0: Combat Resolution Core
	◦	Initiative: Implement InitiativeService and Turn Order queue.
	◦	Actions: Implement Standard, Move, and Bonus action economy.
	◦	Resolution: Connect DiceService to Attack/Defend commands.
	•	v0.2.1: The Enemy
	◦	Enemy Factory: Implement EnemyFactory with Threat Tiers (Minion, Standard, Elite, Boss).
	◦	AI: Implement EnemyAI with behavior trees (Aggressive, Defensive, Tactical).
	•	v0.2.2: Abilities & Resources
	◦	Ability System: Implement IAbilityService to handle costs (Stamina/AP) and cooldowns.
	◦	Archetype Kits: Implement starting abilities for Warrior, Skirmisher, Mystic, and Adept.
	•	v0.2.3: Status Effects
	◦	Registry: Implement the Status Effect system (Buffs/Debuffs like [Bleeding], [Stunned]).
	◦	DoT/HoT: Implement turn-start/turn-end processing for damage-over-time.

Milestone 4: The Survivor (Trauma & Crafting)

Goal: Deepen the gameplay with survival mechanics, crafting, and psychological horror.
	•	v0.3.0: The Trauma Economy
	◦	Stress & Corruption: Implement TraumaService to track Psychic Stress and Runic Blight Corruption.
	◦	Consequences: Implement "Breaking Point" events and permanent Trauma acquisition.
	•	v0.3.1: The Crafting Bench
	◦	Systems: Implement the 4 trades: Bodging, Alchemy, Runeforging, Field Medicine.
	◦	Recipes: Implement recipe discovery and the Craft command.
	•	v0.3.2: Rest & Recovery
	◦	Camp: Implement RestService distinguishing between Sanctuary Rest (Safe) and Wilderness Rest (Ambush Risk).
	•	v0.3.3: Dynamic Environment
	◦	Hazards: Implement Dynamic Hazards (Steam Vents, Pressure Plates) and Ambient Conditions (Psychic Resonance).

To address the minimalist start-up experience and refine the TUI, we will insert a new milestone sequence, v0.3.4 to v0.3.8: The Interface Polish, into the roadmap. This phase focuses on leveraging Spectre.Console to transform functional debug text into an immersive, rich terminal experience without altering core game logic.
Here is the comprehensive implementation plan for v0.3.4 - v0.3.8.

Milestone 4.5: The Interface Polish (v0.3.4 - v0.3.8)
Roadmap Overview
	•	v0.3.4: The Gateway (Menu & Creation) – Immersive startup, animated ASCII art, rich character creation wizard with live stat previews, and narrative intros.
	•	v0.3.5: The HUD (Exploration View) – Persistent status bars, minimap integration, and formatted room description rendering.
	•	v0.3.6: The Tactician (Combat View) – Visual combat grid, initiative timeline, clear damage logs, and enemy intent indicators.
	•	v0.3.7: The Ledger (Management UI) – Tabbed inventory screens, crafting workstations, and the interactive Scavenger's Journal.
	•	v0.3.8: The Feedback (FX & Polish) – Screen shake effects, color themes (accessibilty), context-aware help, and input rebinding.

Detailed Plan: v0.3.4 - The Gateway
Goal: Replace the basic text prompts with a rich, interactive startup experience. The player should feel the atmosphere of Rune & Rust before entering the game loop.
1. Architecture & Data Flow
	•	Input: MainMenuController handles the pre-game loop.
	•	Rendering: TuiService (new) manages screen clearing, layouts, and Spectre.Console widgets.
	•	State: CreationWizardState tracks temporary choices (Lineage/Archetype) to render a "Live Preview" panel before committing.
Workflow: The New Game Flow
	1	Launch: Play Animated ASCII Logo (Glitch Effect).
	2	Menu: Render Interactive Menu (Continue, New Game, Options, Quit).
	3	Wizard (Step 1-3):
	◦	User highlights options (e.g., "Warrior").
	◦	Right Panel updates instantly to show stats (MIGHT 4, STURDINESS 4), bonuses, and lore.
	4	Narrative: Upon confirmation, play a scrolling "Typewriter" intro text based on the chosen Background.
	5	Handoff: Initialize GameService and enter Exploration Mode.

2. Logic Decision Trees
A. Creation Wizard Interaction Input: User navigates Selection List
	1	Highlight Change:
	◦	Fetch Metadata for selection (Description, Stat Modifiers).
	◦	Render Preview Panel: Update charts/text on the right side of the screen.
	2	Selection Confirmed:
	◦	Store selection in CreationWizardState.
	◦	Move to next step (Lineage -> Archetype -> Name).
	3	Finalize:
	◦	Call CharacterFactory.Create().
	◦	Transition to IntroRenderer.

3. Code Implementation
A. Terminal Layer (Creation Wizard) File: RuneAndRust.Terminal/UI/CharacterCreationView.cs
public Character RunWizard()
{
    var layout = new Layout("Root")
        .SplitColumns(
            new Layout("Left"), // Selection Menu
            new Layout("Right") // Live Preview
        );

    // Live update loop
    var lineage = AnsiConsole.Prompt(
        new SelectionPrompt<LineageType>()
            .Title("Select your [green]Lineage[/]:")
            .AddChoices(Enum.GetValues<LineageType>())
            .UseConverter(l => $"{l} ({GetLineageSummary(l)})")
            .HighlightStyle(new Style(foreground: Color.Cyan1))
    );

    // Render preview logic would hook into Spectre's live display capabilities
    // For simplicity, we assume sequential prompts here, but full implementation
    // uses AnsiConsole.Live(layout) to update the Right panel based on selection.

    return _characterFactory.Create(name, lineage, archetype);
}
B. Engine Layer (Intro Service) File: RuneAndRust.Engine/Services/IntroService.cs
public async Task PlayIntro(BackgroundType background)
{
    var text = _narrativeService.GetIntroText(background);
    foreach (char c in text)
    {
        AnsiConsole.Write(c);
        await Task.Delay(30); // Typewriter effect
        if (Console.KeyAvailable)
        {
            AnsiConsole.Write(text.Substring(text.IndexOf(c))); // Skip
            break;
        }
    }
}

4. Deliverable Checklist (v0.3.4)
	•	Terminal:
	◦	[ ] Create MainMenuRenderer with ASCII logo.
	◦	[ ] Implement CharacterCreationView using Spectre.Console.Layout.
	◦	[ ] Implement "Live Preview" panel showing derived stats (HP/Stamina) changing based on Lineage/Archetype selection.
	◦	[ ] Add "Typewriter" effect for intro text.
	•	Engine:
	◦	[ ] Create NarrativeService to fetch flavor text for UI.
	•	Data:
	◦	[ ] Add ASCII art assets (Logo, class icons).
	◦	[ ] Add Intro text definitions for each Background.

Detailed Plan: v0.3.5 - The HUD
Goal: Create a persistent "Heads-Up Display" during the Exploration Phase so players always know their status without typing status.
1. Layout Design
Using Spectre.Console.Layout:
	•	Top (Fixed, 3 lines): Status Bar (Name, HP Bar, Stamina Bar, Stress Bar, Location).
	•	Center (Flexible): Main Output (Room descriptions, interaction results).
	•	Right (Fixed, 20 cols): Minimap & Active Quest.
	•	Bottom (Fixed, 1 line): Input Prompt.
2. Code Implementation
File: RuneAndRust.Terminal/Services/ExplorationRenderer.cs
public void RenderScreen(GameState state, string mainContent)
{
    var grid = new Grid();
    grid.AddColumn();

    // Header
    var hpBar = new ProgressBar(state.Player.CurrentHp, state.Player.MaxHp, Color.Red);
    var stamBar = new ProgressBar(state.Player.CurrentStamina, state.Player.MaxStamina, Color.Yellow);

    grid.AddRow(new Table().AddRow(state.Player.Name, hpBar, stamBar).Border(TableBorder.None));

    // Content
    grid.AddRow(new Panel(mainContent).Header(state.CurrentRoom.Name));

    AnsiConsole.Write(grid);
}
3. Deliverable Checklist (v0.3.5)
	•	Terminal:
	◦	[ ] Implement ExplorationLayout class.
	◦	[ ] Create StatusBar widget (HP/Stamina/Stress visualization).
	◦	[ ] Create MinimapWidget (ASCII grid of visited rooms).
	◦	[ ] Update GameLoop to re-render the full layout on every turn.

Logging & Testing (v0.3.4-v0.3.8)
Logging Requirements
System
Event
Level
Message Template
Properties
UI
View Change
Info
UI Transition: {OldView} -> {NewView}
OldView, NewView
Wizard
Selection
Debug
Wizard Step {Step}: Selected {Value}
Step, Value
Wizard
Complete
Info
Character Created via Wizard: {Name} ({Archetype})
Name, Archetype
Testing Requirements
	•	Unit Tests (CharacterCreationControllerTests.cs):
	◦	Wizard_ValidInput_CreatesCharacter: Mock inputs -> Verify Character object returned.
	◦	Wizard_Cancel_ReturnsNull: Input "cancel" -> Verify null return.
	•	Manual QA:
	◦	Verify ASCII art renders correctly on standard 80x24 and large terminals.
	◦	Verify "Live Preview" stats match actual calculations from StatCalculationService.

Draft Changelog (v0.3.4)
# Changelog: v0.3.4 - The Gateway
**Release Date:** 2025-XX-XX

## Summary
The game now features a modernized Terminal User Interface (TUI) for the startup sequence. The minimalist text prompts have been replaced with a rich, interactive Character Creation Wizard featuring live stat previews, ASCII art, and narrative introductions.

## Features
- **Interactive Main Menu:** Navigate via arrow keys.
- **Character Creation Wizard 2.0:**
    - **Live Preview:** See how Lineage and Archetype affect your HP, Stamina, and Attributes in real-time before confirming.
    - **Rich Descriptions:** Lore and mechanic tooltips displayed alongside choices.
- **Visual Polish:**
    - Animated ASCII Title Screen.
    - Typewriter text effects for narrative intros.
    - Color-coded prompts for clarity.

## Technical
- Integrated `Spectre.Console.Layout` for multi-pane UI construction.
- Implemented `IIntroService` to handle narrative playback.


Here is the revised roadmap for v0.3.10 through v0.3.14, focusing on stability, user experience, comprehensive testing, and documentation before the major "Saga" content expansion.

Milestone 4.8: The Polish & Preparation (v0.3.10 - v0.3.14)

This phase solidifies the "Walking Skeleton" into a robust, testable, and user-friendly engine.

v0.3.10: The Configurator (Settings & Persistence)
Goal: Implement persistent user settings, key rebinding UI, and input normalization across all screens.
	•	v0.3.10a: The Preferences (Settings Engine)
	◦	Scope: Create SettingsService to serialize/deserialize GameSettings (Volume, Colors, TextSpeed) to JSON.
	◦	Deliverable: options.json persistence.
	•	v0.3.10b: The Control Panel (Options UI)
	◦	Scope: Implement OptionsScreenRenderer with tabbed categories (Display, Audio, Gameplay, Controls).
	◦	Deliverable: Accessible Options menu from Main Menu and In-Game pause.
	•	v0.3.10c: The Keymaster (Input Rebinding UI)
	◦	Scope: Connect InputConfigurationService (from v0.3.9c) to the Options UI, allowing users to remap keys interactively.
	◦	Deliverable: "Press key to bind" interface.
v0.3.11: The Archivist (Documentation & Help)
Goal: Populate the "Field Guide" with dynamic data and generate external developer documentation.
	•	v0.3.11a: The Living Guide (In-Game Help)
	◦	Scope: Connect the Journal's "Field Guide" tab to live game data. Auto-generate entries for known Status Effects, Items, and Mechanics based on the loaded assemblies.
	◦	Deliverable: A self-updating wiki inside the game.
	•	v0.3.11b: The Developer’s Handbook (External Docs)
	◦	Scope: Implement a CLI tool (--generate-docs) that parses Attributes, Components, and Entities to output Markdown files for the wiki.
	◦	Deliverable: Automated documentation pipeline.
v0.3.12: The Gauntlet (Automated User Journeys)
Goal: Implement end-to-end integration tests that simulate full player sessions to catch regressions in complex loops.
	•	v0.3.12a: The Explorer's Path (Exploration Loop)
	◦	Scope: Auto-test: Start New Game → Move Rooms → Loot Container → Pick up Item → Verify Inventory.
	◦	Deliverable: Integration test suite ExplorationJourneyTests.cs.
	•	v0.3.12b: The Warrior's Trial (Combat Loop)
	◦	Scope: Auto-test: Enter Combat → Execute Attack → Enemy Reacts → Use Item → Win/Lose → Verify XP/Loot.
	◦	Deliverable: Integration test suite CombatJourneyTests.cs.
	•	v0.3.12c: The Survivor's Cycle (Persistence Loop)
	◦	Scope: Auto-test: Modify State (HP/Pos) → Save Game → Reboot Engine → Load Game → Verify Exact State Match.
	◦	Deliverable: Robust Save/Load verification suite.
v0.3.13: The Scales (Balance & Tuning)
Goal: Audit and tune the math behind Loot tables, Combat TDR, and Economy to ensure fairness and progression.
	•	v0.3.13a: The Loot Audit
	◦	Scope: Create a simulation utility to run 10,000 loot drops. Analyze distribution of Rarity and Item Types against target curves.
	◦	Deliverable: LootDistributionReport.md and adjustment of LootTables.
	•	v0.3.13b: The Combat Simulator
	◦	Scope: Create a headless combat simulator to pit AI vs AI. Analyze Time-To-Kill (TTK) and win rates for Archetypes vs Enemy Tiers.
	◦	Deliverable: Tuned SpawnScalingService values.
v0.3.14: The Experience (UX Polish)
Goal: Smooth out rough edges in the TUI, standardize color usage, and improve feedback.
	•	v0.3.14a: The Palette (Theme Standardization)
	◦	Scope: Review all Renderers (Combat, Map, Inventory). Enforce ThemeService usage for every color. Remove hardcoded ANSI codes.
	◦	Deliverable: 100% Theme coverage.
	•	v0.3.14b: The Flow (Animations & Transitions)
	◦	Scope: Implement transition effects between Game Phases (e.g., "Dissolve" or "Wipe" effect in ASCII) using AnsiConsole.Live.
	◦	Deliverable: Polished screen transitions.

Milestone 4.9: The Long Polish (v0.3.15 - v0.3.24)

v0.3.15: The Scribe (Localization Infrastructure)
Goal: Extract all hardcoded strings into resource files to support future translations and text variability.
	•	v0.3.15a: The Lexicon (String Extraction)
	◦	Scope: Move all UI labels, log messages, and error text from code into .resx or JSON resource files.
	◦	Deliverable: Strings.en-US.json.
	•	v0.3.15b: The Translator (Locale Service)
	◦	Scope: Implement LocaleService to load specific string tables at runtime based on GameSettings.
	◦	Deliverable: Dynamic language switching support.
v0.3.16: The Sentinel (Stability & Recovery)
Goal: Implement robust error handling to prevent hard crashes and preserve save data.
	•	v0.3.16a: The Safety Net (Global Exception Handler)
	◦	Scope: Wrap the main game loop in a master try/catch block that logs stack traces to crash.log and displays a user-friendly error screen.
	◦	Deliverable: Crash reporting system.
	•	v0.3.16b: The Black Box (Emergency Save)
	◦	Scope: Attempt a "Panic Save" (serializing current state to a temp file) when a critical exception occurs.
	◦	Deliverable: recovery.json generation on crash.
v0.3.17: The Architect (Debug Tools)
Goal: Create in-game developer tools to speed up testing of the Saga system.
	•	v0.3.17a: The Console (Overlay UI)
	◦	Scope: Add a toggleable (~ key) debug console overlay in the TUI to execute raw engine commands.
	◦	Deliverable: Runtime command injection.
	•	v0.3.17b: The Cheats (God Mode & Teleport)
	◦	Scope: Implement debug commands: /heal, /god, /teleport [room_id], /spawn [item_id].
	◦	Deliverable: QA cheat suite.
v0.3.18: The Auditor (Performance Tuning)
Goal: Optimize memory usage and render loops for larger dungeons.
	•	v0.3.18a: The Garbage Collector (Memory Profiling)
	◦	Scope: Audit SaveGame serialization for memory leaks and optimize large object allocations (like the map grid).
	◦	Deliverable: Reduced RAM footprint.
	•	v0.3.18b: The Hot Path (Pathfinding Optimization)
	◦	Scope: Optimize NavigationService and enemy AI pathfinding algorithms for larger grids.
	◦	Deliverable: Faster turn processing.
v0.3.19: The Bard (Audio Framework)
Goal: Establish the hooks for audio, even if the TUI only outputs system beeps or basic WAVs for now.
	•	v0.3.19a: The Instrument (Audio Service)
	◦	Scope: Create IAudioService and a cross-platform implementation (e.g., NAudio or simple console beeps) for UI feedback.
	◦	Deliverable: Sound on keypress/menu navigation.
	•	v0.3.19b: The Score (Event Hooks)
	◦	Scope: Wire CombatService events (Hit, Miss, Crit) to play distinct sound cues.
	◦	Deliverable: Auditory combat feedback.
v0.3.20: The Cartographer II (Map Polish)
Goal: Enhance the minimap with persistence and annotations.
	•	v0.3.20a: The Notes (Map Annotations)
	◦	Scope: Allow players to add custom text notes to specific rooms on the map via command.
	◦	Deliverable: /note "Locked chest here" functionality.
	•	v0.3.20b: The Atlas (Map Export)
	◦	Scope: Add a command to export the current discovered map to a text file (ASCII art dump) for external reference.
	◦	Deliverable: map_export.txt.
v0.3.21: The Steward (Save Management)
Goal: Improve the user interface for handling save files.
	•	v0.3.21a: The Preview (Save Metadata)
	◦	Scope: Update SaveGame to include "Screenshot" data (current stats, location name, time played) visible before loading.
	◦	Deliverable: Rich load menu.
	•	v0.3.21b: The Vault (Backup System)
	◦	Scope: Automatically keep the previous 3 autosaves (auto_1, auto_2, auto_3) to prevent soft-locks.
	◦	Deliverable: Rolling autosave rotation.
v0.3.22: The Tactician II (Combat Polish)
Goal: refine the combat information display based on "The Visuals" feedback.
	•	v0.3.22a: The Filter (Log Filtering)
	◦	Scope: Add toggles to the combat log to hide/show specific info (e.g., Hide damage rolls, Show only chat).
	◦	Deliverable: Clutter-free combat logs.
	•	v0.3.22b: The Inspector (Detailed View)
	◦	Scope: Add an inspect [enemy] command in combat to show a popup with known stats/resistances (based on Jötun-Reader lore checks).
	◦	Deliverable: Tactical info screen.
v0.3.23: The Gatekeeper (Input Refactoring)
Goal: Abstract input further to support potential future controller/mouse support.
	•	v0.3.23a: The Abstraction (Input Actions)
	◦	Scope: Replace all hardcoded Console.ReadKey checks with an event-driven ActionInput system (e.g., OnConfirm, OnCancel).
	◦	Deliverable: Fully decoupled input logic.
	•	v0.3.23b: The Mouse (TUI Mouse Support)
	◦	Scope: Enable basic mouse clicking for Spectre.Console selections where supported.
	◦	Deliverable: Clickable menus.
v0.3.24: The Precursor (v0.4.0 Prep)
Goal: Final code cleanup and dependency updates before the massive Saga content drop.
	•	v0.3.24a: The Broom (Deprecation Cleanup)
	◦	Scope: Remove all "Test/Mock" code paths left over from v0.1 and v0.2. Ensure no debug commands remain active in release builds.
	◦	Deliverable: Clean codebase.
	•	v0.3.24b: The Golden Master (Alpha Release)
	◦	Scope: A final stability pass, version bumping, and release packaging script.
	◦	Deliverable: RuneAndRust-v0.4.0-Ready.zip.

## Milestone 5: The Saga (Progression & Content)

Goal: Long-term playability, character advancement, and narrative context.
•   v0.4.0: Saga Progression
    ◦   Leveling: Implement the SagaService for Legend (XP) accumulation and Milestone rewards.
    ◦   Spending: Implement the Saga Menu to spend Progression Points (PP) on Attributes and Skills.
•   v0.4.1: Specializations
    ◦   Unlock: Implement the Specialization unlock system.
    ◦   Trees: Implement Ability Trees for the first 4 specializations (e.g., Berserkr, Jötun-Reader).
•   v0.4.2: Factions & Dialogue
    ◦   Reputation: Implement FactionService (Iron-Banes, Dvergr, etc.).
    ◦   Dialogue: Implement the DialogueService with skill-gated responses.

Based on the completion of Milestone 5 (v0.4.x), the roadmap shifts toward Milestone 6: The Weaver (Magic), Milestone 7: The Architect (Settlements), Milestone 8: The Adversary (Advanced Combat), and Milestone 9: The World (Biomes).
Here is the implementation roadmap for the next 20 versions.

## Milestone 6: The Weaver (Magic Systems)

Goal: Implement core magic systems, including Aether, Spells, and Runes.
•   v0.4.3: Magic Core
    ◦   Scope: Implement AetherService, Spell entity, and Aether (Mana) resource tracking.
•   v0.4.4: Spells & Chants
    ◦   Scope: Implement CastCommand, Chant mechanics (multi-turn casting), and the first set of Mystic spells.
•   v0.4.5: Runic Inscription
    ◦   Scope: Implement Runeforging crafting trade, Rune items, and equipment embossing logic.
•   v0.4.6: The Glitch (Wild Magic)
    ◦   Scope: Implement WildMagicService for critical spell failures and "Paradox" events.
•   v0.4.7: Magic UI
    ◦   Scope: Add Grimoire UI tab and visual feedback for active chants/runes.

## Milestone 7: The Architect (Settlements & Economy)

Goal: Implement settlement generation, district logic, and trade systems.
•   v0.5.0: Settlement Engine
    ◦   Scope: Implement Settlement generation (Safe Zones), District logic, and Enter/Exit commands.
•   v0.5.1: Trade & Merchants
    ◦   Scope: Implement TradeService, Merchant entities, Buy/Sell commands, and dynamic stock.
•   v0.5.2: Advanced Factions
    ◦   Scope: Implement Reputation tiers (Hated to Exalted) and faction-specific service gating.
•   v0.5.3: Quest Chains
    ◦   Scope: Expand QuestService to support multi-stage quests, branching outcomes, and delivery missions.
•   v0.5.4: The World Map
    ◦   Scope: Implement WorldMapService, fast travel mechanics, and "Journey Mode" for inter-settlement travel.
## Milestone 8: The Adversary (Advanced Combat)

Goal: Implement advanced combat systems, including enemy traits, AI, and boss mechanics.
•   v0.6.0: Enemy Traits (Affixes)
    ◦   Scope: Implement CreatureTraitService to generate Elite enemies with modifiers (e.g., "Explosive," "Teleporting").
•   v0.6.1: Advanced AI
    ◦   Scope: Implement BehaviorTree AI for squad tactics (healing, flanking, fleeing).
    •   v0.6.2: Boss Mechanics
    ◦   Scope: Implement BossService for multi-phase encounters and legendary actions.
    •   v0.6.3: Stealth & Ambush
    ◦   Scope: Implement StealthService, Hide command, and surprise round mechanics.
    •   v0.6.4: Companion System
    ◦   Scope: Implement Companion entities, recruitment logic, and squad command UI.
Milestone 9: The World (Biome Expansion)
    •   v0.7.0: Muspelheim (Fire)
    ◦   Scope: Implement Heat mechanic, lava hazards, and Surtr-pattern enemies.
    •   v0.7.1: Niflheim (Ice)
    ◦   Scope: Implement Cold mechanic, freezing hazards, and cryo-preservation lore.
    •   v0.7.2: Jötunheim (Iron)
    ◦   Scope: Implement Industrial biomes, conveyor belt movement, and construct enemies.
    •   v0.7.3: Alfheim (Light)
    ◦   Scope: Implement Glimmer madness mechanic (CPS Stage 2) and crystalline geometry.
    •   v0.7.4: Vanaheim (Growth)
    ◦   Scope: Implement Gene-Storm mutation hazards and vertical canopy exploration.
We can begin specifying v0.4.3: Magic Core whenever you are ready.

Based on the completed biomes in Milestone 9 (v0.7.4), the roadmap shifts to the remaining "Deep" realms, the orbital finale, and the transition to the full GUI release.
Here is the implementation plan for the next 20 versions, covering Milestone 10 through Milestone 13.
Milestone 10: The Forge & The Abyss (Deep Realms)
Focus: Implementing the complex industrial and toxic underground realms defined in Domain 8.
    •   v0.8.0: The Deep Framework
    ◦   Scope: Implement Depth mechanic (Z -1 to -3), Acoustic Stealth for Silent Folk encounters, and Darkness hazard logic.
    •   v0.8.1: Svartalfheim (Zone)
    ◦   Scope: Implement Light-Crystal illumination logic, Dvergr city-states, and Guild reputation vendors.
    •   v0.8.2: Svartalfheim (Economy)
    ◦   Scope: Implement Pure Steel refining mechanics, Deep Gate fast-travel network (limited function), and Trade Route missions.
    •   v0.8.3: Helheim (Zone)
    ◦   Scope: Implement Toxicity fluid dynamics, Waste Processing hazards, and Rusting Labyrinth generation.
    •   v0.8.4: The Sunken Sectors
    ◦   Scope: Implement Submersible vehicle logic for acid lakes and Hafgufa naval combat encounters.
Milestone 11: The Shattered Sky (Endgame)
Focus: The orbital realm of Asgard and the resolution of the Trauma Economy.
    •   v0.9.0: Asgard (Zone)
    ◦   Scope: Implement Orbital physics (Zero-G movement), Vacuum exposure timers, and Pristine loot tables.
    •   v0.9.1: The Genius Loci
    ◦   Scope: Implement Architectural AI (rooms that rearrange themselves) and O.D.I.N. defense protocols.
    •   v0.9.2: Valhalla Archives
    ◦   Scope: Implement Memory-Dive mechanics (playing as ancestors) and Consciousness Upload hazards.
    •   v0.9.3: The Heimdallr Signal
    ◦   Scope: Implement Type Omega CPS hazards and Reality Tearing visual effects near the crash site.
    •   v0.9.4: The Counter-Rune (Endgame)
    ◦   Scope: Implement the Jörmungandr Protocol interaction. Crucial: Per Domain 9, do not resolve the paradox; implement the choice to stabilize or accelerate the Glitch.
Milestone 12: The Interface (1.0 Launch)
Focus: Transitioning from Terminal to the full Avalonia GUI defined in the architecture.
    •   v1.0.0: GUI Foundation
    ◦   Scope: Replace Terminal renderer with AvaloniaUI window, implement Visual Inventory (drag-and-drop), and Paper Doll equipment screen.
    •   v1.0.1: Audio & Atmosphere
    ◦   Scope: Implement AudioService for biome-specific ambience (industrial thrum, forest whispers) and SFXtriggers.
    •   v1.0.2: Visual FX
    ◦   Scope: Implement Shader effects for Runic Blight (glitching text/UI) and Particle Systems for combat spells.
    •   v1.0.3: Accessibility & Localization
    ◦   Scope: Implement high-contrast modes, screen-reader support (already prepped in TUI), and string externalization.
    •   v1.0.4: The Gold Release
    ◦   Scope: Final balance pass, Achievement integration, and "Ironman" save mode enforcement.
Milestone 13: The Legacy (Post-Launch)
Focus: Replayability and systems that extend beyond a single run.
    •   v1.1.0: New Game+ (Legacy System)
    ◦   Scope: Implement Legacy Registry to track completed runs and unlock Ancestral Traits for new characters.
    •   v1.1.1: Faction Warfare
    ◦   Scope: Implement dynamic Territory Control where faction influence shifts based on player contracts.
    •   v1.1.2: Settlement Building
    ◦   Scope: Expand Claim mechanics to allow players to repair and staff their own Ridge Hold or Bunker.
    •   v1.1.3: The Endless Deep
    ◦   Scope: Implement an infinite, procedurally generated Abyssal Zone dungeon for post-game challenges.
    •   v1.1.4: Community Cycles
    ◦   Scope: Implement Daily Runs with fixed seeds and leaderboards based on Legend score.
Shall we begin detailing v0.8.0: The Deep Framework?

Here is the implementation status checklist for Rune & Rust, tracking the "Walking Skeleton" from foundational infrastructure through the current "Interface Polish" phase and into the planned Saga expansion.
Phase 1: Infrastructure & Core Domain (Milestone 1)
Version
Status
Codename
Scope
v0.0.1
✅
The Foundation
Solution scaffolding, DI container, Serilog logging.
v0.0.2
✅
The Domain
Dice mechanics, Attributes enum, basic Character entity.
v0.0.3
✅
The Loop
Game state machine, Command Parser, I/O abstraction.
v0.0.4
✅
Persistence
PostgreSQL/EF Core integration, Save/Load services.
v0.0.5
✅
Spatial Core
3D coordinate system, Room entity, basic navigation.
Phase 2: Exploration & Economy (Milestone 2)
Version
Status
Codename
Scope
v0.1.0
✅
The Survivor
Character creation wizard, Lineage/Archetype selection.
v0.1.1
✅
Interaction
Interactable objects, WITS examination, procedural descriptions.
v0.1.2a
✅
Survival (Base)
Item entities, Quality tiers, equipment slots, burden mechanics.
v0.1.2b
✅
Survival (Player)
Inventory management, equip/unequip logic, burden penalties.
v0.1.2c
✅
Survival (World)
Loot generation, container searching, take/drop commands.
v0.1.3a
✅
Codex (Data)
Data Capture entities, Codex Entry schema, unlock thresholds.
v0.1.3b
✅
Codex (Logic)
Capture generation, auto-assignment, completion tracking.
v0.1.3c
✅
Codex (UI)
Journal TUI, text redaction service, glitch effects.
Phase 3: The Warrior (Milestone 3)
Version
Status
Codename
Scope
v0.2.0a
✅
Arena (State)
Combat state machine, initiative sorting, combatant adapter.
v0.2.0b
✅
Exchange (Action)
Attack resolution, stamina costs, hit/miss/crit logic.
v0.2.0c
✅
Interface (UI)
Combat TUI, turn order table, scrolling log.
v0.2.1a
✅
Armory (Gear)
Weapon damage dice, armor soak integration, victory loot.
v0.2.1b
✅
Affliction (Status)
Status effects (Bleed, Stun), DoT ticking, stacking rules.
v0.2.1c
✅
Visuals (UI)
Status icons in UI, post-combat victory screen.
v0.2.2a
✅
Bestiary (Data)
Enemy templates, factory scaling, initial mob roster.
v0.2.2b
✅
Mind (AI)
Enemy AI behaviors (Aggressive, Defensive, Fleeing).
v0.2.2c
✅
Elite (Traits)
Procedural elite enemies with traits (Explosive, Vampiric).
v0.2.3a
✅
Fuel (Resource)
Stamina regen, Aether pool, Overcast mechanic.
v0.2.3b
✅
Engine (Ability)
EffectScript parser (DAMAGE, HEAL, STATUS commands).
v0.2.3c
✅
Arsenal (Kits)
Tier 1 archetype abilities seeded, use command wired.
v0.2.4a
✅
Loadout (Data)
Enemy ability hydration and template integration.
v0.2.4b
📝
Tactician (Select)
Utility-based AI ability selection logic.
v0.2.4c
📝
Omen (Telegraph)
Enemy charge attacks, "Chant" mechanic, telegraph UI.
Phase 4: The Survivor (Milestone 4)
Version
Status
Codename
Scope
v0.3.0a
✅
Weight (Stress)
Psychic Stress resource, WILL-based Resolve Checks.
v0.3.0b
✅
Stain (Blight)
Corruption resource, permanent tiers, Terminal Error state.
v0.3.0c
✅
Scar (Trauma)
Breaking Point event, permanent Trauma acquisition.
v0.3.1a
✅
Blueprint (Craft)
Crafting engine, Recipe registry, WITS checks.
v0.3.1b
✅
Tinkerer (Bodge)
Repair/Salvage logic, Field Medicine constraints.
v0.3.1c
✅
Alchemist (Volatile)
Alchemy/Runeforging, Catastrophe mechanics (Explosions).
v0.3.1d
✅
Infrastructure
Docker PostgreSQL container, EF design-time factory.
v0.3.1e
✅
Validation
PostgreSQL integration tests, JSONB query validation.
v0.3.2a
✅
Campfire (Logic)
Rest mechanics, resource consumption, recovery formulas.
v0.3.2b
✅
Watch (Ambush)
Ambush risk calculation, Camp Craft mitigation.
v0.3.2c
✅
Dawn (Integrate)
Rest UI renderer, rest/camp command wiring.
v0.3.3a
✅
Ground (Hazards)
Interactive traps (Vents, Plates), trigger logic.
v0.3.3b
✅
Air (Conditions)
Room-wide ambient effects (Toxic Air, Psychic Resonance).
v0.3.3c
✅
Ecosystem
Procedural hazard population based on biome/danger.
Phase 5: Interface Polish (Milestone 4.5)
Version
Status
Codename
Scope
v0.3.4a
✅
Facade (Menu)
Animated ASCII title screen, glitch effects.
v0.3.4b
✅
Forge (Wizard)
Split-screen character creator with live stat preview.
v0.3.4c
✅
Prologue (Intro)
Typewriter narrative engine, seamless game entry.
v0.3.5a
✅
Dashboard (HUD)
Persistent 3-pane exploration UI (Header/Body/Log).
v0.3.5b
✅
Map (Minimap)
ASCII minimap with Fog of War and Z-levels.
v0.3.5c
✅
Surveyor (Room)
Rich-text room panel with entity highlighting.
v0.3.5d
✅
Stabilizer
Critical bug fixes, mouse input filtering.
v0.3.6a
✅
Grid (Combat)
Tactical grid visualization (Front/Back rows).
v0.3.6b
✅
Timeline (Turn)
Horizontal initiative timeline, rich combat log.
v0.3.6c
✅
Telegraph (Intent)
Enemy intent icons, status effect symbols.
v0.3.7a
✅
Pack (Inventory)
Split-screen inventory UI, burden bar.
v0.3.7b
✅
Bench (Crafting)
Tabbed crafting UI, recipe details, ingredient checks.
v0.3.7c
✅
Archive (Journal)
Tabbed Journal UI, text redaction, glitch rendering.
v0.3.8
✅
Dynamic Engine
Template-based room generation with variable substitution.
v0.3.9a
✅
Impact (FX)
Screen shake, border flash effects.
v0.3.9b
✅
Lens (Theme)
Accessibility themes (High Contrast, Colorblind).
v0.3.9c
✅
Guide (Controls)
Context help system, Key rebinding logic.
v0.3.10a
✅
Preferences
Persistent settings engine (JSON).
v0.3.10b
✅
Control Panel
Interactive Options UI.
v0.3.10c
✅
Keymaster
Interactive Key Rebinding UI.
Upcoming Polish (Milestone 4.8 - 4.9)
Version
Status
Codename
Scope
v0.3.11
📝
Archivist
In-game help wiki, auto-generated docs.
v0.3.12
📝
Gauntlet
End-to-end integration test suite.
v0.3.13
📝
Scales
Balance tuning (loot tables, combat math).
v0.3.14
📝
Experience
UX polish, color standardization.
v0.3.15
📝
Scribe
Localization infrastructure.
v0.3.16
📝
Sentinel
Crash reporting and emergency save.
v0.3.17
📝
Architect
Debug console overlay.
v0.3.18
📝
Auditor
Performance profiling.
v0.3.19
📝
Bard
Audio service hooks.
v0.3.20
📝
Cartographer II
Map annotations and export.
v0.3.21
📝
Steward
Save management UI.
v0.3.22
📝
Tactician II
Combat log filtering, enemy inspection.
v0.3.23
📝
Gatekeeper
Input refactoring (mouse support).
v0.3.24
📝
Precursor
Final cleanup for Alpha (v0.4.0).
Future Roadmap (Milestones 5-13)
Milestone
Version
Focus
Key Features
5. Saga
v0.4.x
Progression
Leveling, Specializations, Factions.
6. Weaver
v0.4.3+
Magic
Spells, Chants, Wild Magic, Runeforging.
7. Architect
v0.5.x
Settlements
Safe Zones, Trade, Quest Chains.
8. Adversary
v0.6.x
Adv. Combat
Traits, Squad AI, Bosses, Stealth.
9. World
v0.7.x
Biomes
Muspelheim, Niflheim, Jötunheim, Alfheim.
10. Deep
v0.8.x
Underworld
Svartalfheim, Helheim, Sunken Sectors.
11. Sky
v0.9.x
Endgame
Asgard, Valhalla, Counter-Rune.
12. Launch
v1.0.x
GUI
Avalonia GUI, Audio, Achievements.
13. Legacy
v1.1.x
Replay
New Game+, Territory Control.
