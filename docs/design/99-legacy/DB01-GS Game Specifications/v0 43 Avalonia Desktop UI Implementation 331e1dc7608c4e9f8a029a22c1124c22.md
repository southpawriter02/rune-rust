# v0.43: Avalonia Desktop UI Implementation

Type: UI
Description: Implements comprehensive cross-platform desktop GUI using Avalonia UI. Delivers tactical combat grid with 16×16 pixel art sprites, full character management, dungeon navigation with minimap, endgame mode selection, and performance-optimized rendering at 60 FPS. 21 child specifications covering foundation, combat UI, character management, exploration, endgame, and polish.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.1-v0.42 (All core systems)
Implementation Difficulty: Very Complex
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.43.1: Project Setup & MVVM Framework (v0%2043%201%20Project%20Setup%20&%20MVVM%20Framework%203179f7a759e6459bb34937028e78d29a.md), v0.43.4: Combat Grid Control & Positioning (v0%2043%204%20Combat%20Grid%20Control%20&%20Positioning%20d1616609201b434687ad166945905827.md), v0.43.7: Environmental Hazards & Terrain (v0%2043%207%20Environmental%20Hazards%20&%20Terrain%20536997269cce4ae9a415b5a41dbd4902.md), v0.43.6: Status Effects & Visual Indicators (v0%2043%206%20Status%20Effects%20&%20Visual%20Indicators%20e69089c51d4444dc900c5be2621d72b5.md), v0.43.3: Navigation & Window Management (v0%2043%203%20Navigation%20&%20Window%20Management%20f60082e021bd42b1af30ea04ba1a309b.md), v0.43.5: Combat Actions & Turn Management (v0%2043%205%20Combat%20Actions%20&%20Turn%20Management%20d02c0ac5ba85424bbd9361cf40f81a4c.md), v0.43.2: Sprite System & Asset Pipeline (v0%2043%202%20Sprite%20System%20&%20Asset%20Pipeline%206a4d47a48e154f4295afbd1cfaad49cb.md), v0.43.11: Specialization & Ability Trees (v0%2043%2011%20Specialization%20&%20Ability%20Trees%20875ca770c01d4193b89692b70853ae05.md), v0.43.13: Minimap & Vertical Layer Visualization (v0%2043%2013%20Minimap%20&%20Vertical%20Layer%20Visualization%20b5a4292e2284450ead4c41df4f98e87b.md), v0.43.14: Room Interactions & Search (v0%2043%2014%20Room%20Interactions%20&%20Search%2013079960affa483894db765f4cd31585.md), v0.43.12: Dungeon Navigation & Room Display (v0%2043%2012%20Dungeon%20Navigation%20&%20Room%20Display%20b20a71f37532403693418bbcfc73fb80.md), v0.43.9: Character Sheet & Stats Display (v0%2043%209%20Character%20Sheet%20&%20Stats%20Display%20d1ba96444c62457c97721dc24e1ce2c0.md), v0.43.10: Inventory & Equipment UI (v0%2043%2010%20Inventory%20&%20Equipment%20UI%200a2a7c47b88e4ec3a4724f8d484236a1.md), v0.43.8: Combat Animations & Feedback (v0%2043%208%20Combat%20Animations%20&%20Feedback%20c690fe8b2b96476294b19a6dfda980d8.md), v0.43.17: Boss Phase Indicators & Mechanics Display (v0%2043%2017%20Boss%20Phase%20Indicators%20&%20Mechanics%20Display%20c1af141d0a2e482296433dfecc1b6cfc.md), v0.43.18: Settings & Configuration (v0%2043%2018%20Settings%20&%20Configuration%20b295037bf89c4bdbb6b50246af59d547.md), v0.43.21: UI Testing, Optimization & Bug Fixes (v0%2043%2021%20UI%20Testing,%20Optimization%20&%20Bug%20Fixes%20400a2ddba1f8455881e71dfb648b1b77.md), v0.43.20: Tooltips & Help System (v0%2043%2020%20Tooltips%20&%20Help%20System%20e888127e51304ba68fea1009f27709ae.md), v0.43.19: Save/Load System UI (v0%2043%2019%20Save%20Load%20System%20UI%20209b19d012b44dcab83e6e6b18cd75ed.md), v0.43.15: Meta-Progression & Achievements UI (v0%2043%2015%20Meta-Progression%20&%20Achievements%20UI%208d23715e67684d119643adc98af6ad65.md), v0.43.16: Endgame Mode Selection & Modifiers (v0%2043%2016%20Endgame%20Mode%20Selection%20&%20Modifiers%20f0be463c003d4a8c94f20da887131a03.md)
Template Validated: No
Voice Validated: No

**Status:** Design Phase

**Prerequisites:** v0.1-v0.42 (All core systems)

**Timeline:** 115-155 hours (14-19 weeks part-time)

**Goal:** Implement comprehensive cross-platform desktop GUI using Avalonia UI

**Philosophy:** Visual excellence without sacrificing tactical depth

---

## I. Executive Summary

