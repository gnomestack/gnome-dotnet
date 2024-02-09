namespace Gnome;

public class AggregateError : ExceptionError
{
    public AggregateError(string message, params Error[] errors)
        : base(message)
    {
        this.Code = "AggregateError";
        this.InnerErrors = errors;
    }

    public AggregateError(params Error[] errors)
        : base("Aggregate error")
    {
        this.Code = "AggregateError";
        this.InnerErrors = errors;
    }

    public AggregateError(params Exception[] exceptions)
        : base(new AggregateException(exceptions))
    {
        this.Code = "AggregateError";
        this.InnerErrors = exceptions.Select(Convert).ToArray();
    }

    public AggregateError(AggregateException ex)
        : base(ex)
    {
        this.Code = "AggregateError";
        this.InnerErrors = ex.InnerExceptions.Select(Convert).ToArray();
    }

    public Error[] InnerErrors { get; set; }

    public static implicit operator AggregateError(AggregateException ex)
        => new(ex);

    public override Exception ToException()
    {
        if (this.Exception is not null)
            return this.Exception;

        return new AggregateException(this.InnerErrors.Select(e => e.ToException()));
    }
}