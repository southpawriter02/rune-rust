using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using Serilog;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.17: Service implementation for boss display information.
/// Provides UI-ready data for boss encounters.
/// </summary>
public class BossDisplayService : IBossDisplayService
{
    private readonly ILogger _logger;

    // Boss titles by enemy type
    private static readonly Dictionary<EnemyType, string> BossTitles = new()
    {
        { EnemyType.RuinWarden, "Guardian of the Forgotten Depths" },
        { EnemyType.AethericAberration, "The Void Touched" },
        { EnemyType.ForlornArchivist, "Keeper of Lost Memories" },
        { EnemyType.OmegaSentinel, "The Final Protocol" }
    };

    // Phase descriptions
    private static readonly Dictionary<BossPhase, string> PhaseDescriptions = new()
    {
        { BossPhase.Phase1, "Teaching Phase - The boss tests your defenses" },
        { BossPhase.Phase2, "Escalation Phase - The boss reveals new abilities" },
        { BossPhase.Phase3, "Desperation Phase - The boss fights for survival" }
    };

    public BossDisplayService(ILogger logger)
    {
        _logger = logger;
        _logger.Information("[BossDisplayService] Initialized");
    }

    /// <inheritdoc/>
    public BossDisplayData GetBossDisplayData(Enemy boss)
    {
        if (!boss.IsBoss)
        {
            _logger.Warning("[BossDisplayService] GetBossDisplayData called for non-boss enemy: {Name}", boss.Name);
            return new BossDisplayData { BossName = boss.Name };
        }

        var currentPhase = boss.Phase;
        var totalPhases = 3; // Standard 3-phase boss

        var displayData = new BossDisplayData
        {
            BossName = boss.Name,
            BossTitle = GetBossTitle(boss.Type),
            CurrentHP = boss.HP,
            MaxHP = boss.MaxHP,
            CurrentPhase = currentPhase,
            TotalPhases = totalPhases,
            PhaseSegments = GetPhaseSegments(boss, currentPhase),
            IsVulnerable = boss.VulnerableTurnsRemaining > 0,
            VulnerableTurnsRemaining = boss.VulnerableTurnsRemaining,
            Boss = boss
        };

        _logger.Debug("[BossDisplayService] Generated display data for {Name}: Phase {Phase}/{Total}, HP {HP}/{MaxHP}",
            boss.Name, currentPhase, totalPhases, boss.HP, boss.MaxHP);

        return displayData;
    }

    /// <inheritdoc/>
    public List<PhaseHealthSegment> GetPhaseSegments(Enemy boss, int currentPhase)
    {
        var segments = new List<PhaseHealthSegment>();
        var totalPhases = 3;

        // Standard phase thresholds: 100-66%, 66-33%, 33-0%
        var thresholds = new[] { 1.0f, 0.66f, 0.33f, 0.0f };
        var phaseNames = new[] { "Phase I", "Phase II", "Phase III" };

        for (int i = 1; i <= totalPhases; i++)
        {
            segments.Add(new PhaseHealthSegment
            {
                PhaseNumber = i,
                IsCurrentPhase = i == currentPhase,
                IsCompleted = i < currentPhase,
                HealthThreshold = thresholds[i - 1],
                PhaseName = phaseNames[i - 1]
            });
        }

        return segments;
    }

    /// <inheritdoc/>
    public BossMechanicWarning CreateMechanicWarning(
        string abilityName,
        string description,
        int turnsRemaining,
        DangerLevel dangerLevel,
        bool canInterrupt)
    {
        var warning = new BossMechanicWarning
        {
            MechanicName = abilityName,
            Description = description,
            TurnsRemaining = turnsRemaining,
            WarningTime = turnsRemaining * 6f, // Approximate seconds per turn
            DangerLevel = dangerLevel,
            CanBeInterrupted = canInterrupt,
            Icon = GetDangerIcon(dangerLevel)
        };

        _logger.Debug("[BossDisplayService] Created mechanic warning: {Name} ({Danger}), {Turns} turns",
            abilityName, dangerLevel, turnsRemaining);

        return warning;
    }

    /// <inheritdoc/>
    public string GetBossTitle(EnemyType enemyType)
    {
        if (BossTitles.TryGetValue(enemyType, out var title))
        {
            return title;
        }

        // Generate a generic title for unknown boss types
        return "Ancient Terror";
    }

    /// <inheritdoc/>
    public float CalculateEnrageProgress(int turnCount, int enrageTurn)
    {
        if (enrageTurn <= 0) return 0f;
        return Math.Clamp((float)turnCount / enrageTurn, 0f, 1f);
    }

    /// <inheritdoc/>
    public DangerLevel GetAbilityDangerLevel(int damageDice, int damageBonus, bool isAoE)
    {
        // Calculate average damage
        var avgDamage = (damageDice * 3.5f) + damageBonus; // d6 average = 3.5

        // Apply AoE multiplier
        if (isAoE)
        {
            avgDamage *= 1.5f;
        }

        return avgDamage switch
        {
            < 15 => DangerLevel.Low,
            < 30 => DangerLevel.Medium,
            < 50 => DangerLevel.High,
            _ => DangerLevel.Lethal
        };
    }

    /// <inheritdoc/>
    public string GetPhaseDescription(BossPhase phase)
    {
        if (PhaseDescriptions.TryGetValue(phase, out var description))
        {
            return description;
        }
        return "Unknown Phase";
    }

    private static string GetDangerIcon(DangerLevel level)
    {
        return level switch
        {
            DangerLevel.Low => "\u26A0",      // Warning sign
            DangerLevel.Medium => "\u2757",   // Exclamation mark
            DangerLevel.High => "\u2622",     // Radioactive
            DangerLevel.Lethal => "\u2620",   // Skull and crossbones
            _ => "\u26A0"
        };
    }
}
