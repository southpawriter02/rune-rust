# v0.33.2: Reputation Mechanics & World Reactions

Type: Technical
Description: ReputationService implementation, witness system, merchant price modifiers, random encounter frequency adjustments, faction hostility triggers. 8-12 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.33.1 (Database complete)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.33: Faction System & Reputation (v0%2033%20Faction%20System%20&%20Reputation%20161115b505034a2fa3ad8288e2513b36.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.33.2-REPUTATION

**Parent Specification:** v0.33 Faction System & Reputation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-12 hours

**Prerequisites:** v0.33.1 (Database complete)

---

## I. Overview

Reputation calculation engine and dynamic world reactions including merchant prices, random encounter frequency, and faction hostility.

### Core Deliverables

- **ReputationService** - Core reputation calculations
- **Witness System** - Actions affect reputation when observed
- **Merchant Price Modifiers** - Reputation affects trade prices
- **Random Encounter Adjustments** - Reputation changes hostile/friendly spawn rates
- **Faction Hostility Triggers** - Combat initiation at low reputation

---

## II. ReputationService Implementation

### A. Core Methods

```csharp
public class ReputationService
{
    public void ModifyReputation(int characterId, int factionId, int change, string reason)
    {
        // Clamp between -100 and +100
        // Update reputation_tier
        // Log change
    }
    
    public string GetReputationTier(int reputationValue)
    {
        if (reputationValue >= 75) return "Exalted";
        if (reputationValue >= 50) return "Allied";
        if (reputationValue >= 25) return "Friendly";
        if (reputationValue >= -25) return "Neutral";
        if (reputationValue >= -75) return "Hostile";
        return "Hated";
    }
    
    public float GetPriceModifier(string reputationTier)
    {
        return reputationTier switch
        {
            "Exalted" => 0.70f,  // -30%
            "Allied" => 0.80f,   // -20%
            "Friendly" => 0.90f, // -10%
            "Neutral" => 1.0f,
            "Hostile" => 1.25f,  // +25%
            "Hated" => 1.50f,    // +50%
            _ => 1.0f
        };
    }
}
```

---

## III. Witness System

**Actions tracked:**

- Kill enemy (faction affiliation matters)
- Spare enemy
- Complete quest
- Attack faction member
- Trade with faction
- Donate resources

**Implementation:**

```csharp
public void ProcessWitnessedAction(int characterId, string actionType, int? targetFactionId)
{
    var nearbyNPCs = GetNearbyNPCs(characterId);
    foreach (var npc in nearbyNPCs)
    {
        var reputationChange = CalculateReputationChange(actionType, npc.FactionId, targetFactionId);
        if (reputationChange != 0)
        {
            ModifyReputation(characterId, npc.FactionId, reputationChange, $"Witnessed: {actionType}");
        }
    }
}
```

---

## IV. Testing Requirements

**Unit Tests:**

- Reputation tier calculation
- Price modifier accuracy
- Witness system detection
- Reputation clamping (-100 to +100)
- Tier transitions

---

**Reputation mechanics complete. Proceed to faction content (v0.33.3).**