using System.Runtime.InteropServices;

namespace MonoPlus.Utils.General;

/// <summary>
/// Small helper class to store current <see cref="OSPlatform"/> in <see cref="Platform"/>
/// </summary>
public static class OS
{
    /// <summary>
    /// Current <see cref="OSPlatform"/>, or <see langword="null"/> if failed to detect
    /// </summary>
    public static OSPlatform? Platform;

    /// <summary>
    /// Sets <see cref="Platform"/> based on <see cref="RuntimeInformation.IsOSPlatform"/>
    /// </summary>
    public static void Initialize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Platform = OSPlatform.Windows;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Platform = OSPlatform.OSX;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Platform = OSPlatform.Linux;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) Platform = OSPlatform.FreeBSD;
    }
}
