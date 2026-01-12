namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the blend state between two biomes.
/// </summary>
public readonly record struct TransitionBlend
{
    /// <summary>
    /// Gets the source biome ID.
    /// </summary>
    public string SourceBiomeId { get; init; }

    /// <summary>
    /// Gets the target biome ID.
    /// </summary>
    public string TargetBiomeId { get; init; }

    /// <summary>
    /// Gets the blend ratio (0.0 = pure source, 1.0 = pure target).
    /// </summary>
    public float Ratio { get; init; }

    /// <summary>
    /// Gets whether this is a pure (non-blended) state.
    /// </summary>
    public bool IsPure => Ratio <= 0.0f || Ratio >= 1.0f;

    /// <summary>
    /// Gets the dominant biome ID.
    /// </summary>
    public string DominantBiome => Ratio >= 0.5f ? TargetBiomeId : SourceBiomeId;

    /// <summary>
    /// Creates a blend for a position within a transition.
    /// </summary>
    public static TransitionBlend ForPosition(string source, string target, float ratio) => new()
    {
        SourceBiomeId = source,
        TargetBiomeId = target,
        Ratio = Math.Clamp(ratio, 0f, 1f)
    };

    /// <summary>
    /// Creates a pure (non-blended) state.
    /// </summary>
    public static TransitionBlend Pure(string biomeId) => new()
    {
        SourceBiomeId = biomeId,
        TargetBiomeId = biomeId,
        Ratio = 0f
    };

    /// <summary>
    /// Selects a biome for spawn based on blend weights.
    /// </summary>
    public string SelectBiomeForSpawn(Random random)
    {
        if (IsPure) return DominantBiome;
        return random.NextSingle() < Ratio ? TargetBiomeId : SourceBiomeId;
    }

    /// <summary>
    /// Gets a blended descriptor by selecting from either biome.
    /// </summary>
    public string GetBlendedDescriptor(string sourceDesc, string targetDesc, Random random)
    {
        if (IsPure) return Ratio >= 1.0f ? targetDesc : sourceDesc;
        return random.NextSingle() < Ratio ? targetDesc : sourceDesc;
    }
}
