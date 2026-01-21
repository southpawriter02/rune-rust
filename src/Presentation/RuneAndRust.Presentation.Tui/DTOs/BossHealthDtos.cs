namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object containing boss health display information.
/// </summary>
/// <param name="BossId">The unique identifier for the boss definition.</param>
/// <param name="BossName">The display name of the boss.</param>
/// <param name="CurrentHealth">The current health value.</param>
/// <param name="MaxHealth">The maximum health value.</param>
/// <param name="HealthPercent">The calculated health percentage (0-100).</param>
/// <param name="CurrentPhaseNumber">The current phase number (1-based).</param>
/// <remarks>
/// <para>Used to transfer boss health state from the combat system to the
/// <see cref="UI.BossHealthBarDisplay"/> for rendering.</para>
/// <para>The <paramref name="HealthPercent"/> is pre-calculated to simplify
/// phase marker positioning and color determination.</para>
/// </remarks>
/// <example>
/// <code>
/// var dto = new BossHealthDisplayDto(
///     BossId: "skeleton-king",
///     BossName: "Skeleton King",
///     CurrentHealth: 2847,
///     MaxHealth: 5000,
///     HealthPercent: 57,
///     CurrentPhaseNumber: 2);
/// </code>
/// </example>
public record BossHealthDisplayDto(
    string BossId,
    string BossName,
    int CurrentHealth,
    int MaxHealth,
    int HealthPercent,
    int CurrentPhaseNumber);

/// <summary>
/// Data transfer object for phase marker display information.
/// </summary>
/// <param name="PhaseNumber">The phase number (1-based).</param>
/// <param name="PhaseName">The display name of the phase.</param>
/// <param name="ThresholdPercent">The health percentage threshold (0-100).</param>
/// <remarks>
/// <para>Phase markers are displayed on the boss health bar to indicate
/// when phase transitions will occur.</para>
/// <para>The <paramref name="ThresholdPercent"/> determines the horizontal
/// position of the marker on the health bar.</para>
/// </remarks>
/// <example>
/// <code>
/// var markers = new List&lt;PhaseMarkerDto&gt;
/// {
///     new(2, "Empowered", 75),
///     new(3, "Enraged", 50),
///     new(4, "Desperate", 25)
/// };
/// </code>
/// </example>
public record PhaseMarkerDto(
    int PhaseNumber,
    string PhaseName,
    int ThresholdPercent);

/// <summary>
/// Data transfer object for damage animation information.
/// </summary>
/// <param name="PreviousHealth">The health value before damage.</param>
/// <param name="CurrentHealth">The health value after damage.</param>
/// <param name="DamageAmount">The amount of damage dealt (positive) or healed (negative).</param>
/// <remarks>
/// <para>Used to trigger damage animation on the boss health bar when
/// the boss takes damage during combat.</para>
/// <para>A positive <paramref name="DamageAmount"/> indicates damage,
/// while a negative value indicates healing.</para>
/// </remarks>
/// <example>
/// <code>
/// // Boss takes 150 damage
/// var animationDto = new DamageAnimationDto(
///     PreviousHealth: 2997,
///     CurrentHealth: 2847,
///     DamageAmount: 150);
/// 
/// // Boss heals for 50 HP
/// var healingDto = new DamageAnimationDto(
///     PreviousHealth: 2847,
///     CurrentHealth: 2897,
///     DamageAmount: -50);
/// </code>
/// </example>
public record DamageAnimationDto(
    int PreviousHealth,
    int CurrentHealth,
    int DamageAmount);
