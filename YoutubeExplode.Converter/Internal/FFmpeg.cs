using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Builders;

namespace YoutubeExplode.Converter.Internal
{
    internal class FFmpeg
    {
        private readonly string _ffmpegFilePath;

        public FFmpeg(string ffmpegFilePath) => _ffmpegFilePath = ffmpegFilePath;

        public async Task ConvertAsync(
            string outputFilePath,
            IReadOnlyList<string> inputFilePaths,
            string format,
            string preset,
            bool avoidTranscoding,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_ffmpegFilePath))
                throw new InvalidOperationException($"FFmpeg doesn't exist: {_ffmpegFilePath}. Working dir: {Directory.GetCurrentDirectory()}.");

            var arguments = new ArgumentsBuilder();

            // Inputs
            foreach (var filePath in inputFilePaths)
            {
                arguments
                    .Add("-thread_queue_size").Add(512)
                    .Add("-i").Add(filePath);
            }

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

            await Cli.Wrap(_ffmpegFilePath)
                .WithArguments(arguments.Build())
                .ExecuteAsync(cancellationToken);
        }
    }
}