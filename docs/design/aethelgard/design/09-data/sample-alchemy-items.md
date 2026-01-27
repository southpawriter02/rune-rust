---
id: DATA-ALCHEMY-ITEMS
title: "Sample Alchemy Items"
version: 1.1
status: draft
last-updated: 2025-12-15
related-files:
  - path: "docs/04-systems/crafting/alchemy.md"
    status: Core mechanics
  - path: "docs/.background/alchemy.md"
    status: Source ideas
  - path: "docs/08-ui/inventory-ui.md"
    status: Display format
---

# Sample Alchemy Items

This document contains sample alchemical recipes and their resulting items for implementation and writer reference.

---

## 1. Quick-Clot Paste

> *"Slap it on a wound and it seals like hot wax. Hurts almost as much as the cut did."*

### 1.1 Item Data

| Property | Value |
|----------|-------|
| **Item ID** | `ITEM-ALCH-QUICKCLOT` |
| **Name** | Quick-Clot Paste |
| **Category** | Draught (Consumable) |
| **Rarity** | Uncommon |
| **Base Weight** | 0.3 lbs |
| **Base Value** | 25 Scrip |
| **Stack Size** | 5 |

### 1.2 Recipe

| Property | Value |
|----------|-------|
| **Recipe ID** | `RECIPE-ALCH-QUICKCLOT` |
| **DC** | 12 |
| **Time** | 30 minutes |
| **Unlock** | Alchemy Rank 1 |

| Ingredient | Quantity | Rarity |
|------------|----------|--------|
| Red Moss | 3 | Common |
| Rendered Fat | 2 | Common |
| Salt | 1 | Common |

### 1.3 Effect

| Effect | Description |
|--------|-------------|
| **Primary** | Instantly removes [Bleeding] status |
| **Secondary** | Prevents [Bleeding] reapplication for 10 minutes |
| **Duration** | Permanent (until re-wounded) |
| **Application** | External (apply to wound) |

### 1.4 Quality Variations

| Quality | Effect Modification |
|---------|---------------------|
| **Weak** | Removes [Bleeding] but prevention lasts only 5 minutes |
| **Standard** | As described |
| **Potent** | Prevention lasts 15 minutes |
| **Masterwork** | Prevention lasts 30 minutes; +5 HP on application |

### 1.5 Voice Text

| Context | Text |
|---------|------|
| **Examine** | "A thick, rust-red paste sealed in waxed paper. Smells of iron and rendered tallow. The texture is gritty, almost like wet sand mixed with congealed blood." |
| **Use** | "You smear the paste into the wound. It burns — a white-hot searing that makes you gasp — then the bleeding stops. The flesh seals like candle wax." |
| **Already Applied** | "The wound is already sealed. The paste would be wasted." |

### 1.6 TUI Display

```
┌────────────────────────────────────────┐
│ QUICK-CLOT PASTE                       │
│ [Uncommon] Draught — Medical           │
├────────────────────────────────────────┤
│ Effect: Stops [Bleeding] instantly     │
│         Prevents reapply for 10 min    │
│ Weight: 0.3 lbs                        │
│ Value:  25 Scrip                       │
├────────────────────────────────────────┤
│ A thick, rust-red paste sealed in      │
│ waxed paper. Smells of iron and        │
│ rendered tallow.                       │
└────────────────────────────────────────┘
```

---

## 2. Glitch-Sight Draught

> *"Your eyes film over with static. You see what shouldn't be seen."*

### 2.1 Item Data

| Property | Value |
|----------|-------|
| **Item ID** | `ITEM-ALCH-GLITCHSIGHT` |
| **Name** | Glitch-Sight Draught |
| **Category** | Draught (Consumable) |
| **Rarity** | Rare |
| **Base Weight** | 0.4 lbs |
| **Base Value** | 85 Scrip |
| **Stack Size** | 3 |

### 2.2 Recipe

| Property | Value |
|----------|-------|
| **Recipe ID** | `RECIPE-ALCH-GLITCHSIGHT` |
| **DC** | 18 |
| **Time** | 2 hours |
| **Unlock** | Alchemy Rank 2 + Blight-Touched formula (quest) |

