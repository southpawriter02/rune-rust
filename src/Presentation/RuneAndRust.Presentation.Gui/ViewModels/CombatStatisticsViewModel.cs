namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// View model for displaying combat statistics in the combat summary.
/// </summary>
/// <remarks>
/// <para>
/// Displays 6 metrics from combat:
/// <list type="bullet">
///   <item><description>Rounds — Total rounds in combat</description></item>
///   <item><description>Damage Dealt — Total damage output by player</description></item>
///   <item><description>Damage Taken — Total damage received by player</description></item>
///   <item><description>Criticals — Number of critical hits landed</description></item>
///   <item><description>Abilities Used — Number of abilities activated</description></item>
///   <item><description>Items Used — Number of consumables used</description></item>
/// </list>
/// </para>
/// </remarks>
public class CombatStatisticsViewModel
{
    /// <summary>
    /// Gets the number of rounds in combat.
    /// </summary>
    public int Rounds { get; }

    /// <summary>
    /// Gets the total damage dealt by the player.
    /// </summary>
    public int DamageDealt { get; }

    /// <summary>
    /// Gets the total damage taken by the player.
    /// </summary>
    public int DamageTaken { get; }

    /// <summary>
    /// Gets the number of critical hits landed.
    /// </summary>
    public int Criticals { get; }

    /// <summary>
    /// Gets the number of abilities used.
    /// </summary>
    public int AbilitiesUsed { get; }

    /// <summary>
    /// Gets the number of items used.
    /// </summary>
    public int ItemsUsed { get; }

    /// <summary>
    /// Gets the formatted rounds display text.
    /// </summary>
    public string RoundsText => $"Rounds: {Rounds}";

    /// <summary>
    /// Gets the formatted damage dealt display text.
    /// </summary>
    public string DamageDealtText => $"Damage Dealt: {DamageDealt:N0}";

    /// <summary>
    /// Gets the formatted damage taken display text.
    /// </summary>
    public string DamageTakenText => $"Damage Taken: {DamageTaken:N0}";

    /// <summary>
    /// Gets the formatted criticals display text.
    /// </summary>
    public string CriticalsText => $"Criticals: {Criticals}";

    /// <summary>
    /// Gets the formatted abilities used display text.
    /// </summary>
    public string AbilitiesText => $"Abilities: {AbilitiesUsed}";

    /// <summary>
    /// Gets the formatted items used display text.
    /// </summary>
    public string ItemsUsedText => $"Items Used: {ItemsUsed}";

    /// <summary>
    /// Creates a new combat statistics view model with default values.
    /// </summary>
    public CombatStatisticsViewModel()
    {
    }

    /// <summary>
    /// Creates a new combat statistics view model with specified values.
    /// </summary>
    /// <param name="rounds">Number of rounds.</param>
    /// <param name="damageDealt">Total damage dealt.</param>
    /// <param name="damageTaken">Total damage taken.</param>
    /// <param name="criticals">Critical hits landed.</param>
    /// <param name="abilitiesUsed">Abilities used.</param>
    /// <param name="itemsUsed">Items used.</param>
    public CombatStatisticsViewModel(
        int rounds,
        int damageDealt,
        int damageTaken,
        int criticals,
        int abilitiesUsed,
        int itemsUsed)
    {
        Rounds = rounds;
        DamageDealt = damageDealt;
        DamageTaken = damageTaken;
        Criticals = criticals;
        AbilitiesUsed = abilitiesUsed;
        ItemsUsed = itemsUsed;
    }
}
