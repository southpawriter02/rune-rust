# v0.33.4: Service Implementation & Testing

Type: Technical
Description: FactionService orchestration, FactionEncounterService, unit test suite (12+ tests, 85%+ coverage), integration testing. 7-10 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.33.1, v0.33.2, v0.33.3 complete
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.33: Faction System & Reputation (v0%2033%20Faction%20System%20&%20Reputation%20161115b505034a2fa3ad8288e2513b36.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.33.4-SERVICES

**Parent Specification:** v0.33 Faction System & Reputation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 7-10 hours

**Prerequisites:** v0.33.1, v0.33.2, v0.33.3 complete

---

## I. Overview

Complete service layer orchestration for faction system including FactionService, FactionEncounterService, and comprehensive testing.

### Core Deliverables

- **FactionService** - Primary orchestration
- **FactionEncounterService** - Random encounters
- **Unit Test Suite** (12+ tests, 85%+ coverage)
- **Integration Testing**

---

## II. FactionService

```csharp
public class FactionService
{
    private readonly ReputationService _reputationService;
    private readonly ILogger<FactionService> _logger;
    
    public int GetFactionReputation(int characterId, int factionId)
    {
        // Query Characters_FactionReputations
    }
    
    public List<Quest> GetAvailableFactionQuests(int characterId, int factionId)
    {
        var reputation = GetFactionReputation(characterId, factionId);
        // Return quests where required_reputation <= reputation
    }
    
    public List<Reward> GetFactionRewards(int characterId, int factionId)
    {
        var reputation = GetFactionReputation(characterId, factionId);
        // Return rewards where required_reputation <= reputation
    }
    
    public bool CheckFactionHostility(int characterId, int factionId)
    {
        var reputation = GetFactionReputation(characterId, factionId);
        return reputation <= -26; // Hostile or Hated
    }
}
```

---

## III. FactionEncounterService

```csharp
public class FactionEncounterService
{
    public Encounter? GenerateFactionEncounter(int characterId, string biome)
    {
        // Check reputation with biome-dominant factions
        // Roll for encounter based on reputation tier
        // Generate hostile patrol (low rep) or friendly patrol (high rep)
    }
}
```

---

## IV. Testing Requirements

**Unit Tests (12+):**

1. Reputation tier calculation
2. Price modifier accuracy
3. Faction hostility threshold
4. Quest unlock at thresholds
5. Reward unlock at thresholds
6. Witness system reputation changes
7. Reputation clamping
8. Mutual exclusivity (Iron-Bane vs God-Sleeper)
9. Encounter frequency by reputation
10. Allied faction protection
11. Enemy faction penalties
12. Independent neutrality

---

## V. Success Criteria

- [ ]  All services implemented
- [ ]  85%+ test coverage
- [ ]  Serilog logging throughout
- [ ]  Integration tests pass
- [ ]  Faction encounters spawn correctly
- [ ]  Reputation affects world dynamically

---

**Service implementation complete. v0.33 Faction System ready for deployment.**