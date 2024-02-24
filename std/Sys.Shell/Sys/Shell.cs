using System.Collections.Concurrent;

using Gnome.Diagnostics;
using Gnome.IO;

using static Gnome.ValueResult<Gnome.Diagnostics.PsOutput>;

namespace Gnome.Sys;

public static class Shell
{
    private static readonly ConcurrentDictionary<string, ShellExecutor> s_executors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["bash"] = new BashExecutor(),
            ["sh"] = new ShExecutor(),
            ["cmd"] = new CmdExecutor(),
            ["pwsh"] = new PwshExecutor(),
            ["powershell"] = new PowerShellExecutor(),
            ["node"] = new NodeExecutor(),
            ["deno"] = new DenoExecutor(),
            ["deno-js"] = new DenoJsExecutor(),
            ["ruby"] = new RubyExecutor(),
            ["python"] = new PythonExecutor(),
            ["dotnet-script"] = new DotNetScriptExecutor(),
            ["dotnet"] = new DotNetScriptExecutor(),
            ["fsharp"] = new FsiExecutor(),
            ["fsi"] = new FsiExecutor(),
        };

    public static void Register(string shell, ShellExecutor executor)
    {
        s_executors[shell] = executor;
    }

    public static void Register(string shell, Func<ShellExecutor> executor)
    {
        if (!s_executors.TryGetValue(shell, out var _))
            s_executors[shell] = executor();
    }

    public static PsOutput Exec(
        string shell,
        string script,
        PsStartInfo? info = null)
    {
        if (!s_executors.TryGetValue(shell, out var executor))
            throw new NotSupportedException($"Shell {shell} is not supported.");

        return executor.Exec(script, info);
    }

    public static ValueResult<PsOutput> ExecAsResult(
        string shell,
        string script,
        PsStartInfo? info = null)
    {
        if (!s_executors.TryGetValue(shell, out var executor))
            return new NotSupportedException($"Shell {shell} is not supported.");

        return executor.ExecAsResult(script, info);
    }

    public static ValueTask<PsOutput> ExecAsync(
        string shell,
        string script,
        PsStartInfo? info = null,
        CancellationToken cancellationToken = default)
    {
        if (!s_executors.TryGetValue(shell, out var executor))
            throw new NotSupportedException($"Shell {shell} is not supported.");

        return executor.ExecAsync(script, info, cancellationToken);
    }

    public static ValueTask<ValueResult<PsOutput>> ExecAsResultAsync(
        string shell,
        string script,
        PsStartInfo? info = null,
        CancellationToken cancellationToken = default)
    {
        if (!s_executors.TryGetValue(shell, out var executor))
            return Fail(new NotSupportedException($"Shell {shell} is not supported."));

        return executor.ExecAsResultAsync(script, info, cancellationToken);
    }

    public static PsOutput Run(
        string file,
        PsStartInfo? info = null)
    {
        var r = GetExecutorForFile(file, info);
        var executor = r.Expect();
        return executor.Run(file, info);
    }

    public static PsOutput Run(
        string shell,
        string file,
        PsStartInfo? info = null)
    {
        if (!s_executors.TryGetValue(shell, out var executor))
        {
            throw new NotSupportedException($"Shell {shell} is not supported.");
        }

        return executor.Run(file, info);
    }

    public static Result<PsOutput, Exception> RunAsResult(
        string shell,
        string file,
        PsStartInfo? info = null)
    {
        if (!s_executors.TryGetValue(shell, out var executor))
        {
            return new NotSupportedException($"Shell {shell} is not supported.");
        }

        return executor.Run(file, info);
    }

    public static Result<PsOutput> RunAsResult(
        string file,
        PsStartInfo? info = null)
    {
        var r = GetExecutorForFile(file, info);
        if (r.IsError)
            return r.Error;

        var executor = r.Value;
        return executor.Run(file, info);
    }

    public static ValueTask<PsOutput> RunAsync(
        string file,
        PsStartInfo? info = null,
        CancellationToken cancellationToken = default)
    {
        var r = GetExecutorForFile(file, info);
        var executor = r.Expect();
        return executor.RunAsync(file, info, cancellationToken);
    }

    public static ValueTask<PsOutput> RunAsync(
        string shell,
        string file,
        PsStartInfo? info = null,
        CancellationToken cancellationToken = default)
    {
        if (!s_executors.TryGetValue(shell, out var executor))
        {
            throw new NotSupportedException($"Shell {shell} is not supported.");
        }

        return executor.RunAsync(file, info, cancellationToken);
    }

    public static ValueTask<ValueResult<PsOutput>> RunAsResultAsync(
        string shell,
        string file,
        PsStartInfo? info = null,
        CancellationToken cancellationToken = default)
    {
        if (!s_executors.TryGetValue(shell, out var executor))
        {
            return Fail(new NotSupportedException($"Shell {shell} is not supported."));
        }

        return executor.RunAsResultAsync(file, info, cancellationToken);
    }

    public static ValueTask<ValueResult<PsOutput>> RunAsResultAsync(
        string file,
        PsStartInfo? info = null,
        CancellationToken cancellationToken = default)
    {
        var r = GetExecutorForFile(file, info);
        if (r.IsError)
            return Fail(r.Error);

        var executor = r.Value;
        return executor.RunAsResultAsync(file, info, cancellationToken);
    }

    private static Result<ShellExecutor> GetExecutorForFile(
        string file,
        PsStartInfo? info = null)
    {
        info ??= new PsStartInfo();
        using var stream = FileSystem.OpenFile(file);
        using var reader = new StreamReader(stream);
        var firstLine = reader.ReadLine();
        if (firstLine is null)
        {
            throw new InvalidOperationException($"File {file} is empty.");
        }

        if (firstLine.StartsWith("#!"))
        {
            firstLine = firstLine.TrimStart('#', '!');
            if (Platform.IsWindows())
            {
                var firstSpace = firstLine.IndexOf(' ');

                // this means no additional arguments
                if (firstSpace == -1)
                {
                    // ignore /usr/bin from /usr/bin/env
                    var fileName = Path.GetFileName(firstLine);
                    if (s_executors.TryGetValue(fileName, out var executor1))
                        return executor1;

                    var exe = ExePathFinder.Default.Find(fileName);
                    if (exe is not null)
                    {
                        info
                            .WithArgs(file)
                            .FileName = exe;
                    }
                    else
                    {
                        // see if its on the path
                        info.WithArgs(file)
                            .FileName = fileName;
                    }

                    return new GenericExecutor(info);
                }
                else
                {
                    var exe = firstLine.Substring(0, firstLine.IndexOf(' '));
                    var args = firstLine.Substring(firstSpace + 1);
                    var psArgs = PsArgs.From(args);
                    if (exe == "/usr/bin/env")
                    {
                        exe = psArgs[0];
                        psArgs.RemoveAt(0);
                    }

                    if (exe.Contains("/"))
                    {
                        exe = Path.GetFileName(exe);
                    }

                    if (psArgs.Count == 0 && s_executors.TryGetValue(exe, out var executor1))
                        return executor1;

                    var existingExe = ExePathFinder.Default.Find(exe);
                    if (existingExe is not null)
                    {
                        info
                            .WithArgs(file)
                            .FileName = existingExe;
                    }
                    else
                    {
                        // see if its on the path
                        info.WithArgs(file)
                            .FileName = exe;
                    }

                    return new GenericExecutor(info);
                }
            }
            else
            {
                var firstSpace = firstLine.IndexOf(' ');
                if (firstSpace == -1)
                {
                    info.WithArgs(file)
                        .FileName = firstLine;
                }
                else
                {
                    var exe = firstLine.Substring(0, firstLine.IndexOf(' '));
                    var args = firstLine.Substring(firstSpace + 1);
                    var psArgs = PsArgs.From(args);
                    psArgs.Add(file);
                    info
                        .WithArgs(psArgs)
                        .FileName = exe;
                }

                return new GenericExecutor(info);
            }
        }

        var extension = Path.GetExtension(file);
        foreach (var kvp in s_executors)
        {
            var executor = kvp.Value;
            if (executor.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                return executor;
            }
        }

        return new NotSupportedException($"File {file} is not supported.");
    }
}