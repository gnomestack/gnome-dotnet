namespace Gnome;

public class ArgumentNullError : ArgumentError
{
    public ArgumentNullError(string paramName)
        : base(paramName, $"Argument {paramName} is null.")
    {
        this.ParamName = paramName;
    }

    public ArgumentNullError(string paramName, string message)
        : base(paramName, message)
    {
    }

    public ArgumentNullError(string paramName, string message, IInnerError inner)
        : base(paramName, message, inner)
    {
    }

    public ArgumentNullError(ArgumentNullException ex)
        : base(ex)
    {
    }

    public static implicit operator ArgumentNullError(ArgumentNullException ex)
        => new(ex);

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new ArgumentNullException(this.ParamName, this.Message);
    }
}