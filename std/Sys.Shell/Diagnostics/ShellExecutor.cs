using Gnome.IO;
using Gnome.Sys;

using Process = System.Diagnostics.Process;
using Ps = Gnome.Sys.Process;

namespace Gnome.Diagnostics;

public abstract class ShellExecutor
{
    public abstract string Shell { get; }

    public abstract string Extension { get; }

    public virtual PsOutput Exec(
        string script,
        PsStartInfo? info = null)
    {
        var scriptFile = this.GenerateScriptFile(script, this.Extension);
        try
        {
            return this.Run(scriptFile, info);
        }
        finally
        {
            if (File.Exists(scriptFile))
            {
                File.Delete(scriptFile);
            }
        }
    }

    public virtual ValueResult<PsOutput> ExecAsResult(
        string script,
        PsStartInfo? info = null)
    {
        var scriptFile = this.GenerateScriptFile(script, this.Extension);
        try
        {
            return this.Run(scriptFile, info);
        }
        catch (Exception ex)
        {
            return ex;
        }
        finally
        {
            if (File.Exists(scriptFile))
            {
                File.Delete(scriptFile);
            }
        }
    }

    public virtual async ValueTask<PsOutput> ExecAsync(
        string inlineScript,
        PsStartInfo? info = null,
        CancellationToken cancellationToken = default)
    {
        var scriptFile = this.GenerateScriptFile(inlineScript, this.Extension);
        try
        {
            return await this.RunAsync(scriptFile, info, cancellationToken)
                .NoCap();
        }
        finally
        {
            if (File.Exists(scriptFile))
            {
                File.Delete(scriptFile);
            }
        }
    }

    public virtual async ValueTask<ValueResult<PsOutput>> ExecAsResultAsync(
        string inlineScript,
        PsStartInfo? info = null,
        CancellationToken cancellationToken = default)
    {
        var scriptFile = this.GenerateScriptFile(inlineScript, this.Extension);
        try
        {
            return await this.RunAsResultAsync(scriptFile, info, cancellationToken)
                .NoCap();
        }
        catch (Exception ex)
        {
            return ex;
        }
        finally
        {
            if (File.Exists(scriptFile))
            {
                File.Delete(scriptFile);
            }
        }
    }

    public virtual PsOutput Run(
        string file,
        PsStartInfo? info = null)
    {
        using var child = this.Spawn(file, info);
        return child.WaitForOutput();
    }

    public virtual ValueResult<PsOutput> RunAsResult(
        string file,
        PsStartInfo? info = null)
    {
        using var child = this.Spawn(file, info);
        return child.WaitForResult();
    }

    public virtual async ValueTask<PsOutput> RunAsync(
        string file,
        PsStartInfo? info = null,
        CancellationToken cancellationToken = default)
    {
        using var child = this.Spawn(file, info);
        return await child.WaitForOutputAsync(cancellationToken).NoCap();
    }

    public virtual async ValueTask<ValueResult<PsOutput>> RunAsResultAsync(
        string file,
        PsStartInfo? info = null,
        CancellationToken cancellationToken = default)
    {
        using var child = this.Spawn(file, info);
        return await child.WaitForResultAsync(cancellationToken).NoCap();
    }

    protected virtual PsChild Spawn(string file, PsStartInfo? info)
    {
        var exe = ExePathFinder.Default.FindOrThrow(this.Shell);
        info ??= new PsStartInfo();
        info.FileName = exe;
        info.Args.Add(file);
        return Ps.Spawn(info);
    }

    protected virtual string GenerateScriptFile(string script, string extension)
    {
        var fileName = Path.GetRandomFileName();
        var temp = Path.Combine(Path.GetTempPath(), $"{fileName}{extension}");
        if (!Platform.IsWindows() && script.Contains("\r\n"))
        {
            script = script.Replace("\r\n", "\n");
        }

        File.WriteAllText(temp, script);
        if (!Platform.IsWindows())
        {
            FileSystem.ChangeMode(temp, UnixMode.GroupExecute | UnixMode.OtherExecute | UnixMode.UserExecute | UnixMode.UserRead | UnixMode.UserWrite);
        }

        return temp;
    }
}