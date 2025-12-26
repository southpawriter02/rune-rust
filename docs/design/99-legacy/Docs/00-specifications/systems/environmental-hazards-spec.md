# Environmental Hazards System Specification

> **Specification ID**: SPEC-SYSTEM-013
> **Version**: 1.0.0
> **Status**: Canonical
> **Last Updated**: 2025-11-28
> **Layer**: 0 (Canonical Reference)

---

## 1. Overview

### 1.1 Purpose

This specification documents the complete Environmental Hazards system, including dynamic hazards, cover mechanics, traps, weather effects, ambient conditions, destructible terrain, and environmental manipulation. The system provides tactical depth through environmental interaction, enabling players to weaponize, avoid, or endure environmental dangers.

### 1.2 Design Philosophy

1. **Telegraphed Dangers**: No sudden deaths; hazards have clear triggers and notifications
2. **Player Agency**: Can weaponize, avoid, or tank through environment
3. **Specialization Synergy**: Analysts detect hazards; Controllers manipulate them
4. **Narrative Integration**: Environmental hazards tie to world lore (800-year decay, system failures)
5. **Mechanics Depth**: Multiple interaction layers (destruction, cover, manipulation, stress)
6. **Modular Design**: Services compose together for flexible environmental scenes

### 1.3 Service Architecture

| Service | Version | Purpose |
|---------|---------|---------|
| `HazardService` | v0.6 | Core hazard damage/stress/checks |
| `HazardDatabase` | v0.16 | Pre-seeded hazard templates |
| `HazardSpawner` | v0.11 | Room hazard population |
| `EnvironmentalCombatService` | v0.22 | Central orchestration hub |
| `EnvironmentalObjectService` | v0.22.1 | Destructible objects and cover |
| `EnvironmentalStressService` | v0.15 | Trauma-based environmental stress |
| `EnvironmentalManipulationService` | v0.22.3 | Push/pull, collapses, env kills |
| `AmbientConditionService` | v0.22.2 | Room-wide persistent effects |
| `WeatherEffectService` | v0.22.2 | Dynamic weather conditions |
| `TrapService` | v0.20 | Battlefield trap management |
| `CoverService` | v0.20.2 | Physical/metaphysical cover |
| `DestructionService` | v0.13 | Terrain destruction and world state |

---

## 2. Dynamic Hazard System

### 2.1 Hazard Types

#### Legacy Hazard Types (v0.6)

| Type | Description | Mechanic |
|------|-------------|----------|
| `GenericDamage` | Simple flat damage | Backward compatibility |
| `ToxicFumes` | Damage + Stress per turn | Unavoidable |
| `ToxicSludge` | Dice-based damage | Drainable |
| `UnstableFlooring` | FINESSE check or fall | DC-based |
| `ElectricalHazard` | Variable damage | Contact-based |
| `Radiation` | Flat damage per turn | Cumulative |
| `Fire` | Damage + spread potential | Progressive |
| `Ice` | Movement penalty + cold damage | Terrain effect |
| `Darkness` | Accuracy penalty + stress | Sensory |
| `Vacuum` | Immediate lethal | Protection required |

#### Dynamic Hazard Types (v0.11-v0.16)

