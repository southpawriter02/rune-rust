# NPC & Dialogue System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-28
> **Status**: Active
> **Specification ID**: SPEC-SYSTEM-012

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2025-11-28 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [x] **Review**: Ready for stakeholder review
- [x] **Approved**: Approved for implementation
- [x] **Active**: Currently implemented and maintained
- [ ] **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders
- **Primary Owner**: Narrative Design Lead
- **Design**: Dialogue flow, NPC personalities, merchant economy
- **Implementation**: NPCService.cs, DialogueService.cs, MerchantService.cs
- **QA/Testing**: Dialogue tree validation, transaction testing

---

## Executive Summary

### Purpose Statement
The NPC & Dialogue System manages non-player character interactions, branching dialogue trees with skill-check gated options, dynamic merchant transactions with reputation-based pricing, and rich NPC flavor text generation.

### Scope
**In Scope**:
- 11 NPC definitions with faction affiliations
- 3 merchant NPCs with dynamic pricing
- 8 dialogue trees with branching conversations
- Skill-check gated dialogue options
- 8 dialogue outcome types
- 102+ NPC flavor text descriptors
- Reputation-based price modifiers
- Merchant inventory and restocking
- Transaction processing (buy/sell)
- GUI components for dialogue and shopping

**Out of Scope**:
- NPC pathfinding/movement → Combat/Exploration systems
- NPC combat AI → `SPEC-SYSTEM-005` Enemy AI
- Romance/relationship system → Future enhancement
- Voice acting → Text-based only

### Success Criteria
- **Player Experience**: Dialogues feel responsive with meaningful choices
- **Technical**: Dialogue trees load in <100ms, transactions process in <50ms
- **Design**: Skill checks provide alternative paths, not blocks
- **Balance**: Merchant prices fair across reputation tiers

---

## Related Documentation

### Dependencies
**Depends On**:
- `SPEC-SYSTEM-011`: Faction & Territory (reputation affects pricing/disposition)
- `SPEC-PROGRESSION-001`: Character Progression (skill checks use attributes)
- Quest System: Dialogue can give/complete quests

**Depended Upon By**:
- `SPEC-SYSTEM-010`: Companion System (faction-gated recruitment)
- `SPEC-SYSTEM-011`: Faction & Territory (dialogue affects reputation)

### Related Specifications
- `SPEC-SYSTEM-009`: GUI Implementation (dialogue/merchant UI)
- `SPEC-SYSTEM-011`: Faction & Territory (reputation integration)
- `SPEC-ECONOMY-001`: Loot & Equipment (item transactions)

### Implementation Documentation
- `V0.38.11_IMPLEMENTATION_SUMMARY.md`: NPC descriptors & dialogue barks

### Code References
- **NPC Service**: `RuneAndRust.Engine/NPCService.cs`
- **Dialogue Service**: `RuneAndRust.Engine/DialogueService.cs`
- **Flavor Text**: `RuneAndRust.Engine/NPCFlavorTextService.cs`
- **Pricing**: `RuneAndRust.Engine/PricingService.cs`
- **Transactions**: `RuneAndRust.Engine/TransactionService.cs`
- **Merchant**: `RuneAndRust.Engine/MerchantService.cs`
- **Data Models**: `RuneAndRust.Core/Dialogue/`, `RuneAndRust.Core/NPC.cs`
- **NPC Data**: `Data/NPCs/*.json` (11 files)
- **Dialogue Data**: `Data/Dialogues/*.json` (8 files)

---

## Design Philosophy

### Design Pillars

1. **Meaningful Dialogue Choices**
   - **Rationale**: Player skill investment should unlock new dialogue paths
   - **Examples**: High WITS reveals hidden information; Negotiation unlocks better deals
   - **Anti-Pattern**: Skill checks that just block progression entirely

2. **Dynamic NPC Relationships**
   - **Rationale**: NPCs should respond to player's faction standing
   - **Examples**: Hostile faction NPCs refuse trade; Allied faction NPCs offer discounts
   - **Anti-Pattern**: Static NPC behavior regardless of player reputation

3. **Rich World Building Through NPCs**
   - **Rationale**: NPCs are primary source of lore and world context
   - **Examples**: 102+ descriptors for varied NPC appearances and reactions
   - **Anti-Pattern**: Generic, interchangeable NPC responses

