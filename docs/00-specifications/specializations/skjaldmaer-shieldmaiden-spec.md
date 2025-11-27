# Skjaldmaer (Shieldmaiden) Specialization - Complete Specification

> **Specification ID**: SPEC-SPECIALIZATION-SKJALDMAER
> **Version**: 1.0
> **Last Updated**: 2025-11-27
> **Status**: Draft - Implementation Review

---

## Document Control

### Purpose
This document provides the complete specification for the Skjaldmaer (Shieldmaiden) specialization, including:
- Design philosophy and mechanical identity
- All 9 abilities with full specifications
- Current implementation status
- GUI integration requirements
- Combat system integration points
- Testing requirements

This document serves as a template for documenting other specializations.

### Related Files
| Component | File Path | Status |
|-----------|-----------|--------|
| Service Implementation | `RuneAndRust.Engine/SkjaldmaerService.cs` | Implemented |
| Data Seeding | `RuneAndRust.Persistence/SkjaldmaerSeeder.cs` | Implemented |
| Tests | `RuneAndRust.Tests/SkjaldmaerSpecializationTests.cs` | Implemented |
| Specialization Tree UI | `RuneAndRust.DesktopUI/Views/SpecializationTreeView.axaml` | Generic (not Skjaldmaer-specific) |
| Combat UI | `RuneAndRust.DesktopUI/Views/CombatView.axaml` | No specialization integration |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
|----------|-------|
| **Internal Name** | Skjaldmaer |
| **Display Name** | Shieldmaiden |
| **Specialization ID** | 26003 |
| **Archetype** | Warrior (ArchetypeID = 1) |
| **Path Type** | Coherent |
| **Mechanical Role** | Tank / Psychic Stress Mitigation |
| **Primary Attribute** | STURDINESS |
| **Secondary Attribute** | WILL |
| **Resource System** | Stamina |
| **Trauma Risk** | Low |
| **Icon** | :shield: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 10 PP | Higher than standard 3 PP (reflects power level) |
| **Minimum Legend** | 5 | Mid-game specialization |
| **Maximum Corruption** | 100 | Coherent path - no corruption restriction |
| **Minimum Corruption** | 0 | No minimum corruption |
| **Required Quest** | None | No quest prerequisite |

### 1.3 Design Philosophy

**Tagline**: "The Bastion of Coherence"

**Core Fantasy**: The Skjaldmaer is a living firewall against both physical trauma and mental breakdown. In a world where reality glitches, she shields not just bodies but sanity itself. Her shield is a grounding rod against the psychic scream of the Great Silence.

**Mechanical Identity**:
1. **Dual Protection**: Shields both HP and Psychic Stress simultaneously
2. **Trauma Economy Anchor**: Actively mitigates party Psychic Stress through abilities and auras
3. **WILL-Based Tanking**: Taunt system draws aggro with WILL-based projection of coherence
4. **Ultimate Sacrifice**: Capstone ability allows absorbing permanent Trauma to save allies

**Design Pillars**:
- Physical and mental protection are equally important
- Tank role extends beyond damage absorption to sanity preservation
- Resource management creates meaningful decisions (Stamina vs Psychic Stress costs)
- Progression rewards investment with increasingly powerful party-wide effects

### 1.4 Specialization Description (Full Text)

> The bastion of coherence--a living firewall against both physical trauma and mental breakdown. In a world where reality glitches, the Skjaldmaer shields not just bodies but sanity itself. Her shield is a grounding rod against the psychic scream of the Great Silence. Her power comes from indomitable WILL channeled into protection, transforming the tank role from 'meat shield' to 'reality anchor.'
>
> This specialization provides dual protection: shields both HP and Psychic Stress simultaneously. As the Trauma Economy anchor, she actively mitigates party Psychic Stress through abilities and auras. Her taunt system draws aggro with WILL-based projection of coherence. Unparalleled damage reduction and HP pool make her the ultimate soak master.
>
> The ultimate expression is Bastion of Sanity--absorb Trauma to save an ally from permanent mental scarring.

---

## 2. Ability Tree Structure

### 2.1 Tree Overview

| Tier | Name | Abilities | PP Cost Each | PP Required in Tree | Total PP for Tier |
|------|------|-----------|--------------|---------------------|-------------------|
| 1 | Foundation | 3 | 3 PP | 0 PP | 9 PP |
| 2 | Advanced | 3 | 4 PP | 8 PP | 12 PP |
| 3 | Mastery | 2 | 5 PP | 16 PP | 10 PP |
| 4 | Capstone | 1 | 6 PP | 24 PP + both Tier 3 | 6 PP |
| **Total** | | **9** | | | **37 PP** |

### 2.2 Ability Summary Table

