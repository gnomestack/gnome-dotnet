namespace Gnome.Diagnostics;

public class ExePathHint
{
    public ExePathHint(string name)
    {
        this.Name = name;
    }

    public string Name { get; }

    public string? Executable { get; set; }

    public string? EnvVariable { get; set; }

    public string? CachedPath { get; set; }

    public HashSet<string> Windows { get; set; } = new();

    public HashSet<string> Linux { get; set; } = new();

    public HashSet<string> Darwin { get; set; } = new();
}