# Mystic Specialization Backlog: Design Specification

## Document Control

| Field               | Value                                                                                              |
| :------------------ | :------------------------------------------------------------------------------------------------- |
| **Document ID**     | RR-DES-MYSTIC-SPECS-001                                                                            |
| **Feature Name**    | Mystic Specialization Backlog (4 of 5 Mystic specializations)                                      |
| **Module Scope**    | Domain (Enums/ValueObjects) + Application (Services/Interfaces/DTOs) + Config (specializations.json)|
| **Status**          | Draft                                                                                              |
| **Author**          | Claude (AI Assistant)                                                                              |
| **Date**            | 2026-02-28                                                                                         |
| **Reviewers**       | Ryan (Project Owner)                                                                               |
| **Est. Hours**      | 40-55 (4 specializations x 9 abilities each + tests + config)                                      |
| **Parent Document** | `docs/design/aethelgard/design/02-entities/players/archetypes-and-specializations.md`               |

---

## Problem Statement

The Mystic archetype currently has **1 of 5 specializations implemented** (Seiðkona). The remaining four --- Blót-Priest, Echo-Caller, Rust-Witch, and Varð-Warden --- have complete design documentation (overview files, 36 individual ability specs, tier breakdowns, and power curve analysis) but zero code. This means:

- Players choosing the Mystic archetype have only one viable build path (Seiðkona), severely limiting replayability.
- The `specializations.json` config file lists only one Mystic entry, so the specialization selection UI presents an incomplete picture.
- Two of the four Heretical-path Mystic builds (Blót-Priest, Rust-Witch) and both Coherent-path builds (Echo-Caller, Varð-Warden) are inaccessible despite their design docs being finalized since December 2025.
- The `Player` entity has Seiðkona-specific properties (`AetherResonance`, `AccumulatedAethericDamage`, `HashSet<SeidkonaAbilityId>`) but no corresponding storage for the other four specializations.

**Why now?** This is Tier 3, Priority #3 in the Aethelgard migration backlog. It is self-contained --- it does not depend on the reputation or quest chain systems (Tier 3 priorities #1 and #2, now complete). It is also the single largest body of work remaining: 4 specializations x 9 abilities = 36 new abilities to implement, plus resource services, corruption services, and ability services for each.

**Cost of not solving:** The Mystic archetype remains a single-path class, undermining the design goal of "4 archetypes x 5 specializations = 20 unique build paths." Players who want a defensive caster (Varð-Warden), a DoT specialist (Rust-Witch), a sacrificial healer (Blót-Priest), or a crowd controller (Echo-Caller) simply cannot play those builds.

---

## Proposed Solution

Implement all four remaining Mystic specializations by replicating the established Seiðkona pattern. Each specialization gets:

1. **AbilityId enum** (Domain) --- 9 ability IDs across 4 tiers, following the existing naming and range conventions.
2. **CorruptionTrigger enum** (Domain) --- Only for Heretical-path specs (Blót-Priest, Rust-Witch). Coherent-path specs (Echo-Caller, Varð-Warden) skip this since they have no corruption risk.
3. **Resource value objects** (Domain) --- Unique per-specialization resources where the design doc specifies them (e.g., Blót-Priest's HP-to-AP conversion tracking, Rust-Witch's Corroded stack management).
4. **Ability result value objects** (Domain) --- One per active ability, capturing damage dealt, resources spent, corruption triggered, and status effects applied.
5. **Service interfaces** (Application) --- `I{Spec}AbilityService` for all four; `I{Spec}ResourceService` and `I{Spec}CorruptionService` where applicable.
6. **Service implementations** (Application) --- Full ability execution logic following the Seiðkona guard chain pattern: validate specialization → validate unlock → calculate effective AP → validate AP → capture pre-state → evaluate corruption → deduct AP → apply effects → apply corruption → return result.
7. **Player entity extensions** (Domain) --- `HashSet<{Spec}AbilityId>` for tracking unlocked abilities, plus any spec-specific resource properties.
8. **Config entry** (specializations.json) --- Tier structure, PP costs, ability ID lists, and corruption risk parameters.
9. **Unit tests** --- Following the `TestSeidkonaAbilityService` pattern with virtual dice roll overrides for deterministic testing.

The implementation order is: **Rust-Witch → Blót-Priest → Echo-Caller → Varð-Warden**. This sequence ensures:

- Rust-Witch (Heretical) starts with the simplest unique mechanic ([Corroded] stacking is a straightforward DoT counter) while still requiring the full Heretical service trio (ability + resource + corruption).
- Blót-Priest (Heretical) builds on the Heretical pattern with the more complex HP-to-AP conversion mechanic.
- Echo-Caller (Coherent) introduces the Echo Chain spreading system without corruption overhead.
- Varð-Warden (Coherent) finishes with the barrier/zone system, which is the most mechanically distinct from existing abilities.

---

## Architecture

### Component Diagram

