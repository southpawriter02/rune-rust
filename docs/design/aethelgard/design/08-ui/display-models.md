---
id: SPEC-UI-DISPLAY-MODELS
title: "Display Models — UI-Agnostic Data Records"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Core/UI/DisplayModels/"
    status: Planned
---

# Display Models — UI-Agnostic Data Records

---

## 1. Overview

Display models are **immutable data records** that package game state for UI consumption. They contain:
- Pre-formatted strings (no UI-side formatting logic)
- Calculated values (percentages, display text)
- Visual hints (colors, icons) without UI framework dependencies

**Design Principles:**
- **Serializable**: Can be logged, cached, or transmitted
- **Immutable**: `record` types prevent accidental mutation
- **Framework-Agnostic**: No Avalonia, no Console dependencies
- **Pre-Computed**: Minimize UI-layer calculations

---

## 2. Combat Display Models

### 2.1 CombatDisplay

Root model for combat view state.

```csharp
public record CombatDisplay(
    string Title,                              // "Tactical Combat"
    string StatusMessage,                      // "Your turn — Select action"
    int Round,                                 // Current round number
    string CurrentPhase,                       // "Player Turn", "Enemy Turn"
    bool IsPlayerTurn,
    IReadOnlyList<CombatantDisplay> Allies,
    IReadOnlyList<CombatantDisplay> Enemies,
    IReadOnlyList<CombatLogEntry> CombatLog,
    IReadOnlyList<TurnOrderEntry> TurnOrder,
    GridDisplay Grid,
    IReadOnlyList<SmartCommand> AvailableActions,
    bool IsBossFight,
    BossDisplay? Boss
);
```

### 2.2 CombatantDisplay

Display data for a single combatant.

```csharp
public record CombatantDisplay(
    Guid Id,
    string Name,
    string SpriteKey,                          // "warrior_male", "goblin_scout"
    int CurrentHP,
    int MaxHP,
    double HPPercent,                          // 0.0 - 1.0
    string HPDisplay,                          // "45/60"
    int CurrentStamina,
    int MaxStamina,
    double StaminaPercent,
    bool IsPlayer,
    bool IsCompanion,
    bool IsEnemy,
    bool IsAlive,
    bool IsSelected,
    GridPosition Position,
    IReadOnlyList<StatusEffectDisplay> StatusEffects,
    string StanceDisplay                       // "Aggressive", "Defensive"
);
```

### 2.3 GridDisplay

Tactical battlefield grid state.

```csharp
public record GridDisplay(
    int Columns,                               // 6
    int Rows,                                  // 4
    IReadOnlyList<GridCellDisplay> Cells,
    GridPosition? SelectedCell,
    GridPosition? HoveredCell,
    IReadOnlyList<GridPosition> HighlightedCells,
    HighlightType HighlightType                // None, Movement, Attack, Ability
);

public record GridCellDisplay(
    GridPosition Position,
    bool IsPlayerZone,                         // Rows 0-1
    bool IsEnemyZone,                          // Rows 2-3
    bool IsOccupied,
    Guid? OccupantId,
    CoverType CoverType,                       // None, Physical, Metaphysical
    bool HasHazard,
    string? HazardType,                        // "Fire", "Poison", "Ice"
    bool IsValidTarget,
    bool IsSelected,
    bool IsHovered
);

public enum HighlightType { None, Movement, Attack, Ability }
```

### 2.4 StatusEffectDisplay

```csharp
public record StatusEffectDisplay(
    string Name,                               // "[Bleeding]"
    string Icon,                               // "🩸"
    string Description,                        // "Losing 1d6 HP per turn"
    int RemainingDuration,                     // Turns remaining
    string DurationDisplay,                    // "3 turns"
    bool IsDebuff,
    bool IsBuff,
    string ColorHex                            // "#FF0000" for debuffs
);
```

### 2.5 BossDisplay

```csharp
public record BossDisplay(
    string Name,
    string Title,                              // "The Corrupted Warden"
    int CurrentHP,
    int MaxHP,
    double HPPercent,
    int CurrentPhase,                          // Boss phase (1, 2, 3)
    int TotalPhases,
    IReadOnlyList<double> PhaseThresholds,     // [0.75, 0.50, 0.25]
    string MechanicWarning,                    // "Preparing Devastating Strike!"
    int? EnrageTimer                           // Turns until enrage
);
```

