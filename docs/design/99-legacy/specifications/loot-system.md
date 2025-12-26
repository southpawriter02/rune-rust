# Loot System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-LOOT-v5.0
Parent item: Encounter System — Core System Specification v5.0 (Encounter%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20c82c42d7129c4843a86f2e69cd72f0d7.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## **I. Core Philosophy: Scavenger's Reward**

The Loot System governs **post-combat rewards, item discovery, and equipment acquisition**. Loot represents salvaged hardware, crafting materials, currency, and artifacts recovered from defeated enemies, searched containers, and resource nodes throughout Aethelgard.

**Design Pillars:**

- **Risk = Reward:** Harder enemies drop better loot (enemy tier → quality tier)
- **Rarity Creates Excitement:** Myth-Forged drops are memorable, run-defining moments
- **Class-Appropriate Targeting:** Higher chance of useful drops for player's archetype
- **Progression Pacing:** Quality distribution matches run duration and difficulty curve

---

## **II. System Scope & Dependencies**

### **System Classification**

- **Type:** Mechanic (Child of Encounter System)
- **Parent System:** Encounter System — Core System Specification v5.0

### **Integration Points**

**Upstream Dependencies:**

- Combat Resolution System (enemy defeat triggers loot)
- Enemy Database (enemy type determines drop tables)
- Dynamic Scaling System (TDR affects quality distribution)
- Character System (class for appropriate drops)

**Downstream Dependencies:**

- Equipment System (generated items flow to inventory)
- Crafting System (component drops)
- Economy System (currency generation)

### **Code References**

- **Primary Service:** `RuneAndRust.Engine/LootService.cs`
- **Boss Loot:** `RuneAndRust.Engine/BossLootService.cs`
- **Spawner:** `RuneAndRust.Engine/LootSpawner.cs`
- **Equipment Database:** `RuneAndRust.Engine/EquipmentDatabase.cs`

---

## **III. Loot Sources**

### **A. Enemy Drops**

**Drop Categories:**

1. **Currency (Cogs):** Guaranteed on all enemy defeats
2. **Common Drops:** Crafting materials (60-80% chance)
3. **Uncommon Drops:** Equipment, consumables (20-40% chance)
4. **Rare Drops:** Artifacts, recipes (5-15% chance)
5. **Guaranteed Drops:** Quest items (100% if applicable)

### **B. Containers**

| Container Type | Contents | Quality Range |
| --- | --- | --- |
| Chests | Multiple items, higher quality | Scavenged-Optimized |
| Crates | Crafting materials | Common-Uncommon |
| Corpses | Equipped gear, personal items | Varies by corpse |
| Lockers | Consumables, medical supplies | Scavenged-Clan-Forged |

### **C. Resource Nodes**

| Node Type | Yields | Location |
| --- | --- | --- |
| Scrap Piles | Metal, components | Industrial areas |
| Aether Veins | Shards, crystals | Corrupted zones |
| Fungal Growths | Alchemical ingredients | Damp areas |

---

## **IV. Enemy-Based Drop Tables**

### **Quality Distribution by Enemy Tier**

| Enemy Tier | Jury-Rigged | Scavenged | Clan-Forged | Optimized | Myth-Forged | No Drop |
| --- | --- | --- | --- | --- | --- | --- |
| **Fodder** (Servitor) | 60% | 30% | 0% | 0% | 0% | 10% |
| **Standard** (Drone) | 0% | 40% | 40% | 20% | 0% | 0% |
| **Elite** | 0% | 30% | 45% | 20% | 5% | 0% |
| **Boss** (Warden) | 0% | 0% | 0% | 30% | 70% | 0% |

### **Loot Generation Algorithm**

```csharp
Equipment? GenerateLoot(Enemy enemy, PlayerCharacter? player)
{
    return enemy.Type switch
    {
        EnemyType.CorruptedServitor => GenerateServitorLoot(player),
        EnemyType.BlightDrone => GenerateDroneLoot(player),
        EnemyType.RuinWarden => GenerateBossLoot(player),
        _ => null
    };
}

// Servitor Drop Table (Trash Mob)
Equipment? GenerateServitorLoot(PlayerCharacter? player)
{
    int roll = [Random.Next](http://Random.Next)(100);
    
    if (roll < 10) return null;                         // 10% no drop
    else if (roll < 70) quality = QualityTier.JuryRigged;   // 60% Tier 0
    else quality = QualityTier.Scavenged;                   // 30% Tier 1
    
    return GenerateRandomItem(quality, player);
}

// Boss Drop Table (Guaranteed High-Tier)
Equipment? GenerateBossLoot(PlayerCharacter? player)
{
    int roll = [Random.Next](http://Random.Next)(100);
    
    if (roll < 30) quality = QualityTier.Optimized;     // 30% Tier 3
    else quality = QualityTier.MythForged;              // 70% Tier 4
    
    return GenerateClassAppropriateItem(quality, player);  // 100% class-appropriate
}
```

---

## **V. Class-Appropriate Loot**

### **Filtering Logic**

**Standard Enemies:** 60% chance to filter for class-appropriate weapons

**Boss Enemies:** 100% class-appropriate (always useful)

### **Archetype Weapon Mappings**

| Archetype | Primary Attribute | Weapon Categories |
| --- | --- | --- |
| **Warrior** | MIGHT | Axe, Greatsword, Blunt, HeavyBlunt |
| **Skirmisher** | FINESSE | Spear, Dagger, Blade, Rifle |
| **Mystic** | WILL | Staff, Focus |
| **Adept** | WITS | EnergyMelee, Rifle, Staff |

**Armor:** Not class-restricted (all archetypes can use all armor types)

---

## **VI. Currency Generation**

### **Cogs (Silver Marks) Formula**

```jsx
CogsDrop = BaseCogs × EnemyTier × (1 + Random(0, 0.3))
```

| Enemy Type | Base Cogs | Variance | Expected Range |
| --- | --- | --- | --- |
| Fodder (Minion) | 10 | 5 | 10-15 |
| Standard | 20 | 10 | 20-30 |
| Elite | 50 | 20 | 50-70 |
| Boss | 200 | 50 | 200-260 |

---

## **VII. Equipment Quality Tiers**

### **Weapon Damage Scaling (Axe Example)**

| Tier | Name | Damage | Avg | Bonuses | Special |
| --- | --- | --- | --- | --- | --- |
| 0 | Rusty Hatchet | 1d6 | 3.5 | None | None |
| 1 | Scavenged Axe | 1d6+1 | 4.5 | None | None |
| 2 | Clan-Forged Axe | 1d6+3 | 6.5 | +1 MIGHT | None |
| 3 | Rune-Etched Axe | 2d6 | 7.0 | +1 MIGHT | None |
| 4 | Dvergr Maul | 2d6+4 | 11.0 | +2 MIGHT | Unique effect |

**Power Curve:** Tier 0 → Tier 4 = **~3× damage increase**

### **Armor HP Scaling (Medium Armor Example)**

| Tier | Name | HP Bonus | Defense | Bonuses |
| --- | --- | --- | --- | --- |
| 0 | Scrap Plating | +10 | +1 | None |
| 1 | Scavenged Mail | +15 | +1 | None |
| 2 | Chain Hauberk | +20 | +2 | None |
| 3 | Rune-Etched Mail | +20 | +2 | +1 STURDINESS |
| 4 | Architect's Vestments | +25 | +2 | +2 STURDINESS, +1 WILL |

---

## **VIII. Special Properties**

### **Equipment Property Rolls (10% Chance)**

| Property | Effect | Tier Requirement |
| --- | --- | --- |
| [Reinforced] | +10 max HP | Tier 2+ |
| [Energizing] | +10 max Stamina | Tier 2+ |
| [Aether-Infused] | +10 max AP | Tier 2+ |
| [Honed] | +1d10 accuracy | Tier 3+ |
| [Fortified] | +1 Soak | Tier 3+ |

### **Myth-Forged Special Effects (Tier 4 Only)**

| Effect | Description | Example Item |
| --- | --- | --- |
| Ignores Armor | Damage bypasses Defense bonus | Warden's Greatsword |
| Lifesteal | Heal 25% of damage dealt | Heretical Blade |
| Chain Lightning | Attack bounces to 2 additional targets | Arc-Cannon |
| Regeneration | Heal X HP per turn | Juggernaut Frame |
| AoE Damage | Damage all adjacent enemies | World-Breaker |

---

## **IX. Expected Loot Distribution**

### **Standard 15-Minute Run**

*(10 Servitors, 5 Drones, 1 Boss)*

| Quality Tier | Expected Drops | Percentage |
| --- | --- | --- |
| Jury-Rigged (0) | 6.0 items | 37.5% |
| Scavenged (1) | 5.0 items | 31.3% |
| Clan-Forged (2) | 2.0 items | 12.5% |
| Optimized (3) | 1.3 items | 8.1% |
| Myth-Forged (4) | 0.7 items | 4.4% |
| **Total** | **~16 items** | **100%** |

**Rarity Classification:**

- **Common (T0-T1):** 68.8% — Frequent upgrades, vendor fodder
- **Uncommon (T2):** 12.5% — Mid-game power spike
- **Rare (T3):** 8.1% — Late-game competitive gear
- **Legendary (T4):** 4.4% — Run-defining finds

---

## **X. Artifact Drop Rates**

### **Boss-Only Artifact Chances**

| Encounter Type | Artifact Chance |
| --- | --- |
| Standard encounters | 0% |
| Elite encounters | 5% |
| Boss (TDR 60-80) | 10% |
| Boss (TDR 81+) | 20% |

---

## **XI. Balance & Tuning**

### **Tunable Parameters**

| Parameter | Location | Current | Range | Impact |
| --- | --- | --- | --- | --- |
| BaseDropChance | LootService | 0.6 | 0.4-0.8 | Drop frequency |
| MythForgedChance | LootService | 0.02 | 0.01-0.05 | Legendary rarity |
| ClassAppropriateChance | LootService | 0.60 | 0.4-0.8 | Useful weapon rate |
| BossTier4Rate | LootService | 0.70 | 0.5-0.9 | Boss reward value |
| BaseCogs | LootService | 10 | 5-20 | Currency flow |

### **Balance Targets**

**Weapon vs Armor Priority:** Weapon upgrades provide **2-3× higher value** than equivalent armor upgrades due to offensive scaling and special effects.

**Myth-Forged Rarity:** <5% overall drop rate maintains excitement when found; not expected every run.

**Boss Guarantee:** 100% drop rate + 70% Tier 4 chance ensures boss victories feel impactful.

---

## **XII. Database Schema**

### **Loot_Tables Table**

```sql
CREATE TABLE Loot_Tables (
  table_id INTEGER PRIMARY KEY,
  enemy_type TEXT NOT NULL,
  tier_0_weight INTEGER DEFAULT 0,
  tier_1_weight INTEGER DEFAULT 0,
  tier_2_weight INTEGER DEFAULT 0,
  tier_3_weight INTEGER DEFAULT 0,
  tier_4_weight INTEGER DEFAULT 0,
  null_weight INTEGER DEFAULT 0
);
```

### **Loot_Drops Table**

```sql
CREATE TABLE Loot_Drops (
  drop_id INTEGER PRIMARY KEY,
  combat_id INTEGER NOT NULL,
  item_instance_id INTEGER,
  currency_amount INTEGER DEFAULT 0,
  dropped_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (combat_id) REFERENCES Combat_Instances(combat_id),
  FOREIGN KEY (item_instance_id) REFERENCES Item_Instances(instance_id)
);
```

---

## **XIII. Service Architecture**

### **LootService**

```csharp
public class LootService
{
  public Equipment? GenerateLoot(Enemy enemy, PlayerCharacter? player)
  public Equipment? GenerateRandomItem(QualityTier quality, PlayerCharacter? player)
  public Equipment? GenerateClassAppropriateItem(QualityTier quality, PlayerCharacter player)
  public int GenerateCurrencyDrop(Enemy enemy)
  public List<CraftingComponent> GenerateComponentDrops(Enemy enemy)
}
```

### **BossLootService**

```csharp
public class BossLootService
{
  public BossLootResult GenerateBossLoot(Boss boss, PlayerCharacter player)
  public Equipment GenerateSignatureItem(Boss boss)
  public bool RollArtifactDrop(int tdr)
}
```

---

*This specification follows the v5.0 Three-Tier Template standard. Sources: SPEC-ECONOMY-001 (Loot & Equipment), SPEC-ECONOMY-001 (Loot Generation ROG) from Imported Game Docs. The Loot System is a child mechanic of the Encounter System.*