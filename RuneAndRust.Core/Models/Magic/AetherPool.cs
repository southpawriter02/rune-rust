using System;

namespace RuneAndRust.Core.Models.Magic
{
    public class AetherPool
    {
        public int Current { get; set; }
        public int Max { get; set; }
        public int Flux { get; set; }

        public AetherPool(int max)
        {
            Max = max;
            Current = max;
            Flux = 0;
        }

        public void Modify(int amount)
        {
            Current = Math.Clamp(Current + amount, 0, Max);
        }

        public void AddFlux(int amount)
        {
            Flux += amount;
        }

        public void ResetFlux()
        {
            Flux = 0;
        }

        // For serialization
        public AetherPool() { }
    }
}
