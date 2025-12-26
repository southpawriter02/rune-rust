# AI Behavior Pattern Template

Parent item: Template (Template%202ba55eb312da80a8b0f3fbfdcd27220c.md)

**Purpose**: Code templates for implementing enemy AI behavior patterns in `EnemyAI.cs` that match the probability distributions designed in the Enemy Design Worksheet.

**When to Use**: After completing Step 4 (AI Behavior Pattern) of the Enemy Design Worksheet, use these templates to implement the AI logic in `RuneAndRust.Engine/EnemyAI.cs`.

**Reference**: See Step 4 of `/docs/templates/enemy-design-worksheet.md` for probability distribution design.

---

## Template Selection Guide

Choose the template that matches your enemy's archetype from Step 1 of the worksheet:

| Archetype | Template to Use | Probability Pattern |
| --- | --- | --- |
| **Tank** | Defensive Pattern | 40-50% attack, 30-40% defense, 20-30% utility |
| **DPS** | Aggressive Pattern | 70-80% offense, 20-30% defensive/utility |
| **Glass Cannon** | Aggressive Pattern | 80-90% offense, 10-20% defensive/utility |
| **Support** | Tactical Pattern | 30-40% attack, 30-40% buff/debuff, 20-30% heal |
| **Swarm** | Aggressive Pattern | 90% offense, 10% utility |
| **Caster** | Tactical Pattern | 60% magic attack, 20% debuff, 20% utility |
| **Mini-Boss** | Phase-Based Pattern | 2 phases with HP thresholds |
| **Boss** | Phase-Based Pattern | 2-3 phases with HP thresholds |

---

## Implementation Location

**File**: `RuneAndRust.Engine/EnemyAI.cs`

**Step 1**: Add your enemy to the `DetermineAction()` switch statement:

```csharp
public static string DetermineAction(Enemy enemy, PlayerCharacter player)
{
    return enemy.Type switch
    {
        // ... existing cases ...

        EnemyType.YourEnemyName => DetermineYourEnemyNameAction(enemy),

        // ... rest of cases ...
        _ => "BasicAttack"
    };
}

```

**Step 2**: Implement your enemy's AI method using one of the templates below.

---

## Template 1: Aggressive Pattern

**Use For**: Glass Cannon, DPS, Swarm archetypes

**Probability Distribution**:

- 70-90% offensive actions (BasicAttack, special attacks)
- 10-30% defensive/utility actions (DefensiveStance, flee)

**Code Template**:

```csharp
private static string DetermineYourEnemyNameAction(Enemy enemy)
{
    int roll = Random.Next(100); // Roll 0-99

    // Low HP trigger: Flee at < 30% HP (optional for Glass Cannon)
    if (enemy.HP < (enemy.MaxHP * 0.3))
    {
        return "Flee"; // Glass Cannon archetype flees when low
    }

    // Aggressive Pattern: 80% offense, 20% defensive
    if (roll < 80) // 0-79 = 80% chance
    {
        return "BasicAttack"; // Primary offensive action
    }
    else // 80-99 = 20% chance
    {
        return "DefensiveStance"; // Defensive fallback
    }
}

```

**Example: High-Damage Swarm Enemy** (90% offense, 10% utility):

```csharp
private static string DetermineSwarmDroneAction(Enemy enemy)
{
    int roll = Random.Next(100);

    // Swarm AI: 90% aggressive attack
    if (roll < 90) // 0-89 = 90% chance
    {
        return "BasicAttack"; // Relentless swarming behavior
    }
    else // 90-99 = 10% chance
    {
        return "Wait"; // Occasionally reposition
    }
}

```

**Example: DPS Enemy with Special Attack** (40% basic, 30% special, 20% defensive, 10% utility):

```csharp
private static string DetermineDPSEnemyAction(Enemy enemy)
{
    int roll = Random.Next(100);

    // 40% BasicAttack
    if (roll < 40) // 0-39
    {
        return "BasicAttack";
    }
    // 30% SpecialAttack (70% total offense so far)
    else if (roll < 70) // 40-69
    {
        return "PowerStrike"; // Special offensive ability
    }
    // 20% Defensive
    else if (roll < 90) // 70-89
    {
        return "DefensiveStance";
    }
    // 10% Utility
    else // 90-99
    {
        return "Wait";
    }
}

```

