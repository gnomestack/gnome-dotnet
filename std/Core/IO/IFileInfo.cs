namespace Gnome.IO;

public interface IFileInfo : IFileSystemInfo
{
    string Ext { get; }

    Option<string> Dirname { get; }
}