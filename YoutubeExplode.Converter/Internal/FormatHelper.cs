using System;
using System.Linq;

namespace YoutubeExplode.Converter.Internal
{
    internal static class FormatHelper
    {
        private static readonly string[] AudioOnlyFormats = {"mp3", "m4a", "wav", "wma", "ogg", "aac", "opus"};

        public static bool IsAudioOnlyFormat(string format) =>
            AudioOnlyFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
    }
}