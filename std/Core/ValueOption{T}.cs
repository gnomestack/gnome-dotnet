using System.ComponentModel;

namespace Gnome;

public readonly struct ValueOption<T> : IOption<T, ValueOption<T>>
    where T : notnull
{
    private readonly T? value;

    public ValueOption()
    {
        this.value = default;
        this.HasValue = false;
    }

    public ValueOption(T? value)
    {
        this.value = value;
        this.HasValue = value is not null;
    }

    public bool HasValue { get; }

    public T Value
        => this.value ?? throw new InvalidOperationException("Option does not have a value.");

    public static ValueOption<T> From(T? value)
        => new(value);

    public ValueOption<T> And(ValueOption<T> other)
        => this.HasValue ? other : this;

    public ValueOption<T> And(Func<ValueOption<T>> factory)
        => this.HasValue ? factory() : this;

    public ValueOption<T> And(T? other)
        => this.HasValue ? new(other) : this;

    public bool Equals(IOption<T>? other)
    {
        if (other is null)
            return false;

        if (this.HasValue != other.HasValue)
            return false;

        return !this.HasValue || EqualityComparer<T>.Default.Equals(this.Value, other.Value);
    }

    public T GetValueOrDefault(T defaultValue)
        => this.HasValue ? this.value! : defaultValue;

    public T GetValueOrDefault(Func<T> defaultValueFactory)
        => this.HasValue ? this.value! : defaultValueFactory();

    public ValueOption<T> Or(ValueOption<T> other)
        => this.HasValue ? this : other;

    public ValueOption<T> Or(Func<ValueOption<T>> factory)
        => this.HasValue ? this : factory();

    public ValueOption<T> Or(T? other)
        => this.HasValue ? this : new(other);

    public ValueOption<T> Or(Func<T, bool> predicate, ValueOption<T> other)
        => (this.HasValue && predicate(this.value!)) ? this : other;

    public ValueOption<T> Or(Func<T, bool> predicate, Func<ValueOption<T>> factory)
        => (this.HasValue && predicate(this.value!)) ? this : factory();

    public ValueOption<TOther> Select<TOther>(Func<T, TOther> map)
        where TOther : notnull
        => this.HasValue ? new(map(this.Value)) : new();

    public ValueOption<TOther> Select<TOther>(Func<T, TOther> map, Func<TOther> factory)
        where TOther : notnull
        => this.HasValue ? new(map(this.Value)) : new(factory());

    public bool Test(Func<T, bool> predicate)
        => this.HasValue && predicate(this.Value);

    public override string ToString()
        => this.HasValue ? (this.Value?.ToString() ?? string.Empty) : "None";

    public ValueOption<T> Where(Func<T, bool> predicate)
        => this.HasValue && predicate(this.Value) ? this : new();
}