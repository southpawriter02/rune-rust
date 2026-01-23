namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Fluent builder for constructing <see cref="SocialContext"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// The social context builder aggregates all modifiers and factors affecting
/// a social interaction check. It provides convenient methods for common
/// modifier scenarios while allowing direct modifier injection.
/// </para>
/// </remarks>
public class SocialContextBuilder : ISocialContextBuilder
{
    private readonly ILogger<SocialContextBuilder>? _logger;
    
    private SocialInteractionType _interactionType;
    private string _targetId = string.Empty;
    private DispositionLevel _targetDisposition = DispositionLevel.CreateNeutral();
    private string? _targetFactionId;
    private string? _cultureId;
    private int _baseDc = 2;
    private readonly List<SocialModifier> _socialModifiers = new();
    private readonly List<EquipmentModifier> _equipmentModifiers = new();
    private readonly List<SituationalModifier> _situationalModifiers = new();
    private readonly List<string> _appliedStatuses = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SocialContextBuilder"/> class.
    /// </summary>
    public SocialContextBuilder()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SocialContextBuilder"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public SocialContextBuilder(ILogger<SocialContextBuilder>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithInteractionType(SocialInteractionType interactionType)
    {
        _interactionType = interactionType;
        _logger?.LogDebug("Set interaction type to {InteractionType}", interactionType);
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithTarget(string targetId, DispositionLevel disposition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));
        
        _targetId = targetId;
        _targetDisposition = disposition;
        _logger?.LogDebug(
            "Set target {TargetId} with disposition {Disposition} ({DiceModifier:+0;-0;+0}d10)",
            targetId, disposition.Category, disposition.DiceModifier);
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithFaction(string factionId, int playerStanding)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(factionId, nameof(factionId));
        
        _targetFactionId = factionId;

        // Convert standing to dice modifier based on faction reputation thresholds
        var diceModifier = playerStanding switch
        {
            >= 75 => 1,   // Honored
            >= 25 => 0,   // Friendly/Neutral
            >= -24 => 0,  // Neutral
            >= -74 => -1, // Unfriendly
            _ => -2       // Hostile
        };

        if (diceModifier != 0)
        {
            var standing = playerStanding switch
            {
                >= 75 => "Honored",
                <= -75 => "Hostile",
                <= -25 => "Unfriendly",
                _ => "Neutral"
            };

            _socialModifiers.Add(SocialModifier.FactionStanding(factionId, diceModifier, standing));
            _logger?.LogDebug(
                "Added faction modifier for {FactionId}: {Standing} ({DiceModifier:+0;-0;+0}d10)",
                factionId, standing, diceModifier);
        }

        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithCulture(string cultureId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureId, nameof(cultureId));
        
