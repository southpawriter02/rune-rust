# v0.42: Enemy AI Improvements & Behavior Polish

Type: Mechanic
Description: Implements intelligent enemy AI using tactical positioning, ability prioritization, and adaptive behaviors. Delivers tactical decision-making system, ability usage optimization, 8+ distinct AI behavior archetypes, boss AI improvements with phase-aware ability rotation, and difficulty scaling behaviors from Normal to NG+5.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.1 (Combat System), v0.20-v0.20.5 (Tactical Grid), v0.21-v0.23 (Advanced Combat), v0.40 (Endgame)
Implementation Difficulty: Hard
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.42.1: Tactical Decision-Making & Target Selection (v0%2042%201%20Tactical%20Decision-Making%20&%20Target%20Selectio%2030bdc32815474cb4aaccb9938b1681fa.md), v0.42.2: Ability Usage & Behavior Patterns (v0%2042%202%20Ability%20Usage%20&%20Behavior%20Patterns%209ac68e3239c84cd0975cb2ce6b3dba96.md), v0.42.4: Integration & Difficulty Scaling (v0%2042%204%20Integration%20&%20Difficulty%20Scaling%202aeaf10bc4854fbeb7e10feeaa87a824.md), v0.42.3: Boss AI & Advanced Behaviors (v0%2042%203%20Boss%20AI%20&%20Advanced%20Behaviors%200dbc74d51462439fb644f51e748e021a.md)
Template Validated: No
Voice Validated: No

**Status:** Design Phase

**Prerequisites:** v0.1 (Combat System), v0.20-v0.20.5 (Tactical Grid), v0.21-v0.23 (Advanced Combat), v0.40 (Endgame)

**Timeline:** 20-30 hours (3-4 weeks part-time)

**Goal:** Transform enemy AI from basic threat generation to intelligent, tactical opposition

**Philosophy:** Enemies should feel dangerous because they're smart, not just because they have big numbers

---

## I. Executive Summary

v0.42 implements **intelligent enemy AI** that uses tactical positioning, ability prioritization, and adaptive behaviors to create challenging, fair, and memorable combat encounters.

**What v0.42 Delivers:**

- Tactical decision-making system (target selection, positioning, threat assessment)
- Ability usage optimization (when to use special abilities vs basic attacks)
- Behavior pattern system (8+ distinct AI personalities)
- Boss AI improvements (phase-aware ability rotation)
- Difficulty scaling behaviors (Normal → NG+5)
- Integration with tactical grid, advanced combat, and endgame systems

**Success Metric:**

Players describe enemies as "challenging but fair" and attribute defeats to strategic mistakes, not RNG or cheap mechanics. Boss encounters feel memorable and skill-testing.

---

## II. Design Philosophy

### A. Smart Opponents, Not Stat Walls

**Principle:** Difficulty comes from intelligent play, not inflated HP/damage.

**Design Rationale:**

Many games create difficulty by giving enemies massive stat bonuses. This creates bullet sponge tedium rather than tactical challenge. v0.42 makes enemies dangerous through **intelligent decision-making**.

**Example: Basic Enemy (Pre-v0.42):**

```jsx
AI Logic:
1. Pick random target
2. Use random ability
3. Stand still

Result: Predictable, non-threatening, boring
```

**Example: Basic Enemy (Post-v0.42):**

```jsx
AI Logic:
1. Assess threat levels (who's dealing most damage?)
2. Evaluate positioning (am I flanked? Can I flank?)
3. Choose optimal ability (stun the healer, not the tank)
4. Reposition if vulnerable

Result: Tactical, threatening, engaging
```

**Why This Matters:**

- Players respect enemies that "outplay" them
- Victories feel earned, not lucky
- Tactical grid system (v0.20) becomes meaningful
- Advanced combat mechanics (v0.21-v0.23) are fully utilized

### B. Behavior Diversity Through Archetypes

**Principle:** Different enemies have distinct AI personalities, not just different stats.

**8 Core AI Archetypes:**

**1. Aggressive (Berserker Pattern):**

- Prioritizes high-damage targets
- Uses offensive abilities liberally
- Ignores defensive positioning
- Rushes toward enemies
- Example: Skar-Horde Berserkers, Magma Elementals