```
RuneAndRust.Domain
├── Enums/
│   ├── RustWitchAbilityId.cs          (25001-25009)
│   ├── RustWitchCorruptionTrigger.cs   (Heretical)
│   ├── BlotPriestAbilityId.cs          (30010-30018)
│   ├── BlotPriestCorruptionTrigger.cs  (Heretical)
│   ├── EchoCallerAbilityId.cs          (28010-28018)
│   └── VardWardenAbilityId.cs          (29010-29018) ← REASSIGNED from 28010
│
└── ValueObjects/
    ├── CorrodedStackTracker.cs          (Rust-Witch: stack count + per-target tracking)
    ├── SacrificialCastingState.cs       (Blót-Priest: HP→AP conversion rates by rank)
    ├── EchoChainState.cs                (Echo-Caller: chain range, damage%, target count)
    ├── RunicBarrierState.cs             (Varð-Warden: barrier HP, duration, position)
    ├── SanctifiedGroundState.cs         (Varð-Warden: zone radius, heal/damage per turn)
    ├── RustWitchCorruptionRiskResult.cs (Heretical corruption evaluation)
    └── BlotPriestCorruptionRiskResult.cs(Heretical corruption evaluation)

RuneAndRust.Application
├── Interfaces/
│   ├── IRustWitchAbilityService.cs
│   ├── IRustWitchCorruptionService.cs   (Heretical)
│   ├── IBlotPriestAbilityService.cs
│   ├── IBlotPriestCorruptionService.cs  (Heretical)
│   ├── IBlotPriestSacrificialService.cs (HP→AP conversion)
│   ├── IEchoCallerAbilityService.cs
│   ├── IEchoCallerChainService.cs       (Echo chain spreading)
│   ├── IVardWardenAbilityService.cs
│   └── IVardWardenBarrierService.cs     (Barrier/zone management)
│
├── Services/
│   ├── RustWitchAbilityService.cs
│   ├── RustWitchCorruptionService.cs
│   ├── BlotPriestAbilityService.cs
│   ├── BlotPriestCorruptionService.cs
│   ├── BlotPriestSacrificialService.cs
│   ├── EchoCallerAbilityService.cs
│   ├── EchoCallerChainService.cs
│   ├── VardWardenAbilityService.cs
│   └── VardWardenBarrierService.cs
│
└── DTOs/
    └── (Result records per active ability — see Data Contract section)
```

### Relationship to Existing Seiðkona Pattern

```
Seiðkona (Reference Implementation)
├── SeidkonaAbilityId.cs          (800-808)
├── SeidkonaCorruptionTrigger.cs  (5 triggers, probability-based)
├── AetherResonanceResource.cs    (0-10 building resource)
├── AccumulatedAethericDamage.cs  (immutable damage tracking)
├── SeidkonaCorruptionRiskResult.cs
├── ISeidkonaAbilityService.cs    (9 ability methods)
├── ISeidkonaResonanceService.cs  (initialize, build, reset)
├── ISeidkonaCorruptionService.cs (d100 probability evaluation)
├── SeidkonaAbilityService.cs     (~1135 lines)
├── SeidkonaResonanceService.cs
└── SeidkonaCorruptionService.cs

New specializations follow this same tri-service pattern where applicable,
with Coherent specs omitting the corruption service entirely.
```

---

## Data Contract / API

### Ability ID Ranges

| Specialization | Range      | Notes                                                    |
|----------------|-----------|----------------------------------------------------------|
| Rust-Witch     | 25001-25009 | Per design doc (no collision)                           |
| Echo-Caller    | 28010-28018 | Per design doc (no collision after Varð-Warden reassign)|
| Varð-Warden    | 29010-29018 | **REASSIGNED** from 28010 (collision with Echo-Caller)  |
| Blót-Priest    | 30010-30018 | Per design doc (no collision)                           |

**Collision Resolution:** The Echo-Caller and Varð-Warden design docs both specify IDs 28010-28018. Since Echo-Caller was assigned that range in SPEC-ECHO-CALLER-28002 (matching the spec number), it retains the 28xxx range. Varð-Warden (SPEC-VARD-WARDEN-28001) is reassigned to 29010-29018, keeping all four Mystic specs in adjacent thousands (25k, 28k, 29k, 30k).

### Per-Specialization Ability Tables

#### Rust-Witch (Heretical — Debuffer/DoT)

| ID    | Name                | Tier | Type    | PP | AP Cost | Self-Corruption | Key Mechanic                        |
|-------|---------------------|------|---------|----|---------|-----------------|-------------------------------------|
| 25001 | Philosopher of Dust | 1    | Passive | 3  | —       | —               | +dice to analysis vs corrupted      |
| 25002 | Corrosive Curse     | 1    | Active  | 3  | 2 AP    | +2 (+1 at R3)   | Apply [Corroded] stacks (1→2→3)    |
| 25003 | Entropic Field      | 1    | Passive | 3  | —       | —               | Aura: enemies -1 Armor per stack    |
| 25004 | System Shock        | 2    | Active  | 4  | 3 AP    | +3 (+2 at R3)   | [Corroded] + [Stunned] on Mechanical|
| 25005 | Flash Rust          | 2    | Active  | 4  | 4 AP    | +4 (+3 at R3)   | AoE [Corroded] to all enemies       |
| 25006 | Accelerated Entropy | 2    | Passive | 4  | —       | —               | [Corroded] deals 2d6/stack/turn     |
| 25007 | Unmaking Word       | 3    | Active  | 5  | 4 AP    | +4              | Double existing [Corroded] stacks   |
| 25008 | Cascade Reaction    | 3    | Passive | 5  | —       | —               | Spread [Corroded] on enemy death    |
| 25009 | Entropic Cascade    | 4    | Active  | 6  | 5 AP    | +6              | Execute threshold OR 6d6 Arcane     |

**Unique Mechanics:**

- **[Corroded] Status:** DoT stack mechanic, max 5 stacks per target. Base damage: 1d4/stack/turn. With Accelerated Entropy (25006): 2d6/stack/turn. Each stack also applies -1 Armor penalty.
- **Self-Corruption:** Every active ability inflicts corruption on the caster. Values decrease by 1 at Rank 3 for Tier 1-2 abilities, remain fixed for Tier 3+.
- **Execute Mechanic (Entropic Cascade):** If target has >50 Corruption OR 5 [Corroded] stacks, instant kill. Otherwise, 6d6 Arcane damage.
- **Cascade Spreading (Cascade Reaction):** When a [Corroded] enemy dies, all remaining stacks spread to adjacent enemies within 1 tile.

