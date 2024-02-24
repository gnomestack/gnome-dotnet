using System.Diagnostics.CodeAnalysis;

namespace Gnome;

public readonly struct ValueResult<TValue, TError> : IResult<TValue, TError>
    where TValue : notnull
    where TError : notnull
{
    private readonly TValue? value;

    private readonly TError? error;

    public ValueResult(TValue? value, TError? error, bool ok)
    {
        this.value = value;
        this.error = error;
        this.IsOk = true;
    }

    public bool IsOk { get; }

    public bool IsError
        => !this.IsOk;

    public TValue Value
        => this.value ?? throw new InvalidOperationException("Result does not have a value.");

    public TError Error
        => this.error ?? throw new InvalidOperationException("Result does not have an error.");

    public static implicit operator ValueResult<TValue, TError>(TValue? value)
        => new(value, default!, true);

    public static implicit operator ValueResult<TValue, TError>(TError? error)
        => new(default!, error, false);

    public static implicit operator ValueResult<TValue, TError>((TValue?, TError?, bool) result)
        => new(result.Item1, result.Item2, result.Item3);

    public static implicit operator TValue(ValueResult<TValue, TError> result)
        => result.Value;

    public static implicit operator TError(ValueResult<TValue, TError> result)
        => result.Error;

    public static implicit operator Task<ValueResult<TValue, TError>>(ValueResult<TValue, TError> result)
        => Task.FromResult(result);

    public static implicit operator ValueTask<ValueResult<TValue, TError>>(ValueResult<TValue, TError> result)
        => result;

    public static ValueResult<TValue, TError> Ok(TValue value)
        => new(value, default!, true);

    public static ValueResult<TValue, TError> Fail(TError error)
        => new(default!, error, false);

    public void Deconstruct([MaybeNull] out TValue value, out TError error, out bool isOk)
    {
        value = this.value!;
        error = this.error!;
        isOk = this.IsOk;
    }

    public TError GetErrorOrDefault(TError defaultError)
        => this.IsOk ? defaultError : this.error!;

    public TError GetErrorOrDefault(Func<TError> defaultErrorFactory)
        => this.IsOk ? defaultErrorFactory() : this.error!;

    public TValue GetValueOrDefault(TValue defaultValue)
        => this.IsOk ? this.value! : defaultValue;

    public TValue GetValueOrDefault(Func<TValue> defaultValueFactory)
        => this.IsOk ? this.value! : defaultValueFactory();

    public ValueResult<TValue, TError> Inspect(Action<TValue> action)
    {
        if (this.IsOk)
            action(this.Value);

        return this;
    }

    public ValueResult<TValue, TError> InspectError(Action<TError> action)
    {
        if (!this.IsOk)
            action(this.Error);

        return this;
    }

    public ValueResult<TValue, TError> And(ValueResult<TValue, TError> other)
        => this.IsOk ? other : this;

    public ValueResult<TValue, TError> And(Func<ValueResult<TValue, TError>> factory)
        => this.IsOk ? factory() : this;

    public ValueResult<TValue, TError> And(TValue value)
        => this.IsOk ? new(value, default!, true) : this;

    public bool Equals(IResult<TValue, TError>? other)
    {
        if (other is null)
            return false;

        if (this.IsOk != other.IsOk)
            return false;

        return this.IsOk ? EqualityComparer<TValue>.Default.Equals(this.Value, other.Value) : EqualityComparer<TError>.Default.Equals(this.Error, other.Error);
    }

    public ValueResult<TValue, TError> Or(ValueResult<TValue, TError> other)
        => this.IsOk ? this : other;

    public ValueResult<TValue, TError> Or(Func<ValueResult<TValue, TError>> factory)
        => this.IsOk ? this : factory();

    public ValueResult<TValue, TError> Or(TValue value)
        => this.IsOk ? this : new(value, default!, true);

    public ValueResult<TValue, TError> Or(Func<TValue, bool> predicate, ValueResult<TValue, TError> other)
        => (this.IsOk && predicate(this.Value)) ? this : other;

    public ValueResult<TValue, TError> Or(Func<TValue, bool> predicate, Func<ValueResult<TValue, TError>> factory)
        => (this.IsOk && predicate(this.Value)) ? this : factory();

    public ValueResult<TOther, TError> Select<TOther>(Func<TValue, TOther> map)
        where TOther : notnull
        => this.IsOk ? new(map(this.Value), default!, true) : new(default!, this.Error, false);

    public ValueResult<TOther, TError> Select<TOther>(Func<TValue, TOther> map, Func<TOther> factory)
        where TOther : notnull
        => this.IsOk ? new(map(this.Value), default!, true) : new(factory(), default!, true);

    public ValueResult<TValue, TOtherError> SelectError<TOtherError>(Func<TError, TOtherError> map)
        where TOtherError : notnull
        => this.IsOk ? new(this.value, default!, true) : new(default!, map(this.Error), false);

    public ValueResult<TValue, TOtherError> SelectError<TOtherError>(Func<TError, TOtherError> map, Func<TOtherError> factory)
        where TOtherError : notnull
         => this.IsOk ? new(default!, factory(), false) : new(default!, map(this.Error), false);

    public bool Test(Func<TValue, bool> predicate)
        => this.IsOk && predicate(this.Value);

    public bool TestError(Func<TError, bool> predicate)
        => !this.IsOk && predicate(this.Error);

    public Option<TValue> ToValue()
        => this.IsOk ? new(this.Value) : new();

    public Option<TError> ToError()
        => this.IsOk ? new() : new(this.Error);
}