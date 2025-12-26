---
id: SPEC-UI-COMBAT
title: "Combat UI — TUI & GUI Specification"
version: 1.0
status: draft
last-updated: 2025-12-14
related-files:
  - path: "docs/03-combat/combat-resolution.md"
    status: Reference
  - path: "docs/08-ui/tui-layout.md"
    status: Reference
  - path: "docs/08-ui/commands/combat.md"
    status: Reference
---

# Combat UI — TUI & GUI Specification

> *"Information is power. In the chaos of battle, clarity saves lives."*

---

## 1. Overview

This specification defines the terminal (TUI) and graphical (GUI) interfaces for combat encounters, providing clear turn order, combatant status, action options, and tactical feedback.

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-UI-COMBAT` |
| Category | UI System |
| Priority | Critical |
| Status | Draft |

### 1.2 Design Pillars

- **Turn Clarity** — Always visible whose turn it is
- **Immersive Damage** — No numbers by default; gauge by appearance
- **Action Accessibility** — Smart commands for common actions
- **Tactical Feedback** — Clear hit/miss/crit messaging
- **Conditional Grid** — Positional grid only when encounter requires it

---

## 2. TUI Combat Layout

### 2.1 Full Combat Screen

```
┌─────────────────────────────────────────────────────────────────────┐
│  HP: 45/60 ████████░░  Stamina: 55/100 ██████░░░░  [Focused]        │
├─────────────────────────────────────────────────────┬───────────────┤
│  ╔══════════════════════════════════════════════╗   │ TURN ORDER    │
│  ║  COMBAT — Round 3                  Turn: YOU ║   │ ────────────  │
│  ╟──────────────────────────────────────────────╢   │ → YOU         │
│  ║  ALLIES                                      ║   │   Goblin ♣    │
│  ║  YOU:    45/60 HP  ████████░░░░  [Focused]   ║   │   Orc ♣       │
│  ║  Bjorn:  30/40 HP  ████████░░░░  [—]         ║   │   Bjorn       │
│  ╟──────────────────────────────────────────────╢   │               │
│  ║  ENEMIES                                     ║   │ ┌───────────┐ │
│  ║  Goblin: 12/30 HP  ████░░░░░░  [Bleeding]    ║   │ │   A B C   │ │
│  ║  Orc:    25/50 HP  █████░░░░░  [—]           ║   │ │ 1 P . .   │ │
│  ╚══════════════════════════════════════════════╝   │ │ 2 A . E   │ │
│                                                     │ │ 3 . . E   │ │
│                                                     │ └───────────┘ │
├─────────────────────────────────────────────────────┴───────────────┤
│  → You attack Goblin for 12 damage                                  │
│  ★ CRITICAL HIT!                                                    │
│  [Status] Goblin is now [Bleeding] (1d4/turn, 3 turns)              │
│  → Goblin attacks you for 5 damage (soaked 3)                       │
├─────────────────────────────────────────────────────────────────────┤
│  [1] attack goblin  [2] use Skewer on orc  [3] defend  [4] flee     │
├─────────────────────────────────────────────────────────────────────┤
│  [Combat] > _                                                       │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Component Breakdown

| Component | Description | Lines |
|-----------|-------------|-------|
| **Header Bar** | Player HP, Stamina, active status effects | 1 |
| **Round/Turn Header** | Current round and whose turn | 1 |
| **Combatant Panels** | Allies & Enemies with condition and status | 6-8 |
| **Turn Order** | Initiative queue with current turn arrow | 6 |
| **Combat Grid** | Positional grid (grid-enabled encounters only) | 5 |
| **Combat Log** | Rolling feed of combat events | 4 |
| **Smart Commands** | Context-aware action shortcuts | 1 |
| **Input Prompt** | `[Combat] >` | 1 |

### 2.3 Side Panel Components

**Turn Order Panel:**
```
TURN ORDER
────────────
→ YOU           ← Current turn indicator
  Goblin ♣      ← ♣ = enemy
  Orc ♣
  Bjorn         ← Ally (no symbol)
```

**Combat Grid (Grid-Enabled Encounters Only):**

> [!NOTE]
> **Design Decision:** The combat grid is only displayed in **grid-enabled encounters** (tactical battles, boss fights, complex terrain). Standard encounters use theater-of-the-mind positioning.

**Standard Grid Size:** 4 rows × 6-8 columns

