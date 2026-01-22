# v0.18 Balance Analysis - Phase 1: Data Analysis & Problem Identification

**Date:** 2025-11-12
**Status:** Phase 1 Complete - Ready for Adjustments
**Methodology:** Data-driven analysis using v0.17 documentation as ground truth

---

## Executive Summary

This document analyzes balance across all game systems using extracted data from code and documentation. Key findings:

1. **Abilities**: Significant cost-to-impact disparities, especially in Tier 2-3
2. **Enemies**: TTK variance is acceptable, but Legend value scaling needs tuning
3. **Equipment**: Quality tier progression is smooth, but some outliers exist
4. **Resources**: Stamina economy is too restrictive for Mystic-style builds
5. **Progression**: XP curve is front-loaded, late-game feels grindy

**Priority Balance Issues** (ordered by severity):
1. 🔴 **CRITICAL**: Rally Cry costs too much for its effect (20 Stamina for minimal healing)
2. 🔴 **CRITICAL**: Whirlwind Strike unclear damage calculation (1d8 vs 1d6+1?)
3. 🔴 **CRITICAL**: Miracle Worker Capstone too expensive (60 Stamina, unrealistic to use)
4. 🟡 **HIGH**: Several Tier 2 abilities more expensive than Tier 3 equivalents
5. 🟡 **HIGH**: Enemy Legend values don't match actual difficulty (Trivial enemies = 5 Legend)
6. 🟢 **MEDIUM**: Equipment quality tier bonuses inconsistent
7. 🟢 **MEDIUM**: Stamina regeneration absent (documented but not implemented)

---

## Section 1: Ability Balance Analysis

### 1.1 Warrior Abilities (13 total from AbilityDatabase.cs)

#### Tier 1 (Legend 1-2)

| Ability | Stamina Cost | Effect | Cost-to-Impact Score | Balance Assessment |
|---------|--------------|--------|---------------------|-------------------|
| **Crushing Blow** | 15 | 2d6 damage + prone | 7.5 avg damage / 15 cost = **0.50** | ✅ Balanced |
| **Rally Cry** | 20 | Party heal 1d8, +1 attack, -5 Stress | 4.5 avg heal / 20 cost = **0.23** | 🔴 UNDERPOWERED |

**Analysis:**
- **Rally Cry** is severely undercosted for its effect. Costs 20 Stamina but only heals ~4-5 HP to allies. Compare to:
  - Healing Salve: 2d6 (avg 7 HP) for a consumable
  - Medicinal Tonic: 3d6 (avg 10.5 HP) for a rare consumable
  - Rally Cry should heal at least 2d6 to justify 20 Stamina cost
- **Crushing Blow** is appropriately costed

**Recommendations:**
1. Rally Cry: Increase heal to 2d6 HP (from 1d8), OR reduce cost to 12 Stamina
2. Rally Cry: Extend range to 20ft (from 15ft) to improve tactical positioning

---

#### Tier 2 (Legend 3-4)

| Ability | Stamina Cost | Effect | Cost-to-Impact Score | Balance Assessment |
|---------|--------------|--------|---------------------|-------------------|
| **Whirlwind Strike** | 25 | 1d8 damage to ALL within 5ft | ??? | ⚠️ UNCLEAR |
| **Second Wind** | 20 | Self-heal 2d10 HP, remove Wounded | 11 avg heal / 20 cost = **0.55** | ✅ Balanced |
| **Armor Breaker** | 20 | 2d6 damage, ignores armor, -3 Defense debuff | 7 avg damage / 20 cost = **0.35** (+ debuff) | ✅ Balanced |
| **Intimidating Presence** | 15 | AOE fear (disadvantage on attacks) | N/A (control) | ✅ Balanced |

**Analysis:**
- **Whirlwind Strike**: Damage calculation is ambiguous. Code says "1d8 per target (≈ 1d6+1)" but DamageDice = 1. Need clarification:
  - If it's 1d8 per target: 4.5 avg × 3 targets = 13.5 total damage for 25 Stamina = **0.54 ratio** (balanced)
  - If it's 1d6 per target: 3.5 avg × 3 targets = 10.5 total damage for 25 Stamina = **0.42 ratio** (slightly underpowered)
- **Second Wind** is appropriately costed for self-healing
- **Armor Breaker** provides good value with both damage and debuff

**Recommendations:**
1. Whirlwind Strike: Clarify damage calculation in code comments and description
2. Whirlwind Strike: If damage is 1d6 per target, increase to 1d8 per target to match description

---

#### Tier 3 (Legend 5-6)

| Ability | Stamina Cost | Effect | Cost-to-Impact Score | Balance Assessment |
|---------|--------------|--------|---------------------|-------------------|
| **Unstoppable** | 30 | 3 turns: immune CC, half damage, +2 attack | N/A (defensive buff) | ✅ Balanced |
| **Execute** | 25 | Instakill if HP < 30%, else 2d10 damage | 11 avg / 25 cost = **0.44** (+ instakill) | ✅ Balanced |
| **Bulwark** | 0 | Stance: taunt all within 15ft, +5 Defense | N/A (free stance) | ✅ Balanced |

**Analysis:**
- All Tier 3 abilities are appropriately powerful for their tier
- **Bulwark** being free (stance) is a good design choice
- **Execute** has high variance (instakill vs moderate damage) which creates exciting gameplay moments

**Recommendations:**
- No changes needed for Tier 3 abilities

---

#### Tier 4 (Legend 7+) - Ultimates

| Ability | Stamina Cost | Effect | Cost-to-Impact Score | Balance Assessment |
|---------|--------------|--------|---------------------|-------------------|
| **Titan's Strength** | 40 | 4 turns: double MIGHT, +10 damage, immovable | N/A (massive buff) | ✅ Balanced |
| **Last Stand** | 35 | Prevent death, gain 50 temp HP for 5 turns | N/A (death prevention) | ✅ Balanced |

**Analysis:**
- Both ultimates feel appropriately epic and costly
- Titan's Strength at 40 Stamina is 33% more than a Warrior's base Stamina pool (30), forcing Milestone upgrades
- Last Stand can only be used once per day, limiting spam

**Recommendations:**
- No changes needed for Tier 4 abilities

---

#### Heretical Abilities (Corruption-based)

| Ability | Stamina Cost | Effect | Corruption Cost | Balance Assessment |
|---------|--------------|--------|----------------|-------------------|
| **Embrace the Machine** | 20 | +3 Tech checks, +2 Defense for 1 hour | 5 (permanent) | ✅ Balanced |
| **Jötun-Reader's Gift** | 30 | 3d8 psychic damage, ignores armor, inflicts 5 Corruption | 8 (permanent) | ⚠️ HIGH RISK |
| **Symbiotic Regeneration** | 25 | Heal 3d10 HP, gain [Infected] for 3 turns | 6 (permanent) | ✅ Balanced |