#### Blót-Priest (Heretical — Sacrificial Healer)

| ID    | Name               | Tier | Type    | PP | AP Cost       | Self-Corruption | Key Mechanic                              |
|-------|--------------------|------|---------|----|---------------|-----------------|-------------------------------------------|
| 30010 | Sanguine Pact      | 1    | Passive | 3  | —             | +1/conversion   | Unlocks HP→AP conversion (2:1→1.5:1→1:1)  |
| 30011 | Blood Siphon       | 1    | Active  | 3  | 2 AP          | +1/siphon       | 3d6→5d6 damage + lifesteal                |
| 30012 | Gift of Vitae      | 1    | Active  | 3  | 2 AP          | Transfer        | Heal ally 4d10→8d10, transfers Corruption  |
| 30013 | Blood Ward         | 2    | Active  | 4  | 10-15 HP      | +2              | HP→Shield (2.5-3.5x value)                |
| 30014 | Exsanguinate       | 2    | Active  | 4  | 3 AP          | +1/tick         | DoT curse + lifesteal per tick             |
| 30015 | Crimson Vigor      | 2    | Passive | 4  | —             | —               | [Bloodied] bonuses to healing/siphon       |
| 30016 | Hemorrhaging Curse  | 3    | Active  | 5  | 4 AP          | +3              | DoT + [Bleeding] + anti-healing debuff     |
| 30017 | Martyr's Resolve   | 3    | Passive | 5  | —             | —               | +Soak, +Resolve when [Bloodied]            |
| 30018 | Heartstopper       | 4    | Active  | 6  | 5 AP + 10 HP  | +10 or +15      | AoE heal (Crimson Deluge) OR execute       |

**Unique Mechanics:**

- **Sacrificial Casting (Sanguine Pact):** Converts HP to AP at rank-dependent rates. Rank 1: 2 HP per 1 AP. Rank 2: 1.5 HP per 1 AP. Rank 3: 1 HP per 1 AP. Each conversion adds +1 Corruption. Cannot reduce HP below 1.
- **[Bloodied] Threshold:** Multiple abilities gain bonuses when the Blót-Priest's HP is below 50%. Crimson Vigor makes this the preferred operating state.
- **Blight Transference (Gift of Vitae):** Healing allies transfers a portion of the caster's Corruption to them. This is the primary identity mechanic --- the Blót-Priest is the only specialization that intentionally corrupts allies.
- **Capstone Dual Mode (Heartstopper):** Once per combat, either Crimson Deluge (AoE heal, +10 Corruption to self, +5 to allies) or Final Anathema (single-target execute, +15 Corruption to self). The mode is chosen at cast time.
- **Trauma Risk: EXTREME.** Highest Corruption generation of any specialization in the system. Even the Seiðkona's probability-based corruption is mild by comparison.

#### Echo-Caller (Coherent — Psychic Artillery/Crowd Control)

| ID    | Name               | Tier | Type    | PP | AP Cost | Key Mechanic                             |
|-------|--------------------|------|---------|----|---------|------------------------------------------|
| 28010 | Echo Attunement    | 1    | Passive | 3  | —       | -1 Aether cost, +psychic resist          |
| 28011 | Scream of Silence  | 1    | Active  | 3  | 2 AP    | [Echo] 2d8→3d8 Psychic, +bonus vs Feared |
| 28012 | Phantom Menace     | 1    | Active  | 3  | 2 AP    | [Echo] Apply [Feared] to target          |
| 28013 | Echo Cascade       | 2    | Passive | 4  | —       | Echo Chain +range/+damage/+targets       |
| 28014 | Reality Fracture   | 2    | Active  | 4  | 3 AP    | [Echo] Damage + [Disoriented] + Push     |
| 28015 | Terror Feedback    | 2    | Passive | 4  | —       | Restore 15-20 Aether on Fear application |
| 28016 | Fear Cascade       | 3    | Active  | 5  | 4 AP    | [Echo] AoE [Feared] spread               |
| 28017 | Echo Displacement  | 3    | Active  | 5  | 4 AP    | [Echo] Forced teleportation              |
| 28018 | Silence Made Weapon| 4    | Active  | 6  | 6 AP    | Ultimate AoE scaling with Fear count     |

**Unique Mechanics:**

- **[Echo] Tag System:** Most active abilities carry the [Echo] tag, meaning they can trigger Echo Chain spreading after the primary effect resolves.
- **Echo Chain Spreading:** After an [Echo]-tagged ability hits, it spreads to adjacent enemies. Base: 1 tile range, 50% damage, 1 additional target. With Echo Cascade (28013) Rank 2: 2 tiles, 70% damage. Rank 3: 3 tiles, 80% damage, 2 additional targets.
- **[Feared] Exploitation:** Scream of Silence and other abilities deal bonus damage (+1d8 to +2d8) against [Feared] targets.
- **Terror Feedback Loop (28015):** Restores 15-20 Aether whenever the Echo-Caller applies [Feared] to an enemy. This creates a resource-positive cycle: apply Fear → regain Aether → cast more abilities → spread more Fear.
- **No Corruption Risk:** As a Coherent-path spec, the Echo-Caller has no corruption service, no corruption triggers, and no self-corruption mechanics. Maximum Corruption is 100 but effectively unreachable through normal gameplay.