4. **Fair Merchant Economy**
   - **Rationale**: Trade should feel rewarding, not exploitative
   - **Examples**: Reputation discounts (up to 30%), category modifiers for specialization
   - **Anti-Pattern**: Merchants that always rip off players

---

## NPC Registry

### 11 Implemented NPCs

| ID | Name | Archetype | Faction | Merchant | Room |
|----|------|-----------|---------|----------|------|
| kjartan_merchant | Kjartan the Merchant | Merchant | MidgardCombine | Yes (General) | 5 |
| kjartan_smith | Kjartan the Rust-Smith | Dvergr | MidgardCombine | No | 6 |
| thorvald_guard | Thorvald the Guard | Guard | MidgardCombine | No | 7 |
| bjorn_exile | Bjorn the Exile | Bandit | Independents | No | 8 |
| gunnar_raider | Gunnar the Raider | Raider | MidgardCombine | No | 9 |
| astrid_reader | Astrid the Jötun-Reader | Citizen | Independents | No | 10 |
| rolf_hermit | Rolf the Hermit | Citizen | Independents | No | 11 |
| eydis_survivor | Eydis the Survivor | Raider | Independents | No | 12 |
| sigrun_scavenger | Sigrun the Scavenger | Raider | Independents | No | 13 |
| ragnhild_apothecary | Ragnhild the Apothecary | Seidkona | Independents | Yes (Apothecary) | 14 |
| ulf_scrapper | Ulf the Scrapper | Raider | Independents | Yes (ScrapTrader) | 15 |

### NPC Archetypes

| Archetype | Subtypes | Description |
|-----------|----------|-------------|
| **Dvergr** | Tinkerer, Machinist, ForgeMaster | Tech-focused craftspeople |
| **Seidkona** | WanderingSeidkona, VillageSeidkona, BattleSeidkona | Magic practitioners |
| **Bandit** | Leader, Enforcer, Lookout | Criminal types |
| **Raider** | Scavenger, Scout, Veteran | Combat-focused survivors |
| **Merchant** | Prosperous, Struggling, Exotic | Traders |
| **Guard** | Veteran, Recruit, Captain | Security forces |
| **Citizen** | Artisan, Laborer, Elder | Common folk |
| **Forlorn** | Various | Corruption-touched |

---

## NPC Data Model

### NPC Properties

```
NPC
├── Id: string (primary key)
├── Name: string (display name)
├── Description: string (physical description)
├── InitialGreeting: string (fallback greeting)
├── Archetype: string
├── Subtype: string
├── RoomId: int (location)
├── IsHostile: bool
├── Faction: FactionType
├── BaseDisposition: int (-100 to +100)
├── CurrentDisposition: int (calculated)
├── RootDialogueId: string (entry dialogue node)
├── EncounteredTopics: List<string>
├── HasBeenMet: bool
├── IsAlive: bool
└── QuestFlags: Dictionary<string, bool>
```

### Disposition System

**Disposition Range**: -100 to +100

**Calculation**:
```
CurrentDisposition = BaseDisposition + (FactionReputation × 0.5)
Clamped to [-100, +100]
```

**Disposition Tiers**:

| Tier | Range | Effect |
|------|-------|--------|
| **Friendly** | +50 to +100 | Best prices, full dialogue access |
| **Neutral-Positive** | +10 to +49 | Standard interactions |
| **Neutral** | -9 to +9 | Standard interactions |
| **Unfriendly** | -49 to -10 | Limited dialogue, price markup |
| **Hostile** | -100 to -50 | May refuse interaction, hostile |

---

## Merchant System

### 3 Merchant NPCs

| Merchant | Type | Restock | Specialization |
|----------|------|---------|----------------|
| **Kjartan** | General | 3 days | Weapons, armor, general goods |
| **Ragnhild** | Apothecary | 2 days | Consumables, healing items |
| **Ulf** | ScrapTrader | 3 days | Materials, components, scrap |

### Merchant Data Model

```
Merchant (extends NPC)
├── Type: MerchantType
├── Inventory: ShopInventory
├── RestockIntervalDays: int
├── LastRestockTime: DateTime
├── BaseMarkup: float (default 2.0)
├── ReputationPriceRange: float (default 0.3)
├── BarterSkillImpact: float (default 0.2)
└── CategoryModifiers: Dictionary<string, float>
```

### Category Modifiers by Merchant

