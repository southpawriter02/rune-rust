# Domain Check Template

Use this template when creating new domain validation checks.

---

## Structure

```markdown
# Domain N: [Domain Name] Validation Check

**Domain ID:** `DOM-N`
**Severity:** P1-CRITICAL | P2-HIGH | P3-MEDIUM | P4-LOW
**Applies To:** [Content types this check applies to]

---

## Canonical Ground Truth

[Summary of the canonical rules this domain enforces]

### Key Constraints

- [Constraint 1]
- [Constraint 2]
- [Constraint N]

### Terminology Reference

| Approved | Forbidden | Reason |
|----------|-----------|--------|
| [term] | [term] | [why] |

---

## Validation Checklist

Run these checks against the content:

- [ ] [Check item 1]?
- [ ] [Check item 2]?
- [ ] [Check item N]?

---

## Common Violations

| Pattern | Example | Why It Fails |
|---------|---------|--------------|
| [pattern] | "[quoted example]" | [explanation] |

---

## Green Flags

These indicate the content is likely compliant:

- [Indicator 1]
- [Indicator 2]

---

## Remediation Strategies

### Option 1: [Strategy Name]

[Description of how to fix violations using this approach]

### Option 2: [Strategy Name]

[Alternative remediation approach]

---

## Decision Tree

[Flowchart or decision logic for complex edge cases]

---

## Examples

### PASS Example

**Content:** "[Example text that passes]"
**Why:** [Explanation]

### FAIL Example

**Content:** "[Example text that fails]"
**Violation:** [What rule it breaks]
**Remediation:** "[Corrected text]"
```