**Analysis:**
- **Jötun-Reader's Gift** deals exceptional damage (avg 13.5, ignores armor) but costs 8 permanent Corruption
- Corruption costs are permanent and lead to Breaking Points (see Trauma Economy doc)
- These abilities should feel powerful but risky - current balance achieves this

**Recommendations:**
- No changes needed for Heretical abilities (risk-reward is appropriate)

---

### 1.2 Specialization Abilities (Adept Trees)

#### Bone-Setter (Support/Healer)

| Ability | Stamina Cost | Effect | Cost-to-Impact Score | Balance Assessment |
|---------|--------------|--------|---------------------|-------------------|
| **Mend Wound** | 5 | Heal using poultice | Variable (consumes item) | ✅ Efficient |
| **Apply Tourniquet** | 0 | Remove [Bleeding] | N/A (free emergency) | ✅ Balanced |
| **Anatomical Insight** | 25 | Apply [Vulnerable] (+25% damage for 3 turns) | N/A (debuff) | ⚠️ EXPENSIVE |
| **Administer Antidote** | 15 | Remove [Poisoned] and [Disease] | N/A (cleanse) | ✅ Balanced |
| **Cognitive Realignment** | 30 | Remove fear/disoriented, restore 2d6 Stress | 7 avg Stress / 30 cost = **0.23** | ⚠️ EXPENSIVE |
| **Miracle Worker** (Capstone) | 60 | Heal 4d6 HP, cleanse ALL physical debuffs | 14 avg heal / 60 cost = **0.23** | 🔴 TOO EXPENSIVE |

**Analysis:**
- **Miracle Worker** costs 60 Stamina, which is:
  - 150% of a Warrior's base Stamina (30)
  - 120% of a Scavenger/Adept's base Stamina (40)
  - Even at Milestone 3, an Adept has only 55 Stamina total
  - This means the Capstone ability is **unusable** without full Stamina + consumables
- **Anatomical Insight** and **Cognitive Realignment** are also expensive for their effects
- Compare to **Mend Wound** (5 Stamina) which is highly efficient

**Recommendations:**
1. **Miracle Worker**: Reduce cost to 40 Stamina (from 60) - still expensive but realistic
2. **Miracle Worker**: Alternatively, increase heal to 6d6 HP to justify 60 Stamina cost
3. **Anatomical Insight**: Reduce cost to 20 Stamina (from 25) OR increase duration to 4 turns
4. **Cognitive Realignment**: Reduce cost to 25 Stamina (from 30)

---

#### Jötun-Reader (Utility/Analyst)

| Ability | Stamina Cost | Effect | Stress Cost | Balance Assessment |
|---------|--------------|--------|------------|-------------------|
| **Analyze Weakness** | 30 | Reveal enemy HP, resistances, vulnerabilities | 5 Stress | ⚠️ EXPENSIVE |
| **Exploit Design Flaw** | 35 | Apply [Analyzed] (+2 Accuracy for 3 turns) | 0 | 🔴 TOO EXPENSIVE |
| **Navigational Bypass** | 30 | Party +2d trap resistance for 3 rooms | 0 | ✅ Situational |

**Analysis:**
- **Exploit Design Flaw** costs 35 Stamina, making it the most expensive non-Capstone ability
  - Compare to Anatomical Insight (25 Stamina for [Vulnerable])
  - Both grant party-wide combat advantages, but Exploit costs 40% more
- **Analyze Weakness** costs 30 Stamina + 5 Stress for information (no direct combat benefit)
  - This is very expensive for a utility ability
  - Should cost less OR provide additional combat benefit

**Recommendations:**
1. **Exploit Design Flaw**: Reduce cost to 28 Stamina (from 35) - still expensive but reasonable
2. **Analyze Weakness**: Reduce cost to 25 Stamina (from 30) OR reduce Stress cost to 3

---

### 1.3 Ability Cost Distribution Summary

**All Abilities by Stamina Cost** (25+ abilities analyzed):

| Cost Range | Count | Percentage | Assessment |
|------------|-------|------------|-----------|
| 0 (Passive/Free) | 3 | 12% | Good for utility |
| 5-15 | 6 | 24% | Low-cost, efficient |
| 16-25 | 9 | 36% | Standard cost range |
| 26-40 | 5 | 20% | High-cost, powerful |
| 41-60 | 2 | 8% | Ultimates |

**Key Insights:**
1. Most abilities (60%) cost 15-25 Stamina, which is **50-63% of base Stamina pools**
2. This means characters can use **2-3 abilities per combat** before exhaustion
3. No Stamina regeneration in v0.1 makes long combats punishing
4. High-cost abilities (40+) require full Stamina, limiting tactical flexibility

---

## Section 2: Enemy Balance Analysis

### 2.1 Enemy HP and Damage by Tier

#### Trivial Tier (Legend < 10)

| Enemy | HP | Damage | Legend Value | TTK (Warrior) | TTK (Mystic) |
|-------|----|----|--------------|--------------|--------------|
| **Corroded Sentry** | 15 | 1d6 (avg 3.5) | 5 | ~2 hits | ~3 hits |
| **Scrap-Hound** | 10 | 1d6 (avg 3.5) | 10 | ~2 hits | ~2 hits |
| **Corrupted Servitor** | 15 | 1d6 (avg 3.5) | 10 | ~2 hits | ~3 hits |

**Analysis:**
- **Corroded Sentry** grants only **5 Legend** despite having 15 HP (same as Corrupted Servitor which grants 10)
- TTK (Time to Kill) is consistent at 2-3 hits across all Trivial enemies
- These enemies die quickly and pose minimal threat (as intended for tutorial/early game)

**Recommendations:**
1. **Corroded Sentry**: Increase Legend value to 8 (from 5) to match HP investment
2. Consider grouping Trivial enemies in encounters (2-3 at once) to maintain challenge

---

#### Low Tier (Legend 10-25)

| Enemy | HP | Damage | Legend Value | TTK (Warrior) | TTK (Mystic) |
|-------|----|----|--------------|--------------|--------------|
| **Husk Enforcer** | 25 | 1d6+2 (avg 5.5) | 15 | ~3 hits | ~4 hits |
| **Arc-Welder Unit** | 30 | 2d6 (avg 7) | 20 | ~3-4 hits | ~5 hits |
| **Blight-Drone** | 25 | 1d6+1 (avg 4.5) | 25 | ~3 hits | ~4 hits |

