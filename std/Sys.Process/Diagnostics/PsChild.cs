using System.Diagnostics;
using System.Text;

using Gnome.IO.Extensions;
using Gnome.Sys;
using Gnome.Util.Strings;

using Proc = System.Diagnostics.Process;

namespace Gnome.Diagnostics;

public sealed class PsChild : IDisposable
{
    private readonly Proc process;

    private readonly int processId;

    private readonly List<IDisposable> disposables = new();

    private bool disposed;

    private DateTime exitTime;

    private byte[] stdout = Array.Empty<byte>();

    private byte[] stderr = Array.Empty<byte>();

    internal PsChild(PsStartInfo startInfo)
    {
        this.process = new Proc();

        if (startInfo.Verb is not null)
        {
            if (startInfo.Verb == "sudo")
            {
                startInfo.Args.Insert(0, startInfo.FileName);
                startInfo.FileName = "sudo";
            }

            this.process.StartInfo.Verb = startInfo.Verb;
        }

        var si = this.process.StartInfo;
        si.FileName = startInfo.FileName;
        si.CreateNoWindow = startInfo.CreateNoWindow;
        si.UseShellExecute = startInfo.UseShellExecute;
        si.RedirectStandardInput = startInfo.Stdin == Stdio.Piped;

        if (Platform.IsWindows())
            si.LoadUserProfile = startInfo.LoadUserProfile;

        if (Platform.IsWindows() && !startInfo.User.IsNullOrWhiteSpace())
        {
            si.UserName = startInfo.User;

            if (startInfo.Password is not null)
            {
                si.Password = startInfo.Password;
            }
            else if (!startInfo.PasswordInClearText.IsNullOrWhiteSpace())
            {
                si.PasswordInClearText = startInfo.PasswordInClearText;
            }

            if (!startInfo.Domain.IsNullOrWhiteSpace())
            {
                si.Domain = startInfo.Domain;
            }
        }

#if NET5_0_OR_GREATER
        foreach (var arg in startInfo.Args)
        {
            si.ArgumentList.Add(arg);
        }
#else
        si.Arguments = startInfo.Args.ToString();
#endif

        if (!startInfo.Cwd.IsNullOrWhiteSpace())
            si.WorkingDirectory = startInfo.Cwd;

        if (startInfo.Env is not null)
        {
            foreach (var kvp in startInfo.Env)
            {
                si.Environment[kvp.Key] = kvp.Value;
            }
        }

        si.RedirectStandardOutput = startInfo.Stdout != Stdio.Inherit;
        si.RedirectStandardError = startInfo.Stderr != Stdio.Inherit;
        si.RedirectStandardInput = startInfo.Stdin != Stdio.Inherit;
        if (si.RedirectStandardError || si.RedirectStandardOutput)
        {
            si.CreateNoWindow = true;
            si.UseShellExecute = false;
        }

        this.process.Start();
        var now = DateTime.Now;
        try
        {
            this.StartTime = this.process.StartTime;
        }
        catch (Exception e)
        {
            this.StartTime = now;
            Debug.WriteLine(e);
        }

        this.processId = this.process.Id;
    }

    ~PsChild()
    {
        this.Dispose();
    }

    public int Id => this.processId;

    public DateTime StartTime { get; }

    public DateTime ExitTime => this.exitTime;

    public Stream Stderr => this.process.StandardError.BaseStream;

    public StreamReader StderrReader => this.process.StandardError;

    public Stream Stdin => this.process.StandardInput.BaseStream;

    public StreamWriter StdinWriter => this.process.StandardInput;

    public Stream Stdout => this.process.StandardOutput.BaseStream;

    public StreamReader StdoutReader => this.process.StandardOutput;

    public bool IsStdoutRedirected => this.process.StartInfo.RedirectStandardOutput;

    public bool IsStdoutPiped { get; private set; }

    public bool IsStderrRedirected => this.process.StartInfo.RedirectStandardError;

    public bool IsStderrPiped { get; private set; }

    public bool IsStdinRedirected => this.process.StartInfo.RedirectStandardInput;

    public void PipeTo(Stream stream)
    {
        this.GuardPiped();
        this.process.StandardOutput.BaseStream.CopyTo(stream);
    }

    public void PipeTo(TextWriter writer)
    {
        this.GuardPiped();
        this.process.StandardOutput.PipeTo(writer);
    }

    public void PipeTo(ICollection<string> lines)
    {
        this.GuardPiped();
        this.process.StandardOutput.PipeTo(lines);
    }

