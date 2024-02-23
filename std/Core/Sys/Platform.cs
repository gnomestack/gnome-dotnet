using System.Runtime.InteropServices;

#pragma warning disable CS8981
using os = System.OperatingSystem;

namespace Gnome.Sys;

#pragma warning disable S3400 // Methods should not return constants (false positive)
public static partial class Platform
{
    private static readonly Lazy<EnvVariables> s_env = new(() => new EnvVariables());

    private static readonly Lazy<EnvPaths> s_paths = new(() => new EnvPaths(s_env.Value));

    public static Architecture Arch => RuntimeInformation.OSArchitecture;

    public static EnvVariables Env => s_env.Value;

    public static bool Is64Bit => Environment.Is64BitOperatingSystem;

    public static EnvPaths Paths => s_paths.Value;

    public static bool IsWindows()
    {
#if NET5_0_OR_GREATER
        return os.IsWindows();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
    }

    public static bool IsWindowsVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0)
    {
#if NET5_0_OR_GREATER
        return os.IsWindowsVersionAtLeast(major, minor, build, revision);
#else
        if (!IsWindows())
            return false;
        return IsOSVersionAtLeast(major, minor, build, 0);
#endif
    }

    public static bool IsLinux()
    {
#if NET5_0_OR_GREATER
        return os.IsLinux();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#endif
    }

    public static bool IsOSPlatformVersionAtLeast(string platform, int major, int minor = 0, int build = 0, int revision = 0)
    {
#if NET5_0_OR_GREATER
        return os.IsOSPlatformVersionAtLeast(platform, major, minor, build, revision);
#else
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Create(platform)))
            return false;
        return IsOSVersionAtLeast(major, minor, build, 0);
#endif
    }

    public static bool IsMacOS()
    {
#if NET5_0_OR_GREATER
        return os.IsMacOS();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif
    }

    public static bool IsMacOSVersionAtLeast(int major, int minor = 0, int build = 0)
    {
#if NET5_0_OR_GREATER
        return os.IsMacOSVersionAtLeast(major, minor, build);
#else
        if (!IsMacOS())
            return false;
        return IsOSVersionAtLeast(major, minor, build, 0);
#endif
    }

    public static bool IsFreeBSD()
    {
#if NET5_0_OR_GREATER
        return os.IsFreeBSD();
#else
        return false;
#endif
    }

    public static bool IsAndroid()
    {
#if NET5_0_OR_GREATER
        return os.IsAndroid();
#else
        return false;
#endif
    }

    public static bool IsAndroidVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0)
    {
#if NET5_0_OR_GREATER
        return os.IsAndroidVersionAtLeast(major, minor, build, revision);
#else
        return false;
#endif
    }

    public static bool IsIOS()
    {
#if NET5_0_OR_GREATER
        return os.IsIOS();
#else
        return false;
#endif
    }

    public static bool IsWasi()
#if NET8_0_OR_GREATER
        => os.IsWasi();
#else
        => false;
#endif

    public static bool IsBrowser()
#if NET8_0_OR_GREATER
        => os.IsBrowser();
#else
        => false;
#endif

    public static bool IsDarwin()
        => IsMacOS() || IsIOS() || IsWatchOS() || IsTvOS();

    public static bool IsMacCatalyst()
    {
#if NET5_0_OR_GREATER
        return os.IsMacCatalyst();
#else
        return false;
#endif
    }

    public static bool IsMacCatalystVersionAtLeast(int major, int minor = 0, int build = 0)
    {
#if NET5_0_OR_GREATER
        return os.IsMacCatalystVersionAtLeast(major, minor, build);
#else
        return false;
#endif
    }

    public static bool IsWatchOS()
    {
#if NET5_0_OR_GREATER
        return os.IsWatchOS();
#else
        return false;
#endif
    }

    public static bool IsWatchOSVersionAtLeast(int major, int minor = 0, int build = 0)
    {
#if NET5_0_OR_GREATER
        return os.IsWatchOSVersionAtLeast(major, minor, build);
#else
        return false;
#endif
    }

    public static bool IsFreeBSDVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0)
    {
#if NET5_0_OR_GREATER
        return os.IsFreeBSDVersionAtLeast(major, minor, build, revision);
#else
        return false;
#endif
    }

    public static bool IsTvOS()
    {
#if NET5_0_OR_GREATER
        return os.IsTvOS();
#else
        return false;
#endif
    }

    public static bool IsTvOSVersionAtLeast(int major, int minor = 0, int build = 0)
    {
#if NET5_0_OR_GREATER
        return os.IsTvOSVersionAtLeast(major, minor, build);
#else
        return false;
#endif
    }

#if !NET5_0_OR_GREATER
    private static bool IsOSVersionAtLeast(int major, int minor, int build, int revision)
    {
        Version current = Environment.OSVersion.Version;

        if (current.Major != major)
            return current.Major > major;

        if (current.Minor != minor)
            return current.Minor > minor;

        if (current.Build != build)
            return current.Build > build;

        return current.Revision >= revision
            || (current.Revision == -1 && revision == 0); // it is unavailable on OSX and Environment.OSVersion.Version.Revision returns -1
    }
#endif
}