---

## Template 2: Defensive Pattern

**Use For**: Tank archetype

**Probability Distribution**:

- 40-50% attack actions
- 30-40% defensive buffs (DefensiveStance, self-heal)
- 20-30% utility (positioning, buffs)

**Code Template**:

```csharp
private static string DetermineYourTankEnemyAction(Enemy enemy)
{
    int roll = Random.Next(100);

    // Low HP trigger: Prioritize self-heal at < 40% HP
    if (enemy.HP < (enemy.MaxHP * 0.4) && !enemy.HasUsedSpecialAbility)
    {
        enemy.HasUsedSpecialAbility = true; // One-time heal
        return "LastStand"; // Self-heal ability
    }

    // Tank Pattern: 45% attack, 35% defense, 20% utility
    if (roll < 45) // 0-44 = 45% chance
    {
        return "BasicAttack"; // Moderate offense
    }
    else if (roll < 80) // 45-79 = 35% chance
    {
        return "DefensiveStance"; // High defense priority
    }
    else // 80-99 = 20% chance
    {
        return "Wait"; // Utility/positioning
    }
}

```

**Example: Tank with Self-Heal** (40% attack, 30% defense, 20% heal, 10% utility):

```csharp
private static string DetermineVaultCustodianAction(Enemy enemy)
{
    int roll = Random.Next(100);

    // Emergency heal at < 35% HP
    if (enemy.HP < (enemy.MaxHP * 0.35) && !enemy.HasUsedSpecialAbility)
    {
        enemy.HasUsedSpecialAbility = true;
        return "GuardianProtocol"; // Self-heal + defense buff
    }

    // 40% BasicAttack
    if (roll < 40)
    {
        return "BasicAttack";
    }
    // 30% DefensiveStance
    else if (roll < 70)
    {
        return "DefensiveStance";
    }
    // 20% Self-Heal (if HP < 60%)
    else if (roll < 90 && enemy.HP < (enemy.MaxHP * 0.6))
    {
        return "Regenerate"; // Heal over time
    }
    // 10% Wait
    else
    {
        return "Wait";
    }
}

```

---

## Template 3: Tactical Pattern

**Use For**: Support, Caster archetypes

**Probability Distribution**:

- 30-50% attack/magic attacks
- 30-40% buff allies/debuff player
- 20-30% heal allies/summons

**Code Template**:

```csharp
private static string DetermineYourCasterEnemyAction(Enemy enemy)
{
    int roll = Random.Next(100);

    // Tactical Pattern: 50% magic attack, 30% debuff, 20% utility
    if (roll < 50) // 0-49 = 50% chance
    {
        return "PsychicBlast"; // Primary magic attack
    }
    else if (roll < 80) // 50-79 = 30% chance
    {
        return "CursePlayer"; // Debuff player (e.g., Silenced, Vulnerable)
    }
    else // 80-99 = 20% chance
    {
        return "Wait"; // Utility/repositioning
    }
}

```

**Example: Support Enemy with Ally Buffs** (30% attack, 40% buff, 20% heal, 10% utility):

```csharp
private static string DetermineCorruptedEngineerAction(Enemy enemy)
{
    int roll = Random.Next(100);

    // Support AI: Prioritize allies
    // 30% BasicAttack
    if (roll < 30)
    {
        return "BasicAttack";
    }
    // 40% Buff Allies (if allies present)
    else if (roll < 70)
    {
        // Note: Actual implementation needs ally detection logic
        return "OverchargeAlly"; // Buff ally damage
    }
    // 20% Heal Allies (if ally HP < 50%)
    else if (roll < 90)
    {
        return "EmergencyRepairAlly"; // Heal weakest ally
    }
    // 10% Utility
    else
    {
        return "Wait";
    }
}

```

**Example: Caster with Multiple Spells** (60% magic, 20% debuff, 20% defensive):

```csharp
private static string DetermineForlornScholarAction(Enemy enemy)
{
    int roll = Random.Next(100);

    // Caster AI: High magic attack rate
    // 60% Magic Attacks (split between two spells)
    if (roll < 30) // 30% PsychicBlast
    {
        return "PsychicBlast"; // Primary magic attack
    }
    else if (roll < 60) // 30% ArcaneBarrage
    {
        return "ArcaneBarrage"; // Secondary magic attack
    }
    // 20% Debuff
    else if (roll < 80)
    {
        return "InflictSilence"; // Prevent player spellcasting
    }
    // 20% Defensive
    else
    {
        return "DefensiveStance";
    }
}

```

