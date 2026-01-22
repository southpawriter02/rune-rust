using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Manages RNG context stack and seed locking for context-aware randomness.
/// </summary>
/// <remarks>
/// <para>
/// This service coordinates between game systems and the IRandomProvider,
/// ensuring appropriate seeding behavior for different gameplay scenarios.
/// </para>
/// <para>
/// Context stack enables nested contexts (e.g., a dialogue check during combat)
/// while maintaining proper seed behavior on exit.
/// </para>
/// <para>
/// Thread Safety: This class is NOT thread-safe. It is designed for
/// single-threaded game logic.
/// </para>
/// </remarks>
public class RngContextService : IRngContextService
{
    private readonly IRandomProvider _randomProvider;
    private readonly ILogger<RngContextService> _logger;
    private readonly Stack<RngContext> _contextStack;
    private readonly Dictionary<RngContext, int> _lockedSeeds;

    /// <summary>
    /// Creates a new RngContextService.
    /// </summary>
    /// <param name="randomProvider">The random provider to manage.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="randomProvider"/> or <paramref name="logger"/> is null.
    /// </exception>
    public RngContextService(
        IRandomProvider randomProvider,
        ILogger<RngContextService> logger)
    {
        _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contextStack = new Stack<RngContext>();
        _lockedSeeds = new Dictionary<RngContext, int>();

        _logger.LogInformation(
            "RngContextService initialized with IRandomProvider (current seed: {Seed})",
            _randomProvider.GetCurrentSeed());
    }

    /// <inheritdoc />
    public void EnterContext(RngContext context)
    {
        _logger.LogDebug(
            "Entering RNG context: {Context} (stack depth: {Depth})",
            context, _contextStack.Count + 1);

        _contextStack.Push(context);

        // Apply locked seed if available for this context
        if (_lockedSeeds.TryGetValue(context, out var lockedSeed))
        {
            _logger.LogDebug(
                "Applying locked seed {Seed} for context {Context}",
                lockedSeed, context);
            _randomProvider.SetSeed(lockedSeed);
        }
        else if (context is RngContext.Exploration or RngContext.Dialogue or RngContext.Default)
        {
            // Fresh seed for non-locked contexts that support it
            var freshSeed = GenerateFreshSeed();
            _logger.LogDebug(
                "Generating fresh seed {Seed} for context {Context}",
                freshSeed, context);
            _randomProvider.SetSeed(freshSeed);
        }
        else
        {
            // Combat and Crafting without locked seed: keep current RNG state
            _logger.LogDebug(
                "Context {Context} has no locked seed; keeping current RNG state (seed: {Seed})",
                context, _randomProvider.GetCurrentSeed());
        }
    }

    /// <inheritdoc />
    public void ExitContext()
    {
        if (_contextStack.Count == 0)
        {
            _logger.LogWarning("ExitContext called with empty context stack");
            return;
        }

        var exitedContext = _contextStack.Pop();
        _logger.LogDebug(
            "Exited RNG context: {Context} (stack depth now: {Depth})",
            exitedContext, _contextStack.Count);

        // If there's a previous context, restore its seed behavior
        if (_contextStack.Count > 0)
        {
            var previousContext = _contextStack.Peek();

            if (_lockedSeeds.TryGetValue(previousContext, out var previousSeed))
            {
                _logger.LogDebug(
                    "Restoring locked seed {Seed} for previous context {Context}",
                    previousSeed, previousContext);
                _randomProvider.SetSeed(previousSeed);
            }
            else
            {
                _logger.LogDebug(
                    "Previous context {Context} has no locked seed; RNG state unchanged",
                    previousContext);
            }
        }
        else
        {
            _logger.LogDebug("Context stack now empty; using Default context behavior");
        }
    }

    /// <inheritdoc />
    public RngContext GetCurrentContext()
    {
        return _contextStack.Count > 0 ? _contextStack.Peek() : RngContext.Default;
    }

    /// <inheritdoc />
    public void LockSeedForContext(RngContext context, int seed)
    {
        var wasLocked = _lockedSeeds.ContainsKey(context);
        _lockedSeeds[context] = seed;

        _logger.LogDebug(
            "{Action} seed {Seed} for context {Context}",
            wasLocked ? "Updated locked" : "Locked", seed, context);

        // If this context is currently active, apply the seed immediately
        if (_contextStack.Count > 0 && _contextStack.Peek() == context)
        {
            _logger.LogDebug(
                "Context {Context} is currently active; applying seed {Seed} immediately",
                context, seed);
            _randomProvider.SetSeed(seed);
        }
    }

    /// <inheritdoc />
    public void ReleaseSeedLock(RngContext context)
    {
        if (_lockedSeeds.Remove(context))
        {
            _logger.LogDebug(
                "Released seed lock for context {Context}",
                context);
        }
        else
        {
            _logger.LogDebug(
                "No seed lock to release for context {Context}",
                context);
        }
    }

    /// <inheritdoc />
    public bool IsContextSeedLocked(RngContext context)
    {
        return _lockedSeeds.ContainsKey(context);
    }

    /// <inheritdoc />
    public int? GetLockedSeed(RngContext context)
    {
        return _lockedSeeds.TryGetValue(context, out var seed) ? seed : null;
    }

    /// <summary>
    /// Generates a fresh seed from current time.
    /// </summary>
    /// <returns>A seed based on <see cref="DateTime.UtcNow"/> ticks.</returns>
    private static int GenerateFreshSeed()
    {
        return (int)(DateTime.UtcNow.Ticks % int.MaxValue);
    }
}
