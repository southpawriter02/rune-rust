namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the target type for a foraging attempt, determining base DC and yield type.
/// </summary>
/// <remarks>
/// <para>
/// Higher DCs yield more valuable resources but are harder to find.
/// Players can choose to focus on specific resource types or search
/// generally for whatever they can find.
/// </para>
/// <para>
/// Target summary:
/// <list type="bullet">
///   <item><description>CommonSalvage: DC 10, yields 2d10 scrap - Easy, always available</description></item>
///   <item><description>UsefulSupplies: DC 14, yields 1d6 rations - Moderate difficulty</description></item>
///   <item><description>ValuableComponents: DC 18, yields 1d4 components - Requires skill</description></item>
///   <item><description>HiddenCache: DC 22, yields 1d100 Marks + item - Difficult, high reward</description></item>
/// </list>
/// </para>
/// <para>
/// Note: Hidden caches can also be discovered incidentally when any 10 is rolled
/// during a foraging check, regardless of the ForageTarget selected.
/// </para>
/// </remarks>
public enum ForageTarget
{
    /// <summary>
    /// Search for common salvage materials (scrap metal, wiring, debris).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The easiest foraging target, always available in any environment.
    /// Yields basic materials useful for repairs and trading.
    /// </para>
    /// <para>
    /// Base DC: 10.
    /// Primary Yield: 2d10 scrap.
    /// Notes: Easy, always available.
    /// </para>
    /// </remarks>
    CommonSalvage = 0,

    /// <summary>
    /// Search for useful supplies (rations, water, basic medical).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Moderate difficulty, yields consumable supplies essential
    /// for survival in the wasteland.
    /// </para>
    /// <para>
    /// Base DC: 14.
    /// Primary Yield: 1d6 rations.
    /// Notes: Moderate difficulty.
    /// </para>
    /// </remarks>
    UsefulSupplies = 1,

    /// <summary>
    /// Search for valuable components (tech parts, rare materials).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires skill to identify and extract. Yields valuable
    /// components useful for crafting and high-value trading.
    /// </para>
    /// <para>
    /// Base DC: 18.
    /// Primary Yield: 1d4 components.
    /// Notes: Requires skill.
    /// </para>
    /// </remarks>
    ValuableComponents = 2,

    /// <summary>
    /// Search specifically for hidden caches left by other survivors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The most difficult target, but yields the highest rewards.
    /// Hidden caches contain currency and random valuable items.
    /// </para>
    /// <para>
    /// Base DC: 22.
    /// Primary Yield: 1d100 Marks + random item.
    /// Notes: Difficult, high reward.
    /// </para>
    /// <para>
    /// Note: Caches can also be discovered incidentally when any 10 is rolled
    /// during a foraging check, regardless of the ForageTarget selected.
    /// </para>
    /// </remarks>
    HiddenCache = 3
}
