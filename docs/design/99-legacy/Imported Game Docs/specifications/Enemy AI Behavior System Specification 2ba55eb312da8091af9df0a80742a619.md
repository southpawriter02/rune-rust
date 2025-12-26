# Enemy AI Behavior System Specification

Parent item: Specs: Systems (Specs%20Systems%202ba55eb312da80c6aa36ce6564319160.md)

> Template Version: 1.0
Last Updated: 2025-11-27
Status: Active
Specification ID: SPEC-SYSTEM-005
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2025-11-27 | AI | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [x]  **Review**: Ready for stakeholder review
- [x]  **Approved**: Approved for implementation
- [x]  **Active**: Currently implemented and maintained
- [ ]  **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders

- **Primary Owner**: Combat Designer
- **Design**: AI archetypes, behavior patterns, boss mechanics
- **Balance**: Difficulty scaling, threat assessment
- **Implementation**: EnemyAIOrchestrator.cs, AI service ecosystem
- **QA/Testing**: AI decision validation, edge cases

---

## Executive Summary

### Purpose Statement

The Enemy AI Behavior System provides intelligent decision-making for enemies through archetype-based behavior patterns, threat assessment, ability prioritization, and adaptive difficulty scaling.

### Scope

**In Scope**:

- AI archetype system (Aggressive, Defensive, Support, etc.)
- Threat assessment and target selection
- Ability prioritization and scoring
- Boss AI with phase mechanics
- Adaptive difficulty scaling
- Challenge Sector AI modifications
- Performance monitoring

**Out of Scope**:

- Enemy stat definitions → `SPEC-COMBAT-003` (Enemy Design)
- Combat damage calculation → `SPEC-COMBAT-002`
- Encounter composition → `SPEC-SYSTEM-007` (Encounter Generation)
- Loot drops → `SPEC-ECONOMY-001`

### Success Criteria

- **Player Experience**: Combat feels challenging but fair; enemies behave intelligently
- **Technical**: AI decisions complete in <100ms per enemy
- **Design**: Each archetype creates distinct gameplay experience
- **Balance**: Difficulty scaling maintains engagement across skill levels

---

## Related Documentation

### Dependencies

**Depends On**:

- Combat System: Combat state, actions → `SPEC-COMBAT-001`
- Character System: Player stats for threat assessment → `SPEC-PROGRESSION-001`
- Ability System: Enemy abilities → `SPEC-PROGRESSION-003`

**Depended Upon By**:

- Combat Engine: Invokes AI for enemy turns → `SPEC-COMBAT-001`
- Boss Encounters: Uses boss AI subsystem → `SPEC-COMBAT-004`
- Encounter Generation: Composes enemy groups → `SPEC-SYSTEM-007`

### Code References

- **Primary Service**: `RuneAndRust.Engine/AI/EnemyAIOrchestrator.cs`
- **Threat Assessment**: `RuneAndRust.Engine/AI/ThreatAssessmentService.cs`
- **Target Selection**: `RuneAndRust.Engine/AI/TargetSelectionService.cs`
- **Ability Prioritization**: `RuneAndRust.Engine/AI/AbilityPrioritizationService.cs`
- **Boss AI**: `RuneAndRust.Engine/AI/BossAIService.cs`
- **Adaptive Difficulty**: `RuneAndRust.Engine/AI/AdaptiveDifficultyService.cs`
- **Situational Analysis**: `RuneAndRust.Engine/AI/SituationalAnalysisService.cs`

---

## Design Philosophy

### Design Pillars

1. **Archetype-Driven Behavior**
    - **Rationale**: Different enemy types should fight differently
    - **Examples**: Aggressive enemies rush, Defensive enemies protect allies
2. **Intelligent but Readable**
    - **Rationale**: Players should understand why enemies act as they do
    - **Examples**: Low-health enemies retreat, healers prioritize wounded allies
3. **Scalable Difficulty**
    - **Rationale**: AI should adapt to player skill level
    - **Examples**: Higher difficulty = smarter targeting, better ability usage

### Player Experience Goals

