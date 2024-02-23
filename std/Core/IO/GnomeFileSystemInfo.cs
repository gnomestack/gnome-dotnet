using Microsoft.VisualBasic;

namespace Gnome.IO;

public class GnomeFileSystemInfo : IFileSystemInfo
{
    private readonly FileSystemInfo fsInfo;
    private Lazy<string?> s_parentName;

    public GnomeFileSystemInfo(FileSystemInfo fsInfo, bool? isFile = null)
    {
        this.fsInfo = fsInfo;

        isFile ??= fsInfo is FileInfo;
        this.IsDir = !isFile.Value;
        this.IsFile = isFile.Value;
        this.s_parentName = new(() => System.IO.Path.GetDirectoryName(fsInfo.FullName));
    }

    public string Name => this.fsInfo.Name;

    public bool IsDir { get; }

    public bool IsFile { get; }

#if NETLEGACY
    public bool IsSymLink => false;
#else
    public bool IsSymLink => this.fsInfo.LinkTarget is not null;
#endif

    public FileAttributes Attributes => this.fsInfo.Attributes;

    public bool Exists => this.fsInfo.Exists;

    public Option<string> ParentName => this.s_parentName.Value;

    public string Path => this.fsInfo.FullName;

    public DateTime CreatedAt => this.fsInfo.CreationTime;

    public DateTime AccessedAt => this.fsInfo.LastAccessTime;

    public DateTime ModifiedAt => this.fsInfo.LastWriteTime;

#if NETLEGACY
    public UnixMode? Mode => null;
#else
    public UnixMode? Mode => (UnixMode)this.fsInfo.UnixFileMode;
#endif

    public void Refresh()
    {
        this.fsInfo.Refresh();
        this.s_parentName = new Lazy<string?>(() => System.IO.Path.GetDirectoryName(this.fsInfo.FullName));
    }
}