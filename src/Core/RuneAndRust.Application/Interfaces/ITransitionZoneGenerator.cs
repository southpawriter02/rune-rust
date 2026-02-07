using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Generates transition zones between adjacent realms with interpolated environmental properties.
/// </summary>
/// <remarks>
/// <para>
/// ITransitionZoneGenerator is responsible for creating <see cref="TransitionZone"/> instances
/// that represent the gradual environmental shift between two realms. It uses the
/// <see cref="IBiomeAdjacencyService"/> compatibility matrix to determine whether transitions
/// are possible and how many zones are required.
/// </para>
/// <para>
/// Compatibility rules:
/// <list type="bullet">
/// <item><strong>Compatible</strong> — 0-1 transition zone (usually direct connection)</item>
/// <item><strong>RequiresTransition</strong> — 1-3 transition zones with property interpolation</item>
/// <item><strong>Incompatible</strong> — no transition possible (returns null)</item>
/// </list>
/// </para>
/// <para>
/// Property interpolation uses the linear formula: <c>result = from + (to - from) * blend</c>
/// applied to all six <see cref="RealmBiomeProperties"/> fields (temperature, aetheric intensity,
/// humidity, light level, scale factor, corrosion rate).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Generate a single transition zone between Midgard and Vanaheim
/// var zone = generator.GenerateTransition(RealmId.Midgard, RealmId.Vanaheim);
/// if (zone != null)
/// {
///     Console.WriteLine($"Temperature at midpoint: {zone.InterpolatedProperties.TemperatureCelsius}°C");
/// }
///
/// // Generate a 3-zone sequence between Midgard and Jotunheim
/// var sequence = generator.GenerateTransitionSequence(RealmId.Midgard, RealmId.Jotunheim, 3);
/// foreach (var z in sequence)
/// {
///     Console.WriteLine($"Zone {z.SequenceIndex}: {z.BlendFactor:P0} blend");
/// }
/// </code>
/// </example>
public interface ITransitionZoneGenerator
{
    /// <summary>
    /// Generates a single transition zone between two realms at 50% blend.
    /// </summary>
    /// <param name="fromRealm">Source realm identifier.</param>
    /// <param name="toRealm">Destination realm identifier.</param>
    /// <returns>
    /// A <see cref="TransitionZone"/> with interpolated properties at the midpoint,
    /// or <c>null</c> if the realms are incompatible or identical.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns null in two cases:
    /// <list type="number">
    /// <item>The realms are the same (no transition needed)</item>
    /// <item>The realms are incompatible (e.g., Muspelheim ↔ Niflheim)</item>
    /// </list>
    /// </para>
    /// <para>
    /// For compatible pairs, generates a single zone at 50% blend.
    /// For RequiresTransition pairs, also generates at 50% blend (use
    /// <see cref="GenerateTransitionSequence"/> for multi-zone transitions).
    /// </para>
    /// </remarks>
    TransitionZone? GenerateTransition(RealmId fromRealm, RealmId toRealm);

    /// <summary>
    /// Generates a sequence of transition zones between two realms.
    /// </summary>
    /// <param name="from">Source realm identifier.</param>
    /// <param name="to">Destination realm identifier.</param>
    /// <param name="roomCount">Number of transition rooms to generate (1-3).</param>
    /// <returns>
    /// An ordered list of <see cref="TransitionZone"/> objects from source to destination.
    /// Returns an empty list if the realms are incompatible or identical.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Blend factors are evenly distributed across the sequence:
    /// <list type="bullet">
    /// <item>1 room: blend at 0.50</item>
    /// <item>2 rooms: blends at 0.33 and 0.67</item>
    /// <item>3 rooms: blends at 0.25, 0.50, and 0.75</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">If roomCount is less than 1 or greater than 3.</exception>
    IReadOnlyList<TransitionZone> GenerateTransitionSequence(RealmId from, RealmId to, int roomCount);

    /// <summary>
    /// Linearly interpolates environmental properties between two realms.
    /// </summary>
    /// <param name="from">Source realm properties.</param>
    /// <param name="to">Destination realm properties.</param>
    /// <param name="blend">
    /// Interpolation factor where 0.0 returns source properties and 1.0 returns destination properties.
    /// </param>
    /// <returns>A new <see cref="RealmBiomeProperties"/> with interpolated values.</returns>
    /// <remarks>
    /// <para>
    /// Uses the linear interpolation formula for all numeric properties:
    /// <c>result = from + (to - from) * blend</c>
    /// </para>
    /// <para>
    /// Example at blend = 0.5 (Midgard → Vanaheim):
    /// <list type="bullet">
    /// <item>Temperature: 18°C + (28°C - 18°C) × 0.5 = 23°C</item>
    /// <item>Humidity: 60% + (70% - 60%) × 0.5 = 65%</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">If blend is less than 0.0 or greater than 1.0.</exception>
    RealmBiomeProperties InterpolateProperties(RealmBiomeProperties from, RealmBiomeProperties to, double blend);

    /// <summary>
    /// Determines whether a transition zone can be generated between two realms.
    /// </summary>
    /// <param name="from">Source realm identifier.</param>
    /// <param name="to">Destination realm identifier.</param>
    /// <returns>
    /// <c>true</c> if the realms are compatible or require transition zones;
    /// <c>false</c> if the realms are identical or incompatible.
    /// </returns>
    /// <remarks>
    /// This method is a lightweight check that does not generate any zone data.
    /// It delegates to <see cref="IBiomeAdjacencyService.GetCompatibility"/> to
    /// determine compatibility status.
    /// </remarks>
    bool CanGenerateTransition(RealmId from, RealmId to);
}