| Type | Trigger | Damage | Effects | Area |
|------|---------|--------|---------|------|
| `SteamVent` | Automatic | 15 Fire | - | 1 tile |
| `LivePowerConduit` | OnMovement | 2d6 Lightning | - | Single |
| `UnstableCeiling` | OnLoudAction | 25 Physical | [Stunned] | 3 tiles |
| `ToxicSporeCloud` | Automatic | 1d4 | [Poisoned] | 3x3 |
| `CorrodedGrating` | OnMovement | 2d10 | Falls | Single |
| `LeakingCoolant` | Automatic | - | Difficult terrain | 2x2 |
| `RadiationLeak` | Automatic | 1d6 | Cumulative | 4x4 |
| `SporeCloud` | Automatic | 0 | [Infected] 30% | 5ft |
| `AutomatedTurret` | OnMovement | 2d6 | 80% activation | Single |
| `CollapsingCeiling` | Automatic 30% | 3d8 | Difficult terrain | 3x3 |
| `DataStream` | OnProximity (2) | 0 | [Stunned] 50% | 2ft |
| `FungalGrowth` | Manual | 0 | Blocking terrain | 1ft |
| `UnstableGrating` | OnMovement | 2d10 | [Prone] | Single |
| `PsychicEcho` | OnProximity (3) | 0 Psychic | [Shaken] 40% | All |
| `RadiationSource` | Automatic | 1d6 | [Irradiated] | 4x4 |

#### Muspelheim Biome Hazards (v0.29.2+)

| Type | Damage/Turn | Coverage | Special Rules |
|------|-------------|----------|---------------|
| `BurningGround` | 8 Fire | 15% | Persistent terrain |
| `LavaRiver` | Instant death | 10% | Blocks movement |
| `HighPressureSteamVent` | 16 Fire | 5% | [Disoriented], destructible |
| `VolatileGasPocket` | 24 Fire (4d6) | 3% | Chain reaction, AoE radius 3 |
| `ScorchedMetalPlating` | - | 12% | Movement cost 2x |
| `MoltenSlagPool` | 4 Fire | 8% | [Slowed] |
| `CollapsingCatwalk` | 20% collapse | 5% | Random failure |
| `ThermalMirage` | - | - | Ranged attack penalty |

### 2.2 Hazard Triggers

| Trigger | Behavior |
|---------|----------|
| `Automatic` | Activates every turn at set chance |
| `OnProximity` | Activates when character enters range |
| `OnLoudAction` | Triggered by explosions, heavy attacks |
| `OnMovement` | Triggered by stepping on tile |
| `Manual` | Deliberately activated by player/NPC |

### 2.3 Hazard Data Model

```
DynamicHazard:
  HazardId: string              # Unique identifier
  Name: string                  # Display name
  Description: string           # Flavor text
  Type: DynamicHazardType       # Hazard category

  # Activation
  ActivationChance: float       # 1.0 = always, 0.4 = 40%
  Trigger: HazardTrigger        # Trigger condition

  # Damage
  DamageDice: int               # Number of dice
  DamageDieSize: int            # Die size (default 6)
  DamageType: string            # "Physical", "Fire", etc.

  # Area of Effect
  AreaSize: int                 # 1 = single, 3 = 3x3
  AffectsAllCombatants: bool    # Room-wide effect

  # Properties
  IsOneTime: bool               # Removed after trigger
  RequiresProximity: bool       # Range-based activation
  ProximityRange: int           # Tiles for proximity

  # Status Effects
  AppliesStatusEffect: string?  # "Poisoned", "Corroded"
  StatusEffectChance: float     # Application chance

  # Coherent Glitch
  EnhancedByCondition: string?  # "Flooded", etc.
  EnhancementMultiplier: float  # Damage multiplier
```

### 2.4 Hazard Processing

**Automatic Hazards** (per turn):
```
If ActivationChance >= random(0-1):
  If DamageDice > 0:
    damage = Roll(DamageDice, DamageDieSize)
    ApplyDamage(target, damage, DamageType)
  If StressPerTurn > 0:
    ApplyStress(target, StressPerTurn)
  If AppliesStatusEffect:
    If StatusEffectChance >= random(0-1):
      ApplyStatus(target, StatusEffect)
```

**Check-Based Hazards**:
```
If AttributeCheck(target, CheckAttribute) >= CheckDC:
  # Success - no damage
Else:
  damage = Roll(CheckFailureDice, 6)
  ApplyDamage(target, damage)
```

