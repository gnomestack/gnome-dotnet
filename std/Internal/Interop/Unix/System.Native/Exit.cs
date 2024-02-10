// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal unsafe partial class Sys
    {
#if NET7_0_OR_GREATER
        [DoesNotReturn]
        [LibraryImport(Interop.Libraries.SystemNative, EntryPoint = "SystemNative_Exit")]
        internal static partial void Exit(int exitCode);
#else
        [DoesNotReturn]
        [DllImport(Interop.Libraries.SystemNative, EntryPoint = "SystemNative_Exit")]
        internal static extern void Exit(int exitCode);
#endif
    }
}