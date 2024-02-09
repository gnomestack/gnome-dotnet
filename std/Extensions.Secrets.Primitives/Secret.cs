namespace Gnome.Extensions.Secrets;

public static class Secret
{
    public static Secret<T> From<T>(ReadOnlySpan<T> value)
        where T : unmanaged
        => new(value);

    public static Secret<T> From<T>(T[] value)
        where T : unmanaged
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return new(value);
    }

    public static Secret<char> From(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        // The string is already in memory, so there isn't a point in encrypting it.
        return new(value.AsSpan(), false);
    }

    public static string RevealString(this Secret<char> secret)
        => new(secret.Reveal());
}