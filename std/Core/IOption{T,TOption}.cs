namespace Gnome;

public interface IOption<T, TOption> : IOption<T>
    where T : notnull
    where TOption : IOption<T, TOption>, new()
{
#if !NETLEGACY
    static abstract TOption From(T? value);
#endif
}