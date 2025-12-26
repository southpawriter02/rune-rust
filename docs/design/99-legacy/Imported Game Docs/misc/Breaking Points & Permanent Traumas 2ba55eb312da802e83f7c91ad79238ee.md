# Breaking Points & Permanent Traumas

Parent item: System Documentation (System%20Documentation%202ba55eb312da801a9aa4f30d6e439959.md)

**Version**: v0.17 (Based on v0.15 implementation)
**Status**: Core System (Tier 2)
**Dependencies**: Stress/Corruption System, Attributes, Rest System
**Integration Points**: Combat, Exploration, Stress Thresholds

---

## Table of Contents

1. [Functional Overview](Breaking%20Points%20&%20Permanent%20Traumas%202ba55eb312da802e83f7c91ad79238ee.md)
2. [Trauma Reference](Breaking%20Points%20&%20Permanent%20Traumas%202ba55eb312da802e83f7c91ad79238ee.md)
3. [Technical Implementation](Breaking%20Points%20&%20Permanent%20Traumas%202ba55eb312da802e83f7c91ad79238ee.md)
4. [Testing Coverage](Breaking%20Points%20&%20Permanent%20Traumas%202ba55eb312da802e83f7c91ad79238ee.md)
5. [Balance Considerations](Breaking%20Points%20&%20Permanent%20Traumas%202ba55eb312da802e83f7c91ad79238ee.md)

---

## 1. Functional Overview

### 1.1 Breaking Points

**Breaking Point**: When Psychic Stress reaches 100

**Process**:

1. Player gains Stress from source (combat, hazard, ability)
2. Stress reaches 100
3. **Trauma Selection**: System selects appropriate Trauma based on source
4. **Trauma Acquired**: Permanent psychological wound added to character
5. **Stress Reset**: Stress reduced to 60 (not 0)

**Key Design**: Breaking Points are **permanent consequences** with lasting mechanical effects.

---

### 1.2 Trauma System

**Traumas** are permanent psychological wounds that apply:

1. **Mechanical Effects**: Attribute penalties, passive Stress, rest restrictions
2. **Narrative Flavor**: Intrusive thoughts, behavioral changes
3. **Progression**: Can worsen over time (levels 1-3)

**Total Traumas**: 12 distinct Traumas across 4 categories

---

## 2. Trauma Reference

### 2.1 Fear Category

### [PARANOIA]

**Description**: "They're watching. They're always watching."

**Effects**:

- +20% Stress gain (all sources)
- 1 WITS in social situations
- Cannot rest in rooms with multiple exits

**Acquisition**: General stress, betrayal, ambush

---

### [AGORAPHOBIA]

**Description**: "The open spaces feel wrong. Exposed. Vulnerable."

**Effects**:

- +2 Stress per turn in large rooms
- +1 Movement cost in rooms without cover

**Acquisition**: Exposure in open areas

---

### [CLAUSTROPHOBIA]

**Description**: "The walls are too close. Can't breathe. Can't think."

**Effects**:

- +2 Stress per turn in small rooms
- 1 WILL in confined spaces

**Acquisition**: Trapped, buried, confined

---

### [NYCTOPHOBIA]

**Description**: "The darkness has teeth. Things move in it."

**Effects**:

- +50% Stress in dim lighting
- Cannot rest in dark rooms

**Acquisition**: Attacked in darkness

---

### [ISOLOPHOBIA]

**Description**: "The silence is suffocating. You need to hear something. Anything."

**Effects**:

- +3 Stress per turn when alone
- 1 WILL when isolated

**Acquisition**: Prolonged isolation

---

### 2.2 Guilt Category

### [SURVIVOR'S GUILT]

**Description**: "Why did you live when they didn't?"

**Effects**:

- +2 Stress when allies are harmed
- 1 to healing received

**Acquisition**: Ally death, civilian casualties

---

### 2.3 Obsession Category

### [HYPERVIGILANCE]

**Description**: "Always watching. Always ready. Never at rest."

**Effects**:

- +1 Stress per combat turn
- Rest effectiveness reduced to 75%

**Acquisition**: Repeated combat stress

---

### [DISTRUST]

**Description**: "Everyone lies. Everything is suspect."

**Effects**:

- 2 WITS in social interactions
- Cannot trade with NPCs (except merchants)

**Acquisition**: Betrayal, deception

