namespace Gnome.Diagnostics;

internal class DenoExecutor : ShellExecutor
{
    public DenoExecutor()
    {
        ExePathFinder.Default.RegisterOrUpdate("deno", (entry) =>
        {
            entry.Windows.AddRange(new HashSet<string>()
            {
                @"%USERPROFILE%\.deno\bin\deno.exe",
                @"%ChocolateyInstall%\lib\deno\tools\deno.exe",
            });

            entry.Linux.AddRange(new HashSet<string>()
            {
                @"${HOME}\.deno\bin\deno",
            });
        });
    }

    public override string Shell => "deno";

    public override string Extension => ".ts";

    protected override PsChild Spawn(string file, PsStartInfo? info)
    {
        var exe = ExePathFinder.Default.FindOrThrow("deno");
        info ??= new PsStartInfo();
        info.FileName = exe;
        info.WithArgs(new[] { "run", "-A", "--unstable", file });

        return info.Spawn();
    }
}