---

## 3. Character Display Models

### 3.1 CharacterDisplay

Full character sheet data.

```csharp
public record CharacterDisplay(
    // Identity
    string Name,
    string ClassName,                          // "Warrior"
    string SpecializationName,                 // "Atgeir-Wielder"
    string? ArchetypeName,
    string SpriteKey,
    
    // Attributes
    AttributeDisplay Might,
    AttributeDisplay Finesse,
    AttributeDisplay Wits,
    AttributeDisplay Will,
    AttributeDisplay Sturdiness,
    
    // Resources
    ResourceDisplay HP,
    ResourceDisplay Stamina,
    ResourceDisplay? AetherPool,               // Mystic only
    ResourceDisplay? SpecialResource,          // Savagery, Fervor, Momentum
    
    // Trauma
    TraumaDisplay Trauma,
    
    // Progression
    ProgressionDisplay Progression,
    
    // Combat Stats
    DerivedStatsDisplay DerivedStats,
    
    // Equipment Summary
    string EquippedWeaponName,
    string EquippedArmorName,
    int AbilityCount,
    int InventoryCount,
    int MaxInventorySize
);
```

### 3.2 AttributeDisplay

```csharp
public record AttributeDisplay(
    string Name,                               // "MIGHT"
    int Value,                                 // 7
    int Modifier,                              // +2
    string ModifierDisplay,                    // "+2"
    string ColorHex                            // "#DC143C" for MIGHT
);
```

### 3.3 ResourceDisplay

```csharp
public record ResourceDisplay(
    string Name,                               // "HP"
    int Current,
    int Max,
    double Percent,                            // 0.0 - 1.0
    string Display,                            // "45/60"
    string ColorHex,                           // "#DC143C" for HP
    bool IsLow                                 // < 25%
);
```

### 3.4 TraumaDisplay

```csharp
public record TraumaDisplay(
    int PsychicStress,
    int MaxPsychicStress,                      // 100
    double PsychicStressPercent,
    string PsychicStressLevel,                 // "Low", "Moderate", "High", "Critical"
    int Corruption,
    int MaxCorruption,
    double CorruptionPercent,
    string CorruptionLevel,
    int TraumaCount,
    IReadOnlyList<string> TraumaNames
);
```

### 3.5 ProgressionDisplay

```csharp
public record ProgressionDisplay(
    int Legend,                                // Total XP
    int Milestone,                             // Current level
    int CurrentXP,
    int XPToNextLevel,
    double XPProgress,                         // 0.0 - 1.0
    string XPDisplay,                          // "250/500"
    int ProgressionPoints,                     // Unspent PP
    int Currency                               // Dvergr Cogs
);
```

### 3.6 DerivedStatsDisplay

```csharp
public record DerivedStatsDisplay(
    int Speed,
    int Accuracy,
    int Evasion,
    int CritChance,
    int PhysicalDefense,
    int MetaphysicalDefense,
    int AttackPower,
    int Initiative
);
```

---

## 4. Room Display Models

### 4.1 RoomDisplay

```csharp
public record RoomDisplay(
    string Name,                               // "Abandoned Workshop"
    string Description,                        // Narrative text
    string AtmosphereText,                     // Environmental flavor
    string BiomeName,                          // "Svartalfheim"
    string BiomeColorHex,                      // Biome accent color
    string LayerDisplay,                       // "Depth: 3"
    string? HazardWarning,                     // Active hazard description
    bool IsCleared,
    bool IsSearched,
    bool CanRest,
    IReadOnlyList<ExitDisplay> Exits,
    IReadOnlyList<RoomFeatureDisplay> Features,
    IReadOnlyList<NPCDisplay> NPCs,
    IReadOnlyList<EnemyDisplay> Enemies,
    IReadOnlyList<ItemDisplay> ItemsOnGround
);
```

### 4.2 ExitDisplay

```csharp
public record ExitDisplay(
    string Direction,                          // "north"
    string DisplayName,                        // "North ↑"
    string Icon,                               // "↑" or "⬆"
    bool IsVertical,                           // Stairs, ladder
    string? VerticalType,                      // "Stairs Down"
    bool IsLocked,
    string? LockDescription,                   // "Requires Iron Key"
    bool IsHidden,
    Guid TargetRoomId
);
```

