// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class Sys
    {
        internal static unsafe string GetHostName()
        {
            const int HOST_NAME_MAX = 255;
            const int ArrLength = HOST_NAME_MAX + 1;

            byte* name = stackalloc byte[ArrLength];
            int err = GetHostName(name, ArrLength);
            if (err != 0)
            {
                // This should never happen.  According to the man page,
                // the only possible errno for gethostname is ENAMETOOLONG,
                // which should only happen if the buffer we supply isn't big
                // enough, and we're using a buffer size that the man page
                // says is the max for POSIX (and larger than the max for Linux).
                Debug.Fail($"GetHostName failed with error {err}");
                throw new InvalidOperationException($"{nameof(GetHostName)}: {err}");
            }

            // If the hostname is truncated, it is unspecified whether the returned buffer includes a terminating null byte.
            name[ArrLength - 1] = 0;
#if NET5_0_OR_GREATER
            return Marshal.PtrToStringUTF8((IntPtr)name)!;
#else
            return Marshal.PtrToStringAnsi((IntPtr)name)!;
#endif
        }

#if NET7_0_OR_GREATER
        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_GetHostName", SetLastError = true)]
        private static unsafe partial int GetHostName(byte* name, int nameLength);
#else

        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_GetHostName", SetLastError = true)]
        private static unsafe extern int GetHostName(byte* name, int nameLength);
#endif
    }
}