// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class Sys
    {
#if NET7_0_OR_GREATER
        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_Log")]
        internal static unsafe partial void Log(byte* buffer, int count);

        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_LogError")]
        internal static unsafe partial void LogError(byte* buffer, int count);
#else
        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_Log")]
        internal static extern unsafe void Log(byte* buffer, int count);

        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_LogError")]
        internal static extern unsafe void LogError(byte* buffer, int count);
#endif
    }
}