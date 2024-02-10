// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static partial class Sys
    {
#pragma warning disable S1939, RCS1042 // Inheritance list should not be redundant
        internal enum NodeType : int
        {
            DT_UNKNOWN = 0,
            DT_FIFO = 1,
            DT_CHR = 2,
            DT_DIR = 4,
            DT_BLK = 6,
            DT_REG = 8,
            DT_LNK = 10,
            DT_SOCK = 12,
            DT_WHT = 14,
        }

#if NET7_0_OR_GREATER
        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_OpenDir", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        internal static partial IntPtr OpenDir(string path);

        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_GetReadDirRBufferSize", SetLastError = false)]
        [SuppressGCTransition]
        internal static partial int GetReadDirRBufferSize();

        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_ReadDirR")]
        internal static unsafe partial int ReadDirR(IntPtr dir, byte* buffer, int bufferSize, DirectoryEntry* outputEntry);

        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_CloseDir", SetLastError = true)]
        internal static partial int CloseDir(IntPtr dir);
#else
        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_OpenDir", SetLastError = true)]
        internal static extern IntPtr OpenDir(string path);

        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_GetReadDirRBufferSize", SetLastError = false)]
        internal static extern int GetReadDirRBufferSize();

        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_ReadDirR")]
        internal static unsafe extern int ReadDirR(IntPtr dir, byte* buffer, int bufferSize, DirectoryEntry* outputEntry);

        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_CloseDir", SetLastError = true)]
        internal static extern int CloseDir(IntPtr dir);
#endif

#pragma warning disable SA1203 // Constants should appear before fields (we want to keep the struct together)
        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct DirectoryEntry
        {
            internal byte* Name;
            internal int NameLength;
            internal NodeType InodeType;

            internal const int NameBufferSize = 256; // sizeof(dirent->d_name) == NAME_MAX + 1

#if NET7_0_OR_GREATER
            internal ReadOnlySpan<char> GetName(Span<char> buffer)
            {
                // -1 for null terminator (buffer will not include one),
                //  and -1 because GetMaxCharCount pessimistically assumes the buffer may start with a partial surrogate
                Debug.Assert(buffer.Length >= Encoding.UTF8.GetMaxCharCount(NameBufferSize - 1 - 1), "buffer is too small to hold the maximum possible name length");

                Debug.Assert(this.Name != null, "should not have a null name");

                // In this case the struct was allocated via struct dirent *readdir(DIR *dirp);
                ReadOnlySpan<byte> nameBytes = (this.NameLength == -1)
                    ? new ReadOnlySpan<byte>(this.Name, new ReadOnlySpan<byte>(this.Name, NameBufferSize).IndexOf<byte>(0))
                    : new ReadOnlySpan<byte>(this.Name, this.NameLength);

                Debug.Assert(nameBytes.Length > 0, "we shouldn't have gotten a garbage value from the OS");

                int charCount = Encoding.UTF8.GetChars(nameBytes, buffer);
                ReadOnlySpan<char> value = buffer.Slice(0, charCount);
                Debug.Assert(this.NameLength != -1 || !value.Contains('\0'), "should not have embedded nulls if we parsed the end of string");
                return value;
            }
#endif
        }
    }
}