### 4.3 RoomFeatureDisplay

```csharp
public record RoomFeatureDisplay(
    Guid Id,
    string Name,
    string Description,
    string Icon,                               // "⚙", "📜", "💀"
    string FeatureType,                        // "Mechanism", "Loot", "Info"
    bool IsInteractable,
    bool IsInteracted,
    string? InteractionHint,                   // "Pull lever (MIGHT DC 4)"
    string? SkillCheckDisplay                  // "(MIGHT DC 4)"
);
```

### 4.4 NPCDisplay

```csharp
public record NPCDisplay(
    Guid Id,
    string Name,
    string Title,                              // "Wandering Merchant"
    string SpriteKey,
    bool CanTalk,
    bool HasQuest,
    bool HasShop,
    string FactionDisplay                      // "Rusthaven"
);
```

---

## 5. Inventory Display Models

### 5.1 InventoryDisplay

```csharp
public record InventoryDisplay(
    IReadOnlyList<EquipmentSlotDisplay> EquipmentSlots,
    IReadOnlyList<EquipmentItemDisplay> Equipment,
    IReadOnlyList<ConsumableItemDisplay> Consumables,
    int EquipmentCount,
    int MaxEquipmentSlots,
    int ConsumableCount,
    int MaxConsumableSlots,
    string EquipmentCountDisplay,              // "12/20"
    string ConsumableCountDisplay              // "5/10"
);
```

### 5.2 EquipmentSlotDisplay

```csharp
public record EquipmentSlotDisplay(
    string SlotType,                           // "Weapon", "Armor"
    string SlotIcon,                           // "⚔", "🛡"
    bool IsEmpty,
    EquipmentItemDisplay? EquippedItem
);
```

### 5.3 EquipmentItemDisplay

```csharp
public record EquipmentItemDisplay(
    Guid Id,
    string Name,                               // "Iron Longsword"
    string FullName,                           // "[Uncommon] Iron Longsword"
    string Description,
    string TypeDisplay,                        // "Weapon - Sword"
    string QualityName,                        // "ClanForged"
    string QualityColorHex,                    // "#4A90E2"
    string SlotIcon,
    string? DamageDisplay,                     // "2d8+3"
    string? DefenseDisplay,                    // "+4 Soak"
    string StatsSummary,                       // "2d8+3, +2 Accuracy"
    string? BonusesDisplay,                    // "+1 MIGHT"
    bool HasSpecialEffect,
    string? SpecialEffectName,
    bool IsEquipped
);
```

### 5.4 ConsumableItemDisplay

```csharp
public record ConsumableItemDisplay(
    Guid Id,
    string Name,
    string Description,
    string TypeDisplay,                        // "Medicine"
    int Quantity,
    string QuantityDisplay,                    // "x3"
    bool IsMasterwork,
    string EffectsDisplay,                     // "Restore 2d6 HP"
    int? HPRestore,
    int? StaminaRestore
);
```

---

## 6. Dialogue Display Models

### 6.1 DialogueDisplay

```csharp
public record DialogueDisplay(
    string SpeakerName,
    string SpeakerTitle,
    string SpeakerSpriteKey,
    string DialogueText,
    IReadOnlyList<DialogueOptionDisplay> Options,
    bool IsLeavable
);
```

### 6.2 DialogueOptionDisplay

```csharp
public record DialogueOptionDisplay(
    int Index,                                 // 1-9 for hotkeys
    string Text,                               // The option text
    string DisplayText,                        // Full formatted display
    bool RequiresSkillCheck,
    string? SkillCheckAttribute,               // "MIGHT", "WITS"
    int? SkillCheckDC,
    double? SuccessChance,                     // 0.0 - 1.0
    string? SuccessChanceDisplay,              // "(65%)"
    bool LeadsToTrade,
    bool LeadsToQuest
);
```

---

## 7. Skill Check Display Models

### 7.1 SkillCheckDisplay

```csharp
public record SkillCheckDisplay(
    string Attribute,                          // "MIGHT"
    int DC,
    int PoolSize,                              // Number of dice
    string PoolDisplay,                        // "8d10"
    double SuccessChance,
    string SuccessChanceDisplay,               // "(72%)"
    bool HasBonus,
    string? BonusSource                        // "Masterwork Tools +1"
);
```

