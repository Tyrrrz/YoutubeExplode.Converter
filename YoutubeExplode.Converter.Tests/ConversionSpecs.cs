using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Converter.Options;
using YoutubeExplode.Converter.Tests.Fixtures;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter.Tests
{
    public class ConversionSpecs : IClassFixture<TempOutputFixture>, IClassFixture<FFmpegFixture>
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly TempOutputFixture _tempOutputFixture;
        private readonly FFmpegFixture _ffmpegFixture;

        public ConversionSpecs(ITestOutputHelper testOutput, TempOutputFixture tempOutputFixture, FFmpegFixture ffmpegFixture)
        {
            _testOutput = testOutput;
            _tempOutputFixture = tempOutputFixture;
            _ffmpegFixture = ffmpegFixture;
        }

        [Fact]
        public async Task I_can_download_a_video_by_merging_best_streams_into_a_single_mp4_file()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "mp4");
            var converter = new YoutubeClient().Videos.Streams.GetConverter(_ffmpegFixture.FilePath);

            // Act
            await converter.ConvertStreamsAsync("AI7ULzgf8RU", outputFilePath, "mp4");

            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task I_can_download_a_video_by_merging_best_streams_into_a_single_webm_file()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "webm");
            var converter = new YoutubeClient().Videos.Streams.GetConverter(_ffmpegFixture.FilePath);

            // Act
            await converter.ConvertStreamsAsync("FkklG9MA0vM", outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task I_can_download_a_video_by_merging_best_streams_into_a_single_mp3_file()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "mp3");
            var converter = new YoutubeClient().Videos.Streams.GetConverter(_ffmpegFixture.FilePath);

            // Act
            await converter.ConvertStreamsAsync("AI7ULzgf8RU", outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task I_can_download_a_video_by_merging_best_streams_into_a_single_ogg_file()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "ogg");
            var converter = new YoutubeClient().Videos.Streams.GetConverter(_ffmpegFixture.FilePath);

            // Act
            await converter.ConvertStreamsAsync("AI7ULzgf8RU", outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task I_can_download_a_video_with_a_custom_target_framerate()
        {
            // Arrange
            var outputFilePath = _tempOutputFixture.GetTempFilePath();
            var converter = new YoutubeClient().Videos.Streams.GetConverter(_ffmpegFixture.FilePath);

            var options = new ConversionOptionsBuilder(outputFilePath)
                .SetFormat("mp4")
                .SetTargetFramerate(new Framerate(15))
                .SetPreset(EncoderPreset.UltraFast)
                .Build();

            // Act
            await converter.ConvertStreamsAsync("AI7ULzgf8RU", options);

            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);

            // No real way to assert framerate...
        }

        [Fact]
        public async Task I_can_download_a_video_with_a_custom_target_bitrate()
        {
            // Arrange
            var outputFilePath = _tempOutputFixture.GetTempFilePath();
            var converter = new YoutubeClient().Videos.Streams.GetConverter(_ffmpegFixture.FilePath);

            var options = new ConversionOptionsBuilder(outputFilePath)
                .SetFormat("mp4")
                .SetTargetBitrate(new Bitrate(500_000))
                .SetPreset(EncoderPreset.UltraFast)
                .Build();

            // Act
            await converter.ConvertStreamsAsync("AI7ULzgf8RU", options);

            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);

            // No real way to assert bitrate...
        }

        [Fact]
        public async Task I_can_download_a_video_and_it_reports_progress_correctly()
        {
            // Arrange
            var progressReports = new List<double>();
            var progress = new Progress<double>(p =>
            {
                _testOutput.WriteLine($"Progress: {p:P2}");
                progressReports.Add(p);
            });

            var outputFilePath = _tempOutputFixture.GetTempFilePath();
            var converter = new YoutubeClient().Videos.Streams.GetConverter(_ffmpegFixture.FilePath);

            // Act
            await converter.ConvertStreamsAsync("AI7ULzgf8RU", outputFilePath, progress);

            // Assert
            progressReports.Should().NotBeEmpty();
            progressReports.Should().Contain(1.0);
        }
    }
}