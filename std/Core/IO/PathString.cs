namespace Gnome.IO;

public class PathString
{
    private readonly string pathValue;

    public PathString(string pathValue)
    {
        this.pathValue = pathValue;
    }

    public static implicit operator string(PathString pathString)
    {
        return pathString.pathValue;
    }

    public static implicit operator PathString(string pathValue)
    {
        return new PathString(pathValue);
    }

    public override string ToString()
        => this.pathValue;
}