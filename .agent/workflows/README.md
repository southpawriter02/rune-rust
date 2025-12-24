# Agent Workflows

Workflows provide step-by-step procedures for common tasks. Unlike rules (which are always active), workflows are **context-specific** and invoked when needed.

## 📂 Workflow Categories

### development/ - Code Implementation Workflows
Workflows for implementing features and code changes.

| File | When to Use | Steps Overview |
|------|-------------|----------------|
| `feature-implementation.md` | Implementing a spec-to-code feature | Analysis → Contract → Tests → Implementation → Registration |
| `component-creation.md` | Creating UI components | ViewModel → View → Integration → Navigation |
| `database-migration.md` | Database schema changes | Draft SQL → Verify types → Seed data → Migration file |

**Use these when:** Working on C# code, UI components, or database changes.

---

### quality-assurance/ - Testing & Validation Workflows
Workflows for ensuring quality and consistency.

| File | When to Use | Steps Overview |
|------|-------------|----------------|
| `qa-spec.md` | Validating specification quality | Template check → Content audit → Cross-reference validation |
| `narrative-validation.md` | Validating in-game content | Domain compliance → Voice check → Precision audit |
| `validate-narrative.md` | Quick narrative validation | AAM compliance → Domain 4 check |

**Use these when:** Auditing specs, reviewing content, or ensuring narrative consistency.

---

### onboarding/ - Context & Getting Started
Workflows for gaining context and scaffolding new work.

| File | When to Use | Steps Overview |
|------|-------------|----------------|
| `gaining-context.md` | Understanding a system/topic | Cross-refs → Directory nav → Golden standards → Core mechanics |
| `prompt-scaffold.md` | Structuring prompts | Template → Context → Requirements → Validation |

**Use these when:** Starting work on unfamiliar code/content, or structuring complex requests.

---

## 🎯 Workflow Selection Guide

### "I need to implement a feature from a spec"
→ Use [development/feature-implementation.md](development/feature-implementation.md)

### "I need to create a new UI screen"
→ Use [development/component-creation.md](development/component-creation.md)

### "I need to add/modify database tables"
→ Use [development/database-migration.md](development/database-migration.md)

### "I need to validate a specification"
→ Use [quality-assurance/qa-spec.md](quality-assurance/qa-spec.md)

### "I need to check if content violates lore rules"
→ Use [quality-assurance/narrative-validation.md](quality-assurance/narrative-validation.md)

### "I'm unfamiliar with a system and need context"
→ Use [onboarding/gaining-context.md](onboarding/gaining-context.md)

### "I need to structure a complex prompt"
→ Use [onboarding/prompt-scaffold.md](onboarding/prompt-scaffold.md)

---

## 📋 Workflow Patterns

All workflows follow a consistent structure:

```markdown
---
description: Brief workflow description
---

# Workflow Title

## Overview
What this workflow accomplishes and when to use it.

## Prerequisites
What you need before starting.

## Steps
1. **Step Name (Persona):** Action to take
2. **Step Name (Persona):** Next action
...

## Verification
How to confirm successful completion.

## Common Issues
Troubleshooting guide.
```

### Workflow Personas
Workflows reference agent personas from `/CLAUDE.md`:

- **🏗️ Forge-Master**: Backend & Systems (C#, Services, DI)
- **🎨 Rune-Scribe**: UI/UX (Avalonia, XAML, ViewModels)
- **📜 Archivist**: Data & Lore (SQL, JSON, Game Content)
- **🛡️ QA Sentinel**: Testing (xUnit, Validation)

Each step indicates which persona should execute it.

---

## 🔄 Using Workflows

### Sequential Execution
Most workflows are designed to be executed **in order**, with each step building on the previous.

**Example: Feature Implementation**
```
1. Analysis → Identify requirements
2. Contract Definition → Create interfaces
3. Test Scaffolding → Write failing tests
4. Implementation → Make tests pass
5. Registration → Wire up DI
```

### Parallel Execution
Some workflows allow parallel execution of independent steps.

**Example: Component Creation**
```
1. ViewModel + View → Can be developed in parallel
2. Integration → Requires both above to be complete
```

### Iteration
Some workflows are iterative and may loop.

**Example: Narrative Validation**
```
1. Check content → Find issues
2. Fix issues → Update content
3. Re-check → Repeat until clean
```

---

## 🚀 Quick Start Workflows

### First Time in Codebase
1. [onboarding/gaining-context.md](onboarding/gaining-context.md)
2. Read `/CLAUDE.md`
3. Review [/rules/07-project/current-phase.md](../rules/07-project/current-phase.md)

### Implementing New Game Feature
1. [onboarding/gaining-context.md](onboarding/gaining-context.md) - Understand related systems
2. [development/feature-implementation.md](development/feature-implementation.md) - Implement feature
3. [quality-assurance/qa-spec.md](quality-assurance/qa-spec.md) - Validate result

### Creating New UI
1. [onboarding/gaining-context.md](onboarding/gaining-context.md) - Understand UI patterns
2. [development/component-creation.md](development/component-creation.md) - Create component
3. Manual testing - Verify UI behavior

### Database Schema Change
1. Review existing schema
2. [development/database-migration.md](development/database-migration.md) - Create migration
3. Test migration - Apply and rollback

---

## 📊 Workflow Dependencies

Some workflows reference each other:

```
gaining-context.md
    ↓
feature-implementation.md → qa-spec.md
    ↓
component-creation.md → Manual Testing
    ↓
database-migration.md
```

**Reading the diagram:**
- Start with context-gathering
- Feature implementation may spawn UI or database work
- All work should be validated

---

## 🛠️ Customizing Workflows

Workflows can be adapted based on:

### Project Phase
- **Prototyping**: Skip test scaffolding, focus on rapid implementation
- **Production**: Follow all steps rigorously
- **Maintenance**: Add extra validation steps

### Feature Complexity
- **Simple**: Combine steps
- **Complex**: Add intermediate validation
- **Critical**: Add peer review steps

### Team Size
- **Solo**: Use workflows as checklists
- **Team**: Add handoff points between personas

---

## 🔍 Finding Related Rules

Workflows often reference rules. Key mappings:

| Workflow Topic | Related Rules |
|----------------|---------------|
| Feature Implementation | [rules/01-technical/coding-standards.md](../rules/01-technical/coding-standards.md) |
| Component Creation | [rules/01-technical/coding-standards.md](../rules/01-technical/coding-standards.md) |
| Database Migration | [rules/01-technical/technical-stack.md](../rules/01-technical/technical-stack.md) |
| Narrative Validation | [rules/03-content/](../rules/03-content/), [rules/05-domains/](../rules/05-domains/) |
| QA Spec | [rules/06-audit/](../rules/06-audit/) |

---

## 🔄 Updating Workflows

When modifying workflows:
1. Test the workflow end-to-end
2. Update this README if adding/removing files
3. Ensure persona references are accurate
4. Add verification steps
5. Include common issues section

---

## 📖 Related Resources

- Parent: [/.agent/README.md](../README.md)
- Rules: [/.agent/rules/README.md](../rules/README.md)
- Main Agent Doc: [/CLAUDE.md](/CLAUDE.md)
