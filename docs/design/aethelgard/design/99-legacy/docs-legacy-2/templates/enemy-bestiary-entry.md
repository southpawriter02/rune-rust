# Enemy Bestiary Entry Template

## [Enemy Name]

**Enemy ID:** `[enemy_id]`
**Type:** [Draugr-Pattern / Symbiotic Plate / Jötun-Reader / Haugbui-Class / Forlorn]
**Threat Level:** [Trivial / Low / Medium / High / Lethal / Boss]
**Category:** [Melee / Ranged / Caster / Support / Elite]

---

### Base Stats

| Stat | Value | Notes |
|------|-------|-------|
| **HP** | [X] | [Base value at Legend 1] |
| **Defense Score** | [X] | [Base defense] |
| **Soak** | [X] | [Flat damage reduction] |
| **Initiative** | [X] | [Turn order bonus] |
| **Movement** | [X] | [Squares per turn] |

---

### Attributes

| Attribute | Value | Effect |
|-----------|-------|--------|
| **MIGHT** | [1-10] | Melee damage, physical resistance |
| **FINESSE** | [1-10] | Ranged damage, initiative, evasion |
| **WITS** | [1-10] | Detection, tactical abilities |
| **WILL** | [1-10] | Magic damage/resist, Stress resist |
| **STURDINESS** | [1-10] | HP pool, Soak bonus |

---

### Resistances & Vulnerabilities

**Resistances:**
| Damage Type | Resistance % | Notes |
|-------------|--------------|-------|
| Physical | [X%] | [Why resistant] |
| Fire | [X%] | [Why resistant] |
| Lightning | [X%] | [Why resistant] |
| Arcane | [X%] | [Why resistant] |

**Vulnerabilities:**
| Damage Type | Vulnerability % | Notes |
|-------------|-----------------|-------|
| Fire | [-X%] | [Why vulnerable] |
| [Corroded] | [+X%] | [Why vulnerable] |

**Immunities:**
- [Status Effect 1]: [Why immune]
- [Status Effect 2]: [Why immune]

---

### Abilities

#### Ability 1: [Ability Name]
**Type:** [Active / Passive / Reaction]
**Cooldown:** [X turns / None]
**Target:** [Single / Area / Self]

**Effect:**
[Detailed description of what the ability does]

**Damage/Healing:** [XdY + Z of [Type]]
**Status Effects:** [List effects with durations]
**Special:** [Any special mechanics]

---

#### Ability 2: [Ability Name]
[Same structure as Ability 1]

---

#### Ability 3: [Ability Name]
[Same structure as Ability 1]

---

### AI Behavior Pattern

**Priority System:**
1. [First priority action/condition]
2. [Second priority action/condition]
3. [Third priority action/condition]
4. [Default behavior]

**Targeting Logic:**
- Primary: [What it targets first]
- Secondary: [What it targets if primary unavailable]
- Avoids: [What it avoids targeting]

**Tactical Behavior:**
- [Behavior 1]: [When and why]
- [Behavior 2]: [When and why]
- [Behavior 3]: [When and why]

**Flee Condition:** [HP threshold or condition that triggers flee]

---

### Loot Table

| Item | Drop Rate | Quantity | Quality |
|------|-----------|----------|---------|
| [Item 1] | [X%] | [Y-Z] | [Tier] |
| [Item 2] | [X%] | [Y-Z] | [Tier] |
| [Item 3] | [X%] | [Y-Z] | [Tier] |

**Guaranteed Drops:**
- [Item X] (100% drop rate)

**Rare Drops:**
- [Item Y] (5% drop rate) - [Special condition]

**XP Reward:** [X XP base]

---

### Trauma Impact

**Stress Infliction:**
- On Encounter: [+X Stress]
- Proximity (per turn): [+Y Stress if within Z rows]
- On Attack: [+A Stress]
- On Death: [+B Stress to killer]

**Corruption Infliction:**
- On Encounter: [+X Corruption]
- Special Attacks: [+Y Corruption if hit by ability]
- On Death: [+Z Corruption if condition met]

---

### Scaling Formula

**HP Scaling:**
```
Final_HP = Base_HP + (Player_Legend × Scaling_Factor)
```
- Base_HP: [X]
- Scaling_Factor: [Y]
- Example at Legend 5: [Final value]

**Damage Scaling:**
```
Final_Damage = Base_Damage + (Player_Legend × Damage_Dice)
```
- Base_Damage: [XdY+Z]
- Scaling: [+1dY per Legend level]

**Defense Scaling:**
```
Final_Defense = Base_Defense + (Player_Legend ÷ 3)
```

---

### Encounter Design

**Preferred Group Composition:**
- [X] × [This Enemy]
- [Y] × [Supporting Enemy Type]
- [Z] × [Optional Enemy Type]

**Room Archetypes:**
- Works well in: [Room type 1, Room type 2]
- Avoid in: [Room type 3]

**Difficulty Curve:**
- First Encounter: Legend [X]
- Recommended: Legend [Y-Z]
- Last Encounter: Legend [A]

**Tactics Against:**
- Counter 1: [Strategy]
- Counter 2: [Strategy]
- Weakness: [Exploitable weakness]

---

### Balance Data

**Time to Kill (Average):**
- Solo Player: [X turns]
- 2-Player Party: [Y turns]
- Full Party: [Z turns]

**Threat Rating:**
- Damage Output: [X/10]
- Survivability: [Y/10]
- Control: [Z/10]
- Overall: [A/10]

**Balance Notes:**
[Any special considerations for balance]

---

### Lore & Description

**In-Game Description:**
> [Player-facing description seen on first encounter]

**Flavor Text:**
> [Atmospheric lore text]

**Layer 2 Analysis:** (Optional)
> [Technical/diagnostic description in Layer 2 voice]

**v5.0 Background:**
[How this enemy fits into Aethelgard lore - origin, purpose, decay state]

---

### Technical Implementation

**File:** `RuneAndRust.Core/Enemy.cs`
**Enum Value:** `EnemyType.[EnumName]`
**Factory:** `RuneAndRust.Engine/EnemyFactory.cs`
**Method:** `Create[EnemyName]()`

**Database Reference:**
```sql
-- If stored in database
SELECT * FROM Enemies WHERE enemy_id = [enemy_id];
```

**Code Example:**
```csharp
public static Enemy Create[EnemyName]()
{
    return new Enemy
    {
        Name = "[Enemy Name]",
        // ... properties
    };
}
```

---

### Visual Design (ASCII Art)

```
[ASCII art representation if applicable]
```

---

### Testing Notes

**Test Coverage:**
- [ ] AI behavior tested
- [ ] Scaling formula verified
- [ ] Loot table functional
- [ ] Abilities work correctly
- [ ] Trauma infliction accurate

**Known Issues:**
- Issue 1: [Description]
- Issue 2: [Description]

---

### Changelog

**v0.X - [Date]**
- Initial implementation
- HP: [value], Damage: [value]

**v0.X+1 - [Date]**
- Balance adjustment: [What changed]
- Bug fix: [What was fixed]

---

**Last Updated:** [Date]
**Status:** ✅ Complete | ⏳ In Progress | ❌ Needs Review
