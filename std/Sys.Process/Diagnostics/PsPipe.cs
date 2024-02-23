namespace Gnome.Diagnostics;

public class PsPipe
{
    private PsChild child;

    public PsPipe(PsCommand command)
        : this(command.BuildStartInfo())
    {
    }

    public PsPipe(PsStartInfo startInfo)
    {
        startInfo.WithStdio(Stdio.Piped);
        this.child = new PsChild(startInfo);
    }

    public PsPipe(PsChild child)
    {
        this.child = child;
    }

    public PsPipe Pipe(string fileName, PsArgs? args = null)
        => this.Pipe(new PsStartInfo(fileName, args ?? new PsArgs()));

    public PsPipe Pipe(PsCommand command)
        => this.Pipe(command.BuildStartInfo());

    public PsPipe Pipe(PsStartInfo startInfo)
    {
        startInfo.WithStdio(Stdio.Piped);
        var next = new PsChild(startInfo);
        this.child.PipeTo(next);
        this.child.Wait();
        this.child.Dispose();
        this.child = next;

        return this;
    }

    public PsPipe Pipe(PsChild next)
    {
        this.child.PipeTo(next);
        this.child.Wait();
        this.child.Dispose();
        this.child = next;

        return this;
    }

    public async Task<PsPipe> PipeAsync(PsChild next, CancellationToken cancellationToken = default)
    {
        await this.child.PipeToAsync(next, cancellationToken)
            .ConfigureAwait(false);
        this.child.Dispose();
        this.child = next;
        return this;
    }

    public Task<PsPipe> PipeAsync(PsCommand command, CancellationToken cancellationToken)
        => this.PipeAsync(command.BuildStartInfo(), cancellationToken);

    public async Task<PsPipe> PipeAsync(PsStartInfo startInfo, CancellationToken cancellationToken = default)
    {
        startInfo.WithStdio(Stdio.Piped);
        var next = new PsChild(startInfo);
        await this.child.PipeToAsync(next, cancellationToken)
            .ConfigureAwait(false);
        this.child.Dispose();
        this.child = next;
        return this;
    }

    public Task<PsPipe> PipeAsync(string fileName, PsArgs? args = null, CancellationToken cancellationToken = default)
        => this.PipeAsync(new PsStartInfo(fileName, args ?? new PsArgs()), cancellationToken);

    public PsOutput Output()
    {
        return this.child.WaitForOutput();
    }

    public ValueTask<PsOutput> OutputAsync(CancellationToken cancellationToken = default)
    {
        return this.child.WaitForOutputAsync(cancellationToken);
    }
}