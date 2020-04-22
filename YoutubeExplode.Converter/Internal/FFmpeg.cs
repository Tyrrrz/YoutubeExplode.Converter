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
using YoutubeExplode.Converter.Internal.Extensions;

namespace YoutubeExplode.Converter.Internal
{
    internal partial class FFmpeg
    {
        private readonly string _ffmpegFilePath;

        public FFmpeg(string ffmpegFilePath) => _ffmpegFilePath = ffmpegFilePath;

        public async Task ConvertAsync(
            string outputFilePath,
            IReadOnlyList<string> inputFilePaths,
            string format,
            string preset,
            bool avoidTranscoding,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var arguments = new ArgumentsBuilder();

            // Inputs
            foreach (var filePath in inputFilePaths)
                arguments.Add("-i").Add(filePath);

            // Format
            arguments.Add("-f").Add(format);

            // Preset
            arguments.Add("-preset").Add(preset);

            // Transcoding
            if (avoidTranscoding)
                arguments.Add("-c").Add("copy");

            // Optimizations
            arguments
                .Add("-threads").Add(Environment.ProcessorCount)
                .Add("-nostdin")
                .Add("-shortest")
                .Add("-y");

            // Output
            arguments.Add(outputFilePath);

            // StdErr pipe for progress reporting
            var stdErrPipe = progress?.Pipe(p => new FFmpegProgressRouter(p)) ?? PipeTarget.Null;

            await Cli.Wrap(_ffmpegFilePath)
                .WithArguments(arguments.Build())
                .WithStandardErrorPipe(stdErrPipe)
                .ExecuteAsync(cancellationToken);
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