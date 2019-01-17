using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;

namespace YoutubeExplode.Converter.Internal
{
    internal class FfmpegCli
    {
        private readonly string _ffmpegFilePath;

        public FfmpegCli(string ffmpegFilePath)
        {
            _ffmpegFilePath = ffmpegFilePath;
        }

        public Task ProcessAsync(IReadOnlyList<string> inputFilePaths,
            string outputFilePath, string format, bool transcode,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var args = new List<string>();

            // Set input files
            foreach (var inputFilePath in inputFilePaths)
                args.Add($"-i \"{inputFilePath}\"");

            // Set output format
            args.Add($"-f {format}");

            // Skip transcoding if it's not required
            if (!transcode)
                args.Add("-c copy");

            // Optimize mp4 transcoding
            if (transcode && string.Equals(format, "mp4", StringComparison.OrdinalIgnoreCase))
                args.Add("-preset ultrafast");

            // Set max threads
            args.Add($"-threads {Environment.ProcessorCount}");

            // Disable stdin so that the process will not hang waiting for user input
            args.Add("-nostdin");

            // Trim streams to shortest
            args.Add("-shortest");

            // Overwrite files
            args.Add("-y");

            // Set output file
            args.Add($"\"{outputFilePath}\"");

            // Set up progress parsing and routing
            var totalDuration = TimeSpan.Zero;
            var progressRouter = new Action<string>(s =>
            {
                // Parse total duration if it's not known yet
                if (totalDuration == TimeSpan.Zero)
                {
                    var totalDurationRaw = Regex.Match(s, @"Duration:\s(\d\d:\d\d:\d\d.\d\d)").Groups[1].Value;
                    if (totalDurationRaw.IsNotBlank())
                        totalDuration = TimeSpan.ParseExact(totalDurationRaw, "c", CultureInfo.InvariantCulture);
                }
                // Parse current duration and report progress if total duration is known
                else
                {
                    var currentDurationRaw = Regex.Match(s, @"time=(\d\d:\d\d:\d\d.\d\d)").Groups[1].Value;
                    if (currentDurationRaw.IsNotBlank())
                    {
                        var currentDuration =
                            TimeSpan.ParseExact(currentDurationRaw, "c", CultureInfo.InvariantCulture);

                        // Report progress
                        progress?.Report(currentDuration.TotalMilliseconds / totalDuration.TotalMilliseconds);
                    }
                }
            });

            // Run CLI
            return new Cli(_ffmpegFilePath)
                .SetWorkingDirectory(Directory.GetCurrentDirectory())
                .SetArguments(args.JoinToString(" "))
                .SetStandardErrorCallback(progressRouter) // handle stderr to parse and route progress
                .SetCancellationToken(cancellationToken)
                .EnableExitCodeValidation()
                .EnableStandardErrorValidation(false) // disable stderr validation because ffmpeg writes progress there
                .ExecuteAsync();
        }
    }
}