**Analysis:**
- Low Tier enemies have consistent HP (25-30) and moderate damage (4.5-7 avg)
- **Blight-Drone** is the most rewarding (25 Legend for 25 HP) - good value
- **Arc-Welder Unit** is slightly tankier (30 HP) but only grants 20 Legend (should be 25)
- TTK of 3-5 hits creates satisfying combat pacing

**Recommendations:**
1. **Arc-Welder Unit**: Increase Legend value to 25 (from 20) to match HP increase
2. **Husk Enforcer**: Slightly underpowered for 15 Legend - consider +5 HP (25 → 30) OR increase Legend to 18

---

#### Medium Tier (Legend 30-50)

| Enemy | HP | Damage | Legend Value | TTK (Warrior) | TTK (Mystic) |
|-------|----|----|--------------|--------------|--------------|
| **Shrieker** | 35 | 1d6 (avg 3.5) + AOE | 30 | ~4 hits | ~5 hits |
| **Jötun-Reader Fragment** | 40 | 2d6 (avg 7) | 35 | ~4-5 hits | ~6 hits |
| **Forlorn Scholar** | 30 | 2d6 (avg 7) | 35 | ~3-4 hits | ~5 hits |
| **Servitor Swarm** | 50 | 2d6 (avg 7) | 30 | ~5 hits | ~7 hits |
| **War-Frame** (Mini-boss) | 50 | 2d6 (avg 7) | 50 | ~5 hits | ~7 hits |

**Analysis:**
- **Servitor Swarm** is significantly tankier (50 HP) but only grants 30 Legend (should be 40+)
- **Forlorn Scholar** grants 35 Legend for 30 HP + can be negotiated with (good design)
- **War-Frame** appropriately valued as mini-boss (50 Legend for 50 HP)
- Medium Tier enemies have appropriate TTK (4-7 hits), creating longer engagements

**Recommendations:**
1. **Servitor Swarm**: Increase Legend value to 40 (from 30) to match 50 HP
2. **Shrieker**: Add clarification that AOE psychic scream deals additional damage (currently unclear)

---

#### High Tier (Legend 50-75)

| Enemy | HP | Damage | Legend Value | TTK (Warrior) | TTK (Mystic) |
|-------|----|----|--------------|--------------|--------------|
| **Bone-Keeper** | 60 | 3d6 (avg 10.5) | 50 | ~6 hits | ~8 hits |
| **Failure Colossus** | 80 | 4d6+3 (avg 17) | 60 | ~8 hits | ~11 hits |
| **Rust-Witch** | 70 | 3d6 (avg 10.5) | 75 | ~7 hits | ~9 hits |
| **Vault Custodian** | 70 | 2d6+2 (avg 9) + Soak 6 | 75 | ~9 hits (w/ Soak) | ~12 hits |

**Analysis:**
- **Bone-Keeper** slightly undervalued (50 Legend for 60 HP + high damage) - should be 55-60 Legend
- **Failure Colossus** deals MASSIVE damage (avg 17 per hit) - can 2-shot a Mystic at M0
- **Vault Custodian** has Soak 6 (reduces incoming damage), making TTK much longer
- High Tier enemies create meaningful combat challenges (6-12 hits to kill)

**Recommendations:**
1. **Bone-Keeper**: Increase Legend value to 55 (from 50)
2. **Failure Colossus**: Reduce damage to 3d6+4 (from 4d6+3) to prevent one-shot scenarios - avg 14.5 instead of 17
3. **Vault Custodian**: Reduce Soak to 4 (from 6) to prevent excessive tankiness

---

#### Boss Tier (Legend 100-150)

| Enemy | HP | Damage | Legend Value | TTK (Warrior, M2) | Special Mechanics |
|-------|----|----|--------------|-------------------|-------------------|
| **Ruin-Warden** | 80 | 2d6 (avg 7) | 100 | ~7 hits | 2-phase fight, Boss tag |
| **Forlorn Archivist** | 80 | 3d6 (avg 10.5) | 150 | ~7 hits | 3-phase, Forlorn aura, +5 Stress/turn |
| **Aetheric Aberration** | 60 | 3d6 (avg 10.5) | 100 | ~5 hits | Forlorn aura, psychic damage |
| **Omega Sentinel** | 100 | 3d6+3 (avg 13.5) + Soak 8 | 150 | ~11 hits (w/ Soak) | 3-phase, massive armor |
| **Sentinel Prime** | 90 | 5d6 (avg 17.5) + Soak 6 | 100 | ~10 hits (w/ Soak) | Tactical AI phases, plasma rifle |

**Analysis:**
- **Ruin-Warden** is appropriately balanced for first major boss (80 HP, moderate damage)
- **Aetheric Aberration** seems UNDERPOWERED (60 HP for 100 Legend boss) - dies too quickly
- **Omega Sentinel** is extremely tanky (100 HP + Soak 8) and deals high damage - appropriate for 150 Legend
- **Sentinel Prime** deals ABSURD damage (avg 17.5) - can kill a Warrior in 5-6 hits even at Milestone 3
- Boss TTK ranges from 5-11 hits, which is good variance depending on player build

**Recommendations:**
1. **Aetheric Aberration**: Increase HP to 75 (from 60) OR increase damage to 4d6 to match 100 Legend value
2. **Sentinel Prime**: Reduce damage to 4d6 (from 5d6) to prevent unfair one-shot scenarios - avg 14 instead of 17.5
3. **Omega Sentinel**: Reduce Soak to 6 (from 8) to slightly lower TTK and prevent frustration

---

### 2.2 Enemy Legend Value Efficiency

**Legend per HP Ratio** (ideal range: 0.8-1.2):

| Enemy | HP | Legend | Ratio (Legend/HP) | Assessment |
|-------|----|----|-------------------|-----------|
| Corroded Sentry | 15 | 5 | **0.33** | 🔴 Undervalued |
| Corrupted Servitor | 15 | 10 | **0.67** | 🟡 Slightly low |
| Blight-Drone | 25 | 25 | **1.00** | ✅ Perfect |
| War-Frame | 50 | 50 | **1.00** | ✅ Perfect |
| Servitor Swarm | 50 | 30 | **0.60** | 🔴 Undervalued |
| Bone-Keeper | 60 | 50 | **0.83** | ✅ Good |
| Ruin-Warden | 80 | 100 | **1.25** | ✅ Good (boss bonus) |
| Aetheric Aberration | 60 | 100 | **1.67** | ⚠️ High (boss, but too squishy) |

**Key Insights:**
1. Many enemies fall below 0.8 ratio (undervalued for their HP)
2. Bosses should have 1.2-1.5 ratio (bonus for special mechanics)
3. **Corroded Sentry** (0.33 ratio) is severely undervalued
4. **Servitor Swarm** (0.60 ratio) is also significantly undervalued