| Merchant | Equipment | Consumable | Component |
|----------|-----------|------------|-----------|
| Kjartan | 1.0× | 0.8× | 0.6× |
| Ragnhild | 0.5× | 1.2× | 0.9× |
| Ulf | 0.3× | 0.2× | 1.0× |

### Pricing System

**Base Formula**:
```
BUY_PRICE = BasePrice × BaseMarkup × CategoryModifier × ReputationModifier
SELL_PRICE = BasePrice / BaseMarkup × CategoryModifier × ReputationModifier
```

**Reputation Price Modifiers**:

| Reputation | Buy Modifier | Sell Modifier | Effect |
|------------|--------------|---------------|--------|
| -100 (Hostile) | 1.3× | 0.7× | 30% markup, 30% less for sales |
| 0 (Neutral) | 1.0× | 1.0× | No change |
| +100 (Friendly) | 0.7× | 1.3× | 30% discount, 30% better sales |

### Core Stock (Always Available)

**Kjartan (General)**:
- Healing Poultice (infinite)
- Repair Kit (infinite)
- 2-3 Uncommon weapons
- 1-2 Uncommon armor
- 10% chance: Rare weapon

**Ragnhild (Apothecary)**:
- Healing Poultice (infinite)
- Antidote (infinite)
- Stamina Draught (limited)
- Medicinal herbs
- 20% chance: Rare consumable

**Ulf (Scrap Trader)**:
- ScrapMetal (infinite)
- RustedComponents (infinite)
- CommonHerb (infinite)
- 5-8 Random materials
- 2-3 Cheap consumables

---

## Dialogue System

### Dialogue Tree Structure

```
DialogueNode
├── Id: string (unique identifier)
├── Text: string (NPC's speech)
├── Options: List<DialogueOption>
└── EndsConversation: bool

DialogueOption
├── Text: string (player's choice)
├── SkillCheck: SkillCheckRequirement? (optional)
├── NextNodeId: string? (null = end)
├── Outcome: DialogueOutcome? (optional)
└── IsVisible: bool

DialogueOutcome
├── Type: OutcomeType
├── Data: string (context-specific)
├── ReputationChange: int
└── AffectedFaction: FactionType?
```

### 8 Outcome Types

| Type | Effect | Data Field |
|------|--------|------------|
| **Information** | Reveals lore/hints | Description text |
| **QuestGiven** | Adds quest to log | Quest ID |
| **QuestComplete** | Completes quest | Quest ID |
| **ReputationChange** | Modifies faction rep | Faction + amount |
| **ItemReceived** | Grants item | Item ID |
| **ItemLost** | Removes item | Item ID |
| **InitiateCombat** | Starts fight | Enemy type |
| **EndConversation** | Terminates dialogue | - |

### Skill Check Requirements

```
SkillCheckRequirement
├── Attribute: string (WITS, WILL, MIGHT, FINESSE, STURDINESS)
├── TargetValue: int (minimum required)
├── Skill: Specialization? (optional)
└── SkillRanks: int (required ranks)
```

**Check Logic**:
- Uses "hard check" system - options hidden if player doesn't meet requirements
- Attribute AND Skill checked if both specified (AND logic)
- Currently checks specialization presence (ranks pending v0.9)

### 8 Dialogue Trees

| NPC | Nodes | Topics | Outcomes |
|-----|-------|--------|----------|
| astrid_dialogues | 5+ | Knowledge, survival, quests | Quest, Reputation |
| bjorn_dialogues | Multiple | Exile life | Various |
| eydis_dialogues | Multiple | Survival | Various |
| gunnar_dialogues | Multiple | Raiding | Various |
| kjartan_dialogues | Multiple | Trade, rumors | Various |
| rolf_dialogues | Multiple | Hermit wisdom | Various |
| sigrun_dialogues | Multiple | Scavenging | Various |
| thorvald_dialogues | Multiple | Guard duty | Various |

---

## NPC Flavor Text System

### 102+ Descriptors

| Type | Count | Purpose |
|------|-------|---------|
| **Physical** | 27 | Appearance descriptions |
| **Ambient Barks** | 46 | Idle dialogue |
| **Reactions** | 29 | Emotional responses |

### Physical Descriptor Types

| Type | Description |
|------|-------------|
| FullBody | Complete physical description |
| Face | Facial features |
| Clothing | Attire and dress |
| Equipment | Carried gear |
| Bearing | Posture and manner |
| Distinguishing | Unique features |