v0.43 implements a **professional desktop GUI** for Rune & Rust using Avalonia UI, replacing the Spectre.Console terminal interface with a rich graphical experience while maintaining all existing gameplay mechanics.

**What v0.43 Delivers:**

- Cross-platform desktop application (Windows, macOS, Linux)
- Tactical combat grid with 16×16 pixel art sprites
- Full character management and progression visualization
- Dungeon navigation with minimap and vertical layer display
- Endgame mode selection and meta-progression UI
- Performance-optimized rendering (60 FPS target)
- Complete integration with all existing systems (v0.1-v0.42)

**Success Metric:**

Players describe the GUI as "professional" and "intuitive" while maintaining the tactical complexity of the terminal version. All gameplay features accessible through visual interface.

---

## II. Design Philosophy

### A. Visual Clarity Without Complexity Hiding

**Principle:** Graphics enhance understanding, not obscure mechanics.

**Design Rationale:**

Many games use flashy visuals to hide shallow mechanics. Rune & Rust has deep tactical systems (v0.20-v0.23, v0.42) that deserve clear visual representation. The GUI should make complex mechanics **more** accessible, not less.

**Example: Tactical Grid (Pre-v0.43):**

```
Terminal Output:
  A  B  C  D  E  F
1 [S][ ][▓][ ][ ][ ]  S=Shieldmaiden, ▓=Cover
2 [ ][B][ ][ ][ ][ ]  B=Berserker
3 [ ][ ][ ][ ][D][ ]  D=Draugr
4 [ ][ ][🔥][G][ ][ ]  🔥=Hazard, G=Goblin

Result: Functional but cluttered, hard to see positioning relationships
```

**Example: Tactical Grid (Post-v0.43):**

```
Avalonia Grid Control:
- 6×4 cells (100×100px each)
- Pixel art sprites at scale (48×48px)
- HP bars overlay on units
- Cover icons (▓/▒) at cell corners
- Hazards with animated effects
- Flanking indicators (red triangles)
- Movement range highlighting (blue cells)
- Attack range highlighting (red cells)

Result: All information visible at a glance, tactical relationships clear
```

**Why This Matters:**

- New players understand positioning faster
- Experienced players make better tactical decisions
- All v0.20-v0.23 advanced combat mechanics visually represented
- v0.42 AI behavior observable through unit animations

### B. Preservation of Existing Mechanics

**Principle:** GUI changes presentation, not gameplay.

**Zero Regression Policy:**

Every mechanic from v0.1-v0.42 must work identically in GUI:

- Combat resolution (v0.15)
- Tactical positioning (v0.20-v0.20.5)
- Stances and formations (v0.21)
- Environmental hazards (v0.22)
- Status effects (v0.21.3)
- Enemy AI behaviors (v0.42)
- Difficulty scaling (v0.40, v0.42.4)

**Implementation Strategy:**

- GUI calls **same** service methods as terminal UI
- No duplicate game logic
- Shared `CombatState` as source of truth
- Terminal UI remains functional during/after GUI development

**Example: Attack Action**

```csharp
// Terminal UI (Existing)
public async Task ExecuteAttackAsync()
{
    var target = await PromptForTargetAsync();
    var result = await _combatEngine.ProcessPlayerAction(
        new AttackAction { ActorId = _[player.Id](http://player.Id), TargetId = [target.Id](http://target.Id) }
    );
    DisplayResult(result);
}

// GUI (New)
public async Task ExecuteAttackAsync()
{
    // Select target via grid click instead of prompt
    var target = await _gridControl.SelectTargetAsync(TargetType.Enemy);
    
    // SAME service call
    var result = await _combatEngine.ProcessPlayerAction(
        new AttackAction { ActorId = _[player.Id](http://player.Id), TargetId = [target.Id](http://target.Id) }
    );
    
    // Different display (visual instead of text)
    await _combatAnimator.PlayAttackAsync(result);
    UpdateCombatLog(result);
}
```

### C. Progressive Enhancement Through Child Specs

**Principle:** Each child spec delivers a working, testable feature.

**Incremental Delivery Strategy:**

21 child specifications, each 5-8 hours:

- **Foundation** (v0.43.1-v0.43.3): Project setup, sprites, navigation
- **Combat UI** (v0.43.4-v0.43.8): Grid, actions, effects, animations
- **Character Management** (v0.43.9-v0.43.11): Stats, inventory, progression
- **Exploration** (v0.43.12-v0.43.14): Navigation, map, interactions
- **Endgame & Meta** (v0.43.15-v0.43.17): Achievements, modes, bosses
- **Systems & Polish** (v0.43.18-v0.43.21): Settings, saves, optimization

**Each Child Spec Delivers:**

- Working feature demonstrable in isolation
- Integration tests with existing systems
- Performance benchmarks
- User-facing documentation

**Why 21 Specs:**

- Manageable 5-8 hour chunks (part-time sustainable)
- Clear deliverables and progress tracking
- Early feedback opportunities
- Reduced risk of scope creep
- Easier to parallelize if needed

### D. Cross-Platform Parity

**Principle:** Identical experience on Windows, macOS, and Linux.

**Avalonia Guarantees:**

