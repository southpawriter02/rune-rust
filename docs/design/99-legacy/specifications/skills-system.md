# Skills System — Core System Specification v5.0

Type: Core System
Priority: Should-Have
Status: Proposed
Balance Validated: No
Document ID: AAM-SPEC-CORESYSTEM-SKILLS-v5.0
Proof-of-Concept Flag: No
Sub-Type: Core
Sub-item: System Bypass — Skill Specification v5.0 (System%20Bypass%20%E2%80%94%20Skill%20Specification%20v5%200%209d93943403ac49feabbf6705c36f40aa.md), Wasteland Survival — Skill Specification v5.0 (Wasteland%20Survival%20%E2%80%94%20Skill%20Specification%20v5%200%206b5089b8d4e746fe987cbc93d8ed606c.md), Rhetoric — Skill Specification v5.0 (Rhetoric%20%E2%80%94%20Skill%20Specification%20v5%200%20bdfbc0bb22c341329d685492d776a4a7.md), Acrobatics — Skill Specification v5.0 (Acrobatics%20%E2%80%94%20Skill%20Specification%20v5%200%2021ea27f8dd97451d83f37c049daccae5.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

## **I. Core Philosophy: The Practical Arts of a Broken World**

The Skills System represents the **non-combat subroutines** a survivor learns to navigate Aethelgard's catastrophically broken reality. While Archetypes define a character's foundational "operating system" and Specializations represent combat expertise, Skills are the quiet, essential arts that keep survivors alive outside of battle.

Skills are not flashy saga-worthy powers—they are the practical tools for debugging locks, exploiting environmental weaknesses, tracking prey through glitched terrain, and persuading faction leaders.

**Design Pillars:**

- **Focused Toolkit:** Only four core skills, ensuring each is meaningful
- **Attribute + Expertise:** Dice Pool = Governing Attribute + Skill Rank
- **Five-Rank Progression:** Novice → Apprentice → Journeyman → Expert → Master
- **Trauma Economy Integration:** Using skills on corrupted objects inflicts Psychic Stress
- **Universal Access:** Any character can invest in any skill
- **Adept's Kingdom:** Adepts excel at skill-based gameplay

---

## **II. System Components**

### **Core Skill Subsystems (4 Skills)**

**1. System Bypass Skill**

- Governing Attribute: WITS
- Applications: Lockpicking, hacking terminals, disarming traps
- Primary Users: Scrap-Tinkers, Gantry-Runners, Jötun-Readers

**2. Acrobatics Skill**

- Governing Attribute: FINESSE
- Applications: Climbing, balancing, leaping chasms, stealth movement
- Primary Users: Strandhöggs, Myrk-gengr, Skirmisher archetypes

**3. Wasteland Survival Skill**

- Governing Attribute: WITS
- Applications: Tracking enemies/prey, foraging resources, navigation
- Primary Users: Veiðimaðr, Myr-Stalkers, Adept archetypes

**4. Rhetoric Skill**

- Governing Attribute: WILL
- Applications: Persuasion, deception, intimidation, social manipulation
- Primary Users: Skalds, Kaupmaðr (Merchants), Thuls (Scholars)

---

## **III. Calculation Formulas**

### **Skill Check Dice Pool**

```jsx
Dice Pool = Governing Attribute + Skill Rank + Situational Modifiers
```

**Value Ranges:**

- Minimum: 1d10 | Typical: 5-10d10 | Maximum: 18d10

---

### **Skill Rank Progression Cost**

| Rank | Title | PP to Attain | Cumulative PP |
| --- | --- | --- | --- |
| 0 | Untrained | 0 | 0 |
| 1 | Novice | 2 | 2 |
| 2 | Apprentice | 3 | 5 |
| 3 | Journeyman | 4 | 9 |
| 4 | Expert | 5 | 14 |
| 5 | Master | 6 | 20 |

---

### **Trauma Economy Integration**

```jsx
Baseline Psychic Stress = Object Corruption Tier × Skill DC Modifier
```

**Stress Costs by Object Type:**

- [Blighted] Object: 5-10 Psychic Stress
- [Glitched] Object: 3-8 Psychic Stress
- [Psychic Resonance] Object: 8-15 Psychic Stress
- Normal Object: 0 Psychic Stress

---

## **IV. Integration Points**

**Primary Dependencies:**

- **Attributes System** — Reads WITS, FINESSE, WILL
- **Dice Pool System** — Uses d10 pool resolution
- **Saga System** — Skills purchased with PP
- **Trauma Economy System** — Skill checks on corrupted objects inflict Stress

**Referenced By:**

- All Adept Specializations
- Dialogue System (Rhetoric checks)
- Puzzle System (System Bypass, Acrobatics)
- Exploration System (Wasteland Survival)

---

## **V. Database Schema Requirements**

### **Skills Table (Static Reference)**

```sql
CREATE TABLE Skills (
  skill_id INT PRIMARY KEY AUTO_INCREMENT,
  skill_name VARCHAR(50) NOT NULL UNIQUE,
  governing_attribute ENUM('WITS', 'FINESSE', 'WILL') NOT NULL,
  description TEXT NOT NULL
);
```

### **Character_Skills Table**

```sql
CREATE TABLE Character_Skills (
  character_skill_id INT PRIMARY KEY AUTO_INCREMENT,
  character_id INT NOT NULL,
  skill_id INT NOT NULL,
  skill_rank INT NOT NULL DEFAULT 0,
  pp_invested INT NOT NULL DEFAULT 0,
  
  UNIQUE KEY uk_character_skill (character_id, skill_id),
  CONSTRAINT chk_skill_rank CHECK (skill_rank >= 0 AND skill_rank <= 5)
);
```

---

## **VI. Service Integration Architecture**

### **Core Service: SkillService**

**Key Methods:**

```jsx
MakeSkillCheck(characterID, skillName, dc, context) → SkillCheckResult
GetCharacterSkills(characterID) → List<CharacterSkill>
UpgradeSkillRank(characterID, skillName, ppCost) → UpgradeResult
```

---

## **VII. UI/Feedback Requirements**

### **Skills Screen**

```
+-----------------------------------------------------------------------------+
| SKILL               | RANK      | GOVERNING ATTR | DICE POOL | PP TO NEXT  |
|=====================|===========|================|===========|=============|
| System Bypass       | 3 (Jour.) | WITS (7)       | 10d10     | 4 PP        |
| Acrobatics          | 2 (Appr.) | FINESSE (6)    | 8d10      | 4 PP        |
| Wasteland Survival  | 1 (Nov.)  | WITS (7)       | 8d10      | 3 PP        |
| Rhetoric            | 0 (Untr.) | WILL (5)       | 5d10      | 2 PP        |
+-----------------------------------------------------------------------------+
```

### **Skill Check Feedback**

- **Success:** "SUCCESS! The lock clicks open."
- **Failure:** "FAILURE. The mechanism resists. +8 Psychic Stress"
- **Fumble:** "CRITICAL FAILURE! Alarm triggered! +12 Psychic Stress"

---

## **VIII. Design Philosophy**

### **Balance Philosophy**

**Universal Access, Specialized Mastery:** Any archetype can invest, but Adepts get specialization abilities that amplify skills

**Four Skills, Deep Investment:** Focused list ensures each skill has rich applications. 20 PP to Master one skill is significant.

**Non-Combat Progression:** Skills provide meaningful progression outside combat

### **Design Trade-offs**

**Why Only Four Skills:** Prevents analysis paralysis, each covers broad category

**Why Attribute + Rank:** Attributes matter (innate talent counts), prevents dump stat builds

**Why Escalating PP Costs:** Early ranks accessible, later ranks reward specialization

---

*Parent specification complete. Child skill specifications (System Bypass, Acrobatics, Wasteland Survival, Rhetoric) to follow.*