```
      A   B   C   D   E   F   G   H
    ┌───┬───┬───┬───┬───┬───┬───┬───┐
  1 │ . │ P │ . │ . │ . │ . │ E │ . │   P = Player (You)
    ├───┼───┼───┼───┼───┼───┼───┼───┤   A = Ally
  2 │ A │ . │ . │ . │ . │ E │ . │ . │   E = Enemy
    ├───┼───┼───┼───┼───┼───┼───┼───┤   . = Empty
  3 │ . │ . │ . │ . │ . │ . │ . │ E │
    ├───┼───┼───┼───┼───┼───┼───┼───┤
  4 │ . │ A │ . │ . │ . │ . │ . │ . │
    └───┴───┴───┴───┴───┴───┴───┴───┘
```

**Grid Navigation:**

| Command | Example | Effect |
|---------|---------|--------|
| **Coordinate** | `move C2` | Move to column C, row 2 |
| **Direction** | `move east` | Move one cell east |
| **Position Query** | `where am I` | "You are at B1" |

**Movement Range Display:**
```
> move

  Your movement range (2 cells):
  Available: A1, C1, D1, B2, C2
  
  Enter destination: _
```



---

## 3. Combatant Display

### 3.1 Player HP Display (Numbers Shown)

Players always see their own exact HP:

```
YOU:    45/60 HP  ████████░░░░  [Focused]
```

### 3.2 Enemy Condition Display (Appearance-Based)

> [!IMPORTANT]
> **Design Decision:** Enemy HP numbers are **hidden by default**. Players must use `examine` to gauge enemy condition by appearance.

**Default Display (No Numbers):**
```
Goblin: [████████░░░░]  [Bleeding]   ← Bar only, no numbers
Orc:    [██████████░░]  [—]
```

**Condition Descriptions (examine command):**

| HP Range | Appearance | Description |
|----------|------------|-------------|
| 100% | Uninjured | "The goblin looks fresh and ready to fight." |
| 76-99% | Scratched | "The goblin has minor cuts but seems unfazed." |
| 51-75% | Wounded | "The goblin is bleeding and moves more cautiously." |
| 26-50% | Badly Hurt | "The goblin staggers, favoring a wounded leg." |
| 11-25% | Near Death | "The goblin can barely stand, gasping for breath." |
| 1-10% | Critical | "The goblin is on death's door, barely conscious." |

**Example `examine goblin`:**
```
> examine goblin

  GOBLIN SCOUT
  ─────────────────────────────────────
  The goblin is badly hurt. It staggers and
  clutches a bleeding wound on its side.
  Blood drips onto the floor with each step.
  
  Status: [Bleeding] — 3 turns remaining
```

### 3.3 HP Bar Colors

| HP Percentage | Color |
|---------------|-------|
| 76-100% | Green |
| 51-75% | Yellow |
| 26-50% | Orange |
| 1-25% | Red |
| 0% | Gray (DEAD) |

### 3.3 Status Effect Display

| Status | Color | Symbol |
|--------|-------|--------|
| Buff (positive) | Green | `[Focused]` |
| Debuff (negative) | Red | `[Bleeding]` |
| Control (stun/root) | Purple | `[Stunned]` |
| Neutral | Gray | `[—]` (no effects) |

### 3.4 Death State

```
Goblin: DEAD  ────────────
```

---

## 4. Combat Log Panel

### 4.1 Event Types

| Event | Prefix | Color | Example |
|-------|--------|-------|---------|
| **Attack (hit)** | `→` | Cyan | `→ You attack Goblin for 12 damage` |
| **Attack (miss)** | `○` | Gray | `○ Goblin misses you` |
| **Critical hit** | `★` | Yellow | `★ CRITICAL HIT!` |
| **Heal** | `+` | Green | `+ Potion restores 15 HP` |
| **Status applied** | `[Status]` | Purple | `[Status] Goblin is now [Bleeding]` |
| **Status expired** | `[Status]` | Gray | `[Status] [Focused] has worn off` |
| **Death** | `✗` | Red | `✗ Goblin has been slain!` |
| **Flee attempt** | `◇` | Yellow | `◇ You attempt to flee...` |
| **Round marker** | `───` | White | `─── ROUND 4 ───` |

### 4.2 Damage Display Modes

> [!NOTE]
> **Design Decision:** Damage numbers are **hidden by default**. Players see narrative feedback, not "12 damage."

**Default Mode (Narrative):**
```
→ You slash the Goblin with your longsword
★ CRITICAL HIT! The blow nearly cleaves it in two!
[Status] The Goblin begins bleeding profusely
```

