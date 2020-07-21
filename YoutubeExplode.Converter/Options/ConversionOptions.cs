using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter.Options
{
    /// <summary>
    /// Conversion options.
    /// </summary>
    public class ConversionOptions
    {
        /// <summary>
        /// Output file path.
        /// </summary>
        public string OutputFilePath { get; }

        /// <summary>
        /// Output format.
        /// </summary>
        public string Format { get; }

        /// <summary>
        /// Encoder preset.
        /// </summary>
        public EncoderPreset Preset { get; }

        /// <summary>
        /// Target framerate.
        /// </summary>
        public Framerate? TargetFramerate { get; }

        /// <summary>
        /// Target bitrate.
        /// </summary>
        public Bitrate? TargetBitrate { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ConversionOptions"/>.
        /// </summary>
        /// <param name="outputFilePath"></param>
        /// <param name="format"></param>
        /// <param name="preset"></param>
        /// <param name="targetFramerate"></param>
        /// <param name="targetBitrate"></param>
        public ConversionOptions(
            string outputFilePath,
            string format,
            EncoderPreset preset,
            Framerate? targetFramerate,
            Bitrate? targetBitrate)
        {
            OutputFilePath = outputFilePath;
            Format = format;
            Preset = preset;
            TargetFramerate = targetFramerate;
            TargetBitrate = targetBitrate;
        }
    }
}