| Ingredient | Quantity | Rarity |
|------------|----------|--------|
| Blight Bloom | 2 | Rare |
| Dream Fungus | 2 | Uncommon |
| Aetheric Dust | 1 | Rare |

### 2.3 Effect

| Effect | Description |
|--------|-------------|
| **Primary** | See through illusions; detect [Glitched] zones within 30m |
| **Secondary** | Detect Forlorn within 30m (even through walls) |
| **Duration** | 10 minutes |
| **Application** | Ingested |
| **Side Effect** | +5 Psychic Stress per use |
| **Risk** | 10% chance of hallucinations (WILL DC 14 or [Confused] 1 round) |

### 2.4 Quality Variations

| Quality | Effect Modification |
|---------|---------------------|
| **Weak** | Range reduced to 15m; duration 5 minutes |
| **Standard** | As described |
| **Potent** | Range increased to 45m; hallucination risk reduced to 5% |
| **Masterwork** | Range 60m; duration 20 minutes; no hallucination risk |

### 2.5 Voice Text

| Context | Text |
|---------|------|
| **Examine** | "A vial of liquid that shouldn't exist — it shifts between colors that have no names, occasionally flickering like a broken display. Looking at it too long makes your teeth ache." |
| **Use** | "The draught tastes like copper and regret. Your vision fractures, then reforms. The world gains new edges — impossible angles, hidden geometries. You see the static beneath reality." |
| **Detect Forlorn** | "There. Through the wall. A presence that screams without sound. Your enhanced sight reveals its corona of grief — cold blue light bleeding through stone." |
| **Detect Glitch** | "The air ahead shimmers wrong. Your Glitch-touched eyes see the fracture in reality — a zone where the laws of physics forgot their own rules." |
| **Hallucination** | "The walls breathe. Faces emerge from the stone — familiar faces, dead faces. You blink hard, trying to separate truth from vision." |

### 2.6 TUI Display

```
┌────────────────────────────────────────┐
│ GLITCH-SIGHT DRAUGHT                   │
│ [Rare] Draught — Blight-Touched        │
├────────────────────────────────────────┤
│ Effect: See illusions; detect [Glitch] │
│         Detect Forlorn within 30m      │
│ Duration: 10 minutes                   │
│ Side Effect: +5 Psychic Stress         │
│ Risk: 10% hallucination chance         │
│ Weight: 0.4 lbs                        │
│ Value:  85 Scrip                       │
├────────────────────────────────────────┤
│ A vial of liquid that shouldn't exist  │
│ — it shifts between colors that have   │
│ no names. Looking at it too long       │
│ makes your teeth ache.                 │
│                                        │
│ ⚠ BLIGHT-TOUCHED: Handle with care     │
└────────────────────────────────────────┘
```

---

## 3. Rust-Proof Oil

> *"Scavenger standard issue. Your gear survives Jötunheim. Maybe you do too."*

### 3.1 Item Data

| Property | Value |
|----------|-------|
| **Item ID** | `ITEM-ALCH-RUSTPROOFOIL` |
| **Name** | Rust-Proof Oil |
| **Category** | Coating (Applied) |
| **Rarity** | Common |
| **Base Weight** | 0.5 lbs |
| **Base Value** | 15 Scrip |
| **Stack Size** | 10 |
| **Max Charges** | 5 |
| **Charge Type** | Application (per equipment piece) |

### 3.2 Recipe

| Property | Value |
|----------|-------|
| **Recipe ID** | `RECIPE-ALCH-RUSTPROOFOIL` |
| **DC** | 12 |
| **Time** | 1 hour |
| **Unlock** | Alchemy Rank 1 |

| Ingredient | Quantity | Rarity |
|------------|----------|--------|
| Rendered Fat | 3 | Common |
| Burnt Copper | 2 | Common |
| Salt | 2 | Common |

### 3.3 Effect

| Effect | Description |
|--------|-------------|
| **Primary** | Prevents equipment degradation from environmental corrosion |
| **Secondary** | +1 to equipment durability checks |
| **Duration** | 8 hours per application |
| **Application** | External (apply to equipment) |
| **Charges** | 5 applications per jar |
| **Coverage** | 1 charge = 1 weapon OR 1 armor piece OR 3 tools |

