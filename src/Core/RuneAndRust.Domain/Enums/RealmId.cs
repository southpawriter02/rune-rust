namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies the nine canonical realms of Aethelgard.
/// </summary>
/// <remarks>
/// <para>
/// Each realm corresponds to a numbered "Deck" of the YGGDRASIL Network—the
/// colossal orbital ring that connected the world-stations before The Glitch.
/// The integer values match their deck numbers for configuration serialization.
/// </para>
/// <para>
/// Post-Glitch, each realm has transformed from its original function into
/// a distinct biome with unique environmental hazards and inhabitants.
/// </para>
/// </remarks>
public enum RealmId
{
    /// <summary>
    /// Deck 1 — The Shattered Heavens.
    /// </summary>
    /// <remarks>
    /// Pre-Glitch: Command and Control Hub.
    /// Post-Glitch: Reality-warped command deck; haunted by glitching echoes.
    /// Primary hazard: Reality Flux.
    /// </remarks>
    Asgard = 1,

    /// <summary>
    /// Deck 2 — The Flickering Groves.
    /// </summary>
    /// <remarks>
    /// Pre-Glitch: Aetheric Research Laboratories.
    /// Post-Glitch: Bioluminescent fungal forests; psychic radiation.
    /// Primary hazard: CPS Exposure.
    /// </remarks>
    Alfheim = 2,

    /// <summary>
    /// Deck 3 — The Strangling Green.
    /// </summary>
    /// <remarks>
    /// Pre-Glitch: Biodome Agricultural Sector.
    /// Post-Glitch: Overgrown jungle threatening to consume all.
    /// Primary hazard: Mutagenic Spores.
    /// </remarks>
    Vanaheim = 3,

    /// <summary>
    /// Deck 4 — The Tamed Ruin.
    /// </summary>
    /// <remarks>
    /// Pre-Glitch: Civilian Habitation and Agricultural Production.
    /// Post-Glitch: Agricultural heartland; most populous realm.
    /// Primary hazard: None (relatively safe).
    /// </remarks>
    Midgard = 4,

    /// <summary>
    /// Deck 5 — The Frozen Deeps.
    /// </summary>
    /// <remarks>
    /// Pre-Glitch: Cryogenic Storage and Deep Archives.
    /// Post-Glitch: Flash-frozen wasteland; ancient data vaults.
    /// Primary hazard: Extreme Cold.
    /// </remarks>
    Niflheim = 5,

    /// <summary>
    /// Deck 6 — The Sunless Forges.
    /// </summary>
    /// <remarks>
    /// Pre-Glitch: Industrial Manufacturing and Resource Processing.
    /// Post-Glitch: Lightless industrial hellscape; toxic atmosphere.
    /// Primary hazard: Total Darkness.
    /// </remarks>
    Svartalfheim = 6,

    /// <summary>
    /// Deck 7 — The Land of Giants.
    /// </summary>
    /// <remarks>
    /// Pre-Glitch: Megafauna Habitats and Ecological Reserves.
    /// Post-Glitch: Macro-scale environments; everything is enormous.
    /// Primary hazard: Giant Scale.
    /// </remarks>
    Jotunheim = 7,

    /// <summary>
    /// Deck 8 — The Burning Caldera.
    /// </summary>
    /// <remarks>
    /// Pre-Glitch: Geothermal Power Generation.
    /// Post-Glitch: Volcanic nightmare; rivers of magma.
    /// Primary hazard: Intense Heat.
    /// </remarks>
    Muspelheim = 8,

    /// <summary>
    /// Deck 9 — The Realm of the Dead.
    /// </summary>
    /// <remarks>
    /// Pre-Glitch: Medical and Recycling Facilities.
    /// Post-Glitch: Toxic necropolis; industrial decay.
    /// Primary hazard: Toxic Atmosphere.
    /// </remarks>
    Helheim = 9
}
