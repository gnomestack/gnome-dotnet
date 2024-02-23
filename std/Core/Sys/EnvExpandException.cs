namespace Gnome.Sys;

public class EnvExpandException : SystemException
{
    public EnvExpandException()
        : base()
    {
    }

    public EnvExpandException(string message)
        : base(message)
    {
    }

    public EnvExpandException(string message, Exception inner)
        : base(message, inner)
    {
    }
}