**Triggered Hazards**:
```
On TriggerEvent(hazard.Trigger):
  If IsOneTime AND HasActivated:
    return
  ProcessHazardDamage(hazard, target)
  If IsOneTime:
    DisableHazard(hazard)
```

---

## 3. Cover System

### 3.1 Cover Quality Tiers

| Quality | Defense Bonus | WILL Bonus | Notes |
|---------|---------------|------------|-------|
| None | +0 | +0 | No cover |
| Light | +2 | - | Minimal protection |
| Heavy | +4 | +4 (metaphysical) | Standard cover |
| Total | +6 | +4 | Blocks line of sight |

### 3.2 Cover Types

| Type | Examples | Properties |
|------|----------|------------|
| Physical | Pillars, Crates, Debris, Consoles | Defense vs Ranged |
| Metaphysical | Runic Anchors | WILL vs Psychic |
| Both | Sanctified Barricades | Combined bonuses |

### 3.3 Cover Mechanics

**Bonus Application**:
- Only applies to ranged/psychic attacks
- Attacker and defender must be in opposing zones
- Melee attacks ignore all cover

**Cover Durability**:
```
coverDamage = max(1, damageToTarget / 4)  # 25% of target damage
```
- Default HP: 20
- States: Active → Damaged (50%) → Destroyed

**Cover Destruction**:
- Physical cover: Completely destroyed
- Both types: Converts to Metaphysical only ("Runic Anchor" remains)
- Creates terrain aftermath (optional)

### 3.4 Cover Data Model

```
CoverObject:
  ObjectId: int
  GridPosition: string           # "Front_Left_Column_2"
  CoverQuality: CoverQuality     # None/Light/Heavy/Total
  CoverType: CoverType           # Physical/Metaphysical/Both
  CurrentDurability: int         # Current HP
  MaxDurability: int             # Max HP (default 20)
  SoakValue: int                 # Damage reduction
```

---

## 4. Trap System

### 4.1 Trap Placement

**Validation Rules**:
- Target tile must exist
- Target tile must be unoccupied
- Traps are invisible to enemies by default
- Default duration: 3 turns

### 4.2 Trap Effects

| Effect Type | Default | Parameters |
|-------------|---------|------------|
| Damage | 2d6 | DamageDice |
| Status | [Rooted] 2 turns | StatusEffect, Duration |
| Debuff | TODO | Attribute penalties |
| AreaEffect | 2d6 per target | Radius |

### 4.3 Trap Triggers

| Trigger | Behavior |
|---------|----------|
| OnEnter | Activates when any combatant enters tile |
| OnExit | Activates when combatant leaves tile |
| Manual | Owner activates deliberately |

### 4.4 Trap Processing

```
On CharacterEntersTile(tile):
  traps = GetTrapsOnTile(tile)
  For each trap in traps:
    If trap.TriggerType == OnEnter:
      ApplyTrapEffect(trap, character)
      If character.HasAbility("RunicSynergy"):
        RestoreStamina(character, 10)
      RemoveTrap(trap)

Each Turn:
  DecrementTrapDurations()
  # Remove traps with TurnsRemaining <= 0
```

### 4.5 Trap Data Model

```
BattlefieldTrap:
  TrapId: string
  TrapName: string
  Position: GridPosition
  OwnerId: string               # Player who placed trap
  TurnsRemaining: int           # Default 3
  IsVisible: bool               # Invisible to enemies
  EffectType: TrapEffectType    # Damage/Status/Debuff/AreaEffect
  EffectData: Dictionary        # Effect parameters
  TriggerType: TrapTriggerType  # OnEnter/OnExit/Manual
```

---

## 5. Weather System

### 5.1 Weather Types

