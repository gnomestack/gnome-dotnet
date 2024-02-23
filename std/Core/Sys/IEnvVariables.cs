namespace Gnome.Sys;

public interface IEnvVariables : IEnumerable<KeyValuePair<string, string>>
{
    Option<string> this[string name] { get; set; }

    string Expand(string value, EnvExpandOptions? options = null);

    ReadOnlySpan<char> Expand(ReadOnlySpan<char> value, EnvExpandOptions? options = null);

    Result<string> ExpandAsResult(string value, EnvExpandOptions? options = null);

    ValueResult<char[]> ExpandAsResult(ReadOnlySpan<char> value, EnvExpandOptions? options = null);

    Option<string> Get(string name);

    void Set(string name, string value);

    void Remove(string name);

    bool Has(string name);
}