        _cultureId = cultureId;
        _logger?.LogDebug("Set culture to {CultureId}", cultureId);
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithBaseDc(int baseDc)
    {
        _baseDc = Math.Max(1, baseDc);
        _logger?.LogDebug("Set base DC to {BaseDc}", _baseDc);
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithSocialModifier(SocialModifier modifier)
    {
        _socialModifiers.Add(modifier);
        _logger?.LogDebug("Added social modifier: {Modifier}", modifier.ToShortDescription());
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithEquipment(EquipmentModifier modifier)
    {
        _equipmentModifiers.Add(modifier);
        _logger?.LogDebug("Added equipment modifier: {Modifier}", modifier.ToShortDescription());
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithSituation(SituationalModifier modifier)
    {
        _situationalModifiers.Add(modifier);
        _logger?.LogDebug("Added situational modifier: {Modifier}", modifier.ToShortDescription());
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithAppliedStatus(string statusId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(statusId, nameof(statusId));
        
        if (!_appliedStatuses.Contains(statusId, StringComparer.OrdinalIgnoreCase))
        {
            _appliedStatuses.Add(statusId);
            _logger?.LogDebug("Added applied status: {StatusId}", statusId);
        }
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder TargetIsSuspicious()
    {
        _socialModifiers.Add(SocialModifier.Suspicious());
        _logger?.LogDebug("Target is suspicious: Deception DC +4");
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder TargetIsTrusting()
    {
        _socialModifiers.Add(SocialModifier.Trusting());
        _logger?.LogDebug("Target is trusting: Deception DC -4");
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithArgumentAlignment(bool aligned, string? reason = null)
    {
        _socialModifiers.Add(SocialModifier.ArgumentAlignment(aligned, reason));
        var effect = aligned ? "+1d10" : "-1d10";
        _logger?.LogDebug("Argument alignment: {Aligned} ({Effect})", aligned, effect);
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithEvidence(string evidenceDescription)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceDescription, nameof(evidenceDescription));
        
        _socialModifiers.Add(SocialModifier.Evidence(evidenceDescription));
        _logger?.LogDebug("Added evidence: {Evidence} (+2d10)", evidenceDescription);
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithStrengthComparison(bool playerStronger)
    {
        _socialModifiers.Add(SocialModifier.StrengthComparison(playerStronger));
        var effect = playerStronger ? "+1d10" : "-1d10";
        _logger?.LogDebug("Strength comparison: Player {Comparison} ({Effect})", 
            playerStronger ? "stronger" : "weaker", effect);
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder TargetHasBackup()
    {
        _socialModifiers.Add(SocialModifier.HasBackup());
        _logger?.LogDebug("Target has backup: Intimidation -1d10");
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WieldingArtifact(string artifactName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactName, nameof(artifactName));
        
        _socialModifiers.Add(SocialModifier.WieldingArtifact(artifactName));
        _logger?.LogDebug("Wielding artifact: {Artifact} (Intimidation +1d10)", artifactName);
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithReputation(string status, int diceModifier,
        IReadOnlyList<SocialInteractionType>? appliesToTypes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status, nameof(status));
        
        _socialModifiers.Add(SocialModifier.Reputation(status, diceModifier, appliesToTypes));
        _logger?.LogDebug("Added reputation: [{Status}] ({DiceModifier:+0;-0;+0}d10)", status, diceModifier);
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder IsUntrustworthy()
    {
        _socialModifiers.Add(SocialModifier.Untrustworthy());
        _logger?.LogDebug("Player is untrustworthy: All social checks DC +3");
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder TargetIsStressed()
    {
        _socialModifiers.Add(SocialModifier.TargetStressed());
        _logger?.LogDebug("Target is stressed: Persuasion +1d10");
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder TargetIsFeared()
    {
        _socialModifiers.Add(SocialModifier.TargetFeared());
        _logger?.LogDebug("Target is feared: Persuasion +1d10");
        return this;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder WithCulturalKnowledge(string cultureName, bool fluent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureName, nameof(cultureName));
        
        _socialModifiers.Add(SocialModifier.CulturalKnowledge(cultureName, fluent));
        var effect = fluent ? "+1d10" : "-1d10";
        _logger?.LogDebug("Cultural knowledge: {Culture} {Fluency} ({Effect})", 
            cultureName, fluent ? "fluent" : "none", effect);
        return this;
    }

    /// <inheritdoc/>
    public SocialContext Build()
    {
        if (string.IsNullOrEmpty(_targetId))
        {
            throw new InvalidOperationException("Target ID must be set before building social context.");
        }

        // Filter modifiers that apply to the current interaction type
        var applicableModifiers = _socialModifiers
            .Where(m => m.AppliesTo(_interactionType))
            .ToList();

        var context = new SocialContext(
            InteractionType: _interactionType,
            TargetId: _targetId,
            TargetDisposition: _targetDisposition,
            TargetFactionId: _targetFactionId,
            CultureId: _cultureId,
            BaseDc: _baseDc,
            SocialModifiers: applicableModifiers.AsReadOnly(),
            EquipmentModifiers: _equipmentModifiers.AsReadOnly(),
            SituationalModifiers: _situationalModifiers.AsReadOnly(),
            AppliedStatuses: _appliedStatuses.AsReadOnly());

        _logger?.LogInformation(
            "Built social context for {InteractionType} vs {TargetId}: " +
            "Disposition {Disposition} ({DiceModifier:+0;-0;+0}d10), " +
            "Base DC {BaseDc}, {ModifierCount} modifiers, Effective DC {EffectiveDc}",
            _interactionType,
            _targetId,
            _targetDisposition.Category,
            _targetDisposition.DiceModifier,
            _baseDc,
            context.ModifierCount,
            context.EffectiveDc);

        return context;
    }

    /// <inheritdoc/>
    public ISocialContextBuilder Reset()
    {
        _interactionType = default;
        _targetId = string.Empty;
        _targetDisposition = DispositionLevel.CreateNeutral();
        _targetFactionId = null;
        _cultureId = null;
        _baseDc = 2;
        _socialModifiers.Clear();
        _equipmentModifiers.Clear();
        _situationalModifiers.Clear();
        _appliedStatuses.Clear();
        
        _logger?.LogDebug("Builder reset to default state");
        return this;
    }
}
