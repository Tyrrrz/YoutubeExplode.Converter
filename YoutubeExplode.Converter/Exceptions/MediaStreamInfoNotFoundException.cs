using System;

namespace YoutubeExplode.Converter.Exceptions
{
    /// <summary>
    /// Thrown when there was no media stream info in the set matching input preferences.
    /// </summary>
    public class MediaStreamInfoNotFoundException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="MediaStreamInfoNotFoundException"/>.
        /// </summary>
        public MediaStreamInfoNotFoundException(string message)
            : base(message)
        {
        }
    }
}