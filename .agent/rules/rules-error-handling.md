---
trigger: always_on
---

# Error Handling Rules

## 11.1 Argument Validation
- **ALWAYS** validate arguments at method entry
- Use built-in throw helpers for common validations:

```csharp
ArgumentException.ThrowIfNullOrWhiteSpace(abilityId);
ArgumentOutOfRangeException.ThrowIfNegative(cooldown);
ArgumentOutOfRangeException.ThrowIfLessThan(unlockLevel, 1);
ArgumentNullException.ThrowIfNull(item);
```

## 1.2 Result Objects
- For expected failure cases, return result objects instead of throwing
- Include success status, error message, and relevant data

```csharp
public record AbilityValidationResult(bool IsValid, string? FailureReason);
public record AbilityResult(bool Success, string Message, IReadOnlyList<AppliedEffect> EffectsApplied);
```