#### Varð-Warden (Coherent — Defensive Caster/Battlefield Controller)

| ID    | Name                 | Tier | Type     | PP | AP Cost | Key Mechanic                           |
|-------|----------------------|------|----------|----|---------|----------------------------------------|
| 29010 | Sanctified Resolve I | 1    | Passive  | 3  | —       | +1d10 WILL vs Push/Pull (NO RANKS)     |
| 29011 | Runic Barrier        | 1    | Active   | 3  | 3 AP    | Create wall (30/40/50 HP by rank)      |
| 29012 | Consecrate Ground    | 1    | Active   | 3  | 3 AP    | Create healing/damage zone             |
| 29013 | Rune of Shielding    | 2    | Active   | 4  | 3 AP    | Buff ally: +Soak, +Corruption resist   |
| 29014 | Reinforce Ward       | 2    | Active   | 4  | 2 AP    | Heal barrier HP or boost zone effect   |
| 29015 | Warden's Vigil       | 2    | Passive  | 4  | —       | Row-wide Stress resistance (NO RANKS)  |
| 29016 | Glyph of Sanctuary   | 3    | Active   | 5  | 5 AP    | Party temp HP + Stress immunity        |
| 29017 | Aegis of Sanctity    | 3    | Passive  | 5  | —       | Barrier reflect damage + zone cleanse  |
| 29018 | Indomitable Bastion  | 4    | Reaction | 6  | 5 AP    | Negate fatal damage, create barrier    |

**Unique Mechanics:**

- **Runic Barriers:** Physical wall constructs with HP (30 at Rank 1, 40 at Rank 2, 50 at Rank 3). Duration: 2-4 turns. Block movement, line-of-sight, and projectiles. At Rank 3, barrier destruction deals 2d6 Arcane AoE to adjacent enemies.
- **Sanctified Ground Zones:** Consecrated areas that provide ongoing effects. Healing allies: 1d6 → 2d6 HP/turn. Damaging Blighted/Undying enemies: 1d6 → 2d6 Arcane/turn. At Rank 3: +1d10 Resolve to allies within the zone.
- **NO RANKS Abilities:** Sanctified Resolve I (29010) and Warden's Vigil (29015) have fixed effects that do not scale with rank investment. They cost PP to unlock but are always at maximum power.
- **Reaction Mechanic (Indomitable Bastion):** Triggers as a reaction when an ally would take fatal damage. Negates the damage, creates a barrier around the ally, and grants temporary invulnerability. Once per expedition. This is the only reaction-type ability in the Mystic archetype.
- **No Corruption Risk:** Coherent path. No corruption service needed.

### Corruption Trigger Enums (Heretical Specs Only)

#### RustWitchCorruptionTrigger

```csharp
public enum RustWitchCorruptionTrigger
{
    /// <summary>Corrosive Curse cast — +2 Corruption (R3: +1)</summary>
    CorrosiveCurseCast = 1,

    /// <summary>System Shock cast — +3 Corruption (R3: +2)</summary>
    SystemShockCast = 2,

    /// <summary>Flash Rust cast — +4 Corruption (R3: +3)</summary>
    FlashRustCast = 3,

    /// <summary>Unmaking Word cast — +4 Corruption (all ranks)</summary>
    UnmakingWordCast = 4,

    /// <summary>Entropic Cascade capstone — +6 Corruption (all ranks)</summary>
    EntropicCascadeCast = 5
}
```

The Rust-Witch corruption model is **deterministic** (like Berserkr), not probability-based (like Seiðkona). Every active ability always inflicts a fixed amount of self-corruption. The risk is in resource management, not dice rolls.

#### BlotPriestCorruptionTrigger

```csharp
public enum BlotPriestCorruptionTrigger
{
    /// <summary>HP→AP conversion via Sanguine Pact — +1 Corruption per conversion</summary>
    SacrificialConversion = 1,

    /// <summary>Blood Siphon lifesteal — +1 Corruption per siphon</summary>
    BloodSiphonLifesteal = 2,

    /// <summary>Gift of Vitae Corruption transfer to ally</summary>
    BlightTransference = 3,

    /// <summary>Exsanguinate DoT tick — +1 Corruption per tick</summary>
    ExsanguinateTick = 4,

    /// <summary>Blood Ward creation — +2 Corruption</summary>
    BloodWardCreation = 5,

    /// <summary>Hemorrhaging Curse application — +3 Corruption</summary>
    HemorrhagingCurseApplication = 6,

    /// <summary>Heartstopper: Crimson Deluge mode — +10 Corruption</summary>
    HeartStopperCrimsonDeluge = 7,

    /// <summary>Heartstopper: Final Anathema mode — +15 Corruption</summary>
    HeartStopperFinalAnathema = 8
}
```

The Blót-Priest corruption model is also **deterministic** but with the highest volume of any spec. Multiple abilities trigger corruption per combat round, and the capstone can add +10 or +15 in a single action.

### Result Value Object Contracts

Each active ability produces an immutable result record. Following the Seiðkona pattern (`SeidrBoltResult`, `UnravelingResult`, etc.), each result captures:

**Common Properties (all results):**

```csharp
public bool IsSuccess { get; init; }
public string Description { get; init; }
public int AetherSpent { get; init; }
```

**Heretical-path results also include:**

```csharp
public int CorruptionGained { get; init; }
public {Spec}CorruptionTrigger? Trigger { get; init; }
```

**Per-ability unique properties vary.** For example:

