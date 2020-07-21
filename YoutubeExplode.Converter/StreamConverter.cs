using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter.Internal;
using YoutubeExplode.Converter.Options;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// Stream converter.
    /// </summary>
    public partial class StreamConverter
    {
        private readonly StreamsClient _client;
        private readonly FFmpeg _ffmpeg;

        /// <summary>
        /// Initializes an instance of <see cref="StreamConverter"/>.
        /// </summary>
        public StreamConverter(StreamsClient client, string ffmpegFilePath)
        {
            Platform.EnsureDesktop();

            _client = client;
            _ffmpeg = new FFmpeg(ffmpegFilePath);
        }

        /// <summary>
        /// Converts specified streams.
        /// </summary>
        public async Task ConvertStreamsAsync(
            IReadOnlyList<IStreamInfo> streamInfos,
            ConversionOptions options,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var streams = await Task.WhenAll(streamInfos.Select(async s => await _client.GetAsync(s)));

            var isTranscodingRequired = streamInfos.Any(s => IsTranscodingRequired(s.Container, options.Format));

            await _ffmpeg.ConvertStreamsAsync(
                streams,
                options,
                isTranscodingRequired,
                progress,
                cancellationToken
            );

            progress?.Report(1);
        }

        /// <summary>
        /// Converts most fitting streams for the specified video ID.
        /// </summary>
        public async Task ConvertStreamsAsync(
            VideoId videoId,
            ConversionOptions options,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var streamManifest = await _client.GetManifestAsync(videoId);
            var streamInfos = GetBestMediaStreamInfos(streamManifest, options.Format).ToArray();

            await ConvertStreamsAsync(
                streamInfos,
                options,
                progress,
                cancellationToken
            );
        }

        /// <summary>
        /// Converts most fitting streams for the specified video ID.
        /// </summary>
        public async Task ConvertStreamsAsync(
            VideoId videoId,
            string outputFilePath,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var options = new ConversionOptionsBuilder(outputFilePath).Build();

            await ConvertStreamsAsync(
                videoId,
                options,
                progress,
                cancellationToken
            );
        }

        /// <summary>
        /// Converts most fitting streams for the specified video ID.
        /// </summary>
        public async Task ConvertStreamsAsync(
            VideoId videoId,
            string outputFilePath,
            string format,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var options = new ConversionOptionsBuilder(outputFilePath)
                .SetFormat(format)
                .Build();

            await ConvertStreamsAsync(
                videoId,
                options,
                progress,
                cancellationToken
            );
        }
    }

    public partial class StreamConverter
    {
        private static readonly string[] AudioOnlyFormats = {"mp3", "m4a", "wav", "wma", "ogg", "aac", "opus"};

        private static bool IsAudioOnlyFormat(string format) =>
            AudioOnlyFormats.Contains(format, StringComparer.OrdinalIgnoreCase);

        private static bool IsTranscodingRequired(Container container, string format) =>
            !string.Equals(container.Name, format, StringComparison.OrdinalIgnoreCase);

        private static IEnumerable<IStreamInfo> GetBestMediaStreamInfos(StreamManifest streamManifest, string format)
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
            if (!IsAudioOnlyFormat(format))
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
    }

    /// <summary>
    /// Extensions for <see cref="StreamConverter"/>.
    /// </summary>
    public static class StreamConverterExtensions
    {
        /// <summary>
        /// Gets an instance of <see cref="StreamConverter"/> that can be used to mux and convert streams.
        /// </summary>
        public static StreamConverter GetConverter(this StreamsClient client, string ffmpegFilePath) =>
            new StreamConverter(client, ffmpegFilePath);

        /// <summary>
        /// Gets an instance of <see cref="StreamConverter"/> that can be used to mux and convert streams.
        /// </summary>
        public static StreamConverter GetConverter(this StreamsClient client) =>
            client.GetConverter(FFmpeg.GetDefaultExecutableFilePath());
    }
}