# Documentation Audit Report

**Date:** 2025-12-08
**Auditor:** Jules
**Scope:** Templates, Specializations, and Status Effects (Active Specs)
**Reference Standard:** `docs/00-project/DOCUMENTATION_STANDARDS.md` and Golden Standards

---

## Executive Summary

A comprehensive audit was conducted on the documentation templates and active specifications. The review reveals widespread nonconformity with the "Golden Standards," particularly regarding sections that were likely added recently: **Balance Data**, **Voice Guidance**, and **Changelog**.

While the **Golden Standard files** (`Berserkr`, `Corridor Maker`, `Bleeding`) are perfect, the vast majority of other specs (and even the templates themselves) fail to meet these new requirements.

---

## 1. Golden Standard Verification

The following files were verified as 100% conformant and serve as valid references:

| Category | File | Status |
|----------|------|--------|
| **Specialization** | `docs/03-character/specializations/berserkr/berserkr-overview.md` | ✅ PASS |
| **Ability** | `docs/03-character/specializations/ruin-stalker/abilities/corridor-maker.md` | ✅ PASS |
| **Status Effect** | `docs/04-systems/status-effects/bleeding.md` | ✅ PASS |

---

## 2. Template Audit

The templates in `docs/.templates/` were reviewed. Several core templates are missing sections required by the `DOCUMENTATION_STANDARDS.md`.

| Template | Missing Sections | Status |
|----------|------------------|--------|
| `system.md` | **Voice Guidance** | 🔴 Nonconformant |
| `resource.md` | **Voice Guidance** | 🔴 Nonconformant |
| `craft.md` | **Voice Guidance** | 🔴 Nonconformant |
| `skill.md` | None (Conformant) | ✅ PASS |
| `ability.md` | None (Conformant) | ✅ PASS |
| `specialization.md` | None (Conformant) | ✅ PASS |
| `status-effect.md` | None (Conformant) | ✅ PASS |

> **Note:** The `DOCUMENTATION_STANDARDS.md` states "Required Sections (All Specs): ... Voice Guidance". The System, Resource, and Craft templates currently violate this.

---

## 3. Specification Audit

### 3.1 Specialization Overviews

| File | Missing Sections | Status |
|------|------------------|--------|
| `bone-setter/bone-setter-overview.md` | Balance Data, Voice Guidance, Implementation Status, Changelog | 🔴 Critical |
| `ruin-stalker/ruin-stalker-overview.md` | Balance Data, Voice Guidance, Implementation Status, Changelog | 🔴 Critical |
| `runasmidr/runasmidr-overview.md` | Situational Power, Synergies, Balance Data, Voice Guidance, Implementation Status, Changelog | 🔴 Critical |

### 3.2 Status Effects

Almost all status effects (except the Golden Standard `bleeding.md`) are missing the new required sections.

| File | Missing Sections | Status |
|------|------------------|--------|
| `corroded.md` | Voice Guidance, Balance Data, Changelog | 🔴 Critical |
| `disoriented.md` | Voice Guidance, Balance Data, Implementation Status, Changelog | 🔴 Critical |
| `feared.md` | Voice Guidance, Balance Data, Changelog | 🔴 Critical |
| `fortified.md` | Voice Guidance, Balance Data, Implementation Status, Changelog | 🔴 Critical |
| `hasted.md` | Voice Guidance, Balance Data, Implementation Status, Changelog | 🔴 Critical |
| `inspired.md` | Voice Guidance, Balance Data, Implementation Status, Changelog | 🔴 Critical |
| `poisoned.md` | Voice Guidance, Balance Data, Changelog | 🔴 Critical |
| `rooted.md` | Voice Guidance, Balance Data, Changelog | 🔴 Critical |
| `silenced.md` | Voice Guidance, Balance Data, Changelog | 🔴 Critical |
| `slowed.md` | Voice Guidance, Balance Data, Changelog | 🔴 Critical |
| `stunned.md` | Voice Guidance, Balance Data, Changelog | 🔴 Critical |

---

## 4. Recommendations

1.  **Update Templates:** Immediately update `system.md`, `resource.md`, and `craft.md` to include the required "Voice Guidance" section (even if just a placeholder or reference to tone).
2.  **Systematic Remediation:** A remediation plan is needed to bring the ~20 active specs up to the Golden Standard. This involves writing creative content (Voice Guidance) and analytical content (Balance Data) for each.
3.  **Update Tracker:** The `SPEC_REMEDIATION_TRACKER.md` should be updated to reflect that while the *templates* for Ability/Spec/Status are fixed, the *content* files (and other templates) are still largely nonconformant.
