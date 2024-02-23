using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;

using Gnome.Sys;

namespace Gnome.IO;

public static partial class FileSystem
{
    [UnsupportedOSPlatform("windows")]
    [UnsupportedOSPlatform("browser")]
    public static void ChangeOwner(string path, int userId, int groupId)
    {
        if (Platform.IsWindows())
            throw new PlatformNotSupportedException("Chown is not supported on Windows.");

        Interop.Sys.ChOwn(path, userId, groupId);
    }

    public static Result ChangeOwnerAsResult(string path, int userId, int groupId)
    {
        try
        {
            if (Platform.IsWindows())
                return new PlatformNotSupportedException("Chown is not supported on Windows.");

            ChangeOwner(path, userId, groupId);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }

#if NET7_0_OR_GREATER
    [UnsupportedOSPlatform("windows")]
    [UnsupportedOSPlatform("browser")]
    public static void ChangeMode(string path, UnixFileMode mode)
    {
        if (Platform.IsWindows())
            throw new PlatformNotSupportedException("Chown is not supported on Windows.");

        Interop.Sys.ChMod(path, (int)mode);
    }
#endif

    [UnsupportedOSPlatform("windows")]
    [UnsupportedOSPlatform("browser")]
    public static void ChangeMode(string path, UnixMode mode)
    {
        if (Platform.IsWindows())
            throw new PlatformNotSupportedException("Chown is not supported on Windows.");

        Interop.Sys.ChMod(path, (int)mode);
    }

    [UnsupportedOSPlatform("windows")]
    [UnsupportedOSPlatform("browser")]
    public static Result ChangeModeAsResult(string path, UnixMode mode)
    {
        try
        {
            if (Platform.IsWindows())
                return new PlatformNotSupportedException("Chown is not supported on Windows.");

            ChangeMode(path, mode);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static void CopyDir(string sourceDir, string destinationDir, bool recursive, bool overwrite = false)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        EnsureDir(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            if (FileExists(targetFilePath))
            {
                if (overwrite)
                    File.Delete(targetFilePath);
                else
                    throw new IOException($"File already exists: {targetFilePath}");
            }

            file.CopyTo(targetFilePath);
        }

        if (!recursive)
        {
            return;
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDir(subDir.FullName, newDestinationDir, true, overwrite);
        }
    }

    public static Result CopyDirAsResult(
        string sourceDir,
        string destinationDir,
        bool recursive,
        bool overwrite = false)
    {
        try
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                return new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            EnsureDir(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                if (FileExists(targetFilePath))
                {
                    if (overwrite)
                        File.Delete(targetFilePath);
                    else
                        return new IOException($"File already exists: {targetFilePath}");
                }

                file.CopyTo(targetFilePath);
            }

            if (!recursive)
            {
                return Result.Ok();
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                var r = CopyDirAsResult(subDir.FullName, newDestinationDir, true, overwrite);
                if (!r.IsOk)
                    return r;
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static DirectoryInfo EnsureDir(string path)
    {
        var di = new DirectoryInfo(path);
        if (!di.Exists)
        {
            di.Create();
        }

        return di;
    }

    public static Result<DirectoryInfo> EnsureDirAsResult(string path)
    {
        try
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }

            return di;
        }
        catch (Exception e)
        {
            return Result<DirectoryInfo>.Fail(e);
        }
    }

    public static Result<FileStream> EnsureFileAsResult(string path)
    {
        try
        {
            var fi = new FileInfo(path);
            if (!fi.Exists)
            {
                return fi.Create();
            }

            return fi.Open(FileMode.Open);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<FileInfo> EnsureTouchFileAsResult(string path)
    {
        try
        {
            var fi = new FileInfo(path);
            if (!fi.Exists)
            {
                fi.Create().Dispose();
                return fi;
            }

            return fi;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public static bool IsFile(string path)
    {
        var r = GetFileAttrsAsResult(path);
        return r.Test(o => o is not FileAttributes.Directory);
    }

    public static bool IsDir(string path)
    {
        var r = GetFileAttrsAsResult(path);
        return r.Test(o => o is FileAttributes.Directory);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FileAttributes GetFileAttrs(string path)
    {
        return File.GetAttributes(path);
    }

    [Pure]
    public static Result<FileAttributes> GetFileAttrsAsResult(string path)
    {
        try
        {
            return File.GetAttributes(path);
        }
        catch (Exception e)
        {
            return Result<FileAttributes>.Fail(e);
        }
    }

    public static DirectoryInfo MakeDir(string path)
        => Directory.CreateDirectory(path);

    public static Result<DirectoryInfo> MakeDirAsResult(string path)
    {
        try
        {
            return Directory.CreateDirectory(path);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MoveDir(string source, string destination)
        => Directory.Move(source, destination);

    public static Result MoveDirAsResult(string source, string destination)
    {
        try
        {
            Directory.Move(source, destination);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MoveFile(string source, string destination)
        => File.Move(source, destination);

    public static Result MoveFileAsResult(string source, string destination)
    {
        try
        {
            File.Move(source, destination);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FileStream OpenFile(string path)
        => File.OpenRead(path);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FileStream OpenFile(string path, FileMode mode)
        => File.Open(path, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FileStream OpenFile(string path, FileMode mode, FileAccess access)
        => File.Open(path, mode, access);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FileStream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        => File.Open(path, mode, access, share);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<FileStream> OpenFileAsResult(string path)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<FileStream> OpenFileAsResult(string path, FileMode mode)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<FileStream> OpenFileAsResult(string path, FileMode mode, FileAccess access)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<FileStream> OpenFileAsResult(string path, FileMode mode, FileAccess access, FileShare share)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ReadFile(string path)
        => File.ReadAllBytes(path);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string> ReadFileLines(string path)
        => File.ReadLines(path);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] ReadFileLineArray(string path)
        => File.ReadAllLines(path);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadFileText(string path)
        => File.ReadAllText(path);

    [Pure]
    public static IEnumerable<IFileSystemInfo> ReadDir(string path)
    {
        var di = new DirectoryInfo(path);
        return new FileSystemEnumerable(di);
    }

    [Pure]
    public static IEnumerable<IFileSystemInfo> ReadDir(string path, string search)
    {
        var di = new DirectoryInfo(path);
        return new FileSystemEnumerable(di, search);
    }

    [Pure]
    public static IEnumerable<IFileSystemInfo> ReadDir(string path, string search, SearchOption searchOption)
    {
        var di = new DirectoryInfo(path);
        return new FileSystemEnumerable(di, search, searchOption);
    }

    public static Result<IEnumerable<IFileSystemInfo>> ReadDirAsResult(string path)
    {
        try
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                return new DirectoryNotFoundException(path);
            }

            return new Result<IEnumerable<IFileSystemInfo>>(new FileSystemEnumerable(di));
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<IEnumerable<IFileSystemInfo>> ReadDirAsResult(string path, string searchPattern)
    {
        try
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                return new DirectoryNotFoundException(path);
            }

            return new Result<IEnumerable<IFileSystemInfo>>(new FileSystemEnumerable(di, searchPattern));
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Result<IEnumerable<IFileSystemInfo>> ReadDirAsResult(
        string path,
        string searchPattern,
        SearchOption searchOption)
    {
        try
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                return new DirectoryNotFoundException(path);
            }

            return new Result<IEnumerable<IFileSystemInfo>>(new FileSystemEnumerable(di, searchPattern, searchOption));
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static void RemoveFile(string path)
        => File.Delete(path);

    public static Result RemoveFileAsResult(string path)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveDir(string path, bool recursive)
        => Directory.Delete(path, recursive);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveDir(string path)
        => Directory.Delete(path);

    public static Result RemoveDirAsResult(string path)
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

    public static Result RemoveDirAsResult(string path, bool recursive)
    {
        try
        {
            Directory.Delete(path, recursive);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static void WriteFile(string path, byte[] bytes)
        => File.WriteAllBytes(path, bytes);

    public static void WriteFileText(string path, string text)
        => File.WriteAllText(path, text);

    public static void WriteFileText(string path, string text, bool append)
    {
        if (append)
        {
            File.AppendAllText(path, text);
            return;
        }

        File.WriteAllText(path, text);
    }

    public static void WriteFileText(string path, string text, Encoding encoding)
        => File.WriteAllText(path, text, encoding);

    public static void WriteFileText(string path, string text, Encoding encoding, bool append)
    {
        if (append)
        {
            File.AppendAllText(path, text, encoding);
            return;
        }

        File.WriteAllText(path, text, encoding);
    }

    public static void WriteFileLines(string path, IEnumerable<string> lines)
        => File.WriteAllLines(path, lines);

    public static void WriteFileLines(string path, IEnumerable<string> lines, bool append)
    {
        if (append)
        {
            File.AppendAllLines(path, lines);
            return;
        }

        File.WriteAllLines(path, lines);
    }

    public static void WriteFileLines(string path, IEnumerable<string> lines, Encoding encoding)
        => File.WriteAllLines(path, lines, encoding);

    public static void WriteFileLines(string path, IEnumerable<string> lines, Encoding encoding, bool append)
    {
        if (append)
        {
            File.AppendAllLines(path, lines, encoding);
            return;
        }

        File.WriteAllLines(path, lines, encoding);
    }

    private sealed class FileSystemEnumerable : IEnumerable<IFileSystemInfo>
    {
        private readonly DirectoryInfo di;

        private readonly string search;

        private readonly SearchOption searchOption;

#if !NETLEGACY
        private readonly EnumerationOptions? enumerationOptions;

        public FileSystemEnumerable(DirectoryInfo di, string? search, EnumerationOptions enumerationOptions)
        {
            this.di = di;
            this.search = search ?? "*";
            this.enumerationOptions = enumerationOptions;
        }
#endif

        public FileSystemEnumerable(DirectoryInfo di, string? search = null, SearchOption? searchOption = null)
        {
            this.di = di;
            this.search = search ?? "*";
            this.searchOption = searchOption ?? SearchOption.TopDirectoryOnly;
        }

        public IEnumerator<IFileSystemInfo> GetEnumerator()
        {
#if !NETLEGACY
            if (this.enumerationOptions is not null)
            {
                foreach (var item in this.di.EnumerateFileSystemInfos(this.search, this.enumerationOptions))
                {
                    if (item is FileInfo fi)
                    {
                        yield return new GnomeFileInfo(fi);
                    }
                    else if (item is DirectoryInfo di)
                    {
                        yield return new GnomeDirInfo(di);
                    }
                }

                yield break;
            }
#endif
            foreach (var item in this.di.EnumerateFileSystemInfos(this.search, this.searchOption))
            {
                if (item is FileInfo fi)
                {
                    yield return new GnomeFileInfo(fi);
                }
                else if (item is DirectoryInfo di)
                {
                    yield return new GnomeDirInfo(di);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}