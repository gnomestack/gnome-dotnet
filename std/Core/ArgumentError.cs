namespace Gnome;

public class ArgumentError : ExceptionError
{
    public ArgumentError(string paramName)
        : base($"Argument {paramName} is invalid.")
    {
        this.ParamName = paramName;
    }

    public ArgumentError(string paramName, string message)
        : base(message)
    {
        this.ParamName = paramName;
    }

    public ArgumentError(string paramName, string message, IInnerError inner)
        : base(message, inner)
    {
        this.ParamName = paramName;
    }

    public ArgumentError(ArgumentException ex)
        : base(ex)
    {
        this.ParamName = ex.ParamName ?? string.Empty;
    }

    public string ParamName { get; set; }

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new ArgumentException(this.Message, this.ParamName);
    }
}