- Same XAML markup on all platforms
- Consistent rendering via SkiaSharp
- Platform-agnostic input handling
- Native look-and-feel where appropriate

**Testing Strategy:**

- Primary development on Windows
- Weekly builds tested on macOS and Ubuntu
- Platform-specific issues tracked and resolved
- No platform gets second-class treatment

**Why Cross-Platform:**

- Expands potential player base
- Validates architecture quality
- Prevents Windows lock-in
- Future-proofs against platform changes

### E. Performance as a Feature

**Principle:** Smooth, responsive UI is a gameplay feature.

**Performance Targets:**

- **60 FPS** during combat animations
- **<50ms** UI response time to player input
- **<3 seconds** application launch time
- **<500 MB** memory usage during typical session
- **<100ms** turn processing time

**Optimization Strategy:**

- Sprite caching (pre-render at common scales)
- Async loading for non-critical assets
- Reactive updates (only re-render changed data)
- Profiling and optimization in v0.43.20

**Why Performance Matters:**

- Tactical decisions require mental focus
- Lag breaks immersion and frustration
- Smooth animations communicate game state clearly
- Low-end hardware accessibility

---

## III. System Overview

### Current State Analysis (Pre-v0.43)

**Existing Terminal UI:**

- Spectre.Console for rendering
- Text-based grid display (ASCII art)
- Keyboard-driven navigation
- Combat log with colored output
- Menu-based action selection

**What Works Well:**

- Fast development iteration
- Clear information hierarchy
- Keyboard efficiency for experienced players
- Low resource usage

**What Needs GUI:**

- Tactical positioning hard to visualize
- New players struggle with ASCII representations
- Status effects not immediately visible
- Limited visual feedback for actions
- No mouse support
- Difficulty conveying complex multi-unit scenarios

**Why v0.43 Now:**

- Core systems complete (v0.1-v0.42)
- Combat mechanics fully implemented
- AI behavior ready for visualization
- Endgame content needs better presentation
- Polish phase appropriate for UI upgrade

### Scope Definition

**✅ In Scope (v0.43):**

**Core Infrastructure:**

- Avalonia project setup and configuration
- MVVM architecture (ReactiveUI)
- Dependency injection integration
- Sprite rendering system (16×16 pixel art)
- Navigation framework

**Combat Visualization:**

- 6×4 tactical grid control
- Unit sprite rendering at positions
- HP bars and status indicators
- Cover and hazard visualization
- Movement and attack range highlighting
- Combat log panel
- Action menu with ability selection
- Turn order display
- Combat animations and effects

**Character Management:**

- Character sheet with full stats
- Specialization tree visualization
- Inventory grid display
- Equipment slot management
- Drag-and-drop equipping
- Item tooltips and comparisons
- Trauma/corruption meters

**Dungeon Navigation:**

- Room description and display
- Exit indicators and connections
- Minimap showing explored areas
- Vertical layer visualization
- Search and interaction UI
- Biome-specific backgrounds

**Endgame & Meta:**

- Achievement tracking display
- Meta-progression unlocks UI
- NG+ tier selection
- Challenge Sector configuration
- Boss Gauntlet progress
- Endless Mode wave display

**System UI:**

- Settings and configuration
- Keybind remapping
- Save/load browser
- Volume and audio controls
- Colorblind mode options

**❌ Out of Scope:**

**Future Enhancements:**

- Multiplayer/co-op UI (v2.0+)
- Mod browser and management (v2.0+)
- In-game level editor (v2.0+)
- Advanced particle systems (v0.44+)
- Full voice acting integration (v2.0+)

**Not UI Problems:**

- New gameplay mechanics (separate specs)
- Balance changes (v0.44-v0.46 focus)
- New content (enemies, items, abilities)
- Performance of game engine (already optimized)

**Why These Limits:**

v0.43 is UI presentation and player interaction only. Gameplay logic remains unchanged. Content expansion is separate.

### System Lifecycle

