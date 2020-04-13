using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// Extensions for backwards compatibility.
    /// </summary>
    public static class BackwardCompatibilityExtensions
    {
        /// <summary>
        /// Downloads given media streams and processes them into a file using specified format.
        /// </summary>
        public static Task DownloadAndProcessMediaStreamsAsync(this IYoutubeConverter converter,
            IReadOnlyList<IStreamInfo> streamInfos,
            string filePath, string format,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default) =>
            converter.DownloadAndProcessMediaStreamsAsync(streamInfos, filePath, format, ConversionPreset.Medium, progress, cancellationToken);

        /// <summary>
        /// Downloads a video to a file using specified format by selecting media streams from the given set.
        /// </summary>
        public static Task DownloadVideoAsync(this IYoutubeConverter converter,
            StreamManifest streamManifest, string filePath, string format,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default) =>
            converter.DownloadVideoAsync(streamManifest, filePath, format, ConversionPreset.Medium, progress, cancellationToken);
    }
}