| Weather | Accuracy | Movement | Stress | Damage | Status |
|---------|----------|----------|--------|--------|--------|
| RealityStorm | -2 | - | 3/turn | 2d6 Psychic | - |
| StaticDischarge | - | - | - | 3d6 Lightning | Amplifies 1.5x |
| CorrosionCloud | - | - | - | 2d6 Poison | [Corroded] |
| PsychicResonanceStorm | - | - | 5/turn | 3d6 Psychic | [Disoriented] 20% |
| ToxicFog | -1 | - | - | 1d6-2d6 Poison | [Poisoned] 25% |
| RadiationPulse | - | - | - | 2d6-3d6 Radiation | Degradation 1.5x |
| SystemGlitch | -1 | +1 cost | - | - | - |
| VoidIncursion | - | - | 4/turn | 2d6-4d6 Corruption | - |
| DataStorm | -1 | - | 3/turn | - | - |
| TemporalDistortion | -2 | +2 cost | - | - | - |

### 5.2 Weather Intensity

| Intensity | Multiplier | Description |
|-----------|------------|-------------|
| Low | 0.5x | Mild effects |
| Moderate | 1.0x | Standard (default) |
| High | 1.5x | Severe effects |
| Extreme | 2.0x | Catastrophic |

### 5.3 Hazard Amplification

Weather can amplify specific hazard types:
- StaticDischarge: LivePowerConduit damage × 1.5
- Flooded condition: Electrical hazards × 2.0
- ExtremeHeat: Fire hazards × 1.5

### 5.4 Weather Data Model

```
WeatherEffect:
  WeatherId: int
  WeatherType: WeatherType
  Intensity: WeatherIntensity   # Low/Moderate/High/Extreme
  DurationTurns: int?           # null = permanent
  TurnsRemaining: int

  # Combat Modifiers
  AccuracyModifier: int
  DamageModifier: int
  MovementCostModifier: int
  StressPerTurn: int

  # Damage over Time
  DamageFormula: string?        # "2d6", "3d8"
  DamageType: string?

  # Status Effects
  StatusEffectApplied: string?
  StatusEffectChance: float

  # Hazard Interaction
  AmplifiesHazards: bool
  HazardAmplificationMultiplier: float
  AffectedHazardTypes: List<DynamicHazardType>
```

---

## 6. Ambient Conditions

### 6.1 Condition Types

| Condition | Effects | Resolve DC | Attribute |
|-----------|---------|------------|-----------|
| PsychicResonance | +2 stress/turn, -1 WILL | 12 | WILL |
| ToxicAir | [Poisoned] 20% | 14 | STURDINESS |
| RunicInstability | Wild magic 20% | - | - |
| Flooded | +1 movement cost, 2x electrical | - | - |
| CorrodedAtmosphere | 1 degradation/turn, [Corroded] 15% | - | - |
| DimLighting | -1 accuracy | - | - |
| ExtremeHeat | 1.5x fire hazards | - | - |
| HighRadiation | +2 corruption/turn | 16 | STURDINESS |

### 6.2 Condition Processing

Each turn, ambient conditions:
1. Apply damage/stress if applicable
2. Roll for status effect application
3. Modify combat parameters (accuracy, movement)
4. Can be suppressed by specific abilities ("Purify Air")

---

## 7. Environmental Stress

### 7.1 Stress Sources

| Source | Base Stress | Condition |
|--------|-------------|-----------|
| Psychic Resonance | 2/turn | Room ambient condition |
| Nyctophobia | 3 × multiplier | Dim lighting + trauma |
| Enemy presence | 1 | With Hypervigilance trauma |
| Paranoia | +2 | Enemies present + trauma |
| Agoraphobia | Variable | Large room + trauma |
| Claustrophobia | Variable | Small room + trauma |
| Isolophobia | Variable | Alone + trauma |

### 7.2 Room Size Detection

**Large Room Keywords**: vast, sprawling, enormous, cavernous, hall, atrium, plaza

**Small Room Keywords**: cramped, narrow, tight, confined, closet, alcove, crawlspace

---

## 8. Destructible Terrain

### 8.1 Terrain HP Values

