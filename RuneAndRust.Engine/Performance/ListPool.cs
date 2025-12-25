using Microsoft.Extensions.ObjectPool;

namespace RuneAndRust.Engine.Performance;

/// <summary>
/// Interface for a pooled List&lt;T&gt; service to reduce heap allocations
/// during high-frequency collection operations like rendering (v0.3.18a).
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public interface IListPool<T>
{
    /// <summary>
    /// Rents a List from the pool. The list is guaranteed to be empty.
    /// </summary>
    /// <returns>An empty List instance.</returns>
    List<T> Rent();

    /// <summary>
    /// Returns a List to the pool for reuse.
    /// The list will be cleared before the next rental.
    /// </summary>
    /// <param name="list">The List to return.</param>
    void Return(List<T> list);
}

/// <summary>
/// Object pool for List&lt;T&gt; instances to reduce GC pressure during rendering (v0.3.18a).
/// Uses Microsoft.Extensions.ObjectPool for thread-safe pooling with automatic reset.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public class ListPool<T> : IListPool<T>
{
    private readonly ObjectPool<List<T>> _pool;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListPool{T}"/> class.
    /// Creates a List pool with default sizing (max 16 retained instances per processor).
    /// </summary>
    public ListPool()
    {
        var provider = new DefaultObjectPoolProvider();
        _pool = provider.Create(new ListPooledObjectPolicy<T>());
    }

    /// <inheritdoc/>
    public List<T> Rent() => _pool.Get();

    /// <inheritdoc/>
    public void Return(List<T> list) => _pool.Return(list);
}

/// <summary>
/// Policy for pooling List&lt;T&gt; instances.
/// Clears the list on return to ensure a clean state for the next rental.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
internal class ListPooledObjectPolicy<T> : PooledObjectPolicy<List<T>>
{
    /// <inheritdoc/>
    public override List<T> Create() => new();

    /// <inheritdoc/>
    public override bool Return(List<T> obj)
    {
        obj.Clear();
        return true;
    }
}
