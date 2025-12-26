# Equipment System — Core System Specification v5.0

Type: Core System
Priority: Must-Have
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-CORESYSTEM-EQUIPMENT-v5.0
Proof-of-Concept Flag: No
Session Handoffs: Consolidation Work — Phase 1A Core Systems Audit Complete (https://www.notion.so/Consolidation-Work-Phase-1A-Core-Systems-Audit-Complete-0c51f0058f3a478fb7bc6a2c192cac2a?pvs=21)
Sub-Type: Core
Sub-item: Weapon System — Mechanic Specification v5.0 (Weapon%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%203b57116120194adaadb89ebc879f863c.md), Armor System — Mechanic Specification v5.0 (Armor%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%2060c356d64339444b9f1b737ba9ce1ae1.md), Artifact & Attunement System — Mechanic Specification v5.0 (Artifact%20&%20Attunement%20System%20%E2%80%94%20Mechanic%20Specificat%20c3919bd01ec3466eac34ecaa4c843dba.md), Crafting System — Mechanic Specification v5.0 (Crafting%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20e5c7686c031c49b79946311a94873f62.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

## **I. Core Philosophy: The Scavenger's Loadout**

The Equipment System is the **mechanical framework for managing gear, weapons, armor, and consumables**. It defines how characters equip items, how equipped gear modifies stats, and how inventory capacity constrains carrying power.

Equipment represents the physical manifestation of a character's build—salvaged hardware, jury-rigged tools, and precious artifacts that define a survivor's capabilities.

**Design Pillars:**

- **Clear Loadout Management:** Paper doll with defined equipment slots
- **Inventory Constraints:** Carry capacity forces meaningful choices
- **Quality Hierarchy:** Jury-Rigged → Myth-Forged progression
- **Attunement Limits:** 3 artifact slots maximum (never increases)

---

## **II. System Scope & Dependencies**

### **System Classification**

- **Type:** Core System (Parent Orchestrator)
- **Child Systems:** Weapon System, Armor System, Artifact & Attunement System

### **Integration Points**

**Upstream Dependencies:**

- Attributes System (carry capacity, proficiency)
- Saga System (equipment unlocks via PP)
- Loot System (equipment acquisition)

**Downstream Dependencies:**

- Combat System (weapons/armor determine combat capabilities)
- Resource Systems (gear modifies HP/Stamina/AP)
- All Specializations (class abilities reference equipment)

---

## **III. Equipment Slots (Paper Doll)**

**Total: 10 Equipment Slots**

**Armor Slots (5):**

- Head: helmets, hoods, masks
- Chest: armor, coats, robes
- Hands: gauntlets, gloves
- Legs: greaves, pants
- Feet: boots, shoes

**Weapon Slots (2):**

- Main Hand: one-handed or two-handed weapons
- Off Hand: shields, one-handed weapons, tools
- Two-handed weapons occupy BOTH slots

**Accessory Slots (3):**

- Accessory 1-3: rings, amulets, trinkets

---

## **IV. Equipment Categories**

- **Weapons:** One-handed melee, two-handed melee, ranged, shields
- **Armor:** Light (low Soak), Medium (moderate Soak), Heavy (high Soak)
- **Artifacts:** Unique items requiring attunement
- **Consumables:** Potions, throwables, tools (stackable, single-use)
- **Components:** Crafting materials, quest items (no combat utility)

---

## **V. Quality Tiers**

1. **Jury-Rigged (Gray):** Barely functional, low stats
2. **Scavenged (White):** Standard salvaged gear, reliable
3. **Clan-Forged (Green):** Professionally crafted, enhanced stats
4. **Optimized (Blue):** Pre-Glitch tech refurbished, significant bonuses
5. **Myth-Forged (Purple/Gold):** Legendary artifacts, unique abilities, requires attunement

---

## **VI. Inventory Management**

### **Carry Capacity**

```jsx
Max Capacity = 20 + (STURDINESS × 2)

Examples:
STURDINESS 5 → 30 capacity
STURDINESS 8 → 36 capacity
```

**Encumbrance:**

- Normal: 0-80% (no penalties)
- Encumbered: 81-100% (-1d10 Agility)
- Overloaded: 101%+ (cannot move)

**Item Bulk:**

- Weightless: 0 (consumables, components)
- Light: 1 (one-handed weapons, light armor)
- Medium: 2 (two-handed weapons, medium armor)
- Heavy: 3-5 (heavy armor, shields)

---

## **VII. Attunement System**

**Attunement Slots: 3 (fixed, never increases)**

**Rules:**

- Only Artifacts and Myth-Forged items require attunement
- Un-attuned Artifacts provide NO benefits
- Changes ONLY during Sanctuary Rest
- Prevents artifact power stacking

**Process:**

1. Must be at Runic Anchor
2. Select Artifact to attune
3. If 3 slots full, unattune existing item first
4. Takes effect immediately

---

## **VIII. Equip/Unequip Mechanics**

**Equipping Requirements:**

```jsx
if (item.isTwoHanded && (mainHandOccupied || offHandOccupied)) → Fail
if (!character.HasProficiency(item.proficiencyRequired)) → Fail
if (item.requiresAttunement && attunedSlots >= 3) → Fail
if (item.minSTURDINESS > character.STURDINESS) → Fail
```

**Actions:**

- Equip: Free Action, bonuses applied immediately
- Unequip: Free Action, bonuses removed

---

## **IX. Database Schema**

### **Items Table (Static Templates)**

```sql
CREATE TABLE Items (
  item_id INTEGER PRIMARY KEY,
  name TEXT NOT NULL,
  item_type TEXT NOT NULL,
  equipment_slot TEXT,
  quality TEXT NOT NULL,
  bulk_weight INTEGER DEFAULT 1,
  requires_attunement BOOLEAN DEFAULT FALSE,
  proficiency_required TEXT,
  min_sturdiness INTEGER DEFAULT 0,
  max_durability INTEGER,
  properties TEXT,
  value INTEGER
);
```

### **Item_Instances Table**

```sql
CREATE TABLE Item_Instances (
  instance_id INTEGER PRIMARY KEY,
  base_item_id INTEGER NOT NULL,
  owner_character_id INTEGER,
  equipped_slot TEXT,
  current_durability INTEGER,
  quantity INTEGER DEFAULT 1,
  is_attuned BOOLEAN DEFAULT FALSE,
  FOREIGN KEY (base_item_id) REFERENCES Items(item_id)
);
```

---

## **X. Service Architecture**

**EquipmentService Methods:**

```csharp
public bool EquipItem(int characterId, int itemInstanceId)
public bool UnequipItem(int characterId, string slot)
public ItemInstance GetEquippedItem(int characterId, string slot)
public bool CanEquip(int characterId, int itemInstanceId)
public int GetCurrentCarryWeight(int characterId)
public int GetMaxCarryCapacity(int characterId)
public bool IsEncumbered(int characterId)
public bool AttuneArtifact(int characterId, int itemInstanceId)
public bool UnattuneArtifact(int characterId, int itemInstanceId)
```

---

## **XI. UI Systems**

### **Equipment Screen**

```
╔══════════════════════════════════════╗
║ Character's Loadout                  ║
╠══════════════════════════════════════╣
║ Head:     [Clan-Forged] Helm         ║
║ Chest:    [Scavenged] Plate          ║
║ MainHand: [Axe of Woe] (Artifact)    ║
║                                      ║
║ Attunement: 1/3                      ║
║ HP: 110/110  Soak: 8                 ║
╚══════════════════════════════════════╝
```

### **Inventory Screen**

```
╔════════════════════════════════════════╗
║ Character's Inventory                  ║
║ Capacity: 18/24 (Encumbered)           ║
╠════════════════════════════════════════╣
║ SLOT | NAME              | BULK        ║
║ MH   | Combat Axe        | 1           ║
║      | Healing Poultice  | 0  (x5)     ║
╚════════════════════════════════════════╝
```

---

## **XII. Systemic Integration**

**Combat Impact:**

- Equipped weapons determine attack damage/accuracy
- Equipped armor provides Soak (damage reduction)
- Shields enable `block` action

**Resource Modification:**

- [Reinforced] armor: +10 max HP
- [Energizing] gear: +10 max Stamina
- [Aether-Infused] items: +10 max AP

**Build Expression:**

- Heavy armor + shield = Tank build (high Soak, low mobility)
- Light armor + dual-wield = DPS build (high damage, low defense)
- Medium armor + two-handed = Balanced build

---

## **XIII. Balance Considerations**

**Attunement Limit:**

- 3 slots prevent "carry all artifacts" power stacking
- Forces meaningful choice between powerful items
- Cannot be increased (core balance constraint)

**Carry Capacity:**

- STURDINESS investment = more carrying power
- Trade-off: carry more loot vs invest in combat stats

**Quality Scaling:**

- Myth-Forged ~3-5× more powerful than Scavenged
- Requires attunement slot (opportunity cost)

---

*This specification follows the v5.0 Three-Tier Template standard. The Equipment System is the parent orchestrator for all gear management.*