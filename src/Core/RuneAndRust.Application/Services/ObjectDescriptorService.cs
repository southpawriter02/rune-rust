using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Generates state-based descriptions for interactive objects.
/// </summary>
/// <remarks>
/// This service provides progressive detail descriptions for interactive objects
/// based on their type, state, and the depth of examination. It integrates with
/// the environment context from v0.0.11a for coherent descriptions.
/// </remarks>
public class ObjectDescriptorService
{
    private readonly DescriptorService _descriptorService;
    private readonly ObjectDescriptorConfiguration _config;
    private readonly ILogger<ObjectDescriptorService> _logger;

    public ObjectDescriptorService(
        DescriptorService descriptorService,
        ObjectDescriptorConfiguration config,
        ILogger<ObjectDescriptorService> logger)
    {
        _descriptorService = descriptorService ?? throw new ArgumentNullException(nameof(descriptorService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "ObjectDescriptorService initialized with {ObjectTypes} object types",
            _config.ObjectTypes.Count);
    }

    /// <summary>
    /// Generates a complete descriptor for an interactive object.
    /// </summary>
    /// <param name="context">The object descriptor context.</param>
    /// <returns>A complete interactive object descriptor.</returns>
    public InteractiveObjectDescriptor GenerateDescriptor(ObjectDescriptorContext context)
    {
        _logger.LogDebug(
            "Generating descriptor for {ObjectType} in state {State} at depth {Depth}",
            context.ObjectType, context.State, context.Depth);

        var tags = BuildTags(context);
        var objectTypeName = context.ObjectType.ToString().ToLowerInvariant();
        var stateName = context.State.ToString().ToLowerInvariant();

        return new InteractiveObjectDescriptor
        {
            ObjectType = context.ObjectType,
            State = context.State,
            GlanceDescription = GetGlanceDescription(objectTypeName, stateName, tags),
            LookDescription = GetLookDescription(objectTypeName, stateName, tags),
            ExamineDescription = GetExamineDescription(objectTypeName, stateName, context, tags),
            InteractionHint = GetInteractionHint(context.ObjectType, context.State)
        };
    }

    /// <summary>
    /// Gets a brief glance description for room entry.
    /// </summary>
    public string GetGlanceDescription(
        InteractiveObjectType objectType,
        ObjectState state)
    {
        var objectTypeName = objectType.ToString().ToLowerInvariant();
        var stateName = state.ToString().ToLowerInvariant();
        return GetGlanceDescription(objectTypeName, stateName, []);
    }

    /// <summary>
    /// Gets a description appropriate for the examination depth.
    /// </summary>
    public string GetDescription(ObjectDescriptorContext context)
    {
        var descriptor = GenerateDescriptor(context);
        return descriptor.GetDescription(context.Depth);
    }

    private string GetGlanceDescription(string objectType, string state, List<string> tags)
    {
        // Pool path: objects.{type}_{state}_glance or objects.{type}_glance
        var poolPath = $"objects.{objectType}_{state}_glance";
        var description = _descriptorService.GetDescriptor(poolPath);

        if (string.IsNullOrEmpty(description))
        {
            poolPath = $"objects.{objectType}_glance";
            description = _descriptorService.GetDescriptor(poolPath);
        }

        return description ?? GetDefaultGlance(objectType, state);
    }

    private string GetLookDescription(string objectType, string state, List<string> tags)
    {
        // Pool path: objects.{type}_{state}_look or objects.{type}_look
        var poolPath = $"objects.{objectType}_{state}_look";
        var description = _descriptorService.GetDescriptor(poolPath);

        if (string.IsNullOrEmpty(description))
        {
            poolPath = $"objects.{objectType}_look";
            description = _descriptorService.GetDescriptor(poolPath);
        }

        return description ?? GetDefaultLook(objectType, state);
    }

    private string GetExamineDescription(
        string objectType,
        string state,
        ObjectDescriptorContext context,
        List<string> tags)
    {
        // Pool path: objects.{type}_{state}_examine or objects.{type}_examine
        var poolPath = $"objects.{objectType}_{state}_examine";
        var description = _descriptorService.GetDescriptor(poolPath);

        if (string.IsNullOrEmpty(description))
        {
            poolPath = $"objects.{objectType}_examine";
            description = _descriptorService.GetDescriptor(poolPath);
        }

        // Add material/age details if present
        if (!string.IsNullOrEmpty(context.Material))
        {
            description = AddMaterialDetail(description, context.Material);
        }

        if (!string.IsNullOrEmpty(context.Age))
        {
            description = AddAgeDetail(description, context.Age);
        }

        return description ?? GetDefaultExamine(objectType, state);
    }

    private string? GetInteractionHint(InteractiveObjectType objectType, ObjectState state)
    {
        return (objectType, state) switch
        {
            (InteractiveObjectType.Door, ObjectState.Locked) => "It might be opened with the right key.",
            (InteractiveObjectType.Door, ObjectState.Closed) => "It could be opened.",
            (InteractiveObjectType.Chest, ObjectState.Locked) => "A key might reveal its contents.",
            (InteractiveObjectType.Chest, ObjectState.Closed) => "It awaits opening.",
            (InteractiveObjectType.Lever, ObjectState.Up) or
            (InteractiveObjectType.Lever, ObjectState.Down) => "It could be pulled.",
            (InteractiveObjectType.Lever, ObjectState.Stuck) => "It seems jammed.",
            (InteractiveObjectType.Altar, ObjectState.Active) => "It radiates power.",
            (InteractiveObjectType.Altar, ObjectState.Inactive) => "It might be activated.",
            _ => null
        };
    }

    private List<string> BuildTags(ObjectDescriptorContext context)
    {
        var tags = new List<string>(context.Tags);

        if (context.Environment != null)
        {
            if (!string.IsNullOrEmpty(context.Environment.Value.Biome))
                tags.Add(context.Environment.Value.Biome);
            if (!string.IsNullOrEmpty(context.Environment.Value.Era))
                tags.Add(context.Environment.Value.Era);
        }

        if (!string.IsNullOrEmpty(context.Material))
            tags.Add(context.Material);

        if (!string.IsNullOrEmpty(context.Age))
            tags.Add(context.Age);

        return tags;
    }

    private static string AddMaterialDetail(string? description, string material)
    {
        if (string.IsNullOrEmpty(description)) return $"It is made of {material}.";
        return $"{description} It is crafted from {material}.";
    }

    private static string AddAgeDetail(string? description, string age)
    {
        if (string.IsNullOrEmpty(description)) return $"It appears {age}.";
        return $"{description} It appears {age}.";
    }

    private static string GetDefaultGlance(string objectType, string state)
        => $"A {state} {objectType} is here.";

    private static string GetDefaultLook(string objectType, string state)
        => $"You see a {objectType}. It is {state}.";

    private static string GetDefaultExamine(string objectType, string state)
        => $"Upon closer examination, you see a {objectType} that is {state}.";
}
