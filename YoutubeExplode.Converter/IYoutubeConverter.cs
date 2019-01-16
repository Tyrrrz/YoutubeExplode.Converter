using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// Interface for <see cref="YoutubeConverter"/>.
    /// </summary>
    public interface IYoutubeConverter
    {
        /// <summary>
        /// Downloads given media streams and processes them into a file in the specified format.
        /// </summary>
        Task DownloadAndProcessMediaStreamsAsync(IReadOnlyList<MediaStreamInfo> mediaStreamInfos,
            string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a video to a file in the specified format using the highest quality media streams from the set.
        /// </summary>
        Task DownloadVideoAsync(MediaStreamInfoSet mediaStreamInfoSet, string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a video to a file in the specified format using the highest quality media streams available.
        /// </summary>
        Task DownloadVideoAsync(string videoId, string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a video to a file using the highest quality media streams available.
        /// </summary>
        Task DownloadVideoAsync(string videoId, string filePath,
            IProgress<double> progress = null, 
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