---

## Section 3: Equipment Balance Analysis

### 3.1 Weapon Damage Progression by Quality Tier

#### Warrior Weapons (MIGHT-based)

| Weapon | Quality | Damage | Stamina Cost | Avg Damage | Bonuses | Assessment |
|--------|---------|--------|--------------|------------|---------|-----------|
| **Rusty Hatchet** | Jury-Rigged | 1d6+0 | 5 | 3.5 | -1 Accuracy | ✅ Appropriate |
| **Scavenged Axe** | Scavenged | 1d6+1 | 5 | 4.5 | None | ✅ Good progression |
| **Clan-Forged Axe** | Clan-Forged | 1d6+3 | 5 | 6.5 | +1 MIGHT | ✅ Strong upgrade |
| **Optimized War Axe** | Optimized | 2d6+0 | 6 | 7.0 | +1 Accuracy | ✅ Excellent |
| **Bent Greatsword** | Jury-Rigged | 1d6+2 | 8 | 5.5 | -1 Accuracy | ✅ Appropriate |
| **Scavenged Greatsword** | Scavenged | 1d6+4 | 8 | 7.5 | None | ✅ Good |
| **Clan-Forged Greatsword** | Clan-Forged | 1d6+6 | 8 | 9.5 | +1 MIGHT | ⚠️ Very strong |
| **Warden's Greatsword** | Myth-Forged | 2d6+4 | 10 | 11.0 | Ignores 50% armor | ✅ Epic |

**Analysis:**
- Weapon damage progression is smooth from Jury-Rigged to Myth-Forged
- **Clan-Forged Greatsword** (1d6+6) might be slightly too strong for Tier 3 - avg 9.5 damage
  - Compare to Optimized War Axe (2d6+0) which averages 7.0
  - Greatsword has 36% higher damage despite being lower tier
- Stamina cost increases are appropriate (5 → 6 → 8 → 10)

**Recommendations:**
1. **Clan-Forged Greatsword**: Reduce bonus to +5 (from +6) - avg 8.5 instead of 9.5
2. Consider standardizing "d10" weapons as "d6+4" for consistency (1d10 avg = 5.5, 1d6+4 avg = 7.5)

---

#### Scavenger Weapons (FINESSE-based)

| Weapon | Quality | Damage | Stamina Cost | Avg Damage | Special | Assessment |
|--------|---------|--------|--------------|------------|---------|-----------|
| **Makeshift Spear** | Jury-Rigged | 1d6+0 | 4 | 3.5 | -1 Accuracy | ✅ Cheap, weak |
| **Scavenged Spear** | Scavenged | 1d6+1 | 4 | 4.5 | None | ✅ Good |
| **Clan-Forged Spear** | Clan-Forged | 1d6+3 | 4 | 6.5 | +1 FINESSE | ✅ Strong |
| **Optimized Combat Lance** | Optimized | 2d6+0 | 5 | 7.0 | Reach 2 zones, +2 Accuracy | ✅ Excellent |
| **Sharpened Scrap** | Jury-Rigged | 1d6-2 | 3 | 1.5 | -1 Accuracy | ⚠️ Terrible |
| **Scavenged Dagger** | Scavenged | 1d6+0 | 3 | 3.5 | None | ✅ Fast |
| **Clan-Forged Blade** | Clan-Forged | 1d6+2 | 3 | 5.5 | +1 FINESSE | ✅ Good |
| **Assassin's Fang** | Myth-Forged | 2d6+0 | 4 | 7.0 | Crit = [Bleeding] | ✅ Strong |

**Analysis:**
- Scavenger weapons are appropriately lower-cost (3-5 Stamina vs 5-10 for Warrior)
- **Sharpened Scrap** (1d6-2) has avg 1.5 damage - almost worthless
  - This is intentional for "desperate starting weapon" but may feel too punishing
- Dagger progression is smooth and balanced
- Spear progression is excellent

**Recommendations:**
1. **Sharpened Scrap**: Increase damage to 1d6-1 (from 1d6-2) - avg 2.5 instead of 1.5
2. Consider allowing unarmed combat as fallback if players discard Sharpened Scrap

---

#### Mystic Weapons (WILL-based)

| Weapon | Quality | Damage | Stamina Cost | Special Effect | Assessment |
|--------|---------|--------|--------------|----------------|-----------|
| **Crude Staff** | Jury-Rigged | 1d6-2 | 5 | -1 Accuracy | ⚠️ Weak |
| **Scavenged Staff** | Scavenged | 1d6+0 | 5 | -1 Stamina to abilities | ✅ Good utility |
| **Clan-Forged Staff** | Clan-Forged | 1d6+1 | 5 | -2 Stamina to abilities, +1 WILL | ✅ Strong |
| **Runestone Staff** | Myth-Forged | 1d6+4 | 6 | -3 Stamina, +1 bonus die to abilities | ✅ Epic |
| **Mind-Render Scepter** | Myth-Forged | 3d6+3 | 7 | Ignores armor, Heretical -5 Stress | ✅ Powerful |
| **Cracked Crystal** | Jury-Rigged | 0d6+0 | 0 | +1 bonus die to abilities | ⚠️ No melee |
| **Scavenged Focus** | Scavenged | 0d6+0 | 0 | +2 bonus dice to abilities | ✅ Strong utility |
| **Clan-Forged Focus** | Clan-Forged | 0d6+0 | 0 | +3 bonus dice, +1 WILL | ✅ Excellent |
| **Aetheric Amplifier** | Myth-Forged | 0d6+0 | 0 | +4 bonus dice, +5 Stamina on kill | ✅ Epic |

**Analysis:**
- Mystic weapons have dual progression paths: Staff (melee) vs Focus (pure caster)
- **Crude Staff** (1d6-2, avg 1.5 damage) is extremely weak for 5 Stamina cost
- Focus items grant 0 melee damage but massive bonus dice to abilities
- **Mind-Render Scepter** is EXTREMELY powerful (avg 13.5 damage, ignores armor, reduces Heretical costs)
- Stamina reduction effects on Staves create interesting resource management

**Recommendations:**
1. **Crude Staff**: Increase damage to 1d6-1 (from 1d6-2) OR reduce Stamina cost to 4
2. **Mind-Render Scepter**: Consider reducing damage to 3d6+2 (from 3d6+3) to prevent dominance
3. **Cracked Crystal**: Add note "Cannot be used for basic attacks" to clarify 0 damage

---

### 3.2 Armor Progression by Quality Tier