**Verbose Mode (Numbers — Settings Toggle):**
```
→ You attack Goblin for 12 damage
   [Accuracy: 8d10 → 3 successes vs 1 defense]
   [Damage: 2d8+3 = 15, soaked 3]
```

### 4.3 Log Buffer

| Setting | Value |
|---------|-------|
| Visible lines | 4 (default) |
| Buffer size | 50 entries |
| Scroll | `[PgUp/PgDn]` |

---

## 5. Smart Commands (Combat Context)

### 5.1 Dynamic Command Generation

Smart commands adapt to the current combat state:

**Player's turn, healthy:**
```
[1] attack goblin  [2] use Skewer on orc  [3] defend  [4] flee
```

**Player's turn, low HP:**
```
[1] defend  [2] use Health Potion  [3] flee  [4] attack goblin
```

**Player's turn, enemy low:**
```
[1] attack goblin (finish!)  [2] attack orc  [3] defend
```

### 5.2 Smart Command Logic

| Condition | Suggested Action |
|-----------|------------------|
| Player HP < 25% | Prioritize `defend`, `heal`, `flee` |
| Enemy HP < 20% | Show `(finish!)` indicator |
| Ability available | Include strongest ability |
| Counter-attack ready | Show `defend` for counter |

---

## 6. Action Feedback

### 6.1 Attack Resolution Display

**Hit:**
```
> attack goblin

  [Accuracy Roll: 8d10 (FINESSE 5 + Combat 3)]
  Roll: 10, 8, 7, 5, 4, 3, 2, 1  → 2 successes
  Goblin Defense: 1 success
  ✓ HIT! (1 net success)
  
  [Damage: 2d8 + 3 (Longsword + MIGHT)]
  Roll: 7 + 5 + 3 = 15
  Goblin Soak: 3
  → 12 damage!
  
  Goblin: 20/30 → 8/30 HP
```

**Critical Hit (5+ net successes):**
```
  ★ CRITICAL HIT! (5 net successes)
  [Damage dice DOUBLED]
  
  [Damage: 4d8 + 3]
  Roll: 8 + 7 + 6 + 4 + 3 = 28
  ...
```

**Miss:**
```
> attack orc

  [Accuracy Roll: 8d10]
  Roll: 6, 5, 4, 3, 3, 2, 2, 1  → 0 successes
  ○ MISS
```

### 6.2 Ability Use Feedback

```
> use skewer on orc

  [Skewer II — 35 Stamina]
  Stamina: 55 → 20
  
  [Accuracy Roll: 10d10]
  Roll: 9, 8, 8, 7, 6, 5, 4, 3, 2, 1  → 3 successes
  ✓ HIT!
  
  [Damage: 2d8 + 1d10 + 5]
  → 18 damage!
  [Bleeding] applied (3 turns)
```

### 6.3 Defense Feedback

```
> defend

  You raise your guard.
  [+2 Soak until your next turn]
  [Counter-attack enabled]
```

### 6.4 Flee Feedback

**Success:**
```
> flee

  [Flee Attempt]
  Your FINESSE (5) vs Enemy average (3)
  Roll: 6, 8, 9, 4, 7 → 2 successes
  Enemy: 5, 3, 2 → 0 successes
  ✓ ESCAPE!
  
  You break away from combat!
```

**Failure:**
```
> flee

  [Flee Attempt]
  Your FINESSE (4) vs Enemy average (5)
  Roll: 3, 2, 4, 5 → 0 successes
  Enemy: 8, 9, 6, 4, 3 → 2 successes
  ✗ BLOCKED!
  
  The enemies cut off your escape. You lose your turn.
```

---

## 7. Round Markers

### 7.1 Round Start Display

```
─────────────────── ROUND 4 ───────────────────
```

### 7.2 Round Summary (Optional — Configurable)

> [!NOTE]
> **Design Decision:** Round summaries are optional and can be enabled in settings.

At round end (if enabled):

```
─────────────────── ROUND 3 SUMMARY ───────────────────
  Your wounds: Minor scratches
  Enemy status: Goblin badly hurt, Orc wounded
  Status effects: [Bleeding] applied to Goblin
───────────────────────────────────────────────────────
```

---

## 8. Victory/Defeat Screens

### 8.1 Victory