**Target Experience**: Enemies feel like intelligent opponents, not predictable patterns

**Moment-to-Moment Gameplay**:

- Enemy evaluates battlefield state
- Selects appropriate target based on threat/opportunity
- Chooses ability matching situation
- Player observes and can predict/counter behavior

**Learning Curve**:

- **Novice** (0-2 hours): Enemies present straightforward challenges
- **Intermediate** (2-10 hours): Recognizes archetype patterns, exploits weaknesses
- **Expert** (10+ hours): Predicts AI decisions, manipulates threat levels

---

## Functional Requirements

### FR-001: Decide Enemy Action

**Priority**: Critical
**Status**: Implemented

**Description**:
System must determine optimal action for an enemy based on battlefield state, archetype, and available abilities.

**Rationale**:
Core AI function that drives all enemy behavior in combat.

**Acceptance Criteria**:

- [x]  Evaluates battlefield state
- [x]  Gets AI intelligence level from difficulty
- [x]  Uses boss AI path for boss enemies
- [x]  Uses normal AI path for regular enemies
- [x]  Applies difficulty scaling to final action
- [x]  Applies Challenge Sector modifiers if present
- [x]  Returns valid EnemyAction with target and ability

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/AI/EnemyAIOrchestrator.cs:DecideActionAsync()`

---

### FR-002: Assess Threat Levels

**Priority**: High
**Status**: Implemented

**Description**:
System must calculate threat values for all potential targets to inform target selection.

**Rationale**:
Intelligent targeting requires understanding relative threat levels.

**Formula/Logic**:

```
ThreatScore = BaseThreat × DamageModifier × HealthModifier × PositionModifier × BuffModifier

Components:
  BaseThreat = Character class base (Warrior: 1.2, Adept: 0.9, etc.)
  DamageModifier = RecentDamageDealt / AverageDamage
  HealthModifier = CurrentHP / MaxHP (low = lower threat)
  PositionModifier = Front row = 1.2, Back row = 0.8
  BuffModifier = Active buffs increase threat

Example:
  Warrior in front row, dealt 30 damage, at 80% HP
  BaseThreat = 1.2
  DamageModifier = 1.5 (above average damage)
  HealthModifier = 0.8
  PositionModifier = 1.2 (front row)
  BuffModifier = 1.0 (no buffs)

  ThreatScore = 1.2 × 1.5 × 0.8 × 1.2 × 1.0 = 1.73

```

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/AI/ThreatAssessmentService.cs`

---

### FR-003: Select Target

**Priority**: High
**Status**: Implemented

**Description**:
System must select optimal target based on threat assessment, archetype preferences, and situational factors.

**Rationale**:
Target selection determines combat flow and creates tactical decisions.

**Selection Criteria by Archetype**:

| Archetype | Primary Target | Secondary Target |
| --- | --- | --- |
| Aggressive | Highest threat | Lowest HP |
| Defensive | Nearest enemy | Attacking ally |
| Support | Wounded ally | Buffable ally |
| Opportunistic | Lowest HP enemy | Most vulnerable |
| Controller | Most threatening | Grouped enemies |

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/AI/TargetSelectionService.cs`

---

### FR-004: Prioritize Abilities

**Priority**: High
**Status**: Implemented

**Description**:
System must score and select optimal ability based on situation, target, and archetype.

**Rationale**:
Ability selection creates combat variety and challenge.

**Scoring Formula**:

```
TotalScore = (Damage × 0.4) + (Utility × 0.3) + (Efficiency × 0.2) + (Situation × 0.1)
TotalScore *= ArchetypeModifier

Components:
  Damage: Expected damage output
  Utility: Status effects, positioning value
  Efficiency: Resource cost vs benefit
  Situation: Contextual appropriateness

Example:
  Ability: "Stunning Strike" for Aggressive enemy
  Damage = 60 (moderate)
  Utility = 80 (stun effect)
  Efficiency = 70 (medium cost)
  Situation = 90 (target is casting)
  ArchetypeModifier = 0.9 (Aggressive prefers damage)

  Score = (60×0.4 + 80×0.3 + 70×0.2 + 90×0.1) × 0.9
        = (24 + 24 + 14 + 9) × 0.9 = 63.9

