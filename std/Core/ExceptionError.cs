using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Gnome;

public class ExceptionError : Error
{
    private readonly Exception? ex;

    public ExceptionError(string message, IInnerError? inner = null, [CallerMemberName] string target = "")
        : base(message, inner, target)
    {
        this.ex = null;
    }

    public ExceptionError(Exception ex, [CallerMemberName] string target = "")
        : base(ex.Message, ex.InnerException is null ? null : new ExceptionError(ex.InnerException), target)
    {
        this.ex = ex;
        var e = ex.GetType().Name;

        // Remove the "Exception" suffix
        this.Code = e.Substring(0, e.Length - 9);
    }

    public override string? Target
    {
        get
        {
            if (base.Target is not null)
                return base.Target;

            base.Target = string.Empty;
#if NETLEGACY
            try
            {
                var target = string.Empty;

                if (this.ex?.TargetSite is null)
                {
                    base.Target = string.Empty;
                    return base.Target;
                }

                if (this.ex.TargetSite?.DeclaringType?.FullName is not null)
                    target = this.ex.TargetSite.DeclaringType.FullName + ".";

                if (this.ex.TargetSite?.Name is not null)
                    target += this.ex.TargetSite.Name;

                base.Target = target;
            }
            catch
            {
                base.Target = string.Empty;
            }
#endif
            return base.Target;
        }

        protected set => base.Target = value;
    }

    public override string? StackTrace
    {
        get
        {
            if (base.StackTrace is not null)
                return base.StackTrace;

            if (this.ex is null)
                return null;

            base.StackTrace = this.ex.StackTrace;
            return this.ex.StackTrace;
        }

        protected set => base.StackTrace = value;
    }

    protected Exception? Exception => this.ex;

    public override Exception ToException()
    {
        return this.ex ?? new InvalidOperationException(this.Message);
    }

    public override string ToString()
    {
        return this.ex?.ToString() ?? base.ToString();
    }
}