# Enemy Design Worksheet

Parent item: Template (Template%202ba55eb312da80a8b0f3fbfdcd27220c.md)

**Purpose**: Quick-start checklist for creating balanced enemies that conform to SPEC-COMBAT-012 design guidelines.

**When to Use**: Before implementing any new enemy in `EnemyFactory.cs` or modifying existing enemies.

**Reference**: See `docs/00-specifications/combat/enemy-design-spec.md` for complete design system documentation.

---

## Step 1: Core Identity

**Enemy Name**: _______________________________________________

**Threat Tier** (select one):

- [ ]  **Low** (10-15 HP, 5-10 attr, 1d6 to 1d6+2, Legend 10-20)
- [ ]  **Medium** (25-50 HP, 8-16 attr, 1d6+1 to 2d6, Legend 15-50)
- [ ]  **High** (60-70 HP, 12-17 attr, 2d6+2 to 3d6, Legend 55-75)
- [ ]  **Lethal** (80-90 HP, 13-20 attr, 3d6+4 to 4d6, Legend 60-100)
- [ ]  **Boss** (75-100 HP, 13-20 attr, 2d6 to 3d6+3, Legend 100-150)

**Archetype** (select one):

- [ ]  **Tank** (High HP/STURDINESS, Soak 4-6, low-medium damage, defensive AI)
- [ ]  **DPS** (Balanced stats, medium HP, consistent damage, aggressive AI 70-80%)
- [ ]  **Glass Cannon** (Low HP 10-20, high FINESSE/MIGHT, high damage, aggressive AI 80-90%)
- [ ]  **Support** (Medium HP, high WITS 4-5, buff/debuff abilities, tactical AI 30-40% attack)
- [ ]  **Swarm** (Low-medium stats, numbers-based, collective HP, aggressive AI 90%)
- [ ]  **Caster** (Medium HP, high WILL 4-7, psychic damage, tactical AI 60% magic, 20% debuff)
- [ ]  **Mini-Boss** (High HP 50-70, phase mechanics, Soak 4-6, IsBoss=false)
- [ ]  **Boss** (Very high HP 75-100, 2-3 phases, IsBoss=true, ultimate abilities)

**Thematic Role** (1-2 sentences):

---

---

**Design Intent** (what challenge does this enemy create?):

---

---

## Step 2: Stat Budget Allocation

**Reference Budgets** (from selected threat tier above):

| Tier | HP Range | Attribute Budget | Damage Range | Legend Value |
| --- | --- | --- | --- | --- |
| Selected: _______ | _______ | _______ | _______ | _______ |

**Allocated Stats**:

**HP**: _________

- [ ]  Within budget range? ✓ / ✗
- [ ]  Matches archetype pattern (Tank high, Glass Cannon low)? ✓ / ✗

**Attributes** (MIGHT / FINESSE / WITS / WILL / STURDINESS):

- MIGHT: _____ (melee damage, physical resistance)
- FINESSE: _____ (ranged damage, initiative, evasion)
- WITS: _____ (detection, tactical abilities)
- WILL: _____ (magic damage/resist, Stress resistance)
- STURDINESS: _____ (HP pool, Soak bonus)

**Total Attribute Points**: _____

- [ ]  Within budget range? ✓ / ✗
- [ ]  Matches archetype pattern (Caster: high WILL, Tank: high STURDINESS)? ✓ / ✗

**Damage**:

- BaseDamageDice: _____ d6 (number of dice)
- DamageBonus: _____ (flat bonus)
- **Total Damage**: ***d6+*** (avg damage: _______)
- [ ]  Within tier damage range? ✓ / ✗
- [ ]  Damage variance acceptable (max/min < 4:1 for Lethal/Boss)? ✓ / ✗
- [ ]  No one-shot potential (< 80% of Legend-appropriate player HP)? ✓ / ✗

**Legend Value**: _____

- Legend/HP Ratio: _____ (divide Legend by HP)
- [ ]  Within 0.5-1.0 ratio range? ✓ / ✗
- [ ]  +10-20 bonus for special mechanics (self-heal, buffs)? Applied: Y / N
- [ ]  +20-50 bonus for boss mechanics? Applied: Y / N

---

## Step 3: Special Mechanics Flags

