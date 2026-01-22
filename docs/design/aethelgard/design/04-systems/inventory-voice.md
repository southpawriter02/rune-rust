---
id: SPEC-SYSTEMS-INVENTORY-VOICE
title: "Inventory System - Voice Guidance"
version: 1.0.0
status: Draft
last-updated: 2025-05-20
---

# Inventory System: Voice Guidance

> "A warrior carries only what keeps them alive. All else is burden for the grave."

## 1. Core Philosophy

The Inventory is not a "bag of holding" or a "digital list." It is a physical burden. Items have weight, heft, and history. The act of managing inventory is the act of deciding what is worth the effort of carrying.

**Key Themes:**
*   **Burden:** Items are heavy. Carrying them is a labor.
*   **Scavenging:** Everything is old, used, and salvaged. Nothing is factory-new.
*   **Reverence:** Tools are extensions of the will. Weapons are partners.

## 2. Terminology Replacement

| System Term | Narrative Term | Context |
| :--- | :--- | :--- |
| **Inventory** | Pack, Burden, Kit, Haul | "Check your pack." |
| **Equip** | Bind, Wield, Don, Attune | "Bind the blade to your hand." |
| **Unequip** | Sheathe, Stow, Release | "Stow the artifact." |
| **Drop** | Discard, Abandon, Leave to Rust | "Leave it to the rust." |
| **Consume** | Imbibe, Use, Break (seal) | "Break the seal and drink." |
| **Encumbered** | Overburdened, Slowed, Heavy-Laden | "The weight drags at your soul." |
| **Capacity** | Strength-Limit, Carry-Limit | "You can carry no more." |

## 3. Feedback & Flavor Text

### 3.1 Inspecting the Pack
*   **Empty:** "Your pack is light. Too light. You will starve if you do not find supplies."
*   **Full:** "The straps dig into your shoulders. You carry the wealth of the dead."
*   **Near Limit:** "Your burden grows heavy. Choose your next prize carefully."

### 3.2 Equipping Items
*   **Weapon:** " The grip is cold, but it warms to your touch. It thirsts."
*   **Armor:** "You clasp the plates. The weight is comforting, like a second skin of iron."
*   **Artifact:** "You feel the hum of the Old World as you attune the device."

### 3.3 Consuming Items
*   **Healing:** "The bitter taste of alchemical mending burns your throat, but the pain fades."
*   **Buff:** "The stimulant floods your blood with fire. Your heart beats like a hammer."

### 3.4 Dropping Items
*   **Standard:** "You let it fall. The rust will claim it soon enough."
*   **Valuable:** "It pains you to leave such a prize, but survival comes first."
*   **Broken:** "Useless scrap. You cast it aside."

## 4. Item Description Guidelines

When writing item descriptions, follow these rules:

1.  **Sensory First:** Describe the smell (oil, rot), feel (cold, rough), and sound (humming, rattling).
2.  **No Modern Materials:** No "plastic", "nylon", or "aluminum". Use "resin", "spider-silk", "light-metal".
3.  **Function as Mystery:** Don't explain *how* it works (batteries, circuits). Explain *what it does* (glows, burns, thinks).

**Example:**
*   ❌ *Bad:* "A flashlight with LED bulb. Requires 2 AA batteries."
*   ✅ *Good:* "A sun-stick. Twist the iron collar to wake the trapped light within. It demands a spark-tithe to function."
id: SPEC-SYSTEM-INVENTORY-VOICE
title: "Voice Guidance: Inventory & Items"
version: 1.0
status: draft
last-updated: 2025-05-15
parent: inventory.md
---

# Voice Guidance: Inventory & Items

---

## 1. Design Philosophy

The Inventory is not a "bag of holding" or a digital list. It is a physical **Burden**. Items are heavy, clanking, and dirty. Managing them is a ritual of survival, not just sorting data.

- **Weight is Burden:** Do not speak of kilograms or pounds. Speak of *heft*, *drag*, and *strain*.
- **Equipping is Binding:** You do not "equip" an item; you *bind* it to your flesh, *don* the iron, or *wield* the tool.
- **Consuming is Ritual:** You do not just "drink a potion"; you *imbibe the bitter draught* or *anoint your wounds*.
- **Trash is Sacrifice:** Dropping an item is *casting it aside* or *feeding it to the rust*.

---

## 2. Vocabulary Replacement

| Modern / Generic RPG | **Aethelgard Voice** |
| :--- | :--- |
| **Inventory** | Pack, Burden, Hoard, Kit |
| **Weight / Encumbrance** | Burden, Strain, Heft, Drag |
| **Slot** | Fitting, Grip, Mount, Place |
| **Equip** | Wield, Don, Bind, Clasp |
| **Unequip** | Shed, Sheathe, Unbind, Cast Off |
| **Potion** | Draught, Tincture, Phial, Blood-Water |
| **Gold / Credits** | Scrip, Old-Coin, Tithe-Iron |
| **Loot** | Salvage, Finds, Dredge |
| **Identify** | Divine, Appraise, Read the Runes |
| **Durability** | Integrity, Temper, Health of Iron |

---

## 3. Flavor Text Examples

### 3.1. Examining the Inventory
*Instead of "You have 5 items and 10 gold."*

> "Your pack is heavy with the tithe of the dead. Old coins rattle against steel."
> "The straps dig into your shoulders. You carry the weight of what you have found."
> "Your kit is sparse; only the desperate essentials remain."

### 3.2. Equipping Items
*Instead of "You equipped the Iron Sword."*

> "You grip the iron hilt. It feels cold and eager."
> "You clasp the rusted chainmail about your chest. It smells of old blood."
> "You bind the amulet to your throat. It hums against your skin."

### 3.3. Unequipping Items
*Instead of "You unequipped the Helmet."*

> "You pull the helm from your head, gasping the stale air."
> "You sheathe the blade. The singing metal falls silent."
> "You cast off the heavy plate, your body light but naked to the dark."

### 3.4. Consuming Items
*Instead of "You drank the Health Potion."*

> "You uncork the phial and swallow the bitter red sludge. Fire knits your flesh."
> "You crush the spirit-stone. A cold wind rushes through your veins."
> "You bandage the wound with rot-moss. The pain dulls to a throb."

### 3.5. Dropping Items
*Instead of "You dropped the shield."*

> "You let the shield fall. It clatters loudly on the stone floor."
> "You leave the heavy boots behind. The rust will claim them now."

---

## 4. Item Description Standards

When describing items (via `examine` or `look`), follow these rules:

1.  **Sensory First:** Describe smell, texture, or sound before visual shape.
    *   *Bad:* "A small red potion."
    *   *Good:* "A glass vial smelling of copper and sulfur. The liquid inside is thick as blood."
2.  **Origin Unknown:** The character does not know who made it or exactly how it works (unless it's primitive).
    *   *Bad:* "A standard issue military battery."
    *   *Good:* "A brick of dense, grey stone that hums with trapped lightning."
3.  **Condition:** Everything is old. Mention rust, scratches, patches, or stains.
    *   *Bad:* "A shiny new sword."
    *   *Good:* "A blade of bright steel, miraculously untouched by the rot, though the leather grip is worn smooth."

---

## 5. UI Feedback Messages

The UI should reflect this tone in its feedback:

- **Inventory Full:** "You can carry no more. The burden is too great."
- **Item Too Heavy:** "It is too heavy to lift. The earth claims it."
- **Cannot Equip:** "That does not fit you, or you lack the strength to bind it."
- **Item Broken:** "The tool has shattered. It is useless slag now."

---
