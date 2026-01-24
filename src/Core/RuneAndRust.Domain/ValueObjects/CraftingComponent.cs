// ------------------------------------------------------------------------------
// <copyright file="CraftingComponent.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents a salvaged component that can be used in crafting improvised tools.
// Components are acquired through critical successes on bypass checks.
// Part of v0.15.4g Improvised Tool Crafting System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a salvaged component that can be used in crafting improvised tools.
/// Components are acquired through critical successes on bypass checks.
/// </summary>
/// <remarks>
/// <para>
/// Components are categorized by type (Metal, Wire, Circuit, Misc) and may be
/// common or rare. Some components can substitute for others of compatible types.
/// </para>
/// <para>
/// Component acquisition:
/// <list type="bullet">
///   <item><description>Lockpicking crits yield: High-Tension Spring, Scrap Metal (50%)</description></item>
///   <item><description>Terminal Hacking crits yield: Circuit Fragment, Wire, Capacitor (16%)</description></item>
///   <item><description>Trap Disarmament crits yield: Wire, Metal Clips</description></item>
///   <item><description>Glitch Exploitation crits yield: Circuit Fragment, Capacitor (50%)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="ComponentId">Unique identifier for this component instance.</param>
/// <param name="ComponentName">Display name of the component.</param>
/// <param name="ComponentType">Type classification (Metal, Wire, Circuit, Misc).</param>
/// <param name="SourceDescription">Where this component was salvaged from.</param>
/// <param name="IsRare">True if this is a rare/valuable component.</param>
public readonly record struct CraftingComponent(
    string ComponentId,
    string ComponentName,
    ComponentType ComponentType,
    string SourceDescription,
    bool IsRare = false)
{
    // -------------------------------------------------------------------------
    // Factory Methods - Metal Components
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a Scrap Metal component.
    /// </summary>
    /// <param name="source">Description of where the component was salvaged from.</param>
    /// <returns>A new Scrap Metal component.</returns>
    /// <remarks>
    /// Scrap Metal is common, salvaged from locks, structures, and containers.
    /// Used in: Shim Picks (3× required).
    /// </remarks>
    public static CraftingComponent CreateScrapMetal(string source = "salvaged mechanism")
    {
        return new CraftingComponent(
            ComponentId: Guid.NewGuid().ToString(),
            ComponentName: "Scrap Metal",
            ComponentType: ComponentType.Metal,
            SourceDescription: source);
    }

    /// <summary>
    /// Creates a Metal Clips component.
    /// </summary>
    /// <param name="source">Description of where the component was salvaged from.</param>
    /// <returns>A new Metal Clips component.</returns>
    /// <remarks>
    /// Metal Clips are common, salvaged from traps, containers, and locks.
    /// Used in: Bypass Clamps (4× required).
    /// </remarks>
    public static CraftingComponent CreateMetalClips(string source = "salvaged mechanism")
    {
        return new CraftingComponent(
            ComponentId: Guid.NewGuid().ToString(),
            ComponentName: "Metal Clips",
            ComponentType: ComponentType.Metal,
            SourceDescription: source);
    }

    /// <summary>
    /// Creates a High-Tension Spring component.
    /// </summary>
    /// <param name="source">Description of where the component was salvaged from.</param>
    /// <returns>A new High-Tension Spring component.</returns>
    /// <remarks>
    /// High-Tension Springs are uncommon, primarily salvaged from complex locks.
    /// Note: Not currently used in any base recipes, but valuable for trading.
    /// </remarks>
    public static CraftingComponent CreateHighTensionSpring(string source = "salvaged lock")
    {
        return new CraftingComponent(
            ComponentId: Guid.NewGuid().ToString(),
            ComponentName: "High-Tension Spring",
            ComponentType: ComponentType.Metal,
            SourceDescription: source);
    }

    // -------------------------------------------------------------------------
    // Factory Methods - Wire Components
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a Copper Wire component.
    /// </summary>
    /// <param name="source">Description of where the component was salvaged from.</param>
    /// <returns>A new Copper Wire component.</returns>
    /// <remarks>
    /// Copper Wire is common, salvaged from terminals and electronics.
    /// Used in: Wire Probe (2× required).
    /// </remarks>
    public static CraftingComponent CreateCopperWire(string source = "salvaged mechanism")
    {
        return new CraftingComponent(
            ComponentId: Guid.NewGuid().ToString(),
            ComponentName: "Copper Wire",
            ComponentType: ComponentType.Wire,
            SourceDescription: source);
    }

    /// <summary>
    /// Creates a generic Wire component.
    /// </summary>
    /// <param name="source">Description of where the component was salvaged from.</param>
    /// <returns>A new Wire component.</returns>
    /// <remarks>
    /// Wire is common, salvaged from terminals, traps, and electronics.
    /// Used in: Glitch Trigger (4× required).
    /// </remarks>
    public static CraftingComponent CreateWire(string source = "salvaged mechanism")
    {
        return new CraftingComponent(
            ComponentId: Guid.NewGuid().ToString(),
            ComponentName: "Wire",
            ComponentType: ComponentType.Wire,
            SourceDescription: source);
    }

    // -------------------------------------------------------------------------
    // Factory Methods - Circuit Components
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a Capacitor component.
    /// </summary>
    /// <param name="source">Description of where the component was salvaged from.</param>
    /// <returns>A new Capacitor component.</returns>
    /// <remarks>
    /// Capacitors are RARE, salvaged from military terminals, advanced traps,
    /// and glitched devices. Their scarcity makes Glitch Triggers difficult to craft.
    /// Used in: Glitch Trigger (1× required).
    /// </remarks>
    public static CraftingComponent CreateCapacitor(string source = "salvaged mechanism")
    {
        return new CraftingComponent(
            ComponentId: Guid.NewGuid().ToString(),
            ComponentName: "Capacitor",
            ComponentType: ComponentType.Circuit,
            SourceDescription: source,
            IsRare: true);
    }

    /// <summary>
    /// Creates a Circuit Fragment component.
    /// </summary>
    /// <param name="source">Description of where the component was salvaged from.</param>
    /// <returns>A new Circuit Fragment component.</returns>
    /// <remarks>
    /// Circuit Fragments are uncommon, salvaged from terminals and devices.
    /// Note: Not currently used in any base recipes, but valuable for trading.
    /// </remarks>
    public static CraftingComponent CreateCircuitFragment(string source = "salvaged terminal")
    {
        return new CraftingComponent(
            ComponentId: Guid.NewGuid().ToString(),
            ComponentName: "Circuit Fragment",
            ComponentType: ComponentType.Circuit,
            SourceDescription: source);
    }

    // -------------------------------------------------------------------------
    // Factory Methods - Misc Components
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a Handle component.
    /// </summary>
    /// <param name="source">Description of where the component was salvaged from.</param>
    /// <returns>A new Handle component.</returns>
    /// <remarks>
    /// Handles are common, salvaged from tools and devices.
    /// Used in: Wire Probe (1× required).
    /// </remarks>
    public static CraftingComponent CreateHandle(string source = "salvaged mechanism")
    {
        return new CraftingComponent(
            ComponentId: Guid.NewGuid().ToString(),
            ComponentName: "Handle",
            ComponentType: ComponentType.Misc,
            SourceDescription: source);
    }

    // -------------------------------------------------------------------------
    // Instance Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this component can substitute for the given type.
    /// </summary>
    /// <param name="required">The required component type.</param>
    /// <returns>True if this component can be used in place of the required type.</returns>
    /// <remarks>
    /// <para>
    /// Substitution rules:
    /// <list type="bullet">
    ///   <item><description>A component always matches its own type</description></item>
    ///   <item><description>Metal can substitute for Misc (improvised handles, etc.)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This represents the flexible nature of improvised crafting, where
    /// survivors make do with what they have.
    /// </para>
    /// </remarks>
    public bool CanSubstituteFor(ComponentType required)
    {
        // Same type always works
        if (ComponentType == required)
        {
            return true;
        }

        // Metal can substitute for Misc (improvised handles, etc.)
        if (ComponentType == ComponentType.Metal && required == ComponentType.Misc)
        {
            return true;
        }

        return false;
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a display string for the component.
    /// </summary>
    /// <returns>A formatted description of the component.</returns>
    public string ToDisplayString()
    {
        var rareTag = IsRare ? " [Rare]" : "";
        return $"[{ComponentName}]{rareTag} - {SourceDescription}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"CraftingComponent[Name={ComponentName} Type={ComponentType} " +
               $"Rare={IsRare} Source={SourceDescription}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
