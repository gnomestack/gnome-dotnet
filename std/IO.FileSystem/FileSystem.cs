using System.Runtime.Versioning;
using System.Text;

using Gnome.Sys;

using Microsoft.Win32.SafeHandles;

namespace Gnome;

public static partial class FileSystem
{
    [UnsupportedOSPlatform("windows")]
    public static Result SetFileOwner(string path, int owner, int group)
    {
        try
        {
            if (OS.IsWindows())
                return new PlatformNotSupportedException("Setting file owner with unix ids is not supported on Windows.");

            Interop.Sys.ChOwn(path, owner, group);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    [UnsupportedOSPlatform("windows")]
    public static Result<DirectoryInfo> MakeDirectory(string path, UnixFileMode mode)
    {
        try
        {
#if NET7_0_OR_GREATER
            return Directory.CreateDirectory(path, mode);
#else
            var di = Directory.CreateDirectory(path);
            Interop.Sys.ChMod(path, (int)mode);
            return di;
#endif
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<UnixFileMode> GetFileMode(string path)
    {
        try
        {
#if NET7_0_OR_GREATER
            return File.GetUnixFileMode(path);
#else
            var fs = default(Interop.Sys.FileStatus);
            Interop.Sys.LStat(path, out fs);
            return (UnixFileMode)fs.Mode;
#endif
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<UnixFileMode> GetFileMode(SafeFileHandle fileHandle)
    {
        try
        {
            if (OS.IsWindows())
                return new PlatformNotSupportedException("Getting file mode with unix mode is not supported on Windows.");

#if NET7_0_OR_GREATER
            return File.GetUnixFileMode(fileHandle);
#else
            var fs = default(Interop.Sys.FileStatus);
            Interop.Sys.FStat(fileHandle, out fs);
            return (UnixFileMode)fs.Mode;
#endif
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result ChangeFileMode(string path, UnixFileMode mode)
    {
        try
        {
#if NET7_0_OR_GREATER
            File.SetUnixFileMode(path, mode);
            return Result.Ok();
#else
            Interop.Sys.ChMod(Path.GetFullPath(path), (int)mode);
            return Result.Ok();
#endif
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result RemoveFile(string path)
    {
        try
        {
            File.Delete(path);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result RemoveDirectory(string path)
    {
        try
        {
            Directory.Delete(path);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result EnsureDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                return Result.Ok();
            }

            Directory.CreateDirectory(path);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<StreamReader> OpenFileReader(string path)
    {
        try
        {
            return File.OpenText(path);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<StreamWriter> OpenFileWriter(string path)
    {
        try
        {
            return File.CreateText(path);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<FileStream> OpenFileStream(
        string path,
        FileMode mode)
    {
        try
        {
            return File.Open(path, mode);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<FileStream> OpenFileStream(
        string path,
        FileMode mode,
        FileAccess access)
    {
        try
        {
            return File.Open(path, mode, access);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<FileStream> OpenFileStream(
        string path,
        FileMode mode,
        FileAccess access,
        FileShare share)
    {
        try
        {
            return File.Open(path, mode, access, share);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<FileStream> OpenReadFileStream(string path)
    {
        try
        {
            return File.OpenRead(path);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<FileStream> OpenWriteFileStream(string path)
    {
        try
        {
            return File.OpenWrite(path);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<byte[]> ReadFileBytes(string path)
    {
        try
        {
            return File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<string> ReadFileText(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<string> ReadFileText(string path, Encoding encoding)
    {
        try
        {
            return File.ReadAllText(path, encoding);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<string[]> ReadFileLines(string path)
    {
        try
        {
            return File.ReadAllLines(path);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<string[]> ReadFileLines(string path, Encoding encoding)
    {
        try
        {
            return File.ReadAllLines(path, encoding);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result WriteFileText(string path, string contents)
    {
        try
        {
            File.WriteAllText(path, contents);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }
}