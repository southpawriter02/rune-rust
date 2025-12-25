using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace RuneAndRust.Engine.Performance;

/// <summary>
/// Interface for a pooled StringBuilder service to reduce heap allocations
/// during high-frequency string operations like rendering (v0.3.18a).
/// </summary>
public interface ITextBufferPool
{
    /// <summary>
    /// Rents a StringBuilder from the pool. The builder is guaranteed to be empty.
    /// </summary>
    /// <returns>An empty StringBuilder instance.</returns>
    StringBuilder Rent();

    /// <summary>
    /// Returns a StringBuilder to the pool for reuse.
    /// The builder will be cleared before the next rental.
    /// </summary>
    /// <param name="sb">The StringBuilder to return.</param>
    void Return(StringBuilder sb);

    /// <summary>
    /// Convenience method that extracts the string from the builder and returns it to the pool.
    /// </summary>
    /// <param name="sb">The StringBuilder to extract from and return.</param>
    /// <returns>The string content of the builder.</returns>
    string GetStringAndReturn(StringBuilder sb);
}

/// <summary>
/// Object pool for StringBuilder instances to reduce GC pressure during rendering (v0.3.18a).
/// Uses Microsoft.Extensions.ObjectPool for thread-safe pooling with automatic reset.
/// </summary>
public class TextBufferPool : ITextBufferPool
{
    private readonly ObjectPool<StringBuilder> _pool;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBufferPool"/> class.
    /// Creates a StringBuilder pool with default sizing (initial capacity 100, max retained 4096).
    /// </summary>
    public TextBufferPool()
    {
        var provider = new DefaultObjectPoolProvider();
        _pool = provider.CreateStringBuilderPool();
    }

    /// <inheritdoc/>
    public StringBuilder Rent() => _pool.Get();

    /// <inheritdoc/>
    public void Return(StringBuilder sb) => _pool.Return(sb);

    /// <inheritdoc/>
    public string GetStringAndReturn(StringBuilder sb)
    {
        var result = sb.ToString();
        _pool.Return(sb);
        return result;
    }
}
