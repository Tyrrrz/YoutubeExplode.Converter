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
        /// Downloads given media streams and processes them into a file using specified format.
        /// </summary>
        Task DownloadAndProcessMediaStreamsAsync(IReadOnlyList<MediaStreamInfo> mediaStreamInfos,
            string filePath, string format,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a video to a file using specified format by selecting media streams from the given set.
        /// </summary>
        Task DownloadVideoAsync(MediaStreamInfoSet mediaStreamInfoSet, string filePath, string format,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a video to a file using specified format.
        /// </summary>
        Task DownloadVideoAsync(string videoId, string filePath, string format,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a video to a file.
        /// </summary>
        Task DownloadVideoAsync(string videoId, string filePath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    }
}
