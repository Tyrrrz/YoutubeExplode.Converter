using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter.Internal;
using YoutubeExplode.Converter.Internal.Extensions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// The entry point for <see cref="Converter"/>.
    /// </summary>
    public partial class YoutubeConverter : IYoutubeConverter
    {
        private readonly YoutubeClient _youtube;
        private readonly FFmpeg _ffmpeg;

        /// <summary>
        /// Creates an instance of <see cref="YoutubeConverter"/>.
        /// </summary>
        public YoutubeConverter(YoutubeClient youtube, string ffmpegFilePath)
        {
            _youtube = youtube;
            _ffmpeg = new FFmpeg(ffmpegFilePath);

            Platform.EnsureDesktop();
        }

        /// <summary>
        /// Creates an instance of <see cref="YoutubeConverter"/>.
        /// </summary>
        public YoutubeConverter(YoutubeClient youtube)
            : this(youtube, GetDefaultFFmpegFilePath())
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
        public async Task DownloadAndProcessMediaStreamsAsync(IReadOnlyList<IStreamInfo> streamInfos,
            string filePath, string format, ConversionPreset preset,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            var sessionId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);

            // Split progress reporting
            var progressMixer = progress?.Pipe(p => new ProgressMixer(p));

            // Generate names for pipes that will transfer media streams
            var streamPipeNames = streamInfos
                .Select((_, i) => $"yte-conv-{sessionId}-{i}")
                .ToArray();

            // Create named pipes
            var streamPipes = streamInfos
                .Zip(streamPipeNames, (_, n) => new NamedPipeServerStream(n, PipeDirection.Out))
                .ToArray();

            foreach (var asd in Directory.EnumerateFiles(Path.GetTempPath(), "CoreFxPipe_*"))
                Console.WriteLine(asd);

            // Start piping asynchronously
            var streamPipingTasks = streamInfos
                .Zip(streamPipes, async (s, p) =>
                {
                    var streamProgress = progressMixer?.Split(1.0 / streamInfos.Count);
                    await p.WaitForConnectionAsync(cancellationToken);
                    await _youtube.Videos.Streams.CopyToAsync(s, p, streamProgress, cancellationToken);
                    p.Disconnect();
                })
                .ToArray();

            try
            {
                // Get virtual file paths for the named pipes
                var streamFilePaths = streamPipeNames
                    .Select(Platform.GetNamedPipeUniversalPath)
                    .ToArray();

                // Avoid transcoding if the input and output formats much
                var avoidTranscoding = !streamInfos.Any(s => IsTranscodingRequired(s.Container, format));

                // Process streams
                await _ffmpeg.ConvertAsync(filePath, streamFilePaths, format, preset, avoidTranscoding, cancellationToken);
                await Task.WhenAll(streamPipingTasks);

                // Report completion in case there are rounding issues in progress reporting
                progress?.Report(1);
            }
            finally
            {
                foreach (var streamPipe in streamPipes)
                    streamPipe.Dispose();
            }
        }

        /// <inheritdoc />
        public async Task DownloadVideoAsync(StreamManifest streamManifest, string filePath, string format, ConversionPreset preset,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            var streamInfos = GetBestMediaStreamInfos(streamManifest, format).ToArray();
            await DownloadAndProcessMediaStreamsAsync(streamInfos, filePath, format, preset, progress, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DownloadVideoAsync(string videoId, string filePath, string format, ConversionPreset preset,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);
            await DownloadVideoAsync(streamManifest, filePath, format, preset, progress, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DownloadVideoAsync(string videoId, string filePath, string format,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            await DownloadVideoAsync(videoId, filePath, format, ConversionPreset.Medium, progress, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DownloadVideoAsync(string videoId, string filePath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            var format = Path
                .GetExtension(filePath)?
                .TrimStart('.')
                .NullIfWhiteSpace() ?? "mp4";

            await DownloadVideoAsync(videoId, filePath, format, progress, cancellationToken);
        }
    }

    public partial class YoutubeConverter
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

    public partial class YoutubeConverter
    {
        private static string GetDefaultFFmpegFilePath()
        {
            // Check the probe directory and see if there's anything that resembles FFmpeg there.
            // If not, fallback to just "ffmpeg" and hope it's either in current working directory or on PATH.

            var primaryProbeDirPath = AppDomain.CurrentDomain.BaseDirectory;

            var ffmpegFilePath = new DirectoryInfo(primaryProbeDirPath)
                .EnumerateFiles()
                .Select(f => f.FullName)
                .Select(Path.GetFileNameWithoutExtension)
                .FirstOrDefault(n => string.Equals(n, "ffmpeg", StringComparison.OrdinalIgnoreCase));

            return ffmpegFilePath ?? "ffmpeg";
        }
    }
}