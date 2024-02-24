namespace Gnome.Diagnostics;

internal class PowerShellExecutor : ShellExecutor
{
    public PowerShellExecutor()
    {
        ExePathFinder.Default.RegisterOrUpdate("powershell", (entry) =>
        {
            entry.Windows.AddRange(new HashSet<string>()
            {
                "%ProgramFiles%/PowerShell/7/pwsh.exe",
                "%ProgramFiles(x86)%/PowerShell/7/pwsh.exe",
                "%ProgramFiles%/PowerShell/6/pwsh.exe",
                "%ProgramFiles(x86)%/PowerShell/6/pwsh.exe",
            });

            entry.Linux.AddRange(new HashSet<string>()
            {
                "/opt/microsoft/powershell/7/pwsh",
                "/opt/microsoft/powershell/6/pwsh",
            });
        });
    }

    public override string Shell => "pwsh";

    public override string Extension => ".ps1";

    protected override string GenerateScriptFile(string script, string extension)
    {
        script = $$"""
                   $ErrorActionPreference = 'Stop'
                   {{script}}
                   if ((Test-Path -LiteralPath variable:\\LASTEXITCODE)) { exit $LASTEXITCODE }
                   """;
        return base.GenerateScriptFile(script, extension);
    }

    protected override PsChild Spawn(string file, PsStartInfo? info)
    {
        var exe = ExePathFinder.Default.FindOrThrow("pwsh");
        info ??= new PsStartInfo();
        info.FileName = exe;
        info.WithArgs(
            new[]
            {
                "-ExecutionPolicy",
                "Bypass",
                "-NoLogo",
                "-NoProfile",
                "-NonInteractive",
                "-Command",
                $". {file}",
            });

        return info.Spawn();
    }
}