# v0.38.6: Combat & Action Flavor Text - Implementation Guide

## Overview

v0.38.6 adds comprehensive combat flavor text to Rune & Rust, transforming generic combat logs into dynamic, contextual storytelling moments.

### Before v0.38.6

```
You attack the Servitor. Hit for 12 damage.

```

### After v0.38.6

```
Your Rusted Longsword bites into the Servitor's chassis with a satisfying crunch.
The Servitor's chassis dents under your blow, circuits sparking.
Steam hisses from fractured pipes as you fight.

```

## Components

### 1. Database Schema

**File**: `Data/v0.38.6_combat_flavor_text_schema.sql`

Three new tables:

- `Combat_Action_Descriptors`: 200+ player and enemy action descriptions
- `Enemy_Voice_Profiles`: 5 enemy archetype personalities
- `Environmental_Combat_Modifiers`: 50+ biome-specific atmospheric elements

### 2. Content Population

**Player Actions** (`v0.38.6_player_action_descriptors.sql`):

- 100+ player attack descriptors
- All weapon types: Swords, Axes, Hammers, Bows
- All outcomes: Miss, Deflected, Glancing, Solid, Devastating, Critical
- Defense actions: Dodge, Parry, Block

**Enemy Voices** (`v0.38.6_enemy_voice_profiles.sql`):

- **Servitor**: Mechanical, emotionless, relentless
- **Forlorn**: Mournful, hollow, trapped between life and death
- **Corrupted Dvergr**: Fractured speech, mad technical jargon
- **Blight-Touched Beast**: Animalistic, suffering, corrupted
- **Aether-Wraith**: Reality-bending, incomprehensible, horrifying

**Environmental Modifiers** (`v0.38.6_environmental_combat_modifiers.sql`):

- The Roots: Rust, corrosion, industrial decay
- Muspelheim: Heat, magma, volcanic fury
- Niflheim: Ice, cold, freezing winds
- Alfheim: Reality warping, paradox energy
- Jötunheim: Titanic machinery, industrial grandeur

### 3. C# Models

**Location**: `RuneAndRust.Core/CombatFlavor/`

- `CombatActionDescriptor.cs`: Action flavor text model
- `EnemyVoiceProfile.cs`: Enemy personality model
- `EnvironmentalCombatModifier.cs`: Biome atmosphere model

### 4. Repository Extensions

**File**: `RuneAndRust.Persistence/DescriptorRepository_CombatFlavorExtensions.cs`

Database access methods:

```csharp
// Get player action descriptors
GetCombatActionDescriptors(category, weaponType, outcomeType)

// Get enemy voice profile
GetEnemyVoiceProfile(archetype)

// Get environmental modifiers
GetEnvironmentalCombatModifiers(biomeName, modifierType)

```

### 5. Combat Flavor Text Service

**File**: `RuneAndRust.Engine/CombatFlavorTextService.cs`

Core service for generating dynamic flavor text:

```csharp
var service = new CombatFlavorTextService(repository);

// Player attack
var attackText = service.GeneratePlayerAttackText(
    WeaponType.SwordOneHanded,
    "Rusted Longsword",
    "Corrupted Servitor",
    CombatOutcome.SolidHit);

// Enemy attack
var enemyText = service.GenerateEnemyAttackText(
    EnemyArchetype.Servitor,
    "Servitor Scout",
    isSpecialAttack: false);

// Enemy reaction
var reactionText = service.GenerateEnemyDamageReaction(
    EnemyArchetype.Servitor,
    "Servitor Scout",
    damageAmount: 15,
    isDying: false);

// Environmental atmosphere
var envText = service.GenerateEnvironmentalReaction("The_Roots");

```

## Template Variables

Descriptors use template variables for dynamic content:

- `{Weapon}`: Player's weapon name
- `{Enemy}`: Enemy name
- `{Target_Location}`: Hit location (torso, arm, leg, etc.)
- `{Vital_Location}`: Critical hit location (heart, core, neck, etc.)
- `{Armor_Location}`: Armor type (plating, carapace, hide, etc.)
- `{Damage_Type_Descriptor}`: Blood, gore, sparks, etc.
- `{Enemy_Reaction}`: Cry, howl, shriek, etc.
- `{Environment_Feature}`: Wall, pillar, etc.

## Integration with Combat Engine

The CombatFlavorTextService is designed to complement the existing v0.22 CombatEngine:

```csharp
public class EnhancedCombatEngine
{
    private readonly CombatEngine _combatEngine;
    private readonly CombatFlavorTextService _flavorService;

    public void ExecutePlayerAttack(CombatState state, Enemy target)
    {
        // Execute combat logic (v0.22)
        _combatEngine.PlayerAttack(state, target);

        // Add flavor text enhancement (v0.38.6)
        var outcome = DetermineOutcome(attackSuccesses, defendSuccesses, isCritical);
        var flavorText = _flavorService.GeneratePlayerAttackText(
            weaponType,
            weaponName,
            target.Name,
            outcome);

        state.AddLogEntry(flavorText);

        // Add enemy reaction
        if (target.IsAlive)
        {
            var reaction = _flavorService.GenerateEnemyDamageReaction(
                target.Archetype,
                target.Name,
                damageDealt,
                isDying: !target.IsAlive);
            state.AddLogEntry(reaction);
        }

        // Environmental atmosphere (30% chance)
        var envText = _flavorService.GenerateEnvironmentalReaction(
            state.CurrentRoom.BiomeName);
        if (!string.IsNullOrEmpty(envText))
        {
            state.AddLogEntry(envText);
        }
    }
}

```

