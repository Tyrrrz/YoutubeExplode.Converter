using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Builders;
using YoutubeExplode.Converter.Internal;
using YoutubeExplode.Converter.Internal.Extensions;
using YoutubeExplode.Converter.Options;

namespace YoutubeExplode.Converter
{
    // Note: FFmpeg can pipe to stdout but not for all formats (for mp4, in particular, it can't)
    internal partial class FFmpeg
    {
        private readonly string _executableFilePath;

        public FFmpeg(string executableFilePath) =>
            _executableFilePath = executableFilePath;

        // Ideally should use named pipes, but they don't work really well in .NET Core on Unix systems
        private async Task<IReadOnlyList<TempFile>> SetupTempFilesAsync(
            IReadOnlyList<Stream> inputs,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var progressMixer = progress?.Pipe(p => new ProgressMixer(p));
            var streamProgressOperations = inputs.Select(_ => progressMixer?.Split(1.0 / inputs.Count)).ToArray();

            return await Task.WhenAll(
                inputs.Zip(streamProgressOperations, async (stream, localProgress)  =>
                {
                    var tempFile = TempFile.Create();
                    using var tempFileStream = File.Create(tempFile.Path);

                    await stream.CopyToAsync(
                        tempFileStream,
                        localProgress,
                        cancellationToken
                    );

                    return tempFile;
                })
            );
        }

        private string GetArguments(
            IReadOnlyList<string> inputFilePaths,
            ConversionOptions options,
            bool isTranscodingRequired)
        {
            var arguments = new ArgumentsBuilder();

            foreach (var inputFilePath in inputFilePaths)
                arguments.Add("-i").Add(inputFilePath);

            arguments.Add("-f").Add(options.Format);

            arguments.Add("-preset").Add(options.Preset.ToString().ToLowerInvariant());

            if (options.TargetFramerate != null)
                arguments.Add("-r").Add(options.TargetFramerate.Value.FramesPerSecond);

            if (options.TargetBitrate != null)
                arguments.Add("-b").Add(options.TargetBitrate.Value.BitsPerSecond);

            // We can only skip transcoding if the input/output streams match one to one
            if (!isTranscodingRequired && options.TargetFramerate == null && options.TargetBitrate == null)
                arguments.Add("-c").Add("copy");

            arguments
                .Add("-threads").Add(Environment.ProcessorCount)
                .Add("-nostdin")
                .Add("-shortest")
                .Add("-y");

            arguments.Add(options.OutputFilePath);

            return arguments.Build();
        }

        private async Task ExecuteCommandAsync(
            IReadOnlyList<string> inputFilePaths,
            ConversionOptions options,
            bool isTranscodingRequired,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var arguments = GetArguments(
                inputFilePaths,
                options,
                isTranscodingRequired
            );

            var stdErrPipe = progress?.Pipe(p => new FFmpegProgressRouter(p)) ?? PipeTarget.Null;

            await Cli.Wrap(_executableFilePath)
                .WithArguments(arguments)
                .WithStandardErrorPipe(stdErrPipe)
                .ExecuteAsync(cancellationToken);
        }

        public async Task ConvertStreamsAsync(
            IReadOnlyList<Stream> inputs,
            ConversionOptions options,
            bool isTranscodingRequired,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var setupProgressPortion = isTranscodingRequired ? 0.15 : 0.99;
            var progressMixer = progress?.Pipe(p => new ProgressMixer(p));

            var setupProgress = progressMixer?.Split(setupProgressPortion);
            var executeProgress = progressMixer?.Split(1 - setupProgressPortion);

            var inputFiles = await SetupTempFilesAsync(
                inputs,
                setupProgress,
                cancellationToken
            );

            try
            {
                await ExecuteCommandAsync(
                    inputFiles.Select(f => f.Path).ToArray(),
                    options,
                    isTranscodingRequired,
                    executeProgress,
                    cancellationToken
                );
            }
            finally
            {
                foreach (var inputFile in inputFiles)
                    inputFile.Dispose();
            }
        }
    }

    internal partial class FFmpeg
    {
        public static string GetDefaultExecutableFilePath()
        {
            // Check the probe directory and see if there's anything that resembles FFmpeg there.
            // If not, fallback to just "ffmpeg" and hope it's either in current working directory or on PATH.

            var ffmpegFilePath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                .EnumerateFiles()
                .Select(f => f.FullName)
                .FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f), "ffmpeg", StringComparison.OrdinalIgnoreCase));

            return ffmpegFilePath ?? "ffmpeg";
        }
    }

    internal partial class FFmpeg
    {
        private class FFmpegProgressRouter : PipeTarget
        {
            private readonly StringBuilder _buffer  = new StringBuilder();
            private readonly IProgress<double> _output;

            private TimeSpan? _totalDuration;
            private TimeSpan? _lastOffset;

            public FFmpegProgressRouter(IProgress<double> output) => _output = output;

            private TimeSpan? TryParseTotalDuration(string data) => data
                .Pipe(s => Regex.Match(s, @"Duration:\s(\d\d:\d\d:\d\d.\d\d)").Groups[1].Value)
                .NullIfWhiteSpace()?
                .Pipe(s => TimeSpan.ParseExact(s, "c", CultureInfo.InvariantCulture));

            private TimeSpan? TryParseCurrentOffset(string data) => data
                .Pipe(s => Regex.Matches(s, @"time=(\d\d:\d\d:\d\d.\d\d)").Cast<Match>().LastOrDefault()?.Groups[1].Value)?
                .NullIfWhiteSpace()?
                .Pipe(s => TimeSpan.ParseExact(s, "c", CultureInfo.InvariantCulture));

            private void HandleBuffer()
            {
                var data = _buffer.ToString();

                _totalDuration ??= TryParseTotalDuration(data);
                if (_totalDuration == null)
                    return;

                var currentOffset = TryParseCurrentOffset(data);
                if (currentOffset == null || currentOffset == _lastOffset)
                    return;

                _lastOffset = currentOffset;

                var progress = (currentOffset.Value.TotalMilliseconds / _totalDuration.Value.TotalMilliseconds).Clamp(0, 1);
                _output.Report(progress);
            }

            public override async Task CopyFromAsync(Stream source, CancellationToken cancellationToken = default)
            {
                using var reader = new StreamReader(source, Console.OutputEncoding, false, 1024, true);

                var buffer = new char[1024];
                int charsRead;

                while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _buffer.Append(buffer, 0, charsRead);
                    HandleBuffer();
                }
            }
        }
    }
}