### 7.2 SkillCheckResultDisplay

```csharp
public record SkillCheckResultDisplay(
    IReadOnlyList<int> DiceRolls,              // [10, 8, 7, 5, 4, 3, 2, 1]
    string DiceDisplay,                        // "Roll: [10, 8, 7, 5, 4, 3, 2, 1]"
    int Successes,
    int Botches,
    int NetSuccesses,
    string ResultSummary,                      // "→ 2 Successes, 1 Botch = 1 Net"
    bool IsSuccess,
    bool IsCritical,
    bool IsFumble,
    string OutcomeLabel,                       // "✓ SUCCESS" or "★ CRITICAL" or "✗ FUMBLE"
    string OutcomeColorHex                     // Green, Gold, Red
);
```

---

## 8. Loot Display Models

### 8.1 LootDisplay

```csharp
public record LootDisplay(
    string SourceName,                         // "Goblin Scout" or "Rusty Chest"
    IReadOnlyList<LootItemDisplay> Items,
    int Currency,                              // Gold found
    string CurrencyDisplay,                    // "Gold: 25"
    bool HasItems
);
```

### 8.2 LootItemDisplay

```csharp
public record LootItemDisplay(
    Guid Id,
    string Name,
    string QualityName,
    string QualityColorHex,
    string TypeIcon,                           // "⚔", "🛡", "💊"
    string StatPreview,                        // "2d8+3" or "+4 Soak"
    bool IsNew
);
```

---

## 9. Color Constants

### 9.1 Quality Colors

```csharp
public static class QualityColors
{
    public const string JuryRigged = "#808080";    // Gray
    public const string Scavenged = "#FFFFFF";     // White
    public const string ClanForged = "#4A90E2";    // Blue
    public const string Optimized = "#9400D3";     // Purple
    public const string MythForged = "#FFD700";    // Gold
}
```

### 9.2 Resource Colors

```csharp
public static class ResourceColors
{
    public const string HP = "#DC143C";            // Red
    public const string Stamina = "#4CAF50";       // Green
    public const string AetherPool = "#9400D3";    // Purple
    public const string Stress = "#FF8C00";        // Orange
    public const string Corruption = "#8B0000";    // Dark Red
}
```

### 9.3 Attribute Colors

```csharp
public static class AttributeColors
{
    public const string Might = "#DC143C";         // Red
    public const string Finesse = "#00CED1";       // Cyan
    public const string Wits = "#9400D3";          // Purple
    public const string Will = "#FFD700";          // Gold
    public const string Sturdiness = "#8B4513";    // Brown
}
```

---

## 10. Display Model Factory

### 10.1 IDisplayModelFactory

```csharp
public interface IDisplayModelFactory
{
    CombatDisplay CreateCombatDisplay(CombatState state);
    CharacterDisplay CreateCharacterDisplay(PlayerCharacter character);
    RoomDisplay CreateRoomDisplay(Room room, PlayerCharacter player);
    InventoryDisplay CreateInventoryDisplay(PlayerCharacter character);
    DialogueDisplay CreateDialogueDisplay(DialogueState state);
    LootDisplay CreateLootDisplay(LootResult loot);
}
```

### 10.2 Purpose

The factory:
- Transforms domain models → display models
- Pre-computes all formatting
- Applies localization (future)
- Handles null/missing data gracefully

---

## 11. Implementation Status

| Model | File Path | Status |
|-------|-----------|--------|
| CombatDisplay | `RuneAndRust.Core/UI/Models/CombatDisplay.cs` | ❌ Planned |
| CharacterDisplay | `RuneAndRust.Core/UI/Models/CharacterDisplay.cs` | ❌ Planned |
| RoomDisplay | `RuneAndRust.Core/UI/Models/RoomDisplay.cs` | ❌ Planned |
| InventoryDisplay | `RuneAndRust.Core/UI/Models/InventoryDisplay.cs` | ❌ Planned |
| DialogueDisplay | `RuneAndRust.Core/UI/Models/DialogueDisplay.cs` | ❌ Planned |
| IDisplayModelFactory | `RuneAndRust.Engine/UI/DisplayModelFactory.cs` | ❌ Planned |
