namespace Gnome.Diagnostics;

internal class GenericExecutor : ShellExecutor
{
    private readonly PsStartInfo info;

    public GenericExecutor(PsStartInfo info)
        => this.info = info;

    public override string Shell => "generic";

    public override string Extension => ".*";

    protected override PsChild Spawn(string file, PsStartInfo? info)
    {
        return this.info.Spawn();
    }
}