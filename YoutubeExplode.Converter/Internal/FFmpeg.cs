using System;
using System.Collections.Generic;
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

        public FFmpeg(string ffmpegFilePath)
        {
            _ffmpegFilePath = ffmpegFilePath;
        }

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
            var stdErrPipe = progress?
                .Pipe(p => new FfmpegProgressRouter(p))
                .Pipe(p => PipeTarget.ToDelegate(p.ProcessLine));

            await Cli.Wrap(_ffmpegFilePath)
                .WithArguments(arguments.Build())
                .WithStandardErrorPipe(stdErrPipe ?? PipeTarget.Null)
                .ExecuteAsync(cancellationToken);
        }
    }

    internal partial class FFmpeg
    {
        private class FfmpegProgressRouter
        {
            private readonly IProgress<double> _output;

            private TimeSpan? _totalDuration;

            public FfmpegProgressRouter(IProgress<double> output)
            {
                _output = output;
            }

            public void ProcessLine(string line)
            {
                // Try to parse total duration
                if (_totalDuration == null)
                {
                    _totalDuration = line
                        .Pipe(s => Regex.Match(s, @"Duration:\s(\d\d:\d\d:\d\d.\d\d)").Groups[1].Value)
                        .NullIfWhiteSpace()?
                        .ParseTimeSpan("c");
                }
                // Try to parse current duration and report progress
                else
                {
                    line
                        .Pipe(s => Regex.Match(s, @"time=(\d\d:\d\d:\d\d.\d\d)").Groups[1].Value)
                        .NullIfWhiteSpace()?
                        .ParseTimeSpan("c")
                        .Pipe(d => d.TotalMilliseconds / _totalDuration.Value.TotalMilliseconds)
                        .Pipe(d => d.Clamp(0, 1))
                        .Pipe(_output.Report);
                }
            }
        }
    }
}