## Database Initialization

Execute SQL scripts in order:

```sql
-- 1. Create schema
.read Data/v0.38.6_combat_flavor_text_schema.sql

-- 2. Populate player actions
.read Data/v0.38.6_player_action_descriptors.sql

-- 3. Populate enemy voices
.read Data/v0.38.6_enemy_voice_profiles.sql

-- 4. Populate environmental modifiers
.read Data/v0.38.6_environmental_combat_modifiers.sql

-- 5. Link voice profiles to descriptors
.read Data/v0.38.6_enemy_voice_profile_population.sql

```

## Testing

**Unit Tests**: `RuneAndRust.Tests/CombatFlavorTextServiceTests.cs`

- Template variable substitution
- Weapon type filtering
- Outcome type filtering
- Enemy archetype voices
- Environmental modifiers

**Integration Demo**: `RuneAndRust.Tests/CombatFlavorIntegrationDemo.cs`

- Before/after comparisons
- Full combat sequences
- Enemy voice demonstrations
- Environmental atmosphere showcase

Run tests:

```bash
dotnet test --filter "FullyQualifiedName~CombatFlavor"

```

## Success Criteria (All Met ✓)

- [x]  100+ player action descriptors
- [x]  All weapon types covered
- [x]  All outcome types covered
- [x]  Environmental context modifiers (5 biomes)
- [x]  5+ enemy archetype voices defined
- [x]  Attack descriptors per archetype (10+ each)
- [x]  Damage reactions per archetype (5+ each)
- [x]  Death descriptors per archetype (5+ each)
- [x]  Combat_Action_Descriptors table
- [x]  Enemy_Voice_Profiles table
- [x]  Environmental_Combat_Modifiers table
- [x]  200+ total descriptors inserted
- [x]  CombatFlavorTextService complete
- [x]  Player attack text generation
- [x]  Enemy attack text generation
- [x]  Enemy reaction text generation
- [x]  Environmental reaction integration
- [x]  Unit tests (80%+ coverage)
- [x]  Template variable substitution tests
- [x]  Integration demonstration

## Performance Considerations

- **Randomization**: Service uses seeded Random for variety
- **Caching**: Repository queries can be cached if needed
- **Trigger Chance**: Environmental modifiers use configurable probability (default 30%)
- **Template Processing**: Regex-based, compiled pattern for efficiency

## Future Enhancements

Potential additions beyond v0.38.6:

1. **Ability/Galdr Flavor Text** (v0.38.7)
    - Special attack descriptions
    - Magical effect flavor
2. **Status Effect Descriptions** (v0.38.8)
    - Burning, frozen, poisoned text
    - Buff/debuff application flavor
3. **Weapon-Specific Traits**
    - Unique text for legendary weapons
    - Enchantment descriptions
4. **Combo Descriptors**
    - Multi-attack sequences
    - Finishing move variations

## Examples

### Sword Combat Sequence

```
You bring your Rusted Longsword around in an arc that slashes across the Servitor.
Warning klaxons blare from the damaged Servitor.
The clash of combat echoes through corroded halls.

The Servitor's articulated limb swings at you with mechanical precision.
You parry the Servitor's attack with your Rusted Longsword, metal ringing on metal.

You find a critical weakness—your Rusted Longsword plunges into the Servitor's core with devastating precision!
The Servitor collapses, its corrupted runes dimming to darkness.

```

### Bow vs. Beast

```
Your arrow punches through the Blight-Touched Wolf's flank with a wet thunk.
The beast howls in pain and rage.
Crystalline growths glitter in the dim light.

The beast lunges with unnatural speed, jaws snapping.
You twist aside, the beast's attack passing harmlessly by.

Your arrow strikes a vital point—the beast convulses as the shaft pierces its heart!
The Blight-touched animal finally finds rest.

```

### Alfheim Horror

```
The Aether-Wraith's form flickers as it phases through reality to strike you.
You try to dodge but aren't fast enough—the Wraith's attack connects!

Your Frost-Kissed Blade passes through where the Wraith was—will be—is.
Reality flickers with each exchange of blows.

Your blade cleaves deep, nearly severing the Wraith's essence!
The Wraith unravels, reality reasserting itself where it stood.

```

## Architecture Decisions

### Why Separate Service?

- **Modularity**: Can be enabled/disabled independently
- **Testing**: Easy to test in isolation
- **Backwards Compatibility**: Doesn't break existing combat
- **Performance**: Only runs when needed

### Why Template Variables?

- **Flexibility**: Same descriptor, many contexts
- **Maintenance**: Update once, apply everywhere
- **Variety**: Combine templates with random elements

### Why Enemy Voice Profiles?

- **Consistency**: Each enemy type has distinct personality
- **Immersion**: Servitors sound mechanical, Forlorn sound mournful
- **Lore Integration**: Voices reflect enemy backstory

## Conclusion

v0.38.6 transforms combat from mechanical roll reporting into immersive storytelling. Every swing, dodge, and fatal blow now has weight and atmosphere, reinforcing the dark Norse dungeon crawler aesthetic.

**Total Content**: 250+ unique descriptors across player actions, enemy voices, and environmental atmosphere.

**Integration Ready**: Drop-in enhancement for existing combat system.

**Tested & Verified**: Comprehensive unit and integration tests confirm quality and variety.