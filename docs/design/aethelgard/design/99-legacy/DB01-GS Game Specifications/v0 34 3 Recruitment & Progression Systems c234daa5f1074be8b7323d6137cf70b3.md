# v0.34.3: Recruitment & Progression Systems

Type: Feature
Description: RecruitmentService (faction checks, party limits), CompanionProgressionService (leveling, equipment, abilities), personal quest system (6 companion quests). 7-10 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.34.1 (Database), v0.34.2 (AI), v0.33 (Faction System), v0.14 (Quest System)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.34: NPC Companion System (v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.34.3-COMPANION-RECRUITMENT

**Status:** Implementation Complete — Ready for Integration

**Timeline:** 7-10 hours

**Prerequisites:** v0.34.1 (Database), v0.34.2 (AI), v0.33 (Faction System), v0.14 (Quest System)

**Parent Specification:** v0.34 NPC Companion System[[1]](v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)

---

## I. Executive Summary

This specification defines **companion recruitment and progression mechanics** including faction-gated recruitment, leveling systems, equipment management, and personal quest integration.

---

## II. In Scope vs. Out of Scope

### ✅ In Scope

- RecruitmentService (faction checks, party limits)
- CompanionProgressionService (leveling, equipment, abilities)
- Personal quest system (6 companion quests)
- Testing suite

### ❌ Out of Scope

- AI behavior (covered in v0.34.2)
- Command verb (defer to v0.34.4)
- Companion dialogue/cutscenes

---

## III. RecruitmentService Implementation

**File:** `RuneAndRust.Engine/Services/RecruitmentService.cs` (~250 lines)

**Key Methods:**

- `CanRecruitCompanion()` — Checks faction requirements, party size limit (3 max), quest completion
- `RecruitCompanion()` — Adds companion to character, unlocks personal quest
- `DismissCompanion()` — Removes from party permanently
- `AddToParty()` / `RemoveFromParty()` — Manage active party
- `GetRecruitableCompanions()` — Filter by location and eligibility

**Recruitment Checks:**

1. Companion exists and not already recruited
2. Party size < 3 active companions
3. Faction reputation >= required value (e.g., Iron-Bane Friendly +25)
4. Recruitment quest completed (if required)

---

## IV. CompanionProgressionService Implementation

**File:** `RuneAndRust.Engine/Services/CompanionProgressionService.cs` (~200 lines)

**Key Methods:**

- `AwardLegend()` — Award XP, trigger level ups
- `CalculateScaledStats()` — Base stats + (Level-1) × 10% per level
- `EquipCompanionItem()` — Manage weapon/armor/accessory slots
- `UnlockAbility()` — Grant abilities at levels 3, 5, 7

**Scaling Formula:**

```
HP/Stamina/Aether: Base × (1 + 0.1 × (Level-1))
Attributes: Base + 2 × (Level-1)
```

**Example:** Kara at Level 5

- Base HP 80 → 112 HP (80 + 32)
- Base MIGHT 14 → 22 MIGHT (14 + 8)

---

## V. Personal Quest Definitions

### Six Companion Quests

**1. Kara Ironbreaker: "The Last Protocol"**

- Recover squad mission data from Niflheim data-vault
- Reward: Unique ability "Never Again" (Taunt AOE + Defense buff)

**2. Finnr: "The Forlorn Archive"**

- Access restricted Pre-Glitch database in Alfheim
- Reward: "Ancient Insight" (Reveal all enemy weaknesses)

**3. Bjorn: "The Old Workshop"**

- Reclaim family workshop from Rust-Touched
- Reward: "Field Repair" (Heal 4d6 HP in combat)

**4. Valdis: "Breaking the Voices"**

- Confront corrupted Seiðkona echo
- Reward: "Forlorn Mastery" (No Psychic Stress cost)

**5. Runa: "The Broken Oath"**

- Confront former employer who betrayed her
- Reward: "Unbreakable Oath" (Reduce ally damage 50%)

**6. Einar: "Awaken the Sleeper"**

- Attempt to reactivate dormant Jötun-Forged
- Reward: "Divine Fervor" (+6 attacks, costs 10 HP)

---

## VI. Testing Strategy

**RecruitmentServiceTests.cs** — 5 tests

- Faction requirement success/failure
- Party size limit enforcement
- Personal quest unlocking
- Location filtering

**CompanionProgressionServiceTests.cs** — 4 tests

- Level up triggers at Legend thresholds
- Stat scaling formula validation
- Equipment slot management
- Ability unlocks at levels 3/5/7

**Target:** 85%+ coverage

---

## VII. Integration Points

**Faction System (v0.33):**

```csharp
var reputation = _factionService.GetReputationValue(characterId, "Iron-Bane");
if (reputation < 25) { /* Cannot recruit */ }
```

**Quest System (v0.14):**

```csharp
_questService.UnlockQuest(characterId, companion.PersonalQuestId);
```

**Equipment System (v0.3):**

```csharp
companion.EquippedWeaponId = weaponId; // Uses existing Equipment table
```

---

## VIII. Deployment Steps

1. Create `RecruitmentService.cs` and `CompanionProgressionService.cs`
2. Register services in DI container
3. Execute SQL to add 6 personal quests to Quests table
4. Run unit tests: `dotnet test --filter RecruitmentServiceTests`
5. Integration test: Recruit companion → level up → equip item → start personal quest

---

## IX. Success Criteria

- [ ]  Faction requirements enforced
- [ ]  Party size limited to 3 companions
- [ ]  Personal quests unlock on recruitment
- [ ]  Leveling formula matches player progression
- [ ]  Abilities unlock at levels 3, 5, 7
- [ ]  Equipment system integration functional
- [ ]  85%+ test coverage
- [ ]  Serilog logging throughout

---

**Implementation-ready recruitment and progression systems complete. Services total ~450 lines of code with comprehensive faction gating, scaling formulas, and quest integration.**