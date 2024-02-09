using System.Runtime.InteropServices;

namespace Gnome.Sys;

public static class ArchitectureExtensions
{
    public static bool Is64Bit(this Architecture arch)
    {
        return arch == Architecture.X64 || arch == Architecture.Arm64;
    }

    public static bool IsWasm(this Architecture arch)
    {
#if NET8_0_OR_GREATER
        return arch == Architecture.Wasm;
#else
        return false;
#endif
    }
}