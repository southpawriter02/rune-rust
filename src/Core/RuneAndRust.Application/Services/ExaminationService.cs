using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using DomainSkillCheckService = RuneAndRust.Domain.Services.SkillCheckService;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for examination and perception operations.
/// </summary>
public class ExaminationService : IExaminationService
{
    private readonly IExaminationRepository _examinationRepository;
    private readonly IPerceptionRepository _perceptionRepository;
    private readonly IFloraFaunaRepository _floraFaunaRepository;
    private readonly DomainSkillCheckService _skillCheckService;
    private readonly Random _random;

    public ExaminationService(
        IExaminationRepository examinationRepository,
        IPerceptionRepository perceptionRepository,
        IFloraFaunaRepository floraFaunaRepository,
        DomainSkillCheckService? skillCheckService = null,
        Random? random = null)
    {
        _examinationRepository = examinationRepository ?? throw new ArgumentNullException(nameof(examinationRepository));
        _perceptionRepository = perceptionRepository ?? throw new ArgumentNullException(nameof(perceptionRepository));
        _floraFaunaRepository = floraFaunaRepository ?? throw new ArgumentNullException(nameof(floraFaunaRepository));
        _skillCheckService = skillCheckService ?? new DomainSkillCheckService();
        _random = random ?? new Random();
    }

    public async Task<ExaminationResultDto> ExamineObjectAsync(
        Guid objectId,
        string objectType,
        ObjectCategory category,
        int witsValue,
        Biome? currentBiome = null,
        CancellationToken ct = default)
    {
        // Determine highest layer based on WITS check
        var (highestLayer, checkResult) = _skillCheckService.DetermineExaminationLayer(witsValue);
        var maxLayer = (ExaminationLayer)highestLayer;

        // Get descriptors up to that layer
        var descriptors = await _examinationRepository.GetDescriptorsByLayerAsync(
            category, objectType, maxLayer, currentBiome, ct);

        // Build layer texts
        var layerTexts = descriptors
            .OrderBy(d => d.Layer)
            .Select(d => d.ToDto())
            .ToList();

        // Build composite description from all unlocked layers
        var compositeDescription = layerTexts.Count > 0
            ? string.Join("\n\n", layerTexts.Select(l => l.Text))
            : GetDefaultDescription(objectType, category);

        // Check for hints and solutions
        var revealsHint = descriptors.Any(d => d.RevealsHint);
        var revealedSolution = descriptors
            .Where(d => d.RevealsSolutionId != null)
            .Select(d => d.RevealsSolutionId)
            .FirstOrDefault();

        return new ExaminationResultDto(
            ObjectId: objectId,
            ObjectName: objectType,
            WitsRoll: checkResult.RollResult,
            WitsTotal: checkResult.TotalResult,
            HighestLayerUnlocked: maxLayer,
            CompositeDescription: compositeDescription,
            RevealedHint: revealsHint,
            RevealedSolutionId: revealedSolution,
            LayerTexts: layerTexts
        );
    }

    public async Task<PassivePerceptionResultDto> CheckPassivePerceptionAsync(
        Guid roomId,
        int passivePerception,
        IReadOnlyList<HiddenElement> hiddenElements,
        CancellationToken ct = default)
    {
        var revealedElements = new List<RevealedElementDto>();
        var narratives = new List<string>();

        foreach (var element in hiddenElements.Where(h => !h.IsRevealed))
        {
            if (element.CanBeDetectedBy(passivePerception))
            {
                element.Reveal();
                revealedElements.Add(element.ToDto());

                // Get a perception descriptor for narrative flavor
                var descriptors = await _perceptionRepository.GetDescriptorsAsync(
                    element.ElementType, PerceptionSuccessLevel.Standard, null, ct);

                if (descriptors.Count > 0)
                {
                    var descriptor = descriptors[_random.Next(descriptors.Count)];
                    narratives.Add(descriptor.DescriptorText);
                }
                else
                {
                    narratives.Add(element.DiscoveryText);
                }
            }
        }

        var narrative = narratives.Count > 0
            ? string.Join("\n\n", narratives)
            : null;

        return new PassivePerceptionResultDto(
            RoomId: roomId,
            PassivePerception: passivePerception,
            RevealedElements: revealedElements,
            PerceptionNarrative: narrative
        );
    }