```
APPLICATION START
  ↓
┌─────────────────────────────────────────────────────────────┐
│ INITIALIZATION                                               │
│  1. Configure DI container (services, ViewModels)            │
│  2. Load sprite definitions from resources                   │
│  3. Initialize SaveRepository connection                     │
│  4. Load user settings/configuration                         │
│  5. Show main window with menu                               │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ MAIN MENU                                                    │
│  - New Game → Character Creation                             │
│  - Continue → Load most recent save                          │
│  - Load Game → Save browser                                  │
│  - Settings → Configuration panel                            │
│  - Achievements → Meta-progression display                   │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ DUNGEON EXPLORATION (Primary Game Loop)                      │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ Room Display                                         │    │
│  │  - Description and environmental details             │    │
│  │  - Available exits (N/S/E/W/U/D)                     │    │
│  │  - Hazards and features                              │    │
│  │  - Minimap with explored rooms                       │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ Actions                                              │    │
│  │  - Search Room → Loot/enemy encounter                │    │
│  │  - Rest → HP/Stamina recovery (Psychic Stress risk)  │    │
│  │  - Move → Select exit → Load new room               │    │
│  │  - Character → Character sheet view                  │    │
│  │  - Inventory → Inventory management                  │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
  ↓ (Enemy encounter triggered)
┌─────────────────────────────────────────────────────────────┐
│ COMBAT (v0.43.4-v0.43.8)                                     │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ Combat Grid Display                                  │    │
│  │  - 6×4 grid with units at positions                  │    │
│  │  - HP bars and status effects                        │    │
│  │  - Cover indicators                                  │    │
│  │  - Environmental hazards                             │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ Player Turn                                          │    │
│  │  1. Select action (Attack/Defend/Ability/Item/Move)  │    │
│  │  2. Select target (if applicable)                    │    │
│  │  3. Execute action → CombatEngine.ProcessAction()    │    │
│  │  4. Animate result                                   │    │
│  │  5. Update combat log                                │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ Enemy Turn (Automated)                               │    │
│  │  1. AI decides action (v0.42 intelligence)           │    │
│  │  2. Execute action                                   │    │
│  │  3. Animate result                                   │    │
│  │  4. Update combat log                                │    │
│  └─────────────────────────────────────────────────────┘    │
│  ↓ (Repeat until victory/defeat/flee)                       │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ Combat End                                           │    │
│  │  - Victory → Loot screen → Return to exploration     │    │
│  │  - Defeat → Game over screen → Main menu            │    │
│  │  - Flee → Return to previous room                   │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ CHARACTER PROGRESSION (v0.43.9-v0.43.11)                     │
│  - Level up → Attribute allocation UI                        │
│  - Specialization unlock → Tree visualization                │
│  - Ability rank up → PP spending interface                   │
│  - Equipment upgrade → Comparison tooltip                    │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ ENDGAME (v0.43.15-v0.43.17)                                  │
│  - Complete dungeon → Victory screen                         │
│  - Unlock NG+ tier → Difficulty selection                    │
│  - Challenge Sector → Modifier configuration                 │
│  - Boss Gauntlet → Sequential boss fights                    │
│  - Endless Mode → Wave progression                           │
│  - Meta-progression → Achievement tracking                   │
└─────────────────────────────────────────────────────────────┘
```

---

## IV. Child Specifications Overview

### Foundation (3 specs, 16-23 hours)

**v0.43.1: Project Setup & MVVM Framework (5-8 hours)**

- Create `RuneAndRust.DesktopUI` Avalonia project
- Configure DI container with service registration
- Implement ViewModelBase with ReactiveUI
- Set up basic window and application lifecycle
- Test: Application launches successfully

**v0.43.2: Sprite System & Asset Pipeline (6-8 hours)**

- Define PixelSprite data structure (16×16 with palette)
- Implement sprite rendering with SkiaSharp
- Create SpriteService with caching
- Load sprite definitions from JSON resources
- Test: Sprites render at multiple scales (3x, 5x)

**v0.43.3: Navigation & Window Management (5-7 hours)**

- Implement NavigationService for view routing
- Create MainWindow layout with sidebar navigation
- Set up view registration and resolution
- Add keyboard shortcut handling
- Test: Can navigate between views via menu and hotkeys

---

### Combat UI (5 specs, 29-38 hours)

**v0.43.4: Combat Grid Control & Positioning (6-8 hours)**

- Create CombatGridControl custom control
- Render 6×4 grid with cell states
- Display unit sprites at GridPosition
- Implement cell selection and highlighting
- Integrate with v0.20 BattlefieldGrid
- Test: Grid displays current CombatState accurately

**v0.43.5: Combat Actions & Turn Management (6-8 hours)**

- Create action menu panel (Attack/Defend/Ability/Item/Move)
- Implement CombatViewModel with ReactiveUI
- Connect actions to CombatEngine service calls
- Add target selection mode
- Display turn order and current phase
- Test: Can execute all combat actions via GUI

**v0.43.6: Status Effects & Visual Indicators (5-7 hours)**

- Create status effect icon system
- Render status effects on units
- Implement tooltip system for effects
- Add HP bar overlay rendering
- Display buff/debuff indicators
- Test: All v0.21.3 status effects visible

**v0.43.7: Environmental Hazards & Terrain (5-7 hours)**

- Render cover indicators (▓ physical, ▒ metaphysical)
- Display environmental hazards (fire, poison, etc.)
- Show elevation differences visually
- Implement terrain type indicators
- Integrate with v0.22 environmental combat
- Test: All v0.22 hazards and terrain types display

**v0.43.8: Combat Animations & Feedback (6-8 hours)**

- Implement attack animation system
- Create damage number popups
- Add sprite flash on hit
- Implement healing effect visualization
- Create ability effect animations
- Test: Animations play smoothly at 60 FPS

---

### Character Management (3 specs, 17-24 hours)

**v0.43.9: Character Sheet & Stats Display (6-8 hours)**

- Create CharacterSheetView layout
- Display attributes (MIGHT, FINESSE, WITS, WILL, STURDINESS)
- Show derived stats (HP, Stamina, Speed, Accuracy)
- Implement trauma meters (Psychic Stress, Corruption)
- Add legend/XP progression bar
- Test: All v0.6-v0.10 stats display correctly