### 16 Bark Types

| Bark Type | Trigger |
|-----------|---------|
| AtWork | NPC performing job |
| IdleConversation | NPC chatting |
| Observation | Noticing something |
| Warning | Danger alert |
| Celebration | Happy event |
| Concern | Worried |
| Suspicion | Suspicious of player |
| Encouragement | Boosting morale |
| Complaint | Grumbling |
| Teaching | Sharing knowledge |
| Threat | Intimidation |
| Insult | Hostile remark |
| Wounded | In pain |
| Fleeing | Running away |
| BattleCry | Combat start |
| Greeting | Hello |

### 12 Reaction Types

| Reaction | Trigger Events |
|----------|----------------|
| Surprised | Unexpected events |
| Angry | Provocation |
| Fearful | Danger |
| Relieved | Danger passed |
| Suspicious | Questionable behavior |
| Joyful | Good news |
| Pained | Taking damage |
| Confused | Unclear situation |
| Impressed | Skillful action |
| Disgusted | Repulsive event |
| Grateful | Help received |
| Betrayed | Trust broken |

### Descriptor Context Filters

| Filter | Values |
|--------|--------|
| Condition | Healthy, Wounded, Exhausted, Affluent, Impoverished, BattleReady |
| BiomeContext | Muspelheim, Niflheim, Alfheim, The_Roots |
| AgeCategory | Young, MiddleAged, Elderly, Ageless |
| ActivityContext | Working, Idle, Trading, Guarding, Crafting, etc. |
| DispositionContext | Hostile, Unfriendly, Neutral, Friendly, Allied |

### Fallback Strategy

1. Try specific context (all parameters)
2. Try without least specific parameter
3. Continue removing parameters
4. Use hardcoded fallback if no match

---

## Service Methods

### NPCService

| Method | Purpose |
|--------|---------|
| `LoadNPCDatabase()` | Load all NPC JSON files |
| `GetNPC(id)` | Get NPC by ID |
| `GetNPCsInRoom(room)` | Get NPCs in room |
| `UpdateDisposition(npc, player)` | Update NPC disposition |
| `IsHostile(npc)` | Check hostility |
| `FindNPCByName(room, name)` | Fuzzy name lookup |
| `MarkAsMet(npc)` | Track encounter |
| `RecordTopic(npc, topic)` | Track dialogue topics |
| `GetMerchant(id)` | Get merchant NPC |
| `IsMerchant(npc)` | Check if merchant |

### DialogueService

| Method | Purpose |
|--------|---------|
| `LoadDialogueDatabase()` | Load dialogue JSON |
| `StartConversation(npc, player)` | Begin dialogue |
| `GetAvailableOptions(node, player)` | Filter by skill checks |
| `SelectOption(option, player)` | Execute choice |
| `ProcessOutcome(outcome, player, npc)` | Apply effects |
| `EndConversation()` | Terminate dialogue |
| `MeetsRequirement(player, check)` | Validate skill check |

### NPCFlavorTextService

| Method | Purpose |
|--------|---------|
| `GenerateNPCPhysicalDescription()` | Single descriptor |
| `GenerateCompleteAppearance()` | Full appearance |
| `GenerateAmbientBark()` | Idle dialogue |
| `GenerateContextualBark()` | Auto-typed bark |
| `GenerateReaction()` | Emotional response |
| `GeneratePlayerApproachReaction()` | Approach response |
| `GenerateCombatReaction()` | Combat response |

### PricingService

| Method | Purpose |
|--------|---------|
| `CalculateBuyPrice()` | Player purchase price |
| `CalculateSellPrice()` | Player sell price |
| `CalculateReputationModifier()` | Rep-based modifier |
| `GetPriceDisplay()` | Formatted price string |
| `GetFinalBuyPrice()` | Complete buy calculation |
| `GetFinalSellPrice()` | Complete sell calculation |

### TransactionService

| Method | Purpose |
|--------|---------|
| `BuyItem(merchant, item, player, qty)` | Execute purchase |
| `SellEquipment(merchant, equip, player)` | Sell equipment |
| `SellComponents(merchant, type, qty, player)` | Sell components |
| `FindMerchantInRoom(room)` | Locate merchant |
| `HasMerchantInRoom(room)` | Check availability |

