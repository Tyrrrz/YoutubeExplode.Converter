using System;
using System.Globalization;

namespace YoutubeExplode.Converter.Internal.Extensions
{
    internal static class StringExtensions
    {
        public static string? NullIfWhiteSpace(this string s) =>
            !string.IsNullOrWhiteSpace(s)
                ? s
                : null;

        public static TimeSpan ParseTimeSpan(this string s, string format) =>
            TimeSpan.ParseExact(s, format, CultureInfo.InvariantCulture);
    }
}