namespace Gnome.Diagnostics;

internal class CmdExecutor : ShellExecutor
{
    public override string Shell => "cmd";

    public override string Extension => ".cmd";

    protected override string GenerateScriptFile(string script, string extension)
    {
        script = $"""
                  @echo off
                  {script}
                  """;
        return base.GenerateScriptFile(script, extension);
    }

    protected override PsChild Spawn(string file, PsStartInfo? info)
    {
        var exe = ExePathFinder.Default.FindOrThrow("cmd");
        info ??= new PsStartInfo();
        info.FileName = exe;
        info.WithArgs(
                new[]
                {
                    "/D",
                    "/E:ON",
                    "/V:OFF",
                    "/S",
                    "/C", $"CALL \"{file}\"",
                });

        return info.Spawn();
    }
}