```
╔══════════════════════════════════════════════════════════════╗
║                        ★ VICTORY ★                           ║
╟──────────────────────────────────────────────────────────────╢
║  Enemies Slain: 2                                            ║
║  Rounds: 4                                                   ║
╟──────────────────────────────────────────────────────────────╢
║  REWARDS                                                     ║
║  ──────────────────────────                                  ║
║  Legend: +15                                                 ║
║  Gold: 47                                                    ║
║  [Uncommon] Rusty Shortsword                                 ║
║  Health Draught x2                                           ║
╟──────────────────────────────────────────────────────────────╢
║  Stress: -10 (combat victory)                                ║
╚══════════════════════════════════════════════════════════════╝

Press [Enter] to continue...
```

### 8.2 Defeat

```
╔══════════════════════════════════════════════════════════════╗
║                        ✗ DEFEAT ✗                            ║
╟──────────────────────────────────────────────────────────────╢
║  You have fallen in battle.                                  ║
║                                                              ║
║  [Death Save required...]                                    ║
╚══════════════════════════════════════════════════════════════╝
```

---

## 9. GUI Combat Panel

### 9.1 Layout

```
┌───────────────────────────────────────────────────────────────────────┐
│  COMBAT — Round 3                                          YOUR TURN │
├───────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────────┐   │
│ │                          COMBAT GRID                            │   │
│ │   ┌───┬───┬───┬───┬───┬───┐                                     │   │
│ │   │   │   │   │   │   │   │                                     │   │
│ │   │   │ P │   │   │ E │   │  ← Visual grid with character icons │   │
│ │   │   │   │   │ A │   │ E │                                     │   │
│ │   └───┴───┴───┴───┴───┴───┘                                     │   │
│ └─────────────────────────────────────────────────────────────────┘   │
├───────────────────────────────────────────────────────────────────────┤
│ ALLIES                           │ ENEMIES                           │
│ ┌───────────────────────────────┐│┌───────────────────────────────┐  │
│ │ [P] YOU       45/60 ████████░░││ [E] Goblin  12/30 ████░░░░░░   │  │
│ │     [Focused]                 ││     [Bleeding]                 │  │
│ │ [A] Bjorn     30/40 ████████░░││ [E] Orc     25/50 █████░░░░░   │  │
│ │     [—]                       ││     [—]                        │  │
│ └───────────────────────────────┘│└───────────────────────────────┘  │
├───────────────────────────────────────────────────────────────────────┤
│ COMBAT LOG                                                            │
│ ┌───────────────────────────────────────────────────────────────────┐│
│ │ → You attack Goblin for 12 damage                                 ││
│ │ ★ CRITICAL HIT!                                                   ││
│ │ [Status] Goblin is now [Bleeding]                                 ││
│ │ → Goblin attacks you for 5 damage                                 ││
│ └───────────────────────────────────────────────────────────────────┘│
├───────────────────────────────────────────────────────────────────────┤
│ ACTIONS                                                               │
│ ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐           │
│ │  ⚔ Attack  │ │  🛡 Defend │ │ ⚡ Skewer  │ │  🏃 Flee   │           │
│ │  [1]       │ │  [2]       │ │  [3] 35 St │ │  [4]       │           │
│ └────────────┘ └────────────┘ └────────────┘ └────────────┘           │
├───────────────────────────────────────────────────────────────────────┤
│ TURN ORDER: → YOU • Goblin • Orc • Bjorn                              │
└───────────────────────────────────────────────────────────────────────┘
```

### 9.2 GUI Components

| Component | Description |
|-----------|-------------|
| **Combat Grid** | Interactive tactical grid with character sprites |
| **Combatant Cards** | Clickable cards showing HP, status, abilities |
| **Combat Log** | Scrollable event feed with filters |
| **Action Bar** | Button bar with hotkeys and ability icons |
| **Turn Order Track** | Horizontal initiative tracker |

### 9.3 Interactive Elements

| Element | Click Action | Hover Action |
|---------|--------------|--------------|
| **Enemy card** | Target for attack | Show stats tooltip |
| **Ability button** | Use ability | Show ability details |
| **Grid cell** | Move to cell | Highlight path |
| **Status icon** | — | Show status description |

---

## 10. CombatViewModel

### 10.1 Interface

