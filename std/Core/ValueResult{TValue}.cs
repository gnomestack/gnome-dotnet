using System.Diagnostics.CodeAnalysis;

namespace Gnome;

public readonly struct ValueResult<TValue> : IResult<TValue, Error>
    where TValue : notnull
{
    private readonly TValue? value;

    private readonly Error? error;

    public ValueResult(TValue value)
    {
        this.value = value;
        this.error = default!;
        this.IsOk = true;
    }

    public ValueResult(Error error)
    {
        this.value = default!;
        this.error = error;
        this.IsOk = false;
    }

    public bool IsOk { get; }

    public TValue Value
        => this.value ?? throw new InvalidOperationException("Result does not have a value.");

    public Error Error
        => this.error ?? throw new InvalidOperationException("Result does not have an error.");

    public static implicit operator ValueResult<TValue>(TValue value)
        => new(value);

    public static implicit operator ValueResult<TValue>(Error error)
        => new(error);

    public static implicit operator ValueResult<TValue>(Exception exception)
        => new(Error.Convert(exception));

    public static implicit operator TValue(ValueResult<TValue> result)
        => result.Value;

    public static implicit operator Error(ValueResult<TValue> result)
        => result.Error;

    public static implicit operator Task<ValueResult<TValue>>(ValueResult<TValue> result)
        => Task.FromResult(result);

    public static implicit operator ValueTask<ValueResult<TValue>>(ValueResult<TValue> result)
        => result;

    public static ValueResult<TValue> Ok(TValue value)
        => new(value);

    public static ValueResult<TValue> Fail(Error error)
        => new(error);

    public static ValueResult<TValue> Fail(Exception exception)
        => new(Error.Convert(exception));

    public void Deconstruct([MaybeNull] out TValue value, [MaybeNull] out Error error, out bool isOk)
    {
        value = this.value!;
        error = this.error!;
        isOk = this.IsOk;
    }

    public Error GetErrorOrDefault(Error defaultError)
        => this.IsOk ? defaultError : this.error!;

    public Error GetErrorOrDefault(Func<Error> defaultErrorFactory)
        => this.IsOk ? defaultErrorFactory() : this.error!;

    public TValue GetValueOrDefault(TValue defaultValue)
        => this.IsOk ? this.value! : defaultValue;

    public TValue GetValueOrDefault(Func<TValue> defaultValueFactory)
        => this.IsOk ? this.value! : defaultValueFactory();

    public ValueResult<TValue> Inspect(Action<TValue> action)
    {
        if (this.IsOk)
            action(this.Value);

        return this;
    }

    public ValueResult<TValue> InspectError(Action<Error> action)
    {
        if (!this.IsOk)
            action(this.Error);

        return this;
    }

    public ValueResult<TValue> And(ValueResult<TValue> other)
        => this.IsOk ? other : this;

    public ValueResult<TValue> And(Func<ValueResult<TValue>> factory)
        => this.IsOk ? factory() : this;

    public ValueResult<TValue> And(TValue value)
        => this.IsOk ? new(value) : this;

    public bool Equals(IResult<TValue, Error>? other)
    {
        if (other is null)
            return false;

        if (this.IsOk != other.IsOk)
            return false;

        return this.IsOk ?
            EqualityComparer<TValue>.Default.Equals(this.Value, other.Value) :
            EqualityComparer<Error>.Default.Equals(this.Error, other.Error);
    }

    public ValueResult<TValue> Or(ValueResult<TValue> other)
        => this.IsOk ? this : other;

    public ValueResult<TValue> Or(Func<ValueResult<TValue>> factory)
        => this.IsOk ? this : factory();

    public ValueResult<TValue> Or(TValue value)
        => this.IsOk ? this : new(value);

    public ValueResult<TValue> Or(Func<TValue, bool> predicate, ValueResult<TValue> other)
        => (this.IsOk && predicate(this.Value)) ? this : other;

    public ValueResult<TValue> Or(Func<TValue, bool> predicate, Func<ValueResult<TValue>> factory)
        => (this.IsOk && predicate(this.Value)) ? this : factory();

    public ValueResult<TOther> Select<TOther>(Func<TValue, TOther> map)
        where TOther : notnull
        => this.IsOk ? new(map(this.Value)) : new(this.error!);

    public ValueResult<TOther> Select<TOther>(Func<TValue, TOther> map, Func<TOther> factory)
        where TOther : notnull
        => this.IsOk ? new(map(this.Value)) : new(factory());

    public ValueResult<TValue> SelectError(Func<Error, Error> map)
        => this.IsOk ? this : new(map(this.Error));

    public ValueResult<TValue, TOtherError> SelectError<TOtherError>(Func<Error, TOtherError> map)
        where TOtherError : notnull
        => this.IsOk ? new(this.value, default!, true) : new(default!, map(this.Error), false);

    public ValueResult<TValue, TOtherError> SelectError<TOtherError>(Func<Error, TOtherError> map, Func<TOtherError> factory)
        where TOtherError : notnull
         => this.IsOk ? new(default!, factory(), false) : new(default!, map(this.Error), false);

    public bool Test(Func<TValue, bool> predicate)
        => this.IsOk && predicate(this.Value);

    public bool TestError(Func<Error, bool> predicate)
        => !this.IsOk && predicate(this.Error);

    public Option<TValue> ToValue()
        => this.IsOk ? new(this.Value) : new();

    public Option<Error> ToError()
        => this.IsOk ? new() : new(this.Error);
}