**v0.43.10: Inventory & Equipment UI (6-8 hours)**

- Create inventory grid display
- Implement item icon rendering
- Add equipment slot visualization
- Implement drag-and-drop equipping
- Create item tooltip with stats
- Integrate with v0.15 equipment system
- Test: Can manage inventory and equip items

**v0.43.11: Specialization & Ability Trees (5-8 hours)**

- Create specialization tree visualization
- Display available and unlocked abilities
- Show Progression Point (PP) costs
- Implement ability rank up interface
- Add ability tooltip with full details
- Integrate with v0.11-v0.14 specializations
- Test: Can view and upgrade specializations

---

### Exploration (3 specs, 16-22 hours)

**v0.43.12: Dungeon Navigation & Room Display (6-8 hours)**

- Create DungeonExplorationView layout
- Display room description and features
- Render available exits (N/S/E/W/U/D)
- Implement room action buttons (Search/Rest/Move)
- Add biome-specific backgrounds
- Integrate with v0.4 dungeon generation
- Test: Can navigate between rooms

**v0.43.13: Minimap & Vertical Layer Visualization (5-7 hours)**

- Create minimap control showing room graph
- Implement fog-of-war for unexplored rooms
- Add vertical layer indicator (depth/height)
- Display vertical connections (stairs, shafts)
- Integrate with v0.5 vertical progression
- Test: Minimap accurately reflects explored dungeon

**v0.43.14: Room Interactions & Search (5-7 hours)**

- Implement search action UI
- Create loot display panel
- Add rest/camp confirmation dialog
- Display environmental storytelling elements
- Integrate with v0.4 room features
- Test: Room interactions work as expected

---

### Endgame & Meta (3 specs, 15-21 hours)

**v0.43.15: Meta-Progression & Achievements UI (5-7 hours)**

- Create achievement tracking display
- Show meta-progression unlocks
- Display account-level statistics
- Implement cosmetic unlock browser
- Integrate with v0.41 meta-progression
- Test: Achievements and unlocks display correctly

**v0.43.16: Endgame Mode Selection & Modifiers (5-7 hours)**

- Create NG+ tier selection UI
- Implement Challenge Sector configuration panel
- Add Boss Gauntlet mode selector
- Create Endless Mode wave tracker
- Display difficulty modifiers and rewards
- Integrate with v0.40 endgame content
- Test: Can select and start endgame modes

**v0.43.17: Boss Encounter Enhancements (5-7 hours)**

- Create boss-specific UI elements
- Display phase indicators for bosses
- Show boss ability rotation hints
- Implement add management visualization
- Add boss health phase transitions
- Integrate with v0.42.3 boss AI
- Test: Boss encounters feel epic and clear

---

### Systems & Polish (4 specs, 21-29 hours)

**v0.43.18: Settings & Configuration UI (5-7 hours)**

- Create settings panel layout
- Implement volume controls (master, SFX, music)
- Add keybind remapping interface
- Create colorblind mode options
- Implement window mode settings
- Add UI scale slider
- Test: All settings persist and apply

**v0.43.19: Save/Load Browser & Management (5-7 hours)**

- Create save game browser UI
- Display save metadata (timestamp, dungeon, legend)
- Implement load game functionality
- Add delete save confirmation
- Create autosave indicator
- Integrate with existing SaveRepository
- Test: Can save and load games from GUI

**v0.43.20: Performance Optimization (5-7 hours)**

- Profile application performance
- Optimize sprite rendering pipeline
- Implement asset preloading
- Reduce memory allocations in hot paths
- Add performance monitoring overlay (debug)
- Test: Consistent 60 FPS, <500MB memory

**v0.43.21: Testing, Validation & Documentation (6-8 hours)**

- Create comprehensive test suite
- Validate all integration points
- Test on all target platforms
- Create user-facing documentation
- Write developer setup guide
- Prepare release notes
- Test: All functionality works as specified

---

## V. Technical Architecture

### Layer Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    RuneAndRust.DesktopUI                     │
│  ┌────────────┐  ┌────────────┐  ┌────────────────────────┐ │
│  │   Views    │  │ ViewModels │  │      Services          │ │
│  │  (XAML)    │  │ (ReactiveUI)│  │  - SpriteService       │ │
│  │            │  │            │  │  - NavigationService   │ │
│  │ - Main     │  │ - Combat   │  │  - ConfigService       │ │
│  │ - Combat   │  │ - Character│  │  - DialogService       │ │
│  │ - Character│  │ - Dungeon  │  │  - AnimationService    │ │
│  │ - Dungeon  │  │ - Settings │  │  - AudioService        │ │
│  └────────────┘  └────────────┘  └────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            ↓ References (NO UI in Engine)
┌─────────────────────────────────────────────────────────────┐
│                    RuneAndRust.Engine                        │
│  - CombatEngine           - DungeonGenerator                 │
│  - EnemyAIService         - CharacterProgressionService      │
│  - EquipmentService       - StatusEffectService              │
│  - All game logic services (v0.1-v0.42)                      │
└─────────────────────────────────────────────────────────────┘
                            ↓ References
