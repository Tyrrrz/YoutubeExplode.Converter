using System;
using System.IO;

namespace YoutubeExplode.Converter.Internal
{
    internal partial class TempFile : IDisposable
    {
        public string Path { get; }

        public TempFile(string filePath) => Path = filePath;

        public void Dispose()
        {
            try
            {
                File.Delete(Path);
            }
            catch (FileNotFoundException)
            {
            }
        }
    }

    internal partial class TempFile
    {
        public static TempFile Create() => new TempFile(System.IO.Path.GetTempFileName());
    }
}