using System.IO;

namespace YoutubeExplode.Converter.Internal
{
    internal static class FileEx
    {
        public static bool TryDelete(string filePath)
        {
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}