| ID | Ability Name | Tier | Type | Action | Target | Resource Cost | Key Effect |
|----|--------------|------|------|--------|--------|---------------|------------|
| 26019 | Sanctified Resolve | 1 | Passive | Free | Self | None | +dice vs Fear/Stress |
| 26020 | Shield Bash | 1 | Active | Standard | Single Enemy | 40 Stamina | Damage + Stagger |
| 26021 | Oath of the Protector | 1 | Active | Standard | Single Ally | 35 Stamina | +Soak, +Stress resist |
| 26022 | Guardian's Taunt | 2 | Active | Standard | AoE Enemies | 30 Stamina + Stress | Taunt enemies |
| 26023 | Shield Wall | 2 | Active | Standard | Self + Allies | 45 Stamina | AoE defensive buff |
| 26024 | Interposing Shield | 2 | Reaction | Reaction | Adjacent Ally | 25 Stamina | Redirect critical hit |
| 26025 | Implacable Defense | 3 | Active | Standard | Self | 40 Stamina | Debuff immunity |
| 26026 | Aegis of the Clan | 3 | Passive | Free | Allies | None | Auto-protect stressed allies |
| 26027 | Bastion of Sanity | 4 | Passive+Reaction | Passive/Reaction | Aura/Ally | 40 Stress + 1 Corruption | Absorb ally Trauma |

---

## 3. Detailed Ability Specifications

### 3.1 Tier 1: Foundational Resolve

---

#### 3.1.1 Sanctified Resolve (ID: 26019)

**Type**: Passive | **Action**: Free Action | **Target**: Self

**Description**: Mental fortitude training grants resistance to Fear and Psychic Stress.

**Mechanical Summary**: Bonus dice vs [Fear] and Psychic Stress resistance checks

**Resource Cost**: None

**Attribute Used**: WILL

**Rank Progression**:

| Rank | PP Cost | Effect |
|------|---------|--------|
| 1 | 3 PP | +1 die to WILL Resolve Checks vs [Fear] and Psychic Stress |
| 2 | +20 PP | +2 dice to WILL Resolve Checks vs [Fear] and Psychic Stress |
| 3 | +0 PP | +3 dice + reduce ambient Psychic Stress gain by 10% |

**Implementation Status**:
- [x] Service method: `SkjaldmaerService.GetSanctifiedResolveBonus(rank)`
- [x] Service method: `SkjaldmaerService.GetSanctifiedResolveStressReduction(rank)`
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier1()`
- [ ] GUI: Passive indicator in combat UI
- [ ] Combat: Integration with WILL Resolve Check system
- [ ] Combat: Ambient Stress reduction modifier application

**GUI Requirements**:
- Show passive buff icon when active (in character status area)
- Display bonus dice in roll tooltips during relevant checks
- Rank 3: Show ambient stress reduction indicator

**Combat Integration**:
```
When character makes WILL Resolve Check vs [Fear] or Psychic Stress:
  bonusDice = SkjaldmaerService.GetSanctifiedResolveBonus(ability.CurrentRank)
  rollPool += bonusDice

When calculating ambient Psychic Stress gain:
  reduction = SkjaldmaerService.GetSanctifiedResolveStressReduction(ability.CurrentRank)
  ambientStress *= (1.0 - reduction)
```

---

#### 3.1.2 Shield Bash (ID: 26020)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Melee)

**Description**: Slam shield into foe--a brutal statement of physical truth.

**Mechanical Summary**: Physical melee attack with [Staggered] chance

**Resource Cost**: 40 Stamina

**Attribute Used**: MIGHT

**Damage Type**: Physical

**Status Effects**: [Staggered]

**Rank Progression**:

| Rank | PP Cost | Damage | Stagger Chance | Special |
|------|---------|--------|----------------|---------|
| 1 | 3 PP | 1d8 + MIGHT | 50% | - |
| 2 | +20 PP | 2d8 + MIGHT | 65% | - |
| 3 | +0 PP | 3d8 + MIGHT | 75% | Push to Back Row on Stagger |

**Implementation Status**:
- [x] Service method: `SkjaldmaerService.ExecuteShieldBash(caster, target, rank)`
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier1()`
- [ ] GUI: Ability button in combat action panel
- [ ] GUI: Target selection for single enemy
- [ ] Combat: Integration with CombatEngine.PlayerUseAbility()
- [ ] Status Effect: [Staggered] definition and icon

**GUI Requirements**:
- Ability appears in combat ability list when unlocked
- Shows Stamina cost (40) before use
- Target selection highlights valid melee targets
- Combat log shows damage dealt and Stagger result
- Rank 3: Show "Pushed to Back Row" in combat log

**Combat Integration**:
```csharp
// In CombatEngine or ability execution flow:
if (ability.Name == "Shield Bash" && character.HasSpecialization(26003))
{
    var service = new SkjaldmaerService(connectionString);
    var (damage, staggered, pushed, message) = service.ExecuteShieldBash(
        caster, target, ability.CurrentRank);

    target.HP -= damage;
    if (staggered) target.AddStatusEffect("Staggered", duration: 1);
    if (pushed) target.Position = MoveToBackRow(target.Position);

    combatState.AddLogEntry(message);
}
```

**Status Effect Definition Needed**:
```csharp
new StatusEffectDefinition {
    Name = "Staggered",
    Description = "Off-balance and vulnerable. -1 to Defense, cannot take Reactions.",
    Duration = 1,
    Icon = "staggered.png",
    MechanicalEffects = new[] { "-1 Defense", "No Reactions" }
}
```

---

#### 3.1.3 Oath of the Protector (ID: 26021)

