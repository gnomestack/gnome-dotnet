using System.Diagnostics.CodeAnalysis;

namespace Gnome;

public class Option<T> : IOption<T, Option<T>>
    where T : notnull
{
    private T? value;

    public Option(T? value)
    {
        this.value = value;
        this.HasValue = value is not null;
    }

    public Option()
    {
        this.value = default!;
        this.HasValue = false;
    }

    public bool HasValue { get; private set; }

    public T Value => this.value! ?? throw new InvalidOperationException("Option does not have a value.");

    public static implicit operator T(Option<T> option)
        => option.Value;

    public static implicit operator Option<T>(T? value)
        => new(value);

    public static implicit operator Option<T>(ValueTuple value)
        => new();

    public static Option<T> From(T? value)
        => new(value);

    public Option<T> And(Option<T> other)
        => this.HasValue ? other : this;

    public Option<T> And(Func<Option<T>> factory)
        => this.HasValue ? factory() : this;

    public Option<T> And(T? other)
        => this.HasValue ? new(other) : this;

    public void Deconstruct([MaybeNull] out T value, out bool hasValue)
    {
        value = this.value!;
        hasValue = this.HasValue;
    }

    public Option<T> Inspect(Action<T> action)
    {
        if (this.HasValue)
            action(this.Value);

        return this;
    }

    public bool Equals(IOption<T>? other)
    {
        if (other is null)
            return false;

        if (this.HasValue != other.HasValue)
            return false;

        return !this.HasValue || EqualityComparer<T>.Default.Equals(this.Value, other.Value);
    }

    public T GetValueOrDefault(T defaultValue)
        => this.HasValue ? this.Value : defaultValue;

    public T GetValueOrDefault(Func<T> defaultValueFactory)
        => this.HasValue ? this.Value : defaultValueFactory();

    public Option<T> Or(Option<T> other)
        => this.HasValue ? this : other;

    public Option<T> Or(Func<Option<T>> factory)
        => this.HasValue ? this : factory();

    public Option<T> Or(T? other)
        => this.HasValue ? this : new(other);

    public Option<T> Or(Func<T, bool> predicate, Option<T> other)
        => (this.HasValue && predicate(this.value!)) ? this : other;

    public Option<T> Or(Func<T, bool> predicate, Func<Option<T>> factory)
        => (this.HasValue && predicate(this.value!)) ? this : factory();

    public Option<TOther> Select<TOther>(Func<T, TOther> map)
        where TOther : notnull
        => this.HasValue ? new(map(this.Value)) : new();

    public Option<TOther> Select<TOther>(Func<T, TOther> map, Func<TOther> factory)
        where TOther : notnull
        => this.HasValue ? new(map(this.Value)) : new(factory());

    public Option<T> Replace(T value)
    {
        this.value = value;
        this.HasValue = value is not null;
        return this;
    }

    public T Take()
    {
        var value = this.Value;
        this.HasValue = false;
        this.value = default!;
        return value;
    }

    public bool Test(Func<T, bool> predicate)
        => this.HasValue && predicate(this.Value);

    public Result<T> ToResult(Func<Error> errorFactory)
        => this.HasValue ? new(this.Value) : new(errorFactory());

    public Result<T> ToResult()
        => this.HasValue ? new(this.Value) : new(new InvalidOperationException("Option does not have a value."));

    public override string ToString()
        => this.HasValue ? (this.Value?.ToString() ?? string.Empty) : "None";

    public Option<T> Where(Func<T, bool> predicate)
        => this.HasValue && predicate(this.Value) ? this : new();

    public Option<(T, TOther)> Zip<TOther>(Option<TOther> other)
        where TOther : notnull
        => this.HasValue && other.HasValue ? new((this.Value, other.Value)) : new();

    public Option<(T, TOther)> Zip<TOther>(TOther other)
        where TOther : notnull
        => this.HasValue ? new((this.Value, other)) : new();
}