```

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/AI/AbilityPrioritizationService.cs:SelectOptimalAbilityAsync()`

---

### FR-005: Execute Boss AI

**Priority**: High
**Status**: Implemented

**Description**:
System must provide specialized AI for boss enemies including phase management, add spawning, and signature abilities.

**Rationale**:
Boss fights require more complex behavior than regular enemies.

**Boss AI Features**:

- Phase transitions based on HP thresholds
- Signature ability rotations
- Add management (spawn/protect minions)
- Enrage timers
- Special mechanics per boss

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/AI/BossAIService.cs`

---

### FR-006: Apply Adaptive Difficulty

**Priority**: Medium
**Status**: Implemented

**Description**:
System must adjust AI behavior based on player performance and difficulty settings.

**Rationale**:
Adaptive difficulty maintains engagement across skill levels.

**Intelligence Levels**:

| Level | Description | Behaviors |
| --- | --- | --- |
| 1 | Basic | Random targeting, basic abilities |
| 2 | Standard | Threat-based targeting, rotation |
| 3 | Advanced | Synergy awareness, cooldown tracking |
| 4 | Expert | Optimal targeting, ability combos |
| 5 | Master | Predictive, counter-play |

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/AI/AdaptiveDifficultyService.cs`

---

### FR-007: Analyze Situational Context

**Priority**: Medium
**Status**: Implemented

**Description**:
System must evaluate battlefield context to inform all AI decisions.

**Rationale**:
Context-aware AI creates more believable behavior.

**Situational Factors**:

- Ally HP states (any critical?)
- Player buffs/debuffs
- Terrain features (cover, hazards)
- Turn order position
- Resource states (stamina, abilities on cooldown)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/AI/SituationalAnalysisService.cs`

---

## System Mechanics

### Mechanic 1: AI Archetype System

**Overview**:
Each enemy has an AI archetype that influences all behavior decisions.

**Archetype Definitions**:

| Archetype | Description | Behavior Pattern |
| --- | --- | --- |
| Aggressive | Deals maximum damage | Attack highest threat, use damage abilities |
| Defensive | Protects self/allies | Prioritize defensive abilities, intercept attacks |
| Support | Buffs allies, debuffs enemies | Heal wounded, buff damage dealers |
| Opportunistic | Exploits weaknesses | Target low HP, use status effects |
| Controller | Manipulates battlefield | AoE abilities, crowd control |
| Berserker | Reckless damage | All-out attack, ignore defense |
| Cautious | Preserves resources | Efficient ability usage, retreat if wounded |

**Archetype Configuration**:

```
Aggressive:
  DamageAbilityModifier: 1.3
  UtilityAbilityModifier: 0.8
  DefensiveAbilityModifier: 0.6
  ThreatTargetWeight: 1.0
  VulnerableTargetWeight: 0.7

```

---

### Mechanic 2: Decision Pipeline

**Overview**:
AI decisions flow through a pipeline of services, each contributing to final action.

**Pipeline Flow**:

```
1. EnemyAIOrchestrator.DecideActionAsync()
   │
   ├─> Get Intelligence Level (AdaptiveDifficultyService)
   │
   ├─> Create Decision Context
   │
   ├─> If Boss → BossAIService.DecideBossActionAsync()
   │   └─> Phase management, signature abilities
   │
   └─> If Normal → DecideNormalEnemyActionAsync()
       │
       ├─> ThreatAssessmentService.AssessThreats()
       │   └─> Calculate threat per character
       │
       ├─> TargetSelectionService.SelectTarget()
       │   └─> Choose based on archetype + threat
       │
       ├─> AbilityPrioritizationService.SelectAbility()
       │   └─> Score and select optimal ability
       │
       └─> SituationalAnalysisService.AnalyzeSituation()
           └─> Contextual adjustments

2. Apply Difficulty Scaling
3. Apply Challenge Sector Modifiers (if any)
4. Return Final EnemyAction