**Type**: Active | **Action**: Standard Action | **Target**: Single Ally

**Description**: Extend protective aura to single ally, shielding flesh and mind.

**Mechanical Summary**: Single target ally buff: +Soak and +Psychic Stress resistance

**Resource Cost**: 35 Stamina

**Rank Progression**:

| Rank | PP Cost | Soak Bonus | Stress Dice | Duration | Special |
|------|---------|------------|-------------|----------|---------|
| 1 | 3 PP | +2 Soak | +1 die | 2 turns | - |
| 2 | +20 PP | +3 Soak | +2 dice | 2 turns | - |
| 3 | +0 PP | +4 Soak | +2 dice | 3 turns | Cleanse 1 mental debuff |

**Implementation Status**:
- [x] Service method: `SkjaldmaerService.ExecuteOathOfProtector(caster, target, rank)`
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier1()`
- [ ] GUI: Ability button with ally targeting
- [ ] GUI: Target selection highlights allies
- [ ] Combat: Apply buff to target character
- [ ] Combat: Track buff duration

**GUI Requirements**:
- Ability appears in combat ability list when unlocked
- Target selection mode for allies only
- Show buff icon on protected ally
- Display remaining duration on buff icon
- Rank 3: Show "Cleansed [Effect]" in combat log

**Combat Integration**:
```csharp
if (ability.Name == "Oath of the Protector" && character.HasSpecialization(26003))
{
    var service = new SkjaldmaerService(connectionString);
    var (soakBonus, stressDice, duration, cleansed, message) =
        service.ExecuteOathOfProtector(caster, targetAlly, ability.CurrentRank);

    targetAlly.AddBuff(new Buff {
        Name = "Oath of the Protector",
        SoakBonus = soakBonus,
        StressResistanceDice = stressDice,
        RemainingDuration = duration,
        Source = caster.Name
    });

    combatState.AddLogEntry(message);
}
```

---

### 3.2 Tier 2: Advanced Guardianship

---

#### 3.2.1 Guardian's Taunt (ID: 26022)

**Type**: Active | **Action**: Standard Action | **Target**: AoE Front Row / All Enemies

**Description**: Projection of coherent will draws even maddened creatures to attack.

**Mechanical Summary**: Taunt enemies to attack caster; costs Psychic Stress

**Resource Cost**: 30 Stamina + Variable Psychic Stress (see ranks)

**Prerequisites**: 8 PP invested in Skjaldmaer tree

**Status Effects Applied**: [Taunted]

**Rank Progression**:

| Rank | PP Cost | Targets | Duration | Stress Cost | Special |
|------|---------|---------|----------|-------------|---------|
| 1 | 4 PP | Front Row enemies | 2 rounds | 5 Stress | - |
| 2 | +20 PP | Front Row enemies | 2 rounds | 3 Stress | Reduced cost |
| 3 | +0 PP | ALL enemies | 2 rounds | 5 Stress | Affects back row |

**Implementation Status**:
- [x] Service method: `SkjaldmaerService.ExecuteGuardiansTaunt(caster, frontRow, backRow, rank)`
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier2()`
- [ ] GUI: Ability button (no target selection - AoE)
- [ ] GUI: Show Psychic Stress cost in ability tooltip
- [ ] Combat: Apply [Taunted] status to affected enemies
- [ ] Status Effect: [Taunted] definition and icon

**GUI Requirements**:
- Show both Stamina AND Psychic Stress cost
- Display number of enemies that will be taunted
- Warning if Stress cost would trigger Breaking Point
- Combat log shows all affected enemies

**Combat Integration**:
```csharp
if (ability.Name == "Guardian's Taunt" && character.HasSpecialization(26003))
{
    var service = new SkjaldmaerService(connectionString);
    var frontRow = combatState.Enemies.Where(e => e.Position?.Row == Row.Front).ToList();
    var backRow = combatState.Enemies.Where(e => e.Position?.Row == Row.Back).ToList();

    var (taunted, stressCost, duration, message) =
        service.ExecuteGuardiansTaunt(caster, frontRow, backRow, ability.CurrentRank);

    // Apply Taunted status to affected enemies
    var targetEnemies = ability.CurrentRank >= 3 ?
        frontRow.Concat(backRow) : frontRow;

    foreach (var enemy in targetEnemies)
    {
        enemy.AddStatusEffect(new StatusEffect {
            Name = "Taunted",
            TauntTarget = caster,
            RemainingDuration = duration
        });
    }

    combatState.AddLogEntry(message);
}
```

**Status Effect Definition Needed**:
```csharp
new StatusEffectDefinition {
    Name = "Taunted",
    Description = "Must target the taunting character with attacks if possible.",
    Duration = 2,
    Icon = "taunted.png",
    MechanicalEffects = new[] { "Must attack taunt source" },
    TauntSource = characterId
}
```

---

#### 3.2.2 Shield Wall (ID: 26023)

**Type**: Active | **Action**: Standard Action | **Target**: Self + Adjacent Front Row Allies

**Description**: Plant feet creating bastion of physical and metaphysical stability.

**Mechanical Summary**: AoE defensive buff: +Soak, immune to Push/Pull, +Stress resistance