---

## Template 4: Phase-Based Pattern

**Use For**: Mini-Boss, Boss archetypes

**Probability Distribution**:

- **Phase 1** (HP > 50%): 50-60% primary attack, balanced behavior
- **Phase 2** (HP ≤ 50%): 30-50% special abilities/AoE, increased aggression
- **Phase 3** (HP ≤ 25%, optional): 50%+ desperation ultimate, summons

**Code Template**:

```csharp
private static string DetermineYourBossEnemyAction(Enemy enemy)
{
    int roll = Random.Next(100);

    // Phase 3: HP ≤ 25% (Desperation Phase)
    if (enemy.HP <= (enemy.MaxHP * 0.25))
    {
        if (enemy.Phase != 3)
        {
            enemy.Phase = 3; // Trigger Phase 3
            return "DesprationUltimate"; // One-time phase transition ability
        }

        // Phase 3 AI: 50% ultimate, 30% AoE, 20% summons
        if (roll < 50)
        {
            return "UltimateAbility"; // High-damage desperation attack
        }
        else if (roll < 80)
        {
            return "AoEAttack"; // Area damage
        }
        else
        {
            return "SummonMinions"; // Reinforcements
        }
    }

    // Phase 2: HP ≤ 50% (Aggressive Phase)
    else if (enemy.HP <= (enemy.MaxHP * 0.5))
    {
        if (enemy.Phase != 2)
        {
            enemy.Phase = 2; // Trigger Phase 2
            return "PhaseTransition"; // One-time phase transition ability
        }

        // Phase 2 AI: 40% special, 40% basic, 20% utility
        if (roll < 40)
        {
            return "SpecialAbility"; // Increased special ability usage
        }
        else if (roll < 80)
        {
            return "BasicAttack";
        }
        else
        {
            return "DefensiveStance";
        }
    }

    // Phase 1: HP > 50% (Standard Phase)
    else
    {
        // Phase 1 AI: 60% basic, 30% special, 10% defensive
        if (roll < 60)
        {
            return "BasicAttack"; // Conservative early game
        }
        else if (roll < 90)
        {
            return "SpecialAbility";
        }
        else
        {
            return "DefensiveStance";
        }
    }
}

```

**Example: Boss with 3 Phases** (Forlorn Archivist):

```csharp
private static string DetermineForlornArchivistAction(Enemy enemy)
{
    int roll = Random.Next(100);

    // Phase 3: HP ≤ 25% (Summon Spam Phase)
    if (enemy.HP <= (enemy.MaxHP * 0.25))
    {
        if (enemy.Phase != 3)
        {
            enemy.Phase = 3;
            return "FinalSummoning"; // Summon 3 minions at once
        }

        // 60% Summon, 30% PsychicBlast, 10% Defensive
        if (roll < 60)
        {
            return "SummonForlornServant";
        }
        else if (roll < 90)
        {
            return "PsychicBlast";
        }
        else
        {
            return "DefensiveStance";
        }
    }

    // Phase 2: HP ≤ 50% (Aggressive Psychic Phase)
    else if (enemy.HP <= (enemy.MaxHP * 0.5))
    {
        if (enemy.Phase != 2)
        {
            enemy.Phase = 2;
            return "PsychicOverload"; // AoE psychic damage + Stress
        }

        // 50% PsychicBlast, 30% Summon, 20% Debuff
        if (roll < 50)
        {
            return "PsychicBlast";
        }
        else if (roll < 80)
        {
            return "SummonForlornServant";
        }
        else
        {
            return "InflictCorruption"; // Debuff player
        }
    }

    // Phase 1: HP > 50% (Balanced Phase)
    else
    {
        // 40% BasicAttack, 30% PsychicBlast, 20% Summon, 10% Defensive
        if (roll < 40)
        {
            return "BasicAttack";
        }
        else if (roll < 70)
        {
            return "PsychicBlast";
        }
        else if (roll < 90)
        {
            return "SummonForlornServant";
        }
        else
        {
            return "DefensiveStance";
        }
    }
}

```

