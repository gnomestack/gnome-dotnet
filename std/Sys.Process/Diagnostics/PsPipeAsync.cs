namespace Gnome.Diagnostics;

public class PsPipeAsync
{
    private readonly PsChild child;
    private readonly List<Func<PsChild, Task<PsChild>>> steps = new();

    public PsPipeAsync(PsCommand command)
        : this(command.BuildStartInfo())
    {
    }

    public PsPipeAsync(PsStartInfo startInfo)
    {
        startInfo.WithStdio(Stdio.Piped);
        this.child = new PsChild(startInfo);
    }

    public PsPipeAsync(PsChild child)
    {
        this.child = child;
    }

    public PsPipeAsync Pipe(
        string fileName,
        PsArgs? args = null,
        CancellationToken cancellationToken = default)
        => this.Pipe(new PsStartInfo(fileName, args ?? new PsArgs()), cancellationToken);

    public PsPipeAsync Pipe(PsCommand command, CancellationToken cancellationToken = default)
        => this.Pipe(command.BuildStartInfo(), cancellationToken);

    public PsPipeAsync Pipe(PsStartInfo startInfo, CancellationToken cancellationToken = default)
    {
        this.steps.Add(async (child) =>
        {
            startInfo.WithStdio(Stdio.Piped);
            var next = new PsChild(startInfo);
            await child.PipeToAsync(next)
                .ConfigureAwait(false);
            await child.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            child.Dispose();

            return next;
        });
        return this;
    }

    public PsPipeAsync Pipe(PsChild next, CancellationToken cancellationToken = default)
    {
        this.steps.Add(async (child) =>
        {
            await child.PipeToAsync(next, cancellationToken)
                .ConfigureAwait(false);
            await child.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            child.Dispose();

            return next;
        });

        return this;
    }

    public async Task<PsOutput> OutputAsync(CancellationToken cancellationToken = default)
    {
        var child = this.child;
        foreach (var step in this.steps)
        {
            child = await step(child);
        }

        return await child.WaitForOutputAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}