**Resource Cost**: 45 Stamina

**Prerequisites**: 8 PP invested in Skjaldmaer tree

**Status Effects Applied**: [Fortified]

**Rank Progression**:

| Rank | PP Cost | Soak Bonus | Stress Dice | Duration | Special |
|------|---------|------------|-------------|----------|---------|
| 1 | 4 PP | +3 Soak | +1 die | 2 turns | Immune to Push/Pull |
| 2 | +20 PP | +4 Soak | +2 dice | 2 turns | Immune to Push/Pull |
| 3 | +0 PP | +5 Soak | +2 dice | 3 turns | [Fortified] status |

**Implementation Status**:
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier2()`
- [ ] Service method: Not yet implemented (needs ExecuteShieldWall)
- [ ] GUI: Ability button (auto-target self + adjacent allies)
- [ ] Combat: Apply buff to multiple targets
- [ ] Status Effect: [Fortified] definition and icon

**GUI Requirements**:
- Preview affected allies before confirmation
- Show area of effect on combat grid
- Buff icons on all affected characters
- Duration tracking per character

**Status Effect Definition Needed**:
```csharp
new StatusEffectDefinition {
    Name = "Fortified",
    Description = "Heavily defended position. Cannot be moved by forced movement.",
    Duration = 2,
    Icon = "fortified.png",
    MechanicalEffects = new[] { "Immune to Push", "Immune to Pull", "Immune to Knockback" }
}
```

---

#### 3.2.3 Interposing Shield (ID: 26024)

**Type**: Reaction | **Action**: Reaction | **Target**: Adjacent Ally

**Description**: React to incoming Critical Hit on adjacent ally, redirecting to self.

**Mechanical Summary**: Reaction: Redirect Critical Hit to self with damage reduction

**Resource Cost**: 25 Stamina

**Prerequisites**: 8 PP invested in Skjaldmaer tree

**Trigger Condition**: Adjacent ally is hit by a Critical Hit

**Rank Progression**:

| Rank | PP Cost | Damage Taken | Special |
|------|---------|--------------|---------|
| 1 | 4 PP | 50% of original damage | Once per round |
| 2 | +20 PP | 40% of original damage | Once per round |
| 3 | +0 PP | 30% of original damage | Reflect 10% back to attacker |

**Implementation Status**:
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier2()`
- [ ] Service method: Not yet implemented (needs ExecuteInterposingShield)
- [ ] GUI: **Reaction prompt system needed**
- [ ] Combat: Detect critical hit on adjacent ally
- [ ] Combat: Prompt player for reaction decision

**GUI Requirements** (CRITICAL GAP):
- **Reaction Prompt Dialog**: When trigger condition met:
  - Show dialog: "Interposing Shield: [Ally] is about to take [X] critical damage. Intercept?"
  - Show: Cost (25 Stamina), damage you would take (X * reduction%)
  - Options: [Intercept] [Decline]
  - Auto-decline if insufficient Stamina
- Combat log shows interception and damage transfer

**Combat Integration**:
```csharp
// In damage resolution, BEFORE applying critical hit damage:
if (attack.IsCriticalHit && defender.IsAlly)
{
    var skjaldmaer = combatState.GetAdjacentCharacterWithAbility(defender, "Interposing Shield");
    if (skjaldmaer != null && skjaldmaer.Stamina >= 25)
    {
        // PROMPT PLAYER for reaction decision
        var decision = await combatUI.ShowReactionPrompt(new ReactionPrompt {
            AbilityName = "Interposing Shield",
            Trigger = $"{defender.Name} is about to take {attack.Damage} critical damage",
            Cost = "25 Stamina",
            Effect = $"Take {CalculateReducedDamage(attack.Damage, rank)}% damage instead"
        });

        if (decision == ReactionDecision.Accept)
        {
            var reducedDamage = CalculateReducedDamage(attack.Damage, ability.CurrentRank);
            skjaldmaer.HP -= reducedDamage;
            skjaldmaer.Stamina -= 25;

            if (ability.CurrentRank >= 3)
            {
                var reflectedDamage = (int)(attack.Damage * 0.10);
                attacker.HP -= reflectedDamage;
            }

            // Original target takes NO damage
            return; // Skip normal damage application
        }
    }
}
```

---

### 3.3 Tier 3: Mastery of the Bulwark

---

#### 3.3.1 Implacable Defense (ID: 26025)

**Type**: Active | **Action**: Standard Action | **Target**: Self

**Description**: Achieve state of perfect focus--immovable against physical and mental assault.

**Mechanical Summary**: Self-buff: Immune to major debuffs and +Soak

**Resource Cost**: 40 Stamina

**Prerequisites**: 16 PP invested in Skjaldmaer tree

**Rank Progression**:

| Rank | PP Cost | Duration | Immunities | Soak | Special |
|------|---------|----------|------------|------|---------|
| 1 | 5 PP | 3 turns | [Stun], [Staggered], [Knocked Down], [Fear], [Disoriented] | +0 | - |
| 2 | +20 PP | 3 turns | Same | +2 | - |
| 3 | +0 PP | 3 turns | Same | +3 | Aura: adjacent allies immune to [Fear] |

