namespace Gnome;

public readonly struct ValueResult : IResult<ValueTuple, Error>
{
    private readonly ValueTuple value;

    private readonly Error error;

    public ValueResult()
    {
        this.value = default;
        this.error = default!;
        this.IsOk = true;
    }

    public ValueResult(Error error)
    {
        this.value = default;
        this.error = error;
        this.IsOk = false;
    }

    public static ValueResult Default { get; } = default;

    public ValueTuple Value => this.IsOk ? this.value : throw new InvalidOperationException("Result does not have a value.");

    public Error Error => this.IsOk ? throw new InvalidOperationException("Result does not have an error.") : this.error!;

    public bool IsOk { get; }

    public static implicit operator ValueResult(Error error)
        => new(error);

    public static implicit operator ValueResult(ValueTuple value)
        => new();

    public static implicit operator ValueResult(ValueResult<ValueTuple, Error> result)
        => result.IsOk ? new() : new(result.Error);

    public static implicit operator ValueResult(Result<ValueTuple> result)
        => result.IsOk ? new() : new(result.Error);

    public static ValueResult Ok()
        => Default;

    public static ValueResult<TValue> Ok<TValue>(TValue value)
        where TValue : notnull
        => new(value);

    public static ValueResult<TValue, TError> Ok<TValue, TError>(TValue value)
        where TValue : notnull
        where TError : notnull
        => new(value, default!, true);

    public static ValueResult Fail(Error error)
        => new(error);

    public static ValueResult Fail(Exception exception)
        => new(Error.Convert(exception));

    public static ValueResult<TValue> Fail<TValue>(Error error)
        where TValue : notnull
        => new(error);

    public static ValueResult<TValue> Fail<TValue>(Exception exception)
        where TValue : notnull
        => new(Error.Convert(exception));

    public static ValueResult<TValue, TError> Fail<TValue, TError>(TError error)
        where TValue : notnull
        where TError : notnull
        => new(default!, error, false);

    public Error GetErrorOrDefault(Error defaultError)
        => this.IsOk ? defaultError : this.error;

    public Error GetErrorOrDefault(Func<Error> defaultErrorFactory)
        => this.IsOk ? defaultErrorFactory() : this.error;

    public ValueTuple GetValueOrDefault(ValueTuple defaultValue)
        => this.IsOk ? defaultValue : this.value;

    public ValueTuple GetValueOrDefault(Func<ValueTuple> defaultValueFactory)
        => this.IsOk ? defaultValueFactory() : this.value;
}