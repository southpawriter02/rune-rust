# Rune & Rust - GUI Specification

## Version 0.45 - Comprehensive User Interface Documentation

**Document Version:** 1.0
**Last Updated:** November 2024
**Target Framework:** Avalonia UI 11.x with ReactiveUI
**Architecture:** MVVM Pattern with Controllers

---

## Table of Contents

1. [Introduction](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
2. [Main Menu View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
3. [Combat View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
4. [Character Sheet View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
5. [Dungeon Exploration View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
6. [Inventory & Equipment View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
7. [Character Creation View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
8. [Settings View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
9. [Save/Load View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
10. [Endgame Mode Selection View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
11. [Victory Screen](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
12. [Death Screen](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
13. [Minimap View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
14. [Help Browser View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
15. [Specialization Tree View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
16. [Meta-Progression View](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
17. [Global Keyboard Shortcuts](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)
18. [UI Services & Controllers](Rune%20&%20Rust%20-%20GUI%20Specification%202ba55eb312da80078a81dada32c66507.md)

---

## 1. Introduction

### 1.1 Purpose

This document provides comprehensive specifications for every user interface element in Rune & Rust. It covers all buttons, labels, input fields, displays, and interactive behaviors across the entire application.

### 1.2 Design Philosophy

- **16x16 Pixel Art Aesthetic**: All sprites rendered at native resolution with SkiaSharp scaling
- **Dark Theme**: Dark gray backgrounds (#1C1C1C) with colored accent elements
- **Quality-Based Coloring**: Equipment quality indicated by border colors (Gray → White → Blue → Purple → Gold)
- **Reactive Updates**: All UI elements update automatically via ReactiveUI bindings
- **Accessibility First**: Colorblind modes, screen reader support, configurable UI scale

### 1.3 Terminology (Aethelgard Lore)

| Game Term | UI Display |
| --- | --- |
| Survivors | Player characters |
| Undying | Death system (characters lost on death) |
| Sectors | Dungeon levels/biomes |
| Legend | Experience points |
| PP | Progression Points (skill unlock currency) |
| Milestones | Level-up thresholds |
| Psychic Stress | Mental trauma meter (0-100) |
| Runic Blight Corruption | Physical corruption meter (0-100) |

---

## 2. Main Menu View

**ViewModel:** `MenuViewModel.cs`**Controller:** `MainMenuController`

### 2.1 Display Elements

| Element | Type | Content |
| --- | --- | --- |
| Title | Label | "Rune & Rust" |
| Version | Label | "v0.44.1 - Game Flow Integration" |

### 2.2 Buttons

| Button | Command | Behavior | Enabled Condition |
| --- | --- | --- | --- |
| **New Game** | `NewGameCommand` | Navigates to Character Creation | Always |
| **Continue** | `ContinueGameCommand` | Loads most recent save | `HasSavedGame == true` |
| **Load Game** | `LoadGameCommand` | Opens Save/Load browser | Always |
| **Settings** | `SettingsCommand` | Opens Settings view | Always |
| **Achievements** | `AchievementsCommand` | Opens Meta-Progression view | Always |
| **Sprite Demo** | `SpriteDemoCommand` | Opens sprite demonstration | Always |
| **Endgame** | `EndgameCommand` | Opens Endgame Mode selection | Always |
| **Exit** | `ExitCommand` | Prompts to quit application | Always |

### 2.3 Properties

| Property | Type | Description |
| --- | --- | --- |
| `Title` | string | Application title ("Rune & Rust") |
| `Version` | string | Current version string |
| `HasSavedGame` | bool | Whether continue button should be enabled |

---

## 3. Combat View

**ViewModel:** `CombatViewModel.cs`**Controller:** `CombatController`

### 3.1 Display Elements

### 3.1.1 Header Section

| Element | Type | Binding | Description |
| --- | --- | --- | --- |
| Title | Label | `Title` | "Tactical Combat" |
| Status Message | Label | `StatusMessage` | Current turn/action feedback |

### 3.1.2 Battlefield Grid

| Element | Description |
| --- | --- |
| Grid Layout | 6x4 cells (2 zones × 2 rows × variable columns) |
| Cell Size | `CellSize` property (default 80px) |
| Unit Sprites | Dictionary `UnitSprites` mapping positions to SKBitmap |
| Unit Data | Dictionary `UnitData` mapping positions to Combatant info |
| Highlighted Cells | `HighlightedPositions` for valid targets |
| Selected Cell | `SelectedPosition` with distinct styling |
| Hovered Cell | `HoveredPosition` for preview |

### 3.1.3 Combat HUD Elements

| Element | Type | Binding | Description |
| --- | --- | --- | --- |
| Combat Log | ScrollViewer/ListView | `CombatLog` | Recent combat events (max 50) |
| Turn Order | ListView | `TurnOrder` | Initiative order display |
| Player HP | Progress Bar | `CombatState.Player.HP/MaxHP` | Current health |
| Player Stamina | Progress Bar | `CombatState.Player.Stamina/MaxStamina` | Action resource |

### 3.1.4 Environmental Display

| Element | Type | Binding | Description |
| --- | --- | --- | --- |
| Hazard Overlays | Visual indicators | `EnvironmentalObjects` | Fire, poison, ice, lightning hazards |
| Terrain Info | Tooltip | `Grid.Tiles` | Cover, difficult terrain markers |

### 3.2 Buttons (Combat Actions)

| Button | Command | Behavior | Enabled When |
| --- | --- | --- | --- |
| **Attack** | `AttackCommand` | Enters attack targeting mode | `IsPlayerTurn == true` |
| **Defend** | `DefendCommand` | Takes defensive stance | `IsPlayerTurn == true` |
| **Use Ability** | `UseAbilityCommand` | Opens ability selection | `IsPlayerTurn == true` |
| **Use Item** | `UseItemCommand` | Opens consumable selection | `IsPlayerTurn == true` |
| **Move** | `MoveCommand` | Enters movement targeting mode | `IsPlayerTurn == true` |
| **End Turn** | `EndTurnCommand` | Passes turn to next participant | `IsPlayerTurn == true` |
| **Flee** | `FleeCommand` | Attempts to escape combat | `IsPlayerTurn == true` |

### 3.3 Grid Interaction Commands

| Command | Parameter | Behavior |
| --- | --- | --- |
| `CellClickedCommand` | GridPosition | Selects cell or confirms target |
| `CellHoveredCommand` | GridPosition | Updates hover preview |

### 3.4 Targeting Modes

| Mode | Visual Feedback | Valid Targets |
| --- | --- | --- |
| `None` | Normal display | N/A |
| `AttackTarget` | Enemy positions highlighted | Living enemies |
| `MovementTarget` | Adjacent empty cells highlighted | Empty walkable cells |
| `AbilityTarget` | Ability-specific range highlighted | Depends on ability |

### 3.5 Turn Order Entry Display

| Property | Type | Description |
| --- | --- | --- |
| `Name` | string | Participant name |
| `IsPlayer` | bool | Player character indicator |
| `IsCompanion` | bool | Companion indicator |
| `IsActive` | bool | Current turn highlight |

### 3.6 Boss Combat Extensions

When `IsBossFight == true`:

| Element | Type | Description |
| --- | --- | --- |
| Boss Name | Label | Large display of boss name |
| Boss HP Bar | Progress Bar | Multi-segment HP display with phase markers |
| Mechanic Warnings | Alert Panel | Telegraphed attack warnings |
| Enrage Timer | Counter | Turns until enrage |

---

## 4. Character Sheet View

**ViewModel:** `CharacterSheetViewModel.cs`

### 4.1 Identity Section

| Element | Type | Binding | Description |
| --- | --- | --- | --- |
| Character Name | Label | `CharacterName` | Survivor's name |
| Class | Label | `ClassName` | Warrior/Mystic/Adept/Ranger |
| Specialization | Label | `SpecializationName` | Formatted specialization name |
| Archetype | Label | `ArchetypeName` | Optional archetype display |

### 4.2 Core Attributes Display

| Attribute | Property | Range | Description |
| --- | --- | --- | --- |
| MIGHT | `Might` | 3-18 | Physical power, melee damage |
| FINESSE | `Finesse` | 3-18 | Agility, ranged accuracy |
| WITS | `Wits` | 3-18 | Intelligence, critical chance |
| WILL | `Will` | 3-18 | Mental fortitude, Aether |
| STURDINESS | `Sturdiness` | 3-18 | Durability, HP bonus |

### 4.3 Resource Pools

| Pool | Properties | Color | Visibility |
| --- | --- | --- | --- |
| HP | `CurrentHP`, `MaxHP`, `HPPercent` | Red (#DC143C) | Always |
| Stamina | `CurrentStamina`, `MaxStamina`, `StaminaPercent` | Green (#4CAF50) | Always |
| Aether Pool | `CurrentAP`, `MaxAP` | Purple (#9400D3) | `HasAetherPool == true` |
| Savagery | `Savagery`, `MaxSavagery` | Red-orange | `HasSavagery == true` |
| Righteous Fervor | `RighteousFervor`, `MaxRighteousFervor` | Gold | `HasRighteousFervor == true` |
| Momentum | `Momentum`, `MaxMomentum` | Cyan | `HasMomentum == true` |

### 4.4 Derived Combat Stats

| Stat | Property | Calculation |
| --- | --- | --- |
| Speed | `Speed` | 5 + FINESSE mod + (WITS mod / 2) |
| Accuracy | `Accuracy` | max(FINESSE mod, MIGHT mod) |
| Evasion | `Evasion` | 10 + FINESSE mod |
| Crit Chance | `CritChance` | 5% + WITS mod |
| Physical Defense | `PhysicalDefense` | 10 + STURDINESS mod + armor |
| Metaphysical Defense | `MetaphysicalDefense` | 10 + WILL mod |
| Attack Power | `AttackPower` | Base damage + MIGHT mod |
| Initiative | `Initiative` | WITS mod + (FINESSE mod / 2) |

### 4.5 Trauma Meters

| Meter | Properties | Warning Levels |
| --- | --- | --- |
| Psychic Stress | `PsychicStress`, `PsychicStressPercent`, `PsychicStressLevel` | Low (<25), Moderate (25-49), High (50-74), Critical (75+) |
| Corruption | `Corruption`, `CorruptionPercent`, `CorruptionLevel` | Same thresholds |
| Trauma Count | `TraumaCount`, `HasTraumas` | Number of permanent traumas |

### 4.6 Progression Section

| Element | Property | Description |
| --- | --- | --- |
| Legend Level | `Legend` | Current Legend points |
| Milestone | `Milestone` | Current milestone tier |
| XP Progress | `CurrentXP`, `XPToNextLevel`, `XPProgress` | Progress to next milestone |
| Progression Points | `ProgressionPoints` | Available PP for abilities |
| Currency | `Currency` | Dvergr Cogs |

### 4.7 Stance System

| Element | Property | Description |
| --- | --- | --- |
| Current Stance | `CurrentStanceName` | Calculated/Aggressive/Defensive/Evasive |
| Stance Description | `CurrentStanceDescription` | Modifier summary |
| Shifts Remaining | `StanceShiftsRemaining` | Stance changes this turn |

### 4.8 Equipment Summary

| Element | Property | Description |
| --- | --- | --- |
| Weapon | `EquippedWeaponName` | Current weapon or "Unarmed" |
| Armor | `EquippedArmorName` | Current armor or "Unarmored" |
| Abilities | `AbilityCount` | Number of learned abilities |
| Inventory | `InventoryCount`, `MaxInventorySize` | Item counts |

---

## 5. Dungeon Exploration View

**ViewModel:** `DungeonExplorationViewModel.cs`**Controller:** `ExplorationController`

### 5.1 Room Information Display

| Element | Type | Binding | Description |
| --- | --- | --- | --- |
| Room Name | Label | `RoomName` | Current location name |
| Room Description | TextBlock | `RoomDescription` | Narrative description |
| Atmosphere | TextBlock | `AtmosphereText` | Environmental flavor text |
| Biome | Label | `BiomeName` | Formatted biome name with color |
| Layer | Label | `LayerDisplay` | Vertical depth narrative |
| Hazard Warning | Alert | `HazardWarning` | Active hazard description |

### 5.2 Player Status HUD

| Element | Type | Binding | Description |
| --- | --- | --- | --- |
| HP Display | Label | `HPDisplay` | "{current}/{max}" format |
| Stamina Display | Label | `StaminaDisplay` | "{current}/{max}" format |
| Stress Display | Label | `StressDisplay` | "{current}/100" format |

### 5.3 Exits List

**Collection:** `AvailableExits` (ObservableCollection<ExitViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Direction | string | north/south/east/west/up/down |
| DisplayName | string | Formatted direction with icon |
| TargetRoomId | string | Destination room identifier |
| IsVertical | bool | Stair/ladder/elevator indicator |
| VerticalType | string | Type description for vertical |
| TraversalRequirements | string | Any requirements to traverse |

### 5.4 Room Features List

**Collection:** `RoomFeatures` (ObservableCollection<RoomFeatureViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Icon | string | Single character icon |
| Name | string | Feature name |
| Description | string | Feature details |
| IsInteractable | bool | Can player interact |
| FeatureType | string | Info/Hazard/Loot/NPC/Puzzle |

### 5.5 Commands

| Button | Command | Behavior | Enabled Condition |
| --- | --- | --- | --- |
| **Move** | `MoveCommand` | Move to selected exit | Exit selected |
| **Search** | `SearchRoomCommand` | Search current room | `CanSearch == true` |
| **Look** | `LookCommand` | Detailed perception check | Always |
| **Investigate** | `InvestigateCommand` | WITS skill check | Always |
| **Rest** | `RestCommand` | Opens rest dialog | Room cleared |
| **Character** | `ViewCharacterCommand` | Open character sheet | Always |
| **Inventory** | `ViewInventoryCommand` | Open inventory | Always |
| **Interact** | `InteractCommand` | Interact with feature | Feature selected |
| **Engage** | `EngageCommand` | Start combat | `HasEnemies == true` |
| **Toggle Map** | `ToggleMinimapCommand` | Show/hide minimap | Always |

### 5.6 Search Result Overlay

**Displayed when:** `IsSearchResultVisible == true`

| Element | Description |
| --- | --- |
| Result Type | Empty/WithLoot/WithSecrets/WithEncounter |
| Loot Items | List of found equipment/materials/currency |
| Secrets | Environmental discoveries |
| Encounter Warning | If search triggered combat |

**Commands:**

- `CollectLootCommand` - Collect found items
- `CloseSearchResultCommand` - Close overlay

### 5.7 Rest Confirmation Dialog

**Displayed when:** `IsRestDialogVisible == true`

| Element | Description |
| --- | --- |
| Rest Type | Sanctuary (full) vs Regular (partial + stress) |
| HP Preview | Expected HP recovery |
| Stamina Preview | Expected Stamina recovery |
| Stress Warning | Stress increase for non-sanctuary rest |

**Commands:**

- `ConfirmRestCommand` - Execute rest
- `CancelRestCommand` - Cancel dialog

---

## 6. Inventory & Equipment View

**ViewModel:** `InventoryViewModel.cs`**Controller:** `LootController` (for post-combat)

### 6.1 Display Labels

| Element | Type | Binding | Description |
| --- | --- | --- | --- |
| Title | Label | `Title` | "Inventory & Equipment" |
| Inventory Count | Label | `InventoryCountDisplay` | "{count} / {max}" |
| Consumables Count | Label | `ConsumablesCountDisplay` | "{count} / {max}" |
| Status Message | Label | `StatusMessage` | Action feedback |

### 6.2 Equipment Slots Panel

**Collection:** `EquipmentSlots` (ObservableCollection<EquipmentSlotViewModel>)

| Slot | Icon | Bindings |
| --- | --- | --- |
| Weapon | "W" | `EquippedItem`, `IsEmpty`, `EquippedItemName`, `EquippedItemStats` |
| Armor | "A" | Same as above |

### 6.3 Equipment Items List

**Collection:** `EquipmentItems` (ObservableCollection<EquipmentItemViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Name | string | Display name with quality prefix |
| Description | string | Item description |
| TypeDisplay | string | Weapon/Armor/Accessory |
| QualityName | string | Quality tier name |
| QualityColor | string | Hex color by quality |
| SlotIcon | string | Slot type indicator |
| DamageDisplay | string | Weapon damage formula |
| DefenseDisplay | string | Armor defense bonus |
| StatsSummary | string | One-line stat summary |
| BonusesDisplay | string | Special bonuses |
| HasSpecialEffect | bool | Has special ability |

**Quality Colors:**

- JuryRigged: #808080 (Gray)
- Scavenged: #FFFFFF (White)
- ClanForged: #4A90E2 (Blue)
- Optimized: #9400D3 (Purple)
- MythForged: #FFD700 (Gold)

### 6.4 Consumables List

**Collection:** `ConsumableItems` (ObservableCollection<ConsumableItemViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Name | string | Display name |
| Description | string | Item description |
| TypeDisplay | string | Medicine/Food/Utility |
| QualityColor | string | Standard (white) or Masterwork (gold) |
| IsMasterwork | bool | Enhanced quality indicator |
| EffectsDisplay | string | Effect summary |
| HPRestore | int | Total HP recovery |
| StaminaRestore | int | Total Stamina recovery |

### 6.5 Commands

| Button | Command | Parameter | Behavior |
| --- | --- | --- | --- |
| **Equip** | `EquipItemCommand` | EquipmentItemViewModel | Equip from inventory |
| **Unequip** | `UnequipSlotCommand` | EquipmentSlotViewModel | Unequip to inventory |
| **Drop** | `DropItemCommand` | InventoryItemViewModel | Remove item |
| **Use** | `UseConsumableCommand` | ConsumableItemViewModel | Apply consumable |
| **Sort by Type** | `SortByTypeCommand` | - | Sort inventory |
| **Sort by Quality** | `SortByQualityCommand` | - | Sort by quality tier |
| **Sort by Name** | `SortByNameCommand` | - | Alphabetical sort |

### 6.6 Loot Collection Mode

**Active when:** `IsInLootCollectionMode == true`

| Element | Binding | Description |
| --- | --- | --- |
| Pending Loot | `PendingLootItems` | Items to collect |
| Loot Summary | `LootSummary` | Overview text |
| Has Pending | `HasPendingLoot` | Any items remaining |

**Commands:**

- `CollectLootItemCommand` - Collect single item
- `CollectAllLootCommand` - Collect all items
- `CompleteLootCollectionCommand` - Finish and proceed

### 6.7 Equipment Comparison

**Displayed on hover:** `CurrentComparison`

| Property | Description |
| --- | --- |
| Damage Change | +/- damage comparison |
| Defense Change | +/- defense comparison |
| Stat Changes | Attribute modifier differences |

---

## 7. Character Creation View

**ViewModel:** `CharacterCreationViewModel.cs`**Controller:** `CharacterCreationController`

### 7.1 Workflow Steps

| Step | Enum Value | Title |
| --- | --- | --- |
| 1 | `Lineage` | "Choose Your Lineage" |
| 2 | `Background` | "Choose Your Background" |
| 3 | `Attributes` | "Allocate Attributes" |
| 4 | `Archetype` | "Choose Your Archetype" |
| 5 | `Summary` | "Confirm Your Survivor" |

**Note:** Specializations are unlocked via PP during gameplay, not during creation.

### 7.2 Step Navigation

| Property | Type | Description |
| --- | --- | --- |
| CurrentStep | CharacterCreationStep | Current workflow step |
| StepTitle | string | Display title for step |
| CanGoBack | bool | Can navigate to previous step |
| CanGoForward | bool | Can navigate to next step |

### 7.3 Step 1: Lineage Selection

**Collection:** `AvailableLineages` (ObservableCollection<LineageInfo>)

| Property | Type | Description |
| --- | --- | --- |
| Lineage | enum | Lineage value |
| Name | string | Display name |
| Description | string | Lineage description |

**Command:** `SelectLineageCommand` - Advances to Background step

### 7.4 Step 2: Background Selection

**Collection:** `AvailableBackgrounds` (ObservableCollection<BackgroundInfo>)

| Property | Type | Description |
| --- | --- | --- |
| Background | enum | Background value |
| Name | string | Display name |
| Description | string | Background description |
| PrimaryAttribute | string | Main attribute affected |
| AttributeColor | string | Color for attribute indicator |

**Command:** `SelectBackgroundCommand` - Advances to Attributes step

### 7.5 Step 3: Attribute Allocation

| Property | Type | Range | Description |
| --- | --- | --- | --- |
| AttributeMight | int | 3-10 | Current MIGHT value |
| AttributeFinesse | int | 3-10 | Current FINESSE value |
| AttributeWits | int | 3-10 | Current WITS value |
| AttributeWill | int | 3-10 | Current WILL value |
| AttributeSturdiness | int | 3-10 | Current STURDINESS value |
| RemainingAttributePoints | int | 0-15 | Points left to allocate |
| UseAdvancedMode | bool | - | Enable manual allocation |

**Cost System:**

- Values 1-8: 1 point per level
- Values 9-10: 2 points per level

**Per-Attribute Commands:**

| Command | Parameter | Description |
| --- | --- | --- |
| `IncreaseMightCommand` | - | Add 1 to MIGHT |
| `DecreaseMightCommand` | - | Remove 1 from MIGHT |
| (Same pattern for all 5 attributes) |  |  |

**Properties for enable state:**

- `CanIncreaseMight`, `CanDecreaseMight` (pattern for all attributes)
- `MightIncreaseCost`, `MightDecreaseCost` (cost indicators)

**Command:** `ConfirmAttributesCommand` - Advances to Archetype step

### 7.6 Step 4: Archetype Selection

**Collection:** `AvailableArchetypes` (ObservableCollection<CharacterClass>)

Values: Warrior, Mystic, Adept, Ranger

**Command:** `SelectArchetypeCommand` - Advances to Summary step

### 7.7 Step 5: Summary & Confirmation

| Element | Binding | Description |
| --- | --- | --- |
| Character Name | `CharacterName` | Editable name field (max 20 chars) |
| Summary Lineage | `SummaryLineage` | Selected lineage |
| Summary Background | `SummaryBackground` | Selected background |
| Summary Archetype | `SummaryArchetype` | Selected class |
| Summary Attributes | `SummaryAttributes` | Attribute distribution |
| Validation Errors | `ValidationErrors` | Any validation issues |

**Commands:**

- `ConfirmCharacterCommand` - Create character and start game
- `CancelCommand` - Return to main menu
- `BackCommand` - Return to previous step

---

## 8. Settings View

**ViewModel:** `SettingsViewModel.cs`

### 8.1 Graphics Settings

| Setting | Property | Type | Range/Options |
| --- | --- | --- | --- |
| Fullscreen | `IsFullscreen` | bool | Toggle |
| V-Sync | `VsyncEnabled` | bool | Toggle |
| Resolution | `SelectedResolutionIndex` | int | Index in `AvailableResolutions` |
| Target FPS | `TargetFPS` | int | 30-240 |
| Quality Level | `QualityLevel` | int | 0=Low, 1=Medium, 2=High |
| Particle Effects | `ParticleEffects` | bool | Toggle |
| Screen Shake | `ScreenShake` | bool | Toggle |

**Available Resolutions:** 1280x720, 1366x768, 1600x900, 1920x1080, 2560x1440, 3840x2160

### 8.2 Audio Settings

| Setting | Property | Type | Range |
| --- | --- | --- | --- |
| Master Volume | `MasterVolume` | float | 0.0-1.0 |
| Music Volume | `MusicVolume` | float | 0.0-1.0 |
| SFX Volume | `SFXVolume` | float | 0.0-1.0 |
| UI Volume | `UIVolume` | float | 0.0-1.0 |
| Ambient Volume | `AmbientVolume` | float | 0.0-1.0 |
| Muted | `IsMuted` | bool | Toggle |

### 8.3 Gameplay Settings

| Setting | Property | Type | Range/Options |
| --- | --- | --- | --- |
| Auto-Save | `AutoSaveEnabled` | bool | Toggle |
| Auto-Save Interval | `AutoSaveInterval` | int | 1-30 minutes |
| Show Damage Numbers | `ShowDamageNumbers` | bool | Toggle |
| Show Hit Confirmation | `ShowHitConfirmation` | bool | Toggle |
| Pause on Focus Lost | `PauseOnFocusLost` | bool | Toggle |
| Show Grid Coordinates | `ShowGridCoordinates` | bool | Toggle |
| Combat Speed | `CombatSpeed` | float | 0.5x-2.0x |
| Allow Animation Skip | `AllowAnimationSkip` | bool | Toggle |
| Show Tutorial Hints | `ShowTutorialHints` | bool | Toggle |
| Confirm End Turn | `ConfirmEndTurn` | bool | Toggle |

### 8.4 Accessibility Settings

| Setting | Property | Type | Range/Options |
| --- | --- | --- | --- |
| UI Scale | `UIScale` | float | 0.8-1.5 |
| Colorblind Mode | `ColorblindMode` | bool | Toggle |
| Colorblind Type | `ColorblindTypeIndex` | int | 0=None, 1=Protanopia, 2=Deuteranopia, 3=Tritanopia |
| Reduced Motion | `ReducedMotion` | bool | Toggle |
| High Contrast | `HighContrast` | bool | Toggle |
| Font Scale | `FontScale` | float | 0.8-1.5 |
| Show Subtitles | `ShowSubtitles` | bool | Toggle |
| Screen Reader Support | `ScreenReaderSupport` | bool | Toggle |
| Visual Audio Cues | `VisualAudioCues` | bool | Toggle |

### 8.5 Controls Settings

| Setting | Property | Type | Range/Options |
| --- | --- | --- | --- |
| Show Keyboard Hints | `ShowKeyboardHints` | bool | Toggle |
| Mouse Sensitivity | `MouseSensitivity` | float | 0.1-3.0 |
| Invert Scroll | `InvertScroll` | bool | Toggle |
| Edge Scrolling | `EdgeScrolling` | bool | Toggle |
| Double-Click Confirm | `DoubleClickConfirm` | bool | Toggle |

### 8.6 Commands

| Button | Command | Behavior |
| --- | --- | --- |
| **Save** | `SaveSettingsCommand` | Save all settings to config |
| **Reset to Defaults** | `ResetToDefaultsCommand` | Restore default values |
| **Cancel** | `CancelCommand` | Revert to saved values |
| **Back** | `BackCommand` | Return to previous view |

### 8.7 State Properties

| Property | Type | Description |
| --- | --- | --- |
| HasUnsavedChanges | bool | Any settings modified |

---

## 9. Save/Load View

**ViewModel:** `SaveLoadViewModel.cs`

### 9.1 Display Elements

| Element | Type | Binding | Description |
| --- | --- | --- | --- |
| Title | Label | - | "Save/Load Game" |
| Status Message | Label | `StatusMessage` | Operation feedback |
| Loading Indicator | Spinner | `IsLoading` | During operations |
| Save Name Input | TextBox | `NewSaveName` | For new saves |

### 9.2 Save Files List

**Collection:** `SaveFiles` (ObservableCollection<SaveFileViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| SaveName | string | Display name |
| CharacterName | string | Survivor name |
| CharacterClass | string | Class display |
| Level | int | Milestone level |
| PlayTime | string | Formatted play time |
| SaveDate | string | Formatted save date |
| IsQuickSave | bool | Quick save indicator |
| IsAutoSave | bool | Auto save indicator |

### 9.3 Selected Save Details

| Property | Binding | Description |
| --- | --- | --- |
| Selected Save | `SelectedSave` | Currently selected file |

### 9.4 Commands

| Button | Command | Behavior | Enabled Condition |
| --- | --- | --- | --- |
| **Save** | `SaveGameCommand` | Create new save | Valid name entered |
| **Load** | `LoadGameCommand` | Load selected save | Save selected |
| **Delete** | `DeleteSaveCommand` | Delete selected save | Save selected |
| **Refresh** | `RefreshSavesCommand` | Reload save list | Always |
| **Quick Save** | `QuickSaveCommand` | Overwrite quick save | Always |
| **Quick Load** | `QuickLoadCommand` | Load quick save | Quick save exists |
| **Back** | `BackCommand` | Return to previous view | Always |

---

## 10. Endgame Mode Selection View

**ViewModel:** `EndgameModeViewModel.cs`

### 10.1 Available Modes

| Mode | Description | Requirements |
| --- | --- | --- |
| NG+ | Restart with increased difficulty | Always available |
| Challenge Sector | Handcrafted extreme challenges | `ChallengeSectorsUnlocked` |
| Boss Gauntlet | Sequential boss fights | `BossGauntletUnlocked` |
| Endless Mode | Infinite wave survival | `EndlessModeUnlocked` |

### 10.2 Mode Selection

| Property | Type | Description |
| --- | --- | --- |
| SelectedMode | EndgameMode | Currently selected mode |
| ModeDescription | string | Description of selected mode |
| IsNGPlusMode | bool | NG+ mode selected |
| IsChallengeSectorMode | bool | Challenge Sector selected |
| IsBossGauntletMode | bool | Boss Gauntlet selected |
| IsEndlessMode | bool | Endless Mode selected |
| CanStartMode | bool | Selection valid |

### 10.3 NG+ Configuration

| Property | Type | Description |
| --- | --- | --- |
| SelectedNGPlusTier | int | Current tier (1-5) |
| MaxUnlockedNGPlusTier | int | Highest available tier |
| CanIncrementTier | bool | Can increase tier |
| CanDecrementTier | bool | Can decrease tier |

### 10.4 Modifiers Display

**Collection:** `ActiveModifiers` (ObservableCollection<DifficultyModifierViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Name | string | Modifier name |
| Description | string | Effect description |
| DisplayValue | string | "+50%" or "2x" format |
| ValueColor | string | Red for detrimental, green for beneficial |
| CategoryIcon | string | Icon by category |

### 10.5 Rewards Display

**Collection:** `RewardMultipliers` (ObservableCollection<RewardMultiplierViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Type | string | "Legend Points", "Loot Quality" |
| Multiplier | float | Reward multiplier |
| DisplayText | string | "Type: x1.5" format |
| IsBonus | bool | Multiplier > 1.0 |
| RewardColor | string | Gold for bonus |

### 10.6 Challenge Sectors

**Collection:** `ChallengeSectors` (ObservableCollection<ChallengeSectorViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Name | string | Sector name |
| Description | string | Sector description |
| DifficultyTier | string | Moderate/Hard/Extreme/Near-Impossible |
| DifficultyDisplay | string | "Extreme (2.5x)" format |
| IsCompleted | bool | Previously completed |
| AttemptCount | int | Number of attempts |
| UniqueRewardName | string | Special reward |

### 10.7 Boss Gauntlet

**Collection:** `GauntletSequences` (ObservableCollection<GauntletSequenceViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| SequenceName | string | Gauntlet name |
| Description | string | Description |
| BossCount | int | Number of bosses |
| MaxFullHeals | int | Healing limit |
| MaxRevives | int | Revive limit |
| ResourceLimits | string | "3 Heals / 1 Revive" format |
| TitleReward | string | Unlockable title |

### 10.8 Commands

| Button | Command | Behavior |
| --- | --- | --- |
| **Back** | `BackCommand` | Return to menu |
| **Start** | `StartModeCommand` | Begin selected mode |
| **+** | `IncrementTierCommand` | Increase NG+ tier |
| **-** | `DecrementTierCommand` | Decrease NG+ tier |
| **Select Mode** | `SelectModeCommand` | Change mode selection |

---

## 11. Victory Screen

**ViewModel:** `VictoryScreenViewModel.cs`**Controller:** `VictoryController`

### 11.1 Header Section

| Element | Binding | Value |
| --- | --- | --- |
| Title | `Title` | "VICTORY" |
| Subtitle | `Subtitle` | "The Sector has been conquered!" |
| Rating Message | `RatingMessage` | Based on performance |

**Rating Messages:**

- S: "Legendary Performance!"
- A: "Excellent Run!"
- B: "Well Done!"
- C: "Good Effort!"
- Default: "Sector Conquered!"

### 11.2 Character Summary

| Element | Binding | Description |
| --- | --- | --- |
| Survivor Name | `SurvivorName` | Character name |
| Details | `SurvivorDetails` | "Class (Specialization)" |

### 11.3 Statistics Display

| Category | Properties |
| --- | --- |
| **Progression** | `FinalLevel`, `TotalLegendEarned`, `LegendToHallOfLegends`, `ProgressionPointsSpent` |
| **Trauma** | `FinalPsychicStress`, `FinalCorruption`, `TraumaCount` |
| **Combat** | `TotalKills` |
| **Exploration** | `RoomsExplored`, `TotalRooms`, `SecretsFound`, `ExplorationPercentage` |
| **Economy** | `FinalCurrency` |
| **Time** | `FormattedPlaytime` |
| **Performance** | `PerformanceRating` (S/A/B/C) |

### 11.4 Mode Information

| Property | Description |
| --- | --- |
| NGPlusTier | NG+ tier if applicable |
| IsNGPlusRun | Was this an NG+ run |
| ChallengeSectorName | Challenge sector if applicable |
| IsChallengeSectorRun | Was this a challenge run |

### 11.5 Commands

| Button | Command | Behavior |
| --- | --- | --- |
| **Continue to Endgame** | `ProceedToEndgameCommand` | Open endgame selection |
| **Return to Menu** | `ReturnToMenuCommand` | Return to main menu |

---

## 12. Death Screen

**ViewModel:** `DeathScreenViewModel.cs`**Controller:** `DeathController`

### 12.1 Header Section

| Element | Binding | Value |
| --- | --- | --- |
| Title | `Title` | "THE SAGA ENDS" |
| Subtitle | `Subtitle` | "Your journey has come to an end..." |

### 12.2 Character Summary

| Element | Binding | Description |
| --- | --- | --- |
| Survivor Name | `SurvivorName` | Character name |
| Details | `SurvivorDetails` | "Class (Specialization)" |
| Cause of Death | `CauseOfDeath` | How character died |

### 12.3 Statistics Display

| Property | Description |
| --- | --- |
| FinalLevel | Milestone reached |
| TotalLegendEarned | Legend accumulated |
| LegendToHallOfLegends | Legend transferred to meta-progression |
| ProgressionPointsSpent | PP spent on abilities |
| FinalPsychicStress | Final stress level |
| FinalCorruption | Final corruption level |
| TraumaCount | Permanent traumas |
| TotalKills | Enemies defeated |
| RoomsExplored | Rooms visited |
| FinalCurrency | Currency held |
| FormattedPlaytime | Total play time |

### 12.4 Meta-Progression Message

| Element | Binding | Description |
| --- | --- | --- |
| Hall of Legends | `HallOfLegendsMessage` | Legend transferred message |

### 12.5 Commands

| Button | Command | Behavior |
| --- | --- | --- |
| **Return to Menu** | `ReturnToMenuCommand` | Return to main menu |

---

## 13. Minimap View

**ViewModel:** `MinimapViewModel.cs`

### 13.1 Map Display

| Property | Type | Description |
| --- | --- | --- |
| RoomNodes | ObservableCollection | Room positions to display |
| Connections | ObservableCollection | Lines between rooms |
| ZoomLevel | float | 0.5-3.0 scale |
| PanOffset | Point | View offset |
| DisplayedLayer | VerticalLayer | Current depth layer |

### 13.2 Room Node Properties

| Property | Type | Description |
| --- | --- | --- |
| Position | Point | Canvas position |
| IsCurrentRoom | bool | Player location |
| IsExplored | bool | Room visited |
| HasUpConnection | bool | Stair up indicator |
| HasDownConnection | bool | Stair down indicator |
| RoomType | string | Start/Boss/Normal/Combat/Puzzle/Fog |
| BiomeColor | string | Biome-specific color |
| RoomName | string | Tooltip name |
| IsSanctuary | bool | Safe room indicator |
| HasEnemies | bool | Uncleared enemies |

### 13.3 Layer Information

| Property | Description |
| --- | --- |
| LayerDisplayName | "Ground Level", "Deep Roots", etc. |
| LayerDepthDisplay | Narrative depth description |
| CanGoUp | Higher layer available |
| CanGoDown | Lower layer available |
| AvailableLayers | All explored layers |

### 13.4 Statistics

| Property | Description |
| --- | --- |
| ExploredRoomCount | Rooms visited |
| TotalRoomCount | Total in dungeon |
| ExplorationProgress | "X/Y rooms explored" |

### 13.5 Commands

| Button | Command | Behavior |
| --- | --- | --- |
| **+** | `ZoomInCommand` | Increase zoom (max 3.0) |
| **-** | `ZoomOutCommand` | Decrease zoom (min 0.5) |
| **Reset** | `ResetViewCommand` | Reset zoom and pan |
| **Up** | `LayerUpCommand` | Go to higher layer |
| **Down** | `LayerDownCommand` | Go to lower layer |
| **Center** | `CenterOnPlayerCommand` | Center on player |

---

## 14. Help Browser View

**ViewModel:** `HelpViewModel.cs`

### 14.1 Search and Filter

| Element | Binding | Description |
| --- | --- | --- |
| Search Query | `SearchQuery` | Text search input |
| Category Filter | `SelectedCategory` | Category dropdown |
| Categories | `Categories` | Available categories |
| Topic Count | `TopicCount` | Number of displayed topics |

### 14.2 Help Topics List

**Collection:** `HelpTopics` (ObservableCollection<HelpTopicViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Key | string | Unique identifier |
| Title | string | Topic title |
| Description | string | Brief description |
| DetailedHelp | string | Full help text |
| Category | string | Topic category |
| Icon | string | Topic icon |
| Shortcut | string | Keyboard shortcut if applicable |
| IsNew | bool | New feature indicator |
| IsSelected | bool | Current selection state |
| HasDetailedHelp | bool | Has expanded content |
| HasShortcut | bool | Has keyboard shortcut |
| HasIcon | bool | Has icon |

### 14.3 Topic Detail

| Element | Binding | Displayed When |
| --- | --- | --- |
| Selected Topic | `SelectedTopic` | `HasSelectedTopic == true` |

### 14.4 Commands

| Button | Command | Behavior |
| --- | --- | --- |
| **Clear** | `ClearSearchCommand` | Reset search and filter |
| **Back** | `BackCommand` | Return to previous view |

---

## 15. Specialization Tree View

**ViewModel:** `SpecializationTreeViewModel.cs`**Controller:** `ProgressionController`

### 15.1 Specialization Info

| Property | Binding | Description |
| --- | --- | --- |
| Name | `SpecializationName` | Specialization display name |
| Description | `SpecializationDescription` | Specialization overview |
| Tagline | `SpecializationTagline` | Flavor tagline |
| Icon | `SpecializationIcon` | Emoji icon |

### 15.2 Resources Display

| Property | Description |
| --- | --- |
| AvailablePP | Progression Points to spend |
| PPSpentInTree | Points already invested |

### 15.3 Ability Tiers

**Collections:**

- `FoundationTier` (Tier 1) - 3 PP to unlock
- `CoreTier` (Tier 2) - 5 PP to unlock, requires 6 PP in tree
- `AdvancedTier` (Tier 3) - 8 PP to unlock, requires 15 PP in tree
- `MasteryTier` (Tier 4) - 12 PP to unlock, requires 24 PP in tree

### 15.4 Ability Node Properties

| Property | Type | Description |
| --- | --- | --- |
| Name | string | Ability name |
| Description | string | Ability description |
| MechanicalSummary | string | Effect summary |
| TierLevel | int | 1-4 tier |
| TierName | string | Foundation/Core/Advanced/Mastery |
| IsUnlocked | bool | Player has ability |
| CurrentRank | int | Current rank (1-3) |
| MaxRank | int | Maximum rank |
| UnlockCost | int | PP to unlock |
| RankUpCost | int | PP to rank up |
| CanUnlock | bool | Can player unlock |
| CanRankUp | bool | Can player rank up |
| PrerequisiteText | string | Requirements display |
| AbilityType | string | Active/Passive/Reaction |
| ActionType | string | Standard/Bonus/Free |
| TargetType | string | Self/Single/AOE |
| ResourceCostText | string | Stamina/Stress/etc. costs |
| CooldownText | string | Cooldown description |
| RankDisplay | string | "2/3" format |
| ActionButtonText | string | "Unlock (3 PP)" or "Rank Up (5 PP)" |
| BackgroundColor | string | Unlocked vs locked |
| BorderColor | string | Unlocked/Available/Locked |

### 15.5 Progression Mode

**Active when:** `IsInProgressionMode == true` (milestone reached)

| Property | Description |
| --- | --- |
| ProgressionSummary | Level-up summary text |
| CanCompleteProgression | Can finish spending |

### 15.6 Commands

| Button | Command | Parameter | Behavior |
| --- | --- | --- | --- |
| **Unlock** | `UnlockAbilityCommand` | AbilityNodeViewModel | Unlock ability |
| **Rank Up** | `RankUpAbilityCommand` | AbilityNodeViewModel | Increase rank |
| **Select** | `SelectNodeCommand` | AbilityNodeViewModel | View details |
| **Complete** | `CompleteProgressionCommand` | - | Finish progression |
| **Skip** | `SkipProgressionCommand` | - | Save PP for later |

---

## 16. Meta-Progression View

**ViewModel:** `MetaProgressionViewModel.cs`

### 16.1 Statistics Summary

| Property | Description |
| --- | --- |
| TotalAchievements | Total achievements available |
| UnlockedAchievements | Achievements earned |
| CompletionPercentage | Achievement progress (0.0-1.0) |
| TotalAchievementPoints | Total points earned |
| TotalRuns | Campaign attempts |
| SuccessfulRuns | Victories |
| CurrentMilestoneTier | Account tier |
| CurrentTierName | "Initiate", etc. |
| HighestNewGamePlus | Max NG+ completed |
| HighestEndlessWave | Best endless score |
| TotalPlayTimeDisplay | Formatted total time |

### 16.2 Achievements List

**Collection:** `Achievements` (ObservableCollection<AchievementViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Name | string | Achievement name |
| Description | string | Achievement description |
| DisplayName | string | Hidden for secret achievements |
| DisplayDescription | string | Hidden for secrets |
| Category | string | Milestone/Combat/Exploration/Challenge/Narrative |
| IsUnlocked | bool | Earned status |
| IsSecret | bool | Hidden achievement |
| AchievementPoints | int | Point value |
| Progress | float | Current progress (0.0-1.0) |
| CurrentValue | int | Progress numerator |
| TargetValue | int | Progress denominator |
| RewardDescription | string | "+X Achievement Points" |
| IconText | string | "★" or "☆" |
| IconColor | string | Category-based color |
| BackgroundColor | string | Unlocked vs locked |
| BorderColor | string | Status indicator |

### 16.3 Account Unlocks

**Collection:** `Unlocks` (ObservableCollection<UnlockViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Name | string | Unlock name |
| Description | string | Unlock description |
| Requirement | string | How to unlock |
| Type | AccountUnlockType | Convenience/Variety/Progression/Cosmetic/Knowledge |
| IsUnlocked | bool | Unlock status |
| TypeDisplay | string | Type name |
| TypeColor | string | Type-based color |
| TypeIcon | string | Type emoji |

### 16.4 Cosmetics

**Collection:** `Cosmetics` (ObservableCollection<CosmeticViewModel>)

| Property | Type | Description |
| --- | --- | --- |
| Name | string | Cosmetic name |
| Description | string | Cosmetic description |
| Type | CosmeticType | Title/Portrait/UITheme/etc. |
| UnlockRequirement | string | How to unlock |
| TypeDisplay | string | Type name |
| TypeColor | string | Type-based color |
| TypeIcon | string | Type emoji |

### 16.5 Filter Controls

| Property | Description |
| --- | --- |
| FilterCategory | Selected category filter |
| Categories | Available categories: All, Milestone, Combat, Exploration, Challenge, Narrative |
| SelectedAchievement | Currently selected achievement |

### 16.6 Commands

| Button | Command | Behavior |
| --- | --- | --- |
| **Back** | `BackCommand` | Return to menu |
| **Refresh** | `RefreshCommand` | Reload data |

---

## 17. Global Keyboard Shortcuts

**Registered in:** `MainWindowViewModel.RegisterKeyboardShortcuts()`

| Key | Command | Description |
| --- | --- | --- |
| Escape | NavigateToMenu | Return to main menu |
| F1 | NavigateToHelp | Open help browser |
| F5 | QuickSave | Save to quick save slot |
| F9 | QuickLoad | Load quick save |
| M | NavigateToDungeon | Open dungeon map |
| C | NavigateToCharacter | Open character sheet |
| I | NavigateToInventory | Open inventory |
| T | NavigateToSpecializationTree | Open ability tree |
| L | NavigateToSaveLoad | Open save/load menu |
| Backspace | NavigateBack | Go to previous view |

---

## 18. UI Services & Controllers

### 18.1 Navigation Service

**Interface:** `INavigationService`

| Method | Description |
| --- | --- |
| `NavigateTo<T>()` | Navigate to ViewModel type |
| `NavigateTo(ViewModelBase)` | Navigate to instance |
| `NavigateBack()` | Return to previous view |
| `RegisterViewModelFactory<T>()` | Register factory for type |

### 18.2 Controllers

| Controller | Responsibility |
| --- | --- |
| `MainMenuController` | Menu navigation, game start |
| `CharacterCreationController` | Character creation workflow |
| `GameStateController` | Central game state management |
| `CombatController` | Combat flow and transitions |
| `ExplorationController` | Room navigation, events |
| `LootController` | Post-combat loot collection |
| `ProgressionController` | Level-up and PP spending |
| `VictoryController` | Victory state and rewards |
| `DeathController` | Death handling and meta-progression |

### 18.3 Key Services

| Service | Responsibility |
| --- | --- |
| `ISpriteService` | 16x16 pixel art sprite loading |
| `IDialogService` | Modal dialogs and confirmations |
| `IAudioService` | Sound effects and music |
| `IConfigurationService` | Settings persistence |
| `ISaveGameService` | Save/load operations |
| `ITooltipService` | Help topic management |
| `IKeyboardShortcutService` | Shortcut registration |
| `IAnimationService` | Combat animations |
| `IStatusEffectIconService` | Status effect icons |
| `IHazardVisualizationService` | Environmental hazard display |

---

## Document History

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | Nov 2024 | Initial comprehensive GUI specification |

---

*This document is part of the Rune & Rust technical documentation suite.*