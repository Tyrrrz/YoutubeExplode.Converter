﻿using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using YoutubeExplode.Converter.Tests.Internal;

namespace YoutubeExplode.Converter.Tests
{
    [TestFixture]
    public class YoutubeConverterTests
    {
        public string TempDirPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "Temp");

        [OneTimeTearDown]
        public void Cleanup()
        {
            if (Directory.Exists(TempDirPath))
                Directory.Delete(TempDirPath, true);
        }

        [Test]
        public async Task YoutubeConverter_DownloadVideoAsync_Test(
            [ValueSource(typeof(TestData), nameof(TestData.VideoIds))] string videoId,
            [ValueSource(typeof(TestData), nameof(TestData.OutputFormats))] string format)
        {
            // Arrange
            Directory.CreateDirectory(TempDirPath);
            var outputFilePath = Path.Combine(TempDirPath, $"{Guid.NewGuid()}.{format}");
            var progress = new ProgressCollector<double>();
            var converter = new YoutubeConverter();

            // Act
            await converter.DownloadVideoAsync(videoId, outputFilePath, format, progress);
            var fileInfo = new FileInfo(outputFilePath);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(fileInfo.Exists, Is.True, "File exists");
                Assert.That(fileInfo.Length, Is.GreaterThan(0), "File size");
                Assert.That(progress.GetAll(), Is.Not.Empty, "Progress");
            });
        }
    }
}