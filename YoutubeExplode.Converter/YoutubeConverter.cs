using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter.Internal;
using YoutubeExplode.Converter.Services;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// The entry point for <see cref="Converter"/>.
    /// </summary>
    public class YoutubeConverter : IYoutubeConverter
    {
        private readonly IYoutubeClient _youtubeClient;
        private readonly IMediaStreamInfoSelector _mediaStreamInfoSelector;

        private readonly FfmpegCli _ffmpeg;

        /// <summary>
        /// Creates an instance of <see cref="YoutubeConverter"/>.
        /// </summary>
        public YoutubeConverter(IYoutubeClient youtubeClient, IMediaStreamInfoSelector mediaStreamInfoSelector,
            string ffmpegFilePath)
        {
            _youtubeClient = youtubeClient.GuardNotNull(nameof(youtubeClient));
            _mediaStreamInfoSelector = mediaStreamInfoSelector.GuardNotNull(nameof(mediaStreamInfoSelector));

            ffmpegFilePath.GuardNotNull(nameof(ffmpegFilePath));
            _ffmpeg = new FfmpegCli(ffmpegFilePath);
        }

        /// <summary>
        /// Creates an instance of <see cref="YoutubeConverter"/>.
        /// </summary>
        public YoutubeConverter(IYoutubeClient youtubeClient, string ffmpegFilePath)
            : this(youtubeClient, MediaStreamInfoSelector.Instance, ffmpegFilePath)
        {
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

        private async Task DownloadAndProcessMediaStreamsAsync(IReadOnlyList<MediaStreamInfo> streamInfos,
            string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Determine if transcoding is required - if one of the input stream containers doesn't match the output format
            var transcode = streamInfos
                .Select(s => s.Container.GetFileExtension())
                .Any(f => !string.Equals(f, format, StringComparison.OrdinalIgnoreCase));

            // Set up progress-related stuff
            var progressMixer = progress != null ? new ProgressMixer(progress) : null;
            var downloadProgressPortion = transcode ? 0.15 : 0.99;
            var ffmpegProgressPortion = 1 - downloadProgressPortion;
            var totalContentLength = streamInfos.Sum(s => s.Size);

            // Keep track of the downloaded streams
            var streamFilePaths = new List<string>();
            try
            {
                // Download all streams
                foreach (var streamInfo in streamInfos)
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
                    await _youtubeClient
                        .DownloadMediaStreamAsync(streamInfo, streamFilePath, streamDownloadProgress, cancellationToken)
                        .ConfigureAwait(false);
                }

                // Set up process progress handler
                var ffmpegProgress = progressMixer?.Split(ffmpegProgressPortion);

                // Process streams (mux/transcode/etc)
                await _ffmpeg
                    .ProcessAsync(streamFilePaths, filePath, format, transcode, ffmpegProgress, cancellationToken)
                    .ConfigureAwait(false);

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
        public Task DownloadVideoAsync(MediaStreamInfoSet mediaStreamInfoSet, VideoQuality videoQuality, 
            string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            mediaStreamInfoSet.GuardNotNull(nameof(mediaStreamInfoSet));
            filePath.GuardNotNull(nameof(filePath));
            format.GuardNotNull(nameof(format));

            // Select stream infos
            var streamInfos = _mediaStreamInfoSelector.Select(mediaStreamInfoSet, videoQuality, format);

            // Download media streams and process them into one file
            return DownloadAndProcessMediaStreamsAsync(streamInfos, filePath, format, progress, cancellationToken);
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
            var mediaStreamInfoSet = await _youtubeClient.GetVideoMediaStreamInfosAsync(videoId)
                .ConfigureAwait(false);

            // Get highest video quality
            var videoQuality = mediaStreamInfoSet.GetAllVideoQualities().OrderByDescending(q => q).First();

            // Download media streams and process them into one file
            await DownloadVideoAsync(mediaStreamInfoSet, videoQuality, filePath, format, progress, cancellationToken)
                .ConfigureAwait(false);
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
            if (format.IsBlank())
                format = "mp4";

            return DownloadVideoAsync(videoId, filePath, format, progress, cancellationToken);
        }
    }
}