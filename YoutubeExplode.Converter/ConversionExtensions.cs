using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter.Internal;
using YoutubeExplode.Converter.Internal.Extensions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// Extensions for <see cref="VideoClient"/> that provide interface for downloading videos through FFmpeg.
    /// </summary>
    public static class ConversionExtensions
    {
        private static bool IsTranscodingRequired(Container container, ConversionFormat format) =>
            !string.Equals(container.Name, format.Name, StringComparison.OrdinalIgnoreCase);

        private static IEnumerable<IStreamInfo> GetBestMediaStreamInfos(
            StreamManifest streamManifest,
            ConversionFormat format)
        {
            // Fail if there are no available streams
            if (!streamManifest.Streams.Any())
                throw new ArgumentException("There are no streams available.", nameof(streamManifest));

            // Use single muxed stream if adaptive streams are not available
            if (!streamManifest.GetAudioOnly().Any() || !streamManifest.GetVideoOnly().Any())
            {
                // Priority: video quality -> transcoding
                yield return streamManifest
                    .GetMuxed()
                    .OrderByDescending(s => s.VideoQuality)
                    .ThenByDescending(s => !IsTranscodingRequired(s.Container, format))
                    .First();

                yield break;
            }

            // Include audio stream
            // Priority: transcoding -> bitrate
            yield return streamManifest
                .GetAudioOnly()
                .OrderByDescending(s => !IsTranscodingRequired(s.Container, format))
                .ThenByDescending(s => s.Bitrate)
                .First();

            // Include video stream
            if (!format.IsAudioOnly)
            {
                // Priority: video quality -> framerate -> transcoding
                yield return streamManifest
                    .GetVideoOnly()
                    .OrderByDescending(s => s.VideoQuality)
                    .ThenByDescending(s => s.Framerate)
                    .ThenByDescending(s => !IsTranscodingRequired(s.Container, format))
                    .First();
            }
        }

        /// <summary>
        /// Downloads specified media streams and processes them into a single file.
        /// </summary>
        public static async Task DownloadAsync(
            this VideoClient videoClient,
            IReadOnlyList<IStreamInfo> streamInfos,
            ConversionRequest request,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Platform.EnsureDesktop();

            var isTranscodingRequired = streamInfos.Any(s => IsTranscodingRequired(s.Container, request.Format));

            // Progress setup
            var progressMixer = progress?.Pipe(p => new ProgressMixer(p));
            var downloadProgressPortion = isTranscodingRequired ? 0.15 : 0.99;
            var totalStreamSize = streamInfos.Sum(s => s.Size.TotalBytes);

            // Temp files for streams
            var streamFilePaths = new List<string>(streamInfos.Count);

            try
            {
                // Download streams
                foreach (var streamInfo in streamInfos)
                {
                    var streamIndex = streamFilePaths.Count + 1;
                    var streamFilePath = $"{request.OutputFilePath}.stream-{streamIndex}.tmp";

                    streamFilePaths.Add(streamFilePath);

                    var streamDownloadProgress = progressMixer?.Split(
                        downloadProgressPortion * streamInfo.Size.TotalBytes / totalStreamSize
                    );

                    await videoClient.Streams.DownloadAsync(
                        streamInfo,
                        streamFilePath,
                        streamDownloadProgress,
                        cancellationToken
                    );
                }

                // Mux/convert streams
                var conversionProgress = progressMixer?.Split(1 - downloadProgressPortion);

                await new FFmpeg(request.FFmpegCliFilePath).ExecuteAsync(
                    streamFilePaths,
                    request.OutputFilePath,
                    request.Format.Name,
                    request.Preset.ToString().ToLowerInvariant(),
                    isTranscodingRequired,
                    conversionProgress,
                    cancellationToken
                );

                progress?.Report(1);
            }
            finally
            {
                // Delete temp files
                foreach (var streamFilePath in streamFilePaths)
                {
                    try
                    {
                        File.Delete(streamFilePath);
                    }
                    catch
                    {
                        // Try our best but don't crash
                    }
                }
            }
        }

        /// <summary>
        /// Selects best media streams for the specified video, downloads them, and processes into a single file.
        /// </summary>
        public static async Task DownloadAsync(
            this VideoClient videoClient,
            VideoId videoId,
            ConversionRequest request,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var streamManifest = await videoClient.Streams.GetManifestAsync(videoId);
            var streamInfos = GetBestMediaStreamInfos(streamManifest, request.Format).ToArray();

            await videoClient.DownloadAsync(
                streamInfos,
                request,
                progress,
                cancellationToken
            );
        }

        /// <summary>
        /// Selects best media streams for the specified video, downloads them, and processes into a single file.
        /// Conversion format is derived from file extension, unless explicitly specified.
        /// </summary>
        public static async Task DownloadAsync(
            this VideoClient videoClient,
            VideoId videoId,
            string outputFilePath,
            Action<ConversionRequestBuilder> configure,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var requestBuilder = new ConversionRequestBuilder(outputFilePath);
            configure(requestBuilder);
            var request = requestBuilder.Build();

            await videoClient.DownloadAsync(videoId, request, progress, cancellationToken);
        }

        /// <summary>
        /// Selects best media streams for the specified video, downloads them, and processes into a single file.
        /// Conversion format is derived from file extension. If none is specified, mp4 is chosen as the default.
        /// </summary>
        public static async Task DownloadAsync(
            this VideoClient videoClient,
            VideoId videoId,
            string outputFilePath,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            await videoClient.DownloadAsync(videoId, outputFilePath, o => {}, progress, cancellationToken);
        }
    }
}