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
    /// The entry point for <see cref="YoutubeExplode.Converter"/>.
    /// </summary>
    public class YoutubeConverter : IYoutubeConverter
    {
        private readonly IYoutubeClient _client;
        private readonly FfmpegCli _ffmpeg;

        /// <summary>
        /// Creates an instance of <see cref="YoutubeConverter"/>.
        /// </summary>
        public YoutubeConverter(IYoutubeClient client, string ffmpegFilePath)
        {
            _client = client.GuardNotNull(nameof(client));

            ffmpegFilePath.GuardNotNull(nameof(ffmpegFilePath));
            _ffmpeg = new FfmpegCli(ffmpegFilePath);
        }

        /// <summary>
        /// Creates an instance of <see cref="YoutubeConverter"/>.
        /// </summary>
        public YoutubeConverter(IYoutubeClient client)
            : this(client, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg"))
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
            // Set up progress mixer
            var progressMixer = progress != null ? new ProgressMixer(progress) : null;
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

                    // Set up download progress handler (80% of the total progress)
                    var streamDownloadProgress = progressMixer?.Split(0.8 * streamInfo.Size / totalContentLength);

                    // Download stream
                    await _client
                        .DownloadMediaStreamAsync(streamInfo, streamFilePath, streamDownloadProgress, cancellationToken)
                        .ConfigureAwait(false);
                }

                // Avoid transcoding if the output format and input stream formats are all mp4
                var transcode = !(format == "mp4" &&
                                  streamInfos.All(s => s.Container == Container.Mp4 || s.Container == Container.M4A));

                // Set up process progress handler (20% of the total progress)
                var processProgress = progressMixer?.Split(0.2);

                // Process streams (mux/transcode/etc)
                await _ffmpeg
                    .ProcessAsync(streamFilePaths, filePath, format, transcode, processProgress, cancellationToken)
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

        private static IReadOnlyList<MediaStreamInfo> PickBestMediaStreamInfos(MediaStreamInfoSet set, string format)
        {
            var result = new List<MediaStreamInfo>();

            // Add the highest bitrate audio, prefer m4a
            result.Add(set.Audio.Where(s => s.Container == Container.M4A).WithHighestBitrate() ??
                       set.Audio.WithHighestBitrate());

            // Check if needs video
            if (!FormatHelper.IsAudioOnlyFormat(format))
            {
                // Add the highest quality video, prefer mp4
                result.Add(set.Video.Where(s => s.Container == Container.Mp4).WithHighestVideoQuality() ??
                           set.Video.WithHighestVideoQuality());
            }

            return result;
        }

        /// <inheritdoc />
        public async Task DownloadVideoAsync(string videoId, string filePath, string format,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            videoId.GuardNotNull(nameof(videoId));
            filePath.GuardNotNull(nameof(filePath));
            format.GuardNotNull(nameof(format));

            // Turn format lowercase to simplify comparison
            format = format.ToLowerInvariant();

            // Get stream info set
            var set = await _client.GetVideoMediaStreamInfosAsync(videoId).ConfigureAwait(false);

            // Select stream infos
            var streamInfos = PickBestMediaStreamInfos(set, format);

            // Download media streams and process them into one file
            await DownloadAndProcessMediaStreamsAsync(streamInfos, filePath, format, progress, cancellationToken)
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