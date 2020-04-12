using System;
using System.Runtime.InteropServices;

namespace YoutubeExplode.Converter.Internal
{
    internal static class Platform
    {
        public static void EnsureDesktop()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                !RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                throw new PlatformNotSupportedException("YoutubeExplode.Converter works only on desktop operating systems.");
        }
    }
}