# UFS-1204: Hook and Drag

Status: Proposed
Balance Validated: No
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent System:** Atgeir-Wielder Specialization (Tier 2)
**Version:** 5.1 (Consolidated)
**Last Updated:** 2025-11-29

## I. Identity & Governance (The Soul)

*The conceptual grounding of the ability. Must pass Setting Fundamental checks.*

- **Core Fantasy:** You are the disruption in the system. You do not wait for enemies to engage; you violently alter the geometry of the battlefield to suit your formation.
- **Narrative Description:** Using the recurved blade of the Atgeir, the wielder bypasses the frontline to snag a priority target hiding in the rear, yanking them into the kill zone.
- **Visual Metaphor:** A butcher’s hook meets a relentless anchor.
- **Governance Validation (Aethelgard Fundamentals):**
    - **No Magic:** :white_check_mark: Purely kinetic/physical leverage.
    - **No High-Tech Creation:** :white_check_mark: Relies on weapon shape and user strength, not active sci-fi gadgets.
    - **Tone:** :white_check_mark: "Visceral," "Disruptive."

## II. Mechanics & Logic (The Body)

*The mathematical and state-based rules. Used for implementation.*

### 1. Data Definitions

| Variable | Rank 2 (Base) | Rank 3 (Mastery) | Scaling Logic |
| --- | --- | --- | --- |
| **Resource Cost** | 45 Stamina | 45 Stamina | Flat |
| **Cooldown** | 4 Turns | 4 Turns | Flat |
| **Targeting** | Enemy [Back Row] | Enemy [Back Row] | Strict Positional |
| **Damage** | 3d8 Physical | 4d8 Physical | +1d8 |
| **Pull Bonus** | +2 Dice | +3 Dice | +1 Die |
| **On Success** | [Pull] + [Slowed] | [Pull] + [Stunned] | Status Upgrade |

### 2. Resolution Pipeline (The Code Logic)

*Sequence of operations for the Game Engine.*

1. **Validation Phase:**
    - `Check`: Player is in `CombatState`.
    - `Check`: Target is in `BackRow`.
    - `Check`: Player has >= 45 `Stamina`.
    - `Check`: `Cooldown_1204` <= 0.
2. **Cost Phase:**
    - `Deduct`: 45 `Stamina`.
    - `Set`: `Cooldown_1204` = 4.
3. **Calculation Phase (The "Hook"):**
    - **Step A (Attack Roll):** Calculate Hit vs. Evasion.
        - *If Miss:* Sequence Ends. Log: "Missed".
    - **Step B (The Pull Check):**
        - `AttackerPool` = MIGHT + `PullBonus` (2 or 3).
        - `DefenderPool` = STURDINESS + `StabilityMods`.
        - `Result`: If `AttackerHits` > `DefenderHits`, Success.
4. **Resolution Phase (On Success):**
    - `Apply`: Damage (Roll Xd8).
    - `Move`: Target Transform moves from `BackRow` -> `FrontRow` (Grid Interpolation).
    - `Apply Status`:
        - **Rank 2:** `[Slowed]` (1 Turn).
        - **Rank 3:** `[Stunned]` (1 Turn) + `[Slowed]` (1 Turn).
5. **Resolution Phase (On Fail - Optional Polish):**
    - `Apply`: Half Damage (Glancing blow).
    - `Log`: "Target resisted the pull."

### 3. Synergies & Tags

- **Tags:** `#ForcedMovement`, `#Melee`, `#Reach`, `#CrowdControl`.
- **Combo (Berserkr):** Pulls target into range of short-range burst attacks.
- **Counter (Casters):** Specifically targets low-Sturdiness back-liners.

## III. Presentation (The Face)

*The GUI, Audio, and VFX specifications. Used for Unity/Godot implementation.*

### 1. GUI Elements

- **Icon:** `:hook_icon:` A stylized Atgeir head hooked around a stylized silhouette.
    - *Border:* Silver (Rank 2) / Gold (Rank 3).
- **Targeting Reticle:**
    - When hovering a valid Back Row target, draw a **Red Dashed Line** from Target to the empty Front Row slot in front of the Player.
    - *Tooltip:* "75% Chance to Pull" (Calculated dynamically based on MIGHT vs EST. STURDINESS).
- **Floating Text:**
    - On Hit: `<< DRAGGED! >>` (Red text, bold).
    - On Damage: `24 HP`.

### 2. Animation & VFX

- **Pre-Cast:** Player model lowers stance, weapon extends (The "Reach" pose).
- **Execution:**
    - *Animation:* `Anim_Attack_Pull_Heavy`. Not a thrust, but a forward lunge followed by a violent retraction.
    - *VFX:* A "wind shear" distortion line follows the path of the hook.
    - *Impact:* If Pull succeeds, target sprite slides quickly (0.2s lerp) to front row. Dust particles at their feet to show resistance.
- **Audio:**
    - *Cast:* `sfx_weapon_whoosh_heavy.wav`
    - *Impact:* `sfx_flesh_impact_thud.wav`
    - *Pull Success:* `sfx_armor_scrape_gravel.wav` (The sound of them being dragged).

### 3. Rank Differentiation (Visuals)

- **Rank 2:** Standard metal weapon trail.
- **Rank 3:** The weapon trail has a subtle "distortion/glitch" effect (referencing the setting's Glitch nature), implying the force is so great it disturbs the air.

---