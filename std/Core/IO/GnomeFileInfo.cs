namespace Gnome.IO;

public class GnomeFileInfo : GnomeFileSystemInfo, IFileInfo
{
    private readonly FileInfo fileInfo;

    public GnomeFileInfo(FileInfo fileInfo)
        : base(fileInfo, true)
    {
        this.fileInfo = fileInfo;
        this.Dirname = this.fileInfo.DirectoryName;
    }

    public string Ext => this.fileInfo.Extension;

    public Option<string> Dirname { get; }
}