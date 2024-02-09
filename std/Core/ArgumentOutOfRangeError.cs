namespace Gnome;

public class ArgumentOutOfRangeError : ArgumentError
{
    public ArgumentOutOfRangeError(string paramName)
        : base($"Argument {paramName} is out of range.")
    {
        this.ParamName = paramName;
    }

    public ArgumentOutOfRangeError(string paramName, object? value)
        : base(paramName)
    {
        this.ActualValue = value;
    }

    public ArgumentOutOfRangeError(string paramName, string message)
        : base(paramName, message)
    {
    }

    public ArgumentOutOfRangeError(string paramName, object? value, string message)
        : base(paramName, message)
    {
        this.ActualValue = value;
    }

    public ArgumentOutOfRangeError(string message, string paramName, IInnerError inner)
        : base(message, paramName, inner)
    {
    }

    public ArgumentOutOfRangeError(ArgumentOutOfRangeException ex)
        : base(ex)
    {
        this.ActualValue = ex.ActualValue;
    }

    public object? ActualValue { get; set; }

    public static implicit operator ArgumentOutOfRangeError(ArgumentOutOfRangeException ex)
        => new(ex);

    public override Exception ToException()
    {
        return this.Exception ?? new ArgumentOutOfRangeException(this.ParamName, this.ActualValue, this.Message);
    }
}