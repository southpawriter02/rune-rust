namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.3: Result of an object interaction attempt
/// </summary>
public class InteractionResult
{
    /// <summary>
    /// Whether the interaction was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Description of the result
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Skill check result (if applicable)
    /// </summary>
    public SkillCheckResult? CheckResult { get; set; }

    /// <summary>
    /// Consequence result (if applicable)
    /// </summary>
    public ConsequenceResult? Consequence { get; set; }

    /// <summary>
    /// Reason for failure (if applicable)
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Whether object state changed
    /// </summary>
    public bool StateChanged { get; set; }

    /// <summary>
    /// New state (if changed)
    /// </summary>
    public string? NewState { get; set; }

    /// <summary>
    /// Creates a successful interaction result
    /// </summary>
    public static InteractionResult SuccessResult(string description, ConsequenceResult? consequence = null)
    {
        return new InteractionResult
        {
            Success = true,
            Description = description,
            Consequence = consequence
        };
    }

    /// <summary>
    /// Creates a failed interaction result
    /// </summary>
    public static InteractionResult FailureResult(string description, SkillCheckResult? checkResult = null, string? failureReason = null)
    {
        return new InteractionResult
        {
            Success = false,
            Description = description,
            CheckResult = checkResult,
            FailureReason = failureReason
        };
    }

    /// <summary>
    /// Creates a result for already-used object
    /// </summary>
    public static InteractionResult AlreadyUsed(string description)
    {
        return new InteractionResult
        {
            Success = false,
            Description = description,
            FailureReason = "Object already used"
        };
    }

    /// <summary>
    /// Creates a result for insufficient requirements
    /// </summary>
    public static InteractionResult InsufficientRequirements(string description, string requirement)
    {
        return new InteractionResult
        {
            Success = false,
            Description = description,
            FailureReason = $"Requires: {requirement}"
        };
    }
}

/// <summary>
/// v0.38.3: Result of skill check
/// v0.38.10: Enhanced with flavor text support
/// </summary>
public class SkillCheckResult
{
    /// <summary>
    /// Whether the check was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of successes rolled
    /// </summary>
    public int Successes { get; set; }

    /// <summary>
    /// Dice roll result
    /// </summary>
    public List<int> Roll { get; set; } = new();

    /// <summary>
    /// Check type
    /// </summary>
    public SkillCheckType CheckType { get; set; }

    /// <summary>
    /// Difficulty class
    /// </summary>
    public int DC { get; set; }

    /// <summary>
    /// v0.38.10: Narrative flavor text for the skill check attempt
    /// </summary>
    public string? AttemptDescription { get; set; }

    /// <summary>
    /// v0.38.10: Narrative flavor text for the skill check result
    /// </summary>
    public string? ResultDescription { get; set; }

    /// <summary>
    /// Gets a formatted string of the roll
    /// </summary>
    public string GetRollString()
    {
        return $"[{string.Join(", ", Roll)}] = {Successes} success{(Successes != 1 ? "es" : "")}";
    }
}

/// <summary>
/// v0.38.3: Result of consequence execution
/// </summary>
public class ConsequenceResult
{
    /// <summary>
    /// Consequence type
    /// </summary>
    public ConsequenceType Type { get; set; }

    /// <summary>
    /// Description of the consequence
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether the consequence was successfully executed
    /// </summary>
    public bool Executed { get; set; }

    /// <summary>
    /// Target object/entity (if applicable)
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Action performed (if applicable)
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Loot granted (if applicable)
    /// </summary>
    public List<string>? LootGranted { get; set; }

    /// <summary>
    /// Quest hook data (if applicable)
    /// </summary>
    public string? QuestHook { get; set; }

    /// <summary>
    /// Narrative clue (if applicable)
    /// </summary>
    public string? NarrativeClue { get; set; }

    /// <summary>
    /// Damage dealt (for traps)
    /// </summary>
    public string? Damage { get; set; }

    /// <summary>
    /// Status effect applied (for traps)
    /// </summary>
    public List<object>? StatusEffect { get; set; }

    /// <summary>
    /// Creates an unlock consequence
    /// </summary>
    public static ConsequenceResult Unlock(string description, string? target = null)
    {
        return new ConsequenceResult
        {
            Type = ConsequenceType.Unlock,
            Description = description,
            Executed = true,
            Target = target,
            Action = "unlock"
        };
    }

    /// <summary>
    /// Creates a trigger consequence
    /// </summary>
    public static ConsequenceResult Trigger(string description, string? action = null)
    {
        return new ConsequenceResult
        {
            Type = ConsequenceType.Trigger,
            Description = description,
            Executed = true,
            Action = action
        };
    }

    /// <summary>
    /// Creates a spawn consequence
    /// </summary>
    public static ConsequenceResult Spawn(string description, string? target = null)
    {
        return new ConsequenceResult
        {
            Type = ConsequenceType.Spawn,
            Description = description,
            Executed = true,
            Target = target,
            Action = "spawn"
        };
    }

    /// <summary>
    /// Creates a reveal consequence
    /// </summary>
    public static ConsequenceResult Reveal(string description, string? narrativeClue = null, string? questHook = null)
    {
        return new ConsequenceResult
        {
            Type = ConsequenceType.Reveal,
            Description = description,
            Executed = true,
            NarrativeClue = narrativeClue,
            QuestHook = questHook
        };
    }

    /// <summary>
    /// Creates a loot consequence
    /// </summary>
    public static ConsequenceResult Loot(string description, List<string>? lootGranted = null)
    {
        return new ConsequenceResult
        {
            Type = ConsequenceType.Loot,
            Description = description,
            Executed = true,
            LootGranted = lootGranted
        };
    }

    /// <summary>
    /// Creates a trap consequence
    /// </summary>
    public static ConsequenceResult Trap(string description, string? damage = null, List<object>? statusEffect = null)
    {
        return new ConsequenceResult
        {
            Type = ConsequenceType.Trap,
            Description = description,
            Executed = true,
            Damage = damage,
            StatusEffect = statusEffect
        };
    }

    /// <summary>
    /// Creates a no-consequence result
    /// </summary>
    public static ConsequenceResult None()
    {
        return new ConsequenceResult
        {
            Type = ConsequenceType.None,
            Description = "No consequence.",
            Executed = true
        };
    }
}
