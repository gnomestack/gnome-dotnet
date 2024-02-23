using System.Runtime.Versioning;
using System.Security;
using System.Text;

namespace Gnome.Diagnostics;

public class PsStartInfo
{
    private readonly List<IDisposable> disposables = new();

    public PsStartInfo()
    {
    }

    public PsStartInfo(string fileName)
    {
        this.FileName = fileName;
    }

    public PsStartInfo(string fileName, PsArgs args)
    {
        this.FileName = fileName;
        this.Args = args;
    }

    public string FileName { get; set; } = string.Empty;

    public PsArgs Args { get; set; } = new PsArgs();

    public string? Cwd { get; set; }

    public IDictionary<string, string?>? Env { get; set; }

    public Stdio Stdout { get; set; }

    public Stdio Stderr { get; set; }

    public Stdio Stdin { get; set; }

    public string? User { get; set; }

    public string? Verb { get; set; }

    [SupportedOSPlatform("windows")]
    [CLSCompliant(false)]
    public SecureString? Password { get; set; }

    [SupportedOSPlatform("windows")]
    public string? PasswordInClearText { get; set; }

    [SupportedOSPlatform("windows")]
    public string? Domain { get; set; }

    public bool LoadUserProfile { get; set; } = false;

    public bool CreateNoWindow { get; set; } = false;

    public bool UseShellExecute { get; set; } = false;

    public PsStartInfo WithArgs(PsArgs args)
    {
        this.Args = args;
        return this;
    }

    public PsStartInfo WithCwd(string cwd)
    {
        this.Cwd = cwd;
        return this;
    }

    public PsStartInfo WithEnv(IDictionary<string, string?> env)
    {
        this.Env = env;
        return this;
    }

    public PsStartInfo SetEnv(string name, string value)
    {
        this.Env ??= new Dictionary<string, string?>();
        this.Env[name] = value;
        return this;
    }

    public PsStartInfo SetEnv(IEnumerable<KeyValuePair<string, string?>> values)
    {
        this.Env ??= new Dictionary<string, string?>();
        foreach (var kvp in values)
        {
            this.Env[kvp.Key] = kvp.Value;
        }

        return this;
    }

    public PsStartInfo WithDisposable(IDisposable disposable)
    {
        this.disposables.Add(disposable);
        return this;
    }

    public PsStartInfo WithStdOut(Stdio stdio)
    {
        this.Stdout = stdio;
        return this;
    }

    public PsStartInfo WithStdErr(Stdio stdio)
    {
        this.Stderr = stdio;
        return this;
    }

    public PsStartInfo WithStdIn(Stdio stdio)
    {
        this.Stdin = stdio;
        return this;
    }

    public PsStartInfo WithStdio(Stdio stdio)
    {
        this.Stdout = stdio;
        this.Stderr = stdio;
        this.Stdin = stdio;
        return this;
    }

    public PsStartInfo WithVerb(string verb)
    {
        this.Verb = verb;
        return this;
    }

    public PsStartInfo AsWindowsAdmin()
    {
        this.Verb = "runas";
        return this;
    }

    public PsStartInfo AsSudo()
    {
        this.Verb = "sudo";
        return this;
    }

    [SupportedOSPlatform("windows")]
    public PsStartInfo WithUser(string user)
    {
        this.User = user;
        return this;
    }

    [SupportedOSPlatform("windows")]
    public PsStartInfo WithPassword(string password)
    {
        this.PasswordInClearText = password;
        return this;
    }

    [SupportedOSPlatform("windows")]
    public PsStartInfo WithDomain(string domain)
    {
        this.Domain = domain;
        return this;
    }

    public PsChild Spawn()
    {
        return new(this);
    }
}