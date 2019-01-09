using System.Collections.Generic;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Converter.Services
{
    /// <summary>
    /// Provider for selecting optimal media streams based on input preferences.
    /// </summary>
    public interface IMediaStreamInfoSelector
    {
        /// <summary>
        /// Selects the most optimal media streams from the set for encoding, based on output format, preferred video quality and framerate.
        /// </summary>
        IReadOnlyList<MediaStreamInfo> Select(MediaStreamInfoSet mediaStreamInfoSet, string format,
            VideoQuality? preferredVideoQuality = null, int? preferredFramerate = null);
    }
}