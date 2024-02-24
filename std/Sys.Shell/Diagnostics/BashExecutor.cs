using Gnome.Sys;

namespace Gnome.Diagnostics;

internal sealed class BashExecutor : ShellExecutor
{
    private const string DefaultShell = "bash";

    public BashExecutor()
    {
        ExePathFinder.Default.RegisterOrUpdate(DefaultShell, (entry) =>
        {
            entry.Windows.AddRange(new HashSet<string>()
            {
                @"%ProgramFiles%\Git\bin\bash.exe",
                @"%ProgramFiles%\Git\usr\bin\bash.exe",
                @"%ChocolateyInstall%\msys2\usr\bin\bash.exe",
                @"%SystemDrive%\msys64\usr\bin\bash.exe",
                @"%SystemDrive%\msys\usr\bin\bash.exe",
                @"%SystemRoot%\System32\bash.exe",
            });
        });
    }

    public override string Shell => DefaultShell;

    public override string Extension => ".sh";

    protected override PsChild Spawn(string file, PsStartInfo? info)
    {
        var exe = ExePathFinder.Default.FindOrThrow(DefaultShell);
        if (Platform.IsWindows())
        {
            file = file.Replace("\\", "/");
            if (exe.EndsWith("System32\\bash.exe", StringComparison.OrdinalIgnoreCase))
            {
                file = "/mnt/" + "c" + file.Substring(1).Replace(":", string.Empty);
            }
        }

        info ??= new PsStartInfo();
        info.FileName = exe;
        info.WithArgs(new[] { "-noprofile", "--norc", "-e", "-o", "pipefail", "-c", file });
        return info.Spawn();
    }
}