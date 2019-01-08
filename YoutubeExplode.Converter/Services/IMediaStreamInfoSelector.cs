using System.Collections.Generic;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Converter.Services
{
    /// <summary>
    /// Provider for selecting optimal media streams based on input criteria.
    /// </summary>
    public interface IMediaStreamInfoSelector
    {
        /// <summary>
        /// Selects the most optimal media streams from the set for encoding, based on preferred quality and output format.
        /// </summary>
        IReadOnlyList<MediaStreamInfo> Select(MediaStreamInfoSet mediaStreamInfoSet,
            VideoQuality videoQuality, string format);
    }
}