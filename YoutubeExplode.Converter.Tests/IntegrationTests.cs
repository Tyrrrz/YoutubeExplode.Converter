using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YoutubeExplode.Converter.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        public string TestDirPath => TestContext.CurrentContext.TestDirectory;
        public string TempDirPath => Path.Combine(TestDirPath, "Temp");

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
            var converter = new YoutubeConverter();

            Directory.CreateDirectory(TempDirPath);
            var outputFilePath = Path.Combine(TempDirPath, $"{Guid.NewGuid()}");

            await converter.DownloadVideoAsync(videoId, outputFilePath, format);

            var fileInfo = new FileInfo(outputFilePath);

            Assert.That(fileInfo.Exists, Is.True);
            Assert.That(fileInfo.Length, Is.GreaterThan(0));
        }
    }
}