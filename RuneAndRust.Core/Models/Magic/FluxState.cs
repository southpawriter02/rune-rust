namespace RuneAndRust.Core.Models.Magic
{
    /// <summary>
    /// Represents the environmental magic volatility (Flux).
    /// </summary>
    public class FluxState
    {
        public int CurrentValue { get; set; }
        public int DissipationRate { get; set; } = 5;
        public int CriticalThreshold { get; set; } = 50;
        public int MaxValue { get; set; } = 100;
    }
}