**2. Defensive (Guardian Pattern):**

- Protects nearby allies
- Uses defensive abilities to shield others
- Positions between threats and allies
- Prioritizes intercepting attacks
- Example: Rusted Wardens, Iron-Bane Crusaders

**3. Cautious (Survivor Pattern):**

- Retreats when HP drops below 50%
- Uses cover extensively
- Prioritizes self-preservation
- Avoids risky engagements
- Example: Scavengers, Undying Scouts

**4. Reckless (Suicide Pattern):**

- Charges into melee regardless of danger
- Uses high-risk/high-reward abilities
- Ignores HP thresholds
- No retreat logic
- Example: Corrupted Thralls, Blighted Hounds

**5. Tactical (Commander Pattern):**

- Coordinates with allies
- Uses positioning to maximize advantage
- Prioritizes high-value targets
- Adapts to player strategy
- Example: Forge-Masters, Jötun Commanders

**6. Support (Healer Pattern):**

- Prioritizes healing injured allies
- Stays at range
- Uses buffs on strongest allies
- Retreats when threatened
- Example: Bone-Menders, Reality Shapers

**7. Control (Crowd Control Pattern):**

- Uses status effects liberally
- Targets clustered enemies
- Prioritizes disabling high-threat targets
- Maintains distance
- Example: Frost-Weavers, Psychic Leeches

**8. Ambusher (Stealth Pattern):**

- Waits for optimal moment to strike
- Targets isolated enemies
- Uses positioning for flanking
- Retreats after burst damage
- Example: Rust-Stalkers, Shadow-Forms

**Why Multiple Archetypes:**

- Forces players to adapt tactics per encounter
- Creates memorable enemy identities
- Prevents "one strategy beats everything"
- Leverages existing enemy variety (20+ types)

### C. Boss AI as Peak Challenge

**Principle:** Boss encounters showcase the best of enemy AI capabilities.

**Boss AI Enhancements:**

**Phase-Aware Behavior:**

- Phase 1: Standard pattern to teach mechanics
- Phase 2: Increased aggression, new abilities
- Phase 3: Desperate tactics, maximum lethality

**Ability Rotation:**

- Bosses use abilities in intelligent sequences
- No random spam, deliberate rotations
- Telegraphed attacks give fair warning
- Vulnerability windows reward player execution

**Add Management:**