┌─────────────────────────────────────────────────────────────┐
│                     RuneAndRust.Core                         │
│  - CombatState            - BattlefieldGrid                  │
│  - PlayerCharacter        - Enemy                            │
│  - All data models                                           │
└─────────────────────────────────────────────────────────────┘
                            ↓ References
┌─────────────────────────────────────────────────────────────┐
│                  RuneAndRust.Persistence                     │
│  - SaveRepository         - Database schema                  │
│  - Save/Load logic                                           │
└─────────────────────────────────────────────────────────────┘
```

### Service Layer (v0.43 New Services)

```csharp
// Sprite rendering and caching
public interface ISpriteService
{
    Bitmap? GetSpriteBitmap(string spriteName, int scale = 3);
    void RegisterSprite(string name, PixelSprite sprite);
    IEnumerable<string> GetAvailableSprites();
}

// View navigation
public interface INavigationService
{
    ViewModelBase? CurrentView { get; }
    void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    event EventHandler<ViewModelBase>? CurrentViewChanged;
}

// User settings persistence
public interface IConfigurationService
{
    UIConfiguration LoadConfiguration();
    void SaveConfiguration(UIConfiguration config);
    Task<bool> ValidateConfigurationAsync();
}

// Modal dialogs
public interface IDialogService
{
    Task<bool> ShowConfirmationAsync(string title, string message);
    Task ShowMessageAsync(string title, string message);
    Task<T?> ShowDialogAsync<T>(DialogViewModel<T> viewModel);
}

// Combat animations
public interface IAnimationService
{
    Task PlayAttackAnimationAsync(AttackResult result);
    Task PlayDamageNumberAsync(int damage, GridPosition position);
    Task PlayHealingEffectAsync(int healing, GridPosition position);
    Task PlayStatusEffectAsync(StatusEffect effect, GridPosition position);
}

// Audio playback
public interface IAudioService
{
    void PlaySound(string soundName);
    void PlayMusic(string trackName);
    void SetVolume(AudioChannel channel, float volume);
    void StopAll();
}
```

### Data Models (v0.43 New Models)

```csharp
// 16×16 pixel sprite definition
public class PixelSprite
{
    public string Name { get; set; } = string.Empty;
    public string[] PixelData { get; set; } = Array.Empty<string>(); // 16 rows
    public Dictionary<char, string> Palette { get; set; } = new();
    
    public SKBitmap ToBitmap(int scale = 3);
}

// UI configuration
public class UIConfiguration
{
    public float MasterVolume { get; set; } = 1.0f;
    public float SFXVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 1.0f;
    public Dictionary<string, Key> KeyBindings { get; set; } = new();
    public ColorblindMode ColorblindMode { get; set; } = ColorblindMode.None;
    public WindowMode WindowMode { get; set; } = WindowMode.Windowed;
    public float UIScale { get; set; } = 1.0f;
}

// Grid cell display state
public class GridCellViewModel : ViewModelBase
{
    public GridPosition Position { get; set; }
    public bool HasCover { get; set; }
    public CoverType CoverType { get; set; }
    public bool HasHazard { get; set; }
    public EnvironmentalHazard? Hazard { get; set; }
    public CombatantViewModel? Occupant { get; set; }
    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }
    public bool IsValidMoveTarget { get; set; }
    public bool IsValidAttackTarget { get; set; }
}

// Combatant display wrapper
public class CombatantViewModel : ViewModelBase
{
    public Combatant Model { get; set; }
    public Bitmap? Sprite { get; set; }
    public string Name => [Model.Name](http://Model.Name);
    public int HP => Model.CurrentHP;
    public int MaxHP => Model.MaxHP;
    public float HPPercent => (float)HP / MaxHP;
    public ObservableCollection<StatusEffectViewModel> StatusEffects { get; set; }
    public bool IsPlayer => Model is PlayerCharacter;
}
```

---

## VI. Integration Points

### v0.1: Combat System Integration

```csharp
// GUI calls same CombatEngine methods as terminal UI
public class CombatViewModel : ViewModelBase
{
    private readonly ICombatEngine _combatEngine;
    
    public async Task ProcessPlayerActionAsync(CombatAction action)
    {
        // SAME method call as terminal UI
        var result = await _combatEngine.ProcessPlayerAction(action);
        
        // Different display method
        await _animationService.PlayActionAsync(result);
        UpdateCombatLog(result);
        RefreshGrid();
    }
}
```

### v0.20-v0.20.5: Tactical Grid Integration

```csharp
// CombatGridControl renders BattlefieldGrid from v0.20
public class CombatGridControl : Control
{
    public BattlefieldGrid? Grid { get; set; } // From v0.20
    