### 3.4 Quality Variations

| Quality | Effect Modification |
|---------|---------------------|
| **Weak** | Duration 4 hours; no durability bonus |
| **Standard** | As described |
| **Potent** | Duration 12 hours; +2 durability bonus |
| **Masterwork** | Duration 24 hours; +2 durability; one application covers ALL carried equipment |

### 3.5 Voice Text

| Context | Text |
|---------|------|
| **Examine** | "A ceramic jar of thick, amber oil with the sharp scent of copper and animal fat. Leaves an iridescent sheen on anything it touches. Standard scavenger kit in the corrosive depths." |
| **Use (Weapon)** | "You work the oil into the blade's surface, filling every pit and scratch. The metal gleams with a protective film — the Rust won't claim this one today." |
| **Use (Armor)** | "You massage the oil into every joint and plate, working it into the chain links. The armor grows supple, resistant to the creeping decay of Jötunheim's atmosphere." |
| **Expired** | "The coating has thinned. You can see the first orange blooms of oxidation forming. Time to reapply." |

### 3.6 TUI Display

```
┌────────────────────────────────────────┐
│ RUST-PROOF OIL                         │
│ [Common] Coating — Utility             │
├────────────────────────────────────────┤
│ Charges: 5/5 (per application)         │
│ Effect: Prevents corrosion damage      │
│         +1 to durability checks        │
│ Duration: 8 hours per application      │
│ Coverage: 1 weapon OR armor OR 3 tools │
│ Weight: 0.5 lbs                        │
│ Value:  15 Scrip                       │
├────────────────────────────────────────┤
│ A ceramic jar of thick, amber oil      │
│ with the sharp scent of copper and     │
│ animal fat. Standard scavenger kit     │
│ in the corrosive depths.               │
└────────────────────────────────────────┘
```

---

## 4. Implementation Notes

### 4.1 Database Schema Addition

```sql
-- Add to Items table
-- Note: MaxCharges and ChargeType are NULL for single-use items
INSERT INTO Items (ItemId, Name, Category, Subcategory, Rarity, BaseWeight, BaseValue, MaxStack, MaxCharges, ChargeType, Description)
VALUES
('ITEM-ALCH-QUICKCLOT', 'Quick-Clot Paste', 'Draught', 'Medical', 'Uncommon', 0.3, 25, 5, NULL, NULL, 'A thick, rust-red paste that instantly stops bleeding.'),
('ITEM-ALCH-GLITCHSIGHT', 'Glitch-Sight Draught', 'Draught', 'Blight-Touched', 'Rare', 0.4, 85, 3, NULL, NULL, 'A reality-bending draught that reveals hidden threats.'),
('ITEM-ALCH-RUSTPROOFOIL', 'Rust-Proof Oil', 'Coating', 'Utility', 'Common', 0.5, 15, 10, 5, 'Application', 'Protective coating that prevents equipment corrosion.');

-- Add to Recipes table
INSERT INTO Recipes (RecipeId, OutputId, DC, TimeMinutes, TradeId, RequiredRank)
VALUES
('RECIPE-ALCH-QUICKCLOT', 'ITEM-ALCH-QUICKCLOT', 12, 30, 'TRADE-ALCHEMY', 1),
('RECIPE-ALCH-GLITCHSIGHT', 'ITEM-ALCH-GLITCHSIGHT', 18, 120, 'TRADE-ALCHEMY', 2),
('RECIPE-ALCH-RUSTPROOFOIL', 'ITEM-ALCH-RUSTPROOFOIL', 12, 60, 'TRADE-ALCHEMY', 1);

-- Add to RecipeIngredients table
INSERT INTO RecipeIngredients (RecipeId, IngredientId, Quantity)
VALUES
-- Quick-Clot Paste
('RECIPE-ALCH-QUICKCLOT', 'MAT-REDMOSS', 3),
('RECIPE-ALCH-QUICKCLOT', 'MAT-RENDEREDFAT', 2),
('RECIPE-ALCH-QUICKCLOT', 'MAT-SALT', 1),
-- Glitch-Sight Draught
('RECIPE-ALCH-GLITCHSIGHT', 'MAT-BLIGHTBLOOM', 2),
('RECIPE-ALCH-GLITCHSIGHT', 'MAT-DREAMFUNGUS', 2),
('RECIPE-ALCH-GLITCHSIGHT', 'MAT-AETHERICDUST', 1),
-- Rust-Proof Oil
('RECIPE-ALCH-RUSTPROOFOIL', 'MAT-RENDEREDFAT', 3),
('RECIPE-ALCH-RUSTPROOFOIL', 'MAT-BURNTCOPPER', 2),
('RECIPE-ALCH-RUSTPROOFOIL', 'MAT-SALT', 2);
```

