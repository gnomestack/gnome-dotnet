using System.Collections;
using System.Text;

namespace Gnome.IO;

public sealed class LinesEnumerator : IEnumerable<string>, IAsyncEnumerable<string>, IDisposable, IAsyncDisposable
{
    private readonly TextReader reader;

    public LinesEnumerator(byte[] data, Encoding? encoding = null)
        : this(new MemoryStream(data), encoding)
    {
    }

    public LinesEnumerator(Stream stream, Encoding? encoding = null, int bufferSize = -1,  bool leaveOpen = false)
    {
        this.reader = new StreamReader(stream, encoding, false, bufferSize, leaveOpen);
    }

    public LinesEnumerator(TextReader reader)
    {
        this.reader = reader;
    }

    public IEnumerator<string> GetEnumerator()
    {
        string? line = null;
        while ((line = this.reader.ReadLine()) != null)
            yield return line;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => this.GetEnumerator();

    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
#if NETLEGACY
        string? line = null;
        while ((line = await this.reader.ReadLineAsync().NoCap()) != null)
        {
            yield return line;
        }
#else
        string? line = null;
        while ((line = await this.reader.ReadLineAsync(cancellationToken).NoCap()) != null)
        {
            yield return line;
        }
#endif
    }

    public void Dispose()
    {
        this.reader.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (this.reader is IAsyncDisposable readerAsyncDisposable)
        {
            await readerAsyncDisposable.DisposeAsync();
        }
        else
        {
            this.reader.Dispose();
        }
    }
}