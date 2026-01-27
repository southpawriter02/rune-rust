# Armor Mechanics and Statistics

## Core Mechanic: Soak

Soak is flat damage reduction applied to every incoming physical attack. Armor's primary function is converting lethal damage into survivable impacts.

**Damage Calculation:**
`Final Damage = Max(0, Raw Damage - Total Soak)`

**Design Rationale:**
*   Heavy armor excels against many weak attacks
*   Less effective against single massive hits
*   Flat reduction creates predictable defense
*   **Minimum damage rule:** attacks always deal at least 1 damage (concussive trauma)

### Armor Slot System

Characters equip armor across 5 slots, each contributing to total Soak:

| Slot | Description | Primary Armor Piece |
| :--- | :--- | :--- |
| **Head** | Helmets, hoods, masks, coifs | Protects skull; often includes face coverage |
| **Chest** | Cuirass, hauberk, jerkin, breastplate | Primary armor piece; highest Soak contribution |
| **Hands** | Gauntlets, gloves, bracers, vambraces | Protects hands and forearms |
| **Legs** | Greaves, cuisses, leggings, chausses | Protects thighs and shins |
| **Feet** | Boots, sabatons, shoes, greaves | Protects feet and ankles |

### Armor Slot Soak Distribution

| Slot | Light Armor | Medium Armor | Heavy Armor |
| :--- | :--- | :--- | :--- |
| **Head** | 1 Soak | 2 Soak | 3 Soak |
| **Chest** | 2 Soak | 4 Soak | 6 Soak |
| **Hands** | 1 Soak | 2 Soak | 3 Soak |
| **Legs** | 1 Soak | 2 Soak | 3 Soak |
| **Feet** | 1 Soak | 2 Soak | 3 Soak |
| **Total** | **6 Soak** | **12 Soak** | **18 Soak** |

### Mixed Armor Sets
Characters can mix armor types across slots. Penalty calculation uses the **heaviest piece rule**:
*   Wearing **ANY** Medium armor piece = Medium armor penalties apply
*   Wearing **ANY** Heavy armor piece = Heavy armor penalties apply

## Manufacturing Tiers

### Tier Classifications

| Tier | Designation | Quality | Typical Value (PS) | Characteristics |
| :--- | :--- | :--- | :--- | :--- |
| **0** | **Jury-Rigged** | Scrap | 10–40 PS | Improvised; gaps in coverage; uncomfortable |
| **1** | **Scavenged** | Salvage | 40–150 PS | Recovered components; functional but worn |
| **2** | **Clan-Forged** | Standard | 150–500 PS | Community smithing; reliable construction |
| **3** | **Dvergr-Forged** | Superior | 500–1,500 PS | Pure Principles manufacture; exceptional quality |
| **4** | **Optimized** | Pre-Glitch Tech | 1,500–5,000 PS | Carefully maintained ancestor hardware (non-Aetheric only) |
| **5** | **Myth-Forged** | Legendary | 5,000–30,000+ PS | Master-crafted; unique properties; named armor |

### Quality Tier Effects

| Quality | Soak Modifier | Defense Bonus | Special |
| :--- | :--- | :--- | :--- |
| **Jury-Rigged** | -1 Soak | None | Durability -50% |
| **Scavenged** | Base | None | |
| **Clan-Forged** | +1 Soak | +1 Defense (Medium+) | +10 max HP |
| **Optimized** | +2 Soak | +2 Defense (Heavy) | +20 max HP |
| **Myth-Forged** | +3 Soak | +3 Defense | Unique abilities |

## Combat Statistics

### Soak Calculation

**Total Soak = Sum of all equipped armor pieces + STURDINESS contribution + Quality bonus + Ability bonuses**

| Component | Calculation |
| :--- | :--- |
| **Base Soak** | Sum of armor piece Soak values |
| **STURDINESS Contribution** | STURDINESS / 2 (rounded down) |
| **Quality Bonus** | -1 to +3 based on armor tier |
| **Ability Bonuses** | Defensive Posture, temporary buffs, specialization features |

### Armor Comparison Matrix

| Armor Type | Soak (Full Set) | Defense Bonus | Agility Penalty | Stamina Cost | Stealth |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Cloth/Padded** | 6 | +0 | None | None | Normal |
| **Leather** | 6 | +0 | None | None | Normal |
| **Studded Leather** | 6 | +0 | None | None | Normal |
| **Chain Mail** | 12 | +1 (Clan+) | -1d10 | +2 | Normal |
| **Scale Mail** | 12 | +1 (Clan+) | -1d10 | +2 | Normal |
| **Splint/Ring** | 12 | +1 (Clan+) | -1d10 | +2 | Normal |
| **Half Plate** | 18 | +2 (Optimized+) | -2d10 | +5 | Disadvantage |
| **Full Plate** | 18 | +2 (Optimized+) | -2d10 | +5 | Disadvantage |
| **Fortress Armor** | 18–24 | +3 (Myth) | -3d10 | +8 | Impossible |

### Damage Reduction by Armor Tier

| Armor Class | Typical Soak | Average Hit Reduction | Effectiveness vs. Weak Attacks | Effectiveness vs. Strong Attacks |
| :--- | :--- | :--- | :--- | :--- |
| **Unarmored** | 0–3 | 0–15% | None | None |
| **Light** | 6–9 | 20–35% | Moderate | Low |
| **Medium** | 12–15 | 40–55% | High | Moderate |
| **Heavy** | 18–24 | 60–80% | Excellent | High |
