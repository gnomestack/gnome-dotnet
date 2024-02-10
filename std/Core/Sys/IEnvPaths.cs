namespace Gnome.Sys;

public interface IEnvPaths : IEnumerable<string>
{
    IEnvPaths Append(string path);

    IEnvPaths Prepend(string path);

    bool Has(string path);

    bool Remove(string path);
}