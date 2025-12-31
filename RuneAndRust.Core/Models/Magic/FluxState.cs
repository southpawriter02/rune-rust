namespace RuneAndRust.Core.Models.Magic
{
    public class FluxState
    {
        /// <summary>
        /// The current level of environmental volatility (0-100).
        /// </summary>
        public int CurrentValue { get; set; }

        /// <summary>
        /// How much Flux naturally dissipates at the end of each round.
        /// </summary>
        public int DissipationRate { get; set; } = 5;

        /// <summary>
        /// The threshold at which Backlash risks begin (default 50).
        /// </summary>
        public int CriticalThreshold { get; set; } = 50;
    }
}
