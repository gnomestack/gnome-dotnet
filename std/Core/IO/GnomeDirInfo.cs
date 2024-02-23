namespace Gnome.IO;

public class GnomeDirInfo : GnomeFileSystemInfo, IDirInfo
{
    private readonly DirectoryInfo directoryInfo;

    public GnomeDirInfo(DirectoryInfo directoryInfo)
        : base(directoryInfo, false)
    {
        this.directoryInfo = directoryInfo;
    }
}