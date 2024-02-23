using System.Runtime.CompilerServices;
using System.Text;

namespace Gnome.IO;

public static partial class FileSystem
{
#if NET7_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken = default)
        => File.ReadAllBytesAsync(path, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<string> ReadFileLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
        => File.ReadLinesAsync(path, encoding, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<string> ReadFileLinesAsync(string path, CancellationToken cancellationToken = default)
        => File.ReadLinesAsync(path, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<string[]> ReadFileLineArrayAsync(string path, CancellationToken cancellationToken = default)
        => File.ReadAllLinesAsync(path, cancellationToken);
#endif
}