- `CorrosiveCurseResult` adds `int StacksApplied`, `int TotalStacksOnTarget`, `string TargetName`
- `BloodSiphonResult` adds `int DamageDealt`, `int HealthRestored`, `int CorruptionGained`
- `EchoChainResult` adds `int PrimaryDamage`, `int ChainDamage`, `int TargetsHit`, `string[] AffectedTargetNames`
- `RunicBarrierResult` adds `int BarrierHp`, `int Duration`, `GridPosition Position`

Full result contracts will be defined per-ability during implementation, following the patterns established by the Seiðkona result objects.

### Player Entity Extensions

Each specialization requires additions to `Player.cs`:

```csharp
// --- Rust-Witch ---
public HashSet<RustWitchAbilityId> UnlockedRustWitchAbilities { get; private set; } = new();
public bool HasRustWitchAbilityUnlocked(RustWitchAbilityId id) => UnlockedRustWitchAbilities.Contains(id);
public void UnlockRustWitchAbility(RustWitchAbilityId id) => UnlockedRustWitchAbilities.Add(id);
public int GetRustWitchPPInvested() => UnlockedRustWitchAbilities.Count * /* tier-weighted cost */;

// --- Blót-Priest ---
public HashSet<BlotPriestAbilityId> UnlockedBlotPriestAbilities { get; private set; } = new();
public bool HasBlotPriestAbilityUnlocked(BlotPriestAbilityId id) => UnlockedBlotPriestAbilities.Contains(id);
public void UnlockBlotPriestAbility(BlotPriestAbilityId id) => UnlockedBlotPriestAbilities.Add(id);
public int GetBlotPriestPPInvested() => UnlockedBlotPriestAbilities.Count * /* tier-weighted cost */;

// --- Echo-Caller ---
public HashSet<EchoCallerAbilityId> UnlockedEchoCallerAbilities { get; private set; } = new();
public bool HasEchoCallerAbilityUnlocked(EchoCallerAbilityId id) => UnlockedEchoCallerAbilities.Contains(id);
public void UnlockEchoCallerAbility(EchoCallerAbilityId id) => UnlockedEchoCallerAbilities.Add(id);
public int GetEchoCallerPPInvested() => UnlockedEchoCallerAbilities.Count * /* tier-weighted cost */;

// --- Varð-Warden ---
public HashSet<VardWardenAbilityId> UnlockedVardWardenAbilities { get; private set; } = new();
public bool HasVardWardenAbilityUnlocked(VardWardenAbilityId id) => UnlockedVardWardenAbilities.Contains(id);
public void UnlockVardWardenAbility(VardWardenAbilityId id) => UnlockedVardWardenAbilities.Add(id);
public int GetVardWardenPPInvested() => UnlockedVardWardenAbilities.Count * /* tier-weighted cost */;
```

The `GetXPPInvested()` methods follow the Seiðkona pattern: they iterate the HashSet, look up each ability's tier, and sum the PP costs (Tier 1 = 3 PP, Tier 2 = 4 PP, Tier 3 = 5 PP, Capstone = 6 PP).

### Config: specializations.json Entries

Each specialization needs an entry in `config/specializations.json` following the Seiðkona pattern. The structure per entry is:

```json
{
  "id": "rust-witch",
  "name": "Rust-Witch",
  "displayName": "Rust-Witch",
  "tagline": "Entropy given form",
  "archetypeId": "mystic",
  "description": "A debuffer and DoT specialist who weaponizes [Corroded] stacks...",
  "specialResource": {
    "name": "Corroded Stack Management",
    "description": "Tracks [Corroded] stacks on enemies (max 5/target)"
  },
  "corruptionRisk": {
    "type": "deterministic",
    "description": "Every active ability inflicts fixed self-Corruption"
  },
  "tiers": [
    {
      "tier": 1,
      "ppCost": 3,
      "unlockThreshold": 0,
      "abilities": [25001, 25002, 25003]
    },
    {
      "tier": 2,
      "ppCost": 4,
      "unlockThreshold": 8,
      "abilities": [25004, 25005, 25006]
    },
    {
      "tier": 3,
      "ppCost": 5,
      "unlockThreshold": 16,
      "abilities": [25007, 25008]
    },
    {
      "tier": 4,
      "ppCost": 6,
      "unlockThreshold": 24,
      "abilities": [25009]
    }
  ]
}
```

Blót-Priest, Echo-Caller, and Varð-Warden follow the same structure with their respective IDs, descriptions, and tier breakdowns.

---

## Constraints

1. **No new NuGet dependencies.** All four specializations must be implementable with the existing project dependencies (FluentAssertions, NUnit, Moq, System.Text.Json, Microsoft.Extensions.Logging).

2. **Immutable value object pattern.** All result objects, corruption risk results, and resource snapshots must be immutable records (`readonly record struct` or `record`), consistent with `FactionReputation`, `SeidkonaCorruptionRiskResult`, and `AccumulatedAethericDamage`.

3. **Virtual dice roll methods for testing.** All random number generation (d4, d6, d8, d10, d100) must be `internal virtual` methods on the ability service, allowing test subclasses to override for deterministic outcomes. This mirrors `TestSeidkonaAbilityService`.

4. **Guard chain execution pattern.** Every active ability must follow: validate spec → validate unlock → calculate effective AP → validate AP → capture pre-state → evaluate corruption (Heretical only) → deduct AP → apply effects → apply corruption → return result. Skipping steps breaks the audit trail.

5. **PP tier structure must match design docs.** Tier 1 = 3 PP, Tier 2 = 4 PP, Tier 3 = 5 PP, Capstone = 6 PP. Unlock thresholds at 8 PP invested (Tier 2), 16 PP invested (Tier 3), 24 PP invested (Capstone). These values are non-negotiable --- they are hardcoded in the design docs.

