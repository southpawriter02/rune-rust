namespace RuneAndRust.Core.EndlessMode;

/// <summary>
/// v0.40.4: Endless Mode Season
/// Seasonal leaderboard period (3 months)
/// </summary>
public class EndlessSeason
{
    /// <summary>Unique season identifier</summary>
    public string SeasonId { get; set; } = string.Empty;

    /// <summary>Season display name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Season start date</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Season end date</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Is this the currently active season?</summary>
    public bool IsActive { get; set; } = false;

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Season duration</summary>
    public TimeSpan Duration => EndDate - StartDate;

    /// <summary>Season duration in days</summary>
    public int DurationDays => (int)Duration.TotalDays;

    /// <summary>Is season currently ongoing?</summary>
    public bool IsOngoing
    {
        get
        {
            var now = DateTime.UtcNow;
            return now >= StartDate && now <= EndDate;
        }
    }

    /// <summary>Has season ended?</summary>
    public bool HasEnded => DateTime.UtcNow > EndDate;

    /// <summary>Days remaining in season</summary>
    public int DaysRemaining
    {
        get
        {
            if (HasEnded) return 0;
            var remaining = EndDate - DateTime.UtcNow;
            return Math.Max(0, (int)remaining.TotalDays);
        }
    }

    /// <summary>Season progress percentage (0-100)</summary>
    public float ProgressPercentage
    {
        get
        {
            if (!IsOngoing) return HasEnded ? 100f : 0f;

            var total = (EndDate - StartDate).TotalDays;
            var elapsed = (DateTime.UtcNow - StartDate).TotalDays;
            return (float)(elapsed / total * 100);
        }
    }

    /// <summary>Display text for season</summary>
    public string DisplayText => IsActive
        ? $"{Name} (Active • {DaysRemaining} days left)"
        : $"{Name} ({StartDate:MMM yyyy} - {EndDate:MMM yyyy})";
}