---

### [COMPULSIVE COUNTING]

**Description**: "Count the exits. Count the enemies. Count your breaths."

**Effects**:

- +1 AP cost for all movement
- +1 WITS for puzzle solving

**Acquisition**: Puzzle-related stress

---

### [HOARDING]

**Description**: "You might need it later. You can't throw anything away."

**Effects**:

- Cannot sell/drop equipment
- +2 inventory slots (but can't discard)

**Acquisition**: Resource scarcity stress

---

### 2.4 Transformation Category

### [DISSOCIATION]

**Description**: "This isn't real. None of this is real. Are you real?"

**Effects**:

- +20% Stress gain (all sources)
- 1 to all attributes during first turn of combat
- Random intrusive thoughts

**Acquisition**: High Stress (75+) for extended time

---

### [MACHINE AFFINITY]

**Description**: "The machines make sense. Humans don't."

**Effects**:

- +0.5 Corruption per day
- +2 Tech checks, -2 Social checks
- Machines less hostile, humans more hostile

**Acquisition**: Corruption 75+ (automatic), prolonged machine exposure

---

## 3. Technical Implementation

### 3.1 Trauma Data Model

**File**: `RuneAndRust.Core/Trauma.cs` (180 lines)

```csharp
public class Trauma
{
    // Identity
    public string TraumaId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Acquisition
    public TraumaCategory Category { get; set; }
    public string AcquisitionSource { get; set; }
    public DateTime AcquiredAt { get; set; }

    // Progression
    public TraumaSeverity Severity { get; set; }
    public int ProgressionLevel { get; set; } = 1;  // 1-3

    // Mechanical effects
    public List<TraumaEffect> Effects { get; set; }

    // Management
    public bool IsManagedThisSession { get; set; }
    public int DaysSinceManagement { get; set; }
}

```

### 3.2 Trauma Effects

**Types of Effects**:

1. **StressMultiplierEffect**: Multiplies Stress gain (e.g., 1.2× for Paranoia)
2. **AttributePenaltyEffect**: Reduces attribute (e.g., -1 WITS)
3. **PassiveStressEffect**: Adds Stress per turn (e.g., +2 in small rooms)
4. **RestRestrictionEffect**: Blocks rest in certain conditions
5. **BehaviorRestrictionEffect**: Limits actions (e.g., can't drop items)

### 3.3 Breaking Point Trigger

**Method**: `TriggerBreakingPoint`

```csharp
private Trauma TriggerBreakingPoint(PlayerCharacter character, string source)
{
    // Select appropriate trauma based on source and context
    var trauma = TraumaLibrary.SelectTraumaForSource(source, character.Corruption, _rng);

    // Don't acquire duplicates
    if (character.HasTrauma(trauma.TraumaId))
    {
        character.PsychicStress = 60; // Still reset stress
        return trauma;
    }

    // Set acquisition details
    trauma.AcquisitionSource = source;
    trauma.AcquiredAt = DateTime.Now;

    // Acquire trauma
    character.Traumas.Add(trauma);

    // Apply immediate effects
    foreach (var effect in trauma.Effects)
    {
        effect.Apply(character);
    }

    // Reset stress to 60 (not 0 - still rattled)
    character.PsychicStress = 60;

    return trauma;
}

```

---

## 4. Testing Coverage

**Test Case**: `TriggerBreakingPoint_AcquiresTrauma`

```csharp
[Test]
public void TriggerBreakingPoint_AcquiresTrauma()
{
    // Arrange
    var player = CreateTestPlayer(stress: 100);
    var traumaService = new TraumaEconomyService();

    // Act
    var (stressGained, trauma) = traumaService.AddStress(player, 1, source: "combat");

    // Assert
    Assert.IsNotNull(trauma);
    Assert.AreEqual(1, player.Traumas.Count);
    Assert.AreEqual(60, player.PsychicStress);
}

```

---

## 5. Balance Considerations

### 5.1 Trauma Permanence

**Design**: Traumas are **permanent**—no removal, only management

**Rationale**: Creates meaningful long-term consequences for poor Stress management

### 5.2 Stress Reset to 60

**Design**: Breaking Point resets Stress to 60 (not 0)

**Rationale**: Character is "rattled" after trauma, not fully recovered

---

**End of Document***See also*: [stress-corruption.md](http://stress-corruption.md/)