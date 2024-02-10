namespace Gnome.Sys;

public interface IEnvVariables : IEnumerable<KeyValuePair<string, string>>
{
    Option<string> this[string name] { get; set; }

    Option<string> Get(string name);

    void Set(string name, string value);

    void Remove(string name);

    bool Has(string name);
}