**Implementation Status**:
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier3()`
- [ ] Service method: Not yet implemented
- [ ] GUI: Self-targeting ability button
- [ ] Combat: Apply immunity buff
- [ ] Combat: Rank 3 aura effect on adjacent allies

**GUI Requirements**:
- Show immunity buff icon with duration
- Rank 3: Show aura indicator on adjacent allies
- Tooltip lists all immunities

---

#### 3.3.2 Aegis of the Clan (ID: 26026)

**Type**: Passive | **Action**: Free Action | **Target**: Allies in Crisis

**Description**: Automatic protection triggers when ally enters mental crisis.

**Mechanical Summary**: Passive: Auto-apply Oath of Protector when ally Stress hits 66%+

**Resource Cost**: None (automatic)

**Prerequisites**: 16 PP invested in Skjaldmaer tree

**Trigger Condition**: Any ally's Psychic Stress crosses 66% threshold

**Rank Progression**:

| Rank | PP Cost | Duration | Limit | Special |
|------|---------|----------|-------|---------|
| 1 | 5 PP | 1 turn | Once per ally per combat | Auto-applies Oath |
| 2 | +20 PP | 2 turns | Once per ally per combat | - |
| 3 | +0 PP | 2 turns | Once per ally per combat | Also reduce Stress by 10 |

**Implementation Status**:
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier3()`
- [ ] Service method: Not yet implemented
- [ ] Combat: Monitor ally Psychic Stress changes
- [ ] Combat: Automatic trigger when threshold crossed
- [ ] GUI: Notification when ability triggers

**Combat Integration**:
```csharp
// In TraumaEconomyService.AddStress() or wherever stress is modified:
public void OnAllyStressChanged(PlayerCharacter ally, int previousStress, int newStress)
{
    // Check if crossed 66% threshold
    int maxStress = ally.MaxPsychicStress; // Usually 100
    int threshold = (int)(maxStress * 0.66);

    if (previousStress < threshold && newStress >= threshold)
    {
        // Find Skjaldmaer with Aegis of the Clan
        var skjaldmaer = party.FindCharacterWithAbility("Aegis of the Clan");
        if (skjaldmaer != null && !HasUsedAegisOnAlly(skjaldmaer, ally))
        {
            // Auto-trigger Oath of the Protector
            var rank = GetAbilityRank(skjaldmaer, "Aegis of the Clan");
            ApplyOathOfProtectorBuff(ally, rank);

            if (rank >= 3)
            {
                ally.PsychicStress = Math.Max(0, ally.PsychicStress - 10);
            }

            MarkAegisUsedOnAlly(skjaldmaer, ally);
            combatLog.Add($"Aegis of the Clan triggers! {ally.Name} receives Oath protection.");
        }
    }
}
```

---

### 3.4 Tier 4: Capstone

---

#### 3.4.1 Bastion of Sanity (ID: 26027)

**Type**: Passive + Reaction | **Action**: Passive / Reaction | **Target**: Aura (All Allies) / Ally in Crisis

**Description**: Become living Runic Anchor--a kernel of stable reality.

**Mechanical Summary**: Passive aura + Reaction: Absorb ally's permanent Trauma

**Resource Cost**: Reaction component costs 40 Psychic Stress + 1 Corruption

**Prerequisites**:
- 24 PP invested in Skjaldmaer tree
- Must have both Tier 3 abilities (Implacable Defense + Aegis of the Clan)

**Cooldown**: Once per combat (reaction component)

**Components**:

**PASSIVE AURA** (always active while in Front Row):
- All allies in same row gain +1 WILL
- All allies in same row gain -10% ambient Psychic Stress gain

**REACTION** (once per combat):
- **Trigger**: Ally would gain permanent Trauma from Breaking Point
- **Effect**: Skjaldmaer absorbs the Trauma
- **Cost to Skjaldmaer**: 40 Psychic Stress + 1 Corruption
- **Result**: Ally avoids Trauma entirely

