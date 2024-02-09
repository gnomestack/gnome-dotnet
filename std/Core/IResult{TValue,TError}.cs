namespace Gnome;

public interface IResult<TValue, TError> : IResult
    where TValue : notnull
    where TError : notnull
{
    TValue Value { get; }

    TError Error { get; }

    TValue GetValueOrDefault(TValue defaultValue);

    TValue GetValueOrDefault(Func<TValue> defaultValueFactory);

    TError GetErrorOrDefault(TError defaultError);

    TError GetErrorOrDefault(Func<TError> defaultErrorFactory);
}