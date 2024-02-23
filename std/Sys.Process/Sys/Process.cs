using System.Runtime.CompilerServices;
using System.Text;

using Gnome.Diagnostics;
using Gnome.IO;
using Gnome.Text;

using Process2 = System.Diagnostics.Process;

namespace Gnome.Sys;

public static class Process
{
    private static readonly Lazy<string[]> s_argv = new(() =>
    {
        var argv = new string[Environment.GetCommandLineArgs().Length - 1];
        var args = Environment.GetCommandLineArgs();
        Array.Copy(args, 1, argv, 0, args.Length - 1);
        return argv;
    });

    public static int ProcessorCount
        => Environment.ProcessorCount;

    public static int ExitCode
    {
        get => Environment.ExitCode;
        set => Environment.ExitCode = value;
    }

    public static string CommandLine
        => Environment.CommandLine;

    public static string[] Argv => s_argv.Value;

    public static string CurrentDirectory
    {
        get => Environment.CurrentDirectory;
        set => Environment.CurrentDirectory = value;
    }

#if !NETLEGACY
    public static bool IsPrivileged
        => Environment.IsPrivilegedProcess;
#endif

    public static int Id
    {
        get
        {
#if NETLEGACY
            return Process2.GetCurrentProcess().Id;
#else
            return Environment.ProcessId;
#endif
        }
    }

    public static Process2 GetCurrentProcess()
        => Process2.GetCurrentProcess();

    public static IEnumerable<string> Lines(PsCommand command)
        => Lines(command.BuildStartInfo());

    public static IEnumerable<string> Lines(string fileName, PsArgs? args = null)
    {
        var si = new PsStartInfo(fileName);
        if (args is not null)
            si.WithArgs(args);

        return Lines(si);
    }

    public static IEnumerable<string> Lines(PsStartInfo startInfo)
    {
        startInfo.WithStdOut(Stdio.Piped);
        using var child = new PsChild(startInfo);
        using var lines = new LinesEnumerator(child.Stdout);
        foreach (var line in lines)
        {
            yield return line;
        }
    }

    public static async IAsyncEnumerable<string> LinesAsync(
            PsCommand command,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var child = new PsChild(command.BuildStartInfo());
        await using var lines = new LinesEnumerator(child.Stdout);
        await foreach (var line in lines)
        {
            yield return line;
        }
    }

    public static async IAsyncEnumerable<string> LinesAsync(
        string fileName,
        PsArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var si = new PsStartInfo(fileName);
        if (args is not null)
            si.WithArgs(args);

        using var child = new PsChild(si);
        await using var lines = new LinesEnumerator(child.Stdout);
        await foreach (var line in lines)
        {
            yield return line;
        }
    }

    public static async IAsyncEnumerable<string> LinesAsync(PsStartInfo startInfo, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        startInfo.WithStdOut(Stdio.Piped);
        using var child = new PsChild(startInfo);
        await using var lines = new LinesEnumerator(child.Stdout);
        await foreach (var line in lines)
        {
            yield return line;
        }
    }

    public static PsOutput Quiet(PsCommand command)
        => Quiet(command.BuildStartInfo());

    public static PsOutput Quiet(string fileName, PsArgs? args = null)
    {
        var si = new PsStartInfo(fileName);
        if (args is not null)
            si.WithArgs(args);

        return Quiet(si);
    }

    public static PsOutput Quiet(PsStartInfo startInfo)
    {
        startInfo.WithStdOut(Stdio.Piped)
            .WithStdErr(Stdio.Piped);

        using var child = new PsChild(startInfo);
        var output = child.WaitForOutput();
        return output;
    }

    public static ValueTask<PsOutput> QuietAsync(
        string fileName,
        PsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        var si = new PsStartInfo(fileName);
        if (args is not null)
            si.WithArgs(args);

        return QuietAsync(si, cancellationToken);
    }

    public static ValueTask<PsOutput> QuietAsync(PsCommand command, CancellationToken cancellationToken = default)
        => QuietAsync(command.BuildStartInfo(), cancellationToken);

