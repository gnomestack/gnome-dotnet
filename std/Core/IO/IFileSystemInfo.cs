namespace Gnome.IO;

public interface IFileSystemInfo
{
    string Name { get; }

    bool IsDir { get; }

    bool IsFile { get; }

    bool IsSymLink { get; }

    FileAttributes Attributes { get; }

    bool Exists { get; }

    Option<string> ParentName { get; }

    string Path { get; }

    /// <summary>
    /// Gets the birthtime on linux/mac and creation time on windows.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the atime on linux/mac and last access time on windows.
    /// </summary>
    DateTime AccessedAt { get; }

    /// <summary>
    /// Gets the mtime on linux/mac and last write time on windows.
    /// </summary>
    DateTime ModifiedAt { get; }

    /// <summary>
    /// Gets file mode on linux/mac and null on windows.
    /// </summary>
    UnixMode? Mode { get; }
}