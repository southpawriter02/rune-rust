# Enemy Design Proposal

**Purpose**: Lightweight concept pitch template for proposing new enemies before committing to full design worksheet.

**When to Use**: Before starting the Enemy Design Worksheet, use this to pitch concepts for team review and get early feedback on thematic fit, roster diversity, and design intent.

**Next Step**: Once approved, proceed to `/docs/templates/enemy-design-worksheet.md` for full stat allocation and implementation planning.

---

## Proposal Overview

**Proposed By**: ___________________________ **Date**: ___________

**Enemy Name**: _______________________________________________

**One-Sentence Concept**:

_____________________________________________________________________________

**Thematic Justification** (Why does this enemy belong in Aethelgard? How does it fit the world lore?):

_____________________________________________________________________________

_____________________________________________________________________________

_____________________________________________________________________________

---

## Design Parameters

**Proposed Threat Tier** (select one):
- [ ] **Low** (Common mobs, tutorial enemies, 2-3 turn TTK)
- [ ] **Medium** (Standard encounters, 4-6 turn TTK)
- [ ] **High** (Dangerous foes, 7-10 turn TTK)
- [ ] **Lethal** (Elite enemies, 10-15 turn TTK)
- [ ] **Boss** (Major encounters, 12-20 turn TTK, multi-phase)

**Proposed Archetype** (select one):
- [ ] **Tank** (High HP/defense, draws aggro, protects allies)
- [ ] **DPS** (Consistent damage dealer, balanced stats)
- [ ] **Glass Cannon** (High damage, low survivability)
- [ ] **Support** (Buffs allies, debuffs player, tactical)
- [ ] **Swarm** (Numbers-based threat, low individual stats)
- [ ] **Caster** (Magic damage, psychic attacks, high WILL)
- [ ] **Mini-Boss** (Phase mechanics, higher HP, no IsBoss flag)
- [ ] **Boss** (Multi-phase, IsBoss flag, guaranteed Tier 4 loot)

**Special Mechanics** (check all that apply):
- [ ] **IsForlorn** (Trauma Aura - inflicts Psychic Stress/Corruption)
- [ ] **IsBoss** (Multi-phase AI, boss UI, guaranteed Tier 4 loot)
- [ ] **Soak** (Flat damage reduction, specify value: _____)
- [ ] **Self-Healing** (Guardian Protocol, Last Stand, etc.)
- [ ] **Summons/Reinforcements** (Boss-tier mechanics)
- [ ] **Buff/Debuff Abilities** (Support archetype)
- [ ] **Poison/DoT Effects** (Sludge-Crawler style)
- [ ] **Other**: _____________________________________________________________

---

## Design Intent

**What challenge does this enemy create for the player?**

_____________________________________________________________________________

_____________________________________________________________________________

**What gap in the current enemy roster does this fill?** (e.g., "We lack a Medium-tier caster", "No Glass Cannon enemies exist yet"):

_____________________________________________________________________________

_____________________________________________________________________________

**How does this enemy encourage diverse player strategies?** (e.g., "Forces careful positioning", "Rewards high-Wits builds"):

_____________________________________________________________________________

_____________________________________________________________________________

---

## Roster Diversity Check

**Current Enemy Count by Threat Tier** (reference `/docs/00-specifications/combat/enemy-design-spec.md` Appendix A):

| Tier | Current Count | After This Enemy |
|------|---------------|------------------|
| Low | _____ | _____ |
| Medium | _____ | _____ |
| High | _____ | _____ |
| Lethal | _____ | _____ |
| Boss | _____ | _____ |

**Current Enemy Count by Archetype**:

| Archetype | Current Count | After This Enemy |
|-----------|---------------|------------------|
| Tank | _____ | _____ |
| DPS | _____ | _____ |
| Glass Cannon | _____ | _____ |
| Support | _____ | _____ |
| Swarm | _____ | _____ |
| Caster | _____ | _____ |
| Mini-Boss | _____ | _____ |
| Boss | _____ | _____ |

**Diversity Assessment**:
- [ ] This enemy fills an underrepresented tier (< 5 enemies in tier)
- [ ] This enemy fills an underrepresented archetype (< 3 enemies in archetype)
- [ ] This enemy creates redundancy (tier/archetype already well-represented)
- [ ] Redundancy is justified because: _______________________________________

---

## Thematic Consistency Validation

