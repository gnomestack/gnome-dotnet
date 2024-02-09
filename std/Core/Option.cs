namespace Gnome;

public static class Option
{
    public static Option<T> Some<T>(T value)
        where T : notnull
        => new Option<T>(value);

    public static Option<T> None<T>()
        where T : notnull
        => new();

    public static Option<T> From<T>(T? value)
        where T : notnull
        => new(value);
}