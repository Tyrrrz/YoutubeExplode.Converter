using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Converter.Tests.Internal;

namespace YoutubeExplode.Converter.Tests
{
    public class YoutubeConverterTests : IClassFixture<TempOutputFixture>, IClassFixture<FFmpegFixture>
    {
        private readonly TempOutputFixture _tempOutputFixture;
        private readonly FFmpegFixture _ffmpegFixture;

        public YoutubeConverterTests(TempOutputFixture tempOutputFixture, FFmpegFixture ffmpegFixture)
        {
            _tempOutputFixture = tempOutputFixture;
            _ffmpegFixture = ffmpegFixture;
        }

        [Theory, CombinatorialData]
        public async Task YoutubeConverter_DownloadVideoAsync_Test(
            [CombinatorialValues("AI7ULzgf8RU", "-qmBhoeQgv8")] string videoId,
            [CombinatorialValues("mp4", "mp3")] string format)
        {
            // Arrange
            var outputFilePath = Path.Combine(_tempOutputFixture.DirPath, $"{Guid.NewGuid()}.{format}");
            var progress = new ProgressCollector<double>();
            var converter = new YoutubeConverter(new YoutubeClient(), _ffmpegFixture.FilePath);

            // Act
            await converter.DownloadVideoAsync(videoId, outputFilePath, format, progress);
            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
            progress.GetAll().Should().NotBeEmpty();
        }
    }
}