    public async Task<SearchResultDto> PerformActiveSearchAsync(
        Guid roomId,
        int witsValue,
        IReadOnlyList<HiddenElement> hiddenElements,
        CancellationToken ct = default)
    {
        // Perform active WITS check
        var checkResult = _skillCheckService.PerformCheck(witsValue, 0); // No DC, use raw result

        var revealedElements = new List<RevealedElementDto>();
        var narratives = new List<string>();

        foreach (var element in hiddenElements.Where(h => !h.IsRevealed))
        {
            if (checkResult.TotalResult >= element.DetectionDC)
            {
                element.Reveal();
                revealedElements.Add(element.ToDto());

                // Determine success level based on how much we exceeded the DC
                var successLevel = checkResult.TotalResult >= element.DetectionDC + 5
                    ? PerceptionSuccessLevel.Expert
                    : PerceptionSuccessLevel.Standard;

                var descriptors = await _perceptionRepository.GetDescriptorsAsync(
                    element.ElementType, successLevel, null, ct);

                if (descriptors.Count > 0)
                {
                    var descriptor = descriptors[_random.Next(descriptors.Count)];
                    narratives.Add(descriptor.DescriptorText);
                }
                else
                {
                    narratives.Add(element.DiscoveryText);
                }
            }
        }

        var searchNarrative = revealedElements.Count > 0
            ? string.Join("\n\n", narratives)
            : "You search the area carefully but find nothing hidden.";

        return new SearchResultDto(
            RoomId: roomId,
            WitsRoll: checkResult.RollResult,
            WitsTotal: checkResult.TotalResult,
            RevealedElements: revealedElements,
            SearchNarrative: searchNarrative,
            FoundAnything: revealedElements.Count > 0
        );
    }

    public async Task<AmbientObservationDto?> GetAmbientObservationAsync(
        Biome biome,
        int witsValue,
        CancellationToken ct = default)
    {
        var descriptors = await _floraFaunaRepository.GetByBiomeAsync(biome, null, ct);
        if (descriptors.Count == 0)
            return null;

        // Select a random species from available descriptors
        var speciesGroups = descriptors.GroupBy(d => d.SpeciesName).ToList();
        if (speciesGroups.Count == 0)
            return null;

        var selectedGroup = speciesGroups[_random.Next(speciesGroups.Count)];
        var speciesName = selectedGroup.Key;

        // Determine layer based on WITS
        var (highestLayer, _) = _skillCheckService.DetermineExaminationLayer(witsValue);
        var maxLayer = (ExaminationLayer)highestLayer;

        // Get descriptors for this species up to the determined layer
        var speciesDescriptors = await _floraFaunaRepository.GetSpeciesDescriptorsAsync(
            speciesName, maxLayer, ct);

        if (speciesDescriptors.Count == 0)
            return null;

        // Build observation from all available layers
        var orderedDescriptors = speciesDescriptors.OrderBy(d => d.Layer).ToList();
        var observation = string.Join("\n\n", orderedDescriptors.Select(d => d.DescriptorText));

        var highestDescriptor = orderedDescriptors.Last();

        return new AmbientObservationDto(
            SpeciesName: speciesName,
            ScientificName: highestDescriptor.ScientificName,
            Category: highestDescriptor.Category,
            LayerUnlocked: maxLayer,
            Observation: observation,
            AlchemicalUse: highestDescriptor.AlchemicalUse,
            IsHarvestable: highestDescriptor.IsHarvestable,
            HarvestDC: highestDescriptor.HarvestDC
        );
    }

    private static string GetDefaultDescription(string objectType, ObjectCategory category)
    {
        return category switch
        {
            ObjectCategory.Door => $"A {objectType.ToLower()}.",
            ObjectCategory.Machinery => $"Some kind of {objectType.ToLower()}.",
            ObjectCategory.Container => $"A {objectType.ToLower()} that might contain something.",
            ObjectCategory.Corpse => $"The remains of {objectType.ToLower()}.",
            ObjectCategory.Inscription => $"Some writing: {objectType}.",
            _ => $"You see {objectType.ToLower()}."
        };
    }
}