### MerchantService

| Method | Purpose |
|--------|---------|
| `CheckAndRestock(merchant, time)` | Check/perform restock |
| `RestockInventory(merchant, time)` | Full restock |
| `InitializeCoreStock(merchant)` | Setup permanent stock |
| `GenerateRotatingStock(merchant)` | Create rotating items |
| `GetShopListing(merchant)` | Basic item list |
| `GetShopListingWithPrices()` | Display with prices |

---

## Database Schema

### NPC Flavor Descriptor Tables

```sql
CREATE TABLE NPC_Physical_Descriptors (
    descriptor_id INTEGER PRIMARY KEY,
    npc_archetype TEXT NOT NULL,
    npc_subtype TEXT NOT NULL,
    descriptor_type TEXT NOT NULL,
    condition TEXT,
    biome_context TEXT,
    age_category TEXT,
    descriptor_text TEXT NOT NULL,
    weight REAL DEFAULT 1.0,
    is_active BOOLEAN DEFAULT 1,
    tags TEXT
);

CREATE TABLE NPC_Ambient_Bark_Descriptors (
    descriptor_id INTEGER PRIMARY KEY,
    npc_archetype TEXT NOT NULL,
    npc_subtype TEXT NOT NULL,
    bark_type TEXT NOT NULL,
    activity_context TEXT,
    disposition_context TEXT,
    biome_context TEXT,
    trigger_condition TEXT,
    dialogue_text TEXT NOT NULL,
    weight REAL DEFAULT 1.0,
    is_active BOOLEAN DEFAULT 1,
    tags TEXT
);

CREATE TABLE NPC_Reaction_Descriptors (
    descriptor_id INTEGER PRIMARY KEY,
    npc_archetype TEXT NOT NULL,
    npc_subtype TEXT NOT NULL,
    reaction_type TEXT NOT NULL,
    trigger_event TEXT NOT NULL,
    intensity TEXT,
    prior_disposition TEXT,
    action_tendency TEXT,
    biome_context TEXT,
    reaction_text TEXT NOT NULL,
    weight REAL DEFAULT 1.0,
    is_active BOOLEAN DEFAULT 1,
    tags TEXT
);
```

### Indices

```sql
CREATE INDEX idx_npc_physical_lookup
    ON NPC_Physical_Descriptors(npc_archetype, npc_subtype, descriptor_type, condition);
CREATE INDEX idx_npc_bark_lookup
    ON NPC_Ambient_Bark_Descriptors(npc_archetype, npc_subtype, bark_type, activity_context);
CREATE INDEX idx_npc_reaction_lookup
    ON NPC_Reaction_Descriptors(npc_archetype, npc_subtype, reaction_type, trigger_event);
```

---

## GUI Implementation

### Current Status

**Backend**: Complete (6 services, multiple models, 11 NPCs, 8 dialogues)
**GUI**: Partial (NPC display in rooms, TalkCommand placeholder)

### Planned GUI Components

#### 1. Dialogue Window

**Location**: Overlay during conversation
**Purpose**: Display dialogue tree and options

```
┌─────────────────────────────────────────────────────────────┐
│ CONVERSATION: Astrid the Jötun-Reader                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ [Portrait]                                                  │
│                                                             │
│ "The old data cores hold secrets the Iron-Banes would      │
│ rather stay buried. But knowledge... knowledge is power    │
│ in this broken world."                                      │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ DIALOGUE OPTIONS                                            │
│                                                             │
│ [1] Tell me about the data cores.                          │
│                                                             │
│ [2] [WITS 8] What secrets are you hiding?                  │
│     └─ Requires: WITS 8 ✓                                  │
│                                                             │
│ [3] [Jötun-Reader] Share what you know about the Glitch.   │
│     └─ Requires: Jötun-Reader Specialization ✗             │
│                                                             │
│ [4] I should go.                                            │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ Disposition: [████████░░] Neutral-Positive (+35)           │
│ Faction: Independents                                       │
└─────────────────────────────────────────────────────────────┘
```

**Data Bindings**:
- `DialogueViewModel.CurrentNode` → Display text
- `DialogueViewModel.AvailableOptions` → Option buttons
- `DialogueViewModel.NPC` → Name, portrait, disposition
- Color coding for skill checks (green=met, red=not met)

#### 2. Merchant Shop Window

