# Design Review Checklist

**Purpose**: Reviewer validation tool for completed Enemy Design Worksheets before implementation begins.

**When to Use**: After a designer completes `/docs/templates/enemy-design-worksheet.md`, use this checklist to validate balance, thematic fit, and compliance with SPEC-COMBAT-012 before code changes.

**Workflow**: Proposal → Worksheet → **THIS CHECKLIST** → Implementation → Bestiary Entry

---

## Review Information

**Reviewer**: ___________________________ **Date**: ___________

**Designer**: ___________________________ **Worksheet Date**: ___________

**Enemy Name**: _______________________________________________

**Proposed Threat Tier**: _______________  **Proposed Archetype**: _______________

---

## Section 1: Thematic Consistency

**Aethelgard Lore Compliance**:
- [ ] Enemy concept aligns with v0.24 narrative constraints (no pre-Collapse tech)
- [ ] Enemy fits established faction/region (Vault, Dormant, Forlorn, etc.)
- [ ] Visual/aesthetic description matches grimdark industrial fantasy tone
- [ ] Thematic role (1-2 sentences) is clear and compelling

**Narrative Integration**:
- [ ] Enemy spawn method is appropriate (procedural vs. scripted encounter)
- [ ] If IsForlorn: Trauma mechanics fit enemy's traumatic nature (psychic horror, existential dread)
- [ ] If IsBoss: Enemy justifies boss-tier treatment (major narrative threat)

**Design Intent Validation**:
- [ ] "What challenge does this enemy create?" is answered clearly
- [ ] Challenge encourages diverse player strategies (not just stat check)
- [ ] Enemy fills identified gap in roster or justifies redundancy

**Issues Found**:

_____________________________________________________________________________

---

## Section 2: Stat Budget Compliance

**Reference**: SPEC-COMBAT-012 Threat Tier Guidelines (Section 3.1)

**HP Allocation**:
- [ ] HP is within tier range (Worksheet Step 2)
  - Low: 10-15 | Medium: 25-50 | High: 60-70 | Lethal: 80-90 | Boss: 75-100
- [ ] HP matches archetype pattern:
  - Tank: High end of range | Glass Cannon: Low end (10-20) | Standard: Mid-range

**Attribute Budget**:
- [ ] Total attribute points within tier budget:
  - Low: 5-10 | Medium: 8-16 | High: 12-17 | Lethal: 13-20 | Boss: 13-20
- [ ] Attributes match archetype pattern:
  - Tank: High STURDINESS | Caster: High WILL (4-7) | DPS: Balanced
  - Glass Cannon: High MIGHT/FINESSE, low STURDINESS
  - Support: High WITS (4-5)

**Damage Budget**:
- [ ] Damage dice + bonus within tier range:
  - Low: 1d6 to 1d6+2 | Medium: 1d6+1 to 2d6 | High: 2d6+2 to 3d6
  - Lethal: 3d6+4 to 4d6 | Boss: 2d6 to 3d6+3
- [ ] Average damage calculated correctly (Worksheet Step 2, line 74)
- [ ] Damage variance acceptable (max damage / min damage):
  - Lethal/Boss: < 4:1 ratio (e.g., 4d6 = 24 max / 4 min = 6:1 is too high)

**Legend Value**:
- [ ] Legend value within tier range:
  - Low: 10-20 | Medium: 15-50 | High: 55-75 | Lethal: 60-100 | Boss: 100-150
- [ ] Legend/HP ratio is 0.5-1.0 (divide Legend by HP)
- [ ] Special mechanics bonuses applied correctly:
  - Self-heal/buffs: +10-20 | Boss mechanics: +20-50

**Issues Found**:

_____________________________________________________________________________

---

## Section 3: Special Mechanics Validation

**IsForlorn (Trauma Aura)**:
- [ ] If IsForlorn: Stress/Corruption values specified (Worksheet Step 3)
- [ ] Trauma cost matches tier guidelines:
  - Medium: 10-25 total | High: 15-25 | Lethal: 25-40 | Boss: 30-80
- [ ] Trauma mechanics have thematic justification (psychic horror, existential dread)

**IsBoss (Multi-Phase Combat)**:
- [ ] If IsBoss: Number of phases specified (2-3 typical)
- [ ] Phase HP thresholds are clear (e.g., Phase 2 at ≤50% HP)
- [ ] Phase-based AI probabilities defined in Step 4

**Soak (Flat Damage Reduction)**:
- [ ] Soak value within cap: **Max 4 for non-boss, Max 6 for boss**
- [ ] Effective HP multiplier calculated (Worksheet Step 3, line 118)
- [ ] If Soak > 4 for non-boss: Designer justified reasoning (no bullet sponges)

**v0.18 Balance Warning - Soak Abuse Check**:
- [ ] **CRITICAL**: No non-boss enemies with Soak > 4 (Vault Custodian lesson)
- [ ] **CRITICAL**: No boss enemies with Soak > 6 (Omega Sentinel lesson)

