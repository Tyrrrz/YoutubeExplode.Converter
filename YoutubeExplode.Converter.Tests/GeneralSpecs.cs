using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Converter.Tests.Fixtures;

namespace YoutubeExplode.Converter.Tests
{
    public class GeneralSpecs : IClassFixture<TempOutputFixture>, IClassFixture<FFmpegFixture>
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly TempOutputFixture _tempOutputFixture;

        public GeneralSpecs(ITestOutputHelper testOutput, TempOutputFixture tempOutputFixture)
        {
            _testOutput = testOutput;
            _tempOutputFixture = tempOutputFixture;
        }

        [Fact]
        public async Task I_can_download_a_video_by_merging_best_streams_into_a_single_mp4_file()
        {
            // Arrange
            var youtube = new YoutubeClient();
            var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "mp4");

            // Act
            await youtube.Videos.DownloadAsync("AI7ULzgf8RU", outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task I_can_download_a_video_by_merging_best_streams_into_a_single_webm_file()
        {
            // Arrange
            var youtube = new YoutubeClient();
            var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "webm");

            // Act
            await youtube.Videos.DownloadAsync("FkklG9MA0vM", outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task I_can_download_a_video_by_merging_best_streams_into_a_single_mp3_file()
        {
            // Arrange
            var youtube = new YoutubeClient();
            var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "mp3");

            // Act
            await youtube.Videos.DownloadAsync("AI7ULzgf8RU", outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task I_can_download_a_video_by_merging_best_streams_into_a_single_ogg_file()
        {
            // Arrange
            var youtube = new YoutubeClient();
            var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "ogg");

            // Act
            await youtube.Videos.DownloadAsync("AI7ULzgf8RU", outputFilePath);

            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
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

            var youtube = new YoutubeClient();
            var outputFilePath = _tempOutputFixture.GetTempFilePath();

            // Act
            await youtube.Videos.DownloadAsync("AI7ULzgf8RU", outputFilePath, progress);

            // Assert
            progressReports.Should().NotBeEmpty();
            progressReports.Should().Contain(1.0);
        }
    }
}