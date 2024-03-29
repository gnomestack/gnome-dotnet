// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static partial class Sys
    {
#if NET7_0_OR_GREATER
        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_Stat", SetLastError = true)]
        internal static partial int Stat(ref byte path, out FileStatus output);
#else
        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_Stat", SetLastError = true)]
        internal static extern int Stat(ref byte path, out FileStatus output);
#endif

        internal static int Stat(ReadOnlySpan<char> path, out FileStatus output)
        {
            var converter = new ValueUtf8Converter(stackalloc byte[DefaultPathBufferSize]);
            int result = Stat(ref MemoryMarshal.GetReference(converter.ConvertAndTerminateString(path)), out output);
            converter.Dispose();
            return result;
        }

#if NET7_0_OR_GREATER
        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_LStat", SetLastError = true)]
        internal static partial int LStat(ref byte path, out FileStatus output);
#else
        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_LStat", SetLastError = true)]
        internal static extern int LStat(ref byte path, out FileStatus output);
#endif

        internal static int LStat(ReadOnlySpan<char> path, out FileStatus output)
        {
            var converter = new ValueUtf8Converter(stackalloc byte[DefaultPathBufferSize]);
            int result = LStat(ref MemoryMarshal.GetReference(converter.ConvertAndTerminateString(path)), out output);
            converter.Dispose();
            return result;
        }
    }
}