### 4.2 Effect Implementation

```csharp
public interface IAlchemyEffect
{
    string EffectId { get; }
    void Apply(Character target, ItemQuality quality);
    bool CanApply(Character target);
    string GetUseText(Character target, ItemQuality quality);
}

public class QuickClotEffect : IAlchemyEffect
{
    public string EffectId => "EFFECT-QUICKCLOT";

    public void Apply(Character target, ItemQuality quality)
    {
        target.RemoveStatus(StatusType.Bleeding);

        int preventionMinutes = quality switch
        {
            ItemQuality.Weak => 5,
            ItemQuality.Standard => 10,
            ItemQuality.Potent => 15,
            ItemQuality.Masterwork => 30,
            _ => 10
        };

        target.AddImmunity(StatusType.Bleeding, TimeSpan.FromMinutes(preventionMinutes));

        if (quality == ItemQuality.Masterwork)
        {
            target.Heal(5);
        }
    }

    public bool CanApply(Character target) => target.HasStatus(StatusType.Bleeding);

    public string GetUseText(Character target, ItemQuality quality) =>
        "You smear the paste into the wound. It burns — a white-hot searing — then the bleeding stops.";
}
```

### 4.3 Inventory Display Integration

Per [inventory-ui.md](../08-ui/inventory-ui.md) Section 5 (Item Stacking):

| Item | Category | Max Stack |
|------|----------|-----------|
| Quick-Clot Paste | Draughts | 5 |
| Glitch-Sight Draught | Draughts | 3 |
| Rust-Proof Oil | Draughts | 10 |

---

## 5. Writer's Guide: Creating Alchemy Items

### 5.1 Item Creation Checklist

- [ ] Define unique Item ID (`ITEM-ALCH-[NAME]`)
- [ ] Set appropriate rarity based on ingredient rarity
- [ ] Balance DC against effect power (see Section 5.2)
- [ ] Write sensory-first description (smell, texture, appearance)
- [ ] Create voice text for all interaction contexts
- [ ] Define quality variations (Weak → Masterwork)
- [ ] Add side effects for Blight-Touched items
- [ ] Create TUI display block

### 5.2 DC Balancing Guidelines

| Effect Power | Suggested DC | Ingredient Cost |
|--------------|--------------|-----------------|
| Minor utility | 8-10 | 3-4 common |
| Standard combat | 12-14 | 4-5 common/uncommon |
| Strong effect | 16-18 | 3-4 uncommon + rare |
| Powerful/risky | 18-20 | Multiple rare |
| Legendary | 22+ | Rare + Aetheric components |

### 5.3 Side Effect Guidelines (Blight-Touched)

Blight-Touched items should always have:
1. **Stress cost** (+3 to +10 Psychic Stress)
2. **Risk percentage** (5-20% chance of negative effect)
3. **Warning in description** (⚠ symbol in TUI)

### 5.4 Voice Consistency

Follow the alchemy voice profile from [alchemy.md](../04-systems/crafting/alchemy.md):

| Property | Guideline |
|----------|-----------|
| **Tone** | Practical, observant, slightly grim |
| **Sensory** | Describe smell, texture, taste, visual |
| **No Modern Terms** | "Amber oil" not "yellow liquid" |
| **Effect as Experience** | Describe how it feels, not just what it does |

---

## 6. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.1 | 2025-12-15 | Updated Rust-Proof Oil to use charge system |
| 1.0 | 2025-12-15 | Initial sample items: Quick-Clot Paste, Glitch-Sight Draught, Rust-Proof Oil |
