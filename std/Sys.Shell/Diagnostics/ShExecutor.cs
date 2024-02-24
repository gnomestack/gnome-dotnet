using System.Diagnostics;

namespace Gnome.Diagnostics;

internal class ShExecutor : ShellExecutor
{
    public ShExecutor()
    {
        ExePathFinder.Default.RegisterOrUpdate("sh", (entry) =>
        {
            entry.Windows.AddRange(new HashSet<string>()
            {
                @"%ProgramFiles%\Git\usr\bin\sh.exe",
                @"%ChocolateyInstall%\msys2\usr\bin\sh.exe",
                @"%SystemDrive%\msys64\usr\bin\sh.exe",
                @"%SystemDrive%\msys\usr\bin\sh.exe",
            });
        });
    }

    public override string Shell => "sh";

    public override string Extension => ".sh";

    protected override PsChild Spawn(string file, PsStartInfo? info)
    {
        var exe = ExePathFinder.Default.FindOrThrow("sh");
        info ??= new PsStartInfo();
        info.WithArgs(new[] { "-e", file });

        Debug.WriteLine($"{exe} -e {file}");

        return info.Spawn();
    }
}