| Terrain Type | HP | Creates Rubble |
|--------------|----:|:--------------:|
| CollapsedPillar | 30 | Yes |
| RustedBulkhead | 40 | Yes |
| RubblePile | 15 | No |
| CorrodedGrating | 10 | No (falls) |
| SteelBarricade | 35 | Yes |
| ExposedConduit | 20 | No |
| BrokenGantry | 25 | Yes |

### 8.2 Hazard HP Values

| Hazard Type | HP | Destructible |
|-------------|----:|:------------:|
| SteamVent | 20 | Yes |
| LivePowerConduit | 15 | Yes |
| CorrodedGrating | 10 | Yes |
| LeakingCoolant | 12 | Yes |
| PressurizedPipe | 18 | Yes |
| UnstableCeiling | - | No |
| ToxicSporeCloud | - | No |
| RadiationLeak | - | No |

### 8.3 Destruction Damage Formula

```
damage = (player.MIGHT × 2) + weaponBonus + random(-2, +3)
```

Applied against terrain/hazard durability each attack.

### 8.4 Destruction Aftermath

When destructible objects are destroyed:
1. Record world state change
2. Spawn rubble if applicable
3. Create terrain effect (Difficult, Hazardous)
4. Trigger secondary effects (explosions, chain reactions)

---

## 9. Environmental Objects

### 9.1 Object Types

| Type | Description | Example |
|------|-------------|---------|
| Cover | Provides defensive bonuses | Pillars, Crates |
| Hazard | Environmental danger | Fire, Toxic pools |
| Interactive | Can be activated | Consoles, Switches |
| Obstacle | Blocks movement | Barriers, Caltrops |
| Terrain | Static features | Walls, Floors |

### 9.2 Object States

| State | Description |
|-------|-------------|
| Active | Functional and usable |
| Damaged | Below 50% durability |
| Destroyed | Non-functional |
| Triggered | One-time effects activated |
| Depleted | Resources exhausted |
| Disabled | Temporarily suppressed |

### 9.3 Object Factories

**CreateCover**(roomId, position, name, quality, durability, soakValue):
- Creates physical/metaphysical cover object
- Default durability: 20 HP

**CreateExplosiveObject**(damageFormula, radius, canTriggerAdjacents):
- Creates explosive barrel, spore pod, etc.
- Enables chain reactions if configured

**CreateUnstableCeiling**():
- Damage: 25 Physical
- Radius: 3 tiles
- Effect: [Stunned]
- One-time use

**CreateSteamVent**():
- Damage: 15 Fire
- Radius: 1 tile
- Cooldown: 2 turns
- Soak: 5

### 9.4 Object Data Model

```
EnvironmentalObject:
  ObjectId: int
  RoomId: int
  GridPosition: string?
  ObjectType: EnvironmentalObjectType

  # Display
  Name: string
  Description: string
  Icon: string

  # Structure
  IsDestructible: bool
  CurrentDurability: int?
  MaxDurability: int?
  SoakValue: int                # Damage reduction

  # Hazard Properties
  IsHazard: bool
  HazardTrigger: HazardTrigger
  DamageFormula: string?        # "6d10 Fire"
  DamageType: string?
  StatusEffect: string?
  IgnoresSoak: bool

  # Movement
  BlocksMovement: bool
  BlocksLineOfSight: bool

  # Cover
  ProvidesCover: bool
  CoverQuality: CoverQuality
  CoverArc: string?             # JSON directions

  # Interaction
  IsInteractive: bool
  InteractionType: InteractionType
  InteractionCost: int          # Stamina
  InteractionSkillCheck: string?

  # State
  State: EnvironmentalObjectState
  TriggersRemaining: int
  CooldownRemaining: int
  CooldownDuration: int

  # Destruction
  CreatesTerrainOnDestroy: string?
  TerrainDuration: int?

  # Chain Reaction
  ExplosionRadius: int
  CanTriggerAdjacents: bool
```