**Example: Mini-Boss with 2 Phases** (War Frame):

```csharp
private static string DetermineWarFrameAction(Enemy enemy)
{
    int roll = Random.Next(100);

    // Phase 2: HP ≤ 40% (Berserk Phase)
    if (enemy.HP <= (enemy.MaxHP * 0.4))
    {
        if (enemy.Phase != 2)
        {
            enemy.Phase = 2;
            return "BerserkMode"; // +50% damage, -2 Defense
        }

        // Phase 2: 70% PowerStrike, 20% BasicAttack, 10% Defensive
        if (roll < 70) // Aggressive special attack spam
        {
            return "PowerStrike";
        }
        else if (roll < 90)
        {
            return "BasicAttack";
        }
        else
        {
            return "DefensiveStance";
        }
    }

    // Phase 1: HP > 40% (Standard Phase)
    else
    {
        // Phase 1: 50% BasicAttack, 30% PowerStrike, 20% Defensive
        if (roll < 50)
        {
            return "BasicAttack";
        }
        else if (roll < 80)
        {
            return "PowerStrike";
        }
        else
        {
            return "DefensiveStance";
        }
    }
}

```

---

## Template 5: Conditional AI Pattern

**Use For**: Enemies with complex conditional logic (ally-dependent, environment-dependent, status-dependent)

**Example: Support Enemy with Ally Detection**:

```csharp
private static string DetermineSupportEnemyAction(Enemy enemy, List<Enemy> allies)
{
    int roll = Random.Next(100);

    // Conditional: Check if any ally is low HP
    bool anyAllyLowHP = allies.Any(ally => ally.HP < (ally.MaxHP * 0.5));

    // Conditional: Check if any ally exists
    bool hasAllies = allies.Count > 0;

    // Priority 1: Heal low HP allies
    if (anyAllyLowHP && roll < 60) // 60% chance to heal if ally low
    {
        return "HealAlly"; // Target lowest HP ally
    }

    // Priority 2: Buff allies if present
    if (hasAllies && roll < 40) // 40% chance to buff (if no heal needed)
    {
        return "BuffAlly";
    }

    // Priority 3: Default to basic attack
    if (roll < 70)
    {
        return "BasicAttack";
    }
    else
    {
        return "DefensiveStance";
    }
}

```

**Example: Enemy with Status-Dependent AI** (Poisoner):

```csharp
private static string DetermineSludgeCrawlerAction(Enemy enemy, PlayerCharacter player)
{
    int roll = Random.Next(100);

    // Conditional: Check if player is already poisoned
    bool playerPoisoned = player.PoisonTurnsRemaining > 0;

    // If player not poisoned, prioritize poison attack
    if (!playerPoisoned && roll < 70) // 70% chance to poison
    {
        return "PoisonSpit"; // Apply poison DoT
    }

    // If player already poisoned, default to basic attack
    if (roll < 80)
    {
        return "BasicAttack";
    }
    else
    {
        return "DefensiveStance";
    }
}

```

---

## Common AI Patterns & Best Practices

### 1. Probability Roll Structure

```csharp
int roll = Random.Next(100); // Always 0-99

// Use cumulative thresholds for clarity
if (roll < 40)      // 0-39 = 40%
    { /* Action 1 */ }
else if (roll < 70) // 40-69 = 30%
    { /* Action 2 */ }
else if (roll < 90) // 70-89 = 20%
    { /* Action 3 */ }
else                // 90-99 = 10%
    { /* Action 4 */ }

```

### 2. Low HP Triggers

```csharp
// Emergency actions at low HP (typically < 30-40%)
if (enemy.HP < (enemy.MaxHP * 0.3))
{
    // One-time abilities: Use HasUsedSpecialAbility flag
    if (!enemy.HasUsedSpecialAbility)
    {
        enemy.HasUsedSpecialAbility = true;
        return "LastStand"; // Self-heal, buff, or desperation attack
    }

    // Repeatable low HP behavior
    return "Flee"; // Glass Cannon archetype
}

```

### 3. Phase Transitions

```csharp
// Always check and update Phase property
if (enemy.HP <= (enemy.MaxHP * 0.5) && enemy.Phase != 2)
{
    enemy.Phase = 2; // Update phase before returning action
    return "PhaseTransitionAbility"; // One-time transition effect
}

```