```csharp
public interface ICombatViewModel
{
    // State
    bool IsInCombat { get; }
    int CurrentRound { get; }
    Guid CurrentTurnId { get; }
    bool IsPlayerTurn { get; }
    
    // Participants
    IReadOnlyList<CombatantViewModel> Allies { get; }
    IReadOnlyList<CombatantViewModel> Enemies { get; }
    IReadOnlyList<CombatantViewModel> TurnOrder { get; }
    
    // Grid
    CombatGridViewModel? Grid { get; }
    
    // Actions
    IReadOnlyList<CombatActionViewModel> AvailableActions { get; }
    CombatantViewModel? SelectedTarget { get; set; }
    
    // Log
    IReadOnlyList<CombatLogEntry> CombatLog { get; }
    
    // Commands
    ICommand AttackCommand { get; }
    ICommand DefendCommand { get; }
    ICommand UseAbilityCommand { get; }
    ICommand FleeCommand { get; }
    ICommand EndTurnCommand { get; }
    
    // Events
    event Action<CombatResultArgs> OnCombatEnded;
}

public record CombatantViewModel(
    Guid Id,
    string Name,
    bool IsPlayer,
    bool IsAlly,
    int CurrentHp,
    int MaxHp,
    float HpPercentage,
    IReadOnlyList<StatusEffectViewModel> StatusEffects,
    bool IsDead,
    bool IsCurrentTurn
);

public record CombatActionViewModel(
    int Hotkey,
    string Name,
    string? TargetName,
    int? StaminaCost,
    bool IsEnabled,
    string? DisabledReason
);
```

---

## 11. Configuration

| Setting | Default | Options |
|---------|---------|--------|
| `VerboseMode` | false | true/false (shows damage numbers) |
| `ShowAccuracyRolls` | false | true/false (verbose mode only) |
| `CombatLogLines` | 4 | 2-8 |
| `ShowRoundSummary` | false | true/false |
| `AnimateAttacks` | true | true/false (GUI only) |

---

## 12. Implementation Status

| Component | TUI Status | GUI Status |
|-----------|------------|------------|
| Combat screen layout | ❌ Planned | ❌ Planned |
| Combatant HP bars | ❌ Planned | ❌ Planned |
| Turn order display | ❌ Planned | ❌ Planned |
| Combat grid | ❌ Planned | ❌ Planned |
| Combat log | ❌ Planned | ❌ Planned |
| Smart commands | ❌ Planned | ❌ Planned |
| Attack feedback | ❌ Planned | ❌ Planned |
| Victory/Defeat screens | ❌ Planned | ❌ Planned |
| CombatViewModel | ❌ Planned | ❌ Planned |

---

## 13. Phased Implementation Guide

### Phase 1: Core Combat Display
- [ ] Combat screen layout with box drawing
- [ ] Combatant list with HP bars
- [ ] Turn order panel
- [ ] `[Combat] >` prompt

### Phase 2: Action System
- [ ] Smart command generation
- [ ] Attack resolution display
- [ ] Ability use feedback
- [ ] Defend/Flee feedback

### Phase 3: Combat Log
- [ ] Event type formatting
- [ ] Color coding
- [ ] Scroll support
- [ ] Round markers

### Phase 4: Victory/Defeat
- [ ] Victory screen with rewards
- [ ] Defeat screen
- [ ] Legend/loot display

### Phase 5: GUI Implementation
- [ ] CombatViewModel
- [ ] Combat grid with sprites
- [ ] Action button bar
- [ ] Animations and visual effects

---

## 14. Testing Requirements

### 14.1 TUI Tests
- [ ] HP bars render correctly at all percentages
- [ ] Status effects display with correct colors
- [ ] Turn order updates when combatant dies
- [ ] Smart commands prioritize correctly
- [ ] Combat log scrolls properly

### 14.2 GUI Tests
- [ ] Grid click selects target
- [ ] Action buttons enable/disable correctly
- [ ] Animations complete properly
- [ ] Turn order track updates

### 14.3 Integration Tests
- [ ] Full combat: Init → Attack → Victory → Rewards
- [ ] Flee: Attempt → Success → Exit combat
- [ ] Death: HP = 0 → Defeat screen

---

## 15. Related Specifications

| Spec | Relationship |
|------|--------------|
| [combat-resolution.md](../03-combat/combat-resolution.md) | Core combat mechanics |
| [commands/combat.md](commands/combat.md) | Combat command syntax |
| [tui-layout.md](tui-layout.md) | Screen composition |
| [terminal-adapter.md](terminal-adapter.md) | Terminal rendering |
| [status-effects.md](../03-combat/status-effects.md) | Status effect system |

---

## 16. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |
