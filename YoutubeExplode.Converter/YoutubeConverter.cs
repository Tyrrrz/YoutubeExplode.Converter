using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter.Internal;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// The entry point for <see cref="Converter"/>.
    /// </summary>
    public partial class YoutubeConverter : IYoutubeConverter
    {
        private readonly IYoutubeClient _youtubeClient;
        private readonly FfmpegCli _ffmpeg;

        /// <summary>
        /// Creates an instance of <see cref="YoutubeConverter"/>.
        /// </summary>
        public YoutubeConverter(IYoutubeClient youtubeClient, string ffmpegFilePath)
        {
            _youtubeClient = youtubeClient.GuardNotNull(nameof(youtubeClient));

            ffmpegFilePath.GuardNotNull(nameof(ffmpegFilePath));
            _ffmpeg = new FfmpegCli(ffmpegFilePath);
        }

        /// <summary>
        /// Creates an instance of <see cref="YoutubeConverter"/>.
        /// </summary>
        public YoutubeConverter(IYoutubeClient youtubeClient)
            : this(youtubeClient, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg"))
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="YoutubeConverter"/>.
        /// </summary>
        public YoutubeConverter()
            : this(new YoutubeClient())
        {
        }

        /// <inheritdoc />
        public async Task DownloadAndProcessMediaStreamsAsync(IReadOnlyList<MediaStreamInfo> mediaStreamInfos,
            string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            mediaStreamInfos.GuardNotNull(nameof(mediaStreamInfos));
            filePath.GuardNotNull(nameof(filePath));
            format.GuardNotNull(nameof(format));

            // Determine if transcoding is required for at least one of the streams
            var transcode = mediaStreamInfos.Any(s => IsTranscodingRequired(s.Container, format));

            // Set up progress-related stuff
            var progressMixer = progress != null ? new ProgressMixer(progress) : null;
            var downloadProgressPortion = transcode ? 0.15 : 0.99;
            var ffmpegProgressPortion = 1 - downloadProgressPortion;
            var totalContentLength = mediaStreamInfos.Sum(s => s.Size);

            // Keep track of the downloaded streams
            var streamFilePaths = new List<string>();
            try
            {
                // Download all streams
                foreach (var streamInfo in mediaStreamInfos)
                {
                    // Generate file path
                    var streamIndex = streamFilePaths.Count + 1;
                    var streamFilePath = $"{filePath}.stream-{streamIndex}.tmp";

                    // Add file path to list
                    streamFilePaths.Add(streamFilePath);

                    // Set up download progress handler
                    var streamDownloadProgress =
                        progressMixer?.Split(downloadProgressPortion * streamInfo.Size / totalContentLength);

                    // Download stream
                    await _youtubeClient.DownloadMediaStreamAsync(streamInfo, streamFilePath, streamDownloadProgress, cancellationToken);
                }

                // Set up process progress handler
                var ffmpegProgress = progressMixer?.Split(ffmpegProgressPortion);

                // Process streams (mux/transcode/etc)
                await _ffmpeg.ProcessAsync(streamFilePaths, filePath, format, transcode, ffmpegProgress, cancellationToken);

                // Report completion in case there are rounding issues in progress reporting
                progress?.Report(1);
            }
            finally
            {
                // Delete all stream files
                foreach (var streamFilePath in streamFilePaths)
                    FileEx.TryDelete(streamFilePath);
            }
        }

        /// <inheritdoc />
        public async Task DownloadVideoAsync(MediaStreamInfoSet mediaStreamInfoSet, string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            mediaStreamInfoSet.GuardNotNull(nameof(mediaStreamInfoSet));
            filePath.GuardNotNull(nameof(filePath));
            format.GuardNotNull(nameof(format));

            var mediaStreamInfos = new List<MediaStreamInfo>();

            // Get best audio stream (priority: transcoding -> bitrate)
            var audioStreamInfo = mediaStreamInfoSet.Audio
                .OrderByDescending(s => !IsTranscodingRequired(s.Container, format))
                .ThenByDescending(s => s.Bitrate)
                .FirstOrDefault();

            // Add to result
            mediaStreamInfos.Add(audioStreamInfo);

            // If needs video - get best video stream (priority: quality -> framerate -> transcoding)
            if (!IsAudioOnlyFormat(format))
            {
                var videoStreamInfo = mediaStreamInfoSet.Video
                    .OrderByDescending(s => s.VideoQuality)
                    .ThenByDescending(s => s.Framerate)
                    .ThenByDescending(s => !IsTranscodingRequired(s.Container, format))
                    .FirstOrDefault();

                // Add to result
                mediaStreamInfos.Add(videoStreamInfo);
            }

            // Download media streams and process them
            await DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, filePath, format, progress, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DownloadVideoAsync(string videoId, string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            videoId.GuardNotNull(nameof(videoId));
            filePath.GuardNotNull(nameof(filePath));
            format.GuardNotNull(nameof(format));

            // Get stream info set
            var mediaStreamInfoSet = await _youtubeClient.GetVideoMediaStreamInfosAsync(videoId);

            // Download video with known stream info set
            await DownloadVideoAsync(mediaStreamInfoSet, filePath, format, progress, cancellationToken);
        }

        /// <inheritdoc />
        public Task DownloadVideoAsync(string videoId, string filePath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            videoId.GuardNotNull(nameof(videoId));
            filePath.GuardNotNull(nameof(filePath));

            // Determine output file format from extension
            var format = Path.GetExtension(filePath)?.TrimStart('.');

            // If no extension is set - default to mp4 format
            if (format.IsNullOrWhiteSpace())
                format = "mp4";

            // Download video with known format
            return DownloadVideoAsync(videoId, filePath, format, progress, cancellationToken);
        }
    }

    public partial class YoutubeConverter
    {
        private static readonly string[] AudioOnlyFormats = {"mp3", "m4a", "wav", "wma", "ogg", "aac", "opus"};

        private static bool IsAudioOnlyFormat(string format) =>
            AudioOnlyFormats.Contains(format, StringComparer.OrdinalIgnoreCase);

        private static bool IsTranscodingRequired(Container container, string format)
            => !string.Equals(container.GetFileExtension(), format, StringComparison.OrdinalIgnoreCase);
    }
}