### 4. Ally-Dependent Logic

```csharp
// Support/Buffer enemies need ally detection
// Note: Requires EnemyAI.cs to pass List<Enemy> allies parameter

bool hasAllies = allies.Any(ally => ally.IsAlive);
bool anyAllyLowHP = allies.Any(ally => ally.HP < (ally.MaxHP * 0.5));

if (hasAllies)
{
    // Prioritize supporting allies over attacking
}
else
{
    // Default to offensive behavior if solo
}

```

### 5. Status Effect Checks

```csharp
// Check player status before applying redundant effects
bool playerSilenced = player.SilencedTurnsRemaining > 0;
bool playerVulnerable = player.VulnerableTurnsRemaining > 0;

if (!playerSilenced && roll < 40)
{
    return "InflictSilence"; // Don't re-apply Silenced
}

```

---

## Validation Checklist

After implementing your AI behavior method, verify:

**Probability Distribution**:

- [ ]  All probability thresholds add up to 100% (0-99 total)
- [ ]  Probabilities match Step 4 of Enemy Design Worksheet (±5% acceptable)
- [ ]  Archetype pattern matches (Aggressive: 70-90% offense, Defensive: 40-50% attack, etc.)

**Phase Logic** (if mini-boss/boss):

- [ ]  Phase transitions trigger at correct HP thresholds (50%, 25%)
- [ ]  `enemy.Phase` property updated before returning transition ability
- [ ]  Each phase has distinct probability distribution (Phase 2 more aggressive than Phase 1)

**Conditional Logic**:

- [ ]  Low HP triggers use correct thresholds (< 30-40% for emergencies)
- [ ]  One-time abilities use `HasUsedSpecialAbility` flag
- [ ]  Ally-dependent logic handles empty ally list gracefully
- [ ]  Status effect checks prevent redundant applications

**Code Quality**:

- [ ]  Method name follows convention: `Determine[EnemyName]Action(Enemy enemy)`
- [ ]  Added to `DetermineAction()` switch statement in EnemyAI.cs
- [ ]  Comments explain probability breakdowns
- [ ]  Returns valid action strings (BasicAttack, DefensiveStance, ability names from combat system)

---

## Testing Your AI Implementation

**In-Game Testing**:

1. Spawn enemy in test encounter
2. Observe 10-20 turns of behavior
3. Manually count action frequencies:
    - Did BasicAttack occur ~70% of the time for Aggressive archetype?
    - Did DefensiveStance occur ~35% for Tank archetype?
    - Did phase transitions trigger at correct HP thresholds?
4. Verify low HP triggers (reduce enemy HP to < 30%, observe emergency behavior)

**Example Test Log**:

```
Turn 1: BasicAttack (roll: 42)
Turn 2: BasicAttack (roll: 15)
Turn 3: SpecialAbility (roll: 68)
Turn 4: BasicAttack (roll: 33)
Turn 5: DefensiveStance (roll: 91)
...
Turn 15: LastStand (HP: 12/40, emergency trigger)

Frequency Analysis:
- BasicAttack: 11/15 turns = 73% (target: 70%, ✓)
- SpecialAbility: 3/15 turns = 20% (target: 20%, ✓)
- DefensiveStance: 1/15 turns = 7% (target: 10%, ~✓)

```

---

## Reference Documentation

**Related Files**:

- **Enemy Design Worksheet**: `/docs/templates/enemy-design-worksheet.md` (Step 4 probability design)
- **SPEC-COMBAT-012**: `/docs/00-specifications/combat/enemy-design-spec.md` (FR-004 AI Behavior Patterns)
- **EnemyAI.cs**: `RuneAndRust.Engine/EnemyAI.cs` (actual implementation file)
- **Enemy.cs**: `RuneAndRust.Core/Enemy.cs` (Enemy class properties: HP, Phase, HasUsedSpecialAbility)

**SPEC-COMBAT-012 References**:

- **FR-004**: AI Behavior Patterns (archetype-specific probability distributions)
- **FR-002**: Stat Budget Allocation (attribute effects on AI decisions)
- **FR-003**: Special Mechanics Flags (IsBoss, IsForlorn impact on AI)

---

**Last Updated**: 2025-11-22
**Status**: ✅ Complete