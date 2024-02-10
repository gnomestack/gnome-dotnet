// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class Sys
    {
#if NET7_0_OR_GREATER
        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_GetPid")]
        internal static partial int GetPid();
#else
        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_GetPid")]
        internal static extern int GetPid();
#endif
    }
}