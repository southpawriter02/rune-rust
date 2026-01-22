# Equipment Registry Entry Template

Parent item: Template (Template%202ba55eb312da80a8b0f3fbfdcd27220c.md)

## [Equipment Name]

**Item ID:** `[item_id]`**Type:** [Weapon / Armor / Accessory]
**Subtype:** [Sword / Axe / Bow / Head / Chest / Hands / Ring / etc.]
**Quality Tier:** [Jury-Rigged / Scavenged / Clan-Forged / Optimized / Masterwork]

---

### Basic Information

| Property | Value |
| --- | --- |
| **Equipment Slot** | [MainHand / OffHand / Head / Chest / Hands / Legs / Feet / Accessory] |
| **Weight Class** | [Light / Medium / Heavy] (Armor only) |
| **Hands Required** | [1 / 2] (Weapons only) |
| **Proficiency** | [Warrior / Adept / Universal] |
| **Rarity** | [Common / Uncommon / Rare / Epic / Legendary] |

---

### Stats (Weapons)

**Damage:**

- Base Damage: [XdY]
- Damage Type: [Physical / Fire / Lightning / Arcane / Mixed]
- Bonus Damage: [+Z]

**Range:** [Melee / Short (2-3 rows) / Long (4-6 rows)]

**Attack Speed:** [Fast / Normal / Slow]

- Stamina Cost: [X per attack]
- Initiative Modifier: [+/-X]

**Special Properties:**

- Property 1: [Description with exact values]
- Property 2: [Description with exact values]

---

### Stats (Armor)

**Defense:**

- Soak: [X] (Flat damage reduction)
- Defense Bonus: [+Y to Defense Score]
- Evasion: [+/-Z%]

**Resistances:**

| Damage Type | Resistance |
| --- | --- |
| Physical | [+X%] |
| Fire | [+X%] |
| Lightning | [+X%] |

**Movement:**

- Speed Penalty: [-X squares] (Heavy armor)
- Stamina Drain: [+Y per turn] (Heavy armor)

**Special Properties:**

- Property 1: [Description with exact values]
- Property 2: [Description with exact values]

---

### Stats (Accessory)

**Attribute Bonuses:**

| Attribute | Bonus |
| --- | --- |
| MIGHT | [+X] |
| FINESSE | [+X] |
| WITS | [+X] |
| WILL | [+X] |
| STURDINESS | [+X] |

**Resource Bonuses:**

| Resource | Bonus |
| --- | --- |
| Max HP | [+X] |
| Max Stamina | [+X] |
| Stress Resistance | [+X%] |
| Corruption Resistance | [+X%] |

**Special Properties:**

- Property 1: [Description with exact values]
- Property 2: [Description with exact values]

---

### Requirements

**Level Requirement:** [Legend X]

**Attribute Requirements:**

- MIGHT: [X+]
- FINESSE: [X+]
- Other: [X+]

**Corruption Cost:**

- Equip: [+X Corruption] (if heretical)
- Per Rest: [+Y Corruption] (ongoing cost)

---

### Acquisition

**Sources:**

- Loot Drop: [Enemy Type] ([X%] drop rate)
- Quest Reward: [Quest Name]
- Crafting: [Recipe / Components]
- Vendor: [Location] ([X] currency)
- Guaranteed: [Room / Event]

**First Available:** Legend [X]

**Upgrade Path:**

- Upgrades from: [Previous tier equipment]
- Upgrades to: [Next tier equipment]

---

### Quality Tier Modifiers

**Base Stats:** [As shown above]

**Masterwork Bonus (if Masterwork):**

- 
- [Bonus 2]: [Additional property]
- [Special effect]: [Unique effect]

**Durability:**

- Max Durability: [X] (if system exists)
- Repair Cost: [Y] (if system exists)

---

### Synergies

**Works Well With:**

- [Ability A]: [How they synergize]
- [Equipment B]: [Set bonus or combo effect]
- [Specialization C]: [Why this spec benefits]

**Enhanced By:**

- [Buff D]: [How enhancement works]
- [Passive E]: [How enhancement works]

**Build Recommendations:**

- Archetype 1: [Why this equipment fits]
- Archetype 2: [Why this equipment fits]

---

### Balance Data

**Power Level:** [Tier appropriate / Overtuned / Undertuned]

**Effectiveness by Enemy Type:**

| Enemy Type | Effectiveness | Notes |
| --- | --- | --- |
| Draugr-Pattern | [Rating] | [Why] |
| Symbiotic Plate | [Rating] | [Why] |
| Mechanical | [Rating] | [Special interaction] |

**DPS Analysis (Weapons):**

- Burst Damage: [X per hit]
- Sustained DPS: [Y per turn]
- Stamina Efficiency: [Damage per Stamina]

**Survivability Analysis (Armor):**

- Effective HP Increase: [+X%]
- Damage Mitigation: [Y%]
- Trade-offs: [Drawbacks]

**Value Rating:**

- Early Game: [X/10]
- Mid Game: [Y/10]
- Late Game: [Z/10]

---

### Comparison

**vs [Similar Equipment 1]:**

- Pro: [Advantage]
- Con: [Disadvantage]
- Best for: [Use case]

**vs [Similar Equipment 2]:**

- Pro: [Advantage]
- Con: [Disadvantage]
- Best for: [Use case]

---

### Usage Notes

**Best Used By:**

- Specialization 1: [Why]
- Specialization 2: [Why]

**Best Paired With:**

- Equipment slot 1: [Recommendation]
- Equipment slot 2: [Recommendation]

**Situational Uses:**

- Scenario 1: [When this equipment excels]
- Scenario 2: [When this equipment excels]

**Avoid Using When:**

- Scenario 1: [When to avoid]
- Scenario 2: [When to avoid]

---

### Lore & Description

**In-Game Description:**

> [Player-facing item description]
> 

**Flavor Text:**

> [Atmospheric lore text]
> 

**Examination Text:** (Extended description)

> [Longer description revealed on closer inspection]
> 

**v5.0 Background:**
[How this equipment fits into Aethelgard lore - origin, manufacturer, decay state]

**Pre-Glitch Context:** (if applicable)
[What this item was before The Glitch]

---

### Technical Implementation

**File:** `RuneAndRust.Core/Equipment.cs` or `RuneAndRust.Engine/EquipmentDatabase.cs`**Method:** `[MethodName]()`

**Database Reference:**

```sql
SELECT * FROM Equipment WHERE item_id = [item_id];

```

**Code Example:**

```csharp
public static Equipment Create[EquipmentName]()
{
    return new Equipment
    {
        Name = "[Equipment Name]",
        Type = EquipmentType.[Type],
        Quality = QualityTier.[Tier],
        // ... properties
    };
}

```

---

### Visual Design

**ASCII Icon:**

```
[Icon if applicable]

```

**Description Visual Cues:**

- Color: [If using color coding]
- Symbol: [If using symbol system]
- Rarity Indicator: [How rarity is shown]

---

### Testing Notes

**Test Coverage:**

- [ ]  Stats apply correctly when equipped
- [ ]  Special properties function as intended
- [ ]  Durability system works (if applicable)
- [ ]  Loot drop rates verified
- [ ]  Requirements enforced

**Known Issues:**

- Issue 1: [Description]
- Issue 2: [Description]

---

### Changelog

**v0.X - [Date]**

- Initial implementation
- Stats: [values]

**v0.X+1 - [Date]**

- Balance adjustment: [What changed]
- Bug fix: [What was fixed]

---

**Last Updated:** [Date]
**Status:** ✅ Complete | ⏳ In Progress | ❌ Needs Review