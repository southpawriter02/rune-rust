// ═══════════════════════════════════════════════════════════════════════════════
// CharacterCreationController.cs
// Application service orchestrating the 6-step character creation workflow.
// Manages state transitions, validates selections against domain providers,
// coordinates with the ViewModel builder for TUI rendering, and delegates
// final character creation to the character factory. After creation, persists
// the Player entity via IPlayerRepository. Provides forward/backward navigation
// and session lifecycle management (initialize, cancel).
// Version: 0.17.5g
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Orchestrates the character creation workflow, managing state transitions,
/// validating selections, and coordinating with providers and the character factory.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CharacterCreationController"/> is the central orchestrator for the
/// character creation workflow. It sits between the TUI screens (v0.17.5f) and
/// the domain providers, translating user selections into validated state
/// transitions with ViewModel updates.
/// </para>
/// <para>
/// <strong>Dependencies:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><see cref="ILineageProvider"/> — validates lineage selections</description></item>
///   <item><description><see cref="IBackgroundProvider"/> — validates background selections</description></item>
///   <item><description><see cref="IArchetypeProvider"/> — validates archetype selections</description></item>
///   <item><description><see cref="ISpecializationProvider"/> — validates specialization-archetype compatibility</description></item>
///   <item><description><see cref="IViewModelBuilder"/> — builds ViewModel after every state change</description></item>
///   <item><description><see cref="INameValidator"/> — validates character name at confirmation</description></item>
///   <item><description><see cref="ICharacterFactory"/> (optional) — creates Player entity from completed state</description></item>
///   <item><description><see cref="IPlayerRepository"/> (optional) — persists Player entity to storage after creation</description></item>
///   <item><description><see cref="ILogger{T}"/> — structured diagnostic logging</description></item>
/// </list>
/// <para>
/// <strong>Session Lifecycle:</strong> Call <see cref="Initialize"/> to start a new
/// session, step methods to progress through the workflow, <see cref="GoBack"/> to
/// navigate backward, <see cref="Cancel"/> to abort, and <see cref="ConfirmCharacterAsync"/>
/// to finalize. After confirmation or cancellation, <see cref="IsSessionActive"/> is <c>false</c>.
/// </para>
/// </remarks>
/// <seealso cref="ICharacterCreationController"/>
/// <seealso cref="CharacterCreationState"/>
/// <seealso cref="StepResult"/>
/// <seealso cref="CharacterCreationResult"/>
public class CharacterCreationController : ICharacterCreationController
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Provider for lineage definitions and attribute modifiers.</summary>
    private readonly ILineageProvider _lineageProvider;

    /// <summary>Provider for background definitions and grants.</summary>
    private readonly IBackgroundProvider _backgroundProvider;

    /// <summary>Provider for archetype definitions and specialization mappings.</summary>
    private readonly IArchetypeProvider _archetypeProvider;

    /// <summary>Provider for specialization definitions and archetype compatibility.</summary>
    private readonly ISpecializationProvider _specializationProvider;

    /// <summary>Builder for creating ViewModels from creation state.</summary>
    private readonly IViewModelBuilder _viewModelBuilder;

    /// <summary>Validator for character names (length, pattern, profanity).</summary>
    private readonly INameValidator _nameValidator;

    /// <summary>
    /// Optional factory for creating Player entities from completed state.
    /// Null until v0.17.5e provides the implementation.
    /// </summary>
    private readonly ICharacterFactory? _characterFactory;

    /// <summary>
    /// Optional repository for persisting Player entities to storage.
    /// When null, character creation succeeds but persistence is skipped (logged as warning).
    /// </summary>
    private readonly IPlayerRepository? _playerRepository;

    /// <summary>Logger for structured diagnostic output.</summary>
    private readonly ILogger<CharacterCreationController> _logger;

    /// <summary>
    /// Current creation session state. Null when no session is active.
    /// </summary>
    private CharacterCreationState? _state;

    // ═══════════════════════════════════════════════════════════════════════════
    // ERROR MESSAGES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Error when an operation is attempted without an active session.</summary>
    private const string ErrorNoActiveSession = "No active creation session.";

    /// <summary>Error when Clan-Born lineage is selected without a flexible bonus.</summary>
    private const string ErrorClanBornRequiresBonus =
        "Clan-Born lineage requires selecting a flexible attribute bonus.";

    /// <summary>Error when the selected specialization is not valid for the archetype.</summary>
    private const string ErrorInvalidSpecialization =
        "Selected specialization is not available for the chosen archetype.";

    /// <summary>Error when attribute allocation is not complete.</summary>
    private const string ErrorInvalidAttributes =
        "Attribute allocation is not complete. All points must be spent.";

    /// <summary>Error when character creation is attempted with incomplete state.</summary>
    private const string ErrorIncompleteState =
        "Cannot confirm character: not all steps are complete.";

    /// <summary>Error when trying to go back from the first step.</summary>
    private const string ErrorAlreadyAtFirstStep = "Already at first step.";

    /// <summary>Error when persistence fails after character creation.</summary>
    private const string ErrorPersistenceFailed =
        "Character was created but could not be saved. Please try again.";

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterCreationController"/> class.
    /// </summary>
    /// <param name="lineageProvider">Provider for lineage definitions. Must not be null.</param>
    /// <param name="backgroundProvider">Provider for background definitions. Must not be null.</param>
    /// <param name="archetypeProvider">Provider for archetype definitions. Must not be null.</param>
    /// <param name="specializationProvider">Provider for specialization definitions. Must not be null.</param>
    /// <param name="viewModelBuilder">Builder for creating ViewModels. Must not be null.</param>
    /// <param name="nameValidator">Validator for character names. Must not be null.</param>
    /// <param name="logger">Logger for structured diagnostics. Must not be null.</param>
    /// <param name="characterFactory">
    /// Optional factory for creating Player entities. When null,
    /// <see cref="ConfirmCharacterAsync"/> returns a pending success result
    /// without creating a Player.
    /// </param>
    /// <param name="playerRepository">
    /// Optional repository for persisting Player entities. When null,
    /// <see cref="ConfirmCharacterAsync"/> skips persistence and logs a warning.
    /// Persistence is only attempted when both factory and repository are available.
    /// </param>
    public CharacterCreationController(
        ILineageProvider lineageProvider,
        IBackgroundProvider backgroundProvider,
        IArchetypeProvider archetypeProvider,
        ISpecializationProvider specializationProvider,
        IViewModelBuilder viewModelBuilder,
        INameValidator nameValidator,
        ILogger<CharacterCreationController> logger,
        ICharacterFactory? characterFactory = null,
        IPlayerRepository? playerRepository = null)
    {
        ArgumentNullException.ThrowIfNull(lineageProvider);
        ArgumentNullException.ThrowIfNull(backgroundProvider);
        ArgumentNullException.ThrowIfNull(archetypeProvider);
        ArgumentNullException.ThrowIfNull(specializationProvider);
        ArgumentNullException.ThrowIfNull(viewModelBuilder);
        ArgumentNullException.ThrowIfNull(nameValidator);
        ArgumentNullException.ThrowIfNull(logger);

        _lineageProvider = lineageProvider;
        _backgroundProvider = backgroundProvider;
        _archetypeProvider = archetypeProvider;
        _specializationProvider = specializationProvider;
        _viewModelBuilder = viewModelBuilder;
        _nameValidator = nameValidator;
        _logger = logger;
        _characterFactory = characterFactory;
        _playerRepository = playerRepository;

        _logger.LogDebug(
            "CharacterCreationController initialized. CharacterFactory available: {FactoryAvailable}, PlayerRepository available: {RepositoryAvailable}",
            _characterFactory != null, _playerRepository != null);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ICharacterCreationController IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsSessionActive => _state != null;

    /// <inheritdoc />
    public CharacterCreationState Initialize()
    {
        _state = CharacterCreationState.Create();

        _logger.LogInformation(
            "Character creation session initialized. SessionId: {SessionId}, CurrentStep: {CurrentStep}",
            _state.SessionId, _state.CurrentStep);

        return _state;
    }

    /// <inheritdoc />
    public Task<StepResult> SelectLineageAsync(Lineage lineage, CoreAttribute? flexibleBonus = null)
    {
        if (_state == null)
            return Task.FromResult(CreateNoSessionResult());

        _logger.LogDebug(
            "Selecting lineage: {Lineage}, FlexibleBonus: {FlexibleBonus}. SessionId: {SessionId}",
            lineage, flexibleBonus, _state.SessionId);

        // Validate Clan-Born requires flexible bonus
        if (lineage == Lineage.ClanBorn && !flexibleBonus.HasValue)
        {
            _logger.LogDebug(
                "Validation failed: Clan-Born lineage selected without flexible bonus. SessionId: {SessionId}",
                _state.SessionId);
            return Task.FromResult(CreateFailureResult(ErrorClanBornRequiresBonus));
        }

        // Update state
        _state.SelectedLineage = lineage;
        _state.FlexibleAttributeBonus = flexibleBonus;
        _state.CurrentStep = CharacterCreationStep.Background;
        _state.LastModifiedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Lineage selected: {Lineage}. Advancing to Background step. SessionId: {SessionId}",
            lineage, _state.SessionId);

        return Task.FromResult(CreateSuccessResult(CharacterCreationStep.Background));
    }

    /// <inheritdoc />
    public Task<StepResult> SelectBackgroundAsync(Background background)
    {
        if (_state == null)
            return Task.FromResult(CreateNoSessionResult());

        _logger.LogDebug(
            "Selecting background: {Background}. SessionId: {SessionId}",
            background, _state.SessionId);

        // Update state
        _state.SelectedBackground = background;
        _state.CurrentStep = CharacterCreationStep.Attributes;
        _state.LastModifiedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Background selected: {Background}. Advancing to Attributes step. SessionId: {SessionId}",
            background, _state.SessionId);

        return Task.FromResult(CreateSuccessResult(CharacterCreationStep.Attributes));
    }

    /// <inheritdoc />
    public Task<StepResult> ConfirmAttributesAsync(AttributeAllocationState attributes)
    {
        if (_state == null)
            return Task.FromResult(CreateNoSessionResult());

        _logger.LogDebug(
            "Confirming attribute allocation. IsComplete: {IsComplete}, PointsRemaining: {PointsRemaining}. SessionId: {SessionId}",
            attributes.IsComplete, attributes.PointsRemaining, _state.SessionId);

        // Validate attribute allocation is complete
        if (!attributes.IsComplete)
        {
            _logger.LogDebug(
                "Validation failed: Attribute allocation not complete. PointsRemaining: {PointsRemaining}. SessionId: {SessionId}",
                attributes.PointsRemaining, _state.SessionId);
            return Task.FromResult(CreateFailureResult(ErrorInvalidAttributes));
        }

        // Update state
        _state.Attributes = attributes;
        _state.CurrentStep = CharacterCreationStep.Archetype;
        _state.LastModifiedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Attributes confirmed. Advancing to Archetype step. SessionId: {SessionId}",
            _state.SessionId);

        return Task.FromResult(CreateSuccessResult(CharacterCreationStep.Archetype));
    }

    /// <inheritdoc />
    public Task<StepResult> SelectArchetypeAsync(Archetype archetype)
    {
        if (_state == null)
            return Task.FromResult(CreateNoSessionResult());

        _logger.LogDebug(
            "Selecting archetype: {Archetype} (PERMANENT). SessionId: {SessionId}",
            archetype, _state.SessionId);

        // Update state — clear specialization if archetype changes
        _state.SelectedArchetype = archetype;
        _state.SelectedSpecialization = null;
        _state.CurrentStep = CharacterCreationStep.Specialization;
        _state.LastModifiedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Archetype selected: {Archetype} (PERMANENT). Advancing to Specialization step. SessionId: {SessionId}",
            archetype, _state.SessionId);

        return Task.FromResult(CreateSuccessResult(CharacterCreationStep.Specialization));
    }

    /// <inheritdoc />
    public Task<StepResult> SelectSpecializationAsync(SpecializationId specialization)
    {
        if (_state == null)
            return Task.FromResult(CreateNoSessionResult());

        _logger.LogDebug(
            "Selecting specialization: {Specialization}. SessionId: {SessionId}",
            specialization, _state.SessionId);

        // Validate specialization is valid for the selected archetype
        if (_state.SelectedArchetype.HasValue)
        {
            var validSpecs = _specializationProvider
                .GetByArchetype(_state.SelectedArchetype.Value);

            if (!validSpecs.Any(s => s.SpecializationId == specialization))
            {
                _logger.LogDebug(
                    "Validation failed: Specialization {Specialization} not valid for archetype {Archetype}. SessionId: {SessionId}",
                    specialization, _state.SelectedArchetype.Value, _state.SessionId);
                return Task.FromResult(CreateFailureResult(ErrorInvalidSpecialization));
            }
        }

        // Update state
        _state.SelectedSpecialization = specialization;
        _state.CurrentStep = CharacterCreationStep.Summary;
        _state.LastModifiedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Specialization selected: {Specialization}. Advancing to Summary step. SessionId: {SessionId}",
            specialization, _state.SessionId);

        return Task.FromResult(CreateSuccessResult(CharacterCreationStep.Summary));
    }

    /// <inheritdoc />
    public async Task<CharacterCreationResult> ConfirmCharacterAsync(string name)
    {
        if (_state == null)
        {
            _logger.LogWarning("ConfirmCharacter called with no active session");
            return CharacterCreationResult.Failed(ErrorNoActiveSession);
        }

        _logger.LogDebug(
            "Confirming character with name: '{Name}'. SessionId: {SessionId}",
            name, _state.SessionId);

        // Validate name
        var nameResult = _nameValidator.Validate(name);
        if (!nameResult.IsValid)
        {
            _logger.LogDebug(
                "Name validation failed: {Error}. SessionId: {SessionId}",
                nameResult.ErrorMessage, _state.SessionId);
            return CharacterCreationResult.Failed(
                nameResult.ErrorMessage!,
                nameResult.ErrorMessage!);
        }

        _state.CharacterName = name;

        // Verify all steps complete
        if (!_state.IsComplete)
        {
            _logger.LogDebug(
                "Creation state is not complete. SessionId: {SessionId}",
                _state.SessionId);
            return CharacterCreationResult.Failed(ErrorIncompleteState);
        }

        // ─── Check name uniqueness via repository (if available) ────────────
        if (_playerRepository != null)
        {
            _logger.LogDebug(
                "Checking name uniqueness for '{Name}'. SessionId: {SessionId}",
                name, _state.SessionId);

            var nameExists = await _playerRepository.ExistsWithNameAsync(name);
            if (nameExists)
            {
                _logger.LogWarning(
                    "Name uniqueness check failed: '{Name}' already exists. SessionId: {SessionId}",
                    name, _state.SessionId);
                return CharacterCreationResult.Failed(
                    $"A character named '{name}' already exists.",
                    $"A character named '{name}' already exists.");
            }

            _logger.LogDebug(
                "Name '{Name}' is available. SessionId: {SessionId}",
                name, _state.SessionId);
        }

        // ─── Create character via factory (if available) ─────────────────────
        if (_characterFactory == null)
        {
            _logger.LogWarning(
                "Character factory not available — returning pending result. SessionId: {SessionId}",
                _state.SessionId);
            return CharacterCreationResult.Succeeded(
                null,
                $"Character '{name}' ready for creation (factory pending)");
        }

        var character = await _characterFactory.CreateCharacterAsync(_state);

        _logger.LogInformation(
            "Character created: {Name} ({Lineage} {Archetype}). SessionId: {SessionId}",
            name, _state.SelectedLineage, _state.SelectedArchetype, _state.SessionId);

        // ─── Persist character via repository (if available) ─────────────────
        if (_playerRepository != null)
        {
            _logger.LogDebug(
                "Persisting character '{Name}' (Id: {PlayerId}). SessionId: {SessionId}",
                name, character.Id, _state.SessionId);

            var saveResult = await _playerRepository.SaveAsync(character);

            if (!saveResult.Success)
            {
                _logger.LogError(
                    "Failed to persist character '{Name}' (Id: {PlayerId}): {Error}. SessionId: {SessionId}",
                    name, character.Id, saveResult.ErrorMessage, _state.SessionId);
                return CharacterCreationResult.Failed(
                    ErrorPersistenceFailed,
                    saveResult.ErrorMessage ?? ErrorPersistenceFailed);
            }

            _logger.LogInformation(
                "Character '{Name}' persisted successfully (Id: {PlayerId}). SessionId: {SessionId}",
                name, character.Id, _state.SessionId);
        }
        else
        {
            _logger.LogWarning(
                "Player repository not available — skipping persistence for '{Name}'. SessionId: {SessionId}",
                name, _state.SessionId);
        }

        // Clear state after successful creation (and optional persistence)
        _state = null;

        return CharacterCreationResult.Succeeded(
            character,
            $"{name} begins their saga.");
    }

    /// <inheritdoc />
    public StepResult GoBack()
    {
        if (_state == null)
            return CreateNoSessionResult();

        var previousStep = _state.CurrentStep.GetPreviousStep();
        if (previousStep == null)
        {
            _logger.LogDebug(
                "Cannot go back from first step ({Step}). SessionId: {SessionId}",
                _state.CurrentStep, _state.SessionId);
            return CreateFailureResult(ErrorAlreadyAtFirstStep);
        }

        _state.CurrentStep = previousStep.Value;
        _state.LastModifiedAt = DateTime.UtcNow;

        _logger.LogDebug(
            "Navigated back to step: {Step}. SessionId: {SessionId}",
            previousStep.Value, _state.SessionId);

        return CreateSuccessResult(previousStep.Value);
    }

    /// <inheritdoc />
    public void Cancel()
    {
        if (_state != null)
        {
            _logger.LogInformation(
                "Character creation cancelled. SessionId: {SessionId}",
                _state.SessionId);
            _state = null;
        }
    }

    /// <inheritdoc />
    public (CharacterCreationState State, CharacterCreationViewModel ViewModel) GetCurrentState()
    {
        if (_state == null)
            throw new InvalidOperationException(ErrorNoActiveSession);

        var viewModel = _viewModelBuilder.Build(_state);
        return (_state, viewModel);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful <see cref="StepResult"/> with the current ViewModel.
    /// </summary>
    /// <param name="currentStep">The step the workflow is now at.</param>
    /// <returns>A success <see cref="StepResult"/> with the rebuilt ViewModel.</returns>
    private StepResult CreateSuccessResult(CharacterCreationStep currentStep)
    {
        var viewModel = _viewModelBuilder.Build(_state!);
        return StepResult.Succeeded(
            currentStep,
            currentStep.GetNextStep(),
            viewModel);
    }

    /// <summary>
    /// Creates a failed <see cref="StepResult"/> with a single error message.
    /// </summary>
    /// <param name="error">The validation error message.</param>
    /// <returns>A failure <see cref="StepResult"/> with the current ViewModel.</returns>
    private StepResult CreateFailureResult(string error)
    {
        var viewModel = _state != null
            ? _viewModelBuilder.Build(_state)
            : CharacterCreationViewModel.Empty;

        return StepResult.Failed(
            _state?.CurrentStep ?? CharacterCreationStep.Lineage,
            viewModel,
            error);
    }

    /// <summary>
    /// Creates a failed <see cref="StepResult"/> from a list of errors.
    /// </summary>
    /// <param name="errors">The list of validation error messages.</param>
    /// <returns>A failure <see cref="StepResult"/> with the current ViewModel.</returns>
    private StepResult CreateFailureResult(IReadOnlyList<string> errors)
    {
        var viewModel = _state != null
            ? _viewModelBuilder.Build(_state)
            : CharacterCreationViewModel.Empty;

        return StepResult.Failed(
            _state?.CurrentStep ?? CharacterCreationStep.Lineage,
            viewModel,
            errors);
    }

    /// <summary>
    /// Creates a failed <see cref="StepResult"/> for operations with no active session.
    /// </summary>
    /// <returns>A failure <see cref="StepResult"/> with an empty ViewModel.</returns>
    private StepResult CreateNoSessionResult()
    {
        _logger.LogWarning("Operation attempted with no active session");
        return StepResult.Failed(
            CharacterCreationStep.Lineage,
            CharacterCreationViewModel.Empty,
            ErrorNoActiveSession);
    }
}
