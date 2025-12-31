using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces
{
    public interface IAetherService
    {
        /// <summary>
        /// Gets the current Flux level (0-100).
        /// </summary>
        int GetCurrentFlux();

        /// <summary>
        /// Gets the current Flux threshold classification.
        /// </summary>
        FluxThreshold GetThreshold();

        /// <summary>
        /// Modifies the current Flux level.
        /// </summary>
        /// <param name="amount">Amount to add (positive) or remove (negative).</param>
        /// <param name="source">The source of the modification (e.g., "Spell:Spark", "Dissipation").</param>
        /// <returns>The new Flux level.</returns>
        int ModifyFlux(int amount, string source);

        /// <summary>
        /// Reduces Flux by the standard dissipation rate. Usually called at end of round.
        /// </summary>
        /// <returns>The amount dissipated.</returns>
        int DissipateFlux();

        /// <summary>
        /// Resets Flux to 0 (e.g., on map change).
        /// </summary>
        void Reset();
    }
}