- Bosses summon adds at strategic moments
- Adds coordinate with boss (don't just stand there)
- Boss protects adds when necessary
- Add waves create pressure, not tedium

**Adaptive Difficulty:**

- Boss recognizes player strategy
- Adjusts targeting if player is healing too much
- Changes positioning if player camps one spot
- Forces tactical adaptation mid-fight

**Example: Forge-Master Thrain (Pre-v0.42):**

```jsx
Phase 1 (100-66% HP):
- Random basic attack
- Random special ability
- Stand in middle of room

Phase 2 (66-33% HP):
- Same but +50% damage

Phase 3 (33-0% HP):
- Same but +100% damage

Result: Stat check, not skill check
```

**Example: Forge-Master Thrain (Post-v0.42):**

```jsx
Phase 1 (100-66% HP):
- Opening: Summon 2 Forge-Hardened adds
- While adds alive: Focus on area denial (Lava Torrents)
- If adds die: Switch to melee pressure
- Ability rotation: Lava Torrent → Molten Grasp → Basic Attack × 2

Phase 2 (66-33% HP):
- Transition: "The Forge awakens!" - AOE fire pulse
- Summon 3 adds (1 Healer, 2 Warriors)
- Prioritize killing healer first (player knows this)
- Boss targets whoever attacks healer
- Ability rotation: Meteor Strike → Flame Breath → Molten Armor (self-buff)

Phase 3 (33-0% HP):
- Enrage: All abilities deal +50% damage
- No more adds, pure boss DPS race
- Ability rotation becomes faster
- Charges at ranged characters
- Uses Molten Crash (room-wide AOE) every 5 turns

Result: Memorable, fair, skill-testing encounter
```

### D. Difficulty Scaling Through Intelligence

**Principle:** Higher difficulties increase AI intelligence, not just stats.

**AI Intelligence by Difficulty:**

**Normal (NG+0):**

- Basic threat assessment
- Simple ability priorities
- Occasional tactical errors
- Forgiving targeting logic

**NG+1-2:**

- Improved threat assessment
- Better ability usage timing
- Fewer tactical errors
- Punishes obvious mistakes

**NG+3-4:**

- Advanced threat calculation
- Near-optimal ability usage
- Coordinated group tactics
- Punishes most mistakes

**NG+5:**

- Perfect threat assessment
- Optimal ability usage
- Maximum group coordination
- Punishes all mistakes

**Challenge Sectors:**

- AI uses sector modifiers intelligently
- Adapts to challenge constraints
- Example: In [No Healing] sector, AI focuses on burst damage

**Boss Gauntlet:**

- Each boss progressively smarter
- Boss 8 has perfect AI execution
- Tests player's ability to adapt

**Endless Mode:**

- AI intelligence scales with wave number
- Wave 1-10: Normal intelligence
- Wave 11-20: NG+2 intelligence
- Wave 21-30: NG+4 intelligence
- Wave 31+: NG+5 intelligence

**Why Scaling Intelligence:**

- Makes NG+ feel genuinely harder, not just spongier
- Rewards system mastery, not just character optimization
- Creates "I learned to play better" moments
- Respects player time (no artificial grind gates)

### E. Fair Challenge, Not Cheap Difficulty

**Principle:** AI should beat players through superior tactics, not cheap tricks.

**Design Rules:**

**✅ FAIR AI Behaviors:**

- Uses same ability rules as player
- Respects action economy (can't spam abilities)
- Follows positioning rules (no teleporting)
- Telegraphs dangerous abilities
- Has exploitable patterns
- Makes occasional "mistakes" at lower difficulties

**❌ UNFAIR AI Behaviors (Forbidden):**

- Reading player inputs (no frame-perfect reactions)
- Cheating on cooldowns or resources
- Ignoring game rules (phasing through walls)
- Instant reactions (give players time to respond)
- Hidden information advantages
- Punishing correct play

**Example: Fair Stun Usage:**

```jsx
✅ FAIR:
- AI sees player charging big attack
- AI uses stun ability (if off cooldown)
- Player can see stun coming (telegraph)
- Player can dodge or accept the stun

Result: "They outplayed me, I should have dodged"

❌ UNFAIR:
- AI waits for exact frame player presses attack button
- AI instantly uses stun (no telegraph)
- Player cannot react

Result: "That's bullshit, they read my inputs"
```

**Why Fairness Matters:**

- Players blame themselves for losses, not the AI
- Victories feel earned and satisfying
- Encourages learning and adaptation
- Builds player skill and confidence

---

## III. System Overview

### Current State Analysis (Pre-v0.42)

**Existing Enemy AI:**

- Basic threat generation (random or nearest target)
- Simple ability usage (use when off cooldown)
- No tactical positioning
- No behavior differentiation
- No difficulty scaling beyond stats

**What Works:**

- Enemies spawn correctly
- Combat resolution functional
- Abilities execute properly
- Turn order managed

**What Doesn't Work:**

- Enemies feel predictable and passive
- No tactical challenge beyond raw stats
- Grid system underutilized by AI
- Boss encounters feel like stat checks
- Higher difficulties just mean bigger numbers

**Why v0.42 is Needed:**

- Tactical grid (v0.20) wasted on dumb AI
- Advanced combat (v0.21-v0.23) unused by enemies
- Endgame content (v0.40) needs intelligent opposition
- Polish phase demands quality AI

### Scope Definition

**✅ In Scope (v0.42):**

**Core AI Systems:**

- Tactical decision-making service
- Threat assessment algorithms
- Target selection logic
- Positioning evaluation
- Ability prioritization system
- Behavior pattern framework

**AI Archetypes:**

- 8 distinct behavior patterns
- Archetype assignment per enemy type
- Difficulty scaling per archetype

**Boss AI:**

- Phase-aware behavior
- Ability rotation system
- Add management logic
- Adaptive targeting

**Integration:**

- Tactical grid positioning
- Advanced combat mechanics
- Endgame difficulty scaling
- Database schema for AI configuration

**Testing:**

- 80%+ unit test coverage
- AI behavior validation
- Performance benchmarks
- Playtesting validation

**❌ Out of Scope:**

**Not AI Problems:**

- New enemy types (content, not AI)
- New abilities (balance, not AI)
- New status effects (mechanics, not AI)
- Visual polish (v0.43 focus)

**Future Expansions:**

- Machine learning AI (v2.0+)
- Player behavior prediction (v2.0+)
- Dynamic difficulty adjustment (v2.0+)
- Procedural AI generation (v2.0+)

**Why These Limits:**

v0.42 is AI architecture and behavior polish only. Content expansion is separate. Keep scope tight for timely delivery.

### System Lifecycle

```jsx
ENEMY TURN START
  ↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 1: SITUATIONAL ANALYSIS                                │
│   - Assess current battlefield state                        │
│   - Identify threats and opportunities                      │
│   - Evaluate self HP, resources, positioning                │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 2: THREAT ASSESSMENT                                   │
│   - Calculate threat score for each enemy                   │
│   - Consider: damage output, HP, positioning, abilities     │
│   - Weight by AI archetype (Aggressive prioritizes damage)  │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 3: TARGET SELECTION                                    │
│   - Choose primary target based on threat scores            │
│   - Apply archetype modifiers (Healer → lowest HP ally)     │
│   - Consider secondary objectives (protect ally, etc.)      │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 4: ABILITY SELECTION                                   │
│   - Evaluate available abilities                            │
│   - Score each ability for current situation                │
│   - Consider: damage, utility, cooldown, resource cost      │
│   - Choose highest-scoring ability                          │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 5: POSITIONING DECISION                                │
│   - Evaluate current position                               │
│   - Check for flanking opportunities                        │
│   - Assess vulnerability (am I flanked?)                    │
│   - Consider cover and elevation                            │
│   - Decide: stay, advance, retreat, reposition              │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 6: EXECUTE DECISION                                    │
│   - Move if repositioning decided                           │
│   - Use selected ability on chosen target                   │
│   - Log decision for debugging                              │
└─────────────────────────────────────────────────────────────┘
  ↓
ENEMY TURN END
```

---

## IV. Child Specifications Overview

### v0.42.1: Tactical Decision-Making & Target Selection (5-8 hours)

**Focus:** Core AI decision-making framework

**Deliverables:**

- TacticalDecisionService architecture
- Threat assessment algorithms
- Target selection logic
- Situational analysis system
- Database schema for AI weights

**Key Systems:**

- Threat scoring (damage × priority × vulnerability)
- Target prioritization per archetype
- Situational awareness (flanked? low HP? allies nearby?)
- Decision tree framework

---

### v0.42.2: Ability Usage & Behavior Patterns (5-8 hours)

**Focus:** Ability prioritization and behavior archetypes

**Deliverables:**

- Ability scoring system
- 8 AI behavior archetypes
- Cooldown management
- Resource optimization
- Archetype assignment database

**Key Systems:**

- Ability evaluation (damage vs utility vs cooldown)
- Behavior pattern implementation
- Archetype-specific logic
- Ability rotation for bosses

---

### v0.42.3: Boss AI & Advanced Behaviors (5-8 hours)

**Focus:** Boss-specific AI and complex encounter logic

**Deliverables:**

- Phase-aware boss AI
- Add management system
- Ability rotation framework
- Adaptive difficulty logic
- Boss AI configuration database

**Key Systems:**

- Phase transition behaviors
- Boss ability rotations
- Add coordination
- Player strategy recognition

---

### v0.42.4: Integration & Difficulty Scaling (5-8 hours)

**Focus:** Endgame integration and difficulty tuning

**Deliverables:**

- NG+ AI intelligence scaling
- Challenge Sector AI adaptation
- Boss Gauntlet AI progression
- Endless Mode wave scaling
- Performance optimization
- Comprehensive testing

**Key Systems:**

- Difficulty-based intelligence modifiers
- Endgame mode AI variants
- Performance profiling
- AI debugging tools

---

## V. Technical Architecture

### Service Layer

```csharp
public interface ITacticalDecisionService
{
    Task<EnemyAction> DecideActionAsync(Enemy enemy, BattlefieldState state);
    Task<Character> SelectTargetAsync(Enemy enemy, List<Character> targets);
    Task<int> CalculateThreatScoreAsync(Enemy enemy, Character target);
    Task<GridPosition> EvaluateBestPositionAsync(Enemy enemy, BattlefieldState state);
}

public interface IAbilityPrioritizationService
{
    Task<Ability> SelectOptimalAbilityAsync(Enemy enemy, Character target, BattlefieldState state);
    Task<float> ScoreAbilityAsync(Ability ability, Enemy user, Character target);
    Task<List<Ability>> GetAvailableAbilitiesAsync(Enemy enemy);
}

public interface IBehaviorPatternService
{
    Task<AIArchetype> GetArchetypeAsync(Enemy enemy);
    Task ApplyArchetypeBehaviorAsync(Enemy enemy, EnemyAction action);
    Task<float> GetArchetypeModifierAsync(AIArchetype archetype, ThreatFactor factor);
}

public interface IBossAIService
{
    Task<BossAction> DecideBossActionAsync(Boss boss, BattlefieldState state);
    Task<AbilityRotation> GetPhaseRotationAsync(Boss boss, int currentPhase);
    Task ManageAddsAsync(Boss boss, BattlefieldState state);
    Task<bool> ShouldTransitionPhaseAsync(Boss boss);
}
```

### Data Models

```csharp
public enum AIArchetype
{
    Aggressive,      // High damage priority, ignore defense
    Defensive,       // Protect allies, use shields
    Cautious,        // Self-preservation, retreat when low
    Reckless,        // Charge in, ignore danger
    Tactical,        // Optimal play, coordinate
    Support,         // Heal allies, buff teammates
    Control,         // CC priority, disable threats
    Ambusher         // Wait for opportunity, burst damage
}

public enum ThreatFactor
{
    DamageOutput,    // How much damage target deals
    CurrentHP,       // How easy to kill
    Positioning,     // Tactical advantage/disadvantage
    Abilities,       // Threat from target's abilities
    StatusEffects,   // Buffs/debuffs on target
    Proximity        // Distance from AI
}

public class EnemyAction
{
    public Enemy Actor { get; set; }
    public Character Target { get; set; }
    public Ability SelectedAbility { get; set; }
    public GridPosition? MoveTo { get; set; }
    public ActionType Type { get; set; }  // Attack, Move, Defend, UseAbility
    public float ConfidenceScore { get; set; }  // 0-1, how certain AI is
}

public class BattlefieldState
{
    public List<Character> PlayerCharacters { get; set; }
    public List<Enemy> Enemies { get; set; }
    public TacticalGrid Grid { get; set; }
    public int CurrentTurn { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

public class ThreatAssessment
{
    public Character Target { get; set; }
    public float TotalThreatScore { get; set; }
    public Dictionary<ThreatFactor, float> FactorScores { get; set; }
    public string Reasoning { get; set; }  // For debugging
}
```

---

## VI. Integration Points

### v0.1: Combat System Integration

```csharp
// In CombatService.cs - Enemy turn processing

public async Task ProcessEnemyTurnAsync(Enemy enemy)
{
    // OLD (Pre-v0.42): Random target, random ability
    // var target = _targets.Random();
    // var ability = enemy.Abilities.Random();
    
    // NEW (Post-v0.42): Intelligent decision-making
    var battlefieldState = await BuildBattlefieldStateAsync();
    var action = await _tacticalDecisionService.DecideActionAsync(enemy, battlefieldState);
    
    // Log AI reasoning for debugging
    _logger.Information(
        "Enemy {EnemyId} ({Archetype}) chose action: Target={TargetId}, Ability={AbilityId}, Reason={Reason}",
        enemy.EnemyId, enemy.AIArchetype, [action.Target](http://action.Target)?.CharacterId, 
        action.SelectedAbility?.AbilityId, action.Reasoning);
    
    // Execute decided action
    await ExecuteEnemyActionAsync(action);
}
```

### v0.20-v0.20.5: Tactical Grid Integration

```csharp
// AI uses tactical positioning from v0.20

public async Task<GridPosition> EvaluateBestPositionAsync(Enemy enemy, BattlefieldState state)
{
    var currentPos = enemy.GridPosition;
    var possiblePositions = state.Grid.GetReachablePositions(currentPos, enemy.MovementRange);
    
    var scores = new Dictionary<GridPosition, float>();
    
    foreach (var pos in possiblePositions)
    {
        float score = 0f;
        
        // Consider flanking opportunities (v0.20.1)
        var flankingTargets = state.Grid.GetFlankableTargets(pos);
        score += flankingTargets.Count * 10f;
        
        // Consider cover (v0.20.2)
        var coverLevel = state.Grid.GetCoverLevel(pos);
        score += (int)coverLevel * 5f;
        
        // Consider elevation (v0.20)
        var elevation = state.Grid.GetElevation(pos);
        if (elevation > enemy.GridPosition.Elevation)
            score += 8f;  // High ground bonus
        
        // Consider proximity to allies (formation, v0.20.5)
        var nearbyAllies = state.Grid.GetAlliesInRange(pos, range: 2);
        if (enemy.AIArchetype == AIArchetype.Defensive)
            score += nearbyAllies.Count * 6f;  // Defensive wants to stay with group
        
        scores[pos] = score;
    }
    
    return scores.OrderByDescending(kvp => kvp.Value).First().Key;
}
```

### v0.21-v0.23: Advanced Combat Integration

```csharp
// AI uses stances (v0.21.1), status effects (v0.21.3), environmental combat (v0.22)

public async Task<Ability> SelectOptimalAbilityAsync(Enemy enemy, Character target, BattlefieldState state)
{
    var abilities = await GetAvailableAbilitiesAsync(enemy);
    var scores = new Dictionary<Ability, float>();
    
    foreach (var ability in abilities)
    {
        float score = ability.BaseDamage;
        
        // Consider status effects (v0.21.3)
        if (ability.AppliesStatusEffect)
        {
            if (target.IsVulnerableToCC())
                score += 15f;  // Target has low Will, CC is effective
        }
        
        // Consider environmental effects (v0.22)
        if (ability.IsEnvironmentalDamage)
        {
            var hazards = state.Grid.GetHazardsNear(target.GridPosition);
            if (hazards.Any())
                score += 20f;  // Can push target into hazards
        }
        
        // Consider stance (v0.21.1)
        if (enemy.CurrentStance == CombatStance.Offensive)
            score *= 1.2f;  // Offensive stance boosts damage priority
        
        scores[ability] = score;
    }
    
    return scores.OrderByDescending(kvp => kvp.Value).First().Key;
}
```

### v0.40: Endgame Difficulty Scaling

```csharp
// AI intelligence scales with NG+ tier, Challenge Sectors, Endless waves

public async Task<EnemyAction> DecideActionAsync(Enemy enemy, BattlefieldState state)
{
    // Get difficulty context
    var ngPlusTier = await _gameStateService.GetCurrentNGPlusTierAsync();
    var endlessWave = await _gameStateService.GetCurrentEndlessWaveAsync();
    var challengeModifiers = await _gameStateService.GetActiveChallengeModifiersAsync();
    
    // Calculate AI intelligence level
    int intelligenceLevel = CalculateIntelligenceLevel(ngPlusTier, endlessWave);
    
    // Apply intelligence modifiers
    var decision = await MakeDecisionAsync(enemy, state, intelligenceLevel);
    
    // Lower intelligence = occasional mistakes
    if (intelligenceLevel < 3 && Random.NextDouble() < 0.15)  // 15% mistake chance at low intelligence
    {
        _logger.Debug("AI {EnemyId} made tactical error (intelligence={Level})", 
            enemy.EnemyId, intelligenceLevel);
        decision = await MakeSuboptimalDecisionAsync(enemy, state);
    }
    
    return decision;
}

private int CalculateIntelligenceLevel(int ngPlusTier, int? endlessWave)
{
    // Intelligence scale: 0 (dumb) to 5 (perfect)
    
    if (endlessWave.HasValue)
    {
        // Endless Mode: intelligence scales with wave
        if (endlessWave <= 10) return 1;
        if (endlessWave <= 20) return 2;
        if (endlessWave <= 30) return 3;
        if (endlessWave <= 40) return 4;
        return 5;  // Wave 40+: perfect AI
    }
    
    // NG+ Mode: intelligence scales with tier
    return Math.Min(ngPlusTier, 5);  // NG+5 = intelligence 5
}
```

---

## VII. Success Criteria

**v0.42 is DONE when:**

### ✅ Core AI Systems

- [ ]  TacticalDecisionService operational
- [ ]  Threat assessment calculates accurate scores
- [ ]  Target selection chooses logical targets
- [ ]  Ability prioritization selects optimal abilities
- [ ]  Positioning evaluation uses grid tactics

### ✅ Behavior Archetypes

- [ ]  8 AI archetypes implemented
- [ ]  Each archetype has distinct behavior
- [ ]  Archetype assignment database complete
- [ ]  Enemies use archetype-appropriate tactics

### ✅ Boss AI

- [ ]  Phase-aware behavior functional
- [ ]  Ability rotations execute properly
- [ ]  Add management coordinates with boss
- [ ]  Adaptive targeting responds to player strategy

### ✅ Difficulty Scaling

- [ ]  NG+ tiers increase AI intelligence
- [ ]  Challenge Sector modifiers affect AI
- [ ]  Boss Gauntlet progression works
- [ ]  Endless Mode wave scaling functional

### ✅ Integration

- [ ]  Tactical grid positioning utilized
- [ ]  Advanced combat mechanics employed
- [ ]  Status effects used intelligently
- [ ]  Environmental combat leveraged

### ✅ Quality

- [ ]  80%+ unit test coverage
- [ ]  AI reasoning logged for debugging
- [ ]  Performance <50ms per enemy decision
- [ ]  No infinite loops or hangs

### ✅ Playtesting Validation

- [ ]  Players describe enemies as "smart"
- [ ]  Boss encounters feel challenging but fair
- [ ]  NG+5 enemies require tactical play
- [ ]  No "cheap" or "unfair" AI complaints

---

## VIII. Timeline

**Total: 20-30 hours (3-4 weeks part-time)**

**Week 1: Core Decision-Making (v0.42.1)** — 5-8 hours

- Tactical decision framework
- Threat assessment algorithms
- Target selection logic
- Database schema

**Week 2: Ability & Behavior (v0.42.2)** — 5-8 hours

- Ability prioritization system
- 8 behavior archetypes
- Archetype assignment
- Cooldown management

**Week 3: Boss AI (v0.42.3)** — 5-8 hours

- Phase-aware logic
- Ability rotations
- Add management
- Adaptive difficulty

**Week 4: Integration & Tuning (v0.42.4)** — 5-8 hours

- Endgame integration
- Difficulty scaling
- Performance optimization
- Comprehensive testing

---

## IX. Benefits

### For Gameplay

- ✅ **Challenging Combat:** Enemies provide genuine tactical challenge
- ✅ **Fair Difficulty:** Players feel defeats are deserved, victories earned
- ✅ **Tactical Depth:** Grid positioning becomes crucial, not optional
- ✅ **Memorable Encounters:** Boss fights feel unique and skill-testing

### For Endgame

- ✅ **NG+ Value:** Higher difficulties feel genuinely harder, not just spongier
- ✅ **Challenge Sectors:** Extreme modifiers create unique AI behaviors
- ✅ **Boss Gauntlet:** Sequential bosses test adaptability
- ✅ **Endless Mode:** Wave scaling provides infinite challenge

### For Polish

- ✅ **Professional Feel:** AI quality signals overall game quality
- ✅ **Replayability:** Smart enemies prevent "solved" gameplay
- ✅ **Player Respect:** Players recommend game for "challenging AI"
- ✅ **Content Leverage:** Existing enemies feel new with better AI

---

## X. After v0.42 Ships

**You'll Have:**

- ✅ Intelligent enemy AI that uses tactics, not just stats
- ✅ 8 distinct behavior archetypes creating enemy variety
- ✅ Boss encounters that showcase peak AI capabilities
- ✅ Difficulty scaling through intelligence, not just numbers
- ✅ Full integration with tactical grid and advanced combat

**Next Steps:**

- **v0.43:** Ability Balance & Visual Feedback Polish
- **v0.44:** Room Templates & Environmental Storytelling
- **v0.45:** Quest Narrative & Dialogue Polish
- **v0.46:** Equipment Progression & Loot Balance

**The AI transforms from predictable to challenging, from fair to engaging.**

---

**Ready to make enemies worthy opponents.**