**Location**: Full-screen or large overlay
**Purpose**: Buy/sell items with price display

```
┌─────────────────────────────────────────────────────────────┐
│ MERCHANT: Kjartan the Merchant                              │
│ Type: General Goods        Disposition: Friendly (+62)     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐│
│ │ [BUY] [SELL]                                            ││
│ └─────────────────────────────────────────────────────────┘│
│                                                             │
│ SHOP INVENTORY                                              │
│ ┌───────────────────────────────────────────────────────┐  │
│ │ Item                      Stock    Price              │  │
│ ├───────────────────────────────────────────────────────┤  │
│ │ Healing Poultice          ∞        56 Cogs  [-20%]   │  │
│ │ Repair Kit                ∞        140 Cogs [-20%]   │  │
│ │ Iron Axe (Uncommon)       2        280 Cogs [-20%]   │  │
│ │ Leather Armor (Uncommon)  1        350 Cogs [-20%]   │  │
│ │ Steel Sword (Rare)        1        720 Cogs [-20%]   │  │
│ └───────────────────────────────────────────────────────┘  │
│                                                             │
│ Selected: Iron Axe (Uncommon)                              │
│ Base Price: 350 Cogs                                        │
│ Reputation Discount: -20%                                   │
│ Final Price: 280 Cogs                                       │
│                                                             │
│ Quantity: [1] [-] [+]                                      │
│ Your Cogs: 1,250                                           │
│                                                             │
│ [BUY ITEM]                              [CLOSE SHOP]       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**SELL Tab**:
```
│ YOUR ITEMS                                                  │
│ ┌───────────────────────────────────────────────────────┐  │
│ │ Item                      Qty      Sell Price         │  │
│ ├───────────────────────────────────────────────────────┤  │
│ │ Rusty Sword               1        45 Cogs   [+20%]  │  │
│ │ Scrap Metal               15       8 Cogs ea [+20%]  │  │
│ │ Health Potion             3        25 Cogs   [+20%]  │  │
│ └───────────────────────────────────────────────────────┘  │
```

#### 3. NPC Examination Panel

**Location**: Sidebar or popup
**Purpose**: Show NPC details and initiate dialogue

```
┌─────────────────────────────────────────────────────────────┐
│ NPC: Thorvald the Guard                                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ [Portrait/Sprite]                                           │
│                                                             │
│ APPEARANCE                                                  │
│ A weathered veteran with scarred arms and a stern gaze.    │
│ His rust-stained armor bears the marks of countless        │
│ battles, and he carries a well-maintained spear.           │
│                                                             │
│ DISPOSITION                                                 │
│ [████████░░] Neutral-Positive (+28)                        │
│                                                             │
│ FACTION                                                     │
│ MidgardCombine                                              │
│                                                             │
│ STATUS                                                      │
│ ● Non-hostile                                               │
│ ● First meeting                                             │
│                                                             │
│ [TALK]                                [CLOSE]              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### 4. Conversation Log

**Location**: Sidebar or tab
**Purpose**: Track dialogue history and outcomes

```
┌─────────────────────────────────────────────────────────────┐
│ CONVERSATION LOG                                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ [Astrid the Jötun-Reader]                                  │
│ ├─ "The old data cores hold secrets..."                    │
│ ├─ You asked about the data cores.                         │
│ ├─ [WITS 8] You pressed for hidden secrets.               │
│ │   └─ Reputation: Independents +5                         │
│ ├─ Quest Received: "The Corrupted Archive"                 │
│ └─ Conversation ended.                                      │
│                                                             │
│ [Kjartan the Merchant]                                      │
│ ├─ Bought: Healing Poultice × 3 (168 Cogs)                 │
│ ├─ Sold: Rusty Sword (45 Cogs)                             │
│ └─ Trade completed.                                         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### ViewModel Architecture

```
NPCSystemViewModel
├── DialogueViewModel
│   ├── CurrentNPC: NPC
│   ├── CurrentNode: DialogueNode
│   ├── AvailableOptions: ObservableCollection<DialogueOptionDisplay>
│   ├── DialogueHistory: ObservableCollection<DialogueLogEntry>
│   ├── SelectOptionCommand: ReactiveCommand<int>
│   ├── EndConversationCommand: ReactiveCommand
│   └── IsInConversation: bool
│
├── MerchantShopViewModel
│   ├── CurrentMerchant: Merchant
│   ├── ShopInventory: ObservableCollection<ShopItemDisplay>
│   ├── PlayerInventory: ObservableCollection<SellableItemDisplay>
│   ├── SelectedItem: ShopItemDisplay
│   ├── Quantity: int
│   ├── FinalPrice: int
│   ├── BuyCommand: ReactiveCommand
│   ├── SellCommand: ReactiveCommand
│   └── ActiveTab: ShopTab (Buy/Sell)
│
└── NPCExaminationViewModel
    ├── NPC: NPC
    ├── PhysicalDescription: string
    ├── DispositionDisplay: DispositionViewModel
    ├── TalkCommand: ReactiveCommand
    └── IsMerchant: bool