**IsForlorn** (Trauma Aura):

- [ ]  **Yes** → Forlorn enemies inflict passive Psychic Stress/Corruption
- [ ]  **No** → Standard enemy, no trauma aura

If **Yes**, specify:

- Stress/turn proximity aura: +_____ Stress (within _____ tiles)
- Corruption on encounter: +_____ Corruption
- Total trauma cost estimate: ~_____ Stress / ~_____ Corruption
- [ ]  Trauma cost matches tier (Medium 10-25, High 15-25, Lethal 25-40, Boss 30-80)? ✓ / ✗

**IsBoss** (Multi-Phase Combat):

- [ ]  **Yes** → Enables phase-based AI, boss UI, guaranteed Tier 4 loot
- [ ]  **No** → Standard enemy or mini-boss (Phase AI without IsBoss flag)

If **Yes**, specify:

- Number of phases: _____ (2-3 typical)
- Phase 1 HP threshold: > _____% HP
- Phase 2 HP threshold: ≤ _____% HP (typically 50%)
- Phase 3 HP threshold: ≤ _____% HP (typically 25%, optional)

**IsChampion** (Elite Variant):

- [ ]  **Reserved for future use** (not implemented in v0.24)
- [ ]  Not applicable

**Soak** (Flat Damage Reduction):

- Soak value: _____ (0-6 range)
- [ ]  Within cap (max 4 for non-boss, max 6 for boss)? ✓ / ✗
- Effective HP multiplier: ~×_____ (use formula: 1 ÷ (1 - (Soak ÷ 10)))

**v0.18 Balance Warning**:

- [ ]  If Soak > 4 for non-boss, justify why (prevents bullet sponge): _______________
- [ ]  If Soak > 6 for boss, reduce to 6 (Omega Sentinel lesson learned)

**Other Special Abilities**:

- [ ]  Self-healing (e.g., Guardian Protocol, Last Stand)
- [ ]  Buff allies (e.g., OverchargeAlly, EmergencyRepairAlly)
- [ ]  Poison DoT (e.g., Sludge-Crawler)
- [ ]  Summons/reinforcements (e.g., boss mechanics)
- [ ]  Other: _____________________________________________________________

---

## Step 4: AI Behavior Pattern

**Selected Archetype Pattern** (from Step 1): _______________________

**Implementation Reference**: After designing your AI probabilities below, use `/docs/templates/ai-behavior-pattern-template.md` for code templates that implement these patterns in `EnemyAI.cs`.

**AI Probability Distribution** (must total 100%):

**For Aggressive Pattern** (Glass Cannon, DPS, Swarm):

- _____% BasicAttack / offensive actions (target: 70-90%)
- _____% Defensive / utility actions (target: 10-30%)

**For Defensive Pattern** (Tank):

- _____% Attack actions (target: 40-50%)
- _____% DefensiveStance / defense buff (target: 30-40%)
- _____% Self-heal / utility (target: 20-30%)

**For Tactical Pattern** (Support, Caster):

- _____% Attack / magic attacks (target: 30-50%)
- _____% Buff allies / debuff player (target: 30-40%)
- _____% Heal allies / summons (target: 20-30%)

**For Phase-Based Pattern** (Boss, Mini-Boss):

**Phase 1** (HP > _____% threshold):

- _____% Primary attack (target: 50-60%)
- _____% Secondary attack
- _____% Utility / defensive

**Phase 2** (HP ≤ _____% threshold):

- _____% Special abilities / AoE (target: 30-50%, increased from Phase 1)
- _____% Primary attack
- _____% Utility / summons

**Phase 3** (HP ≤ _____% threshold, if applicable):

- _____% Desperation ultimate (target: 50%+)
- _____% AoE / summons
- _____% Defensive protocols

**Special AI Rules** (optional):

- [ ]  Low HP threshold triggers specific action (e.g., < 30% HP → self-heal priority)
- [ ]  Flee condition: HP < _____% (typically Glass Cannon archetype)
- [ ]  Ally-dependent logic (e.g., Support prioritizes buffing allies)
- [ ]  Other: _____________________________________________________________

---

## Step 5: Balance Validation

**Time-to-Kill (TTK) Targets**:

| Threat Tier | Solo Player TTK Target | Your Estimated TTK | Within Range? |
| --- | --- | --- | --- |
| Selected: _______ | _______ turns | _______ turns | ✓ / ✗ |

**TTK Calculation** (simplified):

```
Player avg damage: ~6-7 (Legend 1), ~10-12 (Legend 3), ~15-18 (Legend 5)
Player hit rate: ~65%
Effective DPS = (Your_Player_Damage × 0.65)

TTK = (Enemy_HP + (Enemy_HP × (Soak ÷ 10))) ÷ Effective_DPS

```

Manual calculation:

- Enemy HP: _____
- Effective HP (with Soak): _____ HP × (1 + (_____ Soak ÷ 10)) = _____ effective HP
- Player DPS: _____ damage × 0.65 hit rate = _____ DPS
- **TTK**: _____ effective HP ÷ _____ DPS = **_____ turns**

**Damage Output Validation**:

| Threat Tier | Enemy Avg Damage Target | Your Enemy Avg Damage | % of Player HP (30 HP @ Legend 1) | Within Range? |
| --- | --- | --- | --- | --- |
| Selected: _______ | _______ damage | _______ damage | _____% | ✓ / ✗ |
- [ ]  Enemy damage does NOT one-shot player (< 80% HP in single hit)? ✓ / ✗
- [ ]  Enemy threatens 2-shot only if Lethal/Boss tier? ✓ / ✗

**Loot Quality Mapping**:

| Threat Tier | Primary Loot Quality | Drop Rate |
| --- | --- | --- |
| Selected: _______ | Tier _____ | _____% |
- [ ]  Loot quality matches threat tier (Low → T0-T1, Boss → T4 at 70%)? ✓ / ✗

---

## Step 6: Implementation Checklist

**Code Files to Modify**:

**1. Enemy.cs** (`RuneAndRust.Core/Enemy.cs`):

- [ ]  Add `EnemyType.[YourEnemyName]` to enum (line ~5-38)
- [ ]  Enum placement: Place after similar tier/theme enemies

**2. EnemyFactory.cs** (`RuneAndRust.Engine/EnemyFactory.cs`):

- [ ]  Add switch case in `CreateEnemy()` method (line ~6-43)
- [ ]  Implement `Create[YourEnemyName]()` method:
    - [ ]  Set `MaxHP` and `HP` to allocated value
    - [ ]  Set `Attributes` with (might, finesse, wits, will, sturdiness)
    - [ ]  Set `BaseDamageDice` and `DamageBonus`
    - [ ]  Set `IsBoss`, `IsForlorn`, `Soak` flags as appropriate
    - [ ]  Set `BaseLegendValue` to calculated value
    - [ ]  Set `Phase = 1` if phase-based AI

**3. EnemyAI.cs** (`RuneAndRust.Engine/EnemyAI.cs`):

- [ ]  Add switch case in `DetermineAction()` method
- [ ]  Implement `Determine[YourEnemyName]Action(Enemy enemy)` method (see `/docs/templates/ai-behavior-pattern-template.md` for code templates):
    - [ ]  Use `Random.Next(100)` for probability rolls
    - [ ]  Implement archetype-appropriate probability distribution (see Template 1-5 for your archetype)
    - [ ]  Add phase logic if mini-boss/boss (check `enemy.HP` thresholds, see Template 4)

**4. LootService.cs** (`RuneAndRust.Engine/LootService.cs`):

- [ ]  Add switch case in `GenerateLoot()` method (line ~22-43)
- [ ]  Implement `Generate[YourEnemyName]Loot(PlayerCharacter? player)`:
    - [ ]  Use threat tier → loot quality mapping (SPEC-COMBAT-012 FR-007)
    - [ ]  Boss tier: 70% Tier 4, 30% Tier 3
    - [ ]  Apply class-appropriate filtering (60% standard, 100% boss)

**5. DormantProcess.cs** (`RuneAndRust.Core/Population/DormantProcess.cs`) (optional):

- [ ]  If enemy spawns via procedural generation, add to population tables
- [ ]  Set `ThreatLevel` enum value (Minimal/Low/Medium/High/Boss)

**Documentation**:

**6. Bestiary Entry** (use `docs/templates/enemy-bestiary-entry.md`):