---

## 10. Environmental Manipulation

### 10.1 Push/Pull Mechanics

**PushIntoHazard**(target, direction, distance):
```
hazards = GetHazardsInPath(target.Position, direction, distance)
For each hazard in hazards:
  damage = ParseAndRollDamage(hazard.DamageFormula)
  ApplyDamage(target, damage)
Return totalDamage
```

### 10.2 Controlled Collapse

**TriggerControlledCollapse**(objectId):
```
affectedCharacters = GetCharactersInArea(object.Position, radius)
baseDamage = IsTerrain(object) ? 20 : 10
For each character:
  ApplyDamage(character, baseDamage)
DestroyObject(object)
CreateCollapseTerrain(object.Position)
```

### 10.3 Chain Reactions

```
On ObjectDestroyed(object):
  If object.CanTriggerAdjacents:
    nearbyObjects = GetObjectsInRadius(object.Position, object.ExplosionRadius)
    For each nearby in nearbyObjects:
      If nearby.IsDestructible:
        ApplyDamage(nearby, explosionDamage)
        # May recursively trigger more destructions
```

### 10.4 Environmental Kill Tracking

The system tracks environmental kills for:
- Statistics and achievements
- Specialization passive triggers
- Combat log entries

---

## 11. Hazard Spawning

### 11.1 Room-Based Hazard Counts

| Room Type | Count | Chance Distribution |
|-----------|-------|---------------------|
| Start | 0-1 | 30% for 1 |
| Secret | 0-1 | 40% for 1 |
| Boss | 1-2 | 70% for 2 |
| Normal | 0-2 | 40% none, 40% one, 20% two |

### 11.2 Coherent Glitch Modifiers

Environmental conditions affect hazard spawning weights:

| Condition | Hazard Type | Weight Modifier |
|-----------|-------------|-----------------|
| Flooded | Electrical | +2.0x |
| Rust-Horror enemies | Spore/Toxic | +1.5x |
| ExtremeHeat | Fire | +1.5x |

### 11.3 Hazard Creation Factories

The HazardSpawner provides factories for common hazards:
- `CreateSteamVent()` - Geothermal steam
- `CreateLivePowerConduit()` - Exposed wiring
- `CreateUnstableCeiling()` - Collapsing infrastructure
- `CreateToxicSporeCloud()` - Fungal contamination
- `CreateCorrodedGrating()` - Breakable floor
- `CreateLeakingCoolant()` - Chemical spill

---

## 12. Environmental Events

### 12.1 Event Types

| Event Type | Description |
|------------|-------------|
| ObjectDestroyed | Environmental object destroyed |
| HazardTriggered | Hazard activated |
| EnvironmentalKill | Character killed by environment |
| CoverPlaced | Cover established |
| CoverDestroyed | Cover destroyed |
| CeilingCollapse | Controlled collapse triggered |
| PushIntoHazard | Character pushed into hazard |
| InteractionTriggered | Interactive object activated |
| AmbientDamage | Ambient condition damage |
| WeatherEffectApplied | Weather modified combat |
| ChainReaction | Destruction cascade |

### 12.2 Event Data Model

```
EnvironmentalEvent:
  EventId: int
  CombatInstanceId: int
  TurnNumber: int
  EventType: EnvironmentalEventType
  ObjectId: int?
  ActorId: int?
  Targets: List<int>            # Character IDs
  DamageDealt: int
  Kills: int
  StatusEffectApplied: string?
  Description: string?
  Timestamp: DateTime
```

---

## 13. Integration Points

### 13.1 Combat System Integration

The EnvironmentalCombatService orchestrates environmental processing:

**Turn Start**:
1. Apply ambient condition effects
2. Apply weather effects
3. Check automatic hazards at character positions

**Turn End**:
1. Decrement trap durations
2. Advance weather effects
3. Process cooldowns on reusable hazards

