using System;
using System.Collections.Generic;

namespace YoutubeExplode.Converter.Internal
{
    internal static class Extensions
    {
        public static bool IsEmpty(this string s) => string.IsNullOrEmpty(s);

        public static string JoinToString<T>(this IEnumerable<T> source, string separator) =>
            string.Join(separator, source);

        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            // If value is less than min - return min
            if (value.CompareTo(min) <= 0)
                return min;

            // If value is greater than max - return max
            if (value.CompareTo(max) >= 0)
                return max;

            // Otherwise - return value
            return value;
        }
    }
}