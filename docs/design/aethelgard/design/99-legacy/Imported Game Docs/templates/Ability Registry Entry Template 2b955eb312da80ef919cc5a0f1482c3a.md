# Ability Registry Entry Template

Parent item: Template (Template%202ba55eb312da80a8b0f3fbfdcd27220c.md)

## [Ability Name]

**Ability ID:** `[ability_id]`**Specialization:** [Warrior Spec / Adept Spec / Universal]
**Tier:** [1 / 2 / 3 / Capstone]
**Type:** [Active / Passive]
**Category:** [Damage / Heal / Buff / Debuff / Control / Utility]

---

### Basic Information

| Property | Value |
| --- | --- |
| **Unlock Requirement** | [PP cost / Level requirement / Prerequisite ability] |
| **Maximum Rank** | [1-3] |
| **Cooldown** | [X turns / None] |
| **Range** | [Melee / Short / Long / Self / Ally] |
| **Target Type** | [Single / Area / Self / All Allies / All Enemies] |

---

### Resource Costs

| Resource | Cost | Notes |
| --- | --- | --- |
| **Stamina** | [X points] | [Per use / Per turn] |
| **Stress** | [+X / -X] | [Immediate / Over time] |
| **Corruption** | [+X / -X] | [Immediate / Per use] |
| **Aether** | [X points] | [If Adept ability] |

---

### Effect (Rank 1)

**Base Effect:**
[Detailed description of what the ability does at Rank 1]

**Damage/Healing:**

- Base: [XdY + Z]
- Scaling: [+ ATTRIBUTE × multiplier]
- Type: [Physical / Arcane / Fire / Lightning / etc.]

**Duration:** [X turns / Instant / Permanent]

**Status Effects Applied:**

- [Status Effect 1]: [Duration, stacks, conditions]
- [Status Effect 2]: [Duration, stacks, conditions]

**Special Properties:**

- Property 1: [Description with exact values]
- Property 2: [Description with exact values]

---

### Rank Progression

### Rank 2 (Unlock: [Requirement])

**Changes from Rank 1:**

- Change 1: [Exact modification]
- Change 2: [Exact modification]

**Updated Effect:**
[Full description of Rank 2 effect with all values]

### Rank 3 (Unlock: [Requirement])

**Changes from Rank 2:**

- Change 1: [Exact modification]
- Change 2: [Exact modification]

**Updated Effect:**
[Full description of Rank 3 effect with all values]

---

### Synergies

**Enhances:**

- [Ability A]: [How they synergize]
- [Ability B]: [How they synergize]

**Enhanced By:**

- [Ability C]: [How they synergize]
- [Equipment D]: [How they synergize]

**Combos:**

- [Ability E] → [This Ability] → [Ability F]: [Combo effect]

**Anti-Synergies:**

- [Ability G]: [Why they conflict]

---

### Balance Data

**Power Budget:** [Low / Medium / High / Ultimate]

**Effectiveness by Target:**

| Target Type | Effectiveness | Notes |
| --- | --- | --- |
| Draugr-Pattern | [% modifier] | [Why] |
| Symbiotic Plate | [% modifier] | [Why] |
| Jötun-Reader | [% modifier] | [Why] |
| Mechanical | [% modifier] | [Why] |
| Forlorn | [% modifier] | [Why] |

**DPS Analysis (at Legend 5):**

- Single Target: [X damage/turn]
- Area: [Y total damage/turn]
- Sustained: [Z damage over N turns]

**Resource Efficiency:**

- Damage per Stamina: [X]
- Healing per Stamina: [Y]
- Value rating: [Low / Medium / High]

---

### Usage Notes

**Best Used For:**

- Scenario 1: [When to use this ability]
- Scenario 2: [When to use this ability]

**Avoid Using When:**

- Scenario 1: [When NOT to use this ability]
- Scenario 2: [When NOT to use this ability]

**Build Recommendations:**

- Archetype 1: [How this ability fits]
- Archetype 2: [How this ability fits]

---

### Technical Implementation

**File:** `RuneAndRust.Engine/[FileName].cs`**Class:** `[ClassName]`**Method:** `[MethodName]()`

**Database Reference:**

```sql
SELECT * FROM Abilities WHERE ability_id = [ability_id];

```

**Code Example:**

```csharp
// Example of ability implementation
public void [MethodName](Character caster, Character target)
{
    // Implementation summary
}

```

---

### Lore & Description

**In-Game Description:**

> [Player-facing ability description]
> 

**Flavor Text:**

> [Atmospheric lore text]
> 

**v5.0 Compliance:**
[How this ability fits into Aethelgard setting - tech, fungal AI, or heretical]

---

### Changelog

**v0.X - [Date]**

- Initial implementation
- Damage: [values]
- Cost: [values]

**v0.X+1 - [Date]**

- Balance adjustment: [What changed]
- Bug fix: [What was fixed]

---

**Last Updated:** [Date]
**Status:** ✅ Complete | ⏳ In Progress | ❌ Needs Review