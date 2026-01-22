# Enemy AI Behavior System — Mechanic Specification v5.0

Type: Mechanic
Description: Intelligent enemy decision-making through archetype-based behavior patterns, threat assessment, ability prioritization, adaptive difficulty scaling, and boss AI with phase mechanics.
Priority: Must-Have
Status: Review
Target Version: Alpha
Dependencies: Encounter System, Combat Resolution System, Character System, Ability System
Implementation Difficulty: Very Complex
Balance Validated: No
Document ID: AAM-SPEC-MECH-ENEMYAI-v5.0
Parent item: Encounter System — Core System Specification v5.0 (Encounter%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20c82c42d7129c4843a86f2e69cd72f0d7.md)
Proof-of-Concept Flag: No
Related Projects: (PROJECT) Game Specification Consolidation & Standardization (https://www.notion.so/PROJECT-Game-Specification-Consolidation-Standardization-e1d0c8b2ea2042f9b9c08471c6077c92?pvs=21)
Session Handoffs: Consolidation Work — Phase 1A Core Systems Audit Complete (https://www.notion.so/Consolidation-Work-Phase-1A-Core-Systems-Audit-Complete-0c51f0058f3a478fb7bc6a2c192cac2a?pvs=21)
Sub-Type: Combat
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Layer: Layer 3 (Technical)
Voice Validated: No

## **I. Core Philosophy: Intelligent Opposition**

The Enemy AI Behavior System provides **intelligent decision-making for all hostile entities** in Aethelgard. It drives enemy actions through archetype-based behavior patterns, threat assessment, ability prioritization, and adaptive difficulty scaling.

**Design Pillars:**

- **Archetype-Driven Behavior:** Different enemy types fight differently (Aggressive rush, Defensive protect)
- **Intelligent but Readable:** Players can understand and predict enemy behavior
- **Scalable Difficulty:** AI adapts to player skill level via intelligence tiers

---

## **II. System Components**

### **Service Architecture**

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

### **Decision Pipeline**

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

## **III. AI Archetype System**

Each enemy has an AI archetype that influences all behavior decisions.

### **Archetype Definitions**

| Archetype | Description | Behavior Pattern |
| --- | --- | --- |
| **Aggressive** | Deals maximum damage | Attack highest threat, use damage abilities |
| **Defensive** | Protects self/allies | Prioritize defensive abilities, intercept attacks |
| **Support** | Buffs allies, debuffs enemies | Heal wounded, buff damage dealers |
| **Opportunistic** | Exploits weaknesses | Target low HP, use status effects |
| **Controller** | Manipulates battlefield | AoE abilities, crowd control |
| **Berserker** | Reckless damage | All-out attack, ignore defense |
| **Cautious** | Preserves resources | Efficient ability usage, retreat if wounded |

### **Target Selection by Archetype**

| Archetype | Primary Target | Secondary Target |
| --- | --- | --- |
| Aggressive | Highest threat | Lowest HP |
| Defensive | Nearest enemy | Attacking ally |
| Support | Wounded ally | Buffable ally |
| Opportunistic | Lowest HP enemy | Most vulnerable |
| Controller | Most threatening | Grouped enemies |

### **Archetype Configuration Example**

```
Aggressive:
  DamageAbilityModifier: 1.3
  UtilityAbilityModifier: 0.8
  DefensiveAbilityModifier: 0.6
  ThreatTargetWeight: 1.0
  VulnerableTargetWeight: 0.7
```

---

## **IV. Threat Assessment**

### **Threat Score Formula**

```
ThreatScore = BaseThreat × DamageModifier × HealthModifier × PositionModifier × BuffModifier

Components:
  BaseThreat = Character class base (Warrior: 1.2, Adept: 0.9, etc.)
  DamageModifier = RecentDamageDealt / AverageDamage
  HealthModifier = CurrentHP / MaxHP (low = lower threat)
  PositionModifier = Front row = 1.2, Back row = 0.8
  BuffModifier = Active buffs increase threat
```

**Worked Example:**

```
Warrior in front row, dealt 30 damage, at 80% HP
  BaseThreat = 1.2
  DamageModifier = 1.5 (above average damage)
  HealthModifier = 0.8
  PositionModifier = 1.2 (front row)
  BuffModifier = 1.0 (no buffs)

  ThreatScore = 1.2 × 1.5 × 0.8 × 1.2 × 1.0 = 1.73
```

---

## **V. Ability Prioritization**

### **Scoring Formula**

```
TotalScore = (Damage × 0.4) + (Utility × 0.3) + (Efficiency × 0.2) + (Situation × 0.1)
TotalScore *= ArchetypeModifier

Components:
  Damage: Expected damage output
  Utility: Status effects, positioning value
  Efficiency: Resource cost vs benefit
  Situation: Contextual appropriateness
```

**Worked Example:**

```
Ability: "Stunning Strike" for Aggressive enemy
  Damage = 60 (moderate)
  Utility = 80 (stun effect)
  Efficiency = 70 (medium cost)
  Situation = 90 (target is casting)
  ArchetypeModifier = 0.9 (Aggressive prefers damage)

  Score = (60×0.4 + 80×0.3 + 70×0.2 + 90×0.1) × 0.9
        = (24 + 24 + 14 + 9) × 0.9 = 63.9
```

---

## **VI. Adaptive Difficulty**

### **Intelligence Levels**

| Level | Description | Behaviors |
| --- | --- | --- |
| 1 | Basic | Random targeting, basic abilities |
| 2 | Standard | Threat-based targeting, rotation |
| 3 | Advanced | Synergy awareness, cooldown tracking |
| 4 | Expert | Optimal targeting, ability combos |
| 5 | Master | Predictive, counter-play |

---

## **VII. Situational Analysis**

**Factors Evaluated:**

- Ally HP states (any critical?)
- Player buffs/debuffs
- Terrain features (cover, hazards)
- Turn order position
- Resource states (stamina, abilities on cooldown)

---

## **VIII. Performance Requirements**

| Metric | Target | Warning | Critical |
| --- | --- | --- | --- |
| Per-enemy decision | <100ms | >150ms | >300ms |
| Per-turn total | <500ms | >750ms | >1000ms |
| Boss decision | <200ms | >300ms | >500ms |

---

## **IX. State Management**

| Variable | Type | Persistence | Default | Description |
| --- | --- | --- | --- | --- |
| IntelligenceLevel | int | Session | 3 | Current AI difficulty |
| BossPhase | int | Combat | 1 | Current boss phase |
| ThreatTable | Dict | Combat | {} | Threat scores per character |
| AbilityCooldowns | Dict | Combat | {} | Enemy ability cooldowns |

---

## **X. Balance & Tuning**

### **Tunable Parameters**

| Parameter | Location | Current | Min | Max | Impact |
| --- | --- | --- | --- | --- | --- |
| ThreatDecayRate | ThreatAssessment | 0.1/turn | 0 | 0.5 | Threat memory |
| ArchetypeModifiers | Config | varies | 0.5 | 1.5 | Behavior emphasis |
| IntelligenceLevel | Difficulty | 1-5 | 1 | 5 | AI smartness |

### **Balance Targets**

- **Combat Duration:** 6-20 turns (target), 8-15 turns (current)
- **Player Death Rate:** 5-15% based on difficulty (~10% at standard)

---

## **XI. Integration Points**

**Dependencies:**

- Combat Resolution System → Combat state, actions
- Character System → Player stats for threat assessment
- Ability System → Enemy abilities

**Depended Upon By:**

- Combat Engine → Invokes AI for enemy turns
- Boss Encounter System → Uses boss AI subsystem
- Encounter System → Composes enemy groups

---

*This specification follows the v5.0 Three-Tier Template standard. The Enemy AI Behavior System drives intelligent enemy decision-making through archetype patterns and adaptive difficulty.*