```

---

### Mechanic 3: Performance Monitoring

**Overview**:
AI performance is monitored to ensure responsive gameplay.

**Monitoring Points**:

- Decision time per enemy
- Average decision time per archetype
- Peak decision time
- Decision cache hit rate

**Performance Targets**:

| Metric | Target | Warning | Critical |
| --- | --- | --- | --- |
| Per-enemy decision | <100ms | >150ms | >300ms |
| Per-turn total | <500ms | >750ms | >1000ms |
| Boss decision | <200ms | >300ms | >500ms |

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/AI/AIPerformanceMonitor.cs`

---

## State Management

### System State

**State Variables**:

| Variable | Type | Persistence | Default | Description |
| --- | --- | --- | --- | --- |
| IntelligenceLevel | int | Session | 3 | Current AI difficulty |
| BossPhase | int | Combat | 1 | Current boss phase |
| ThreatTable | Dict | Combat | {} | Threat scores per character |
| AbilityCooldowns | Dict | Combat | {} | Enemy ability cooldowns |

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
| --- | --- | --- | --- | --- | --- | --- |
| ThreatDecayRate | ThreatAssessment | 0.1/turn | 0 | 0.5 | Threat memory | Low |
| ArchetypeModifiers | Config | varies | 0.5 | 1.5 | Behavior emphasis | Medium |
| IntelligenceLevel | Difficulty | 1-5 | 1 | 5 | AI smartness | Per difficulty |

### Balance Targets

**Target 1: Combat Duration**

- **Metric**: Average combat turns
- **Current**: 8-15 turns
- **Target**: 6-20 turns
- **Levers**: AI aggression, healing priorities

**Target 2: Player Death Rate**

- **Metric**: Deaths per X encounters
- **Current**: ~10% at standard difficulty
- **Target**: 5-15% based on difficulty
- **Levers**: Intelligence level, targeting

---

## Implementation Guidance

### Implementation Status

**Current State**: Complete

**Completed**:

- [x]  EnemyAIOrchestrator as central coordinator
- [x]  ThreatAssessmentService
- [x]  TargetSelectionService
- [x]  AbilityPrioritizationService
- [x]  BossAIService with phase mechanics
- [x]  AdaptiveDifficultyService
- [x]  SituationalAnalysisService
- [x]  ChallengeSectorAIService
- [x]  Performance monitoring

### Code Architecture

```
RuneAndRust.Engine/AI/
  ├─ EnemyAIOrchestrator.cs       // Central coordinator
  ├─ ThreatAssessmentService.cs   // Threat calculation
  ├─ TargetSelectionService.cs    // Target selection
  ├─ AbilityPrioritizationService.cs  // Ability scoring
  ├─ BossAIService.cs             // Boss-specific AI
  ├─ AdaptiveDifficultyService.cs // Difficulty scaling
  ├─ SituationalAnalysisService.cs // Context analysis
  ├─ BehaviorPatternService.cs    // Archetype patterns
  ├─ AbilityRotationService.cs    // Rotation management
  ├─ ChallengeSectorAIService.cs  // Sector modifiers
  └─ AIPerformanceMonitor.cs      // Performance tracking

```

---

## Appendix

### Appendix A: Terminology

| Term | Definition |
| --- | --- |
| **Archetype** | AI behavior pattern (Aggressive, Defensive, etc.) |
| **Threat** | Numerical measure of target danger |
| **Intelligence Level** | Difficulty-based AI capability (1-5) |
| **Phase** | Boss encounter stage with distinct behavior |
| **Rotation** | Sequence of abilities used by enemy |

### Appendix B: AI Archetype Summary

| Archetype | Target Priority | Ability Priority | Damage/Defense |
| --- | --- | --- | --- |
| Aggressive | Highest threat | Damage | High/Low |
| Defensive | Attacking ally | Protection | Low/High |
| Support | Wounded ally | Heal/Buff | Low/Medium |
| Opportunistic | Lowest HP | Finishers | Medium/Low |
| Controller | Grouped targets | AoE/CC | Medium/Medium |

---

**End of Specification**