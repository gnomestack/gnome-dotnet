using System.Diagnostics.CodeAnalysis;

namespace Gnome;

public class Result<TValue, TError> : IResult<TValue, TError>
    where TValue : notnull
    where TError : notnull
{
    private TValue? value;

    private TError? error;

    public Result(TValue? value = default, TError? error = default, bool ok = false)
    {
        this.value = value;
        this.error = error;
        this.IsOk = true;
    }

    public bool IsOk { get; private set; }

    public TValue Value
        => this.value ?? throw new InvalidOperationException("Result does not have a value.");

    public TError Error
        => this.error ?? throw new InvalidOperationException("Result does not have an error.");

    public static implicit operator Result<TValue, TError>(TValue? value)
        => new(value, default!, true);

    public static implicit operator Result<TValue, TError>(TError? error)
        => new(default!, error, false);

    public static implicit operator Result<TValue, TError>((TValue?, TError?, bool) result)
        => new(result.Item1, result.Item2, result.Item3);

    public static implicit operator TValue(Result<TValue, TError> result)
        => result.Value;

    public static implicit operator TError(Result<TValue, TError> result)
        => result.Error;

    public static implicit operator Task<Result<TValue, TError>>(Result<TValue, TError> result)
        => Task.FromResult(result);

    public static implicit operator ValueTask<Result<TValue, TError>>(Result<TValue, TError> result)
        => new(result);

    public static Result<TValue, TError> Ok(TValue value)
        => new(value, default!, true);

    public static Result<TValue, TError> Fail(TError error)
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

    public Result<TValue, TError> Inspect(Action<TValue> action)
    {
        if (this.IsOk)
            action(this.Value);

        return this;
    }

    public Result<TValue, TError> InspectError(Action<TError> action)
    {
        if (!this.IsOk)
            action(this.Error);

        return this;
    }

    public Result<TValue, TError> And(Result<TValue, TError> other)
        => this.IsOk ? other : this;

    public Result<TValue, TError> And(Func<Result<TValue, TError>> factory)
        => this.IsOk ? factory() : this;

    public Result<TValue, TError> And(TValue value)
        => this.IsOk ? new(value, default!, true) : this;

    public bool Equals(IResult<TValue, TError>? other)
    {
        if (other is null)
            return false;

        if (this.IsOk != other.IsOk)
            return false;

        return this.IsOk ? EqualityComparer<TValue>.Default.Equals(this.Value, other.Value) : EqualityComparer<TError>.Default.Equals(this.Error, other.Error);
    }

    public Result<TValue, TError> Or(Result<TValue, TError> other)
        => this.IsOk ? this : other;

    public Result<TValue, TError> Or(Func<Result<TValue, TError>> factory)
        => this.IsOk ? this : factory();

    public Result<TValue, TError> Or(TValue value)
        => this.IsOk ? this : new(value, default!, true);

    public Result<TValue, TError> Or(Func<TValue, bool> predicate, Result<TValue, TError> other)
        => (this.IsOk && predicate(this.Value)) ? this : other;

    public Result<TValue, TError> Or(Func<TValue, bool> predicate, Func<Result<TValue, TError>> factory)
        => (this.IsOk && predicate(this.Value)) ? this : factory();

    public Result<TValue, TError> Replace(TValue value)
    {
        this.value = value;
        this.IsOk = true;
        return this;
    }

    public Result<TValue, TError> ReplaceError(TError error)
    {
        this.error = error;
        this.IsOk = false;
        return this;
    }

    public Result<TOther, TError> Select<TOther>(Func<TValue, TOther> map)
        where TOther : notnull
        => this.IsOk ? new(map(this.Value), default!, true) : new(default!, this.Error, false);

    public Result<TOther, TError> Select<TOther>(Func<TValue, TOther> map, Func<TOther> factory)
        where TOther : notnull
        => this.IsOk ? new(map(this.Value), default!, true) : new(factory(), default!, true);

    public Result<TValue, TOtherError> SelectError<TOtherError>(Func<TError, TOtherError> map)
        where TOtherError : notnull
        => this.IsOk ? this.value! : map(this.error!);

    public Result<TValue, TOtherError> SelectError<TOtherError>(Func<TError, TOtherError> map, Func<TOtherError> factory)
        where TOtherError : notnull
         => this.IsOk ? new(error: factory()) : new(error: map(this.Error));

    public bool Test(Func<TValue, bool> predicate)
        => this.IsOk && predicate(this.Value);

    public bool TestError(Func<TError, bool> predicate)
        => !this.IsOk && predicate(this.Error);

    public Option<TValue> ToValue()
        => this.IsOk ? new(this.Value) : new();

    public Option<TError> ToError()
        => this.IsOk ? new() : new(this.Error);
}