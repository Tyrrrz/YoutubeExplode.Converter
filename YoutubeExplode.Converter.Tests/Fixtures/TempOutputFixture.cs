using System;
using System.IO;

namespace YoutubeExplode.Converter.Tests.Fixtures
{
    public class TempOutputFixture : IDisposable
    {
        public string DirPath => Path.Combine(Path.GetDirectoryName(typeof(FFmpegFixture).Assembly.Location)!, "TempOutput");

        public TempOutputFixture() => Directory.CreateDirectory(DirPath);

        public void Dispose()
        {
            if (Directory.Exists(DirPath))
                Directory.Delete(DirPath, true);
        }
    }
}