6. **Backward compatibility with existing Player serialization.** The new `HashSet<XAbilityId>` properties on Player must serialize/deserialize without breaking existing save files. New properties must default to empty sets when absent from saved data.

7. **Varð-Warden ID reassignment.** Design doc specifies 28010-28018 (collides with Echo-Caller). Spec mandates 29010-29018. Implementation must use the reassigned range.

---

## Alternatives Considered

### Alternative 1: Shared Base Ability Service

**Approach:** Create an abstract `BaseMysticAbilityService<TAbilityId, TCorruptionTrigger>` that all four specializations inherit from, containing the shared guard chain, PP validation, and AP deduction logic.

**Pros:** Eliminates ~200 lines of duplicated guard-chain code per specialization. Changes to the execution pattern propagate automatically.

**Cons:** The Seiðkona implementation does not use a base class --- it is a standalone service. Introducing a base class would create an asymmetry where Seiðkona differs from the other four. Additionally, the four new specs have meaningfully different execution flows: Heretical specs need corruption evaluation mid-chain while Coherent specs skip it; the Blót-Priest has HP-based costs; the Varð-Warden has reaction-triggered abilities. Forcing these into a single generic base creates awkward conditional branches.

**Verdict:** Rejected. The mechanical diversity between specs makes a shared base more complex than the duplication it eliminates. Each spec's ability service should be self-contained, like Seiðkona, with shared patterns enforced by convention and code review rather than inheritance.

### Alternative 2: Implement All Specs Simultaneously via Parallel Agents

**Approach:** Use the parallel dispatch skill to implement all four specializations concurrently.

**Pros:** Faster wall-clock time. Four independent codebases with no shared state.

**Cons:** Each spec touches `Player.cs` (adding HashSet properties), `specializations.json` (adding config entries), and potentially `DependencyInjection.cs` (adding registrations). Parallel writes to shared files create merge conflicts. Additionally, lessons learned from the first implementation (e.g., naming conventions, test patterns, edge cases in the guard chain) would not inform the other three.

**Verdict:** Rejected. Sequential implementation allows each spec to benefit from patterns established by the previous one. Shared file conflicts are avoided entirely.

### Alternative 3: Stub Implementations with TODO Markers

**Approach:** Create the enum, interface, and config files for all four specs first, then fill in ability logic ability-by-ability across all specs.

**Pros:** Gets the config and type system in place quickly. Players could see all specializations listed even before abilities work.

**Cons:** Creates a large surface area of non-functional code. Partially implemented specializations are worse than missing ones because they appear selectable but crash at runtime. Unit tests cannot be written until ability logic exists.

**Verdict:** Rejected. Full vertical implementation (one spec at a time, top to bottom) ensures each deliverable is complete, testable, and functional before moving on.

---

## Error Handling

| Error Case | Handling Strategy | Example |
|------------|-------------------|---------|
| Player has wrong specialization | Return failure result with descriptive message | `IsSuccess = false, Description = "Corrosive Curse requires Rust-Witch specialization, but player has Seiðkona"` |
| Ability not unlocked | Return failure result | `IsSuccess = false, Description = "Flash Rust (25005) is not unlocked. Requires 4 PP in Tier 2."` |
| Insufficient AP | Return failure result with deficit | `IsSuccess = false, Description = "Corrosive Curse requires 2 AP, but player has 1 AP remaining."` |
| Insufficient HP (Blót-Priest only) | Return failure result, never reduce below 1 | `IsSuccess = false, Description = "Blood Ward requires 15 HP sacrifice, but player has 12 HP. Cannot reduce below 1."` |
| [Corroded] at max stacks (Rust-Witch) | Apply ability but cap stacks at 5, log warning | `Description includes "Target already at maximum [Corroded] stacks (5). No additional stacks applied."` |
| Barrier placement on occupied tile (Varð-Warden) | Return failure result | `IsSuccess = false, Description = "Cannot place Runic Barrier at (3,5) — tile is occupied."` |
| Target already [Feared] (Echo-Caller) | Allow reapplication (refreshes duration), no double Aether recovery | Terror Feedback only triggers on fresh Fear application, not refreshes. |
| Capstone used twice in combat (Blót-Priest) | Return failure result | `IsSuccess = false, Description = "Heartstopper has already been used this combat."` |
| Reaction trigger when no AP (Varð-Warden) | Return failure result | `IsSuccess = false, Description = "Indomitable Bastion requires 5 AP to trigger as reaction."` |
| Unknown ability ID passed to service | Throw `ArgumentOutOfRangeException` | Hard error --- this indicates a programming bug, not a game state issue. |
| Null player argument | Throw `ArgumentNullException` | Follows existing Seiðkona pattern. |

---

## Performance Considerations

Performance is not a primary concern for this feature. The ability services execute during turn-based combat, where sub-second response times are the norm. The most computationally intensive operation is Echo Chain spreading (Echo-Caller), which iterates adjacent tiles and applies damage to up to 2 additional targets. Even with maximum chain length (3 tiles, 2 targets at Rank 3), this is O(n) where n is the number of enemies, which will never exceed ~20 in a combat encounter.

**Monitoring target:** Each ability execution should complete in <10ms. Log a warning if any ability takes >50ms to resolve.

---

## Success Criteria

