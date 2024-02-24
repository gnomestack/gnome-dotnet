namespace Gnome.Diagnostics;

internal class FsiExecutor : ShellExecutor
{
    public FsiExecutor()
    {
        ExePathFinder.Default.RegisterOrUpdate("fsi", (entry) =>
        {
            entry.Windows.AddRange(new HashSet<string>()
            {
                @"%USERPROFILE%\.dotnet\dotnet.exe",
            });

            entry.Linux.AddRange(new HashSet<string>()
            {
                @"${HOME}/.dotnet/dotnet",
            });
        });
    }

    public override string Extension => ".fsx";

    public override string Shell => "fsi";

    protected override PsChild Spawn(string file, PsStartInfo? info)
    {
        var exe = ExePathFinder.Default.FindOrThrow("fsi");
        info ??= new PsStartInfo();
        info.FileName = exe;
        info.WithArgs(new[] { "fsi", file });

        return info.Spawn();
    }
}