**Other Special Abilities**:
- [ ] Self-healing abilities have cooldown or HP threshold triggers
- [ ] Buff/debuff abilities have clear probability distribution (Step 4)
- [ ] Poison/DoT damage specified with duration
- [ ] Summon mechanics do not create performance issues (< 5 summons per boss)

**Issues Found**:

_____________________________________________________________________________

---

## Section 4: AI Behavior Pattern Validation

**Archetype Pattern Match**:
- [ ] AI probabilities match selected archetype (Worksheet Step 4):
  - Aggressive (Glass Cannon, DPS, Swarm): 70-90% offense
  - Defensive (Tank): 40-50% attack, 30-40% defense, 20-30% utility
  - Tactical (Support, Caster): 30-50% attack, 30-40% buff/debuff, 20-30% heal
  - Phase-Based (Boss, Mini-Boss): Escalating aggression across phases

**Probability Distribution**:
- [ ] AI probabilities total 100% (or close enough for Random.Next(100))
- [ ] Primary action has highest probability (no 5-way ties)
- [ ] Low-probability actions (< 10%) have clear situational triggers

**Phase Logic (if applicable)**:
- [ ] Phase 1 probabilities favor conservative/standard attacks
- [ ] Phase 2 probabilities increase special abilities/AoE (by 20-30%)
- [ ] Phase 3 probabilities favor desperation ultimates (50%+)
- [ ] Phase transitions based on HP thresholds, not turn count

**Special AI Rules**:
- [ ] Low HP triggers (< 30% HP) have clear actions (self-heal, flee)
- [ ] Flee conditions appropriate for archetype (Glass Cannon only)
- [ ] Ally-dependent logic makes sense (Support buffs allies, not self)

**Issues Found**:

_____________________________________________________________________________

---

## Section 5: Balance Validation

**Time-to-Kill (TTK) Targets**:
- [ ] Estimated TTK calculated (Worksheet Step 5, lines 188-201)
- [ ] TTK within tier target:
  - Low: 2-3 turns | Medium: 4-6 turns | High: 7-10 turns
  - Lethal: 10-15 turns | Boss: 12-20 turns
- [ ] TTK accounts for Soak (effective HP multiplier applied)

**Damage Output Validation**:
- [ ] Enemy average damage calculated (XdY+Z → avg = (X × 3.5) + Z)
- [ ] Enemy damage as % of player HP (assume 30 HP @ Legend 1):
  - Low: 10-20% (3-6 damage) | Medium: 15-30% (5-9 damage)
  - High: 25-40% (8-12 damage) | Lethal: 30-50% (9-15 damage) | Boss: 25-45% (8-14 damage)

**v0.18 Balance Warning - One-Shot Prevention**:
- [ ] **CRITICAL**: Enemy max damage < 80% of player HP (prevent one-shots)
  - Lethal tier: Max 4d6 (24 damage max) vs. 30 HP = 80% is acceptable
  - Lethal tier: 4d6+3 (27 damage max) vs. 30 HP = 90% is **TOO HIGH**
- [ ] **CRITICAL**: Only Boss/Lethal can threaten 2-shot (> 40% HP per hit)

**Damage Variance Check**:
- [ ] Max damage / min damage ratio acceptable:
  - Low/Medium: Any variance OK (small numbers)
  - High: < 3:1 ratio | Lethal/Boss: < 4:1 ratio
- [ ] If variance too high: Recommend reducing dice count, increasing damage bonus

**Issues Found**:

_____________________________________________________________________________

---

## Section 6: Loot Quality Mapping

**Reference**: SPEC-COMBAT-012 FR-007 (Loot System Integration)

**Loot Tier Compliance**:
- [ ] Loot quality matches threat tier (Worksheet Step 5, lines 212-218):
  - Low: Tier 0-1 (60-80% drop rate) | Medium: Tier 1-2 (50-70%)
  - High: Tier 2-3 (60-80%) | Lethal: Tier 3 (70-90%) | Boss: Tier 4 (70% guaranteed)
- [ ] If IsBoss: 70% Tier 4, 30% Tier 3 confirmed
- [ ] Class-appropriate filtering applied (60% standard, 100% boss)

**Legend-to-Loot Ratio**:
- [ ] Legend value justifies loot quality (High Legend → High Tier loot)
- [ ] No "loot pinata" enemies (Low HP, High Legend, guaranteed Tier 4)

**Issues Found**:

_____________________________________________________________________________

---

## Section 7: Roster Diversity & v0.18 Pitfall Avoidance

**Roster Diversity Check**:
- [ ] Tier representation: Does this tier have < 5 enemies? (fills gap) OR > 5 enemies? (justified redundancy)
- [ ] Archetype representation: Does this archetype have < 3 enemies? (fills gap) OR > 3 enemies? (justified redundancy)
- [ ] Enemy creates unique challenge (not just "Sentry-04 but with +10 HP")

