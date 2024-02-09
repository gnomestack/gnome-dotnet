namespace Gnome;

public interface IOption<T> : IEquatable<IOption<T>>
    where T : notnull
{
    bool HasValue { get; }

    T Value { get; }

    T GetValueOrDefault(T defaultValue);

    T GetValueOrDefault(Func<T> defaultValueFactory);
}