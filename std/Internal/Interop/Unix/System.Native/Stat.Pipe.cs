// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

internal static partial class Interop
{
    internal static partial class Sys
    {
#if NET7_0_OR_GREATER
        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_FStat", SetLastError = true)]
        internal static partial int FStat(SafePipeHandle fd, out FileStatus output);
#else
        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_FStat", SetLastError = true)]
        internal static extern int FStat(SafePipeHandle fd, out FileStatus output);
#endif
    }
}