| Armor | Quality | HP Bonus | Defense Bonus | Special | FINESSE Penalty | Assessment |
|-------|---------|----------|---------------|---------|----------------|-----------|
| **Tattered Leathers** | Jury-Rigged | +2 | +1 | None | None | ✅ Weak start |
| **Scavenged Leathers** | Scavenged | +5 | +2 | +1 FINESSE | None | ✅ Good |
| **Clan-Forged Leathers** | Clan-Forged | +10 | +3 | +1 FINESSE, +1 Evasion | None | ✅ Strong |
| **Shadow Weave** | Myth-Forged | +15 | +4 | +2 FINESSE, +2 Evasion, [Hasted] < 50% HP | None | ✅ Epic |
| **Scrap Plating** | Jury-Rigged | +5 | +2 | None | -1 FINESSE | ✅ Heavy penalty |
| **Scavenged Chainmail** | Scavenged | +10 | +3 | None | None | ✅ Balanced |
| **Clan-Forged Plate** | Clan-Forged | +15 | +4 | +1 STURDINESS | None | ✅ Strong |
| **Warden's Aegis** | Myth-Forged | +25 | +5 | +2 STURDINESS, Immune [Bleeding] | None | ✅ Excellent |
| **Bent Plate** | Jury-Rigged | +8 | +2 | None | -2 FINESSE | ⚠️ Very heavy |
| **Scavenged Heavy Plate** | Scavenged | +15 | +4 | +1 STURDINESS | -1 FINESSE | ✅ Tanky |
| **Clan-Forged Full Plate** | Clan-Forged | +20 | +5 | +2 STURDINESS | None | ⚠️ No penalty? |
| **Juggernaut Frame** | Myth-Forged | +40 | +6 | +3 STURDINESS, Regen 5 HP/turn | -2 FINESSE | ✅ Epic tank |

**Analysis:**
- Light Armor (Leathers): Smooth progression from +2/+1 to +15/+4, grants FINESSE/Evasion
- Medium Armor (Chainmail/Plate): Good balance of HP and Defense, minimal penalties
- Heavy Armor (Plate): High HP/Defense but with FINESSE penalties
- **Clan-Forged Full Plate** (+20 HP, +5 Defense, +2 STURDINESS, **no FINESSE penalty**) seems too strong
  - Compare to Scavenged Heavy Plate (+15 HP, +4 Defense, +1 STURDINESS, -1 FINESSE)
  - Clan-Forged should have -1 FINESSE penalty for consistency

**Recommendations:**
1. **Clan-Forged Full Plate**: Add -1 FINESSE penalty (currently has none)
2. **Bent Plate**: Reduce FINESSE penalty to -1 (from -2) to match other Jury-Rigged items
3. Consider standardizing quality tier progressions:
   - Jury-Rigged: -1 or -2 penalty
   - Scavenged: -1 or 0 penalty
   - Clan-Forged: 0 penalty + attribute bonus
   - Optimized/Myth-Forged: 0 penalty + multiple bonuses

---

## Section 4: Resource Economy Analysis

### 4.1 Stamina Economy by Class

#### Warrior (30 Base Stamina)

**Stamina per Combat** (Milestone 0):
- Base: 30 Stamina
- Typical ability costs: 15-25 Stamina
- **Result**: 1-2 abilities per combat

**Example Combat Sequence:**
1. Turn 1: Crushing Blow (15 Stamina) → 15 remaining
2. Turn 2: Basic Attack (free) → 15 remaining
3. Turn 3: Intimidating Presence (15 Stamina) → 0 remaining
4. Turn 4+: Basic Attacks only (out of Stamina)

**Analysis:**
- Warriors are highly Stamina-constrained at M0
- Can only use 2 abilities in a 4+ turn combat before exhaustion
- No regeneration means Warriors must rely on basic attacks
- By Milestone 3 (45 Stamina), can use 2-3 abilities comfortably

**Assessment:** ✅ Balanced for Warrior archetype (low Stamina, high HP)

---

#### Scavenger/Adept (40 Base Stamina)

**Stamina per Combat** (Milestone 0):
- Base: 40 Stamina
- Typical ability costs: 10-20 Stamina
- **Result**: 2-3 abilities per combat

**Example Combat Sequence:**
1. Turn 1: Ability (15 Stamina) → 25 remaining
2. Turn 2: Ability (15 Stamina) → 10 remaining
3. Turn 3: Ability (10 Stamina) → 0 remaining
4. Turn 4+: Basic actions only

**Assessment:** ✅ Balanced for balanced archetype

---

#### Mystic (50 Base Stamina)

**Stamina per Combat** (Milestone 0):
- Base: 50 Stamina
- Typical ability costs: 8-30 Stamina
- **Result**: 3-5 abilities per combat

**Example Combat Sequence (PROBLEMATIC):**
1. Turn 1: Aetheric Bolt (8 Stamina) → 42 remaining
2. Turn 2: Disrupt (12 Stamina) → 30 remaining
3. Turn 3: Aetheric Bolt (8 Stamina) → 22 remaining
4. Turn 4: Analyze (20 Stamina) → 2 remaining
5. Turn 5+: Out of Stamina, but has 30 HP and likely took 20+ damage

**Analysis:**
- Mystics can cast more abilities (3-5) but die faster (30 HP base)
- In practice, Mystics exhaust Stamina around the same time they reach critical HP
- However, **high-cost Mystic abilities (Corruption Nova: 50 Stamina) are UNUSABLE** at M0
- Even with equipment (-2 to -3 Stamina from Staves), 50 Stamina abilities require full pool

**Assessment:** ⚠️ Mystic Stamina economy is RESTRICTIVE for high-cost abilities

---

### 4.2 Stamina Regeneration (MISSING FEATURE)

**Documentation states:**
> "Stamina does **not** regenerate during combat in v0.1 (future feature)"
> "Future (v0.5+): Per-turn regeneration (+3-5 Stamina per turn)"

**Current State:** NO regeneration implemented

**Impact:**
- Long combats (5+ turns) leave players with 0 Stamina
- Only recovery options: consumables (Stamina Tonic: +2d6) or rest
- This creates frustrating gameplay where players spam basic attacks after 3-4 turns

**Recommendations:**
1. Implement per-turn Stamina regeneration: +3 Stamina/turn for all classes
2. Alternatively, grant Stamina on kill: +5 Stamina per enemy defeated
3. Add "Second Wind" passive: Regenerate Stamina equal to 10% of MaxStamina per turn

---

### 4.3 HP Economy by Class

#### Time-to-Death (TTD) Analysis

**Warrior (50 HP base, M0):**
- vs Trivial enemy (3.5 avg damage): 14 hits to die
- vs Low Tier enemy (5.5 avg damage): 9 hits to die
- vs Medium Tier enemy (7 avg damage): 7 hits to die
- vs High Tier enemy (10.5 avg damage): 5 hits to die
- vs Boss enemy (13.5 avg damage): 4 hits to die