    public static async ValueTask<PsOutput> QuietAsync(PsStartInfo startInfo, CancellationToken cancellationToken = default)
    {
        startInfo.WithStdOut(Stdio.Piped)
            .WithStdErr(Stdio.Piped);

        using var child = new PsChild(startInfo);
        var output = await child.WaitForOutputAsync(cancellationToken).NoCap();
        return output;
    }

    public static PsOutput Run(PsCommand command)
        => Run(command.BuildStartInfo());

    public static PsOutput Run(PsStartInfo startInfo)
    {
        using var child = new PsChild(startInfo);
        var output = child.WaitForOutput();
        return output;
    }

    public static PsOutput Run(string fileName, PsArgs? args = null)
    {
        var si = new PsStartInfo(fileName);
        if (args is not null)
            si.WithArgs(args);

        return Run(si);
    }

    public static ValueTask<PsOutput> RunAsync(
        string fileName,
        PsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        var si = new PsStartInfo(fileName);
        if (args is not null)
            si.WithArgs(args);

        return RunAsync(si, cancellationToken);
    }

    public static ValueTask<PsOutput> RunAsync(PsCommand command, CancellationToken cancellationToken = default)
        => RunAsync(command.BuildStartInfo(), cancellationToken);

    public static async ValueTask<PsOutput> RunAsync(PsStartInfo startInfo, CancellationToken cancellationToken = default)
    {
        using var child = new PsChild(startInfo);
        var output = await child.WaitForOutputAsync(cancellationToken).NoCap();
        return output;
    }

    public static PsChild Spawn(PsStartInfo startInfo)
        => new(startInfo);

    public static PsChild Spawn(PsCommand command)
        => new(command.BuildStartInfo());

#pragma warning disable S4144
    public static PsChild Spawn(string fileName, PsArgs? args = null)
    {
        var si = new PsStartInfo(fileName);
        if (args is not null)
            si.WithArgs(args);

        return new(si);
    }

    public static PsPipe Chain(string fileName, PsArgs? args = null)
    {
        var si = new PsStartInfo(fileName);
        if (args is not null)
            si.WithArgs(args);

        return new(si);
    }

    public static PsPipe Chain(PsStartInfo si)
        => new(si);

    public static PsPipe Chain(PsChild child)
        => new(child);

    public static PsPipe Chain(PsCommand command)
        => new(command);

    public static PsPipeAsync ChainAsync(PsChild child)
        => new(child);

    public static PsPipeAsync ChainAsync(PsStartInfo si)
        => new(si);

    public static PsPipeAsync ChainAsync(PsCommand command)
        => new(command.BuildStartInfo());

    public static string Text(PsCommand command)
        => Text(command.BuildStartInfo());

    public static string Text(string fileName, PsArgs? args = null)
    {
        var si = new PsStartInfo(fileName);
        if (args is not null)
            si.WithArgs(args);

        return Text(si);
    }

    public static string Text(PsStartInfo startInfo)
    {
        startInfo.WithStdOut(Stdio.Piped);
        using var child = new PsChild(startInfo);
        var sb = new StringBuilder();
        child.PipeTo(new StringWriter(sb));
        var output = sb.ToString();
        sb.Clear();
        return output;
    }

    public static Task<string> TextAsync(
        string fileName,
        PsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        var si = new PsStartInfo(fileName);
        if (args is not null)
            si.WithArgs(args);

        return TextAsync(si, cancellationToken);
    }

    public static Task<string> TextAsync(PsCommand command, CancellationToken cancellationToken = default)
        => TextAsync(command.BuildStartInfo(), cancellationToken);

    public static async Task<string> TextAsync(PsStartInfo startInfo, CancellationToken cancellationToken = default)
    {
        startInfo.WithStdOut(Stdio.Piped);
        using var child = new PsChild(startInfo);
        var sb = new StringBuilder();
        await child.PipeToAsync(new StringWriter(sb), -1, cancellationToken).NoCap();
        var output = sb.ToString();
        sb.Clear();
        return output;
    }

    public static string? Which(string command)
        => ExePathFinder.Which(command);
}