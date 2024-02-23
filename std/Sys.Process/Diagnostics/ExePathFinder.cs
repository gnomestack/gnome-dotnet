using System.Collections.Concurrent;

using Gnome.IO;
using Gnome.Sys;
using Gnome.Util.Strings;

namespace Gnome.Diagnostics;

public partial class ExePathFinder
{
    private readonly ConcurrentDictionary<string, ExePathHint> entries = new(StringComparer.OrdinalIgnoreCase);

    public static ExePathFinder Default { get; } = new();

    public ExePathHint? this[string name]
    {
        get => this.entries.TryGetValue(name, out var entry) ? entry : null;
        set
        {
            if (value is null)
                this.entries.TryRemove(name, out _);
            else
                this.entries[name] = value;
        }
    }

    public void Register(string name, ExePathHint entry)
    {
        this.entries[name] = entry;
        if (entry.EnvVariable.IsNullOrWhiteSpace())
        {
            entry.EnvVariable = name.ScreamingSnakeCase() + "_PATH";
        }
    }

    public void Register(string name, Func<ExePathHint> factory)
    {
        if (!this.entries.TryGetValue(name, out _))
        {
            this.entries[name] = factory();
        }
    }

    public void RegisterOrUpdate(string name, Action<ExePathHint> update)
    {
        if (!this.entries.TryGetValue(name, out var entry))
        {
            entry = new ExePathHint(name);
            this.Register(name, entry);
        }

        update(entry);
    }

    public void Update(string name, Action<ExePathHint> update)
    {
        if (this.entries.TryGetValue(name, out var entry))
        {
            update(entry);
        }
    }

    public bool Has(string name)
    {
        return this.entries.ContainsKey(name);
    }

    public string FindOrThrow(string name)
    {
        var path = this.Find(name);
        if (path is null)
            throw new FileNotFoundException($"Could not find {name} on the PATH.");

        return path;
    }

    public ValueResult<string, FileNotFoundException> FindAsResult(string name)
    {
        var path = this.Find(name);
        if (path is null)
            return new FileNotFoundException($"Could not find {name} on the PATH.");

        return path;
    }

    public string? Find(string name)
    {
#if NET5_0_OR_GREATER
        if (Path.IsPathFullyQualified(name))
            return name;
#else
        if (Path.IsPathRooted(name) && FileSystem.IsFile(name))
            return name;
#endif
        var entry = this[name];
        if (entry is null)
        {
            entry = new ExePathHint(name);
            this.Register(name, entry);
        }

        if (!entry.EnvVariable.IsNullOrWhiteSpace())
        {
            var cached = !entry.CachedPath.IsNullOrWhiteSpace();
            var envPath = Platform.Env.Get(entry.EnvVariable);
            if (!envPath.IsNullOrWhiteSpace())
            {
                if (cached && envPath == entry.CachedPath)
                    return envPath;

                envPath = Platform.Env.Expand(envPath);
                envPath = Path.GetFullPath(envPath);
                if (cached && entry.CachedPath == envPath)
                    return envPath;

                var tmp = Which(envPath);
                if (tmp is not null)
                {
                    entry.CachedPath = tmp;
                    return tmp;
                }
            }
        }

        if (!entry.CachedPath.IsNullOrWhiteSpace())
            return entry.CachedPath;

        var exe = entry.Executable ?? name;
        exe = Which(exe);
        if (exe is not null)
        {
            entry.Executable = Path.GetFileName(exe);
            entry.CachedPath = exe;
            return exe;
        }

        if (Platform.IsWindows())
        {
            foreach (var attempt in entry.Windows)
            {
                exe = attempt;
                exe = Platform.Env.Expand(exe);
                exe = Which(exe);
                if (exe is null)
                {
                    continue;
                }

                entry.Executable = Path.GetFileName(exe);
                entry.CachedPath = exe;
                return exe;
            }

            return null;
        }

        if (Platform.IsMacOS())
        {
            foreach (var attempt in entry.Darwin)
            {
                exe = attempt;
                exe = Platform.Env.Expand(exe);
                exe = Which(exe);
                if (exe is null)
                {
                    continue;
                }

                entry.Executable = Path.GetFileName(exe);
                entry.CachedPath = exe;
                return exe;
            }
        }

        foreach (var attempt in entry.Linux)
        {
            exe = attempt;
            exe = Platform.Env.Expand(exe);
            exe = Which(exe);
            if (exe is null)
            {
                continue;
            }

            entry.Executable = Path.GetFileName(exe);
            entry.CachedPath = exe;
            return exe;
        }

        return null;
    }
}