1. All 36 new abilities (9 per spec x 4 specs) are executable through their respective ability services with correct damage/healing/status effect calculations.
2. All Heretical-path abilities correctly apply self-corruption at the design-doc-specified amounts.
3. All Coherent-path abilities execute without any corruption side effects.
4. `specializations.json` contains entries for all 4 new Mystic specializations with correct tier structures.
5. `Player.cs` stores and retrieves ability unlock state for all 4 new specializations.
6. Unit test coverage: minimum 80% line coverage per ability service, with every active ability having at least 3 test cases (success, insufficient AP, ability not unlocked).
7. The solution builds with zero warnings and all existing tests continue to pass.

---

## Acceptance Criteria

| #  | Category       | Criterion                                                                                          | Verification     |
|----|----------------|----------------------------------------------------------------------------------------------------|------------------|
| 1  | Enum           | `RustWitchAbilityId` enum contains 9 values (25001-25009) with XML docs                           | Unit test        |
| 2  | Enum           | `BlotPriestAbilityId` enum contains 9 values (30010-30018) with XML docs                          | Unit test        |
| 3  | Enum           | `EchoCallerAbilityId` enum contains 9 values (28010-28018) with XML docs                          | Unit test        |
| 4  | Enum           | `VardWardenAbilityId` enum contains 9 values (29010-29018) with XML docs                          | Unit test        |
| 5  | Corruption     | `RustWitchCorruptionTrigger` has 5 values matching design doc                                     | Unit test        |
| 6  | Corruption     | `BlotPriestCorruptionTrigger` has 8 values matching design doc                                    | Unit test        |
| 7  | Service        | `IRustWitchAbilityService` has execute methods for all 5 active abilities                         | Compilation      |
| 8  | Service        | `IBlotPriestAbilityService` has execute methods for all 6 active abilities                        | Compilation      |
| 9  | Service        | `IEchoCallerAbilityService` has execute methods for all 5 active abilities + chain methods         | Compilation      |
| 10 | Service        | `IVardWardenAbilityService` has execute methods for all 5 active abilities + reaction trigger      | Compilation      |
| 11 | Corruption     | Rust-Witch: Corrosive Curse deals +2 self-corruption at R1/R2, +1 at R3                          | Unit test        |
| 12 | Corruption     | Blót-Priest: Sanguine Pact conversion adds +1 corruption per conversion                           | Unit test        |
| 13 | Corruption     | Echo-Caller: No ability triggers corruption                                                        | Unit test        |
| 14 | Corruption     | Varð-Warden: No ability triggers corruption                                                       | Unit test        |
| 15 | Mechanic       | Rust-Witch: [Corroded] stacks cap at 5 per target                                                 | Unit test        |
| 16 | Mechanic       | Rust-Witch: Entropic Cascade executes at >50 Corruption OR 5 [Corroded] stacks                    | Unit test        |
| 17 | Mechanic       | Blót-Priest: HP→AP conversion cannot reduce HP below 1                                            | Unit test        |
| 18 | Mechanic       | Blót-Priest: Heartstopper once-per-combat enforced                                                 | Unit test        |
| 19 | Mechanic       | Echo-Caller: Echo Chain spreads to correct number of targets per rank                              | Unit test        |
| 20 | Mechanic       | Echo-Caller: Terror Feedback restores Aether on fresh [Feared] application                        | Unit test        |
| 21 | Mechanic       | Varð-Warden: Runic Barrier HP scales by rank (30/40/50)                                           | Unit test        |
| 22 | Mechanic       | Varð-Warden: Indomitable Bastion triggers as reaction, once per expedition                        | Unit test        |
| 23 | Config         | `specializations.json` contains all 4 new entries with correct tier/PP/ability ID structures       | Integration test |
| 24 | Player         | Player can unlock, query, and persist abilities for all 4 new specializations                      | Unit test        |
| 25 | Build          | Solution compiles with zero errors and zero new warnings                                           | Build            |
| 26 | Regression     | All existing unit tests continue to pass                                                           | Test suite       |
| 27 | Guard Chain    | Every active ability validates: spec → unlock → AP → corruption (if Heretical) → execute → result | Unit test        |

---

## Open Questions