    public void PipeTo(FileInfo file)
    {
        this.GuardPiped();
        this.process.StandardOutput.PipeTo(file);
    }

    public void PipeTo(PsChild child)
    {
        this.GuardPiped();

        if (!child.IsStdinRedirected)
            throw new InvalidOperationException("Cannot pipe to child proccess' input when child process input is not redirected.");

        this.process.StandardOutput.BaseStream.CopyTo(child.process.StandardInput.BaseStream);
        child.process.StandardInput.BaseStream.Close();
    }

    public Task PipeToAsync(Stream stream, int bufferSize = -1, CancellationToken cancellationToken = default)
    {
        this.GuardPiped();

        if (bufferSize < 1)
            bufferSize = 4096;

        return this.process.StandardOutput.BaseStream.CopyToAsync(stream, bufferSize, cancellationToken);
    }

    public Task PipeToAsync(TextWriter writer, int bufferSize = -1, CancellationToken cancellationToken = default)
    {
        this.GuardPiped();
        return this.process.StandardOutput.PipeToAsync(writer, bufferSize, cancellationToken);
    }

    public Task PipeToAsync(ICollection<string> lines, CancellationToken cancellationToken = default)
    {
        this.GuardPiped();
        return this.process.StandardOutput.PipeToAsync(lines, cancellationToken: cancellationToken);
    }

    public Task PipeToAsync(FileInfo file, Encoding? encoding, int bufferSize = -1, CancellationToken cancellationToken = default)
    {
        this.GuardPiped();
        return this.process.StandardOutput.PipeToAsync(file,  encoding, bufferSize, cancellationToken);
    }

    public async Task PipeToAsync(PsChild child, CancellationToken cancellationToken = default)
    {
        this.GuardPiped();

        if (!child.IsStdinRedirected)
            throw new InvalidOperationException("Cannot pipe to child's input when child input is not redirected.");

#if NETLEGACY
        await this.process.StandardOutput.BaseStream.CopyToAsync(child.process.StandardInput.BaseStream)
            .ConfigureAwait(false);
        child.process.StandardInput.BaseStream.Close();
#else
        await this.process.StandardOutput.BaseStream.CopyToAsync(child.process.StandardInput.BaseStream, cancellationToken)
            .ConfigureAwait(false);
        child.process.StandardInput.BaseStream.Close();
#endif
    }

    public void PipeErrorTo(Stream stream)
    {
        if (this.IsStderrPiped)
            throw new InvalidOperationException("Cannot pipe to stderr when stderr is captured.");

        if (!this.IsStderrRedirected)
            throw new InvalidOperationException("Cannot pipe to stderr when stream is not redirected.");

        this.process.StandardError.BaseStream.CopyTo(stream);
    }

    public void PipeErrorTo(TextWriter writer)
    {
        if (this.IsStderrPiped)
            throw new InvalidOperationException("Cannot pipe to stderr when stderr is captured.");

        if (!this.IsStderrRedirected)
            throw new InvalidOperationException("Cannot pipe to stderr when stream is not redirected.");

        this.process.StandardError.PipeTo(writer);
    }

    public void PipeErrorTo(ICollection<string> lines)
    {
        if (this.IsStderrPiped)
            throw new InvalidOperationException("Cannot pipe to stderr when stderr is captured.");

        if (!this.IsStderrRedirected)
            throw new InvalidOperationException("Cannot pipe to stderr when stream is not redirected.");

        this.process.StandardError.PipeTo(lines);
    }

    public void PipeErrorTo(FileInfo file)
    {
        if (this.IsStderrPiped)
            throw new InvalidOperationException("Cannot pipe to stderr when stderr is captured.");

        if (!this.IsStderrRedirected)
            throw new InvalidOperationException("Cannot pipe to stderr when stream is not redirected.");

        this.process.StandardError.PipeTo(file);
    }

    public void PipeFrom(ICollection<string> lines)
    {
        if (!this.IsStdinRedirected)
            throw new InvalidOperationException("Cannot pipe stdin from stream when input is not redirected.");

        this.process.StandardInput.Write(lines);
    }

    public void PipeFrom(FileInfo file)
    {
        if (!this.IsStdinRedirected)
            throw new InvalidOperationException("Cannot pipe stdin from stream when input is not redirected.");

        this.process.StandardInput.Write(file);
    }

    public void PipeFrom(Stream stream)
    {
        if (!this.IsStdinRedirected)
            throw new InvalidOperationException("Cannot pipe stdin from stream when input is not redirected.");

        this.process.StandardInput.Write(stream);
    }

