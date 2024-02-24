namespace Gnome.Diagnostics;

internal class NodeExecutor : ShellExecutor
{
    public NodeExecutor()
    {
        ExePathFinder.Default.RegisterOrUpdate("node", (entry) =>
        {
            entry.Windows.AddRange(new HashSet<string>()
            {
                @"%ProgramFiles%\nodejs\node.exe",
                @"%ProgramFiles(x86)%\nodejs\node.exe",
            });
        });
    }

    public override string Shell => "node";

    public override string Extension => ".js";
}