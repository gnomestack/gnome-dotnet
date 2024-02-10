namespace Gnome;

public class Result : IResult<ValueTuple, Error>
{
    private readonly Error error;

    private ValueTuple value;

    public Result()
    {
        this.IsOk = true;
        this.value = default!;
        this.error = default!;
    }

    public Result(Error error)
    {
        this.value = default!;
        this.error = error;
        this.IsOk = false;
    }

    public ValueTuple Value => this.IsOk ? this.value : throw new InvalidOperationException("Result does not have a value.");

    public Error Error => this.IsOk ? throw new InvalidOperationException("Result does not have an error.") : this.error!;

    public bool IsOk { get; private set; }

    public static implicit operator Result(Error error)
        => new(error);

    public static implicit operator Result(Exception exception)
        => new(Error.Convert(exception));

    public static implicit operator Result(ValueTuple value)
        => new();

    public static implicit operator Result(ValueResult<ValueTuple, Error> result)
        => result.IsOk ? new() : new(result.Error);

    public static implicit operator Result(Result<ValueTuple> result)
        => result.IsOk ? new() : new(result.Error);

    public static implicit operator Result<ValueTuple>(Result result)
        => result.IsOk ? new(result.Value) : new(result.Error);

    public static implicit operator ValueResult<ValueTuple, Error>(Result result)
        => result.IsOk ? new(result.Value, default!, true) : new(default!, result.Error, false);

    public static implicit operator ValueTuple(Result result)
        => result.Value;

    public static implicit operator Error(Result result)
        => result.Error;

    public static Result Ok()
        => new();

    public static Result<TValue> Ok<TValue>(TValue value)
        where TValue : notnull
        => new(value);

    public static ValueResult<TValue, TError> Ok<TValue, TError>(TValue value)
        where TValue : notnull
        where TError : notnull
        => new(value, default!, true);

    public static Result Fail(Error error)
        => new(error);

    public static Result Fail(Exception error)
        => new(Error.Convert(error));

    public static Result<TValue> Fail<TValue>(Error error)
        where TValue : notnull
        => new(error);

    public static Result<TValue> Fail<TValue>(Exception exception)
        where TValue : notnull
        => new(Error.Convert(exception));

    public static ValueResult<TValue, TError> Fail<TValue, TError>(TError error)
        where TValue : notnull
        where TError : notnull
        => new(default!, error, false);

    public Result And(Result other)
        => this.IsOk ? other : this;

    public Result And(Func<Result> factory)
        => this.IsOk ? factory() : this;

    public ValueTuple GetValueOrDefault(ValueTuple defaultValue)
        => this.IsOk ? this.value : defaultValue;

    public ValueTuple GetValueOrDefault(Func<ValueTuple> defaultValueFactory)
        => this.IsOk ? this.value : defaultValueFactory();

    public Error GetErrorOrDefault(Error defaultError)
        => this.IsOk ? defaultError : this.error!;

    public Error GetErrorOrDefault(Func<Error> defaultErrorFactory)
        => this.IsOk ? defaultErrorFactory() : this.error!;

    public Result<TValue> Select<TValue>(Func<TValue> selector)
        where TValue : notnull
        => this.IsOk ? new(selector()) : new(this.error);

    public Result<TValue> Select<TValue>(Func<TValue> selector, Func<TValue> defaultValueFactory)
        where TValue : notnull
        => this.IsOk ? new(selector()) : new(defaultValueFactory());

    public Result<TValue> Select<TValue>(TValue value)
        where TValue : notnull
        => new(value);

    public ValueResult<TValue, TError> Select<TValue, TError>(Func<TValue> map, Func<Error, TError> errorMap)
        where TValue : notnull
        where TError : notnull
        => this.IsOk ? new(map(), default!, true) : new(default!, errorMap(this.error), false);

    public ValueResult<TValue, TError> Select<TValue, TError>(Func<bool, Error?, ValueResult<TValue, TError>> map)
        where TValue : notnull
        where TError : notnull
        => map(this.IsOk, this.error);

    public Result SelectError(Func<Error, Error> map)
    {
        if (this.IsOk)
            return this;

        return new(map(this.error));
    }
}