### 13.2 Trauma Economy Integration

Environmental stress feeds into the Trauma Economy:
- Ambient conditions apply passive stress
- Room conditions trigger trauma-specific stress
- Weather applies stress per turn
- Characters break under accumulated stress

### 13.3 Grid System Integration

Position-based mechanics:
- GridPosition format: "Zone/Row/ColumnN"
- Distance: Manhattan distance with zone consideration
- AoE: Radius-based character detection
- Movement cost modifiers applied by terrain/weather

---

## 14. GUI Planning

### 14.1 Planned UI Components

#### 14.1.1 Environmental Status Panel

```
+-----------------------------------------------+
| ENVIRONMENTAL CONDITIONS                       |
+-----------------------------------------------+
| Ambient: Flooded (+1 move cost)               |
| Weather: Toxic Fog (-1 accuracy)              |
|                                               |
| Active Hazards:                               |
| [!] Steam Vent (2,3) - 15 Fire/turn          |
| [!] Radiation Leak (5,1) - 1d6/turn          |
|                                               |
| Your Cover: Heavy (+4 Defense) [15/20 HP]    |
+-----------------------------------------------+
```

**EnvironmentalStatusViewModel**:
```
AmbientConditions: ObservableCollection<AmbientConditionDisplay>
ActiveWeather: WeatherDisplay?
NearbyHazards: ObservableCollection<HazardDisplay>
CurrentCover: CoverDisplay?
EnvironmentalWarnings: ObservableCollection<string>
```

#### 14.1.2 Hazard Detail Tooltip

```
+--------------------------------+
| [!] STEAM VENT                 |
+--------------------------------+
| Superheated geothermal steam   |
| erupts from damaged pipes.     |
|                                |
| Damage: 15 Fire per turn       |
| Trigger: Automatic             |
| Area: 1 tile                   |
| Cooldown: 2 turns              |
|                                |
| [Destructible: 20 HP]          |
+--------------------------------+
```

**HazardDetailViewModel**:
```
HazardName: string
Description: string
DamageFormula: string
DamageType: string
TriggerType: string
AreaDescription: string
IsDestructible: bool
CurrentHP: int?
MaxHP: int?
CooldownRemaining: int?
StatusEffects: List<string>
```

#### 14.1.3 Cover Management Panel

```
+-----------------------------------------------+
| COVER POSITIONS                                |
+-----------------------------------------------+
| [*] Collapsed Pillar (3,2)                    |
|     Quality: Heavy (+4 Defense)               |
|     HP: 20/30                                 |
|     Type: Physical                            |
|                                               |
| [*] Runic Anchor (4,1)                        |
|     Quality: Heavy (+4 WILL)                  |
|     HP: Indestructible                        |
|     Type: Metaphysical                        |
|                                               |
| [ Take Cover ] [ Destroy Cover ]              |
+-----------------------------------------------+
```

**CoverManagementViewModel**:
```
AvailableCover: ObservableCollection<CoverDisplay>
SelectedCover: CoverDisplay?
PlayerCurrentCover: CoverDisplay?
TakeCoverCommand: ReactiveCommand
DestroyCoverCommand: ReactiveCommand
```

#### 14.1.4 Weather Forecast Display

```
+-----------------------------------------------+
| SECTOR WEATHER                                 |
+-----------------------------------------------+
| Current: Reality Storm (High)                  |
|   - Accuracy: -2                              |
|   - Stress: 3/turn                            |
|   - Damage: 2d6 Psychic (high intensity)      |
|                                               |
| Duration: 5 turns remaining                    |
|                                               |
| Forecast: Clearing                             |
+-----------------------------------------------+
```

**WeatherForecastViewModel**:
```
CurrentWeather: WeatherDisplay?
WeatherIntensity: string
AccuracyModifier: int
StressPerTurn: int
DamageFormula: string?
TurnsRemaining: int?
ForecastDescription: string
WeatherHistory: ObservableCollection<WeatherDisplay>
```

