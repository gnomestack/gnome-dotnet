namespace Gnome.Diagnostics;

public abstract class PsCommand
{
    private PsStartInfo? args = null;

    public PsCommand WithStartInfo(PsStartInfo startInfo)
    {
        this.args = startInfo;
        return this;
    }

    public PsStartInfo BuildStartInfo()
    {
        this.args ??= new PsStartInfo();
        this.args.FileName = this.GetExecutablePath();
        this.args.Args = this.BuildPsArgs();

        return this.args;
    }

    protected abstract string GetExecutablePath();

    protected virtual PsArgs BuildPsArgs()
        => new();
}