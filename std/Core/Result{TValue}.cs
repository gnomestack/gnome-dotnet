using System.Diagnostics.CodeAnalysis;

namespace Gnome;

public class Result<TValue> : IResult<TValue, Error>
    where TValue : notnull
{
    private TValue? value;

    private Error? error;

    public Result(TValue value)
    {
        this.value = value;
        this.IsOk = true;
    }

    public Result(Error error)
    {
        this.error = error;
        this.IsOk = false;
    }

    public bool IsOk { get; private set; }

    public TValue Value
        => this.value ?? throw new InvalidOperationException("Result does not have a value.");

    public Error Error
        => this.error ?? throw new InvalidOperationException("Result does not have an error.");

    public static implicit operator Result<TValue>(Error error)
        => new(error);

    public static implicit operator Result<TValue>(TValue value)
        => new(value);

    public static implicit operator Result<TValue>(ValueResult<TValue, Error> result)
        => result.IsOk ? new(result.Value) : new(result.Error);

    public static implicit operator TValue(Result<TValue> result)
        => result.Value;

    public static implicit operator Error(Result<TValue> result)
        => result.Error;

    public static implicit operator ValueResult<TValue, Error>(Result<TValue> result)
        => result.IsOk ? new(result.Value, default!, true) : new(default!, result.Error, false);

    public static implicit operator Task<Result<TValue>>(Result<TValue> result)
        => Task.FromResult(result);

    public static implicit operator ValueTask<Result<TValue>>(Result<TValue> result)
        => new(result);

    public static Result<TValue> Ok(TValue value)
        => new(value);

    public static Result<TValue> Fail(Error error)
        => new(error);

    public static Result<TValue> Fail(Exception exception)
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

    public Result<TValue> Inspect(Action<TValue> action)
    {
        if (this.IsOk)
            action(this.Value);

        return this;
    }

    public Result<TValue> InspectError(Action<Error> action)
    {
        if (!this.IsOk)
            action(this.Error);

        return this;
    }

    public Result<TValue> And(Result<TValue> other)
        => this.IsOk ? other : this;

    public Result<TValue> And(Func<Result<TValue>> factory)
        => this.IsOk ? factory() : this;

    public Result<TValue> And(TValue value)
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

    public Result<TValue> Or(Result<TValue> other)
        => this.IsOk ? this : other;

    public Result<TValue> Or(Func<Result<TValue>> factory)
        => this.IsOk ? this : factory();

    public Result<TValue> Or(TValue value)
        => this.IsOk ? this : new(value);

    public Result<TValue> Or(Func<TValue, bool> predicate, Result<TValue> other)
        => (this.IsOk && predicate(this.Value)) ? this : other;

    public Result<TValue> Or(Func<TValue, bool> predicate, Func<Result<TValue>> factory)
        => (this.IsOk && predicate(this.Value)) ? this : factory();

    public Result<TValue> Replace(TValue value)
    {
        this.value = value;
        this.IsOk = true;
        return this;
    }

    public Result<TValue> ReplaceError(Error error)
    {
        this.error = error;
        this.IsOk = false;
        return this;
    }

    public Result<TOther> Select<TOther>(Func<TValue, TOther> map)
        where TOther : notnull
        => this.IsOk ? new(map(this.Value)) : new(this.Error);

    public Result<TOther> Select<TOther>(Func<TValue, TOther> map, Func<TOther> factory)
        where TOther : notnull
        => this.IsOk ? new(map(this.Value)) : new(factory());

    public ValueResult<TOther, TOtherError> Select<TOther, TOtherError>(Func<bool, TValue?, Error?, ValueResult<TOther, TOtherError>> map)
        where TOther : notnull
        where TOtherError : notnull
        => map(this.IsOk, this.value, this.error!);

    public ValueResult<TOther, TOtherError> Select<TOther, TOtherError>(Func<TValue, TOther> map, Func<Error, TOtherError> mapError)
        where TOther : notnull
        where TOtherError : notnull
        => this.IsOk ? new(map(this.Value), default!, true) : new(default!, mapError(this.Error), false);

    public Result<TValue> SelectError<TOtherError>(Func<Error, TOtherError> map)
        where TOtherError : notnull, Error
        => this.IsOk ? new(this.value!) : new(map(this.Error));

    public Result<TValue> SelectError<TOtherError>(Func<Error, TOtherError> map, Func<TOtherError> factory)
        where TOtherError : notnull, Error
        => this.IsOk ? new(factory()) : new(map(this.Error));

    public bool Test(Func<TValue, bool> predicate)
        => this.IsOk && predicate(this.Value);

    public bool TestError(Func<Error, bool> predicate)
        => !this.IsOk && predicate(this.Error);

    public Option<TValue> ToOption()
        => this.IsOk ? new(this.Value) : new();

    public Option<Error> ToOptionError()
        => this.IsOk ? new() : new(this.Error);
}