**Mystic (30 HP base, M0):**
- vs Trivial enemy (3.5 avg damage): 9 hits to die
- vs Low Tier enemy (5.5 avg damage): 5 hits to die
- vs Medium Tier enemy (7 avg damage): 4 hits to die
- vs High Tier enemy (10.5 avg damage): **3 hits to die**
- vs Boss enemy (13.5 avg damage): **2 hits to die**

**Analysis:**
- **Mystics die in 2-3 hits from High Tier enemies and bosses** - extremely fragile
- Warriors survive 4-5 hits from same enemies - 67% more survivability
- This is intentional (glass cannon vs tank design) BUT:
  - 2-shot deaths feel frustrating, not strategic
  - Mystics have limited defensive options (Aetheric Shield: 10 Stamina for 1 turn)

**Recommendations:**
1. Increase Mystic base HP to 35 (from 30) to prevent 2-shot scenarios
2. Add Mystic-specific defensive passive: "Aetheric Barrier: Negate first hit per combat"
3. Reduce High Tier enemy damage by ~10% (see Enemy Balance section)

---

### 4.4 Healing Economy

**Available Healing Sources:**

| Source | HP Restored | Availability | Cost |
|--------|-------------|--------------|------|
| Healing Salve | 2d6 (avg 7) | Consumable (limited) | Free (found) |
| Medicinal Tonic | 3d6 (avg 10.5) | Consumable (rare) | Free (found) |
| Survivalist (Scavenger) | 2d6 (avg 7) | 20 Stamina, once/combat | Stamina |
| Mend Wound (Bone-Setter) | Variable | 5 Stamina + consumable | Stamina + item |
| Second Wind (Warrior) | 2d10 (avg 11) | 20 Stamina, once/combat | Stamina |
| Sanctuary Rest | Full HP | Once per Sanctuary | Time/risk |
| Milestone | Full HP | Per Milestone | Progression |

**Healing vs Damage Ratio:**
- Average enemy damage: 7-10 HP per hit
- Average healing: 7-11 HP per source
- **Result**: Healing barely keeps pace with incoming damage

**Analysis:**
- In-combat healing is LIMITED (1-2 consumables per encounter)
- Stamina-based healing competes with offensive abilities
- No passive HP regeneration (only Juggernaut Frame armor: +5 HP/turn)
- Players must rely on attrition management and Sanctuary rests

**Assessment:** ✅ Healing economy is appropriately restrictive (encourages tactical play)

**Recommendations:**
- Consider adding Milestone healing to +5 HP/turn for 3 turns (temporary buff after Milestone)
- Add equipment enchantment: "Vampiric: Heal 10% of damage dealt"

---

## Section 5: Progression Curve Analysis

### 5.1 Legend (XP) Curve

**Milestone Requirements:**
- M0 → M1: 100 Legend (cumulative: 100)
- M1 → M2: 150 Legend (cumulative: 250)
- M2 → M3: 200 Legend (cumulative: 450)

**Enemy Legend Values (Average by Tier):**
- Trivial: 7.5 avg
- Low: 18 avg
- Medium: 35 avg
- High: 60 avg
- Boss: 125 avg

**Encounters to Milestone:**

| Milestone | Legend Needed | Trivial Enemies | Low Enemies | Medium Enemies | High Enemies |
|-----------|---------------|----------------|-------------|---------------|--------------|
| M0 → M1 | 100 | 13 enemies | 6 enemies | 3 enemies | 2 enemies |
| M1 → M2 | 150 | 20 enemies | 8 enemies | 4 enemies | 3 enemies |
| M2 → M3 | 200 | 27 enemies | 11 enemies | 6 enemies | 3 enemies |

**Typical v0.4 Playthrough** (East Wing path, from Balance Review v0.4):
- Combat encounters: 6 + 1 boss
- Total Enemy HP: 310 + 80 (boss) = 390 HP
- Total Legend: 300 + 100 (boss) = 400 Legend
- **Result**: Reaches Milestone 3 by end of run (450 Legend total)

**Analysis:**
- XP curve is FRONT-LOADED - players reach M1 quickly (100 Legend = 3-4 encounters)
- M2 → M3 feels grindy (requires 6 Medium Tier encounters or 11 Low Tier)
- Boss fights grant 100-150 Legend (massive spike, 1-2 Milestones in single fight)
- Current curve is tuned for SHORT runs (15-20 minutes, documented in Legend-Leveling.md)

**Assessment:** ⚠️ XP curve is appropriate for v0.1 scope BUT may need adjustment for longer campaigns

**Recommendations:**
1. For v0.18: No changes (curve is intentional for short runs)
2. For future versions: Consider exponential curve for longer campaigns
   - M0 → M1: 100 Legend (same)
   - M1 → M2: 200 Legend (increased from 150)
   - M2 → M3: 350 Legend (increased from 200)

---

### 5.2 Progression Points (PP) Economy

**PP Sources:**
- Starting: 2 PP
- Per Milestone: +1 PP
- **Total by M3**: 2 + 3 = 5 PP

**PP Costs:**
- Attribute increase: 1 PP per +1 attribute (cap 6)
- Ability rank advancement: 2-3 PP per rank (not fully implemented)
- Specialization unlock: 10 PP (one-time)

**Example Builds:**

**Build 1: Balanced Warrior (5 PP)**
- Specialization: None (save 10 PP)
- Attributes: +3 MIGHT, +2 STURDINESS (5 PP spent)
- Result: Well-rounded fighter, no specialization