**Implementation Status**:
- [x] Service method: `SkjaldmaerService.TriggerBastionOfSanity(skjaldmaer, ally, trauma, alreadyUsed)`
- [x] Service method: `SkjaldmaerService.GetBastionOfSanityAura()`
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerCapstone()`
- [ ] GUI: **Reaction prompt for Trauma absorption**
- [ ] GUI: Aura indicator on affected allies
- [ ] Combat: Detect Breaking Point in TraumaEconomyService
- [ ] Combat: Prompt for Trauma absorption decision

**GUI Requirements** (CRITICAL GAP):

**Passive Aura Display**:
- Show aura icon on Skjaldmaer when in Front Row
- Show buff icon on allies affected by aura
- Tooltip: "+1 WILL, -10% Psychic Stress gain"

**Reaction Prompt** (when ally would gain Trauma):
```
+------------------------------------------+
| BASTION OF SANITY                        |
+------------------------------------------+
| [Ally Name] is about to gain permanent   |
| Trauma: [Trauma Name]                    |
|                                          |
| Absorb this Trauma?                      |
|                                          |
| COST TO YOU:                             |
|   - 40 Psychic Stress                    |
|   - 1 Corruption                         |
|                                          |
| [Absorb Trauma]    [Let Trauma Pass]     |
+------------------------------------------+
```

**Combat Integration**:
```csharp
// In TraumaEconomyService.OnBreakingPoint():
public async Task<Trauma?> OnBreakingPoint(PlayerCharacter character)
{
    var trauma = DetermineTrauma(character);

    // Check for Bastion of Sanity intervention
    var skjaldmaer = party.FindCharacterWithAbility("Bastion of Sanity");
    if (skjaldmaer != null &&
        skjaldmaer.IsInFrontRow() &&
        !HasUsedBastionThisCombat(skjaldmaer))
    {
        // PROMPT PLAYER
        var decision = await combatUI.ShowReactionPrompt(new ReactionPrompt {
            AbilityName = "Bastion of Sanity",
            Title = "Absorb Ally's Trauma?",
            Trigger = $"{character.Name} is about to gain permanent Trauma: {trauma.Name}",
            Cost = "40 Psychic Stress, 1 Corruption",
            Effect = $"{character.Name} avoids Trauma entirely"
        });

        if (decision == ReactionDecision.Accept)
        {
            var service = new SkjaldmaerService(connectionString);
            var (triggered, stress, corruption, message) =
                service.TriggerBastionOfSanity(skjaldmaer, character, trauma, false);

            MarkBastionUsedThisCombat(skjaldmaer);
            combatLog.Add(message);

            return null; // No trauma for original character
        }
    }

    // Normal trauma application
    character.Traumas.Add(trauma);
    return trauma;
}
```

---

## 4. Implementation Gap Analysis

### 4.1 Backend Implementation Status

| Component | Status | Notes |
|-----------|--------|-------|
| `SkjaldmaerService.cs` | :white_check_mark: Implemented | 4 ability methods + 3 passive getters |
| `SkjaldmaerSeeder.cs` | :white_check_mark: Implemented | All 9 abilities seeded |
| `SkjaldmaerSpecializationTests.cs` | :white_check_mark: Implemented | 473 lines of tests |
| CombatEngine integration | :x: Missing | Service methods never called |
| Status effect definitions | :x: Missing | [Taunted], [Fortified] not defined |

### 4.2 GUI Integration Status

| Component | Status | Priority | Notes |
|-----------|--------|----------|-------|
| Ability list in combat | :x: Missing | HIGH | No way to select Skjaldmaer abilities |
| Target selection for allies | :x: Missing | HIGH | Oath of Protector needs ally targeting |
| Stamina display in combat | :x: Missing | CRITICAL | Can't see resource availability |
| Psychic Stress display | :x: Missing | CRITICAL | Guardian's Taunt costs Stress |
| Reaction prompt system | :x: Missing | HIGH | Interposing Shield, Bastion of Sanity |
| Passive ability indicators | :x: Missing | MEDIUM | Sanctified Resolve, Aegis, Auras |
| Status effect icons | :x: Missing | MEDIUM | [Taunted], [Fortified], [Staggered] |
| Buff duration tracking | :x: Missing | MEDIUM | Show remaining turns on buffs |

### 4.3 Combat System Integration Status

| Integration Point | Status | Notes |
|-------------------|--------|-------|
| Ability execution routing | :x: Missing | CombatEngine doesn't call SkjaldmaerService |
| Status effect system | Partial | Some effects exist, Skjaldmaer-specific missing |
| Reaction system | :x: Missing | No framework for reaction prompts |
| Aura effect system | :x: Missing | Passive auras not tracked |
| Breaking Point interception | :x: Missing | Bastion of Sanity trigger |
| Stress threshold monitoring | :x: Missing | Aegis of the Clan trigger |

---

## 5. GUI Integration Requirements

### 5.1 Combat View Enhancements Needed

#### 5.1.1 Resource Display Panel

Add to `CombatView.axaml` - Player resource bars:

```xml
<!-- Player Resources Panel -->
<Border Grid.Row="0" Grid.Column="1" ...>
    <StackPanel>
        <!-- HP Bar (existing) -->
        <TextBlock Text="HP"/>
        <ProgressBar Value="{Binding PlayerHP}" Maximum="{Binding PlayerMaxHP}"/>

        <!-- Stamina Bar (NEEDED) -->
        <TextBlock Text="Stamina"/>
        <ProgressBar Value="{Binding PlayerStamina}" Maximum="{Binding PlayerMaxStamina}"
                     Foreground="Green"/>

        <!-- Psychic Stress Bar (NEEDED) -->
        <TextBlock Text="Psychic Stress"/>
        <ProgressBar Value="{Binding PlayerPsychicStress}" Maximum="100"
                     Foreground="Purple"/>
        <TextBlock Text="{Binding PsychicStressThreshold}" FontSize="10"/>
    </StackPanel>
</Border>
```

#### 5.1.2 Ability Selection Dialog

New component needed: `AbilitySelectionDialog.axaml`

```xml
<Window Title="Select Ability">
    <ItemsControl ItemsSource="{Binding AvailableAbilities}">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Button Command="{Binding SelectAbilityCommand}"
                        CommandParameter="{Binding}">
                    <StackPanel>
                        <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                        <TextBlock Text="{Binding ResourceCostText}" FontSize="10"/>
                        <TextBlock Text="{Binding MechanicalSummary}" FontSize="10"/>
                    </StackPanel>
                </Button>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</Window>
