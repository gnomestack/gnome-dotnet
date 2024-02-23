using System.Collections;

namespace Gnome.Sys;

public class EnvVariables : IEnvVariables
{
    public Option<string> this[string name]
    {
        get => this.Get(name);
        set => this.Set(name, value);
    }

    public string Expand(string value, EnvExpandOptions? options = null)
        => EnvVariablesExpander.Expand(value, options);

    public ReadOnlySpan<char> Expand(ReadOnlySpan<char> value, EnvExpandOptions? options = null)
        => EnvVariablesExpander.Expand(value, options);

    public Result<string> ExpandAsResult(string value, EnvExpandOptions? options = null)
        => EnvVariablesExpander.ExpandAsResult(value, options);

    public ValueResult<char[]> ExpandAsResult(ReadOnlySpan<char> value, EnvExpandOptions? options = null)
        => EnvVariablesExpander.ExpandAsResult(value, options);

    public Option<string> Get(string name)
        => Option.From(System.Environment.GetEnvironmentVariable(name));

    public bool Has(string name)
        => Environment.GetEnvironmentVariable(name) is not null;

    public void Set(string name, string value)
        => System.Environment.SetEnvironmentVariable(name, value);

    public void Remove(string name)
        => System.Environment.SetEnvironmentVariable(name, null);

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        var map = System.Environment.GetEnvironmentVariables();
        foreach (var key in map.Keys)
        {
            if (key is not string k || string.IsNullOrWhiteSpace(k))
                continue;

            var rawValue = map[key];
            if (rawValue is not string v || string.IsNullOrWhiteSpace(v))
                continue;

            yield return new KeyValuePair<string, string>(k, v);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => this.GetEnumerator();
}