    public override void Render(DrawingContext context)
    {
        if (Grid == null) return;
        
        foreach (var tile in Grid.Tiles.Values)
        {
            RenderCell(context, tile);
            
            // Render cover from v0.20.2
            if (tile.CoverType != CoverType.None)
                RenderCoverIndicator(context, tile);
            
            // Render flanking from v0.20.1
            if (tile.IsFlankingPosition)
                RenderFlankingIndicator(context, tile);
            
            // Render occupant from v0.20
            if (tile.Occupant != null)
                RenderCombatant(context, tile.Occupant);
        }
    }
}
```

### v0.21-v0.23: Advanced Combat Integration

```csharp
// Display status effects from v0.21.3
public void RenderStatusEffects(CombatantViewModel combatant)
{
    foreach (var effect in combatant.StatusEffects)
    {
        var icon = _spriteService.GetStatusEffectIcon(effect.Type);
        // Render icon with tooltip showing effect details
    }
}

// Display environmental hazards from v0.22
public void RenderHazards(GridTile tile)
{
    if (tile.Hazard != null)
    {
        var hazardSprite = _spriteService.GetHazardSprite(tile.Hazard.Type);
        // Render hazard with animated effect
    }
}

// Display stance from v0.21.1
public void RenderCombatantStance(CombatantViewModel combatant)
{
    var stanceIcon = GetStanceIcon(combatant.Model.CurrentStance);
    // Render stance indicator on unit
}
```

### v0.40: Endgame Integration

```csharp
// Display NG+ tier and difficulty modifiers
public class EndgameModeViewModel : ViewModelBase
{
    public void DisplayNGPlusTier()
    {
        var tier = _gameStateService.GetCurrentNGPlusTier();
        NGPlusTierText = $"NG+{tier}";
        
        // Show difficulty modifiers from v0.40
        var modifiers = _gameStateService.GetNGPlusModifiers(tier);
        DifficultyModifiers = [modifiers.Select](http://modifiers.Select)(m => m.Description).ToList();
    }
}
```

### v0.42: AI Visualization

```csharp
// Show AI decision-making visually
public async Task ShowEnemyTurnAsync(Enemy enemy)
{
    // Highlight enemy making decision
    HighlightUnit(enemy);
    
    // Show target selection (from v0.42.1)
    var targetedCell = enemy.TargetPosition;
    HighlightCell(targetedCell, [Color.Red](http://Color.Red));
    
    // Show AI archetype indicator (from v0.42.2)
    DisplayArchetypeIcon(enemy.AIArchetype);
    
    // Animate action
    await _animationService.PlayEnemyActionAsync(enemy.DecidedAction);
}
```

---

## VII. Database Schema

### UI Configuration Tables

```sql
-- User interface settings
CREATE TABLE UIConfiguration (
    ConfigId INT PRIMARY KEY IDENTITY,
    UserId UNIQUEIDENTIFIER NOT NULL,  -- For multi-user support
    MasterVolume DECIMAL(3,2) NOT NULL DEFAULT 1.0,
    SFXVolume DECIMAL(3,2) NOT NULL DEFAULT 1.0,
    MusicVolume DECIMAL(3,2) NOT NULL DEFAULT 1.0,
    ColorblindMode VARCHAR(20) NOT NULL DEFAULT 'None',
    WindowMode VARCHAR(20) NOT NULL DEFAULT 'Windowed',
    WindowWidth INT NOT NULL DEFAULT 1280,
    WindowHeight INT NOT NULL DEFAULT 720,
    UIScale DECIMAL(3,2) NOT NULL DEFAULT 1.0,
    ShowTutorialHints BIT NOT NULL DEFAULT 1,
    ShowGridCoordinates BIT NOT NULL DEFAULT 0,
    ShowDamageNumbers BIT NOT NULL DEFAULT 1,
    AnimationSpeed VARCHAR(20) NOT NULL DEFAULT 'Normal',
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UNIQUE (UserId)
);

-- Keybind configuration
CREATE TABLE UIKeybindings (
    KeybindId INT PRIMARY KEY IDENTITY,
    ConfigId INT NOT NULL,
    ActionName VARCHAR(100) NOT NULL,
    PrimaryKey VARCHAR(50) NOT NULL,
    ModifierKeys VARCHAR(100),  -- Ctrl, Alt, Shift (comma-separated)
    SecondaryKey VARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (ConfigId) REFERENCES UIConfiguration(ConfigId) ON DELETE CASCADE,
    UNIQUE (ConfigId, ActionName)
);

-- Sprite definitions (if not using JSON)
CREATE TABLE SpriteDefinitions (
    SpriteId INT PRIMARY KEY IDENTITY,
    SpriteName VARCHAR(100) NOT NULL UNIQUE,
    SpriteType VARCHAR(50) NOT NULL,  -- Player, Enemy, Item, Terrain, Effect
    PixelData NVARCHAR(MAX) NOT NULL,  -- JSON array of 16 strings
    PaletteData NVARCHAR(MAX) NOT NULL,  -- JSON object of char -> color
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX IX_Sprite_Type (SpriteType)
);

-- UI animation preferences
CREATE TABLE UIAnimationSettings (
    AnimationId INT PRIMARY KEY IDENTITY,
    ConfigId INT NOT NULL,
    AnimationType VARCHAR(50) NOT NULL,  -- Attack, Heal, StatusEffect, etc.
    IsEnabled BIT NOT NULL DEFAULT 1,
    SpeedMultiplier DECIMAL(3,2) NOT NULL DEFAULT 1.0,
    FOREIGN KEY (ConfigId) REFERENCES UIConfiguration(ConfigId) ON DELETE CASCADE,
    UNIQUE (ConfigId, AnimationType)
);
```

### Default Configuration Data

```sql
-- Default keybindings
INSERT INTO UIKeybindings (ConfigId, ActionName, PrimaryKey, ModifierKeys)
VALUES
    (1, 'Attack', 'D1', NULL),           -- 1 key
    (1, 'Defend', 'D2', NULL),           -- 2 key
    (1, 'Ability', 'D3', NULL),          -- 3 key
    (1, 'Item', 'D4', NULL),             -- 4 key
    (1, 'Move', 'D5', NULL),             -- 5 key
    (1, 'CharacterSheet', 'C', NULL),    -- C key
    (1, 'Inventory', 'I', NULL),         -- I key
    (1, 'Map', 'M', NULL),               -- M key
    (1, 'Rest', 'R', NULL),              -- R key
    (1, 'Search', 'S', NULL),            -- S key
    (1, 'Flee', 'F', NULL),              -- F key
    (1, 'EndTurn', 'Space', NULL),       -- Space key
    (1, 'Confirm', 'Enter', NULL),       -- Enter key
    (1, 'Cancel', 'Escape', NULL),       -- Escape key
    (1, 'QuickSave', 'F5', NULL),        -- F5 key
    (1, 'QuickLoad', 'F9', NULL);        -- F9 key

-- Default animation settings
INSERT INTO UIAnimationSettings (ConfigId, AnimationType, IsEnabled, SpeedMultiplier)
VALUES
    (1, 'Attack', 1, 1.0),
    (1, 'Heal', 1, 1.0),
    (1, 'StatusEffect', 1, 1.0),
    (1, 'Movement', 1, 1.0),
    (1, 'DamageNumber', 1, 1.0),
    (1, 'ScreenTransition', 1, 1.0);
```

---

## VIII. Success Criteria

**v0.43 is DONE when:**

### ✅ Functionality

- [ ]  All 21 child specifications complete and tested
- [ ]  Can play full dungeon run using only GUI
- [ ]  All v0.1-v0.42 features accessible through UI
- [ ]  Save/load system works from GUI
- [ ]  Cross-platform builds successful (Windows, macOS, Linux)

### ✅ Integration

- [ ]  No regressions in existing gameplay mechanics
- [ ]  Terminal UI still functional (parallel development)
- [ ]  All Engine service calls work identically
- [ ]  CombatState synchronization perfect
- [ ]  Save files compatible between terminal and GUI

### ✅ Performance

- [ ]  Consistent 60 FPS during combat
- [ ]  <50ms UI response time
- [ ]  <3 second application launch
- [ ]  <500 MB memory usage
- [ ]  <100ms turn processing

### ✅ Quality

- [ ]  Zero critical bugs
- [ ]  All validation checklists passed
- [ ]  User testing feedback positive
- [ ]  Documentation complete
- [ ]  Developer setup guide written

### ✅ User Experience

- [ ]  New players can learn combat in <5 minutes
- [ ]  All UI elements have clear visual feedback
- [ ]  Keyboard navigation throughout
- [ ]  Tooltips on all interactive elements
- [ ]  Colorblind mode tested and functional

---

## IX. Timeline

**Total: 115-155 hours (14-19 weeks part-time @ 8 hours/week)**

**Foundation (Weeks 1-3):** v0.43.1-v0.43.3

**Combat UI (Weeks 4-7):** v0.43.4-v0.43.8

**Character Management (Weeks 8-10):** v0.43.9-v0.43.11

**Exploration (Weeks 11-13):** v0.43.12-v0.43.14

**Endgame & Meta (Weeks 14-16):** v0.43.15-v0.43.17

**Systems & Polish (Weeks 17-19):** v0.43.18-v0.43.21

**Milestones:**

- **Milestone 1 (Week 3):** Application launches with sprite demo
- **Milestone 2 (Week 7):** Playable combat with all actions
- **Milestone 3 (Week 10):** Full character management
- **Milestone 4 (Week 13):** Dungeon navigation functional
- **Milestone 5 (Week 16):** Endgame modes accessible
- **Milestone 6 (Week 19):** Full release ready

---

## X. Risk Assessment

### Technical Risks

- **Avalonia learning curve:** Mitigate with Phase 1 focus on fundamentals
- **Performance issues:** Profiling and optimization in v0.43.20
- **Cross-platform bugs:** Weekly testing on all platforms
- **Memory leaks:** Proper disposal patterns and profiling

### Project Risks

- **Scope creep:** Strict adherence to child spec deliverables
- **Timeline overrun:** 5-8 hour child specs allow for buffer
- **Integration failures:** Continuous integration testing
- **User feedback late:** Early demos after Milestone 1

### Mitigation Strategies

- Work incrementally (one child spec at a time)
- Test on target platforms weekly
- Get user feedback at each milestone
- Document issues and blockers immediately
- Maintain parallel terminal UI for fallback

---

**Ready to implement comprehensive desktop GUI.**