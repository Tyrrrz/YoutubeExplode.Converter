using System.Collections;

namespace YoutubeExplode.Converter.Tests
{
    public static class TestData
    {
        public static IEnumerable OutputFormats
        {
            get
            {
                yield return "mp4";
                yield return "mp3";
            }
        }

        public static IEnumerable VideoIds
        {
            get
            {
                yield return "AI7ULzgf8RU";
                yield return "-qmBhoeQgv8";
            }
        }
    }
}