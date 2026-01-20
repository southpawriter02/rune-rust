using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for monster group definitions (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// JsonMonsterGroupProvider loads monster group definitions from a JSON configuration file
/// and provides thread-safe access to the loaded definitions with indexed lookups.
/// </para>
/// <para>Expected JSON format:</para>
/// <code>
/// {
///   "groups": [
///     {
///       "groupId": "goblin-warband",
///       "name": "Goblin Warband",
///       "description": "A coordinated goblin raiding party",
///       "leaderRole": "leader",
///       "tags": ["goblinoid", "dungeon-level-1"],
///       "members": [
///         { "monsterDefinitionId": "goblin-shaman", "count": 1, "role": "leader" },
///         { "monsterDefinitionId": "goblin-warrior", "count": 3, "role": "melee" }
///       ],
///       "tactics": ["Flank", "FocusFire", "ProtectLeader"],
///       "synergies": [
///         {
///           "synergyId": "shamans-blessing",
///           "name": "Shaman's Blessing",
///           "trigger": "Always",
///           "sourceRole": "leader",
///           "attackBonus": 1
///         }
///       ]
///     }
///   ]
/// }
/// </code>
/// <para>
/// The provider builds an index for efficient lookup by group ID.
/// </para>
/// </remarks>
public class JsonMonsterGroupProvider : IMonsterGroupProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary index: group ID -> MonsterGroupDefinition.
    /// </summary>
    private readonly Dictionary<string, MonsterGroupDefinition> _groups;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<JsonMonsterGroupProvider> _logger;

    /// <summary>
    /// Path to the configuration file.
    /// </summary>
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new JsonMonsterGroupProvider instance.
    /// </summary>
    /// <param name="configPath">Path to the monster-groups.json configuration file.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when configPath or logger is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the configuration file is invalid.</exception>
    public JsonMonsterGroupProvider(string configPath, ILogger<JsonMonsterGroupProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _groups = new Dictionary<string, MonsterGroupDefinition>(StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug(
            "Initializing monster group provider from configuration file: {ConfigPath}",
            configPath);

        LoadGroups();

        _logger.LogInformation(
            "Monster group provider initialized successfully with {GroupCount} groups, total {MemberCount} members",
            _groups.Count,
            _groups.Values.Sum(g => g.TotalMemberCount));
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public MonsterGroupDefinition? GetGroup(string groupId)
    {
        ArgumentNullException.ThrowIfNull(groupId);

        var found = _groups.TryGetValue(groupId.ToLowerInvariant(), out var group);

        _logger.LogDebug(
            "GetGroup: {GroupId} -> {Result}",
            groupId,
            found ? group!.Name : "not found");

        return group;
    }

    /// <inheritdoc />
    public IReadOnlyList<MonsterGroupDefinition> GetAllGroups()
    {
        var groups = _groups.Values.ToList();

        _logger.LogDebug(
            "GetAllGroups: returning {Count} groups",
            groups.Count);

        return groups;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetGroupIds()
    {
        var ids = _groups.Keys.ToList();

        _logger.LogDebug(
            "GetGroupIds: returning {Count} group IDs",
            ids.Count);

        return ids;
    }

    /// <inheritdoc />
    public bool GroupExists(string groupId)
    {
        ArgumentNullException.ThrowIfNull(groupId);

        var exists = _groups.ContainsKey(groupId.ToLowerInvariant());

        _logger.LogDebug(
            "GroupExists: {GroupId} -> {Result}",
            groupId,
            exists);

        return exists;
    }

    /// <inheritdoc />
    public IReadOnlyList<MonsterGroupDefinition> GetGroupsByTag(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        var groups = _groups.Values
            .Where(g => g.HasTag(tag))
            .ToList();

        _logger.LogDebug(
            "GetGroupsByTag: tag={Tag} -> {Count} groups",
            tag,
            groups.Count);

        return groups;
    }

    /// <inheritdoc />
    public void Reload()
    {
        _logger.LogInformation("Reloading monster group definitions from {ConfigPath}", _configPath);

        _groups.Clear();
        LoadGroups();

        _logger.LogInformation(
            "Monster group definitions reloaded: {GroupCount} groups loaded",
            _groups.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads monster group definitions from the JSON configuration file.
    /// </summary>
    private void LoadGroups()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Monster group configuration file not found: {Path}",
                _configPath);

            throw new FileNotFoundException(
                $"Monster group configuration file not found: {_configPath}",
                _configPath);
        }

        var json = File.ReadAllText(_configPath);

        _logger.LogDebug(
            "Read {Length} bytes from monster group configuration file",
            json.Length);

        var config = JsonSerializer.Deserialize<MonsterGroupsConfigDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Groups is null || config.Groups.Count == 0)
        {
            _logger.LogError(
                "Monster group configuration is empty or invalid");

            throw new InvalidDataException(
                "Monster group configuration must contain at least one group definition.");
        }

        _logger.LogDebug(
            "Parsing {Count} monster group definitions from configuration",
            config.Groups.Count);

        foreach (var dto in config.Groups)
        {
            try
            {
                var group = MapToMonsterGroupDefinition(dto);

                // Validate the group definition
                var errors = group.GetValidationErrors();
                if (errors.Count > 0)
                {
                    _logger.LogWarning(
                        "Group '{GroupId}' has validation warnings: {Warnings}",
                        dto.GroupId,
                        string.Join("; ", errors));
                }

                _groups[group.GroupId] = group;

                _logger.LogDebug(
                    "Loaded group: {GroupId} ({Name}) - {MemberCount} members, {TacticCount} tactics, {SynergyCount} synergies",
                    group.GroupId,
                    group.Name,
                    group.TotalMemberCount,
                    group.Tactics.Count,
                    group.Synergies.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load group '{GroupId}': {Error}",
                    dto.GroupId,
                    ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Maps a DTO from JSON to a MonsterGroupDefinition domain entity.
    /// </summary>
    /// <param name="dto">The DTO from JSON deserialization.</param>
    /// <returns>A MonsterGroupDefinition entity.</returns>
    private MonsterGroupDefinition MapToMonsterGroupDefinition(MonsterGroupDto dto)
    {
        _logger.LogDebug(
            "Mapping group DTO: {GroupId} with {MemberCount} members, {TacticCount} tactics, {SynergyCount} synergies",
            dto.GroupId,
            dto.Members?.Count ?? 0,
            dto.Tactics?.Count ?? 0,
            dto.Synergies?.Count ?? 0);

        var group = MonsterGroupDefinition.Create(
            dto.GroupId,
            dto.Name,
            dto.Description ?? string.Empty);

        // Set leader role if provided
        if (!string.IsNullOrWhiteSpace(dto.LeaderRole))
        {
            group.WithLeader(dto.LeaderRole);
        }

        // Map members
        if (dto.Members != null)
        {
            foreach (var memberDto in dto.Members)
            {
                var member = MapToGroupMember(memberDto);
                group.WithMember(member);

                _logger.LogDebug(
                    "  Mapped member: {MonsterDefId} x{Count} (role: {Role})",
                    member.MonsterDefinitionId,
                    member.Count,
                    member.Role ?? "none");
            }
        }

        // Map tactics
        if (dto.Tactics != null)
        {
            var tactics = dto.Tactics
                .Select(ParseGroupTactic)
                .Where(t => t.HasValue)
                .Select(t => t!.Value)
                .ToArray();

            if (tactics.Length > 0)
            {
                group.WithTactics(tactics);

                _logger.LogDebug(
                    "  Mapped tactics: {Tactics}",
                    string.Join(", ", tactics));
            }
        }

        // Map synergies
        if (dto.Synergies != null)
        {
            foreach (var synergyDto in dto.Synergies)
            {
                var synergy = MapToGroupSynergy(synergyDto);
                if (synergy.IsValid)
                {
                    group.WithSynergy(synergy);

                    _logger.LogDebug(
                        "  Mapped synergy: {SynergyId} ({Trigger})",
                        synergy.SynergyId,
                        synergy.Trigger);
                }
            }
        }

        // Map tags
        if (dto.Tags != null && dto.Tags.Count > 0)
        {
            group.WithTags(dto.Tags.ToArray());

            _logger.LogDebug(
                "  Mapped tags: {Tags}",
                string.Join(", ", dto.Tags));
        }

        return group;
    }

    /// <summary>
    /// Maps a member DTO to a GroupMember entity.
    /// </summary>
    /// <param name="dto">The member DTO from JSON.</param>
    /// <returns>A GroupMember entity.</returns>
    private GroupMember MapToGroupMember(GroupMemberDto dto)
    {
        var member = GroupMember.Create(
            dto.MonsterDefinitionId,
            dto.Count > 0 ? dto.Count : 1,
            dto.Role);

        // Map preferred position if provided
        if (dto.PreferredPosition != null)
        {
            var offset = new GridOffset(
                dto.PreferredPosition.DeltaX,
                dto.PreferredPosition.DeltaY);
            member.WithPreferredPosition(offset);
        }

        return member;
    }

    /// <summary>
    /// Maps a synergy DTO to a GroupSynergy entity.
    /// </summary>
    /// <param name="dto">The synergy DTO from JSON.</param>
    /// <returns>A GroupSynergy entity.</returns>
    private GroupSynergy MapToGroupSynergy(GroupSynergyDto dto)
    {
        var trigger = ParseSynergyTrigger(dto.Trigger);

        var synergy = GroupSynergy.Create(
            dto.SynergyId,
            dto.Name,
            trigger);

        // Set optional properties
        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            synergy.WithDescription(dto.Description);
        }

        if (!string.IsNullOrWhiteSpace(dto.SourceRole))
        {
            synergy.WithSourceRole(dto.SourceRole);
        }

        if (!string.IsNullOrWhiteSpace(dto.StatusEffectId))
        {
            synergy.WithStatusEffect(dto.StatusEffectId);
        }

        if (dto.AttackBonus != 0)
        {
            synergy.WithAttackBonus(dto.AttackBonus);
        }

        if (dto.DamageBonus != 0)
        {
            synergy.WithDamageBonus(dto.DamageBonus);
        }

        if (dto.AppliesToAllMembers.HasValue)
        {
            synergy.WithAppliesToAllMembers(dto.AppliesToAllMembers.Value);
        }

        return synergy;
    }

    /// <summary>
    /// Parses a group tactic string to the enum value.
    /// </summary>
    /// <param name="value">The string value from JSON.</param>
    /// <returns>The parsed enum value, or null if invalid.</returns>
    private GroupTactic? ParseGroupTactic(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (Enum.TryParse<GroupTactic>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        _logger.LogWarning(
            "Unknown group tactic '{Value}', skipping",
            value);

        return null;
    }

    /// <summary>
    /// Parses a synergy trigger string to the enum value.
    /// </summary>
    /// <param name="value">The string value from JSON.</param>
    /// <returns>The parsed enum value, defaulting to Always.</returns>
    private SynergyTrigger ParseSynergyTrigger(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return SynergyTrigger.Always;
        }

        if (Enum.TryParse<SynergyTrigger>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        _logger.LogWarning(
            "Unknown synergy trigger '{Value}', defaulting to Always",
            value);

        return SynergyTrigger.Always;
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTO CLASSES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Root configuration DTO for monster-groups.json.
    /// </summary>
    private sealed class MonsterGroupsConfigDto
    {
        public List<MonsterGroupDto>? Groups { get; set; }
    }

    /// <summary>
    /// DTO for individual monster group definitions in JSON.
    /// </summary>
    private sealed class MonsterGroupDto
    {
        public string GroupId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LeaderRole { get; set; }
        public List<string>? Tags { get; set; }
        public List<GroupMemberDto>? Members { get; set; }
        public List<string>? Tactics { get; set; }
        public List<GroupSynergyDto>? Synergies { get; set; }
    }

    /// <summary>
    /// DTO for group member definitions in JSON.
    /// </summary>
    private sealed class GroupMemberDto
    {
        public string MonsterDefinitionId { get; set; } = string.Empty;
        public int Count { get; set; } = 1;
        public string? Role { get; set; }
        public GridOffsetDto? PreferredPosition { get; set; }
    }

    /// <summary>
    /// DTO for grid offset in JSON.
    /// </summary>
    private sealed class GridOffsetDto
    {
        public int DeltaX { get; set; }
        public int DeltaY { get; set; }
    }

    /// <summary>
    /// DTO for group synergy definitions in JSON.
    /// </summary>
    private sealed class GroupSynergyDto
    {
        public string SynergyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Trigger { get; set; }
        public string? SourceRole { get; set; }
        public string? StatusEffectId { get; set; }
        public int AttackBonus { get; set; }
        public int DamageBonus { get; set; }
        public bool? AppliesToAllMembers { get; set; }
    }
}
