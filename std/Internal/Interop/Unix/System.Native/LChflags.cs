// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

[SuppressMessage("Garbage", "S3903")]
internal static partial class Interop
{
    internal static partial class Sys
    {
        internal static readonly bool SupportsHiddenFlag = CanGetHiddenFlag() != 0;

        internal static readonly bool CanSetHiddenFlag = LChflagsCanSetHiddenFlag() != 0;

        #pragma warning disable S2344, RCS1135 // Enumeration type names should not have "Flags" or "Enum" suffixes
        [Flags]
        internal enum UserFlags : uint
        {
            UF_HIDDEN = 0x8000,
        }

#if NET7_0_OR_GREATER
        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_LChflags", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        internal static partial int LChflags(string path, uint flags);

        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_FChflags", SetLastError = true)]
        internal static partial int FChflags(SafeHandle fd, uint flags);

        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_LChflagsCanSetHiddenFlag")]
        [SuppressGCTransition]
        private static partial int LChflagsCanSetHiddenFlag();

        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_CanGetHiddenFlag")]
        [SuppressGCTransition]
        private static partial int CanGetHiddenFlag();
#else
        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_LChflags", SetLastError = true)]
        internal static extern int LChflags(string path, uint flags);

        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_FChflags", SetLastError = true)]
        internal static extern int FChflags(SafeHandle fd, uint flags);

        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_LChflagsCanSetHiddenFlag")]
        [SuppressGCTransition]
        private static extern int LChflagsCanSetHiddenFlag();

        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_CanGetHiddenFlag")]
        [SuppressGCTransition]
        private static extern int CanGetHiddenFlag();
#endif
    }
}