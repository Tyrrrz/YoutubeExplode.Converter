using System;
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
        /// Downloads a video to a file using its stream info set, preferred video quality and the specified format.
        /// </summary>
        Task DownloadVideoAsync(MediaStreamInfoSet mediaStreamInfoSet, VideoQuality videoQuality,
            string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a video to a file using the specified format.
        /// </summary>
        Task DownloadVideoAsync(string videoId, string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a video to a file.
        /// </summary>
        Task DownloadVideoAsync(string videoId, string filePath,
            IProgress<double> progress = null, 
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