**Aethelgard Lore Compliance** (check all that apply):
- [ ] Enemy fits established faction/region (Vault, Dormant, Forlorn, etc.)
- [ ] Enemy respects v0.24 narrative constraints (no pre-Collapse technology)
- [ ] Enemy visual/aesthetic aligns with grimdark industrial fantasy tone
- [ ] Enemy behavior/AI matches thematic role (e.g., Forlorn = trauma-inducing)

**Narrative Integration**:
- [ ] Enemy can spawn naturally via procedural generation (DormantProcess.cs)
- [ ] Enemy requires scripted encounter (specific quest/location only)
- [ ] Enemy is boss-tier (unique encounter, one-time spawn)

**Lore Notes** (optional - how does this enemy tie into Aethelgard's history or factions?):

_____________________________________________________________________________

_____________________________________________________________________________

---

## Potential Implementation Risks

**Design Complexity** (select one):
- [ ] **Low** (Standard stats, existing AI pattern, no special mechanics)
- [ ] **Medium** (Moderate stats, standard AI with 1-2 special abilities)
- [ ] **High** (Complex AI pattern, phase mechanics, unique abilities)
- [ ] **Very High** (Boss-tier, 3+ phases, multiple unique mechanics)

**Known Risks** (check all that apply):
- [ ] **Balance Risk**: Tier/archetype combination untested (e.g., Low-tier Boss)
- [ ] **AI Complexity**: Requires new AI behavior pattern (not covered by existing templates)
- [ ] **Code Changes**: Requires new mechanics in `CombatEngine.cs` or `EffectManager.cs`
- [ ] **Loot System Impact**: Requires new loot tables or drop logic
- [ ] **Performance Risk**: Summons/swarm mechanics may impact turn processing
- [ ] **Other**: _____________________________________________________________

**Mitigation Plan** (if risks identified):

_____________________________________________________________________________

_____________________________________________________________________________

---

## Approval Checklist

**Reviewer**: ___________________________ **Date**: ___________

**Thematic Fit**:
- [ ] Enemy concept aligns with Aethelgard lore and v0.24 narrative constraints
- [ ] Enemy fills thematic gap or justifies redundancy

**Roster Diversity**:
- [ ] Tier/archetype combination needed or justified
- [ ] Enemy does not create excessive redundancy (< 5 in tier, < 3 in archetype)

**Design Intent**:
- [ ] Challenge is clear and compelling
- [ ] Encourages diverse player strategies (not just stat check)

**Implementation Feasibility**:
- [ ] Design complexity matches available development time
- [ ] No major code refactors required (or justified if needed)

**Decision**:
- [ ] **APPROVED** → Proceed to Enemy Design Worksheet (`/docs/templates/enemy-design-worksheet.md`)
- [ ] **APPROVED WITH REVISIONS** → Address feedback below before worksheet:

_____________________________________________________________________________

_____________________________________________________________________________

- [ ] **REJECTED** → Reason:

_____________________________________________________________________________

_____________________________________________________________________________

---

## Next Steps (If Approved)

1. **Proceed to Enemy Design Worksheet**: `/docs/templates/enemy-design-worksheet.md`
   - Complete 7-step design process (Core Identity → Implementation Checklist)
   - Allocate stat budgets using SPEC-COMBAT-012 tier guidelines
   - Design AI behavior pattern using `/docs/templates/ai-behavior-pattern-template.md`

2. **Implementation**: Follow worksheet Step 6 to modify code files
   - `Enemy.cs` (add enum)
   - `EnemyFactory.cs` (create factory method)
   - `EnemyAI.cs` (implement AI behavior)
   - `LootService.cs` (add loot generation)

3. **Documentation**: Create bestiary entry using `/docs/templates/enemy-bestiary-entry.md`

4. **Validation**: Submit completed worksheet for design review using `/docs/templates/design-review-checklist.md`

---

**Reference Documentation**:
- **SPEC-COMBAT-012**: `/docs/00-specifications/combat/enemy-design-spec.md` (Threat tiers, stat budgets, archetype taxonomy)
- **Enemy Design Worksheet**: `/docs/templates/enemy-design-worksheet.md` (Full design process, 7 steps)
- **AI Behavior Template**: `/docs/templates/ai-behavior-pattern-template.md` (AI implementation code templates)
- **Design Review Checklist**: `/docs/templates/design-review-checklist.md` (Post-worksheet validation)
