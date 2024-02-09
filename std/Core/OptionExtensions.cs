namespace Gnome;

public static class OptionExtensions
{
    public static bool IsNullOrWhiteSpace(this Option<string> option)
        => option.Test(x => string.IsNullOrWhiteSpace(x));

    public static bool IsNullOrEmpty(this Option<string> option)
        => option.Test(x => string.IsNullOrEmpty(x));
}