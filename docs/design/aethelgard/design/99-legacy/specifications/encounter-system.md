# Encounter System — Core System Specification v5.0

Type: Core System
Priority: Must-Have
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-CORESYSTEM-ENCOUNTER-v5.0
Proof-of-Concept Flag: No
Sub-Type: Core
Sub-item: Turn System — Mechanic Specification v5.0 (Turn%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20dcaa69e421ef4aeeb822b6f011f47ad2.md), Action System — Mechanic Specification v5.0 (Action%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20412467feaecb4834a2a61b34a6f0f708.md), Dynamic Scaling System — Mechanic Specification v5.0 (Dynamic%20Scaling%20System%20%E2%80%94%20Mechanic%20Specification%20v5%2037c45a37627f474698b51fcfc1361317.md), Loot System — Mechanic Specification v5.0 (Loot%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20b663b28922864715aa6a0134b27699e8.md), Enemy AI Behavior System — Mechanic Specification v5.0 (Enemy%20AI%20Behavior%20System%20%E2%80%94%20Mechanic%20Specification%20%20f6ba1ce138074563953d5b247c7058e7.md), Boss Encounter System — Mechanic Specification v5.0 (Boss%20Encounter%20System%20%E2%80%94%20Mechanic%20Specification%20v5%20%20ef8421ced2a44221bc2caac52867cb29.md), Enemy Design & Bestiary System — Mechanic Specification v5.0 (Enemy%20Design%20&%20Bestiary%20System%20%E2%80%94%20Mechanic%20Specific%201e6f5c6f47e84b37bce5c6e91f9a182e.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

## **I. Core Philosophy: Orchestrated Confrontation**

The Encounter System is the **parent orchestrator for all hostile confrontations** in Aethelgard. It manages combat initiation, enemy spawning, battlefield setup, and encounter resolution. An encounter is a structured confrontation between coherent processes (the party) and corrupted processes (enemies).

This system transforms exploration into combat seamlessly, manages enemy AI coordination, and determines rewards upon victory.

**Design Pillars:**

- **Seamless Transition:** Exploration → Combat state transition
- **Dynamic Spawning:** Enemy composition based on location and TDR
- **Battlefield Setup:** Grid positioning, hazards, cover initialization
- **Victory/Defeat Resolution:** Loot generation, Legend awards, death handling

---

## **II. System Components**

### **Child Systems & Mechanics**

**Combat Initiation:**

- Trigger detection (hostile room entry, ambush, dialogue failure)
- State transition (Exploration → Combat)
- Enemy spawn and placement

**Turn System:**

- Initiative calculation (Vigilance = FINESSE + WITS)
- Static turn order generation
- Turn/round sequencing
- *See: Turn System — Mechanic Specification v5.0*

**Action System:**

- Standard/Free/Reaction action economy
- Action validation and consumption
- Turn advancement
- *See: Action System — Mechanic Specification v5.0*

**Combat Resolution:**

- Victory conditions (all enemies defeated)
- Defeat conditions (all players in System Crashing state)
- Flee mechanics

**Loot System:**

- Post-combat loot generation
- Quality tier determination
- *See: Loot System — Mechanic Specification v5.0*

**Dynamic Scaling:**

- Enemy stat scaling based on party Power Score
- TDR (Target Difficulty Rating) calculation
- *See: Dynamic Scaling System — Mechanic Specification v5.0*

---

## **III. Combat Initiation**

### **A. Triggers**

**1. Hostile Room Entry**

```jsx
if (room.HasHostileEntities && !room.HostilesCleared) {
  EncounterService.InitiateCombat(room.EncounterID);
}
```

**2. Ambush During Rest**

```jsx
if (AttemptingRestAtCampsite && RollAmbushChance()) {
  EncounterService.InitiateAmbush(party, location);
}
```

**3. Dialogue Failure**

```jsx
if (RhetoricCheck.Failed && NPC.CanBecomeHostile) {
  EncounterService.InitiateCombatFromDialogue(npcID, party);
}
```

**4. Glitched Remnant Recovery**

```jsx
if (RecoveringRemnant && Roll(1,100) <= 60) {
  EncounterService.InitiateForlornAmbush(remnant.Location);
}
```

---

### **B. Initialization Sequence**

```jsx
1. GameEngine.SetState(CombatState)
2. EncounterService.LoadEncounter(encounterID)
3. EncounterService.SpawnEnemies(enemyList, placements)
4. EncounterService.SetupBattlefield(grid, hazards, cover)
5. TurnSystemService.CalculateVigilance(allCombatants)
6. TurnSystemService.GenerateTurnOrder(combatID)
7. ActionEconomyService.InitializeActions(allCombatants)
8. UIService.SwitchToCombatLayout(turnOrder)
9. LogMessage("> Hostile processes detected! Combat initiated! <")
10. TurnSystemService.StartCombat(combatID)
```

---

## **IV. Battlefield Setup**

### **A. Combat Grid**

```
[PLAYER ZONE]          [ENEMY ZONE]
Back Row  Front Row | Front Row  Back Row
   P1       P2      |    E1        E2
   P3       P4      |    E3        E4
```

**Positioning Rules:**

- **Front Row:** Melee range, vulnerable to melee attacks
- **Back Row:** Safe from melee (unless enemies close gap), ranged attacks only
- **Polearms:** Can attack from Back Row (Reach property)

### **B. Environmental Hazards**

**Static Terrain:**

- Cover (provides +2 Defense vs ranged)
- Elevation (high ground grants +1d10 accuracy)
- Chokepoints (limits enemy advance)

**Dynamic Hazards:**

- [Burning Ground]: 2d10 fire damage at End of Turn
- [Toxic Haze]: 1d10 poison damage at Start of Turn, -1d10 accuracy
- [Electrified Floor]: 3d10 lightning damage on movement
- [Psychic Resonance]: Accumulates Psychic Stress per turn

---

## **V. Combat Loop**

### **A. Main Loop Structure**

```csharp
while (IsCombatActive(combatID)) {
  combatant = TurnSystem.GetCurrentCombatant(combatID);
  
  TurnSystem.ProcessStartOfTurnPhase(combatant);
  TurnSystem.ProcessActionPhase(combatant);
  TurnSystem.ProcessEndOfTurnPhase(combatant);
  
  if (AllEnemiesDead(combatID)) {
    ResolveVictory(combatID);
    break;
  }
  
  if (AllPlayersDown(combatID)) {
    ResolveDefeat(combatID);
    break;
  }
  
  TurnSystem.AdvanceToNextTurn(combatID);
}
```

---

## **VI. Combat Resolution**

### **A. Victory Conditions**

**Condition:** All enemy entities reduced to 0 HP

**Resolution Sequence:**

1. End combat loop
2. Calculate Legend award (based on TDR)
3. Generate loot (based on enemy types + quality tiers)
4. Restore partial resources (50% Stamina)
5. Mark room as cleared (HostilesCleared = true)
6. Transition back to ExplorationState
7. Display victory message + rewards summary

**Legend Award Formula:**

```jsx
LegendAwarded = BaseEncounterLegend × TDRMultiplier

TDR Multipliers:
- Trivial (TDR < PowerScore - 2): 0× (no Legend)
- Easy (TDR = PowerScore - 1): 0.5×
- Standard (TDR = PowerScore): 1.0×
- Difficult (TDR = PowerScore + 1): 1.5×
- Deadly (TDR = PowerScore + 2): 2.0×
```

---

### **B. Defeat Conditions**

**Condition:** All player characters in [System Crashing] state (0 HP)

**Resolution Sequence:**

1. End combat loop
2. For each player character: InitiateResurrection()
3. Transition to ExplorationState at Sanctuary
4. Display death/resurrection narrative

**See:** Death & Resurrection System v5.0 for full details

---

### **C. Flee Mechanics**

**Requirements:**

- At least one player character must have Standard Action available

**Resolution:**

```jsx
partyVigilance = Average(party.Vigilance);
enemyVigilance = Average(enemies.Vigilance);
dicePool = partyVigilance;
DC = 10 + enemyVigilance;

if (RollCheck(dicePool, DC)) {
  // Success: Party escapes
} else {
  // Failure: All party members lose Standard Action
}
```

---

## **VII. Database Schema**

### **Encounters Table**

```sql
CREATE TABLE Encounters (
  encounter_id INTEGER PRIMARY KEY,
  room_id INTEGER NOT NULL,
  encounter_type TEXT NOT NULL,
  enemy_composition TEXT NOT NULL,
  base_legend_reward INTEGER,
  tdr_rating INTEGER,
  is_cleared BOOLEAN DEFAULT FALSE,
  battlefield_config TEXT,
  FOREIGN KEY (room_id) REFERENCES Rooms(room_id)
);
```

### **Combat_Instances Table**

```sql
CREATE TABLE Combat_Instances (
  combat_id INTEGER PRIMARY KEY,
  encounter_id INTEGER NOT NULL,
  round_number INTEGER DEFAULT 1,
  current_turn_index INTEGER DEFAULT 0,
  is_active BOOLEAN DEFAULT TRUE,
  outcome TEXT,
  FOREIGN KEY (encounter_id) REFERENCES Encounters(encounter_id)
);
```

---

## **VIII. Service Architecture**

### **EncounterService**

```csharp
public class EncounterService
{
  public int InitiateCombat(int encounterID)
  public int InitiateAmbush(Party party, Location location)
  public void SetupBattlefield(int combatID, BattlefieldConfig config)
  public void SpawnEnemies(int combatID, EnemyComposition composition)
  public void ResolveVictory(int combatID)
  public void ResolveDefeat(int combatID)
  public bool AttemptFlee(int combatID)
}
```

---

## **IX. Integration Points**

**Primary Dependencies:**

- Turn System — Manages turn order and sequencing
- Action System — Tracks action economy per combatant
- Enemy Database — Spawns enemy stat blocks
- Dynamic Scaling System — Scales enemy stats to party level

**Referenced By:**

- All Combat Mechanics (accuracy, damage, status effects)
- Exploration System (triggers combat on room entry)
- Death & Resurrection System (handles defeat)
- Loot System (generates post-combat rewards)
- Legend System (awards progression points)

**Modifies:**

- Game State — ExplorationState → CombatState transition
- Room State — Marks rooms as cleared
- Character Resources — HP/Stamina/AP changes during combat

---

## **X. Design Philosophy**

**Encounter Pacing:**

- Standard encounters every 2-3 rooms
- Boss encounters every 5-8 rooms or end of region
- Ambush encounters 10-20% chance when resting at campsites

**Difficulty Curve:**

- Early game: TDR = PowerScore (fair fights)
- Mid game: TDR = PowerScore + 1 (challenging)
- Late game: Mix of TDR = PowerScore (grinding) and +2 (bosses)

**Victory Rewards:**

- Legend awards encourage combat engagement
- Loot quality scales with TDR
- Trivial encounters grant no Legend (anti-grinding mechanic)

---

*This specification follows the v5.0 Three-Tier Template standard. The Encounter System orchestrates all hostile confrontations from initiation to resolution.*