    public void PipeFrom(TextReader reader)
    {
        if (!this.IsStdinRedirected)
            throw new InvalidOperationException("Cannot pipe stdin from stream when input is not redirected.");

        this.process.StandardInput.Write(reader);
    }

    public PsChild AddDisposable(IDisposable disposable)
    {
        this.disposables.Add(disposable);
        return this;
    }

    public int Wait()
    {
        if (this.process.HasExited)
        {
            this.exitTime = this.process.ExitTime;
            return this.process.ExitCode;
        }

        this.process.WaitForExit();
        this.exitTime = this.process.ExitTime;
        return this.process.ExitCode;
    }

    public ValueResult<PsOutput> WaitForResult()
    {
        try
        {
            return this.WaitForOutput();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public PsOutput WaitForOutput()
    {
        if (this.IsStdoutRedirected && !this.IsStdoutPiped)
        {
            using var ms = new MemoryStream();
            this.process.StandardOutput.PipeTo(ms);
            ms.Flush();
            this.stdout = ms.ToArray();
        }

        if (this.IsStderrRedirected && !this.IsStderrPiped)
        {
            using var ms = new MemoryStream();
            this.process.StandardError.PipeTo(ms);
            ms.Flush();
            this.stderr = ms.ToArray();
        }

        if (this.process.HasExited)
        {
            this.exitTime = this.process.ExitTime;
        }
        else
        {
            this.process.WaitForExit();
            this.exitTime = this.process.ExitTime;
        }

        return new PsOutput(
            this.process.StartInfo.FileName,
            this.process.ExitCode,
            this.stdout,
            this.stderr,
            this.StartTime,
            this.exitTime);
    }

    public async Task<int> WaitAsync(CancellationToken cancellationToken)
    {
        if (this.process.HasExited)
        {
            this.exitTime = this.process.ExitTime;
            return this.process.ExitCode;
        }

        await this.process.WaitForExitAsync(cancellationToken)
            .ConfigureAwait(false);
        return this.process.ExitCode;
    }

    public async ValueTask<ValueResult<PsOutput>> WaitForResultAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var psoutput = await this.WaitForOutputAsync(cancellationToken)
                .NoCap();

            return psoutput;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async ValueTask<PsOutput> WaitForOutputAsync(CancellationToken cancellationToken)
    {
        if (this.IsStdoutRedirected && !this.IsStdoutPiped)
        {
            using var ms = new MemoryStream();
            await this.process.StandardOutput.PipeToAsync(ms, cancellationToken: cancellationToken)
                .NoCap();
            await ms.FlushAsync(cancellationToken)
                .NoCap();
            this.stdout = ms.ToArray();
        }

        if (this.IsStderrRedirected && !this.IsStderrPiped)
        {
            using var ms = new MemoryStream();
            await this.process.StandardError
                .PipeToAsync(ms, cancellationToken: cancellationToken)
                .NoCap();

            await ms.FlushAsync(cancellationToken)
                .NoCap();
            this.stderr = ms.ToArray();
        }

        if (this.process.HasExited)
        {
            this.exitTime = this.process.ExitTime;
        }
        else
        {
            await this.process.WaitForExitAsync(cancellationToken).NoCap();
            this.exitTime = this.process.ExitTime;
        }

        return new PsOutput(
            this.process.StartInfo.FileName,
            this.process.ExitCode,
            this.stdout,
            this.stderr,
            this.StartTime,
            this.exitTime);
    }

    public void Kill()
    {
        this.process.Kill();
    }

    public void Dispose()
    {
        if (this.disposed)
            return;

        this.disposed = true;
        GC.SuppressFinalize(this);
        this.process.Dispose();
        if (this.disposables.Count == 0)
            return;

        foreach (var disposable in this.disposables)
        {
            disposable.Dispose();
        }
    }

    private void GuardPiped()
    {
        if (this.IsStdoutPiped)
            throw new InvalidOperationException("Cannot pipe stdout. Stdout stream can only be read once.");

        if (!this.IsStdoutRedirected)
            throw new InvalidOperationException("Cannot pipe to stdout when stream is not redirected.");

        this.IsStdoutPiped = true;
    }

    private void GuardErrorPiped()
    {
        if (this.IsStderrPiped)
            throw new InvalidOperationException("Cannot pipe stderr. Stderr stream can only be read once.");

        if (!this.IsStderrRedirected)
            throw new InvalidOperationException("Cannot pipe to stderr when stream is not redirected.");

        this.IsStderrPiped = true;
    }
}