```

#### 5.1.3 Reaction Prompt Dialog

New component needed: `ReactionPromptDialog.axaml`

```xml
<Window Title="{Binding AbilityName}">
    <StackPanel Margin="20">
        <TextBlock Text="{Binding TriggerDescription}" TextWrapping="Wrap"/>
        <Border Background="#2a2a2a" Padding="10" Margin="0,10">
            <StackPanel>
                <TextBlock Text="Cost:" FontWeight="Bold"/>
                <TextBlock Text="{Binding CostDescription}"/>
            </StackPanel>
        </Border>
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
            <Button Content="Accept" Command="{Binding AcceptCommand}"/>
            <Button Content="Decline" Command="{Binding DeclineCommand}"/>
        </StackPanel>
    </StackPanel>
</Window>
```

### 5.2 CombatViewModel Enhancements

Add to `CombatViewModel.cs`:

```csharp
// Resource properties (NEEDED)
public int PlayerStamina => _combatState?.Player.Stamina ?? 0;
public int PlayerMaxStamina => _combatState?.Player.MaxStamina ?? 100;
public int PlayerPsychicStress => _combatState?.Player.PsychicStress ?? 0;
public string PsychicStressThreshold => PlayerPsychicStress switch
{
    < 33 => "Low",
    < 66 => "Moderate",
    < 100 => "HIGH - Breaking Point risk!",
    _ => "BREAKING POINT"
};

// Ability selection (NEEDED)
public ObservableCollection<AbilityViewModel> AvailableAbilities { get; } = new();

// Reaction system (NEEDED)
public async Task<ReactionDecision> ShowReactionPromptAsync(ReactionPrompt prompt)
{
    var dialog = new ReactionPromptDialog { DataContext = prompt };
    return await dialog.ShowDialog<ReactionDecision>(mainWindow);
}
```

### 5.3 Status Effect Icon Mappings

Add to `StatusEffectIconService.cs`:

```csharp
private static readonly Dictionary<string, string> SkjaldmaerStatusIcons = new()
{
    ["Taunted"] = "Icons/StatusEffects/taunted.png",
    ["Fortified"] = "Icons/StatusEffects/fortified.png",
    ["Staggered"] = "Icons/StatusEffects/staggered.png",
    ["Oath of the Protector"] = "Icons/StatusEffects/oath_protection.png",
    ["Implacable Defense"] = "Icons/StatusEffects/implacable.png",
    ["Bastion Aura"] = "Icons/StatusEffects/bastion_aura.png"
};
```

---

## 6. Combat System Integration Requirements

### 6.1 CombatEngine Integration

Modify `CombatEngine.cs` to route Skjaldmaer abilities:

```csharp
public bool PlayerUseAbility(CombatState state, Ability ability, Enemy? target)
{
    // Check if this is a Skjaldmaer ability
    if (IsSkjaldmaerAbility(ability))
    {
        return ExecuteSkjaldmaerAbility(state, ability, target);
    }

    // ... existing ability handling
}

private bool ExecuteSkjaldmaerAbility(CombatState state, Ability ability, object? target)
{
    var service = new SkjaldmaerService(_connectionString);
    var character = state.Player;
    var rank = ability.CurrentRank;

    switch (ability.Name)
    {
        case "Shield Bash":
            if (target is Enemy enemy)
            {
                var result = service.ExecuteShieldBash(character, enemy, rank);
                // Apply damage and status effects
                return true;
            }
            break;

        case "Oath of the Protector":
            if (target is PlayerCharacter ally)
            {
                var result = service.ExecuteOathOfProtector(character, ally, rank);
                // Apply buff
                return true;
            }
            break;

        case "Guardian's Taunt":
            // AoE - no specific target
            var frontRow = state.Enemies.Where(e => e.Position?.Row == Row.Front).ToList();
            var backRow = state.Enemies.Where(e => e.Position?.Row == Row.Back).ToList();
            var result = service.ExecuteGuardiansTaunt(character, frontRow, backRow, rank);
            // Apply Taunted status
            return true;

        // ... other abilities
    }

    return false;
}
```

### 6.2 Reaction System Framework

New service needed: `ReactionService.cs`

```csharp
public class ReactionService
{
    private readonly ICombatUI _combatUI;

    public async Task<bool> CheckAndPromptReactions(
        CombatState state,
        ReactionTrigger trigger,
        object triggerContext)
    {
        // Find characters with reactions that match this trigger
        var reactors = FindCharactersWithReaction(state, trigger);

        foreach (var reactor in reactors)
        {
            if (CanUseReaction(reactor, trigger))
            {
                var prompt = BuildReactionPrompt(reactor, trigger, triggerContext);
                var decision = await _combatUI.ShowReactionPromptAsync(prompt);

                if (decision == ReactionDecision.Accept)
                {
                    ExecuteReaction(reactor, trigger, triggerContext);
                    return true; // Reaction used
                }
            }
        }

