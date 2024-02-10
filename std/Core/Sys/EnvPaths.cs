using System.Collections;

using static Gnome.Sys.OS;

namespace Gnome.Sys;

public class EnvPaths : IEnvPaths
{
    private readonly EnvVariables env;

    private readonly string pathName;

    public EnvPaths(EnvVariables env)
    {
        this.env = env;
        this.pathName = IsWindows() ? "Path" : "PATH";
    }

    public static string[] Split(string paths)
    {
        return paths.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string Join(string[] paths)
    {
#if NET5_0_OR_GREATER
        return string.Join(Path.PathSeparator, paths);
#else
        return string.Join(Path.PathSeparator.ToString(), paths);
#endif
    }

    public static string Join(IEnumerable<string> paths)
    {
#if NET5_0_OR_GREATER
        return string.Join(Path.PathSeparator, paths);
#else
        return string.Join(Path.PathSeparator.ToString(), paths);
#endif
    }

    public IEnvPaths Append(string path)
    {
        var pathValue = this.env.Get(this.pathName);
        var paths = Split(pathValue.GetValueOrDefault(string.Empty));
        if (Has(paths, path))
            return this;

        Array.Resize(ref paths, paths.Length + 1);
        paths[paths.Length - 1] = path;

        this.env.Set(this.pathName, Join(paths));
        return this;
    }

    public IEnvPaths Prepend(string path)
    {
        var pathValue = this.env.Get(this.pathName);
        var paths = Split(pathValue.GetValueOrDefault(string.Empty));
        if (Has(paths, path))
            return this;

        Array.Resize(ref paths, paths.Length + 1);
        Array.Copy(paths, 0, paths, 1, paths.Length - 1);
        paths[0] = path;

        this.env.Set(this.pathName, Join(paths));
        return this;
    }

    public bool Remove(string path)
    {
        var pathValue = this.env.Get(this.pathName);
        var paths = Split(pathValue.GetValueOrDefault(string.Empty));
        if (!Has(paths, path))
            return false;

        var list = new List<string>(paths);
        list.Remove(path);

        this.env.Set(this.pathName, Join(list));
        return true;
    }

    public bool Has(string path)
    {
        var pathValue = this.env.Get(this.pathName);
        var paths = Split(pathValue.GetValueOrDefault(string.Empty));
        return Has(paths, path);
    }

    public IEnumerator<string> GetEnumerator()
    {
        var pathValue = this.env.Get(this.pathName);
        var paths = Split(pathValue.GetValueOrDefault(string.Empty));
        foreach (var path in paths)
            yield return path;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => this.GetEnumerator();

    private static bool Has(string[] paths, string path)
    {
        if (IsWindows())
        {
            foreach (var next in paths)
            {
                if (path.Equals(next, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        return Array.IndexOf(paths, path) != -1;
    }
}