**Build 2: Specialized Adept (5 PP - IMPOSSIBLE)**
- Specialization: Bone-Setter (10 PP - can't afford)
- Attributes: N/A
- Result: CANNOT UNLOCK SPECIALIZATION with only 5 PP by M3

**Analysis:**
- **Specializations cost 10 PP but players only earn 5 PP by M3** - CRITICAL ISSUE
- This means specializations are LOCKED until late game (M6+) or players must skip attribute increases
- Current PP economy forces a choice: attributes OR specialization (not both)

**Assessment:** 🔴 CRITICAL ISSUE - Specialization cost (10 PP) is unreachable in v0.1 scope (M0-M3)

**Recommendations:**
1. **URGENT**: Reduce Specialization unlock cost to 3 PP (from 10) to make achievable by M2-M3
2. Alternatively, increase PP gain to +2 PP per Milestone (total 8 PP by M3)
3. Alternatively, grant bonus PP at character creation (3 PP instead of 2 PP)

---

## Section 6: Cross-System Balance Issues

### 6.1 Stamina Costs vs Stamina Pools

**Problem**: Many high-cost abilities are UNUSABLE at early game due to base Stamina pools

| Ability | Cost | Required Stamina Pool | Class Accessibility (M0) |
|---------|------|----------------------|--------------------------|
| Rally Cry | 20 | 20 (67% of Warrior) | ⚠️ Warrior can barely use |
| Whirlwind Strike | 25 | 25 (83% of Warrior) | 🔴 Warrior nearly exhausted |
| Analyze Weakness | 30 | 30 (100% of Warrior) | 🔴 Warrior cannot use twice |
| Exploit Design Flaw | 35 | 35 (117% of Warrior) | 🔴 Warrior CANNOT use at M0 |
| Titan's Strength | 40 | 40 (133% of Warrior) | 🔴 Warrior CANNOT use at M0 |
| Miracle Worker | 60 | 60 (200% of Warrior) | 🔴 NO CLASS can use at M0 |
| Corruption Nova | 50 | 50 (100% of Mystic) | ⚠️ Mystic fully exhausted |

**Analysis:**
- Abilities costing 30+ Stamina are effectively LOCKED at M0
- Warriors (30 Stamina) cannot use 40+ Stamina abilities even at M1 (35 Stamina)
- **Miracle Worker** (60 Stamina) requires M3 Adept (55 Stamina) + consumable to use ONCE

**Recommendations:**
1. Reduce all abilities costing 30+ Stamina by 20-30%
2. Add Stamina scaling: Abilities cost less at higher Milestones (e.g., -5 Stamina at M2+)
3. Implement Stamina regeneration (+3/turn) to enable high-cost abilities

---

### 6.2 Damage Scaling vs HP Scaling

**Problem**: Enemy damage scales faster than player HP, creating one-shot scenarios

**Player HP Progression (Mystic):**
- M0: 30 HP (base)
- M1: 40 HP (+10 from Milestone)
- M2: 50 HP (+20 from Milestones)
- M3: 60 HP (+30 from Milestones)

**Enemy Damage Progression:**
- Trivial: 3.5 avg
- Low: 5.5 avg (+57% from Trivial)
- Medium: 7 avg (+27% from Low)
- High: 10.5 avg (+50% from Medium)
- Boss: 13.5 avg (+29% from High)

**Damage Increase vs HP Increase:**
- Enemy damage increases by +286% (Trivial to Boss: 3.5 → 13.5)
- Player HP increases by +100% (M0 to M3: 30 → 60)
- **Result**: Late-game enemies deal proportionally MORE damage relative to player HP

**Analysis:**
- At M0, Mystic survives 9 hits from Trivial enemy (comfortable)
- At M3, Mystic survives 4 hits from Boss enemy (dangerous)
- Damage scaling outpaces HP scaling, making late game more punishing

**Recommendations:**
1. Increase Milestone HP bonus to +12 HP (from +10 HP) to match damage scaling
2. Reduce High Tier and Boss damage by ~10% across the board
3. Add equipment with % damage reduction (e.g., "Reduce incoming damage by 10%")

---

### 6.3 Legend Value vs Difficulty

**Problem**: Some enemies grant Legend values that don't match their actual difficulty

**Undervalued Enemies** (Legend too low for difficulty):
- **Corroded Sentry**: 15 HP, 5 Legend (ratio 0.33) - should be 10-12 Legend
- **Servitor Swarm**: 50 HP, 30 Legend (ratio 0.60) - should be 40 Legend
- **Husk Enforcer**: 25 HP, 1d6+2 damage, 15 Legend - feels harder than 15 Legend

**Overvalued Enemies** (Legend too high for difficulty):
- **Aetheric Aberration**: 60 HP, 100 Legend (boss) - dies too quickly for boss status

**Recommendations:**
1. Adjust Legend values to match HP × 0.8-1.2 ratio for non-bosses
2. Adjust Legend values to match HP × 1.2-1.5 ratio for bosses
3. Add "effective HP" calculation (HP × difficulty modifiers) for Legend calculation

---

## Section 7: Priority Balance Adjustment List

### 7.1 Critical Issues (Must Fix for v0.18)

1. **🔴 Specialization Cost Too High (10 PP)**
   - **Problem**: Players only earn 5 PP by M3, cannot unlock specializations
   - **Fix**: Reduce cost to 3 PP OR increase PP gain to +2/Milestone
   - **Impact**: Enables specialization gameplay in v0.1 scope

2. **🔴 Miracle Worker Unusable (60 Stamina)**
   - **Problem**: Capstone ability costs 60 Stamina (Adept has 40-55 Stamina max)
   - **Fix**: Reduce cost to 40 Stamina
   - **Impact**: Makes Capstone actually usable

3. **🔴 Rally Cry Underpowered (20 Stamina for 1d8 heal)**
   - **Problem**: Costs 20 Stamina, only heals ~4.5 HP (worse than 5 Stamina consumable)
   - **Fix**: Increase heal to 2d6 HP OR reduce cost to 12 Stamina
   - **Impact**: Makes Warrior support viable

4. **🔴 Whirlwind Strike Ambiguous Damage**
   - **Problem**: Description says "1d8 per target" but code has DamageDice = 1
   - **Fix**: Clarify in code and description, ensure 1d8 per target
   - **Impact**: Fixes confusion and balance

5. **🔴 Corroded Sentry Undervalued (5 Legend)**
   - **Problem**: Has 15 HP (same as Corrupted Servitor) but only grants 5 Legend
   - **Fix**: Increase to 10 Legend
   - **Impact**: Rewards players fairly for defeating enemy

---

### 7.2 High Priority Issues (Should Fix for v0.18)

6. **🟡 Exploit Design Flaw Too Expensive (35 Stamina)**
   - **Problem**: Most expensive non-Capstone ability, Warriors can't use at M0
   - **Fix**: Reduce to 28 Stamina
   - **Impact**: Makes Jötun-Reader abilities accessible

7. **🟡 Analyze Weakness Too Expensive (30 Stamina + 5 Stress)**
   - **Problem**: Costs entire Warrior Stamina pool for information only
   - **Fix**: Reduce to 25 Stamina OR 3 Stress
   - **Impact**: Makes utility ability worth using

8. **🟡 Servitor Swarm Undervalued (30 Legend for 50 HP)**
   - **Problem**: Tankiest Medium Tier enemy but grants less Legend than weaker enemies
   - **Fix**: Increase to 40 Legend
   - **Impact**: Rewards players for high HP enemy

9. **🟡 Failure Colossus One-Shot Risk (4d6+3 = 17 avg damage)**
   - **Problem**: Can kill Mystic in 2 hits, Scavenger in 3 hits
   - **Fix**: Reduce to 3d6+4 (avg 14.5 damage)
   - **Impact**: Prevents unfair one-shots while keeping boss threatening

10. **🟡 Sentinel Prime Excessive Damage (5d6 = 17.5 avg)**
    - **Problem**: Highest damage in game, kills Warriors in 5-6 hits
    - **Fix**: Reduce to 4d6 (avg 14 damage)
    - **Impact**: Keeps boss challenging without frustration

11. **🟡 Anatomical Insight Expensive (25 Stamina)**
    - **Problem**: Costs 25 Stamina for [Vulnerable] debuff (compare: Crushing Blow 15 Stamina for damage + prone)
    - **Fix**: Reduce to 20 Stamina OR extend duration to 4 turns
    - **Impact**: Makes Bone-Setter support role more viable

12. **🟡 Cognitive Realignment Expensive (30 Stamina)**
    - **Problem**: Costs 30 Stamina to remove mental debuffs + heal 2d6 Stress
    - **Fix**: Reduce to 25 Stamina
    - **Impact**: Makes mental health management accessible

---

### 7.3 Medium Priority Issues (Nice to Fix for v0.18)

13. **🟢 Aetheric Aberration Squishy (60 HP for 100 Legend boss)**
    - **Fix**: Increase HP to 75
    - **Impact**: Makes boss fight last longer

14. **🟢 Omega Sentinel Too Tanky (Soak 8)**
    - **Fix**: Reduce Soak to 6
    - **Impact**: Reduces TTK to prevent frustration

15. **🟢 Vault Custodian Too Tanky (Soak 6)**
    - **Fix**: Reduce Soak to 4
    - **Impact**: Balances mini-boss fight

16. **🟢 Clan-Forged Greatsword Too Strong (1d6+6)**
    - **Fix**: Reduce bonus to +5
    - **Impact**: Balances weapon progression

17. **🟢 Clan-Forged Full Plate Missing Penalty**
    - **Fix**: Add -1 FINESSE penalty
    - **Impact**: Consistent with heavy armor design

18. **🟢 Sharpened Scrap Too Weak (1d6-2 = 1.5 avg)**
    - **Fix**: Increase to 1d6-1 (2.5 avg)
    - **Impact**: Reduces frustration for starting Scavenger

19. **🟢 Crude Staff Too Weak (1d6-2 = 1.5 avg, 5 Stamina)**
    - **Fix**: Increase to 1d6-1 OR reduce cost to 4 Stamina
    - **Impact**: Improves starting Mystic experience

20. **🟢 Bone-Keeper Undervalued (50 Legend for 60 HP)**
    - **Fix**: Increase to 55 Legend
    - **Impact**: Fair reward for high-damage enemy

---

## Section 8: Recommended Implementation Order

### Phase 2: Ability Balance (Start Here)

**Week 1: Critical Ability Fixes**
1. Rally Cry: Change heal from 1d8 to 2d6
2. Miracle Worker: Reduce cost from 60 to 40 Stamina
3. Whirlwind Strike: Clarify damage as 1d8 per target, update code
4. Exploit Design Flaw: Reduce cost from 35 to 28 Stamina
5. Analyze Weakness: Reduce cost from 30 to 25 Stamina

**Week 2: Secondary Ability Fixes**
6. Anatomical Insight: Reduce cost from 25 to 20 Stamina
7. Cognitive Realignment: Reduce cost from 30 to 25 Stamina

### Phase 3: Enemy Balance

**Week 3: Enemy Legend Values**
8. Corroded Sentry: Increase from 5 to 10 Legend
9. Servitor Swarm: Increase from 30 to 40 Legend
10. Bone-Keeper: Increase from 50 to 55 Legend

**Week 4: Enemy Damage Tuning**
11. Failure Colossus: Reduce damage from 4d6+3 to 3d6+4
12. Sentinel Prime: Reduce damage from 5d6 to 4d6

**Week 5: Enemy Defense Tuning**
13. Aetheric Aberration: Increase HP from 60 to 75
14. Omega Sentinel: Reduce Soak from 8 to 6
15. Vault Custodian: Reduce Soak from 6 to 4

### Phase 4: Equipment Balance

**Week 6: Equipment Adjustments**
16. Clan-Forged Greatsword: Reduce bonus from +6 to +5
17. Clan-Forged Full Plate: Add -1 FINESSE penalty
18. Sharpened Scrap: Increase damage from 1d6-2 to 1d6-1
19. Crude Staff: Increase damage from 1d6-2 to 1d6-1

### Phase 5: Progression System

**Week 7: PP Economy Fix**
20. Specialization unlock cost: Reduce from 10 PP to 3 PP
    - **OR** Increase PP gain from +1 to +2 per Milestone
    - **OR** Increase starting PP from 2 to 3

---

## Section 9: Testing Plan

### 9.1 Ability Testing

**For each adjusted ability:**
1. Verify new cost/damage values in code
2. Test in combat simulation (early, mid, late game)
3. Verify cost-to-impact ratio is 0.4-0.6 for damage abilities
4. Verify Stamina pool coverage (can use 2-3 abilities per combat)

### 9.2 Enemy Testing

**For each adjusted enemy:**
1. Verify new HP/damage/Legend values in code
2. Calculate new TTK for all classes at M0, M2
3. Verify Legend/HP ratio is 0.8-1.2 (non-boss) or 1.2-1.5 (boss)
4. Playtest full encounter to verify "feels" balanced

### 9.3 Full Playthrough Testing

**After all changes:**
1. Full playthrough with Warrior (East Wing)
2. Full playthrough with Mystic (West Wing)
3. Full playthrough with Adept + Bone-Setter specialization
4. Full playthrough with Adept + Jötun-Reader specialization
5. Verify all Milestones reachable, all abilities usable, no frustrating moments

---

## Section 10: Conclusion

### Key Findings

1. **Abilities**: 7 critical issues, 6 high priority issues identified
2. **Enemies**: 10 enemies need Legend value or damage tuning
3. **Equipment**: 4 items need minor adjustments
4. **Progression**: 1 CRITICAL issue (Specialization cost too high)

### Estimated Time

- **Ability Balance**: 8-10 hours
- **Enemy Balance**: 6-8 hours
- **Equipment Balance**: 2-3 hours
- **Progression Balance**: 2 hours
- **Testing**: 8-10 hours
- **Total**: 26-33 hours (within v0.18 scope of 20-30 hours)

### Next Steps

1. Review this document with team
2. Prioritize critical issues (items 1-5)
3. Implement fixes in priority order
4. Playtest after each phase
5. Document changes in CHANGELOG
6. Update v0.17 statistical registry with new values

---

**End of Phase 1 Analysis**
