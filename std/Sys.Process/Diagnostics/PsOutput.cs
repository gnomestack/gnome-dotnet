using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

using Gnome.IO;

namespace Gnome.Diagnostics;

public readonly struct PsOutput
{
    public PsOutput()
    {
        this.ExitCode = 0;
        this.FileName = string.Empty;
        this.Stdout = Array.Empty<byte>();
        this.Stderr = Array.Empty<byte>();
        this.StartTime = DateTime.MinValue;
        this.ExitTime = DateTime.MinValue;
    }

    public PsOutput(
        string fileName,
        int exitCode,
        byte[]? stdout = null,
        byte[]? stderr = null,
        DateTime? startTime = null,
        DateTime? exitTime = null)
    {
        this.FileName = fileName;
        this.ExitCode = exitCode;
        this.Stdout = stdout ?? Array.Empty<byte>();
        this.Stderr = stderr ?? Array.Empty<byte>();
        this.StartTime = startTime ?? DateTime.MinValue;
        this.ExitTime = exitTime ?? DateTime.MinValue;
    }

    public static PsOutput Empty { get; } = new();

    public int ExitCode { get; }

    public string FileName { get; }

    public byte[] Stdout { get; }

    public byte[] Stderr { get; }

    public DateTime StartTime { get; }

    public DateTime ExitTime { get; }

    public void Deconstruct(
        out int exitCode,
        out byte[] stdout,
        out byte[] stderr)
    {
        exitCode = this.ExitCode;
        stdout = this.Stdout;
        stderr = this.Stderr;
    }

    public void Deconstruct(
        out string fileName,
        out int exitCode,
        out byte[] stdout,
        out byte[] stderr,
        out DateTime startTime,
        out DateTime exitTime)
    {
        fileName = this.FileName;
        exitCode = this.ExitCode;
        stdout = this.Stdout;
        stderr = this.Stderr;
        startTime = this.StartTime;
        exitTime = this.ExitTime;
    }

    public string Text(Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return encoding.GetString(this.Stdout);
    }

    public IEnumerable<string> Lines(Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        using var lines = new LinesEnumerator(this.Stdout, encoding);
        foreach (var line in lines)
            yield return line;
    }

    [SuppressMessage("Roslynator", "RCS1163:Unused parameter.")]
    public async IAsyncEnumerable<string> LinesAsync(
        Encoding? encoding,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        encoding ??= Encoding.UTF8;
        await using var lines = new LinesEnumerator(this.Stdout, encoding);
        await foreach (var line in lines)
            yield return line;
    }

    public void ThrowOnInvalidExitCode(Func<int, bool>? validate = null)
    {
        if (validate is null)
        {
            if (this.ExitCode != 0)
            {
                throw new ProcessException(this.ExitCode, this.FileName);
            }

            return;
        }

        if (!validate(this.ExitCode))
        {
            throw new ProcessException(this.ExitCode, this.FileName);
        }
    }

/*
    public Result<PsOutput, ProcessException> ToResult(Func<int, bool>? validate = null)
    {
        if (validate is null)
        {
            if (this.ExitCode != 0)
                return Result.Err<PsOutput, ProcessException>(new ProcessException(this.ExitCode, this.FileName));

            return Result.Ok<PsOutput, ProcessException>(this);
        }

        if (validate(this.ExitCode))
            return Result.Ok<PsOutput, ProcessException>(this);

        return Result.Err<PsOutput, ProcessException>(new ProcessException(this.ExitCode, this.FileName));
    }
    */
}