**v0.18 Lessons Learned - Critical Pitfalls**:

**Pitfall 1: One-Shot Deaths**:
- [ ] **VERIFIED**: No enemy deals > 80% player HP in single hit
  - Example fix: Failure Colossus 4d6+3 → 3d6+4 (prevent one-shots)

**Pitfall 2: Damage Variance**:
- [ ] **VERIFIED**: Lethal/Boss max/min damage ratio < 4:1
  - Example fix: Sentinel Prime 5d6 → 4d6 (reduce unfair variance)

**Pitfall 3: Bullet Sponge (Soak Abuse)**:
- [ ] **VERIFIED**: Soak ≤ 4 for non-boss, Soak ≤ 6 for boss
  - Example fix: Vault Custodian Soak 6 → 4, Omega Sentinel Soak 8 → 6

**Pitfall 4: Stat Bloat**:
- [ ] **VERIFIED**: Attributes within tier budget (no 20+ attr for Medium tier)

**Pitfall 5: Legend Mismatch**:
- [ ] **VERIFIED**: Legend/HP ratio is 0.5-1.0

**Issues Found**:

_____________________________________________________________________________

---

## Section 8: Implementation Checklist Validation

**Code Files Identified**:
- [ ] Worksheet Step 6 lists all required files (Enemy.cs, EnemyFactory.cs, EnemyAI.cs, LootService.cs)
- [ ] If procedural spawn: DormantProcess.cs included
- [ ] If scripted encounter: Quest/event trigger file specified

**AI Behavior Pattern Template Reference**:
- [ ] Worksheet Step 4 references `/docs/templates/ai-behavior-pattern-template.md`
- [ ] Designer selected appropriate template (1-5) for archetype

**Bestiary Entry Planned**:
- [ ] Worksheet Step 6 includes bestiary entry task
- [ ] Bestiary entry will use `/docs/templates/enemy-bestiary-entry.md` template

**Testing Plan**:
- [ ] Designer specified test encounter or spawn method
- [ ] TTK playtest plan exists (solo player, appropriate Legend tier)

**Issues Found**:

_____________________________________________________________________________

---

## Final Approval Decision

**Summary of Issues** (consolidate all "Issues Found" from Sections 1-8):

_____________________________________________________________________________

_____________________________________________________________________________

_____________________________________________________________________________

**Revision Requests** (specific changes required before approval):

_____________________________________________________________________________

_____________________________________________________________________________

_____________________________________________________________________________

**Approval Decision**:
- [ ] **APPROVED** → Designer may proceed to implementation (modify code files)
- [ ] **APPROVED WITH MINOR REVISIONS** → Address issues above, no re-review needed
- [ ] **REVISE AND RESUBMIT** → Major issues found, requires full re-review after fixes
- [ ] **REJECTED** → Fundamental design flaws, recommend new proposal

**Reviewer Signature**: ___________________________ **Date**: ___________

---

## Next Steps (If Approved)

**For Designer**:
1. **Implementation**: Follow Worksheet Step 6 to modify code files
   - Add `EnemyType.[YourEnemy]` to `Enemy.cs`
   - Implement `Create[YourEnemy]()` in `EnemyFactory.cs`
   - Implement `Determine[YourEnemy]Action()` in `EnemyAI.cs`
   - Implement `Generate[YourEnemy]Loot()` in `LootService.cs`
   - (Optional) Add spawn table entry to `DormantProcess.cs`

2. **Testing**: Build, run, playtest
   - Verify stats match worksheet
   - Verify AI probabilities match behavior pattern
   - Playtest TTK and difficulty feel
   - Verify loot drops

3. **Documentation**: Create bestiary entry
   - Use `/docs/templates/enemy-bestiary-entry.md` template
   - Document final stats, abilities, AI behavior, loot table
   - Include v0.X balance notes if adjustments made during testing

4. **Commit**: Commit all changes with descriptive message
   - Reference worksheet and review in commit message
   - Include bestiary entry in same commit

**For Reviewer**:
- [ ] Archive this completed checklist (save to `/docs/reviews/[enemy-name]-review-[date].md`)
- [ ] Track design patterns that work well (share with team)
- [ ] Update SPEC-COMBAT-012 if new best practices discovered

---

**Reference Documentation**:
- **SPEC-COMBAT-012**: `/docs/00-specifications/combat/enemy-design-spec.md` (Full design system, 1,494 lines)
- **Enemy Design Worksheet**: `/docs/templates/enemy-design-worksheet.md` (7-step design process being reviewed)
- **AI Behavior Pattern Template**: `/docs/templates/ai-behavior-pattern-template.md` (AI implementation reference)
- **Enemy Bestiary Entry**: `/docs/templates/enemy-bestiary-entry.md` (Post-implementation documentation)