| # | Question | Owner | Resolution Deadline |
|---|----------|-------|---------------------|
| 1 | Should the Seiðkona service be registered in DI as part of this work, or is that a separate task? The existing DependencyInjection.cs does not register Seiðkona services despite the implementation existing. | Ryan | Before implementation begins |
| 2 | The Varð-Warden Indomitable Bastion is a "reaction" ability --- does the current combat system support reaction triggers, or does this need to be stubbed? | Ryan | Before Varð-Warden implementation |
| 3 | Do the NO RANKS abilities (Sanctified Resolve I, Warden's Vigil) still count toward PP invested totals for tier unlock thresholds? | Ryan | Before Varð-Warden implementation |
| 4 | The Blót-Priest Gift of Vitae transfers Corruption to allies --- does this require consent/confirmation in the UI, or is it automatic? For implementation purposes, the service will apply it automatically and the UI can add confirmation later. | Ryan | Can be deferred to UI phase |

---

## Dependencies

### Upstream (this feature depends on)

| Dependency | Status | Notes |
|------------|--------|-------|
| `Player.cs` entity | Resolved | Exists at 2234 lines. Will be extended with 4 new HashSet properties. |
| `SeidkonaAbilityService.cs` pattern | Resolved | 1135-line reference implementation. |
| `specializations.json` config | Resolved | Exists with Seiðkona entry. Will be extended. |
| `config/specializations.json` JSON schema | Resolved | Schema established by Seiðkona entry. |
| `ReputationTier` and corruption enums | Resolved | Existing enum patterns in Domain layer. |
| NUnit + FluentAssertions + Moq | Resolved | Test framework already in use. |

### Downstream (depends on this feature)

| Dependency | Status | Notes |
|------------|--------|-------|
| Specialization selection UI | Unresolved | Cannot display 4 new Mystic specs until config and services exist. |
| Combat integration | Unresolved | Abilities won't fire in combat until services are registered in DI. |
| Character builder | Unresolved | PP allocation for new specs requires Player extensions. |

---

## Development Standards

### Changelog Requirements

- **Detail Level:** One-liner per ability service, grouped by specialization. Full description for architectural decisions (e.g., ID reassignment).
- **Format:** Keep a Changelog (Unreleased section).
- **Audience:** Developers. Players won't see this until the specializations are wired into the combat system.

### Logging Standards

| Level | Use For | Example |
|-------|---------|---------|
| DEBUG | Ability execution internals: AP calculation, corruption evaluation, dice rolls | `"RustWitch.CorrosiveCurse: Target={TargetName}, StacksBefore={Before}, StacksAfter={After}, SelfCorruption={Amount}"` |
| INFO | Ability execution success, tier unlocks, significant state changes | `"Player {PlayerId} executed Corrosive Curse on {TargetName}. Applied {Stacks} [Corroded] stacks."` |
| WARN | Capped stacks, insufficient resources (non-error), reaction trigger with low AP | `"[Corroded] stack cap reached on {TargetName} (5/5). No additional stacks applied."` |
| ERROR | Programming bugs only (null players, invalid ability IDs) | `"RustWitchAbilityService.Execute called with null player."` |

All log messages must use structured logging templates (`{PropertyName}` placeholders, not string interpolation).

### Unit Testing Expectations

**What must be tested:**
- Every active ability: success path, insufficient AP, ability not unlocked, wrong specialization
- Corruption: correct amounts at each rank for Heretical specs, zero corruption for Coherent specs
- Unique mechanics: [Corroded] max stacks, HP→AP conversion floor, Echo Chain target counts, barrier HP scaling
- Edge cases: capstone once-per-combat, reaction trigger conditions, [Bloodied] threshold boundary

**What can be skipped:**
- Passive ability "execution" (passives modify constants, they don't have execute methods)
- `ToString()` on result objects
- Config file existence (covered by integration tests)

**Test naming convention:** `{MethodName}_{Scenario}_{ExpectedBehavior}`
Example: `ExecuteCorrosiveCurse_TargetAtMaxStacks_CapsAtFiveStacks`

**Dice roll override pattern:**

```csharp
internal class TestRustWitchAbilityService : RustWitchAbilityService
{
    private readonly Queue<int> _d6Rolls = new();
    private readonly Queue<int> _d4Rolls = new();

    public void QueueD6Roll(int value) => _d6Rolls.Enqueue(value);
    public void QueueD4Roll(int value) => _d4Rolls.Enqueue(value);

    internal override int RollD6() => _d6Rolls.Count > 0 ? _d6Rolls.Dequeue() : base.RollD6();
    internal override int RollD4() => _d4Rolls.Count > 0 ? _d4Rolls.Dequeue() : base.RollD4();
}
```

### Dependency Tracking

No new external dependencies. All implementation uses existing .NET 9 BCL and project references.

---

## Deliverable Checklist

| #  | Deliverable                                      | Status  |
|----|--------------------------------------------------|---------|
| 1  | SPEC-MYSTIC-SPECIALIZATIONS-001.md (this doc)   | Draft   |
| 2  | Rust-Witch: AbilityId enum + CorruptionTrigger   | Pending |
| 3  | Rust-Witch: Value objects (results, tracker)     | Pending |
| 4  | Rust-Witch: Service interfaces                   | Pending |
| 5  | Rust-Witch: Service implementations              | Pending |
| 6  | Rust-Witch: Player.cs extensions                 | Pending |
| 7  | Rust-Witch: Unit tests                           | Pending |
| 8  | Blót-Priest: AbilityId enum + CorruptionTrigger  | Pending |
| 9  | Blót-Priest: Value objects (results, sacrificial)| Pending |
| 10 | Blót-Priest: Service interfaces                  | Pending |
| 11 | Blót-Priest: Service implementations             | Pending |
| 12 | Blót-Priest: Player.cs extensions                | Pending |
| 13 | Blót-Priest: Unit tests                          | Pending |
| 14 | Echo-Caller: AbilityId enum                      | Pending |
| 15 | Echo-Caller: Value objects (results, chain state)| Pending |
| 16 | Echo-Caller: Service interfaces                  | Pending |
| 17 | Echo-Caller: Service implementations             | Pending |
| 18 | Echo-Caller: Player.cs extensions                | Pending |
| 19 | Echo-Caller: Unit tests                          | Pending |
| 20 | Varð-Warden: AbilityId enum (29010-29018)       | Pending |
| 21 | Varð-Warden: Value objects (results, barriers)   | Pending |
| 22 | Varð-Warden: Service interfaces                  | Pending |
| 23 | Varð-Warden: Service implementations             | Pending |
| 24 | Varð-Warden: Player.cs extensions                | Pending |
| 25 | Varð-Warden: Unit tests                          | Pending |
| 26 | specializations.json: All 4 new entries           | Pending |
| 27 | DependencyInjection.cs: Register all new services | Pending |
| 28 | Build verification: zero errors, zero warnings    | Pending |
| 29 | Test verification: all tests pass                 | Pending |

---

## Document History

| Version | Date       | Author | Changes                                    |
|---------|------------|--------|--------------------------------------------|
| 0.1     | 2026-02-28 | Claude | Initial draft — full spec for 4 Mystic specializations |