```

---

## Integration Points

### Current Integrations

| System | Integration | Status |
|--------|-------------|--------|
| Faction Reputation | Disposition calculation | ✅ Complete |
| Faction Reputation | Price modifiers | ✅ Complete |
| Room Population | NPC placement | ✅ Complete |
| TalkCommand | Dialogue initiation | ⚠️ Partial |

### Pending Integrations

| Integration | Description | Priority |
|-------------|-------------|----------|
| Dialogue UI | Full tree visualization | High |
| Merchant UI | Shop interface | High |
| Ambient Barks | Trigger during exploration | Medium |
| Combat Reactions | NPC dialogue in combat | Medium |
| Quest System | Dialogue quest distribution | Medium |

---

## Performance Considerations

### Load Times

| Operation | Target | Notes |
|-----------|--------|-------|
| NPC database load | <500ms | 11 NPCs from JSON |
| Dialogue tree load | <100ms | Per conversation |
| Descriptor lookup | <10ms | Indexed queries |
| Transaction process | <50ms | Buy/sell |

### Caching

- NPC database cached on startup
- Dialogue trees cached per conversation
- Descriptors use weighted random with DB caching

---

## Testing Strategy

### Unit Tests

- Skill check validation
- Price calculations
- Transaction processing
- Disposition updates

### Integration Tests

- Full dialogue tree traversal
- Buy/sell workflows
- Reputation affecting prices
- Quest distribution from dialogue

### Manual Testing Checklist

- [ ] All 11 NPCs loadable
- [ ] All 8 dialogue trees navigable
- [ ] Skill checks filter options correctly
- [ ] Reputation modifies prices correctly
- [ ] Merchants restock on schedule
- [ ] All outcome types execute correctly
- [ ] Disposition updates from faction changes

---

## Known Limitations & Future Work

### Current Limitations

1. **No Dialogue UI**: Backend complete, visual display pending
2. **No Merchant Shop UI**: Transactions work, no interface
3. **Ambient Barks Unused**: Service ready, not triggered
4. **Hard Skill Checks Only**: No partial success/soft checks

### Planned Enhancements

1. **Soft Skill Checks**: Partial success with consequences
2. **Barter Skill**: Additional price discounts
3. **NPC Schedules**: Time-based location changes
4. **Relationship Tracking**: Long-term NPC relationships
5. **Voice Lines**: Audio integration (future)

---

## Appendix A: NPC JSON Format

```json
{
  "Id": "unique_id",
  "Name": "Display Name",
  "Description": "Physical description",
  "InitialGreeting": "Fallback greeting text",
  "Archetype": "Dvergr",
  "Subtype": "Tinkerer",
  "RoomId": 5,
  "IsHostile": false,
  "Faction": "MidgardCombine",
  "BaseDisposition": 10,
  "CurrentDisposition": 10,
  "RootDialogueId": "npc_greeting",
  "EncounteredTopics": [],
  "HasBeenMet": false,
  "IsAlive": true,
  "QuestFlags": {}
}
```

## Appendix B: Dialogue JSON Format

```json
[
  {
    "Id": "node_greeting",
    "Text": "NPC's dialogue text here.",
    "Options": [
      {
        "Text": "Player's response option",
        "SkillCheck": {
          "Attribute": "WITS",
          "TargetValue": 8
        },
        "NextNodeId": "node_wits_success",
        "Outcome": {
          "Type": "ReputationChange",
          "ReputationChange": 5,
          "AffectedFaction": "Independents"
        }
      },
      {
        "Text": "Goodbye.",
        "NextNodeId": null
      }
    ],
    "EndsConversation": false
  }
]
```

---

**End of Specification**