- [ ]  Create `/docs/bestiary/[your-enemy-name].md`
- [ ]  Fill out all sections: Stats, Abilities, AI Behavior, Loot Table, Trauma Impact
- [ ]  Include scaling formulas if enemy uses Legend-based scaling
- [ ]  Document v0.X balance adjustments if any

**Testing & Validation**:

- [ ]  Build and run game successfully (no compile errors)
- [ ]  Spawn enemy in test encounter
- [ ]  Verify stats match worksheet (HP, damage, attributes)
- [ ]  Verify AI behavior matches probability distribution
- [ ]  Playtest TTK: Does it feel within tier targets? (2-3 turns Low, 12-20 turns Boss)
- [ ]  Playtest difficulty: Too easy? Too hard? Adjust HP/damage using "safe parameters"
- [ ]  Verify loot drops match tier quality mapping
- [ ]  If IsForlorn: Verify trauma infliction matches worksheet values

---

## Step 7: Common Pitfalls & v0.18 Lessons

**Avoid These Mistakes**:

- [ ]  **One-Shot Deaths**: Never exceed 80% of player HP in single hit (Lethal tier max 4d6)
    - v0.18 Fix: Failure Colossus 4d6+3 → 3d6+4 (prevent one-shots)
- [ ]  **Damage Variance**: Keep max/min ratio under 4:1 for Lethal/Boss tiers
    - v0.18 Fix: Sentinel Prime 5d6 → 4d6 (reduce unfair variance)
- [ ]  **Bullet Sponge**: Soak cap at 4 for non-boss, 6 for boss
    - v0.18 Fix: Vault Custodian Soak 6 → 4, Omega Sentinel Soak 8 → 6
- [ ]  **Stat Bloat**: Use stat budgets, don't add attributes without justification
    - Medium tier: 8-16 attr points total, not 20+
- [ ]  **Legend Mismatch**: Ensure Legend/HP ratio is 0.5-1.0
    - v0.18 Fix: Multiple enemies had Legend adjusted to match HP investment

**Safe Balance Adjustments** (if TTK/difficulty feels off):

1. **Adjust HP ±20%**: Linear TTK impact, safe (e.g., 15 HP → 18 HP adds 1 turn)
2. **Adjust Damage Bonus ±1-2**: Affects lethality without variance (e.g., 1d6+1 → 1d6+2)
3. **Adjust Soak ±1**: Major impact (+1 Soak ≈ +15% effective HP)
4. **Adjust AI Probabilities ±10%**: Changes behavior without breaking role

**Dangerous Adjustments** (requires full revalidation):

1. **Changing Damage Dice Count** (e.g., 2d6 → 3d6): Affects max damage, variance, one-shot risk
2. **Changing Threat Tier**: Affects spawning, loot quality, 15+ encounter compositions
3. **Adding/Removing IsBoss or IsForlorn**: Triggers major system changes

---

## Worksheet Complete

**Final Checklist**:

- [ ]  All stats within SPEC-COMBAT-012 tier budgets
- [ ]  Archetype pattern matches AI behavior probabilities
- [ ]  TTK and damage output within tier targets
- [ ]  No v0.18 pitfalls (one-shots, bullet sponges, damage variance)
- [ ]  Loot quality matches threat tier
- [ ]  Code implemented in 4-5 files (Enemy.cs, EnemyFactory.cs, EnemyAI.cs, LootService.cs, optional DormantProcess.cs)
- [ ]  Bestiary entry created with complete documentation
- [ ]  Playtested and validated in-game

**Designed By**: ___________________________ **Date**: ___________

**Reviewed By**: ___________________________ **Date**: ___________

---

**Reference Documentation**:

- **SPEC-COMBAT-012**: `/docs/00-specifications/combat/enemy-design-spec.md` (1,494 lines, complete design system)
- **AI Behavior Template**: `/docs/templates/ai-behavior-pattern-template.md` (658 lines, AI implementation code templates)
- **Bestiary Template**: `/docs/templates/enemy-bestiary-entry.md` (284 lines, documentation template)
- **Implementation Files**: `RuneAndRust.Core/Enemy.cs`, `RuneAndRust.Engine/EnemyFactory.cs`, `RuneAndRust.Engine/EnemyAI.cs`