### 14.2 Grid Visualization Enhancements

**Hazard Indicators**:
- Fire hazards: Orange/red tile overlay
- Toxic hazards: Green tile overlay
- Electrical hazards: Blue/yellow tile overlay
- Radiation hazards: Purple tile overlay
- Psychic hazards: Pink/violet tile overlay

**Cover Indicators**:
- Physical cover: Shield icon on tile
- Metaphysical cover: Rune icon on tile
- Damaged cover: Cracked icon overlay
- Cover arc: Directional indicators

**Trap Indicators** (player traps only):
- Hidden trap: Faint dotted outline
- Player's trap: Visible icon

---

## 15. Data File Locations

### 15.1 Service Files

| File | Path |
|------|------|
| HazardService | `RuneAndRust.Engine/HazardService.cs` |
| HazardDatabase | `RuneAndRust.Engine/HazardDatabase.cs` |
| HazardSpawner | `RuneAndRust.Engine/HazardSpawner.cs` |
| EnvironmentalCombatService | `RuneAndRust.Engine/EnvironmentalCombatService.cs` |
| EnvironmentalObjectService | `RuneAndRust.Engine/EnvironmentalObjectService.cs` |
| EnvironmentalStressService | `RuneAndRust.Engine/EnvironmentalStressService.cs` |
| EnvironmentalManipulationService | `RuneAndRust.Engine/EnvironmentalManipulationService.cs` |
| AmbientConditionService | `RuneAndRust.Engine/AmbientConditionService.cs` |
| WeatherEffectService | `RuneAndRust.Engine/WeatherEffectService.cs` |
| TrapService | `RuneAndRust.Engine/TrapService.cs` |
| CoverService | `RuneAndRust.Engine/CoverService.cs` |
| DestructionService | `RuneAndRust.Engine/DestructionService.cs` |

### 15.2 Data Model Files

| File | Path |
|------|------|
| HazardType | `RuneAndRust.Core/HazardType.cs` |
| DynamicHazard | `RuneAndRust.Core/DynamicHazard.cs` |
| WeatherEffect | `RuneAndRust.Core/WeatherEffect.cs` |
| EnvironmentalObject | `RuneAndRust.Core/EnvironmentalObject.cs` |
| EnvironmentalEvent | `RuneAndRust.Core/EnvironmentalEvent.cs` |
| BattlefieldTrap | `RuneAndRust.Core/BattlefieldTrap.cs` |
| DestructibleElement | `RuneAndRust.Core/DestructibleElement.cs` |

### 15.3 SQL Data Files

| Version | Path |
|---------|------|
| v0.29.2 | `Data/v0.29.2_environmental_hazards.sql` |
| v0.30.2 | `Data/v0.30.2_environmental_hazards.sql` |
| v0.31.2 | `Data/v0.31.2_environmental_hazards.sql` |
| v0.32.2 | `Data/v0.32.2_environmental_hazards.sql` |

---

## 16. Version History

| Version | Date | Changes |
|---------|------|---------|
| v0.6 | - | Basic HazardService with legacy types |
| v0.11 | - | Dynamic hazard system, HazardSpawner |
| v0.13 | - | DestructionService, world state tracking |
| v0.15 | - | Environmental stress, Trauma integration |
| v0.16 | - | HazardDatabase expansion, new hazard types |
| v0.20 | - | TrapService for battlefield traps |
| v0.20.2 | - | CoverService with durability |
| v0.22 | - | EnvironmentalCombatService orchestration |
| v0.22.1 | - | EnvironmentalObjectService enhancements |
| v0.22.2 | - | Weather and Ambient condition systems |
| v0.22.3 | - | Environmental manipulation mechanics |
| v0.29.2+ | - | Biome-specific hazards (Muspelheim) |

---

**End of Specification**
