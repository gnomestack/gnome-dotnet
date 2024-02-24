namespace Gnome.Diagnostics;

internal class DenoJsExecutor : ShellExecutor
{
    public DenoJsExecutor()
    {
        ExePathFinder.Default.RegisterOrUpdate("deno-js", (entry) =>
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

    public override string Extension => ".js";

    protected override PsChild Spawn(string file, PsStartInfo? info)
    {
        var exe = ExePathFinder.Default.FindOrThrow("deno");
        info ??= new PsStartInfo();
        info.FileName = exe;
        info.WithArgs(new[] { "run", "-A", "--unstable", file });

        return info.Spawn();
    }
}