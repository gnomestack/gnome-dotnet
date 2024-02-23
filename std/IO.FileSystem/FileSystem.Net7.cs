#if NET7_0_OR_GREATER
using System.Runtime.Versioning;

using Gnome.Sys;

using Microsoft.Win32.SafeHandles;

namespace Gnome;

public static partial class FileSystem
{
    [UnsupportedOSPlatform("windows")]
    public static Result ChangeFileMode(SafeFileHandle fileHandle, UnixFileMode mode)
    {
        try
        {
            if (Platform.IsWindows())
                return new PlatformNotSupportedException("Changing file mode with unix mode is not supported on Windows.");

            File.SetUnixFileMode(fileHandle, mode);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }
}
#endif