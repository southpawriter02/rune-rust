namespace RuneAndRust.Infrastructure.Services.Audio;

/// <summary>
/// Represents a cached audio file for playback.
/// </summary>
/// <remarks>
/// Holds the loaded audio data and path for cache management.
/// Implements <see cref="IDisposable"/> for resource cleanup.
/// </remarks>
public class CachedAudio : IDisposable
{
    /// <summary>
    /// Gets the file path of the cached audio.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long FileSize { get; }

    /// <summary>
    /// Gets whether the audio has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Creates a new cached audio instance.
    /// </summary>
    /// <param name="path">Path to the audio file.</param>
    public CachedAudio(string path)
    {
        Path = path;

        // Get file size if file exists
        if (File.Exists(path))
        {
            var fileInfo = new FileInfo(path);
            FileSize = fileInfo.Length;
        }
    }

    /// <summary>
    /// Releases audio resources.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;

        // Future: Release NAudio or other audio library resources
        IsDisposed = true;
    }
}