        return false; // No reaction used
    }
}

public enum ReactionTrigger
{
    CriticalHitOnAlly,      // Interposing Shield
    AllyWouldGainTrauma,    // Bastion of Sanity
    AllyStressThreshold,    // Aegis of the Clan (passive, no prompt)
    // ... future triggers
}
```

---

## 7. Specialization Tree Integration

### 7.1 Loading Skjaldmaer Abilities

Modify `SpecializationTreeViewModel.cs` to load from database instead of demo data:

```csharp
public void LoadSpecializationAbilities(int specializationId)
{
    var abilities = _abilityRepository.GetAbilitiesForSpecialization(specializationId);

    FoundationTier.Clear();
    CoreTier.Clear();
    AdvancedTier.Clear();
    MasteryTier.Clear();

    foreach (var ability in abilities)
    {
        var node = new AbilityNodeViewModel(ability, Character,
            isUnlocked: CharacterHasAbility(ability.AbilityID),
            currentRank: GetAbilityRank(ability.AbilityID));

        switch (ability.TierLevel)
        {
            case 1: FoundationTier.Add(node); break;
            case 2: CoreTier.Add(node); break;
            case 3: AdvancedTier.Add(node); break;
            case 4: MasteryTier.Add(node); break;
        }
    }
}
```

### 7.2 Unlocking and Ranking Abilities

The current implementation in `SpecializationTreeViewModel.UnlockAbility()` needs to:
1. Persist the unlock to database via `AbilityRepository`
2. Add the ability to character's combat ability list
3. Integrate with `SkjaldmaerService` for ability-specific initialization

---

## 8. Testing Requirements

### 8.1 Existing Tests (SkjaldmaerSpecializationTests.cs)

| Test Category | Count | Status |
|---------------|-------|--------|
| Seeding Tests | 3 | :white_check_mark: Pass |
| Ability Structure Tests | 4 | :white_check_mark: Pass |
| Ability Execution Tests | 6 | :white_check_mark: Pass |
| Guardian's Taunt Tests | 4 | :white_check_mark: Pass |
| Capstone Tests | 3 | :white_check_mark: Pass |
| Passive Ability Tests | 3 | :white_check_mark: Pass |

### 8.2 Additional Tests Needed

| Test Category | Tests Needed |
|---------------|--------------|
| GUI Integration | Ability selection dialog shows Skjaldmaer abilities |
| GUI Integration | Resource bars display correctly |
| GUI Integration | Reaction prompts appear at correct triggers |
| Combat Integration | Shield Bash applies Staggered status |
| Combat Integration | Guardian's Taunt applies Taunted status |
| Combat Integration | Bastion of Sanity intercepts Trauma |
| Combat Integration | Auras apply buffs to correct targets |
| Status Effects | [Taunted] forces targeting |
| Status Effects | [Fortified] prevents forced movement |

---

## 9. Implementation Priority

### Phase 1: Critical (Combat Usability)
1. Add Stamina/Psychic Stress display to CombatView
2. Implement ability selection dialog in combat
3. Route Skjaldmaer abilities through CombatEngine
4. Define missing status effects ([Taunted], [Fortified], [Staggered])

### Phase 2: High (Core Functionality)
5. Implement reaction prompt system
6. Add Interposing Shield reaction handling
7. Add Bastion of Sanity reaction handling
8. Implement ally targeting for Oath of Protector

### Phase 3: Medium (Polish)
9. Add passive ability indicators
10. Implement aura visual effects
11. Add buff duration tracking display
12. Load Skjaldmaer abilities from database in SpecializationTreeView

### Phase 4: Low (Enhancement)
13. Combat log flavor text for Skjaldmaer abilities
14. Sound effects for ability usage
15. Animation polish for Shield Bash, Shield Wall

---

## 10. Appendix: Status Effect Definitions

### [Staggered]
- **Description**: Off-balance and vulnerable
- **Duration**: 1 turn
- **Effects**: -1 Defense, Cannot take Reactions
- **Applied by**: Shield Bash

### [Taunted]
- **Description**: Compelled to attack the taunting character
- **Duration**: 2 turns
- **Effects**: Must target taunt source with attacks if possible
- **Applied by**: Guardian's Taunt
- **Data**: TauntSourceId (character who applied taunt)

### [Fortified]
- **Description**: Heavily defended position
- **Duration**: 2-3 turns
- **Effects**: Immune to Push, Pull, Knockback
- **Applied by**: Shield Wall (Rank 3)

### [Oath of the Protector]
- **Description**: Protected by the Skjaldmaer's oath
- **Duration**: 2-3 turns
- **Effects**: +2-4 Soak, +1-2 dice vs Psychic Stress
- **Applied by**: Oath of the Protector, Aegis of the Clan
- **Data**: SoakBonus, StressResistanceDice

### [Bastion Aura]
- **Description**: Within the Skjaldmaer's protective presence
- **Duration**: While Skjaldmaer in Front Row with Bastion of Sanity
- **Effects**: +1 WILL, -10% ambient Psychic Stress gain
- **Applied by**: Bastion of Sanity (passive component)

---

**End of Specification**
